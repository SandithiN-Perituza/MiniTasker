import { loginUser, createUser } from "../api/api";

export async function login(email, password) {
  try {
    console.log("Logging in with:", { email, password });
    const user = await loginUser(email, password);
    if (user) {
      // Normalize common backend user shapes
      const isGuid = (val) => typeof val === 'string' && /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(val);
      const normalized = {
        id: user.id ?? user.userId ?? user.localId ?? 'unknown',
        azureAdId: user.azureAdId ?? user.oid ?? user.azureAdObjectId ?? (isGuid(user.id) ? user.id : null) ?? null,
        name: user.name ?? user.displayName ?? user.fullName ?? null,
        email: user.email ?? user.mail ?? user.userPrincipalName ?? user.preferred_username ?? user.upn ?? null,
      };
      localStorage.setItem("user", JSON.stringify(normalized));
      return true;
    }
    return false;
  } catch {
    return false;
  }
}

export async function signup(username, email, password) {
  try {
    await createUser(username, email, password);
    return true;
  } catch {
    return false;
  }
}

export function logout() {
  localStorage.removeItem("user");
}

export function getCurrentUser() {
  const stored = localStorage.getItem("user");
  if (!stored) return null;
  try {
    const parsed = JSON.parse(stored);
    return parsed;
  } catch {
    // If not JSON, could be a token string; derive basic user
    try {
      const token = stored;
      const parts = token.split('.');
      if (parts.length > 1) {
        const payload = parts[1];
        const b64 = payload.replace(/-/g, '+').replace(/_/g, '/');
        const json = atob(b64);
        const claims = JSON.parse(json);
        return {
          id: claims.oid || claims.sub || claims.preferred_username || claims.upn || claims.email || 'unknown',
          azureAdId: claims.oid || claims.sub || null,
          name: claims.name || [claims.given_name, claims.family_name].filter(Boolean).join(' ') || claims.preferred_username || claims.email || null,
          email: claims.preferred_username || claims.upn || claims.mail || claims.email || null,
        };
      }
    } catch (e) {
      console.debug('getCurrentUser token-parse failed', e?.message || e);
    }
    return null;
  }
}

