<template>
  <section
    class="cigar-list-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-50 via-rose-50/40 to-stone-50 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-rose-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="cigar-list"
    aria-labelledby="cigar-list-heading">
    <div
      class="cigar-list-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] max-w-7xl mx-auto">
      <header class="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between pb-6 sm:pb-8">
        <div class="min-w-0">
          <p
            class="text-[0.65rem] uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55 font-semibold mb-1.5">
            Коллекция
          </p>
          <h1
            id="cigar-list-heading"
            class="text-3xl sm:text-4xl font-semibold text-stone-900 dark:text-rose-50/95 tracking-tight text-balance">
            Мои сигары
          </h1>
          <p class="mt-1.5 text-sm text-stone-600 dark:text-stone-400 max-w-xl text-pretty">
            Каталог с фото и оценками.
          </p>
        </div>
        <Button
          data-testid="cigar-list-add"
          class="w-full sm:w-auto shrink-0 min-h-12 px-5 sm:min-h-11 touch-manipulation shadow-md shadow-rose-900/10 dark:shadow-black/40"
          @click="$router.push({ name: 'CigarNew' })"
          icon="pi pi-plus"
          label="Добавить сигару" />
      </header>

      <div
        class="mb-8 rounded-2xl border border-stone-200/90 bg-white/95 p-6 shadow-md shadow-stone-900/5 sm:mb-10 sm:p-8 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50"
        data-testid="cigar-list-filters">
        <div class="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between sm:gap-6">
          <div class="flex min-w-0 flex-1 flex-col gap-4 sm:flex-row sm:items-start sm:gap-6">
            <span
              class="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-rose-100/90 text-rose-900 dark:bg-rose-900/40 dark:text-rose-100 sm:h-14 sm:w-14"
              aria-hidden="true">
              <i class="pi pi-filter-slash text-2xl" />
            </span>
            <div class="min-w-0 flex-1">
              <h2
                id="cigar-list-filters-heading"
                class="text-lg font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 sm:text-xl">
                Фильтры
              </h2>
              <p
                class="mt-1.5 max-w-2xl text-pretty text-sm leading-relaxed text-stone-700 dark:text-stone-300 sm:text-base">
                Сузьте список по названию, бренду, формату и статусу в коллекции.
              </p>
            </div>
          </div>
          <div class="flex shrink-0 flex-col gap-2 sm:items-end">
            <Badge
              v-if="!filtersExpanded && filtersActive"
              value="Фильтры активны"
              severity="warn"
              class="w-fit"
              data-testid="cigar-list-filters-collapsed-hint" />
            <Button
              type="button"
              data-testid="cigar-list-filters-toggle"
              :aria-expanded="filtersExpanded"
              aria-controls="cigar-list-filters-panel"
              :label="filtersExpanded ? 'Свернуть фильтры' : 'Развернуть фильтры'"
              :icon="filtersExpanded ? 'pi pi-chevron-up' : 'pi pi-chevron-down'"
              icon-pos="right"
              severity="secondary"
              outlined
              class="min-h-12 w-full touch-manipulation sm:min-h-11 sm:w-auto"
              :aria-label="filtersExpanded ? 'Свернуть блок фильтров' : 'Развернуть блок фильтров'"
              @click="filtersExpanded = !filtersExpanded" />
          </div>
        </div>

        <Transition name="cl-list-filters-panel">
          <div
            v-show="filtersExpanded"
            id="cigar-list-filters-panel"
            class="mt-6 flex flex-col gap-6 border-t border-stone-100 pt-6 dark:border-stone-700/80 sm:mt-8 sm:gap-8 sm:pt-8"
            role="region"
            aria-labelledby="cigar-list-filters-heading">
            <form
              role="search"
              class="grid grid-cols-1 gap-5 sm:gap-6 lg:grid-cols-12 lg:items-end"
              @submit.prevent>
              <div class="min-w-0 lg:col-span-4">
                <label
                  for="cigar-list-search"
                  class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
                  Поиск
                </label>
                <IconField class="w-full">
                  <InputIcon
                    class="pi pi-search text-stone-400"
                    aria-hidden="true" />
                  <InputText
                    id="cigar-list-search"
                    v-model="filters.search"
                    placeholder="Название или бренд..."
                    class="w-full min-h-12 sm:min-h-11"
                    data-testid="cigar-list-search"
                    autocomplete="off" />
                </IconField>
              </div>
              <div class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:col-span-8 lg:grid-cols-4 lg:gap-6">
                <div class="min-w-0">
                  <label
                    for="cigar-list-filter-brand"
                    class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
                    Бренд
                  </label>
                  <Select
                    v-model="filters.brandId"
                    data-testid="cigar-list-filter-brand"
                    :options="brandOptions"
                    option-label="label"
                    option-value="value"
                    placeholder="Все бренды"
                    class="w-full min-h-12 sm:min-h-11"
                    label-id="cigar-list-filter-brand"
                    :show-clear="true" />
                </div>
                <div class="min-w-0">
                  <label
                    for="cigar-list-filter-strength"
                    class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
                    Крепость
                  </label>
                  <Select
                    v-model="filters.strength"
                    data-testid="cigar-list-filter-strength"
                    :options="strengthOptionsFromData"
                    option-label="label"
                    option-value="value"
                    placeholder="Все"
                    class="w-full min-h-12 sm:min-h-11"
                    label-id="cigar-list-filter-strength"
                    :show-clear="true" />
                </div>
                <div class="min-w-0">
                  <label
                    for="cigar-list-filter-size"
                    class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
                    Формат
                  </label>
                  <Select
                    v-model="filters.size"
                    data-testid="cigar-list-filter-size"
                    :options="sizeOptionsFromData"
                    option-label="label"
                    option-value="value"
                    placeholder="Все"
                    class="w-full min-h-12 sm:min-h-11"
                    label-id="cigar-list-filter-size"
                    :show-clear="true" />
                </div>
                <div class="min-w-0">
                  <label
                    for="cigar-list-filter-humidor"
                    class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
                    Хьюмидор
                  </label>
                  <Select
                    v-model="filters.humidor"
                    data-testid="cigar-list-filter-humidor"
                    :options="humidorFilterOptions"
                    option-label="label"
                    option-value="value"
                    class="w-full min-h-12 sm:min-h-11"
                    label-id="cigar-list-filter-humidor" />
                </div>
                <div class="min-w-0 sm:col-span-2 lg:col-span-2">
                  <label
                    for="cigar-list-filter-smoked"
                    class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
                    Выкурена
                  </label>
                  <Select
                    v-model="filters.smoked"
                    data-testid="cigar-list-filter-smoked"
                    :options="smokedFilterOptions"
                    option-label="label"
                    option-value="value"
                    class="w-full min-h-12 sm:min-h-11"
                    label-id="cigar-list-filter-smoked" />
                </div>
              </div>
            </form>

            <div
              v-if="filtersActive"
              class="flex flex-col items-stretch gap-3 border-t border-stone-100 pt-6 dark:border-stone-700/80 sm:flex-row sm:flex-wrap sm:items-center"
              data-testid="cigar-list-filter-actions">
              <Button
                data-testid="cigar-list-filter-reset"
                class="min-h-12 w-full touch-manipulation sm:w-auto sm:min-h-11"
                label="Сбросить фильтры"
                icon="pi pi-filter-slash"
                severity="secondary"
                outlined
                type="button"
                @click="resetFilters" />
            </div>
          </div>
        </Transition>
      </div>

      <div
        v-if="loading"
        data-testid="cigar-list-loading"
        class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5 sm:gap-6 min-h-[20rem]"
        aria-busy="true"
        aria-live="polite">
        <Skeleton
          v-for="n in 3"
          :key="n"
          class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          height="22rem"
          data-testid="cigar-list-skeleton" />
      </div>

      <div
        v-else-if="error"
        class="rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80 max-w-2xl"
        data-testid="cigar-list-error"
        role="alert">
        <Message severity="error">{{ error }}</Message>
        <Button
          data-testid="cigar-list-retry"
          class="mt-4 min-h-12 w-full sm:w-auto touch-manipulation"
          label="Повторить загрузку"
          icon="pi pi-refresh"
          severity="secondary"
          outlined
          @click="loadCigars" />
      </div>

      <div
        v-else-if="cigars.length > 0 && displayedCigars.length === 0"
        class="text-center rounded-2xl border border-dashed border-amber-700/30 bg-white/80 px-5 py-12 dark:border-amber-200/20 dark:bg-stone-900/60 max-w-xl mx-auto"
        data-testid="cigar-list-filter-empty">
        <span
          class="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-amber-100/90 text-amber-900 dark:bg-amber-900/40 dark:text-amber-100"
          aria-hidden="true">
          <i class="pi pi-search-minus text-2xl" />
        </span>
        <h2 class="text-2xl font-semibold text-stone-900 dark:text-rose-50/95 mb-2">Ничего не найдено</h2>
        <p class="text-stone-600 dark:text-stone-400 mb-6 text-pretty">Попробуйте изменить фильтры или сбросить их.</p>
        <Button
          data-testid="cigar-list-filter-empty-reset"
          class="min-h-12 px-6 touch-manipulation"
          label="Сбросить фильтры"
          icon="pi pi-filter-slash"
          severity="secondary"
          outlined
          @click="resetFilters" />
      </div>

      <div
        v-else-if="cigars.length === 0"
        class="text-center rounded-2xl border border-dashed border-rose-800/25 bg-white/80 px-5 py-12 dark:border-rose-200/15 dark:bg-stone-900/60 max-w-xl mx-auto"
        data-testid="cigar-list-empty">
        <span
          class="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-rose-100/90 text-rose-900 dark:bg-rose-900/40 dark:text-rose-100"
          aria-hidden="true">
          <i class="pi pi-bookmark text-2xl" />
        </span>
        <h2 class="text-2xl font-semibold text-stone-900 dark:text-rose-50/95 mb-2">Пока пусто</h2>
        <p class="text-stone-600 dark:text-stone-400 mb-6 text-pretty">
          Добавьте первую сигару — бренд, формат и ваши заметки останутся под рукой.
        </p>
        <Button
          data-testid="cigar-list-empty-add"
          class="min-h-12 px-6 touch-manipulation"
          label="Добавить сигару"
          icon="pi pi-plus"
          @click="$router.push({ name: 'CigarNew' })" />
      </div>

      <div
        v-else
        class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5 sm:gap-6"
        data-testid="cigar-list-grid">
        <article
          v-for="(cigar, index) in displayedCigars"
          :key="cigar.id"
          v-memo="memoKey(cigar)"
          :data-testid="`cigar-card-${cigar.id}`"
          class="cigar-card-enter group relative flex flex-col overflow-hidden rounded-2xl border border-stone-200/90 bg-white/95 shadow-md shadow-stone-900/5 transition-[box-shadow,transform] duration-300 hover:shadow-lg hover:shadow-rose-900/10 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 dark:hover:shadow-black/70 dark:hover:border-rose-900/30 min-h-[20rem] motion-reduce:transition-none motion-reduce:animate-none"
          :style="{ animationDelay: `${Math.min(index, 8) * 48}ms` }">
          <div
            class="relative z-20 shrink-0 h-48 rounded-t-2xl overflow-hidden border-b border-stone-100 dark:border-stone-700/80 bg-stone-100 dark:bg-stone-800/80">
            <Carousel
              :value="orderUserCigarGalleryImages(cigar.images)"
              :num-visible="1"
              :num-scroll="1"
              class="cigar-carousel w-full h-full max-h-48"
              :circular="(cigar.images?.length ?? 0) > 1"
              :show-indicators="(cigar.images?.length ?? 0) > 1"
              :show-navigators="(cigar.images?.length ?? 0) > 1">
              <template #item="slotProps">
                <button
                  type="button"
                  class="cigar-list-card-image-frame relative h-48 w-full min-h-0 cursor-pointer touch-manipulation border-0 bg-transparent p-0 text-left"
                  :aria-label="`Открыть сигару ${cigar.name}`"
                  @click="viewCigar(cigar)">
                  <div
                    class="cigar-list-card-image-inner absolute inset-0 box-border flex min-h-0 min-w-0 items-center justify-center p-2">
                    <img
                      v-if="carouselItemImageSrc(slotProps.data)"
                      :src="carouselItemImageSrc(slotProps.data)"
                      :alt="cigar.name"
                      loading="lazy"
                      decoding="async" />
                    <i
                      v-else
                      class="pi pi-image text-5xl text-stone-400 dark:text-stone-500"
                      aria-hidden="true" />
                  </div>
                </button>
              </template>
              <template #empty>
                <button
                  type="button"
                  class="cigar-list-card-image-frame relative h-48 w-full min-h-0 cursor-pointer touch-manipulation border-0 bg-transparent p-0 text-left"
                  :aria-label="`Открыть сигару ${cigar.name}`"
                  @click="viewCigar(cigar)">
                  <div
                    class="cigar-list-card-image-inner absolute inset-0 box-border flex min-h-0 min-w-0 items-center justify-center p-2">
                    <i
                      class="pi pi-image text-5xl text-stone-400 dark:text-stone-500"
                      aria-hidden="true" />
                  </div>
                </button>
              </template>
            </Carousel>
          </div>

          <RouterLink
            :to="{ name: 'CigarDetail', params: { id: String(cigar.id) } }"
            class="relative z-10 flex flex-1 flex-col gap-2.5 p-5 min-h-0 no-underline text-inherit focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-rose-700 dark:focus-visible:outline-rose-400 rounded-none">
            <h2
              class="text-lg sm:text-xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 pr-1 line-clamp-2">
              {{ cigar.name }}
            </h2>
            <p class="text-sm font-medium text-stone-700 dark:text-stone-300 line-clamp-1">
              {{ cigar.brand.name }}
            </p>
            <div class="flex flex-wrap items-center gap-x-3 gap-y-1 text-sm text-stone-600 dark:text-stone-400">
              <span v-if="cigar.size">{{ cigar.size }}</span>
              <span v-if="cigar.strength">{{ cigar.strength }}</span>
              <span
                :data-testid="`cigar-card-quantity-${cigar.id}`"
                class="font-medium text-stone-700 dark:text-stone-300">
                {{ collectionQuantityShort(cigar) }}
              </span>
            </div>
            <div
              class="flex flex-wrap items-center justify-between gap-2 mt-auto pt-2 border-t border-stone-100 dark:border-stone-700/80">
              <div
                v-if="cigar.rating != null"
                class="flex items-center gap-1">
                <div
                  class="flex text-rose-500 dark:text-rose-400"
                  aria-hidden="true">
                  <svg
                    v-for="i in 5"
                    :key="i"
                    class="h-4 w-4 shrink-0"
                    :class="(cigar.rating ?? 0) >= i * 2 - 1 ? 'opacity-100' : 'opacity-25'"
                    fill="currentColor"
                    viewBox="0 0 20 20">
                    <path
                      d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                  </svg>
                </div>
                <span class="text-sm text-stone-600 dark:text-stone-400">{{ cigar.rating }}/10</span>
              </div>
              <span
                v-else
                class="text-sm text-stone-500 dark:text-stone-500"
                >Без оценки</span
              >
              <span
                v-if="cigar.price != null"
                class="text-sm font-semibold text-stone-900 dark:text-stone-100 whitespace-nowrap">
                {{ formatPrice(cigar.price) }}
              </span>
            </div>
            <div
              v-if="cigar.humidorId"
              class="flex items-center gap-2 text-sm text-stone-600 dark:text-stone-400 pt-1">
              <i
                class="pi pi-box text-rose-700 dark:text-rose-400 shrink-0"
                aria-hidden="true" />
              <span>В хьюмидоре</span>
            </div>
            <div
              v-if="cigar.isSmoked"
              class="flex items-center gap-2 text-sm text-emerald-700 dark:text-emerald-300 pt-1">
              <i
                class="pi pi-check-circle shrink-0"
                aria-hidden="true" />
              <span>Уже выкурена</span>
            </div>
          </RouterLink>

          <footer
            class="relative z-20 mt-auto flex justify-end gap-2 border-t border-stone-100 bg-stone-50/90 px-3 py-3 dark:border-stone-700/80 dark:bg-stone-950/50">
            <Button
              v-if="!cigar.isSmoked"
              :data-testid="`cigar-smoke-${cigar.id}`"
              class="min-h-11 min-w-11 touch-manipulation"
              icon="pi pi-check-circle"
              text
              rounded
              severity="success"
              aria-label="Отметить как выкуренную"
              @click="confirmMarkAsSmoked(cigar)" />
            <Button
              :data-testid="`cigar-edit-${cigar.id}`"
              class="min-h-11 min-w-11 touch-manipulation"
              icon="pi pi-pencil"
              text
              rounded
              severity="secondary"
              aria-label="Редактировать сигару"
              @click="$router.push({ name: 'CigarEdit', params: { id: String(cigar.id) } })" />
            <Button
              :data-testid="`cigar-delete-${cigar.id}`"
              class="min-h-11 min-w-11 touch-manipulation"
              icon="pi pi-trash"
              text
              rounded
              severity="danger"
              aria-label="Удалить сигару"
              @click="confirmDelete(cigar)" />
          </footer>
        </article>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted, onUnmounted, watch } from 'vue';
  import { RouterLink, useRouter } from 'vue-router';
  import { useLocalStorage } from '@vueuse/core';
  import { useConfirm } from 'primevue/useconfirm';
  import { useToast } from 'primevue/usetoast';
  import api from '@/services/api';
  import cigarService from '../services/cigarService';
  import type { Cigar, CigarImage } from '../services/cigarService';
  import { cigarImageInlineDataSrc, orderUserCigarGalleryImages } from '@/utils/cigarImageDisplay';
  import { strengthOptions } from '@/utils/cigarOptions';

  type HumidorFilterValue = 'all' | 'in_humidor' | 'outside';
  type SmokedFilterValue = 'all' | 'not_smoked' | 'smoked';

  interface ListFilters {
    search: string;
    brandId: number | null;
    strength: string | null;
    size: string | null;
    humidor: HumidorFilterValue;
    smoked: SmokedFilterValue;
  }

  interface SelectOption {
    label: string;
    value: string | number;
  }

  const router = useRouter();
  const confirm = useConfirm();
  const toast = useToast();

  /** По умолчанию свёрнуто — фильтры не занимают место до явного разворачивания. */
  const filtersExpanded = useLocalStorage('cigar-list-filters-expanded', false);

  const filters = ref<ListFilters>({
    search: '',
    brandId: null,
    strength: null,
    size: null,
    humidor: 'all',
    smoked: 'all',
  });

  const humidorFilterOptions: { label: string; value: HumidorFilterValue }[] = [
    { label: 'Все', value: 'all' },
    { label: 'В хьюмидоре', value: 'in_humidor' },
    { label: 'Вне хьюмидора', value: 'outside' },
  ];

  const smokedFilterOptions: { label: string; value: SmokedFilterValue }[] = [
    { label: 'Все', value: 'all' },
    { label: 'Только не выкуренные', value: 'not_smoked' },
    { label: 'Только выкуренные', value: 'smoked' },
  ];

  const loading = ref(true);
  const error = ref<string | null>(null);
  const cigars = ref<Cigar[]>([]);

  /** Blob/data URL по id изображения (MinIO: только после GET с Authorization). */
  const listThumbByImageId = ref<Record<number, string>>({});
  let listThumbLoadGen = 0;

  function revokeListThumbBlobs(rec: Record<number, string>): void {
    for (const u of Object.values(rec)) {
      if (u.startsWith('blob:')) {
        URL.revokeObjectURL(u);
      }
    }
  }

  const filtersActive = computed(() => {
    const f = filters.value;
    return (
      Boolean(f.search?.trim()) ||
      f.brandId != null ||
      f.strength != null ||
      f.size != null ||
      f.humidor !== 'all' ||
      f.smoked !== 'all'
    );
  });

  const brandOptions = computed<SelectOption[]>(() => {
    const map = new Map<number, string>();
    for (const c of cigars.value) {
      const id = c.brand?.id;
      const name = c.brand?.name?.trim();
      if (id != null && name) {
        map.set(id, name);
      }
    }
    return [...map.entries()]
      .sort((a, b) => a[1].localeCompare(b[1], 'ru'))
      .map(([value, label]) => ({ value, label }));
  });

  function uniqueStringOptions(values: (string | null | undefined)[]): SelectOption[] {
    const set = new Set<string>();
    for (const v of values) {
      const t = v?.trim();
      if (t) {
        set.add(t);
      }
    }
    return [...set].sort((a, b) => a.localeCompare(b, 'ru')).map((value) => ({ value, label: value }));
  }

  const strengthOptionsFromData = computed((): SelectOption[] => {
    const present = new Set<string>();
    for (const c of cigars.value) {
      const t = c.strength?.trim();
      if (t) {
        present.add(t);
      }
    }
    const knownValues = new Set(strengthOptions.map((o) => o.value));
    const fromCatalog = strengthOptions.filter((o) => present.has(o.value));
    const extras = [...present]
      .filter((v) => !knownValues.has(v))
      .sort((a, b) => a.localeCompare(b, 'ru'))
      .map((value) => ({ value, label: value }));
    return [...fromCatalog, ...extras];
  });

  const sizeOptionsFromData = computed(() => uniqueStringOptions(cigars.value.map((c) => c.size)));

  function matchesFilters(cigar: Cigar): boolean {
    const f = filters.value;
    const q = f.search.trim().toLowerCase();
    if (q) {
      const name = (cigar.name ?? '').toLowerCase();
      const brand = (cigar.brand?.name ?? '').toLowerCase();
      if (!name.includes(q) && !brand.includes(q)) {
        return false;
      }
    }
    if (f.brandId != null && cigar.brand?.id !== f.brandId) {
      return false;
    }
    if (f.strength != null && (cigar.strength?.trim() ?? '') !== f.strength) {
      return false;
    }
    if (f.size != null && (cigar.size?.trim() ?? '') !== f.size) {
      return false;
    }
    if (f.humidor === 'in_humidor' && cigar.humidorId == null) {
      return false;
    }
    if (f.humidor === 'outside' && cigar.humidorId != null) {
      return false;
    }
    const smoked = Boolean(cigar.isSmoked);
    if (f.smoked === 'not_smoked' && smoked) {
      return false;
    }
    if (f.smoked === 'smoked' && !smoked) {
      return false;
    }
    return true;
  }

  const displayedCigars = computed(() => cigars.value.filter(matchesFilters));

  function resetFilters(): void {
    filters.value = {
      search: '',
      brandId: null,
      strength: null,
      size: null,
      humidor: 'all',
      smoked: 'all',
    };
  }

  watch(
    displayedCigars,
    async () => {
      const gen = ++listThumbLoadGen;
      revokeListThumbBlobs(listThumbByImageId.value);
      const inline: Record<number, string> = {};
      const idsToFetchSet = new Set<number>();

      for (const c of displayedCigars.value) {
        for (const img of c.images ?? []) {
          const local = cigarImageInlineDataSrc(img);
          if (local) {
            inline[img.id] = local;
          } else if (img.id) {
            idsToFetchSet.add(img.id);
          }
        }
      }

      const idsToFetch = [...idsToFetchSet];

      listThumbByImageId.value = { ...inline };

      await Promise.all(
        idsToFetch.map(async (imageId) => {
          try {
            const { data } = await api.get<Blob>(`cigarimages/${imageId}/thumbnail`, { responseType: 'blob' });
            const objectUrl = URL.createObjectURL(data);
            if (gen !== listThumbLoadGen) {
              URL.revokeObjectURL(objectUrl);
              return;
            }
            listThumbByImageId.value = { ...listThumbByImageId.value, [imageId]: objectUrl };
          } catch {
            if (import.meta.env.DEV) {
              console.warn('Миниатюра не загружена для изображения', imageId);
            }
          }
        }),
      );
    },
    { deep: true, immediate: true },
  );

  function carouselItemImageSrc(img: CigarImage | undefined): string {
    if (!img) {
      return '';
    }
    const fromMap = listThumbByImageId.value[img.id];
    if (fromMap) {
      return fromMap;
    }
    return cigarImageInlineDataSrc(img);
  }

  function memoKey(cigar: Cigar): (string | number | null | undefined)[] {
    const imageIds = (cigar.images ?? [])
      .map((i) => i.id)
      .slice()
      .sort((a, b) => a - b)
      .join(',');
    return [
      cigar.id,
      cigar.name,
      cigar.brand?.name,
      cigar.size,
      cigar.strength,
      cigar.quantity,
      cigar.rating,
      cigar.price,
      cigar.humidorId,
      cigar.images?.length,
      imageIds,
    ];
  }

  onUnmounted(() => {
    revokeListThumbBlobs(listThumbByImageId.value);
    listThumbByImageId.value = {};
    listThumbLoadGen += 1;
  });

  async function loadCigars(): Promise<void> {
    loading.value = true;
    error.value = null;
    try {
      cigars.value = await cigarService.getCigars();
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка загрузки сигар:', err);
      }
      error.value = 'Не удалось загрузить сигары. Попробуйте позже.';
    } finally {
      loading.value = false;
    }
  }

  function viewCigar(cigar: Cigar): void {
    if (cigar.id != null) {
      router.push({ name: 'CigarDetail', params: { id: String(cigar.id) } });
    }
  }

  function formatPrice(price: number): string {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: 'RUB',
    }).format(price);
  }

  function collectionQuantityShort(cigar: Cigar): string {
    const q = cigar.quantity;
    const n = q != null && Number.isFinite(q) ? Math.min(9999, Math.max(1, Math.trunc(q))) : 1;
    return `${n} шт.`;
  }

  function confirmDelete(cigar: Cigar): void {
    confirm.require({
      message: `Удалить сигару «${cigar.name}»? Действие нельзя отменить.`,
      header: 'Подтверждение удаления',
      icon: 'pi pi-exclamation-triangle',
      rejectClass: 'p-button-secondary p-button-outlined',
      acceptClass: 'p-button-danger',
      rejectLabel: 'Отмена',
      acceptLabel: 'Удалить',
      accept: async () => {
        if (cigar.id == null) {
          toast.add({
            severity: 'error',
            summary: 'Ошибка',
            detail: 'Не удалось определить ID сигары',
            life: 3000,
          });
          return;
        }
        try {
          await cigarService.deleteCigar(cigar.id);
          cigars.value = cigars.value.filter((c) => c.id !== cigar.id);
          toast.add({ severity: 'success', summary: 'Удалено', detail: 'Сигара удалена', life: 3000 });
        } catch (err) {
          if (import.meta.env.DEV) {
            console.error('Ошибка удаления сигары:', err);
          }
          toast.add({
            severity: 'error',
            summary: 'Ошибка',
            detail: 'Не удалось удалить сигару',
            life: 3000,
          });
        }
      },
    });
  }

  function confirmMarkAsSmoked(cigar: Cigar): void {
    confirm.require({
      message: `Отметить «${cigar.name}» как выкуренную? Сигара будет убрана из хьюмидора.`,
      header: 'Подтверждение',
      icon: 'pi pi-check-circle',
      rejectClass: 'p-button-secondary p-button-outlined',
      acceptClass: 'p-button-success',
      rejectLabel: 'Отмена',
      acceptLabel: 'Отметить',
      accept: async () => {
        if (cigar.id == null) {
          toast.add({
            severity: 'error',
            summary: 'Ошибка',
            detail: 'Не удалось определить ID сигары',
            life: 3000,
          });
          return;
        }
        try {
          const updated = await cigarService.markCigarAsSmoked(cigar.id);
          cigars.value = cigars.value.map((item) => (item.id === updated.id ? updated : item));
          toast.add({
            severity: 'success',
            summary: 'Готово',
            detail: 'Сигара отмечена как выкуренная',
            life: 3000,
          });
        } catch (err) {
          if (import.meta.env.DEV) {
            console.error('Ошибка отметки выкуривания:', err);
          }
          toast.add({
            severity: 'error',
            summary: 'Ошибка',
            detail: 'Не удалось отметить сигару как выкуренную',
            life: 3000,
          });
        }
      },
    });
  }

  onMounted(() => {
    loadCigars();
  });
