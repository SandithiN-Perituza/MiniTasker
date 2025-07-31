import React, { useEffect, useState } from "react";
import { fetchTasks, fetchUsers } from "../api/api";

const statusLabels = ["Pending", "InProgress", "Complete"];

export default function Dashboard() {
  const [tasks, setTasks] = useState([]);
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadData() {
      setLoading(true);
      const [tasksData, usersData] = await Promise.all([fetchTasks(), fetchUsers()]);
      setTasks(tasksData);
      setUsers(usersData);
      setLoading(false);
    }
    loadData();
  }, []);

  function getUserName(userId) {
    const user = users.find((u) => u.id === userId);
    return user ? user.name : "Unassigned";
  }

  if (loading) {
    return <div className="p-8 text-center">Loading...</div>;
  }

  return (
    <div className="p-8">
      <h2 className="text-xl font-semibold mb-4">All Tasks</h2>
      <div className="overflow-x-auto">
        <table className="min-w-full bg-white rounded shadow">
          <thead>
            <tr>
              <th className="px-4 py-2 border">Title</th>
              <th className="px-4 py-2 border">Description</th>
              <th className="px-4 py-2 border">Status</th>
              <th className="px-4 py-2 border">Assigned To</th>
            </tr>
          </thead>
          <tbody>
            {tasks.length === 0 && (
              <tr>
                <td colSpan={4} className="text-center p-4">
                  No tasks found.
                </td>
              </tr>
            )}
            {tasks.map((task) => (
              <tr key={task.id}>
                <td className="px-4 py-2 border">{task.title}</td>
                <td className="px-4 py-2 border">{task.description}</td>
                <td className="px-4 py-2 border">{statusLabels[task.status] || "Unknown"}</td>
                <td className="px-4 py-2 border">{getUserName(task.assignedTo)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
