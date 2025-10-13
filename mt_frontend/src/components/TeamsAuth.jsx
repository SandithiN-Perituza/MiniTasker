import React, { useEffect, useState, useContext } from "react";
import UserContext from "../context/UserContext";
import { acquireTeamsSsoToken, exchangeTokenWithBackend, isInTeams } from "../utils/ssoAuth";
import { TeamsEnvContext } from "../context/TeamsEnvContext";

const TeamsAuth = ({ children }) => {
  const { user, login } = useContext(UserContext);
  const [loading, setLoading] = useState(true);
  const [inTeams, setInTeams] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    let cancelled = false;

    async function run() {
      const inside = await isInTeams();
      if (cancelled) return;
      setInTeams(inside);

      // If not running in Teams, skip SSO.
      if (!inside) {
        setLoading(false);
        return;
      }

      // If user already set, skip.
      if (user) {
        setLoading(false);
        return;
      }

      try {
        const token = await acquireTeamsSsoToken();
        if (cancelled) return;
        const appUser = await exchangeTokenWithBackend(token);
        if (!cancelled) login(appUser);
      } catch (e) {
        if (!cancelled) setError(e.message || "SSO failed");
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    run();
    return () => { cancelled = true; };
  }, [user, login]);

  // Wrap all rendered UI (including loaders / errors) with provider so consumers can still know inTeams
  const content = (() => {
    if (inTeams && loading) {
      return (
        <div className="flex justify-center items-center min-h-screen">
          <div className="text-center">
            <div className="animate-spin rounded-full h-20 w-20 border-b-2 border-blue-500 mx-auto"></div>
            <p className="mt-4 text-gray-600">Signing you in (SSO)...</p>
          </div>
        </div>
      );
    }

    if (inTeams && error && !user) {
      const isResourceMismatch = /resource/i.test(error) || /origin/i.test(error);
      return (
        <div className="flex flex-col justify-center items-center min-h-screen p-4 text-center">
          <p className="text-red-600 font-medium mb-2">Automatic sign-in failed.</p>
          <p className="text-sm text-gray-700 break-all">{error}</p>
          {isResourceMismatch && (
            <div className="mt-4 text-xs text-left max-w-md text-gray-600 space-y-2">
              <p>Checklist:</p>
              <ul className="list-disc ml-4 space-y-1">
                <li>Manifest webApplicationInfo.resource matches Azure AD App ID URI.</li>
                <li>That App ID URI is set as VITE_SSO_RESOURCE in .env.</li>
                <li>Domain is in manifest validDomains and configured in AAD redirect URIs if needed.</li>
                <li>Re-upload updated manifest to Teams after changes.</li>
              </ul>
            </div>
          )}
          <p className="text-sm text-gray-500 mt-6">
            Reload this tab after fixing configuration.
          </p>
        </div>
      );
    }

    return children;
  })();

  return (
    <TeamsEnvContext.Provider value={{ inTeams }}>
      {content}
    </TeamsEnvContext.Provider>
  );
};

export default TeamsAuth;
