import { useEffect } from "react";
import { authentication, app } from "@microsoft/teams-js";
import { useMsal } from "@azure/msal-react";
import { loginRequest } from "../authConfig";
import { loginMicrosoftUser } from "../api/api";

export default function SSOHandler({ setCurrentUser, setRefreshTrigger }) {
  const { instance } = useMsal();

  useEffect(() => {
    const isInTeams = window.parent !== window;
    console.log("Is in Teams:", isInTeams);

    if (isInTeams) {
      app.initialize().then(() => {
        authentication.getAuthToken()
          .then(async (token) => {
            console.log("SSO token received:", token);

            try {
              const response = await fetch("https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api/users/msal-login?saveUser=true", {
                method: "POST",
                headers: {
                  Authorization: `Bearer ${token}`,
                  "Content-Type": "application/json"
                }
              });

              if (response.ok) {
                const user = await response.json();
                console.log("SSO login successful:", user);
                setCurrentUser(user);
                setRefreshTrigger((prev) => prev + 1);
                localStorage.setItem("user", JSON.stringify(user));
              } else {
                console.error("Token validation failed:", await response.text());
                fallbackToMsalLogin();
              }
            } catch (err) {
              console.error("Error calling backend:", err);
              fallbackToMsalLogin();
            }
          })
          .catch((error) => {
            console.error("SSO failed:", error);
            fallbackToMsalLogin();
          });
      }).catch((err) => {
        console.warn("Teams SDK initialization failed:", err);
        fallbackToMsalLogin();
      });
    } else {
      console.log("App is running in browser — skipping Teams SSO.");
    }
  }, [setCurrentUser, setRefreshTrigger]);

  const fallbackToMsalLogin = async () => {
    const isInTeams = window.parent !== window;
    if (isInTeams) {
      console.warn("Fallback MSAL login skipped — popups blocked in Teams.");
      return;
    }

    try {
      console.log("Attempting fallback MSAL login...");
      const loginResponse = await instance.loginPopup(loginRequest);
      const account = loginResponse.account;

      const tokenResponse = await instance.acquireTokenSilent({
        ...loginRequest,
        account,
      });

      const accessToken = tokenResponse.accessToken;
      const response = await loginMicrosoftUser(accessToken);

      if (response.user) {
        setCurrentUser(response.user);
        setRefreshTrigger((prev) => prev + 1);
        localStorage.setItem("accessToken", accessToken);
        localStorage.setItem("user", JSON.stringify(response.user));
        console.log("Fallback MSAL login successful:", response.user);
      } else {
        console.error("Fallback MSAL login failed to retrieve user.");
      }
    } catch (error) {
      console.error("Fallback MSAL login error:", error);
    }
  };

  return null;
}

// import { useEffect } from "react";
// import * as microsoftTeams from "@microsoft/teams-js";
// import { useMsal } from "@azure/msal-react";
// import { loginRequest } from "../authConfig";
// import { loginMicrosoftUser } from "../api/api";

// export default function SSOHandler({ setCurrentUser, setRefreshTrigger }) {
//   const { instance } = useMsal();

//   useEffect(() => {
//     // console.log("Inside SSOHandler:", isInTeams);
//     const isInTeams = window.parent !== window;
//     console.log("Is in Teams:", isInTeams);
//     if (isInTeams) {
//       microsoftTeams.app.initialize()
//         .then(() => {
//           microsoftTeams.authentication.getAuthToken({
//             successCallback: async (token) => {
//               console.log("SSO token received:", token);

//               try {
//                 const response = await fetch("https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api/users/msal-login?saveUser=true", {
//                   method: "POST",
//                   headers: {
//                     Authorization: `Bearer ${token}`,
//                     "Content-Type": "application/json"
//                   }
//                 });

//                 if (response.ok) {
//                   const user = await response.json();
//                   console.log("SSO login successful:", user);
//                   setCurrentUser(user);
//                   setRefreshTrigger((prev) => prev + 1);
//                   localStorage.setItem("user", JSON.stringify(user));
//                 } else {
//                   console.error("Token validation failed:", await response.text());
//                   fallbackToMsalLogin(); // fallback if token validation fails
//                 }
//               } catch (err) {
//                 console.error("Error calling backend:", err);
//                 fallbackToMsalLogin(); // fallback on error
//               }
//             },
//             failureCallback: (error) => {
//               console.error("SSO failed:", error);
//               fallbackToMsalLogin(); // fallback on failure
//             }
//           });
//         })
//         .catch((err) => {
//           console.warn("Teams SDK initialization failed:", err);
//           fallbackToMsalLogin(); // fallback if Teams SDK fails
//         });
//     } else {
//       console.log("App is running in browser — skipping Teams SSO.");
//     }
//   }, [setCurrentUser, setRefreshTrigger]);

//   const fallbackToMsalLogin = async () => {
//     try {
//       console.log("Attempting fallback MSAL login...");
//       const loginResponse = await instance.loginPopup(loginRequest);
//       const account = loginResponse.account;

//       const tokenResponse = await instance.acquireTokenSilent({
//         ...loginRequest,
//         account,
//       });

//       const accessToken = tokenResponse.accessToken;
//       const response = await loginMicrosoftUser(accessToken);

//       if (response.user) {
//         setCurrentUser(response.user);
//         setRefreshTrigger((prev) => prev + 1);
//         localStorage.setItem("accessToken", accessToken);
//         localStorage.setItem("user", JSON.stringify(response.user));
//         console.log("Fallback MSAL login successful:", response.user);
//       } else {
//         console.error("Fallback MSAL login failed to retrieve user.");
//       }
//     } catch (error) {
//       console.error("Fallback MSAL login error:", error);
//     }
//   };

//   return null;
// }

