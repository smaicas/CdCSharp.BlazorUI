# ANALYSIS.md — Plan de análisis pre-producción de `CdCSharp.BlazorUI`

Documento maestro que describe el proceso de revisión exhaustiva de la solución antes del lanzamiento a producción. Está diseñado para ejecutarse **de forma incremental y ordenada**, sin saltarse rincones del código.

> **Regla operativa**: cada sub‑análisis de este documento NO modifica código. Su único resultado es **producir entradas en `TASKS.md`** con los hallazgos, categorizados y priorizados. El trabajo real (fixes, refactors, optimizaciones) se ejecuta a partir de `TASKS.md` en una fase posterior.

---

## 0. Objetivos de la revisión

Detectar y catalogar, en toda la solución:

1. **Incongruencias** de código (patrones divergentes entre componentes equivalentes, nombres inconsistentes, convenciones rotas).
2. **Errores** latentes (bugs, race conditions, null/disposed, async sin captura de contexto, leaks, regressions).
3. **Mejoras** de diseño (APIs públicas, ergonomía, extensibilidad, legibilidad).
4. **Optimizaciones** (render performance, allocations, bundle size, JS interop, CSS payload).
5. **Cumplimiento de estándares** del propio proyecto (contrato `BUIComponentBase`, reglas 1‑10 del CSS architecture, convenciones de test) y estándares externos (a11y WCAG 2.2 AA, NuGet, semver, Roslyn analyzers, .NET naming).
6. **Consolidación CSS** (duplicación entre `CssBundle/*.css` y `*.razor.css`, reglas muertas, `!important` evitables, `--bui-inline-*` huérfanos, fallbacks inconsistentes).
7. **Optimización CSS** (colisiones de especificidad, selectores costosos, variables no resueltas, transiciones redundantes, tokens no utilizados).

---

## 1. Metodología

### 1.1. Fases

| Fase | Nombre | Salida |
|------|--------|--------|
| **F0** | Planificación | `ANALYSIS.md` (este documento) |
| **F1** | Ejecución del análisis | Entradas en `TASKS.md` — una por hallazgo |
| **F2** | Resolución | Commits que cierran tareas de `TASKS.md` |
| **F3** | Verificación final | Re-ejecución de los sub‑análisis críticos sobre la base ya saneada |

### 1.2. Convenciones para `TASKS.md`

Cada entrada generada por este plan debe cumplir:

- **ID estable** con prefijo del área: `ARCH-xx`, `BLD-xx`, `GEN-xx`, `API-xx`, `BASE-xx`, `COMP-xx`, `CSS-SCOPED-xx`, `CSS-BUNDLE-xx`, `CSS-OPT-xx`, `THEME-xx`, `JS-xx`, `ASYNC-xx`, `A11Y-xx`, `PERF-xx`, `SEC-xx`, `TEST-xx`, `DOC-xx`, `PKG-xx`, `L10N-xx`, `CI-xx`, `DOCS-WASM-xx`, `CLAUDE-xx`, `REL-xx`.
- **Título** accionable (verbo + objeto): *"Unificar fallback de `--bui-inline-color` en `_input-family.css`"*.
- **Severidad**: `Blocker` / `Critical` / `Major` / `Minor` / `Polish`.
- **Esfuerzo**: `XS` (<30 min), `S` (<2h), `M` (<1d), `L` (1‑3d), `XL` (>3d).
- **Alcance** (archivos o patrones) y **Criterios de aceptación** (qué tiene que ser cierto para cerrarla).
- **Evidencia**: ruta y nº de línea, o comando reproducible.

Secciones de `TASKS.md`:
```
## Blockers (release-gating)
## Critical
## Major
## Minor
## Polish / opcionales
```

### 1.3. Principios de decisión

- **No ampliar alcance**: si durante un sub‑análisis se detecta algo fuera de su área, se registra como task pero no se profundiza en este pase.
- **Seguir el contrato de `CLAUDE.md`** como fuente de verdad del estándar interno. Cualquier desviación encontrada es automáticamente una `TASKS` de severidad ≥ `Major`.
- **No confundir estilo con corrección**: preferencias personales ≠ hallazgos. Si no hay regla escrita que respalde el cambio, la tarea va a `Polish`.

### 1.4. Marcado de progreso

Conforme avance la ejecución, cada sub‑análisis se **marca como finalizado** actualizando la **tabla agregada** de §4.1 — fuente única de verdad del progreso.

Valores permitidos en la columna `Estado`:

- `[ ] Pendiente` — aún no iniciado.
- `[~] En curso` — iniciado, con al menos un hallazgo o bloqueo registrado.
- `[x] Completado` — todos los pasos del sub‑análisis ejecutados y las tareas derivadas volcadas en `TASKS.md`.
- `[!] Bloqueado` — detenido por dependencia o decisión pendiente.
- `[-] N/A` — descartado con justificación.

**Reglas de actualización**:
1. El estado se cambia **en el mismo turno** en que se avanza el sub‑análisis, nunca en batch al final.
2. Al pasar a `[x]`, rellenar `Tareas` con el nº creadas y `Resumen` con la distribución de severidades (ej. `1 Critical, 3 Major`).
3. Al pasar a `[!]`, poner en `Resumen / Bloqueo` el motivo y qué lo desbloquea.
4. `ANALYSIS.md` se considera **vivo** únicamente para actualizar esta tabla y corregir erratas del plan — no se re‑edita el contenido de los pasos.

### 1.5. Baseline y métricas antes/después

Antes de abrir F1 se capturan y se commitean en `TASKS.md` (sección "Baseline") las métricas de partida, que son las que §3.24 `REL` usará para demostrar mejora:

- `dotnet build -c Release` → nº warnings por proyecto.
- `dotnet test` → nº tests, nº skipped, tiempo total.
- `Test-Coverage.ps1` → % cobertura líneas / ramas por proyecto.
- Tamaño de `wwwroot/css` y `wwwroot/js` tras build Release (KB por archivo).
- LoC de CSS por archivo en `CssBundle/` y `.razor.css`.
- Nº de componentes públicos (ref. §2).
- Salida de los `rg` críticos de §5 (counts).

Las mismas métricas se recapturan al final de F2 y se comparan en el bloque "Mejora" de `TASKS.md`.

### 1.6. Rama y estrategia de commits para F2

- Trabajo ejecutado en rama **`release-prep/1.0`** derivada de `master`.
- Un commit por tarea `TASKS.md` (o grupo cohesivo), con subject `<ID>: <verbo corto>` y body describiendo evidencia.
- Los cambios a `CLAUDE.md` (de §3.23) van en un **único commit final** `docs(claude): refresh standards for 1.0` — último antes del tag.
- Sin squashes que oculten el rastro `ID ↔ commit`.

