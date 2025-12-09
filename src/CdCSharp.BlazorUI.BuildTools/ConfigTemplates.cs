namespace CdCSharp.BlazorUI.BuildTools;

public static class ConfigTemplates
{
    public static string GetPackageJson(string projectName = "cdcsharp-blazorui")
    {
        return $$"""
{
  "name": "{{projectName}}",
  "version": "1.0.0",
  "description": "BlazorUI Component Library",
  "type": "module",
  "main": "wwwroot/js/main.js",
  "scripts": {
    "build": "blazorui-build-tool all .",
    "build:themes": "blazorui-build-tool themes ./CssBundle/themes.css",
    "build:css": "blazorui-build-tool css .",
    "build:js": "blazorui-build-tool js .",
    "dev": "vite",
    "preview": "vite preview"
  },
  "author": "Samuel Maícas (CdCSharp)",
  "license": "ISC",
  "devDependencies": {
    "eslint": "latest",
    "eslint-plugin-es6": "latest",
    "typescript": "latest",
    "vite": "latest",
    "terser": "latest",
    "glob": "latest"
  }
}
""";
    }

    public static string GetTsConfig() => """
{
  "compilerOptions": {
    "baseUrl": "./",
    "module": "ES2022",
    "target": "ES2022",
    "outDir": "./wwwroot/js",
    "moduleResolution": "node",
    "sourceMap": true,
    "strict": true,
    "esModuleInterop": true,
    "skipLibCheck": true,
    "forceConsistentCasingInFileNames": true,
    "typeRoots": [
      "node_modules/@types",
      "./@types"
    ],
    "paths": {
      "@types/*": ["./Types/*"]
    }
  },
  "exclude": [
    "node_modules",
    "wwwroot"
  ],
  "include": [
    "./Types/**/*.ts"
  ]
}
""";

    public static string GetViteConfigJs() => """
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
""";

    public static string GetViteConfigCss() => """
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
""";

    public static string GetMainCss() => """
/* Import generated theme CSS */
@import './themes.css';

/* Additional styles can be added here */

""";
}