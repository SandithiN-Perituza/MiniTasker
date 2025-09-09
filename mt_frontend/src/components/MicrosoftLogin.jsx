import React from "react";
import { useMsal } from "@azure/msal-react";
import { loginRequest } from "../authConfig";
import { loginMicrosoftUser } from "../api/api";
import { useNavigate } from "react-router-dom";

export default function MicrosoftLoginButton({ setRefreshTrigger }) {
  const { instance } = useMsal();
  const navigate = useNavigate();

  const handleMicrosoftLogin = async () => {
    try {
      console.log("Inside microsoft login button");
      // Step 1: Login with Microsoft
      const loginResponse = await instance.loginPopup(loginRequest);
      const account = loginResponse.account;

      // Step 2: Acquire token silently
      const tokenResponse = await instance.acquireTokenSilent({
        ...loginRequest,
        account,
      });

      const accessToken = tokenResponse.accessToken;
      console.log("Access Token:", tokenResponse.accessToken)

      
      // Step 3: Send token to backend
      const response = await loginMicrosoftUser(accessToken);
      const user = response.user;
      console.log("Backend response:", response);
      console.log("Microsoft User :", user);

      if (user) {
        localStorage.setItem("accessToken", accessToken);
        localStorage.setItem("user", JSON.stringify(user));
        console.log("Microsoft user logged in:", user);
        
        if (setRefreshTrigger) {
          setRefreshTrigger(prev => prev + 1);
        }

        navigate("/");
      } else {
        console.error("Failed to save Microsoft user.");
      }
    } catch (error) {
      console.error("Microsoft login error:", error);
    }
  };

  return (
    <button
      className="py-2 px-4 rounded hover:bg-blue-100 text-left"
      onClick={handleMicrosoftLogin}
    >
      Login with Microsoft
    </button>
  );
}
