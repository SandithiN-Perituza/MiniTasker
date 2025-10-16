import React, { useState, useEffect, useRef } from "react";
import UserContext from "./UserContext";
import { msalInstance, graphScopes, ensureMsalInitialized } from "../authConfig";
import { microsoftAuth } from "../api/api";
import * as microsoftTeams from "@microsoft/teams-js";
import { isInTeams } from "../utils/teams";
import { ensureTeamsInitialized } from "../utils/teamsInit";

export function UserProvider({ children }) {
  const [user, setUser] = useState(null);

  useEffect(() => {
    const storedUser = localStorage.getItem("user");
    setUser(storedUser ? JSON.parse(storedUser) : null);
  }, []);

  // Handle redirect responses after ensured initialization
  useEffect(() => {
    let mounted = true;
    (async () => {
      await ensureMsalInitialized();
      try {
        const resp = await msalInstance.handleRedirectPromise();
        if (mounted && resp?.account) {
          msalInstance.setActiveAccount(resp.account);
        }
      } catch (e) {
        console.warn("MSAL redirect handling error", e);
      }
    })();
    return () => { mounted = false; };
  }, []);

  const login = (userData) => {
    localStorage.setItem("user", JSON.stringify(userData));
    setUser(userData);
  };

  const logout = () => {
    localStorage.removeItem("user");
    setUser(null);
  };

  const loginWithMicrosoft = async () => {
    try {
      // TEAMS FLOW
      if (isInTeams()) {
        await ensureTeamsInitialized(); // centralized init

        // NEW: first attempt silent SSO inside Teams (avoids popup issues)
        try {
          const ssoToken = await microsoftTeams.authentication.getAuthToken({ silent: true });
          const saved = await microsoftAuth(ssoToken);
          localStorage.setItem("user", JSON.stringify(saved));
          setUser(saved);
          return saved;
        } catch {
          // silent SSO failed -> fall back
        }

        // Fallback interactive (custom page)
        const idToken = await new Promise((resolve, reject) => {
          microsoftTeams.authentication.authenticate({
            url: `${window.location.origin}/teams-auth-start.html`,
            width: 600,
            height: 560,
            successCallback: (result) => resolve(result),
            failureCallback: (reason) => reject(new Error(reason)),
          });
        });
        const saved = await microsoftAuth(idToken);
        localStorage.setItem("user", JSON.stringify(saved));
        setUser(saved);
        return saved;
      }

      // BROWSER FLOW
      await ensureMsalInitialized();
      let account = msalInstance.getActiveAccount() || msalInstance.getAllAccounts()[0];
      if (!account) {
        try {
          const popupResp = await msalInstance.loginPopup({ scopes: graphScopes, prompt: "select_account" });
          account = popupResp.account;
          msalInstance.setActiveAccount(account);
          // Prefer id token from login response if present
          const idTok = popupResp.idToken;
          if (idTok) {
            const saved = await microsoftAuth(idTok);
            localStorage.setItem("user", JSON.stringify(saved));
            setUser(saved);
            return saved;
          }
        } catch (popupErr) {
          // Fallback to redirect (flow continues after redirect)
          await msalInstance.loginRedirect({ scopes: graphScopes, prompt: "select_account" });
          return;
        }
      }

      // Acquire token silently (after having an account)
      const tokenResp = await msalInstance.acquireTokenSilent({
        scopes: graphScopes,
        account: msalInstance.getActiveAccount(),
      });

      const tokenForBackend = tokenResp.idToken || tokenResp.accessToken;
      const saved = await microsoftAuth(tokenForBackend);
      localStorage.setItem("user", JSON.stringify(saved));
      setUser(saved);
      return saved;
    } catch (e) {
      console.error("Microsoft login failed:", e);
      throw e;
    }
  };

  const autoLoginAttempted = useRef(false);
  useEffect(() => {
    if (!user && isInTeams() && !autoLoginAttempted.current) {
      autoLoginAttempted.current = true;
      loginWithMicrosoft().catch(e => console.warn("Auto Teams SSO failed:", e));
    }
  }, [user]);

  return (
    <UserContext.Provider value={{ user, login, logout, loginWithMicrosoft }}>
      {children}
    </UserContext.Provider>
  );
}

// import React, { useState } from 'react';
// import UserContext from './UserContext';

// export const UserProvider = ({ children }) => {
//   const [currentUser, setCurrentUser] = useState(null);
//   const [refreshTrigger, setRefreshTrigger] = useState(false);

//   return (
//     <UserContext.Provider value={{ currentUser, setCurrentUser, refreshTrigger, setRefreshTrigger }}>
//       {children}
//     </UserContext.Provider>
//   );
// };