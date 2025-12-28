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
    // 2. System preference
    // 3. defaultTheme parameter (if provided)
    // 4. DEFAULT_THEME constant ('dark')
    const savedTheme = localStorage.getItem(THEME_KEY);
    const theme = savedTheme ?? getSystemPreference();

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