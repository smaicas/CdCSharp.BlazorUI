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

The build pipeline is split across two `.targets` files to keep maintainer-only asset generation out of the consumer's build:

- **`src/CdCSharp.BlazorUI/_build/CdCSharp.BlazorUI.Dev.targets`** — imported *only* in the monorepo (via a `Condition="Exists(...)"` import in the csproj). Runs `CdCSharp.BlazorUI.BuildTools` (a separate console executable, not an MSBuild task) as a `BeforeBuild` step, regenerating the CSS bundle and Vite/TypeScript scaffolding.
- **`src/CdCSharp.BlazorUI/_build/CdCSharp.BlazorUI.targets`** — the *consumer-facing* targets file that ships inside the NuGet package under `build/`. By design (F1 decision D-03) this file is a no-op: the bundled CSS (`wwwroot/css/blazorui.css`) and the minified JS under `wwwroot/js/**` are produced by the maintainer build and ship as StaticWebAssets inside the `.nupkg` (see `staticwebassets/` in the pack listing). Consumers do **not** run BuildTools, Node, npm, or Vite — the Razor SDK on their side serves the pre-bundled assets from the restored package.

When modifying the build pipeline, edit `CdCSharp.BlazorUI.Dev.targets`. Keep `CdCSharp.BlazorUI.targets` minimal — any asset-generation work placed there will run inside the consumer's build and fail in environments without the BuildTools toolchain installed.

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

- All components derive from `BUIComponentBase` (or `BUIInputComponentBase` / `BUIVariantComponentBase`) in `CdCSharp.BlazorUI.Abstractions`. Do not inherit directly from `ComponentBase` for library components.
- `BUIComponentBase` captures unmatched attributes, exposes a computed-attributes dictionary built via `BUIComponentAttributesBuilder`, and wires up a per-component JS behavior via `BUIComponentJsBehaviorBuilder` during `OnAfterRenderAsync(firstRender)`. Override `BuildComponentCssVariables` and `BuildComponentDataAttributes` rather than re-implementing attribute composition.
- The style+behavior lifecycle is owned by an internal `BUIComponentPipeline` helper composed into both `BUIComponentBase` and `BUIInputComponentBase<TValue>`. `BUIInputComponentBase<TValue>` cannot inherit `BUIComponentBase` directly because it must derive from `InputBase<TValue>` for `EditContext`/`ValueExpression` participation, so the pipeline is shared by composition. When extending the pipeline (e.g. adding a new phase or perf hook), edit `BUIComponentPipeline` — not the two base classes individually.
- JS interop is split into topic-specific interfaces in `Services/JsInterop/` (`IThemeJsInterop`, `IBehaviorJsInterop`, `IPatternJsInterop`, `IDropdownJsInterop`, `IClipboardJsInterop`, `ITextAreaJsInterop`, `IDraggableJsInterop`, `IColorPickerJsInterop`, `IModalJsInterop`). Corresponding TypeScript lives under `Types/<Feature>/` and is bundled to `wwwroot/js/` by Vite.
- Variants are registered via `AddBlazorUI()` + `AddBlazorUIVariants(b => b.ForComponent<UIButton>().AddVariant(...))` in `ServiceCollectionExtensions.cs`. Variant templates are `Func<TComponent, RenderFragment>` stored in the singleton `IVariantRegistry`.
- `BUIInputDropdown` intentionally does **not** inherit `BUIInputComponentBase`. It composes an internal `BUIDropdownContainer` (which itself derives from `BUIInputComponentBase`) and forwards all input-family parameters (`Value`/`ValueChanged`/`ValueExpression`, state flags, design tokens) down to it. Reason: the dropdown is a dual-responsibility component (trigger + menu), and the trigger's input-family DOM is emitted by the inner container. Consumers see a single `<bui-component>` root rendered by the container with the full `data-bui-*` / `--bui-inline-*` surface. `EditContext`/validation and `IHas*` state propagate via the forwarded `ValueExpression` and explicit parameter delegation — do not duplicate that state on the outer `BUIInputDropdown`.

## DOM/CSS generation via `IHas*` interfaces

