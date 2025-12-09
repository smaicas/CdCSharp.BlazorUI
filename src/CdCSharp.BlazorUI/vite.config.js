import { defineConfig } from 'vite';
import path from 'path';
import { glob } from 'glob';

// Get all TypeScript entry files (capital letter = entry point)
const inputFiles = glob.sync('./Types/**/[A-Z]*.ts').reduce((entries, filePath) => {
    // Extract the module name from the path
    const parts = filePath.split('/');
    const fileName = parts[parts.length - 1].replace('.ts', '');
    const folderName = parts[parts.length - 2];
    
    // Use folder/file as entry name if in a subfolder, otherwise just file
    const entryName = parts.length > 3 ? `${folderName}/${fileName}` : fileName;
    entries[entryName] = filePath;
    return entries;
}, {});

console.log('Entry files found:', inputFiles);

export default defineConfig({
    resolve: {
        alias: {
            '@types': path.resolve(__dirname, 'Types')
        }
    },
    build: {
        outDir: path.resolve(__dirname, 'wwwroot/js'),
        emptyOutDir: false,
        rollupOptions: {
            input: inputFiles,
            output: {
                entryFileNames: (chunkInfo) => {
                    // Preserve folder structure in output
                    return chunkInfo.name.includes('/') 
                        ? '[name].min.js' 
                        : '[name].min.js';
                },
                format: 'es',
                manualChunks: undefined
            }
        },
        target: 'esnext',
        sourcemap: true,
        minify: 'terser',
        terserOptions: {
            compress: {
                drop_console: true,
                drop_debugger: true
            },
            format: {
                comments: false
            }
        }
    }
});