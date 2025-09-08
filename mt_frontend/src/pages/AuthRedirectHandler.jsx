import { useMsal } from "@azure/msal-react";
import { useEffect } from "react";
import { useNavigate } from "react-router-dom";

export default function AuthRedirectHandler() {
  const { accounts } = useMsal();
  const navigate = useNavigate();

  useEffect(() => {
    if (accounts.length > 0) {
      const user = accounts[0];
      localStorage.setItem("user", JSON.stringify(user));
      navigate("/"); // Redirect to TaskList
    }
  }, [accounts, navigate]);

  return <div>Redirecting...</div>; // Optional loading message
}