import * as microsoftTeams from "@microsoft/teams-js";
import { ensureTeamsInitialized } from "./teamsInit";
import { isTeamsDesktop } from "./teams";

/**
 * Unified Teams login for desktop & web (inside Teams client).
 * Strategy:
 * 1. Initialize SDK once.
 * 2. Attempt silent SSO with getAuthToken.
 * 3. If that fails, invoke interactive authenticate() using dedicated auth page.
 * Always returns a bearer token (access or SSO) suitable for backend SSO endpoint.
 */
export async function teamsLogin({ timeoutMs = 12000, forceExternal } = {}) {
  // Initialize SDK (retry-friendly)
  try {
    await ensureTeamsInitialized();
  } catch (e) {
    console.warn("Teams ensure initialize failed; attempting direct initialize", e);
    try {
      if (microsoftTeams?.app?.initialize) {
        await microsoftTeams.app.initialize();
      } else if (microsoftTeams?.initialize) {
        microsoftTeams.initialize();
      }
    } catch (e2) {
      console.warn("Teams direct initialize also failed; continuing", e2);
    }
  }

  // Helper: race a promise with a timeout to avoid endless black screen
  function withTimeout(promise, label) {
    return new Promise((resolve, reject) => {
      const t = setTimeout(() => reject(new Error(`${label} timed out after ${timeoutMs}ms`)), timeoutMs);
      promise.then(v => { clearTimeout(t); resolve(v); }).catch(err => { clearTimeout(t); reject(err); });
    });
  }

  // 1. Silent SSO attempt (allow prompt only if we will not do interactive right away)
  if (!forceExternal && microsoftTeams?.authentication?.getAuthToken) {
    try {
      console.debug("[teamsLogin] Attempting silent getAuthToken");
      // Newer Teams SDK supports options.allowSignInPrompt; keep it false for silent phase
      const ssoToken = await withTimeout(new Promise((resolve, reject) => {
        try {
          microsoftTeams.authentication.getAuthToken({
            allowSignInPrompt: false,
            successCallback: t => resolve(t),
            failureCallback: err => reject(err),
          });
        } catch (cbErr) { reject(cbErr); }
      }), "getAuthToken(silent)");
      if (ssoToken) {
        console.debug("[teamsLogin] Silent SSO succeeded");
        return { token: ssoToken, source: "teams-sso" };
      }
    } catch (silentErr) {
      console.debug("Silent Teams SSO getAuthToken failed", silentErr);
    }
  }

  // 2. Interactive authenticate popup
  const popupUrl = `${window.location.origin}/teams-auth-start.html`; // Auth helper page
  const preferExternal = forceExternal || isTeamsDesktop(); // Force external in desktop to avoid blank webview issues
  if (microsoftTeams?.authentication?.authenticate) {
    try {
      console.debug("[teamsLogin] Starting interactive authenticate", { preferExternal });
      const authResultPromise = microsoftTeams.authentication.authenticate({
        url: popupUrl,
        width: 600,
        height: 560,
        successCallback: undefined, // We handle promise form or fallback below
        failureCallback: undefined,
        ...(preferExternal ? { isExternal: true } : {})
      });

      const result = (authResultPromise && typeof authResultPromise.then === "function")
        ? await withTimeout(authResultPromise, "authenticate")
        : await withTimeout(new Promise((resolve, reject) => {
            try {
              microsoftTeams.authentication.authenticate({
                url: popupUrl,
                width: 600,
                height: 560,
                successCallback: r => resolve(r),
                failureCallback: e => reject(new Error(e || "Teams authenticate failed")),
              });
            } catch (popupErr) { reject(popupErr); }
          }), "authenticate(cb)");

      return { token: result, source: preferExternal ? "teams-authenticate-external" : "teams-authenticate" };
    } catch (interactiveErr) {
      console.error("Teams interactive authenticate failed", interactiveErr);
      // Retry forcing external browser if supported (electron desktop often needs isExternal)
      try {
        console.warn("Retrying Teams authenticate with external flag...");
        const externalPromise = microsoftTeams.authentication.authenticate({
          url: popupUrl,
          width: 800,
          height: 640,
          isExternal: true,
          successCallback: undefined,
          failureCallback: undefined,
        });
        const externalResult = (externalPromise && typeof externalPromise.then === "function")
          ? await externalPromise
          : await new Promise((resolve, reject) => {
              try {
                microsoftTeams.authentication.authenticate({
                  url: popupUrl,
                  width: 800,
                  height: 640,
                  isExternal: true,
                  successCallback: r => resolve(r),
                  failureCallback: e => reject(new Error(e || "External authenticate failed")),
                });
              } catch (extErr) { reject(extErr); }
            });
        return { token: externalResult, source: "teams-authenticate-external" };
      } catch (externalErr) {
        console.error("External Teams authenticate also failed", externalErr);
      }
      throw interactiveErr;
    }
  }

  throw new Error("Teams authentication APIs not available in this host (no getAuthToken/authenticate)");
}

/**
 * Get Teams SSO token with lightweight silent then prompted attempts.
 * Matches Vacation Tracker approach.
 */
export async function getTeamsSsoToken() {
  await ensureTeamsInitialized();
  if (!microsoftTeams?.authentication?.getAuthToken) {
    throw new Error("Teams getAuthToken API unavailable");
  }

  // Silent attempt
  try {
    return await new Promise((resolve, reject) => {
      microsoftTeams.authentication.getAuthToken({
        allowSignInPrompt: false,
        successCallback: t => resolve(t),
        failureCallback: e => reject(new Error(e || "Silent SSO failed"))
      });
    });
  } catch (silentErr) {
    console.debug("Silent Teams SSO failed:", silentErr.message);
  }

  // Prompted attempt
  try {
    return await new Promise((resolve, reject) => {
      microsoftTeams.authentication.getAuthToken({
        allowSignInPrompt: true,
        successCallback: t => resolve(t),
        failureCallback: e => reject(new Error(e || "Prompted SSO failed"))
      });
    });
  } catch (promptErr) {
    console.debug("Prompted Teams SSO failed:", promptErr.message);
  }

  // Final minimal interactive authenticate external
  if (microsoftTeams?.authentication?.authenticate) {
    try {
      return await new Promise((resolve, reject) => {
        microsoftTeams.authentication.authenticate({
          url: `${window.location.origin}/teams-auth-start.html#external`,
            isExternal: true,
          width: 800,
          height: 640,
          successCallback: r => resolve(r),
          failureCallback: e => reject(new Error(e || "External authenticate failed"))
        });
      });
    } catch (extErr) {
      console.error("External authenticate failed:", extErr);
    }
  }
  throw new Error("Teams SSO token acquisition failed (all methods).");
}

export default teamsLogin;
