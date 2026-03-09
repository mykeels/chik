export default {
  plugins: {
    tailwindcss: {},
    autoprefixer: {},
    'postcss-transformselectors': {
      transform: (selector) => {
        const excluded = ['@keyframes', '@font-face', '%', 'from', 'to', ':root'];
        return excluded.some((s) => selector.includes(s))
          ? selector
          : selector
              .split(',')
              .map((s) => `.daily-bible-comics ${s}`)
              .join(',');
      },
    },
  },
};
