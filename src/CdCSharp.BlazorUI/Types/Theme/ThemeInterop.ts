export function Load(): void {
    window['ThemeInterop'] = new ThemeModule.ThemeInteropClass();
}

export module ThemeModule {
    export class ThemeInteropClass {
        private readonly THEME_KEY = 'blazorui-theme';
        private readonly DEFAULT_THEME = 'light';

        /**
         * Get the current theme
         */
        public async Get(): Promise<string> {
            if (typeof Storage !== 'undefined') {
                return window.localStorage[this.THEME_KEY] || this.DEFAULT_THEME;
            }
            return this.DEFAULT_THEME;
        }

        /**
         * Set the theme
         */
        public async Set(theme: string): Promise<void> {
            if (typeof Storage !== 'undefined') {
                window.localStorage[this.THEME_KEY] = theme;
            }
            document.documentElement.setAttribute('data-theme', theme);
        }

        /**
         * Toggle between light and dark themes
         */
        public async Toggle(): Promise<string> {
            const currentTheme = await this.Get();
            const newTheme = currentTheme === 'light' ? 'dark' : 'light';
            await this.Set(newTheme);
            return newTheme;
        }

        /**
         * Initialize theme from user preferences
         */
        public async Initialize(): Promise<void> {
            // Check for saved preference
            const savedTheme = await this.Get();
            if (savedTheme) {
                await this.Set(savedTheme);
                return;
            }

            // Check system preference
            if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
                await this.Set('dark');
            }
        }

        /**
         * Listen for system theme changes
         */
        public ListenSystemChanges(): void {
            if (window.matchMedia) {
                window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', async (e) => {
                    // Only change if user hasn't set a preference
                    const savedTheme = window.localStorage[this.THEME_KEY];
                    if (!savedTheme) {
                        await this.Set(e.matches ? 'dark' : 'light');
                    }
                });
            }
        }
    }
}

// Auto-load on module import
Load();

// Initialize theme on DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', async () => {
        await window['ThemeInterop'].Initialize();
        window['ThemeInterop'].ListenSystemChanges();
    });
} else {
    // DOM is already loaded
    (async () => {
        await window['ThemeInterop'].Initialize();
        window['ThemeInterop'].ListenSystemChanges();
    })();
}