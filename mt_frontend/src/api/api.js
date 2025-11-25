// const API_URL = "https://localhost:7296/api";
const API_URL = "https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api";

// Tasks

import { msalInstance, ensureMsalInitialized, apiScope, graphScopes } from "../authConfig";
import { getCurrentUser } from "../utils/auth";
import { isInTeams } from "../utils/teams";
import { getTeamsSsoToken } from "../utils/teamsAuth";
import reportError from "../utils/errorReporter";

export async function getAuthToken() {
  console.log("Attempting to get authentication token...");
  try {
    console.log("🔍 Starting token acquisition process");
    console.log("teams: isInTeams =", isInTeams());
    // If running inside Teams, prefer the Teams SSO token (suitable for backend SSO)
    if (isInTeams()) {
      try {
        console.log("In Teams: requesting Teams SSO token...");
        const ssoToken = await getTeamsSsoToken();
        if (ssoToken) {
          console.log("✅ Acquired Teams SSO token");
          return ssoToken;
        }
      } catch (teamsErr) {
        console.warn("Teams SSO acquisition failed, falling back to MSAL path:", teamsErr?.message || teamsErr);
      }
    }

    // Ensure MSAL is initialized
    await ensureMsalInitialized();

    const accounts = msalInstance.getAllAccounts();
    console.log("Available accounts:", accounts.length);

    if (accounts.length === 0) {
      console.warn("No MSAL account; returning null token");
      return null;
    }

    const tokenRequest = { scopes: [apiScope], account: accounts[0], forceRefresh: false };

    console.log("Token request:", tokenRequest);
    try {
      // Try silent token acquisition first
      const response = await msalInstance.acquireTokenSilent(tokenRequest);
      console.log("✅ Successfully acquired MSAL token (silent)");

      // Debug: show token claims
      const tokenPayload = JSON.parse(atob(response.accessToken.split('.')[1]));
      console.log("Token claims:", {
        aud: tokenPayload.aud,
        oid: tokenPayload.oid,
        sub: tokenPayload.sub,
        upn: tokenPayload.upn || tokenPayload.preferred_username
      });

      return response.accessToken;
    } catch (silentError) {
      console.log("Silent token acquisition failed, trying popup...", silentError);

      // Fallback to popup with same simple scope
      const response = await msalInstance.acquireTokenPopup(tokenRequest);
      console.log("✅ Successfully acquired MSAL token (popup)");

      // Debug: show token claims
      const tokenPayload = JSON.parse(atob(response.accessToken.split('.')[1]));
      console.log("Token claims:", {
        aud: tokenPayload.aud,
        oid: tokenPayload.oid,
        sub: tokenPayload.sub,
        upn: tokenPayload.upn || tokenPayload.preferred_username
      });

      return response.accessToken;
    }
  } catch (error) {
    console.error("❌ Failed to get authentication token:", error);
    throw new Error(`Token acquisition failed: ${error?.message || error}`);
  }
}

