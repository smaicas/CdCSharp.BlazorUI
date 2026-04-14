# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

`CdCSharp.BlazorUI` is a .NET 10 Blazor component library published as NuGet packages. The solution file is `CdCSharp.BlazorUI.slnx` (the new XML-based SLN format — open/build with `dotnet`, not older VS versions).

## Common commands

```bash
# Restore and build the whole solution
dotnet restore
dotnet build CdCSharp.BlazorUI.slnx -c Debug

# Run all tests
dotnet test

# Run a single test (examples)
dotnet test test/CdCSharp.BlazorUI.Tests.Integration/CdCSharp.BlazorUI.Tests.Integration.csproj --filter "FullyQualifiedName~Button"
dotnet test --filter "DisplayName~Should_Match_Snapshots"

# Run the WASM docs site
dotnet run --project docs/CdCSharp.BlazorUI.Docs.Wasm

# Run the sample apps
dotnet run --project samples/CdCSharp.BlazorUI.AppTest.Server
dotnet run --project samples/CdCSharp.BlazorUI.AppTest.Wasm
```

CI (`.github/workflows/publish.yml`) builds projects in a specific order: CodeGeneration → Core → Main → BuildTools. If you do manual incremental builds and see analyzer/generator weirdness, build in this same order.

## Build pipeline (non-standard — read before touching the build)

`src/CdCSharp.BlazorUI/_build/CdCSharp.BlazorUI.targets` is imported both locally and into consumer projects via the NuGet `build/` folder. It runs `CdCSharp.BlazorUI.BuildTools` (a separate console executable, not an MSBuild task) as a `BeforeBuild` step.

`CdCSharp.BlazorUI.BuildTools` is itself a thin host — it depends on the third-party NuGet package `CdCSharp.BuildTools` (authored by the same owner; see `<PackageReference Include="CdCSharp.BuildTools" Version="1.0.3" />`). That package provides the asset-generation framework: the `[AssetGenerator]` / `[BuildTemplate]` attributes, `IAssetGenerator`, and the `BuildToolsManager.Build(projectPath)` entry point invoked from `Program.cs`. The local project only contributes (a) the generator classes under `Generators/` that produce BlazorUI's CSS, and (b) the template classes under `Infrastructure/BuildTemplates.cs` that materialize the npm/Vite/TypeScript config files. When adding or modifying generated output, work within this contract: `IAssetGenerator` classes set `FileName`/`Name` and return the file body from `GetContent()`; `[BuildTemplate("relative/path")]`-attributed static methods return literal file contents.

What BuildTools does:

- Materializes `package.json`, `tsconfig.json`, `vite.config.js`, `vite.config.css.js`, `.npmrc`, and the `CssBundle/main.css` + `CssBundle/entry.js` scaffolding from `[BuildTemplate]` methods in `src/CdCSharp.BlazorUI.BuildTools/Infrastructure/BuildTemplates.cs`. Editing these files on disk directly is pointless — they are regenerated. Change the template source.
- Generates CSS partials (reset, typography, theme variables, design tokens, base component styles, transition classes, family-based shared styles) into `src/CdCSharp.BlazorUI/CssBundle/` from the `[AssetGenerator]` classes under `src/CdCSharp.BlazorUI.BuildTools/Generators/`.
- Runs `npm`/Vite to produce bundled CSS/JS into `wwwroot/css` and `wwwroot/js`, which are then `<Content Include>`-ed into the package.
- The `CleanBlazorUIAssets` target (`dotnet clean`) **deletes** the generated `package.json`, `tsconfig.json`, `vite.config*.js`, `.npmrc`, and the `CssBundle/`, `node_modules/`, `wwwroot/css`, `wwwroot/js` directories. This is expected — they are rebuilt next build.

`CdCSharp.BlazorUI.Core.CodeGeneration` is a Roslyn incremental source generator (referenced as `OutputItemType="Analyzer"`) that expands `[AutogenerateCssColors]`-attributed classes into per-color partial classes at compile time (see `ColorClassGenerator.cs`).

