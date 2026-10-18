<script setup lang="ts">
import type { Step } from '@/api/recipes';

defineProps<{ steps: Step[] }>()

const displayText = (step: Step) => step.textTranslated ?? step.textOriginal
</script>

<template>
    <section>
        <h2>Steps</h2>
        <ol>
            <li v-for="step in steps" :key="step.order">
                {{ displayText(step) }}
            </li>
        </ol>
    </section>
</template>