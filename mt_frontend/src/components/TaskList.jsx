import React, { useEffect, useState } from "react";
import { fetchTasks, deleteTask } from "../api/api";
import TaskForm from "./TaskForm";
import CommentSection from "./CommentSection";
import Subtasks from "./SubTasks";
import { getCurrentUser } from "../utils/auth";
import { PiWarningCircleBold } from "react-icons/pi";

export default function TaskList() {
  const [tasks, setTasks] = useState([]);
  const [editing, setEditing] = useState(null);
  const [viewingTask, setViewingTask] = useState(null);
  const [showModal, setShowModal] = useState(false);
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("");
  const [showMine, setShowMine] = useState(false);
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

          <button
            className={`px-4 py-2 rounded ${
              showMine ? "bg-green-500 text-white" : "bg-gray-200 text-gray-700"
            }`}
            onClick={() => {
              setShowMine((prev) => !prev);
              setCurrentPage(1);
              // loadTasks();
            }}
          >
            {showMine ? "All Tasks" : "My Tasks"}
          </button>
        </div>
      )}

      {!currentUser && (
        <p className="text-gray-500 mt-4">Please log in to manage tasks.</p>
      )}

      {/* Task Form Modal */}
      {showModal && (
        <div
          className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50"
          onClick={() => setShowModal(false)}
        >
          <div
            className="bg-white p-6 rounded shadow-lg w-full max-w-md"
            onClick={(e) => e.stopPropagation()}
          >
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

      {/* Task View Modal */}
      {viewingTask && (
        <div
          className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50"
          onClick={() => setViewingTask(null)}
        >
          <div
            className="bg-white p-6 rounded shadow-lg w-full max-w-md overflow-y-auto max-h-[90vh]"
            onClick={(e) => e.stopPropagation()}
          >
            <div className="flex flex-row justify-between items-center mb-4 mr-10">
              <div>
                <h2 className="text-xl font-bold mb-2">{viewingTask.title}</h2>
                <p className="text-gray-700 mb-2">{viewingTask.description}</p>
                <p className="text-sm text-gray-500 mb-2">
                  Status: {statusLabels[viewingTask.status]}
                  <br />
                  Assigned to: {viewingTask.assignedUserName || "Unassigned"}
                  <br />
                  Due Date:{" "}
                  {viewingTask.dueDate
                    ? new Date(viewingTask.dueDate).toLocaleDateString()
                    : "No due date"}
                </p>
              </div>
              {viewingTask.dueDate &&
                new Date(viewingTask.dueDate) < new Date() &&
                viewingTask.status !== "Complete" && (
                  <div className="flex flex-col items-center text-red-600 font-semibold">
                    <PiWarningCircleBold className="text-2xl" />
                    Overdue
                  </div>
              )}
            </div>

            {/* Subtasks */}
            <Subtasks taskId={viewingTask.id} />

            {/* Comments */}
            <CommentSection taskId={viewingTask.id} userId={currentUser?.id} />

            <button
              className="mt-4 text-red-500"
              onClick={() => setViewingTask(null)}
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
        {paginatedTasks.map((task) => {
          const isOverdue =
            task.dueDate &&
            new Date(task.dueDate) < new Date() &&
            task.status !== "Complete";

          const taskBgColor = isOverdue
            ? "bg-rose-200"
            : statusColors[task.status];
          return (
            <li
              key={task.id}
              className={`${taskBgColor} p-4 rounded flex justify-between items-center`}
            >
              <div>
                <div className="font-semibold">{task.title}</div>
                <div className="text-sm text-gray-600">{task.description}</div>
                <div className="text-xs text-gray-500">
                  Status: {statusLabels[task.status]} &nbsp;&nbsp; Assigned:{" "}
                  {task.assignedUserName || "Unassigned"}
                </div>
                <div className="text-xs text-gray-500">
                  Due Date:{" "}
                  {task.dueDate
                    ? new Date(task.dueDate).toLocaleDateString()
                    : "No due date"}
                  {isOverdue && (
                    <div className="flex items-center mt-1">
                      <PiWarningCircleBold className="inline-block mr-1 mt-0.5 text-red-600" />
                      <p className="text-red-600 text-sm font-semibold">
                        This task is overdue!
                      </p>
                    </div>
                  )}
                </div>
              </div>

              <div className="flex gap-2">
                <button
                  className="text-purple-500"
                  onClick={() => setViewingTask(task)}
                >
                  View
                </button>

                {currentUser &&
                  String(task.assignedTo) === String(currentUser.id) && (
                    <>
                      <span className="text-gray-500">|</span>
                      <button
                        className="text-blue-500"
                        onClick={() => handleEdit(task)}
                      >
                        Edit
                      </button>
                      <span className="text-gray-500">|</span>
                      <button
                        className="text-red-500"
                        onClick={() => handleDelete(task.id)}
                      >
                        Delete
                      </button>
                    </>
                  )}
              </div>
            </li>
          );
        })}
      </ul>

      {/* Pagination */}
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
