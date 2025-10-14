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
      const hasAlt = !!import.meta.env.VITE_SSO_RESOURCE_ALT;
      return (
        <div className="flex flex-col justify-center items-center min-h-screen p-4 text-center">
          <p className="text-red-600 font-medium mb-3">Automatic sign-in failed.</p>
          <pre className="text-xs text-left bg-red-50 border border-red-200 rounded p-3 max-w-2xl overflow-auto whitespace-pre-wrap">
            {error}
          </pre>
          <div className="mt-4 text-xs text-left max-w-2xl text-gray-700 space-y-1">
            <p>Tips:</p>
            <ul className="list-disc ml-4 space-y-1">
              <li>Confirm Application ID URI(s) you intend to use.</li>
              <li>If one form fails, try the other (simplified vs domain) — both now attempted automatically.</li>
              {!hasAlt && (
                <li>
                  You can set VITE_SSO_RESOURCE_ALT to the domain-qualified URI:
                  api://&lt;your-host&gt;/&lt;appId&gt;
                </li>
              )}
              <li>After changing manifest or App ID URI, uninstall and re-add the app in Teams.</li>
            </ul>
          </div>
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
