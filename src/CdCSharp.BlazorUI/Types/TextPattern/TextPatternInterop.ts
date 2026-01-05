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

    for (let index: number = 0; index < elements.length; index++) {
        let element: ElementPattern = elements[index];
        let span: HTMLSpanElement = document.createElement('span');

        span.innerText = element.value;
        if (element.isSeparator) {
            containerBox.appendChild(span);
            continue;
        }

        if (element.isEditable) {
            span.setAttribute('contenteditable', 'true');
            addTextPatternEvents(span, index, containerBox, element, dotnet, dotnetNotifyChangeTextCallback, dotnetValidatePartialCallback);
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
    const selectTextOnClickEvt = () => selectTextOnClick(span);

    if (!textPatternClickEvents.get(span)) {
        span.addEventListener("click", selectTextOnClickEvt);
        textPatternClickEvents.set(span, selectTextOnClickEvt);
    }

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

    const setDefaultValueNotLengthEvt = () => setDefaultValueNotLength(span, elementPattern);
    if (!textPatternBlurEvents.get(span)) {
        span.addEventListener("blur", setDefaultValueNotLengthEvt);
        textPatternBlurEvents.set(span, setDefaultValueNotLengthEvt);
    }
}

function selectTextOnClick(span: HTMLSpanElement) {
    const range = document.createRange();
    range.selectNodeContents(span);
    const selection = window.getSelection();
    if (!selection) { return; }
    selection.removeAllRanges();
    selection.addRange(range);
};

function goNextOrPrevent(
    span: HTMLSpanElement,
    index: number,
    containerBox: HTMLDivElement,
    elementPattern: ElementPattern,
    dotnet: any,
    dotnetCallback: string,
    dotnetValidatePartialCallback: string) {

    if (span.innerText.length == 0) {
        span.innerText = elementPattern.defaultValue;
        if (dotnet) {
            dotnet.invokeMethodAsync(dotnetCallback, containerBox.innerText);
        }
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
            let valid;
            if (splittedPattern[chIndex] === 'w') {
                valid = new RegExp('[a-zA-Z]', 'g').test(text[chIndex]);
            }
            else if (splittedPattern[chIndex] === 'd') {
                valid = new RegExp('[0-9]', 'g').test(text[chIndex]);
            }
            if (valid) {
                validText += text[chIndex];
            } else {
                cursor = cursor - 1;
            }
        }
        span.innerText = validText;
        setCursorPositionWithinSpan(span, cursor);

        if (span.innerText.length >= elementPattern.length) {
            const nextBlock = findNextEditableBlock(containerBox, span);
            if (nextBlock) {
                nextBlock.click();
            }
        }
        if (dotnet) {
            dotnet.invokeMethodAsync(dotnetCallback, containerBox.innerText);
        }

        if (span.innerText.length === elementPattern.length) {
            dotnet.invokeMethodAsync(dotnetValidatePartialCallback, index, span.innerText).then((valid: boolean) => {
                if (!valid) {
                    span.innerText = elementPattern.defaultValue;
                }
            });
        }
    } else {
        span.innerText = span.innerText.substring(0, elementPattern.length);
        placeCaretAtEnd(span);
        if (dotnet) {
            dotnet.invokeMethodAsync(dotnetCallback, containerBox.innerText);
        }
    }
};

function setDefaultValueNotLength(span: HTMLSpanElement, elementPattern: ElementPattern) {
    if (span.innerText.length != elementPattern.length) {
        span.innerText = elementPattern.defaultValue;
        return;
    }
}

// --- Helpers de Cursor y DOM (Idénticos) ---

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

    if (selection) {
        range.setStart(spanElement.childNodes[0] || spanElement, position);
        range.collapse(true);
        selection.removeAllRanges();
        selection.addRange(range);
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