---

## 2. Inventario del área bajo análisis

Mapa objetivo (para referencia rápida durante la ejecución):

```
src/
  CdCSharp.BlazorUI.Core/               # primitivas, abstracciones, IHas*, theming, CssColor
    Abstractions/
      Behaviors/{Design,State,Javascript,Transitions,Families}/
      JSInterop/
      Services/
    Components/                          # BUIComponentBase, AttributesBuilder, FeatureDefinitions
    Css/  Diagnostics/  Media/  Search/  Themes/  Utilities/
  CdCSharp.BlazorUI/                     # componentes públicos
    Components/{Forms,Generic,Layout,Debug,Diagnostics,Internal,Utils}/
    Services/{JsInterop/, VariantRegistry}
    Types/                               # TypeScript por feature (Behaviors, Dropdown, Modal, …)
    CssBundle/                           # CSS generado (source of truth ≠ disco)
    wwwroot/{css,js}                     # salida bundle Vite
  CdCSharp.BlazorUI.BuildTools/          # host MSBuild → genera CssBundle/*.css y package.json/vite/tsconfig
    Generators/{…, Families/}
    Infrastructure/BuildTemplates.cs
  CdCSharp.BlazorUI.Core.CodeGeneration/ # Roslyn — [AutogenerateCssColors]
  CdCSharp.BlazorUI.CodeGeneration/
  CdCSharp.BlazorUI.SyntaxHighlight/
  CdCSharp.BlazorUI.Localization.Server/
  CdCSharp.BlazorUI.Localization.Wasm/
test/
  CdCSharp.BlazorUI.Tests.Integration/   # bUnit + Verify (xUnit)
  CdCSharp.BlazorUI.Tests.Integration.Templates/
  CdCSharp.BlazorUI.CodeGeneration.Tests/
  CdCSharp.BlazorUI.Core.CodeGeneration.Tests/
  CdCSharp.BlazorUI.Docs.CodeGeneration.Tests/
  CdCSharp.BlazorUI.SyntaxHighlight.Tests/
docs/CdCSharp.BlazorUI.Docs.Wasm/
samples/{AppTest.Server, AppTest.Wasm}/
tools/CdCSharp.BlazorUI.Tools.MaterialIconsScrapper/
```

Lista canónica de componentes públicos (para verificación cruzada — sub‑análisis §6 debe cubrir todos):

- **Forms**: Checkbox, Color, DateAndTime, Dropdown, Number, Radio, Switch, Text, TextArea (+ Forms/Internal).
- **Generic**: Badge, Button, CodeBlock, DataCollections, Loading, Svg, Switch, Tabs, Tree.
- **Layout**: BlazorLayout, Initializer, Card, Dialog, Grid, SidebarLayout, StackedLayout, ThemeGenerator, ThemeSelector, Toast.

---

## 3. Plan por sub‑análisis

Cada sub‑análisis tiene:
- **Código** (prefijo para IDs de tareas).
- **Objetivo** (qué pregunta concreta respondemos).
- **Pasos** (comandos, grep, lecturas, checklist).
- **Salida esperada** (qué entradas en `TASKS.md` debe producir).

### 3.1. Baseline de build y tests — **`BLD`**

**Objetivo**: congelar una línea base verde antes de analizar nada. Sin build limpia no se puede distinguir un hallazgo real de ruido preexistente.

**Pasos**:
1. `dotnet restore`.
2. `dotnet build CdCSharp.BlazorUI.slnx -c Debug` — capturar **todos los warnings** (guardar salida).
3. `dotnet build ... -c Release` — diffear con Debug (warnings que solo aparecen en Release).
4. `dotnet test` — listar tests `skipped`, `inconclusive`, o con output de diagnóstico.
5. Revisar `.github/workflows/publish.yml` y reproducir localmente el orden CodeGeneration → Core → Main → BuildTools.
6. Ejecutar `dotnet clean` y confirmar que `CleanBlazorUIAssets` borra exactamente lo documentado en `CLAUDE.md` (ni más, ni menos).

**Salida**: tareas `BLD-xx` por cada **warning**, test `skipped`/frágil, diferencia entre Debug/Release, fallo al reproducir el orden de CI.

---

### 3.2. Arquitectura de la solución y grafo de dependencias — **`ARCH`**

**Objetivo**: validar que el grafo real coincide con el descrito en `CLAUDE.md`, sin referencias cíclicas, dobles, ni acoplamientos indebidos.

**Pasos**:
1. Parsear `CdCSharp.BlazorUI.slnx` y cada `.csproj` → listado `(proyecto, TargetFramework, PackageReferences, ProjectReferences, GeneratePackageOnBuild, InternalsVisibleTo)`.
2. Verificar que `Core` no referencia `BlazorUI`; que `BuildTools` no es referenciada por runtime; que los proyectos de Localization solo dependen de `Core` o `BlazorUI` según toque.
3. Comprobar que todos los proyectos que se publican como NuGet tienen los metadatos exigidos (`Description`, `Authors`, `PackageTags`, `RepositoryUrl`, `PackageLicenseExpression`, `PackageReadmeFile`, `SymbolPackageFormat=snupkg`).
4. Detectar `<Content Include>` y `<None Include>` con `Pack=true` — confirmar lista: `wwwroot/css`, `wwwroot/js`, `_build/*.targets`, `_build/*.props`, `README.md`.
5. Grafo inverso: quién depende de `InternalsVisibleTo` — asegurar que solo los proyectos de test declarados.
6. `TargetFramework` unificado (`net10.0`) salvo analyzers (`netstandard2.0`).

**Salida**: tareas `ARCH-xx` por dependencia indebida, metadato NuGet faltante, target framework divergente, `Pack` mal configurado.

---

### 3.3. Build pipeline y generación de activos — **`BLD` (pipeline)`**

**Objetivo**: garantizar que el pipeline `BuildTools + BuildTemplates + Generators + Vite` es reproducible, idempotente, y que **ningún archivo generado está siendo editado manualmente**.

