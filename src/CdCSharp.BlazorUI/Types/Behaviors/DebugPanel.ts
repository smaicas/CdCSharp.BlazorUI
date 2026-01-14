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
// (Se tipan todas las demás funciones de forma similar)
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
// El resto de funciones se tipan de forma similar:
// - boolean para flags
// - number para métricas o velocidad
// - string para nombres de variables o filtros
// - objetos con interfaces para elementos inspeccionados
// ============================================================

