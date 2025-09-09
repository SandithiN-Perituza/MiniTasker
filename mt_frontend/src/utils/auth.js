import { loginUser, createUser, loginMicrosoftUser } from "../api/api";
import { msalInstance, loginRequest } from "../authConfig"; 

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

export async function microsoftLogin() {
  try {
    const loginResponse = await msalInstance.loginPopup(loginRequest);
    const account = loginResponse.account;

    const tokenResponse = await msalInstance.acquireTokenSilent({
      ...loginRequest,
      account,
    });

    const accessToken = tokenResponse.accessToken;

    const user = await loginMicrosoftUser(accessToken); // This should return the user object now
    if (user && user.azureAdId) {
      localStorage.setItem("user", JSON.stringify(user));
      return true;
    }

    return false;
  } catch (error) {
    console.error("Microsoft login failed:", error);
    return false;
  }
}


// export async function microsoftLogin() {
//   try {
//     const loginResponse = await msalInstance.loginPopup(loginRequest);
//     const account = loginResponse.account;

//     const tokenResponse = await msalInstance.acquireTokenSilent({
//       ...loginRequest,
//       account,
//     });

//     const accessToken = tokenResponse.accessToken;

//     const user = await loginMicrosoftUser(accessToken);
//     if (user) {
//       localStorage.setItem("user", JSON.stringify(user));
//       return true;
//     }
//     return false;
//   } catch (error) {
//     console.error("Microsoft login failed:", error);
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

