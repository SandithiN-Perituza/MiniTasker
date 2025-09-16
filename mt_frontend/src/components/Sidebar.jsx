// import React, { useEffect, useState } from "react";
// import { Link, useNavigate } from "react-router-dom";
// import { logout } from "../utils/auth";
// import MicrosoftLoginButton from "./MicrosoftLogin";
// import * as microsoftTeams from "@microsoft/teams-js";

// function isLoggedIn() {
//   return !!localStorage.getItem("user");
// }

// export default function Sidebar({ open, onClose, setRefreshTrigger }) {
//   const navigate = useNavigate();
//   const [isInTeams, setIsInTeams] = useState(false);
//   const loggedIn = isLoggedIn();

//   useEffect(() => {
//     // Detect if app is running inside Microsoft Teams
//     microsoftTeams.app.initialize().then(() => {
//       microsoftTeams.app.getContext().then(() => {
//         setIsInTeams(true);
//       });
//     }).catch(() => {
//       setIsInTeams(false);
//     });
//   }, []);

//   function handleLogout() {
//     logout();
//     setRefreshTrigger(prev => prev + 1);
//     onClose();
//     navigate("/login");
//   }
  
//   return (
//     <div
//       className={`fixed inset-0 z-20 transition-all ${open ? "visible" : "invisible"}`}
//       aria-hidden={!open}
//     >
//       <div
//         className={`fixed inset-0 bg-black bg-opacity-30 transition-opacity ${open ? "opacity-100" : "opacity-0"}`}
//         onClick={onClose}
//       />
//       <aside
//         className={`fixed top-0 left-0 h-full w-64 bg-white shadow-lg transform transition-transform ${
//           open ? "translate-x-0" : "-translate-x-full"
//         }`}
//       >
//         <button className="p-4 focus:outline-none" onClick={onClose} aria-label="Close menu">
//           <svg width="24" height="24" fill="none" stroke="currentColor" strokeWidth="2">
//             <path d="M6 18L18 6M6 6l12 12" />
//           </svg>
//         </button>
//         <nav className="flex flex-col gap-2 p-4">
//           {!isInTeams && !loggedIn && (
//             <>
//               <Link to="/login" className="py-2 px-4 rounded hover:bg-blue-100" onClick={onClose}>
//                 Login
//               </Link>
//               <Link to="/signup" className="py-2 px-4 rounded hover:bg-blue-100" onClick={onClose}>
//                 Signup
//               </Link>
//               <MicrosoftLoginButton setRefreshTrigger={setRefreshTrigger} />
//             </>
//           )}
//           {loggedIn && (
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

// import React, { useEffect, useState } from "react";
// import { Link, useNavigate } from "react-router-dom";
// import { logout } from "../utils/auth";
// import MicrosoftLoginButton from "./MicrosoftLogin";
// import * as microsoftTeams from "@microsoft/teams-js";

// function isLoggedIn() {
//   return !!localStorage.getItem("user");
// }

// export default function Sidebar({ open, onClose, setRefreshTrigger }) {
//   const navigate = useNavigate();
//   const [isInTeams, setIsInTeams] = useState(false); // ✅ useState is now used correctly
//   const loggedIn = isLoggedIn();

//   useEffect(() => {
//     // Detect if app is running inside Microsoft Teams
//     microsoftTeams.app.initialize().then(() => {
//       microsoftTeams.app.getContext().then(() => {
//         setIsInTeams(true);
//       });
//     }).catch(() => {
//       setIsInTeams(false);
//     });
//   }, []);
//   console.log("Is in Teams:", isInTeams);

//   function handleLogout() {
//     logout();
//     setRefreshTrigger(prev => prev + 1);
//     onClose();
//     navigate("/login");
//   }

