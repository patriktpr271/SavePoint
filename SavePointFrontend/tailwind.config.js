import daisyui from "daisyui"

/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        'gaming-green': '#10B981',
        'gaming-orange': '#F97316',
        'gaming-cyan': '#0891B2',
      },
    },
  },
  plugins: [daisyui],
  daisyui: {
    themes: [
      {
        savepoint: {
          "primary": "#10B981",           // Gaming Green (Emerald-500)
          "primary-focus": "#047857",     // Darker green for hover
          "primary-content": "#ffffff",   // White text on primary
          
          "secondary": "#F97316",         // Vibrant Orange (Orange-500)
          "secondary-focus": "#EA580C",   // Darker orange for hover
          "secondary-content": "#ffffff", // White text on secondary
          
          "accent": "#0891B2",            // Dark Cyan (Cyan-600)
          "accent-focus": "#0E7490",      // Darker cyan for hover
          "accent-content": "#ffffff",    // White text on accent
          
          "neutral": "#374151",           // Dark Gray
          "neutral-focus": "#1F2937",     // Darker gray
          "neutral-content": "#ffffff",   // White text on neutral
          
          "base-100": "#1f2937",          // Dark gray background
          "base-200": "#111827",          // Darker gray
          "base-300": "#374151",          // Medium gray
          "base-content": "#f9fafb",      // Light text
          
          "info": "#3b82f6",              // Blue
          "success": "#10b981",           // Green (same as primary)
          "warning": "#f59e0b",           // Amber
          "error": "#ef4444",             // Red
        },
      },
      "light",
      "dark",
    ],
    darkTheme: "dark",
    base: true,
    styled: true,
    utils: true,
  },
}