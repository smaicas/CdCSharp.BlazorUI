// === wwwroot/ts/modal.ts ===

interface FocusTrapState {
    previousActiveElement: HTMLElement | null;
    firstFocusable: HTMLElement | null;
    lastFocusable: HTMLElement | null;
    tabHandler: ((e: KeyboardEvent) => void) | null;
}

let scrollLockCount = 0;
let focusTrapState: FocusTrapState = {
    previousActiveElement: null,
    firstFocusable: null,
    lastFocusable: null,
    tabHandler: null
};

const FOCUSABLE_SELECTORS = [
    'button:not([disabled])',
    '[href]',
    'input:not([disabled])',
    'select:not([disabled])',
    'textarea:not([disabled])',
    '[tabindex]:not([tabindex="-1"])'
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

    const focusables = element.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTORS);

    if (focusables.length === 0) {
        element.setAttribute('tabindex', '-1');
        element.focus();
        return;
    }

    focusTrapState.firstFocusable = focusables[0];
    focusTrapState.lastFocusable = focusables[focusables.length - 1];

    focusTrapState.tabHandler = (e: KeyboardEvent): void => {
        if (e.key !== 'Tab') return;

        if (!focusTrapState.firstFocusable || !focusTrapState.lastFocusable) return;

        if (e.shiftKey) {
            if (document.activeElement === focusTrapState.firstFocusable) {
                e.preventDefault();
                focusTrapState.lastFocusable.focus();
            }
        } else {
            if (document.activeElement === focusTrapState.lastFocusable) {
                e.preventDefault();
                focusTrapState.firstFocusable.focus();
            }
        }
    };

    document.addEventListener('keydown', focusTrapState.tabHandler);
    focusTrapState.firstFocusable.focus();
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
        firstFocusable: null,
        lastFocusable: null,
        tabHandler: null
    };
}