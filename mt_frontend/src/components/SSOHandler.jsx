// import { useEffect } from "react";
// import * as microsoftTeams from "@microsoft/teams-js";

// export default function SSOHandler({ setCurrentUser, setRefreshTrigger }) {
//   useEffect(() => {
//     // Check if running inside Teams
//     if (window.parent !== window) {
//       microsoftTeams.app.initialize()
//         .then(() => {
//           microsoftTeams.authentication.getAuthToken({
//             successCallback: async (token) => {
//               console.log("SSO token received:", token);

//               try {
//                 const response = await fetch("https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api", {
//                   method: "POST",
//                   headers: {
//                     Authorization: `Bearer ${token}`,
//                     "Content-Type": "application/json"
//                   }
//                 });

//                 if (response.ok) {
//                   const user = await response.json();
//                   setCurrentUser(user);
//                   setRefreshTrigger((prev) => prev + 1);
//                 } else {
//                   console.error("Token validation failed:", await response.text());
//                 }
//               } catch (err) {
//                 console.error("Error calling backend:", err);
//               }
//             },
//             failureCallback: (error) => {
//               console.error("SSO failed:", error);
//             }
//           });
//         })
//         .catch((err) => {
//           console.warn("Teams SDK initialization failed:", err);
//         });
//     } else {
//       console.log("App is running in browser — skipping Teams SSO.");
//     }
//   }, [setCurrentUser, setRefreshTrigger]);

//   return null;
// }

// // import { useEffect } from "react";
// // import * as microsoftTeams from "@microsoft/teams-js";

// // export default function SSOHandler({ setCurrentUser, setRefreshTrigger }) {
// //   useEffect(() => {
// //     // Check if running inside Teams
// //     if (window.parent !== window) {
// //       microsoftTeams.app.initialize()
// //         .then(() => {
// //           microsoftTeams.app.getContext().then(() => {
// //             microsoftTeams.authentication.getAuthToken({
// //               successCallback: (token) => {
// //                 // Send token to backend to validate and get user info
// //                 fetch("/api/auth/validate", {
// //                   method: "POST",
// //                   headers: {
// //                     Authorization: `Bearer ${token}`,
// //                   },
// //                 })
// //                   .then((res) => res.json())
// //                   .then((user) => {
// //                     setCurrentUser(user);
// //                     setRefreshTrigger((prev) => prev + 1);
// //                   });
// //               },
// //               failureCallback: (error) => {
// //                 console.error("SSO failed:", error);
// //               },
// //             });
// //           });
// //         })
// //         .catch((err) => {
// //           console.warn("Not running inside Microsoft Teams or initialization failed:", err);
// //         });
// //     } else {
// //       console.log("App is running in browser — skipping Teams SSO.");
// //     }
// //   }, []);

// //   return null;
// // }

// import { useEffect } from "react";
// import * as microsoftTeams from "@microsoft/teams-js";

// export default function SSOHandler({ setCurrentUser, setRefreshTrigger }) {
//   useEffect(() => {
//     // Check if running inside Microsoft Teams (iframe)
//     const isInTeams = window.parent !== window;

//     if (isInTeams) {
//       microsoftTeams.app
//         .initialize()
//         .then(() => {
//           // microsoftTeams.authentication.getAuthToken({
//           //   successCallback: async (token) => {
//           //     console.log("SSO token received:", token);

//           //     try {
//           //       const response = await fetch(
//           //         "https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api/users/msal-login",
//           //         {
//           //           method: "POST",
//           //           headers: {
//           //             Authorization: `Bearer ${token}`,
//           //             "Content-Type": "application/json",
//           //           },
//           //         }
//           //       );

//           //       if (response.ok) {
//           //         const data = await response.json();
//           //         localStorage.setItem("user", JSON.stringify(data.user));
//           //         localStorage.setItem("accessToken", token);
//           //         setCurrentUser(data.user);
//           //         setRefreshTrigger((prev) => prev + 1);
//           //       } else {
//           //         console.error(
//           //           "Token validation failed:",
//           //           await response.text()
//           //         );
//           //       }
//           //     } catch (err) {
//           //       console.error("Error calling backend:", err);
//           //     }
//           //   },
//           //   failureCallback: (error) => {
//           //     console.error("SSO failed:", error);
//           //   },
//           // });
//           microsoftTeams.authentication.getAuthToken({
//             successCallback: async (token) => {
//               console.log("SSO token received:", token);

