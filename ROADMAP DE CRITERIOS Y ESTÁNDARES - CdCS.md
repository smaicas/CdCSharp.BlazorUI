ROADMAP DE CRITERIOS Y ESTÁNDARES - CdCSharp.BlazorUI
1. PRINCIPIOS Y ESTÁNDARES BASE
1.1 Arquitectura de Componentes
P1. Jerarquía de Herencia Estricta

Descripción: Todo componente debe heredar de BUIComponentBase o sus derivadas específicas
Motivación: Garantizar comportamiento consistente y acceso a funcionalidad compartida
Implicaciones:

Componentes simples: heredan de BUIComponentBase
Componentes con variantes: heredan de BUIVariantComponentBase<TComponent, TVariant>
Componentes de formulario: heredan de BUIInputComponentBase<TValue>



P2. Comportamientos mediante Interfaces

Descripción: Las capacidades opcionales se implementan mediante interfaces IHas*
Motivación: Composición flexible sin explosión de clases base
Implicaciones:

Cada comportamiento tiene su interfaz: IHasSize, IHasColor, IHasLoading
Los comportamientos se procesan en BUIComponentAttributesBuilder
No usar propiedades ad-hoc, siempre interfaces formales



P3. Variants como Tipos Inmutables

Descripción: Las variantes visuales son objetos inmutables con patrón factory
Motivación: Type safety, evitar strings mágicos, permitir extensibilidad controlada
Implicaciones:

Cada componente con variantes define su tipo *Variant
Variantes built-in como propiedades estáticas
Método Custom() para variantes personalizadas



1.2 Sistema de Estilos
P4. CSS Modular por Componente

Descripción: Cada componente tiene su archivo .razor.css con estilos aislados
Motivación: Evitar conflictos CSS, mantener cohesión
Implicaciones:

Selectores basados en ui-component[data-ui-component="name"]
Clases BEM para partes internas (ui-component__part)
Variables CSS para valores dinámicos



P5. Data Attributes para Estado

Descripción: El estado visual se controla mediante data attributes, no clases CSS
Motivación: Separación clara entre estructura y presentación
Implicaciones:

Estados: data-ui-loading, data-ui-disabled, data-ui-error
Configuración: data-ui-size, data-ui-density, data-ui-variant
CSS selecciona por atributos, no por clases



P6. Variables CSS para Personalización

Descripción: Valores personalizables mediante CSS custom properties
Motivación: Theming dinámico sin recompilación
Implicaciones:

Paleta: --palette-* para colores del tema
Componente: --ui-* para valores específicos
Inline styles solo para variables CSS, nunca estilos directos



1.3 Interoperabilidad y Performance
P7. JavaScript Modular y Lazy

Descripción: JS/TS se carga solo cuando es necesario mediante módulos ES
Motivación: Performance, evitar bloquear el render inicial
Implicaciones:

Un módulo TypeScript por funcionalidad
Interop mediante interfaces C# específicas
Dispose pattern para limpieza de recursos



P8. Generación Automática de Assets

Descripción: CSS común y temas se generan desde C# mediante build tools
Motivación: Single source of truth, evitar duplicación
Implicaciones:

FeatureDefinitions centraliza tokens de diseño
Build pipeline genera CSS desde definiciones C#
Vite procesa y optimiza los bundles finales



1.4 Extensibilidad y Gobernanza
P9. Registry Pattern para Extensibilidad

Descripción: Personalizaciones se registran en tiempo de configuración
Motivación: Control sobre extensiones, evitar modificación directa
Implicaciones:

VariantRegistry para variantes personalizadas
Registro durante startup, no en runtime
Validación de conflictos y duplicados



P10. Abstracciones Primero

Descripción: Definir contratos antes que implementaciones
Motivación: Facilitar testing, permitir múltiples implementaciones
Implicaciones:

Interfaces en .Abstractions namespaces
Implementaciones pueden variar por plataforma
Dependency injection para todo servicio



