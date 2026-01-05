export interface ElementPattern {
    pattern: string;
    value: string;
    length: number;
    defaultValue: string;
    isSeparator: boolean;
    isEditable: boolean;
}

// Mantenemos los WeakMaps como variables privadas del módulo
let textPatternClickEvents = new WeakMap<HTMLSpanElement, EventListener>();
let textPatternInputEvents = new WeakMap<HTMLSpanElement, EventListener>();
let textPatternBlurEvents = new WeakMap<HTMLSpanElement, EventListener>();
let textPatternKeyDownEvents = new WeakMap<HTMLSpanElement, EventListener>();

/**
 * Función principal exportada que Blazor llamará directamente.
 * Mantiene la lógica exacta de limpieza e inicialización original.
 */
export async function TextPatternAddDynamic(
    containerBox: HTMLDivElement,
    elements: Array<ElementPattern>,
    dotnet: any,
    dotnetNotifyChangeTextCallback: string,
    dotnetValidatePartialCallback: string): Promise<void> {
    if (!containerBox.appendChild) { return; }

    containerBox.innerHTML = '';
    containerBox.classList.add('text-pattern-container');

    let editableIndex = 0; // Track only editable elements for validation callback

    for (let i: number = 0; i < elements.length; i++) {
        let element: ElementPattern = elements[i];
        let span: HTMLSpanElement = document.createElement('span');

        span.innerText = element.value;
        span.classList.add(element.isSeparator ? 'pattern-separator' : 'pattern-component');

        if (element.isSeparator) {
            containerBox.appendChild(span);
            continue;
        }

        if (element.isEditable) {
            span.setAttribute('contenteditable', 'true');
            span.dataset.index = editableIndex.toString();
            span.dataset.length = element.length.toString();
            span.dataset.defaultValue = element.defaultValue;
            addTextPatternEvents(span, editableIndex, containerBox, element, dotnet, dotnetNotifyChangeTextCallback, dotnetValidatePartialCallback);
            editableIndex++;
        }
        containerBox.appendChild(span);
    }
}

function addTextPatternEvents(
    span: HTMLSpanElement,
    index: number,
    containerBox: HTMLDivElement,
    elementPattern: ElementPattern,
    dotnet: any,
    dotnetNotifyChangeTextCallback: string,
    dotnetValidatePartialCallback: string) {

    // Click event - select all text
    const selectTextOnClickEvt = () => selectTextOnClick(span);
    if (!textPatternClickEvents.get(span)) {
        span.addEventListener("click", selectTextOnClickEvt);
        textPatternClickEvents.set(span, selectTextOnClickEvt);
    }

    // Input event - validate and navigate
    const goNextOrPreventEvt = () =>
        goNextOrPrevent(
            span,
            index,
            containerBox,
            elementPattern,
            dotnet,
            dotnetNotifyChangeTextCallback,
            dotnetValidatePartialCallback);
    if (!textPatternInputEvents.get(span)) {
        span.addEventListener("input", goNextOrPreventEvt);
        textPatternInputEvents.set(span, goNextOrPreventEvt);
    }

    // Blur event - validate and set default
    const setDefaultValueNotLengthEvt = () => setDefaultValueNotLength(span, elementPattern, dotnet, dotnetNotifyChangeTextCallback, containerBox);
    if (!textPatternBlurEvents.get(span)) {
        span.addEventListener("blur", setDefaultValueNotLengthEvt);
        textPatternBlurEvents.set(span, setDefaultValueNotLengthEvt);
    }

    // KeyDown event - handle navigation keys
    const handleKeyDownEvt = (e: Event) => handleKeyDown(e as KeyboardEvent, span, containerBox);
    if (!textPatternKeyDownEvents.get(span)) {
        span.addEventListener("keydown", handleKeyDownEvt);
        textPatternKeyDownEvents.set(span, handleKeyDownEvt);
    }
}

function selectTextOnClick(span: HTMLSpanElement) {
    const range = document.createRange();
    range.selectNodeContents(span);
    const selection = window.getSelection();
    if (!selection) { return; }
    selection.removeAllRanges();
    selection.addRange(range);
}

function handleKeyDown(event: KeyboardEvent, span: HTMLSpanElement, containerBox: HTMLDivElement) {
    const key = event.key;
    const cursorPosition = getCursorPositionWithinSpan(span);
    const textLength = span.innerText.length;

    if (key === 'ArrowLeft' && cursorPosition === 0) {
        // Move to previous editable field
        event.preventDefault();
        const prevField = findPreviousEditableBlock(containerBox, span);
        if (prevField) {
            prevField.focus();
            placeCaretAtEnd(prevField);
        }
    } else if (key === 'ArrowRight' && cursorPosition === textLength) {
        // Move to next editable field
        event.preventDefault();
        const nextField = findNextEditableBlock(containerBox, span);
        if (nextField) {
            nextField.focus();
            placeCaretAtStart(nextField);
        }
    } else if (key === 'Backspace' && cursorPosition === 0 && textLength === 0) {
        // Move to previous field if current is empty
        event.preventDefault();
        const prevField = findPreviousEditableBlock(containerBox, span);
        if (prevField) {
            prevField.focus();
            placeCaretAtEnd(prevField);
        }
    }
}

