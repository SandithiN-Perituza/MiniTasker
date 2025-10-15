import React, { useState, useContext } from "react";
import UserContext from "../context/UserContext";
import { isInTeams } from "../utils/teams";

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
    } finally {
      setLoading(false);
    }
  }

  if (isInTeams()) return null;

  return (
    <button
      type="button"
      onClick={handleClick}
      disabled={loading}
      className="w-full flex items-center justify-center gap-2 border border-gray-300 rounded px-4 py-2 hover:bg-gray-50 disabled:opacity-50"
    >
      {loading ? "Signing in..." : "Sign in with Microsoft"}
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
