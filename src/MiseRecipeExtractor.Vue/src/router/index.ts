import { createRouter, createWebHistory } from 'vue-router'
import RecipeListView from '@/views/RecipeListView.vue'
import ExtracyView from '@/views/ExtractView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'recipes',
      component: RecipeListView,
    },
    {
      path: '/recipes/:id',
      name: 'recipe',
      component: () => import('@/views/RecipeDetailView.vue'),
      props: true,
    },
    {
      path: '/extract',
      name: 'extract',
      component: () => import('@/views/ExtractView.vue')
    },
  ],
})

export default router
