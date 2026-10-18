import { ref, watch, type Ref } from 'vue'

export function useLocalStorage<T extends string>(
    key: string,
    defaultValue: T,
    isValid: (value: unknown) => value is T,
): Ref<T> {
    function load(): T {
        try {
            const stored = localStorage.getItem(key)
            return isValid(stored) ? stored : defaultValue
        } catch {
            return defaultValue
        }
    }

    const value = ref(load()) as Ref<T>

    watch(value, (newValue) => {
        try {
            localStorage.setItem(key, newValue)
        } catch {
        }
    })

    return value
}

