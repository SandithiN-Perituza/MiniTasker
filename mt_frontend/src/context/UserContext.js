// import React, { createContext, useState, useEffect } from 'react';
// import { getCurrentUser } from '../utils/auth';

// export const UserContext = createContext();

// export const UserProvider = ({ children }) => {
//   const [currentUser, setCurrentUser] = useState(null);
//     const [refreshTrigger, setRefreshTrigger] = useState(false);

//   useEffect(() => {
//     const user = getCurrentUser();
//     setCurrentUser(user);
//   }, []);

//   return (
//     <UserContext.Provider value={{ currentUser, setCurrentUser, refreshTrigger, setRefreshTrigger }}>
//       {children}
//     </UserContext.Provider>
//   );
// };


import { createContext } from 'react';

const UserContext = createContext(null);

export default UserContext;



