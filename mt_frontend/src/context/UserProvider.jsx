import React, { useState, useEffect } from "react";
import UserContext from "./UserContext";
import { msalInstance, graphScopes, ensureMsalInitialized } from "../authConfig";
import { microsoftAuth, unifiedMicrosoftLogin } from "../api/api";
import { isInTeams } from "../utils/teams";
import { getTeamsSsoToken } from "../utils/teamsAuth";
import { syncTeamsDirectory } from "../api/api";

export function UserProvider({ children }) {
  const [user, setUser] = useState(null);
 
  // Normalize various possible shapes the backend or tokens might return into a minimal user
  const isGuid = (val) => typeof val === 'string' && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(val);
  const normalizeUser = (maybe) => {
    try {
      if (!maybe) return null;
      // If unified response wraps { user } or { data }
      const candidate = (maybe.user && typeof maybe.user === 'object') ? maybe.user : (maybe.data && typeof maybe.data === 'object') ? maybe.data : maybe;

      // If candidate is an object with graphToken and no nested user, try to parse graphToken
      if (candidate && typeof candidate === 'object' && candidate.graphToken && (!candidate.id && !candidate.azureAdId && Object.keys(candidate).length <= 3)) {
        const tokenStr = candidate.graphToken;
        const parts = tokenStr.split('.');
        if (parts.length > 1) {
          try {
            const payload = parts[1];
            const b64 = payload.replace(/-/g, '+').replace(/_/g, '/');
            const json = atob(b64);
            const claims = JSON.parse(json);
            return {
              id: claims.oid || claims.sub || claims.preferred_username || claims.upn || claims.email || 'unknown',
              azureAdId: claims.oid || claims.sub || (isGuid(claims?.id) ? claims.id : null) || null,
              name: claims.name || [claims.given_name, claims.family_name].filter(Boolean).join(' ') || claims.preferred_username || claims.email || null,
              email: claims.preferred_username || claims.upn || claims.mail || claims.email || null,
            };
          } catch (e) {
            console.debug('[UserProvider] graphToken parse failed', e?.message || e);
          }
        }
      }

      // If candidate is a JWT string
      if (typeof candidate === 'string') {
        const parts = candidate.split('.');
        if (parts.length > 1) {
          try {
            const payload = parts[1];
            const b64 = payload.replace(/-/g, '+').replace(/_/g, '/');
            const json = atob(b64);
            const claims = JSON.parse(json);
            return {
              id: claims.oid || claims.sub || claims.preferred_username || claims.upn || claims.email || 'unknown',
              azureAdId: claims.oid || claims.sub || null,
              name: claims.name || [claims.given_name, claims.family_name].filter(Boolean).join(' ') || claims.preferred_username || claims.email || null,
              email: claims.preferred_username || claims.upn || claims.mail || claims.email || null,
            };
          } catch (e) {
            console.debug('[UserProvider] token string parse failed', e?.message || e);
          }
        }
      }

      // Otherwise, extract common fields from object
      if (candidate && typeof candidate === 'object') {
        return {
          id: candidate.id ?? candidate.userId ?? candidate.localId ?? 'unknown',
          azureAdId: candidate.azureAdId ?? candidate.oid ?? candidate.azureAdObjectId ?? (isGuid(candidate.id) ? candidate.id : null) ?? null,
          name: candidate.name ?? candidate.displayName ?? candidate.fullName ?? null,
          email: candidate.email ?? candidate.mail ?? candidate.userPrincipalName ?? candidate.preferred_username ?? candidate.upn ?? null,
        };
      }
    } catch (e) {
      console.debug('[UserProvider] normalizeUser error', e?.message || e);
    }
    return null;
  };
 
  useEffect(() => {
    const storedRaw = localStorage.getItem("user");
    if (!storedRaw) {
      setUser(null);
      return;
    }
    try {
      const parsed = JSON.parse(storedRaw);
      console.debug('[UserProvider] storedUser parsed:', parsed);
      const normalized = normalizeUser(parsed) || normalizeUser(parsed.user) || normalizeUser(parsed.data) || null;
      if (normalized) {
        // Ensure localStorage contains the normalized shape for other parts of the app
        try { localStorage.setItem('user', JSON.stringify(normalized)); } catch (e) { console.debug('[UserProvider] failed writing normalized user to localStorage', e?.message || e); }
        setUser(normalized);
      } else {
        setUser(parsed);
      }
    } catch (e) {
      // stored value wasn't JSON (maybe a token string)
      const normalized = normalizeUser(storedRaw);
      if (normalized) {
        try { localStorage.setItem('user', JSON.stringify(normalized)); } catch (e2) { console.debug('[UserProvider] failed writing normalized user token to localStorage', e2?.message || e2); }
        setUser(normalized);
      } else {
        setUser(null);
      }
    }
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