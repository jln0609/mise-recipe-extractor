<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { getRecipeVersions, type RecipeVersion } from '@/api/recipes'
import IngredientList from '@/components/IngredientList.vue';
import StepList from '@/components/StepList.vue';

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

            <IngredientList v-bind:ingredients="latestVersion.ingredients" />

            <StepList v-bind:steps="latestVersion.steps" />

            <template v-if="latestVersion.notes">
                <h2>Notes</h2>
                <p>{{ latestVersion.notes }}</p>
            </template>

            <details v-if="latestVersion.warnings.length > 0" class="extraction-warnings">
                <summary>Extraction warnings ({{ latestVersion.warnings.length }})</summary>
                <ul>
                    <li v-for="(warning, index) in latestVersion.warnings" v-bind:key="index">{{ warning }}</li>
                </ul>
            </details>
        </template>
    </main>
</template>

<style scoped>
.extraction-warnings {
    margin-top: 2rem;
    padding: 0.75rem 1rem;
    border-left: 4px solid #d97706;
    background: rgba(217, 119, 6, 0.08);
    border-radius: 4px;
    font-size: 0.9em;
}

.extraction-warnings summary {
    cursor: pointer;
    font-weight: 600;
}

.extraction-warnings ul {
    margin: 0.5rem 0 0;
    padding-left: 1.25rem;
}
</style>