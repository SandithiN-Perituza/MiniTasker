// Dummy data for testing
let tasks = [
  {
    id: 1,
    title: "Design Homepage",
    description: "Create the homepage layout and style.",
    status: 0,
    assignedTo: 1,
  },
  {
    id: 2,
    title: "API Integration",
    description: "Integrate backend APIs for user authentication.",
    status: 1,
    assignedTo: 2,
  },
  {
    id: 3,
    title: "Write Documentation",
    description: "Document all components and API endpoints.",
    status: 2,
    assignedTo: null,
  },
];

let users = [
  { id: 1, name: "Alice Smith" },
  { id: 2, name: "Bob Johnson" },
  { id: 3, name: "Charlie Lee" },
];

export function fetchTasks() {
  return new Promise((resolve) => {
    setTimeout(() => resolve([...tasks]), 300);
  });
}

export function fetchUsers() {
  return new Promise((resolve) => {
    setTimeout(() => resolve([...users]), 300);
  });
}

export function createTask(task) {
  return new Promise((resolve) => {
    setTimeout(() => {
      const newTask = { ...task, id: tasks.length + 1 };
      tasks.push(newTask);
      resolve(newTask);
    }, 300);
  });
}

export function updateTask(id, updated) {
  return new Promise((resolve) => {
    setTimeout(() => {
      tasks = tasks.map((t) => (t.id === id ? { ...t, ...updated } : t));
      resolve(tasks.find((t) => t.id === id));
    }, 300);
  });
}
