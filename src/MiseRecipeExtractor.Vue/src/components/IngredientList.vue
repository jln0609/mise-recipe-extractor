<script setup lang="ts">
import { computed } from 'vue'
import type { Ingredient } from '@/api/recipes'
import { useLocalStorage } from '@/composables/useLocalStorage';

type SortMode = 'source' | 'alphabetical'

const isSortMode = (value: unknown): value is SortMode => value === 'source' || value === 'alphabetical'

const props = defineProps<{ ingredients: Ingredient[] }>()

const sortMode = useLocalStorage<SortMode>('mise.ingredientSortMode', 'source', isSortMode)

const displayName = (ingredient: Ingredient) => ingredient.nameTranslated ?? ingredient.nameOriginal

const collator = new Intl.Collator('en', { sensitivity: 'base'})

const displayedIngredients = computed(() => sortMode.value === 'source' ? props.ingredients : [...props.ingredients].sort((a,b) => collator.compare(displayName(a), displayName(b)))
)
</script>

<template>
    <section>
        <div class="ingredients-header">
            <h2>Ingredients</h2>
            <select v-model="sortMode" aria-label="Sort ingredients">
                <option value="source">As listed</option>
                <option value="alphabetical">A-Z</option>
            </select>
        </div>
        <table class="ingredients">
            <tbody>
                <tr v-for="(ingredient, index) in displayedIngredients" :key="index">
                    <td>
                        {{ ingredient.nameTranslated ?? ingredient.nameOriginal }}
                        <div v-if="ingredient.notes" class="notes">{{ ingredient.notes }}</div>
                    </td>
                    <td class="amount">{{ ingredient.quantityOriginalText }}</td>
                </tr>
            </tbody>
        </table>
    </section>
</template>

<style scoped>
.ingredients {
    border-collapse: collapse;
}

.ingredients td {
    padding: 0.25rem 1 rem 0.25rem 0;
    vertical-align: top;
}

.ingredients-header {
    display: flex;
    align-items: baseline;
    gap: 1rem;
}

.notes {
    font-size: 0.85em;
    opacity: 0.7;
}
</style>