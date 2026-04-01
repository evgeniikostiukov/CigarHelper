<template>
  <div
    data-testid="app"
    class="app-container app-shell min-h-screen min-w-full flex flex-col bg-gradient-to-b from-stone-50 via-rose-50/45 to-stone-50 text-stone-800 dark:from-stone-900 dark:via-rose-900/12 dark:to-stone-900 dark:text-stone-100">
    <Toast />
    <ConfirmDialog />
    <GlobalSearch ref="searchRef" />

    <!-- Офлайн-баннер -->
    <div
      v-if="!isOnline"
      data-testid="offline-banner"
      class="sticky top-0 z-[60] flex items-center justify-center gap-2 bg-amber-500 px-4 py-2 text-sm font-medium text-amber-950 dark:bg-amber-600 dark:text-amber-50"
      role="alert">
      <i class="pi pi-wifi-off text-base" aria-hidden="true" />
      Вы работаете офлайн
      <span v-if="pendingCount > 0" class="ml-1 rounded-full bg-amber-800/20 px-2 py-0.5 text-xs">
        {{ pendingCount }} {{ pendingCount === 1 ? 'действие' : 'действий' }} в очереди
      </span>
    </div>

    <!-- Обновление PWA -->
    <div
      v-if="needRefresh"
      data-testid="pwa-update-banner"
      class="sticky top-0 z-[60] flex items-center justify-center gap-3 bg-sky-600 px-4 py-2 text-sm font-medium text-white dark:bg-sky-700"
      role="alert">
      <i class="pi pi-refresh text-base" aria-hidden="true" />
      Доступна новая версия
      <Button
        size="small"
        severity="contrast"
        label="Обновить"
        class="!py-1 !text-xs"
        @click="applyUpdate" />
      <Button
        size="small"
        text
        severity="contrast"
        icon="pi pi-times"
        class="!py-1"
        aria-label="Закрыть"
        @click="dismissUpdate" />
    </div>

    <header
      class="app-header sticky top-0 z-50 border-b border-stone-200/90 bg-white/85 shadow-sm shadow-stone-900/5 backdrop-blur-md dark:border-stone-600/80 dark:bg-stone-800/90 dark:shadow-stone-950/40"
      data-testid="app-header">
      <Menubar
        data-testid="app-nav"
        :model="menuItemsVisible"
        class="app-menubar-bar mx-auto max-w-7xl border-0 bg-transparent px-2 sm:px-4">
        <template #start>
          <router-link
            :to="{ name: 'Home' }"
            data-testid="app-nav-brand"
            class="no-underline touch-manipulation rounded-lg py-1.5 pl-1 pr-2 transition-colors focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-rose-700 dark:focus-visible:outline-rose-400">
            <span
              class="text-[0.65rem] font-semibold uppercase tracking-[0.18em] text-rose-900/65 dark:text-rose-200/55">
              Cigar Helper
            </span>
            <span class="sr-only"> — на главную</span>
          </router-link>
        </template>
        <template #end>
          <div class="flex min-w-0 items-center gap-2 sm:gap-3">
            <Button
              v-if="isAuthenticated"
              data-testid="app-nav-search"
              class="hidden min-h-9 shrink-0 touch-manipulation whitespace-nowrap !rounded-lg sm:inline-flex"
              severity="secondary"
              text
              aria-label="Поиск (Ctrl+K)"
              title="Поиск (Ctrl+K)"
              @click="searchRef?.open()">
              <i
                class="pi pi-search text-sm"
                aria-hidden="true" />
              <span class="ml-1.5 text-sm text-stone-500 dark:text-stone-400">
                Поиск
                <kbd
                  class="ml-1 rounded border border-stone-200/80 bg-stone-100/80 px-1 py-px text-[0.6rem] font-mono text-stone-400 dark:border-stone-600/60 dark:bg-stone-800 dark:text-stone-500">
                  Ctrl K
                </kbd>
              </span>
            </Button>
            <Button
              v-if="isAuthenticated"
              data-testid="app-nav-search-icon"
              class="min-h-11 min-w-11 touch-manipulation sm:hidden"
              icon="pi pi-search"
              text
              rounded
              severity="secondary"
              aria-label="Поиск"
              @click="searchRef?.open()" />
            <Button
              v-if="canInstall"
              data-testid="pwa-install-btn"
              class="min-h-11 min-w-11 touch-manipulation"
              icon="pi pi-download"
              text
              rounded
              severity="secondary"
              aria-label="Установить приложение"
              title="Установить приложение"
              @click="install" />
            <div
              v-if="pendingCount > 0 && isOnline"
              data-testid="sync-badge"
              class="relative flex items-center"
              :title="`${pendingCount} действий ожидают синхронизации`">
              <i class="pi pi-sync animate-spin text-lg text-amber-600 dark:text-amber-400" />
              <span
                class="absolute -right-1.5 -top-1 flex h-4 min-w-4 items-center justify-center rounded-full bg-amber-600 px-1 text-[0.6rem] font-bold text-white">
                {{ pendingCount }}
              </span>
            </div>
            <ThemeToggle />
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
                  class="pi pi-user shrink-0 text-base text-rose-800 dark:text-rose-400/90"
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
              class="mx-auto flex min-h-[12rem] max-w-2xl flex-col items-center justify-center gap-4 rounded-2xl border border-stone-200/90 bg-white/90 px-6 py-10 text-center dark:border-stone-600/80 dark:bg-stone-800/80"
              aria-busy="true"
              aria-live="polite">
              <i
                class="pi pi-spinner animate-spin text-3xl text-rose-800/80 dark:text-rose-400/80"
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
  import { computed, onMounted, ref, watch } from 'vue';
  import { useRouter } from 'vue-router';
  import { useToast } from 'primevue/usetoast';
  import Toast from 'primevue/toast';
  import ConfirmDialog from 'primevue/confirmdialog';
  import Menubar from 'primevue/menubar';
  import Button from 'primevue/button';
  import { useAuth } from '@/services/useAuth';
  import { registerApiErrorNotifier } from '@/services/apiErrorNotifier';
  import { hasRole } from '@/utils/roles';
  import ThemeToggle from '@/components/ThemeToggle.vue';
  import GlobalSearch from '@/components/GlobalSearch.vue';
  import { useOnlineStatus } from '@/composables/useOnlineStatus';
  import { usePendingSync } from '@/composables/usePendingSync';
  import { useInstallPrompt } from '@/composables/useInstallPrompt';
  import { usePwaUpdate } from '@/composables/usePwaUpdate';

  interface MenuItem {
    label: string;
    icon: string;
    command: () => void;
    visible?: () => boolean;
  }

  const router = useRouter();
  const toast = useToast();
  const { isAuthenticated, user, logout } = useAuth();

  onMounted(() => {
    registerApiErrorNotifier(({ summary, detail, severity }) => {
      toast.add({
        summary,
        detail,
        severity: severity ?? 'error',
        life: 6000,
      });
    });
  });

  const menuItems = computed<MenuItem[]>(() => [
    {
      label: 'Сводка',
      icon: 'pi pi-chart-pie',
      command: () => router.push({ name: 'Dashboard' }),
      visible: () => isAuthenticated.value,
    },
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

  const searchRef = ref<InstanceType<typeof GlobalSearch> | null>(null);

  const { isOnline } = useOnlineStatus((online) => {
    toast.add({
      summary: online ? 'Соединение восстановлено' : 'Вы офлайн',
      detail: online
        ? 'Ожидающие действия будут синхронизированы.'
        : 'Изменения сохранятся и отправятся при восстановлении сети.',
      severity: online ? 'success' : 'warn',
      life: 4000,
    });
  });

  const { pendingCount, lastError } = usePendingSync();
  const { canInstall, install } = useInstallPrompt();
  const { needRefresh, applyUpdate, dismiss: dismissUpdate } = usePwaUpdate();

  watch(needRefresh, (v) => {
    if (v) {
      toast.add({
        summary: 'Доступно обновление',
        detail: 'Новая версия приложения готова к установке.',
        severity: 'info',
        life: 0,
        closable: true,
      });
    }
  });

  watch(lastError, (err) => {
    if (err) {
      toast.add({ summary: 'Ошибка синхронизации', detail: err, severity: 'error', life: 6000 });
    }
  });

  const handleLogout = (): void => {
    logout();
    router.push({ name: 'Login' });
  };
</script>