Styling is **not** done with CSS class toggles per feature. Components advertise capabilities by implementing marker/property interfaces, and `BUIComponentAttributesBuilder` (`src/CdCSharp.BlazorUI.Core/Components/BUIComponentAttributesBuilder.cs`) reflects over the component at render time to emit `data-bui-*` attributes and `--bui-inline-*` CSS custom properties on the root element. CSS in `src/CdCSharp.BlazorUI/CssBundle/*.css` then selects on those data-attributes and reads those variables.

Call flow:

1. In the component's `BuildRenderTree`, spread `@attributes="ComputedAttributes"` onto the root element (the `<bui-component>` custom tag — `FeatureDefinitions.Tags.Component`).
2. `BUIComponentBase` calls `BUIComponentAttributesBuilder.BuildStyles(this, AdditionalAttributes)` which:
   - Emits `data-bui-component="<kebab-name>"` (with the `BUI` prefix stripped from the type name).
   - For each `IHas*` interface the component implements, sets the matching entry in `FeatureDefinitions.DataAttributes` or `FeatureDefinitions.InlineVariables`.
   - Merges all `--bui-inline-*` vars into a single `style="..."` string, preserving any user-provided inline `style`.
3. `PatchVolatileAttributes` is called on re-renders for the subset of states that flip frequently (`IHasActive`, `IHasDisabled`, `IHasLoading`, `IHasError`, `IHasReadOnly`, `IHasRequired`, `IHasFullWidth`) without rebuilding the full attribute set.
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

### State parameters: `[Parameter] X` vs computed `IsX`

The four input-style state axes (`Disabled`, `Error`, `ReadOnly`, `Required`) and the behavioral axis `Active` follow a two-surface contract on their `IHas*` interface:

- **`[Parameter] public bool X { get; set; }`** — *force from outside*. The parent can pin the state explicitly; it acts as an OR on top of the internal evaluation.
- **`public bool IsX { get; }`** — the *computed truth*. Combines the parameter with any internal condition the component evaluates. This is what `aria-*` attributes, the `disabled`/`readonly` HTML attributes on the rendered element, any JS-behavior gating, and `BUIComponentAttributesBuilder` must read. Components never use the raw `X` parameter for rendering decisions.

The base contract is `IsX = X || <internal>`. The internal part depends on the component:

| Axis | Internal condition layered on top of the parameter |
|---|---|
| `IsDisabled` | `Disabled \|\| (this is IHasLoading l && l.Loading)` — loading implies disabled. |
| `IsError` | `Error \|\| <field has validation messages from EditContext>` — native `EditForm` validation drives the visual error state even when the consumer does not pass `Error=true`. In `BUIInputComponentBase<TValue>` this is cached in `_lastValidationError`, updated in `OnParametersSet` and `HandleValidationStateChanged`. |
| `IsReadOnly` | `ReadOnly` only, by default. |
| `IsRequired` | `Required` only, by default (annotation-driven `Required` could layer in later). |
| `IsActive` | `Active` plus any component-owned notion of "currently active" (hover/focus/open/etc. when applicable). |

The `IHas*` interfaces carry **both** members: `X` (the override) and `IsX` (the truth). `BUIComponentAttributesBuilder` emits `data-bui-<axis>` using `IsX` only, both in `BuildStyles` and in `PatchVolatileAttributes`. If you add a new state axis, follow the same shape: interface exposes `X` + `IsX`, component implements `IsX = X || <internal>`, builder reads `IsX`.

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

## Async / JS interop conventions

