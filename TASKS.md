# TASKS — Cobertura de Tests de Componentes

Objetivo: llevar la suite `test/CdCSharp.BlazorUI.Tests.Integration` a cobertura completa por componente, siguiendo el estándar establecido en `BUIButton` y documentado en `CLAUDE.md` (sección *Testing*).

Convenciones:
- **ID**: `[AREA]-NN`.
- **Estado**: `[ ]` pendiente, `[x]` hecho, `[~]` en curso, `[?]` requiere investigación previa.
- Cada tarea es **un archivo de test** con un único contexto (Rendering / State / Interaction / Variant / Accessibility / Snapshot / Validation / Integration).
- **Ubicación**: `test/CdCSharp.BlazorUI.Tests.Integration/Tests/Components/<ComponentName>/<Component><Context>Tests.cs`.
- **Siempre** `[Theory]` + `[MemberData(nameof(TestScenarios.All), ...)]` salvo indicación `OnlyServer` / `OnlyWasm`.

## Referencia rápida de contextos por componente

Todas las filas comparten el mismo set base. Marcar `—` cuando un contexto no aplique (p.ej. Variant en componentes sin variantes registradas).

| Contexto | Debe cubrir |
|---|---|
| Rendering | Root `bui-component`, `data-bui-component`, `data-bui-variant`, `data-bui-size`, `data-bui-*` iniciales, children estructurales. |
| State | Re-render tras cambio de props; volátiles (`Disabled`/`Loading`/`Error`/`ReadOnly`/`Required`/`FullWidth`/`Active`); preservación de `AdditionalAttributes`. |
| Interaction | Handlers (`OnClick`, `ValueChanged`, `OnInput`, teclado); gating por estado; `EventCallback`; consumer razor cuando haya estado padre. |
| Variant | Registro de variante custom vía `AddBlazorUIVariants` + aserción del marcador. |
| Accessibility | `role`, `aria-*`, foco/teclado, `tabindex`, `label` asociada. |
| Snapshot | `Verify` sobre estados representativos (Default/WithIcon/Loading/Disabled/Error/…). |
| Validation | *(solo inputs)* `EditContext` + `DataAnnotations`; propagación de `IsError`; `ValidationMessage`. |
| Integration | *(solo componentes compuestos)* Parent/child; cascading; registro de hijos. |

---

## A. ESTADO ACTUAL (baseline)

### [x] BASE-01 — `BUIButton` cubierto (estándar de referencia)
- Rendering, State, Interaction, Variant, Snapshot verdes.
- Accessibility comentado → ver `GEN-BUTTON-01`.

### [~] BASE-02 — `BUICultureSelector` (parcial)
- Tests actuales: `Server_BUICultureSelectorRenderingTests`, `Wasm_BUICultureSelectorRenderingTests` (dos clases por hosting).
- Pendiente: unificar convención — renombrar a `BUICultureSelectorRenderingTests` con `[MemberData(nameof(TestScenarios.All))]`, splitear hosting-specific solo si el DOM diverge. Cubre en `LAY-CULT-*`.

### [~] BASE-03 — `BUIInputDateTime` (parcial)
- Tests actuales: `BUIInputDateTimeInteractionTests`. Falta Rendering/State/Accessibility/Snapshot/Validation. Cubre en `FRM-DT-*`.

### [x] BASE-04 — `BUIComponentBase` / `BUIInputComponentBase` (Core)
- Tests en `Tests/Core/BaseComponents/`. Ampliar solo si se añade nueva interface `IHas*` o constante en `FeatureDefinitions`.

### [x] BASE-05 — `ServiceRegistrationTests` / `VariantRegistryTests` / `CssColorSystemTests` (Library)
- Mantener; ampliar al añadir extensiones de `AddBlazorUI*` o primitivas de color.

---

## B. FORMS — componentes de formulario

### BUIInputCheckbox (`Components/Forms/Checkbox/`)

- [x] **FRM-CHK-01** — `BUIInputCheckboxRenderingTests` (16 tests)
- [x] **FRM-CHK-02** — `BUIInputCheckboxStateTests` (16 tests)
- [x] **FRM-CHK-03** — `BUIInputCheckboxInteractionTests` (14 tests)
- [x] **FRM-CHK-04** — `BUIInputCheckboxVariantTests` (4 tests)
- [x] **FRM-CHK-05** — `BUIInputCheckboxAccessibilityTests` (18 tests)
- [x] **FRM-CHK-06** — `BUIInputCheckboxValidationTests` (10 tests)
- [x] **FRM-CHK-07** — `BUIInputCheckboxSnapshotTests` (4 tests, 6+1 states)

