import { getRecipeVersion, getRecipeVersions, type RecipeVersion} from "./api.js";

const versionSelect = document.getElementById("version-select") as HTMLSelectElement;
const titleElement = document.getElementById("recipe-title") as HTMLHeadingElement;
const ingredientsList = document.getElementById("ingredients-list") as HTMLUListElement;
const stepsList = document.getElementById("steps-list") as HTMLOListElement;
const notesElement = document.getElementById("notes") as HTMLParagraphElement;
const warningsElement = document.getElementById("warnings") as HTMLDivElement;

const params = new URLSearchParams(window.location.search);
const recipeId = params.get("id");

function renderVersion(version: RecipeVersion): void {
    titleElement.textContent = version.titleTranslated ?? version.titleOriginal;
    
    ingredientsList.innerHTML = "";
    for (const ingredient of version.ingredients) {
        const item = document.createElement("li");
        const name = ingredient.nameTranslated ?? ingredient.nameOriginal;
        const quantity = ingredient.quantityAmount !== null
            ? `${ingredient.quantityAmount}${ingredient.quantityUnit ?? ""}`
            : ingredient.quantityOriginalText
        item.textContent = `${quantity} ${name}`;
        ingredientsList.appendChild(item);
    }
    
    stepsList.innerHTML = "";
    for (const step of version.steps) {
        const item = document.createElement("li");
        item.textContent = step.textTranslated ?? step.textOriginal;
        stepsList.appendChild(item);
    }
    
    if (version.notes) {
        notesElement.textContent = version.notes;
        notesElement.hidden = false;
    } else {
        notesElement.hidden = true;
    }
    
    warningsElement.innerHTML = "";
    if (version.warnings.length > 0) {
        const heading = document.createElement("h2");
        heading.textContent = "Warnings";
        warningsElement.appendChild(heading);
        
        const list = document.createElement("ul");
        for (const warning of version.warnings) {
            const item = document.createElement("li");
            item.textContent = warning;
            list.appendChild(item);
        }
        warningsElement.appendChild(list);
        warningsElement.hidden = false;
    } else {
        warningsElement.hidden = true;
    }
}

async  function main(): Promise<void> {
    if (!recipeId) {
        titleElement.textContent = "No recipe ID provided.";
        return;
    }
    
    try {
        const versions = await getRecipeVersions(recipeId);
        
        versionSelect.innerHTML = "";
        for (const version of versions) {
            const option = document.createElement("option");
            option.value = version.versionNumber.toString();
            option.textContent = `Version ${version.versionNumber} (${version.status})`;
            versionSelect.appendChild(option);
        }
        
        const latestVersion = versions[versions.length - 1];
        if (!latestVersion) {
            titleElement.textContent = "This recipe has no versions.";
            return;
        }
        versionSelect.value = latestVersion.versionNumber.toString();
        renderVersion(latestVersion);
        
        versionSelect.addEventListener("change", async () => {
            const selectedVersion = await getRecipeVersion(recipeId, Number(versionSelect.value));
            renderVersion(selectedVersion);
        });
    } catch (error) {
        titleElement.textContent = `Failed to load recipe: ${error instanceof Error ? error.message : "Unknown error"}`;
    }
}

main();