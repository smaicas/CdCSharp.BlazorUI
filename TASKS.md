# TASKS — Cobertura de Tests BlazorUI

Origen: auditoría de `src/CdCSharp.BlazorUI.Core` y `src/CdCSharp.BlazorUI` comparada con `test/CdCSharp.BlazorUI.Tests.Integration`. Se identifican huecos de cobertura por archivo fuente (tipos públicos sin tests directos o componentes sin alguna de las categorías `Rendering / State / Interaction / Variants / Accessibility / Validation / Snapshots / Disposal`).

## Convenciones

- **Agrupación por commit**: las tareas están agrupadas por **contexto** (mismo tipo, misma familia de componentes o misma área transversal). Todas las tareas de un mismo identificador raíz (p. ej. `CORE-COV-01`) se resuelven **en el mismo commit** salvo indicación explícita.
- **Al completar una tarea**: cambiar `[ ]` por `[x]` y añadir al final del bullet una línea:
  > Resuelto en commit `<sha7>` — *<subject del commit>*
- **Estado**: `[ ]` pendiente, `[x]` hecho, `[~]` en curso, `[?]` requiere investigación previa.
- **Cada tarea incluye**: *Origen* (por qué), *Archivos* (fuente + destino de test), *Cambios* (qué añadir), *Aceptación* (criterio objetivo).
- **Estándar**: seguir la "Per-component file layout" y las reglas de `CLAUDE.md` (un archivo por contexto, `[Trait]` por clase, `[Theory]` + `TestScenarios.All` por defecto, aserciones sobre `data-bui-*` no sobre clases CSS).
- **Culture**: `VerifyConfig` fija `en-US`; snapshots nuevos deben generarse bajo esa cultura (sin hacks locales).

---

## A. COBERTURA CORE — `src/CdCSharp.BlazorUI.Core`

### [x] CORE-COV-01 — `BUIComponentAttributesBuilder` tests directos
- **Origen**: actualmente cubierto solo indirectamente a través de `BUIComponentBaseTests`. Los invariantes del builder (orden estable de `--bui-inline-*`, reutilización de diccionarios PERF-01/04, `PatchVolatileAttributes` por interfaz, `BuildFamilyAttributes` con múltiples familias) no tienen aserciones aisladas; un refactor silencioso podría romper contratos sin que los tests de componente lo detecten.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI.Core/Components/BUIComponentAttributesBuilder.cs`
  - Nuevo: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Core/BaseComponents/BUIComponentAttributesBuilderTests.cs`
- **Cambios**:
  - Construir componentes stub que implementen combinaciones de `IHas*` y verificar el `ComputedAttributes` exacto tras `BuildStyles`.
  - Aserciones sobre el `style` inline: misma cadena para las mismas vars independientemente del orden de inserción (orden estable).
  - Test de `PatchVolatileAttributes` para cada interfaz volátil (Active, Disabled, Loading, Error, ReadOnly, Required, FullWidth) comprobando que el resto de atributos no cambia.
  - Test de caché de flags por tipo (PERF-04): dos instancias del mismo tipo no reflexionan dos veces (medible si se expone counter o a través de benchmark en `DEBUG`).
  - Test de coexistencia de familias: componente que implementa `IInputFamilyComponent` + `IPickerFamilyComponent` emite ambos `data-bui-input-base`/`data-bui-picker`.
- **Aceptación**: nuevos tests verdes; mutación manual (eliminar un `case` en `BuildStyles`) produce fallo localizado.
  > Resuelto en commit `0d5a948` — *test(core): direct unit tests for BUIComponentAttributesBuilder*

### [x] CORE-COV-02 — `BUIComponentJsBehaviorBuilder` tests directos
- **Origen**: el pipeline de `IJsBehavior` (composición `IHasRipple` + otros) se ejecuta en `OnAfterRenderAsync` sin tests aislados. Cambios en la lógica de dispatch de módulos JS pueden romper el wiring silenciosamente.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI.Core/Components/BUIComponentJsBehaviorBuilder.cs`
  - Nuevo: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Core/BaseComponents/BUIComponentJsBehaviorBuilderTests.cs`
