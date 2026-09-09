import { submitExtraction } from "./api.js";

const form = document.getElementById("extraction-form") as HTMLFormElement;
const imagesInput = document.getElementById("images") as HTMLInputElement;
const platformInput = document.getElementById("platform") as HTMLInputElement;
const sourceUrlInput = document.getElementById("source-url") as HTMLInputElement;
const statusElement = document.getElementById("status") as HTMLParagraphElement;

form.addEventListener("submit", async (event) => {
    event.preventDefault();
    
    const files = imagesInput.files;
    if (!files || files.length === 0) {
        statusElement.textContent = "Please select at least one screenshot.";
        return;
    }
    
    statusElement.textContent = "Uploading and extracting...";
    
    try {
        const recipe = await submitExtraction({
            images: Array.from(files),
            platform: platformInput.value,
            ...(sourceUrlInput.value ? { sourceUrl: sourceUrlInput.value } : {}),
        });
        
        statusElement.textContent = `Success: "${recipe.titleTranslated ?? recipe.titleOriginal}" saved.`;
        form.reset();
    } catch (error) {
        statusElement.textContent = `Extraction failed: ${error instanceof Error ? error.message : "Unknown error"}`;
    }
});