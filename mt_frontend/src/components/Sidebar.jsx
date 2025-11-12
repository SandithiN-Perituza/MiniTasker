import React, { useContext } from "react";
import { Link, useNavigate } from "react-router-dom";
import { msalInstance } from "../utils/msalConfig";
import UserContext from "../context/UserContext";
import { IoMenu } from "react-icons/io5";

export default function Sidebar({ open, onClose }) {
  const navigate = useNavigate();
  const { user, login, logout } = useContext(UserContext);

  function handleLogout() {
    logout(); // Use context logout instead of utils/auth logout
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

      const userData = await res.json();
      login(userData); // Use context login instead of localStorage
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
          <IoMenu size={24} />
        </button>
        <nav className="flex flex-col gap-2 p-4">
          {!user && (
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

