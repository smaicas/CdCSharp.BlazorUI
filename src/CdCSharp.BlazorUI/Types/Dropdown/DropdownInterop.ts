//  ARIA APG Combobox Pattern
//  Tecla Comportamiento
//  Space(en trigger) Abre el menú
//  Enter(en trigger) Abre el menú
//  Arrow Down Abre el menú(si cerrado) o navega a siguiente opción
//  Arrow Up Navega a opción anterior
//  Home Navega a primera opción
//  End Navega a última opción
//  Enter(en menú abierto) Selecciona opción activa y cierra
//  Space(en menú abierto) Selecciona opción activa(NO cierra en multiselect, SÍ cierra en single)
//  Escape Cierra sin seleccionar
//  Tab Cierra el menú y mueve focus al siguiente elemento

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
        const container = triggerElement.closest('[data-bui-component="dropdown-container"]');

        if (container && !container.contains(target)) {
            dotnetRef.invokeMethodAsync('OnClickOutside');
        }
    };

    const keyDownHandler = (e: KeyboardEvent): void => {
        const container = triggerElement.closest('[data-bui-component="dropdown-container"]');
        if (!container?.contains(document.activeElement)) return;

        const isOpen = container.getAttribute('data-bui-dropdown-open') === 'true';
        const relevantKeys = ['Escape', 'ArrowDown', 'ArrowUp', 'Enter', 'Tab', 'Home', 'End'];

        if (e.key === ' ' && isOpen) {
            e.preventDefault();
            dotnetRef.invokeMethodAsync('OnKeyDown', e.key, e.shiftKey, e.ctrlKey);
            return;
        }

        if (e.key.toLowerCase() === 'a' && e.ctrlKey && isOpen) {
            e.preventDefault();
            dotnetRef.invokeMethodAsync('OnKeyDown', e.key.toLowerCase(), e.shiftKey, e.ctrlKey);
            return;
        }

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

    const container = instance.triggerElement.closest('[data-bui-component="dropdown-container"]');
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