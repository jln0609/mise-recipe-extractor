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
        throw new Error(`Failed to submit extraction requesst: ${response.status}`);
    }
    return response.json();
}