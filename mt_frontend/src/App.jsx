import React, { useState } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import Dashboard from "./components/Dashboard";
import Sidebar from "./components/Sidebar";
import Login from "./pages/Login";
import Signup from "./pages/Signup";

export default function App() {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  return (
    <Router>
      <div className="min-h-screen flex flex-col bg-gray-50">
        <header className="bg-blue-600 text-white p-4 flex items-center justify-between">
          <button
            className="mr-2 focus:outline-none"
            onClick={() => setSidebarOpen(true)}
            aria-label="Open menu"
          >
            <svg width="28" height="28" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M4 7h20M4 14h20M4 21h20" />
            </svg>
          </button>
          <span className="text-2xl font-bold flex-1 text-center">MiniTasker Dashboard</span>
        </header>
        <Sidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} />
        <main className="flex-1">
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/login" element={<Login />} />
            <Route path="/signup" element={<Signup />} />
          </Routes>
        </main>
        <footer className="bg-gray-800 text-white text-center p-3 fixed bottom-0 left-0 w-full z-10">
          &copy; {new Date().getFullYear()} MiniTasker
        </footer>
      </div>
    </Router>
  );
}
