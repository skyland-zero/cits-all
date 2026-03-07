import { defineConfig } from '@vben/vite-config';

export default defineConfig(async () => {
  return {
    application: {},
    vite: {
      server: {
        proxy: {
          '/api': {
            changeOrigin: true,
            rewrite: (path) => path.replace(/^\/api/, ''),
            // 本地后端代理目标地址
            target: 'http://localhost:5120',
            ws: true,
          },
        },
      },
    },
  };
});