### BUIInputSwitch (`Components/Forms/Switch/`)

- [ ] **FRM-SW-01** — `BUIInputSwitchRenderingTests` — `data-bui-component="input-switch"` (regresión INPUT-01), `data-bui-active`, track/thumb vars públicas.
- [ ] **FRM-SW-02** — `BUIInputSwitchStateTests` — toggle `Value`, `Disabled`, `ReadOnly`, `Error`, custom track/thumb colors via `--bui-inline-track-*`.
- [ ] **FRM-SW-03** — `BUIInputSwitchInteractionTests` — `ValueChanged`, click, space.
- [ ] **FRM-SW-04** — `BUIInputSwitchVariantTests` — variante custom.
- [ ] **FRM-SW-05** — `BUIInputSwitchAccessibilityTests` — `role="switch"`, `aria-checked`, label, keyboard.
- [ ] **FRM-SW-06** — `BUIInputSwitchValidationTests` — propagación de `IsError` desde `EditContext`.
- [ ] **FRM-SW-07** — `BUIInputSwitchSnapshotTests` — Off, On, Disabled, Error, Custom colors.

### BUIInputRadio (`Components/Forms/Radio/`)

- [ ] **FRM-RD-01** — `BUIInputRadioRenderingTests` — `data-bui-component="input-radio"`, `data-bui-orientation` (regresión STD-05), opciones renderizadas, `data-bui-active` por opción seleccionada.
- [ ] **FRM-RD-02** — `BUIInputRadioStateTests` — cambio de `Value`; cambio de `Orientation`; `Disabled`, `ReadOnly`, `Error`; `Options` dinámicas.
- [ ] **FRM-RD-03** — `BUIInputRadioInteractionTests` — `ValueChanged`, flechas arriba/abajo/izq/der (según `Orientation`), `Home`/`End`.
- [ ] **FRM-RD-04** — `BUIInputRadioVariantTests` — `BUIInputRadioVariant`.
- [ ] **FRM-RD-05** — `BUIInputRadioAccessibilityTests` — `role="radiogroup"`/`role="radio"`, `aria-checked`, roving tabindex.
- [ ] **FRM-RD-06** — `BUIInputRadioValidationTests` — `[Required]` + sin selección.
- [ ] **FRM-RD-07** — `BUIInputRadioSnapshotTests` — vertical/horizontal, selected/none, disabled.

### BUIInputText (`Components/Forms/Text/`) — ✅ `e28ae4a`

- [x] **FRM-TXT-01** — `BUIInputTextRenderingTests` (14 tests)
- [x] **FRM-TXT-02** — `BUIInputTextStateTests` (16 tests)
- [x] **FRM-TXT-03** — `BUIInputTextInteractionTests` (14 tests)
- [x] **FRM-TXT-04** — `BUIInputTextVariantTests` (8 tests)
- [x] **FRM-TXT-05** — `BUIInputTextAccessibilityTests` (14 tests)
- [x] **FRM-TXT-06** — `BUIInputTextValidationTests` (14 tests)
- [x] **FRM-TXT-07** — `BUIInputTextSnapshotTests` (2 tests, 9 states each)

### BUIInputNumber (`Components/Forms/Number/`)

- [ ] **FRM-NUM-01** — `BUIInputNumberRenderingTests` — root, `data-bui-button-placement` (regresión STD-06), step buttons render.
- [ ] **FRM-NUM-02** — `BUIInputNumberStateTests` — `Min`, `Max`, `Step`, `Disabled`, `ReadOnly`, `Error`, `FullWidth`.
- [ ] **FRM-NUM-03** — `BUIInputNumberInteractionTests` — incremento/decremento por botones, flechas arriba/abajo, clamping a `Min`/`Max`, rechazo de entrada no-numérica.
- [ ] **FRM-NUM-04** — `BUIInputNumberVariantTests` — outlined/filled/standard.
- [ ] **FRM-NUM-05** — `BUIInputNumberAccessibilityTests` — `type="number"`, `aria-valuemin`/`valuemax`/`valuenow`.
- [ ] **FRM-NUM-06** — `BUIInputNumberValidationTests` — `[Range]`, `[Required]`.
- [ ] **FRM-NUM-07** — `BUIInputNumberSnapshotTests` — placements (start/end/split), con/sin error.

