# TASKS — Revisión Integral BlazorUI

Origen: revisión exhaustiva de `src/CdCSharp.BlazorUI.Core` + `src/CdCSharp.BlazorUI/Components/**` contra estándares de `CLAUDE.md` (arquitectura `<bui-component>`, interfaces `IHas*`, familias CSS, `FeatureDefinitions`). Hallazgos verificados en código; agrupados por severidad y área.

Convenciones:
- **ID**: `[AREA]-NN` — referencia estable.
- **Estado**: `[ ]` pendiente, `[x]` hecho, `[~]` en curso, `[?]` requiere investigación previa.
- Cada tarea incluye: *Origen* (por qué), *Archivos*, *Cambios*, *Aceptación*.

---

## A. BUGS CRÍTICOS

### [x] CORE-01 — PatchVolatileAttributes omite estados Error/Loading/ReadOnly
> Resuelto en commit `6bba694` — *CORE-01: extend PatchVolatileAttributes to cover Loading/Error/ReadOnly/Required/FullWidth*
- **Origen**: `BUIComponentAttributesBuilder.BuildStyles` aplica Error/Loading/ReadOnly a `ComputedAttributes`, pero `PatchVolatileAttributes` (llamado en cada render de `BUIComponentBase.BuildRenderTree`) solo re-parchea `Active` y `Disabled`. Mid-render, cambios de validación que no pasan por `OnParametersSet` pueden dejar el atributo stale (el `HandleValidationStateChanged` en `BUIInputComponentBase:128` mitiga llamando a `BuildStyles` completo, pero el contrato de `PatchVolatileAttributes` es incompleto).
- **Archivos**: `src/CdCSharp.BlazorUI.Core/Components/BUIComponentAttributesBuilder.cs:69-76`
- **Cambios**:
  - Añadir en `PatchVolatileAttributes` parches para `IHasError`, `IHasLoading`, `IHasReadOnly`, `IHasRequired`, `IHasFullWidth`.
  - Decidir si también `IVariantComponent.CurrentVariant` debe refrescarse aquí.
- **Aceptación**: cambio de `Error`/`Loading` en runtime sin re-llamar `BuildStyles` se refleja en los `data-bui-*` del DOM (test de integración con `Verify`).

