import React, { useEffect, useState } from "react";
import { fetchTasks } from "../api/api";
import TaskForm from "./TaskForm";

export default function TaskList() {
  const [tasks, setTasks] = useState([]);
  const [editing, setEditing] = useState(null);

  function loadTasks() {
    fetchTasks().then(setTasks);
  }

  useEffect(() => {
    loadTasks();
  }, []);

  return (
    <div className="max-w-2xl mx-auto mt-8">
      <TaskForm onSuccess={loadTasks} task={editing} />
      <h2 className="text-xl font-bold mt-8 mb-4">Tasks</h2>
      <ul className="space-y-2">
        {tasks.map((task) => (
          <li key={task.id} className="bg-gray-100 p-4 rounded flex justify-between items-center">
            <div>
              <div className="font-semibold">{task.title}</div>
              <div className="text-sm text-gray-600">{task.description}</div>
              <div className="text-xs text-gray-500">
                Status: {["Pending", "InProgress", "Complete"][task.status]} | 
                Assigned: {task.assignedUser ? task.assignedUser.name : "Unassigned"}
              </div>
            </div>
            <div>
              <button
                className="text-blue-500 mr-2"
                onClick={() => setEditing(task)}
              >
                Edit
              </button>
              {/* Delete not implemented in backend, so not shown */}
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}
