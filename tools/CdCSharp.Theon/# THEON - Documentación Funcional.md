# THEON - Documentación Funcional

## Descripción General

THEON es un sistema multi-agente para análisis de código que utiliza un backend LLM (vía LMStudio) para analizar repositorios de código fuente, generar documentación, responder consultas y producir archivos.

El sistema coordina agentes especializados dinámicamente creados según las necesidades de cada consulta, con capacidad de validación cruzada y mejora iterativa de respuestas.

---

## RF-01: Inicialización y Pre-análisis

### Descripción

Al ejecutarse sobre un directorio de proyecto, el sistema realiza un análisis estructural previo que permite a los agentes comprender la arquitectura del código sin necesidad de cargar todos los archivos en memoria.

### Comportamiento

1. Escanea recursivamente el directorio buscando archivos `.csproj`
2. Para cada proyecto encontrado:
   - Extrae referencias a paquetes NuGet y proyectos
   - Clasifica archivos por tipo: C# (.cs), Razor (.razor), TypeScript (.ts/.tsx), CSS (.css/.scss)
   - Identifica si es proyecto de test mediante referencias a xunit, nunit o mstest
3. Para proyectos no-test, ejecuta destructuración con Roslyn:
   - Extrae namespaces
   - Extrae tipos públicos e internos
   - Extrae miembros públicos e internos
   - Genera modelo con modificadores, tipos base y firmas
4. Genera archivos de salida en `{projectPath}/.theon/preanalysis/`

### Archivos generados

| Archivo | Contenido |
|---------|-----------|
| `{AssemblyName}.json` | Estructura completa en JSON |
| `{AssemblyName}.llm.txt` | Formato compacto optimizado para LLM |
| `structure.json` | Estructura global del proyecto |
| `project.llm.txt` | Resumen global para LLM |

### Filtrado de archivos

Respeta `dgignore.txt` o `.gitignore` en la raíz del proyecto.

Patrones excluidos por defecto: `bin/`, `obj/`, `.vs/`, `.vscode/`, `.idea/`, `.git/`, `node_modules/`, `packages/`, `TestResults/`, `*.user`, `*.suo`, `.DS_Store`, `_Imports.razor`

### Detección automática

| Patrón | Condición |
|--------|-----------|
| Blazor | Archivos .razor presentes |
| TypeScript | Archivos .ts/.tsx presentes |
| Entity Framework | Referencias a EntityFramework |

### Tipo de proyecto

Blazor > ASP.NET Core > WPF > Avalonia > Class Library (default)

---

## RF-02: Interfaz REPL

### Comandos

| Comando | Descripción |
|---------|-------------|
| `<texto>` | Procesa como consulta |
| `agents` | Lista agentes y estado |
| `metrics` | Muestra métricas |
| `save` | Guarda sesión |
| `load <id>` | Carga sesión |
| `sessions` | Lista sesiones |
| `help` | Ayuda |
| `exit` | Salir |

### Salida

Carpeta `.theon/responses/{numero}_{timestamp}_{slug}/` conteniendo:
- `response.md`: Respuesta completa con metadata
- Archivos generados por el agente

---

## RF-03: Orquestador Central

### Responsabilidades

1. Recibir consultas del usuario
2. Decidir estrategia de enrutamiento
3. Crear agentes dinámicamente
4. Ejecutar agentes y procesar solicitudes
5. Coordinar validación cruzada
6. Gestionar comunicación entre agentes

### Estrategias de enrutamiento

| Estrategia | Uso |
|------------|-----|
| `existing` | Reutilizar agente existente |
| `new` | Crear nuevo especialista |
| `direct` | Query trivial sin agente |

### Decisión de enrutamiento

Campos JSON requeridos:
- `targetExpertise`: expertise necesaria (nunca vacía)
- `suggestedFiles`: archivos para contexto inicial
- `reasoning`: justificación

### Fallback por palabras clave

