interface PatternCallbacksRelay {
    invokeMethodAsync(methodName: string, ...args: any[]): Promise<any>;
}

interface PatternInstance {
    container: HTMLElement;
    dotnetRef: PatternCallbacksRelay;
    componentId: string;
}

const patternInstances = new Map<string, PatternInstance>();

export function initializePattern(
    container: HTMLElement,
    dotnetRef: PatternCallbacksRelay,
    componentId: string
): void {
    if (!container || !componentId) return;

    // Clean up existing instance
    disposePattern(componentId);

    // Store instance
    patternInstances.set(componentId, {
        container,
        dotnetRef,
        componentId
    });

    // Attach event listeners to container
    container.addEventListener('input', handleInput);
    container.addEventListener('focus', handleFocus, true);
    container.addEventListener('blur', handleBlur, true);
    container.addEventListener('click', handleClick, true);
    container.addEventListener('paste', handlePaste);
    container.addEventListener('keydown', handleKeyDown);
}

async function handleInput(e: Event): Promise<void> {
    const target = e.target as HTMLElement;
    if (!isEditableSpan(target)) return;

    const instance = getInstanceFromElement(target);
    if (!instance) return;

    const index = getSpanIndex(target);
    const value = target.textContent || '';
    const maxLength = parseInt(target.dataset.maxlength || '0');

    try {
        // Always notify C# of input
        await instance.dotnetRef.invokeMethodAsync('OnSpanInput', index, value);

        // If complete, validate
        if (value.length === maxLength) {
            const isValid = await instance.dotnetRef.invokeMethodAsync('OnSpanComplete', index, value);

            if (isValid) {
                // Move to next span using click
                const nextSpan = instance.container.querySelector(
                    `[data-index="${index + 1}"][contenteditable="true"]`
                ) as HTMLElement;

                if (nextSpan) {
                    setTimeout(() => nextSpan.click(), 10);
                }
            }
        }
    } catch (error) {
        console.error('Input handling error:', error);
    }
}

async function handleClick(e: Event): Promise<void> {
    const target = e.target as HTMLElement;
    if (!isEditableSpan(target)) return;

    // Select all content on click
    selectSpanContent(target);
}

async function handleFocus(e: FocusEvent): Promise<void> {
    const target = e.target as HTMLElement;
    if (!isEditableSpan(target)) return;

    const instance = getInstanceFromElement(target);
    if (!instance) return;

    const index = getSpanIndex(target);

    try {
        await instance.dotnetRef.invokeMethodAsync('OnSpanFocus', index);
    } catch (error) {
        console.error('Focus handling error:', error);
    }
}

async function handleBlur(e: FocusEvent): Promise<void> {
    const target = e.target as HTMLElement;
    if (!isEditableSpan(target)) return;

    const instance = getInstanceFromElement(target);
    if (!instance) return;

    const index = getSpanIndex(target);

    try {
        await instance.dotnetRef.invokeMethodAsync('OnSpanBlur', index);
    } catch (error) {
        console.error('Blur handling error:', error);
    }
}

async function handlePaste(e: ClipboardEvent): Promise<void> {
    e.preventDefault();

    const target = e.target as HTMLElement;
    if (!isEditableSpan(target)) return;

    const instance = getInstanceFromElement(target);
    if (!instance || !e.clipboardData) return;

    const text = e.clipboardData.getData('text');

    try {
        await instance.dotnetRef.invokeMethodAsync('OnPaste', text);
    } catch (error) {
        console.error('Paste handling error:', error);
    }
}

async function handleKeyDown(e: KeyboardEvent): Promise<void> {
    const target = e.target as HTMLElement;
    if (!isEditableSpan(target)) return;

    const instance = getInstanceFromElement(target);
    if (!instance) return;

    const index = getSpanIndex(target);

    if (e.key === 'Tab') {
        e.preventDefault();

        const spans = Array.from(
            instance.container.querySelectorAll('[contenteditable="true"]')
        ) as HTMLElement[];

        const currentIndex = spans.findIndex(s => s === target);

        if (e.shiftKey && currentIndex > 0) {
            spans[currentIndex - 1].click();
        } else if (!e.shiftKey && currentIndex < spans.length - 1) {
            spans[currentIndex + 1].click();
        }
    } else if (e.key === 'Backspace' && target.textContent === '') {
        // Move to previous span if current is empty
        e.preventDefault();

        const prevSpan = instance.container.querySelector(
            `[data-index="${index - 1}"][contenteditable="true"]`
        ) as HTMLElement;

        if (prevSpan) {
            prevSpan.click();
            // Place cursor at end
            const range = document.createRange();
            const selection = window.getSelection();
            range.selectNodeContents(prevSpan);
            range.collapse(false);
            selection?.removeAllRanges();
            selection?.addRange(range);
        }
    }
}

// Public methods for C#
export function updateSpanValue(componentId: string, index: number, value: string): void {
    const instance = patternInstances.get(componentId);
    if (!instance) return;

    const span = instance.container.querySelector(
        `[data-index="${index}"]`
    ) as HTMLElement;

    if (span && span.textContent !== value) {
        span.textContent = value;

        // If focused, maintain selection
        if (document.activeElement === span) {
            selectSpanContent(span);
        }
    }
}

//export function selectSpanContent(componentId: string, index: number): void {
//    const instance = patternInstances.get(componentId);
//    if (!instance) return;

//    const span = instance.container.querySelector(
//        `[data-index="${index}"]`
//    ) as HTMLElement;

//    if (span) {
//        selectSpanContent(span);
//    }
//}

export function focusSpan(componentId: string, index: number): void {
    const instance = patternInstances.get(componentId);
    if (!instance) return;

    const span = instance.container.querySelector(
        `[data-index="${index}"][contenteditable="true"]`
    ) as HTMLElement;

    if (span) {
        span.click();
    }
}

export function disposePattern(componentId: string): void {
    const instance = patternInstances.get(componentId);
    if (!instance) return;

    // Remove event listeners
    instance.container.removeEventListener('input', handleInput);
    instance.container.removeEventListener('focus', handleFocus, true);
    instance.container.removeEventListener('blur', handleBlur, true);
    instance.container.removeEventListener('click', handleClick, true);
    instance.container.removeEventListener('paste', handlePaste);
    instance.container.removeEventListener('keydown', handleKeyDown);

    // Remove from map
    patternInstances.delete(componentId);
}

// Helper functions
function isEditableSpan(element: HTMLElement): boolean {
    return element.tagName === 'SPAN' && element.contentEditable === 'true';
}

function getSpanIndex(span: HTMLElement): number {
    return parseInt(span.dataset.index || '-1');
}

function getInstanceFromElement(element: HTMLElement): PatternInstance | null {
    const container = element.closest('[data-pattern-id]') as HTMLElement;
    if (!container) return null;

    const componentId = container.dataset.patternId;
    if (!componentId) return null;

    return patternInstances.get(componentId) || null;
}

function selectSpanContent(span: HTMLElement): void {
    const range = document.createRange();
    range.selectNodeContents(span);
    const selection = window.getSelection();
    selection?.removeAllRanges();
    selection?.addRange(range);
}