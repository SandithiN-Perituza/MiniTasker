import React, { useState } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { MsalProvider } from "@azure/msal-react";
import { msalInstance } from "./utils/msalConfig";
import { UserProvider } from "./context/UserProvider";
import { TeamsProvider } from "./context/TeamsProvider";
import TaskList from "./components/TaskList";
import Sidebar from "./components/Sidebar";
import Login from "./pages/Login";
import Signup from "./pages/Signup";
import Header from "./components/Header";
import Footer from "./components/Footer";

export default function App() {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  console.log("--- Sandithi's version - feature-microsoft-login ---");

  return (
    <MsalProvider instance={msalInstance}>
      <TeamsProvider>
        <UserProvider>
          <Router>
            <div className="flex flex-col min-h-screen bg-gray-50">
              <Header onMenuClick={() => setSidebarOpen(true)} />
              <Sidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} />
              <main className="flex-1 p-4">
                <Routes>
                  <Route path="/" element={<TaskList />} />
                  <Route path="/login" element={<Login />} />
                  <Route path="/signup" element={<Signup />} />
                </Routes>
              </main>
              <Footer />
            </div>
          </Router>
        </UserProvider>
      </TeamsProvider>
    </MsalProvider>
  );
}