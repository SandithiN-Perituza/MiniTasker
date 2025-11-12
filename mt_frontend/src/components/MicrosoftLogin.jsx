import { useMsal } from "@azure/msal-react";
import { useState } from "react";

const MicrosoftLogin = () => {
  const { instance } = useMsal();
  const [isLoggingIn, setIsLoggingIn] = useState(false);

  const handleMicrosoftLogin = async () => {
    if (isLoggingIn) return;
    
    setIsLoggingIn(true);
    
    try {
      // Ensure instance is initialized
      if (!instance.getConfiguration()) {
        throw new Error("MSAL instance not properly initialized");
      }

      const loginRequest = {
        scopes: ["User.Read"],
        prompt: "select_account"
      };

      const result = await instance.loginPopup(loginRequest);
      
      if (result) {
        console.log("Login successful:", result);
        // Handle successful login
      }
    } catch (error) {
      console.error("Login failed:", error);
      // Handle login error
    } finally {
      setIsLoggingIn(false);
    }
  };

  return (
    <button 
      onClick={handleMicrosoftLogin} 
      disabled={isLoggingIn}
      className="microsoft-login-btn"
    >
      {isLoggingIn ? "Signing in..." : "Sign in with Microsoft"}
    </button>
  );
};

export default MicrosoftLogin;
