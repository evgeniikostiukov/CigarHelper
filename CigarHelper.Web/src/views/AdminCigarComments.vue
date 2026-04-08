<template>
  <section
    class="admin-cigar-comments-root"
    data-testid="admin-cigar-comments-page"
    aria-labelledby="admin-cigar-comments-heading">
    <div class="mb-6">
      <h2
        id="admin-cigar-comments-heading"
        class="text-balance text-2xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 sm:text-3xl">
        Комментарии
      </h2>
      <p class="mt-1.5 max-w-2xl text-pretty text-sm text-stone-600 dark:text-stone-400">
        Очередь на проверку: комментарии к справочнику и к публичным коллекциям. После одобрения они видны всем.
      </p>
    </div>

    <div
      v-if="loading && !hasEverLoaded"
      class="min-h-[12rem] space-y-3"
      data-testid="admin-cigar-comments-loading"
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
        data-testid="admin-cigar-comments-empty">
        <p class="font-semibold text-stone-900 dark:text-rose-50/95">Очередь пуста</p>
        <p class="mt-2 text-sm text-stone-600 dark:text-stone-400">Нет комментариев, ожидающих модерации.</p>
      </div>

      <DataTable
        v-else
        :value="items"
        data-key="id"
        responsive-layout="scroll"
        class="text-sm"
        data-testid="admin-cigar-comments-table">
        <Column
          field="createdAt"
          header="Дата"
          sortable>
          <template #body="{ data }">
            {{ formatWhen(data.createdAt) }}
          </template>
        </Column>
        <Column
          field="authorUsername"
          header="Автор"
          sortable />
        <Column header="Контекст">
          <template #body="{ data }">
            <span class="text-stone-600 dark:text-stone-400">{{ data.targetSummary }}</span>
          </template>
        </Column>
        <Column header="Текст">
          <template #body="{ data }">
            <p class="max-w-md whitespace-pre-wrap text-pretty text-stone-800 dark:text-stone-200">
              {{ data.body }}
            </p>
          </template>
        </Column>
        <Column
          header=""
          style="width: 1%">
          <template #body="{ data }">
            <div class="flex flex-wrap justify-end gap-1">
              <Button
                type="button"
                icon="pi pi-check"
                rounded
                severity="success"
                text
                class="min-h-10 min-w-10"
                :loading="actingId === data.id"
                :aria-label="`Одобрить комментарий ${data.id}`"
                data-testid="admin-cigar-comment-approve"
                @click="approve(data.id)" />
              <Button
                type="button"
                icon="pi pi-times"
                rounded
                severity="danger"
                text
                class="min-h-10 min-w-10"
                :loading="actingId === data.id"
                :aria-label="`Отклонить комментарий ${data.id}`"
                data-testid="admin-cigar-comment-reject"
                @click="reject(data.id)" />
            </div>
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
  import Button from 'primevue/button';
  import Column from 'primevue/column';
  import DataTable from 'primevue/datatable';
  import Message from 'primevue/message';
  import Paginator from 'primevue/paginator';
  import Skeleton from 'primevue/skeleton';
  import { useToast } from 'primevue/usetoast';
  import type { PageState } from 'primevue/paginator';
  import * as adminComments from '@/services/adminCigarCommentsService';

  const toast = useToast();

  const loading = ref(true);
  const hasEverLoaded = ref(false);
  const loadError = ref<string | null>(null);
  const items = ref<adminComments.AdminCigarCommentRow[]>([]);
  const totalCount = ref(0);
  const page = ref(1);
  const pageSize = ref(20);
  const actingId = ref<number | null>(null);

  function formatWhen(iso: string): string {
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
      const res = await adminComments.fetchPendingComments(page.value, pageSize.value);
      items.value = res.items ?? [];
      totalCount.value = res.totalCount ?? 0;
      hasEverLoaded.value = true;
    } catch {
      loadError.value = 'Не удалось загрузить очередь.';
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

  async function approve(id: number): Promise<void> {
    actingId.value = id;
    try {
      await adminComments.approveCigarComment(id);
      toast.add({ severity: 'success', summary: 'Одобрено', life: 2000 });
      await load();
    } finally {
      actingId.value = null;
    }
  }

  async function reject(id: number): Promise<void> {
    actingId.value = id;
    try {
      await adminComments.rejectCigarComment(id);
      toast.add({ severity: 'info', summary: 'Отклонено', life: 2000 });
      await load();
    } finally {
      actingId.value = null;
    }
  }

  onMounted(() => {
    void load();
  });
</script>
