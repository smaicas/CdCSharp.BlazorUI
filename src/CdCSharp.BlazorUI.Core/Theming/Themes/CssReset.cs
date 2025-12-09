namespace CdCSharp.BlazorUI.Core.Theming.Themes;

public static class CssReset
{
    public static string GetResetCss() => """
        /* CSS Reset */
        *, *::before, *::after {
            box-sizing: border-box;
        }
        
        * {
            margin: 0;
        }
        
        html {
            block-size: 100%;
            font-size: var(--font-size, 16px);
        }
        
        body {
            min-height: 100vh;
            line-height: 1.5;
            font-family: var(--font-family);
            font-size: 1rem;
            padding: 0;
            margin: 0;
            background-color: var(--background);
            color: var(--foreground);
            -webkit-font-smoothing: antialiased;
        }
        
        h1, h2, h3, h4, h5, h6 {
            font-family: var(--heading-font-family);
            font-weight: 600;
            line-height: 1.2;
        }
        
        h1 { font-size: 2.5rem; }
        h2 { font-size: 2rem; }
        h3 { font-size: 1.75rem; }
        h4 { font-size: 1.5rem; }
        h5 { font-size: 1.25rem; }
        h6 { font-size: 1rem; }
        
        p {
            font-size: 1rem;
            margin-block-end: 1em;
        }
        
        img, picture, video, canvas, svg {
            display: block;
            max-width: 100%;
        }
        
        input, button, textarea, select {
            font: inherit;
            color: inherit;
        }
        
        button {
            background: none;
            border: none;
            padding: 0;
            cursor: pointer;
        }
        
        a {
            color: inherit;
            text-decoration: none;
        }
        
        /* Utility classes for theming */
        :root {
            /* Map current theme variables to generic names */
            --primary: var(--light-primary);
            --primary-contrast: var(--light-primary-contrast);
            --secondary: var(--light-secondary);
            --secondary-contrast: var(--light-secondary-contrast);
            --background: var(--light-background);
            --surface: var(--light-surface);
            --foreground: var(--light-foreground);
            --error: var(--light-error);
            --success: var(--light-success);
            --warning: var(--light-warning);
            --info: var(--light-info);
            --border-color: var(--light-border-color);
            
            --font-size: var(--light-font-size);
            --font-family: var(--light-font-family);
            --heading-font-family: var(--light-heading-font-family);
            
            --spacing-xs: var(--light-spacing-xs);
            --spacing-sm: var(--light-spacing-sm);
            --spacing-md: var(--light-spacing-md);
            --spacing-lg: var(--light-spacing-lg);
            --spacing-xl: var(--light-spacing-xl);
            
            --border-radius: var(--light-border-radius);
        }
        
        [data-theme="dark"] {
            --primary: var(--dark-primary);
            --primary-contrast: var(--dark-primary-contrast);
            --secondary: var(--dark-secondary);
            --secondary-contrast: var(--dark-secondary-contrast);
            --background: var(--dark-background);
            --surface: var(--dark-surface);
            --foreground: var(--dark-foreground);
            --error: var(--dark-error);
            --success: var(--dark-success);
            --warning: var(--dark-warning);
            --info: var(--dark-info);
            --border-color: var(--dark-border-color);
            
            --font-size: var(--dark-font-size);
            --font-family: var(--dark-font-family);
            --heading-font-family: var(--dark-heading-font-family);
            
            --spacing-xs: var(--dark-spacing-xs);
            --spacing-sm: var(--dark-spacing-sm);
            --spacing-md: var(--dark-spacing-md);
            --spacing-lg: var(--dark-spacing-lg);
            --spacing-xl: var(--dark-spacing-xl);
            
            --border-radius: var(--dark-border-radius);
        }
        """;
}
