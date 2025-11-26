import React, { useContext, useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { fetchTasks } from "../api/api";
import CommentSection from "../components/CommentSection";
import Subtasks from "../components/SubTasks";
import UserContext from "../context/UserContext";
import { PiWarningCircleBold } from "react-icons/pi";

export default function TaskDetail() {
  const { taskId } = useParams();
  const navigate = useNavigate();
  const { user } = useContext(UserContext);
  const [task, setTask] = useState(null);
  const [loading, setLoading] = useState(true);

  const statusLabels = {
    Pending: "Pending",
    InProgress: "In Progress",
    Complete: "Complete",
  };

  useEffect(() => {
    const loadTask = async () => {
      try {
        const tasks = await fetchTasks();
        const foundTask = tasks.find((t) => t.id === parseInt(taskId));
        if (foundTask) {
          setTask(foundTask);
        } else {
          alert("Task not found");
          navigate("/");
        }
      } catch (error) {
        console.error("Error loading task:", error);
        alert("Error loading task");
        navigate("/");
      } finally {
        setLoading(false);
      }
    };

    loadTask();
  }, [taskId, navigate]);

  if (loading) {
    return (
      <div className="max-w-2xl mx-auto mt-8">
        <p className="text-gray-500">Loading task...</p>
      </div>
    );
  }

  if (!task) {
    return (
      <div className="max-w-2xl mx-auto mt-8">
        <p className="text-gray-500">Task not found</p>
        <button
          className="mt-4 px-4 py-2 bg-blue-500 text-white rounded"
          onClick={() => navigate("/")}
        >
          Back to Tasks
        </button>
      </div>
    );
  }

  const isOverdue =
    task.dueDate &&
    new Date(task.dueDate) < new Date() &&
    task.status !== "Complete";

  return (
    <div className="max-w-2xl mx-auto mt-8">
      <button
        className="mb-4 px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
        onClick={() => navigate("/")}
      >
        ← Back to Tasks
      </button>

      <div className="bg-white p-6 rounded-lg shadow-lg">
        <div className="flex flex-row justify-between items-start mb-4">
          <div className="flex-1">
            <h1 className="text-2xl font-bold mb-4">{task.title}</h1>
            <p className="text-gray-700 mb-4">{task.description}</p>
            <div className="space-y-2 text-sm text-gray-600">
              <p>
                <span className="font-semibold">Status:</span>{" "}
                {statusLabels[task.status]}
              </p>
              <p>
                <span className="font-semibold">Assigned to:</span>{" "}
                {task.assignedUserName || "Unassigned"}
              </p>
              <p>
                <span className="font-semibold">Due Date:</span>{" "}
                {task.dueDate
                  ? new Date(task.dueDate).toLocaleDateString()
                  : "No due date"}
              </p>
            </div>
          </div>
          {isOverdue && (
            <div className="flex flex-col items-center text-red-600 font-semibold ml-4">
              <PiWarningCircleBold className="text-3xl" />
              <span className="text-sm mt-1">Overdue</span>
            </div>
          )}
        </div>

        {/* Subtasks Section */}
        <div className="mt-6 mb-3">
          {/* <h2 className="text-xl font-semibold mb-3">Subtasks</h2> */}
          <Subtasks taskId={task.id} />
        </div>

        {/* Comments Section */}
        <div className="mt-6 mb-3">
          {/* <h2 className="text-xl font-semibold mb-3">Comments</h2> */}
          <CommentSection taskId={task.id} userId={user?.id} />
        </div>
      </div>
    </div>
  );
}