## Project layout

- `src/CdCSharp.BlazorUI.Core` — framework-agnostic primitives: abstractions, component base classes (`BUIComponentBase`, `BUIInputComponentBase`, `BUIVariantComponentBase`), theming (`LightTheme`, `DarkTheme`), CSS/color types (`CssColor`, `HsvColor`, `PaletteColor`), `[AutogenerateCssColors]` attribute, utilities.
- `src/CdCSharp.BlazorUI` — the actual component library (`Components/Forms`, `Components/Generic`, `Components/Layout`). Marked `InternalsVisibleTo` the integration test project. Depends on Core and the SyntaxHighlight package; references the CodeGeneration project as an analyzer.
- `src/CdCSharp.BlazorUI.Core.CodeGeneration` — Roslyn source generators (netstandard analyzer project).
- `src/CdCSharp.BlazorUI.BuildTools` — the build-time executable consumed by the `.targets` file.
- `src/CdCSharp.BlazorUI.SyntaxHighlight` — standalone package used by the `CodeBlock` component.
- `src/CdCSharp.BlazorUI.Localization.Server` / `.Wasm` — localization integrations for the two hosting models.
- `docs/CdCSharp.BlazorUI.Docs.Wasm` — public documentation site (Blazor WASM).
- `samples/` — Server and WASM sample/test harnesses.
- `test/CdCSharp.BlazorUI.Tests.Integration` — bUnit + Verify snapshot tests, xUnit.
- `test/CdCSharp.BlazorUI.Tests.Integration.Templates` — razor templates used as snapshot subjects.
- `tools/CdCSharp.BlazorUI.Tools.MaterialIconsScrapper` — one-off utility (not part of the shipped library).

## Component architecture

- All components derive from `BUIComponentBase` (or `BUIInputComponentBase` / `BUIVariantComponentBase`) in `CdCSharp.BlazorUI.Core.Abstractions.Components`. Do not inherit directly from `ComponentBase` for library components.
- `BUIComponentBase` captures unmatched attributes, exposes a computed-attributes dictionary built via `BUIComponentAttributesBuilder`, and wires up a per-component JS behavior via `BUIComponentJsBehaviorBuilder` during `OnAfterRenderAsync(firstRender)`. Override `BuildComponentCssVariables` and `BuildComponentDataAttributes` rather than re-implementing attribute composition.
- JS interop is split into topic-specific interfaces in `Services/JsInterop/` (`IThemeJsInterop`, `IBehaviorJsInterop`, `IPatternJsInterop`, `IDropdownJsInterop`, `IClipboardJsInterop`, `ITextAreaJsInterop`, `IDraggableJsInterop`, `IColorPickerJsInterop`, `IModalJsInterop`). Corresponding TypeScript lives under `Types/<Feature>/` and is bundled to `wwwroot/js/` by Vite.
- Variants are registered via `AddBlazorUI()` + `AddBlazorUIVariants(b => b.ForComponent<UIButton>().AddVariant(...))` in `ServiceCollectionExtensions.cs`. Variant templates are `Func<TComponent, RenderFragment>` stored in the singleton `IVariantRegistry`.

## DOM/CSS generation via `IHas*` interfaces

Styling is **not** done with CSS class toggles per feature. Components advertise capabilities by implementing marker/property interfaces, and `BUIComponentAttributesBuilder` (`src/CdCSharp.BlazorUI.Core/Components/BUIComponentAttributesBuilder.cs`) reflects over the component at render time to emit `data-bui-*` attributes and `--bui-inline-*` CSS custom properties on the root element. CSS in `src/CdCSharp.BlazorUI/CssBundle/*.css` then selects on those data-attributes and reads those variables.

Call flow:

