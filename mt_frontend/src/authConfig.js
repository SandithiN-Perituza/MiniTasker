// MSAL / Auth configuration & helpers
// -----------------------------------
// This file centralizes authentication logic to avoid scattered consent / token errors (e.g. AADSTS65001 consent_required).
// Key practices implemented:
//  * Always acquire tokens for a specific account (no implicit global)
//  * Explicit scopes instead of /.default for SPA delegated permissions
//  * Defensive error handling + fallback to interactive when silent fails
//  * Single initialization gate to prevent race conditions
//  * Environment-aware redirect URI (local dev vs deployed)

import { PublicClientApplication, InteractionRequiredAuthError } from "@azure/msal-browser";

// Prefer injecting redirect via env so local dev works without changing code.
// Define VITE_AUTH_REDIRECT_URI in local .env if needed; fallback to current origin for flexibility.
const redirectUri = window.location.origin || "https://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net";

export const msalConfig = {
  auth: {
    clientId: "f6c2a5e9-3bd5-4223-ad2c-618846a668c5", // FRONTEND SPA App Registration (DO NOT use first‑party Teams client IDs)
    authority: "https://login.microsoftonline.com/7b967b11-c0b9-402b-b483-d694f50dfb82", // Tenant-specific to avoid accidental multi-tenant consent prompts
    redirectUri, // dynamic
    // postLogoutRedirectUri: redirectUri, // Uncomment if you need explicit logout redirect control
  },
  cache: {
    cacheLocation: "localStorage", // Persist across tabs (consider 'sessionStorage' if stricter security desired)
    storeAuthStateInCookie: false, // Set true only for IE11 / legacy edge issues
  },
  system: {
    // Iframe logins are typically blocked; only enable if hosting inside an allowed frame (e.g. Teams tab) AND you tested it.
    allowRedirectInIframe: false,
  },
};

// Delegated scopes for calling the protected custom API. Ensure the API app exposes 'access_as_user'.
const apiScopes = ["openid", "profile", "offline_access", "api://59aef810-e681-4b84-bc17-2561fe854c0e/access_as_user"];
// Microsoft Graph basic profile scopes (extend if you need mail, etc.)
const graphScopes = ["User.Read"];

export const msalInstance = new PublicClientApplication(msalConfig);

let msalInitPromise;
export function ensureMsalInitialized() {
  if (!msalInitPromise) {
    msalInitPromise = msalInstance.initialize();
  }
  return msalInitPromise;
}

// Internal: choose active account (persist choice if multiple)
function getActiveAccount() {
  let account = msalInstance.getActiveAccount();
  if (account) return account;
  const accounts = msalInstance.getAllAccounts();
  if (accounts.length === 1) {
    msalInstance.setActiveAccount(accounts[0]);
    return accounts[0];
  }
  // If multiple accounts, caller should trigger an account selection UI.
  return null;
}

export async function loginInteractive(preferredMode = "popup") {
  await ensureMsalInitialized();
  const request = { scopes: apiScopes };
  const mode = preferredMode === "redirect" ? "redirect" : "popup";
  if (mode === "popup") {
    const resp = await msalInstance.loginPopup(request);
    msalInstance.setActiveAccount(resp.account);
    return resp.account;
  } else {
    msalInstance.loginRedirect(request);
    return undefined; // Flow will continue after redirect
  }
}

export async function logout(preferredMode = "popup") {
  await ensureMsalInitialized();
  const mode = preferredMode === "redirect" ? "redirect" : "popup";
  const account = getActiveAccount();
  if (mode === "popup") {
    await msalInstance.logoutPopup({ account });
  } else {
    await msalInstance.logoutRedirect({ account });
  }
}

async function acquireToken(scopesArray) {
  await ensureMsalInitialized();
  let account = getActiveAccount();
  if (!account) {
    // No account - force interactive login
    await loginInteractive();
    account = getActiveAccount();
  }
  const request = { scopes: scopesArray, account };
  try {
    return await msalInstance.acquireTokenSilent(request);
  } catch (err) {
    if (err instanceof InteractionRequiredAuthError) {
      // Fallback interactive consent
      if (document.visibilityState === "visible") {
        return await msalInstance.acquireTokenPopup(request);
      }
    }
    throw err;
  }
}

// Public helpers
export async function acquireApiToken() {
  return acquireToken(apiScopes);
}

export async function acquireGraphToken() {
  return acquireToken(graphScopes);
}

// Utility to get decoded ID token claims (for displaying profile info without Graph call)
export function getIdTokenClaims() {
  const account = getActiveAccount();
  return account?.idTokenClaims || null;
}

// Diagnose common consent / configuration issues quickly in UI components if needed
export function explainAuthError(error) {
  if (!error) return null;
  const msg = typeof error === "string" ? error : error.errorMessage || error.message;
  if (msg?.includes("AADSTS65001")) {
    return "Consent required: Ensure the SPA app has the API scope added under API Permissions and admin/user consent granted.";
  }
  if (msg?.includes("AADSTS70011")) {
    return "Invalid scope: Verify the scope name matches the Expose an API definition.";
  }
  return null;
}

export { apiScopes, graphScopes };
