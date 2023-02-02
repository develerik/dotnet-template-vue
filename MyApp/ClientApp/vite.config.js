/* eslint-disable import/no-extraneous-dependencies */

import { defineConfig } from "vite";
import ViteVuePlugin from "@vitejs/plugin-vue";
import { ViteMinifyPlugin } from "vite-plugin-minify";

import cssnano from "cssnano";
import postcssImport from "postcss-import";
import postcssPresetEnv from "postcss-preset-env";

import { dependencies } from "./package.json";

export default defineConfig(({ mode }) => ({
  css: {
    postcss: {
      plugins: [
        postcssImport,
        postcssPresetEnv,
        ...(mode === "production" ? [cssnano] : []),
      ],
    },
  },
  build: {
    outDir: "./dist",
    emptyOutDir: true,
    rollupOptions: {
      output: {
        chunkFileNames: "js/[hash].min.js",
        entryFileNames: "js/[hash].min.js",
        manualChunks: Object
          .keys(dependencies)
          // workaround for dependencies whose name starts with '@'
          .filter(key => ![].includes(key))
          .reduce((chunks, name) => {
            // eslint-disable-next-line no-param-reassign
            chunks[name] = [name];
            return chunks;
          }, {}),
        assetFileNames: ({ name }) => {
          if (/\.(woff2?|ttf|eot)$/.test(name ?? "")) {
            return "fonts/[hash][extname]";
          }

          if (/\.(png|jpe?g|svg)$/.test(name ?? "")) {
            return "img/[hash].min[extname]";
          }

          if (/\.css$/.test(name ?? "")) {
            return "css/[hash].min[extname]";
          }

          return "assets/[hash][extname]";
        },
      },
    },
  },
  plugins: [
    ViteVuePlugin(),
    ViteMinifyPlugin({
      removeComments: true,
    }),
  ],
  server: {
    port: 5173,
  },
}));
