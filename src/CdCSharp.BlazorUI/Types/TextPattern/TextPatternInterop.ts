export interface ElementPattern {
    pattern: string;
    value: string;
    length: number;
    defaultValue: string;
    isSeparator: boolean;
    isEditable: boolean;
}

const textPatternEvents = {
    click: new WeakMap<HTMLSpanElement, EventListener>(),
    input: new WeakMap<HTMLSpanElement, EventListener>(),
    blur: new WeakMap<HTMLSpanElement, EventListener>(),
    keydown: new WeakMap<HTMLSpanElement, EventListener>()
};

export async function TextPatternAddDynamic(
    containerBox: HTMLDivElement,
    elements: Array<ElementPattern>,
    dotnet: any,
    dotnetNotifyChangeTextCallback: string,
    dotnetValidatePartialCallback: string): Promise<void> {

    if (!containerBox || !containerBox.appendChild) {
        return;
    }

    cleanupContainer(containerBox);

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

function cleanupContainer(containerBox: HTMLDivElement) {
    const spans = containerBox.querySelectorAll('span[contenteditable]');
    spans.forEach(span => {
        removeAllEvents(span as HTMLSpanElement);
    });
}

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

    const existingHandler = map.get(span);
    if (existingHandler) {
        span.removeEventListener(type, existingHandler);
    }

    span.addEventListener(type, handler);
    map.set(span, handler);
}

function removeAllEvents(span: HTMLSpanElement) {
    const eventTypes: Array<keyof typeof textPatternEvents> = ['click', 'input', 'blur', 'keydown'];

    eventTypes.forEach(type => {
        const handler = textPatternEvents[type].get(span);
        if (handler) {
            span.removeEventListener(type, handler);
            textPatternEvents[type].delete(span);
        }
    });
}

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
            })
            .catch((error: any) => {
                console.error('Validation error:', error);
            });
    }

    notifyDotNet(dotnet, notifyCallback, containerBox.innerText);
}

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
        cursor: Math.min(newCursor, result.length)
    };
}

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
    if (dotnet && callback) {
        dotnet.invokeMethodAsync(callback, value)
            .catch((error: any) => {
                console.error('DotNet notification error:', error);
            });
    }
}

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
        if (prev) {
            prev.focus();
            placeCaretAtEnd(prev);
        }
    }

    if (event.key === 'ArrowRight' && cursor === len) {
        event.preventDefault();
        const next = findNextEditableBlock(containerBox, span);
        if (next) {
            next.focus();
            placeCaretAtStart(next);
        }
    }

    if (event.key === 'Backspace' && cursor === 0 && len === 0) {
        event.preventDefault();
        const prev = findPreviousEditableBlock(containerBox, span);
        if (prev) {
            prev.focus();
            placeCaretAtEnd(prev);
        }
    }
}

function selectTextOnClick(span: HTMLSpanElement) {
    const range = document.createRange();
    range.selectNodeContents(span);
    const sel = window.getSelection();
    if (sel) {
        sel.removeAllRanges();
        sel.addRange(range);
    }
}

function getCursorPositionWithinSpan(span: HTMLElement): number {
    const sel = window.getSelection();
    if (!sel || sel.rangeCount === 0) return 0;

    try {
        const range = sel.getRangeAt(0);
        const pre = range.cloneRange();
        pre.selectNodeContents(span);
        pre.setEnd(range.startContainer, range.startOffset);
        return pre.toString().length;
    } catch {
        return 0;
    }
}

function setCursorPositionWithinSpan(span: HTMLElement, position: number) {
    const range = document.createRange();
    const sel = window.getSelection();

    if (!sel) return;

    try {
        const textNode = span.childNodes[0] || span;
        const maxPosition = span.textContent?.length ?? 0;
        const safePosition = Math.min(position, maxPosition);

        range.setStart(textNode, safePosition);
        range.collapse(true);
        sel.removeAllRanges();
        sel.addRange(range);
    } catch {
        placeCaretAtEnd(span);
    }
}

function findNextEditableBlock(containerBox: HTMLDivElement, current: HTMLSpanElement): HTMLSpanElement | null {
    let found = false;
    for (const child of containerBox.children) {
        if (child === current) {
            found = true;
        } else if (found && child instanceof HTMLSpanElement && child.contentEditable === 'true') {
            return child;
        }
    }
    return null;
}

function findPreviousEditableBlock(containerBox: HTMLDivElement, current: HTMLSpanElement): HTMLSpanElement | null {
    let prev: HTMLSpanElement | null = null;
    for (const child of containerBox.children) {
        if (child === current) {
            return prev;
        }
        if (child instanceof HTMLSpanElement && child.contentEditable === 'true') {
            prev = child;
        }
    }
    return null;
}

function placeCaretAtEnd(el: HTMLElement) {
    const range = document.createRange();
    const sel = window.getSelection();
    if (sel) {
        range.selectNodeContents(el);
        range.collapse(false);
        sel.removeAllRanges();
        sel.addRange(range);
    }
}

function placeCaretAtStart(el: HTMLElement) {
    const range = document.createRange();
    const sel = window.getSelection();
    if (sel) {
        range.selectNodeContents(el);
        range.collapse(true);
        sel.removeAllRanges();
        sel.addRange(range);
    }
}