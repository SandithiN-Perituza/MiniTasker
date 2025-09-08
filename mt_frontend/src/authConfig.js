// export const msalConfig = {
//   auth: {
//     clientId: "YOUR_CLIENT_ID", // from Azure AD App Registration
//     authority: "https://login.microsoftonline.com/YOUR_TENANT_ID", // your tenant ID
//     redirectUri: "http://localhost:5173", // or your deployed URL
//   },
//   cache: {
//     cacheLocation: "localStorage", // or "sessionStorage"
//     storeAuthStateInCookie: false,
//   },
// };

// export const msalConfig = {
//   auth: {
//     clientId: "YOUR_CLIENT_ID", // from Azure AD App Registration
//     authority: "https://login.microsoftonline.com/YOUR_TENANT_ID", // your tenant ID
//     redirectUri: "https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net/auth/callback", // deployed URL
//   },
//   cache: {
//     cacheLocation: "localStorage",
//     storeAuthStateInCookie: false,
//   },
// };

// var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

import { PublicClientApplication } from "@azure/msal-browser";
 
const msalConfig = {
  auth: {
    clientId: "3ee5758e-f933-4383-ba25-8c5f0fb848a4", 
    authority: "https://login.microsoftonline.com/7b967b11-c0b9-402b-b483-d694f50dfb82", // Tenant ID
    redirectUri: "https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net/auth/callback", // Deployed URL 
  },
  cache: {
    cacheLocation: "localStorage",
    storeAuthStateInCookie: false,
  },
};

export const loginRequest = {
  scopes: ["openid", "profile", "email", "api://086fdd43-c0b7-4997-a181-dbf938026ae5/access_as_user"]
}

export const msalInstance = new PublicClientApplication(msalConfig);