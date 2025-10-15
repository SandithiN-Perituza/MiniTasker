export function isInTeams() {
  // Heuristic: iframe or user agent contains 'Teams'
  return (typeof window !== "undefined") &&
         ((window.parent && window.parent !== window) || /teams/i.test(navigator.userAgent));
}
