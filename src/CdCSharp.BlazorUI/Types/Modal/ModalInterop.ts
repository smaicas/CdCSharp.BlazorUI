// === wwwroot/ts/modal.ts ===

interface ModalInstance {
    dotnetRef: any;
    closeOnEscape: boolean;
    closeOnOverlayClick: boolean;
    overlayElement: HTMLElement;
    keyDownHandler: (e: KeyboardEvent) => void;
    overlayClickHandler: (e: MouseEvent) => void;
}

interface FocusTrapState {
    previousActiveElement: HTMLElement | null;
    trapElement: HTMLElement | null;
    firstFocusable: HTMLElement | null;
    lastFocusable: HTMLElement | null;
    tabHandler: (e: KeyboardEvent) => void;
}

const instances = new Map<string, ModalInstance>();

let focusTrapState: FocusTrapState = {
    previousActiveElement: null,
    trapElement: null,
    firstFocusable: null,
    lastFocusable: null,
    tabHandler: () => { }
};

const FOCUSABLE_SELECTORS = [
    'button:not([disabled])',
    '[href]',
    'input:not([disabled])',
    'select:not([disabled])',
    'textarea:not([disabled])',
    '[tabindex]:not([tabindex="-1"])'
].join(', ');

export function initialize(
    overlayElement: HTMLElement,
    dotnetRef: any,
    hostId: string,
    closeOnEscape: boolean,
    closeOnOverlayClick: boolean
): void {
    if (instances.has(hostId)) {
        return;
    }

    const keyDownHandler = (e: KeyboardEvent): void => {
        const instance = instances.get(hostId);
        if (e.key === 'Escape' && instance?.closeOnEscape) {
            dotnetRef.invokeMethodAsync('OnEscapePressed');
        }
    };

    const overlayClickHandler = (e: MouseEvent): void => {
        const instance = instances.get(hostId);
        if (e.target === overlayElement && instance?.closeOnOverlayClick) {
            dotnetRef.invokeMethodAsync('OnOverlayClick');
        }
    };

    const instance: ModalInstance = {
        dotnetRef,
        closeOnEscape,
        closeOnOverlayClick,
        overlayElement,
        keyDownHandler,
        overlayClickHandler
    };

    document.addEventListener('keydown', keyDownHandler);
    overlayElement.addEventListener('click', overlayClickHandler);

    document.body.style.overflow = 'hidden';

    instances.set(hostId, instance);
}

export function updateOptions(
    hostId: string,
    closeOnEscape: boolean,
    closeOnOverlayClick: boolean
): void {
    const instance = instances.get(hostId);
    if (instance) {
        instance.closeOnEscape = closeOnEscape;
        instance.closeOnOverlayClick = closeOnOverlayClick;
    }
}

export function dispose(hostId: string): void {
    const instance = instances.get(hostId);
    if (!instance) {
        return;
    }

    document.removeEventListener('keydown', instance.keyDownHandler);
    instance.overlayElement.removeEventListener('click', instance.overlayClickHandler);

    instances.delete(hostId);

    if (instances.size === 0) {
        document.body.style.overflow = '';
    }
}

export function trapFocus(element: HTMLElement): void {
    focusTrapState.previousActiveElement = document.activeElement as HTMLElement;
    focusTrapState.trapElement = element;

    const focusables = element.querySelectorAll<HTMLElement>(FOCUSABLE_SELECTORS);

    if (focusables.length === 0) {
        element.setAttribute('tabindex', '-1');
        element.focus();
        return;
    }

    focusTrapState.firstFocusable = focusables[0];
    focusTrapState.lastFocusable = focusables[focusables.length - 1];

    focusTrapState.tabHandler = (e: KeyboardEvent): void => {
        if (e.key !== 'Tab') {
            return;
        }

        if (!focusTrapState.firstFocusable || !focusTrapState.lastFocusable) {
            return;
        }

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
    document.removeEventListener('keydown', focusTrapState.tabHandler);

    if (focusTrapState.previousActiveElement) {
        focusTrapState.previousActiveElement.focus();
    }

    focusTrapState = {
        previousActiveElement: null,
        trapElement: null,
        firstFocusable: null,
        lastFocusable: null,
        tabHandler: () => { }
    };
}