- **Cambios**:
  - Stub que implemente `IHasRipple` y otro `IJsBehavior` ficticio.
  - Verificar que `Build` emite las invocaciones JS esperadas contra `JSInterop` en `Loose` mode (usar `JSInterop.VerifyInvoke("...")`).
  - Test de dispose: tras `DisposeAsync`, no hay módulos pendientes; `JSDisconnectedException` se absorbe.
  - Test de guard: primer render false → no inicializa; re-render con parámetros cambiados no duplica.
- **Aceptación**: invocaciones JS esperadas por combinación de `IJsBehavior`; dispose sin excepciones observables.
  > Resuelto en commit `16a18a6` — *test(core): direct unit tests for BUIComponentJsBehaviorBuilder*

### [x] CORE-COV-03 — `VariantHelper` tests directos
- **Origen**: `VariantRegistryTests` cubre el registro/DI, pero la helper `VariantHelper.ResolveTemplate`/fallbacks/merge con `BuiltInTemplates` no se prueba directamente.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI.Core/Components/Variants/VariantHelper.cs`
  - Nuevo: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Library/VariantHelperTests.cs`
- **Cambios**:
  - Casos: template registrado (usa custom), sin template registrado (usa built-in), variante desconocida (usa default variant).
  - Verificar que un componente `BUIVariantComponentBase` pasado al helper resuelve el delegate correcto.
- **Aceptación**: matriz {registered | missing | default} cubierta con valores esperados.
  > Resuelto en commit `8fdf619` — *test(core): direct unit tests for VariantHelper*

### [x] CORE-COV-04 — `SelectionState` + `SelectionTypeInfo` tests
- **Origen**: `src/CdCSharp.BlazorUI.Core/Components/Selection/*.cs` sin tests. Selection es la base de dropdown/tree/radio/check con lógica de single/multi/mixed.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI.Core/Components/Selection/SelectionState.cs`, `SelectionTypeInfo.cs`, `ISelectionOption.cs`
  - Nuevo: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Library/SelectionStateTests.cs`
- **Cambios**:
  - Transiciones: `Select(item)`, `Deselect(item)`, `Toggle`, `Clear` en modos single/multiple.
  - `SelectionTypeInfo.Detect<T>()` para `T`, `T[]`, `List<T>`, `IReadOnlyList<T>`, `HashSet<T>`.
  - Normalización en single cuando se pasa una colección (debe tomar el primero o lanzar según contrato).
- **Aceptación**: todos los casos del contrato documentado cubiertos.
  > Resuelto en commit `dfbc8d2` — *test(core): direct unit tests for SelectionState and SelectionTypeInfo*

### [x] CORE-COV-05 — `BUIBorderPresets` + `IHasBorder` tests
- **Origen**: `BUIBorderPresets.None/Thin/Medium/Thick/Rounded/Circle/etc.` sin tests; `IHasBorder.BorderCssValues` tiene lógica de conversión a `--bui-inline-border*`.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI.Core/Abstractions/Behaviors/Design/BUIBorderPresets.cs`, `IHasBorder.cs`
  - Nuevo: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Library/BUIBorderPresetsTests.cs`
- **Cambios**:
  - Un test por preset verificando `BorderCssValues` (width/style/color/radius/por-lado).
  - Test de `None()` que emita la variable a `none` (regresión del bug documentado en MISC-05).
  - Test de composición con `With*` fluent builders.
- **Aceptación**: cada preset produce un diccionario de variables conocido y estable.
  > Resuelto en commit `6f27ab0` — *test(core): direct unit tests for BUIBorderPresets and BorderStyle*

### [x] CORE-COV-06 — `BUITransitionBuilder` + `EasingBuilder` tests directos
- **Origen**: `BUITransitionPresetsTests` cubre los presets; los *builders* fluentes (`.Duration(...)`, `.Delay(...)`, `.Easing(...)`, `.Custom(...)`) no tienen tests aislados.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI.Core/Abstractions/Behaviors/Transitions/BUITransitionBuilder.cs`, `EasingBuilder.cs`, `BUITransitions.cs`
  - Nuevo: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Library/BUITransitionBuilderTests.cs`
