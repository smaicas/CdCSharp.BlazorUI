# Configuración de GitHub Repository

Este documento describe la configuración necesaria en GitHub para el flujo de trabajo CdCSharp.BlazorUI.

## Configuración Automática (Recomendado)

### 1. Crear Labels de Severity

Ve a **Actions > Setup Repository > Run workflow** y selecciona:
- ✅ Setup labels: `true`
- ⬜ Setup branch protection: `false` (por ahora)

Esto creará automáticamente los labels:

| Label | Color | Descripción |
|-------|-------|-------------|
| `severity:blocker` | 🔴 #d9534f | Bloquea el release - debe resolverse antes de publicar |
| `severity:critical` | 🔴 #e74c3c | Problema crítico - alta prioridad |
| `severity:major` | 🟠 #f39c12 | Problema importante - afecta funcionalidad |
| `severity:minor` | 🟡 #f1c40f | Problema menor - mejora deseable |
| `severity:polish` | 🟢 #2ecc71 | Refinamiento - calidad de vida |

### 2. Configurar Branch Protection

**Nota:** El workflow de branch protection requiere un token con permisos de administrador. Si falla, configura manualmente siguiendo las instrucciones abajo.

Para ejecutar:
- Ve a **Actions > Setup Repository > Run workflow**
- ✅ Setup labels: `false` (ya están creados)
- ✅ Setup branch protection: `true`

---

## Configuración Manual (Alternativa)

Si el workflow automático no funciona, configura manualmente:

### Branch Protection para `main`

Ve a **Settings > Branches > Add rule**

**Branch name pattern:** `main`

#### Protect matching branches

- ✅ **Require a pull request before merging**
  - ✅ Require approvals: `1`
  - ✅ Dismiss stale PR approvals when new commits are pushed
  - ⬜ Require review from Code Owners
  - ⬜ Restrict who can dismiss pull request reviews

- ✅ **Require status checks to pass before merging**
  - ✅ Require branches to be up to date before merging
  - Status checks that are required:
    - `build-and-test`
    - `coverage-check`

- ⬜ Require conversation resolution before merging

- ⬜ Require signed commits

- ⬜ Require linear history ✅ **IMPORTANTE: Activar esto**

- ⬜ Require deployments to succeed before merging

- ⬜ Require merge queue

- ⬜ Lock branch

- ⬜ Do not allow bypassing the above settings

#### Rules applied to everyone including administrators

- ⬜ Allow force pushes
- ⬜ Allow deletions

---

### Branch Protection para `develop`

Ve a **Settings > Branches > Add rule**

**Branch name pattern:** `develop`

#### Protect matching branches

- ✅ **Require a pull request before merging**
  - ✅ Require approvals: `1`
  - ✅ Dismiss stale PR approvals when new commits are pushed
  - ⬜ Require review from Code Owners
  - ⬜ Restrict who can dismiss pull request reviews

- ✅ **Require status checks to pass before merging**
  - ✅ Require branches to be up to date before merging
  - Status checks that are required:
    - `build-and-test`

- ⬜ Require conversation resolution before merging

- ⬜ Require signed commits

- ⬜ Require linear history ✅ **IMPORTANTE: Activar esto**

- ⬜ Require deployments to succeed before merging

- ⬜ Require merge queue

- ⬜ Lock branch

- ⬜ Do not allow bypassing the above settings

#### Rules applied to everyone including administrators

- ⬜ Allow force pushes
- ⬜ Allow deletions

---

## Configuración de Secrets

### NUGET_API_KEY

Ve a **Settings > Secrets and variables > Actions > New repository secret**

- **Name:** `NUGET_API_KEY`
- **Secret:** Tu API key de NuGet.org

Para obtener la API key:
1. Ve a https://www.nuget.org/
2. Login > Account > API Keys
3. Create new key
4. Selecciona scope: Push
5. Selecciona el package CdCSharp.BlazorUI
6. Copia la key y guárdala en el secret

---

## Configuración de Merge

Ve a **Settings > General > Pull Requests**

### Merge Button

- ⬜ Allow merge commits ❌ **Desactivar**
- ✅ **Allow squash merging** ✅ **Activar (default)**
  - Default commit message: `Default message`
- ⬜ Allow rebase merging ❌ **Desactivar**

### Merge Queue

- ⬜ Allow merge queue (opcional, para repos con mucho tráfico)

---

## Verificación

Después de configurar, verifica:

1. **Labels:** Settings > Labels - Deben aparecer los 5 labels de severity
2. **Branch protection:** Settings > Branches - Deben aparecer 2 reglas (main, develop)
3. **Secrets:** Settings > Secrets - Debe existir `NUGET_API_KEY`
4. **Merge settings:** Settings > General - Solo squash merge habilitado

---

## Flujo de Trabajo Esperado

Con esta configuración:

1. **Developer** crea feature branch desde develop
2. **Developer** trabaja y hace commits frecuentes
3. **Developer** ejecuta `./scripts/dev-tools.ps1 ready` (squash + rebase)
4. **Developer** crea PR → GitHub verifica: 1 commit, rebase hecho, CI verde
5. **Maintainer** revisa y hace squash merge
6. **GitHub** publica automáticamente preview en cada push a develop
7. **Maintainer** ejecuta `./scripts/admin-tools.ps1 release X.Y.Z` para estable

El historial será lineal sin "metro lines".
