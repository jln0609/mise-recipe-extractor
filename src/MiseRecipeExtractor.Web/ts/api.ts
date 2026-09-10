//
// extracting recipes
//

export interface Recipe {
    id: string;
    platform: string;
    sourceUrl: string | null;
    currentVersionNumber: number;
    titleOriginal: string;
    titleTranslated: string | null;
    status: string;
    warnings: string[];
    notes: string | null;
}

const API_BASE_URL = "http://192.168.1.198:5249";

export async function getRecipes(): Promise<Recipe[]> {
    const response = await fetch(`${API_BASE_URL}/api/recipes`);
    if (!response.ok) {
        throw new Error(`Failed to fetch recipes: ${response.status}`);
    }
    return response.json();
}

export interface ExtractionRequest {
    images: File[],
    platform: string,
    sourceUrl?: string
}

export async function submitExtraction(request: ExtractionRequest): Promise<Recipe> {
    const formData = new FormData();
    for (const image of request.images) {
        formData.append("Images", image);
    }
    formData.append("Platform", request.platform);
    if (request.sourceUrl) {
        formData.append("SourceUrl", request.sourceUrl);
    }
    
    const response = await fetch(`${API_BASE_URL}/api/extractions`, {method: "POST", body: formData});
    if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || `Failed to submit extraction request: ${response.status}`);

    }
    return response.json();
}

//
// viewing recipes
//
export interface Ingredient {
    nameOriginal: string;
    nameTranslated: string | null;
    quantityOriginalText: string;
    quantityAmount: number | null;
    quantityUnit: string | null;
    quantityConfidence: string;
    notes: string | null;
}

export interface Step {
    order: number;
    textOriginal: string;
    textTranslated: string | null;
    durationSeconds: number | null;
    orderIsInferred: boolean;
}

export interface RecipeVersion {
    versionNumber: number;
    status: string;
    titleOriginal: string;
    titleTranslated: string | null;
    ingredients: Ingredient[];
    steps: Step[];
    warnings: string[];
    notes: string | null;
    createdAt: string;
}

export async function getRecipeVersions(recipeId: string): Promise<RecipeVersion[]> {
    const response = await fetch(`${API_BASE_URL}/api/recipes/${recipeId}/versions`);
    if (!response.ok) {
        throw new Error(`Failed to fetch recipe versions: ${response.status}`);
    }
    return response.json();
}

export async function getRecipeVersion(recipeId: string, versionNumber: number): Promise<RecipeVersion> {
    const response = await fetch(`${API_BASE_URL}/api/recipes/${recipeId}/versions/${versionNumber}`);
    if (!response.ok) {
        throw new Error(`Failed to fetch recipe version ${versionNumber}: ${response.status}`);
    }
    return response.json();
}