import React, { useEffect, useState } from "react";
import { useTeams } from "../context/TeamsProvider";
import { useUser } from "../context/UserProvider";
// NEW: import Teams SDK
import * as microsoftTeams from "@microsoft/teams-js";

const TeamsAuth = ({ children }) => {
  const { inTeams, teamsUser, teamsContext } = useTeams();
  const { setUser } = useUser();
  // NEW: local state for token + errors
  const [authToken, setAuthToken] = useState(null);
  const [authError, setAuthError] = useState(null);
  const [loadingToken, setLoadingToken] = useState(false);

  useEffect(() => {
    if (inTeams && teamsUser) {
      // Auto-login user from Teams context
      setUser({
        id: teamsContext?.user?.id,
        name: teamsContext?.user?.displayName,
        email: teamsContext?.user?.userPrincipalName,
        source: "teams",
      });
      // NEW: attempt silent token acquisition once
      if (!authToken && !authError) {
        setLoadingToken(true);
        microsoftTeams.initialize();
        microsoftTeams.authentication.getAuthToken({
          successCallback: (token) => {
            setAuthToken(token);
            setLoadingToken(false);
          },
          failureCallback: (err) => {
            setAuthError(err);
            setLoadingToken(false);
          },
        });
      }
    }
  }, [inTeams, teamsUser, teamsContext, setUser, authToken, authError]);

  // NEW: interactive login fallback
  const handleInteractiveLogin = () => {
    setAuthError(null);
    setLoadingToken(true);
    microsoftTeams.authentication.authenticate({
      url: `${window.location.origin}/teams-auth-start.html`, // placeholder: your auth popup page
      width: 600,
      height: 600,
      successCallback: (resultToken) => {
        setAuthToken(resultToken);
        setLoadingToken(false);
      },
      failureCallback: (err) => {
        setAuthError(err);
        setLoadingToken(false);
      },
    });
  };

  if (inTeams && !teamsUser) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-blue-500 mx-auto"></div>
          <p className="mt-4 text-gray-600">Authenticating with Teams...</p>
        </div>
      </div>
    );
  }

  // NEW: show token loading state
  if (inTeams && teamsUser && loadingToken && !authToken && !authError) {
    return (
      <div className="flex justify-center items-center min-h-screen">
        <div className="text-center">
          <div className="animate-spin rounded-full h-24 w-24 border-b-2 border-indigo-500 mx-auto"></div>
          <p className="mt-4 text-gray-600">Acquiring Microsoft 365 token...</p>
        </div>
      </div>
    );
  }

  // NEW: show error + retry option
  if (inTeams && authError && !authToken) {
    return (
      <div className="flex flex-col items-center justify-center min-h-screen space-y-4">
        <div className="max-w-md text-center">
          <p className="text-red-600 font-medium">
            Could not acquire authentication token.
          </p>
          <p className="text-sm text-gray-600">
            {typeof authError === "string"
              ? authError
              : "Please sign in with your Microsoft account."}
          </p>
        </div>
        <button
          onClick={handleInteractiveLogin}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
        >
          Sign in / Retry
        </button>
      </div>
    );
  }

  // NEW: optionally inject token into children (if child expects function-as-child)
  if (typeof children === "function") {
    return children({ authToken });
  }

  return children;
};

export default TeamsAuth;