### BUIInputTextArea (`Components/Forms/TextArea/`)

- [ ] **FRM-TA-01** — `BUIInputTextAreaRenderingTests` — root, `data-bui-resize`, `data-bui-autoresize` (regresión STD-07), rows/cols.
- [ ] **FRM-TA-02** — `BUIInputTextAreaStateTests` — `Value`, `Disabled`, `ReadOnly`, `Error`, `Loading`, `AutoResize` flip.
- [ ] **FRM-TA-03** — `BUIInputTextAreaInteractionTests` — `onchange`, `oninput`, focus → `data-bui-floated`.
- [ ] **FRM-TA-04** — `BUIInputTextAreaVariantTests` — variantes.
- [ ] **FRM-TA-05** — `BUIInputTextAreaAccessibilityTests` — label, `aria-invalid`.
- [ ] **FRM-TA-06** — `BUIInputTextAreaValidationTests` — `[Required]`, `[StringLength]`.
- [ ] **FRM-TA-07** — `BUIInputTextAreaSnapshotTests` — fixed / autoresize / resize directions.

### BUIInputColor + BUIColorPicker (`Components/Forms/Color/`)

- [ ] **FRM-COL-01** — `BUIInputColorRenderingTests` — root, preview swatch, input value format.
- [ ] **FRM-COL-02** — `BUIInputColorStateTests` — `Value` (hex/rgb/hsl), `Format`, `DisplayMode`, `Disabled`.
- [ ] **FRM-COL-03** — `BUIInputColorInteractionTests` — cambio de valor → `ValueChanged`; apertura del picker.
- [ ] **FRM-COL-04** — `BUIColorPickerRenderingTests` — root, `data-bui-picker`, grid/slider/input subcomponents.
- [ ] **FRM-COL-05** — `BUIColorPickerStateTests` — cambio de formato, cambio de color seleccionado, `Disabled`/`ReadOnly`.
- [ ] **FRM-COL-06** — `BUIColorPickerInteractionTests` — click en cell, slider input, hex input.
- [ ] **FRM-COL-07** — `BUIInputColorValidationTests` — `[Required]`, formato inválido.
- [ ] **FRM-COL-08** — `BUIInputColorAccessibilityTests` — labels, `aria-label` en sliders.
- [ ] **FRM-COL-09** — `BUIInputColorSnapshotTests` — cada `DisplayMode` × formato.

### BUIInputDateTime + BUIDatePicker + BUITimePicker (`Components/Forms/DateAndTime/`)

- [ ] **FRM-DT-01** — `BUIInputDateTimeRenderingTests` — root, `data-bui-component="input-date-time"`, placeholder, mask.
- [ ] **FRM-DT-02** — `BUIInputDateTimeStateTests` — `Value`, `Min`, `Max`, `Disabled`, `ReadOnly`, `Format` (date/time/datetime).
- [x] **FRM-DT-03** — `BUIInputDateTimeInteractionTests` — ya existe; revisar cobertura 12h culture + picker open en readonly.
- [ ] **FRM-DT-04** — `BUIDatePickerRenderingTests` — grid, mes/año nav, cell selection.
- [ ] **FRM-DT-05** — `BUIDatePickerInteractionTests` — navegación meses, selección día, rango.
- [ ] **FRM-DT-06** — `BUITimePickerRenderingTests` — 12h/24h, hour/minute/second slots.
- [ ] **FRM-DT-07** — `BUITimePickerInteractionTests` — selección hora, AM/PM, incremento.
- [ ] **FRM-DT-08** — `BUIInputDateTimeAccessibilityTests` — `role="dialog"` al abrir, labels, teclado.
- [ ] **FRM-DT-09** — `BUIInputDateTimeValidationTests` — `[Required]`, rango fuera de `Min`/`Max`.
- [ ] **FRM-DT-10** — `BUIInputDateTimeSnapshotTests` — date / time / datetime × empty / filled / error.

### BUIInputDropdown + BUIInputDropdownTree + BUIDropdownContainer (`Components/Forms/Dropdown/`)

