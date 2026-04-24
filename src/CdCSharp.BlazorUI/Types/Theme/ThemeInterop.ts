const THEME_KEY = 'blazorui-theme';
const DEFAULT_THEME = 'dark';

function getSystemPreference(): string {
    return window.matchMedia('(prefers-color-scheme: dark)').matches
        ? 'dark'
        : 'light';
}

export function initialize(defaultTheme?: string): void {
    // Priority order:
    // 1. localStorage (user's manual selection)
    // 2. defaultTheme parameter (if provided by the consumer)
    // 3. System preference (prefers-color-scheme)
    // 4. DEFAULT_THEME constant ('dark')
    const savedTheme = localStorage.getItem(THEME_KEY);
    const theme = savedTheme ?? defaultTheme ?? getSystemPreference() ?? DEFAULT_THEME;

    document.documentElement.setAttribute('data-theme', theme);
}

export function getTheme(): string {
    return document.documentElement.getAttribute('data-theme') ?? DEFAULT_THEME;
}

export function setTheme(theme: string): void {
    localStorage.setItem(THEME_KEY, theme);
    document.documentElement.setAttribute('data-theme', theme);
}

export function toggleTheme(themes: string[]): string {
    const current = getTheme();
    const index = themes.indexOf(current);
    const next = themes[(index + 1) % themes.length];
    setTheme(next);
    return next;
}

// Kept in sync with BUIThemePaletteBase. Canonical source lives in C#; if a property is
// added/removed there, update this list too (see JS-06 for history and automation plan).
const PALETTE_VARS = [
    '--palette-activetint',
    '--palette-background',
    '--palette-backgroundcontrast',
    '--palette-border',
    '--palette-error',
    '--palette-errorcontrast',
    '--palette-highlight',
    '--palette-hovertint',
    '--palette-info',
    '--palette-infocontrast',
    '--palette-primary',
    '--palette-primarycontrast',
    '--palette-secondary',
    '--palette-secondarycontrast',
    '--palette-shadow',
    '--palette-success',
    '--palette-successcontrast',
    '--palette-surface',
    '--palette-surfacecontrast',
    '--palette-warning',
    '--palette-warningcontrast'
];

function waitForStyles(): Promise<void> {
    if (document.readyState === 'complete') {
        return Promise.resolve();
    }
    return new Promise(resolve => {
        window.addEventListener('load', () => resolve(), { once: true });
    });
}

export async function getPalette(): Promise<Record<string, string>> {
    await waitForStyles();

    return new Promise(resolve => {
        requestAnimationFrame(() => {
            const style = getComputedStyle(document.documentElement);
            const result: Record<string, string> = {};
            for (const v of PALETTE_VARS) {
                result[v] = style.getPropertyValue(v).trim();
            }
            resolve(result);
        });
    });
}