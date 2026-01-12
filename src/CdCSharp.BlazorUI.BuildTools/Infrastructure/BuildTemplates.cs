using CdCSharp.BuildTools.Attributes;
using System.Diagnostics.CodeAnalysis;

namespace CdCSharp.BlazorUI.BuildTools.Infrastructure;

[ExcludeFromCodeCoverage]
public class BuildTemplates
{
    [BuildTemplate(".npmrc")]
    public static string GetNpmRcTemplate() => """
        fund=false
        audit=false  
        """;

    [BuildTemplate("CssBundle/entry.js")]
    public static string GetEntryJsTemplate() => """import "./main.css";""";

    [BuildTemplate("package.json")]
    public static string GetPackageJsonTemplate() => """
{
  "name": "cdcsharp-blazorui",
  "version": "1.0.0",
  "description": "BlazorUI Component Library",
  "type": "module",
  "public static": true,
  "scripts": {
    "build": "echo 'Use MSBuild to build BlazorUI'"
  },
  "devDependencies": {
    "typescript": "latest",
    "vite": "latest",
    "terser": "latest",
    "glob": "latest"
  }
}
""";

    [BuildTemplate("tsconfig.json")]
    public static string GetTsConfigTemplate() => """
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

    [BuildTemplate("CssBundle/main.css")]
    public static string GetMainCssTemplate() => """
/* === GLOBAL CSS BUNDLE === */

/* 1. Reset & Base */
@import './reset.css';

/* 2. Theme Variables */
@import './themes.css';
@import './initialize-themes.css';

/* 3. Universal Component Styles */
@import './common-classes.css';
@import './transition-classes.css';

/* 4. Family-based Shared Styles */
@import './_input-family.css';
@import './_dropdown-family.css';
""";

    [BuildTemplate("vite.config.js")]
    public static string GetViteJsConfigTemplate() => """
import { defineConfig } from 'vite';
import path from 'path';
import { glob } from 'glob';

const inputFiles = glob.sync('./Types/**/[A-Z]*.ts').reduce((entries, filePath) => {
    const parts = filePath.split('/');
    const fileName = parts[parts.length - 1].replace('.ts', '');
    const folderName = parts[parts.length - 2];
    const entryName = parts.length > 3 ? `${folderName}/${fileName}` : fileName;
    entries[entryName] = filePath;
    return entries;
}, {});

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
            preserveEntrySignatures: 'strict',
            output: {
                entryFileNames: '[name].min.js',
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

    [BuildTemplate("vite.config.css.js")]
    public static string GetViteCssConfigTemplate() => """
import { defineConfig } from "vite";

export default defineConfig({
    build: {
        outDir: "wwwroot/css",
        emptyOutDir: false,
        cssCodeSplit: false,
        rollupOptions: {
            input: "./CssBundle/entry.js",
            output: {
                assetFileNames: (assetInfo) => {
                    if (assetInfo.name.endsWith(".css")) {
                        return "blazorui.css";
                    }
                    return "[name][extname]";
                }
            },
            plugins: [
                {
                    name: "remove-js-output",
                    generateBundle(_, bundle) {
                        Object.keys(bundle).forEach((file) => {
                            if (file.endsWith(".js")) {
                                delete bundle[file];
                            }
                        });
                    }
                }
            ]
        }
    }
});
""";
}
