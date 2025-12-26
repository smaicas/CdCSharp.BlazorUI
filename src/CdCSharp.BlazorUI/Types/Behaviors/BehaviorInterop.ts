interface RippleConfiguration {
    color?: string;
    duration?: number;
}

interface BehaviorConfiguration {
    ripple?: RippleConfiguration;
}

class RippleBehavior {
    private element: HTMLElement;
    private config: RippleConfiguration;
    private clickHandler: (e: MouseEvent) => void;

    constructor(element: HTMLElement, config: RippleConfiguration) {
        this.element = element;
        this.config = config;
        this.clickHandler = this.handleClick.bind(this);
        this.element.addEventListener('click', this.clickHandler);
    }

    private handleClick(e: MouseEvent) {
        const rect = this.element.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        const x = e.clientX - rect.left - size / 2;
        const y = e.clientY - rect.top - size / 2;

        const ripple = document.createElement('span');
        ripple.className = 'ui-ripple';
        ripple.style.width = ripple.style.height = size + 'px';
        ripple.style.left = x + 'px';
        ripple.style.top = y + 'px';

        if (this.config.color) {
            ripple.style.backgroundColor = this.config.color;
        }

        const duration = this.config.duration || 600;
        ripple.style.animationDuration = `${duration}ms`;

        this.element.appendChild(ripple);

        setTimeout(() => {
            ripple.remove();
        }, duration);
    }

    dispose() {
        this.element.removeEventListener('click', this.clickHandler);
    }
}

class BehaviorManager {
    private behaviors: Array<{ dispose: () => void }> = [];

    constructor(element: HTMLElement, config: BehaviorConfiguration) {
        if (config.ripple) {
            this.behaviors.push(new RippleBehavior(element, config.ripple));
        }
    }

    dispose() {
        this.behaviors.forEach(b => b.dispose());
        this.behaviors = [];
    }
}

export function attachBehaviors(
    element: HTMLElement,
    config: BehaviorConfiguration
): BehaviorManager {
    return new BehaviorManager(element, config);
}