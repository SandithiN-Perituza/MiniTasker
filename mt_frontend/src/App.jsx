// import React, { useState } from "react";
// import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
// import TaskList from "./components/TaskList";
// import Sidebar from "./components/Sidebar";
// import Login from "./pages/Login";
// import Signup from "./pages/Signup";
// import Header from "./components/Header";
// import Footer from "./components/Footer";
// // import AuthRedirectHandler from "./pages/AuthRedirectHandler";
// import SSOHandler from "./components/SSOHandler";

// export default function App() {
//   const [sidebarOpen, setSidebarOpen] = useState(false);
//   const [refreshTrigger, setRefreshTrigger] = useState(0);
//   const [currentUser, setCurrentUser] = useState(null);
//   console.log("Current User in App:", currentUser);

//   return (
//     <Router>
//       <div className="flex flex-col min-h-screen bg-gray-50">
//         <SSOHandler setCurrentUser={setCurrentUser} setRefreshTrigger={setRefreshTrigger} />

//         <Header onMenuClick={() => setSidebarOpen(true)} refreshTrigger={refreshTrigger} />
//         <Sidebar open={sidebarOpen} onClose={() => setSidebarOpen(false)} setRefreshTrigger={setRefreshTrigger}/>

//         <main className="flex-1 p-4">
//           <Routes>
//             <Route path="/" element={<TaskList />} />
//             <Route path="/login" element={<Login />} />
//             <Route path="/signup" element={<Signup />} />
//             {/* <Route path="/auth/callback" element={<MicrosoftLoginButton />} /> */}
//             {/* <Route path="/auth/callback" element={<AuthRedirectHandler />} /> */}
//           </Routes>
//         </main>

//         <Footer />
//       </div>
//     </Router>
//   );
// }

// ======CURRENT STABLE=========================
// import React, { useState } from "react";
// import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
// import TaskList from "./components/TaskList";
// import Sidebar from "./components/Sidebar";
// import Login from "./pages/Login";
// import Signup from "./pages/Signup";
// import Header from "./components/Header";
// import Footer from "./components/Footer";
// import SSOHandler from "./components/SSOHandler";
// import Privacy from "./pages/Privacy";
// import Terms from "./pages/Terms";

// export default function App() {
//   const [sidebarOpen, setSidebarOpen] = useState(false);
//   const [refreshTrigger, setRefreshTrigger] = useState(0);
//   const [currentUser, setCurrentUser] = useState(null);

//   console.log("Current User in App.jsx:", currentUser);

//   return (
//     <Router>
//       <div className="flex flex-col min-h-screen bg-gray-50">
//         {/* Automatically handles Teams SSO */}
//         <SSOHandler
//           setCurrentUser={setCurrentUser}
//           setRefreshTrigger={setRefreshTrigger}
//         />

//         {/* Header and Sidebar */}
//         <Header onMenuClick={() => setSidebarOpen(true)} refreshTrigger={refreshTrigger} />
//         <Sidebar
//           open={sidebarOpen}
//           onClose={() => setSidebarOpen(false)}
//           setRefreshTrigger={setRefreshTrigger}
//           currentUser={currentUser}
//         />

//         {/* Main content */}
//         <main className="flex-1 p-4">
//           <Routes>
//             <Route path="/" element={<TaskList />} />
//             <Route path="/login" element={<Login />} />
//             <Route path="/signup" element={<Signup />} />
//             <Route path="/privacy" element={<Privacy />} />
//             <Route path="/terms" element={<Terms />} />
//           </Routes>
//         </main>

//         <Footer />
//       </div>
//     </Router>
//   );
// }

// ============================================

import React, { useState, useContext, useEffect } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import TaskList from "./components/TaskList";
import Sidebar from "./components/Sidebar";
import Login from "./pages/Login";
import Signup from "./pages/Signup";
import Header from "./components/Header";
import Footer from "./components/Footer";
import SSOHandler from "./components/SSOHandler";
import Privacy from "./pages/Privacy";
import Terms from "./pages/Terms";
import UserContext from "./context/UserContext";
// import { getCurrentUser } from "./utils/auth";

export default function App() {
  const [sidebarOpen, setSidebarOpen] = useState(false);
  const { currentUser, setCurrentUser, refreshTrigger, setRefreshTrigger } =
    useContext(UserContext);

  console.log("---Sandithi's version---");
  
  console.log("Actual iframe origin:", window.location.origin);

  console.log("Current User in App.jsx:", currentUser);

  useEffect(() => {
    console.log("Current User in App.jsx in useEffect:", currentUser);
  }, [currentUser]);

  return (
    <Router>
      <div className="flex flex-col min-h-screen bg-gray-50">
        <SSOHandler
          setCurrentUser={setCurrentUser}
          setRefreshTrigger={setRefreshTrigger}
        />
        <Header
          onMenuClick={() => setSidebarOpen(true)}
          refreshTrigger={refreshTrigger}
        />
        <Sidebar
          open={sidebarOpen}
          onClose={() => setSidebarOpen(false)}
          setRefreshTrigger={setRefreshTrigger}
        />
        <main className="flex-1 p-4">
          <Routes>
            <Route path="/" element={<TaskList />} />
            <Route path="/login" element={<Login />} />
            <Route path="/signup" element={<Signup />} />
            <Route path="/privacy" element={<Privacy />} />
            <Route path="/terms" element={<Terms />} />
          </Routes>
        </main>
        <Footer />
      </div>
    </Router>
  );
}