**Pasos**:
1. `dotnet clean && dotnet build` dos veces seguidas → los `CssBundle/*.css` deben ser **byte‑idénticos** entre ejecuciones (hash md5). Si no, hay no‑determinismo.
2. `git status` tras `dotnet build` debe estar limpio (aparte de artefactos ignorados). Si hay cambios en `CssBundle/*.css` es porque alguien los tocó a mano contra regla → tarea `Major`.
3. Verificar que cada `IAssetGenerator` define `FileName` único y que la lista de salida coincide 1:1 con los archivos de `CssBundle/`.
4. Verificar que cada `[BuildTemplate("ruta")]` apunta a un archivo que efectivamente se materializa y que luego es consumido por Vite o por `.targets`.
5. Inspeccionar `src/CdCSharp.BlazorUI/_build/CdCSharp.BlazorUI.targets` por: targets duplicados, `BeforeBuild` sin `Condition`, rutas con `\\` (no funciona en Linux CI), `ExitCode` no propagado.
6. `.npmrc`, `package.json`, `vite.config*.js`, `tsconfig.json` generados — revisar versiones pinneadas y lockfiles (si se commitean o no).
7. Validar que `wwwroot/css` y `wwwroot/js` se generan pero **no se commitean** (o están correctamente ignorados/comiteados, según decisión documentada).
8. Comprobar que `FeatureDefinitions` es la única fuente de nombres `data-bui-*` / `--bui-*` / clases `bui-*` tanto en `Generators/` como en los `[BuildTemplate]`.

**Salida**: tareas `BLD-xx` por no‑determinismo, edición manual de generados, duplicado de nombres, hardcodes fuera de `FeatureDefinitions`, targets frágiles.

---

### 3.4. Código generado (Roslyn) — **`GEN`**

**Objetivo**: verificar los source generators (`Core.CodeGeneration`, `CodeGeneration`) y sus tests.

**Pasos**:
1. Abrir cada generator incremental, verificar uso de `IncrementalValuesProvider<T>` con `ForAttributeWithMetadataName` (no `SyntaxProvider` ingenuo).
2. Confirmar que los outputs son **deterministas** (mismo input → mismo output, ordenación estable).
3. `AnalyzerReleases.Shipped.md` / `Unshipped.md` presentes si se emiten diagnostics.
4. Tests existen y cubren: caso feliz, clase parcial preexistente, nombre con caracteres inválidos, diagnostics emitidos.
5. `[AutogenerateCssColors]` — revisar tests en `Core.CodeGeneration.Tests` y `CodeGeneration.Tests` — buscar solapamientos.

**Salida**: tareas `GEN-xx`.

---

### 3.5. API pública y compatibilidad — **`API`**

**Objetivo**: congelar contrato público antes de `1.0.0`. Nada debe renombrarse ni romper tras release.

**Pasos**:
1. Generar listado de tipos y miembros `public` de `CdCSharp.BlazorUI` y `CdCSharp.BlazorUI.Core` (vía reflexión o `dotnet-api-tools`/`Microsoft.CodeAnalysis.PublicApiAnalyzers`).
2. Para cada **componente**: revisar `[Parameter]`s — nombres consistentes (`Value`/`ValueChanged`/`ValueExpression`, `OnClick`/`OnChange`, `Disabled`/`IsDisabled` → elegir uno y alinear), tipos nullable correctos, `EventCallback<T>` vs `Action<T>`.
3. `CascadingParameter` duplicados o redundantes.
4. `RenderFragment` vs `RenderFragment<T>` — uso justificado.
5. Verificar que no hay clases `public` que deberían ser `internal`.
6. `sealed` dónde tiene sentido, `virtual` donde lo requiere el patrón.
7. `XML <summary>` en todos los miembros públicos (ver §3.18).
8. Verificar `EditorBrowsable(EditorBrowsableState.Never)` en primitivas de infraestructura que no deben aparecer en IntelliSense del consumidor.

**Salida**: tareas `API-xx`. Severidad mínima `Major` porque impactan semver post‑release.

---

### 3.6. Contrato `BUIComponentBase` / `BUIInputComponentBase` / `BUIVariantComponentBase` — **`BASE`**

**Objetivo**: confirmar que todos los componentes respetan el contrato base y que el base no tiene lagunas.

**Pasos**:
1. Leer las tres clases base + `BUIComponentAttributesBuilder` + `BUIComponentJsBehaviorBuilder` completo.
2. `grep` de componentes que heredan directamente de `ComponentBase` → debería ser 0 en `src/CdCSharp.BlazorUI/Components`.
3. Verificar que todo componente emite `<bui-component>` como root + `@attributes="ComputedAttributes"`. Cualquier root distinto o spread omitido = tarea `Critical`.
4. `BuildComponentCssVariables` / `BuildComponentDataAttributes` — cada override debe **devolver** (no mutar ajenas), no invalidar el diccionario compartido.
5. `PatchVolatileAttributes` — confirmar que solo se aplica a estados volátiles (`Active`, `Disabled`, `Loading`, `Error`, `ReadOnly`, `Required`, `FullWidth`); cualquier otro estado que flipée en runtime y no esté patchado es bug latente.
6. `OnAfterRenderAsync(firstRender)` — buscar duplicaciones con `BUIComponentJsBehaviorBuilder` (si un componente hace su propio interop y el base ya lo haría via `IHasRipple` u otra `IJsBehavior`, hay doble registro).
7. `IDisposable`/`IAsyncDisposable` — patrón `_disposed` + `try/catch (JSDisconnectedException|InvalidOperationException)` en todos los que lo requieran.
8. Verificar ausencia de `.ConfigureAwait(false)` en todo `src/` (regla dura del proyecto).

**Salida**: `BASE-xx`.

---

### 3.7. Revisión componente a componente — **`COMP`**

**Objetivo**: pase vertical por cada componente. Se ejecuta **uno por uno** para no saltar ninguno.

**Checklist por componente** (producir una tarea por ítem fallido):

1. **Root**: `<bui-component>` + `@attributes="ComputedAttributes"`.
2. **`data-bui-component`**: kebab name correcto (sin `BUI`).
3. **`IHas*` implementadas**: coinciden con las variables/attrs que usa su CSS.
4. **Familia declarada**: si es input/picker/data‑collection, implementa la marker interface y **no** duplica lógica que ya provee la familia.
5. **Parámetros**:
   - Nombres coherentes con el resto de la librería.
   - Default values consistentes.
   - `EventCallback` en lugar de `Action` para eventos.
   - Sin `[Parameter]` que sea de uso interno.
6. **Validación** (si input): `EditContext` respetado, `ValidationMessage` integrado, `data-bui-error` se alinea con `FieldIdentifier`.
7. **Cascading**: consumidos solo los `[CascadingParameter]` necesarios.
8. **Razor markup**: sin lógica compleja embebida (mover a code‑behind), sin inline styles duros (usar `--bui-inline-*`), sin `class="..."` hardcodeado para estado (usar `data-bui-*`).
9. **Eventos de teclado y ratón**: `@onkeydown`/`@onclick.stopPropagation` aplicados consistentemente.
10. **Referencias `ElementReference`**: liberadas en `DisposeAsync`.
11. **`StateHasChanged`**: llamadas justificadas (evitar loops), preferir `InvokeAsync(StateHasChanged)` desde callbacks async cuando proceda.
12. **Children content**: `ChildContent` solo cuando el componente es contenedor; en otros casos, `RenderFragment` con nombre descriptivo.
13. **Unmatched attributes**: verificar que llegan al DOM correcto (no al wrapper interior por accidente).

