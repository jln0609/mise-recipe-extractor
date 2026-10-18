<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <header class="site-header">
    <RouterLink to="/" class="brand">
      <!-- Empty alt: the visible "Mise" text already names the link for screen readers -->
      <img src="@/assets/mise-logo.svg" alt="" width="36" height="36" />
      <span>Mise</span>
    </RouterLink>
    <nav class="site-nav">
      <RouterLink to="/extract">Extract</RouterLink>
    </nav>
  </header>

  <div class="page">
    <RouterView />
  </div>
</template>

<style scoped>
.site-header {
  position: sticky;
  top: 0;
  z-index: 10;
  padding: 0.75rem 1.5rem;
  background-color: var(--color-background);
  border-bottom: 1px solid var(--color-border);
}

.site-nav a.router-link-active {
  font-weight: 600;
}

.brand {
  display: inline-flex;
  align-items: center;
  gap: 0.6rem;
  font-size: 1.4rem;
  font-weight: 600;
  color: var(--color-heading);
  text-decoration: none;
}

/* The starter CSS adds a green hover background to all links; not wanted on the logo */
.brand:hover {
  background-color: transparent;
}

.page {
  max-width: 960px;
  margin: 0 auto;
  padding: 1.5rem;
}

</style>