//   return (
//     <div
//       className={`fixed inset-0 z-20 transition-all ${open ? "visible" : "invisible"}`}
//       aria-hidden={!open}
//     >
//       <div
//         className={`fixed inset-0 bg-black bg-opacity-30 transition-opacity ${open ? "opacity-100" : "opacity-0"}`}
//         onClick={onClose}
//       />
//       <aside
//         className={`fixed top-0 left-0 h-full w-64 bg-white shadow-lg transform transition-transform ${
//           open ? "translate-x-0" : "-translate-x-full"
//         }`}
//       >
//         <button className="p-4 focus:outline-none" onClick={onClose} aria-label="Close menu">
//           <svg width="24" height="24" fill="none" stroke="currentColor" strokeWidth="2">
//             <path d="M6 18L18 6M6 6l12 12" />
//           </svg>
//         </button>
//         <nav className="flex flex-col gap-2 p-4">
//           {!loggedIn && (
//             <>
//               <Link to="/login" className="py-2 px-4 rounded hover:bg-blue-100" onClick={onClose}>
//                 Login
//               </Link>
//               <Link to="/signup" className="py-2 px-4 rounded hover:bg-blue-100" onClick={onClose}>
//                 Signup
//               </Link>
//               <MicrosoftLoginButton setRefreshTrigger={setRefreshTrigger} />
//             </>
//           )}
//           {loggedIn && (
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

// import React, { useEffect, useState } from "react";
// import { Link, useNavigate } from "react-router-dom";
// import { logout } from "../utils/auth";
// import MicrosoftLoginButton from "./MicrosoftLogin";
// import * as microsoftTeams from "@microsoft/teams-js";

// function isLoggedIn() {
//   return !!localStorage.getItem("user");
// }

// export default function Sidebar({ open, onClose, setRefreshTrigger }) {
//   const navigate = useNavigate();
//   const [isInTeams, setIsInTeams] = useState(false);
//   const [currentUser, setCurrentUser] = useState(null);
//   const loggedIn = isLoggedIn();

//   console.log("currentUser:", currentUser);
//   console.log("isInTeams:", isInTeams);

//   useEffect(() => {
//     // Detect if app is running inside Microsoft Teams
//     if (window.parent !== window) {
//       microsoftTeams.app.initialize()
//         .then(() => {
//           microsoftTeams.authentication.getAuthToken({
//             successCallback: async (token) => {
//               console.log("SSO token received:", token);

//               try {
//                 const response = await fetch("https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api/msal-login", {
//                   method: "POST",
//                   headers: {
//                     Authorization: `Bearer ${token}`,
//                     "Content-Type": "application/json"
//                   }
//                 });

//                 if (response.ok) {
//                   const data = await response.json();
//                   localStorage.setItem("user", JSON.stringify(data.user));
//                   localStorage.setItem("accessToken", token);
//                   setCurrentUser(data.user);
//                   setRefreshTrigger(prev => prev + 1);
//                 } else {
//                   console.error("Token validation failed:", await response.text());
//                 }
//               } catch (err) {
//                 console.error("Error calling backend:", err);
//               }
//             },
//             failureCallback: (error) => {
//               console.error("SSO failed:", error);
//             }
//           });
//           setIsInTeams(true);
//         })
//         .catch((err) => {
//           console.warn("Teams SDK initialization failed:", err);
//           setIsInTeams(false);
//         });
//     }
//   }, [setRefreshTrigger]);

//   function handleLogout() {
//     logout();
//     setRefreshTrigger(prev => prev + 1);
//     onClose();
//     navigate("/login");
//   }

//   return (
//     <div
//       className={`fixed inset-0 z-20 transition-all ${open ? "visible" : "invisible"}`}
//       aria-hidden={!open}
//     >
//       <div
//         className={`fixed inset-0 bg-black bg-opacity-30 transition-opacity ${open ? "opacity-100" : "opacity-0"}`}
//         onClick={onClose}
//       />
//       <aside
//         className={`fixed top-0 left-0 h-full w-64 bg-white shadow-lg transform transition-transform ${
//           open ? "translate-x-0" : "-translate-x-full"
//         }`}
//       >
//         <button className="p-4 focus:outline-none" onClick={onClose} aria-label="Close menu">
//           <svg width="24" height="24" fill="none" stroke="currentColor" strokeWidth="2">
//             <path d="M6 18L18 6M6 6l12 12" />
//           </svg>
//         </button>
//         <nav className="flex flex-col gap-2 p-4">
//           {!loggedIn && !isInTeams && (
//             <>
//               <Link to="/login" className="py-2 px-4 rounded hover:bg-blue-100" onClick={onClose}>
//                 Login
//               </Link>
//               <Link to="/signup" className="py-2 px-4 rounded hover:bg-blue-100" onClick={onClose}>
//                 Signup
//               </Link>
//               <MicrosoftLoginButton setRefreshTrigger={setRefreshTrigger} />
//             </>
//           )}
//           {loggedIn && (
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

