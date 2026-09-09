import { getRecipes } from "./api.js";

const listElement = document.getElementById("recipe-list") as HTMLUListElement;

async function loadRecipes(): Promise<void> {
    try {
        const recipes = await getRecipes();
        listElement.innerHTML = "";
        
        for (const recipe of recipes) {
            const item = document.createElement("li");
            const link = document.createElement("a");
            link.href = `recipe.html?id=${recipe.id}`;
            link.textContent = recipe.titleTranslated ?? recipe.titleOriginal;
            item.appendChild(link);
            listElement.appendChild(item);
        }
    } catch (error) {
        listElement.innerHTML = "";
        const errorItem = document.createElement("li");
        errorItem.textContent = `Failed to load recipes: ${error instanceof Error ? error.message : "Unknown error"}`;
        listElement.appendChild(errorItem);
    }
}

loadRecipes();
