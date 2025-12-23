const THEME_KEY = 'blazorui-theme';
const DEFAULT_THEME = 'dark';

export function initialize(defaultTheme?: string): void {
    const theme =
        localStorage.getItem(THEME_KEY) ??
        defaultTheme ??
        DEFAULT_THEME;

    document.documentElement.setAttribute('data-theme', theme);
}

export function getTheme(): string {
    return document.documentElement.getAttribute('data-theme')
        ?? DEFAULT_THEME;
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