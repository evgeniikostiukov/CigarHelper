import { defineConfig, loadEnv } from 'vite';
import vue from '@vitejs/plugin-vue';
import { fileURLToPath, URL } from 'url';
import Components from 'unplugin-vue-components/vite';
import { PrimeVueResolver } from '@primevue/auto-import-resolver';
import VitePluginVueDevTools from 'vite-plugin-vue-devtools';

// https://vitejs.dev/config/
export default defineConfig(({ command, mode }) => {
  const rootDir = fileURLToPath(new URL('.', import.meta.url));
  const env = loadEnv(mode, rootDir, 'VITE_');
  const enableDevTools =
    command === 'serve' && (env.VITE_ENABLE_DEVTOOLS ?? '0') === '1';

  return {
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
      // Плагин `vite-plugin-vue-devtools` падал на Vite 8 с ошибкой
      // "Cannot read properties of undefined (reading 'rpc')", поэтому держим флажок.
      ...(enableDevTools ? [VitePluginVueDevTools()] : []),
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
  };
});