Lista a cubrir (mismo orden que §2; cada componente = un bloque separado en `TASKS.md`):

`Badge, Button, CodeBlock, DataCollections, Loading, Svg, Switch (Generic), Tabs, Tree, Checkbox, Color, DateAndTime, Dropdown, Number, Radio, Switch (Forms), Text, TextArea, BlazorLayout, Initializer, Card, Dialog, Grid, SidebarLayout, StackedLayout, ThemeGenerator, ThemeSelector, Toast`.

**Salida**: `COMP-<nombre>-xx`.

---

### 3.8. CSS scoped por componente (`.razor.css`) — **`CSS-SCOPED`**

**Objetivo**: cumplir las 10 reglas de "CSS architecture" de `CLAUDE.md` en **cada** `.razor.css`.

**Pasos** (por archivo):
1. Selector raíz **exacto**: `bui-component[data-bui-component="<kebab>"]`.
2. Uso del patrón `--_<componente>-<prop>: var(--bui-inline-*, <default>)` y consumo solo de los `--_*`.
3. Estado solo por `data-bui-*`, nunca por clases.
4. Sizing con `calc(base * var(--bui-size-multiplier, 1))` — prohibir media queries por tamaño.
5. Density con `var(--bui-density-multiplier, 1)`.
6. Colores de tema vía `var(--palette-*)` — detectar colores hex/rgb/hsl hardcoded (`grep -nE '#[0-9a-fA-F]{3,8}|rgba?\(|hsla?\('`).
7. Clases BEM correctas `bui-<componente>__<elem>[--<modificador>]`.
8. Transiciones — solo si implementa `IHasTransitions`.
9. No `!important` salvo con comentario que justifique.
10. Sin selectores globales (`::deep`) salvo casos aprobados.
11. **Cross‑check DOM ↔ CSS (scoped)**: para cada componente, construir dos conjuntos y diffearlos:
    - `ClassesEnCSS(comp)` = clases definidas o referenciadas como selector en su `*.razor.css` (`bui-<comp>__…`, modificadores, estados).
    - `ClassesEnDOM(comp)` = clases que el `.razor` aplica realmente en el markup (literales y condicionales).
    - Detectar:
      - **Selectores huérfanos** (en CSS pero no en DOM) → candidatos a borrar.
      - **Clases huérfanas en DOM** (aplicadas en el markup pero sin regla que las seleccione) → candidatas a eliminar del `.razor` o a añadir la regla faltante si la intención era estilarlas.
      - **Clases que "deberían" aplicarse pero no**: elementos semánticos sin BEM (`<span>` anónimo dentro del componente cuando la convención exige `bui-<comp>__label` u otra); marcar como añadir la clase al DOM.
    - Complementario: revisar presencia de `data-bui-*` en selectores que no están emitidos por ninguna `IHas*`, y al revés (attributes emitidos nunca seleccionados).

**Salida**: `CSS-SCOPED-<comp>-xx`.

---

### 3.9. Bundle CSS global — **`CSS-BUNDLE`**

**Objetivo**: revisar cada `CssBundle/*.css` **a través de su generator** y/o template, no del archivo generado.

**Pasos**:
1. Cada generator produce CSS que **solo** referencia `FeatureDefinitions` / `--palette-*` / variables ya declaradas en un archivo anterior (respetar orden de carga documentado en `CLAUDE.md`).
2. `_reset.css` — revisar que el reset elegido no aplasta foco visible (`outline`) ni semántica de `legend`/`fieldset`.
3. `_typography.css` — escala tipográfica completa (`--bui-font-size-xs..2xl`), `font-family` con fallback seguro.
4. `_themes.css` + `_initialize-themes.css` — cada variable `--palette-*` definida en Light y Dark (set A de claves idéntico). Activación por `[data-bui-theme]` consistente con el JS de theming.
5. `_tokens.css` — escala completa de z‑index, opacidades, multiplicadores; nada hardcoded en otros archivos que debería venir de aquí.
6. `_base.css` — mapeo `[data-bui-size]`/`[data-bui-density]` → multiplicadores ocurre **una sola vez**.
7. `_transition-classes.css` — cada clase usada por algún componente; detectar clases sin consumo (`grep` inverso contra `data-bui-transitions` en components).
8. Familias (`_input-family.css`, `_picker-family.css`, `_data-collection-family.css`) — contrato DOM que imponen coincide con lo que los componentes de la familia efectivamente renderizan.
9. `_scrollbar.css` — si se aplica globalmente, documentado; si no, ámbito claro.

**Salida**: `CSS-BUNDLE-xx`.

---

### 3.10. Consolidación y optimización CSS — **`CSS-OPT`**

**Objetivo**: reducir el peso y la redundancia del CSS enviado al cliente, y **sincronizar DOM ↔ CSS** a nivel global.

**Pasos**:
1. **Duplicación**: detectar propiedades idénticas repetidas en múltiples `.razor.css` que podrían moverse al bundle familiar correspondiente.
2. **Variables huérfanas**:
   - `grep -RhoE -- '--bui-inline-[a-z-]+'` contra `BUIComponentAttributesBuilder` / `FeatureDefinitions` / componentes — listar variables definidas pero nunca consumidas y viceversa.
   - Mismo ejercicio con `--palette-*` y `--_*` privadas.