- [ ] **FRM-DD-01** — `BUIInputDropdownRenderingTests` — root, delegación a container (regresión STD-11), `data-bui-input-base` en container.
- [ ] **FRM-DD-02** — `BUIInputDropdownStateTests` — `Value`, `Options`, `Placeholder`, `Disabled`, `ReadOnly`, `Error`, `IsOpen` flip.
- [ ] **FRM-DD-03** — `BUIInputDropdownInteractionTests` — abrir/cerrar, selección de opción, flechas, Esc.
- [ ] **FRM-DD-04** — `BUIInputDropdownVariantTests` — outlined/filled/standard.
- [ ] **FRM-DD-05** — `BUIInputDropdownTreeRenderingTests` — tree dentro de dropdown.
- [ ] **FRM-DD-06** — `BUIInputDropdownTreeInteractionTests` — expand/collapse + selección.
- [ ] **FRM-DD-07** — `BUIInputDropdownAccessibilityTests` — `role="combobox"`/`listbox`, `aria-expanded`, `aria-activedescendant`.
- [ ] **FRM-DD-08** — `BUIInputDropdownValidationTests` — `[Required]`, `EditContext` reflejado en container.
- [ ] **FRM-DD-09** — `BUIInputDropdownSnapshotTests` — closed / open / with-selection / disabled.
- [ ] **FRM-DD-10** — `BUIInputDropdownIntegrationTests` — uso dentro de `EditForm`, validación + submit.

---

## C. GENERIC — componentes genéricos

### BUIButton (ya cubierto)

- [ ] **GEN-BUTTON-01** — `BUIButtonAccessibilityTests` (descomentar + adaptar al contrato actual). Cubrir `aria-disabled`, `aria-busy` (loading), `aria-label`, `role`, `tabindex`, `Enter`/`Space`.

### BUIBadge + BUINotificationBadge (`Components/Generic/Badge/`)

- [ ] **GEN-BADGE-01** — `BUIBadgeRenderingTests` — root, color, placement, shape.
- [ ] **GEN-BADGE-02** — `BUIBadgeStateTests` — cambio de content, visibility.
- [ ] **GEN-BADGE-03** — `BUIBadgeVariantTests` — `BUIBadgeVariant`.
- [ ] **GEN-BADGE-04** — `BUIBadgeAccessibilityTests` — `role="status"` o `aria-label` según semántica.
- [ ] **GEN-BADGE-05** — `BUINotificationBadgeRenderingTests` — count, overflow max, dot mode.
- [ ] **GEN-BADGE-06** — `BUINotificationBadgeStateTests` — incremento de count, reset a 0 (visibilidad).
- [ ] **GEN-BADGE-07** — `BUIBadgeSnapshotTests` — variantes + placements.

### BUICodeBlock (`Components/Generic/CodeBlock/`)

- [ ] **GEN-CODE-01** — `BUICodeBlockRenderingTests` — root, language attr, pre/code DOM, líneas numeradas si aplica.
- [ ] **GEN-CODE-02** — `BUICodeBlockStateTests` — cambio de `Code`/`Language`; toggle `ShowLineNumbers`.
- [ ] **GEN-CODE-03** — `BUICodeBlockInteractionTests` — botón copy → invoca `IClipboardJsInterop`, feedback temporizado vía CancellationToken.
- [ ] **GEN-CODE-04** — `BUICodeBlockAccessibilityTests` — `aria-label` en copy button, screen-reader-friendly feedback.
- [ ] **GEN-CODE-05** — `BUICodeBlockSnapshotTests` — cada lenguaje soportado × con/sin line numbers.
- [ ] **GEN-CODE-06** — `BUICodeBlockSecurityTests` — input XSS (`</pre><script>alert(1)</script>`) → renderiza escapado (regresión SEC-02).

### BUILoadingIndicator (`Components/Generic/Loading/`)

- [ ] **GEN-LOAD-01** — `BUILoadingIndicatorRenderingTests` — root, `role="img"`, `aria-label` (regresión A11Y-01), variantes (spinner/dots/linear).
- [ ] **GEN-LOAD-02** — `BUILoadingIndicatorStateTests` — cambio de variante, color, size, progreso (linear).
- [ ] **GEN-LOAD-03** — `BUILoadingIndicatorVariantTests` — variante custom.
- [ ] **GEN-LOAD-04** — `BUILoadingIndicatorAccessibilityTests` — `role="progressbar"` + `aria-valuenow/min/max` en linear determinate.
- [ ] **GEN-LOAD-05** — `BUILoadingIndicatorSnapshotTests` — cada variante × indeterminate/determinate.

### BUISvgIcon (`Components/Generic/Svg/`)

