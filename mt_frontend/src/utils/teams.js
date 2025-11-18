export function isInTeams() {
  // Heuristic: iframe or user agent contains 'Teams'
  return (typeof window !== "undefined") &&
         ((window.parent && window.parent !== window) || /teams/i.test(navigator.userAgent));
}

// Extra detection for the native / desktop Teams (Electron) client
export function isTeamsDesktop() {
  if (typeof navigator === 'undefined') return false;
  const ua = navigator.userAgent || '';
  // Electron-based Teams often includes 'Electron' plus 'Teams'
  const isElectron = /electron/i.test(ua) || (window.process && window.process.versions && window.process.versions.electron);
  return isElectron && /teams/i.test(ua);
}

