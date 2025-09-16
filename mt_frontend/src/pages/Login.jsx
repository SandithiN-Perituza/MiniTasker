import React, { useState, useContext } from "react";
import { useNavigate } from "react-router-dom";
import { login } from "../utils/auth";
import UserContext from "../context/UserContext";

export default function Login() {
  const { setCurrentUser, setRefreshTrigger } = useContext(UserContext);

  const [form, setForm] = useState({ email: "", password: "" });
  const [error, setError] = useState("");
  const navigate = useNavigate();

  function handleChange(e) {
    setForm({ ...form, [e.target.name]: e.target.value });
  }

  async function handleSubmit(e) {
    e.preventDefault();
    console.log("Form submitted:", form);
    console.log("Form submitted:email--", form.email);
    console.log("Form submitted:password--", form.password);

    const success = await login(form.email, form.password);
    console.log("Login success:", success);
    if (success) {
      const storedUser = localStorage.getItem("user");
      if (storedUser) {
        const user = JSON.parse(storedUser);
        setCurrentUser(user);
        setRefreshTrigger((prev) => !prev);
      }
      navigate("/");
    } else {
      setError("Invalid credentials", form.email);
    }
  }

  return (
    <div className="max-w-sm mx-auto mt-16 bg-white p-6 rounded shadow">
      <h2 className="text-xl font-bold mb-4">Login</h2>
      <form onSubmit={handleSubmit} className="space-y-4">
        <input
          className="border p-2 w-full"
          name="email"
          placeholder="Email"
          value={form.email}
          onChange={handleChange}
          required
        />
        <input
          className="border p-2 w-full"
          name="password"
          type="password"
          placeholder="Password"
          value={form.password}
          onChange={handleChange}
          required
        />
        {error && <div className="text-red-500">{error}</div>}
        <button
          className="bg-blue-600 text-white px-4 py-2 rounded w-full"
          type="submit"
        >
          Login
        </button>
      </form>
    </div>
  );
}