2. CRITERIOS OBLIGATORIOS POR FASE
2.1 Criterios Arquitectónicos

 Componente hereda de la clase base correcta
 Behaviors implementados mediante interfaces IHas*
 No dependencias circulares entre proyectos
 Separación Core/Abstractions/Implementation respetada

2.2 Criterios de Diseño de Componentes

 Variant types definidos si hay múltiples estilos
 RenderFragment patterns para templates personalizables
 Parámetros tipados fuertemente (no object ni dynamic)
 Dispose pattern implementado si usa recursos

2.3 Criterios de API Pública

 Propiedades con valores por defecto sensatos
 Naming consistente con componentes existentes
 EventCallbacks para interacción, no Actions
 Documentación XML en miembros públicos

2.4 Criterios de Testing

 Tests de integración para comportamiento observable
 Tests unitarios para lógica compleja aislada
 Cobertura de variantes y estados edge case
 Tests de accesibilidad automatizados

2.5 Criterios de Accesibilidad

 ARIA attributes apropiados
 Navegación por teclado funcional
 Contrast ratios WCAG AA mínimo
 Screen reader friendly

3. ROADMAP POR FASES
FASE 1: Componentes Base Completados
Objetivo: Finalizar y estabilizar componentes fundamentales
Componentes:

BUIButton - Completar todas las variantes (Outlined, Text, Fab)
BUIInputText - Agregar máscaras y validación en tiempo real
BUICheckbox, BUIRadio, BUISwitch - Componentes de selección
BUISelect - Dropdown con soporte multi-select

Estándares Aplicables:

P1, P2, P3: Arquitectura de componentes
P4, P5, P6: Sistema de estilos
P7: JavaScript para interacciones (ripple)

Checklist de Validación:

 Todos los componentes tienen variantes definidas
 CSS modular implementado sin !important
 Data attributes para todos los estados
 Tests de integración cubren happy path
 Ejemplos en documentación

FASE 2: Sistema de Layout
Objetivo: Componentes de estructura y navegación
Componentes:

BUIGrid - Sistema de grid responsive
BUIContainer, BUIRow, BUICol - Layout helpers
BUIAppBar, BUIDrawer - Navegación principal
BUITabs, BUIBreadcrumb - Navegación secundaria

Estándares Aplicables:

P1, P2: Arquitectura base
P4, P5: CSS responsive con media queries
P8: Generación de breakpoints desde C#
P10: Abstracciones para layout engines

Checklist de Validación:

 Breakpoints consistentes con design system
 SSR compatible (no JS para layout inicial)
 Responsive sin JavaScript
 Performance: no layout shifts

FASE 3: Componentes de Datos
Objetivo: Visualización y manipulación de datos
Componentes:

BUITable - Tabla con sort, filter, pagination
BUIDataGrid - Grid avanzado virtualizado
BUIChart - Gráficos con Chart.js wrapper
BUITreeView - Árbol jerárquico

Estándares Aplicables:

P1, P2, P3: Arquitectura completa
P7: JavaScript para virtualización
P9: Registry para column types
P10: Abstracciones para data sources

Checklist de Validación:

 Virtualización para 10k+ filas
 Accesibilidad en tablas complejas
 Templating para celdas custom
 Export capabilities

FASE 4: Componentes de Feedback
Objetivo: Notificaciones y estado
Componentes:

BUISnackbar, BUIToast - Notificaciones temporales
BUIDialog, BUIModal - Diálogos modales
BUIProgress - Linear y circular
BUITooltip, BUIPopover - Overlays informativos

Estándares Aplicables:

P1, P2: Arquitectura base
P6: CSS animations via custom properties
P7: JavaScript para positioning
P9: Registry para notification queues

Checklist de Validación:

 Stacking y z-index gestionado
 Animations respetan prefers-reduced-motion
 Focus trap en modales
 Queue management para notificaciones

