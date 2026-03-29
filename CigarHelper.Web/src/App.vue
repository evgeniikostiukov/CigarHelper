<template>
  <div
    data-testid="app"
    class="app-container app-shell min-h-screen min-w-full flex flex-col bg-gradient-to-b from-stone-100 via-amber-50/30 to-stone-100 text-stone-800 dark:from-stone-950 dark:via-amber-950/15 dark:to-stone-950 dark:text-stone-200">
    <Toast />
    <ConfirmDialog />

    <header
      class="app-header sticky top-0 z-50 border-b border-stone-200/90 bg-white/85 shadow-sm shadow-stone-900/5 backdrop-blur-md dark:border-stone-700/80 dark:bg-stone-900/90 dark:shadow-black/40"
      data-testid="app-header">
      <Menubar
        data-testid="app-nav"
        :model="menuItemsVisible"
        class="app-menubar-bar mx-auto max-w-7xl border-0 bg-transparent px-2 sm:px-4">
        <template #start>
          <router-link
            :to="{ name: 'Home' }"
            data-testid="app-nav-brand"
            class="no-underline touch-manipulation rounded-lg py-1.5 pl-1 pr-2 transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-amber-700 dark:focus-visible:outline-amber-400">
            <span
              class="text-[0.65rem] font-semibold uppercase tracking-[0.18em] text-amber-900/70 dark:text-amber-200/55">
              Cigar Helper
            </span>
            <span class="sr-only"> — на главную</span>
          </router-link>
        </template>
        <template #end>
          <div class="flex items-center gap-2 sm:gap-3">
            <div
              v-if="isAuthenticated"
              class="flex items-center gap-1 sm:gap-2">
              <router-link
                :to="{ name: 'Profile' }"
                data-testid="app-nav-profile"
                class="inline-flex max-w-[min(100%,14rem)] min-h-11 items-center gap-2 rounded-xl px-2 py-1.5 font-medium text-stone-800 no-underline transition-colors hover:bg-stone-100 dark:text-stone-100 dark:hover:bg-stone-800 sm:max-w-[20rem]"
                :aria-label="`Профиль: ${user?.unique_name ?? ''}`"
                title="Перейти в профиль">
                <i
                  class="pi pi-user shrink-0 text-base text-amber-800 dark:text-amber-400/90"
                  aria-hidden="true" />
                <span class="truncate text-sm sm:text-base">{{ user?.unique_name }}</span>
              </router-link>
              <Button
                data-testid="app-nav-logout"
                class="min-h-11 min-w-11 touch-manipulation"
                icon="pi pi-sign-out"
                text
                rounded
                severity="secondary"
                aria-label="Выйти"
                @click="handleLogout" />
            </div>

            <template v-else>
              <Button
                data-testid="app-nav-login-wide"
                class="hidden min-h-11 touch-manipulation sm:inline-flex"
                label="Войти"
                icon="pi pi-sign-in"
                text
                @click="router.push({ name: 'Login' })" />
              <Button
                data-testid="app-nav-login-icon"
                class="min-h-11 min-w-11 touch-manipulation sm:hidden"
                icon="pi pi-sign-in"
                text
                rounded
                severity="secondary"
                aria-label="Войти"
                @click="router.push({ name: 'Login' })" />
            </template>
          </div>
        </template>
      </Menubar>
    </header>

    <main
      data-testid="app-main"
      class="app-main flex-1 w-full px-2 py-4 sm:px-6 sm:py-8">
      <router-view v-slot="{ Component }">
        <Suspense>
          <div data-testid="app-router-outlet">
            <component :is="Component" />
          </div>
          <template #fallback>
            <div
              data-testid="app-suspense-fallback"
              class="mx-auto flex min-h-[12rem] max-w-2xl flex-col items-center justify-center gap-4 rounded-2xl border border-stone-200/90 bg-white/90 px-6 py-10 text-center dark:border-stone-700/80 dark:bg-stone-900/80"
              aria-busy="true"
              aria-live="polite">
              <i
                class="pi pi-spinner animate-spin text-3xl text-amber-800/80 dark:text-amber-400/80"
                aria-hidden="true" />
              <p class="text-sm text-stone-600 dark:text-stone-400">Загрузка экрана…</p>
            </div>
          </template>
        </Suspense>
      </router-view>
    </main>
  </div>
