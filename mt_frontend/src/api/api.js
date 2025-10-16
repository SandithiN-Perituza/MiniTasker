// const API_URL = "https://localhost:7296/api";
const API_URL = "https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api";

// Tasks

import { msalInstance, ensureMsalInitialized } from "../authConfig";
import { getCurrentUser } from "../utils/auth";

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
        } catch (e) {
          // Keep original text if not JSON
        }
        
        throw new Error(`Server error (500): ${errorDetails}. Check backend logs for more details.`);
      }
      
      throw new Error(`Notification failed (${response.status}): ${responseText || 'Unknown error'}`);
    }

    // Parse response as JSON if possible
    let result;
    try {
      result = JSON.parse(responseText);
    } catch (e) {
      result = { message: responseText, success: true };
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

// Create a new task
export async function createTask(task) {
  const res = await fetch(`${API_URL}/tasks`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(task),
  });
  return res.json();
}

// Update an existing task
export async function updateTask(id, task) {
  await fetch(`${API_URL}/tasks/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(task),
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