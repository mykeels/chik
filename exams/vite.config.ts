import fs from 'fs';
import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';
import federation from '@originjs/vite-plugin-federation';
import { VitePWA } from 'vite-plugin-pwa';
import rawPlugin from 'vite-raw-plugin';
import * as pkg from './package.json';
import path from 'path';

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd());
  return {
    base: process.env.BASE_URL || '/',
    build: {
      target: 'esnext',
      rollupOptions: {
        input: 'index.html',
        output: {
          format: 'esm',
        },
        external: [],
      },
    },
    plugins: [
      react(),
      // {
      //   name: 'emit-version.json-file',
      //   buildStart() {
      //     const outputFolders = [path.join(__dirname, './dist'), path.join(__dirname, './public')];
      //     for (const outputFolder of outputFolders) {
      //       fs.writeFileSync(
      //         path.join(outputFolder, 'version.json'),
      //         JSON.stringify({ version: pkg.version }, null, 2)
      //       );
      //     }
      //   },
      // },
      ...(process.env.STORYBOOK
        ? []
        : [
            VitePWA({
              registerType: 'autoUpdate',
              manifest: {
                name: 'Daily Bible Comics',
                short_name: 'Daily Bible Comics',
                description: 'Daily Bible Comics',
                theme_color: '#000000',
                background_color: '#000000',
                display: 'fullscreen',
                categories: ['bible', 'comics', 'devotional'],
                screenshots: [],
                icons: [
                  {
                    src: './logo/48x48.png',
                    sizes: '48x48',
                    type: 'image/png',
                  },
                  {
                    src: './logo/72x72.png',
                    sizes: '72x72',
                    type: 'image/png',
                  },
                  {
                    src: './logo/96x96.png',
                    sizes: '96x96',
                    type: 'image/png',
                  },
                  {
                    src: './logo/144x144.png',
                    sizes: '144x144',
                    type: 'image/png',
                  },
                  {
                    src: './logo/180x120.png',
                    sizes: '180x120',
                    type: 'image/png',
                  },
                  {
                    src: './logo/192x192.png',
                    sizes: '192x192',
                    type: 'image/png',
                  },
                  {
                    src: './logo/512x512.png',
                    sizes: '512x512',
                    type: 'image/png',
                  },
                  {
                    src: './logo/1024x500.png',
                    sizes: '1024x500',
                    type: 'image/png',
                  },
                  {
                    src: './logo/1024x1024.png',
                    sizes: '1024x1024',
                    type: 'image/png',
                  },
                  {
                    src: './logo/1080x1920.png',
                    sizes: '1080x1920',
                    type: 'image/png',
                  },
                  {
                    src: './logo/2560x1600.png',
                    sizes: '2560x1600',
                    type: 'image/png',
                  },
                ],
              },
              workbox: {
                navigateFallbackDenylist: [
                  /version\.json$/,
                  /robots\.txt$/,
                  /_headers$/,
                  /\/$/,
                ],
              },
            }),
          ]),
      rawPlugin({
        fileRegex: /\.md$/,
      }),
      federation({
        name: 'chik-exams',
        filename: 'remoteEntry.js',
        exposes: {},
        shared: ['react', 'react-dom', 'react-router'],
      }),
    ],
    resolve: {
      alias: {
        '@': '/src',
      },
    },
    define: {
      __APP_VERSION__: JSON.stringify(pkg.version),
      ...Object.entries(env).reduce((acc: Record<string, string>, [key, value]) => {
        acc[`process.env.${key}`] = JSON.stringify(value);
        return acc;
      }, {}),
    },
  };
});