### [x] INPUT-01 — `BUIInputSwitch` no aplica `ComputedAttributes` al root `<bui-component>`
> Resuelto en commit `b55707d` — *INPUT-01: wrap BUIInputSwitch variant template in its own bui-component root*
- **Origen**: `BUIInputSwitch.razor:50-68` propaga `@attributes="ComputedAttributes"` al componente hijo `<BUISwitch>`, que emite su propio `<bui-component data-bui-component="switch">`. Resultado: el `data-bui-component` queda como `switch` (no `input-switch`), y los data-attrs de estado del wrapper (IsError, IsLoading, IsReadOnly, floated, etc.) se pierden o se mezclan con los del hijo.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Forms/Switch/BUIInputSwitch.razor`
- **Cambios**:
  - Envolver el render en `<bui-component @attributes="ComputedAttributes"> ... </bui-component>`.
  - Pasar estado al hijo `<BUISwitch>` por parámetros explícitos, sin spread de atributos.
  - Revisar si el `.razor.css` del InputSwitch selecciona por `[data-bui-component="input-switch"]` o por `[data-bui-component="switch"]`; alinear tras el cambio.
- **Aceptación**: DOM renderizado muestra `<bui-component data-bui-component="input-switch" data-bui-error data-bui-loading ...>` envolviendo al switch interno.

### [x] INPUT-02 — `async Task` sin `await` en HandleChange (InputText / InputTextArea)
> Resuelto en commit `7401fb7` — *INPUT-02: drop spurious async from HandleChange in Text/TextArea*
- **Origen**: firmas `async Task HandleChange(...)` que solo asignan `CurrentValueAsString`. Genera warning y `Task` completado sincrónicamente con overhead innecesario.
- **Archivos**:
  - `src/CdCSharp.BlazorUI/Components/Forms/Text/BUIInputText.razor:123`
  - `src/CdCSharp.BlazorUI/Components/Forms/TextArea/BUIInputTextArea.razor:~157` (verificar línea)
- **Cambios**: cambiar firmas a `private void HandleChange(ChangeEventArgs e)`. Actualizar bindings `@onchange` (aceptan `EventCallback` y delegados no-async).
- **Aceptación**: compilación sin warning `CS1998`. Tests existentes de Text/TextArea pasan.

### [x] CORE-02 — JS dispose sin captura de `JSDisconnectedException`
> Resuelto en commit `c82fd1f` — *CORE-02: guard JS behavior dispose against disconnected circuit*
- **Origen**: `BUIComponentBase.DisposeAsync` (y equivalente en `BUIInputComponentBase:49-56`) llama a `_behaviorInstance.InvokeVoidAsync("dispose")` y `DisposeAsync()` sin try/catch. En Server, al cerrar circuit, `JSDisconnectedException` burbujea como excepción no observada y puede marcar el circuito como fallido.
- **Archivos**:
  - `src/CdCSharp.BlazorUI.Core/Abstractions/Components/BUIComponentBase.cs` (método `DisposeAsync`)
  - `src/CdCSharp.BlazorUI.Core/Abstractions/Components/BUIInputComponentBase.cs:49-56`
- **Cambios**: envolver en `try { ... } catch (JSDisconnectedException) { } catch (ObjectDisposedException) { }`. Buscar patrón ya usado en `ModuleJsInteropBase` y replicar.
- **Aceptación**: cerrar pestaña/circuit mientras componente está montado no produce excepciones en logs.

### [x] LAYOUT-01 — Memory leak en `BUIInitializer` (OnThemeChanged no desuscrito)
> Resuelto en commit `784c93d` — *LAYOUT-01/02: activate IDisposable on Initializer and guard Toast host race*
- **Origen**: suscribe a `ThemeInterop.OnThemeChanged` en `OnInitialized`/`OnAfterRenderAsync` pero no implementa `Dispose`/`IAsyncDisposable` para desuscribirse.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Layout/BUIInitializer.razor`
- **Cambios**: implementar `IDisposable` y desuscribir handler.
- **Aceptación**: navegación repetida que monta/desmonta `BUIInitializer` no incrementa handlers en `ThemeInterop.OnThemeChanged`.

### [x] LAYOUT-02 — Memory leak en `BUIToastHost` (ToastService.OnChange no desuscrito)
> Resuelto en commit `784c93d` — *LAYOUT-01/02: activate IDisposable on Initializer and guard Toast host race*. Nota: la desuscripción ya existía; el fix añade guard `_disposed` contra race de `InvokeAsync` post-dispose.
- **Origen**: suscripción a evento del servicio singleton sin cleanup; `InvokeAsync(StateHasChanged)` puede ejecutarse sobre instancia ya disposed.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Layout/Toast/BUIToastHost.razor`
- **Cambios**: implementar `IDisposable`, desuscribir `OnChange` en `Dispose`.
- **Aceptación**: unmount de host no deja handler colgado.

### [x] GENERIC-01 — `BUICodeBlock` `StateHasChanged` tras `Task.Delay` sin guard de disposal
> Resuelto en commit `c31c608` — *GENERIC-01: tie CodeBlock copy-feedback delay to component lifetime*
- **Origen**: `BUICodeBlock.razor:~74` — patrón `_copied=true; await Task.Delay(1500); StateHasChanged();`. Si usuario navega, llamada sobre componente disposed → warnings.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Generic/CodeBlock/BUICodeBlock.razor`
- **Cambios**: añadir `CancellationTokenSource` disposed en `Dispose`; pasar token a `Task.Delay`; comprobar token antes de `StateHasChanged`.
- **Aceptación**: no hay warnings "Cannot update component" tras navegación post-copy.

