import { computed, type Ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import type { RecipeVersion } from '@/api/recipes'

export function useSelectedVersion(versions: Ref<RecipeVersion[]>) {
    const route = useRoute()
    const router = useRouter()

    const versionsNewestFirst = computed(() => [...versions.value].sort((a,b) => b.versionNumber - a.versionNumber))

    const latestVersion = computed<RecipeVersion | null> (() => versionsNewestFirst.value[0] ?? null)

    const requestedVersionNumber = computed<number | null>(() => {
        const raw = route.query.v
        if (typeof raw !== 'string') return null
        const parsed = Number(raw)
        return Number.isInteger(parsed) ? parsed : null
    })

    const selectedVersion = computed<RecipeVersion | null>(() => versions.value.find(v => v.versionNumber === requestedVersionNumber.value) ?? latestVersion.value)

    const selectedVersionNumber = computed<number | undefined>({
        get: () => selectedVersion.value?.versionNumber,
        set: (versionNumber) => {
            router.push({ query: { ...route.query, v: String(versionNumber) } })
        },
    })

    return { versionsNewestFirst, latestVersion, selectedVersion, selectedVersionNumber }
}