3. **Selectores costosos**: selectores con >3 combinadores, `*`, `:not()` anidados.
4. **`calc()` anidados redundantes** — `calc(calc(...))` debe ser `calc(...)`.
5. **Transitions**: propiedades `all` — sustituir por listas específicas.
6. **Z‑index fuera de escala** — buscar números crudos en componentes; deben venir de `--bui-z-*`.
7. **Colores** fuera de palette (ya cubierto en §3.8 y §3.9, consolidar aquí las cifras agregadas).
8. **Media queries**: inventariar; idealmente 0 media queries "por tamaño" dentro de la librería (el consumidor decide layout).
9. **`!important`**: censo global, justificación de cada ocurrencia.
10. **PurgeCSS viability**: confirmar que Vite no está minificando a cambio de eliminar reglas que el consumidor compone dinámicamente.
11. **Cross‑check DOM ↔ CSS global**: auditoría agregada sobre toda la solución, no por componente.
    - Construir tres universos:
      - **`UsadoEnDOM`** = unión de (clases, `data-bui-*`, `--bui-inline-*`) que aparecen en `.razor`, `.razor.cs`, `BUIComponentAttributesBuilder`, `FeatureDefinitions`, y templates `[BuildTemplate]`.
      - **`DeclaradoEnCSS`** = unión de selectores de clase, selectores de atributo `[data-bui-*]`, y `var(--bui-inline-*)` en `CssBundle/*.css` + `*.razor.css`.
      - **`PrescritoEnEstandar`** = lo que `CLAUDE.md` / `FeatureDefinitions` dicen que **debe** existir.
    - Reportar:
      - **Reglas CSS sin consumidor** (`DeclaradoEnCSS \ UsadoEnDOM`) → borrar o justificar.
      - **Consumidores sin regla** (`UsadoEnDOM \ DeclaradoEnCSS`) → añadir regla, renombrar, o eliminar del DOM.
      - **Mismatches contra el estándar** (`PrescritoEnEstandar \ (DeclaradoEnCSS ∪ UsadoEnDOM)`) → estándar incumplido o estándar desactualizado (en ese caso alimentar §3.24 `CLAUDE`).
    - Método operativo: un script de auditoría en `tools/` que genere un `CssAudit.txt` reproducible, con las tres columnas y la diferencia. Adjuntar salida al task `CSS-OPT-xx`.
12. **Clases "fantasma" en el markup**: clases aplicadas en `.razor` que no existen en ningún CSS del repo (scoped + bundle + consumer‑overridable). Distinguir dos casos:
    - Clase **pensada para override del consumidor** (p.ej. hook público) → documentar y listar en API pública.
    - Clase **residual** de un refactor → eliminar del markup.
13. **Selectores redundantes por especificidad**: `bui-component[data-bui-component="x"] .bui-x__y` vs `.bui-x__y` — elegir la forma canónica (la primera, según el estándar) y unificar.

**Salida**: `CSS-OPT-xx` con **números** (líneas ahorradas, variables eliminadas, selectores huérfanos eliminados, clases huérfanas removidas del DOM, kB del bundle antes/después estimado).

---

### 3.11. Sistema de theming y paleta — **`THEME`**

**Objetivo**: validar `LightTheme` / `DarkTheme`, `CssColor`, `HsvColor`, `PaletteColor`, `[AutogenerateCssColors]`, y `ThemeSelector`/`ThemeGenerator`.

**Pasos**:
1. Las dos paletas (Light/Dark) definen **el mismo conjunto** de claves.
2. Contrast ratios (`WCAG 2.2 AA`) verificados para pares `surface/surface-contrast`, `primary/primary-contrast`, `error/error-contrast`, etc.
3. `CssColor` + `HsvColor` — revisar operaciones (mix, lighten, darken) por errores numéricos y por NaN con valores extremos.
4. `ThemeGenerator` (componente) — genera combinaciones consistentes; persistencia de selección coherente con `IThemeJsInterop`.
5. `[AutogenerateCssColors]` — cada color generado llega al CSS con el mismo nombre que el palette expone.
6. Transiciones de tema (cambio Light↔Dark) no disparan FOUC; variables `--bui-transition-*` aplicadas al `html` o `body` según convenga.

**Salida**: `THEME-xx`.

---

### 3.12. JS interop / TypeScript — **`JS`**

**Objetivo**: auditar todo `Types/**/*.ts`, el bundle Vite, y los `IJSRuntime` consumers.

**Pasos**:
1. Cada interfaz en `Services/JsInterop/` (`IThemeJsInterop`, `IBehaviorJsInterop`, …) tiene su módulo TS correspondiente y viceversa. `grep` de `import("...")` vs ficheros reales.
2. **Cleanup**: cada TS module expone `init`/`dispose` simétricos y `dispose` es llamado desde el C# en `DisposeAsync`.
3. **Event listeners**: añadidos con opciones (`passive`, `capture`) apropiadas; removidos con exactamente las mismas options y referencia.
4. **Pasivos**: `scroll`/`touchstart`/`touchmove` — si la librería los escucha, deben ser passive salvo que preveagan default.
5. **No `any`** salvo con comentario justificativo. `strict`: true en `tsconfig`.
6. **Leaks**: `MutationObserver`/`IntersectionObserver`/`ResizeObserver` → `disconnect()` en dispose.
7. **IJSObjectReference**: tratado siempre con `await using` o `try/catch (JSDisconnectedException|InvalidOperationException)` en `dispose`.
8. **Bundle size**: inspeccionar `wwwroot/js` tras build; detectar módulos no usados.
9. **Source maps**: activados en Debug, opcionalmente stripped en Release.

**Salida**: `JS-xx`.

---

### 3.13. Async, disposal, concurrencia — **`ASYNC`**

**Objetivo**: pase transversal buscando patrones peligrosos.

**Pasos**:
1. `rg -n 'ConfigureAwait\(' src/` → **debe ser 0**. Cualquier hit es `Critical`.
2. `async void` → solo permitido en event handlers `EventHandler`.
3. `.Result` / `.Wait()` / `.GetAwaiter().GetResult()` en runtime → `Blocker`.
4. `CancellationTokenSource` sin `Dispose`.
5. Event subscriptions sin unsubscribe (`NavigationManager.LocationChanged += ...`).
6. `Task.Run` innecesario en Blazor (sobrecosto sin beneficio).
7. `Task` sin await (fire‑and‑forget) — revisar si es intencionado y documentado.
8. Race condition en components con debouncing / throttling (`DelayedActionHandler`).
9. `InvokeAsync(StateHasChanged)` vs `StateHasChanged` en continuaciones async.

**Salida**: `ASYNC-xx`.

---

### 3.14. Accesibilidad — **`A11Y`**

**Objetivo**: WCAG 2.2 AA razonable para librería UI.

**Pasos por componente interactivo**:
1. `role` correcto cuando el tag nativo no es suficiente (`<bui-component>` custom → roles explícitos).
2. `aria-*` dinámicos (`aria-disabled`, `aria-expanded`, `aria-pressed`, `aria-selected`, `aria-invalid`, `aria-describedby`, `aria-controls`, `aria-labelledby`).
3. Teclado: `Tab` orden lógico, `Enter`/`Space` en buttons, flechas en menús/listboxes/trees/tabs, `Escape` en diálogos/dropdowns.
4. Focus visible: no suprimido por `outline: none` sin reemplazo.
5. Focus management en Dialog/Modal: trap + return al trigger.
6. `prefers-reduced-motion` — respetado en transiciones.
7. Contraste de color (ver §3.11).
8. Labels y descripciones: `<label>` asociado, o `aria-label`/`aria-labelledby`.
9. Datos dinámicos: `aria-live` donde corresponda (toasts, validaciones).
10. Tests de a11y por componente (`*AccessibilityTests.cs`) — cobertura ≥ umbral fijado.