</script>

<style scoped>
  .cigar-list-root {
    position: relative;
    isolation: isolate;
  }

  .cigar-list-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  /*:global(.dark) .cigar-list-grain {
    mix-blend-mode: soft-light;
  }*/

  .cigar-list-card-image-frame {
    display: block;
    width: 100%;
    max-width: 100%;
  }

  .cigar-list-card-image-frame img {
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

  .line-clamp-1 {
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 1;
    overflow: hidden;
  }

  .line-clamp-2 {
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 2;
    overflow: hidden;
  }

  /* PrimeVue: .p-carousel-content-container { overflow: auto } — убираем скролл в карточке */
  .cigar-carousel :deep(.p-carousel-content-container) {
    overflow: hidden;
  }

  /* Карусель: крупнее зоны нажатия для стрелок PrimeVue */
  .cigar-carousel :deep(.p-carousel-prev),
  .cigar-carousel :deep(.p-carousel-next) {
    min-width: 2.75rem;
    min-height: 2.75rem;
  }

  .cigar-card-enter {
    animation: cigar-card-in 0.5s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes cigar-card-in {
    from {
      opacity: 0;
      transform: translateY(10px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  @media (prefers-reduced-motion: reduce) {
    .cigar-card-enter {
      animation: none;
    }
  }

  .cl-list-filters-panel-enter-active,
  .cl-list-filters-panel-leave-active {
    transition:
      opacity 0.2s ease,
      transform 0.2s ease;
  }

  .cl-list-filters-panel-enter-from,
  .cl-list-filters-panel-leave-to {
    opacity: 0;
    transform: translateY(-6px);
  }

  @media (prefers-reduced-motion: reduce) {
    .cl-list-filters-panel-enter-active,
    .cl-list-filters-panel-leave-active {
      transition: none;
    }

    .cl-list-filters-panel-enter-from,
    .cl-list-filters-panel-leave-to {
      transform: none;
    }
  }
</style>
