import * as microsoftTeams from "@microsoft/teams-js";

let initPromise;
/**
 * Ensures the Microsoft Teams JS SDK is initialized only once.
 */
export function ensureTeamsInitialized() {
  if (!initPromise) {
    initPromise = microsoftTeams.app.initialize().catch(e => {
      // Allow retry on failure by clearing cached promise
      initPromise = undefined;
      throw e;
    });
  }
  return initPromise;
}
