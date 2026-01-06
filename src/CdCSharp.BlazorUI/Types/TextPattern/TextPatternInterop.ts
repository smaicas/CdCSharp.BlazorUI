interface PatternCallbacksRelay {
    invokeMethodAsync(methodName: string, ...args: any[]): Promise<any>;
}

interface PatternInstance {
    container: HTMLDivElement;
    dotnetRef: PatternCallbacksRelay;
    handlers: {
        paste: (e: ClipboardEvent) => void;
        keydown: (e: KeyboardEvent) => void;
        input: (e: Event) => void;
        focus: (e: FocusEvent) => void;
        blur: (e: FocusEvent) => void;
    };
}

const patterns: Map<string, PatternInstance> = new Map();

export function initializePattern(
    container: HTMLDivElement,
    dotnet: PatternCallbacksRelay,
    componentId: string
): void {
    if (!container || !componentId) return;

    // Clean up any existing instance
    disposePattern(componentId);

    // Create handlers bound to this instance
    const handlers = {
        paste: async (e: ClipboardEvent) => await handlePaste(e, dotnet),
        keydown: async (e: KeyboardEvent) => await handleKeyDown(e, componentId),
        input: async (e: Event) => await handleInput(e, dotnet),
        focus: async (e: FocusEvent) => await handleFocus(e, dotnet),
        blur: async (e: FocusEvent) => await handleBlur(e, dotnet)
    };

    // Add event listeners
    container.addEventListener('paste', handlers.paste);
    container.addEventListener('keydown', handlers.keydown);
    container.addEventListener('input', handlers.input);
    container.addEventListener('focus', handlers.focus, true);
    container.addEventListener('blur', handlers.blur, true);

    // Store the instance
    patterns.set(componentId, {
        container,
        dotnetRef: dotnet,
        handlers
    });
}

async function handleInput(e: Event, dotnetRef: PatternCallbacksRelay): Promise<void> {
    const target = e.target as HTMLElement;
    if (!target || target.contentEditable !== 'true') return;

    const index = parseInt(target.dataset.index || '-1');
    if (index === -1) return;

    const value = target.textContent || '';

    // Small delay to ensure the cursor position is handled after the DOM update
    await dotnetRef.invokeMethodAsync('HandleSpanInput', index, value)
        .catch(error => console.error('Input callback error:', error));
}

async function handleFocus(e: FocusEvent, dotnetRef: PatternCallbacksRelay): Promise<void> {
    const target = e.target as HTMLElement;
    if (!target || target.contentEditable !== 'true') return;

    const index = parseInt(target.dataset.index || '-1');
    if (index === -1) return;

    await dotnetRef.invokeMethodAsync('HandleSpanFocus', index)
        .catch(error => console.error('Focus callback error:', error));
}

async function handleBlur(e: FocusEvent, dotnetRef: PatternCallbacksRelay): Promise<void> {
    const target = e.target as HTMLElement;
    if (!target || target.contentEditable !== 'true') return;

    const index = parseInt(target.dataset.index || '-1');
    if (index === -1) return;

    await dotnetRef.invokeMethodAsync('HandleSpanBlur', index)
        .catch(error => console.error('Blur callback error:', error));
}

async function handlePaste(e: ClipboardEvent, dotnetRef: PatternCallbacksRelay): Promise<void> {
    e.preventDefault();

    if (!e.clipboardData) return;

    const text = e.clipboardData.getData('text');

    await dotnetRef.invokeMethodAsync('HandlePaste', text)
        .catch(error => console.error('Paste callback error:', error));
}

async function handleKeyDown(e: KeyboardEvent, componentId: string): Promise<void> {
    const target = e.target as HTMLElement;
    if (!target || target.contentEditable !== 'true') return;

    const index = parseInt(target.dataset.index || '-1');
    if (index === -1) return;

    if (e.key === 'Tab') {
        e.preventDefault();

        if (e.shiftKey) {
            focusPreviousSpan(componentId, index);
        } else {
            focusNextSpan(componentId, index);
        }
    }
}

