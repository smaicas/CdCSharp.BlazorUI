# VERSIONING.md — Estrategia de Versionado CdCSharp.BlazorUI

> Flujo de trabajo simplificado con historial lineal. Sin "vías de metro".

---

## 1. Filosofía

- **Una rama de integración**: `develop`
- **Historial lineal**: solo fast-forward o rebase
- **PRs cortos**: máximo 1-3 días de vida
- **Una tarea = un commit** (squash de WIPs antes del push)
- **Issues vinculadas**: cada commit referencia una issue

---

## 2. Estructura de Ramas

```
main     ●────●────●────●────●────●────►  (releases taggeadas)
         ↑    ↑    ↑    ↑    ↑    ↑
develop  ●────┬────┬────┬────┬────┬────►  (integración continua)
              │    │    │    │    │
              ▼    ▼    ▼    ▼    ▼
             PR   PR   PR   PR   PR    (cortas, squash merge)
```

| Rama | Propósito | Protección | Merge |
|------|-----------|------------|-------|
| `main` | Releases estables | ✅ PR obligatorio, 1 approval, CI verde | Fast-forward only |
| `develop` | Integración continua | ✅ PR obligatorio, CI verde, squash merge | Squash merge |
| `feature/*` | Cambios aislados | ❌ No protegida | Squash a develop |

---

## 3. Versionado (SemVer)

| Patrón | Significado | Ejemplo |
|--------|-------------|---------|
| `X.Y.Z-preview.N` | Desarrollo activo | `1.0.0-preview.42` |
| `X.Y.Z-rc.N` | Release candidate | `1.0.0-rc.2` |
| `X.Y.Z` | Release estable | `1.0.0` |

### Reglas de bump

| Cambio | Versión | Ejemplo |
|--------|---------|---------|
| Breaking change | Major | `1.0.0` → `2.0.0` |
| Nueva feature | Minor | `1.0.0` → `1.1.0` |
| Bugfix | Patch | `1.0.0` → `1.0.1` |

---

## 4. Flujo de Trabajo

### 4.1 Para Colaboradores (Desarrolladores)

#### Opción A: Trabajo directo en develop (recomendado)

```powershell
# 1. Actualizar develop
./scripts/dev-tools.ps1 sync

# 2. Trabajar, hacer commits frecuentes (incluso WIPs)
git commit -m "wip: empezando tokens de scrollbar"
git commit -m "wip: integrando con FeatureDefinitions"
git commit -m "fix: corregir fallback de color"

# 3. Antes de push: limpiar historial (squash en un commit)
./scripts/dev-tools.ps1 squash 3  # squash últimos 3 commits

# 4. Push
./scripts/dev-tools.ps1 push
```

#### Opción B: Feature branch corta (para cambios arriesgados)

```powershell
# 1. Crear feature branch
./scripts/dev-tools.ps1 feature css-tokens

# 2. Trabajar (máximo 1-3 días)
# ... commits ...

# 3. Preparar para PR (rebase + squash)
./scripts/dev-tools.ps1 ready

# 4. Crear PR y eliminar rama tras merge
./scripts/dev-tools.ps1 pr "Título del PR" "Descripción del PR"
```

### 4.2 Para Administradores (Maintainers)

#### Publicar Preview

```powershell
# Cada push a develop publica automáticamente una preview
# Versión calculada: 1.1.0-preview.N (N = commits desde último tag)
```

#### Crear Release Candidate

```powershell
# Cuando develop está estable
./scripts/admin-tools.ps1 rc 1.0.0

# Esto:
# 1. Crea rama release/1.0.0 desde develop
# 2. Publica 1.0.0-rc.1
# 3. Abre milestone v1.0.0-rc
```

#### Publicar Release Estable

```powershell
# Cuando RC está aprobada
./scripts/admin-tools.ps1 release 1.0.0

# Esto:
# 1. Fast-forward merge de release/1.0.0 a main
# 2. Tag v1.0.0
# 3. Publica paquete estable
# 4. Merge back a develop
# 5. Cierra milestone
```

#### Hotfix de Emergencia

```powershell
# Para bugs críticos en producción
./scripts/admin-tools.ps1 hotfix 1.0.1

# Esto:
# 1. Crea hotfix/1.0.1 desde main
# 2. Trabajas el fix
# 3. Al terminar: publica y merge a main + develop
```

---

## 5. Convención de Commits

```
<tipo>(<scope>): <descripción>

<cuerpo opcional>

Fixes #<issue-number>
```

### Tipos

