# 7. 🧪 Estandar de Testing y Pruebas de Integración

Este documento establece el estándar de testing y describe los tipos de pruebas de integración cruciales para la estabilidad de la librería.

## 7.1. Filosofía de Pruebas

Las pruebas se centran en verificar la integración correcta entre los componentes de C#, la lógica de JS Interop, y la salida HTML esperada (DOM).

## 7.2. Pruebas de Integración (Bases)

### 7.2.1. Test de Captura de Atributos (`AdditionalAttributes`)

**Objetivo**: Verificar que los atributos HTML no mapeados como parámetros se capturen correctamente y se apliquen al elemento raíz.

* **Caso de Prueba**: Renderizar un componente (ej. `UIComponentBase`) con atributos arbitrarios como `id="test-id"` o `data-custom="value"`.
* **Verificación**: El resultado HTML debe contener exactamente esos atributos en el elemento principal del componente.

### 7.2.2. Test de Composición de Clases CSS

**Objetivo**: Asegurar que las clases base (`GetAdditionalCssClasses`), las clases de variante y las clases proporcionadas por el usuario se fusionen correctamente en `ComputedCssClasses`.

* **Escenario**:
    1.  El componente define una clase base (`base-class`).
    2.  El componente aplica una clase condicional (`active-class`).
    3.  El usuario añade una clase (`user-provided-class`).
* **Resultado Esperado**: El atributo `class` debe ser `base-class active-class user-provided-class` (el orden puede variar, pero deben estar presentes).

## 7.3. Pruebas Críticas de Arquitectura

### 7.3.1. Test de Prioridad de Atributos en Templates de Variantes

**Objetivo**: Verificar que un `RenderFragment` registrado en el `VariantRegistry` tenga la máxima prioridad para establecer el valor de un atributo, incluso sobre un valor proporcionado por el usuario.

* **Componente de Prueba (`TestVariantComponent`)**: Implementa un parámetro `data-test="user-value"` en `AdditionalAttributes`.
* **Template Registrado (`CustomTemplate`)**: Define `data-test="template-value-priority"`.
* **Verificación**: Al renderizar el componente con el template personalizado, el DOM debe mostrar `data-test="template-value-priority"`, confirmando que el valor del template sobrescribió el valor de `AdditionalAttributes`.

### 7.3.2. Test de JS Interop (Carga de Módulos)

**Objetivo**: Confirmar que la lógica de `ModuleJsInteropBase` carga correctamente el módulo JS una sola vez y lo dispone asíncronamente.

* **Caso de Prueba (`ThemeJsInteropTests`)**:
    1.  Se inyecta un *mock* de `IJSRuntime`.
    2.  Se llama a un método (ej. `GetThemeAsync`) dos veces.
* **Verificación**: El *mock* de `IJSRuntime` solo debe registrar **una** llamada a `import(...)` durante la vida útil del objeto, y una llamada a `DisposeAsync()` al finalizar.

### 7.3.3. Test de Theming Persistente

**Objetivo**: Confirmar que el `UIBlazorLayout` llama a la inicialización del tema.

* **Componente de Prueba**: Montar un `Layout` que hereda de `UIBlazorLayout`.
* **Verificación**: El *mock* de `IThemeJsInterop` debe registrar una llamada a `InitializeAsync()` en el primer renderizado.

## 7.4. Pruebas de Utilidad (CssColor y Code Generation)

**Objetivo**: Asegurar la correcta generación de variantes de color.

* **Caso de Prueba**: Verificar que la clase generada (`PrimaryColor.Darken3`) retorne un objeto `CssColor` con la variante y valor esperados, sin errores de compilación.