// import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App.jsx";
import { MsalProvider } from "@azure/msal-react";
import { msalInstance } from "./authConfig";
import { UserProvider } from "./context/UserProvider";

createRoot(document.getElementById("root")).render(
  // <StrictMode>
    <MsalProvider instance={msalInstance}>
      <UserProvider>
        <App />
      </UserProvider>
    </MsalProvider>
  // </StrictMode>

  // <MsalProvider instance={msalInstance}>
  //   <App />
  // </MsalProvider>

  // <StrictMode>
  //   <App />
  // </StrictMode>,
);
