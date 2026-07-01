import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  // Proxy API calls to the ASP.NET Core back-end so the front-end only ever talks
  // HTTP (never the database) and we avoid CORS configuration during development.
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5179',
        changeOrigin: true,
      },
    },
  },
})