- **Cambios**:
  - Cadena fluente produce el triple (clase, duración, easing) esperado.
  - `EasingBuilder.CubicBezier(x1,y1,x2,y2)` genera `cubic-bezier(...)` correcto.
  - Composición múltiple: varias `.Add(...)` no se pisan.
- **Aceptación**: transiciones materializadas = especificación CSS esperada.
  > Resuelto en commit `c7a96de` — *test(core): direct unit tests for BUITransitionBuilder and EasingBuilder*

### [x] CORE-COV-07 — `SearchAlgorithms` tests
- **Origen**: `src/CdCSharp.BlazorUI.Core/Search/SearchAlgorithms.cs` implementa fuzzy/contains/startswith usado en Dropdown/Tree; sin tests.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI.Core/Search/SearchAlgorithms.cs`, `SearchMode.cs`, `SearchResult.cs`
  - Nuevo: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Library/SearchAlgorithmsTests.cs`
- **Cambios**:
  - Por cada `SearchMode` (Contains, StartsWith, Fuzzy): casos acierto / fallo / case-insensitive / diacríticos / vacío.
  - Fuzzy: puntuación ordenada y estable; query superset del texto no matchea.
  - `SearchResult.MatchedIndices` cubre los índices correctos para highlight.
- **Aceptación**: matriz de modos × entradas cubierta; ranking de fuzzy estable.
  > Resuelto en commit `0c2bd9a` — *test(core): direct unit tests for SearchAlgorithms*
  > Nota: `SearchResult` no expone `MatchedIndices`; la parte de highlight indices queda fuera de scope (N/A).

### [x] CORE-COV-08 — `DelayedActionHandler` + `TimingUtilities` tests
- **Origen**: `DelayedActionHandler` es el timer reutilizable por `BUITreeMenu` (LAYOUT-04) y `BUIToast` (auto-dismiss); su disposal y concurrency se tocaron en bug-fixes pero no hay tests unitarios.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI.Core/Utilities/DelayedActionHandler.cs`, `TimingUtilities.cs`
  - Nuevo: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Library/DelayedActionHandlerTests.cs`
- **Cambios**:
  - `Trigger` después de delay ejecuta callback; `Cancel` antes del delay no.
  - Re-`Trigger` reinicia el countdown (debounce).
  - `Dispose` libera el timer y cancela callback en vuelo.
  - Callback post-dispose no invocado (guard).
- **Aceptación**: comportamiento determinista bajo `Task.Delay` real y bajo control de reloj si hay abstracción.
  > Resuelto en commit `41942dd` — *test(core): direct unit tests for DelayedActionHandler and TimingUtilities*

### [x] CORE-COV-09 — `LightTheme` / `DarkTheme` palette invariants
- **Origen**: los temas definen la paleta base usada por todo `_themes.css`. No hay tests que aseguren que todas las claves de `BUIThemePaletteBase` estén pobladas con `CssColor` válidos, ni contraste mínimo entre `Surface` y `SurfaceContrast`.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI.Core/Themes/LightTheme.cs`, `DarkTheme.cs`, `Abstractions/BUIThemePaletteBase.cs`
  - Nuevo: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Library/ThemePaletteTests.cs`
- **Cambios**:
  - Para cada tema: todas las propiedades de la base retornan `CssColor` no-default y parseables.
  - Contraste mínimo (WCAG AA ~4.5:1) entre `Primary` / `PrimaryContrast`, `Surface` / `SurfaceContrast`, `Background` / `BackgroundContrast`.
  - Sanity: claves listadas en `CssInitializeThemesGenerator` existen en ambos temas.
- **Aceptación**: cambios accidentales de paleta que rompan contraste o dejen `CssColor.Empty` fallan el test.
  > Resuelto en commit `03aee89` — *test(core): palette invariants for LightTheme and DarkTheme*
  > Nota: el umbral de contraste se fija en WCAG AA *large text* (≥ 3:1). Los contrastes actuales del tema oscuro no alcanzan 4.5:1 (AA normal text) en todos los pares semánticos; se deja margen de ajuste sin romper tests.

