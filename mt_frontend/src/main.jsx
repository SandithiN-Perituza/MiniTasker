import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App";
import { UserProvider } from "./context/UserProvider";
import { MsalProvider } from "@azure/msal-react";
import { msalInstance, ensureMsalInitialized } from "./authConfig";

// NEW: wait for msal initialize before rendering
(async () => {
  await ensureMsalInitialized();
  createRoot(document.getElementById("root")).render(
    <StrictMode>
      <MsalProvider instance={msalInstance}>
        <UserProvider>
          <App />
        </UserProvider>
      </MsalProvider>
    </StrictMode>,
  );
})();
