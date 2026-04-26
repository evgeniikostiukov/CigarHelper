<template>
  <section
    class="admin-images-root"
    data-testid="admin-images-page"
    aria-labelledby="admin-images-heading">
    <div class="mb-6">
      <h2
        id="admin-images-heading"
        class="text-balance text-2xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 sm:text-3xl">
        Изображения
      </h2>
      <p class="mt-1.5 max-w-2xl text-pretty text-sm text-stone-600 dark:text-stone-400">
        Файлы из внешнего хранилища (каталог сигар и личные коллекции). Логотипы брендов и фото в обзорах здесь не
        показываются — у них отдельное хранение.
      </p>
    </div>

    <div
      v-if="loading && !hasEverLoaded"
      class="min-h-[16rem] space-y-4"
      data-testid="admin-images-loading"
      aria-busy="true">
      <Skeleton
        v-for="n in 8"
        :key="n"
        class="h-40 rounded-2xl border border-stone-200/80 dark:border-stone-700/80" />
    </div>

    <div
      v-else-if="loadError && !hasEverLoaded"
      class="max-w-2xl rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80"
      data-testid="admin-images-error"
      role="alert">
      <Message severity="error">{{ loadError }}</Message>
      <Button
        data-testid="admin-images-retry"
        class="mt-4 min-h-12 touch-manipulation"
        label="Повторить"
        icon="pi pi-refresh"
        severity="secondary"
        outlined
        @click="load" />
    </div>

    <div
      v-else
      class="admin-images-panel rounded-2xl border border-stone-200/90 bg-white/95 p-4 shadow-md dark:border-stone-700/90 dark:bg-stone-900/85 sm:p-6"
      data-testid="admin-images-content">
      <div
        v-if="!loading && hasEverLoaded && totalCount === 0"
        class="rounded-xl border border-dashed border-rose-800/25 px-5 py-12 text-center dark:border-rose-200/15"
        data-testid="admin-images-empty">
        <i
          class="pi pi-images mb-3 text-4xl text-stone-400"
          aria-hidden="true" />
        <p class="font-semibold text-stone-900 dark:text-rose-50/95">Нет загруженных изображений</p>
      </div>

      <template v-else>
        <ul
          class="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4"
          data-testid="admin-images-grid">
          <li
            v-for="img in items"
            :key="img.id"
            class="flex flex-col overflow-hidden rounded-xl border border-stone-200/80 bg-white dark:border-stone-700/80 dark:bg-stone-900/60">
            <div class="relative aspect-[4/3] bg-stone-100 dark:bg-stone-800/80">
              <img
                v-if="thumbById[img.id]"
                :src="thumbById[img.id]"
                :alt="thumbAlt(img)"
                class="h-full w-full object-contain"
                loading="lazy"
                @error="onThumbError(img.id)" />
              <div
                v-else-if="thumbFailed.has(img.id)"
                class="flex h-full flex-col items-center justify-center gap-1 text-stone-400"
                role="img"
                :aria-label="'Нет превью'">
                <i
                  class="pi pi-image text-3xl"
                  aria-hidden="true" />
                <span class="px-2 text-center text-[0.65rem]">Нет превью</span>
              </div>
              <div
                v-else
                class="flex h-full items-center justify-center text-stone-400">
                <i
                  class="pi pi-spinner animate-spin text-2xl"
                  aria-hidden="true" />
              </div>
              <span
                v-if="img.isMain"
                class="absolute left-2 top-2 rounded-md bg-rose-900/90 px-1.5 py-0.5 text-[0.65rem] font-semibold uppercase tracking-wide text-white">
                Главное
              </span>
            </div>
            <div class="min-w-0 flex-1 space-y-1 p-3 text-xs text-stone-600 dark:text-stone-400">
              <p class="truncate font-mono text-[0.7rem] text-stone-500 dark:text-stone-500">#{{ img.id }}</p>
              <p
                class="truncate text-stone-800 dark:text-stone-200"
                :title="img.fileName ?? ''">
                {{ img.fileName || '—' }}
              </p>
              <p>
                <span v-if="img.cigarBaseId">База #{{ img.cigarBaseId }}</span>
                <span v-if="img.userCigarId">Коллекция #{{ img.userCigarId }}</span>
                <span v-if="!img.cigarBaseId && !img.userCigarId">Без привязки</span>
              </p>
              <p class="text-[0.65rem] text-stone-500">{{ formatSize(img) }} · {{ formatDate(img.createdAt) }}</p>
              <Button
                class="mt-2 min-h-10 w-full touch-manipulation text-xs"
                label="Оригинал"
                icon="pi pi-external-link"
                severity="secondary"
                outlined
                @click="openOriginal(img.id)" />
            </div>
          </li>
        </ul>

        <Paginator
          v-if="totalCount > 0"
          data-testid="admin-images-paginator"
          class="mt-6"
          :rows="pageSize"
          :total-records="totalCount"
          :first="first"
          :rows-per-page-options="[12, 24, 48]"
          @page="onPageChange" />
      </template>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
  import Button from 'primevue/button';
  import Message from 'primevue/message';
  import Paginator from 'primevue/paginator';
  import Skeleton from 'primevue/skeleton';
  import { useToast } from 'primevue/usetoast';
  import api from '@/services/api';
  import * as adminImagesApi from '@/services/adminImagesService';
  import type { CigarImageListItem } from '@/services/adminImagesService';

  const toast = useToast();

  const loading = ref(false);
  const loadError = ref<string | null>(null);
  const hasEverLoaded = ref(false);
  const items = ref<CigarImageListItem[]>([]);
  const totalCount = ref(0);
  const pageIndex = ref(0);
  const pageSize = ref(24);

  const thumbById = ref<Record<number, string>>({});
  const thumbFailed = ref<Set<number>>(new Set());
  let thumbLoadGen = 0;

  const first = computed(() => pageIndex.value * pageSize.value);

  function revokeThumbs(rec: Record<number, string>): void {
    for (const url of Object.values(rec)) {
      if (url.startsWith('blob:')) {
        URL.revokeObjectURL(url);
      }
    }
  }

  function thumbAlt(img: CigarImageListItem): string {
    return img.fileName ? `Превью: ${img.fileName}` : `Изображение ${img.id}`;
  }

  function formatSize(img: CigarImageListItem): string {
    if (img.fileSize == null) return 'размер ?';
    if (img.fileSize < 1024) return `${img.fileSize} B`;
    if (img.fileSize < 1024 * 1024) return `${(img.fileSize / 1024).toFixed(1)} KB`;
    return `${(img.fileSize / (1024 * 1024)).toFixed(1)} MB`;
  }

  function formatDate(iso: string): string {
    try {
      const d = new Date(iso);
      return new Intl.DateTimeFormat('ru-RU', { dateStyle: 'short', timeStyle: 'short' }).format(d);
    } catch {
      return iso;
    }
  }

  async function load(): Promise<void> {
    loading.value = true;
    loadError.value = null;
    try {
      const res = await adminImagesApi.fetchAdminCigarImages(pageIndex.value + 1, pageSize.value);
      items.value = res.items;
      totalCount.value = res.totalCount;
      hasEverLoaded.value = true;
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('admin images load', err);
      }
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось загрузить список изображений',
        life: 5000,
      });
      if (!hasEverLoaded.value) {
        loadError.value = 'Не удалось загрузить данные.';
      }
    } finally {
      loading.value = false;
    }
  }

  function onPageChange(event: { page: number; rows: number }): void {
    pageIndex.value = event.page;
    pageSize.value = event.rows;
    void load();
  }

  function onThumbError(id: number): void {
    const url = thumbById.value[id];
    if (url?.startsWith('blob:')) {
      URL.revokeObjectURL(url);
    }
    const next = { ...thumbById.value };
    delete next[id];
    thumbById.value = next;
  }

  /** Открыть оригинал в новой вкладке (запрос с Bearer через fetch + blob URL). */
  async function openOriginal(id: number): Promise<void> {
    try {
      const { data } = await api.get<Blob>(`cigarimages/${id}/data`, {
        responseType: 'blob',
        skipGlobalErrorNotification: true,
      });
      const blobUrl = URL.createObjectURL(data);
      const w = window.open(blobUrl, '_blank', 'noopener,noreferrer');
      if (!w) {
        toast.add({
          severity: 'warn',
          summary: 'Браузер',
          detail: 'Разрешите всплывающие окна или скачайте файл из сети вручную',
          life: 5000,
        });
      }
      window.setTimeout(() => URL.revokeObjectURL(blobUrl), 60_000);
    } catch {
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось открыть файл',
        life: 4000,
      });
    }
  }

  watch(
    items,
    async () => {
      const gen = ++thumbLoadGen;
      revokeThumbs(thumbById.value);
      thumbById.value = {};
      thumbFailed.value = new Set();
      const list = items.value;
      await Promise.all(
        list.map(async (img) => {
          try {
            const { data } = await api.get<Blob>(`cigarimages/${img.id}/thumbnail`, {
              responseType: 'blob',
              skipGlobalErrorNotification: true,
            });
            const objectUrl = URL.createObjectURL(data);
            if (gen !== thumbLoadGen) {
              URL.revokeObjectURL(objectUrl);
              return;
            }
            thumbById.value = { ...thumbById.value, [img.id]: objectUrl };
          } catch {
            if (import.meta.env.DEV) {
              console.warn('Нет превью для', img.id);
            }
            if (gen === thumbLoadGen) {
              thumbFailed.value = new Set(thumbFailed.value).add(img.id);
            }
          }
        }),
      );
    },
    { deep: true },
  );

  onMounted(() => {
    void load();
  });

  onUnmounted(() => {
    thumbLoadGen += 1;
    revokeThumbs(thumbById.value);
    thumbById.value = {};
  });
</script>

<style scoped>
  .admin-images-panel {
    animation: admin-images-in 0.4s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }
  @keyframes admin-images-in {
    from {
      opacity: 0;
      transform: translateY(6px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }
  @media (prefers-reduced-motion: reduce) {
    .admin-images-panel {
      animation: none;
    }
  }
</style>