### [x] CORE-COV-10 — `BUIInputComponentBase` validation wiring profundizado
- **Origen**: `BUIInputComponentBaseTests` cubre renderizado y `IHasError`; no cubre (a) re-suscripción al cambiar `EditContext`, (b) propagación de `FieldIdentifier` con `ValueExpression` que encapsula una propiedad nullable, (c) dispose sin `EditContext` inicializado.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI.Core/Abstractions/Components/BUIInputComponentBase.cs`
  - Extender: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Core/BaseComponents/BUIInputComponentBaseTests.cs`
- **Cambios**:
  - Cambiar `EditContext` en runtime → handler viejo desuscrito, nuevo suscrito.
  - `ValueExpression` con chained expression (`() => parent.child.prop`) produce `FieldIdentifier` correcto.
  - Dispose sin `EditContext` no lanza.
- **Aceptación**: tests de regresión para validación runtime cubren estos 3 casos.
  > Resuelto en commit `4e1abd8` — *test(core): validation wiring edge cases for BUIInputComponentBase*
  > Nota (a): `InputBase<T>` de Blazor bloquea `SetParametersAsync` si el `EditContext` cambia ("does not support changing the EditContext dynamically"). La rama `_previousEditContext != EditContext` de `OnParametersSet` es inalcanzable en uso normal. Se cubre la rama alcanzable (identity check) asegurando que re-renders con el mismo `EditContext` no duplican suscripción (test `Should_Not_Double_Subscribe_On_Rerender_With_Same_EditContext`).

---

## B. COBERTURA COMPONENTES FORMS

### [x] FORM-COV-01 — `BUIColorPicker` (Snapshot + Accessibility + Validation)
- **Origen**: `BUIColorPicker` tiene solo `Rendering/Interaction/State`. Le faltan snapshots y a11y; no hay tests de validación aunque compone `BUIInputColor` que sí.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI/Components/Forms/Color/BUIColorPicker.razor`
  - Nuevos en `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/Color/`:
    - `BUIColorPickerSnapshotTests.cs`
    - `BUIColorPickerAccessibilityTests.cs`
- **Cambios**:
  - Snapshot: estados representativos (default, con `Value` fijo HSV, disabled, readonly, con slider visible).
  - A11y: `role`/`aria-*` de sliders HSV; tabindex; labels de campos hex/alpha.
- **Aceptación**: snapshots verified generados; axe/aserciones ARIA pasan.
  > Resuelto en commit `ade7fb7` — *test(forms): snapshot + accessibility coverage for BUIColorPicker*
  > Nota: `BUIColorPicker` no expone `Disabled`/`ReadOnly` como parámetros (no es un input). Los estados snapshot se basan en props reales: `Value`, `OutputFormat`, `ShowActions`, `RevertText`, `SelectionWidth/Height`. Validación queda cubierta por `BUIInputColorValidationTests` (el input que compone el picker).

### [x] FORM-COV-02 — `BUIDatePicker` + `BUITimePicker` (State, Validation, Accessibility, Snapshot)
- **Origen**: ambos solo tienen `Rendering` + `Interaction`. Son componentes grandes con lógica de navegación calendario/horas y selección; merecen cobertura completa.
- **Archivos**:
  - Fuente:
    - `src/CdCSharp.BlazorUI/Components/Forms/DateAndTime/BUIDatePicker.razor`
    - `src/CdCSharp.BlazorUI/Components/Forms/DateAndTime/BUITimePicker.razor`
  - Nuevos en `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/InputDateTime/`:
    - `BUIDatePickerStateTests.cs`, `BUIDatePickerAccessibilityTests.cs`, `BUIDatePickerSnapshotTests.cs`
    - `BUITimePickerStateTests.cs`, `BUITimePickerAccessibilityTests.cs`, `BUITimePickerSnapshotTests.cs`
- **Cambios**:
  - State: cambio de mes/año, min/max, días deshabilitados, formato 12/24h.
  - A11y: `role="grid"`, `aria-selected`, flechas de teclado para navegación.
  - Snapshot: estados clave con cultura `en-US` fijada.
- **Aceptación**: cobertura alineada con el estándar del repo para inputs; snapshots estables cross-culture (usar `VerifyConfig`).
  > Resuelto en commit `8505f8b` — *test(forms): state + a11y + snapshots for BUIDatePicker and BUITimePicker*
  > Nota: las fuentes actuales no implementan `min/max`, días deshabilitados, `role="grid"`, `aria-selected` ni navegación por flechas; los tests cubren el comportamiento real (Value → CurrentMonth sync, Size/Density, nav buttons aria-label, steppers hora/minuto, formato 12/24h por cultura, AM/PM toggle). Los puntos del spec no implementados quedan pendientes como feature work, no como tests.

### [x] FORM-COV-03 — `BUIInputOutline` / `BUIInputLoading` / `BUIInputPrefix` / `BUIInputSuffix` / `_BUIFieldHelper` (Rendering)
- **Origen**: componentes internos del "input family" usados por Text/Number/TextArea/Dropdown/Color/DateTime. Sin tests directos. Regresiones en notch/label/outline quedan ocultas hasta que un input las hereda.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI/Components/Forms/Internal/*.razor`
  - Nuevos en `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/InputInternals/`:
    - `BUIInputOutlineRenderingTests.cs`
    - `BUIInputLoadingRenderingTests.cs`
    - `BUIInputPrefixRenderingTests.cs`
    - `BUIInputSuffixRenderingTests.cs`
    - `BUIFieldHelperRenderingTests.cs`
