// src/utils/msalConfig.js
import { PublicClientApplication } from "@azure/msal-browser";

const msalConfig = {
  auth: {
    clientId: "3ee5758e-f933-4383-ba25-8c5f0fb848a4", // from Azure AD
    authority: "https://login.microsoftonline.com/7b967b11-c0b9-402b-b483-d694f50dfb82",
    redirectUri: "https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net",
  },
};

export const msalInstance = new PublicClientApplication(msalConfig);