- [ ] **GEN-SVG-01** — `BUISvgIconRenderingTests` — root, catálogo conocido (`BUIIcons.*`), `viewBox`.
- [ ] **GEN-SVG-02** — `BUISvgIconStateTests` — cambio de `Icon`, color, size.
- [ ] **GEN-SVG-03** — `BUISvgIconVariantTests` — variante.
- [ ] **GEN-SVG-04** — `BUISvgIconSecurityTests` — `SvgMarkupSanitizer` elimina `<script>`, `on*`, `<foreignObject>` (regresión SEC-01).
- [ ] **GEN-SVG-05** — `BUISvgIconSnapshotTests` — 3–4 iconos representativos.

### BUISwitch (`Components/Generic/Switch/`)

- [ ] **GEN-SW-01** — `BUISwitchRenderingTests` — `data-bui-component="switch"`, `data-bui-active` (regresión STD-03), vars `--bui-inline-track-*`/`--bui-inline-thumb-*` (regresión STD-09).
- [ ] **GEN-SW-02** — `BUISwitchStateTests` — toggle, `Disabled`.
- [ ] **GEN-SW-03** — `BUISwitchInteractionTests` — click → toggle, space/enter.
- [ ] **GEN-SW-04** — `BUISwitchAccessibilityTests` — `role="switch"`, `aria-checked` (regresión A11Y-04).
- [ ] **GEN-SW-05** — `BUISwitchSnapshotTests` — off/on/disabled/custom colors.

### BUITabs + BUITab (`Components/Generic/Tabs/`)

- [ ] **GEN-TABS-01** — `BUITabsRenderingTests` — root, `role="tablist"`, tabs hijos con `role="tab"`, `data-bui-active="true"` en activo (regresión STD-01), `aria-selected`.
- [ ] **GEN-TABS-02** — `BUITabsStateTests` — cambio de tab activa, registro/desregistro de `BUITab` dinámicos.
- [ ] **GEN-TABS-03** — `BUITabsInteractionTests` — click en tab, flechas izq/der, `Home`/`End` (regresión A11Y-02).
- [ ] **GEN-TABS-04** — `BUITabsVariantTests` — `BUITabsVariant`.
- [ ] **GEN-TABS-05** — `BUITabsAccessibilityTests` — `aria-controls`, `aria-selected`, focus management, roving tabindex.
- [ ] **GEN-TABS-06** — `BUITabsSnapshotTests` — N tabs × tab activa distinta.
- [ ] **GEN-TABS-07** — `BUITabsDisposalTests` — dispose de múltiples tabs thread-safe (regresión DISP-04).

### BUITreeMenu (`Components/Generic/Tree/TreeMenu/`)

- [ ] **GEN-TM-01** — `BUITreeMenuRenderingTests` — root, `data-bui-disabled/active/expanded` en items (regresión STD-02), jerarquía.
- [ ] **GEN-TM-02** — `BUITreeMenuStateTests` — expand/collapse, selected item, `NavigationManager.LocationChanged` actualiza activo.
- [ ] **GEN-TM-03** — `BUITreeMenuInteractionTests` — click expand, hover (con timer de delay), click item, keyboard nav.
- [ ] **GEN-TM-04** — `BUITreeMenuAccessibilityTests` — `role="tree"`/`treeitem"`, `aria-expanded`, `aria-selected`.
- [ ] **GEN-TM-05** — `BUITreeMenuDisposalTests` — timer + nav subscription + disposal sin warnings (regresión LAYOUT-04).
- [ ] **GEN-TM-06** — `BUITreeMenuSnapshotTests` — árbol colapsado/expandido/con-selección.

### BUITreeSelector (`Components/Generic/Tree/TreeSelector/`)

- [ ] **GEN-TS-01** — `BUITreeSelectorRenderingTests` — root, checkboxes por nodo, estado tri-state.
- [ ] **GEN-TS-02** — `BUITreeSelectorStateTests` — selección parent propaga a children; children parciales marcan indeterminate en parent.
- [ ] **GEN-TS-03** — `BUITreeSelectorInteractionTests` — check/uncheck, expand.
- [ ] **GEN-TS-04** — `BUITreeSelectorDisposalTests` — `TreeNodeRegistry` liberado (regresión DISP-03).
- [ ] **GEN-TS-05** — `BUITreeSelectorSnapshotTests` — selección total/parcial/ninguna.

### DataCollections (`Components/Generic/DataCollections/`)

