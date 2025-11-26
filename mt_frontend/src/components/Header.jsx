import React, { useContext, useEffect } from "react";
import { Link } from "react-router-dom";
import UserContext from "../context/UserContext";
import { HiMenu } from "react-icons/hi";

export default function Header({ onMenuClick }) {
  const { user } = useContext(UserContext);
  
  useEffect(() => {
    console.log('[Header] User state updated:', user);
  }, [user]);

  return (
    <header className="bg-blue-700 text-white p-4 flex items-center justify-between">
      <button
        className="mr-2 focus:outline-none"
        onClick={onMenuClick}
        aria-label="Open menu"
      >
        <HiMenu size={28} />
      </button>

      <div className="flex items-center justify-center flex-1 gap-2">
        <img src="/logo.png" alt="Mini Tasker Logo" className="h-8" />
        <span className="text-2xl font-bold">Mini&nbsp;Tasker</span>
      </div>

      <div className="flex items-center gap-4">
        <span
          className={`text-xs font-medium ${
            user ? "text-green-200" : "text-yellow-200"
          }`}
        >
          {user ? "Logged in" : "Not logged in"}
        </span>

        {user?.name ? (
          <Link
            to="#"
            className="text-sm font-medium hover:underline"
            title="Go to Profile"
          >
            Welcome, {user.name}
          </Link>
        ) : null}
      </div>
    </header>
  );
}



// export default function Header({ onMenuClick }) {
//   return (
//     <header className="bg-blue-700 text-white p-4 flex items-center justify-between">
//       <button
//         className="mr-2 focus:outline-none"
//         onClick={onMenuClick}
//         aria-label="Open menu"
//       >
//         <svg width="28" height="28" fill="none" stroke="currentColor" strokeWidth="2">
//           <path d="M4 7h20M4 14h20M4 21h20" />
//         </svg>
//       </button>
//       <div className="flex items-center justify-center flex-1 gap-2">
//         <img src="/logo.png" alt="Mini Tasker Logo" className="h-8" />
//         <span className="text-2xl font-bold">Mini&nbsp;Tasker</span>
//       </div>
//     </header>
//   );
// }
