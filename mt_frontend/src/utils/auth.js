// Dummy auth functions for demonstration. Replace with real API calls.

export function login(username, password) {
  // Simulate login: accept any non-empty username/password
  return new Promise((resolve) => {
    setTimeout(() => {
      if (username && password) {
        localStorage.setItem("user", username);
        resolve(true);
      } else {
        resolve(false);
      }
    }, 500);
  });
}

export function signup(username, password) {
  // Simulate signup: always succeed if fields are filled
  return new Promise((resolve) => {
    setTimeout(() => {
      if (username && password) {
        resolve(true);
      } else {
        resolve(false);
      }
    }, 500);
  });
}

export function logout() {
  localStorage.removeItem("user");
}