- **Do not use `ConfigureAwait(false)`** in library code. Blazor Server relies on the renderer's `SynchronizationContext` so UI state mutations (`StateHasChanged`, `EventCallback.InvokeAsync`) and `IJSRuntime` calls marshal back to the correct dispatcher. Escaping the context via `ConfigureAwait(false)` produces subtle threading bugs under Server. WASM is single-threaded so it is unaffected, but the same code runs on both hosts — the rule is uniform: await without `ConfigureAwait`.
- **JS interop disposal**: wrap `IJSObjectReference` / `InvokeVoidAsync` calls during teardown paths in the 4-exception catch set: `JSDisconnectedException`, `ObjectDisposedException`, `InvalidOperationException`, `TaskCanceledException`. All four map to non-actionable teardown paths (circuit gone, runtime disposed, prerender without circuit, awaited call cancelled). `ModuleJsInteropBase.DisposeAsync` and `BUIComponentPipeline.DisposeBehaviorAsync` are the reference implementations.
- **Disposable components**: `BUIComponentBase` and `BUIInputComponentBase<TValue>` expose a `protected bool IsDisposed { get; }` that flips to `true` the moment `DisposeAsync` / `Dispose(true)` starts running. Derived components that subscribe to `NavigationManager.LocationChanged`, register children through cascading parameters, or hold a `CancellationTokenSource` must gate any post-await continuation on `IsDisposed` before touching component state — do **not** re-introduce a private `_disposed` field. The base classes already gate their own `OnAfterRenderAsync` path (the JS-behavior attach) on `IsDisposed`; derived components only need to guard their own awaits.

## Testing

### Stack

- **bUnit 2.x** — Blazor component renderer.
- **xUnit** — test runner + `[Theory]` / `[Trait]` attributes.
- **FluentAssertions** — preferred assertion style (`.Should().Be(...)`, `.Should().Contain(...)`).
- **Verify** + **Verify.Xunit** + **Verify.Blazor** — snapshot testing (`*.verified.txt`).
- **NSubstitute** — test doubles when an `I*` service cannot be faked locally.

### Project layout

- `test/CdCSharp.BlazorUI.Tests.Integration` — the main test assembly. Tests live under `Tests/`:
  - `Tests/Components/<ComponentName>/` — one folder per BlazorUI component under test.
  - `Tests/Core/BaseComponents/` — tests for `BUIComponentBase` / `BUIInputComponentBase`.
  - `Tests/Library/` — cross-cutting library behaviour (DI registration, `CssColor`, `VariantRegistry`, etc.).
- `test/CdCSharp.BlazorUI.Tests.Integration.Templates` — razor-based test subjects (stubs, consumers). Add a `.razor` here whenever a test needs more than a single component rendered with `ctx.Render<T>(...)` — e.g. to exercise cascading parameters, `EditForm` integration, or multi-component parent/child flows. Consumer names end in `Consumer`, base stubs end in `_TestStub`.
- `test/CdCSharp.BlazorUI.Tests.Integration/Infrastructure/` — shared test plumbing:
  - `Contexts/BlazorTestContextBase.cs` — inherits `BunitContext`, registers `AddBlazorUI()`, `JSRuntimeMode.Loose`, `FakeNavigationManager`. Never construct `BunitContext` directly.
  - `Contexts/ServerTestContext.cs` / `WasmTestContext.cs` — add the hosting-specific localization package.
  - `TestScenarios.cs` — parameter source exposing `All`, `OnlyServer`, `OnlyWasm` (each yields a `BlazorScenario(Name, CreateContext)`). Feeds `[MemberData]`.
  - `Fakes/` — hand-rolled doubles (`FakeNavigationManager`, `FakeServerCultureService`, etc.).
  - `VerifyConfig.cs` — module initializer that scrubs `blazor:elementReference` GUIDs and event IDs from snapshots.
  - `ComponentTestExtensions.cs` — `GetNormalizedMarkup()` (CRLF→LF + trim) for stable snapshot input.

### Per-component file layout (the standard — follow Button)

One folder per component at `Tests/Components/<ComponentName>/`, one class per **context**:

| File | Class | `[Trait]` | Scope |
|---|---|---|---|
| `<Component>RenderingTests.cs` | `<Component>RenderingTests` | `"Component Rendering"` | Initial DOM: root tag, `data-bui-*`, `--bui-inline-*` vars, presence of children. |
| `<Component>StateTests.cs` | `<Component>StateTests` | `"Component State"` | Re-renders after parameter changes (color, size, loading, disabled, icons, ripple, custom attrs, volatile attrs). |
| `<Component>InteractionTests.cs` | `<Component>InteractionTests` | `"Component Interaction"` | Click/change/input/keyboard handlers, `EventCallback` firing, disabled gating. |
| `<Component>VariantTests.cs` | `<Component>VariantTests` | `"Component Variants"` | Custom variant registration via `AddBlazorUIVariants`, default vs. named variant rendering. |
| `<Component>AccessibilityTests.cs` | `<Component>AccessibilityTests` | `"Component Accessibility"` | `role`, `aria-*`, keyboard nav, focus management, tab-index. |
| `<Component>SnapshotTests.cs` | `<Component>SnapshotTests` | `"Component Snapshots"` | `Verify` over normalized markup across representative states (Default, WithIcon, Loading, Disabled, Elevated, etc.). |

