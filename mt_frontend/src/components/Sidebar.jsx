import React from "react";
import { Link, useNavigate } from "react-router-dom";
import { logout } from "../utils/auth";
import { msalInstance } from "../utils/msalConfig";

export default function Sidebar({ open, onClose }) {
  const navigate = useNavigate();
  const loggedIn = isLoggedIn();

  function isLoggedIn() {
    return !!localStorage.getItem("user");
  }

  function handleLogout() {
    logout();
    onClose();
    navigate("/login");
  }

  async function handleMicrosoftLogin() {
    try {
      const loginResponse = await msalInstance.loginPopup({
        scopes: ["user.read"],
      });
      const idToken = loginResponse.idToken;

      // Send token to backend for validation
      const res = await fetch(
        "https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api/auth/microsoft",
        {
          method: "POST",
          headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${idToken}`,
          },
        }
      );

      const user = await res.json();
      localStorage.setItem("user", JSON.stringify(user));
      onClose();
      navigate("/");
    } catch (err) {
      console.error("Microsoft login failed", err);
    }
  }

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
          <svg
            width="24"
            height="24"
            fill="none"
            stroke="currentColor"
            strokeWidth="2"
          >
            <path d="M6 18L18 6M6 6l12 12" />
          </svg>
        </button>
        <nav className="flex flex-col gap-2 p-4">
          {!loggedIn && (
            <>
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
              <button
                className="py-2 px-4 rounded hover:bg-blue-100 text-left"
                onClick={handleMicrosoftLogin}
              >
                Sign in with Microsoft
              </button>
            </>
          )}
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