export function updateSpanValue(componentId: string, index: number, value: string): void {
    const instance = patterns.get(componentId);
    if (!instance) return;

    const span = getSpanByIndex(instance.container, index);
    if (span && span.textContent !== value) {
        // Update the content
        span.textContent = value;

        // If the span is focused, position cursor at the end of the new value
        if (document.activeElement === span) {
            setCursorPosition(span, value.length);
        }
    }
}

export function selectSpanContent(componentId: string, index: number): void {
    const instance = patterns.get(componentId);
    if (!instance) return;

    const span = getSpanByIndex(instance.container, index);
    if (!span) return;

    const range = document.createRange();
    range.selectNodeContents(span);

    const selection = window.getSelection();
    if (selection) {
        selection.removeAllRanges();
        selection.addRange(range);
    }
}

export function focusSpan(componentId: string, index: number): void {
    const instance = patterns.get(componentId);
    if (!instance) return;

    const span = getSpanByIndex(instance.container, index);
    if (span) {
        span.focus();
        setTimeout(() => selectSpanContent(componentId, index), 10);
    }
}

function focusNextSpan(componentId: string, currentIndex: number): void {
    const instance = patterns.get(componentId);
    if (!instance) return;

    const spans = getEditableSpans(instance.container);
    const currentSpanIndex = spans.findIndex(s =>
        parseInt(s.dataset.index || '-1') === currentIndex
    );

    if (currentSpanIndex >= 0 && currentSpanIndex < spans.length - 1) {
        const nextSpan = spans[currentSpanIndex + 1];
        const nextIndex = parseInt(nextSpan.dataset.index || '-1');
        if (nextIndex >= 0) {
            focusSpan(componentId, nextIndex);
        }
    }
}

function focusPreviousSpan(componentId: string, currentIndex: number): void {
    const instance = patterns.get(componentId);
    if (!instance) return;

    const spans = getEditableSpans(instance.container);
    const currentSpanIndex = spans.findIndex(s =>
        parseInt(s.dataset.index || '-1') === currentIndex
    );

    if (currentSpanIndex > 0) {
        const prevSpan = spans[currentSpanIndex - 1];
        const prevIndex = parseInt(prevSpan.dataset.index || '-1');
        if (prevIndex >= 0) {
            focusSpan(componentId, prevIndex);
        }
    }
}

function getSpanByIndex(container: HTMLDivElement, index: number): HTMLSpanElement | null {
    return container.querySelector(`[data-index="${index}"]`);
}

function getEditableSpans(container: HTMLDivElement): HTMLSpanElement[] {
    return Array.from(
        container.querySelectorAll('span[contenteditable="true"]')
    );
}

function setCursorPosition(element: HTMLElement, position: number): void {
    const selection = window.getSelection();
    if (!selection) return;

    // Ensure the element has focus
    if (document.activeElement !== element) {
        element.focus();
    }

    const range = document.createRange();

    // Handle empty element or text node
    if (element.childNodes.length === 0) {
        // Create a text node if element is empty
        const textNode = document.createTextNode('');
        element.appendChild(textNode);
    }

    const textNode = element.childNodes[0];
    const maxPosition = textNode.textContent?.length || 0;
    const safePosition = Math.min(position, maxPosition);

    try {
        range.setStart(textNode, safePosition);
        range.collapse(true);
        selection.removeAllRanges();
        selection.addRange(range);
    } catch (error) {
        console.debug('Could not set cursor position:', error);
    }
}

export function disposePattern(componentId: string): void {
    const instance = patterns.get(componentId);
    if (!instance) return;

    // Remove event listeners
    instance.container.removeEventListener('paste', instance.handlers.paste);
    instance.container.removeEventListener('keydown', instance.handlers.keydown);
    instance.container.removeEventListener('input', instance.handlers.input);
    instance.container.removeEventListener('focus', instance.handlers.focus, true);
    instance.container.removeEventListener('blur', instance.handlers.blur, true);

    // Remove from map
    patterns.delete(componentId);
}