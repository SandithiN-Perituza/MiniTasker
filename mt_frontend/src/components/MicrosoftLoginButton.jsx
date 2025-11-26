import React, { useState, useContext } from "react";
import UserContext from "../context/UserContext";
import { isInTeams, isTeamsDesktop } from "../utils/teams";
import { unifiedMicrosoftLogin } from "../api/api";
import { getTeamsSsoToken } from "../utils/teamsAuth";

/**
 * MicrosoftLoginButton: unified login button that chooses Teams SSO path when inside Teams,
 * otherwise falls back to standard browser MSAL flow provided by UserProvider.loginWithMicrosoft.
 */
export default function MicrosoftLoginButton({ onSuccess, onError }) {
  const { loginWithMicrosoft, login, user } = useContext(UserContext);
  const [loading, setLoading] = useState(false);

  async function handleClick() {
    if (loading) return; // guard
    setLoading(true);
    try {
      if (isInTeams() && window.microsoftTeams) {
        // Use Teams SSO helper but guard with a timeout to avoid hanging
        const timeoutMs = 15000;
        const timeoutPromise = new Promise((_, reject) => setTimeout(() => reject(new Error('Teams SSO timed out')), timeoutMs));

        let token;
        try {
          token = await Promise.race([getTeamsSsoToken(), timeoutPromise]);
        } catch (teamsErr) {
          console.error('Teams SSO failed or timed out:', teamsErr);
          // If desktop client likely cannot perform in-app auth, surface error and avoid silent failure
          if (isTeamsDesktop()) {
            const msg = 'Teams authentication failed or timed out in the desktop client. Please try signing in using the web app or contact your administrator.';
            try { window.alert(msg); } catch (e) { console.log('Alert failed:', e); }
            onError && onError(teamsErr);
            return;
          }

          // For webviews (non-desktop) attempt browser MSAL fallback
          console.warn('Falling back to browser MSAL flow after Teams SSO failure');
          const userData = await loginWithMicrosoft();
          if (userData) {
            login && login(userData);
            onSuccess && onSuccess(userData);
            return userData;
          }
        }

        const savedUser = await unifiedMicrosoftLogin(token);
        console.log('[MicrosoftLoginButton] Teams login successful, savedUser:', savedUser);
        
        // Save to localStorage
        localStorage.setItem("user", JSON.stringify(savedUser));
        
        // Set flag to prevent reload loop
        sessionStorage.setItem('teams_login_reload', 'done');
        
        // Force reload to update UI in Teams
        window.location.reload();
        return;
      }

      // Browser (non-Teams) flow using MSAL logic in UserProvider
      const userData = await loginWithMicrosoft();
      if (userData) {
        onSuccess && onSuccess(userData);
        return userData;
      }
    } catch (err) {
      console.error("Microsoft login failed", err);
      onError && onError(err);
      // Re-throw to allow upstream error handling if caller uses promises
      throw err;
    } finally {
      setLoading(false);
    }
  }

  const inTeams = isInTeams();
  const label = loading
    ? "Signing in..."
    : inTeams
      ? "Sign in with Microsoft (Teams)"
      : "Sign in with Microsoft";

  const alreadyLoggedIn = !!user;
  return (
    <button
      type="button"
      onClick={handleClick}
      disabled={loading || alreadyLoggedIn}
      title={alreadyLoggedIn ? 'Already signed in' : undefined}
      className="w-full flex items-center justify-center gap-2 border border-gray-300 rounded px-4 py-2 hover:bg-gray-50 disabled:opacity-50"
    >
      {alreadyLoggedIn ? 'Signed in' : label}
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
