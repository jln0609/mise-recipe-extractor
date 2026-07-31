

## Setting things up

### 1. Core domain entities
* Recipe, Ingredient, Step, RecipeVersion, value objects like Quantity

### 2. Core interfaces (ports)
* IRecipeRepository, IRecipeExtractor

### 3. Core.Tests

### 4. Adapter one
* e.g. fake/in-memory implementation fo IRecipeRepository with real RecipesController
* prove API -> Core -> "storage" works (without any AI yet)

### 5. Real Infrastructure
* make persistance real (swap fake memory for real EfRecipeRepository and add DbContext)

### 6. AI extraction
* implement IRecipeExtractor (Anthropic API call, prompt, response parsing into Recipe)

### 7. Qire together in the API
* full POST /recipes/extract flow hitting real AI and real DB
* integration tests covering whole path

### iOS ingestion
* once API is stable enough not to change too often
* shortcuts -> webhook