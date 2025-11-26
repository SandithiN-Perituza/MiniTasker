import React, { useEffect, useState } from "react";
import {
  fetchSubtasks,
  addSubtask,
  completeSubtask,
  updateSubtask,
  deleteSubtask,
} from "../api/api";
import { FaCheckCircle, FaRegCircle, FaRegEdit, FaTrash } from "react-icons/fa";
// import { MdDone, MdDoneAll } from "react-icons/md";
import { FaRegTrashCan } from "react-icons/fa6";

export default function Subtasks({ taskId }) {
  const [subtasks, setSubtasks] = useState([]);
  const [newSubtask, setNewSubtask] = useState("");

  useEffect(() => {
    fetchSubtasks(taskId).then(setSubtasks);
  }, [taskId]);

  const handleAdd = async () => {
    if (!newSubtask.trim()) return;

    const subtaskData = {
      title: newSubtask,
      isCompleted: false,
    };

    try {
      const addedResp = await addSubtask(taskId, subtaskData);
      // Backend may return the subtask directly or wrap it in { subtask }
      const added = addedResp?.subtask ?? addedResp;
      if (added && added.id) {
        setSubtasks((prev) => [...prev, added]);
      } else {
        console.warn('addSubtask returned unexpected shape', addedResp);
        // Attempt to refresh list
        const refreshed = await fetchSubtasks(taskId);
        setSubtasks(refreshed);
      }
    } catch (error) {
      console.error('Failed to add subtask:', error);
    }
    setNewSubtask("");
  };

  const handleToggle = async (subtask) => {
    try {
      await completeSubtask(taskId, subtask.id, {
        ...subtask,
        isCompleted: !subtask.isCompleted,
      });
      const refreshed = await fetchSubtasks(taskId);
      setSubtasks(refreshed);
    } catch (error) {
      console.error("Failed to toggle subtask:", error);
    }
  };

  const handleEdit = async (subtask) => {
    const newTitle = prompt("Edit subtask title:", subtask.title);
    if (newTitle && newTitle.trim() !== subtask.title) {
      try {
        const updated = await updateSubtask(taskId, subtask.id, {
          title: newTitle.trim(),
        });
        setSubtasks((prev) =>
          prev.map((s) => (s.id === updated.subtask.id ? updated.subtask : s))
        );
      } catch (error) {
        console.error("Failed to update subtask:", error);
      }
    }
  };

  const handleDelete = async (subtaskId) => {
    if (!window.confirm("Are you sure you want to delete this subtask?")) return;

    try {
      await deleteSubtask(taskId, subtaskId);
      setSubtasks((prev) => prev.filter((s) => s.id !== subtaskId));
    } catch (error) {
      console.error("Failed to delete subtask:", error);
    }
  };

  return (
    <div className="mt-6">
      <h4 className="text-lg font-semibold mb-2">Subtasks</h4>

      <div className="flex gap-2 mb-4">
        <input
          type="text"
          value={newSubtask}
          onChange={(e) => setNewSubtask(e.target.value)}
          placeholder="New subtask"
          className="flex-1 p-2 border rounded"
        />
        <button
          onClick={handleAdd}
          className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700"
        >
          Add
        </button>
      </div>

      <ul className="space-y-2">
        {subtasks.map((subtask) => (
          <li
            key={subtask.id}
            className="flex items-center justify-between bg-gray-100 p-2 rounded"
          >
            <div className="flex items-center gap-2">
              <button onClick={() => handleToggle(subtask)}>
                {subtask.isCompleted ? (
                  <FaCheckCircle className="text-green-600" />
                ) : (
                  <FaRegCircle className="text-gray-400" />
                )}
              </button>
              <span
                className={`${
                  subtask.isCompleted ? "line-through text-gray-500" : ""
                }`}
              >
                {subtask.title}
              </span>
            </div>
            <div className="flex gap-3">
              <button onClick={() => handleEdit(subtask)}>
                <FaRegEdit className="text-blue-500 hover:text-blue-700" />
              </button>
              <button onClick={() => handleDelete(subtask.id)}>
                <FaRegTrashCan className="text-red-500 hover:text-red-700" />
              </button>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}
