// Dummy auth functions for demonstration. Replace with real API calls.

// export function login(username, password) {
//   // Simulate login: accept any non-empty username/password
//   return new Promise((resolve) => {
//     setTimeout(() => {
//       if (username && password) {
//         localStorage.setItem("user", username);
//         resolve(true);
//       } else {
//         resolve(false);
//       }
//     }, 500);
//   });
// }

// export function signup(username, password) {
//   // Simulate signup: always succeed if fields are filled
//   return new Promise((resolve) => {
//     setTimeout(() => {
//       if (username && password) {
//         resolve(true);
//       } else {
//         resolve(false);
//       }
//     }, 500);
//   });
// }

// export function logout() {
//   localStorage.removeItem("user");
// }

import { loginUser, createUser } from "../api/api";

export async function login(email, password) {
  try {
    console.log("Logging in with:", { email, password });
    const user = await loginUser(email, password);
    if (user) {
      localStorage.setItem("user", JSON.stringify(user));
      return true;
    }
    return false;
  } catch {
    return false;
  }
}

export async function signup(username, email, password) {
  try {
    await createUser(username, email, password);
    return true;
  } catch {
    return false;
  }
}

export function logout() {
  localStorage.removeItem("user");
}

export function getCurrentUser() {
  const user = localStorage.getItem("user");
  return user ? JSON.parse(user) : null;
}