//               const response = await fetch(
//                 "https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api/users/msal-login?saveUser=true",
//                 {
//                   method: "POST",
//                   headers: {
//                     Authorization: `Bearer ${token}`,
//                     "Content-Type": "application/json",
//                   },
//                 }
//               );

//               const data = await response.json();
//               localStorage.setItem("user", JSON.stringify(data.user));
//               localStorage.setItem("accessToken", token);
//               setCurrentUser(data.user);
//               setRefreshTrigger((prev) => prev + 1);
//             },
//             failureCallback: (error) => {
//               console.error("SSO failed:", error);
//             },
//           });
//         })
//         .catch((err) => {
//           console.warn("Teams SDK initialization failed:", err);
//         });
//     } else {
//       console.log("App is running in browser — skipping Teams SSO.");
//     }
//   }, [setCurrentUser, setRefreshTrigger]);

//   return null;
// }

//=============CURRENT STABLE=======================
// import { useEffect } from "react";
// import * as microsoftTeams from "@microsoft/teams-js";

// export default function SSOHandler({ setCurrentUser, setRefreshTrigger }) {
//   useEffect(() => {
//     const isInTeams = window.parent !== window;

//     console.log("inside sso handler");
//     console.log("Is in Teams:", isInTeams);
//     console.log("Current user in SSOHandler:", setCurrentUser);

//     if (isInTeams) {
//       microsoftTeams.app.initialize()
//         .then(async () => {
//           try {
//             const token = await microsoftTeams.authentication.getAuthToken({
//               silent: true
//             });

//             console.log("SSO token received:", token);

//             const response = await fetch("https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api/users/teams-sso-login", {
//               method: "POST",
//               headers: {
//                 Authorization: `Bearer ${token}`,
//                 "Content-Type": "application/json"
//               }
//             });

//             if (response.ok) {
//               const data = await response.json();
//               localStorage.setItem("user", JSON.stringify(data.user));
//               localStorage.setItem("accessToken", token);
//               setCurrentUser(data.user);
//               setRefreshTrigger(prev => prev + 1);
//             } else {
//               console.error("Token validation failed:", await response.text());
//             }
//           } catch (error) {
//             console.error("SSO failed:", error);
//           }
//         })
//         .catch((err) => {
//           console.warn("Teams SDK initialization failed:", err);
//         });
//     } else {
//       console.log("App is running in browser — skipping Teams SSO.");
//     }
//   }, [setCurrentUser, setRefreshTrigger]);

//   return null;
// }
//=========================================

import { useEffect } from "react";
import * as microsoftTeams from "@microsoft/teams-js";

export default function SSOHandler({ setCurrentUser, setRefreshTrigger }) {
  useEffect(() => {
    const isInTeams = window.parent !== window;
    console.log("inside sso handler");
    console.log("Is in Teams:", isInTeams);

    if (!isInTeams) {
      console.log("App is running in browser — skipping Teams SSO.");
      return;
    }

    microsoftTeams.app.initialize()
      .then(async () => {
        try {
          const token = await microsoftTeams.authentication.getAuthToken({ silent: true });
          console.log("SSO token received:", token);

          const response = await fetch(
            "https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api/users/teams-sso-login",
            {
              method: "POST",
              headers: {
                Authorization: `Bearer ${token}`,
                "Content-Type": "application/json",
              },
            }
          );

          if (!response.ok) {
            const errorText = await response.text();
            console.error("Token validation failed:", errorText);
            return;
          }

          const data = await response.json();
          console.log("SSO login successful:", data);

          localStorage.setItem("user", JSON.stringify(data.user));
          localStorage.setItem("accessToken", token);
          setCurrentUser(data.user);
          setRefreshTrigger((prev) => prev + 1);
        } catch (error) {
          console.error("SSO failed:", error);
        }
      })
      .catch((err) => {
        console.warn("Teams SDK initialization failed:", err);
      });
  }, [setCurrentUser, setRefreshTrigger]);

  return null;
}