| Palabra clave | Expertise asignada |
|---------------|-------------------|
| document, readme | code documentation |
| test | unit testing |
| refactor | code refactoring |
| bug, error | debugging |
| security | security analysis |
| performance | performance optimization |
| default | code analysis |

### Procesamiento de solicitudes

| Solicitud | Acción |
|-----------|--------|
| REQUEST_FILE_PATHS | Lista archivos |
| REQUEST_FILE | Obtiene contenido |
| CREATE_AGENT | Crea agente |
| QUERY_AGENT | Consulta otro agente |

Profundidad máxima: 5 niveles

---

## RF-04: Agentes Dinámicos

### Características

- Creación bajo demanda
- Especialización configurable
- Historial independiente
- Estados: Activo / Dormido

### Ciclo de vida

Creación → Activo → Dormido → Despertar → Activo

### Propiedades

| Propiedad | Descripción |
|-----------|-------------|
| Id | 8 caracteres hex |
| Name | Nombre descriptivo |
| Expertise | Área de especialización |
| Context | Contexto inicial |
| State | Active / Sleeping |
| ConversationHistory | Historial de mensajes |
| CreatedAt | Timestamp creación |
| LastActiveAt | Última actividad |

### Herramientas - Tags de línea única

| Tag | Sintaxis |
|-----|----------|
| REQUEST_FILE_PATHS | `[REQUEST_FILE_PATHS: assembly="nombre"]` |
| REQUEST_FILE | `[REQUEST_FILE: path="ruta/archivo.cs"]` |
| LIST_GENERATED_FILES | `[LIST_GENERATED_FILES]` |
| QUERY_AGENT | `[QUERY_AGENT: expertise="area" question="pregunta"]` |
| CREATE_AGENT | `[CREATE_AGENT: name="Nombre" expertise="area" files="a.cs,b.cs"]` |
| CONFIDENCE | `[CONFIDENCE: 0.85]` |
| SUGGEST_VALIDATION | `[SUGGEST_VALIDATION: expertise="area1,area2"]` |
| TASK_COMPLETE | `[TASK_COMPLETE]` |

### Herramientas - Tags de bloque

| Tag | Apertura | Cierre |
|-----|----------|--------|
| GENERATE_FILE | `[GENERATE_FILE: name="archivo.ext" language="lang"]` | `[/GENERATE_FILE]` |
| APPEND_TO_FILE | `[APPEND_TO_FILE: name="archivo.ext"]` | `[/APPEND_TO_FILE]` |

### Escala de confianza

| Rango | Significado |
|-------|-------------|
| 0.9-1.0 | Completa, sin lagunas |
| 0.7-0.9 | Buena, asunciones menores |
| 0.5-0.7 | Incompleta |
| <0.5 | Necesita revisión |

---

## RF-05: Gestión de Contexto

### Pre-análisis progresivo

- Estructura inicial cargada al inicio
- Detalle bajo demanda via REQUEST_FILE
- Formato optimizado para tokens

### Compresión de conversación

Cuando historial > umbral:
1. Preserva mensaje de sistema
2. Comprime N mensajes antiguos
3. Genera resumen estructurado
4. Mantiene M mensajes recientes

### Configuración

| Parámetro | Default |
|-----------|---------|
| CompressionThreshold | 10 |
| MessagesToCompress | 7 |
| MessagesToKeep | 3 |

### Tracking de archivos generados

Registro por agente: nombre, lenguaje, tamaño, timestamps, contenido actual

---

## RF-06: Validación Cruzada

### Activación

- Confianza < umbral (default: 0.7)
- Agente usa SUGGEST_VALIDATION

### Flujo

1. Detectar necesidad de validación
2. Determinar expertise del validador
3. Crear/reutilizar agente validador
4. Validador revisa respuesta
5. Si aprueba (confidence >= threshold): termina
6. Si objeta: agente original corrige
7. Repetir hasta aprobación o máximo iteraciones

