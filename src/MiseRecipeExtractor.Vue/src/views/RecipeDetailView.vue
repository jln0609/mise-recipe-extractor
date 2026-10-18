<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { getRecipeVersions, type RecipeVersion } from '@/api/recipes'
import { useSelectedVersion } from '@/composables/useSelectedVersion';
import IngredientList from '@/components/IngredientList.vue';
import StepList from '@/components/StepList.vue';

const props = defineProps<{ id: string }>()

const versions = ref<RecipeVersion[]>([])
const loading = ref(true)
const error = ref<string | null>(null)

const { versionsNewestFirst, latestVersion, selectedVersion, selectedVersionNumber } = useSelectedVersion(versions)

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
        <p v-else-if="!selectedVersion">This recipe has no versions.</p>

        <template v-else>
            <h1>{{ selectedVersion.titleTranslated ?? selectedVersion.titleOriginal  }}</h1>
            <p v-if="selectedVersion.titleTranslated">{{ selectedVersion.titleOriginal }}</p>

            <select
                v-if="versions.length > 1"
                v-model="selectedVersionNumber"
                aria-label="Recipe version"
            >
                <option
                    v-for="version in versionsNewestFirst"
                    :key="version.versionNumber"
                    :value="version.versionNumber"
                >
                    Version {{ version.versionNumber }}{{ version === latestVersion ? ' (latest)' : '' }}
                </option>
            </select>

            <IngredientList v-bind:ingredients="selectedVersion.ingredients" />

            <StepList v-bind:steps="selectedVersion.steps" />

            <template v-if="selectedVersion.notes">
                <h2>Notes</h2>
                <p>{{ selectedVersion.notes }}</p>
            </template>

            <details v-if="selectedVersion.warnings.length > 0" class="extraction-warnings">
                <summary>Extraction warnings ({{ selectedVersion.warnings.length }})</summary>
                <ul>
                    <li v-for="(warning, index) in selectedVersion.warnings" v-bind:key="index">{{ warning }}</li>
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