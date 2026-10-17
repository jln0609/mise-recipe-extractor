import { createRouter, createWebHistory } from 'vue-router'
import RecipeListView from '@/views/RecipeListView.vue'

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
  ],
})

export default router
