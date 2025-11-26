import React, { useEffect, useState } from "react";
import { fetchComments, addComment, getAuthToken, unifiedMicrosoftLogin, fetchUsers } from "../api/api";
import { getCurrentUser } from "../utils/auth";

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
      const currentUser = getCurrentUser();
      const resolvedUserId = userId || currentUser?.id;

      // Ensure numeric userId to satisfy backend model binding
      let numericUserId = resolvedUserId != null ? Number(resolvedUserId) : NaN;

      // If we don't have a local numeric user id, try to derive one from tokens/back-end
      if (!numericUserId || isNaN(numericUserId) || numericUserId <= 0) {
        console.debug('No local numeric user id found, attempting to resolve via auth token');

        try {
          const token = await getAuthToken();
          if (token) {
            // Ask backend to upsert/return the user (handles Teams SSO path)
            try {
              await unifiedMicrosoftLogin(token);
            } catch (loginErr) {
              console.debug('unifiedMicrosoftLogin failed or returned no user:', loginErr?.message || loginErr);
            }

            // Try to read normalized user saved by api.js microsoft login helpers
            const maybeUser = getCurrentUser();
            numericUserId = maybeUser?.id ? Number(maybeUser.id) : numericUserId;

            // If still missing, parse token claims and map via fetchUsers
            if (!numericUserId || isNaN(numericUserId) || numericUserId <= 0) {
              try {
                const parts = token.split('.');
                if (parts.length > 1) {
                  const payload = parts[1];
                  const b64 = payload.replace(/-/g, '+').replace(/_/g, '/');
                  const claimJson = JSON.parse(atob(b64));
                  const oid = claimJson.oid || claimJson.sub;
                  const email = claimJson.preferred_username || claimJson.upn || claimJson.email;

                  try {
                    const users = await fetchUsers();
                    const found = (users || []).find(u => {
                      const azure = (u.azureAdId || u.oid || u.azureAdObjectId || u.id)?.toString();
                      const uemail = (u.email || u.mail || u.userPrincipalName || u.preferred_username || u.upn) || null;
                      if (oid && azure && azure === oid) return true;
                      if (email && uemail && uemail.toLowerCase() === (email || '').toLowerCase()) return true;
                      return false;
                    });

                    if (found) {
                      numericUserId = Number(found.id);
                      // Persist minimal normalized user so subsequent calls don't repeat work
                      try { localStorage.setItem('user', JSON.stringify({ id: found.id, azureAdId: found.azureAdId ?? found.oid ?? null, name: found.name ?? found.displayName ?? null, email: found.email ?? found.mail ?? null })); } catch {}
                    }
                  } catch (uErr) {
                    console.debug('fetchUsers mapping failed', uErr?.message || uErr);
                  }
                }
              } catch (parseErr) {
                console.debug('Token parse failed', parseErr?.message || parseErr);
              }
            }
          }
        } catch (err) {
          console.debug('getAuthToken failed or no token available', err?.message || err);
        }
      }

      if (!numericUserId || isNaN(numericUserId) || numericUserId <= 0) {
        console.error('No valid user id available for comment after resolution attempts; please sign in');
        alert('You must be signed in to post a comment.');
        return;
      }

      const commentData = {
        content: trimmedComment,
        userId: numericUserId,
      };

      const response = await addComment(taskId, commentData);
      // Backend may return { comment } or the comment directly
      const addedComment = response?.comment ?? response;

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
