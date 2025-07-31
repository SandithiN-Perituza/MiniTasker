const API_URL = "http://localhost:7296/api";

export async function fetchTasks() {
  const res = await fetch(`${API_URL}/Tasks`);
  return res.json();
}

export async function createTask(task) {
  const res = await fetch(`${API_URL}/Tasks`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(task),
  });
  return res.json();
}

export async function updateTask(id, task) {
  await fetch(`${API_URL}/Tasks/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(task),
  });
}

export async function fetchUsers() {
  const res = await fetch(`${API_URL}/Users`);
  return res.json();
}