- [ ] **GEN-DC-01** — `BUIDataColumnRenderingTests` — registra en `DataColumnRegistry`, emite header correcto.
- [ ] **GEN-DC-02** — `BUIDataGridRenderingTests` — root `data-bui-data-collection`, header + rows, paginación opcional.
- [ ] **GEN-DC-03** — `BUIDataGridStateTests` — cambio de `Items`, sort, filter, page size.
- [ ] **GEN-DC-04** — `BUIDataGridInteractionTests` — click header → sort, paginación, selección de row.
- [ ] **GEN-DC-05** — `BUIDataGridAccessibilityTests` — `role="table"`, `role="row"`, `aria-sort`.
- [ ] **GEN-DC-06** — `BUIDataCardsRenderingTests` — render items como cards.
- [ ] **GEN-DC-07** — `BUIDataCardsStateTests` — cambio de items, layout.
- [ ] **GEN-DC-08** — `BUIDataCollectionIntegrationTests` — grid ↔ cards con mismos `BUIDataColumn`.
- [ ] **GEN-DC-09** — `BUIDataCollectionSnapshotTests` — grid vacío / con datos / sorted / paginated.

---

## D. LAYOUT — componentes de layout

### BUICard (`Components/Layout/Card/`)

- [ ] **LAY-CARD-01** — `BUICardRenderingTests` — root, slots (header/media/body/footer), `--_card-media-height` (regresión STD-08).
- [ ] **LAY-CARD-02** — `BUICardStateTests` — cambio de contenido, `Shadow`, `Variant`.
- [ ] **LAY-CARD-03** — `BUICardVariantTests` — `BUICardVariant`.
- [ ] **LAY-CARD-04** — `BUICardSnapshotTests` — variantes × con/sin media.

### BUIDialog + BUIDrawer + BUIModalHost + BUIModalContainer (`Components/Layout/Dialog/`)

- [ ] **LAY-DLG-01** — `BUIDialogRenderingTests` — root, open/closed, header/body/footer slots.
- [ ] **LAY-DLG-02** — `BUIDialogStateTests` — abrir/cerrar vía `IsOpen`, click backdrop, `CloseOnBackdrop` flag.
- [ ] **LAY-DLG-03** — `BUIDialogInteractionTests` — Esc cierra, botón close, callback `OnClose`.
- [ ] **LAY-DLG-04** — `BUIDialogAccessibilityTests` — `role="dialog"`, `aria-modal`, focus trap, restauración de focus (regresión A11Y-03).
- [ ] **LAY-DLG-05** — `BUIDrawerRenderingTests` — side (left/right/top/bottom), overlay.
- [ ] **LAY-DLG-06** — `BUIDrawerInteractionTests` — Esc, swipe/drag si aplica.
- [ ] **LAY-DLG-07** — `BUIModalHostRenderingTests` — host vacío; host con modal activo.
- [ ] **LAY-DLG-08** — `BUIModalHostInteractionTests` — servicio `ModalService.Show(...)` → host renderiza modal; `Close()` → limpia.
- [ ] **LAY-DLG-09** — `BUIModalContainerRenderingTests` — cambio de contenido, animation lifecycle basado en `animationend` (regresión LAYOUT-03).
- [ ] **LAY-DLG-10** — `BUIDialogSnapshotTests` — open / closed / with-header-footer / drawer variants.

### BUIGrid + BUIGridItem (`Components/Layout/Grid/`)

- [ ] **LAY-GRID-01** — `BUIGridRenderingTests` — root, `columns`, `rows`, `gap` → `--bui-inline-*`.
- [ ] **LAY-GRID-02** — `BUIGridItemRenderingTests` — `colspan`/`rowspan` vía CSS vars.
- [ ] **LAY-GRID-03** — `BUIGridStateTests` — cambio de template.
- [ ] **LAY-GRID-04** — `BUIGridSnapshotTests` — templates representativos.

### BUISidebarLayout (`Components/Layout/SidebarLayout/`)

- [ ] **LAY-SIDE-01** — `BUISidebarLayoutRenderingTests` — root, side (left/right), colapsado.
- [ ] **LAY-SIDE-02** — `BUISidebarLayoutStateTests` — toggle open/closed, `Side`.
- [ ] **LAY-SIDE-03** — `BUISidebarLayoutInteractionTests` — click toggle, ESC.
- [ ] **LAY-SIDE-04** — `BUISidebarLayoutSnapshotTests` — open/closed × side.

### BUIStackedLayout (`Components/Layout/StackedLayout/`)

- [ ] **LAY-STK-01** — `BUIStackedLayoutRenderingTests` — root, header/footer/content slots.
- [ ] **LAY-STK-02** — `BUIStackedLayoutSnapshotTests`.