import React, { useEffect, useState, useContext } from "react";
import { Link, useNavigate } from "react-router-dom";
import { logout } from "../utils/auth";
import MicrosoftLoginButton from "./MicrosoftLogin";
import * as microsoftTeams from "@microsoft/teams-js";
import UserContext from "../context/UserContext";

export default function Sidebar({ open, onClose, setRefreshTrigger }) {
  const navigate = useNavigate();
  const [isInTeams, setIsInTeams] = useState(false);

  const { currentUser, setCurrentUser } = useContext(UserContext);

  useEffect(() => {
    // Detect if app is running inside Microsoft Teams
    if (window.parent !== window) {
      microsoftTeams.app.initialize()
        .then(() => {
          microsoftTeams.app.getContext().then(() => {
            setIsInTeams(true);
          });
        })
        .catch(() => {
          setIsInTeams(false);
        });
    }
  }, []);
  
  console.log("Current user in sidebar.jsx: ", currentUser);
  console.log("Is in Teams in sidebar.jsx: :", isInTeams);

  const loggedIn = !!currentUser || !!localStorage.getItem("user");

  function handleLogout() {
    logout();
    setCurrentUser(null);
    setRefreshTrigger(prev => prev + 1);
    onClose();
    navigate("/login");
  }

  return (
    <div
      className={`fixed inset-0 z-20 transition-all ${open ? "visible" : "invisible"}`}
      aria-hidden={!open}
    >
      <div
        className={`fixed inset-0 bg-black bg-opacity-30 transition-opacity ${open ? "opacity-100" : "opacity-0"}`}
        onClick={onClose}
      />
      <aside
        className={`fixed top-0 left-0 h-full w-64 bg-white shadow-lg transform transition-transform ${
          open ? "translate-x-0" : "-translate-x-full"
        }`}
      >
        <button className="p-4 focus:outline-none" onClick={onClose} aria-label="Close menu">
          <svg width="24" height="24" fill="none" stroke="currentColor" strokeWidth="2">
            <path d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
        <nav className="flex flex-col gap-2 p-4">
          {/* Show login/signup only if NOT logged in and NOT inside Teams */}
          {!loggedIn && !isInTeams && (
            <>
              <Link to="/login" className="py-2 px-4 rounded hover:bg-blue-100" onClick={onClose}>
                Login
              </Link>
              <Link to="/signup" className="py-2 px-4 rounded hover:bg-blue-100" onClick={onClose}>
                Signup
              </Link>
              <MicrosoftLoginButton setRefreshTrigger={setRefreshTrigger} />
            </>
          )}

          {/* Show logout if logged in */}
          {loggedIn && (
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




//--------------------------------------------------------------------------------------------------------------
// import React from "react";
// import { Link, useNavigate } from "react-router-dom";
// import { logout } from "../utils/auth";

// function isLoggedIn() {
//   return !!localStorage.getItem("user");
// }

// export default function Sidebar({ open, onClose }) {
//   const navigate = useNavigate();
//   const loggedIn = isLoggedIn();

//   function handleLogout() {
//     logout();
//     onClose();
//     navigate("/login");
//   }

//   return (
//     <div
//       className={`fixed inset-0 z-20 transition-all ${open ? "visible" : "invisible"}`}
//       aria-hidden={!open}
//     >
//       <div
//         className={`fixed inset-0 bg-black bg-opacity-30 transition-opacity ${open ? "opacity-100" : "opacity-0"}`}
//         onClick={onClose}
//       />
//       <aside
//         className={`fixed top-0 left-0 h-full w-64 bg-white shadow-lg transform transition-transform ${
//           open ? "translate-x-0" : "-translate-x-full"
//         }`}
//       >
//         <button className="p-4 focus:outline-none" onClick={onClose} aria-label="Close menu">
//           <svg width="24" height="24" fill="none" stroke="currentColor" strokeWidth="2">
//             <path d="M6 18L18 6M6 6l12 12" />
//           </svg>
//         </button>
//         <nav className="flex flex-col gap-2 p-4">
//           {!loggedIn && (
//             <>
//               <Link to="/login" className="py-2 px-4 rounded hover:bg-blue-100" onClick={onClose}>
//                 Login
//               </Link>
//               <Link to="/signup" className="py-2 px-4 rounded hover:bg-blue-100" onClick={onClose}>
//                 Signup
//               </Link>
//             </>
//           )}
//           {loggedIn && (
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

