<template>
  <section
    class="cigar-detail-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-50 via-rose-50/40 to-stone-50 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-rose-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="cigar-detail"
    aria-labelledby="cigar-detail-heading">
    <div
      class="cigar-detail-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] max-w-7xl mx-auto">
      <div
        v-if="loading"
        data-testid="cigar-detail-loading"
        class="space-y-6 min-h-[20rem]"
        aria-busy="true"
        aria-live="polite">
        <Skeleton
          class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80 max-w-md"
          height="3rem" />
        <div class="grid grid-cols-1 lg:grid-cols-3 gap-5 sm:gap-6">
          <Skeleton
            class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
            height="20rem" />
          <div class="lg:col-span-2 space-y-5">
            <Skeleton
              class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
              height="12rem" />
            <Skeleton
              class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
              height="8rem" />
          </div>
        </div>
      </div>

      <div
        v-else-if="error"
        class="rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80 max-w-2xl"
        data-testid="cigar-detail-error"
        role="alert">
        <Message severity="error">{{ error }}</Message>
        <Button
          data-testid="cigar-detail-retry"
          class="mt-4 min-h-12 w-full sm:w-auto touch-manipulation"
          label="Повторить загрузку"
          icon="pi pi-refresh"
          severity="secondary"
          outlined
          @click="loadCigar(route.params.id as string)" />
      </div>

      <template v-else-if="cigar">
        <header class="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between pb-6 sm:pb-8">
          <div class="min-w-0">
            <p
              class="text-[0.65rem] uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55 font-semibold mb-1.5">
              Коллекция
            </p>
            <h1
              id="cigar-detail-heading"
              class="text-3xl sm:text-4xl font-semibold text-stone-900 dark:text-rose-50/95 tracking-tight text-balance">
              {{ cigar.name }}
            </h1>
            <p class="mt-1.5 text-sm text-stone-600 dark:text-stone-400 max-w-xl text-pretty">
              {{ cigar.brand.name }} · карточка сигары и хранение в одном месте.
            </p>
          </div>
          <div class="flex flex-col gap-2 w-full sm:w-auto sm:flex-row sm:flex-wrap sm:justify-end shrink-0">
            <Button
              data-testid="cigar-detail-back"
              class="w-full sm:w-auto min-h-12 sm:min-h-11 px-5 touch-manipulation"
              label="К списку"
              icon="pi pi-arrow-left"
              severity="secondary"
              outlined
              @click="router.push({ name: 'CigarList' })" />
            <Button
              data-testid="cigar-detail-edit"
              class="w-full sm:w-auto min-h-12 sm:min-h-11 px-5 touch-manipulation shadow-md shadow-rose-900/10 dark:shadow-black/40"
              label="Редактировать"
              icon="pi pi-pencil"
              @click="editCigar" />
            <Button
              data-testid="cigar-detail-delete"
              class="w-full sm:w-auto min-h-12 sm:min-h-11 px-5 touch-manipulation"
              label="Удалить"
              icon="pi pi-trash"
              severity="danger"
              outlined
              @click="confirmDelete" />
          </div>
        </header>

        <div
          class="grid grid-cols-1 lg:grid-cols-3 gap-5 sm:gap-6 cigar-detail-enter"
          data-testid="cigar-detail-content">
          <div class="lg:col-span-1">
            <div
              class="rounded-2xl border border-stone-200/90 bg-white/95 shadow-md shadow-stone-900/5 overflow-hidden dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50">
              <div
                v-if="primaryImageSrc"
                class="cigar-detail-image-frame relative aspect-square w-full min-h-0 overflow-hidden bg-stone-100 dark:bg-stone-800/80"
                data-testid="cigar-detail-image">
                <div
                  class="cigar-detail-image-frame-inner absolute inset-0 box-border flex min-h-0 min-w-0 items-center justify-center p-3 sm:p-4">
                  <img
                    :src="primaryImageSrc"
                    :alt="cigar.name"
                    loading="lazy"
                    decoding="async" />
                </div>
              </div>
              <div
                v-else
                class="aspect-square bg-stone-100 dark:bg-stone-800/80 flex items-center justify-center"
                data-testid="cigar-detail-no-image"
                aria-hidden="true">
                <span
                  class="flex h-20 w-20 items-center justify-center rounded-2xl bg-rose-100/90 text-rose-900 dark:bg-rose-900/40 dark:text-rose-100">
                  <i class="pi pi-image text-4xl" />
                </span>
              </div>
            </div>
          </div>

          <div class="lg:col-span-2 space-y-5 sm:space-y-6 min-w-0">
            <section
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 sm:p-6 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50"
              aria-labelledby="cigar-detail-info-heading">
              <h2
                id="cigar-detail-info-heading"
                class="flex items-center gap-2 text-lg font-semibold text-stone-900 dark:text-rose-50/95 mb-4">
                <i
                  class="pi pi-info-circle text-rose-800/80 dark:text-rose-400/90"
                  aria-hidden="true" />
                Основная информация
              </h2>
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4 sm:gap-5">
                <div>
                  <span class="block text-xs font-medium uppercase tracking-wide text-stone-500 dark:text-stone-400">
                    Название
                  </span>
                  <p class="mt-1 text-stone-900 dark:text-stone-100 font-medium">
                    {{ cigar.name }}
                  </p>
                </div>
                <div>
                  <span class="block text-xs font-medium uppercase tracking-wide text-stone-500 dark:text-stone-400">
                    Бренд
                  </span>
                  <p class="mt-1 text-stone-900 dark:text-stone-100 font-medium">
                    {{ cigar.brand.name }}
                  </p>
                </div>
                <div>
                  <span class="block text-xs font-medium uppercase tracking-wide text-stone-500 dark:text-stone-400">
                    Страна
                  </span>
                  <p class="mt-1 text-stone-800 dark:text-stone-200">
                    {{ cigar.country || '—' }}
                  </p>
                </div>
                <div>
                  <span class="block text-xs font-medium uppercase tracking-wide text-stone-500 dark:text-stone-400">
                    Размер
                  </span>
                  <p class="mt-1 text-stone-800 dark:text-stone-200">
                    {{ cigar.size || '—' }}
                  </p>
                </div>
                <div>
                  <span class="block text-xs font-medium uppercase tracking-wide text-stone-500 dark:text-stone-400">
                    Крепость
                  </span>
                  <p class="mt-1 text-stone-800 dark:text-stone-200">
                    {{ getStrengthLabel(cigar.strength) || '—' }}
                  </p>
                </div>
                <div>
                  <span class="block text-xs font-medium uppercase tracking-wide text-stone-500 dark:text-stone-400">
                    Цена
                  </span>
                  <p class="mt-1 text-stone-800 dark:text-stone-200">
                    {{ cigar.price != null ? `${cigar.price} ₽` : '—' }}
                  </p>
                </div>
                <div>
                  <span class="block text-xs font-medium uppercase tracking-wide text-stone-500 dark:text-stone-400">
                    Остаток
                  </span>
                  <p
                    class="mt-1 text-stone-800 dark:text-stone-200"
                    data-testid="cigar-detail-quantity">
                    <template v-if="cigar.isSmoked">0&nbsp;шт. (выкурена)</template>
                    <template v-else>{{ detailRemainingStock }}&nbsp;шт.</template>
                  </p>
                </div>
              </div>
            </section>

            <section
              v-if="cigar.rating != null"
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 sm:p-6 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50"
              aria-labelledby="cigar-detail-rating-heading">
              <h2
                id="cigar-detail-rating-heading"
                class="flex items-center gap-2 text-lg font-semibold text-stone-900 dark:text-rose-50/95 mb-4">
                <i
                  class="pi pi-star text-rose-700 dark:text-rose-400"
                  aria-hidden="true" />
                Оценка
              </h2>
              <div class="flex flex-wrap items-center gap-4">
                <Rating
                  :model-value="cigar.rating"
                  :readonly="true"
                  :cancel="false"
                  :stars="10" />
                <span class="text-lg font-medium text-stone-900 dark:text-stone-100"> {{ cigar.rating }}/10 </span>
              </div>
            </section>

            <section
              v-if="cigar.wrapper || cigar.binder || cigar.filler"
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 sm:p-6 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50"
              aria-labelledby="cigar-detail-blend-heading">
              <h2
                id="cigar-detail-blend-heading"
                class="flex items-center gap-2 text-lg font-semibold text-stone-900 dark:text-rose-50/95 mb-4">
                <i
                  class="pi pi-layer-group text-rose-800/80 dark:text-rose-400/90"
                  aria-hidden="true" />
                Структура сигары
              </h2>
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4 sm:gap-5">
                <div>
                  <span class="block text-xs font-medium uppercase tracking-wide text-stone-500 dark:text-stone-400">
                    Покровный лист
                  </span>
                  <p class="mt-1 text-stone-800 dark:text-stone-200">
                    {{ cigar.wrapper || '—' }}
                  </p>
                </div>
                <div>
                  <span class="block text-xs font-medium uppercase tracking-wide text-stone-500 dark:text-stone-400">
                    Связующий лист
                  </span>
                  <p class="mt-1 text-stone-800 dark:text-stone-200">
                    {{ cigar.binder || '—' }}
                  </p>
                </div>
                <div>
                  <span class="block text-xs font-medium uppercase tracking-wide text-stone-500 dark:text-stone-400">
                    Наполнитель
                  </span>
                  <p class="mt-1 text-stone-800 dark:text-stone-200">
                    {{ cigar.filler || '—' }}
                  </p>
                </div>
              </div>
            </section>

            <section
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 sm:p-6 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50"
              data-testid="cigar-detail-storage"
              aria-labelledby="cigar-detail-storage-heading">
              <h2
                id="cigar-detail-storage-heading"
                class="flex items-center gap-2 text-lg font-semibold text-stone-900 dark:text-rose-50/95 mb-4">
                <i
                  class="pi pi-box text-rose-800/80 dark:text-rose-400/90"
                  aria-hidden="true" />
                Хранение
              </h2>

              <div
                v-if="cigar.humidorId && humidor"
                class="space-y-4">
                <div class="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                  <h3 class="text-base font-semibold text-stone-900 dark:text-rose-50/95">
                    {{ humidor.name }}
                  </h3>
                  <Button
                    data-testid="cigar-detail-open-humidor"
                    class="w-full sm:w-auto min-h-12 sm:min-h-11 touch-manipulation shrink-0"
                    label="Открыть хьюмидор"
                    icon="pi pi-external-link"
                    severity="secondary"
                    outlined
                    @click="goToHumidor(humidor.id!)" />
                </div>
                <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 sm:gap-4">
                  <div
                    class="rounded-xl border border-stone-200/80 bg-stone-50/90 px-4 py-3 dark:border-stone-700/80 dark:bg-stone-950/50">
                    <div class="text-xs font-medium uppercase tracking-wide text-stone-500 dark:text-stone-400">
                      Вместимость
                    </div>
                    <div class="mt-1 text-lg font-semibold text-stone-900 dark:text-stone-100">
                      {{ humidor.currentCount ?? 0 }} / {{ humidor.capacity }} сигар
                    </div>
                  </div>
                  <div
                    v-if="humidor.humidity != null"
                    class="rounded-xl border border-stone-200/80 bg-stone-50/90 px-4 py-3 dark:border-stone-700/80 dark:bg-stone-950/50">
                    <div class="text-xs font-medium uppercase tracking-wide text-stone-500 dark:text-stone-400">
                      Влажность
                    </div>
                    <div class="mt-1 inline-flex items-center gap-2">
                      <Badge
                        :value="humidor.humidity"
                        :severity="humidorService.getHumiditySeverity(humidor.humidity)" />
                    </div>
                  </div>
                </div>
                <p
                  v-if="humidor.description"
                  class="text-sm leading-relaxed text-stone-600 dark:text-stone-400 border-t border-stone-100 dark:border-stone-700/80 pt-4 text-pretty">
                  {{ humidor.description }}
                </p>
              </div>

              <div
                v-else-if="cigar.humidorId && !humidor"
                class="rounded-xl border border-rose-800/20 bg-rose-50/50 px-4 py-3 dark:border-rose-200/15 dark:bg-rose-950/20"
                role="status">
                <p class="text-sm text-stone-700 dark:text-stone-300 mb-3 text-pretty">
                  Данные хьюмидора не загрузились, но сигара привязана к нему.
                </p>
                <Button
                  data-testid="cigar-detail-humidor-fallback"
                  class="min-h-12 w-full sm:w-auto touch-manipulation"
                  label="Перейти по ссылке"
                  icon="pi pi-external-link"
                  severity="secondary"
                  outlined
                  @click="goToHumidor(cigar.humidorId!)" />
              </div>

              <div
                v-else
                class="text-center rounded-xl border border-dashed border-rose-800/25 bg-white/60 px-4 py-8 dark:border-rose-200/15 dark:bg-stone-900/40"
                data-testid="cigar-detail-no-humidor">
                <span
                  class="mx-auto mb-3 flex h-14 w-14 items-center justify-center rounded-2xl bg-rose-100/90 text-rose-900 dark:bg-rose-900/40 dark:text-rose-100"
                  aria-hidden="true">
                  <i class="pi pi-box text-2xl" />
                </span>
                <p class="text-stone-600 dark:text-stone-400 mb-4 text-pretty max-w-md mx-auto text-sm sm:text-base">
                  Сигара пока не привязана к хьюмидору. Укажите хранение при редактировании.
                </p>
                <Button
                  data-testid="cigar-detail-add-humidor"
                  class="min-h-12 px-6 touch-manipulation"
                  label="Редактировать сигару"
                  icon="pi pi-pencil"
                  @click="editCigar" />
              </div>
            </section>

            <section
              v-if="cigar.description"
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 sm:p-6 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50"
              aria-labelledby="cigar-detail-desc-heading">
              <h2
                id="cigar-detail-desc-heading"
                class="flex items-center gap-2 text-lg font-semibold text-stone-900 dark:text-rose-50/95 mb-4">
                <i
                  class="pi pi-file-edit text-rose-800/80 dark:text-rose-400/90"
                  aria-hidden="true" />
                Описание
              </h2>
              <p
                class="text-sm sm:text-base leading-relaxed text-stone-700 dark:text-stone-300 whitespace-pre-wrap text-pretty">
                {{ cigar.description }}
              </p>
            </section>
          </div>
        </div>
      </template>
    </div>

    <ConfirmDialog />
  </section>
