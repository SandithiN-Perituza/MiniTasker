// const API_URL = "https://localhost:7296/api";
const API_URL = "https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api";

// Tasks

import { msalInstance, ensureMsalInitialized } from "../authConfig";
import { getCurrentUser } from "../utils/auth";
import { isInTeams } from "../utils/teams";

// export async function getAuthToken() {
//   try {
//     // First try to get active account
//     let account = msalInstance.getActiveAccount();
    
//     // If no active account, try to get any available account
//     if (!account) {
//       const accounts = msalInstance.getAllAccounts();
//       if (accounts.length > 0) {
//         account = accounts[0];
//         msalInstance.setActiveAccount(account);
//         console.log("Set active account:", account.username);
//       } else {
//         console.log("No MSAL accounts found");
//         return null;
//       }
//     }

//     console.log("Using account for token:", account.username);

//     // Try silent token acquisition
//     const response = await msalInstance.acquireTokenSilent({
//       scopes: ["User.Read"],
//       account: account,
//     });

//     console.log("✅ Successfully acquired MSAL token");
//     return response.accessToken;
//   } catch (error) {
//     console.error("❌ Token acquisition failed:", error);
    
//     // If silent acquisition fails, try popup (optional)
//     try {
//       console.log("Trying popup token acquisition...");
//       const response = await msalInstance.acquireTokenPopup({
//         scopes: ["User.Read"],
//       });
      