function goNextOrPrevent(
    span: HTMLSpanElement,
    index: number,
    containerBox: HTMLDivElement,
    elementPattern: ElementPattern,
    dotnet: any,
    dotnetCallback: string,
    dotnetValidatePartialCallback: string) {

    if (span.innerText.length == 0) {
        // Don't set default value immediately during input
        return;
    }

    if (span.innerText.length <= elementPattern.length) {
        let cursor = getCursorPositionWithinSpan(span);
        let flattenedPattern = elementPattern.pattern
            .replace(/[\\\(\)\^\$]/g, "");
        let text = span.innerText;
        let splittedPattern = flattenedPattern.substring(0, text.length).split('');

        let validText = '';
        for (let chIndex = 0; chIndex < text.length; chIndex++) {
            let valid = false;
            let char = text[chIndex];
            let patternChar = splittedPattern[chIndex];

            if (patternChar === 'w') {
                valid = /[a-zA-Z]/.test(char);
            }
            else if (patternChar === 'd') {
                valid = /[0-9]/.test(char);
            }

            if (valid) {
                validText += char;
            } else {
                cursor = Math.max(0, cursor - 1);
            }
        }

        span.innerText = validText;
        setCursorPositionWithinSpan(span, cursor);

        // Check if we've reached the maximum length
        if (span.innerText.length >= elementPattern.length) {
            // Validate the component before moving to next
            dotnet.invokeMethodAsync(dotnetValidatePartialCallback, index, span.innerText).then((valid: boolean) => {
                if (valid) {
                    const nextBlock = findNextEditableBlock(containerBox, span);
                    if (nextBlock) {
                        setTimeout(() => {
                            nextBlock.click();
                            selectTextOnClick(nextBlock);
                        }, 10);
                    }
                } else {
                    // Invalid value, reset to default
                    span.innerText = elementPattern.defaultValue;
                    selectTextOnClick(span);
                }
            });
        }

        // Notify changes
        if (dotnet) {
            dotnet.invokeMethodAsync(dotnetCallback, containerBox.innerText);
        }
    } else {
        // Text is too long, truncate it
        span.innerText = span.innerText.substring(0, elementPattern.length);
        placeCaretAtEnd(span);
        if (dotnet) {
            dotnet.invokeMethodAsync(dotnetCallback, containerBox.innerText);
        }
    }
}

function setDefaultValueNotLength(
    span: HTMLSpanElement,
    elementPattern: ElementPattern,
    dotnet: any,
    dotnetCallback: string,
    containerBox: HTMLDivElement) {

    if (span.innerText.length === 0 || span.innerText.length !== elementPattern.length) {
        span.innerText = elementPattern.defaultValue;
        if (dotnet) {
            dotnet.invokeMethodAsync(dotnetCallback, containerBox.innerText);
        }
    }
}

// --- Helpers de Cursor y DOM ---

function getCursorPositionWithinSpan(spanElement: HTMLElement): number {
    let cursorPosition = 0;

    if (window.getSelection) {
        let selection = window.getSelection();
        if (selection && selection.rangeCount > 0) {
            let range = selection.getRangeAt(0);
            let preSelectionRange = range.cloneRange();
            preSelectionRange.selectNodeContents(spanElement);
            preSelectionRange.setEnd(range.startContainer, range.startOffset);
            cursorPosition = preSelectionRange.toString().length;
        }
    }

    return cursorPosition;
}

function setCursorPositionWithinSpan(spanElement: HTMLElement, position: number): void {
    let range = document.createRange();
    let selection = window.getSelection();

    if (selection && spanElement.childNodes.length > 0) {
        try {
            range.setStart(spanElement.childNodes[0] || spanElement, Math.min(position, spanElement.textContent?.length || 0));
            range.collapse(true);
            selection.removeAllRanges();
            selection.addRange(range);
        } catch (e) {
            // Handle range errors gracefully
            placeCaretAtEnd(spanElement);
        }
    }
}

function findNextEditableBlock(containerBox: HTMLDivElement, current: HTMLSpanElement): HTMLElement | null {
    let foundCurrent = false;
    for (const child of containerBox.children) {
        if (child === current) {
            foundCurrent = true;
        } else if (foundCurrent && child instanceof HTMLSpanElement && child.contentEditable === "true") {
            return child;
        }
    }
    return null;
}

function findPreviousEditableBlock(containerBox: HTMLDivElement, current: HTMLSpanElement): HTMLElement | null {
    let previous: HTMLElement | null = null;
    for (const child of containerBox.children) {
        if (child === current) {
            return previous;
        } else if (child instanceof HTMLSpanElement && child.contentEditable === "true") {
            previous = child;
        }
    }
    return null;
}

function placeCaretAtEnd(el: HTMLElement) {
    const range = document.createRange();
    const selection = window.getSelection();

    range.selectNodeContents(el);
    range.collapse(false);

    if (selection) {
        selection.removeAllRanges();
        selection.addRange(range);
    }
}

function placeCaretAtStart(el: HTMLElement) {
    const range = document.createRange();
    const selection = window.getSelection();

    range.selectNodeContents(el);
    range.collapse(true);

    if (selection) {
        selection.removeAllRanges();
        selection.addRange(range);
    }
}