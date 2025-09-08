import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App.jsx";
import { MsalProvider } from "@azure/msal-react";
import { msalInstance } from "./authConfig";

createRoot(document.getElementById("root")).render(
  <MsalProvider instance={msalInstance}>
    <App />
  </MsalProvider>

  // <StrictMode>
  //   <App />
  // </StrictMode>,
);