</template>
<script setup lang="ts">
  import { computed } from 'vue';
  import { useRouter } from 'vue-router';
  import Toast from 'primevue/toast';
  import ConfirmDialog from 'primevue/confirmdialog';
  import Menubar from 'primevue/menubar';
  import Button from 'primevue/button';
  import { useAuth } from '@/services/useAuth';
  import { hasRole } from '@/utils/roles';

  interface MenuItem {
    label: string;
    icon: string;
    command: () => void;
    visible?: () => boolean;
  }

  const router = useRouter();
  const { isAuthenticated, user, logout } = useAuth();

  const menuItems = computed<MenuItem[]>(() => [
    {
      label: 'Хьюмидоры',
      icon: 'pi pi-box',
      command: () => router.push({ name: 'HumidorList' }),
      visible: () => isAuthenticated.value,
    },
    {
      label: 'Мои сигары',
      icon: 'pi pi-star',
      command: () => router.push({ name: 'CigarList' }),
      visible: () => isAuthenticated.value,
    },
    {
      label: 'База сигар',
      icon: 'pi pi-database',
      command: () => router.push({ name: 'CigarBases' }),
      visible: () => isAuthenticated.value && hasRole(user.value, 'Admin'),
    },
    {
      label: 'Бренды',
      icon: 'pi pi-tag',
      command: () => router.push({ name: 'Brands' }),
      visible: () => isAuthenticated.value && hasRole(user.value, 'Admin'),
    },
    {
      label: 'Пользователи',
      icon: 'pi pi-users',
      command: () => router.push({ name: 'AdminUsers' }),
      visible: () => isAuthenticated.value && hasRole(user.value, 'Admin'),
    },
    {
      label: 'Обзоры',
      icon: 'pi pi-list',
      command: () => router.push({ name: 'ReviewList' }),
    },
  ]);

  const menuItemsVisible = computed(() => menuItems.value.filter((item) => (item.visible ? item.visible() : true)));

  const handleLogout = (): void => {
    logout();
    router.push({ name: 'Login' });
  };
</script>
<style>
  .app-container {
    min-height: 100vh;
    min-width: 100vw;
    display: flex;
    flex-direction: column;
    box-sizing: border-box;
    padding-left: env(safe-area-inset-left, 0px);
    padding-right: env(safe-area-inset-right, 0px);
    padding-top: env(safe-area-inset-top, 0px);
    padding-bottom: env(safe-area-inset-bottom, 0px);
  }

  @media (max-width: 640px) {
    .app-container .app-main {
      padding-left: 0.5rem;
      padding-right: 0.5rem;
      padding-top: 1rem;
      padding-bottom: 1rem;
    }
  }

  :deep(.app-menubar-bar.p-menubar) {
    border-radius: 0;
    border: none;
    box-shadow: none;
    padding: 0.375rem 0;
  }

  :deep(.app-menubar-bar .p-menubar-root-list) {
    gap: 0.125rem;
  }

  :deep(.app-menubar-bar .p-menuitem-link) {
    padding: 0.5rem 0.75rem;
    min-height: 2.75rem;
    border-radius: 0.75rem;
    transition:
      background-color 0.2s ease,
      color 0.2s ease;
    font-size: 0.875rem;
    font-weight: 500;
    touch-action: manipulation;
  }

  :deep(.app-menubar-bar .p-menuitem-link:hover) {
    background-color: rgb(253 230 138 / 0.35);
  }

  :global(.dark) :deep(.app-menubar-bar .p-menuitem-link:hover) {
    background-color: rgb(39 39 42 / 0.85);
  }

  :deep(.app-menubar-bar .p-menuitem-link:focus-visible) {
    outline: 2px solid rgb(180 83 9);
    outline-offset: 2px;
  }

  :global(.dark) :deep(.app-menubar-bar .p-menuitem-link:focus-visible) {
    outline-color: rgb(251 191 36);
  }

  /* xs breakpoint для скрытия текста на кнопках на мобильных */
  @media (max-width: 480px) {
    .xs\:hidden {
      display: none !important;
    }
    .xs\:inline-flex {
      display: inline-flex !important;
    }
  }

  @media (prefers-reduced-motion: reduce) {
    :deep(.app-menubar-bar .p-menuitem-link) {
      transition: none;
    }
  }
</style>
