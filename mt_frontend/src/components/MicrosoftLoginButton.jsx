import React, { useState, useContext } from "react";
import UserContext from "../context/UserContext";
import { isInTeams } from "../utils/teams";
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
        // Use unified teamsLogin helper (handles silent + interactive)
        const token = await getTeamsSsoToken();
        const savedUser = await unifiedMicrosoftLogin(token);
        login && login(savedUser);
        onSuccess && onSuccess(savedUser);
        return savedUser;
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
