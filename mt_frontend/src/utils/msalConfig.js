// // src/utils/msalConfig.js
// import { PublicClientApplication } from "@azure/msal-browser";

// const msalConfig = {
//   auth: {
//     clientId: "f6c2a5e9-3bd5-4223-ad2c-618846a668c5", // from Azure AD
//     authority: "https://login.microsoftonline.com/7b967b11-c0b9-402b-b483-d694f50dfb82",
//     redirectUri: "https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net",
//     postLogoutRedirectUri: "https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net",
//     navigateToLoginRequestUrl: false
//   },
//   cache: {
//     cacheLocation: "sessionStorage", // Use sessionStorage instead of localStorage for web apps
//     storeAuthStateInCookie: true, // Set to true for web apps to support IE11 and Edge
//   },
//   system: {
//     loggerOptions: {
//       loggerCallback: (level, message, containsPii) => {
//         if (containsPii) {
//           return;
//         }
//         switch (level) {
//           case "Error":
//             console.error(message);
//             return;
//           case "Info":
//             console.info(message);
//             return;
//           case "Verbose":
//             console.debug(message);
//             return;
//           case "Warning":
//             console.warn(message);
//             return;
//         }
//       }
//     }
//   }
// };

// export const msalInstance = new PublicClientApplication(msalConfig);

// // Initialize MSAL instance
// msalInstance.initialize().then(() => {
//   console.log("MSAL initialized successfully");
// }).catch((error) => {
//   console.error("MSAL initialization failed:", error);
// });