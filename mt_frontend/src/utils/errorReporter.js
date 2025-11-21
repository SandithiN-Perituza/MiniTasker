// Simple error reporter that POSTs error DTOs to backend /api/errorlogs
const API_URL = process.env.REACT_APP_API_URL || "https://app-frontbackendtodoapp-test-ahepeja6fadmcuhb.eastus-01.azurewebsites.net/api";

export async function reportError({ message, stack, source = 'frontend' }) {
  try {
    const dto = {
      Message: message,
      StackTrace: stack || 'No stack trace',
      Source: source,
      Timestamp: new Date().toISOString()
    };

    await fetch(`${API_URL}/errorlogs`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(dto)
    });
  } catch (e) {
    // Swallow errors — don't break app when logging fails
    console.warn('Error reporter failed to send log', e);
  }
}

export default reportError;