### [ ] LAYOUT-03 — `BUIDialog`/`BUIDrawer` usan `Task.Delay(150)` fijo para fin de animación
- **Origen**: detección de fin de transición por delay fijo. Frágil (varía con prefers-reduced-motion, `animation-duration` custom). Callback puede correr sobre instancia disposed.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Layout/Dialog/*`
- **Cambios**: sustituir por listener JS `animationend`/`transitionend` vía `IModalJsInterop`. Cleanup en dispose.
- **Aceptación**: ajuste de `animation-duration` via CSS var afecta tiempo real de cierre; reduced-motion no deja residuos.

### [ ] LAYOUT-04 — `BUITreeMenu` timer con callbacks pendientes post-dispose
- **Origen**: `_hoverDelayHandler` puede disparar tras `Dispose`. Suscripción a `NavigationManager.LocationChanged` se libera pero el timer no necesariamente.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Generic/Tree/BUITreeMenu.razor` (línea ~493)
- **Cambios**: `Dispose` llama a `_hoverDelayHandler?.Dispose()` antes de desuscribir NavManager; verificar que todas las mutaciones de estado comprueben `!IsDisposed`.
- **Aceptación**: tests unitarios con timer activo + dispose no fallan.

### [x] LAYOUT-05 — `BUIModalHost` usa `async void HandleModalChange`
> Resuelto en commit `239f404` — *LAYOUT-05: replace async void HandleModalChange with sync wrapper + async Task*
- **Origen**: `async void` traga excepciones y corre fuera del ciclo de render. Debe ser `async Task` invocado con `InvokeAsync` o handler síncrono.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Layout/Dialog/BUIModalHost.razor:~39`
- **Cambios**: cambiar firma a `private async Task HandleModalChange(...)` y suscribir con lambda que use `InvokeAsync`.
- **Aceptación**: excepciones en handler se propagan y no matan silenciosamente el evento.

---

## B. SEGURIDAD

### [x] SEC-01 — `BUISvgIcon` inyección de SVG arbitrario
> Resuelto en commit `f9bb2f0` — *SEC-01: sanitize BUISvgIcon markup before rendering*. Sanitizador mínimo (strip script/iframe/object/embed/foreignObject + on\* + javascript:). Conservador; no sustituye un HTML sanitizer completo.
- **Origen**: `@((MarkupString)Icon)` renderiza markup user-supplied sin sanear. Permite `<script>`, atributos `on*`, `<foreignObject>` con HTML.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Generic/Svg/BUISvgIcon.razor`
- **Cambios**:
  - Opción 1: exigir que `Icon` sea un enum/clave y resolver contra catálogo controlado (sin MarkupString).
  - Opción 2: pasar por sanitizador (`HtmlSanitizer` o similar) que permita solo whitelist de tags/atributos SVG seguros.
  - Documentar en XML-doc si se mantiene el pass-through sin sanitizar y se asume trust.
- **Aceptación**: entrada `"<svg><script>alert(1)</script></svg>"` no ejecuta script en sample.

### [x] SEC-02 — `BUICodeBlock` renderiza MarkupString de Highlighter sin validar
> Auditoría sin cambio de código. `HtmlRenderer.Render` en `src/CdCSharp.BlazorUI.SyntaxHighlight/Rendering/HtmlRenderer.cs:44-60` escapa `<`, `>`, `&`, `"`, `'` en cada token antes de concatenar. El fallback en `BUICodeBlock.HighlightedCode` usa `HttpUtility.HtmlEncode`. No se detectó vector XSS. Seguimiento opcional: añadir test de regresión cuando el proyecto de tests referencie `CdCSharp.BlazorUI.SyntaxHighlight`.
- **Origen**: confianza implícita en que `CdCSharp.BlazorUI.SyntaxHighlight` produce HTML seguro. Si la entrada contiene secuencias que el highlighter no escapa (edge cases), XSS posible.
- **Archivos**:
  - `src/CdCSharp.BlazorUI/Components/Generic/CodeBlock/BUICodeBlock.razor:~61`
  - `src/CdCSharp.BlazorUI.SyntaxHighlight/` (verificar escape de entidades)
- **Cambios**:
  - Auditar `Highlighter.Highlight` para garantizar escape de `<`, `>`, `&`, `"` en tokens.
  - Añadir test con entrada maliciosa `"</pre><script>alert(1)</script>"`.
- **Aceptación**: test de XSS rinde texto literal sin ejecutar.

---

## C. ESTÁNDAR (CLAUDE.md) — Estado via `data-bui-*`, no clases CSS

### [x] STD-01 — `BUITabs` usa clase CSS para tab activa
> Resuelto en commit `d86659c` — *STD-01: express active tab state via data-bui-active, not a CSS modifier class*
- **Origen**: `BUITabs.razor:~67` aplica `bui-tabs__tab--active` condicional en lugar de `data-bui-active`.
- **Archivos**:
  - `src/CdCSharp.BlazorUI/Components/Generic/Tabs/BUITabs.razor`
  - `src/CdCSharp.BlazorUI/Components/Generic/Tabs/BUITab.razor` (si aplica)
  - `*.razor.css` correspondientes
- **Cambios**:
  - Eliminar modificador `--active`; emitir `data-bui-active="@(tab.Id == ActiveTab)"`.
  - Selector CSS: `[data-bui-active="true"]`.
  - Añadir `aria-selected` coherente.
- **Aceptación**: DOM muestra `data-bui-active` en tab activa; CSS estiliza via atributo.

### [x] STD-02 — `BUITreeMenu` usa clases para disabled/active/expanded
- **Origen**: `BUITreeMenu.razor:240-244` aplica `bui-tree-menu__item--disabled/--active/--expanded`.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Generic/Tree/BUITreeMenu.razor` y `.razor.css`
- **Cambios**: migrar a `data-bui-disabled`, `data-bui-active`, y atributo nuevo `data-bui-expanded` (registrar en `FeatureDefinitions.DataAttributes` si aún no existe).
- **Aceptación**: CSS selecciona por atributos; no quedan clases modificador de estado en el HTML.

> Resuelto en commit `30e2f33` — *STD-02: express BUITreeMenu item state via data-bui-\* attributes*

### [x] STD-03 — `BUISwitch` emite `data-checked` fuera de `FeatureDefinitions`
- **Origen**: hardcoded `data-checked` en vez de usar `FeatureDefinitions.DataAttributes.Active`.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Generic/Switch/BUISwitch.razor:~164`
- **Cambios**: mapear semántica `checked` a `data-bui-active` (o registrar constante `Checked` en FeatureDefinitions si se prefiere separar semántica). Actualizar `.razor.css` del Switch y selectores dependientes.
- **Aceptación**: sin strings literales `data-checked` en C#/razor.

> Resuelto en commit `34c2eaf` — *STD-03: route BUISwitch checked state through data-bui-active*

### [x] STD-04 — `BUIInputCheckbox` `data-checked` / `data-indeterminate` sin prefijo `bui-`
- **Origen**: `BUIInputCheckbox.razor:99-105` emite `data-checked`/`data-indeterminate` directos. No siguen convención `data-bui-*` ni existen en `FeatureDefinitions`.
- **Archivos**:
  - `src/CdCSharp.BlazorUI/Components/Forms/Checkbox/BUIInputCheckbox.razor`
  - `src/CdCSharp.BlazorUI/Components/Forms/Checkbox/BUIInputCheckbox.razor.css`
  - `src/CdCSharp.BlazorUI.Core/Components/FeatureDefinitions.cs` (añadir constantes si se mantienen separadas de Active)
- **Cambios**: renombrar a `data-bui-checked` / `data-bui-indeterminate`, registrar constantes, actualizar selectores CSS.
- **Aceptación**: convención homogénea con el resto de componentes.

> Resuelto en commit `edf099a` — *STD-04: prefix BUIInputCheckbox state attributes with bui-*

### [x] STD-05 — `BUIInputRadio` `data-orientation` duplicado y sin prefijo
- **Origen**: `BUIInputRadio.razor:87-91` emite `data-orientation` (sin prefijo) vía `BuildComponentDataAttributes`, y `BUIInputRadio.razor:96` también lo renderiza inline en markup → duplicado.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Forms/Radio/BUIInputRadio.razor` y `.razor.css`
- **Cambios**:
  - Eliminar atributo inline (línea ~96).
  - Renombrar a `data-bui-orientation` en `BuildComponentDataAttributes`.
  - Registrar `FeatureDefinitions.DataAttributes.Orientation`.
  - Actualizar selector CSS.
- **Aceptación**: un solo atributo `data-bui-orientation` en DOM.

> Resuelto en commit `6e3237b` — *STD-05: dedupe and prefix BUIInputRadio orientation attribute*

### [x] STD-06 — `BUIInputNumber` `data-bui-button-placement` sin registrar
- **Origen**: hardcoded en `BUIInputNumber.razor:~153`.
- **Archivos**:
  - `src/CdCSharp.BlazorUI/Components/Forms/Number/BUIInputNumber.razor`
  - `src/CdCSharp.BlazorUI.Core/Components/FeatureDefinitions.cs`
- **Cambios**: añadir constante `FeatureDefinitions.DataAttributes.ButtonPlacement` (o más genérico si reusable), emitir vía `BuildComponentDataAttributes`.
- **Aceptación**: atributo proviene de constante centralizada.

> Resuelto en commit `93af7d0` — *STD-06: register ButtonPlacement attribute in FeatureDefinitions*

### [ ] STD-07 — `BUIInputTextArea` `data-bui-resize` / `data-bui-autoresize` sin registrar
- **Origen**: emitidos inline en markup; no existen en `FeatureDefinitions`.
- **Archivos**:
  - `src/CdCSharp.BlazorUI/Components/Forms/TextArea/BUIInputTextArea.razor:~104-105`
  - `FeatureDefinitions.cs`
- **Cambios**: registrar constantes, mover a `BuildComponentDataAttributes`. Considerar exponer interface `IHasResize` si patrón se repite.
- **Aceptación**: sin literales `data-bui-*` en markup razor.

### [ ] STD-08 — `BUICard` usa var `--bui-card-inline-media-height` fuera de convención
- **Origen**: convención es `--bui-inline-<prop>` (público, uniforme) o `--_<component>-<prop>` (privado). `--bui-card-inline-media-height` mezcla ambos.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Layout/Card/BUICard.razor` + `.razor.css`
- **Cambios**: decidir si la altura de media es pública (renombrar a `--bui-inline-media-height` y usar `IHas*` adecuado) o privada (`--_card-media-height`).
- **Aceptación**: convención alineada con el resto de componentes.

### [ ] STD-09 — `BUISwitch` private vars con prefijo no estándar
- **Origen**: `BUISwitch.razor:171-178` usa `--track-inline-inactive-bg`, `--thumb-inline-active-bg`, `--thumb-inline-inactive-color`. Convención: `--bui-inline-*` para vars públicas y `--_switch-*` para privadas.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Generic/Switch/BUISwitch.razor` + `.razor.css`
- **Cambios**: renombrar:
  - Públicas (overridables): `--bui-inline-track-active`, `--bui-inline-track-inactive`, `--bui-inline-thumb-active`, `--bui-inline-thumb-inactive`.
  - Privadas internas: `--_switch-track-active`, etc., resolviendo `var(--bui-inline-*, default)`.
  - Si se mantiene parametrización por propiedades (`TrackColorInactive`, etc.), registrar en `BuildComponentCssVariables` contra las constantes públicas.
- **Aceptación**: patrón privado/público coherente con `BUIButton.razor.css`.

### [ ] STD-10 — `data-bui-floated` emitido inline en markup (Text/Number/TextArea/Color/DateTime)
- **Origen**: cada input mantiene su `_isFocused`/`_isDirty` y escribe `data-bui-floated="@IsFloated.ToString().ToLowerInvariant()"` en el razor. Debe fluir por `BuildComponentDataAttributes`.
- **Archivos**:
  - `src/CdCSharp.BlazorUI/Components/Forms/Text/BUIInputText.razor`
  - `.../Number/BUIInputNumber.razor`
  - `.../TextArea/BUIInputTextArea.razor`
  - `.../Color/BUIColorPicker.razor` (si aplica)
  - `.../DateAndTime/*`
- **Cambios**:
  - Registrar `FeatureDefinitions.DataAttributes.Floated` (si no existe).
  - Override `BuildComponentDataAttributes` por componente que lea su estado interno.
  - Llamar a `_styleBuilder.PatchVolatileAttributes`/`BuildStyles` (según alcance de CORE-01) al cambiar focus/dirty, o añadir `Floated` a volátiles.
- **Aceptación**: sin literales `data-bui-floated` en razor; atributo cambia con focus/blur en DOM.

### [ ] STD-11 — `BUIInputDropdown` no hereda `BUIInputComponentBase`
- **Origen**: `BUIInputDropdown.razor` hereda `ComponentBase` implícito y compone `BUIDropdownContainer` (que sí hereda la base). Decisión deliberada pero causa inconsistencia: `Value`, `ValueExpression`, validación, `IsError` se gestionan en contenedor, no en el input público; API divergente con el resto.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Forms/Dropdown/BUIInputDropdown.razor` (+ code-behind)
- **Cambios** (decisión previa):
  - Opción A: mantener composición, documentar la razón en CLAUDE.md y añadir delegación de `IsError`/`IsDisabled`/etc. para consumidores.
  - Opción B: refactor para que `BUIInputDropdown` herede `BUIInputComponentBase<TValue, BUIInputDropdown<TValue>, BUIInputVariant>` y `BUIDropdownContainer` sea detalle interno no-input.
- **Aceptación**: API pública coherente — validaciones `EditContext`, `IHas*`, y DOM root consistentes entre inputs.

---

## D. DISPOSAL / PRERENDER

### [ ] DISP-01 — `BUIInputTextArea` JS interop sin guard prerender
- **Origen**: `OnAfterRenderAsync` invoca JS sin comprobar estado de servidor/prerender. En SSR estático puede fallar.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Forms/TextArea/BUIInputTextArea.razor:~89-97`
- **Cambios**: verificar patrón del resto del repo (posiblemente `IsRenderingOnServer` o `OperatingSystem.IsBrowser()` según modelo) y aplicar guard. Usar `firstRender` + try/catch `InvalidOperationException`.
- **Aceptación**: render bajo prerender server no lanza.

### [ ] DISP-02 — `BUIColorPicker` `UpdateHandlerPosition` sin guard
- **Origen**: análogo a DISP-01 en `BUIColorPicker.razor:~113-115`.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Forms/Color/BUIColorPicker.razor`
- **Cambios**: mismo patrón de guard.
- **Aceptación**: idem.

### [ ] DISP-03 — `BUITreeSelector` sin `IDisposable`
- **Origen**: crea `TreeNodeRegistry` y posibles suscripciones; no libera.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Generic/Tree/BUITreeSelector.razor` (+ code-behind)
- **Cambios**: implementar `IAsyncDisposable`, limpiar registry y handlers.
- **Aceptación**: test de unmount no retiene referencias.

### [ ] DISP-04 — `BUITab` disposal síncrono con efecto lateral en padre
- **Origen**: `Dispose()` desregistra del padre (`BUITabs`); si el padre re-renderiza durante dispose del hijo, posible race.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Generic/Tabs/BUITab.razor(.cs)`, `BUITabs.razor(.cs)`
- **Cambios**: hacer `UnregisterTab` thread-safe (lock o snapshot de colección) y no invocar `StateHasChanged` síncrono durante disposal; usar `InvokeAsync`.
- **Aceptación**: tests de dispose de múltiples tabs no fallan.

---

## E. PERFORMANCE

### [ ] PERF-01 — `BUIComponentAttributesBuilder` recrea diccionarios por render
- **Origen**: `BuildStyles` asigna `ComputedAttributes = new Dictionary<...>(...)` cada llamada; también crea `cssVariables` local. Alto churn GC con muchos componentes.
- **Archivos**: `src/CdCSharp.BlazorUI.Core/Components/BUIComponentAttributesBuilder.cs:12-67`
- **Cambios**:
  - Reutilizar `ComputedAttributes`: `ComputedAttributes.Clear()` en vez de `new`.
  - Reutilizar un `Dictionary<string,string>` de instancia para `cssVariables` (campo privado + Clear).
  - Medir impacto con benchmark simple.
- **Aceptación**: sin regresión funcional (tests pasan). Perfil indica menos allocations en hot path de render.

### [ ] PERF-02 — `BuildInlineStyles` concatena con LINQ
- **Origen**: `string.Join("; ", cssVariables.Select(kv => $"{kv.Key}: {kv.Value}"))` — `StringBuilder` sería más eficiente para N>4 vars.
- **Archivos**: `src/CdCSharp.BlazorUI.Core/Components/BUIComponentAttributesBuilder.cs:221`
- **Cambios**: construir con `StringBuilder` reutilizable (campo), cuidado con separador y orden estable.
- **Aceptación**: mismo output; `Verify` snapshots inalterados.

### [ ] PERF-03 — `ColorClassGenerator` reflection no cacheada
- **Origen**: `typeof(Color).GetProperties(...)` cada ejecución del source generator. Aunque se ejecuta en compile-time, ralentiza builds incrementales.
- **Archivos**: `src/CdCSharp.BlazorUI.Core.CodeGeneration/ColorClassGenerator.cs:~88`
- **Cambios**: memoizar resultado en `static readonly` si el generator lo permite (ojo: source generators deben ser deterministas y stateless idealmente).
- **Aceptación**: tiempo de build no degrada; output idéntico.

### [ ] PERF-04 — Reflection IHas* en cada render
- **Origen**: `BuildStyles` chequea `component is IHas*` ~15 veces cada render. Patrón matching sobre interfaces es rápido, pero acumulado en arbol grande pesa. Opción: cachear flags por tipo.
- **Archivos**: `BUIComponentAttributesBuilder.cs`
- **Cambios**: `ConcurrentDictionary<Type, InterfaceFlags>` precalculada en primera instancia; iterar solo las ramas que el tipo implementa.
- **Aceptación**: benchmark muestra mejora en árboles con cientos de componentes; tests pasan.

---

## F. ACCESIBILIDAD

### [ ] A11Y-01 — `BUILoadingIndicator` sin role/aria
- **Origen**: SVG sin `role="img"` ni `aria-label`; variante lineal sin ARIA live region.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Generic/Loading/*`
- **Cambios**:
  - Añadir `role="img" aria-label="@AriaLabel ?? DefaultLoadingLabel"`.
  - Para variante lineal con progreso, envolver en `<div role="progressbar" aria-valuenow="..." aria-valuemin="0" aria-valuemax="100">`.
- **Aceptación**: lighthouse/axe no reporta missing label.

### [ ] A11Y-02 — `BUITabs` navegación por teclado y ARIA
- **Origen**: verificar implementación de `role="tablist"`, `role="tab"`, `aria-selected`, `aria-controls`, flechas izquierda/derecha, `Home`/`End`.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Generic/Tabs/BUITabs.razor`, `BUITab.razor`
- **Cambios**: implementar patrón WAI-ARIA Authoring Practices para tabs.
- **Aceptación**: test manual con teclado + screen reader.

### [ ] A11Y-03 — `BUIDialog` focus trap y ESC
- **Origen**: revisar si modal atrapa focus y cierra con ESC; requerido por WCAG 2.1.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Layout/Dialog/*`, `src/CdCSharp.BlazorUI/Services/JsInterop/IModalJsInterop.cs`
- **Cambios**: asegurar focus trap via JS, handler ESC, restaurar focus al elemento previo al cerrar.
- **Aceptación**: abrir dialog con teclado, tab ciclo dentro, ESC cierra, focus vuelve al botón origen.

### [ ] A11Y-04 — `BUIInputSwitch`/`BUISwitch` labels y ARIA
- **Origen**: revisar `aria-checked`, `role="switch"`, asociación label-input.
- **Archivos**: Switch components.
- **Cambios**: aplicar patrón WAI-ARIA switch.
- **Aceptación**: axe no reporta missing-label.

---

## G. OTROS

### [ ] MISC-01 — `BUIThemeGenerator` silencia excepciones en import
- **Origen**: `catch { }` vacío oculta errores de parseo de colores al importar paleta.
- **Archivos**: `src/CdCSharp.BlazorUI/Components/Layout/ThemeGenerator/*:~145-189`
- **Cambios**: capturar a variable `_importError` y renderizar feedback; o loguear vía `ILogger`.
- **Aceptación**: import de JSON inválido muestra mensaje al usuario.

### [ ] MISC-02 — Revisar llamadas JS con `ConfigureAwait(false)` server-side
- **Origen**: `BUIComponentJsBehaviorBuilder:38` y otras. En Blazor, el contexto de síntesis es importante; `ConfigureAwait(false)` puede saltar el sync context de Server. Validar política en el repo (probablemente dejar tal cual).
- **Archivos**: varios `Services/JsInterop/*`
- **Cambios**: definir convención en CLAUDE.md si no está, aplicar uniformemente.
- **Aceptación**: convención documentada; código coherente.

### [ ] MISC-03 — `FeatureDefinitions` auditoría de cobertura
- **Origen**: hallazgos STD-04/05/06/07/10 indican atributos faltantes. Auditoría completa de qué atributos están en `FeatureDefinitions.DataAttributes` vs cuáles se usan directamente en razor/C#.
- **Archivos**:
  - `src/CdCSharp.BlazorUI.Core/Components/FeatureDefinitions.cs`
  - búsqueda global de literales `"data-bui-*"` y `"--bui-*"` en `src/CdCSharp.BlazorUI/**`
- **Cambios**: registrar constantes faltantes, sustituir literales.
- **Aceptación**: cero literales `data-bui-*`/`--bui-inline-*` fuera de `FeatureDefinitions`, generators y `.razor.css`.

### [ ] MISC-04 — Snapshot tests cubrir cambios de contrato DOM
- **Origen**: tras INPUT-01, STD-*, CORE-01 el DOM emitido cambia. Los snapshots `Verify` deben regenerarse y revisarse manualmente para confirmar que el cambio es intencional.
- **Archivos**: `test/CdCSharp.BlazorUI.Tests.Integration/**/*.verified.*`
- **Cambios**: regenerar y revisar en PR separado o junto al cambio funcional.
- **Aceptación**: diff de snapshots documentado en commit/PR.

---

## Notas

- **CORE-01 y STD-10 están acoplados**: añadir `Floated` a volátiles (CORE-01) facilita implementar STD-10 limpiamente.
- **STD-* comparten patrón**: crear primero las constantes en `FeatureDefinitions`, luego migrar componentes uno a uno.
- **Orden sugerido de ataque**:
  1. CORE-01, CORE-02 (base sólida para resto).
  2. INPUT-01, INPUT-02 (bugs funcionales visibles).
  3. SEC-01, SEC-02.
  4. STD-01..11 por bloques (tabs → switch/checkbox → radio/number/textarea → card → dropdown → floated).
  5. DISP-01..04 + LAYOUT-01..05.
  6. PERF-*.
  7. A11Y-*.
  8. MISC-*.

- **Claims descartados durante verificación** (no son bugs reales):
  - `BUIComponentJsBehaviorBuilder.cs:30` lógica OR — es guard-clause correcto.
  - `BUIInputComponentBase.cs:34` `field || Error` — uso válido de C# 13 `field` keyword.
  - `BUIInputDropdown` herencia — es composición deliberada; reclasificado como inconsistencia API (STD-11) no bug.
