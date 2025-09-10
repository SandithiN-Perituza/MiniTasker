import React, { useState } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import TaskList from "./components/TaskList";
import Sidebar from "./components/Sidebar";
import Login from "./pages/Login";
import Signup from "./pages/Signup";
import Header from "./components/Header";
import Footer from "./components/Footer";
import AuthRedirectHandler from "./pages/AuthRedirectHandler";
import SSOHandler from "./components/SSOHandler";

export default function App() {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [refreshTrigger, setRefreshTrigger] = useState(0);
  const [currentUser, setCurrentUser] = useState(null);
  return (
    <Router>
      <div className="flex flex-col min-h-screen bg-gray-50">
        <SSOHandler setCurrentUser={setCurrentUser} setRefreshTrigger={setRefreshTrigger} />
        
        <Header onMenuClick={() => setSidebarOpen(true)} refreshTrigger={refreshTrigger} />
        <Sidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} setRefreshTrigger={setRefreshTrigger}/>
        
        <main className="flex-1 p-4">
          <Routes>
            <Route path="/" element={<TaskList />} />
            <Route path="/login" element={<Login />} />
            <Route path="/signup" element={<Signup />} />
            {/* <Route path="/auth/callback" element={<MicrosoftLoginButton />} /> */}
            <Route path="/auth/callback" element={<AuthRedirectHandler />} />
          </Routes>
        </main>

        <Footer />
      </div>
    </Router>
  );
}
