import React, { useState, useEffect } from "react";
import { createTask, updateTask, fetchUsers } from "../api/api";

const statusOptions = [
  { value: 0, label: "Pending" },
  { value: 1, label: "InProgress" },
  { value: 2, label: "Complete" },
];

export default function TaskForm({ onSuccess, task }) {
  const [form, setForm] = useState({
    title: "",
    description: "",
    status: 0,
    assignedTo: "",
  });
  const [users, setUsers] = useState([]);

  useEffect(() => {
    fetchUsers().then(setUsers);
  }, []);

  useEffect(() => {
    if (task) {
      setForm({
        title: task.title,
        description: task.description,
        status: task.status,
        assignedTo: task.assignedTo || "",
      });
    }
  }, [task]);

  function handleChange(e) {
    const { name, value } = e.target;
    setForm((f) => ({ ...f, [name]: value }));
  }

  async function handleSubmit(e) {
    e.preventDefault();
    if (task) {
      await updateTask(task.id, { ...task, ...form, assignedTo: form.assignedTo || null });
    } else {
      await createTask({ ...form, assignedTo: form.assignedTo || null });
    }
    onSuccess();
    setForm({ title: "", description: "", status: 0, assignedTo: "" });
  }

  return (
    <form className="space-y-4 p-4 bg-white rounded shadow" onSubmit={handleSubmit}>
      <input
        className="border p-2 w-full"
        name="title"
        placeholder="Title"
        value={form.title}
        onChange={handleChange}
        required
      />
      <textarea
        className="border p-2 w-full"
        name="description"
        placeholder="Description"
        value={form.description}
        onChange={handleChange}
      />
      <select
        className="border p-2 w-full"
        name="status"
        value={form.status}
        onChange={handleChange}
      >
        {statusOptions.map((opt) => (
          <option key={opt.value} value={opt.value}>{opt.label}</option>
        ))}
      </select>
      <select
        className="border p-2 w-full"
        name="assignedTo"
        value={form.assignedTo}
        onChange={handleChange}
      >
        <option value="">Unassigned</option>
        {users.map((u) => (
          <option key={u.id} value={u.id}>{u.name}</option>
        ))}
      </select>
      <button className="bg-blue-500 text-white px-4 py-2 rounded" type="submit">
        {task ? "Update" : "Create"} Task
      </button>
    </form>
  );
}