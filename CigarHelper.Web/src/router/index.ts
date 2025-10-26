import { createRouter, createWebHistory } from 'vue-router';
import Home from '../views/Home.vue';
import { useAuth } from '@/services/useAuth';

const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home,
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/Login.vue'),
    meta: { public: true },
  },
  {
    path: '/humidors',
    name: 'HumidorList',
    component: () => import('../views/HumidorList.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/humidors/new',
    name: 'HumidorForm',
    component: () => import('../views/HumidorForm.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/humidors/:id',
    name: 'HumidorDetail',
    component: () => import('../views/HumidorDetail.vue'),
    props: true,
    meta: { requiresAuth: true },
  },
  {
    path: '/humidors/:id/edit',
    name: 'HumidorEdit',
    component: () => import('../views/HumidorForm.vue'),
    meta: { requiresAuth: true, isEdit: true },
  },
  {
    path: '/cigars',
    name: 'CigarList',
    component: () => import('../views/CigarList.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/cigars/new',
    name: 'CigarNew',
    component: () => import('../views/CigarForm.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/cigars/:id',
    name: 'CigarDetail',
    component: () => import('../views/CigarDetail.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/cigars/:id/edit',
    name: 'CigarEdit',
    component: () => import('../views/CigarForm.vue'),
    meta: { requiresAuth: true, isEdit: true },
  },
  {
    path: '/cigar-bases',
    name: 'CigarBases',
    component: () => import('../views/CigarBases.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/brands',
    name: 'Brands',
    component: () => import('../views/Brands.vue'),
    meta: { requiresAuth: true, requiresAdmin: true },
  },
  // Маршруты для обзоров
  {
    path: '/reviews',
    name: 'ReviewList',
    component: () => import('../views/ReviewList.vue'),
  },
  {
    path: '/reviews/create',
    name: 'ReviewCreate',
    component: () => import('../views/ReviewForm.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/reviews/:id',
    name: 'ReviewDetail',
    component: () => import('../views/ReviewDetail.vue'),
  },
  {
    path: '/reviews/:id/edit',
    name: 'ReviewEdit',
    component: () => import('../views/ReviewForm.vue'),
    meta: { requiresAuth: true, isEdit: true },
  },
];

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
});

// Navigation guard to check for authentication
router.beforeEach((to, from, next) => {
  const { isAuthenticated, user } = useAuth();
  const requiresAuth = to.matched.some((record) => record.meta.requiresAuth);
  const requiresAdmin = to.matched.some((record) => record.meta.requiresAdmin);
  const isPublic = to.matched.some((record) => record.meta.public);

  if (!isAuthenticated.value && requiresAuth) {
    // Пользователь не аутентифицирован и пытается получить доступ к защищенному маршруту
    next({
      path: '/login',
      query: { redirect: to.fullPath }, // Сохраняем исходный путь для редиректа после входа
    });
  } else if (isAuthenticated.value && isPublic) {
    // Аутентифицированный пользователь пытается получить доступ к публичной странице (например, /login)
    next('/'); // Перенаправляем на главную
  } else if (isAuthenticated.value && requiresAdmin) {
    // Пользователь аутентифицирован, но маршрут требует прав администратора
    const roles = Array.isArray(user.value?.role) ? user.value.role : [user.value?.role];
    if (roles.includes('Admin')) {
      next(); // У пользователя есть права
    } else {
      next('/'); // У пользователя нет прав, перенаправляем на главную
    }
  } else {
    // Во всех остальных случаях разрешаем переход
    next();
  }
});

export default router;
