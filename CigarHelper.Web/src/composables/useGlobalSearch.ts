import { ref, computed, watch, onMounted, onUnmounted } from 'vue';
import { useRouter } from 'vue-router';
import { globalSearch, type GlobalSearchResult } from '@/services/searchService';

export interface SearchResultItem {
  type: 'cigar' | 'humidor' | 'cigarBase' | 'brand';
  id: number;
  label: string;
  sub?: string;
  icon: string;
  routeName: string;
  routeParams?: Record<string, string | number>;
  routeQuery?: Record<string, string | number>;
}

const DEBOUNCE_MS = 300;
const MIN_QUERY_LEN = 2;

export function useGlobalSearch() {
  const router = useRouter();
  const isOpen = ref(false);
  const query = ref('');
  const loading = ref(false);
  const rawResult = ref<GlobalSearchResult | null>(null);
  const activeIndex = ref(-1);
  let debounceTimer: ReturnType<typeof setTimeout> | null = null;

  const flatItems = computed<SearchResultItem[]>(() => {
    if (!rawResult.value) return [];
    const r = rawResult.value;
    const items: SearchResultItem[] = [
      ...(r.cigars ?? []).flatMap((c) =>
        c.id == null
          ? []
          : [
              {
                type: 'cigar' as const,
                id: c.id,
                label: c.name ?? '',
                sub: (c.brandName ?? '') + (c.humidorName ? ` · ${c.humidorName}` : ''),
                icon: 'pi pi-star',
                routeName: 'CigarDetail',
                routeParams: { id: c.id },
              },
            ],
      ),
      ...(r.humidors ?? []).flatMap((h) =>
        h.id == null
          ? []
          : [
              {
                type: 'humidor' as const,
                id: h.id,
                label: h.name ?? '',
                sub: h.description ?? undefined,
                icon: 'pi pi-box',
                routeName: 'HumidorDetail',
                routeParams: { id: h.id },
              },
            ],
      ),
      ...(r.cigarBases ?? []).flatMap((cb) =>
        cb.id == null
          ? []
          : [
              {
                type: 'cigarBase' as const,
                id: cb.id,
                label: cb.name ?? '',
                sub: cb.brandName ?? undefined,
                icon: 'pi pi-database',
                routeName: 'CigarBases',
                routeParams: undefined,
                routeQuery: { selectedCigarBaseId: cb.id },
              },
            ],
      ),
      ...(r.brands ?? []).flatMap((b) =>
        b.id == null
          ? []
          : [
              {
                type: 'brand' as const,
                id: b.id,
                label: b.name ?? '',
                sub: b.country ?? undefined,
                icon: 'pi pi-tag',
                routeName: 'Brands',
                routeParams: undefined,
                routeQuery: { selectedBrandId: b.id },
              },
            ],
      ),
    ];
    return items;
  });

  const hasResults = computed(() => flatItems.value.length > 0);

  function open() {
    isOpen.value = true;
    activeIndex.value = -1;
  }

  function close() {
    isOpen.value = false;
    query.value = '';
    rawResult.value = null;
    activeIndex.value = -1;
    if (debounceTimer) clearTimeout(debounceTimer);
  }

  function toggle() {
    if (isOpen.value) close();
    else open();
  }

  function navigateToItem(item: SearchResultItem) {
    router.push({ name: item.routeName, params: item.routeParams, query: item.routeQuery });
    close();
  }

  function selectActive() {
    const item = flatItems.value[activeIndex.value];
    if (item) navigateToItem(item);
  }

  function moveDown() {
    if (flatItems.value.length === 0) return;
    activeIndex.value = (activeIndex.value + 1) % flatItems.value.length;
  }

  function moveUp() {
    if (flatItems.value.length === 0) return;
    activeIndex.value = activeIndex.value <= 0 ? flatItems.value.length - 1 : activeIndex.value - 1;
  }

  async function runSearch(q: string) {
    if (q.trim().length < MIN_QUERY_LEN) {
      rawResult.value = null;
      loading.value = false;
      return;
    }
    loading.value = true;
    try {
      rawResult.value = await globalSearch(q.trim());
    } finally {
      loading.value = false;
    }
    activeIndex.value = -1;
  }

  watch(query, (val) => {
    if (debounceTimer) clearTimeout(debounceTimer);
    if (val.trim().length < MIN_QUERY_LEN) {
      rawResult.value = null;
      loading.value = false;
      return;
    }
    loading.value = true;
    debounceTimer = setTimeout(() => runSearch(val), DEBOUNCE_MS);
  });

  function onKeydown(e: KeyboardEvent) {
    // e.code — физическая позиция клавиши, не зависит от раскладки (работает с кириллицей)
    const isK = e.code === 'KeyK' || e.key.toLowerCase() === 'k';
    if ((e.ctrlKey || e.metaKey) && isK) {
      e.preventDefault();
      toggle();
      return;
    }
    if (!isOpen.value) return;
    switch (e.key) {
      case 'Escape':
        close();
        break;
      case 'ArrowDown':
        e.preventDefault();
        moveDown();
        break;
      case 'ArrowUp':
        e.preventDefault();
        moveUp();
        break;
      case 'Enter':
        e.preventDefault();
        selectActive();
        break;
    }
  }

  onMounted(() => window.addEventListener('keydown', onKeydown));
  onUnmounted(() => window.removeEventListener('keydown', onKeydown));

  return {
    isOpen,
    query,
    loading,
    flatItems,
    hasResults,
    activeIndex,
    open,
    close,
    navigateToItem,
    moveDown,
    moveUp,
    selectActive,
  };
}