export async function sendTestNotification() {
  console.log("🔔 Starting notification test...");

  const currentUser = getCurrentUser();
  console.log("Current user:", currentUser);

  if (!currentUser) {
    throw new Error("Please log in to send notifications");
  }

  console.log("Attempting to get authentication token...");
  let token;
  
  try {
    token = await getAuthToken();
  } catch (tokenError) {
    console.error("❌ Token acquisition failed:", tokenError);
    throw new Error("Could not acquire authentication token. Please try logging in with Microsoft account first.");
  }

  if (!token) {
    throw new Error("Could not acquire authentication token. Please try logging in with Microsoft account first.");
  }

  console.log("✅ Got authentication token, sending notification...");

  // Extract user information from token
  let azureUserId, userEmail, userName;
  try {
    const tokenPayload = JSON.parse(atob(token.split('.')[1]));
    azureUserId = tokenPayload.oid || tokenPayload.sub;
    userEmail = tokenPayload.preferred_username || tokenPayload.upn || tokenPayload.email;
    userName = tokenPayload.name;
    
    console.log("Token payload:", {
      azureUserId,
      userEmail,
      userName,
      audience: tokenPayload.aud
    });
  } catch (error) {
    console.error("Failed to parse token:", error);
    azureUserId = currentUser.id?.toString();
    userEmail = currentUser.email;
    userName = currentUser.name;
  }

  // Enhanced request body with more user information
  const requestBody = {
    message: "Test notification from MiniTasker",
    userId: azureUserId,
    userEmail: userEmail,
    userName: userName,
    timestamp: new Date().toISOString()
  };

  console.log("Request details:", {
    url: `${API_URL}/notification/send-test`,
    body: requestBody,
    hasToken: !!token,
    tokenSnippet: token ? `${token.substring(0, 20)}...` : 'none'
  });

  try {
    const response = await fetch(`${API_URL}/notification/send-test`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`,
      },
      body: JSON.stringify(requestBody),
    });

    console.log("Response status:", response.status);
    console.log("Response headers:", Object.fromEntries(response.headers.entries()));

    // Get response text first to handle both JSON and plain text responses
    const responseText = await response.text();
    console.log("Response body:", responseText);
    let parsedResult = null;
    try { parsedResult = responseText ? JSON.parse(responseText) : null; } catch { parsedResult = null; }
    console.log("Response body:", responseText);
    if (parsedResult) {
      console.log("🔔 NotificationResultDto:", {
      success: parsedResult.Success ?? parsedResult.success,
      method: parsedResult.Method ?? parsedResult.method,
      message: parsedResult.Message ?? parsedResult.message,
      recipientId: parsedResult.RecipientId ?? parsedResult.recipientId,
      senderName: parsedResult.SenderName ?? parsedResult.senderName,
      tokenType: parsedResult.TokenType ?? parsedResult.tokenType,
      errorDetails: parsedResult.ErrorDetails ?? parsedResult.errorDetails,
      timestamp: parsedResult.Timestamp ?? parsedResult.timestamp,
      });
    }
    if (!response.ok) {
      console.error("❌ Error response:", responseText);
      console.error("❌ Error response:", responseText, parsedResult || {});
      
      if (response.status === 401) {
        throw new Error("Authentication failed. Your Azure AD token is not valid for this API. Please check your backend authentication configuration.");
      } else if (response.status === 500) {
        // Parse the error details if available
        let errorDetails = responseText;
        try {
          const errorJson = JSON.parse(responseText);
          errorDetails = errorJson.message || errorJson.error || errorJson.title || responseText;
        } catch (parseErr) {
          // Keep original text if not JSON
          console.debug("Failed to parse error response as JSON", parseErr);
        }
        
        throw new Error(`Server error (500): ${errorDetails}. Check backend logs for more details.`);
      }
      
      throw new Error(`Notification failed (${response.status}): ${responseText || 'Unknown error'}`);
    }

    // Parse response as JSON if possible
    let result;
    try {
      result = JSON.parse(responseText);
    } catch (parseErr2) {
      result = { message: responseText, success: true };
      console.debug("Response parsing fallback used", parseErr2);
    }

    console.log("✅ Notification sent successfully:", result);
    return result;
  } catch (fetchError) {
    console.error("❌ Notification request failed:", fetchError);
    
    // Provide more helpful error messages
    if (fetchError.name === 'TypeError' && fetchError.message.includes('fetch')) {
      throw new Error("Network error: Unable to connect to the backend server. Please check if the backend is running.");
    }
    
    throw fetchError;
  }
}

// Fetch all tasks
export async function fetchTasks() {
  const res = await fetch(`${API_URL}/tasks`);
  return res.json();
}

export async function fetchUserTasks() {
  const token = localStorage.getItem("accessToken");

  const response = await fetch(`${API_URL}/tasks/me`, {
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    throw new Error("Failed to fetch tasks");
  }

  return await response.json();
}

export async function getGraphToken() {
  console.log("🔍 Acquiring Microsoft Graph token...");

  try {
    await ensureMsalInitialized();

    // 1) If inside Teams, try backend OBO exchange using Teams SSO token
    if (isInTeams()) {
      try {
        const ssoToken = await getTeamsSsoToken();
        if (ssoToken) {
          try {
            console.debug("Calling backend microsoftSsoAuth for OBO exchange");
            const resp = await microsoftSsoAuth(ssoToken);
            const candidate = resp?.graphToken || resp?.accessToken || resp?.token || null;
            if (candidate) {
              console.debug("Received Graph token from backend via SSO/OBO");
              return candidate;
            }
            console.debug("Backend returned no graph token in response from microsoftSsoAuth", resp);

            // Secondary attempt: try the regular microsoftAuth endpoint with the same token
            try {
              console.debug("Attempting fallback to microsoftAuth endpoint with SSO token");
              const fb = await microsoftAuth(ssoToken);
              const fbCandidate = fb?.graphToken || fb?.accessToken || fb?.token || null;
              if (fbCandidate) {
                console.debug("Fallback microsoftAuth returned Graph token");
                return fbCandidate;
              }
              console.debug("Fallback microsoftAuth returned no Graph token", fb);
            } catch (fbErr) {
              console.debug("Fallback microsoftAuth failed:", fbErr?.message || fbErr);
            }
          } catch (e) {
            console.debug("microsoftSsoAuth did not return graph token or failed:", e?.message || e);
          }
        } else {
          console.debug("No Teams SSO token available from getTeamsSsoToken");
        }
      } catch (teamsErr) {
        console.debug("Teams SSO path failed or unavailable:", teamsErr?.message || teamsErr);
      }
      // Fall through to MSAL flows after trying backend OBO
    }

    // 2) Try MSAL silent acquisition (browser flows)
    const accounts = msalInstance.getAllAccounts() || [];
    console.log("Available MSAL accounts:", accounts.length);

    if (accounts.length > 0) {
      const account = accounts[0];
      console.log("📋 Using account for Graph token:", account.username || account.homeAccountId || account.localAccountId);
      const tokenRequest = { scopes: graphScopes, account, forceRefresh: false };
      try {
        console.log("🔄 Attempting silent Graph token acquisition...");
        const response = await msalInstance.acquireTokenSilent(tokenRequest);
        console.log("✅ Successfully acquired Graph token (silent)");
        return response.accessToken;
      } catch (silentError) {
        console.log("⚠️ Silent Graph token acquisition failed:", silentError?.message || silentError);
        try { reportError({ message: 'Silent Graph token acquisition failed', stack: silentError?.stack || String(silentError), source: 'getGraphToken' }); } catch {}
        console.log("Silent graph token acquisition failed and interactive popup is disabled here. Falling through to backend fallback.");
      }
    } else {
      console.log("No MSAL accounts available for silent acquisition; will try backend fallback.");
    }

    // 3) Final fallback: ask backend to exchange whatever frontend auth token we can get
    try {
      console.debug("Attempting backend exchange using frontend auth token...");
      const frontendAuth = await getAuthToken();
      if (frontendAuth) {
        try {
          const backendResp = await microsoftSsoAuth(frontendAuth);
          const backendCandidate = backendResp?.graphToken || backendResp?.accessToken || backendResp?.token || null;
          if (backendCandidate) {
            console.debug("Received Graph token from backend via fallback exchange");
            return backendCandidate;
          }
          console.debug("Backend fallback exchange returned no graph token", backendResp);
        } catch (backendErr) {
          console.debug("Backend fallback exchange failed:", backendErr?.message || backendErr);
        }
      } else {
        console.debug("No frontend auth token available for backend fallback");
      }
    } catch (err) {
      console.debug("Fallback backend exchange errored:", err?.message || err);
    }

    // Nothing worked
    console.error("❌ No Graph token could be acquired (MSAL silent, Teams OBO, or backend fallback)");
    return null;
  } catch (error) {
    console.error("❌ Graph token acquisition failed:", error?.message || error);
    try { reportError({ message: "Graph token acquisition failed", stack: error?.stack || String(error), source: 'getGraphToken' }); } catch {}
    return null;
  }
}
// export async function getGraphToken() {
//   console.log("🔍 Acquiring Microsoft Graph token...");

//   try {
//     await ensureMsalInitialized();

//     // First: attempt to get a Graph token directly from MSAL (this will have frontend app id)
//     const accounts = msalInstance.getAllAccounts() || [];
//     console.log("Available MSAL accounts:", accounts.length);

//     const teamsScope = "https://graph.microsoft.com/TeamsActivity.Send";
//     if (accounts.length > 0) {
//       const account = accounts[0];
//       console.log("📋 Using account for direct Graph token:", account.username || account.homeAccountId || account.localAccountId);

//       // Request Graph scope directly so token is issued to the frontend app (prevents backend appid showing up)
//       const tokenRequest = { scopes: [teamsScope], account, forceRefresh: false };
//       try {
//         console.log("🔄 Attempting silent Graph token acquisition (TeamsActivity.Send)...");
//         const response = await msalInstance.acquireTokenSilent(tokenRequest);
//         console.log("✅ Successfully acquired Graph token (silent) from MSAL");
//         return response.accessToken;
//       } catch (silentError) {
//         console.warn("Silent Graph token acquisition failed, trying interactive popup...", silentError?.message || silentError);
//         try {
//           const response = await msalInstance.acquireTokenPopup(tokenRequest);
//           console.log("✅ Successfully acquired Graph token (popup) from MSAL");
//           return response.accessToken;
//         } catch (popupErr) {
//           console.warn("Interactive Graph token acquisition also failed:", popupErr?.message || popupErr);
//           // fall through to other fallback mechanisms below
//         }
//       }
//     } else {
//       console.log("No MSAL accounts available for direct Graph acquisition; will try fallback paths.");
//     }

//     // If running inside Teams, try backend OBO exchange using Teams SSO token (legacy fallback)
//     if (isInTeams()) {
//       try {
//         console.debug("In Teams: attempting backend OBO exchange using Teams SSO token...");
//         const ssoToken = await getTeamsSsoToken();
//         if (ssoToken) {
//           const resp = await microsoftSsoAuth(ssoToken);
//           const candidate = resp?.graphToken || resp?.accessToken || resp?.token || null;
//           if (candidate) {
//             console.debug("Received Graph token from backend via SSO/OBO");
//             return candidate;
//           }
//           console.debug("Backend returned no graph token in microsoftSsoAuth response", resp);
//         } else {
//           console.debug("No Teams SSO token available from getTeamsSsoToken");
//         }
//       } catch (teamsErr) {
//         console.debug("Teams SSO path failed or unavailable:", teamsErr?.message || teamsErr);
//       }
//       // fall through to next fallback
//     }

//     // Final fallback: ask backend to exchange frontend backend token for a Graph token
//     try {
//       console.debug("Attempting backend exchange using frontend auth token...");
//       const frontendAuth = await getAuthToken();
//       if (frontendAuth) {
//         try {
//           const backendResp = await microsoftSsoAuth(frontendAuth);
//           const backendCandidate = backendResp?.graphToken || backendResp?.accessToken || backendResp?.token || null;
//           if (backendCandidate) {
//             console.debug("Received Graph token from backend via fallback exchange");
//             return backendCandidate;
//           }
//           console.debug("Backend fallback exchange returned no graph token", backendResp);
//         } catch (backendErr) {
//           console.debug("Backend fallback exchange failed:", backendErr?.message || backendErr);
//         }
//       } else {
//         console.debug("No frontend auth token available for backend fallback");
//       }
//     } catch (err) {
//       console.debug("Fallback backend exchange errored:", err?.message || err);
//     }

//     console.error("❌ No Graph token could be acquired (MSAL direct, Teams OBO, or backend fallback)");
//     return null;
//   } catch (error) {
//     console.error("❌ Graph token acquisition failed:", error?.message || error);
//     try { reportError({ message: "Graph token acquisition failed", stack: error?.stack || String(error), source: 'getGraphToken' }); } catch {}
//     return null;
//   }
// }
// Create a new task
// api/taskApi.js or wherever your createTask function is
// Create a new task with notification support
export async function createTask(task) {
  console.log("🚀 Creating task with notification support...");

  let backendToken = await getAuthToken();
  let graphToken = await getGraphToken();
  
  if (!backendToken) {
    console.warn("No backend token available, creating task without notification");
    
    // Simple request without notification when no token
    const simpleRequestBody = {
      title: task.title,
      description: task.description,
      assignedTo: task.assignedTo,
      dueDate: task.dueDate,
      actorName: "Unknown" // Add actorName for simple requests
    };

    try {
      const response = await fetch(`${API_URL}/tasks`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(simpleRequestBody)
      });

      const responseText = await response.text();
      let parsed = null;
      try { 
        parsed = responseText ? JSON.parse(responseText) : null; 
      } catch { 
        parsed = null; 
      }

      if (!response.ok) {
        console.error("❌ Task creation failed:", response.status, responseText);
        throw new Error(`Task creation failed: ${response.status} - ${responseText}`);
      }

      const result = parsed ?? { message: responseText, success: true };
      console.log("✅ Task created successfully (no notification):", result);
      return result;
    } catch (fetchError) {
      console.error("❌ Task creation request failed:", fetchError);
      throw fetchError;
    }
  }

  // Extract user information from token when we have one
  const currentUser = getCurrentUser();
  let azureUserId, userEmail, userName;

  try {
    const tokenPayload = JSON.parse(atob(backendToken.split('.')[1]));
    azureUserId = tokenPayload.oid || tokenPayload.sub;
    userEmail = tokenPayload.preferred_username || tokenPayload.upn || tokenPayload.email;
    userName = tokenPayload.name;
    
    console.log("Token payload:", {
      azureUserId,
      userEmail,
      userName,
      audience: tokenPayload.aud
    });
  } catch (error) {
    console.error("Failed to parse token:", error);
    azureUserId = currentUser?.id?.toString();
    userEmail = currentUser?.email;
    userName = currentUser?.name;
  }

  // Get assigned user's Azure AD ID if available
  let assignedUserAzureAdId = null;
  if (task.assignedTo) {
    try {
      const userAzureAdResult = await getUserAzureAdId(task.assignedTo);
      assignedUserAzureAdId = userAzureAdResult?.azureAdId || null;
      console.log("📋 Assigned user Azure AD ID:", assignedUserAzureAdId);
    } catch (error) {
      console.warn("Could not get assigned user's Azure AD ID:", error);
    }
  }

  // ✅ FIXED: Use the correct format that matches CreateTaskWithNotificationRequestDto
  const notificationRequestBody = {
    task: {  // ✅ This triggers the notification request path in backend
      title: task.title,
      description: task.description,
      assignedTo: parseInt(task.assignedTo), // Ensure it's a number
      dueDate: task.dueDate
    },
    message: `You have been assigned a new task: '${task.title}'`,
    userId: azureUserId,           // Creator's Azure AD ID
    userEmail: userEmail,
    userName: userName,
    assignedUserAzureAdId: assignedUserAzureAdId, // Assigned user's Azure AD ID
    authToken: backendToken,       // Backend auth token (optional, already in header)
    graphToken: graphToken,        // Graph API token
    timestamp: new Date().toISOString()
  };

  console.log("📤 Sending notification request:", {
    url: `${API_URL}/tasks`,
    taskData: notificationRequestBody.task,
    userInfo: {
      userId: notificationRequestBody.userId,
      userName: notificationRequestBody.userName,
      assignedUserAzureAdId: notificationRequestBody.assignedUserAzureAdId
    },
    hasGraphToken: !!graphToken
  });

  try {
    const response = await fetch(`${API_URL}/tasks`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${backendToken}`
      },
      body: JSON.stringify(notificationRequestBody)
    });

    const responseText = await response.text();
    let parsed = null;
    try { 
      parsed = responseText ? JSON.parse(responseText) : null; 
    } catch { 
      parsed = null; 
    }

    if (!response.ok) {
      console.error("❌ Task creation failed:", response.status, responseText);
      throw new Error(`Task creation failed: ${response.status} - ${responseText}`);
    }

    const result = parsed ?? { message: responseText, success: true };
    console.log("✅ Task created successfully:", result);
    
    // ✅ ENHANCED: Log the detailed notification result from backend
    if (result.notification) {
      console.log("🔔 NotificationResultDto (createTask):", {
        success: result.notification.success,
        method: result.notification.method,
        message: result.notification.message,
        recipientId: result.notification.recipientId,
        senderName: result.notification.senderName,
        tokenType: result.notification.tokenType,
        errorDetails: result.notification.errorDetails,
        timestamp: result.notification.timestamp,
      });

      if (result.notification.success) {
        console.log("🔔 Notification sent to assigned user successfully!");
        console.log(`📋 Method used: ${result.notification.method}`);
        console.log(`🎯 Token type: ${result.notification.tokenType}`);
      } else {
        console.log("⚠️ Task created but notification failed:");
        console.log(`❌ Error: ${result.notification.errorDetails || result.notification.message}`);
      }
    }

    // Log metadata
    if (result.metadata) {
      console.log("📊 Request metadata:", {
        requestType: result.metadata.requestType,
        notificationAttempted: result.metadata.notificationAttempted,
        taskCreatedAt: result.metadata.taskCreatedAt
      });
    }

    return result;
  } catch (fetchError) {
    console.error("❌ Task creation request failed:", fetchError);
    throw fetchError;
  }
}

