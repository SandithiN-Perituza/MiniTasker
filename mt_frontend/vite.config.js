import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
// import fs from 'fs';

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  
  // server: {
  //   https: true,
  //   // https: {
  //   //   key: fs.readFileSync('./localhost-key.pem'),
  //   //   cert: fs.readFileSync('./localhost.pem'),
  //   // },
  //   // port: 3000,
  // },
})