</template>

<script setup lang="ts">
  import { computed, ref, onMounted } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { useConfirm } from 'primevue/useconfirm';
  import { useToast } from 'primevue/usetoast';
  import { isOfflineQueued } from '@/services/api';
  import cigarService from '../services/cigarService';
  import humidorService from '../services/humidorService';
  import type { Cigar } from '../services/cigarService';
  import type { Humidor } from '../services/humidorService';
  import { strengthOptions } from '../utils/cigarOptions';

  const route = useRoute();
  const router = useRouter();
  const confirm = useConfirm();
  const toast = useToast();

  const cigar = ref<Cigar | null>(null);
  const humidor = ref<Humidor | null>(null);
  const loading = ref(true);
  const error = ref<string | null>(null);

  const primaryImageSrc = computed(() => {
    const c = cigar.value;
    if (!c?.images?.length) return '';
    const first = c.images[0];
    const raw = first.imageData ?? first.data;
    if (raw == null) return '';
    if (typeof raw === 'string') {
      return raw.startsWith('data:') ? raw : `data:image/jpeg;base64,${raw}`;
    }
    return '';
  });

  const detailRemainingStock = computed(() => {
    const c = cigar.value;
    if (!c || c.isSmoked) {
      return 0;
    }
    const q = c.quantity;
    if (q != null && q >= 0) {
      return q;
    }
    return 1;
  });

  function getStrengthLabel(strength: string | null | undefined): string {
    if (!strength) return '';
    const opt = strengthOptions.find((o) => o.value === strength);
    return opt?.label ?? strength;
  }

  async function loadCigar(id: string): Promise<void> {
    loading.value = true;
    error.value = null;
    humidor.value = null;
    try {
      cigar.value = await cigarService.getCigar(id);
      if (cigar.value?.humidorId) {
        await loadHumidor(cigar.value.humidorId);
      }
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка при загрузке сигары:', err);
      }
      error.value = 'Не удалось загрузить данные о сигаре.';
      cigar.value = null;
    } finally {
      loading.value = false;
    }
  }

  async function loadHumidor(id: number): Promise<void> {
    try {
      humidor.value = await humidorService.getHumidor(id);
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Не удалось загрузить данные о хьюмидоре:', err);
      }
      humidor.value = null;
    }
  }

  function editCigar(): void {
    if (!cigar.value?.id) return;
    router.push({ name: 'CigarEdit', params: { id: String(cigar.value.id) } });
  }

  function confirmDelete(): void {
    if (!cigar.value) return;
    confirm.require({
      message: `Вы уверены, что хотите удалить сигару «${cigar.value.name}»? Это действие нельзя отменить.`,
      header: 'Подтверждение удаления',
      icon: 'pi pi-exclamation-triangle',
      rejectClass: 'p-button-secondary p-button-outlined',
      acceptClass: 'p-button-danger',
      rejectLabel: 'Отмена',
      acceptLabel: 'Удалить',
      accept: () => {
        void deleteCigar();
      },
    });
  }

  async function deleteCigar(): Promise<void> {
    if (!cigar.value?.id) return;
    try {
      await cigarService.deleteCigar(cigar.value.id);
      toast.add({
        severity: 'success',
        summary: 'Успешно',
        detail: `Сигара «${cigar.value.name}» удалена`,
        life: 3000,
      });
      router.push({ name: 'CigarList' });
    } catch (err) {
      if (isOfflineQueued(err)) return;
      if (import.meta.env.DEV) {
        console.error('Ошибка при удалении сигары:', err);
      }
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось удалить сигару',
        life: 3000,
      });
    }
  }

  function goToHumidor(humidorId: number): void {
    router.push({ name: 'HumidorDetail', params: { id: String(humidorId) } });
  }

  onMounted(() => {
    void loadCigar(route.params.id as string);
  });
</script>

<style scoped>
  .cigar-detail-root {
    position: relative;
    isolation: isolate;
  }

  .cigar-detail-image-frame {
    width: 100%;
    max-width: 100%;
  }

  .cigar-detail-image-frame img {
    display: block;
    width: auto;
    height: auto;
    min-width: 0;
    min-height: 0;
    max-width: 100%;
    max-height: 100%;
    object-fit: contain;
    object-position: center;
  }

  .cigar-detail-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  /*:global(.dark) .cigar-detail-grain {
    mix-blend-mode: soft-light;
  }*/

  .cigar-detail-enter {
    animation: cigar-detail-in 0.45s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes cigar-detail-in {
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
    .cigar-detail-enter {
      animation: none;
    }
  }
</style>
