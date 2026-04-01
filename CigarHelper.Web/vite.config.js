import { defineConfig, loadEnv } from 'vite';
import vue from '@vitejs/plugin-vue';
import { fileURLToPath, URL } from 'url';
import Components from 'unplugin-vue-components/vite';
import { PrimeVueResolver } from '@primevue/auto-import-resolver';
import VitePluginVueDevTools from 'vite-plugin-vue-devtools';
import { VitePWA } from 'vite-plugin-pwa';

// https://vitejs.dev/config/
export default defineConfig(({ command, mode }) => {
  const rootDir = fileURLToPath(new URL('.', import.meta.url));
  const env = loadEnv(mode, rootDir, 'VITE_');
  // Важно: `vite-plugin-vue-devtools` может падать в рантайме (клиент) с
  // "TypeError: Cannot read properties of undefined (reading 'rpc')".
  // Поэтому по умолчанию держим выключенным и включаем только явным флагом.
  const enableDevTools = (env.VITE_ENABLE_DEVTOOLS ?? '0') === '1' && (command === 'serve' || command === 'dev');
  const isDevCommand = command === 'serve' || command === 'dev';

  return {
    define: {
      // Некоторые devtools-клиенты/инжекты ожидают этот флаг в рантайме.
      // В противном случае в браузере может упасть: "__BUNDLED_DEV__ is not defined".
      __BUNDLED_DEV__: JSON.stringify(isDevCommand),
      // Аналогично: встречается в dev-инжектах, иначе падает
      // "__SERVER_FORWARD_CONSOLE__ is not defined".
      __SERVER_FORWARD_CONSOLE__: JSON.stringify(isDevCommand),
    },
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
      VitePWA({
        strategies: 'injectManifest',
        srcDir: 'src',
        filename: 'sw.ts',
        registerType: 'prompt',
        includeAssets: ['favicon.ico', 'logo.svg', 'apple-touch-icon-180x180.png'],
        manifest: {
          name: 'Cigar Helper',
          short_name: 'CigarHelper',
          description: 'Учёт сигар, хьюмидоров и отзывов',
          theme_color: '#292524',
          background_color: '#fafaf9',
          display: 'standalone',
          scope: '/',
          start_url: '/',
          lang: 'ru',
          icons: [
            { src: 'pwa-64x64.png', sizes: '64x64', type: 'image/png' },
            { src: 'pwa-192x192.png', sizes: '192x192', type: 'image/png' },
            { src: 'pwa-512x512.png', sizes: '512x512', type: 'image/png' },
            { src: 'maskable-icon-512x512.png', sizes: '512x512', type: 'image/png', purpose: 'maskable' },
          ],
        },
        injectManifest: {
          globPatterns: ['**/*.{js,css,html,woff2,png,svg,ico}'],
        },
        devOptions: {
          enabled: false,
        },
      }),
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
    /** Для E2E PWA (`npm run build && npm run preview`) — тот же прокси `/api`, что и в dev. */
    preview: {
      host: true,
      port: 4173,
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