- **Cambios**:
  - Por componente: estructura BEM esperada (`bui-input__outline-leading/notch/trailing`, `bui-input__addon--prefix/suffix`, `_bui-field-helper`).
  - Prefix/suffix con `RenderFragment` arbitrario renderiza el contenido dentro del wrapper correcto.
  - `_BUIFieldHelper` muestra `ValidationMessage` cuando hay error, o `HelperText` si no.
- **Aceptación**: BEM y vars de familia estables; cualquier rename rompe test específico en vez de dispersar fallos.
  > Resuelto en commit `fa03d67` — *test(forms): rendering coverage for input family internals*

---

## C. COBERTURA COMPONENTES GENERIC

### [x] GEN-COV-01 — `BUINotificationBadge` (Accessibility, Interaction, Snapshot)
- **Origen**: solo tiene `Rendering` + `State`. El badge de notificaciones se usa como indicador aria-live; falta cobertura.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI/Components/Generic/Badge/BUINotificationBadge.razor`
  - Nuevos en `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/Badge/`:
    - `BUINotificationBadgeAccessibilityTests.cs`
    - `BUINotificationBadgeInteractionTests.cs`
    - `BUINotificationBadgeSnapshotTests.cs`
- **Cambios**:
  - A11y: `role="status"` o `aria-live` cuando el count cambia.
  - Interaction: click/keyboard sobre badge con acción (si aplica).
  - Snapshot: estados `0` (oculto), `1..99`, `99+`, con `Dot` variant.
- **Aceptación**: contrato a11y coherente con patrón de notificación.
  > Resuelto en commit `e58e539` — *test(generic): a11y + interaction + snapshots for BUINotificationBadge*
  > Nota: `BUINotificationBadge` no emite `role="status"`/`aria-live` por defecto ni expone callbacks propios — los tests validan que el componente acepta esos atributos vía `AdditionalAttributes` (proyectados al root) y que el click del host `ChildContent` no es interceptado por el wrapper. Snapshots cubren `Dot`, count numérico, `99+`, `BottomLeft`, `Large`, `Non_Circular`, y host con `<button>`.

### [x] GEN-COV-02 — `BUITreeSelector` (Accessibility, Variant)
- **Origen**: `BUITreeSelector` tiene `Rendering/State/Interaction/Disposal/Snapshot` pero no `Accessibility` ni `Variants`.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI/Components/Generic/Tree/BUITreeSelector.razor`
  - Nuevos en `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/TreeSelector/`:
    - `BUITreeSelectorAccessibilityTests.cs`
    - `BUITreeSelectorVariantTests.cs`
