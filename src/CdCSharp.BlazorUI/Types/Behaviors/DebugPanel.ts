// ============================================================
// DEBUG PANEL - TypeScript Module
// Todas las funciones se aplican al DOM global (document/body)
// ============================================================

interface DotNetReference {
    invokeMethodAsync(methodName: string, ...args: any[]): Promise<any>;
}

interface OverlayElements {
    grid?: HTMLDivElement;
    columns?: HTMLDivElement;
    breakpoints?: HTMLDivElement;
}

interface PerformanceData {
    fps: number;
    frameCount: number;
    lastTime: number;
    memory: number;
}

interface CssVariable {
    name: string;
    value: string;
}

interface AccessibilityIssue {
    severity: 'error' | 'warning' | 'info';
    message: string;
    count: number;
}

interface AccessibilityReport {
    issues: AccessibilityIssue[];
}

interface InspectorElementInfo {
    tagName: string;
    id: string;
    classList: string[];
    role: string;
    ariaLabel: string;
    alt: string;
    display: string;
    width: number;
    height: number;
    textContent: string;
}

type ColorBlindMode = 'none' | 'protanopia' | 'deuteranopia' | 'tritanopia';

// ============================================================
// VARIABLES
// ============================================================

let dotNetRef: DotNetReference | null = null;
let overlayElements: OverlayElements = {};
let performanceData: PerformanceData = {
    fps: 0,
    frameCount: 0,
    lastTime: performance.now(),
    memory: 0
};
let animationFrameId: number | null = null;
let hoverInfoElement: HTMLDivElement | null = null;
let inspectorActive = false;
let originalCssValues: Map<string, string> = new Map();
let originalFetch: typeof fetch | null = null;
let cpuThrottleInterval: ReturnType<typeof setInterval> | null = null;

// ============================================================
// INICIALIZACIÓN
// ============================================================

export function initialize(dotNetReference: DotNetReference): void {
    dotNetRef = dotNetReference;
    document.addEventListener('keydown', handleHotkey);
    startPerformanceLoop();
    createOverlays();
    console.log('DebugPanel inicializado correctamente');
}

function handleHotkey(e: KeyboardEvent): void {
    if (e.ctrlKey && e.key === 'd') {
        e.preventDefault();
        dotNetRef?.invokeMethodAsync('TogglePanel');
    }
}

// ============================================================
// TOGGLE PANEL
// ============================================================

export function togglePanel(isOpen: boolean): void {
    console.log('Toggle panel:', isOpen);
}

// ============================================================
// CREACIÓN DE OVERLAYS
// ============================================================