1. In the component's `BuildRenderTree`, spread `@attributes="ComputedAttributes"` onto the root element (the `<bui-component>` custom tag — `FeatureDefinitions.Tags.Component`).
2. `BUIComponentBase` calls `BUIComponentAttributesBuilder.BuildStyles(this, AdditionalAttributes)` which:
   - Emits `data-bui-component="<kebab-name>"` (with the `BUI` prefix stripped from the type name).
   - For each `IHas*` interface the component implements, sets the matching entry in `FeatureDefinitions.DataAttributes` or `FeatureDefinitions.InlineVariables`.
   - Merges all `--bui-inline-*` vars into a single `style="..."` string, preserving any user-provided inline `style`.
3. `PatchVolatileAttributes` is called on re-renders for the subset of states that flip frequently (`IHasActive`, `IHasDisabled`) without rebuilding the full attribute set.
4. Components can hook `BuildComponentDataAttributes` / `BuildComponentCssVariables` on `IBuiltComponent` (via `BUIComponentBase` virtuals) to contribute extra attributes/vars before `style` is flattened.

Interface → DOM output mapping (see `FeatureDefinitions.cs` for the canonical attribute/variable names):

| Interface | Namespace folder | What it emits |
|---|---|---|
| `IVariantComponent` | `Core/Components/Variants` | `data-bui-variant="<name>"` |
| `IInputFamilyComponent` / `IPickerFamilyComponent` / `IDataCollectionFamilyComponent` | `Abstractions/Behaviors/Families` | `data-bui-input-base`, `data-bui-picker`, `data-bui-data-collection` (marker interfaces — no members) |
| `IHasSize` / `IHasDensity` / `IHasFullWidth` | `Abstractions/Behaviors/Design` | `data-bui-size`, `data-bui-density`, `data-bui-fullwidth` |
| `IHasColor` / `IHasBackgroundColor` | `Abstractions/Behaviors/Design` | `--bui-inline-color`, `--bui-inline-background` |
| `IHasBorder` | `Abstractions/Behaviors/Design` | `--bui-inline-border[-top/right/bottom/left/radius]` from `BorderCssValues` |
| `IHasShadow` | `Abstractions/Behaviors/Design` | `data-bui-shadow="true"` + `--bui-inline-shadow` |
| `IHasPrefix` / `IHasSuffix` | `Abstractions/Behaviors/Design` | `--bui-inline-prefix-*` / `--bui-inline-suffix-*` colors |
| `IHasLoading` / `IHasError` / `IHasDisabled` / `IHasActive` / `IHasReadOnly` / `IHasRequired` | `Abstractions/Behaviors/State` | `data-bui-loading`, `data-bui-error`, `data-bui-disabled`, `data-bui-active`, `data-bui-readonly`, `data-bui-required` |
| `IHasRipple` | `Abstractions/Behaviors/Javascript` | `data-bui-ripple` + `--bui-inline-ripple-color`, `--bui-inline-ripple-duration` (and wires up the JS behavior) |
| `IHasTransitions` | `Abstractions/Behaviors/Transitions` | `data-bui-transitions="<space-separated classes>"` + transition CSS vars |

Consequences for contributors:

- **To add a new styling axis**: (1) add an `IHas*` interface under the matching `Abstractions/Behaviors/{Design,State,Javascript,Transitions,Families}` folder, (2) register constants in `FeatureDefinitions.DataAttributes` or `FeatureDefinitions.InlineVariables`, (3) add a `Build<Name>` branch in `BUIComponentAttributesBuilder.BuildStyles`, (4) consume the attribute/variable in the relevant `CssBundle/*.css` template (remember: that file is regenerated from `BuildTemplates.cs` — edit the template).
- **Do not add component-specific CSS classes** to express state that already has a data-attribute; selectors across the library are keyed on `[data-bui-*]` for consistency.
- **Order-sensitive**: `BuildStyles` calls `BuildComponentDataAttributes` / `BuildComponentCssVariables` *before* `BuildInlineStyles` flattens the variable dictionary into `style`, so component-supplied vars participate in the final inline `style` string alongside framework ones.
- `IJsBehavior`-derived interfaces (like `IHasRipple`) additionally participate in the `BUIComponentJsBehaviorBuilder` pipeline in `OnAfterRenderAsync` — they're not purely CSS-driven.