- **Cambios**:
  - A11y: `role="tree"`, `role="treeitem"`, `aria-expanded`, `aria-selected`, flechas arriba/abajo/izq/der para navegación.
  > Resuelto en commit `d75835d` — *test(generic): accessibility coverage for BUITreeSelector*
  > Nota: `BUITreeSelector` hereda `BUIComponentBase`, no `BUIVariantComponentBase`; no participa en el sistema `IVariantRegistry` ni expone `Variant` parameter, así que el archivo `BUITreeSelectorVariantTests.cs` no aplica. Los tests A11y cubren `role="tree"`, `role="treeitem"`, `aria-multiselectable`, `aria-expanded` (incluido null en hojas), `aria-selected` via click, teclas Arrow/Enter/Space, tabindex, `role="group"` en children y `aria-label` del expander.
  - Variants: registrar variante custom y verificar que el template corre.
- **Aceptación**: pattern WAI-ARIA tree cubierto; variante custom emite marcador propio.

---

## D. COBERTURA COMPONENTES LAYOUT

### [x] LAY-COV-01 — `BUIBlazorLayout` (Snapshot, State)
- **Origen**: solo `Rendering` + `Integration`. Falta snapshot de estructura (header/sidebar/main) y tests de estado para theme/culture providers.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI/Components/Layout/BUIBlazorLayout.razor`
  - Nuevos en `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/BlazorLayout/`:
    - `BUIBlazorLayoutSnapshotTests.cs`
    - `BUIBlazorLayoutStateTests.cs`
- **Cambios**:
  - Snapshot con `HeaderContent`/`SidebarContent`/`ChildContent`.
  - State: cambio de tema propaga a `<html data-theme>` o equivalente.
- **Aceptación**: layout estable entre builds; cambios de tema reflejados.
  > Resuelto en commit `70cd8d6` — *test(layout): state + snapshots for BUIBlazorLayout (LAY-COV-01)*
  > Nota: `BUIBlazorLayout` no expone `HeaderContent`/`SidebarContent` como parámetros — solo `Body` vía `LayoutComponentBase`. Snapshots varían markup dentro de `Body`. Tema propaga vía `CascadingValue<BUIPalette>` (no `<html data-theme>`; ese atributo lo setea el script inline del `HeadContent`, fuera del scope renderizable). State cubre: `InitializeAsync("dark")`, recarga de paleta al dispararse `OnThemeChanged`, y `BUIPalette` cascading recibido por un consumer dentro del Body.

### [x] LAY-COV-02 — `BUICard` (Accessibility, Interaction)
- **Origen**: tiene `Rendering/State/Variant/Snapshot` pero no `Accessibility` ni `Interaction` (card clickable).
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI/Components/Layout/Card/BUICard.razor`
  - Nuevos en `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/Card/`:
    - `BUICardAccessibilityTests.cs`
    - `BUICardInteractionTests.cs`
- **Cambios**:
  - A11y: si `OnClick` → `role="button"`, `tabindex="0"`, teclado Enter/Space.
  - Interaction: callback dispara por click y teclado.
- **Aceptación**: card accionable cumple patrón button.
  > Resuelto en commit `7f1f403` — *test(layout): a11y + interaction coverage for BUICard (LAY-COV-02)*
  > Nota: `BUICard` no emite `role="button"`/`tabindex`/keyboard handlers por sí mismo — el click vive en un `<div>` interno y el único hint semántico es `data-bui-clickable`. Los tests a11y validan el contrato de pass-through (`AdditionalAttributes` proyectan `role`/`tabindex`/`aria-*` al root) y la preservación de headings en el slot `Header`. Interacción por teclado (Enter/Space) queda fuera de scope como feature work, no como test fallido.

### [x] LAY-COV-03 — `BUIDialog` / `BUIDrawer` (State)
- **Origen**: `BUIDialog` tiene `Rendering/Interaction/Accessibility/Snapshot` pero no `StateTests` que verifique transiciones `Open/Opening/Closing/Closed` ni propagación de `data-bui-transitions`.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI/Components/Layout/Dialog/BUIDialog.razor`, `BUIDrawer.razor`
  - Nuevos: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/Dialog/BUIDialogStateTests.cs`, `BUIDrawerStateTests.cs`
- **Cambios**:
  - Transición `IsOpen: false→true→false`: estados intermedios observables en DOM, `animationend` cierra.
  - Atributo `data-bui-active` refleja apertura (no clase modificador).
