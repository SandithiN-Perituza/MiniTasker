import * as microsoftTeams from "@microsoft/teams-js";

let initialized = false;

async function ensureInitialized() {
  if (initialized) return;
  try {
    await microsoftTeams.app.initialize();
    initialized = true;
  } catch (e) {
    console.warn("Teams init failed (likely not in Teams):", e);
    throw e;
  }
}

/**
 * Checks if app is running inside Microsoft Teams.
 */
export async function isInTeams() {
  try {
    await ensureInitialized();
    return true;
  } catch {
    return false;
  }
}

/**
 * Acquire a Teams SSO token silently.
 * Uses resource (App ID URI) matching manifest webApplicationInfo.resource.
 */
export async function acquireTeamsSsoToken() {
  await ensureInitialized();

  const resource =
    import.meta.env.VITE_SSO_RESOURCE ||
    "api://app-frontendtodoapp-test-cubtfyddfzfradfx.eastus-01.azurewebsites.net/3ee5758e-f933-4383-ba25-8c5f0fb848a4";

  if (!import.meta.env.VITE_SSO_RESOURCE) {
    console.warn(
      "[SSO] VITE_SSO_RESOURCE not set; falling back to hardcoded value. Set it in .env for consistency."
    );
  }

  try {
    // Teams JS 2.x getAuthToken supports options with resources + silent flag
    const token = await microsoftTeams.authentication.getAuthToken({
      resources: [resource],
      silent: true,
    });
    return token;
  } catch (err) {
    console.error("[SSO] getAuthToken failed:", err);
    throw new Error(
      typeof err === "string" ? err : err?.message || "getAuthToken failed"
    );
  }
}

/**
 * Exchange the Teams SSO (AAD) token with backend to get / create app user.
 */
export async function exchangeTokenWithBackend(aadToken) {
  const res = await fetch(
    "https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api/auth/microsoft",
    {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${aadToken}`,
      },
    }
  );
  if (!res.ok) {
    const text = await res.text().catch(() => "");
    console.error("[SSO] Backend exchange failed:", res.status, text);
    throw new Error("Backend SSO exchange failed");
  }
  return res.json();
}
