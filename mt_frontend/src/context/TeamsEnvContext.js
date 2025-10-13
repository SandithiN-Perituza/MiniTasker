import { createContext, useContext } from "react";

export const TeamsEnvContext = createContext({ inTeams: false });

export function useTeamsEnv() {
  return useContext(TeamsEnvContext);
}