// Add this to your api.js file:

export async function getUserAzureAdId(userId) {
  console.log(`🔍 Getting Azure AD ID for user ${userId}...`);
  
  const response = await fetch(`${API_URL}/users/${userId}/azuread-id`);
  
  if (!response.ok) {
    console.error(`❌ Failed to get user Azure AD ID: ${response.status}`);
    throw new Error(`Failed to get user Azure AD ID: ${response.status}`);
  }
  
  const result = await response.json();
  console.log(`📋 User Azure AD ID result:`, result);
  
  return result;
}

// In your task creation function
// export async function createTaskWithNotification(taskData, assignedUserAzureAdId) {
//   console.log("🔔 Creating task with notification...");
//   console.log("📋 Task data:", taskData);
//   console.log("👤 Assigned user Azure AD ID:", assignedUserAzureAdId);

//   const currentUser = getCurrentUser();
//   console.log("Current user:", currentUser);
//   if (!currentUser) {
//     throw new Error("Please log in to create tasks");
//   }

//   let token = await getAuthToken();
//   let graphT = await getGraphToken();
//   if (!token) {
//     console.warn("No token; falling back to plain task creation without notification.");
//     return createTask(taskData);
//   }

//   // Extract user information from token (same as button)
//   let azureUserId, userEmail, userName;

