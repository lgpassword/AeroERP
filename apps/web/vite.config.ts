import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    rolldownOptions: {
      output: {
        manualChunks(id) {
          if (id.includes('node_modules/react') || id.includes('node_modules/react-dom') || id.includes('node_modules/react-router-dom')) {
            return 'react'
          }

          if (id.includes('node_modules/framer-motion')) {
            return 'motion'
          }

          if (id.includes('node_modules/lucide-react')) {
            return 'icons'
          }

          if (id.includes('@aeroerp/ui-kit') || id.includes('@aeroerp/ui-style') || id.includes('/packages/')) {
            return 'aero-ui'
          }
        },
      },
    },
  },
})
