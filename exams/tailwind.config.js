import { scopedPreflightStyles } from 'tailwindcss-scoped-preflight';

/** @type {import('tailwindcss').Config} */
export default {
  darkMode: ['class'],
  content: ['./index.html', './src/**/*.{js,ts,jsx,tsx}'],
  theme: {
    extend: {
      fontFamily: {
        'serif': ['Crimson Pro', 'Georgia', 'serif'],
        'sans': ['Nunito', 'system-ui', 'sans-serif'],
      },
      colors: {
        'primary': '#314CB6',
        'secondary': '#0A81D1',
        'accent': '#427AA1',
        'background': '#EBF2FA',
        'text': '#211A1E',
        'warning': '#F59E0B',
        'error': '#EF4444',
        'success': '#10B981',
        'info': '#3B82F6',
        'border': '#E5E7EB',
        'muted': '#6B7280',
      }
    }
  },
  plugins: [
    require('tailwindcss-animate'),
    require('tailwindcss-scoped-preflight').scopedPreflightStyles({
      cssSelector: '.chik-exams',
      mode: 'matched only',
    }),
  ],
};