//   try {
//     const tokenPayload = JSON.parse(atob(token.split('.')[1]));
//     azureUserId = tokenPayload.oid || tokenPayload.sub;
//     userEmail = tokenPayload.preferred_username || tokenPayload.upn || tokenPayload.email;
//     userName = tokenPayload.name;
//   } catch (error) {
//     console.error("Failed to parse token:", error);
//     azureUserId = currentUser.id?.toString();
//     userEmail = currentUser.email;
//     userName = currentUser.name;
//   }

//   // ✅ Enhanced request body with assigned user's Azure AD ID
//   const requestBody = {
//     task: {
//       title: taskData.title,
//       description: taskData.description,
//       assignedTo: parseInt(taskData.assignedTo), // ✅ Convert to integer
//       dueDate: taskData.dueDate
//     },
//     message: `You have been assigned a new task: '${taskData.title}'`,
//     userId: azureUserId,           // Creator's Azure AD ID
//     userEmail: userEmail,
//     userName: userName,
//     assignedUserAzureAdId: assignedUserAzureAdId, //Assigned user's Azure AD ID
//     authToken: token,           // Backend auth token
//     graphToken: graphT,         // Access token for Graph API
//     timestamp: new Date().toISOString()
//   };

//   console.log("🔍 Full request body:", JSON.stringify(requestBody, null, 2));

