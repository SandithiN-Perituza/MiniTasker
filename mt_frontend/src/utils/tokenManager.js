// Token management utilities
import { getCurrentUser } from "./auth";
import { msalInstance, ensureMsalInitialized } from "../authConfig";

const API_URL = "https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api";

// Force acquire a fresh authentication token
export async function forceTokenRefresh() {
  console.log("🔄 Attempting to force token refresh...");
  
  try {
    // Method 1: Try MSAL silent token acquisition
    await ensureMsalInitialized();
    const accounts = msalInstance.getAllAccounts();
    if (accounts.length > 0) {
      console.log("Found MSAL account, attempting silent token acquisition");
      const tokenRequest = {
        scopes: ["api://59aef810-e681-4b84-bc17-2561fe854c0e/access_as_user"], // Use API scope instead of User.Read
        account: accounts[0]
      };
      
      try {
        const response = await msalInstance.acquireTokenSilent(tokenRequest);
        if (response.accessToken) {
          localStorage.setItem("accessToken", response.accessToken);
          localStorage.setItem("idToken", response.idToken || "");
          console.log("✅ Successfully acquired MSAL token");
          return response.accessToken;
        }
      } catch (msalError) {
        console.log("MSAL silent acquisition failed:", msalError);
        
        // Try popup acquisition as fallback
        try {
          const popupResponse = await msalInstance.acquireTokenPopup(tokenRequest);
          if (popupResponse.accessToken) {
            localStorage.setItem("accessToken", popupResponse.accessToken);
            localStorage.setItem("idToken", popupResponse.idToken || "");
            console.log("✅ Successfully acquired MSAL token via popup");
            return popupResponse.accessToken;
          }
        } catch (popupError) {
          console.log("MSAL popup acquisition failed:", popupError);
        }
      }
    }

    // Method 2: Try to re-authenticate with backend
    const currentUser = getCurrentUser();
    if (currentUser && currentUser.email) {
      console.log("Attempting backend re-authentication");
      
      // This assumes your backend has a token refresh endpoint
      const refreshResponse = await fetch(`${API_URL}/auth/refresh-token`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ 
          userId: currentUser.id, 
          email: currentUser.email,
          name: currentUser.name
        })
      });
      
      if (refreshResponse.ok) {
        const data = await refreshResponse.json();
        if (data.token || data.accessToken || data.jwt) {
          const newToken = data.token || data.accessToken || data.jwt;
          localStorage.setItem("accessToken", newToken);
          console.log("✅ Successfully refreshed token via backend");
          return newToken;
        }
      } else {
        console.log("Backend token refresh failed:", refreshResponse.status);
      }
    }

    console.log("❌ All token refresh methods failed");
    return null;
  } catch (error) {
    console.error("❌ Token refresh error:", error);
    return null;
  }
}

// Check if we have a valid token
export function hasValidToken() {
  const tokens = [
    localStorage.getItem("accessToken"),
    localStorage.getItem("idToken"),
    localStorage.getItem("authToken"),
    localStorage.getItem("jwt")
  ];
  
  return tokens.some(token => token && token.length > 10);
}

// Get the best available token
export function getBestToken() {
  const tokens = [
    localStorage.getItem("accessToken"),
    localStorage.getItem("idToken"),
    localStorage.getItem("authToken"),
    localStorage.getItem("jwt")
  ];
  
  return tokens.find(token => token && token.length > 10) || null;
}

// Clear all tokens
export function clearAllTokens() {
  const tokenKeys = ["accessToken", "idToken", "authToken", "jwt", "bearerToken"];
  tokenKeys.forEach(key => localStorage.removeItem(key));
  console.log("🧹 Cleared all authentication tokens");
}