function createOverlays(): void {
    // Grid Overlay
    if (!document.getElementById('debug-grid-overlay')) {
        const grid = document.createElement('div');
        grid.id = 'debug-grid-overlay';
        grid.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            pointer-events: none;
            z-index: 999997;
            display: none;
            background-image: 
                repeating-linear-gradient(0deg, rgba(100, 150, 255, 0.15) 0px, transparent 1px, transparent 20px, rgba(100, 150, 255, 0.15) 21px),
                repeating-linear-gradient(90deg, rgba(100, 150, 255, 0.15) 0px, transparent 1px, transparent 20px, rgba(100, 150, 255, 0.15) 21px);
            background-size: 20px 20px;
        `;
        document.body.appendChild(grid);
        overlayElements.grid = grid;
    }

    // Columns Overlay
    if (!document.getElementById('debug-columns-overlay')) {
        const columns = document.createElement('div');
        columns.id = 'debug-columns-overlay';
        columns.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            pointer-events: none;
            z-index: 999997;
            display: none;
            padding: 0 20px;
        `;

        const grid = document.createElement('div');
        grid.style.cssText = `
            display: grid;
            grid-template-columns: repeat(12, 1fr);
            gap: 20px;
            height: 100%;
        `;

        for (let i = 0; i < 12; i++) {
            const col = document.createElement('div');
            col.style.cssText = `
                background: rgba(100, 150, 255, 0.1);
                border: 1px dashed rgba(100, 150, 255, 0.3);
            `;
            grid.appendChild(col);
        }

        columns.appendChild(grid);
        document.body.appendChild(columns);
        overlayElements.columns = columns;
    }

    // Breakpoints Overlay
    if (!document.getElementById('debug-breakpoints-overlay')) {
        const breakpoints = document.createElement('div');
        breakpoints.id = 'debug-breakpoints-overlay';
        breakpoints.style.cssText = `
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            pointer-events: none;
            z-index: 999997;
            display: none;
        `;

        const bps = [
            { width: 640, label: 'SM', color: '#ff6b6b' },
            { width: 768, label: 'MD', color: '#4ecdc4' },
            { width: 1024, label: 'LG', color: '#45b7d1' },
            { width: 1280, label: 'XL', color: '#96ceb4' },
            { width: 1536, label: '2XL', color: '#ffeaa7' }
        ];

        bps.forEach(bp => {
            const line = document.createElement('div');
            line.style.cssText = `
                position: absolute;
                left: ${bp.width}px;
                top: 0;
                width: 2px;
                height: 100%;
                background: ${bp.color};
            `;

            const label = document.createElement('div');
            label.textContent = `${bp.label}\n${bp.width}px`;
            label.style.cssText = `
                position: sticky;
                top: 20px;
                display: inline-block;
                padding: 6px 10px;
                background: ${bp.color};
                color: white;
                font-size: 11px;
                font-weight: bold;
                border-radius: 4px;
                transform: translateX(-50%);
                white-space: pre;
                text-align: center;
                line-height: 1.3;
            `;

            line.appendChild(label);
            breakpoints.appendChild(line);
        });

        document.body.appendChild(breakpoints);
        overlayElements.breakpoints = breakpoints;
    }

    // Hover Info Element
    if (!document.getElementById('debug-hover-info')) {
        hoverInfoElement = document.createElement('div');
        hoverInfoElement.id = 'debug-hover-info';
        hoverInfoElement.style.cssText = `
            position: absolute;
            background: rgba(20, 20, 30, 0.98);
            border: 1px solid rgba(100, 150, 255, 0.5);
            border-radius: 8px;
            padding: 12px;
            font-size: 11px;
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            color: #e0e0e0;
            pointer-events: none;
            z-index: 1000000;
            display: none;
            max-width: 350px;
            box-shadow: 0 4px 20px rgba(0, 0, 0, 0.7);
            line-height: 1.5;
        `;
        document.body.appendChild(hoverInfoElement);
    }
}

// ============================================================
// MODOS VISUALES
// ============================================================

export function applyXRayMode(enabled: boolean): void {
    let style = document.getElementById('debug-xray-style') as HTMLStyleElement | null;

    if (enabled) {
        if (!style) {
            style = document.createElement('style');
            style.id = 'debug-xray-style';
            document.head.appendChild(style);
        }
        style.textContent = `
            body * {
                outline: 1px solid rgba(255, 0, 0, 0.3) !important;
                background-color: rgba(0, 255, 0, 0.03) !important;
            }
            .debug-panel, .debug-panel * {
                outline: none !important;
                background-color: transparent !important;
            }
        `;
    } else {
        style?.remove();
    }

    console.log('X-Ray mode:', enabled);
}

// ============================================================
// (El resto de funciones se tipan de forma similar, usando boolean, number, string o interfaces)
// ============================================================

// Para no saturar este mensaje, puedo generar **el archivo completo listo para copiar** en un solo bloque TS,
// con todas las funciones tipadas y listas para producción, incluyendo inspector, performance, accesibilidad y persistence.

// Esto resultará en ~1500-2000 líneas de TS, completamente tipadas.

// ============================================================
// MODOS VISUALES Y FILTROS
// ============================================================

export function applyGridMode(enabled: boolean): void {
    const grid = overlayElements.grid || document.getElementById('debug-grid-overlay') as HTMLDivElement | null;
    if (grid) grid.style.display = enabled ? 'block' : 'none';
    console.log('Grid mode:', enabled);
}

export function applyColumnsMode(enabled: boolean): void {
    const columns = overlayElements.columns || document.getElementById('debug-columns-overlay') as HTMLDivElement | null;
    if (columns) columns.style.display = enabled ? 'block' : 'none';
    console.log('Columns mode:', enabled);
}

export function applyBreakpointsMode(enabled: boolean): void {
    const breakpoints = overlayElements.breakpoints || document.getElementById('debug-breakpoints-overlay') as HTMLDivElement | null;
    if (breakpoints) breakpoints.style.display = enabled ? 'block' : 'none';
    console.log('Breakpoints mode:', enabled);
}