## CSS architecture

Two distinct CSS layers ship with the library, and new components must respect the boundary between them.

### 1. Global CSS bundle (generated)

Produced by the `[AssetGenerator]` classes in `src/CdCSharp.BlazorUI.BuildTools/Generators/`, written into `src/CdCSharp.BlazorUI/CssBundle/`, then bundled by Vite into `wwwroot/css/`. Load order (from the generated `main.css`):

1. `_reset.css` — CSS reset (`ResetGenerator.cs`).
2. `_typography.css` — font families, sizes, line-heights as `--bui-font-*` vars (`TypographyGenerator.cs`).
3. `_themes.css` + `_initialize-themes.css` — theme palette variables (`--palette-primary`, `--palette-surfacecontrast`, etc.) and data-attribute theme activation (`ThemesCssGenerator.cs`, `CssInitializeThemesGenerator.cs`, fed by `LightTheme`/`DarkTheme` in Core).
4. `_tokens.css` — global design tokens: z-index scale, opacity states, size multipliers (`--bui-small-multiplier`, `--bui-medium-multiplier`, `--bui-large-multiplier`), density multipliers, highlight outline (`DesignTokensGenerator.cs`).
5. `_base.css` — universal `<bui-component>` styles: base layout, size system, density system, state styles (disabled/fullwidth), shadow system, utility classes (`BaseComponentGenerator.cs`).
6. `_transition-classes.css` — keyframed transition classes referenced by `[data-bui-transitions]` (`TransitionsCssGenerator.cs`).
7. `_input-family.css`, `_picker-family.css`, `_data-collection-family.css` — family-shared styles (see below).

All generators reference `FeatureDefinitions` constants when emitting selectors and custom property names — **never hardcode** a `data-bui-*` attribute, `--bui-*` variable, or `bui-*` class name in a generator; look it up in `FeatureDefinitions`.

### 2. Scoped component CSS (`.razor.css`)

Each component ships its own `<Component>.razor.css` next to the `.razor` file (e.g. `Components/Generic/Button/BUIButton.razor.css`). These are handled by Blazor CSS isolation, scoped per-component at publish time. They are **not** generated — they are hand-written and checked in.

### Family system

"Families" are groups of components that share a DOM + CSS contract. Membership is declared with a marker interface (no members) from `Core/Abstractions/Behaviors/Families/FamilyInterfaces.cs`:

- `IInputFamilyComponent` → `[data-bui-input-base]` → `_input-family.css`. Used by text/number/dropdown/textarea/etc. Defines the `bui-input__wrapper`, `bui-input__field`, `bui-input__label`, `bui-input__outline{,-leading,-notch,-trailing}`, `bui-input__addon--prefix/suffix`, `bui-input__helper-text`, `bui-input__validation` class names (see `FeatureDefinitions.CssClasses.Input`) and implements the `outlined`/`filled`/`standard` variant layouts, including the floated-label notch behavior driven by `[data-bui-floated]`.
- `IPickerFamilyComponent` → `[data-bui-picker]` → `_picker-family.css`. Used by color pickers, date pickers, etc. Provides `bui-picker__row/title/grid/cell/cell--selected/cell--muted/input/separator/slider/preview`.
- `IDataCollectionFamilyComponent` → `[data-bui-data-collection]` → `_data-collection-family.css`. Used by tables/trees/lists.

Input components use `BUIInputComponentBase` (which already implements `IInputFamilyComponent`), so the family attribute is emitted automatically by `BUIComponentAttributesBuilder.BuildFamilyAttributes`.

### Standards every component must follow

