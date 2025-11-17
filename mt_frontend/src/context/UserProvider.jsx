import React, { useState, useEffect } from "react";
import UserContext from "./UserContext";
import { msalInstance, graphScopes, ensureMsalInitialized } from "../authConfig";
import { microsoftAuth } from "../api/api";
import * as microsoftTeams from "@microsoft/teams-js";

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
      // Try Teams SSO when inside Teams
      try {
        try { await microsoftTeams.app.initialize(); } catch(e){}

        try {
          const ssoToken = await microsoftTeams.authentication.getAuthToken({
            silent: true,
            resources: ["api://59aef810-e681-4b84-bc17-2561fe854c0e"]
          });
          try { localStorage.setItem('accessToken', ssoToken); } catch(e){}
          const saved = await microsoftAuth(ssoToken);
          localStorage.setItem("user", JSON.stringify(saved));
          setUser(saved);
          return saved;
        } catch (teamsSilentErr) {
          console.log("Teams silent auth failed, using interactive authenticate", teamsSilentErr);
          try {
            const token = await new Promise((resolve, reject) => {
              let resolved = false;
              const timeoutMs = 12000;
              let timeout = setTimeout(() => {
                if (resolved) return;
                const clientId = msalInstance.getConfiguration().auth.clientId;
                const redirectUri = msalInstance.getConfiguration().auth.redirectUri || window.location.origin + '/teams-auth-start.html';
                const scope = 'api://59aef810-e681-4b84-bc17-2561fe854c0e/access_as_user';
                const authorizeUrl = `https://login.microsoftonline.com/common/oauth2/v2.0/authorize?client_id=${encodeURIComponent(clientId)}&response_type=token&scope=${encodeURIComponent(scope)}&redirect_uri=${encodeURIComponent(redirectUri)}&prompt=select_account`;
                try {
                  if (microsoftTeams.executeDeepLink) microsoftTeams.executeDeepLink(authorizeUrl);
                  else window.open(authorizeUrl, '_blank');
                } catch (e) { try { window.open(authorizeUrl, '_blank'); } catch {} }
                resolved = true;
                reject(new Error('Teams authenticate timed out; external browser opened'));
              }, timeoutMs);

              microsoftTeams.authentication.authenticate({
                url: `${window.location.origin}/teams-auth-start.html`,
                width: 600,
                height: 535,
                successCallback: (t) => { if (resolved) return; resolved = true; clearTimeout(timeout); resolve(t); },
                failureCallback: (e) => { if (resolved) return; resolved = true; clearTimeout(timeout); reject(e); }
              });
            });
            try { localStorage.setItem('accessToken', token); } catch(e){}
            const saved = await microsoftAuth(token);
            localStorage.setItem("user", JSON.stringify(saved));
            setUser(saved);
            return saved;
          } catch (interactiveErr) {
            console.log("Teams interactive auth failed or timed out, falling back to MSAL", interactiveErr);
          }
        }
      } catch (teamsErr) {
        // not in Teams or Teams SDK not available — fall through to MSAL
      }

      // Fallback to MSAL web flow
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
        } catch (popupErr) {
          await msalInstance.loginRedirect({ scopes: graphScopes, prompt: "select_account" });
          return;
        }
      }

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