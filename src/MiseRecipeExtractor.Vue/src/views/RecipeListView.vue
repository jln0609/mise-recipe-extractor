<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { getRecipes, type Recipe } from '@/api/recipes'

const recipes = ref<Recipe[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

onMounted(async () => {
    try {
        recipes.value = await getRecipes()
    } catch (e) {
        error.value = e instanceof Error ? e.message : String(e)
    } finally {
        loading.value = false
    }
})
</script>

<template>
    <main>
        <h1>Recipes</h1>

        <p v-if="loading">Loading...</p>
        <p v-else-if="error">Could not load recipes: {{ error }}</p>
        <p v-else-if="recipes.length === 0">No recipes yet.</p>
        <ul v-else>
            <li v-for="recipe in recipes" ;key="recipe.id">
                <RouterLink :to="`/recipes/${recipe.id}`">
                    {{ recipe.titleTranslated ?? recipe.titleOriginal }}
                </RouterLink>
            </li>
        </ul>
    </main>
</template>