Not every component needs every file — only the contexts that apply. Families may also warrant `<Component>ValidationTests.cs` (inputs + `EditContext`) or `<Component>IntegrationTests.cs` (parent/child composition).

### Test class conventions

- **Namespace**: `CdCSharp.BlazorUI.Tests.Integration.Tests.Components.<ComponentName>` (mirror folder).
- **Class name ends in the context** (`...RenderingTests`, `...StateTests`, …). One context per class.
- **Attribute on class**: `[Trait("Component <Context>", "<ComponentName>")]` — drives test-explorer grouping.
- **Every test is `[Theory]` with `[MemberData(nameof(TestScenarios.All), MemberType = typeof(TestScenarios))]`** by default so it runs under both Server and WASM. Use `OnlyServer` / `OnlyWasm` only when the behaviour is host-specific (e.g. circuit lifecycle, WASM-only localization).
- **Test method name**: `Should_<ExpectedBehaviour>[_When_<Condition>]` (e.g. `Should_Render_With_Correct_DataAttributes`, `Should_Not_Fire_Click_When_Disabled`).
- **Signature**: `public async Task Should_...(BlazorScenario scenario)`.
- **Body skeleton**:
  ```csharp
  await using BlazorTestContextBase ctx = scenario.CreateContext();

  // Arrange
  IRenderedComponent<BUIFoo> cut = ctx.Render<BUIFoo>(p => p.Add(c => c.X, ...));

  // Act
  cut.Render(p => p.Add(c => c.Y, ...));   // re-render, or cut.Find("...").Click();

  // Assert
  cut.Find("bui-component").GetAttribute("data-bui-...").Should().Be(...);
  ```
- Use `// Arrange` / `// Act` / `// Assert` banner comments — it is the project style, not optional.
- Always dispose the context with `await using` — the `BunitContext` holds a `ServiceProvider`.

### Assertion rules for DOM-bound tests

These mirror the component architecture — don't drift from them:

1. **Target the root via the custom tag**: `cut.Find("bui-component")`. Component identity is `data-bui-component="<kebab-name>"`, not a CSS class.
2. **Read state from `data-bui-*` attributes**, not class lists. E.g. loading → `data-bui-loading="true"`; disabled → `data-bui-disabled="true"`; size → `data-bui-size="large"`. Mirrors rule 7 of the component standard.
3. **Read styling from `--bui-inline-*` vars** in the root element's `style` attribute — `.GetAttribute("style").Should().Contain("--bui-inline-color: rgba(...)")`. Don't assert on private `--_<component>-*` vars.
4. **BEM class assertions** (`.bui-button__icon--leading`) are allowed for structural children that live inside the component, but never for state.
5. **User-supplied attributes** (`class`, `style`, `data-testid`, etc.) must be preserved through re-renders — assert this once per component in `StateTests`.
6. **Volatile attributes**: when flipping `Disabled` / `Loading` / `Error` / `ReadOnly` / `Required` / `FullWidth` / `Active` at runtime, re-read the same `IElement` reference after `cut.Render(...)` and assert the new `data-bui-*` value — that exercises `BUIComponentAttributesBuilder.PatchVolatileAttributes`.

### Interaction tests

- For components whose public contract is a single callback, call `ctx.Render<BUIFoo>` directly and wire `.Add(c => c.OnClick, _ => ...)`.
- For behaviours that depend on hosted usage (parent state, cascading params, `EditForm`), add a razor consumer under `test/CdCSharp.BlazorUI.Tests.Integration.Templates/Components/Consumers/Test<Component>Consumer.razor` and render that. See `TestBUIButtonConsumer` as the reference.
- Assert the **callback side-effect** (counter, captured value) **and** the DOM side-effect (displayed count, new state) when both are reachable.

