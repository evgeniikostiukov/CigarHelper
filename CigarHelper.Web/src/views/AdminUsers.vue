<template>
  <div class="p-4 max-w-7xl mx-auto">
    <div class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Пользователи</h1>
      <p class="text-gray-600 dark:text-gray-400 mt-2">Назначение ролей (только для администраторов)</p>
    </div>

    <Card class="shadow-sm">
      <template #content>
        <div class="mb-4 flex flex-col sm:flex-row gap-3">
          <span class="p-input-icon-left flex-1">
            <i class="pi pi-search" />
            <InputText
              v-model="search"
              placeholder="Поиск по имени или email..."
              class="w-full"
              @keyup.enter="applySearch" />
          </span>
          <Button
            label="Найти"
            icon="pi pi-search"
            @click="applySearch" />
        </div>

        <DataTable
          :value="rows"
          :loading="loading"
          stripedRows
          showGridlines
          responsiveLayout="scroll"
          class="p-datatable-sm">
          <Column
            field="username"
            header="Имя" />
          <Column
            field="email"
            header="Email" />
          <Column
            header="Роль">
            <template #body="{ data }">
              <Dropdown
                v-model="data.draftRole"
                :options="roleOptions"
                optionLabel="label"
                optionValue="value"
                class="min-w-[200px]" />
            </template>
          </Column>
          <Column
            header="Действия"
            style="width: 140px">
            <template #body="{ data }">
              <Button
                label="Сохранить"
                icon="pi pi-check"
                size="small"
                :disabled="!rowDirty(data) || saving"
                :loading="saving && savingId === data.id"
                @click="saveRow(data)" />
            </template>
          </Column>
        </DataTable>

        <Paginator
          class="mt-4"
          :rows="pageSize"
          :totalRecords="totalCount"
          :first="first"
          :rowsPerPageOptions="[10, 20, 50]"
          @page="onPageChange" />
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import axios from 'axios';
  import { useToast } from 'primevue/usetoast';
  import { useConfirm } from 'primevue/useconfirm';
  import Card from 'primevue/card';
  import Button from 'primevue/button';
  import DataTable from 'primevue/datatable';
  import Column from 'primevue/column';
  import Dropdown from 'primevue/dropdown';
  import InputText from 'primevue/inputtext';
  import Paginator from 'primevue/paginator';
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

  async function load(): Promise<void> {
    loading.value = true;
    try {
      const res = await adminApi.fetchAdminUsers(
        pageIndex.value + 1,
        pageSize.value,
        searchApplied.value || undefined
      );
      totalCount.value = res.totalCount;
      rows.value = res.items.map((r) => ({
        ...r,
        draftRole: r.role,
      }));
    } catch {
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось загрузить пользователей',
        life: 4000,
      });
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
            await router.push('/');
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
        message:
          'Вы меняете свою роль. После сохранения вы получите новый токен с обновлёнными правами. Продолжить?',
        header: 'Подтверждение',
        icon: 'pi pi-exclamation-triangle',
        acceptLabel: 'Да',
        rejectLabel: 'Отмена',
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
