import React, { useContext, useState } from "react";
import { Link } from "react-router-dom";
import UserContext from "../context/UserContext";
import { sendTestNotification } from "../api/api";
import { forceTokenRefresh, hasValidToken } from "../utils/tokenManager";

export default function Header({ onMenuClick }) {
  const { user } = useContext(UserContext);
  const [isLoading, setIsLoading] = useState(false);
  const [message, setMessage] = useState("");

  const handleSendNotification = async () => {
    if (!user) {
      alert("Please log in to send notifications");
      return;
    }

    setIsLoading(true);
    setMessage("");

    try {
      console.log("🔔 Starting notification test from header...");
      console.log("Current user:", user);
      
      const result = await sendTestNotification().catch(e => {
        if (/Could not acquire authentication token/i.test(e.message)) {
          console.warn("Skipping notification test, no token.");
          return { notificationSkipped: true };
        }
        throw e;
      });
      console.log("✅ Notification result:", result);
      setMessage("✅ Notification sent!");
      setTimeout(() => setMessage(""), 5000);
    } catch (error) {
      console.error("❌ Notification error:", error);
      const errorMsg = error.message.length > 60 ? 
        error.message.substring(0, 60) + '...' : 
        error.message;
      setMessage(`❌ ${errorMsg}`);
      setTimeout(() => setMessage(""), 10000); // Longer timeout for errors
    } finally {
      setIsLoading(false);
    }
  };

  const handleRefreshToken = async () => {
    if (!user) return;
    
    setMessage("🔄 Refreshing token...");
    try {
      const token = await forceTokenRefresh();
      if (token) {
        setMessage("✅ Token refreshed!");
      } else {
        setMessage("❌ Token refresh failed");
      }
      setTimeout(() => setMessage(""), 3000);
    } catch (error) {
      console.error("Token refresh error:", error);
      setMessage("❌ Token refresh error");
      setTimeout(() => setMessage(""), 3000);
    }
  };

  return (
    <header className="bg-blue-700 text-white p-4 flex items-center justify-between">
      <button
        className="mr-2 focus:outline-none"
        onClick={onMenuClick}
        aria-label="Open menu"
      >
        <svg width="28" height="28" fill="none" stroke="currentColor" strokeWidth="2">
          <path d="M4 7h20M4 14h20M4 21h20" />
        </svg>
      </button>

      <div className="flex items-center justify-center flex-1 gap-2">
        <img src="/logo.png" alt="Mini Tasker Logo" className="h-8" />
        <span className="text-2xl font-bold">Mini&nbsp;Tasker</span>
      </div>

      <div className="flex items-center gap-4">
        {/* Debug info - remove this later */}
        <span className="text-xs text-blue-200">User: {user ? 'Logged in' : 'Not logged in'}</span>
        
        {user && (
          <div className="flex items-center gap-2">
            <button
              onClick={handleRefreshToken}
              className="px-2 py-1 rounded text-xs font-medium bg-blue-500 text-white hover:bg-blue-600 transition-colors"
              title="Refresh authentication token"
            >
              🔄 Auth
            </button>
            
            <button
              onClick={handleSendNotification}
              disabled={isLoading}
              className={`px-3 py-1.5 rounded text-sm font-medium transition-all duration-200 ${
                isLoading
                  ? "bg-gray-400 text-gray-200 cursor-not-allowed"
                  : hasValidToken() 
                    ? "bg-green-500 text-white hover:bg-green-600 active:bg-green-700 hover:shadow-md"
                    : "bg-yellow-500 text-white hover:bg-yellow-600 active:bg-yellow-700 hover:shadow-md"
              }`}
              title={hasValidToken() ? "Send test notification (authenticated)" : "Send test notification (will try to get token)"}
            >
              {isLoading ? (
                <span className="flex items-center">
                  <svg className="animate-spin -ml-1 mr-1 h-3 w-3 text-white" fill="none" viewBox="0 0 24 24">
                    <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                    <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 0 1 8-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 0 1 4 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                  </svg>
                  ...
                </span>
              ) : (
                hasValidToken() ? "🔔 Test ✅" : "🔔 Test"
              )}
            </button>
            
            {message && (
              <span className={`text-xs font-medium ${
                message.startsWith("✅") ? "text-green-300" : "text-red-300"
              }`}>
                {message}
              </span>
            )}
          </div>
        )}

        {/* Always show notification button for testing - remove user check temporarily */}
        <button
          onClick={handleSendNotification}
          disabled={isLoading || !user}
          className={`px-3 py-1.5 rounded text-sm font-medium transition-all duration-200 ${
            isLoading || !user
              ? "bg-gray-400 text-gray-200 cursor-not-allowed"
              : "bg-red-500 text-white hover:bg-red-600 active:bg-red-700 hover:shadow-md"
          }`}
          title={!user ? "Please log in first" : "Send test notification"}
        >
          {isLoading ? "..." : "🔔 Always Visible"}
        </button>

        {user?.name && (
          <Link
            to="/profile"
            className="text-sm font-medium hover:underline"
            title="Go to Profile"
          >
            Welcome, {user.name}
          </Link>
        )}
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
