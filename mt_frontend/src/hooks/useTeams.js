import { useContext } from "react";
import { TeamsContext } from "../context/TeamsContext";

export const useTeams = () => {
  const context = useContext(TeamsContext);
  if (!context) {
    throw new Error("useTeams must be used within a TeamsProvider");
  }
  return context;
};
