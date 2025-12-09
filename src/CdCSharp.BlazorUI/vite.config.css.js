import { defineConfig } from 'vite';
import path from 'path';

export default defineConfig({
    build: {
        minify: true,
        outDir: 'wwwroot/css',
        emptyOutDir: false,
        rollupOptions: {
            input: './CssBundle/main.css',
            output: {
                dir: path.resolve(__dirname, 'wwwroot/css'),
                entryFileNames: 'main.css',  // Sin hash
                assetFileNames: '[name][extname]'
            }
        }
    }
});