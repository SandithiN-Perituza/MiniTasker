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

// export async function Microsoftlogin(email, password) {
//   try {
//     console.log("Logging in with:", { email, password });
//     const user = await loginMicrosoftUser(email, password);
//     if (user) {
//       localStorage.setItem("user", JSON.stringify(user));
//       return true;
//     }
//     return false;
//   } catch {
//     return false;
//   }
// }

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

