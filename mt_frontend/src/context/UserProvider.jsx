import React, { useState, useEffect } from "react";
import UserContext from "./UserContext";
import { msalInstance, graphScopes, ensureMsalInitialized } from "../authConfig";
import { microsoftAuth } from "../api/api";

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
      // Always use browser MSAL flow, even inside Teams
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

  return (
    <UserContext.Provider value={{ user, login, logout, loginWithMicrosoft }}>
      {children}
    </UserContext.Provider>
  );
}

// import React, { useState, useEffect } from "react";
// import  UserContext from "./UserContext";

// export function UserProvider({ children }) {
//   const [user, setUser] = useState(null);

//   useEffect(() => {
//     const storedUser = localStorage.getItem("user");
//     setUser(storedUser ? JSON.parse(storedUser) : null);
//   }, []);

//   const login = (userData) => {
//     localStorage.setItem("user", JSON.stringify(userData));
//     setUser(userData);
//   };

//   const logout = () => {
//     // Clear all authentication data
//     localStorage.removeItem("user");
//     localStorage.removeItem("accessToken");
//     localStorage.removeItem("idToken");
//     localStorage.removeItem("authToken");
//     localStorage.removeItem("jwt");
//     setUser(null);
//     console.log("Logged out and cleared all tokens");
//   };

//   return (
//     <UserContext.Provider value={{ user, login, logout }}>
//       {children}
//     </UserContext.Provider>
//   );
// }

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