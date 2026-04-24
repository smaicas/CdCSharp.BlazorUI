# CdCSharp.BlazorUI

[![NuGet](https://img.shields.io/nuget/v/CdCSharp.BlazorUI.svg)](https://www.nuget.org/packages/CdCSharp.BlazorUI)
[![Build](https://github.com/smaicas/CdCSharp.BlazorUI/actions/workflows/publish.yml/badge.svg)](https://github.com/smaicas/CdCSharp.BlazorUI/actions/workflows/publish.yml)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE.txt)

Component library for Blazor Server and Blazor WebAssembly on .NET 10. Ships themeable, accessible components driven by a reflective `data-bui-*` + CSS-custom-property pipeline, with variants, design tokens, and JS-behavior interop for features like ripples, transitions, dropdowns, and modals.

## Quickstart

```bash
dotnet add package CdCSharp.BlazorUI
```

Register the services in `Program.cs`:

```csharp
using CdCSharp.BlazorUI;

builder.Services.AddBlazorUI();
```

Place `<BUIInitializer />` once in the root layout (typically `MainLayout.razor`) so the theme, JS interop, and asset references are wired up:

```razor
<BUIInitializer />

<main>
    @Body
</main>
```

That's enough to start using any component:

```razor
@using CdCSharp.BlazorUI.Components

<BUIButton Variant="BUIButtonVariant.Filled" OnClick="@HandleClick">
    Click me
</BUIButton>

<BUIInputText @bind-Value="name" Label="Name" />
<BUICard Shadow="true">
    <p>Inside a themed card.</p>
</BUICard>
```

## Packages

| Package | Purpose |
|---|---|
| `CdCSharp.BlazorUI` | Component library (components, variants, theming, JS behaviors). |
| `CdCSharp.BlazorUI.Core` | Framework-agnostic primitives: base component types, `IHas*` behavior interfaces, palette/theme types. |
| `CdCSharp.BlazorUI.SyntaxHighlight` | Dependency-free syntax highlighter used by `BUICodeBlock`. |
| `CdCSharp.BlazorUI.Localization.Server` | Cookie-based `RequestLocalization` + culture endpoint + `BUICultureSelector` for Blazor Server hosts. |
| `CdCSharp.BlazorUI.Localization.Wasm` | `localStorage`-persisted culture + `BUICultureSelector` for Blazor WebAssembly hosts. |
| `CdCSharp.BlazorUI.FluentValidation` | `FluentValidation` integration for BlazorUI forms. |
| `CdCSharp.BlazorUI.BuildTools` | `dotnet` tool invoked by the shipped `.targets` to generate the CSS bundle at build time. |

## Localization: Server vs. WASM

Both localization packages provide the same `BUICultureSelector` component but persist the user's culture differently:

- **`CdCSharp.BlazorUI.Localization.Server`** — use when hosting in Blazor Server. Registers `app.UseRequestLocalization(...)` via an `IStartupFilter`, persists the selected culture as an HTTP cookie, and adds a `/Culture/Set` endpoint so non-JS redirects still update the culture.
- **`CdCSharp.BlazorUI.Localization.Wasm`** — use when hosting in Blazor WebAssembly. Persists the selected culture in `localStorage` via JS interop and configures `CultureInfo.DefaultThreadCurrentCulture` / `DefaultThreadCurrentUICulture` at startup.

For prerendered WASM (hosted WASM app with a Server prerender), install **both** — Server handles the initial HTTP response culture and WASM takes over after boot.

## Documentation

The component catalog, live demos, and API reference live in the docs site (Blazor WASM). Run locally:

```bash
dotnet run --project docs/CdCSharp.BlazorUI.Docs.Wasm
```

## Contributing

1. Fork the repository and create a topic branch off `develop`.
2. Install the pinned SDK — `dotnet --version` must match `global.json` (10.0.203 at time of writing).
3. Run `dotnet build CdCSharp.BlazorUI.slnx -c Debug` and `dotnet test` before opening a PR.
4. Follow the conventions in `AGENTS.md` (component architecture, CSS rules, async/JS interop patterns).
5. Open the PR against `develop`. CI packs and surfaces the `.nupkg` artifacts so reviewers can test-install.

Bug reports and feature requests: [GitHub Issues](https://github.com/smaicas/CdCSharp.BlazorUI/issues).

## License

Released under the [MIT License](LICENSE.txt). © 2026 Samuel Maícas (@cdcsharp).