//       console.log("✅ Successfully acquired token via popup");
//       return response.accessToken;
//     } catch (popupError) {
//       console.error("❌ Popup token acquisition also failed:", popupError);
//       return null;
//     }
//   }
// }
export async function getAuthToken() {
  console.log("Attempting to get authentication token...");
  
  try {
    // Ensure MSAL is initialized
    await ensureMsalInitialized();
    
    const accounts = msalInstance.getAllAccounts();
    console.log("Available accounts:", accounts.length);
    
    if (accounts.length === 0) {
      throw new Error("No Microsoft accounts found. Please log in with Microsoft first.");
    }
    
    const account = accounts[0];
    console.log("Using account for token:", account.username);
    
    // Use the API scope instead of User.Read for backend authentication
    const tokenRequest = {
      scopes: ["api://59aef810-e681-4b84-bc17-2561fe854c0e/access_as_user"],
      account: account,
      forceRefresh: false
    };
    
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
    throw new Error(`Token acquisition failed: ${error.message}`);
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

    if (!response.ok) {
      console.error("❌ Error response:", responseText);
      
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
  // Vacation Tracker style: rely on backend OBO when in Teams
  if (isInTeams()) {
    console.log("In Teams context – skipping front-end Graph token fetch (backend OBO expected).");
    return null;
  }
  
  console.log("🔍 Acquiring Microsoft Graph token...");
  
  try {
    await ensureMsalInitialized();
    
    const accounts = msalInstance.getAllAccounts();
    if (accounts.length === 0) {
      throw new Error("No Microsoft accounts found. Please log in first.");
    }
    
    const account = accounts[0];
    
    // ✅ Request token specifically for Microsoft Graph
    const tokenRequest = {
      scopes: [
        "https://graph.microsoft.com/User.Read",
        "https://graph.microsoft.com/TeamsActivity.Send",
        
      ],
      account: account,
      forceRefresh: false
    };
    // ,
    // "https://graph.microsoft.com/User.Read.All"
    console.log("📝 Graph token request:", tokenRequest);
    
    try {
      const response = await msalInstance.acquireTokenSilent(tokenRequest);
      console.log("✅ Successfully acquired Microsoft Graph token (silent)");
      return response.accessToken;
    } catch (silentError) {
      console.log("Silent token acquisition failed, trying popup...", silentError);
      
      const response = await msalInstance.acquireTokenPopup(tokenRequest);
      console.log("✅ Successfully acquired Microsoft Graph token (popup)");
      return response.accessToken;
    }
    
  } catch (error) {
    console.error("❌ Failed to get Microsoft Graph token:", error);
    throw new Error(`Graph token acquisition failed: ${error.message}`);
  }
}

// Create a new task
// api/taskApi.js or wherever your createTask function is
export async function createTask(task) {
  console.log("🚀 Creating task with notification support...");

  let backendToken;
  
  try {
    backendToken = await getAuthToken();
    console.log("✅ Authentication token acquired for task creation");
  } catch (tokenError) {
    console.error("❌ Token acquisition failed:", tokenError);
    throw new Error("Could not acquire authentication token. Please try logging in with Microsoft account first.");
  }

    // Get Microsoft Graph token for notifications
  let graphToken;
  try {
    graphToken = await getGraphToken();
    console.log("✅ Microsoft Graph token acquired");
  } catch (graphError) {
    console.warn("⚠️ Graph token acquisition failed:", graphError);
    // Continue without graph token - backend will handle fallback
  }

  let azureUserId, userEmail, userName;
  try {
    const tokenPayload = JSON.parse(atob(backendToken.split('.')[1]));
    azureUserId = tokenPayload.oid || tokenPayload.sub;
    userEmail = tokenPayload.preferred_username || tokenPayload.upn || tokenPayload.email;
    userName = tokenPayload.name;
    
    console.log("📋 User info extracted from token:", {
      azureUserId,
      userEmail,
      userName
    });
  } catch (error) {
    console.error("Failed to parse token:", error);
    throw new Error("Invalid token format");
  }

  // Create the request body with notification data
  const requestBody = {
    message: "Test notification from MiniTasker - Task Assignment",
    userId: azureUserId,
    userEmail: userEmail,
    userName: userName,
    timestamp: new Date().toISOString(),
    graphToken: graphToken,
    task: {
      title: task.title,
      description: task.description,
      assignedTo: task.assignedTo,
      dueDate: task.dueDate
    }
  };

  console.log("📤 Sending task creation request:", {
    url: `${API_URL}/tasks`,
    taskData: requestBody.task,
    userInfo: {
      userId: requestBody.userId,
      userName: requestBody.userName
    }
  });

  try {
    const response = await fetch(`${API_URL}/tasks`, {
      method: "POST",
      headers: { 
        "Content-Type": "application/json",
        "Authorization": `Bearer ${backendToken}` // ✅ Send the actual token
      },
      body: JSON.stringify(requestBody)
    });

    if (!response.ok) {
      const errorText = await response.text();
      console.error("❌ Task creation failed:", response.status, errorText);
      throw new Error(`Task creation failed: ${response.status} - ${errorText}`);
    }

    const result = await response.json();
    console.log("✅ Task created successfully:", result);
    
    if (result.notificationSent) {
      console.log("🔔 Notification sent to assigned user!");
    } else {
      console.log("⚠️ Task created but notification failed");
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
export async function createTaskWithNotification(taskData, assignedUserAzureAdId) {
  console.log("🔔 Creating task with notification...");
  console.log("📋 Task data:", taskData);
  console.log("👤 Assigned user Azure AD ID:", assignedUserAzureAdId);

  const currentUser = getCurrentUser();
  if (!currentUser) {
    throw new Error("Please log in to create tasks");
  }

  let token;
  try {
    token = await getAuthToken();
  } catch (tokenError) {
    console.error("❌ Token acquisition failed:", tokenError);
    throw new Error("Could not acquire authentication token");
  }

  let graphT;
  try {
    graphT = await getGraphToken();
  } catch (graphError) {
    console.warn("⚠️ Graph token acquisition failed:", graphError);
    // Continue without graph token - backend will handle fallback
  }

  // Extract user information from token (same as button)
  let azureUserId, userEmail, userName;

  try {
    const tokenPayload = JSON.parse(atob(token.split('.')[1]));
    azureUserId = tokenPayload.oid || tokenPayload.sub;
    userEmail = tokenPayload.preferred_username || tokenPayload.upn || tokenPayload.email;
    userName = tokenPayload.name;
  } catch (error) {
    console.error("Failed to parse token:", error);
    azureUserId = currentUser.id?.toString();
    userEmail = currentUser.email;
    userName = currentUser.name;
  }

  // ✅ Enhanced request body with assigned user's Azure AD ID
  const requestBody = {
    task: {
      title: taskData.title,
      description: taskData.description,
      assignedTo: parseInt(taskData.assignedTo), // ✅ Convert to integer
      dueDate: taskData.dueDate
    },
    message: `You have been assigned a new task: '${taskData.title}'`,
    userId: azureUserId,           // Creator's Azure AD ID
    userEmail: userEmail,
    userName: userName,
    assignedUserAzureAdId: assignedUserAzureAdId, //Assigned user's Azure AD ID
    authToken: token,           // Backend auth token
    graphToken: graphT,         // Access token for Graph API
    timestamp: new Date().toISOString()
  };

  console.log("🔍 Full request body:", JSON.stringify(requestBody, null, 2));

  const response = await fetch(`${API_URL}/tasks`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Authorization": `Bearer ${token}`,
    },
    body: JSON.stringify(requestBody),
  });

  if (!response.ok) {
    throw new Error(`Task creation failed: ${response.status}`);
  }

  return await response.json();
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
  if (res.status === 404) {
    throw new Error("404: /auth/microsoft-sso endpoint not found.");
  }
  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(`Microsoft SSO auth failed (status ${res.status}) ${text}`);
  }
  return res.json();
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