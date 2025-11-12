import * as microsoftTeams from "@microsoft/teams-js";

// Check if running in Teams environment
export const isRunningInTeams = () => {
  // Check for Teams-specific indicators
  return (
    window.parent !== window.self || // Running in iframe
    window.location.ancestorOrigins?.length > 0 || // Has parent origins
    window.navigator.userAgent.includes('Teams') || // Teams user agent
    new URLSearchParams(window.location.search).has('theme') // Teams theme parameter
  );
};

export const initializeTeams = async () => {
  try {
    // Only initialize if we're actually running in Teams
    if (!isRunningInTeams()) {
      console.log("Not running in Teams environment - skipping Teams SDK initialization");
      return false;
    }

    await microsoftTeams.app.initialize();
    console.log("Teams SDK initialized successfully");
    return true;
  } catch (error) {
    console.error("Failed to initialize Teams SDK:", error);
    return false;
  }
};

export const isInTeams = async () => {
  try {
    if (!isRunningInTeams()) {
      return false;
    }
    
    return microsoftTeams.app.isInitialized();
  } catch (error) {
    console.error("Error checking Teams status:", error);
    return false;
  }
};

export const getTeamsContext = async () => {
  try {
    if (!microsoftTeams.app.isInitialized()) {
      return null;
    }
    
    const context = await microsoftTeams.app.getContext();
    return context;
  } catch (error) {
    console.error("Failed to get Teams context:", error);
    return null;
  }
};

export const silentTeamsLogin = async () => {
  try {
    if (!microsoftTeams.app.isInitialized()) {
      return null;
    }

    const context = await getTeamsContext();
    if (!context?.user?.id) {
      throw new Error("No user context available");
    }

    return new Promise((resolve, reject) => {
      const authTokenRequest = {
        successCallback: async (token) => {
          try {
            // For now, return basic user info from context
            // In production, you'd exchange this token with your backend
            const userData = {
              id: context.user.id,
              name: context.user.displayName,
              email: context.user.userPrincipalName,
              token: token
            };
            resolve(userData);
          } catch (error) {
            console.error("Token processing failed:", error);
            reject(error);
          }
        },
        failureCallback: (error) => {
          console.error("Teams SSO failed:", error);
          reject(error);
        }
      };

      microsoftTeams.authentication.getAuthToken(authTokenRequest);
    });
  } catch (error) {
    console.error("Silent Teams login failed:", error);
    return null;
  }
};
