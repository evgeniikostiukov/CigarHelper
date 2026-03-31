<script setup lang="ts">
  import { ref, watch, nextTick } from 'vue';
  import Dialog from 'primevue/dialog';
  import InputText from 'primevue/inputtext';
  import { useGlobalSearch, type SearchResultItem } from '@/composables/useGlobalSearch';

  const search = useGlobalSearch();

  const {
    isOpen,
    query,
    loading,
    flatItems,
    hasResults,
    activeIndex,
    close,
    navigateToItem,
    moveDown,
    moveUp,
    selectActive,
  } = search;

  defineExpose({ open: search.open });

  const inputRef = ref<InstanceType<typeof InputText> | null>(null);

  watch(isOpen, async (val) => {
    if (val) {
      await nextTick();
      (inputRef.value as unknown as { $el: HTMLInputElement } | null)?.$el?.focus();
    }
  });

  const categoryLabels: Record<SearchResultItem['type'], string> = {
    cigar: 'Мои сигары',
    humidor: 'Хьюмидоры',
    cigarBase: 'База сигар',
    brand: 'Бренды',
  };

  function groupedItems(): { type: SearchResultItem['type']; items: SearchResultItem[] }[] {
    const types: SearchResultItem['type'][] = ['cigar', 'humidor', 'cigarBase', 'brand'];
    return types
      .map((t) => ({ type: t, items: flatItems.value.filter((i) => i.type === t) }))
      .filter((g) => g.items.length > 0);
  }

  function globalIndexOf(item: SearchResultItem): number {
    return flatItems.value.indexOf(item);
  }
</script>

<template>
  <Dialog
    v-model:visible="isOpen"
    modal
    :closable="false"
    :show-header="false"
    :pt="{
      root: 'global-search-dialog',
      mask: 'global-search-mask',
      content: 'global-search-content',
    }"
    class="w-full max-w-lg"
    data-testid="global-search-dialog"
    @keydown.esc="close"
    @keydown.down.prevent="moveDown"
    @keydown.up.prevent="moveUp"
    @keydown.enter.prevent="selectActive">
    <div
      class="flex flex-col gap-0 overflow-hidden rounded-2xl border border-stone-200/80 bg-white shadow-2xl shadow-stone-900/15 dark:border-stone-600/80 dark:bg-stone-900">
      <!-- Строка поиска -->
      <div class="flex items-center gap-3 border-b border-stone-200/80 px-4 py-3 dark:border-stone-700/80">
        <i
          class="pi pi-search shrink-0 text-base text-stone-400 dark:text-stone-500"
          aria-hidden="true" />
        <InputText
          ref="inputRef"
          v-model="query"
          placeholder="Поиск сигар, хьюмидоров, брендов…"
          class="flex-1 border-0 bg-transparent p-0 text-base shadow-none outline-none ring-0 focus:ring-0 dark:text-stone-100 dark:placeholder:text-stone-500"
          aria-label="Глобальный поиск"
          data-testid="global-search-input"
          unstyled
          autocomplete="off" />
        <button
          type="button"
          class="flex shrink-0 items-center gap-1 rounded-md border border-stone-200/90 bg-stone-50 px-1.5 py-0.5 text-[0.65rem] font-medium text-stone-400 transition-colors hover:bg-stone-100 dark:border-stone-600/60 dark:bg-stone-800 dark:text-stone-500 dark:hover:bg-stone-700"
          aria-label="Закрыть поиск"
          data-testid="global-search-close"
          @click="close">
          Esc
        </button>
      </div>

      <!-- Результаты -->
      <div
        class="max-h-[60vh] overflow-y-auto overscroll-contain"
        role="listbox"
        aria-label="Результаты поиска">
        <!-- Подсказка до ввода -->
        <div
          v-if="!query || query.trim().length < 2"
          class="flex flex-col items-center justify-center gap-2 px-4 py-8 text-center"
          aria-live="polite">
          <i
            class="pi pi-search text-2xl text-stone-300 dark:text-stone-600"
            aria-hidden="true" />
          <p class="text-sm text-stone-400 dark:text-stone-500">Начните вводить — минимум 2 символа</p>
          <p class="text-xs text-stone-300 dark:text-stone-600">
            <kbd class="rounded bg-stone-100 px-1 py-0.5 font-mono dark:bg-stone-800">↑↓</kbd>
            навигация &nbsp;
            <kbd class="rounded bg-stone-100 px-1 py-0.5 font-mono dark:bg-stone-800">Enter</kbd>
            перейти
          </p>
        </div>

        <!-- Загрузка -->
        <div
          v-else-if="loading"
          class="flex items-center justify-center gap-2 px-4 py-6"
          aria-live="polite"
          aria-busy="true">
          <i
            class="pi pi-spinner animate-spin text-rose-600 dark:text-rose-400"
            aria-hidden="true" />
          <span class="text-sm text-stone-500 dark:text-stone-400">Поиск…</span>
        </div>

        <!-- Нет результатов -->
        <div
          v-else-if="!hasResults && query.trim().length >= 2"
          class="flex flex-col items-center gap-2 px-4 py-8 text-center"
          aria-live="polite">
          <i
            class="pi pi-inbox text-2xl text-stone-300 dark:text-stone-600"
            aria-hidden="true" />
          <p class="text-sm text-stone-400 dark:text-stone-500">Ничего не найдено</p>
        </div>

        <!-- Сгруппированные результаты -->
        <template v-else>
          <div
            v-for="group in groupedItems()"
            :key="group.type">
            <div class="sticky top-0 bg-stone-50/95 px-4 py-1.5 backdrop-blur-sm dark:bg-stone-800/95">
              <span class="text-[0.65rem] font-semibold uppercase tracking-widest text-stone-400 dark:text-stone-500">
                {{ categoryLabels[group.type] }}
              </span>
            </div>
            <ul role="presentation">
              <li
                v-for="item in group.items"
                :key="`${item.type}-${item.id}`"
                role="option"
                :aria-selected="globalIndexOf(item) === activeIndex"
                class="flex cursor-pointer items-center gap-3 px-4 py-2.5 transition-colors"
                :class="[
                  globalIndexOf(item) === activeIndex
                    ? 'bg-rose-50 dark:bg-rose-900/30'
                    : 'hover:bg-stone-50 dark:hover:bg-stone-800/60',
                ]"
                @click="navigateToItem(item)"
                @mouseenter="activeIndex = globalIndexOf(item)">
                <i
                  :class="item.icon"
                  class="shrink-0 text-base text-rose-700/70 dark:text-rose-400/80"
                  aria-hidden="true" />
                <div class="min-w-0 flex-1">
                  <p class="truncate text-sm font-medium text-stone-800 dark:text-stone-100">
                    {{ item.label }}
                  </p>
                  <p
                    v-if="item.sub"
                    class="truncate text-xs text-stone-500 dark:text-stone-400">
                    {{ item.sub }}
                  </p>
                </div>
                <i
                  class="pi pi-arrow-right shrink-0 text-xs text-stone-300 dark:text-stone-600"
                  aria-hidden="true" />
              </li>
            </ul>
          </div>
        </template>
      </div>
    </div>
  </Dialog>
</template>