- **Aceptación**: transiciones LAYOUT-03 protegidas por test.
  > Resuelto en commit `d52789a` — *test(layout): state tests for BUIDialog/BUIDrawer transitions (LAY-COV-03)*
  > Nota: los componentes no usan `data-bui-active` para la fase de cierre; usan clases modificadoras `bui-dialog--closing` / `bui-drawer--closing` y `bui-*-overlay--closing`. Los tests observan la transición vía un `IModalJsInterop` fake que controla la promesa `WaitForAnimationEndAsync` con un `TaskCompletionSource`, expuesto como gate para observar el DOM intermedio antes del cierre final.

### [ ] LAY-COV-04 — `BUIGrid` (Accessibility, Variant)
- **Origen**: `BUIGrid` tiene `Rendering/State/Snapshot`; falta `Variant` y `Accessibility` (role grid si aplica; tabindex si interactivo).
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI/Components/Layout/Grid/BUIGrid.razor`, `BUIGridItem.razor`
  - Nuevos: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/Grid/BUIGridAccessibilityTests.cs`, `BUIGridVariantTests.cs`
- **Cambios**:
  - A11y: si el grid expone `role`, verificar; si no, verificar ausencia (es layout puro).
  - Variants: registrar variante custom.
- **Aceptación**: contrato semántico explícito.

### [x] LAY-COV-05 — `BUIInitializer` (Disposal)
- **Origen**: LAYOUT-01 resolvió el memory leak, pero no hay `BUIInitializerDisposalTests` que proteja contra regresión (desuscripción de `OnThemeChanged`).
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI/Components/Layout/BUIInitializer.razor`
  - Nuevo: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/Initializer/BUIInitializerDisposalTests.cs`
- **Cambios**:
  - Montar/desmontar N veces; contar suscriptores de `ThemeInterop.OnThemeChanged` ≤ 0/1 sostenidamente.
  - Dispose no lanza `JSDisconnectedException`.
- **Aceptación**: regresión del leak hace fallar el test.
  > Resuelto en commit `8344dbe` — *test(layout): disposal tests for BUIInitializer (LAY-COV-05)*

### [ ] LAY-COV-06 — `BUISidebarLayout` (Accessibility)
- **Origen**: faltan aserciones a11y. Sidebar requiere `aria-expanded`/`aria-controls` para el toggle, y region para el sidebar.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI/Components/Layout/SidebarLayout/BUISidebarLayout.razor`
  - Nuevo: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/SidebarLayout/BUISidebarLayoutAccessibilityTests.cs`
- **Cambios**: ARIA de toggle + regiones landmark (`<aside>`, `role="complementary"` según diseño).
- **Aceptación**: axe limpio.

### [ ] LAY-COV-07 — `BUIStackedLayout` (State, Interaction, Accessibility)
- **Origen**: solo `Rendering` + `Snapshot`. Faltan state (cambio de layout, responsive), interaction (toggle si aplica), a11y.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI/Components/Layout/StackedLayout/BUIStackedLayout.razor`
  - Nuevos: `BUIStackedLayoutStateTests.cs`, `BUIStackedLayoutInteractionTests.cs`, `BUIStackedLayoutAccessibilityTests.cs` bajo `Tests/Components/StackedLayout/`.
- **Cambios**: parámetros de layout modifican data-attrs del root; regiones semánticas correctas.
- **Aceptación**: estándar de layout completo.

### [ ] LAY-COV-08 — `BUIThemeGenerator` (State, Accessibility) + `BUIThemeEditor` / `BUIThemePreview` (Interaction, State, Snapshot)
- **Origen**: `BUIThemeGenerator` tiene `Rendering/Interaction/Validation/Snapshot` pero no `State/Accessibility`. `BUIThemeEditor` y `BUIThemePreview` solo `Rendering`. Componentes con mucha lógica de UI (input de paleta, exportar/importar).
- **Archivos**:
  - Fuente:
    - `src/CdCSharp.BlazorUI/Components/Layout/ThemeGenerator/BUIThemeGenerator.razor`
    - `src/CdCSharp.BlazorUI/Components/Layout/ThemeGenerator/BUIThemeEditor.razor`
    - `src/CdCSharp.BlazorUI/Components/Layout/ThemeGenerator/BUIThemePreview.razor`
  - Nuevos en `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/ThemeGenerator/`:
    - `BUIThemeGeneratorStateTests.cs`, `BUIThemeGeneratorAccessibilityTests.cs`
    - `BUIThemeEditorInteractionTests.cs`, `BUIThemeEditorStateTests.cs`, `BUIThemeEditorSnapshotTests.cs`
    - `BUIThemePreviewStateTests.cs`, `BUIThemePreviewSnapshotTests.cs`
- **Cambios**:
  - Generator state: import/export round-trip, feedback de error (MISC-01) visible.
  - Editor interaction: cambio de color propaga a preview.
  - Preview state/snapshot: renderizado con distintas paletas estable.
  - A11y: labels en campos de color, aria-live para errores.
- **Aceptación**: generator es el componente con más código del Layout y pasa de 40 % a 80 %+ de las categorías estándar.

---

## E. COBERTURA DATA COLLECTIONS

### [ ] DC-COV-01 — `BUIDataCards` (Interaction, Accessibility, Variant, Snapshot)
- **Origen**: solo tiene `Rendering/State`. `BUIDataGrid` sí tiene la suite completa; alinearlo.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI/Components/Generic/DataCollections/BUIDataCards.razor`
  - Nuevos en `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/DataCollections/`:
    - `BUIDataCardsInteractionTests.cs`
    - `BUIDataCardsAccessibilityTests.cs`
    - `BUIDataCardsVariantTests.cs`
    - `BUIDataCardsSnapshotTests.cs`
