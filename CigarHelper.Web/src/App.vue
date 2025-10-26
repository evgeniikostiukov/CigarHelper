<template>
  <div
    class="app-container min-h-screen min-w-full flex flex-col bg-gray-50 dark:bg-gray-900 text-gray-800 dark:text-gray-200">
    <Toast />
    <ConfirmDialog />

    <Menubar
      :model="menuItems"
      class="mb-0 border-b border-gray-200 dark:border-gray-700 px-2 sm:px-6">
      <template #start>
        <router-link
          to="/"
          class="no-underline">
          <span class="text-lg sm:text-xl font-bold text-primary">Cigar Helper</span>
        </router-link>
      </template>
      <template #end>
        <div class="flex items-center gap-4">
          <div
            v-if="isAuthenticated"
            class="flex items-center gap-2">
            <span class="font-semibold hidden sm:inline">{{ user?.unique_name }}</span>
            <Button
              @click="handleLogout"
              icon="pi pi-sign-out"
              text
              rounded
              severity="secondary"
              aria-label="Выйти" />
          </div>

          <Button
            v-else
            @click="router.push('/login')"
            label="Войти"
            icon="pi pi-sign-in"
            text
            class="hidden sm:inline-flex" />
          <Button
            v-else
            @click="router.push('/login')"
            icon="pi pi-sign-in"
            text
            rounded
            severity="secondary"
            aria-label="Войти"
            class="sm:hidden" />
        </div>
      </template>
    </Menubar>

    <main class="flex-1 w-full px-2 sm:px-6 py-4 sm:py-8">
      <router-view v-slot="{ Component }">
        <Suspense>
          <div>
            <component :is="Component" />
          </div>
          <template #fallback>
            <div>Loading...</div>
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
      command: () => router.push('/humidors'),
      visible: () => isAuthenticated.value,
    },
    {
      label: 'Мои сигары',
      icon: 'pi pi-star',
      command: () => router.push('/cigars'),
      visible: () => isAuthenticated.value,
    },
    {
      label: 'База сигар',
      icon: 'pi pi-database',
      command: () => router.push('/cigar-bases'),
      visible: () => {
        const roles = Array.isArray(user.value?.role) ? user.value.role : [user.value?.role];
        return isAuthenticated.value && roles.includes('Admin');
      },
    },
    {
      label: 'Бренды',
      icon: 'pi pi-tag',
      command: () => router.push('/brands'),
      visible: () => {
        const roles = Array.isArray(user.value?.role) ? user.value.role : [user.value?.role];
        return isAuthenticated.value && roles.includes('Admin');
      },
    },
    {
      label: 'Обзоры',
      icon: 'pi pi-list',
      command: () => router.push('/reviews'),
    },
  ]);

  const handleLogout = (): void => {
    logout();
    router.push('/login');
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
    .app-container main {
      padding-left: 0.5rem;
      padding-right: 0.5rem;
      padding-top: 1rem;
      padding-bottom: 1rem;
    }
  }

  :deep(.p-menubar) {
    border-radius: 0;
    border: none;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.04);
    padding: 0.5rem 0;
  }
  :deep(.p-menubar-root-list) {
    gap: 0.25rem;
  }
  :deep(.p-menuitem-link) {
    padding: 0.5rem 0.75rem;
    border-radius: 6px;
    transition: background-color 0.2s;
    font-size: 0.875rem;
  }
  :deep(.p-menuitem-link:hover) {
    background-color: var(--surface-hover);
  }
  :deep(.p-button) {
    border-radius: 6px;
    font-weight: 500;
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
</style>
