// Minimal Graph notification (chat message) sender; adjust endpoint/scopes as needed.
// Prerequisites:
// 1. App registration with Chat.ReadWrite or ChannelMessage.Send (depending on target).
// 2. getAuthToken must return a token containing required scopes (consent granted).
// 3. Caller passes authToken from TeamsAuth.

export async function sendTaskNotification({ authToken, recipientAadId, task }) {
  if (!authToken) {
    throw new Error("Missing auth token; acquire via TeamsAuth before sending notification.");
  }
  // Example: send chat message to user (requires existing 1:1 chat or create one via Graph)
  // This is a placeholder illustrating pattern.
  const body = {
    body: {
      content: `You have been assigned task: ${task.title}\nDue: ${task.dueDate || "N/A"}`,
    },
  };

  // NOTE: You might need to resolve chatId first; omitted for brevity.
  const chatId = task.chatId; // ensure you supply this from backend or lookup.

  const res = await fetch(`https://graph.microsoft.com/v1.0/chats/${chatId}/messages`, {
    method: "POST",
    headers: {
      Authorization: `Bearer ${authToken}`,
      "Content-Type": "application/json",
    },
    body: JSON.stringify(body),
  });

  if (!res.ok) {
    const errText = await res.text();
    throw new Error(`Notification failed: ${res.status} ${errText}`);
  }
  return res.json();
}
