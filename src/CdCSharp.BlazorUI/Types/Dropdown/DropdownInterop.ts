interface DropdownCallbacksRelay {
    invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<unknown>;
}

interface DropdownInstance {
    triggerElement: HTMLElement;
    menuElement: HTMLElement | null;
    dotnetRef: DropdownCallbacksRelay;
    componentId: string;
    clickOutsideHandler: (e: MouseEvent) => void;
    keyDownHandler: (e: KeyboardEvent) => void;
}

interface DropdownPosition {
    triggerTop: number;
    triggerLeft: number;
    triggerWidth: number;
    triggerHeight: number;
    viewportHeight: number;
    viewportWidth: number;
    scrollY: number;
}

const dropdownInstances = new Map<string, DropdownInstance>();

export function initialize(
    triggerElement: HTMLElement,
    menuElement: HTMLElement | null,
    dotnetRef: DropdownCallbacksRelay,
    componentId: string
): void {
    if (!triggerElement || !componentId) return;

    dispose(componentId);

    const clickOutsideHandler = (e: MouseEvent): void => {
        const target = e.target as HTMLElement;
        const container = triggerElement.closest('[data-bui-component="input-dropdown"]');

        if (container && !container.contains(target)) {
            dotnetRef.invokeMethodAsync('OnClickOutside');
        }
    };

    const keyDownHandler = (e: KeyboardEvent): void => {
        const container = triggerElement.closest('[data-bui-component="input-dropdown"]');
        if (!container?.contains(document.activeElement)) return;

        const relevantKeys = ['Escape', 'ArrowDown', 'ArrowUp', 'Enter', 'Tab', 'Home', 'End'];

        if (relevantKeys.includes(e.key)) {
            e.preventDefault();
            dotnetRef.invokeMethodAsync('OnKeyDown', e.key, e.shiftKey, e.ctrlKey);
        }
    };

    dropdownInstances.set(componentId, {
        triggerElement,
        menuElement,
        dotnetRef,
        componentId,
        clickOutsideHandler,
        keyDownHandler
    });

    document.addEventListener('mousedown', clickOutsideHandler);
    document.addEventListener('keydown', keyDownHandler);
}

export function getPosition(componentId: string): DropdownPosition | null {
    const instance = dropdownInstances.get(componentId);
    if (!instance) return null;

    const rect = instance.triggerElement.getBoundingClientRect();

    return {
        triggerTop: rect.top,
        triggerLeft: rect.left,
        triggerWidth: rect.width,
        triggerHeight: rect.height,
        viewportHeight: window.innerHeight,
        viewportWidth: window.innerWidth,
        scrollY: window.scrollY
    };
}

export function focusSearchInput(componentId: string): void {
    const instance = dropdownInstances.get(componentId);
    if (!instance) return;

    const container = instance.triggerElement.closest('[data-bui-component="input-dropdown"]');
    const searchInput = container?.querySelector('.bui-dropdown__search-input') as HTMLInputElement;

    if (searchInput) {
        searchInput.focus();
    }
}

export function dispose(componentId: string): void {
    const instance = dropdownInstances.get(componentId);
    if (!instance) return;

    document.removeEventListener('mousedown', instance.clickOutsideHandler);
    document.removeEventListener('keydown', instance.keyDownHandler);

    dropdownInstances.delete(componentId);
}