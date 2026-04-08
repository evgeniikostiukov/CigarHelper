import { createRouter, createWebHistory } from 'vue-router';
import { useAuth } from '@/services/useAuth';
import { hasAnyRole, hasRole } from '@/utils/roles';

const routes = [
  {
    path: '/',
    name: 'Home',
    component: () => import('../views/Home.vue'),
  },
  {
    path: '/onboarding',
    name: 'Onboarding',
    component: () => import('../views/Onboarding.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/dashboard',
    name: 'Dashboard',
    component: () => import('../views/Dashboard.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/login',
    name: 'Login',
    component: () => import('../views/Login.vue'),
    meta: { public: true },
  },
  {
    path: '/profile',
    name: 'Profile',
    component: () => import('../views/Profile.vue'),
    meta: { requiresAuth: true },
  },
  {
    path: '/u/:username',
    name: 'PublicUserProfile',
    component: () => import('../views/PublicUserProfile.vue'),
  },
  {
    path: '/u/:username/humidors/:humidorId',
    name: 'PublicHumidorDetail',
    component: () => import('../views/PublicHumidorDetail.vue'),
    props: true,
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
    component: () => import('../views/CigarCollectionEdit.vue'),
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
  {
    path: '/admin',
    component: () => import('../views/AdminLayout.vue'),
    meta: { requiresAuth: true, requiresAdmin: true },
    children: [
      {
        path: '',
        name: 'AdminDashboard',
        component: () => import('../views/AdminDashboard.vue'),
      },
      {
        path: 'users',
        name: 'AdminUsers',
        component: () => import('../views/AdminUsers.vue'),
      },
      {
        path: 'images',
        name: 'AdminImages',
        component: () => import('../views/AdminImages.vue'),
      },
    ],
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
router.beforeEach((to) => {
  const { isAuthenticated, user } = useAuth();
  const requiresAuth = to.matched.some((record) => record.meta.requiresAuth);
  const requiresAdmin = to.matched.some((record) => record.meta.requiresAdmin);
  const requiresAnyRole = to.matched
    .map((record) => record.meta.requiresAnyRole as string[] | undefined)
    .find((r) => r !== undefined);
  const isPublic = to.matched.some((record) => record.meta.public);
  const needsOnboarding = localStorage.getItem('needsOnboarding') === '1';

  if (!isAuthenticated.value && requiresAuth) {
    // Пользователь не аутентифицирован и пытается получить доступ к защищенному маршруту
    return {
      path: '/login',
      query: { redirect: to.fullPath }, // Сохраняем исходный путь для редиректа после входа
    };
  } else if (isAuthenticated.value && needsOnboarding && to.name !== 'Onboarding' && !isPublic) {
    return { name: 'Onboarding' };
  } else if (isAuthenticated.value && isPublic) {
    // Аутентифицированный пользователь пытается получить доступ к публичной странице (например, /login)
    return '/'; // Перенаправляем на главную
  } else if (isAuthenticated.value && requiresAdmin && !hasRole(user.value, 'Admin')) {
    return '/';
  } else if (isAuthenticated.value && requiresAnyRole && !hasAnyRole(user.value, requiresAnyRole)) {
    return '/';
  }
  // Во всех остальных случаях разрешаем переход
  return true;
});

export default router;