### BUIThemeSelector (`Components/Layout/ThemeSelector/`)

- [ ] **LAY-THS-01** — `BUIThemeSelectorRenderingTests` — root, lista de temas del `IThemeService`.
- [ ] **LAY-THS-02** — `BUIThemeSelectorStateTests` — cambio de tema actual.
- [ ] **LAY-THS-03** — `BUIThemeSelectorInteractionTests` — click → `IThemeJsInterop.ApplyTheme`.
- [ ] **LAY-THS-04** — `BUIThemeSelectorVariantTests` — `BUIThemeSelectorVariant`.
- [ ] **LAY-THS-05** — `BUIThemeSelectorAccessibilityTests` — `role="radiogroup"` si radio-like, labels.
- [ ] **LAY-THS-06** — `BUIThemeSelectorSnapshotTests` — light/dark/custom.

### BUIThemeGenerator + BUIThemeEditor + BUIThemePreview (`Components/Layout/ThemeGenerator/`)

- [ ] **LAY-THG-01** — `BUIThemeGeneratorRenderingTests` — root, sub-slots editor + preview.
- [ ] **LAY-THG-02** — `BUIThemeGeneratorInteractionTests` — edición de color propaga a preview, export/import JSON.
- [ ] **LAY-THG-03** — `BUIThemeGeneratorValidationTests` — import de JSON inválido muestra mensaje (regresión MISC-01).
- [ ] **LAY-THG-04** — `BUIThemeEditorRenderingTests` — controles por paleta.
- [ ] **LAY-THG-05** — `BUIThemePreviewRenderingTests` — muestras de componentes con tema aplicado.
- [ ] **LAY-THG-06** — `BUIThemeGeneratorSnapshotTests` — estado inicial y tras edición.

### BUIToast + BUIToastHost (`Components/Layout/Toast/`)

- [ ] **LAY-TOA-01** — `BUIToastRenderingTests` — root, severity, icon, close button.
- [ ] **LAY-TOA-02** — `BUIToastStateTests` — auto-dismiss timer, pause on hover.
- [ ] **LAY-TOA-03** — `BUIToastInteractionTests` — close manual, callback `OnDismiss`.
- [ ] **LAY-TOA-04** — `BUIToastVariantTests` — `BUIToastVariant`.
- [ ] **LAY-TOA-05** — `BUIToastHostRenderingTests` — posiciones (top-left/top-right/...), vacío/con toasts.
- [ ] **LAY-TOA-06** — `BUIToastHostInteractionTests` — `IToastService.Show(...)` → host renderiza; dispose limpio (regresión LAYOUT-02).
- [ ] **LAY-TOA-07** — `BUIToastAccessibilityTests` — `role="alert"` o `role="status"`, `aria-live`.
- [ ] **LAY-TOA-08** — `BUIToastSnapshotTests` — severities × posiciones.

### BUIInitializer (`Components/Layout/BUIInitializer.razor`)

- [ ] **LAY-INIT-01** — `BUIInitializerRenderingTests` — monta sin markup visible (o minimal).
- [ ] **LAY-INIT-02** — `BUIInitializerInteractionTests` — suscripción a `IThemeJsInterop.OnThemeChanged` + desuscripción en dispose (regresión LAYOUT-01).

### BUIBlazorLayout (`Components/Layout/BUIBlazorLayout.razor`)

- [ ] **LAY-BL-01** — `BUIBlazorLayoutRenderingTests` — `@Body` renderizado, `BUIToastHost`/`BUIModalHost` incluidos.
- [ ] **LAY-BL-02** — `BUIBlazorLayoutIntegrationTests` — modal + toast simultáneos.

### BUICultureSelector (`Components/Layout/` — ya presente en tests)

- [ ] **LAY-CULT-01** — Refactor: fusionar `Server_BUICultureSelectorRenderingTests` + `Wasm_BUICultureSelectorRenderingTests` en `BUICultureSelectorRenderingTests` con `TestScenarios.All`.
- [ ] **LAY-CULT-02** — `BUICultureSelectorStateTests` — cambio de culture.
- [ ] **LAY-CULT-03** — `BUICultureSelectorInteractionTests` — selección → `ITestCultureService` recibe.
- [ ] **LAY-CULT-04** — `BUICultureSelectorAccessibilityTests`.
- [ ] **LAY-CULT-05** — `BUICultureSelectorSnapshotTests`.

---

## E. CORE — base components (ampliaciones)

