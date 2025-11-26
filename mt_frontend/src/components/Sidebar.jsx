import React, { useContext } from "react";
import { useNavigate } from "react-router-dom";
import UserContext from "../context/UserContext";
import { IoMenu } from "react-icons/io5";
import MicrosoftLoginButton from "./MicrosoftLoginButton";

export default function Sidebar({ open, onClose }) {
  const navigate = useNavigate();
  const { user, logout } = useContext(UserContext);

  function handleLogout() {
    logout();
    onClose();
    navigate("/");
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
          {!user ? (
            <MicrosoftLoginButton
              onSuccess={() => {
                onClose();
                navigate("/");
              }}
              onError={(e) => {
                console.error("Microsoft login failed", e);
              }}
            />
          ) : (
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