**Salida**: `A11Y-<comp>-xx`.

---

### 3.15. Performance y re‑render — **`PERF`**

**Objetivo**: detectar re‑renders evitables, allocations por render, y opciones de `ShouldRender`.

**Pasos**:
1. `[CascadingParameter]` que cambian frecuentemente causando cascade renders.
2. `RenderFragment` construidos inline (crean closure por render) vs cacheados.
3. `StringBuilder` / `string.Concat` en hot paths (`BuildStyles`).
4. `AdditionalAttributes` — copias innecesarias del dictionary; usar `IReadOnlyDictionary` si no hay mutación.
5. `ComputedAttributes` — ¿se reconstruye en cada render o se cachea mientras los inputs no cambien? (si cambian poco: memoize).
6. Reflection en `BUIComponentAttributesBuilder` — debe cachear `Type` → metadata.
7. Size multiplier recalculado en CSS (gratis) pero en C#: no recalcular si no cambia.
8. `StateHasChanged` redundantes en loops.
9. Tests con profiler (`BenchmarkDotNet` opcional para el builder).

**Salida**: `PERF-xx`.

---

### 3.16. Seguridad — **`SEC`**

**Pasos**:
1. `MarkupString` — inventariar; cada uso debe documentar por qué es seguro (fuente controlada, no input de usuario).
2. JS interop con strings construidos — riesgo de inyección si se concatena en TS y luego `eval`/`innerHTML`.
3. `Uri`/URLs — validación de schemes permitidos en componentes que enlazan (links, iframe, etc.).
4. Cookies / localStorage — encoding + validación.
5. Dependency audit: `dotnet list package --vulnerable --include-transitive`, `npm audit --production` (sobre `package.json` generado).
6. `InternalsVisibleTo` — solo tests.
7. Clickjacking / frame‑ancestors: Dialog/Modal no vulneran.
8. XSS en `BUICodeBlock` / `SyntaxHighlight` — sanitización del input.

**Salida**: `SEC-xx`.

---

### 3.17. Tests — cobertura y calidad — **`TEST`**

**Objetivo**: que la matriz de tests coincida con la especificada en `CLAUDE.md`.

**Pasos**:
1. Para cada componente, verificar existencia de los 6 archivos estándar (Rendering, State, Interaction, Variants, Accessibility, Snapshots). Los no aplicables, documentar por qué.
2. Cada test method: `[Theory]` + `TestScenarios.All` por defecto. Justificar `OnlyServer`/`OnlyWasm`.
3. Naming `Should_...`, banners `// Arrange`/`// Act`/`// Assert`.
4. Uso de `BlazorTestContextBase` (no `BunitContext` directo).
5. **Snapshots**: archivos `.verified.txt` consistentes, `.received.txt` ausentes en repo.
6. **Cobertura**: ejecutar `Test-Coverage.ps1`; por componente, exigir ≥ umbral (p.ej. 80% líneas, 70% ramas).
7. Verify scrubbers — que siguen escondiendo GUIDs de `blazor:elementReference` y eventIds.
8. `InternalsVisibleTo` — no abusado para "acceder a lo interno por atajo" en tests que podrían testear por API pública.
9. **Flakiness**: tests que usen `Task.Delay`/`await Task.Yield` → candidatos a inestabilidad.
10. Build de test projects sin warnings.

**Salida**: `TEST-<comp>-xx` + `TEST-GLOBAL-xx`.

---

### 3.18. Documentación XML y docs site — **`DOC`**

**Pasos**:
1. `GenerateDocumentationFile=true` en todos los proyectos empaquetados; **cero** CS1591 (missing XML) en build Release.
2. Cada `[Parameter]` público tiene `<summary>` y, cuando procede, `<remarks>` y `<example>`.
3. `docs/CdCSharp.BlazorUI.Docs.Wasm` — revisar páginas por componente: existen todos los componentes listados en §2. Ejemplos compilan. Screenshots/snippets actualizados.
4. `README.md` del repo — completo (actualmente 10 bytes → `Major`).
5. `CHANGELOG.md` — existe y está al día (o al menos preparado para 1.0.0).
6. `CLAUDE.md` coincide con código (revisar tras Fase 2).

**Salida**: `DOC-xx`.

---

### 3.19. Empaquetado NuGet y publicación — **`PKG`**

**Pasos**:
1. `dotnet pack -c Release` → inspeccionar cada `.nupkg` con unzip: rutas `build/`, `buildTransitive/`, `contentFiles/`, assemblies, `README.md`, license file.
2. `PackageValidation` / `Microsoft.CodeAnalysis.PublicApiAnalyzers` opcionales.
3. `.targets`/`.props` en `build/` → rutas funcionan cross‑platform (Linux CI).
4. `assets/` CSS y JS incluidos y referenciados por un `.targets` que añade `<link>`/`<script>` cuando el consumidor llama `AddBlazorUI`.
5. `snupkg` generado para symbols.
6. Versionado: verificar flujo `develop` preview → `main` build → tag `vX.Y.Z` release según `publish.yml`.
7. `PackageLicenseExpression`/`PackageLicenseFile` — uno solo, no ambos.
8. `PackageIcon` presente.

**Salida**: `PKG-xx`.

---

### 3.20. Localización — **`L10N`**

**Pasos**:
1. `Localization.Server` y `Localization.Wasm` comparten contrato (`IBlazorUILocalizer`?) y difieren solo en bootstrap.
2. Claves de recursos coinciden entre ambos.
3. Fallback a cultura invariante funciona.
4. `CultureSelector` component — sin dependencias específicas de hosting duplicadas.
5. Culture switching no provoca leaks de suscripciones.

**Salida**: `L10N-xx`.

---

### 3.21. CI/CD — **`CI`**

**Pasos**:
1. `.github/workflows/publish.yml`: matrices, caches (`actions/setup-dotnet`, `actions/cache` para `~/.nuget/packages` y `node_modules`), `concurrency` para evitar publish duplicados.
2. Secretos: `NUGET_API_KEY` usado con `if: ${{ secrets.NUGET_API_KEY != '' }}`.
3. `workflow_dispatch` con `publish=true` — confirmación explícita y gateada.
4. No hay secretos comiteados (`git log -p --all` grep rápido de patrones obvios).
5. Workflow de PR (tests + build) existe; status checks requeridos en `main`.
6. Dependabot o equivalente configurado (`.github/dependabot.yml`).
7. CodeQL / análisis estático activado.

