# CdCSharp.BlazorUI

Component library for Blazor Server and Blazor WebAssembly on .NET 10.

## Packages

| Package | Purpose |
|---|---|
| `CdCSharp.BlazorUI` | Component library (components, variants, theming, JS behaviors). |
| `CdCSharp.BlazorUI.Core` | Framework-agnostic primitives: base component types, `IHas*` behavior interfaces, palette/theme types. |
| `CdCSharp.BlazorUI.SyntaxHighlight` | Dependency-free syntax highlighter used by `BUICodeBlock`. |
| `CdCSharp.BlazorUI.Localization.Server` | Cookie-based `RequestLocalization` + culture endpoint + `BUICultureSelector` for Blazor Server hosts. |
| `CdCSharp.BlazorUI.Localization.Wasm` | `localStorage`-persisted culture + `BUICultureSelector` for Blazor WebAssembly hosts. |
| `CdCSharp.BlazorUI.BuildTools` | `dotnet` tool invoked by the shipped `.targets` to generate the CSS bundle at build time. |

## Localization: Server vs. WASM

Both localization packages provide the same `BUICultureSelector` component but persist the user's culture differently:

- **`CdCSharp.BlazorUI.Localization.Server`** — use when hosting in Blazor Server. Registers `app.UseRequestLocalization(...)` via an `IStartupFilter`, persists the selected culture as an HTTP cookie, and adds a `/Culture/Set` endpoint so non-JS redirects still update the culture.
- **`CdCSharp.BlazorUI.Localization.Wasm`** — use when hosting in Blazor WebAssembly. Persists the selected culture in `localStorage` via JS interop and configures `CultureInfo.DefaultThreadCurrentCulture` / `DefaultThreadCurrentUICulture` at startup.

For prerendered WASM (hosted WASM app with a Server prerender), install **both** — Server handles the initial HTTP response culture and WASM takes over after boot.