### Expertise del validador

| Original contiene | Validador |
|-------------------|-----------|
| security | security review |
| performance | performance review |
| blazor | Blazor best practices |
| api | API design review |
| database | database design review |
| test | test coverage review |
| otro | code quality review |

### Configuración

| Parámetro | Default |
|-----------|---------|
| MaxIterations | 3 |
| ConfidenceThreshold | 0.7 |

---

## RF-07: Generación de Respuestas

### Estructura de salida

```
.theon/responses/{numero}_{timestamp}_{slug}/
├── response.md
└── {archivos generados}
```

### Contenido de response.md

- Query original
- Timestamp
- Agentes involucrados
- Confianza final
- Tiempo de procesamiento
- Rondas de validación
- Contenido de respuesta
- Archivos generados con preview

---

## RF-08: Validación de Formato

### Validaciones

| Tipo | Error |
|------|-------|
| Tags sin cerrar | GENERATE_FILE/APPEND_TO_FILE sin cierre |
| Tags anidados | GENERATE_FILE dentro de otro |
| Confianza ausente | Sin tag CONFIDENCE |
| Archivos no parseados | Tags presentes pero sin extracción |

### Proceso de corrección

1. Detectar errores
2. Solicitar corrección al agente
3. Reintentar (máximo 2 veces)
4. Registrar errores persistentes

---

## RF-09: Gestión de Sesiones

### Estado de sesión

- ID único
- Timestamp
- Ruta del proyecto
- Estado de agentes (completo)
- Historial del Orquestador
- Métricas
- Archivos generados

### Persistencia

Ubicación: `.theon/sessions/session_{id}_{timestamp}.json`

### Operaciones

| Operación | Descripción |
|-----------|-------------|
| SaveSessionAsync | Guarda a disco |
| LoadSessionAsync | Restaura por ID/ruta |
| ListSessions | Lista disponibles |
| RestoreAgents | Reconstruye agentes |

---

## RF-10: Métricas y Logging

### Métricas por agente

- Tokens entrada/salida
- Requests totales/exitosos
- Tiempo acumulado

### Métricas globales

- Tasa de éxito
- Tiempo promedio por tipo
- Tasa de aprobación validaciones
- Tokens totales
- Queries totales

### Niveles de log

Trace < Debug < Info < Warning < Error

### Archivos

| Archivo | Contenido |
|---------|-----------|
| `logs/theon_{timestamp}.log` | Log principal |
| `traces/{n}_{timestamp}_{agentId}.md` | Trazas prompt/respuesta |

---

## RF-11: Configuración

### TheonOptions

| Sección | Opción | Default |
|---------|--------|---------|
| General | ProjectPath | (requerido) |
| General | OutputPath | .theon |
| LMStudio | BaseUrl | http://localhost:1234/v1/ |
| LMStudio | TimeoutSeconds | 7200 |
| Conversation | CompressionThreshold | 10 |
| Conversation | MessagesToCompress | 7 |
| Conversation | MessagesToKeep | 3 |
| Validation | MaxIterations | 3 |
| Validation | ConfidenceThreshold | 0.7 |

---

## Arquitectura

### Componentes principales

```
Orchestrator
├── LMStudioClient
├── AgentRegistry
├── AgentFactory
├── AgentExecutor
├── FileAccessTool
├── FileOutputTool
├── SessionManager
├── MetricsCollector
├── GeneratedFilesTracker
└── TheonLogger
```

### Flujo de query

```
Usuario → Orchestrator
           ├── DecideRoutingAsync → LMStudioClient
           ├── GetOrCreateAgentAsync → AgentFactory → AgentRegistry
           ├── ExecuteWithRequestsAsync → AgentExecutor → LMStudioClient
           ├── [ValidateAndImproveAsync] → Validador + Agente original
           └── SaveResponseAsync → FileOutputTool
```