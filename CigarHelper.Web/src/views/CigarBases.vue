<template>
  <section
    class="cigar-bases-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-50 via-rose-50/40 to-stone-50 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-rose-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="cigar-bases"
    aria-labelledby="cigar-bases-heading">
    <div
      class="cigar-bases-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] max-w-7xl mx-auto">
      <header class="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between pb-6 sm:pb-8">
        <div class="min-w-0">
          <p
            class="text-[0.65rem] uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55 font-semibold mb-1.5">
            Справочник
          </p>
          <h1
            id="cigar-bases-heading"
            class="text-3xl sm:text-4xl font-semibold text-stone-900 dark:text-rose-50/95 tracking-tight text-balance">
            База сигар
          </h1>
          <p class="mt-1.5 text-sm text-stone-600 dark:text-stone-400 max-w-xl text-pretty">
            Каталог образцов: поиск, фильтры и быстрые действия — на десктопе таблица, на телефоне карточки.
          </p>
        </div>
        <Button
          data-testid="cigar-bases-add"
          class="w-full sm:w-auto shrink-0 min-h-12 px-5 sm:min-h-11 touch-manipulation shadow-md shadow-rose-900/10 dark:shadow-black/40"
          label="Добавить в коллекцию (новая сигара)"
          icon="pi pi-plus"
          @click="createNewCigar" />
      </header>

      <!-- Поиск и фильтры — тот же каркас, что hero на главной (Home.vue) -->
      <div
        class="mb-8 rounded-2xl border border-stone-200/90 bg-white/95 p-6 shadow-md shadow-stone-900/5 sm:mb-10 sm:p-8 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50"
        data-testid="cigar-bases-filters">
        <div class="flex flex-col gap-6 sm:gap-8">
          <div class="flex flex-col gap-4 sm:flex-row sm:items-start sm:gap-6">
            <span
              class="flex h-12 w-12 shrink-0 items-center justify-center rounded-2xl bg-rose-100/90 text-rose-900 dark:bg-rose-900/40 dark:text-rose-100 sm:h-14 sm:w-14"
              aria-hidden="true">
              <i class="pi pi-filter-slash text-2xl" />
            </span>
            <div class="min-w-0 flex-1">
              <h2
                id="cigar-bases-filters-heading"
                class="text-lg font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 sm:text-xl">
                Поиск и фильтры
              </h2>
              <p
                class="mt-1.5 max-w-2xl text-pretty text-sm leading-relaxed text-stone-700 dark:text-stone-300 sm:text-base">
                Уточните выдачу по названию, бренду или крепости — на большом экране удобна таблица, на телефоне те же
                данные в карточках.
              </p>
            </div>
          </div>

          <form
            role="search"
            class="grid grid-cols-1 gap-5 sm:gap-6 lg:grid-cols-12 lg:items-end"
            aria-labelledby="cigar-bases-filters-heading"
            @submit.prevent>
            <div class="min-w-0 lg:col-span-5">
              <label
                for="cigar-bases-search"
                class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
                Поиск
              </label>
              <span class="p-input-icon-left flex w-full [&_input]:w-full [&_input]:min-h-12 sm:[&_input]:min-h-11">
                <i
                  class="pi pi-search text-stone-400"
                  aria-hidden="true" />
                <InputText
                  id="cigar-bases-search"
                  v-model="filters.search"
                  placeholder="Название или бренд..."
                  class="w-full"
                  data-testid="cigar-bases-search"
                  autocomplete="off"
                  @input="onSearch" />
              </span>
            </div>
            <div class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:col-span-7 lg:grid-cols-2 lg:gap-6">
              <div class="min-w-0">
                <label
                  for="cigar-bases-filter-brand-input"
                  class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
                  Бренд
                </label>
                <Dropdown
                  v-model="filters.brand"
                  data-testid="cigar-bases-filter-brand"
                  :options="brandOptions"
                  option-label="label"
                  option-value="value"
                  placeholder="Все бренды"
                  class="w-full [&_.p-dropdown]:min-h-12 sm:[&_.p-dropdown]:min-h-11"
                  input-id="cigar-bases-filter-brand-input"
                  :show-clear="true"
                  @change="onFilterChange" />
              </div>
              <div class="min-w-0">
                <label
                  for="cigar-bases-filter-strength-input"
                  class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
                  Крепость
                </label>
                <Dropdown
                  v-model="filters.strength"
                  data-testid="cigar-bases-filter-strength"
                  :options="strengthOptions"
                  option-label="label"
                  option-value="value"
                  placeholder="Все"
                  class="w-full [&_.p-dropdown]:min-h-12 sm:[&_.p-dropdown]:min-h-11"
                  input-id="cigar-bases-filter-strength-input"
                  :show-clear="true"
                  @change="onFilterChange" />
              </div>
            </div>
          </form>

          <div
            v-if="filtersActive"
            class="flex flex-col items-stretch gap-3 border-t border-stone-100 pt-6 dark:border-stone-700/80 sm:flex-row sm:flex-wrap sm:items-center"
            data-testid="cigar-bases-filter-actions">
            <Button
              data-testid="cigar-bases-filter-reset"
              class="min-h-12 w-full touch-manipulation sm:w-auto sm:min-h-11"
              label="Сбросить фильтры"
              icon="pi pi-filter-slash"
              severity="secondary"
              outlined
              type="button"
              @click="resetFilters" />
          </div>
        </div>
      </div>

      <div
        v-if="error"
        class="mb-6 rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80 max-w-2xl"
        data-testid="cigar-bases-error"
        role="alert">
        <Message severity="error">{{ error }}</Message>
        <Button
          data-testid="cigar-bases-retry"
          class="mt-4 min-h-12 w-full sm:w-auto touch-manipulation"
          label="Повторить загрузку"
          icon="pi pi-refresh"
          severity="secondary"
          outlined
          @click="loadCigars" />
      </div>

      <!-- Загрузка: мобильные скелетоны; на lg таблица сама показывает loading -->
      <div
        v-if="loading"
        data-testid="cigar-bases-loading"
        class="grid min-h-[16rem] grid-cols-1 gap-5 sm:grid-cols-2 lg:hidden sm:gap-6"
        aria-busy="true"
        aria-live="polite">
        <Skeleton
          v-for="n in 3"
          :key="n"
          class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          height="12rem"
          data-testid="cigar-bases-skeleton" />
      </div>

      <div
        v-if="!loading && !error && pagination.totalRecords === 0"
        class="mx-auto max-w-xl rounded-2xl border border-dashed border-rose-800/25 bg-white/80 px-5 py-12 text-center dark:border-rose-200/15 dark:bg-stone-900/60"
        data-testid="cigar-bases-empty">
        <span
          class="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-rose-100/90 text-rose-900 dark:bg-rose-900/40 dark:text-rose-100"
          aria-hidden="true">
          <i class="pi pi-search text-2xl" />
        </span>
        <h2 class="mb-2 text-2xl font-semibold text-stone-900 dark:text-rose-50/95">Ничего не найдено</h2>
        <p class="mb-6 text-pretty text-stone-600 dark:text-stone-400">
          Сбросьте фильтры или измените запрос — в базе пока нет подходящих записей.
        </p>
        <Button
          data-testid="cigar-bases-empty-reset"
          class="min-h-12 touch-manipulation"
          label="Сбросить фильтры"
          icon="pi pi-filter-slash"
          severity="secondary"
          outlined
          @click="resetFilters" />
      </div>

      <!-- Десктоп: таблица -->
      <div
        v-show="!error && (loading || pagination.totalRecords > 0)"
        class="hidden overflow-hidden rounded-2xl border border-stone-200/90 bg-white/95 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 lg:block">
        <DataTable
          data-key="id"
          :value="cigars"
          :loading="loading"
          :paginator="true"
          :rows="pagination.rows"
          :total-records="pagination.totalRecords"
          lazy
          :rows-per-page-options="[10, 20, 50, 100]"
          :sort-field="sortField"
          :sort-order="sortOrder"
          data-testid="cigar-bases-table"
          striped-rows
          show-gridlines
          responsive-layout="scroll"
          class="p-datatable-sm text-stone-800 dark:text-stone-200 [&_.p-datatable-thead>tr>th]:bg-stone-50/90 dark:[&_.p-datatable-thead>tr>th]:bg-stone-950/80"
          :virtual-scroller-options="{ itemSize: 60, scrollHeight: '600px' }"
          @page="onPage"
          @sort="onSort">
          <Column
            header="Логотип"
            style="width: 4rem">
            <template #body="{ data }">
              <div
                class="flex h-10 w-10 items-center justify-center overflow-hidden rounded-full bg-stone-100 dark:bg-stone-800">
                <img
                  v-if="getBrandLogoSrc(data)"
                  :src="getBrandLogoSrc(data)!"
                  :alt="data.brand?.name ?? ''"
                  class="h-full w-full object-cover"
                  width="40"
                  height="40"
                  loading="lazy"
                  decoding="async" />
                <i
                  v-else
                  class="pi pi-image text-lg text-stone-400"
                  aria-hidden="true" />
              </div>
            </template>
          </Column>
          <Column
            field="name"
            header="Название"
            sortable>
            <template #body="{ data }">
              <div class="font-semibold text-stone-900 dark:text-rose-50/90">
                {{ data.name }}
              </div>
            </template>
          </Column>
          <Column
            field="brand.name"
            header="Бренд"
            sortable>
            <template #body="{ data }">
              <div class="flex flex-wrap items-center gap-2">
                <span class="font-medium">{{ data.brand.name }}</span>
                <span
                  v-if="data.country"
                  class="text-xs text-stone-500 dark:text-stone-400">
                  ({{ data.country }})
                </span>
              </div>
            </template>
          </Column>
          <Column
            field="size"
            header="Размер"
            sortable>
            <template #body="{ data }">
              <span v-if="data.size">{{ data.size }}</span>
              <span
                v-else
                class="text-stone-400"
                >—</span
              >
            </template>
          </Column>
          <Column
            field="strength"
            header="Крепость"
            sortable>
            <template #body="{ data }">
              <span
                v-if="data.strength"
                class="rounded-full px-2.5 py-0.5 text-xs font-medium"
                :class="getStrengthBadgeClass(data.strength)">
                {{ getStrengthLabel(data.strength) }}
              </span>
              <span
                v-else
                class="text-stone-400"
                >—</span
              >
            </template>
          </Column>
          <Column
            field="wrapper"
            header="Покровный">
            <template #body="{ data }">
              <span v-if="data.wrapper">{{ data.wrapper }}</span>
              <span
                v-else
                class="text-stone-400"
                >—</span
              >
            </template>
          </Column>
          <Column
            field="binder"
            header="Связующий">
            <template #body="{ data }">
              <span v-if="data.binder">{{ data.binder }}</span>
              <span
                v-else
                class="text-stone-400"
                >—</span
              >
            </template>
          </Column>
          <Column
            field="filler"
            header="Наполнитель">
            <template #body="{ data }">
              <span v-if="data.filler">{{ data.filler }}</span>
              <span
                v-else
                class="text-stone-400"
                >—</span
              >
            </template>
          </Column>
          <Column
            header="Действия"
            :exportable="false"
            style="min-width: 13rem">
            <template #body="{ data }">
              <div class="flex flex-wrap gap-1">
                <Button
                  :data-testid="`cigar-bases-row-view-${data.id}`"
                  class="min-h-11 min-w-11 touch-manipulation"
                  icon="pi pi-eye"
                  text
                  rounded
                  severity="secondary"
                  aria-label="Просмотр"
                  @click="viewCigar(data)" />
                <Button
                  :data-testid="`cigar-bases-row-review-${data.id}`"
                  class="min-h-11 min-w-11 touch-manipulation"
                  icon="pi pi-pencil"
                  text
                  rounded
                  severity="secondary"
                  aria-label="Написать отзыв"
                  @click="writeReview(data)" />
                <Button
                  :data-testid="`cigar-bases-row-add-${data.id}`"
                  class="min-h-11 min-w-11 touch-manipulation"
                  icon="pi pi-plus"
                  text
                  rounded
                  aria-label="Добавить в коллекцию"
                  @click="addToCollection(data)" />
                <Button
                  :data-testid="`cigar-bases-row-copy-${data.id}`"
                  class="min-h-11 min-w-11 touch-manipulation"
                  icon="pi pi-copy"
                  text
                  rounded
                  severity="secondary"
                  aria-label="Создать похожую"
                  @click="createSimilarCigar(data)" />
              </div>
            </template>
          </Column>
        </DataTable>
      </div>

      <!-- Мобильные карточки -->
      <div
        v-show="!error && (loading || pagination.totalRecords > 0)"
        class="lg:hidden">
        <p
          v-if="!loading && pagination.totalRecords > 0"
          class="mb-4 text-sm text-stone-600 dark:text-stone-400"
          data-testid="cigar-bases-mobile-summary">
          Показано {{ pagination.first + 1 }}-{{
            Math.min(pagination.first + pagination.rows, pagination.totalRecords)
          }}
          из
          {{ pagination.totalRecords }}
        </p>

        <div
          v-if="!loading"
          class="grid grid-cols-1 gap-5 sm:grid-cols-2 sm:gap-6"
          data-testid="cigar-bases-mobile-grid">
          <article
            v-for="(cigar, index) in cigars"
            :key="cigar.id"
            v-memo="memoKey(cigar)"
            :data-testid="`cigar-base-card-${cigar.id}`"
            class="cigar-base-card-enter relative flex flex-col overflow-hidden rounded-2xl border border-stone-200/90 bg-white/95 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 min-h-[9rem] motion-reduce:animate-none"
            :style="{ animationDelay: `${Math.min(index, 8) * 40}ms` }">
            <div class="flex gap-4 p-4">
              <div
                class="relative h-24 w-24 shrink-0 overflow-hidden rounded-xl bg-stone-100 dark:bg-stone-800 ring-1 ring-stone-200/80 dark:ring-stone-600/60">
                <img
                  v-if="cigarBaseThumbnailSrc(cigar)"
                  :src="cigarBaseThumbnailSrc(cigar)"
                  :alt="cigar.name"
                  width="96"
                  height="96"
                  class="h-full w-full object-cover"
                  loading="lazy"
                  decoding="async"
                  @error="handleImageError" />
                <div
                  v-else
                  class="flex h-full w-full items-center justify-center">
                  <i
                    class="pi pi-image text-3xl text-stone-400"
                    aria-hidden="true" />
                </div>
              </div>
              <div class="min-w-0 flex-1">
                <h2 class="line-clamp-2 text-base font-semibold text-stone-900 dark:text-rose-50/95">
                  {{ cigar.name }}
                </h2>
                <div class="mt-1 flex flex-wrap items-center gap-2">
                  <img
                    v-if="getBrandLogoSrc(cigar)"
                    :src="getBrandLogoSrc(cigar)!"
                    :alt="cigar.brand.name"
                    class="h-4 w-4 rounded object-contain"
                    width="16"
                    height="16"
                    loading="lazy" />
                  <span class="text-sm font-medium text-stone-700 dark:text-stone-300">{{ cigar.brand.name }}</span>
                  <span
                    v-if="cigar.country"
                    class="text-xs text-stone-500"
                    >({{ cigar.country }})</span
                  >
                </div>
                <div class="mt-2 flex flex-wrap gap-2">
                  <span
                    v-if="cigar.size"
                    class="rounded-full bg-stone-200/80 px-2 py-0.5 text-xs font-medium text-stone-800 dark:bg-stone-700/80 dark:text-stone-200">
                    {{ cigar.size }}
                  </span>
                  <span
                    v-if="cigar.strength"
                    class="rounded-full px-2 py-0.5 text-xs font-medium"
                    :class="getStrengthBadgeClass(cigar.strength)">
                    {{ getStrengthLabel(cigar.strength) }}
                  </span>
                </div>
              </div>
            </div>
            <footer
              class="mt-auto flex flex-wrap justify-end gap-2 border-t border-stone-100 bg-stone-50/90 px-2 py-2 dark:border-stone-700/80 dark:bg-stone-950/50">
              <Button
                :data-testid="`cigar-base-view-${cigar.id}`"
                class="min-h-11 min-w-11 touch-manipulation"
                icon="pi pi-eye"
                text
                rounded
                severity="secondary"
                aria-label="Подробнее"
                @click="viewCigar(cigar)" />
              <Button
                :data-testid="`cigar-base-review-${cigar.id}`"
                class="min-h-11 min-w-11 touch-manipulation"
                icon="pi pi-pencil"
                text
                rounded
                severity="secondary"
                aria-label="Отзыв"
                @click="writeReview(cigar)" />
              <Button
                :data-testid="`cigar-base-add-${cigar.id}`"
                class="min-h-11 min-w-11 touch-manipulation"
                icon="pi pi-plus"
                text
                rounded
                aria-label="В коллекцию"
                @click="addToCollection(cigar)" />
              <Button
                :data-testid="`cigar-base-copy-${cigar.id}`"
                class="min-h-11 min-w-11 touch-manipulation"
                icon="pi pi-copy"
                text
                rounded
                severity="secondary"
                aria-label="Похожая"
                @click="createSimilarCigar(cigar)" />
            </footer>
          </article>
        </div>

        <div
          v-if="!loading && pagination.totalRecords > 0"
          class="mt-6 flex justify-center">
          <Paginator
            data-testid="cigar-bases-paginator"
            :first="pagination.first"
            :rows="pagination.rows"
            :total-records="pagination.totalRecords"
            :rows-per-page-options="[10, 20, 50]"
            template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
            @page="onPage" />
        </div>
      </div>
    </div>

    <CigarDetailDialog
      v-model:visible="showDetailDialog"
      :cigar="selectedCigar"
      @write-review="writeReview"
      @add-to-collection="addToCollection"
      @create-similar-cigar="createSimilarCigar"
      @edit-base-cigar="editBaseCigar" />

    <CigarBaseEditDialog
      v-model:visible="showEditDialog"
      :cigar="editingCigar"
      @saved="onCigarSaved" />
  </section>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import { useToast } from 'primevue/usetoast';
  import cigarService from '@/services/cigarService';
  import type { CigarBase, CigarImage, PaginatedResult, Brand } from '@/services/cigarService';
  import type { DataTablePageEvent, DataTableSortEvent } from 'primevue/datatable';
  import type { PageState } from 'primevue/paginator';
  import CigarDetailDialog from '../components/CigarDetailDialog.vue';
  import CigarBaseEditDialog from '../components/CigarBaseEditDialog.vue';
  import { strengthOptions } from '@/utils/cigarOptions';
  import { arrayBufferToBase64 } from '@/utils/imageUtils';

  interface Filters {
    search: string;
    brand: number | null;
    strength: string | null;
  }

  interface Pagination {
    first: number;
    rows: number;
    totalRecords: number;
  }

  interface SelectOption {
    label: string;
    value: string | number;
  }

  const router = useRouter();
  const toast = useToast();

  const loading = ref<boolean>(true);
  const error = ref<string | null>(null);
  const cigars = ref<CigarBase[]>([]);
  const showDetailDialog = ref<boolean>(false);
  const selectedCigar = ref<CigarBase>();
  const showEditDialog = ref<boolean>(false);
  const editingCigar = ref<CigarBase>();

  const filters = ref<Filters>({
    search: '',
    brand: null,
    strength: null,
  });

  const sortField = ref<string>('name');
  const sortOrder = ref<1 | -1>(1);

  const pagination = ref<Pagination>({
    first: 0,
    rows: 20,
    totalRecords: 0,
  });

  let searchTimeout: ReturnType<typeof setTimeout> | null = null;

  const brandOptions = ref<SelectOption[]>([]);

  const filtersActive = computed(() => {
    const f = filters.value;
    return Boolean(f.search?.trim()) || f.brand != null || f.strength != null;
  });

  /** Пиксели: API отдаёт `CigarImageDto.Data` как `data`, а не `imageData`. */
  function cigarImageBytes(img: CigarImage | undefined): string | number[] | undefined {
    if (!img) {
      return undefined;
    }
    return img.imageData ?? img.data;
  }

  function primaryCigarBaseImage(cigar: CigarBase): CigarImage | undefined {
    const list = cigar.images;
    if (!list?.length) {
      return undefined;
    }
    const withBytes = list.filter((i) => {
      const p = cigarImageBytes(i);
      return p != null && (typeof p === 'string' ? p.length > 0 : p.length > 0);
    });
    if (!withBytes.length) {
      return undefined;
    }
    const main = withBytes.find((i) => i.isMain);
    return main ?? withBytes[0];
  }

  function cigarBaseThumbnailSrc(cigar: CigarBase): string {
    const raw = cigarImageBytes(primaryCigarBaseImage(cigar));
    const b64 = arrayBufferToBase64(raw ?? undefined);
    return b64 ? `data:image/jpeg;base64,${b64}` : '';
  }

  function memoKey(cigar: CigarBase): (string | number | null | undefined)[] {
    const raw = cigarImageBytes(primaryCigarBaseImage(cigar));
    const imgLen = raw == null ? 0 : typeof raw === 'string' ? raw.length : raw.length;
    return [cigar.id, cigar.name, cigar.brand?.name, cigar.country, cigar.size, cigar.strength, imgLen];
  }

  function resetFilters(): void {
    filters.value = { search: '', brand: null, strength: null };
    pagination.value.first = 0;
    if (searchTimeout) {
      clearTimeout(searchTimeout);
      searchTimeout = null;
    }
    loadCigars();
  }

  async function loadCigars(): Promise<void> {
    loading.value = true;
    error.value = null;
    try {
      const params = {
        page: pagination.value.first / pagination.value.rows + 1,
        pageSize: pagination.value.rows,
        sortField: sortField.value,
        sortOrder: sortOrder.value === 1 ? 'asc' : 'desc',
        name: filters.value.search,
        brandId: filters.value.brand,
        strength: filters.value.strength,
      };
      const result: PaginatedResult<CigarBase> = await cigarService.getCigarBasesPaginated(params);
      cigars.value = result.items || [];
      pagination.value.totalRecords = result.totalCount || 0;
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Failed to load cigar bases:', err);
      }
      error.value = 'Не удалось загрузить базу сигар. Попробуйте позже.';
    } finally {
      loading.value = false;
    }
  }

  async function loadBrands(): Promise<void> {
    try {
      const allBrands = await cigarService.getBrands();
      brandOptions.value = allBrands.map((brand: Brand) => ({
        label: brand.name,
        value: brand.id,
      }));
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Failed to load brands:', err);
      }
    }
  }

  function viewCigar(cigar: CigarBase): void {
    selectedCigar.value = cigar;
    showDetailDialog.value = true;
  }

  function addToCollection(cigar: CigarBase): void {
    router.push({
      name: 'CigarNew',
      query: {
        cigarBaseId: String(cigar.id),
        name: cigar.name,
        brandId: String(cigar.brand.id),
        brandName: cigar.brand.name,
        country: cigar.country ?? '',
        size: cigar.size ?? '',
        strength: cigar.strength ?? '',
        description: cigar.description ?? '',
        wrapper: cigar.wrapper ?? '',
        binder: cigar.binder ?? '',
        filler: cigar.filler ?? '',
      },
    });
  }

  function createNewCigar(): void {
    router.push({ name: 'CigarNew' });
  }

  function createSimilarCigar(cigar: CigarBase): void {
    router.push({
      name: 'CigarNew',
      query: {
        name: cigar.name,
        brandId: String(cigar.brand.id),
        country: cigar.country ?? '',
        size: cigar.size ?? '',
        strength: cigar.strength ?? '',
        description: cigar.description ?? '',
        wrapper: cigar.wrapper ?? '',
        binder: cigar.binder ?? '',
        filler: cigar.filler ?? '',
      },
    });
  }

  function editBaseCigar(cigar: CigarBase): void {
    editingCigar.value = { ...cigar };
    showDetailDialog.value = false;
    showEditDialog.value = true;
  }

  function onCigarSaved(updatedCigar: CigarBase): void {
    const index = cigars.value.findIndex((c: CigarBase) => c.id === updatedCigar.id);
    if (index !== -1) {
      cigars.value[index] = { ...cigars.value[index], ...updatedCigar };
    }
    if (selectedCigar.value && selectedCigar.value.id === updatedCigar.id) {
      selectedCigar.value = { ...selectedCigar.value, ...updatedCigar };
    }
    showEditDialog.value = false;
    editingCigar.value = undefined;
    toast.add({
      severity: 'success',
      summary: 'Успешно',
      detail: 'Базовая сигара обновлена',
      life: 3000,
    });
  }

  function writeReview(cigar: CigarBase): void {
    router.push({
      name: 'ReviewCreate',
      query: { cigarBaseId: String(cigar.id), brandName: cigar.brand.name, cigarName: cigar.name },
    });
  }

  function onSearch(): void {
    pagination.value.first = 0;
    if (searchTimeout) {
      clearTimeout(searchTimeout);
    }
    searchTimeout = setTimeout(() => {
      loadCigars();
    }, 300);
  }

  function onFilterChange(): void {
    pagination.value.first = 0;
    loadCigars();
  }

  function onPage(event: DataTablePageEvent | PageState): void {
    pagination.value.first = event.first;
    pagination.value.rows = event.rows;
    loadCigars();
  }

  function onSort(event: DataTableSortEvent): void {
    if (event.sortField && (event.sortOrder === 1 || event.sortOrder === -1)) {
      sortField.value = event.sortField as string;
      sortOrder.value = event.sortOrder;
      loadCigars();
    }
  }

  function getStrengthLabel(strength: string | null | undefined): string {
    if (!strength) return '';
    const option = strengthOptions.find((opt) => opt.value === strength);
    return option ? option.label : strength;
  }

  function getStrengthBadgeClass(strength: string | null | undefined): string {
    if (!strength) return 'bg-stone-200/80 text-stone-800 dark:bg-stone-700/80 dark:text-stone-200';
    const classes: Record<string, string> = {
      very_mild: 'bg-emerald-100/90 text-emerald-900 dark:bg-emerald-900/35 dark:text-emerald-200',
      mild: 'bg-sky-100/90 text-sky-900 dark:bg-sky-900/35 dark:text-sky-200',
      medium: 'bg-rose-100/90 text-rose-950 dark:bg-rose-900/40 dark:text-rose-100',
      full: 'bg-orange-100/90 text-orange-950 dark:bg-orange-900/40 dark:text-orange-100',
      very_full: 'bg-red-100/90 text-red-950 dark:bg-red-900/40 dark:text-red-100',
    };
    return classes[strength] || 'bg-stone-200/80 text-stone-800 dark:bg-stone-700/80 dark:text-stone-200';
  }

  function getBrandLogoSrc(cigar: CigarBase): string | undefined {
    if (cigar.brand?.logoBytes) {
      return `data:image/png;base64,${cigar.brand.logoBytes}`;
    }
    return undefined;
  }

  function handleImageError(event: Event): void {
    const target = event.target as HTMLImageElement;
    if (import.meta.env.DEV) {
      console.error('Ошибка загрузки изображения:', target.src);
    }
    target.style.display = 'none';
  }

  onMounted(() => {
    loadCigars();
    loadBrands();
  });
</script>

<style scoped>
  .cigar-bases-root {
    position: relative;
    isolation: isolate;
  }

  .cigar-bases-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  :global(.dark) .cigar-bases-grain {
    mix-blend-mode: soft-light;
  }

  .line-clamp-2 {
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 2;
    overflow: hidden;
  }

  .cigar-base-card-enter {
    animation: cigar-base-in 0.45s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes cigar-base-in {
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
    .cigar-base-card-enter {
      animation: none;
    }
  }
</style>
