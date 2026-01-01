const CULTURE_KEY = 'BlazorUI.Culture';

export function getStoredCulture(): string | null {
    return localStorage.getItem(CULTURE_KEY);
}

export function setStoredCulture(culture: string): void {
    localStorage.setItem(CULTURE_KEY, culture);
}

if (typeof window !== "undefined") {
    (window as any).blazorCulture = {
        get: () => getStoredCulture(),
        set: (value: string) => setStoredCulture(value)
    };
}