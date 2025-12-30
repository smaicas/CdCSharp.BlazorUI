using CdCSharp.BlazorUI.BuildTools.Pipeline;

namespace CdCSharp.BlazorUI.BuildTools.Infrastructure;

public class BuildConfigurationManager
{
    private readonly BuildContext _context;

    public BuildConfigurationManager(BuildContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync()
    {
        await EnsurePackageJsonAsync();
        await EnsureTsConfigAsync();
        await EnsureViteConfigsAsync();
        await EnsureNpmrcAsync();
        await EnsureCssEntryAsync();
    }

    private async Task EnsurePackageJsonAsync()
    {
        string path = _context.GetFullPath("package.json");
        if (File.Exists(path)) return;

        string content = GetPackageJsonTemplate();
        await File.WriteAllTextAsync(path, content);
    }

    private async Task EnsureTsConfigAsync()
    {
        string path = _context.GetFullPath("tsconfig.json");
        if (File.Exists(path)) return;

        string content = GetTsConfigTemplate();
        await File.WriteAllTextAsync(path, content);
    }

    private async Task EnsureViteConfigsAsync()
    {
        string jsPath = _context.GetFullPath("vite.config.js");
        if (!File.Exists(jsPath))
        {
            await File.WriteAllTextAsync(jsPath, GetViteJsConfigTemplate());
        }

        string cssPath = _context.GetFullPath("vite.config.css.js");
        if (!File.Exists(cssPath))
        {
            await File.WriteAllTextAsync(cssPath, GetViteCssConfigTemplate());
        }
    }

    private async Task EnsureNpmrcAsync()
    {
        string path = _context.GetFullPath(".npmrc");
        if (File.Exists(path)) return;

        await File.WriteAllTextAsync(path, "fund=false\naudit=false\n");
    }

    private async Task EnsureCssEntryAsync()
    {
        string mainCssPath = _context.GetFullPath("CssBundle/main.css");
        if (!File.Exists(mainCssPath))
        {
            await File.WriteAllTextAsync(mainCssPath, GetMainCssTemplate());
        }

        string entryJsPath = _context.GetFullPath("CssBundle/entry.js");
        if (!File.Exists(entryJsPath))
        {
            await File.WriteAllTextAsync(entryJsPath, "import \"./main.css\";\n");
        }
    }

    private string GetPackageJsonTemplate() => """
{
  "name": "cdcsharp-blazorui",
  "version": "1.0.0",
  "description": "BlazorUI Component Library",
  "type": "module",
  "private": true,
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

    private string GetTsConfigTemplate() => """
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

    private string GetMainCssTemplate() => """
@import './reset.css';
@import './themes.css';
@import './initialize-themes.css';
@import './common-classes.css';
@import './transition-classes.css';
""";

    private string GetViteJsConfigTemplate() => """
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

    private string GetViteCssConfigTemplate() => """
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
