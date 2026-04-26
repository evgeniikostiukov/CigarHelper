<template>
  <section
    class="admin-deleted-reviews-root"
    data-testid="admin-deleted-reviews-page"
    aria-labelledby="admin-deleted-reviews-heading">
    <div class="mb-6">
      <h2
        id="admin-deleted-reviews-heading"
        class="text-balance text-2xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 sm:text-3xl">
        Удалённые обзоры
      </h2>
      <p class="mt-1.5 max-w-2xl text-pretty text-sm text-stone-600 dark:text-stone-400">
        Обзоры, скрытые авторами (мягкое удаление). После восстановления снова видны в списках и по прямой ссылке.
      </p>
    </div>

    <div
      v-if="loading && !hasEverLoaded"
      class="min-h-[12rem] space-y-3"
      data-testid="admin-deleted-reviews-loading"
      aria-busy="true">
      <Skeleton
        v-for="n in 4"
        :key="n"
        class="h-24 rounded-2xl border border-stone-200/80 dark:border-stone-700/80" />
    </div>

    <div
      v-else-if="loadError && !hasEverLoaded"
      class="max-w-2xl rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80"
      role="alert">
      <Message severity="error">{{ loadError }}</Message>
      <Button
        class="mt-4 min-h-12 touch-manipulation"
        label="Повторить"
        icon="pi pi-refresh"
        severity="secondary"
        outlined
        @click="load" />
    </div>

    <div
      v-else
      class="rounded-2xl border border-stone-200/90 bg-white/95 p-4 shadow-md dark:border-stone-700/90 dark:bg-stone-900/85 sm:p-6">
      <div
        v-if="!loading && hasEverLoaded && items.length === 0"
        class="rounded-xl border border-dashed border-rose-800/25 px-5 py-12 text-center dark:border-rose-200/15"
        data-testid="admin-deleted-reviews-empty">
        <p class="font-semibold text-stone-900 dark:text-rose-50/95">Нет удалённых обзоров</p>
        <p class="mt-2 text-sm text-stone-600 dark:text-stone-400">Список пуст или всё уже восстановлено.</p>
      </div>

      <DataTable
        v-else
        :value="items"
        data-key="id"
        responsive-layout="scroll"
        class="text-sm"
        data-testid="admin-deleted-reviews-table">
        <Column
          field="deletedAt"
          header="Удалён"
          sortable>
          <template #body="{ data }">
            {{ formatWhen(data.deletedAt) }}
          </template>
        </Column>
        <Column
          field="createdAt"
          header="Создан"
          sortable>
          <template #body="{ data }">
            {{ formatWhen(data.createdAt) }}
          </template>
        </Column>
        <Column
          field="username"
          header="Автор"
          sortable>
          <template #body="{ data }">
            <PublicProfileAuthorBlock
              :username="data.username"
              :is-author-profile-public="data.isAuthorProfilePublic === true"
              :show-avatar="false"
              name-class="text-sm" />
          </template>
        </Column>
        <Column
          field="title"
          header="Заголовок"
          sortable>
          <template #body="{ data }">
            <span class="font-medium text-stone-800 dark:text-stone-200">{{ data.title }}</span>
          </template>
        </Column>
        <Column header="Сигара">
          <template #body="{ data }">
            <span class="text-stone-600 dark:text-stone-400">{{ data.cigarBrand }} · {{ data.cigarName }}</span>
          </template>
        </Column>
        <Column
          header=""
          style="width: 1%">
          <template #body="{ data }">
            <Button
              type="button"
              label="Восстановить"
              icon="pi pi-replay"
              severity="success"
              outlined
              size="small"
              class="min-h-10 touch-manipulation"
              :loading="actingId === data.id"
              :aria-label="`Восстановить обзор ${data.id}`"
              data-testid="admin-deleted-review-restore"
              @click="confirmRestore(data)" />
          </template>
        </Column>
      </DataTable>

      <div
        v-if="totalCount > pageSize"
        class="mt-4 flex justify-center">
        <Paginator
          :first="(page - 1) * pageSize"
          :rows="pageSize"
          :total-records="totalCount"
          template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink"
          @page="onPage" />
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import PublicProfileAuthorBlock from '@/components/PublicProfileAuthorBlock.vue';
  import Button from 'primevue/button';
  import Column from 'primevue/column';
  import DataTable from 'primevue/datatable';
  import Message from 'primevue/message';
  import Paginator from 'primevue/paginator';
  import Skeleton from 'primevue/skeleton';
  import { useToast } from 'primevue/usetoast';
  import { useConfirm } from 'primevue/useconfirm';
  import type { PageState } from 'primevue/paginator';
  import * as adminReviews from '@/services/adminDeletedReviewsService';
  import type { AdminDeletedReviewRow } from '@/services/adminDeletedReviewsService';

  const toast = useToast();
  const confirm = useConfirm();

  const loading = ref(true);
  const hasEverLoaded = ref(false);
  const loadError = ref<string | null>(null);
  const items = ref<AdminDeletedReviewRow[]>([]);
  const totalCount = ref(0);
  const page = ref(1);
  const pageSize = ref(20);
  const actingId = ref<number | null>(null);

  function formatWhen(iso: string | null | undefined): string {
    if (!iso) return '—';
    try {
      return new Date(iso).toLocaleString('ru-RU', { dateStyle: 'short', timeStyle: 'short' });
    } catch {
      return iso;
    }
  }

  async function load(): Promise<void> {
    loading.value = true;
    loadError.value = null;
    try {
      const res = await adminReviews.fetchDeletedReviews(page.value, pageSize.value);
      items.value = res.items ?? [];
      totalCount.value = res.totalCount ?? 0;
      hasEverLoaded.value = true;
    } catch {
      loadError.value = 'Не удалось загрузить список.';
      if (!hasEverLoaded.value) {
        items.value = [];
      }
    } finally {
      loading.value = false;
    }
  }

  function onPage(e: PageState): void {
    page.value = e.page != null ? e.page + 1 : 1;
    pageSize.value = e.rows;
    void load();
  }

  function confirmRestore(row: AdminDeletedReviewRow): void {
    confirm.require({
      message: `Восстановить обзор «${row.title}» (#${row.id})? Он снова станет виден всем.`,
      header: 'Восстановление обзора',
      icon: 'pi pi-replay',
      rejectClass: 'p-button-secondary p-button-outlined',
      acceptClass: 'p-button-success',
      rejectLabel: 'Отмена',
      acceptLabel: 'Восстановить',
      accept: () => {
        void doRestore(row.id);
      },
    });
  }

  async function doRestore(id: number): Promise<void> {
    actingId.value = id;
    try {
      await adminReviews.restoreReview(id);
      toast.add({ severity: 'success', summary: 'Восстановлено', life: 2500 });
      await load();
      if (items.value.length === 0 && page.value > 1) {
        page.value -= 1;
        await load();
      }
    } catch {
      toast.add({ severity: 'error', summary: 'Не удалось восстановить', life: 4000 });
    } finally {
      actingId.value = null;
    }
  }

  onMounted(() => {
    void load();
  });
</script>