//   const response = await fetch(`${API_URL}/tasks`, {
//     method: "POST",
//     headers: {
//       "Content-Type": "application/json",
//       "Authorization": `Bearer ${token}`,
//     },
//     body: JSON.stringify(requestBody),
//   });

//   const responseText = await response.text();
//   let parsed = null;
//   try { parsed = responseText ? JSON.parse(responseText) : null; } catch { parsed = null; }

//   if (parsed) {
//      console.log("Backend result (createTaskWithNotification):", {
//       success: parsed.Success ?? parsed.success,
//       method: parsed.Method ?? parsed.method,
//       message: parsed.Message ?? parsed.message,
//       recipientId: parsed.RecipientId ?? parsed.recipientId,
//       senderName: parsed.SenderName ?? parsed.senderName,
//       tokenType: parsed.TokenType ?? parsed.tokenType,
//       errorDetails: parsed.ErrorDetails ?? parsed.errorDetails,
//       timestamp: parsed.Timestamp ?? parsed.timestamp,
//     });
//   }else {
//     console.log("Backend response (createTaskWithNotification) not JSON:", responseText);
//   }

//   if (!response.ok) {
//     const errorMsg = parsed?.Message || parsed?.message || responseText || `Status ${response.status}`;
//     throw new Error(`Task creation failed: ${response.status}- ${errorMsg}`);
//   }

//   return parsed ?? { message: responseText, success: true };
// }
// export async function createTaskWithNotification(taskData, assignedUserAzureAdId) {
//   console.log("🔔 Creating task with notification...");
//   console.log("📋 Task data:", taskData);
//   console.log("👤 Assigned user Azure AD ID:", assignedUserAzureAdId);

//   const currentUser = getCurrentUser();
//   console.log("Current user:", currentUser);
//   if (!currentUser) {
//     throw new Error("Please log in to create tasks");
//   }

//   let token = await getAuthToken();
//   let graphT = await getGraphToken();
//   if (!token) {
//     console.warn("No token; falling back to plain task creation without notification.");
//     return createTask(taskData);
//   }

//   // Extract user information from token (same as button)
//   let azureUserId, userEmail, userName;

//   try {
//     const tokenPayload = JSON.parse(atob(token.split('.')[1]));
//     azureUserId = tokenPayload.oid || tokenPayload.sub;
//     userEmail = tokenPayload.preferred_username || tokenPayload.upn || tokenPayload.email;
//     userName = tokenPayload.name;
    
//     console.log("Token payload extracted:", {
//       azureUserId,
//       userEmail,
//       userName,
//       audience: tokenPayload.aud
//     });
//   } catch (error) {
//     console.error("Failed to parse token:", error);
//     azureUserId = currentUser.id?.toString();
//     userEmail = currentUser.email;
//     userName = currentUser.name;
//   }

//   // ✅ Enhanced request body with assigned user's Azure AD ID
//   const requestBody = {
//     task: {
//       title: taskData.title,
//       description: taskData.description,
//       assignedTo: parseInt(taskData.assignedTo), // ✅ Convert to integer
//       dueDate: taskData.dueDate
//     },
//     message: `You have been assigned a new task: '${taskData.title}'`,
//     userId: azureUserId,           // Creator's Azure AD ID
//     userEmail: userEmail,
//     userName: userName,
//     assignedUserAzureAdId: assignedUserAzureAdId, //Assigned user's Azure AD ID
//     authToken: token,           // Backend auth token (optional, already in header)
//     graphToken: graphT,         // Access token for Graph API
//     timestamp: new Date().toISOString()
//   };

//   console.log("🔍 Full request body:", JSON.stringify(requestBody, null, 2));

//   try {
//     const response = await fetch(`${API_URL}/tasks`, {
//       method: "POST",
//       headers: {
//         "Content-Type": "application/json",
//         "Authorization": `Bearer ${token}`,
//       },
//       body: JSON.stringify(requestBody),
//     });

//     const responseText = await response.text();
//     let parsed = null;
//     try { 
//       parsed = responseText ? JSON.parse(responseText) : null; 
//     } catch { 
//       parsed = null; 
//     }

//     if (!response.ok) {
//       console.error("❌ Task creation with notification failed:", response.status, responseText);
//       const errorMsg = parsed?.message || parsed?.error || responseText || `Status ${response.status}`;
//       throw new Error(`Task creation failed: ${response.status} - ${errorMsg}`);
//     }

//     const result = parsed ?? { message: responseText, success: true };
//     console.log("✅ Task created with notification successfully:", result);

//     // ✅ ENHANCED: Log the detailed notification result from backend
//     if (result.notification) {
//       console.log("🔔 NotificationResultDto (createTaskWithNotification):", {
//         success: result.notification.success,
//         method: result.notification.method,
//         message: result.notification.message,
//         recipientId: result.notification.recipientId,
//         senderName: result.notification.senderName,
//         tokenType: result.notification.tokenType,
//         errorDetails: result.notification.errorDetails,
//         timestamp: result.notification.timestamp,
//       });

