export interface ElementPattern {
    pattern: string;
    value: string;
    length: number;
    defaultValue: string;
    isSeparator: boolean;
    isEditable: boolean;
}

// --- Event registry (WeakMaps) ---

const textPatternEvents = {
    click: new WeakMap<HTMLSpanElement, EventListener>(),
    input: new WeakMap<HTMLSpanElement, EventListener>(),
    blur: new WeakMap<HTMLSpanElement, EventListener>(),
    keydown: new WeakMap<HTMLSpanElement, EventListener>()
};

// --- Public API (called from Blazor) ---

export async function TextPatternAddDynamic(
    containerBox: HTMLDivElement,
    elements: Array<ElementPattern>,
    dotnet: any,
    dotnetNotifyChangeTextCallback: string,
    dotnetValidatePartialCallback: string): Promise<void> {

    if (!containerBox.appendChild) { return; }

    containerBox.innerHTML = '';
    containerBox.classList.add('text-pattern-container');

    let editableIndex = 0;

    for (const element of elements) {
        const span = document.createElement('span');
        span.innerText = element.value;
        span.classList.add(element.isSeparator ? 'pattern-separator' : 'pattern-component');

        if (element.isSeparator) {
            containerBox.appendChild(span);
            continue;
        }

        if (element.isEditable) {
            span.contentEditable = 'true';
            span.dataset.index = editableIndex.toString();
            span.dataset.length = element.length.toString();
            span.dataset.defaultValue = element.defaultValue;

            addTextPatternEvents(
                span,
                editableIndex,
                containerBox,
                element,
                dotnet,
                dotnetNotifyChangeTextCallback,
                dotnetValidatePartialCallback);

            editableIndex++;
        }

        containerBox.appendChild(span);
    }
}

// --- Event wiring ---

function addTextPatternEvents(
    span: HTMLSpanElement,
    index: number,
    containerBox: HTMLDivElement,
    elementPattern: ElementPattern,
    dotnet: any,
    notifyCallback: string,
    validateCallback: string) {

    addEvent(span, 'click', textPatternEvents.click, () => selectTextOnClick(span));

    addEvent(span, 'input', textPatternEvents.input, () =>
        handleInput(
            span,
            index,
            containerBox,
            elementPattern,
            dotnet,
            notifyCallback,
            validateCallback));

    addEvent(span, 'blur', textPatternEvents.blur, () =>
        setDefaultValueIfInvalid(span, elementPattern, dotnet, notifyCallback, containerBox));

    addEvent(span, 'keydown', textPatternEvents.keydown, (e) =>
        handleKeyDown(e as KeyboardEvent, span, containerBox));
}

function addEvent(
    span: HTMLSpanElement,
    type: keyof typeof textPatternEvents,
    map: WeakMap<HTMLSpanElement, EventListener>,
    handler: EventListener) {

    if (!map.get(span)) {
        span.addEventListener(type, handler);
        map.set(span, handler);
    }
}

// --- Core input logic ---

function handleInput(
    span: HTMLSpanElement,
    index: number,
    containerBox: HTMLDivElement,
    elementPattern: ElementPattern,
    dotnet: any,
    notifyCallback: string,
    validateCallback: string) {

    if (span.innerText.length === 0) {
        return;
    }

    const cursor = getCursorPositionWithinSpan(span);

    const filtered = filterTextByPattern(
        span.innerText,
        elementPattern.pattern,
        cursor,
        elementPattern.length);

    span.innerText = filtered.text;
    setCursorPositionWithinSpan(span, filtered.cursor);

    if (span.innerText.length >= elementPattern.length) {
        dotnet.invokeMethodAsync(validateCallback, index, span.innerText)
            .then((valid: boolean) => {
                if (valid) {
                    moveToNext(containerBox, span);
                } else {
                    span.innerText = elementPattern.defaultValue;
                    selectTextOnClick(span);
                }
            });
    }

    notifyDotNet(dotnet, notifyCallback, containerBox.innerText);
}

// --- Filtering logic (pure function) ---

function filterTextByPattern(
    text: string,
    pattern: string,
    cursor: number,
    maxLength: number): { text: string; cursor: number } {

    const flattened = pattern.replace(/[\\\(\)\^\$]/g, '');
    const allowed = flattened.substring(0, maxLength).split('');

    let result = '';
    let newCursor = cursor;

    for (let i = 0; i < text.length && i < allowed.length; i++) {
        const ch = text[i];
        const p = allowed[i];

        const valid =
            (p === 'w' && /[a-zA-Z]/.test(ch)) ||
            (p === 'd' && /[0-9]/.test(ch));

        if (valid) {
            result += ch;
        } else {
            newCursor = Math.max(0, newCursor - 1);
        }
    }

    return {
        text: result.substring(0, maxLength),
        cursor: newCursor
    };
}

