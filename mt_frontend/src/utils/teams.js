export function isInTeams() {
  if (typeof window === 'undefined') 
  {
    console.log("Window is undefined. Not in Teams.");
    return false;
  }

  // 1) Explicit SDK object present
  try {
    // Some hosts expose microsoftTeams on window or as global
    if (typeof window.microsoftTeams !== 'undefined') 
      {
        console.log("microsoftTeams SDK detected. Running inside Teams.");  
        return true;
      }
  } catch (e) {}

  // 2) Running inside iframe (Teams classic uses iframes for tabs)
  try {
    if (window.parent && window.parent !== window) 
      {
        console.log("Running inside an iframe. Likely inside Teams.");
        return true;
      }
  } catch (e) {}

  // 3) Referrer may include teams.microsoft.com in some webview flows
  try {
    if (document && document.referrer && /teams\.microsoft\.com/i.test(document.referrer)) 
      {
        console.log("Referrer indicates Teams webview.");
        return true;
      }
  } catch (e) {}

  // 4) User agent contains Teams or Electron (desktop client)
  try {
    const ua = navigator.userAgent || '';
    if (/\bteams\b/i.test(ua) || /\bElectron\b/i.test(ua)) 
      {
        console.log("User agent indicates Teams or Electron.");
        return true;
      }
  } catch (e) {}

  return false;
}

// Extra detection for the native / desktop Teams (Electron) client
export function isTeamsDesktop() {
  if (typeof navigator === 'undefined') 
    {
      console.log("Navigator is undefined. Not Teams Desktop.");
      return false;
    }
  const ua = navigator.userAgent || '';
  // Electron-based Teams often includes 'Electron' plus 'Teams'
  const isElectron = /electron/i.test(ua) || (window.process && window.process.versions && window.process.versions.electron);
  return isElectron && /teams/i.test(ua);
}