//       if (result.notification.success) {
//         console.log("🔔 Notification sent to assigned user successfully!");
//         console.log(`📋 Method used: ${result.notification.method}`);
//         console.log(`🎯 Token type: ${result.notification.tokenType}`);
//         console.log(`👤 Recipient: ${result.notification.recipientId}`);
//         console.log(`👤 Sender: ${result.notification.senderName}`);
//       } else {
//         console.log("⚠️ Task created but notification failed:");
//         console.log(`❌ Error: ${result.notification.errorDetails || result.notification.message}`);
//       }
//     } else {
//       // Fallback: check for old response format
//       console.log("📋 Backend response (legacy format check):", {
//         success: result.Success ?? result.success,
//         method: result.Method ?? result.method,
//         message: result.Message ?? result.message,
//         recipientId: result.RecipientId ?? result.recipientId,
//         senderName: result.SenderName ?? result.senderName,
//         tokenType: result.TokenType ?? result.tokenType,
//         errorDetails: result.ErrorDetails ?? result.errorDetails,
//         timestamp: result.Timestamp ?? result.timestamp,
//       });
//     }

//     // Log metadata
//     if (result.metadata) {
//       console.log("📊 Request metadata:", {
//         requestType: result.metadata.requestType,
//         notificationAttempted: result.metadata.notificationAttempted,
//         taskCreatedAt: result.metadata.taskCreatedAt
//       });
//     }

//     return result;
//   } catch (fetchError) {
//     console.error("❌ Network or parsing error in createTaskWithNotification:", fetchError);
    
//     // Provide more helpful error messages
//     if (fetchError.name === 'TypeError' && fetchError.message.includes('fetch')) {
//       throw new Error("Network error: Unable to connect to the backend server. Please check if the backend is running.");
//     }
    
//     throw fetchError;
//   }
// }
export async function createTaskWithNotification(taskData, assignedUserAzureAdId) {
  console.log("🔔 Creating task with notification...");
  console.log("📋 Task data:", taskData);
  console.log("👤 Assigned user Azure AD ID:", assignedUserAzureAdId);

  const currentUser = getCurrentUser();
  console.log("Current user:", currentUser);
  if (!currentUser) {
    throw new Error("Please log in to create tasks");
  }

  let token = await getAuthToken();
  let graphT = await getGraphToken();
  if (!token) {
    console.warn("No token; falling back to plain task creation without notification.");
    return createTask(taskData);
  }

  // Extract user information from token
  let azureUserId, userEmail, userName;
  try {
    const tokenPayload = JSON.parse(atob(token.split('.')[1]));
    azureUserId = tokenPayload.oid || tokenPayload.sub;
    userEmail = tokenPayload.preferred_username || tokenPayload.upn || tokenPayload.email;
    userName = tokenPayload.name;
    console.log("Token payload extracted:", { azureUserId, userEmail, userName, audience: tokenPayload.aud });
  } catch (error) {
    console.error("Failed to parse token:", error);
    azureUserId = currentUser?.id?.toString();
    userEmail = currentUser?.email;
    userName = currentUser?.name;
  }

  const requestBody = {
    task: {
      title: taskData.title,
      description: taskData.description,
      assignedTo: parseInt(taskData.assignedTo),
      dueDate: taskData.dueDate
    },
    message: `You have been assigned a new task: '${taskData.title}'`,
    userId: azureUserId,
    userEmail: userEmail,
    userName: userName,
    assignedUserAzureAdId: assignedUserAzureAdId,
    authToken: token,
    graphToken: graphT,
    timestamp: new Date().toISOString()
  };

  console.log("🔍 Full request body:", JSON.stringify(requestBody, null, 2));

  try {
    const response = await fetch(`${API_URL}/tasks`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${token}`,
      },
      body: JSON.stringify(requestBody),
    });

    const responseText = await response.text();
    let parsed = null;
    try { parsed = responseText ? JSON.parse(responseText) : null; } catch { parsed = null; }

    if (!response.ok) {
      console.error("❌ Task creation with notification failed:", response.status, responseText);
      const errorMsg = parsed?.message || parsed?.error || responseText || `Status ${response.status}`;
      throw new Error(`Task creation failed: ${response.status} - ${errorMsg}`);
    }

    const result = parsed ?? { message: responseText, success: true };
    console.log("✅ Task created with notification successfully:", result);

    // NORMALIZE notification DTO (accept PascalCase or camelCase, nested or root)
    const rawNotif = (result?.notification ?? result?.Notification) || result;
    const notif = {
      success: rawNotif?.Success ?? rawNotif?.success ?? rawNotif?.Succeeded ?? false,
      method: rawNotif?.Method ?? rawNotif?.method ?? null,
      message: rawNotif?.Message ?? rawNotif?.message ?? null,
      recipientId: rawNotif?.RecipientId ?? rawNotif?.recipientId ?? null,
      senderName: rawNotif?.SenderName ?? rawNotif?.senderName ?? null,
      tokenType: rawNotif?.TokenType ?? rawNotif?.tokenType ?? null,
      errorDetails: rawNotif?.ErrorDetails ?? rawNotif?.errorDetails ?? null,
      timestamp: rawNotif?.Timestamp ?? rawNotif?.timestamp ?? null
    };

    console.log("🔔 Normalized NotificationResultDto (createTaskWithNotification):", notif);

    if (notif.success) {
      console.log("🔔 Notification sent to assigned user successfully!");
      console.log(`📋 Method used: ${notif.method}`);
      console.log(`🎯 Token type: ${notif.tokenType}`);
      console.log(`👤 Recipient: ${notif.recipientId}`);
      console.log(`👤 Sender: ${notif.senderName}`);
    } else {
      console.log("⚠️ Task created but notification failed or skipped:");
      console.log(`❌ Error: ${notif.errorDetails || notif.message}`);
    }

    // Log metadata if present
    if (result.metadata) {
      console.log("📊 Request metadata:", {
        requestType: result.metadata.requestType,
        notificationAttempted: result.metadata.notificationAttempted,
        taskCreatedAt: result.metadata.taskCreatedAt
      });
    }

    // Return original result (keep shape for callers) but include normalized notification for easier consumption
    return { ...result, normalizedNotification: notif };
  } catch (fetchError) {
    console.error("❌ Network or parsing error in createTaskWithNotification:", fetchError);
    if (fetchError.name === 'TypeError' && fetchError.message.includes('fetch')) {
      throw new Error("Network error: Unable to connect to the backend server. Please check if the backend is running.");
    }
    throw fetchError;
  }
}

// Update an existing task
export async function updateTask(id, task) {
  await fetch(`${API_URL}/tasks/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(task)
  });
}

