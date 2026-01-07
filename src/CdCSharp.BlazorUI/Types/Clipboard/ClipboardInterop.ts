export async function copyText(text: string): Promise<void> {
    if (!navigator.clipboard?.writeText) {
        throw new Error("Clipboard API not supported in this browser.");
    }

    await navigator.clipboard.writeText(text);
}
