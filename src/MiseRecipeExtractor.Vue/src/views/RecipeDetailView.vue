<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { getRecipeVersions, type RecipeVersion } from '@/api/recipes'

const props = defineProps<{ id: string }>()

const versions = ref<RecipeVersion[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

const latestVersion = computed<RecipeVersion | null> (() => [...versions.value].sort((a,b) => b.versionNumber - a.versionNumber)[0] ?? null,)

onMounted(async () => {
    try {
        versions.value = await getRecipeVersions(props.id)
    } catch (e) {
        error.value = e instanceof Error ? e.message : String(e)
    } finally {loading.value = false}
})
</script>

<template>
    <main>
        <RouterLink to="/">← All Recipes</RouterLink>

        <p v-if="loading">Loading…</p>
        <p v-else-if="error">Could not load recipe: {{ error }}</p>
        <p v-else-if="!latestVersion">This recipe has no versions.</p>

        <template v-else>
            <h1>{{ latestVersion.titleTranslated ?? latestVersion.titleOriginal  }}</h1>
            <p v-if="latestVersion.titleTranslated">{{ latestVersion.titleOriginal }}</p>

            <h2>Ingredients</h2>
            <ul>
                <!-- Index key: the ingredient DTO has no id, and this list is read-only. -->
                <li v-for="(ingredient, index) in latestVersion.ingredients" :key="index">
                    {{ ingredient.nameTranslated ?? ingredient.nameOriginal }}:
                    {{ ingredient.quantityOriginalText }}
                    <span v-if="ingredient.notes">({{ ingredient.notes }})</span>
                </li>
            </ul>

            <h2>Steps</h2>
            <ol>
                <li v-for="step in latestVersion.steps" :key="step.order">
                    {{ step.textTranslated ?? step.textOriginal }}
                </li>
            </ol>

            <template v-if="latestVersion.notes">
                <h2>Notes</h2>
                <p>{{ latestVersion.notes }}</p>
            </template>

            <template v-if="latestVersion.warnings.length > 0">
                <h2>Warnings</h2>
                <ul>
                    <li v-for="(warning, index) in latestVersion.warnings" :key="index">{{ warning }}</li>
                </ul>
            </template>
        </template>
    </main>
</template>