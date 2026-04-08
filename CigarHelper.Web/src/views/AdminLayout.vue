<template>
  <div
    class="admin-layout-root relative isolate -mx-2 min-h-[calc(100vh-6rem)] sm:mx-0"
    data-testid="admin-layout">
    <div
      class="admin-layout-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto max-w-7xl px-3 pb-8 sm:px-6">
      <header class="border-b border-stone-200/90 pb-4 dark:border-stone-700/90">
        <p class="mb-1 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55">
          Администрирование
        </p>
        <h1 class="text-2xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 sm:text-3xl">
          Панель администратора
        </h1>
        <nav
          class="mt-4 flex flex-wrap gap-2"
          aria-label="Разделы админ-панели">
          <RouterLink
            v-for="link in navLinks"
            :key="link.name"
            data-testid="admin-nav-link"
            :data-admin-nav="link.name"
            :to="{ name: link.name }"
            class="inline-flex min-h-11 touch-manipulation items-center gap-2 rounded-xl px-4 py-2 text-sm font-medium no-underline transition-colors bg-white/90 text-stone-800 ring-1 ring-stone-200/90 hover:bg-stone-50 dark:bg-stone-900/80 dark:text-stone-100 dark:ring-stone-700/80 dark:hover:bg-stone-800"
            activeClass="!bg-rose-900 !text-white !shadow-sm !ring-0 dark:!bg-rose-800">
            <i
              :class="link.icon"
              aria-hidden="true" />
            {{ link.label }}
          </RouterLink>
        </nav>
      </header>

      <div class="pt-6">
        <RouterView />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { computed } from 'vue';
  import { RouterLink, RouterView } from 'vue-router';
  import { useAuth } from '@/services/useAuth';
  import { hasRole } from '@/utils/roles';

  const { user } = useAuth();

  const allLinks = [
    { name: 'AdminDashboard' as const, label: 'Обзор', icon: 'pi pi-home', adminOnly: true },
    { name: 'AdminUsers' as const, label: 'Пользователи', icon: 'pi pi-users', adminOnly: true },
    { name: 'AdminImages' as const, label: 'Изображения', icon: 'pi pi-images', adminOnly: true },
    { name: 'AdminCigarComments' as const, label: 'Комментарии', icon: 'pi pi-comments', adminOnly: false },
  ];

  const navLinks = computed(() => {
    const isAdmin = hasRole(user.value, 'Admin');
    return allLinks.filter((l) => !l.adminOnly || isAdmin);
  });
</script>

<style scoped>
  .admin-layout-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }
</style>