**Salida**: `CI-xx`.

---

### 3.22. Docs WASM — **`DOCS-WASM`**

> Nota: los proyectos `samples/AppTest.Server` y `samples/AppTest.Wasm` **quedan fuera de alcance** de este análisis — están marcados para posible eliminación antes del release y no justifican inversión de revisión.

**Pasos**:
1. `dotnet run --project docs/CdCSharp.BlazorUI.Docs.Wasm` arranca sin errores, sin warnings de compilación.
2. Despliegue público (GH Pages o equivalente) — revisar `base href`, rutas relativas, `wwwroot/index.html`, service worker (si aplica).
3. Cobertura documental: existe una página navegable para cada componente listado en §2. Marcar componentes sin doc asociada.
4. Ejemplos en las páginas de docs compilan y se ejecutan — no screenshots estáticos para flujos interactivos.
5. Console del browser limpia al navegar (sin `404`, sin warnings de React/Blazor, sin `Failed to load module`).
6. Lighthouse/accesibilidad sobre la home y dos páginas representativas; anotar score.
7. Código fuente del docs site respeta las mismas convenciones que la librería (`<bui-component>`, no clases de estado, etc.) — sirve de ejemplo canónico.

**Salida**: `DOCS-WASM-xx`.

---

### 3.23. Meta‑revisión de `CLAUDE.md` — **`CLAUDE`**

**Objetivo**: `CLAUDE.md` es la fuente de verdad del estándar interno; durante el análisis puede quedar obsoleta, incompleta o incorrecta. Este sub‑análisis **transversal** recoge todos los cambios que el propio proceso haya revelado sobre el documento maestro.

**Cuándo se alimenta**: continuamente durante F1. Cada vez que un sub‑análisis detecta un conflicto entre código real y `CLAUDE.md`, se abre una tarea `CLAUDE-xx` (además de la tarea del área técnica correspondiente).

**Casos que generan tarea `CLAUDE-xx`**:
1. **Estándar incorrecto**: `CLAUDE.md` prescribe algo que el código demuestra erróneo o contraproducente (ej. un patrón que genera race conditions, un selector CSS que rinde peor). Cambiar el estándar, no el código, si la evidencia va en esa dirección.
2. **Estándar obsoleto**: el documento describe una estructura, clase o archivo que ya no existe o ha cambiado de nombre (ej. `BehaviorsJsInterop` mencionado en `JsInterop/` pero solo existe `BehaviorJsInterop.cs`).
3. **Estándar ambiguo**: una regla admite dos interpretaciones que han llevado a divergencia entre componentes — aclarar con ejemplo positivo y negativo.
4. **Estándar incompleto**: un patrón aplicado consistentemente en el código no aparece documentado (ej. convención para naming de templates de test, naming de `[BuildTemplate]`, uso de `--_component-*` con prefijo privado).
5. **Nuevas áreas**: familias, interfaces `IHas*`, servicios o tipos introducidos después de la última revisión del documento.
6. **Contradicciones internas**: secciones que dicen cosas incompatibles entre sí.
7. **Referencias rotas**: rutas de archivo, nombres de clase o métodos citados que ya no coinciden con `grep`.
8. **Enlaces a documentación externa**: validar al menos los internos (otros `.md`, archivos `.cs`).

**Pasos sistemáticos (pase final una vez cerrada F1 técnica)**:
1. Releer `CLAUDE.md` de principio a fin con el código ya en su estado post‑F2 pensado.
2. Para cada sección: confirmar al menos **un ejemplo de código** citado sigue existiendo en las mismas coordenadas.
3. Validar que todos los `IHas*` listados en la tabla de interfaces coinciden con las interfaces reales (`rg 'interface IHas' src/CdCSharp.BlazorUI.Core/Abstractions/Behaviors/`).
4. Validar que la lista de componentes citados existe en disco.
5. Validar que las reglas CSS 1‑10 son exhaustivas (no falta ninguna regla ya aplicada en el código).
6. Validar que la descripción del pipeline (`BuildTools` + `BuildTemplates` + Vite) coincide con lo observado en §3.3.
7. Validar que la tabla de convenciones de test coincide con §3.17.

**Salida**: `CLAUDE-xx`. Cada tarea especifica:
- Sección y párrafo afectado (`## 3. Component architecture` → bullet N).
- Texto actual literal.
- Texto propuesto.
- Evidencia (ruta:línea) que justifica el cambio.

**Tratamiento especial**: los cambios a `CLAUDE.md` **se aplican en F2 como último commit antes del tag**, para que el documento refleje el estado final post‑saneamiento y no un estado intermedio.

---

### 3.24. Release checklist final — **`REL`**

**Objetivo**: gate de salida. Solo se marca listo cuando:

1. `BLD-*` sin Blockers/Critical.
2. `ARCH-*` resuelto.
3. `API-*` congelado.
4. `COMP-*` revisados todos.
5. `CSS-OPT` con reducción numérica documentada.
6. `A11Y` sin Blockers/Critical.
7. `ASYNC` sin Blockers/Critical.
8. `SEC` sin Blockers/Critical.
9. `TEST` cobertura mínima alcanzada; snapshots al día.
10. `DOC` README + XML docs + docs site al día.
11. `PKG` paquetes inspeccionados a mano.
12. `CI` pipeline verde sobre `main`.
13. `CLAUDE` revisado y commit final aplicado (último antes del tag).
14. Versión `1.0.0` taggeada con `CHANGELOG.md` cerrado.

Cualquier ítem fuera → tarea `REL-xx` "gate abierto".

---

## 4. Orden recomendado de ejecución

1. §3.1 `BLD` → fija base verde.
2. §3.2 `ARCH` + §3.3 `BLD pipeline` → saneo estructural.
3. §3.4 `GEN` (rápido, desbloquea el resto).
4. §3.6 `BASE` → sin contrato base sólido el resto no cierra.
5. §3.5 `API` → congelar superficie pública antes de tocar componentes.
6. §3.7 `COMP` (iterar por los ~28 componentes).
7. §3.8 `CSS-SCOPED` (en paralelo con §3.7 por componente).
8. §3.9 `CSS-BUNDLE` y §3.10 `CSS-OPT`.
9. §3.11 `THEME`, §3.12 `JS`, §3.13 `ASYNC` (paralelizables).
10. §3.14 `A11Y`, §3.15 `PERF`, §3.16 `SEC`.
11. §3.17 `TEST` (se va alimentando durante todo el proceso pero se audita aquí).
12. §3.18 `DOC`, §3.19 `PKG`, §3.20 `L10N`, §3.21 `CI`, §3.22 `DOCS-WASM`.
13. §3.23 `CLAUDE` (pase final + tareas acumuladas durante F1).
14. §3.24 `REL`.

