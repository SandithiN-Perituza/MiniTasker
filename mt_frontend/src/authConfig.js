export const msalConfig = {
  auth: {
    clientId: "YOUR_CLIENT_ID", // from Azure AD App Registration
    authority: "https://login.microsoftonline.com/YOUR_TENANT_ID", // your tenant ID
    redirectUri: "http://localhost:5173", // or your deployed URL
  },
  cache: {
    cacheLocation: "localStorage", // or "sessionStorage"
    storeAuthStateInCookie: false,
  },
};