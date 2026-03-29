<template>
  <section
    class="admin-users-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-100 via-amber-50/40 to-stone-100 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-amber-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="admin-users-page"
    aria-labelledby="admin-users-heading">
    <div
      class="admin-users-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto max-w-7xl">
      <header class="pb-6 sm:pb-8">
        <p
          class="mb-1.5 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-amber-900/65 dark:text-amber-200/55">
          Администрирование
        </p>
        <h1
          id="admin-users-heading"
          class="text-balance text-3xl font-semibold tracking-tight text-stone-900 dark:text-amber-50/95 sm:text-4xl">
          Пользователи
        </h1>
        <p class="mt-1.5 max-w-xl text-pretty text-sm text-stone-600 dark:text-stone-400">
          Поиск, пагинация и смена ролей. Сохранение своей роли обновляет JWT — подтверждение обязательно.
        </p>
      </header>

      <div
        v-if="loading && !hasEverLoaded"
        class="min-h-[20rem] space-y-4"
        data-testid="admin-users-loading"
        aria-busy="true"
        aria-live="polite">
        <Skeleton
          class="h-24 rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          data-testid="admin-users-skeleton-search" />
        <Skeleton
          v-for="n in 6"
          :key="n"
          class="rounded-xl border border-stone-200/80 dark:border-stone-700/80"
          height="3rem"
          data-testid="admin-users-skeleton-row" />
      </div>

      <div
        v-else-if="loadError && !hasEverLoaded"
        class="max-w-2xl rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80"
        data-testid="admin-users-error"
        role="alert">
        <Message severity="error">{{ loadError }}</Message>
        <Button
          data-testid="admin-users-retry"
          class="mt-4 min-h-12 w-full touch-manipulation sm:w-auto"
          label="Повторить загрузку"
          icon="pi pi-refresh"
          severity="secondary"
          outlined
          @click="load" />
      </div>

      <div
        v-else
        class="admin-users-panel-enter rounded-2xl border border-stone-200/90 bg-white/95 p-4 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6"
        data-testid="admin-users-content">
        <div class="mb-6 flex flex-col gap-3 sm:flex-row sm:items-end">
          <div class="min-w-0 flex-1">
            <label
              for="admin-users-search"
              class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
              Поиск
            </label>
            <span class="p-input-icon-left flex w-full [&_input]:min-h-11 [&_input]:w-full">
              <i class="pi pi-search text-stone-400" />
              <InputText
                id="admin-users-search"
                v-model="search"
                data-testid="admin-users-search"
                class="w-full"
                placeholder="Имя или email…"
                @keyup.enter="applySearch" />
            </span>
          </div>
          <Button
            data-testid="admin-users-search-submit"
            class="min-h-12 w-full shrink-0 touch-manipulation sm:w-auto sm:min-h-11"
            label="Найти"
            icon="pi pi-search"
            @click="applySearch" />
        </div>

        <div
          v-if="!loading && hasEverLoaded && totalCount === 0"
          class="rounded-xl border border-dashed border-amber-800/25 bg-white/60 px-5 py-12 text-center dark:border-amber-200/15 dark:bg-stone-900/40"
          data-testid="admin-users-empty">
          <span
            class="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-amber-100/90 text-amber-900 dark:bg-amber-900/40 dark:text-amber-100"
            aria-hidden="true">
            <i class="pi pi-users text-2xl" />
          </span>
          <p class="mb-2 text-lg font-semibold text-stone-900 dark:text-amber-50/95">
            {{ searchApplied.trim() ? 'Ничего не найдено' : 'Пользователей нет' }}
          </p>
          <p class="mb-6 text-pretty text-sm text-stone-600 dark:text-stone-400">
            {{
              searchApplied.trim()
                ? 'Измените запрос или сбросьте поиск.'
                : 'Список пуст — новые пользователи появятся после регистрации.'
            }}
          </p>
          <Button
            v-if="searchApplied.trim()"
            data-testid="admin-users-clear-search"
            class="min-h-12 touch-manipulation"
            label="Сбросить поиск"
            icon="pi pi-times"
            severity="secondary"
            outlined
            @click="clearSearch" />
        </div>

        <template v-else>
          <DataTable
            :value="rows"
            data-testid="admin-users-table"
            :loading="loading"
            striped-rows
            show-grid-lines
            responsive-layout="scroll"
            class="admin-users-datatable text-sm">
            <Column
              field="username"
              header="Имя" />
            <Column
              field="email"
              header="Email" />
            <Column header="Роль">
              <template #body="{ data }">
                <Dropdown
                  v-model="data.draftRole"
                  :options="roleOptions"
                  class="w-full min-w-[min(100%,12rem)]"
                  input-class="min-h-11"
                  option-label="label"
                  option-value="value"
                  :data-testid="`admin-users-role-${data.id}`" />
              </template>
            </Column>
            <Column
              header="Действия"
              style="min-width: 10rem">
              <template #body="{ data }">
                <Button
                  :data-testid="`admin-users-save-${data.id}`"
                  class="min-h-11 touch-manipulation"
                  label="Сохранить"
                  icon="pi pi-check"
                  :disabled="!rowDirty(data) || saving"
                  :loading="saving && savingId === data.id"
                  @click="saveRow(data)" />
              </template>
            </Column>
          </DataTable>

          <Paginator
            v-if="totalCount > 0"
            data-testid="admin-users-paginator"
            class="mt-4"
            :rows="pageSize"
            :total-records="totalCount"
            :first="first"
            :rows-per-page-options="[10, 20, 50]"
            @page="onPageChange" />
        </template>
      </div>
    </div>

    <ConfirmDialog />
  </section>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import axios from 'axios';
  import { useToast } from 'primevue/usetoast';
  import { useConfirm } from 'primevue/useconfirm';
  import Button from 'primevue/button';
  import DataTable from 'primevue/datatable';
  import Column from 'primevue/column';
  import Dropdown from 'primevue/dropdown';
  import InputText from 'primevue/inputtext';
  import Paginator from 'primevue/paginator';
  import ConfirmDialog from 'primevue/confirmdialog';
  import { useAuth } from '@/services/useAuth';
  import authService from '@/services/authService';
  import { getAuthUserId, hasRole } from '@/utils/roles';
  import * as adminApi from '@/services/adminUsersService';
  import type { AdminRole, AdminUserListItem } from '@/services/adminUsersService';

  interface Row extends AdminUserListItem {
    draftRole: AdminRole;
  }

  const router = useRouter();
  const toast = useToast();
  const confirm = useConfirm();
  const { user } = useAuth();

  const loading = ref(false);
  const loadError = ref<string | null>(null);
  const hasEverLoaded = ref(false);
  const saving = ref(false);
  const savingId = ref<number | null>(null);
  const rows = ref<Row[]>([]);
  const totalCount = ref(0);
  const pageIndex = ref(0);
  const pageSize = ref(20);
  const search = ref('');
  const searchApplied = ref('');

  const roleOptions = [
    { label: 'Пользователь', value: 'User' as const },
    { label: 'Модератор', value: 'Moderator' as const },
    { label: 'Администратор', value: 'Admin' as const },
  ];

  const first = computed(() => pageIndex.value * pageSize.value);

  const currentUserId = computed(() => getAuthUserId(user.value));

  function rowDirty(row: Row): boolean {
    return row.draftRole !== row.role;
  }

  function clearSearch(): void {
    search.value = '';
    searchApplied.value = '';
    pageIndex.value = 0;
    void load();
  }

  async function load(): Promise<void> {
    loading.value = true;
    loadError.value = null;
    try {
      const res = await adminApi.fetchAdminUsers(
        pageIndex.value + 1,
        pageSize.value,
        searchApplied.value.trim() || undefined,
      );
      totalCount.value = res.totalCount;
      rows.value = res.items.map((r) => ({
        ...r,
        draftRole: r.role,
      }));
      hasEverLoaded.value = true;
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка загрузки пользователей:', err);
      }
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось загрузить пользователей',
        life: 4000,
      });
      if (!hasEverLoaded.value) {
        loadError.value = 'Не удалось загрузить список пользователей. Попробуйте позже.';
      }
    } finally {
      loading.value = false;
    }
  }

  function applySearch(): void {
    searchApplied.value = search.value;
    pageIndex.value = 0;
    void load();
  }

  function onPageChange(event: { page: number; rows: number }): void {
    pageIndex.value = event.page;
    pageSize.value = event.rows;
    void load();
  }

  function saveRow(row: Row): void {
    if (!rowDirty(row)) return;

    const isSelf = currentUserId.value !== null && row.id === currentUserId.value;

    const submit = (): void => {
      void (async () => {
        saving.value = true;
        savingId.value = row.id;
        try {
          const result = await adminApi.updateUserRole(row.id, row.draftRole, isSelf);
          if (result.newToken) {
            authService.setToken(result.newToken);
          }
          row.role = row.draftRole;
          toast.add({ severity: 'success', summary: 'Сохранено', detail: result.message, life: 3000 });
          if (isSelf && !hasRole(user.value, 'Admin')) {
            await router.push({ name: 'Home' });
          } else {
            await load();
          }
        } catch (err: unknown) {
          let detail = 'Не удалось сохранить';
          if (axios.isAxiosError(err)) {
            const data = err.response?.data as { message?: string } | undefined;
            if (data?.message) detail = data.message;
          }
          toast.add({ severity: 'error', summary: 'Ошибка', detail, life: 5000 });
        } finally {
          saving.value = false;
          savingId.value = null;
        }
      })();
    };

    if (isSelf) {
      confirm.require({
        message: 'Вы меняете свою роль. После сохранения вы получите новый токен с обновлёнными правами. Продолжить?',
        header: 'Подтверждение',
        icon: 'pi pi-exclamation-triangle',
        rejectClass: 'p-button-secondary p-button-outlined',
        acceptClass: 'p-button-primary',
        rejectLabel: 'Отмена',
        acceptLabel: 'Продолжить',
        accept: submit,
      });
    } else {
      submit();
    }
  }

  onMounted(() => {
    void load();
  });
</script>

<style scoped>
  .admin-users-root {
    position: relative;
    isolation: isolate;
  }

  .admin-users-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  :global(.dark) .admin-users-grain {
    mix-blend-mode: soft-light;
  }

  .admin-users-panel-enter {
    animation: admin-users-panel-in 0.45s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes admin-users-panel-in {
    from {
      opacity: 0;
      transform: translateY(8px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  @media (prefers-reduced-motion: reduce) {
    .admin-users-panel-enter {
      animation: none;
    }
  }
</style>
