import React, { useEffect, useState } from "react";
import { TeamsContext } from "./TeamsContext";
import { initializeTeams, isInTeams, getTeamsContext, silentTeamsLogin, isRunningInTeams } from "../utils/teamsConfig";

export const TeamsProvider = ({ children }) => {
  const [teamsInitialized, setTeamsInitialized] = useState(false);
  const [inTeams, setInTeams] = useState(false);
  const [teamsContext, setTeamsContext] = useState(null);
  const [teamsUser, setTeamsUser] = useState(null);
  const [isTeamsEnvironment, setIsTeamsEnvironment] = useState(false);

  useEffect(() => {
    const initializeApp = async () => {
      // First check if we're in Teams environment
      const teamsEnv = isRunningInTeams();
      setIsTeamsEnvironment(teamsEnv);

      if (!teamsEnv) {
        console.log("Running outside Teams - Teams features disabled");
        return;
      }

      const initialized = await initializeTeams();
      setTeamsInitialized(initialized);

      if (initialized) {
        const isTeamsEnvironmentActive = await isInTeams();
        setInTeams(isTeamsEnvironmentActive);

        if (isTeamsEnvironmentActive) {
          const context = await getTeamsContext();
          setTeamsContext(context);

          // Attempt silent login
          const user = await silentTeamsLogin();
          if (user) {
            setTeamsUser(user);
          }
        }
      }
    };

    initializeApp();
  }, []);

  const value = {
    teamsInitialized,
    inTeams,
    teamsContext,
    teamsUser,
    isTeamsEnvironment,
    silentLogin: silentTeamsLogin,
  };

  return (
    <TeamsContext.Provider value={value}>
      {children}
    </TeamsContext.Provider>
  );
};
