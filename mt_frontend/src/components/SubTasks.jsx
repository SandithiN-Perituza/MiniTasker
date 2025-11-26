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
  const [editingSubtask, setEditingSubtask] = useState(null);
  const [editTitle, setEditTitle] = useState("");
  const [deleteCandidate, setDeleteCandidate] = useState(null);

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

  const handleEdit = (subtask) => {
    setEditingSubtask(subtask);
    setEditTitle(subtask.title);
  };

  const confirmEdit = async () => {
    if (!editTitle.trim() || editTitle.trim() === editingSubtask.title) {
      setEditingSubtask(null);
      setEditTitle("");
      return;
    }
    try {
      const updated = await updateSubtask(taskId, editingSubtask.id, {
        title: editTitle.trim(),
      });
      setSubtasks((prev) =>
        prev.map((s) => (s.id === updated.subtask.id ? updated.subtask : s))
      );
      setEditingSubtask(null);
      setEditTitle("");
    } catch (error) {
      console.error("Failed to update subtask:", error);
      alert("Error updating subtask: " + error.message);
    }
  };

  const handleDelete = (subtask) => {
    setDeleteCandidate(subtask);
  };

  const confirmDelete = async () => {
    if (!deleteCandidate) return;
    try {
      await deleteSubtask(taskId, deleteCandidate.id);
      setSubtasks((prev) => prev.filter((s) => s.id !== deleteCandidate.id));
      setDeleteCandidate(null);
    } catch (error) {
      console.error("Failed to delete subtask:", error);
      alert("Error deleting subtask: " + error.message);
      setDeleteCandidate(null);
    }
  };

  return (
    <div className="mt-6">
      <h4 className="text-lg font-semibold mb-2">Subtasks</h4>

      {/* Edit Subtask Modal */}
      {editingSubtask && (
        <div
          className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50"
          onClick={() => {
            setEditingSubtask(null);
            setEditTitle("");
          }}
        >
          <div
            className="bg-white p-6 rounded shadow-lg w-full max-w-md"
            onClick={(e) => e.stopPropagation()}
          >
            <h3 className="text-lg font-semibold mb-4">Edit Subtask</h3>
            <input
              type="text"
              value={editTitle}
              onChange={(e) => setEditTitle(e.target.value)}
              className="w-full p-2 border rounded mb-4"
              placeholder="Subtask title"
              autoFocus
            />
            <div className="flex gap-4 justify-end">
              <button
                className="px-4 py-2 bg-gray-200 rounded"
                onClick={() => {
                  setEditingSubtask(null);
                  setEditTitle("");
                }}
              >
                Cancel
              </button>
              <button
                className="px-4 py-2 bg-blue-500 text-white rounded"
                onClick={confirmEdit}
              >
                Save
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Delete Confirmation Modal */}
      {deleteCandidate && (
        <div
          className="fixed inset-0 bg-black bg-opacity-50 flex justify-center items-center z-50"
          onClick={() => setDeleteCandidate(null)}
        >
          <div
            className="bg-white p-6 rounded shadow-lg w-full max-w-md"
            onClick={(e) => e.stopPropagation()}
          >
            <h3 className="text-lg font-semibold mb-4">Confirm Delete</h3>
            <p className="mb-4">
              Are you sure you want to delete the subtask "{deleteCandidate.title}"?
            </p>
            <div className="flex gap-4 justify-end">
              <button
                className="px-4 py-2 bg-gray-200 rounded"
                onClick={() => setDeleteCandidate(null)}
              >
                Cancel
              </button>
              <button
                className="px-4 py-2 bg-red-500 text-white rounded"
                onClick={confirmDelete}
              >
                Delete
              </button>
            </div>
          </div>
        </div>
      )}

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
              <button onClick={() => handleDelete(subtask)}>
                <FaRegTrashCan className="text-red-500 hover:text-red-700" />
              </button>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
}
