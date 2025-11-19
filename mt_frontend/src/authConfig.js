export const msalConfig = {
  auth: {
    clientId: "f6c2a5e9-3bd5-4223-ad2c-618846a668c5", // Azure AD app (frontend)
    authority: "https://login.microsoftonline.com/7b967b11-c0b9-402b-b483-d694f50dfb82",
    redirectUri: "https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net",
  },
  cache: {
    cacheLocation: "localStorage",
    storeAuthStateInCookie: false,
  },
  // NEW: allow redirect auth when hosted inside an iframe (Teams)
  system: {
    allowRedirectInIframe: true,
  },
};

// NEW: unified API scope (matches Teams manifest webApplicationInfo.resource)
export const apiScope = "api://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net/access_as_user";
export const apiRequest = { scopes: [apiScope] };

// NEW: Graph scopes for profile retrieval
export const graphScopes = ["User.Read"];

// Optional helper to get token (call inside components/hooks)
import { PublicClientApplication } from "@azure/msal-browser";
export const msalInstance = new PublicClientApplication(msalConfig);

// NEW: one-time async initializer to fix "uninitialized_public_client_application"
let msalInitPromise;
export function ensureMsalInitialized() {
  if (!msalInitPromise) {
    msalInitPromise = msalInstance.initialize();
  }
  return msalInitPromise;
}

export async function acquireApiToken() {
  await ensureMsalInitialized();
  try {
    return await msalInstance.acquireTokenSilent(apiRequest);
  } catch {
    try {
      return await msalInstance.acquireTokenPopup(apiRequest);
    } catch {
      return null; // NEW: tolerate failure
    }
  }
}
