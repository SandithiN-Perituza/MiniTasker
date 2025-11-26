import React, { useState } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import { UserProvider } from "./context/UserProvider";
import TaskList from "./components/TaskList";
import TaskDetail from "./pages/TaskDetail";
import Sidebar from "./components/Sidebar";
import Login from "./pages/Login";
import Signup from "./pages/Signup";
import Header from "./components/Header";
import Footer from "./components/Footer";
import DebugConsole from "./components/DebugConsole";

export default function App() {
  const [sidebarOpen, setSidebarOpen] = useState(false);

  return (
    <Router>
      <div className="flex flex-col min-h-screen bg-gray-50">
        <Header onMenuClick={() => setSidebarOpen(true)} />
        <Sidebar
          open={sidebarOpen}
          onClose={() => setSidebarOpen(false)}
        />
        <main className="flex-1 p-4">
          <Routes>
            <Route path="/" element={<TaskList />} />
            <Route path="/task/:taskId" element={<TaskDetail />} />
            <Route path="/login" element={<Login />} />
            <Route path="/signup" element={<Signup />} />
          </Routes>
        </main>
        <Footer />
        <DebugConsole />
      </div>
    </Router>
  );
}


// import React, { useState, useEffect } from "react";
// import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
// import { MsalProvider } from "@azure/msal-react";
// import { msalInstance } from "./utils/msalConfig";
// import { UserProvider } from "./context/UserProvider";
// import { TeamsProvider } from "./context/TeamsProvider";
// import TaskList from "./components/TaskList";
// import Sidebar from "./components/Sidebar";
// import Login from "./pages/Login";
// import Signup from "./pages/Signup";
// import NotificationTest from "./pages/NotificationTest";
// import Header from "./components/Header";
// import Footer from "./components/Footer";

// export default function App() {
//   const [sidebarOpen, setSidebarOpen] = useState(false);

//   // Make MSAL instance available globally for token refresh
//   useEffect(() => {
//     if (typeof window !== 'undefined') {
//       window.msalInstance = msalInstance;
//     }
//   }, []);

//   // Handle Microsoft login redirects
//   useEffect(() => {
//     const handleRedirect = async () => {
//       try {
//         const response = await msalInstance.handleRedirectPromise();
//         if (response) {
//           console.log("Redirect response received in App:", response);
//           // The actual login processing will be handled by the Sidebar component
//           // This just ensures the redirect is properly handled
//         }
//       } catch (error) {
//         console.error("Error handling redirect:", error);
//       }
//     };

//     handleRedirect();
//   }, []);

//   return (
//     <MsalProvider instance={msalInstance}>
//       <TeamsProvider>
//         <UserProvider>
//           <Router>
//             <div className="flex flex-col min-h-screen bg-gray-50">
//               <Header onMenuClick={() => setSidebarOpen(true)} />
//               <Sidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} />
//               <main className="flex-1 p-4">
//                 <Routes>
//                   <Route path="/" element={<TaskList />} />
//                   <Route path="/login" element={<Login />} />
//                   <Route path="/signup" element={<Signup />} />
//                   <Route path="/notification-test" element={<NotificationTest />} />
//                 </Routes>
//               </main>
//               <Footer />
//             </div>
//           </Router>
//         </UserProvider>
//       </TeamsProvider>
//     </MsalProvider>
//   );
// }