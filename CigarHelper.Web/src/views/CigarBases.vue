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
            Каталог образцов с поиском, фильтрами и действиями.
          </p>
        </div>
        <Button
          data-testid="cigar-bases-add"
          class="w-full sm:w-auto shrink-0 min-h-12 px-5 sm:min-h-11 touch-manipulation shadow-md shadow-rose-900/10 dark:shadow-black/40"
          :label="canMutateCatalog ? 'Новая запись справочника' : 'Добавить в коллекцию'"
          icon="pi pi-plus"
          @click="onPrimaryAddClick" />
      </header>

      <!-- Поиск и фильтры — тот же каркас, что hero на главной (Home.vue); тело под спойлер -->
      <div
        class="mb-8 rounded-2xl border border-stone-200/90 bg-white/95 p-6 shadow-md shadow-stone-900/5 sm:mb-10 sm:p-8 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50"
        data-testid="cigar-bases-filters">
        <div class="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between sm:gap-6">
          <div class="flex min-w-0 flex-1 flex-col gap-4 sm:flex-row sm:items-start sm:gap-6">
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
                Уточните выдачу по названию, бренду, крепости из справочника, средним оценкам по отзывам (1–10) или
                отметьте «Нет изображения».
              </p>
            </div>
          </div>
          <div
            class="flex shrink-0 flex-col gap-2 sm:items-end"
            data-testid="cigar-bases-filters-toggle-wrap">
            <Badge
              v-if="!filtersExpanded && filtersActive"
              value="Фильтры активны"
              severity="warn"
              class="w-fit"
              data-testid="cigar-bases-filters-collapsed-hint" />
            <Button
              type="button"
              data-testid="cigar-bases-filters-toggle"
              :aria-expanded="filtersExpanded"
              aria-controls="cigar-bases-filters-panel"
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

        <Transition name="cb-filters-panel">
          <div
            v-show="filtersExpanded"
            id="cigar-bases-filters-panel"
            class="mt-6 flex flex-col gap-6 border-t border-stone-100 pt-6 dark:border-stone-700/80 sm:mt-8 sm:gap-8 sm:pt-8"
            role="region"
            aria-labelledby="cigar-bases-filters-heading">
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
                <IconField class="w-full">
                  <InputIcon
                    class="pi pi-search text-stone-400"
                    aria-hidden="true" />
                  <InputText
                    id="cigar-bases-search"
                    v-model="filters.search"
                    placeholder="Название или бренд..."
                    class="w-full min-h-12 sm:min-h-11"
                    data-testid="cigar-bases-search"
                    autocomplete="off"
                    @input="onSearch" />
                </IconField>
              </div>
              <div
                class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:col-span-7 lg:gap-6"
                :class="canFilterUnmoderated ? 'lg:grid-cols-3' : 'lg:grid-cols-2'">
                <div class="min-w-0">
                  <label
                    for="cigar-bases-filter-brand-input"
                    class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
                    Бренд
                  </label>
                  <Select
                    v-model="filters.brand"
                    data-testid="cigar-bases-filter-brand"
                    :options="brandOptions"
                    option-label="label"
                    option-value="value"
                    placeholder="Все бренды"
                    class="w-full min-h-12 sm:min-h-11"
                    label-id="cigar-bases-filter-brand-input"
                    :show-clear="true"
                    @change="onFilterChange" />
                </div>
                <div class="min-w-0">
                  <label
                    for="cigar-bases-filter-strength-input"
                    class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
                    Крепость
                  </label>
                  <Select
                    v-model="filters.strength"
                    data-testid="cigar-bases-filter-strength"
                    :options="strengthOptions"
                    option-label="label"
                    option-value="value"
                    placeholder="Все"
                    class="w-full min-h-12 sm:min-h-11"
                    label-id="cigar-bases-filter-strength-input"
                    :show-clear="true"
                    @change="onFilterChange" />
                </div>
                <div
                  v-if="canFilterUnmoderated"
                  class="min-w-0">
                  <label
                    for="cigar-bases-filter-moderation-input"
                    class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
                    Модерация (справочник)
                  </label>
                  <Select
                    v-model="filters.moderationFilter"
                    data-testid="cigar-bases-filter-moderation"
                    :options="moderationFilterOptions"
                    option-label="label"
                    option-value="value"
                    class="w-full min-h-12 sm:min-h-11"
                    label-id="cigar-bases-filter-moderation-input"
                    @change="onFilterChange" />
                </div>
              </div>
              <div class="flex items-center gap-2 lg:col-span-12">
                <Checkbox
                  v-model="filters.noImageOnly"
                  input-id="cigar-bases-filter-no-image"
                  data-testid="cigar-bases-filter-no-image"
                  binary
                  @update:model-value="onFilterChange" />
                <label
                  for="cigar-bases-filter-no-image"
                  class="cursor-pointer text-sm text-stone-700 dark:text-stone-300 leading-none select-none">
                  Нет изображения
                </label>
              </div>
              <div class="lg:col-span-12">
                <p class="mb-3 text-xs leading-relaxed text-stone-600 dark:text-stone-400">
                  Средние по отзывам — субъективные шкалы 1–10; это не «Крепость» из карточки справочника выше.
                </p>
                <div
                  class="grid min-w-0 grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3"
                  data-testid="cigar-bases-review-filters">
                  <div class="flex min-w-0 flex-col gap-2">
                    <span class="min-w-0 break-words text-xs font-medium text-stone-600 dark:text-stone-400">
                      Сила / тело (среднее)
                    </span>
                    <div class="grid w-full min-w-0 grid-cols-[minmax(0,1fr)_auto_minmax(0,1fr)] items-center gap-2">
                      <InputNumber
                        v-model="filters.reviewBodyMin"
                        data-testid="cigar-bases-filter-review-body-min"
                        class="min-w-0 w-full"
                        fluid
                        input-class="min-h-11"
                        :min="1"
                        :max="10"
                        :min-fraction-digits="0"
                        :max-fraction-digits="2"
                        placeholder="от"
                        show-clear
                        @update:model-value="onFilterChange" />
                      <span class="shrink-0 self-center text-stone-400">—</span>
                      <InputNumber
                        v-model="filters.reviewBodyMax"
                        data-testid="cigar-bases-filter-review-body-max"
                        class="min-w-0 w-full"
                        fluid
                        input-class="min-h-11"
                        :min="1"
                        :max="10"
                        :min-fraction-digits="0"
                        :max-fraction-digits="2"
                        placeholder="до"
                        show-clear
                        @update:model-value="onFilterChange" />
                    </div>
                  </div>
                  <div class="flex min-w-0 flex-col gap-2">
                    <span class="min-w-0 break-words text-xs font-medium text-stone-600 dark:text-stone-400">
                      Аромат (среднее)
                    </span>
                    <div class="grid w-full min-w-0 grid-cols-[minmax(0,1fr)_auto_minmax(0,1fr)] items-center gap-2">
                      <InputNumber
                        v-model="filters.reviewAromaMin"
                        data-testid="cigar-bases-filter-review-aroma-min"
                        class="min-w-0 w-full"
                        fluid
                        input-class="min-h-11"
                        :min="1"
                        :max="10"
                        :min-fraction-digits="0"
                        :max-fraction-digits="2"
                        placeholder="от"
                        show-clear
                        @update:model-value="onFilterChange" />
                      <span class="shrink-0 self-center text-stone-400">—</span>
                      <InputNumber
                        v-model="filters.reviewAromaMax"
                        data-testid="cigar-bases-filter-review-aroma-max"
                        class="min-w-0 w-full"
                        fluid
                        input-class="min-h-11"
                        :min="1"
                        :max="10"
                        :min-fraction-digits="0"
                        :max-fraction-digits="2"
                        placeholder="до"
                        show-clear
                        @update:model-value="onFilterChange" />
                    </div>
                  </div>
                  <div class="flex min-w-0 flex-col gap-2">
                    <span class="min-w-0 break-words text-xs font-medium text-stone-600 dark:text-stone-400">
                      Сочетания (среднее)
                    </span>
                    <div class="grid w-full min-w-0 grid-cols-[minmax(0,1fr)_auto_minmax(0,1fr)] items-center gap-2">
                      <InputNumber
                        v-model="filters.reviewPairingsMin"
                        data-testid="cigar-bases-filter-review-pairings-min"
                        class="min-w-0 w-full"
                        fluid
                        input-class="min-h-11"
                        :min="1"
                        :max="10"
                        :min-fraction-digits="0"
                        :max-fraction-digits="2"
                        placeholder="от"
                        show-clear
                        @update:model-value="onFilterChange" />
                      <span class="shrink-0 self-center text-stone-400">—</span>
                      <InputNumber
                        v-model="filters.reviewPairingsMax"
                        data-testid="cigar-bases-filter-review-pairings-max"
                        class="min-w-0 w-full"
                        fluid
                        input-class="min-h-11"
                        :min="1"
                        :max="10"
                        :min-fraction-digits="0"
                        :max-fraction-digits="2"
                        placeholder="до"
                        show-clear
                        @update:model-value="onFilterChange" />
                    </div>
                  </div>
                  <div class="flex min-w-0 flex-col gap-2 sm:col-span-2 lg:col-span-1">
                    <label
                      for="cigar-bases-filter-review-min-count"
                      class="text-xs font-medium text-stone-600 dark:text-stone-400">
                      Мин. число отзывов с осями
                    </label>
                    <InputNumber
                      id="cigar-bases-filter-review-min-count"
                      v-model="filters.reviewMinScoredCount"
                      data-testid="cigar-bases-filter-review-min-count"
                      class="w-full max-w-xs"
                      input-class="min-h-11"
                      :min="0"
                      :max="9999"
                      :min-fraction-digits="0"
                      :max-fraction-digits="0"
                      show-clear
                      placeholder="например 2"
                      @update:model-value="onFilterChange" />
                  </div>
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
        </Transition>
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

      <!-- Загрузка: скелетоны в сетке как у выдачи -->
      <div
        v-if="loading"
        data-testid="cigar-bases-loading"
        class="grid min-h-[20rem] grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3 sm:gap-6"
        aria-busy="true"
        aria-live="polite">
        <Skeleton
          v-for="n in 6"
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

      <!-- Список: карточки на всех ширинах, серверная пагинация и сортировка -->
      <div v-show="!error && (loading || pagination.totalRecords > 0)">
        <div
          v-if="!loading && pagination.totalRecords > 0"
          class="mb-5 flex flex-col gap-4 sm:mb-6 lg:flex-row lg:items-end lg:justify-between"
          data-testid="cigar-bases-toolbar">
          <p
            class="text-sm text-stone-600 dark:text-stone-400"
            data-testid="cigar-bases-summary">
            Показано {{ pagination.first + 1 }}-{{
              Math.min(pagination.first + pagination.rows, pagination.totalRecords)
            }}
            из
            {{ pagination.totalRecords }}
          </p>
          <div class="flex w-full flex-col gap-3 sm:flex-row sm:flex-wrap sm:items-end lg:w-auto">
            <div class="min-w-0 flex-1 sm:flex-initial sm:min-w-[12rem]">
              <label
                for="cigar-bases-sort-field"
                class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
                Сортировка
              </label>
              <Select
                id="cigar-bases-sort-field"
                v-model="sortField"
                data-testid="cigar-bases-sort-field"
                :options="sortFieldOptions"
                option-label="label"
                option-value="value"
                class="w-full min-h-12 sm:min-h-11"
                :show-clear="false"
                @change="onSortChange" />
            </div>
            <div class="min-w-0 flex-1 sm:flex-initial sm:min-w-[11rem]">
              <label
                for="cigar-bases-sort-order"
                class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
                Порядок
              </label>
              <Select
                id="cigar-bases-sort-order"
                v-model="sortOrder"
                data-testid="cigar-bases-sort-order"
                :options="sortOrderOptions"
                option-label="label"
                option-value="value"
                class="w-full min-h-12 sm:min-h-11"
                :show-clear="false"
                @change="onSortChange" />
            </div>
          </div>
        </div>

        <div
          v-if="!loading"
          class="grid grid-cols-1 gap-5 sm:grid-cols-2 sm:gap-6 lg:grid-cols-3"
          data-testid="cigar-bases-grid">
          <article
            v-for="(cigar, index) in cigars"
            :key="cigar.id"
            v-memo="memoKey(cigar)"
            :data-testid="`cigar-base-card-${cigar.id}`"
            role="button"
            tabindex="0"
            :aria-label="`Сигара ${cigar.name}, открыть подробности`"
            class="cigar-base-card-enter relative flex min-h-[9rem] cursor-pointer flex-col overflow-hidden rounded-2xl border border-stone-200/90 bg-white/95 shadow-md shadow-stone-900/5 transition-[box-shadow,border-color] duration-200 hover:border-rose-800/25 hover:shadow-lg hover:shadow-rose-900/10 focus:outline-none focus-visible:ring-2 focus-visible:ring-rose-500/50 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 dark:hover:border-rose-900/35 dark:hover:shadow-black/70 motion-reduce:transition-none motion-reduce:animate-none"
            :style="{ animationDelay: `${Math.min(index, 8) * 40}ms` }"
            @click="viewCigar(cigar)"
            @keydown.enter.prevent="viewCigar(cigar)"
            @keydown.space.prevent="viewCigar(cigar)">
            <div class="flex gap-4 p-4">
              <div
                class="relative h-24 w-24 shrink-0 overflow-hidden rounded-xl bg-stone-100 dark:bg-stone-800 ring-1 ring-stone-200/80 dark:ring-stone-600/60">
                <img
                  v-if="cigarBaseThumbnailSrc(cigar)"
                  :src="cigarBaseThumbnailSrc(cigar)"
                  :alt="cigar.name"
                  width="96"
                  height="96"
                  class="h-full w-full object-contain"
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
                <Tag
                  v-if="cigar.isModerated === false"
                  class="mt-1"
                  severity="warn"
                  value="На модерации" />
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
                    v-if="formatVitola(cigar.lengthMm, cigar.diameter)"
                    class="rounded-full bg-stone-200/80 px-2 py-0.5 text-xs font-medium text-stone-800 dark:bg-stone-700/80 dark:text-stone-200">
                    {{ formatVitola(cigar.lengthMm, cigar.diameter) }}
                  </span>
                  <span
                    v-if="cigar.strength"
                    class="rounded-full px-2 py-0.5 text-xs font-medium"
                    :class="getStrengthBadgeClass(cigar.strength)">
                    {{ getStrengthLabel(cigar.strength) }}
                  </span>
                </div>
                <p
                  v-if="blendLine(cigar)"
                  class="mt-2 line-clamp-2 text-pretty text-xs text-stone-500 dark:text-stone-400">
                  {{ blendLine(cigar) }}
                </p>
              </div>
            </div>
            <footer
              class="mt-auto flex flex-wrap justify-end gap-2 border-t border-stone-100 bg-stone-50/90 px-2 py-2 dark:border-stone-700/80 dark:bg-stone-950/50"
              @click.stop>
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
            :rows-per-page-options="[10, 20, 50, 100]"
            template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
            @page="onPage" />
        </div>
      </div>
    </div>

    <CigarDetailDialog
      v-model:visible="showDetailDialog"
      :cigar="selectedCigar"
      :can-edit-catalog="canMutateCatalog"
      @write-review="writeReview"
      @add-to-collection="addToCollection"
      @create-similar-cigar="createSimilarCigar"
      @edit-base-cigar="editBaseCigar" />

    <CigarBaseEditDialog
      v-if="canMutateCatalog"
      v-model:visible="showEditDialog"
      :cigar="editingCigar"
      :prefill-similar="similarTemplateCigar"
      @saved="onCigarSaved" />
  </section>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted, watch } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { useLocalStorage } from '@vueuse/core';
  import { useToast } from 'primevue/usetoast';
  import cigarService from '@/services/cigarService';
  import { useAuth } from '@/services/useAuth';
  import { hasAnyRole } from '@/utils/roles';
  import type { CigarBase, CigarImage, PaginatedResult, Brand } from '@/services/cigarService';
  import type { PageState } from 'primevue/paginator';
  import CigarDetailDialog from '../components/CigarDetailDialog.vue';
  import CigarBaseEditDialog from '../components/CigarBaseEditDialog.vue';
  import InputNumber from 'primevue/inputnumber';
  import { strengthOptions } from '@/utils/cigarOptions';
  import { formatVitola } from '@/utils/vitola';
  import { arrayBufferToBase64 } from '@/utils/imageUtils';
  import { buildCatalogSimilarDraftSnapshot, CATALOG_SIMILAR_DRAFT_STORAGE_KEY } from '@/utils/catalogSimilarDraft';

  type ModerationFilterValue = 'moderated' | 'unmoderated';

  interface Filters {
    search: string;
    brand: number | null;
    strength: string | null;
    moderationFilter: ModerationFilterValue;
    /** Только записи без файла в хранилище (нет CigarImage с StoragePath). */
    noImageOnly: boolean;
    reviewBodyMin: number | null;
    reviewBodyMax: number | null;
    reviewAromaMin: number | null;
    reviewAromaMax: number | null;
    reviewPairingsMin: number | null;
    reviewPairingsMax: number | null;
    reviewMinScoredCount: number | null;
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

  const moderationFilterOptions: { label: string; value: ModerationFilterValue }[] = [
    { label: 'Только промодерированные', value: 'moderated' },
    { label: 'Только не промодерированные', value: 'unmoderated' },
  ];

  /** Значения как в `GetCigarBasesPaginated` (ASP.NET): `brandname`, не `brand.name`. */
  const sortFieldOptions: { label: string; value: string }[] = [
    { label: 'Название', value: 'name' },
    { label: 'Бренд', value: 'brandname' },
    { label: 'Размер', value: 'size' },
    { label: 'Крепость', value: 'strength' },
    { label: 'Страна', value: 'country' },
    { label: 'Ср. сила/тело по отзывам', value: 'reviewavgbodystrength' },
    { label: 'Ср. аромат по отзывам', value: 'reviewavgaromascore' },
    { label: 'Ср. сочетания по отзывам', value: 'reviewavgpairingsscore' },
    { label: 'Число отзывов с осями', value: 'reviewscoredreviewcount' },
  ];

  const sortOrderOptions: { label: string; value: 1 | -1 }[] = [
    { label: 'По возрастанию', value: 1 },
    { label: 'По убыванию', value: -1 },
  ];

  const router = useRouter();
  const route = useRoute();
  const toast = useToast();
  const { user } = useAuth();

  const canMutateCatalog = computed(() => hasAnyRole(user.value, ['Admin', 'Moderator']));

  const canFilterUnmoderated = canMutateCatalog;

  /** Состояние спойлера фильтров запоминается в браузере. */
  const filtersExpanded = useLocalStorage('cigar-bases-filters-expanded', true);

  const loading = ref<boolean>(true);
  const error = ref<string | null>(null);
  const cigars = ref<CigarBase[]>([]);
  const failedImageIds = ref<Set<number>>(new Set());
  const showDetailDialog = ref<boolean>(false);
  const selectedCigar = ref<CigarBase>();
  const showEditDialog = ref<boolean>(false);
  const editingCigar = ref<CigarBase>();
  /** Шаблон для «Создать похожую» в диалоге каталога (только staff). */
  const similarTemplateCigar = ref<CigarBase | undefined>(undefined);

  const filters = ref<Filters>({
    search: '',
    brand: null,
    strength: null,
    moderationFilter: 'moderated',
    noImageOnly: false,
    reviewBodyMin: null,
    reviewBodyMax: null,
    reviewAromaMin: null,
    reviewAromaMax: null,
    reviewPairingsMin: null,
    reviewPairingsMax: null,
    reviewMinScoredCount: null,
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
    return (
      Boolean(f.search?.trim()) ||
      f.brand != null ||
      f.strength != null ||
      f.moderationFilter === 'unmoderated' ||
      f.noImageOnly ||
      f.reviewBodyMin != null ||
      f.reviewBodyMax != null ||
      f.reviewAromaMin != null ||
      f.reviewAromaMax != null ||
      f.reviewPairingsMin != null ||
      f.reviewPairingsMax != null ||
      f.reviewMinScoredCount != null
    );
  });

  /** Байты изображения из инлайн-полей (только DB-хранилище, MinIO не заполняет их). */
  function cigarImageBytes(img: CigarImage | undefined): string | number[] | undefined {
    if (!img) {
      return undefined;
    }
    const raw = img.imageData ?? img.data;
    return raw ?? undefined;
  }

  /** Находит лучшее изображение для отображения: с байтами или по id (MinIO). */
  function primaryCigarBaseImage(cigar: CigarBase): CigarImage | undefined {
    const list = cigar.images;
    if (!list?.length) {
      return undefined;
    }
    const main = list.find((i) => i.isMain);
    return main ?? list[0];
  }

  function cigarBaseThumbnailSrc(cigar: CigarBase): string {
    const img = primaryCigarBaseImage(cigar);
    if (!img) return '';

    // Если изображение уже давало ошибку — показываем плейсхолдер
    if (img.id && failedImageIds.value.has(img.id)) return '';

    // Инлайн-байты (DB-хранилище)
    const raw = cigarImageBytes(img);
    if (raw != null && (typeof raw === 'string' ? raw.length > 0 : raw.length > 0)) {
      const b64 = arrayBufferToBase64(raw);
      return b64 ? `data:image/jpeg;base64,${b64}` : '';
    }

    // MinIO / внешнее хранилище — используем публичный API-эндпоинт
    return img.id ? `/api/cigarimages/${img.id}/thumbnail` : '';
  }

  function memoKey(cigar: CigarBase): (string | number | boolean | null | undefined)[] {
    const img = primaryCigarBaseImage(cigar);
    const imgKey = img?.id ?? 0;
    const imgFailed = img?.id ? (failedImageIds.value.has(img.id) ? 1 : 0) : 0;
    return [
      cigar.id,
      cigar.name,
      cigar.isModerated,
      cigar.brand?.name,
      cigar.country,
      cigar.lengthMm,
      cigar.diameter,
      cigar.strength,
      cigar.wrapper,
      cigar.binder,
      cigar.filler,
      imgKey,
      imgFailed,
    ];
  }

  function blendLine(cigar: CigarBase): string {
    const parts = [cigar.wrapper, cigar.binder, cigar.filler].filter(Boolean) as string[];
    return parts.join(' · ');
  }

  function resetFilters(): void {
    filters.value = {
      search: '',
      brand: null,
      strength: null,
      moderationFilter: 'moderated',
      noImageOnly: false,
      reviewBodyMin: null,
      reviewBodyMax: null,
      reviewAromaMin: null,
      reviewAromaMax: null,
      reviewPairingsMin: null,
      reviewPairingsMax: null,
      reviewMinScoredCount: null,
    };
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
      const params: Record<string, unknown> = {
        page: pagination.value.first / pagination.value.rows + 1,
        pageSize: pagination.value.rows,
        sortField: sortField.value,
        sortOrder: sortOrder.value === 1 ? 'asc' : 'desc',
        search: filters.value.search?.trim() || undefined,
        brandId: filters.value.brand,
        strength: filters.value.strength,
      };
      if (canFilterUnmoderated.value && filters.value.moderationFilter === 'unmoderated') {
        params.unmoderatedOnly = true;
      }
      if (filters.value.noImageOnly) {
        params.withoutImagesOnly = true;
      }
      const f = filters.value;
      if (f.reviewBodyMin != null) params.minReviewBody = f.reviewBodyMin;
      if (f.reviewBodyMax != null) params.maxReviewBody = f.reviewBodyMax;
      if (f.reviewAromaMin != null) params.minReviewAroma = f.reviewAromaMin;
      if (f.reviewAromaMax != null) params.maxReviewAroma = f.reviewAromaMax;
      if (f.reviewPairingsMin != null) params.minReviewPairings = f.reviewPairingsMin;
      if (f.reviewPairingsMax != null) params.maxReviewPairings = f.reviewPairingsMax;
      if (f.reviewMinScoredCount != null) params.minReviewScoredCount = f.reviewMinScoredCount;
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
      },
    });
  }

  function openNewCatalogEntryDialog(): void {
    similarTemplateCigar.value = undefined;
    showDetailDialog.value = false;
    showEditDialog.value = true;
    editingCigar.value = undefined;
  }

  function onPrimaryAddClick(): void {
    if (canMutateCatalog.value) {
      openNewCatalogEntryDialog();
    } else {
      router.push({ name: 'CigarNew' });
    }
  }

  function createSimilarCigar(cigar: CigarBase): void {
    showDetailDialog.value = false;
    if (canMutateCatalog.value) {
      editingCigar.value = undefined;
      similarTemplateCigar.value = { ...cigar };
      showEditDialog.value = true;
      return;
    }
    try {
      sessionStorage.setItem(
        CATALOG_SIMILAR_DRAFT_STORAGE_KEY,
        JSON.stringify(buildCatalogSimilarDraftSnapshot(cigar)),
      );
    } catch {
      /* storage quota / private mode */
    }
    router.push({
      name: 'CigarNew',
      query: { openNewCatalogFromSimilar: '1' },
    });
  }

  function editBaseCigar(cigar: CigarBase): void {
    similarTemplateCigar.value = undefined;
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
      query: {
        cigarBaseId: String(cigar.id),
        brandName: cigar.brand.name,
        cigarName: cigar.name,
        ...(cigar.lengthMm != null ? { lengthMm: String(cigar.lengthMm) } : {}),
        ...(cigar.diameter != null ? { diameter: String(cigar.diameter) } : {}),
      },
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

  function onPage(event: PageState): void {
    pagination.value.first = event.first;
    pagination.value.rows = event.rows;
    loadCigars();
  }

  function onSortChange(): void {
    pagination.value.first = 0;
    loadCigars();
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
      console.warn('Не удалось загрузить изображение:', target.src);
    }
    // Извлекаем ID из URL /api/cigarimages/{id}/thumbnail и помечаем как упавшее.
    // Замена Set на новый экземпляр нужна для срабатывания реактивности → v-memo инвалидируется → v-else показывает плейсхолдер.
    const match = /\/api\/cigarimages\/(\d+)\//.exec(target.src);
    if (match) {
      failedImageIds.value = new Set([...failedImageIds.value, Number(match[1])]);
    } else {
      target.style.display = 'none';
    }
  }

  async function openSelectedCigarBaseFromQuery(cigarBaseIdParam: unknown): Promise<void> {
    const cigarBaseId = Number(cigarBaseIdParam);
    if (!Number.isFinite(cigarBaseId) || cigarBaseId <= 0) return;

    const clearSelectionQuery = async () => {
      const nextQuery = { ...route.query };
      delete nextQuery.selectedCigarBaseId;
      await router.replace({ query: nextQuery });
    };

    try {
      const localCigar = cigars.value.find((c) => c.id === cigarBaseId);
      const cigar = localCigar ?? (await cigarService.getCigarBase(cigarBaseId));
      viewCigar(cigar);
    } catch {
      toast.add({
        severity: 'warn',
        summary: 'Сигара не найдена',
        detail: 'Не удалось открыть выбранную сигару из базы',
        life: 3000,
      });
    } finally {
      await clearSelectionQuery();
    }
  }

  onMounted(() => {
    void loadCigars().then(() => openSelectedCigarBaseFromQuery(route.query.selectedCigarBaseId));
    void loadBrands();
  });

  watch(
    () => route.query.selectedCigarBaseId,
    (value) => {
      if (value) {
        void openSelectedCigarBaseFromQuery(value);
      }
    },
  );

  watch(canFilterUnmoderated, (allowed) => {
    if (!allowed && filters.value.moderationFilter === 'unmoderated') {
      filters.value.moderationFilter = 'moderated';
      pagination.value.first = 0;
      void loadCigars();
    }
  });

  watch(showEditDialog, (open) => {
    if (!open) {
      similarTemplateCigar.value = undefined;
    }
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

  /*:global(.dark) .cigar-bases-grain {
    mix-blend-mode: soft-light;
  }*/

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

  .cb-filters-panel-enter-active,
  .cb-filters-panel-leave-active {
    transition:
      opacity 0.2s ease,
      transform 0.2s ease;
  }

  .cb-filters-panel-enter-from,
  .cb-filters-panel-leave-to {
    opacity: 0;
    transform: translateY(-6px);
  }

  @media (prefers-reduced-motion: reduce) {
    .cb-filters-panel-enter-active,
    .cb-filters-panel-leave-active {
      transition: none;
    }

    .cb-filters-panel-enter-from,
    .cb-filters-panel-leave-to {
      transform: none;
    }
  }
</style>
