import React, { useState, useEffect } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import TaskList from "./components/TaskList";
import Sidebar from "./components/Sidebar";
import Login from "./pages/Login";
import Signup from "./pages/Signup";
import Header from "./components/Header";
import Footer from "./components/Footer";
import AuthRedirectHandler from "./pages/AuthRedirectHandler";
import SSOHandler from "./components/SSOHandler";
import { useMsal } from "@azure/msal-react"
import * as microsoftTeams from "@microsoft/teams-js";

export default function App() {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const [refreshTrigger, setRefreshTrigger] = useState(0);
  const [currentUser, setCurrentUser] = useState(null);
  const { instance } = useMsal();

  useEffect(() => {
  if (window.parent !== window) {
      console.log("Actual iframe origin:", window.location.origin);
      console.log("Parent iframe origin:", window.parent.location.origin);
    microsoftTeams.app.initialize().then(() => {
      instance.handleRedirectPromise().then(async (response) => {
        if (response) {
          const accessToken = response.accessToken;
          const result = await loginMicrosoftUser(accessToken);
          if (result.user) {
            setCurrentUser(result.user);
            setRefreshTrigger(prev => prev + 1);
            localStorage.setItem("accessToken", accessToken);
            localStorage.setItem("user", JSON.stringify(result.user));
          }
        }
      }).catch((error) => {
        console.error("Redirect error:", error);
      });
    }).catch((err) => {
      console.warn("Teams SDK init failed:", err);
    });
  }
}, [instance]);


  // useEffect(() => {
  //   // Only run redirect handler if inside Teams
  //   if (window.parent !== window) {
  //     microsoftTeams.app.initialize().then(() => {
  //       instance.handleRedirectPromise().then(async (response) => {
  //         if (response) {
  //           const accessToken = response.accessToken;
  //           const result = await loginMicrosoftUser(accessToken);
  //           if (result.user) {
  //             setCurrentUser(result.user);
  //             setRefreshTrigger(prev => prev + 1);
  //             localStorage.setItem("accessToken", accessToken);
  //             localStorage.setItem("user", JSON.stringify(result.user));
  //           }
  //         }
  //       }).catch((error) => {
  //         console.error("Redirect error:", error);
  //       });
  //     }).catch((err) => {
  //       console.warn("Teams SDK init failed:", err);
  //     });
  //   }
  // }, [instance]);

  //   useEffect(() => {
  //   instance.handleRedirectPromise().then(async (response) => {
  //     if (response) {
  //       const accessToken = response.accessToken;
  //       const result = await loginMicrosoftUser(accessToken);
  //       if (result.user) {
  //         setCurrentUser(result.user);
  //         setRefreshTrigger(prev => prev + 1);
  //         localStorage.setItem("accessToken", accessToken);
  //         localStorage.setItem("user", JSON.stringify(result.user));
  //       }
  //     }
  //   });
  // }, [instance]);

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