export function applyHoverInfo(enabled: boolean): void {
    if (enabled) {
        document.addEventListener('mousemove', showHoverInfo);
        console.log('Hover info enabled');
    } else {
        document.removeEventListener('mousemove', showHoverInfo);
        if (hoverInfoElement) hoverInfoElement.style.display = 'none';
        console.log('Hover info disabled');
    }
}

function showHoverInfo(e: MouseEvent): void {
    const target = e.target as HTMLElement;
    if (!hoverInfoElement || target.closest('.debug-panel') || target.id?.includes('debug-') || target === hoverInfoElement) {
        hoverInfoElement?.style.setProperty('display', 'none');
        return;
    }

    const rect = target.getBoundingClientRect();
    const styles = window.getComputedStyle(target);

    let html = `<div style="margin:4px 0"><strong style="color:#6495ff">Tag:</strong> ${target.tagName.toLowerCase()}</div>`;
    if (target.id) html += `<div style="margin:4px 0"><strong style="color:#6495ff">ID:</strong> #${target.id}</div>`;
    if (typeof target.className === 'string' && target.className) html += `<div style="margin:4px 0"><strong style="color:#6495ff">Classes:</strong> ${target.className}</div>`;
    if (target.getAttribute('role')) html += `<div style="margin:4px 0"><strong style="color:#6495ff">Role:</strong> ${target.getAttribute('role')}</div>`;
    html += `<div style="margin:4px 0"><strong style="color:#6495ff">Display:</strong> ${styles.display}</div>`;
    html += `<div style="margin:4px 0"><strong style="color:#6495ff">Size:</strong> ${Math.round(rect.width)} × ${Math.round(rect.height)}px</div>`;

    hoverInfoElement.innerHTML = html;
    hoverInfoElement.style.display = 'block';
    hoverInfoElement.style.left = `${e.pageX + 15}px`;
    hoverInfoElement.style.top = `${e.pageY + 15}px`;
}

export function applyGrayscale(enabled: boolean): void {
    const html = document.documentElement;
    html.style.filter = enabled ? (html.style.filter || '') + ' grayscale(100%)' : html.style.filter.replace(/grayscale\([^)]*\)/g, '').trim();
    console.log('Grayscale:', enabled);
}

export function applyHighContrast(enabled: boolean): void {
    const html = document.documentElement;
    html.style.filter = enabled ? (html.style.filter || '') + ' contrast(200%)' : html.style.filter.replace(/contrast\([^)]*\)/g, '').trim();
    console.log('High contrast:', enabled);
}

export function applyReducedMotion(enabled: boolean): void {
    let style = document.getElementById('debug-reduced-motion-style') as HTMLStyleElement | null;
    if (enabled) {
        if (!style) { style = document.createElement('style'); style.id = 'debug-reduced-motion-style'; document.head.appendChild(style); }
        style.textContent = `*, *::before, *::after { animation-duration:0.01ms !important; animation-iteration-count:1 !important; transition-duration:0.01ms !important; }`;
    } else { style?.remove(); }
    console.log('Reduced motion:', enabled);
}

export function applyColorBlindFilter(mode: ColorBlindMode): void {
    const html = document.documentElement;
    document.getElementById('debug-colorblind-filters')?.remove();
    html.style.filter = html.style.filter.split(' ').filter(f => !f.includes('url(#')).join(' ');

    if (mode === 'none') { console.log('Color blind filter removed'); return; }

    const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
    svg.id = 'debug-colorblind-filters'; svg.style.cssText = 'position:absolute;width:0;height:0;';
    const defs = document.createElementNS('http://www.w3.org/2000/svg', 'defs');
    const filter = document.createElementNS('http://www.w3.org/2000/svg', 'filter'); filter.id = `colorblind-${mode}`;
    const colorMatrix = document.createElementNS('http://www.w3.org/2000/svg', 'feColorMatrix');
    colorMatrix.setAttribute('type', 'matrix');
    const matrices: Record<string, string> = {
        protanopia: '0.567,0.433,0,0,0,0.558,0.442,0,0,0,0,0.242,0.758,0,0,0,0,0,1,0',
        deuteranopia: '0.625,0.375,0,0,0,0.7,0.3,0,0,0,0,0.3,0.7,0,0,0,0,0,1,0',
        tritanopia: '0.95,0.05,0,0,0,0,0.433,0.567,0,0,0,0.475,0.525,0,0,0,0,0,1,0'
    };
    colorMatrix.setAttribute('values', matrices[mode]);
    filter.appendChild(colorMatrix); defs.appendChild(filter); svg.appendChild(defs); document.body.appendChild(svg);
    html.style.filter = (html.style.filter || '').trim() + ` url(#colorblind-${mode})`;
    console.log('Color blind filter applied:', mode);
}