| Tipo | Uso | Ejemplo |
|------|-----|---------|
| `feat` | Nueva feature | `feat(css): add scrollbar tokens` |
| `fix` | Bugfix | `fix(button): resolve aria-label issue` |
| `docs` | Documentación | `docs(readme): update installation` |
| `test` | Tests | `test(dropdown): add a11y tests` |
| `refactor` | Refactor | `refactor(builder): optimize BuildStyles` |
| `chore` | Tareas | `chore(ci): update workflow` |
| `breaking` | Breaking change | `breaking(api): rename FeatureDefinitions` |

### Scopes comunes

- `css` — CSS, bundling, tokens
- `components` — Componentes Blazor
- `core` — CdCSharp.BlazorUI.Core
- `ci` — GitHub Actions, workflows
- `build` — BuildTools, generadores
- `tests` — Proyectos de test
- `docs` — Documentación

---

## 6. Relación Issues ↔ Commits

### En el mensaje de commit

```bash
# Cierra la issue automáticamente
Fixes #42

# Referencia sin cerrar
Relates to #55

# Cierra múltiples issues
Fixes #42, Fixes #43
```

### En el PR

```markdown
## Descripción
Implementa el sistema de tokens para scrollbars.

## Cambios
- Add ScrollbarCssGenerator
- Scope scrollbar styles bajo [data-bui-scrollbars]

## Issues
Closes #42
Closes #43

## Checklist
- [x] Tests pasan
- [x] CHANGELOG.md actualizado
- [x] Breaking changes documentados
```

---

## 7. Scripts de Automatización

### 7.1 Para Colaboradores: `scripts/dev-tools.ps1`

| Comando | Descripción |
|---------|-------------|
| `sync` | Actualiza develop desde origin |
| `feature <nombre>` | Crea feature branch desde develop |
| `squash <n>` | Squash últimos N commits en uno |
| `ready` | Rebase sobre develop + squash |
| `push` | Push seguro con force-with-lease |
| `pr <título> <desc>` | Crea PR y limpia |
| `cleanup` | Elimina ramas locales mergeadas |

### 7.2 Para Administradores: `scripts/admin-tools.ps1`

| Comando | Descripción |
|---------|-------------|
| `rc <version>` | Crea release candidate |
| `release <version>` | Publica release estable |
| `hotfix <version>` | Crea hotfix branch |
| `status` | Muestra estado de ramas y versiones |
| `changelog` | Genera CHANGELOG.md desde tags |

---

## 8. Configuración de GitHub

### 8.1 Branch Protection Rules

#### `main`

```yaml
Require pull request reviews: 1
Require status checks: true
  - ci/build
  - ci/test
Require branches to be up to date: true
Restrict pushes: true
  - Allowed: maintainers, admin
Allow force pushes: false
Allow deletions: false
Require linear history: true  # ← Fast-forward only
```

#### `develop`

```yaml
Require pull request reviews: 0  # Opcional para develop
Require status checks: true
  - ci/build
  - ci/test
Require branches to be up to date: true
Allow force pushes: false
Allow deletions: false
Require linear history: true  # ← Rebase/squash only
```

### 8.2 Labels de Severidad

| Label | Color | Hex | Uso |
|-------|-------|-----|-----|
| `severity:blocker` | 🔴 Rojo | `#B60205` | Bloquea cualquier release |
| `severity:critical` | 🟠 Naranja | `#D93F0B` | Debe resolverse antes de RC |
| `severity:major` | 🟡 Amarillo | `#FBCA04` | Feature importante |
| `severity:minor` | 🔵 Azul | `#0E8A16` | Mejora |
| `severity:polish` | ⚪ Gris | `#CCCCCC` | Nice-to-have |

### 8.3 Labels de Tipo

| Label | Color | Uso |
|-------|-------|-----|
| `type:feat` | 💚 Verde | Nueva feature |
| `type:fix` | 🔴 Rojo | Bugfix |
| `type:docs` | 🔵 Azul | Documentación |
| `type:test` | 🟣 Púrpura | Tests |
| `type:refactor` | 🟡 Amarillo | Refactor |
| `type:chore` | ⚪ Gris | Tareas |
| `type:breaking` | 🟠 Naranja | Breaking change |

### 8.4 Milestones

| Nombre | Descripción | Estado |
|--------|-------------|--------|
| `v1.0.0-preview` | Desarrollo continuo | Abierto permanente |
| `v1.0.0-rc` | Release candidate | Se abre al preparar RC |
| `v1.0.0` | Release estable | Se cierra al publicar |

---

## 9. Workflows de GitHub Actions