// Delete a task
export async function deleteTask(id) {
  const response = await fetch(`${API_URL}/tasks/${id}`, {
    method: "DELETE",
  });
  if (!response.ok) {
    throw new Error("Failed to delete task");
  }
  return response.json();
}

// Users
// Fetch all users
export async function fetchUsers() {
  const res = await fetch(`${API_URL}/users`);
  return res.json();
}

// Signup a new user
export async function createUser(name, email, password) {
  console.log("Creating user with:", { name, email, password });
  const res = await fetch(`${API_URL}/users`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ name, email, password }),
  });
  if (!res.ok) throw new Error("Signup failed");
  return res.json();
}

// Login
export async function loginUser(email, password) {
  console.log("Logging in with(inside api.js):", { email, password });
  console.log("API URL:", API_URL);
  const res = await fetch(`${API_URL}/users/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });
  if (!res.ok) return null;
  return res.json();
}

// NEW / UPDATED: Microsoft authentication (replaces upsertMicrosoftUser)
export async function microsoftAuth(token) {
  const res = await fetch(`${API_URL}/auth/microsoft`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });
  if (res.status === 404) {
    throw new Error("404: /auth/microsoft endpoint not found.");
  }
  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(`Microsoft auth failed (status ${res.status}) ${text}`);
  }
  return res.json();
}

// Teams SSO endpoint: backend validates the token via Microsoft.Identity.Web middleware
// and may perform OBO to acquire a Graph token. Use when acquiring tokens via Teams SDK.
export async function microsoftSsoAuth(token) {
  const res = await fetch(`${API_URL}/auth/microsoft-sso`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json",
    },
  });

  console.debug("microsoftSsoAuth response status:", res.status);
  console.debug("microsoftSsoAuth response headers:", [...res.headers.entries()]);
  const raw = await res.text().catch(() => "");
  // Try to parse JSON body when possible
  let parsed;
  try { parsed = raw ? JSON.parse(raw) : null; } catch (e) { parsed = null; }

  if (res.status === 404) {
    console.error("microsoftSsoAuth: /auth/microsoft-sso not found (404)");
    throw new Error("404: /auth/microsoft-sso endpoint not found.");
  }

  if (!res.ok) {
    console.error("microsoftSsoAuth failed", { status: res.status, body: parsed || raw });
    throw new Error(`Microsoft SSO auth failed (status ${res.status}) ${raw}`);
  }

  console.debug("microsoftSsoAuth response parsed:", parsed || raw);
  // Extra debug: log common response fields so we can verify backend returned user and token
  try {
    const fullResp = parsed || raw;
    console.debug('microsoftSsoAuth fullResponse:', fullResp);
    if (parsed && typeof parsed === 'object') {
      console.debug('microsoftSsoAuth graphToken:', parsed.graphToken ?? null);
      console.debug('microsoftSsoAuth exchangeType:', parsed.exchangeType ?? null);
      console.debug('microsoftSsoAuth nested user candidate:', parsed.user ?? parsed.data ?? null);
    }
  } catch (dbgErr) {
    console.debug('microsoftSsoAuth extra debug failed:', dbgErr?.message || dbgErr);
  }
  // If the backend returned a user DTO (or wrapped user), persist a normalized user to localStorage
  try {
    const maybeUser = (parsed && (parsed.user || parsed.data)) ? (parsed.user || parsed.data) : parsed;
    if (maybeUser && (maybeUser.id || maybeUser.azureAdId || maybeUser.email || maybeUser.name)) {
      const isGuid = (val) => typeof val === 'string' && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(val);
      const normalized = {
        id: maybeUser.id ?? maybeUser.userId ?? maybeUser.localId ?? 'unknown',
        azureAdId: maybeUser.azureAdId ?? maybeUser.oid ?? maybeUser.azureAdObjectId ?? (isGuid(maybeUser.id) ? maybeUser.id : null),
        name: maybeUser.name ?? maybeUser.displayName ?? maybeUser.fullName ?? null,
        email: maybeUser.email ?? maybeUser.mail ?? maybeUser.userPrincipalName ?? maybeUser.preferred_username ?? maybeUser.upn ?? null,
      };
      try {
        localStorage.setItem('user', JSON.stringify(normalized));
        console.debug('microsoftSsoAuth saved normalized user to localStorage:', normalized);
      } catch (lsErr) {
        console.debug('microsoftSsoAuth: failed to write user to localStorage', lsErr?.message || lsErr);
      }
    } else if (typeof (parsed || raw) === 'string') {
      // If the response is a raw token (string), attempt to parse JWT claims and derive a minimal user
      const tokenStr = parsed || raw;
      try {
        const parts = tokenStr.split('.');
        if (parts.length > 1) {
          const payload = parts[1];
          const b64 = payload.replace(/-/g, '+').replace(/_/g, '/');
          let claims = null;
          try {
            claims = JSON.parse(atob(b64));
          } catch (e) {
            try {
              const json = decodeURIComponent(
                atob(b64)
                  .split('')
                  .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                  .join('')
              );
              claims = JSON.parse(json);
            } catch (e2) {
              claims = null;
            }
          }

          if (claims) {
            const derived = {
              id: claims.oid || claims.sub || claims.upn || claims.preferred_username || claims.email || 'unknown',
              azureAdId: claims.oid || claims.sub || null,
              name: claims.name || [claims.given_name, claims.family_name].filter(Boolean).join(' ') || claims.preferred_username || claims.email || null,
              email: claims.preferred_username || claims.upn || claims.email || null,
            };
            try {
              localStorage.setItem('user', JSON.stringify(derived));
              console.debug('microsoftSsoAuth derived and saved user from token to localStorage:', derived);
            } catch (lsErr2) {
              console.debug('microsoftSsoAuth: failed to write derived user to localStorage', lsErr2?.message || lsErr2);
            }
          }
        }
      } catch (tokErr) {
        console.debug('microsoftSsoAuth: token-derived user parsing failed', tokErr?.message || tokErr);
      }
    }
    // If parsed is an object with a graphToken but no nested user details,
    // try to parse the graphToken JWT and derive a minimal user.
    if (parsed && typeof parsed === 'object' && parsed.graphToken) {
      const nested = parsed.user || parsed.data || null;
      const isEmptyNested = nested && typeof nested === 'object' && Object.keys(nested).length === 0;
      if (!nested || isEmptyNested) {
        try {
          const tokenStr = parsed.graphToken;
          const parts = tokenStr.split('.');
          if (parts.length > 1) {
            const payload = parts[1];
            const b64 = payload.replace(/-/g, '+').replace(/_/g, '/');
            let claims = null;
            try { claims = JSON.parse(atob(b64)); } catch (e) {
              try {
                const json = decodeURIComponent(
                  atob(b64)
                    .split('')
                    .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
                    .join('')
                );
                claims = JSON.parse(json);
              } catch (e2) { claims = null; }
            }

            if (claims) {
              const derivedFromGraph = {
                id: claims.oid || claims.sub || claims.upn || claims.preferred_username || claims.email || 'unknown',
                azureAdId: claims.oid || claims.sub || null,
                name: claims.name || [claims.given_name, claims.family_name].filter(Boolean).join(' ') || claims.preferred_username || claims.email || null,
                email: claims.preferred_username || claims.upn || claims.email || null,
              };
              try {
                localStorage.setItem('user', JSON.stringify(derivedFromGraph));
                console.debug('microsoftSsoAuth parsed graphToken and saved derived user to localStorage:', derivedFromGraph);
              } catch (lsErr3) {
                console.debug('microsoftSsoAuth: failed to write derivedFromGraph to localStorage', lsErr3?.message || lsErr3);
              }
            }
          }
        } catch (errTokenParse) {
          console.debug('microsoftSsoAuth: parsing parsed.graphToken failed', errTokenParse?.message || errTokenParse);
        }
      }
    }
  } catch (e) {
    console.debug('microsoftSsoAuth: error while attempting to persist user to localStorage', e?.message || e);
  }

  return parsed || { raw };
}

// Debug helper: attempt to get Teams SSO token and log details (useful in Teams devconsole)
export async function debugGetTeamsSsoToken() {
  try {
    console.log("[debug] attempting getTeamsSsoToken...");
    const t = await getTeamsSsoToken();
    console.log("[debug] getTeamsSsoToken returned:", t ? `[token length ${t.length}]` : t);
    return t;
  } catch (e) {
    console.error("[debug] getTeamsSsoToken error:", e?.message || e);
    return null;
  }
}

// NEW: unified login attempt using a Teams (getAuthToken/authenticate) token or MSAL id/access token.
// Tries normal microsoftAuth first; if it fails with a 401 / format issue, falls back to microsoftSsoAuth.
export async function unifiedMicrosoftLogin(token) {
  if (isInTeams()) {
    try {
      return await microsoftSsoAuth(token);
    } catch (primary) {
      console.warn("Primary Teams SSO endpoint failed, trying microsoftAuth fallback", primary);
      try {
        return await microsoftAuth(token);
      } catch (fallback) {
        throw new Error(`Unified Microsoft Teams login failed. SSO: ${primary.message}. Fallback: ${fallback.message}`);
      }
    }
  }
  // Browser path unchanged
  try {
    return await microsoftAuth(token);
  } catch (e) {
    const msg = e?.message || '';
    if (/401|invalid/i.test(msg)) {
      try {
        return await microsoftSsoAuth(token);
      } catch (e2) {
        throw new Error(`Unified Microsoft login failed. Primary: ${msg}. Fallback: ${e2.message || e2}`);
      }
    }
    throw e;
  }
}

// Comments
// Fetch comments for a task
export async function fetchComments(taskId) {
  const res = await fetch(`${API_URL}/tasks/${taskId}/comment`);
  return res.json();
}

// Add a comment to a task
export async function addComment(taskId, comment) {
  const res = await fetch(`${API_URL}/tasks/${taskId}/comment`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(comment),
  });
  return res.json();
}

// Subtasks
// Fetch subtasks for a task
export async function fetchSubtasks(taskId) {
  const res = await fetch(`${API_URL}/tasks/${taskId}/subtask`);
  return res.json();
}

// Add a subtask to a task
export async function addSubtask(taskId, subtask) {
  const res = await fetch(`${API_URL}/tasks/${taskId}/subtask`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(subtask),
  });
  return res.json();
}

// Complete a subtask
export async function completeSubtask(taskId, subtaskId, subtask) {
  const res = await fetch(`${API_URL}/tasks/${taskId}/subtask/${subtaskId}`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(subtask),
  });
  return res.json();
}

// Update a subtask
export async function updateSubtask(taskId, subtaskId, subtask) {
  const res = await fetch(`${API_URL}/tasks/${taskId}/subtask/${subtaskId}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(subtask),
  });
  return res.json();
}

// Delete a subtask
export async function deleteSubtask(taskId, subtaskId) {
  const res = await fetch(`${API_URL}/tasks/${taskId}/subtask/${subtaskId}`, {
    method: "DELETE",
  });
  if (!res.ok) {
    throw new Error("Failed to delete subtask");
  }
  return res.json();
}

// Add optional directory sync after SSO
export async function syncTeamsDirectory(token) {
  // Backend should validate bearer and sync users from Graph
  const res = await fetch(`${API_URL}/users/teams-sync`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${token}`,
      "Content-Type": "application/json"
    }
  });
  if (!res.ok) {
    console.warn("Teams directory sync failed:", res.status);
    return null;
  }
  return res.json();
}