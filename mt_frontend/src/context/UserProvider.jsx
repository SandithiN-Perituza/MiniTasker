import React, { useState, useEffect } from "react";
import UserContext from "./UserContext";
import { msalInstance, graphScopes, ensureMsalInitialized } from "../authConfig";
import { microsoftAuth, unifiedMicrosoftLogin } from "../api/api";
import { isInTeams } from "../utils/teams";
import { getTeamsSsoToken } from "../utils/teamsAuth";
import { syncTeamsDirectory } from "../api/api";

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

  // Silent auto-login in Teams (Vacation Tracker style)
  useEffect(() => {
    let cancelled = false;
    (async () => {
      if (!user && isInTeams() && window.microsoftTeams) {
        try {
          const token = await getTeamsSsoToken(); // silent/prompt sequence
          if (cancelled) return;
          const savedUser = await unifiedMicrosoftLogin(token);
          if (cancelled) return;
          localStorage.setItem("user", JSON.stringify(savedUser));
          setUser(savedUser);
          // Optional directory sync (non-blocking)
          syncTeamsDirectory(token).catch(e => console.debug("Directory sync skipped:", e.message));
        } catch (e) {
          console.debug("Teams auto-SSO unavailable:", e.message);
        }
      }
    })();
    return () => { cancelled = true; };
  }, [user]);
 
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
      if (isInTeams()) {
        // Vacation Tracker style: only Teams SSO token path, no MSAL interactive in iframe
        const token = await getTeamsSsoToken();
        const saved = await unifiedMicrosoftLogin(token);
        localStorage.setItem("user", JSON.stringify(saved));
        setUser(saved);
        syncTeamsDirectory(token).catch(()=>{});
        return saved;
      }
      // Non-Teams (normal browser) flow stays mostly same; prefer popup then fallback to redirect.
      await ensureMsalInitialized();
      let account = msalInstance.getActiveAccount() || msalInstance.getAllAccounts()[0];

      if (!account) {
        try {
          const popupResp = await msalInstance.loginPopup({ scopes: graphScopes, prompt: "select_account" });
          account = popupResp.account;
          msalInstance.setActiveAccount(account);
          const idTok = popupResp.idToken;
          if (idTok) {
            const saved = await microsoftAuth(idTok);
            localStorage.setItem("user", JSON.stringify(saved));
            setUser(saved);
            return saved;
          }
        } catch (e) {
          // If popup blocked, use redirect (safe outside iframe)
          if (e?.errorCode === "popup_window_error") {
            console.warn("Popup blocked; falling back to redirect");
          } else if (String(e?.errorMessage || "").includes("redirect_in_iframe")) {
            console.warn("Redirect attempted in iframe (unexpected outside Teams). Adjust hosting context.", e);
          } else {
            console.warn("Popup login failed, fallback to redirect", e);
          }
          await msalInstance.loginRedirect({ scopes: graphScopes, prompt: "select_account" });
          return;
        }
      }

      // Silent acquire after popup/redirect
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

  return (
    <UserContext.Provider value={{ user, login, logout, loginWithMicrosoft }}>
      {children}
    </UserContext.Provider>
  );
}