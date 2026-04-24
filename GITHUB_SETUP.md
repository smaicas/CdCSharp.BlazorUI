# Configuración de GitHub Repository

Guía para configurar el repositorio CdCSharp.BlazorUI (o cualquier otro con la misma estructura).

## Opción 1: Script PowerShell (Recomendado)

### 1. Crear GitHub Personal Access Token

1. Ve a https://github.com/settings/tokens
2. Click **Generate new token (classic)**
3. Selecciona scopes: `repo` (full control)
4. Copia el token (se muestra solo una vez)

### 2. Ejecutar el script

```powershell
# Desde la raíz del repositorio
./scripts/github-setup.ps1 `
    -Owner smaicas `
    -Repo CdCSharp.BlazorUI `
    -Token ghp_tu_token_aqui `
    -SetupLabels `
    -SetupBranchProtection
```

O solo labels (si no tienes permisos de admin):

```powershell
./scripts/github-setup.ps1 `
    -Owner smaicas `
    -Repo CdCSharp.BlazorUI `
    -Token ghp_tu_token_aqui `
    -SetupLabels
```

### 3. Configurar secret NUGET_API_KEY

```powershell
# Usando GitHub CLI (opcional)
gh secret set NUGET_API_KEY --body "tu-api-key-aqui"
```

O manualmente en: **Settings > Secrets and variables > Actions > New repository secret**

---

## Opción 2: Configuración Manual

### Paso 1: Crear Labels

Ve a **Issues > Labels > New label** y crea:

| Name | Color | Description |
|------|-------|-------------|
| `severity:blocker` | `#d9534f` | Bloquea el release - debe resolverse antes de publicar |
| `severity:critical` | `#e74c3c` | Problema crítico - alta prioridad |
| `severity:major` | `#f39c12` | Problema importante - afecta funcionalidad |
| `severity:minor` | `#f1c40f` | Problema menor - mejora deseable |
| `severity:polish` | `#2ecc71` | Refinamiento - calidad de vida |

### Paso 2: Crear rama `develop` (si no existe)

```bash
# Desde tu repo local
git checkout master
git pull origin master
git checkout -b develop
git push -u origin develop
```

O el script lo hará automáticamente.

### Paso 3: Branch Protection para `master`

Ve a **Settings > Branches > Add rule**

**Branch name pattern:** `master`

#### Protect matching branches

- ✅ **Require a pull request before merging**
  - ✅ Require approvals: `1`
  - ✅ Dismiss stale PR approvals when new commits are pushed
  - ⬜ Require review from Code Owners

- ✅ **Require status checks to pass before merging**
  - ✅ Require branches to be up to date before merging
  - Status checks that are required:
    - `Release Gate / Build Check`
    - `Release Gate / Check Blocking Issues`
    - `Release Gate / Check Public API`

- ⬜ Require conversation resolution before merging
- ⬜ Require signed commits
- ✅ **Require linear history** ← IMPORTANTE
- ⬜ Require deployments to succeed before merging
- ⬜ Lock branch
- ⬜ Do not allow bypassing the above settings

#### Rules applied to everyone including administrators

- ⬜ Allow force pushes
- ⬜ Allow deletions

### Paso 4: Branch Protection para `develop`

Ve a **Settings > Branches > Add rule**

**Branch name pattern:** `develop`

#### Protect matching branches

- ✅ **Require a pull request before merging**
  - ✅ Require approvals: `1`
  - ✅ Dismiss stale PR approvals when new commits are pushed

- ✅ **Require status checks to pass before merging**
  - ✅ Require branches to be up to date before merging
  - Status checks that are required:
    - `Preview Gate / Build & Test`
    - `Preview Gate / Code Coverage`
    - `Preview Gate / Check Public API`

- ⬜ Require conversation resolution before merging
- ⬜ Require signed commits
- ✅ **Require linear history** ← IMPORTANTE
- ⬜ Require deployments to succeed before merging
- ⬜ Lock branch
- ⬜ Do not allow bypassing the above settings

#### Rules applied to everyone including administrators

- ⬜ Allow force pushes
- ⬜ Allow deletions

### Paso 5: Configurar Merge

Ve a **Settings > General > Pull Requests**

#### Merge Button

- ⬜ Allow merge commits ❌ **Desactivar**
- ✅ **Allow squash merging** ✅ **Activar (default)**
  - Default commit message: `Default message`
- ⬜ Allow rebase merging ❌ **Desactivar**

### Paso 6: Configurar Secret NUGET_API_KEY

Ve a **Settings > Secrets and variables > Actions > New repository secret**

- **Name:** `NUGET_API_KEY`
- **Secret:** Tu API key de NuGet.org

Para obtener la API key:
1. Ve a https://www.nuget.org/
2. Login > Account > API Keys
3. Create new key
4. Selecciona scope: Push
5. Selecciona el package CdCSharp.BlazorUI
6. Copia la key

---

## Verificación

Después de configurar, verifica:

1. **Labels:** https://github.com/OWNER/REPO/labels (deben aparecer 5 labels de severity)
2. **Branch protection:** Settings > Branches (deben aparecer 2 reglas)
3. **Secrets:** Settings > Secrets (debe existir `NUGET_API_KEY`)
4. **Merge settings:** Solo squash merge habilitado

---

## Uso en otros repositorios

El script `github-setup.ps1` es reutilizable. Solo cambia los parámetros:

```powershell
./scripts/github-setup.ps1 -Owner mi-org -Repo mi-repo -Token ghp_xxx -SetupLabels -SetupBranchProtection
```

**Nota:** Los nombres de los status checks (`Release Gate / Build Check`, etc.) deben coincidir con los workflows del repositorio. Si usas otros nombres, modifica el script o configura manualmente.
