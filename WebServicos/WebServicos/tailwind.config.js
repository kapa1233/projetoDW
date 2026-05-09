/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./Pages/**/*.cshtml",
    "./Views/**/*.cshtml",
    "./wwwroot/js/**/*.js"
  ],
  corePlugins: {
    preflight: false,
  },
  theme: {
    extend: {
      colors: {
        "ws-bg": "#070b14",
        "ws-surface": "#111827",
        "ws-card": "rgba(255,255,255,0.04)",
        "ws-border": "rgba(255,255,255,0.08)",
        "ws-blue": "#3b82f6",
        "ws-purple": "#8b5cf6",
        "ws-cyan": "#06b6d4",
        "ws-green": "#10b981",
        "ws-text": "#f1f5f9",
        "ws-muted": "#64748b"
      },
      backdropBlur: {
        xs: "2px"
      }
    }
  },
  plugins: []
}