### 9.1 Preview (automático)

Archivo: `.github/workflows/preview.yml`

```yaml
on:
  push:
    branches: [develop]

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      
      - name: Calculate version
        run: |
          LAST_TAG=$(git describe --tags --abbrev=0 --match "v[0-9]*.[0-9]*.[0-9]" origin/main 2>/dev/null || echo "v0.0.0")
          MAJOR=$(echo $LAST_TAG | cut -d. -f1 | tr -d 'v')
          MINOR=$(echo $LAST_TAG | cut -d. -f2)
          NEXT_MINOR=$((MINOR + 1))
          COMMITS=$(git rev-list --count $LAST_TAG..HEAD)
          echo "VERSION=${MAJOR}.${NEXT_MINOR}.0-preview.${COMMITS}" >> $GITHUB_ENV
      
      - run: dotnet pack -p:Version=$VERSION -o ./artifacts
      - run: dotnet nuget push ./artifacts/*.nupkg ...
```

### 9.2 Release (manual)

Archivo: `.github/workflows/release.yml`

```yaml
on:
  push:
    tags:
      - 'v[0-9]+.[0-9]+.[0-9]+'
      - 'v[0-9]+.[0-9]+.[0-9]+-rc.[0-9]+'

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: dotnet pack -p:Version=${GITHUB_REF#refs/tags/v} -o ./artifacts
      - run: dotnet nuget push ./artifacts/*.nupkg ...
      - uses: softprops/action-gh-release@v2
        with:
          files: ./artifacts/*.nupkg
          generate_release_notes: false
          body_path: ./RELEASE_NOTES.md
```

---

## 10. Checklist de Release

Antes de publicar una release estable:

- [ ] Todas las issues `severity:blocker` cerradas
- [ ] Todas las issues `severity:critical` cerradas
- [ ] CI verde (build + tests)
- [ ] Cobertura de tests ≥ umbral definido
- [ ] CHANGELOG.md actualizado
- [ ] PublicAPI.Shipped.txt actualizado (si aplica)
- [ ] README.md actualizado (si aplica)
- [ ] Versión en `Directory.Build.props` actualizada
- [ ] Tag creado con formato `vX.Y.Z`
- [ ] Release notes escritas

---

## 11. Alias de Git Recomendados

Agregar a `~/.gitconfig`:

```ini
[alias]
    # Ver historial limpio
    lg = log --graph --pretty=format:'%Cred%h%Creset -%C(yellow)%d%Creset %s %Cgreen(%cr) %C(bold blue)<%an>%Creset' --abbrev-commit
    
    # Squash últimos N commits
    squash = "!f() { git reset --soft HEAD~$1 && git commit; }; f"
    
    # Push seguro
    pushf = push --force-with-lease
    
    # Sync develop
    sync = "!git checkout develop && git pull origin develop"
    
    # Limpiar ramas locales mergeadas
    cleanup = "!git branch --merged develop | grep -v 'develop$' | xargs git branch -d"
```

---

## 12. Ejemplos de Uso

### Escenario 1: Bugfix rápido

```powershell
# Colaborador
./scripts/dev-tools.ps1 sync
# ... edita archivos ...
git commit -m "fix(button): resolve aria-label when loading

Fixes #123"
./scripts/dev-tools.ps1 push
```

### Escenario 2: Feature compleja (3 días)

```powershell
# Colaborador
./scripts/dev-tools.ps1 feature theme-system
# Día 1
git commit -m "wip: add theme provider"
# Día 2
git commit -m "wip: integrate with components"
# Día 3
git commit -m "fix: dark mode toggle"
./scripts/dev-tools.ps1 ready
./scripts/dev-tools.ps1 pr "feat: implement theme system" "Closes #456"
```

### Escenario 3: Release

```powershell
# Administrador
./scripts/admin-tools.ps1 status
# → develop: 15 commits desde v0.9.0
# → issues blocker: 0, critical: 0

./scripts/admin-tools.ps1 rc 1.0.0
# ... testing de RC ...

./scripts/admin-tools.ps1 release 1.0.0
# → Publicado v1.0.0 en NuGet
```

---

## Referencias

- [Semantic Versioning](https://semver.org/)
- [Conventional Commits](https://www.conventionalcommits.org/)
- [GitHub Flow](https://docs.github.com/en/get-started/quickstart/github-flow)
- [Trunk-Based Development](https://trunkbaseddevelopment.com/)

---

*Última actualización: 2026-04-24*
*Mantenedor: Samuel Maícas (@cdcsharp)*