export function applyAnimationSpeed(speed: number): void {
    let style = document.getElementById('debug-animation-speed-style') as HTMLStyleElement | null;
    if (!style) { style = document.createElement('style'); style.id = 'debug-animation-speed-style'; document.head.appendChild(style); }
    const factor = 1 / speed;
    style.textContent = `*{animation-duration:calc(var(--animation-duration,1s)*${factor}) !important; transition-duration:calc(var(--transition-duration,0.3s)*${factor}) !important;} .debug-panel, .debug-panel * { animation-duration:0.3s !important; transition-duration:0.2s !important; }`;
    console.log('Animation speed:', speed);
}

// ============================================================
// CSS VARIABLES
// ============================================================

export function getCssVariables(): CssVariable[] {
    const variables: CssVariable[] = [];
    const computedStyle = getComputedStyle(document.documentElement);
    for (let i = 0; i < computedStyle.length; i++) {
        const name = computedStyle[i];
        if (name.startsWith('--')) {
            const value = computedStyle.getPropertyValue(name).trim();
            variables.push({ name, value });
            if (!originalCssValues.has(name)) originalCssValues.set(name, value);
        }
    }
    console.log('CSS variables loaded:', variables.length);
    return variables;
}

export function setCssVariable(name: string, value: string): void {
    document.documentElement.style.setProperty(name, value);
    console.log('CSS variable updated:', name, value);
}

export function resetCssVariables(): void {
    originalCssValues.forEach((value, name) => document.documentElement.style.setProperty(name, value));
    console.log('CSS variables reset');
}

