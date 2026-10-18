<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { submitExtraction } from '@/api/recipes'

const router = useRouter()

const files = ref<File[]>([])
const platform = ref('Xiaohongshu')
const sourceUrl = ref('')
const submitting = ref(false)
const error = ref<string | null>(null)

function onFilesSelected(event: Event) {
    const input = event.target as HTMLInputElement
    files.value = Array.from(input.files ?? [])
}

async function onSubmit() {
    if (submitting.value) return

    submitting.value = true
    error.value = null
    try {
        const recipe = await submitExtraction({
            images: files.value,
            platform: platform.value,
            sourceUrl: sourceUrl.value,
        })
        await router.push(`/recipes/${recipe.id}`)
    } catch (e) {
        error.value = e instanceof Error ? e.message : String(e)
    } finally {
        submitting.value = false
    }
}
</script>

<template>
    <main>
        <RouterLink to="/">← All Recipes</RouterLink>
        <fieldset class="extract-field" :disabled="submitting">
            <h1>Extract a recipe</h1>

            <form class="extract-form" @submit.prevent="onSubmit">
                <label>
                    Screenshots
                    <input type="file" accept="image/png,image/jpeg" multiple @change="onFilesSelected" />
                </label>

                <label>
                    Platform
                    <input v-model.trim="platform" type="text" />
                </label>

                <label>
                    SourceUrl (optional)
                    <input v-model.trim="sourceUrl" type="url" placeholder="https://…" />
                </label>

                <button type="submit" :disabled="files.length === 0 || submitting">
                    {{ submitting ? 'Extracting...' : 'Extract' }}
                </button>

                <p v-if="error" class="error" role="alert">{{ error }}</p>
            </form>
        </fieldset>
    </main>
</template>

<style scoped>
.extract-form {
    display: flex;
    flex-direction: column;
    gap: 1rem;
    max-width: 32rem;
}

.extract-form label {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;

    .extract-form button {
        align-self: flex-start;
    }
}

.extract-field {
    border: none;
    padding: 0;
    margin: 0;
}

.error {
    color: #dc2626
}
</style>