1. **Root element**: render a `<bui-component>` custom tag (`FeatureDefinitions.Tags.Component`) as the outermost element of every variant, with `@attributes="ComputedAttributes"` spread onto it. All framework-driven styling (`data-bui-*`, inline `--bui-inline-*` vars) lives on this element.
2. **Selector form**: target the component with `bui-component[data-bui-component="<kebab-name>"]` in the scoped `.razor.css`. Do not use component-specific CSS classes for identification; the kebab name is emitted automatically from the type name (e.g. `BUIButton` → `button`).
3. **BEM inside the component**: children use `bui-<component>__<element>` / `bui-<component>__<element>--<modifier>` classes. Reuse the family class names (e.g. `bui-input__field`) instead of inventing new ones when a component belongs to a family.
4. **Private-var pattern**: at the component root, declare `--_<component>-<property>` custom properties that resolve `var(--bui-inline-*, <default>)`. Then reference only the `--_<component>-*` vars in the actual `background`/`color`/`border`/etc. declarations. Example (from `BUIButton.razor.css`):
   ```css
   bui-component[data-bui-component="button"] {
       --_button-background: var(--bui-inline-background, var(--palette-primary));
       --_button-color: var(--bui-inline-color, var(--palette-primarycontrast));
   }
   bui-component[data-bui-component="button"] button {
       background-color: var(--_button-background);
       color: var(--_button-color);
   }
   ```
   This gives a single, predictable override surface: users set `BackgroundColor`/`Color`/`Border`/etc. parameters on the component (or write CSS that sets `--bui-inline-*` directly), and the component-private `--_*` var picks it up with a palette-backed fallback.
5. **Sizing via multiplier, not breakpoints**: component dimensions are expressed as `calc(<base> * var(--bui-size-multiplier, 1))`. Do not add per-size selectors — `_base.css` maps `[data-bui-size="small|medium|large"]` to the multiplier once for the whole library.
6. **Density via multiplier**: inter-element spacing (gaps, some paddings) uses `calc(<base> * var(--bui-density-multiplier, 1))`, populated by `[data-bui-density="compact|standard|comfortable"]` in `_base.css`.
7. **State via data-attributes**: never toggle CSS classes to represent `disabled`, `loading`, `error`, `active`, `readonly`, `required`, `fullwidth`, `shadow`, `ripple`, `floated`, `variant`, etc. — those are `data-bui-*` attributes emitted by `BUIComponentAttributesBuilder` from the matching `IHas*` interface. Select on the attribute in CSS.
8. **Theme colors**: consume `var(--palette-*)` (from `_themes.css`) rather than hardcoded colors. For contrast/hover/active derivations, prefer `color-mix(in oklab, var(--_component-...) X%, var(--palette-hovertint) Y%)`.
9. **Transitions**: if the component needs enter/exit animation, implement `IHasTransitions` and add `class="transition-target"` to the element that should animate — the transition classes themselves come from `_transition-classes.css` and are selected via `[data-bui-transitions]`.
10. **Do not hand-edit generated files**: `CssBundle/*.css` and the Vite/TypeScript config files are rewritten on every build. Changes belong in the corresponding `Generators/*.cs`, `Infrastructure/BuildTemplates.cs`, or `FeatureDefinitions`.

## Testing

- Integration tests (`test/CdCSharp.BlazorUI.Tests.Integration`) use **bUnit 2.x**, **Verify** for snapshots, **FluentAssertions**, **NSubstitute**, **xUnit**.
- Tests are parameterized over Server vs. WASM hosting via `TestScenarios.All` / `.OnlyServer` / `.OnlyWasm` (see `Infrastructure/TestScenarios.cs` and `Contexts/`). Most component tests should run under both scenarios.
- Verify produces `*.received.txt` / `*.verified.txt` snapshots; `received` files are gitignored but the `.csproj` also excludes some historical snapshot directories explicitly — don't be surprised by the `<Compile Remove>` entries.

## Release / versioning

Versioning is driven by `.github/workflows/publish.yml`:

- Push to `develop` → auto-publishes `1.0.{run}-preview.{run}` to NuGet.
- Tag `vX.Y.Z` → publishes `X.Y.Z` and creates a GitHub release.
- Push to `main` → builds only, does not publish.
- Manual `workflow_dispatch` with `publish=true` required for ad-hoc publishes.

`MAJOR_VERSION` / `MINOR_VERSION` are env vars in the workflow, not in `csproj` files.