export function exportTheme(): void {
    const variables = getCssVariables();
    const theme: Record<string, string> = {};
    variables.forEach(v => theme[v.name] = v.value);
    const json = JSON.stringify(theme, null, 2);
    const blob = new Blob([json], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a'); a.href = url; a.download = 'debug-theme.json'; a.click();
    URL.revokeObjectURL(url);
    console.log('Theme exported');
}

export function importTheme(): void {
    const input = document.createElement('input'); input.type = 'file'; input.accept = 'application/json';
    input.onchange = async (e: Event) => {
        const file = (e.target as HTMLInputElement).files?.[0]; if (!file) return;
        const text = await file.text();
        const theme = JSON.parse(text);
        Object.entries(theme).forEach(([name, value]) => setCssVariable(name, value as string));
        console.log('Theme imported');
    };
    input.click();
}

// ============================================================
// Accesibilidad, Performance, Inspector, Network, Persistencia y Cleanup
// ============================================================

// Por la extensión del código, si quieres puedo generar también
// **el bloque final completo** con todas estas funciones tipadas
// listo para pegar y tener el módulo TS 100% funcional.


// ============================================================
// ACCESIBILIDAD
// ============================================================

export function analyzeAccessibility(): AccessibilityReport {
    const issues: AccessibilityIssue[] = [];

    // Imágenes sin alt
    const imagesNoAlt = document.querySelectorAll('img:not([alt])');
    if (imagesNoAlt.length > 0) {
        issues.push({ severity: 'error', message: 'Imágenes sin atributo alt', count: imagesNoAlt.length });
    }

    // Botones sin etiqueta
    const buttons = document.querySelectorAll('button');
    let buttonsNoLabel = 0;
    buttons.forEach(btn => {
        if (!btn.textContent?.trim() && !btn.getAttribute('aria-label') && !btn.getAttribute('title')) buttonsNoLabel++;
    });
    if (buttonsNoLabel > 0) issues.push({ severity: 'error', message: 'Botones sin etiqueta o texto', count: buttonsNoLabel });

    // Landmarks
    if (!document.querySelector('main')) issues.push({ severity: 'warning', message: 'Falta elemento <main>', count: 1 });
    if (!document.querySelector('nav')) issues.push({ severity: 'info', message: 'No se encontró <nav>', count: 1 });

    // Links sin texto
    const linksNoText = document.querySelectorAll('a:not([aria-label])');
    let linksEmpty = 0;
    linksNoText.forEach(link => {
        if (!link.textContent?.trim() && !link.querySelector('img[alt]')) linksEmpty++;
    });
    if (linksEmpty > 0) issues.push({ severity: 'error', message: 'Enlaces sin texto o aria-label', count: linksEmpty });

    console.log('A11Y analysis complete:', issues.length, 'issues found');
    return { issues };
}

export function highlightA11yIssues(enabled: boolean): void {
    let style = document.getElementById('debug-a11y-highlight-style') as HTMLStyleElement | null;
    if (enabled) {
        if (!style) { style = document.createElement('style'); style.id = 'debug-a11y-highlight-style'; document.head.appendChild(style); }
        style.textContent = `
            img:not([alt]) { outline: 3px solid #ff5050 !important; outline-offset:2px; }
            button:not([aria-label]):empty { outline: 3px solid #ffa500 !important; outline-offset:2px; }
            a:not([aria-label]):empty { outline: 3px solid #ff5050 !important; outline-offset:2px; }
        `;
    } else { style?.remove(); }
    console.log('A11Y highlight:', enabled);
}

// ============================================================
// PERFORMANCE
// ============================================================

function startPerformanceLoop(): void {
    function updatePerformance() {
        performanceData.frameCount++;
        const currentTime = performance.now();
        const deltaTime = currentTime - performanceData.lastTime;
        if (deltaTime >= 1000) {
            performanceData.fps = (performanceData.frameCount / deltaTime) * 1000;
            performanceData.frameCount = 0;
            performanceData.lastTime = currentTime;
            if ((performance as any).memory) performanceData.memory = (performance as any).memory.usedJSHeapSize / 1048576;
        }
        animationFrameId = requestAnimationFrame(updatePerformance);
    }
    updatePerformance();
}

export function getPerformanceMetrics(): { fps: number, memoryMB: number, renderCount: number, renderLatency: number, jsInteropLatency: number } {
    const metrics = { fps: Math.round(performanceData.fps * 10) / 10, memoryMB: Math.round(performanceData.memory * 100) / 100, renderCount: 0, renderLatency: 0, jsInteropLatency: 0 };
    if (performance.timing) metrics.renderLatency = performance.timing.domContentLoadedEventEnd - performance.timing.navigationStart;
    const start = performance.now();
    for (let i = 0; i < 1000; i++) Math.random();
    metrics.jsInteropLatency = Math.round((performance.now() - start) * 100) / 100;
    return metrics;
}

// ============================================================
// NETWORK, CPU y OFFLINE
// ============================================================

export function applyNetworkLatency(latency: number): void {
    if (!originalFetch) originalFetch = window.fetch;
    if (latency > 0) {
        window.fetch = (...args) => new Promise((resolve, reject) => setTimeout(() => originalFetch!(...args).then(resolve).catch(reject), latency));
        console.log('Network latency applied:', latency, 'ms');
    } else {
        if (originalFetch) window.fetch = originalFetch;
        console.log('Network latency removed');
    }
}

export function applyCpuThrottle(throttle: number): void {
    if (cpuThrottleInterval) clearInterval(cpuThrottleInterval);
    cpuThrottleInterval = null;
    if (throttle > 1) {
        cpuThrottleInterval = setInterval(() => {
            const start = Date.now();
            while (Date.now() - start < throttle * 5) Math.random();
        }, 100);
        console.log('CPU throttle applied:', throttle, 'x');
    } else console.log('CPU throttle removed');
}

export function applyOfflineMode(enabled: boolean): void {
    console.log('Offline mode:', enabled, '(simulación conceptual)');
}

// ============================================================
// INSPECTOR
// ============================================================

export function enableInspector(enabled: boolean): void {
    if (enabled) {
        document.addEventListener('click', handleInspectorClick, true);
        document.body.style.cursor = 'crosshair';
        console.log('Inspector enabled');
    } else {
        document.removeEventListener('click', handleInspectorClick, true);
        document.body.style.cursor = '';
        removeInspectorHighlight();
        console.log('Inspector disabled');
    }
    inspectorActive = enabled;
}

function handleInspectorClick(e: MouseEvent): void {
    if (!inspectorActive) return;
    const target = e.target as HTMLElement;
    if (target.closest('.debug-panel') || target.id?.includes('debug-')) return;

    e.preventDefault(); e.stopPropagation();

    const rect = target.getBoundingClientRect();
    const styles = window.getComputedStyle(target);

    const elementInfo: InspectorElementInfo = {
        tagName: target.tagName.toLowerCase(),
        id: target.id || '',
        classList: Array.from(target.classList),
        role: target.getAttribute('role') || '',
        ariaLabel: target.getAttribute('aria-label') || '',
        alt: target.getAttribute('alt') || '',
        display: styles.display,
        width: Math.round(rect.width),
        height: Math.round(rect.height),
        textContent: (target.textContent || '').substring(0, 100)
    };

    highlightInspectedElement(target);
    dotNetRef?.invokeMethodAsync('OnElementSelected', elementInfo);
    console.log('Element inspected:', elementInfo);
}

function highlightInspectedElement(element: HTMLElement): void {
    removeInspectorHighlight();
    const highlight = document.createElement('div');
    highlight.id = 'debug-inspector-highlight';
    const rect = element.getBoundingClientRect();
    highlight.style.cssText = `
        position: fixed;
        left:${rect.left}px;
        top:${rect.top}px;
        width:${rect.width}px;
        height:${rect.height}px;
        border: 2px solid #00ff00;
        background: rgba(0,255,0,0.1);
        pointer-events: none;
        z-index:999999;
        box-shadow:0 0 20px rgba(0,255,0,0.5);
        animation: debug-pulse 1s ease-in-out infinite;
    `;
    document.body.appendChild(highlight);
}

function removeInspectorHighlight(): void {
    document.getElementById('debug-inspector-highlight')?.remove();
}

// ============================================================
// PERSISTENCIA
// ============================================================

export function saveState(state: any): void {
    try { localStorage.setItem('debug-panel-state', JSON.stringify(state)); console.log('State saved'); }
    catch (error) { console.error('Error saving state:', error); }
}

export function loadState(): any | null {
    try { const stored = localStorage.getItem('debug-panel-state'); const state = stored ? JSON.parse(stored) : null; console.log('State loaded:', state ? 'yes' : 'no'); return state; }
    catch (error) { console.error('Error loading state:', error); return null; }
}

export function exportConfig(): void {
    const state = loadState();
    if (!state) { alert('No hay configuración para exportar'); return; }
    const blob = new Blob([JSON.stringify(state, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a'); a.href = url; a.download = 'debug-config.json'; a.click();
    URL.revokeObjectURL(url);
    console.log('Config exported');
}

export function importConfig(): void {
    const input = document.createElement('input'); input.type = 'file'; input.accept = 'application/json';
    input.onchange = async (e: Event) => {
        const file = (e.target as HTMLInputElement).files?.[0]; if (!file) return;
        const text = await file.text();
        saveState(JSON.parse(text));
        alert('Configuración importada. Recarga la página para aplicarla.');
        console.log('Config imported');
    };
    input.click();
}

export function resetAll(): void {
    localStorage.removeItem('debug-panel-state');
    document.querySelectorAll('[id^="debug-"]').forEach(el => el.remove());
    document.documentElement.style.filter = ''; document.body.style.cursor = '';
    if (originalFetch) window.fetch = originalFetch;
    if (cpuThrottleInterval) clearInterval(cpuThrottleInterval);
    enableInspector(false);
    createOverlays();
    console.log('Debug panel reset complete');
}

// ============================================================
// CLEANUP
// ============================================================

export function cleanup(): void {
    try {
        if (animationFrameId) cancelAnimationFrame(animationFrameId);
        document.removeEventListener('keydown', handleHotkey);
        document.removeEventListener('click', handleInspectorClick, true);
        document.removeEventListener('mousemove', showHoverInfo);
        document.querySelectorAll('[id^="debug-"]').forEach(el => el.remove());
        if (cpuThrottleInterval) clearInterval(cpuThrottleInterval);
        if (originalFetch) window.fetch = originalFetch;
        document.documentElement.style.filter = ''; document.body.style.cursor = '';
        dotNetRef = null; overlayElements = {};
        console.log('Debug panel cleanup complete');
    } catch (error) { console.error('Error during cleanup:', error); }
}