### Variant tests

Each component that exposes variants must have a test that:

1. Builds a `BUIButtonVariant.Custom("X")` (or the component's own variant factory).
2. Registers it via `ctx.Services.AddBlazorUIVariants(b => b.ForComponent<BUIFoo>().AddVariant(custom, foo => builder => { ... }))`.
3. Renders with `.Add(c => c.Variant, customVariant)` and asserts that the custom render fragment ran (a marker class / text you emitted).

### Snapshot tests (`Verify`)

- Use the "representative states" pattern from `BUIButtonSnapshotTests`: define an array of `(Name, Builder)` tuples, render each, capture `cut.GetNormalizedMarkup()`, and `await Verify(results).UseParameters(scenario.Name)`.
- One snapshot test per component is usually enough; spread the states across the `testCases` array rather than creating many methods.
- `*.received.txt` is generated on diff — review, then rename to `*.verified.txt` to accept.
- Scrubbers in `VerifyConfig` already strip `blazor:elementReference` GUIDs and event IDs — do not attempt to match them in assertions.
- Snapshots live next to the test file. Historical snapshot paths appear as `<Compile Remove>` entries in the `.csproj`; ignore them.

### Base-component tests

`Tests/Core/BaseComponents/BUIComponentBaseTests.cs` + `BUIInputComponentBaseTests.cs` exercise the framework contract (`ComputedAttributes`, `PatchVolatileAttributes`, `BuildComponentDataAttributes`, validation wiring, `IHas*` reflection). When adding a new `IHas*` interface or a new `FeatureDefinitions` constant, extend these tests first — they catch regressions that per-component tests would miss.

### Library tests

`Tests/Library/` covers things with no single owning component: `ServiceRegistrationTests` (DI), `VariantRegistryTests`, `CssColorSystemTests`. New cross-cutting utilities (color math, registry behaviour, DI extensions) go here, not under `Components/`.

## Public API tracking

Every publishable project (`CdCSharp.BlazorUI.Core`, `CdCSharp.BlazorUI`, `CdCSharp.BlazorUI.SyntaxHighlight`, `CdCSharp.BlazorUI.Localization.Server`, `CdCSharp.BlazorUI.Localization.Wasm`) has `Microsoft.CodeAnalysis.PublicApiAnalyzers` enabled with two sidecar files at the project root:

- `PublicAPI.Shipped.txt` — the API surface currently released to NuGet. Do not hand-edit mid-release.
- `PublicAPI.Unshipped.txt` — additions/removals staged for the next release.

Workflow when you change the public surface:

1. Build. The analyzer raises `RS0016` (missing declaration) for new public members and `RS0017` (stale declaration) for removed ones. Both ship with a Roslyn code-fix — *Add to public API* / *Remove from public API* — that appends the line to `PublicAPI.Unshipped.txt` automatically.
2. Review `Unshipped.txt` as part of the PR; reviewers use it as the diff of the public contract.
3. On release, move all `Unshipped.txt` lines into `Shipped.txt` (plain concatenation, then clear `Unshipped.txt`). The release workflow does not do this automatically — it is a manual maintainer step.

Rules:

- Types that must remain `public` for Razor SDK / reflection / DI but should not appear in consumer IntelliSense get `[EditorBrowsable(EditorBrowsableState.Never)]` instead of being internalized. See `BUIBasePattern` for the canonical example (Razor-generated partial class forces `public`).
- Nested types and members declared `public` inside an `internal` parent still count as part of the API surface to the analyzer — collapse them to `internal` rather than fighting `RS0016`.
- `InternalsVisibleTo` does not affect the analyzer; the tracked surface is what the compiler would emit for an external consumer.

## Release / versioning

Versioning is driven by `.github/workflows/publish.yml`:

- Push to `develop` → auto-publishes `1.0.{run}-preview.{run}` to NuGet.
- Tag `vX.Y.Z` → publishes `X.Y.Z` and creates a GitHub release.
- Push to `main` → builds only, does not publish.
- Manual `workflow_dispatch` with `publish=true` required for ad-hoc publishes.

`MAJOR_VERSION` / `MINOR_VERSION` are env vars in the workflow, not in `csproj` files.