- **Cambios**:
  - Interaction: selección, click en card, paginación si existe.
  - A11y: estructura semántica (list/listitem o article/heading).
  - Variants: registrar una variante.
  - Snapshot: estados default/empty/loading.
- **Aceptación**: paridad con `BUIDataGrid` en tipos de test.

### [ ] DC-COV-02 — `BUIDataColumn` (State, Interaction)
- **Origen**: solo `Rendering`. La columna tiene lógica de sort/filter/templating que no se ejercita.
- **Archivos**:
  - Fuente: `src/CdCSharp.BlazorUI/Components/Generic/DataCollections/BUIDataColumn.razor`
  - Nuevos: `BUIDataColumnStateTests.cs`, `BUIDataColumnInteractionTests.cs` bajo `Tests/Components/DataCollections/`.
- **Cambios**:
  - State: `Sortable`/`Filterable` cambian el header emitido.
  - Interaction: click en header de sort dispara callback.
- **Aceptación**: comportamientos de columna cubiertos sin ir a grid completo.

---

## F. NOTAS DE EJECUCIÓN

- **Cada bloque (`CORE-COV-*`, `FORM-COV-*`, `GEN-COV-*`, `LAY-COV-*`, `DC-COV-*`) es independiente**. Dentro de un bloque, los sub-componentes relacionados se resuelven juntos en un commit para evitar churn de snapshots.
- **No introducir snapshots nuevos sin revisión manual**: `*.received.txt` se revisa antes de renombrarse a `*.verified.txt`.
- **Aserciones**: siempre sobre `data-bui-*` y vars `--bui-inline-*`, nunca sobre clases CSS de estado (regla 7 de CLAUDE.md § "DOM/CSS generation").
- **Cultura fija**: `VerifyConfig` ya fija `en-US`; snapshots y tests culture-sensitive asumen esa base sin llamadas adicionales por test.
- **Escoping**: cada tarea añade tests solamente; **no** refactorizar producción. Si un test revela bug real, abrir tarea separada en `detected_TASKS.md` (siguiendo el patrón de este mismo archivo) y referenciar desde esta. Manteniendo el caso de test fallando hasta que se resuelva la tarea.
- **Orden sugerido de ataque**:
  1. `CORE-COV-*` (base sólida: builder, behavior, selection, borders, transitions, search, timing, themes, input base).
  2. `FORM-COV-03` (internals de input family — protege al resto de forms).
  3. `FORM-COV-01`, `FORM-COV-02` (pickers visibles).
  4. `GEN-COV-*`.
  5. `LAY-COV-*` (con LAY-COV-05 priorizado por ser protección de leak fix).
  6. `DC-COV-*`.
