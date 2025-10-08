import React, { useEffect } from "react";
import { useTeams } from "../context/TeamsProvider";
import { useUser } from "../context/UserProvider";

const TeamsAuth = ({ children }) => {
  const { inTeams, teamsUser, teamsContext } = useTeams();
  const { setUser } = useUser();

  useEffect(() => {
    if (inTeams && teamsUser) {
      // Auto-login user from Teams context
      setUser({
        id: teamsContext?.user?.id,
        name: teamsContext?.user?.displayName,
        email: teamsContext?.user?.userPrincipalName,
        source: 'teams'
      });
    }
  }, [inTeams, teamsUser, teamsContext, setUser]);

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

  return children;
};

export default TeamsAuth;
