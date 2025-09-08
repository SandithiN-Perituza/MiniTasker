import React from "react";
import { useMsal } from "@azure/msal-react";
import { loginRequest } from "../authConfig";

export default function MicrosoftLoginButton() {
  const { instance } = useMsal();

  const handleMicrosoftLogin = () => {
    instance.loginRedirect(loginRequest);
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

// import React, { useEffect, useState } from "react";
// import { useMsal } from "@azure/msal-react";
// import { loginRequest } from "../authConfig";
// import * as microsoftTeams from "@microsoft/teams-js";

// export default function MicrosoftLoginButton() {
//   const { instance } = useMsal();
//   const [isInTeams, setIsInTeams] = useState(false);

//   useEffect(() => {
//     // Detect if app is running inside Microsoft Teams
//     microsoftTeams.app.initialize().then(() => {
//       microsoftTeams.app.getContext().then(() => {
//         setIsInTeams(true);
//       });
//     }).catch(() => {
//       setIsInTeams(false);
//     });
//   }, []);

//   const handleLogin = async () => {
//     try {
//       instance.loginRedirect(loginRequest);
//       if (isInTeams) {
//         // Use redirect login inside Teams
//         instance.loginRedirect(loginRequest);
//       } else {
//         // Use popup login in browser
//         await instance.loginPopup(loginRequest);
//         const tokenResponse = await instance.acquireTokenSilent(loginRequest);
//         const accessToken = tokenResponse.accessToken;

//         // Optional: Send token to backend
//         const response = await fetch("https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api/tasks", {
//           headers: {
//             Authorization: `Bearer ${accessToken}`
//           }
//         });

//         const data = await response.json();
//         console.log("Tasks from backend:", data);
//       }
//     } catch (error) {
//       console.error("Login error:", error);
//     }
//   };

//   return (
//     <button onClick={handleLogin} className="py-2 px-4 rounded hover:bg-blue-100 text-left">
//       Login with Microsoft
//     </button>
//   );
// }