FASE 5: Formularios Avanzados
Objetivo: Componentes de entrada complejos
Componentes:

BUIDatePicker, BUITimePicker - Selectores de fecha/hora
BUIColorPicker - Selector de color
BUIFileUpload - Carga de archivos con preview
BUIRichTextEditor - Editor WYSIWYG

Estándares Aplicables:

Todos los principios P1-P10
Integración completa con EditForm
Localización para fechas/números
Validación asíncrona

Checklist de Validación:

 EditContext integration completa
 Validación client y server side
 Localización funcional
 Drag & drop accesible

FASE 6: Temas y Personalización
Objetivo: Sistema de theming avanzado
Capacidades:

Theme builder visual
Export/import de temas
CSS-in-JS runtime theming
Dark mode automático

Estándares Aplicables:

P6: Variables CSS extendidas
P8: Generación dinámica
P9: Registry para theme providers
P10: Abstracciones para theme engines

Checklist de Validación:

 Temas hot-swappable
 Persistencia de preferencias
 A11y contrast validation
 Performance: no FOUC

4. REGLAS DE GOBERNANZA
4.1 Validación de Nuevos Componentes
Proceso de Review:

PR debe incluir checklist de criterios cumplidos
Build pipeline valida estructura de archivos
Code review verifica patrones arquitectónicos
Tests automáticos validan comportamiento

Criterios de Rechazo Automático:

No hereda de clase base apropiada
Usa estilos inline sin justificación
No tiene tests de integración
Rompe convenciones de naming
Introduce dependencias no aprobadas

4.2 Actualización del Roadmap
Proceso de Cambio:

Propuesta documentada con justificación técnica
Análisis de impacto en componentes existentes
Plan de migración si hay breaking changes
Aprobación por maintainers principales

Cambios Permitidos:

Nuevos principios que no contradigan existentes
Refinamiento de criterios para claridad
Nuevas fases sin alterar las existentes

Cambios Prohibidos:

Eliminar principios fundamentales (P1-P5)
Cambiar arquitectura base sin migración
Relajar criterios de calidad

4.3 Mecanismos de Enforcement
Automatizados:

Analyzers custom para patrones prohibidos
Build pipeline valida estructura
Tests de regresión para comportamientos
Linting para CSS/JS/TS

Manuales:

Code review obligatorio
Documentación de decisiones arquitectónicas
Retrospectivas trimestrales de deuda técnica

5. PROPUESTAS DE CAMBIOS EN ESTÁNDARES ACTUALES
5.1 Migración a CSS Layers
Situación Actual: CSS usa especificidad y orden de carga
Propuesta: Implementar @layer para control explícito de cascada
Motivación:

Evitar guerras de especificidad
Facilitar override de estilos
Mejor aislamiento entre componentes
Impacto: Requiere refactor de CSS existente pero mejora mantenibilidad

5.2 Simplified Variant Pattern
Situación Actual: Variants requieren clase separate y registro manual
Propuesta: Enum-based variants con source generator
Motivación:

Reducir boilerplate
Type safety mejorado
Discoverable en IntelliSense
Impacto: Breaking change pero simplifica API pública

5.3 Reactive Behaviors
Situación Actual: Behaviors son interfaces estáticas
Propuesta: Observable behaviors con Sistema.Reactive
Motivación:

Estados reactivos entre componentes
Mejor integración con state management
Animaciones basadas en transiciones de estado
Impacto: Opt-in, no afecta componentes existentes

5.4 Build-time CSS Optimization
Situación Actual: Todo CSS se incluye siempre
Propuesta: Tree-shaking de CSS no utilizado
Motivación:

Reducir tamaño de bundle
Mejor performance inicial
Carga incremental por ruta
Impacto: Requiere análisis estático de uso de componentes


Este roadmap debe ser tratado como documento vivo pero con cambios controlados. Cada fase debe completarse con sus criterios antes de avanzar. La consistencia y calidad tienen prioridad sobre velocidad de desarrollo.