### 4.1. Tabla agregada de progreso

Vista rápida del estado de cada sub‑análisis. Se actualiza a la vez que el campo `Estado` de la sección correspondiente.

| § | Código | Área | Estado | Tareas | Resumen / Bloqueo |
|---|--------|------|--------|-------:|-------------------|
| 3.1  | `BLD`         | Baseline build + tests                    | `[x]` | 15 | 3 Critical, 7 Major, 3 Minor, 2 Polish |
| 3.2  | `ARCH`        | Arquitectura y grafo de dependencias      | `[x]` | 17 | 4 Critical, 7 Major, 5 Minor, 1 Polish |
| 3.3  | `BLD-PIPE`    | Pipeline de generación de activos         | `[x]` | 17 | 3 Critical, 9 Major, 4 Minor, 1 Polish |
| 3.4  | `GEN`         | Code generators Roslyn                    | `[x]` | 13 | 2 Critical, 3 Major, 5 Minor, 3 Polish |
| 3.5  | `API`         | Superficie pública                        | `[x]` | 14 | 3 Critical, 5 Major, 4 Minor, 2 Polish |
| 3.6  | `BASE`        | Contrato `BUIComponentBase` y familia     | `[x]` | 12 | 2 Critical, 4 Major, 4 Minor, 2 Polish |
| 3.7  | `COMP`        | Revisión componente a componente          | `[x]` | 10 | 1 Critical, 5 Major, 3 Minor, 1 Polish (incluye meta‑task `COMP-AUDIT-CHECKLIST-01` para el pase vertical 28×13) |
| 3.8  | `CSS-SCOPED`  | CSS scoped por componente                 | `[x]` | 9 | 0 Critical, 4 Major, 3 Minor, 2 Polish (incluye meta‑task `CSS-SCOPED-09` para el pase vertical DOM↔CSS) |
| 3.9  | `CSS-BUNDLE`  | Bundle CSS global                         | `[x]` | 5 | 0 Critical, 1 Major, 3 Minor, 1 Polish (nivel bundle; per‑generator cubierto por `BLD-PIPE-01..16`) |
| 3.10 | `CSS-OPT`     | Consolidación y optimización CSS          | `[x]` | 6 | 0 Critical, 2 Major, 3 Minor, 1 Polish (incluye `CSS-OPT-02` como meta herramienta de auditoría tri‑universo) |
| 3.11 | `THEME`       | Theming y paleta                          | `[x]` | 10 | 0 Critical, 3 Major, 5 Minor, 2 Polish |
| 3.12 | `JS`          | JS interop / TypeScript                   | `[x]` | 12 | 0 Critical, 6 Major, 4 Minor, 2 Polish |
| 3.13 | `ASYNC`       | Async, disposal, concurrencia             | `[x]` | 11 | 0 Critical, 5 Major, 4 Minor, 2 Polish |
| 3.14 | `A11Y`        | Accesibilidad                             | `[x]` | 11 | 0 Critical, 4 Major, 5 Minor, 2 Polish |
| 3.15 | `PERF`        | Performance y re‑render                   | `[x]` | 11 | 0 Critical, 3 Major, 5 Minor, 3 Polish |
| 3.16 | `SEC`         | Seguridad                                 | `[x]` | 11 | 0 Critical, 4 Major, 5 Minor, 2 Polish |
| 3.17 | `TEST`        | Cobertura y calidad de tests              | `[x]` | 11 | 0 Critical, 4 Major, 5 Minor, 2 Polish |
| 3.18 | `DOC`         | XML docs                                  | `[x]` | 11 | 0 Critical, 4 Major, 5 Minor, 2 Polish |
| 3.19 | `PKG`         | Empaquetado NuGet                         | `[x]` | 11 | 0 Critical, 4 Major, 5 Minor, 2 Polish |
| 3.20 | `L10N`        | Localización                              | `[x]` | 11 | 0 Critical, 4 Major, 5 Minor, 2 Polish |
| 3.21 | `CI`          | CI/CD                                     | `[x]` | 11 | 0 Critical, 4 Major, 5 Minor, 2 Polish |
| 3.22 | `DOCS-WASM`   | Docs WASM                                 | `[x]` | 11 | 0 Critical, 4 Major, 5 Minor, 2 Polish |
| 3.23 | `CLAUDE`      | Meta‑revisión de `CLAUDE.md`              | `[x]` | 11 | 0 Critical, 4 Major, 5 Minor, 2 Polish |
| 3.24 | `REL`         | Release checklist                         | `[x]` | 11 | 0 Critical, 4 Major, 5 Minor, 2 Polish |

> Leyenda: `[ ]` Pendiente · `[~]` En curso · `[x]` Completado · `[!]` Bloqueado · `[-]` N/A.

---

## 5. Herramientas y comandos de apoyo

```bash
# build + tests base
dotnet restore
dotnet build CdCSharp.BlazorUI.slnx -c Release 2>&1 | tee build-release.log
dotnet test --logger "trx;LogFileName=tests.trx" --collect:"XPlat Code Coverage"

# API pública (congelar)
dotnet list package --include-transitive
dotnet list package --vulnerable --include-transitive
dotnet list package --outdated

# Búsqueda de patrones críticos
rg -n 'ConfigureAwait\('                   src/
rg -n '\.Result|\.Wait\(\)|GetAwaiter\(\)\.GetResult\(\)' src/
rg -n '!important'                          src/ --glob '*.css'
rg -nE '#[0-9a-fA-F]{3,8}|rgba?\(|hsla?\(' src/ --glob '*.css' --glob '!CssBundle/_themes.css'
rg -n 'MarkupString'                        src/

# CSS
npx stylelint 'src/**/*.css' --config .stylelintrc || true   # si se añade
```

---

## 6. Entregables de la Fase F1

Al finalizar la ejecución de este plan debe existir:

- `TASKS.md` con las secciones por severidad y todas las tareas con ID estable.
- Una tabla resumen al inicio de `TASKS.md`:

```
| Área | Blockers | Critical | Major | Minor | Polish |
|------|---------:|---------:|------:|------:|-------:|
| ARCH |          |          |       |       |        |
| …    |          |          |       |       |        |
```

- Métricas objetivas (líneas CSS, kB bundle, % cobertura, nº warnings) capturadas antes/después para poder demostrar mejora en F2.

Este documento es **vivo** únicamente mientras dura F0. Una vez arrancada F1 no se modifica salvo para corregir erratas del plan; los hallazgos van a `TASKS.md`.
