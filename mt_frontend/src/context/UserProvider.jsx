import React, { useState } from 'react';
import UserContext from './UserContext';

export const UserProvider = ({ children }) => {
  const [currentUser, setCurrentUser] = useState(null);
  const [refreshTrigger, setRefreshTrigger] = useState(false);

  return (
    <UserContext.Provider value={{ currentUser, setCurrentUser, refreshTrigger, setRefreshTrigger }}>
      {children}
    </UserContext.Provider>
  );
};
