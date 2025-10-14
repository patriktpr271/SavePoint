import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  base: '/', // Ensures assets are loaded from root when served from wwwroot
  build: {
    outDir: 'dist',
    assetsDir: 'assets',
    sourcemap: false, // Disable sourcemaps for production
    rollupOptions: {
      output: {
        manualChunks: undefined,
      },
    },
  },
  server: {
    proxy: {
      '/api': {
         target: 'http://localhost:5044',
        changeOrigin: true,
        secure: false,
      }
    }
  },
  plugins: [react(), tailwindcss()],
})
