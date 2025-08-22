import React, { useEffect, useState } from "react";
import { fetchComments, addComment } from "../api/api";

export default function CommentSection({ taskId, userId }) {
  const [comments, setComments] = useState([]);
  const [newComment, setNewComment] = useState("");

  useEffect(() => {
    fetchComments(taskId).then(setComments);
  }, [taskId]);

  const handleSubmit = async (e) => {
    e.preventDefault();

    const trimmedComment = newComment.trim();
    if (!trimmedComment) return;

    try {
      const commentData = {
        content: trimmedComment,
        userId: userId,
      };

      const response = await addComment(taskId, commentData);
      const addedComment = response.comment;

      if (addedComment?.id) {
        setComments((prevComments) => [addedComment, ...prevComments]);
      } else {
        console.warn("Comment added but missing ID:", response);
      }

      setNewComment("");
    } catch (error) {
      console.error("Failed to add comment:", error);
    }
  };

  return (
    <div className="mt-6">
      <h4 className="text-lg font-semibold mb-2">Comments</h4>
      <form onSubmit={handleSubmit} className="space-y-2">
        <textarea
          value={newComment}
          onChange={(e) => setNewComment(e.target.value)}
          placeholder="Add a comment..."
          required
          className="w-full p-3 border rounded-md resize-none focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
        <button
          type="submit"
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition"
        >
          Post Comment
        </button>
      </form>

      <ul className="mt-4 space-y-4">
        {comments.map((comment) => (
          <li key={comment.id} className="border-b pb-3">
            <div className="text-sm text-gray-600">
              <strong className="text-blue-700">{comment.userName}</strong> ·{" "}
              <em>{new Date(comment.createdAt).toLocaleString()}</em>
            </div>
            <p className="mt-1 text-gray-800">{comment.content}</p>
          </li>
        ))}
      </ul>
    </div>
  );
}
