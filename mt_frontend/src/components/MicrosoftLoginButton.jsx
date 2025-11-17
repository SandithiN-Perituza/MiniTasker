import React, { useState, useContext } from "react";
import UserContext from "../context/UserContext";
import { isInTeams } from "../utils/teams";
import { msalConfig, apiRequest } from "../authConfig";

export default function MicrosoftLoginButton({ onSuccess, onError }) {
  const { loginWithMicrosoft } = useContext(UserContext);
  const [loading, setLoading] = useState(false);

  async function handleClick() {
    setLoading(true);
    try {
      const u = await loginWithMicrosoft();
      onSuccess && onSuccess(u);
    } catch (e) {
      onError && onError(e);
      // Friendly fallback: open system browser to Azure authorize URL
      try {
        const clientId = msalConfig.auth.clientId;
        const redirectUri = msalConfig.auth.redirectUri || window.location.origin + "/teams-auth-start.html";
        const scope = (apiRequest && apiRequest.scopes && apiRequest.scopes[0]) || "api://59aef810-e681-4b84-bc17-2561fe854c0e/access_as_user";
        const authorizeUrl = `https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id=${encodeURIComponent(clientId)}&response_type=token&scope=${encodeURIComponent(scope)}&redirect_uri=${encodeURIComponent(redirectUri)}&prompt=select_account`;
        // Open in new window (system browser from Teams desktop)
        window.open(authorizeUrl, "_blank");
        alert("Sign-in failed inside Teams client. A browser window was opened to complete sign-in. After signing in, return to Teams and try again.");
      } catch (fallbackErr) {
        console.warn("Fallback open browser failed:", fallbackErr);
      }
    } finally {
      setLoading(false);
    }
  }

  const inTeams = isInTeams();

  return (
    <button
      type="button"
      onClick={handleClick}
      disabled={loading}
      className="w-full flex items-center justify-center gap-2 border border-gray-300 rounded px-4 py-2 hover:bg-gray-50 disabled:opacity-50"
    >
      {loading
        ? "Signing in..."
        : inTeams
        ? "Sign in with Microsoft (Teams)"
        : "Sign in with Microsoft"}
    </button>
  );
}


// import { useMsal } from "@azure/msal-react";
// import { useState } from "react";

// const MicrosoftLogin = () => {
//   const { instance } = useMsal();
//   const [isLoggingIn, setIsLoggingIn] = useState(false);

//   const handleMicrosoftLogin = async () => {
//     if (isLoggingIn) return;
    
//     setIsLoggingIn(true);
    
//     try {
//       // Ensure instance is initialized
//       if (!instance.getConfiguration()) {
//         throw new Error("MSAL instance not properly initialized");
//       }

//       const loginRequest = {
//         scopes: ["User.Read"],
//         prompt: "select_account"
//       };

//       const result = await instance.loginPopup(loginRequest);
      
//       if (result) {
//         console.log("Login successful:", result);
//         // Handle successful login
//       }
//     } catch (error) {
//       console.error("Login failed:", error);
//       // Handle login error
//     } finally {
//       setIsLoggingIn(false);
//     }
//   };

//   return (
//     <button 
//       onClick={handleMicrosoftLogin} 
//       disabled={isLoggingIn}
//       className="microsoft-login-btn"
//     >
//       {isLoggingIn ? "Signing in..." : "Sign in with Microsoft"}
//     </button>
//   );
// };

// export default MicrosoftLogin;
