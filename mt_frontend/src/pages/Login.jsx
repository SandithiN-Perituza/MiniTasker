import React, { useState, useContext } from "react";
import { useNavigate } from "react-router-dom";
import UserContext from "../context/UserContext";
import { isInTeams } from "../utils/teams";

export default function Login() {
  const { login } = useContext(UserContext);
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
      navigate("/");
    } else {
      setError("Invalid credentials", form.email);
    }
  }

  if (isInTeams()) {
    return (
      <div className="max-w-sm mx-auto mt-16 bg-white p-6 rounded shadow">
        <h2 className="text-xl font-bold mb-2">Login</h2>
        <p className="text-sm text-gray-600">
          Signing you in with your Microsoft Teams account...
        </p>
      </div>
    );
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
