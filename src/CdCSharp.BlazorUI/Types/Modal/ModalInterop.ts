// === wwwroot/ts/modal.ts ===

interface FocusTrapState {
    previousActiveElement: HTMLElement | null;
    container: HTMLElement | null;
    firstFocusable: HTMLElement | null;
    lastFocusable: HTMLElement | null;
    tabHandler: ((e: KeyboardEvent) => void) | null;
}

let scrollLockCount = 0;
let focusTrapState: FocusTrapState = {
    previousActiveElement: null,
    container: null,
    firstFocusable: null,
    lastFocusable: null,
    tabHandler: null
};

// Aligned with focus-trap / ariakit: include rich-editor targets and media elements
// so a modal containing a <div contenteditable> or <video controls> traps Tab correctly.
const FOCUSABLE_SELECTORS = [
    'button:not([disabled])',
    '[href]',
    'input:not([disabled])',
    'select:not([disabled])',
    'textarea:not([disabled])',
    '[tabindex]:not([tabindex="-1"])',
    '[contenteditable=""]',
    '[contenteditable="true"]',
    'audio[controls]',
    'video[controls]',
    'iframe',
    'embed',
    'object'
].join(', ');

export function lockScroll(): void {
    scrollLockCount++;
    if (scrollLockCount === 1) {
        document.body.style.overflow = 'hidden';
    }
}

export function unlockScroll(): void {
    scrollLockCount--;
    if (scrollLockCount <= 0) {
        scrollLockCount = 0;
        document.body.style.overflow = '';
    }
}

export function trapFocus(element: HTMLElement): void {
    releaseFocus();

    focusTrapState.previousActiveElement = document.activeElement as HTMLElement;
    focusTrapState.container = element;

    const focusables = element.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTORS);

    if (focusables.length === 0) {
        element.setAttribute('tabindex', '-1');
        element.focus();
    } else {
        focusTrapState.firstFocusable = focusables[0];
        focusTrapState.lastFocusable = focusables[focusables.length - 1];
        focusTrapState.firstFocusable.focus();
    }

    focusTrapState.tabHandler = (e: KeyboardEvent): void => {
        if (e.key !== 'Tab') return;

        const container = focusTrapState.container;
        if (!container) return;

        const active = document.activeElement as HTMLElement | null;
        const first = focusTrapState.firstFocusable;
        const last = focusTrapState.lastFocusable;

        // If focus escaped the dialog (e.g. host wrapper has tabindex=-1),
        // pull it back inside.
        if (!active || !container.contains(active)) {
            if (first) {
                e.preventDefault();
                first.focus();
            }
            return;
        }

        if (!first || !last) return;

        if (e.shiftKey) {
            if (active === first) {
                e.preventDefault();
                last.focus();
            }
        } else {
            if (active === last) {
                e.preventDefault();
                first.focus();
            }
        }
    };

    document.addEventListener('keydown', focusTrapState.tabHandler);
}

export function waitForAnimationEnd(element: HTMLElement, fallbackMs: number): Promise<void> {
    if (!element) return Promise.resolve();

    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    if (prefersReducedMotion) return Promise.resolve();

    return new Promise<void>((resolve) => {
        let done = false;
        const finish = (): void => {
            if (done) return;
            done = true;
            element.removeEventListener('animationend', finish);
            element.removeEventListener('transitionend', finish);
            clearTimeout(timeoutId);
            resolve();
        };
        element.addEventListener('animationend', finish, { once: true });
        element.addEventListener('transitionend', finish, { once: true });
        const timeoutId = window.setTimeout(finish, fallbackMs);
    });
}

export function releaseFocus(): void {
    if (focusTrapState.tabHandler) {
        document.removeEventListener('keydown', focusTrapState.tabHandler);
    }

    if (focusTrapState.previousActiveElement) {
        focusTrapState.previousActiveElement.focus();
    }

    focusTrapState = {
        previousActiveElement: null,
        container: null,
        firstFocusable: null,
        lastFocusable: null,
        tabHandler: null
    };
}