// --- Default & validation helpers ---

function setDefaultValueIfInvalid(
    span: HTMLSpanElement,
    elementPattern: ElementPattern,
    dotnet: any,
    callback: string,
    containerBox: HTMLDivElement) {

    if (span.innerText.length !== elementPattern.length) {
        span.innerText = elementPattern.defaultValue;
        notifyDotNet(dotnet, callback, containerBox.innerText);
    }
}

function notifyDotNet(dotnet: any, callback: string, value: string) {
    dotnet?.invokeMethodAsync(callback, value);
}

// --- Navigation helpers ---

function moveToNext(containerBox: HTMLDivElement, span: HTMLSpanElement) {
    const next = findNextEditableBlock(containerBox, span);
    if (next) {
        setTimeout(() => {
            next.click();
            selectTextOnClick(next);
        }, 10);
    }
}

function handleKeyDown(event: KeyboardEvent, span: HTMLSpanElement, containerBox: HTMLDivElement) {
    const cursor = getCursorPositionWithinSpan(span);
    const len = span.innerText.length;

    if (event.key === 'ArrowLeft' && cursor === 0) {
        event.preventDefault();
        const prev = findPreviousEditableBlock(containerBox, span);
        prev?.focus();
        if (prev) placeCaretAtEnd(prev);
    }

    if (event.key === 'ArrowRight' && cursor === len) {
        event.preventDefault();
        const next = findNextEditableBlock(containerBox, span);
        next?.focus();
        if (next) placeCaretAtStart(next);
    }

    if (event.key === 'Backspace' && cursor === 0 && len === 0) {
        event.preventDefault();
        const prev = findPreviousEditableBlock(containerBox, span);
        prev?.focus();
        if (prev) placeCaretAtEnd(prev);
    }
}

// --- Cursor & DOM helpers (idénticos) ---

function selectTextOnClick(span: HTMLSpanElement) {
    const range = document.createRange();
    range.selectNodeContents(span);
    const sel = window.getSelection();
    sel?.removeAllRanges();
    sel?.addRange(range);
}

function getCursorPositionWithinSpan(span: HTMLElement): number {
    const sel = window.getSelection();
    if (!sel || sel.rangeCount === 0) return 0;

    const range = sel.getRangeAt(0);
    const pre = range.cloneRange();
    pre.selectNodeContents(span);
    pre.setEnd(range.startContainer, range.startOffset);
    return pre.toString().length;
}

function setCursorPositionWithinSpan(span: HTMLElement, position: number) {
    const range = document.createRange();
    const sel = window.getSelection();

    try {
        range.setStart(span.childNodes[0] || span, Math.min(position, span.textContent?.length ?? 0));
        range.collapse(true);
        sel?.removeAllRanges();
        sel?.addRange(range);
    } catch {
        placeCaretAtEnd(span);
    }
}

function findNextEditableBlock(containerBox: HTMLDivElement, current: HTMLSpanElement): HTMLElement | null {
    let found = false;
    for (const child of containerBox.children) {
        if (child === current) found = true;
        else if (found && child instanceof HTMLSpanElement && child.contentEditable === 'true') {
            return child;
        }
    }
    return null;
}

function findPreviousEditableBlock(containerBox: HTMLDivElement, current: HTMLSpanElement): HTMLElement | null {
    let prev: HTMLElement | null = null;
    for (const child of containerBox.children) {
        if (child === current) return prev;
        if (child instanceof HTMLSpanElement && child.contentEditable === 'true') {
            prev = child;
        }
    }
    return null;
}

function placeCaretAtEnd(el: HTMLElement) {
    const range = document.createRange();
    const sel = window.getSelection();
    range.selectNodeContents(el);
    range.collapse(false);
    sel?.removeAllRanges();
    sel?.addRange(range);
}

function placeCaretAtStart(el: HTMLElement) {
    const range = document.createRange();
    const sel = window.getSelection();
    range.selectNodeContents(el);
    range.collapse(true);
    sel?.removeAllRanges();
    sel?.addRange(range);
}
