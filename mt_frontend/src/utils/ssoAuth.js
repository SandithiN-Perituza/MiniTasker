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
 * Attempt helper (unchanged).
 */
async function attemptTokenSequence(attempts) {
  const errors = [];
  for (const a of attempts) {
    try {
      const token = await microsoftTeams.authentication.getAuthToken(a.options);
      if (a.label) console.info(`[SSO] Success: ${a.label}`);
      return { token, errors };
    } catch (e) {
      errors.push({ label: a.label, error: e?.message || e });
      console.warn(`[SSO] Failed: ${a.label} -> ${e?.message || e}`);
    }
  }
  return { token: null, errors };
}

/**
 * Acquire a Teams SSO token silently.
 * Avoid specifying 'resources' to prevent resource/origin mismatch errors for basic tab SSO.
 * Adds proactive validation of resource vs current origin for clearer diagnostics.
 */
export async function acquireTeamsSsoToken() {
  await ensureInitialized();

  // NOTE: Resource attempts disabled by default to avoid "App resource defined in manifest and iframe origin do not match".
  const enableResourceAttempts =
    String(import.meta.env.VITE_SSO_ENABLE_RESOURCE_ATTEMPTS || "").toLowerCase() === "true";

  if (enableResourceAttempts) {
    console.info("[SSO] Resource attempts enabled via VITE_SSO_ENABLE_RESOURCE_ATTEMPTS=true");
  } else {
    console.info("[SSO] Skipping resource-based token attempts (default).");
  }

  const baseResource = import.meta.env.VITE_SSO_RESOURCE || "api://3ee5758e-f933-4383-ba25-8c5f0fb848a4";
  const altResource = import.meta.env.VITE_SSO_RESOURCE_ALT;
  const resourceCandidates = enableResourceAttempts
    ? Array.from(new Set([baseResource, altResource].filter(Boolean)))
    : [];

  // Build minimal attempt list first (plain only)
  const attempts = [
    { label: "plain silent", options: { silent: true } },
    { label: "plain non-silent", options: {} },
    // Optionally append resource attempts if explicitly enabled
    ...resourceCandidates.flatMap(r => ( [
      { label: `resource(${r}) silent`, options: { resources: [r], silent: true } },
      { label: `resource(${r})`, options: { resources: [r] } },
    ] )),
  ];

  const { token, errors } = await attemptTokenSequence(attempts);

  if (token) return token;

  // Interactive fallback
  try {
    const popupUrl = `${window.location.origin}/teams-auth-start.html`;
    console.info("[SSO] Trying interactive Teams authenticate fallback...");
    const interactiveToken = await new Promise((resolve, reject) => {
      microsoftTeams.authentication.authenticate({
        url: popupUrl,
        width: 600,
        height: 535,
        successCallback: (result) => resolve(result),
        failureCallback: (reason) => reject(new Error(reason)),
      });
    });
    return interactiveToken;
  } catch (popupErr) {
    const detail =
      "Teams SSO failed after all attempts.\n" +
      (errors.length
        ? "Attempts:\n" + errors.map(e => ` - ${e.label}: ${e.error}`).join("\n") + "\n"
        : "") +
      `Interactive fallback: ${popupErr?.message || popupErr}\n` +
      "Next steps:\n" +
      "1. Confirm manifest webApplicationInfo.resource matches AAD Application ID URI.\n" +
      "2. Re-upload manifest & remove old app version from Teams.\n" +
      "3. Sign out/in of Teams to clear cached tokens.\n" +
      "4. If mismatch persists, temporarily set VITE_SSO_ENABLE_RESOURCE_ATTEMPTS=true to test resource forms.\n";
    throw new Error(detail);
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
