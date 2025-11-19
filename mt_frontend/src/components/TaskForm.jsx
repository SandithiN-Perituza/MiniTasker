import React, { useState, useEffect } from "react";
import { createTask, createTaskWithNotification, updateTask, fetchUsers, getUserAzureAdId } from "../api/api";
import { PiWarningCircleBold } from 'react-icons/pi';

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
    dueDate: "",
  });
  const [users, setUsers] = useState([]);
  const [errors, setErrors] = useState({});

  useEffect(() => {
    fetchUsers().then((fetchedUsers) => {
      console.log("📋 Fetched users:", fetchedUsers);
      setUsers(fetchedUsers);
    });
  }, []);

  useEffect(() => {
    if (task) {
      const statusMap = {
        Pending: 0,
        InProgress: 1,
        Complete: 2,
      };

      setForm({
        title: task.title,
        description: task.description,
        status: statusMap[task.status] ?? 0,
        assignedTo: task.assignedTo || "",
        dueDate: task.dueDate ? task.dueDate.split("T")[0] : "",
      });
    }
  }, [task]);

  function handleChange(e) {
    const { name, value } = e.target;
    setForm((f) => ({ 
      ...f, 
      [name]: name === "status" ? Number(value) : value,
    }));
    // clear error on user input
    setErrors((errs) => ({ ...errs, [name]: undefined }));
  }

  function validate() {
    const errs = {};
    if (!form.title.trim()) {
      errs.title = "Title is required.";
    }
    if (!form.description.trim()) {
      errs.description = "Description is required.";
    }
    if (!form.assignedTo) {
      errs.assignedTo = "Please assign to a user.";
    }
    if (!form.dueDate) {
      errs.dueDate = "Due date is required.";
    }
    return errs;
  }

  async function handleSubmit(e) {
    e.preventDefault();
    const validationErrors = validate();
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }
    
    const payload = {
      ...form,
      assignedTo: form.assignedTo || null,
    };

    try {
      if (task) {
        // For updates, use the original updateTask function
        await updateTask(task.id, { ...task, ...payload });
      } else {
        const assignedUserId = parseInt(form.assignedTo);
        
        console.log("🔍 Getting Azure AD ID for assigned user:", assignedUserId);
        
        try {
          // Call the new backend API to get Azure AD ID
          const userDetails = await getUserAzureAdId(assignedUserId);
          
          console.log("✅ Got user details from backend:", userDetails);
          
          if (userDetails.hasAzureAdId && userDetails.azureAdId) {
            try {
              await createTaskWithNotification(payload, userDetails.azureAdId);
            } catch (notifyErr) {
              console.warn("Notification path failed, fallback to plain create:", notifyErr.message);
              await createTask(payload);
            }
          } else {
            console.warn("⚠️ Assigned user has no Azure AD ID, creating task without notification");
            alert(`Warning: ${userDetails.name} hasn't logged in via Microsoft yet, so they won't receive a Teams notification.`);
            await createTask(payload);
          }
        } catch (azureIdError) {
          console.error("❌ Failed to get Azure AD ID:", azureIdError);
          console.warn("⚠️ Falling back to regular task creation");
          alert("Could not retrieve user information for notifications. Creating task without notification.");
          await createTask(payload);
        }
      }

      onSuccess();
      
      // reset form
      setForm({
        title: "",
        description: "",
        status: 0,
        assignedTo: "",
        dueDate: "",
      });
      setErrors({});
      
    } catch (error) {
      console.error("❌ Failed to create/update task:", error);
      setErrors({ submit: error.message });
    }
  }

  const isOverdue = form.dueDate && new Date(form.dueDate) < new Date() && form.status !== 2;

  return (
    <form className="space-y-4 p-4 bg-white rounded shadow" onSubmit={handleSubmit} noValidate>
      {errors.submit && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
          {errors.submit}
        </div>
      )}
      
      <div>
        <label className="block mb-1 font-medium" htmlFor="title">
          Title
        </label>
        <input
          id="title"
          name="title"
          type="text"
          placeholder="Enter task title"
          className={`border p-2 w-full ${
            errors.title ? "border-red-500" : ""
          }`}
          value={form.title}
          onChange={handleChange}
        />
        {errors.title && (
          <p className="text-red-500 text-sm mt-1">{errors.title}</p>
        )}
      </div>
      
      <div>
        <label className="block mb-1 font-medium" htmlFor="description">
          Description
        </label>
        <textarea
          id="description"
          name="description"
          placeholder="Enter task description"
          className={`border p-2 w-full ${
            errors.description ? "border-red-500" : ""
          }`}
          value={form.description}
          onChange={handleChange}
        />
        {errors.description && (
          <p className="text-red-500 text-sm mt-1">
            {errors.description}
          </p>
        )}
      </div>
      
      <div>
        <label className="block mb-1 font-medium" htmlFor="status">
          Status
        </label>
        <select
          id="status"
          name="status"
          className="border p-2 w-full"
          value={form.status}
          onChange={handleChange}
        >
          {statusOptions.map((opt) => (
            <option key={opt.value} value={opt.value}>
              {opt.label}
            </option>
          ))}
        </select>
      </div>
      
      <div>
        <label className="block mb-1 font-medium" htmlFor="assignedTo">
          Assign To
        </label>
        <select
          id="assignedTo"
          name="assignedTo"
          className={`border p-2 w-full ${
            errors.assignedTo ? "border-red-500" : ""
          }`}
          value={form.assignedTo}
          onChange={handleChange}
        >
          <option value="">Unassigned</option>
          {users.map((u) => (
            <option key={u.id} value={u.id}>
              {u.name}
            </option>
          ))}
        </select>
        {errors.assignedTo && (
          <p className="text-red-500 text-sm mt-1">
            {errors.assignedTo}
          </p>
        )}
      </div>
      
      <div>
        <label className="block mb-1 font-medium" htmlFor="dueDate">
          Due Date
        </label>
        <input
          id="dueDate"
          name="dueDate"
          type="date"
          className={`border p-2 w-full ${
            errors.dueDate ? "border-red-500" : ""
          }`}
          value={form.dueDate}
          onChange={handleChange}
        />
        {errors.dueDate && (
          <p className="text-red-500 text-sm mt-1">{errors.dueDate}</p>
        )}
        {isOverdue && (
          <div className="flex items-center">
            <PiWarningCircleBold className="inline-block mr-1 mt-1.5 text-red-600" />
            <p className="text-red-600 text-sm mt-1 font-semibold">
              This task is overdue!
            </p>
          </div>
        )}
      </div>

      <button className="bg-blue-500 text-white px-4 py-2 rounded" type="submit">
        {task ? "Update" : "Create"} Task
      </button>
    </form>
  );
}