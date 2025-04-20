import { createRouter, createWebHistory } from 'vue-router'

// Lazy-load views
const Home = () => import('../views/Home.vue')
const Login = () => import('../views/Login.vue')
const HumidorList = () => import('../views/HumidorList.vue')
const HumidorDetail = () => import('../views/HumidorDetail.vue')
const HumidorForm = () => import('../views/HumidorForm.vue')
const CigarList = () => import('../views/CigarList.vue')
const ReviewList = () => import('../views/ReviewList.vue')
const ReviewDetail = () => import('../views/ReviewDetail.vue')
const ReviewForm = () => import('../views/ReviewForm.vue')

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      name: 'home',
      component: Home
    },
    {
      path: '/login',
      name: 'login',
      component: Login
    },
    {
      path: '/humidors',
      name: 'humidors',
      component: HumidorList,
      meta: { requiresAuth: true }
    },
    {
      path: '/humidors/new',
      name: 'humidorNew',
      component: HumidorForm,
      meta: { requiresAuth: true }
    },
    {
      path: '/humidors/:id',
      name: 'humidorDetail',
      component: HumidorDetail,
      meta: { requiresAuth: true }
    },
    {
      path: '/humidors/:id/edit',
      name: 'humidorEdit',
      component: HumidorForm,
      meta: { requiresAuth: true, isEdit: true }
    },
    {
      path: '/cigars',
      name: 'cigars',
      component: CigarList,
      meta: { requiresAuth: true }
    },
    // Маршруты для обзоров
    {
      path: '/reviews',
      name: 'reviews',
      component: ReviewList
    },
    {
      path: '/reviews/create',
      name: 'reviewCreate',
      component: ReviewForm,
      meta: { requiresAuth: true }
    },
    {
      path: '/reviews/:id',
      name: 'reviewDetail',
      component: ReviewDetail
    },
    {
      path: '/reviews/:id/edit',
      name: 'reviewEdit',
      component: ReviewForm,
      meta: { requiresAuth: true, isEdit: true }
    }
  ]
})

// Navigation guard to check for authentication
router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token')
  
  if (to.matched.some(record => record.meta.requiresAuth)) {
    if (!token) {
      next({ name: 'login', query: { redirect: to.fullPath } })
    } else {
      next()
    }
  } else {
    next()
  }
})

export default router 