interface DragCallbacksRelay {
    invokeMethodAsync(methodName: string, ...args: unknown[]): Promise<unknown>;
}

interface DragInstance {
    mouseMove: (e: MouseEvent) => void;
    mouseUp: (e: MouseEvent) => void;
}

const instances = new Map<string, DragInstance>();

export function startDrag(
    element: HTMLElement,
    dotNetRef: DragCallbacksRelay,
    componentId: string
): void {
    if (instances.has(componentId)) return;

    const handlers: DragInstance = {
        mouseMove: (e: MouseEvent) => {
            dotNetRef.invokeMethodAsync('OnMouseMove', e.clientX, e.clientY);
        },
        mouseUp: (e: MouseEvent) => {
            dotNetRef.invokeMethodAsync('OnMouseUp', e.clientX, e.clientY);
            stopDrag(componentId);
        }
    };

    document.addEventListener('mousemove', handlers.mouseMove);
    document.addEventListener('mouseup', handlers.mouseUp);

    instances.set(componentId, handlers);
}

export function stopDrag(componentId: string): void {
    const handlers = instances.get(componentId);
    if (!handlers) return;

    document.removeEventListener('mousemove', handlers.mouseMove);
    document.removeEventListener('mouseup', handlers.mouseUp);

    instances.delete(componentId);
}