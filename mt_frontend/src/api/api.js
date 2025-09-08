// const API_URL = "https://localhost:7296/api";
const API_URL = "https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api";

// Tasks
// Fetch all tasks
export async function fetchTasks() {
  const res = await fetch(`${API_URL}/tasks`);
  return res.json();
}

// Create a new task
export async function createTask(task) {
  const res = await fetch(`${API_URL}/tasks`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(task),
  });
  return res.json();
}

// Update an existing task
export async function updateTask(id, task) {
  await fetch(`${API_URL}/tasks/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(task),
  });
}

// Delete a task
export async function deleteTask(id) {
  const response = await fetch(`${API_URL}/tasks/${id}`, {
    method: "DELETE",
  });
  if (!response.ok) {
    throw new Error("Failed to delete task");
  }
  return response.json();
}

// Users
// Fetch all users
export async function fetchUsers() {
  const res = await fetch(`${API_URL}/users`);
  return res.json();
}

// Signup a new user
export async function createUser(name, email, password) {
  console.log("Creating user with:", { name, email, password });
  const res = await fetch(`${API_URL}/users`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ name, email, password }),
  });
  if (!res.ok) throw new Error("Signup failed");
  return res.json();
}

// Login
export async function loginUser(email, password) {
  console.log("Logging in with(inside api.js):", { email, password });
  console.log("API URL:", API_URL);
  const res = await fetch(`${API_URL}/users/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  });
  if (!res.ok) return null;
  return res.json();
}


// Microsoft Login

export async function loginMicrosoftUser(accessToken) {
  const res = await fetch(`${API_URL}/users/msal-login?saveUser=true`, {
    method: "POST",
    headers: {
      "Authorization": `Bearer ${accessToken}`,
    },
  });

  if (!res.ok) {
    console.error("Failed to login Microsoft user");
    return null;
  }

  try {
    return await res.json();
  } catch {
    const text = await res.text();
    return { message: text };
  }
}



// Comments
// Fetch comments for a task
export async function fetchComments(taskId) {
  const res = await fetch(`${API_URL}/tasks/${taskId}/comment`);
  return res.json();
}

// Add a comment to a task
export async function addComment(taskId, comment) {
  const res = await fetch(`${API_URL}/tasks/${taskId}/comment`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(comment),
  });
  return res.json();
}

// Subtasks
// Fetch subtasks for a task
export async function fetchSubtasks(taskId) {
  const res = await fetch(`${API_URL}/tasks/${taskId}/subtask`);
  return res.json();
}

// Add a subtask to a task
export async function addSubtask(taskId, subtask) {
  const res = await fetch(`${API_URL}/tasks/${taskId}/subtask`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(subtask),
  });
  return res.json();
}

// Complete a subtask
export async function completeSubtask(taskId, subtaskId, subtask) {
  const res = await fetch(`${API_URL}/tasks/${taskId}/subtask/${subtaskId}`, {
    method: "PATCH",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(subtask),
  });
  return res.json();
}

// Update a subtask
export async function updateSubtask(taskId, subtaskId, subtask) {
  const res = await fetch(`${API_URL}/tasks/${taskId}/subtask/${subtaskId}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(subtask),
  });
  return res.json();
}

// Delete a subtask
export async function deleteSubtask(taskId, subtaskId) {
  const res = await fetch(`${API_URL}/tasks/${taskId}/subtask/${subtaskId}`, {
    method: "DELETE",
  });
  if (!res.ok) {
    throw new Error("Failed to delete subtask");
  }
  return res.json();
}