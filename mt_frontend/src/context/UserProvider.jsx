import React, { useState, useEffect, useRef } from "react";
import UserContext from "./UserContext";
import { msalInstance, graphScopes, ensureMsalInitialized } from "../authConfig";
import { microsoftAuth, exchangeObo } from "../api/api";
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
  // TEAMS FLOW (uses teams-auth-start.html which now shares the same SPA clientId)
      if (isInTeams()) {
        await ensureTeamsInitialized(); // centralized init

        // Try silent SSO inside Teams (avoids popup issues)
        try {
          const ssoToken = await microsoftTeams.authentication.getAuthToken({ silent: true });
          const saved = await microsoftAuth(ssoToken);
          localStorage.setItem("user", JSON.stringify(saved));
          // NEW: OBO exchange for API token
            try {
              const obo = await exchangeObo(ssoToken);
              localStorage.setItem("accessToken", obo.accessToken);
            } catch (oboErr) {
              console.warn("Silent Teams OBO failed:", oboErr);
            }
          setUser(saved);
          return saved;
        } catch {
          // silent SSO failed -> fall back
        }

  // Fallback: interactive popup via Teams authentication.authenticate (will load teams-auth-start.html)
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
        // NEW: OBO exchange after interactive
        try {
          const obo = await exchangeObo(idToken);
          localStorage.setItem("accessToken", obo.accessToken);
        } catch (oboErr) {
          console.warn("Interactive Teams OBO failed:", oboErr);
        }
        setUser(saved);
        return saved;
      }

      // BROWSER FLOW
      await ensureMsalInitialized();
      let account = msalInstance.getActiveAccount() || msalInstance.getAllAccounts()[0];
      if (!account) {
        try {
          // Use loginPopup for first login (not silent)
          const popupResp = await msalInstance.loginPopup({ scopes: graphScopes, prompt: "select_account" });
          account = popupResp.account;
          msalInstance.setActiveAccount(account);
          // Prefer id token from login response if present
          const idTok = popupResp.idToken;
          if (idTok) {
            const saved = await microsoftAuth(idTok);
            localStorage.setItem("user", JSON.stringify(saved));
            // continue to get API token below
          }
        } catch (popupErr) {
          console.warn("MSAL popup login failed:", popupErr);
          // Fallback to redirect (opens in full browser window)
          await msalInstance.loginRedirect({ scopes: graphScopes, prompt: "select_account" });
          return;
        }
      }

      // Acquire API token silently (no OBO needed in plain browser)
      const tokenResp = await msalInstance.acquireTokenSilent({
        scopes: ["api://59aef810-e681-4b84-bc17-2561fe854c0e/access_as_user"],
        account: msalInstance.getActiveAccount(),
      });
      localStorage.setItem("accessToken", tokenResp.accessToken); // NEW store API token

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