import React, { useEffect, useState } from "react";
import { fetchTasks, deleteTask } from "../api/api";
import TaskForm from "./TaskForm";
import { getCurrentUser } from "../utils/auth";

export default function TaskList() {
  const [tasks, setTasks] = useState([]);
  const [editing, setEditing] = useState(null);
  const [showModal, setShowModal] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [showMine, setShowMine] = useState(false);            // ← new state
  const [currentPage, setCurrentPage] = useState(1);
  const [currentUser, setCurrentUser] = useState(null);

  const tasksPerPage = 5;

  function loadTasks() {
    fetchTasks().then((data) => {
      console.log("Fetched tasks:", data);
      setTasks(data);
    });
  }

  const handleDelete = async (id) => {
    if (!currentUser) return;
    const task = tasks.find((t) => t.id === id);
    if (String(task?.assignedTo) !== String(currentUser.id)) {
      alert("You can only delete tasks assigned to you.");
      return;
    }

    if (window.confirm("Are you sure you want to delete this task?")) {
      try {
        await deleteTask(id);
        loadTasks();
      } catch (error) {
        alert("Error deleting task: " + error.message);
      }
    }
  };

  const handleEdit = (task) => {
    if (String(task?.assignedTo) !== String(currentUser.id)) {
      alert("You can only edit tasks assigned to you.");
      return;
    }

    setEditing(task);
    setShowModal(true);
  };

  const statusLabels = {
    Pending: "Pending",
    InProgress: "In Progress",
    Complete: "Complete",
  };

  const statusColors = {
    Pending: "bg-yellow-100",
    InProgress: "bg-blue-100",
    Complete: "bg-green-100",
  };

  // Apply search, status and “my tasks” filters
  const filteredTasks = tasks
    .filter((task) => {
      const matchesSearch = task.title
        .toLowerCase()
        .includes(searchTerm.toLowerCase());
      const matchesStatus = statusFilter ? task.status === statusFilter : true;
      const matchesMine = showMine
        ? String(task.assignedTo) === String(currentUser?.id)
        : true;
      return matchesSearch && matchesStatus && matchesMine;
    })
    .sort((a, b) => b.id - a.id);

  const totalPages = Math.ceil(filteredTasks.length / tasksPerPage);
  const paginatedTasks = filteredTasks.slice(
    (currentPage - 1) * tasksPerPage,
    currentPage * tasksPerPage
  );

  useEffect(() => {
    loadTasks();
    const user = getCurrentUser();
    setCurrentUser(user);
  }, []);

  return (
    <div className="max-w-2xl mx-auto mt-8">
      {currentUser && (
        <div className="flex gap-4">
          <button
            className="bg-blue-500 text-white px-4 py-2 rounded"
            onClick={() => {
              setEditing(null);
              setShowModal(true);
            }}
          >
            Add Task
          </button>

          {/* My Tasks toggle */}
          <button
            className={`px-4 py-2 rounded ${
              showMine ? "bg-green-500 text-white" : "bg-gray-200 text-gray-700"
            }`}
            onClick={() => {
              setShowMine((prev) => !prev);
              setCurrentPage(1);
            }}
          >
            {showMine ? "All Tasks" : "My Tasks"}
          </button>
        </div>
      )}

      {!currentUser && (
        <p className="text-gray-500 mt-4">Please log in to manage tasks.</p>
      )}

      {/* Modal for TaskForm */}
      {showModal && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50">
          <div className="bg-white p-6 rounded shadow-lg w-full max-w-md">
            <TaskForm
              onSuccess={() => {
                loadTasks();
                setShowModal(false);
                setEditing(null);
              }}
              task={editing}
            />
            <button
              className="mt-4 text-red-500"
              onClick={() => setShowModal(false)}
            >
              Close
            </button>
          </div>
        </div>
      )}

      {/* Search and Filter */}
      <div className="flex gap-4 mt-6 mb-4">
        <input
          type="text"
          placeholder="Search tasks..."
          className="border p-2 rounded w-full"
          value={searchTerm}
          onChange={(e) => {
            setSearchTerm(e.target.value);
            setCurrentPage(1);
          }}
        />
        <select
          className="border p-2 rounded"
          value={statusFilter}
          onChange={(e) => {
            setStatusFilter(e.target.value);
            setCurrentPage(1);
          }}
        >
          <option value="">All</option>
          <option value="Pending">Pending</option>
          <option value="InProgress">In Progress</option>
          <option value="Complete">Complete</option>
        </select>
      </div>

      <h2 className="text-xl font-bold mt-8 mb-4">Tasks</h2>
      <ul className="space-y-2">
        {paginatedTasks.map((task) => (
          <li
            key={task.id}
            className={`${statusColors[task.status]} p-4 rounded flex justify-between items-center`}
          >
            <div>
              <div className="font-semibold">{task.title}</div>
              <div className="text-sm text-gray-600">{task.description}</div>
              <div className="text-xs text-gray-500">
                Status: {statusLabels[task.status]} &nbsp;&nbsp;
                Assigned: {task.assignedUserName || "Unassigned"}
              </div>
            </div>

            <div>
              {currentUser &&
                String(task.assignedTo) === String(currentUser.id) && (
                  <>
                    <button
                      className="text-blue-500 mx-2"
                      onClick={() => handleEdit(task)}
                    >
                      Edit
                    </button>
                    &nbsp;<span className="text-gray-500">|</span>&nbsp;
                    <button
                      className="text-red-500 mx-2"
                      onClick={() => handleDelete(task.id)}
                    >
                      Delete
                    </button>
                  </>
                )}
            </div>
          </li>
        ))}
      </ul>

      {/* Pagination Controls */}
      {totalPages > 1 && (
        <div className="flex justify-center items-center mt-6 space-x-2">
          <button
            className="px-3 py-1 bg-gray-300 rounded disabled:opacity-50"
            onClick={() => setCurrentPage((p) => Math.max(p - 1, 1))}
            disabled={currentPage === 1}
          >
            Prev
          </button>
          <span className="px-3 py-1">
            {currentPage} / {totalPages}
          </span>
          <button
            className="px-3 py-1 bg-gray-300 rounded disabled:opacity-50"
            onClick={() => setCurrentPage((p) => Math.min(p + 1, totalPages))}
            disabled={currentPage === totalPages}
          >
            Next
          </button>
        </div>
      )}
    </div>
  );
}
