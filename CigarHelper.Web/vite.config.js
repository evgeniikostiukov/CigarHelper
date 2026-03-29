import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import { fileURLToPath, URL } from 'url';
import Components from 'unplugin-vue-components/vite';
import { PrimeVueResolver } from '@primevue/auto-import-resolver';
import VitePluginVueDevTools from 'vite-plugin-vue-devtools';

// https://vitejs.dev/config/
export default defineConfig(({ command }) => ({
  plugins: [
    vue({
      script: {
        // Включаем опцию defineModel для Vue 3
        defineModel: true,
        // Добавляем опцию для обработки JSX
        propsDestructure: true,
        compilerOptions: {
          isCustomElement: (tag) => tag.includes('-'),
        },
      },
    }),
    Components({
      resolvers: [PrimeVueResolver()],
      dts: true,
    }),
    // Только dev (`vite` / `vite serve`): Vue DevTools не нужны в production-сборке.
    ...(command === 'serve' ? [VitePluginVueDevTools()] : []),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    host: true,
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5184',
        changeOrigin: true,
        secure: false,
      },
    },
  },
  build: {
    rollupOptions: {
      output: {
        manualChunks(id) {
          if (id.includes('node_modules/@tiptap/')) {
            return 'tiptap';
          }
        },
      },
    },
  },
}));
