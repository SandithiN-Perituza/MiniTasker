// import React, { useContext } from "react";
// import { Link, useNavigate } from "react-router-dom";
// import { msalInstance } from "../utils/msalConfig";
// import UserContext from "../context/UserContext";
// import { IoMenu } from "react-icons/io5";

// export default function Sidebar({ open, onClose }) {
//   const navigate = useNavigate();
//   const { user, login, logout } = useContext(UserContext);

//   function handleLogout() {
//     logout(); // Use context logout instead of utils/auth logout
//     onClose();
//     navigate("/login");
//   }

//   async function handleMicrosoftLogin() {
//     try {
//       // Use redirect flow instead of popup for web app registration
//       const loginRequest = {
//         scopes: ["user.read"],
//         prompt: "select_account",
//       };

//       // Check if we're already in the middle of a redirect flow
//       const response = await msalInstance.handleRedirectPromise();

//       if (response) {
//         // We just returned from a redirect, process the response
//         console.log("Redirect response received:", response);
//         await processLoginResponse(response);
//       } else {
//         // Start the redirect flow
//         console.log("Starting Microsoft login redirect...");
//         await msalInstance.loginRedirect(loginRequest);
//         // Note: execution will not continue here as the page redirects
//         return;
//       }
//     } catch (err) {
//       console.error("Microsoft login failed", err);
//       // Show user-friendly error message
//       alert(
//         "Microsoft login failed. Please try again or contact support if the issue persists."
//       );
//     }
//   }

//   async function processLoginResponse(loginResponse) {
//     try {
//       const idToken = loginResponse.idToken;
//       const accessToken = loginResponse.accessToken;

//       console.log("Microsoft login tokens:", {
//         idToken: !!idToken,
//         accessToken: !!accessToken,
//       });

//       // Set the active account for future token requests
//       if (loginResponse.account) {
//         msalInstance.setActiveAccount(loginResponse.account);
//         console.log("Set active account:", loginResponse.account.username);
//       }

//       // Send token to backend for validation
//       const res = await fetch(
//         "https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api/auth/microsoft",
//         {
//           method: "POST",
//           headers: {
//             "Content-Type": "application/json",
//             Authorization: `Bearer ${idToken}`,
//           },
//         }
//       );

//       const userData = await res.json();
//       console.log("Backend response:", userData);

//       // Store the tokens for API calls
//       if (accessToken) {
//         localStorage.setItem("accessToken", accessToken);
//         console.log("Stored Microsoft access token");
//       }

//       if (idToken) {
//         localStorage.setItem("idToken", idToken);
//         console.log("Stored Microsoft ID token");
//       }

//       // If backend provides its own token, use that instead
//       if (userData.token || userData.accessToken) {
//         localStorage.setItem(
//           "accessToken",
//           userData.token || userData.accessToken
//         );
//         console.log("Stored backend access token");
//       }

//       login(userData); // Use context login
//       onClose();
//       navigate("/");
//     } catch (error) {
//       console.error("Error processing login response:", error);
//       throw error;
//     }
//   }

//   return (
//     <div
//       className={`fixed inset-0 z-20 transition-all ${
//         open ? "visible" : "invisible"
//       }`}
//       aria-hidden={!open}
//     >
//       <div
//         className={`fixed inset-0 bg-black bg-opacity-30 transition-opacity ${
//           open ? "opacity-100" : "opacity-0"
//         }`}
//         onClick={onClose}
//       />
//       <aside
//         className={`fixed top-0 left-0 h-full w-64 bg-white shadow-lg transform transition-transform ${
//           open ? "translate-x-0" : "-translate-x-full"
//         }`}
//       >
//         <button
//           className="p-4 focus:outline-none"
//           onClick={onClose}
//           aria-label="Close menu"
//         >
//           <IoMenu size={24} />
//         </button>
//         <nav className="flex flex-col gap-2 p-4">
//           {!user && (
//             <>
//               <Link
//                 to="/login"
//                 className="py-2 px-4 rounded hover:bg-blue-100"
//                 onClick={onClose}
//               >
//                 Login
//               </Link>
//               <Link
//                 to="/signup"
//                 className="py-2 px-4 rounded hover:bg-blue-100"
//                 onClick={onClose}
//               >
//                 Signup
//               </Link>
//               <button
//                 className="py-2 px-4 rounded hover:bg-blue-100 text-left"
//                 onClick={handleMicrosoftLogin}
//               >
//                 Sign in with Microsoft
//               </button>
//             </>
//           )}
//           {user && (
//             <button
//               className="py-2 px-4 rounded hover:bg-blue-100 text-left"
//               onClick={handleLogout}
//             >
//               Logout
//             </button>
//           )}
//         </nav>
//       </aside>
//     </div>
//   );
// }

import React, { useContext } from "react";
import { Link, useNavigate } from "react-router-dom";
// import { msalInstance } from "../utils/msalConfig";
import UserContext from "../context/UserContext";
import { IoMenu } from "react-icons/io5";
// import * as microsoftTeams from "@microsoft/teams-js";
import { isInTeams } from "../utils/teams";
import MicrosoftLoginButton from "./MicrosoftLoginButton";
 
export default function Sidebar({ open, onClose }) {
  const navigate = useNavigate();
  const { user, logout } = useContext(UserContext);
 
  function handleLogout() {
    logout();
    onClose();
    navigate("/login");
  }
 
  // Removed Microsoft login fallback
  return (
    <div
      className={`fixed inset-0 z-20 transition-all ${
        open ? "visible" : "invisible"
      }`}
      aria-hidden={!open}
    >
      <div
        className={`fixed inset-0 bg-black bg-opacity-30 transition-opacity ${
          open ? "opacity-100" : "opacity-0"
        }`}
        onClick={onClose}
      />
      <aside
        className={`fixed top-0 left-0 h-full w-64 bg-white shadow-lg transform transition-transform ${
          open ? "translate-x-0" : "-translate-x-full"
        }`}
      >
        <button
          className="p-4 focus:outline-none"
          onClick={onClose}
          aria-label="Close menu"
        >
          <IoMenu size={24} />
        </button>
        <nav className="flex flex-col gap-2 p-4">
          {!user && !isInTeams() && (
            <>
              <MicrosoftLoginButton
                onSuccess={() => {
                  onClose();
                  navigate("/");
                }}
                onError={(e) => {
                  console.error("Microsoft login failed", e);
                }}
              />
              <div className="text-xs text-gray-400 text-center">or</div>
              <Link
                to="/login"
                className="py-2 px-4 rounded hover:bg-blue-100"
                onClick={onClose}
              >
                Login
              </Link>
              <Link
                to="/signup"
                className="py-2 px-4 rounded hover:bg-blue-100"
                onClick={onClose}
              >
                Signup
              </Link>
            </>
          )}
          {user && (
            <button
              className="py-2 px-4 rounded hover:bg-blue-100 text-left"
              onClick={handleLogout}
            >
              Logout
            </button>
          )}
        </nav>
      </aside>
    </div>
  );
}