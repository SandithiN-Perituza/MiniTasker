// src/utils/msalConfig.js
import { PublicClientApplication } from "@azure/msal-browser";

const msalConfig = {
  auth: {
    clientId: "3ee5758e-f933-4383-ba25-8c5f0fb848a4",
    authority: "https://login.microsoftonline.com/common", // changed from tenant-specific
    redirectUri: "https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net",
  },
};

export const msalInstance = new PublicClientApplication(msalConfig);