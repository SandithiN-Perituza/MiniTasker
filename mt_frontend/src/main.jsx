// import { StrictMode } from "react";
// import { createRoot } from "react-dom/client";
// import "./index.css";
// import App from "./App";
// import { UserProvider } from "./context/UserProvider";

// import { PublicClientApplication } from "@azure/msal-browser";
// import { MsalProvider } from "@azure/msal-react";
// import { msalInstance } from "./utils/msalConfig"; // your config file

// createRoot(document.getElementById("root")).render(
//   <StrictMode>
//     <UserProvider>
//       <MsalProvider instance={msalInstance}>
//         <App />
//       </MsalProvider>
//     </UserProvider>
//   </StrictMode>,
// );


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