### [ ] CORE-T-01 — `BUIComponentBaseTests` — cobertura de `PatchVolatileAttributes` post-CORE-01
- Verificar que cambios runtime en `Error`/`Loading`/`ReadOnly`/`Required`/`FullWidth` refrescan `data-bui-*` sin full rebuild.

### [ ] CORE-T-02 — `BUIComponentBaseTests` — `BuildComponentDataAttributes` reejecutado en re-render
- Regresión MISC-05.

### [ ] CORE-T-03 — `BUIInputComponentBaseTests` — `PatchVolatileAttributes` tras focus/blur
- Regresión STD-10 + MISC-05.

### [ ] CORE-T-04 — `BUIComponentAttributesBuilderTests` — cache de `IHas*` flags (regresión PERF-04)
- Construir N instancias del mismo tipo; verificar que reflection se evalúa una vez.

### [ ] CORE-T-05 — `FeatureDefinitionsTests` — ninguna constante huérfana
- Enumerar reflectivamente; asegurar que cada `DataAttributes.*` y `InlineVariables.*` aparece al menos en un generator o componente (evita dead constants).

---

## F. LIBRARY — cross-cutting (ampliaciones)

### [ ] LIB-01 — `CssColorSystemTests` — conversiones HSV↔RGB, mezcla `color-mix` fallback.

### [ ] LIB-02 — `ServiceRegistrationTests` — `AddBlazorUILocalizationServer`/`Wasm` expuestos y no duplicados.

### [ ] LIB-03 — `VariantRegistryTests` — multi-componente, sobreescritura, fallback a default.

### [ ] LIB-04 — `PaletteColorTests` — cada `PaletteColor` resuelve a `var(--palette-*)` correcto.

### [ ] LIB-05 — `BUIShadowPresetsTests` — `Elevation(n)` genera `--bui-inline-shadow` válido para cada n.

### [ ] LIB-06 — `BUITransitionPresetsTests` — `HoverLift`/etc emite `data-bui-transitions` + vars correctas.

---

## G. INFRAESTRUCTURA DE TESTS (soporte)

### [ ] INFRA-01 — Limpiar `<Compile Remove>` heredados en `.csproj`
- Entradas obsoletas de snapshot paths con el `BlazorScenario.ToString()` completo. Verificar que ya no aplican y eliminar.

### [ ] INFRA-02 — Helper `AssertBuiComponent(cut, kebabName)`
- Extensión en `ComponentTestExtensions` para reducir boilerplate `cut.Find("bui-component").GetAttribute("data-bui-component").Should().Be(...)`.

### [ ] INFRA-03 — Helper `RenderWithEditForm<TModel>(ctx, model, childBuilder)`
- Facilita tests de Validation sin crear un consumer razor por caso.

### [ ] INFRA-04 — Convención de nombres de snapshot
- Hoy se usa `.UseParameters(scenario.Name)` → `_scenario=Server.verified.txt`. Mantener y documentar si se añaden más dimensiones (culture, theme, etc.).

---

## Orden sugerido de ataque

1. **FRM-TXT-*** (Input text es la base de la familia; habilita pattern para Number/TextArea/Dropdown).
2. **FRM-CHK/SW/RD-*** (inputs simples; cubren STD-03/04/05).
3. **FRM-NUM/TA/DT/COL/DD-*** (inputs complejos).
4. **GEN-BUTTON-01** (descomentar Accessibility del estándar).
5. **GEN-BADGE/SVG/LOAD/SW/TABS-*** (genéricos simples).
6. **GEN-TM/TS/DC-*** (genéricos complejos).
7. **LAY-CARD/GRID/SIDE/STK-*** (layouts estáticos).
8. **LAY-DLG/TOA/THG/THS-*** (layouts con servicios y JS).
9. **LAY-INIT/BL/CULT-*** (cierre).
10. **CORE-T-\* + LIB-\*** (en paralelo, cuando se detecten gaps desde tests de componente).
11. **INFRA-\*** (refactor continuo conforme se repiten patrones).

## Notas

- Cada PR debe incluir también la regeneración de snapshots afectados (`*.verified.txt`).
- Si un test descubre un bug real en producción, abrir una entrada en `done_TASKS.md`-sibling (nuevo `TASKS_bugs.md` o sección nueva aquí) y dejar el test **rojo** hasta arreglar, no silenciarlo.
- Cuando un `IHas*` nuevo se añada al core, actualizar también `CORE-T-*` y `MISC-03` equivalent en memoria viva.
