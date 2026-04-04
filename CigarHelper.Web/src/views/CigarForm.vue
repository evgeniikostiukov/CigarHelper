<template>
  <section
    class="cigar-form-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-50 via-rose-50/40 to-stone-50 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-rose-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="cigar-form"
    aria-labelledby="cigar-form-heading">
    <div
      class="cigar-form-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto max-w-4xl">
      <header class="flex flex-col gap-4 pb-6 sm:flex-row sm:items-end sm:justify-between sm:pb-8">
        <div class="min-w-0">
          <p
            class="mb-1.5 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55">
            Справочник → коллекция
          </p>
          <h1
            id="cigar-form-heading"
            class="text-balance text-3xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 sm:text-4xl">
            Добавить из базы
          </h1>
          <p class="mt-1.5 max-w-xl text-pretty text-sm text-stone-600 dark:text-stone-400">
            Выберите модерированную сигару из справочника, укажите цену, оценку, заметки о вкусе и аромате, при
            необходимости фото и хьюмидор — запись попадёт в вашу коллекцию.
          </p>
        </div>
        <Button
          data-testid="cigar-form-back"
          class="min-h-12 w-full shrink-0 touch-manipulation sm:min-h-11 sm:w-auto"
          icon="pi pi-arrow-left"
          label="К списку сигар"
          severity="secondary"
          outlined
          @click="router.push({ name: 'CigarList' })" />
      </header>

      <div
        class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/40 sm:p-6 cigar-form-enter">
        <Message
          v-if="saveError"
          data-testid="cigar-form-save-error"
          class="mb-6"
          severity="error"
          :closable="false">
          {{ saveError }}
        </Message>

        <form
          data-testid="cigar-form-fields"
          class="flex flex-col gap-6 sm:gap-8"
          @submit.prevent="handleSubmit">
          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-4 text-lg font-semibold text-stone-900 dark:text-rose-50/95">Сигара из справочника</h2>
            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <div class="flex flex-col gap-2 md:col-span-2">
                <label
                  for="name"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Название <span class="text-red-600 dark:text-red-400">*</span>
                </label>
                <AutoComplete
                  id="name"
                  data-testid="cigar-form-name"
                  :model-value="selectedBase"
                  :suggestions="filteredCigars"
                  class="w-full"
                  input-class="min-h-11 w-full"
                  :class="{ 'p-invalid': errors.base }"
                  data-key="id"
                  option-label="name"
                  option-group-label="brand"
                  option-group-children="cigars"
                  :dropdown="true"
                  :virtual-scroller-options="{ itemSize: 50 }"
                  :loading="searchLoading"
                  :delay="300"
                  :min-length="0"
                  placeholder="Выберите сигару из справочника"
                  @update:model-value="onCigarAutocompleteModelUpdate"
                  @complete="searchCigars">
                  <template #optiongroup="slotProps">
                    <div class="p-2 font-semibold text-stone-800 dark:text-stone-200">
                      {{ slotProps.option.brand }}
                    </div>
                  </template>
                  <template #option="slotProps">
                    <div class="flex items-center">
                      <div>
                        <div class="font-semibold text-stone-900 dark:text-stone-100">
                          {{ slotProps.option.name }}
                        </div>
                        <div class="text-xs text-stone-500 dark:text-stone-400">
                          <span class="mr-2">{{ slotProps.option.brand.name }}</span>
                          <span
                            v-if="slotProps.option.size"
                            class="mr-2"
                            >{{ slotProps.option.size }}</span
                          >
                          <span v-if="slotProps.option.strength">{{
                            getStrengthLabel(slotProps.option.strength)
                          }}</span>
                        </div>
                      </div>
                    </div>
                  </template>
                  <template #empty>
                    <div class="p-2 text-stone-500 dark:text-stone-400">
                      {{ searchLoading ? 'Поиск…' : 'Сигары не найдены.' }}
                    </div>
                  </template>
                </AutoComplete>
                <small
                  v-if="errors.base"
                  class="text-sm text-red-600 dark:text-red-400"
                  >{{ errors.base }}</small
                >
              </div>

              <div class="flex flex-col gap-2 md:col-span-2">
                <label
                  for="cigar-form-brand-readonly"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Бренд
                </label>
                <InputText
                  id="cigar-form-brand-readonly"
                  data-testid="cigar-form-brand"
                  class="min-h-11 w-full"
                  readonly
                  :model-value="selectedBase?.brand?.name ?? ''"
                  placeholder="Появится после выбора сигары" />
              </div>

              <div class="flex flex-col gap-2 md:col-span-2">
                <label
                  for="price"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Цена (₽)
                </label>
                <InputNumber
                  id="price"
                  v-model="form.price"
                  data-testid="cigar-form-price"
                  class="flex! w-full"
                  input-class="min-h-11"
                  :min-fraction-digits="2"
                  :max-fraction-digits="2"
                  placeholder="0.00"
                  suffix=" ₽"
                  fluid />
              </div>

              <div class="flex flex-col gap-2 md:col-span-2">
                <label
                  for="cigar-form-rating"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Оценка
                </label>
                <div class="flex flex-col gap-2 sm:flex-row sm:flex-wrap sm:items-center sm:gap-4">
                  <Rating
                    id="cigar-form-rating"
                    v-model="form.rating"
                    data-testid="cigar-form-rating"
                    :stars="10" />
                  <span class="text-sm text-stone-600 dark:text-stone-400">
                    {{ form.rating != null ? `${form.rating}/10` : 'Без оценки' }}
                  </span>
                </div>
                <small class="text-stone-500 dark:text-stone-400">
                  Повторный клик по выбранной звезде снимает оценку.
                </small>
              </div>

              <div class="flex flex-col gap-2">
                <label
                  for="taste"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Вкус
                </label>
                <InputText
                  id="taste"
                  v-model="form.taste"
                  data-testid="cigar-form-taste"
                  class="min-h-11 w-full"
                  placeholder="Ваши заметки о вкусе" />
              </div>

              <div class="flex flex-col gap-2">
                <label
                  for="aroma"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Аромат
                </label>
                <InputText
                  id="aroma"
                  v-model="form.aroma"
                  data-testid="cigar-form-aroma"
                  class="min-h-11 w-full"
                  placeholder="Ваши заметки об аромате" />
              </div>
            </div>
          </div>

          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-4 text-lg font-semibold text-stone-900 dark:text-rose-50/95">Фото</h2>
            <div class="flex flex-col gap-6 lg:grid lg:grid-cols-[minmax(0,14rem)_1fr] lg:items-start lg:gap-8">
              <div class="flex flex-col gap-2 lg:sticky lg:top-4">
                <span class="text-xs font-medium text-stone-600 dark:text-stone-400">Предпросмотр</span>
                <div
                  class="cigar-form-image-frame relative aspect-[4/5] w-full max-w-[14rem] min-h-0 overflow-hidden rounded-xl border border-stone-200/90 bg-stone-100/90 sm:aspect-square dark:border-stone-600/80 dark:bg-stone-900/60"
                  data-testid="cigar-form-image-preview">
                  <div
                    class="cigar-form-image-frame-inner absolute inset-0 box-border flex min-h-0 min-w-0 items-center justify-center p-2">
                    <img
                      v-if="previewUrl"
                      :src="previewUrl"
                      alt=""
                      loading="lazy"
                      decoding="async" />
                    <div
                      v-else
                      class="flex flex-col items-center gap-2 px-3 py-6 text-center text-stone-400 dark:text-stone-500">
                      <i
                        class="pi pi-image text-3xl opacity-70"
                        aria-hidden="true" />
                      <span class="text-xs leading-snug">Добавьте ссылки на фото ниже</span>
                    </div>
                  </div>
                </div>
              </div>

              <div class="flex min-w-0 flex-col gap-4">
                <div
                  class="flex flex-col gap-2"
                  data-testid="cigar-form-image-urls">
                  <span class="text-xs font-medium text-stone-600 dark:text-stone-400">Ссылки на фото</span>
                  <div
                    v-for="(_u, idx) in form.imageUrls"
                    :key="'img-url-' + idx"
                    class="flex items-center gap-2">
                    <InputText
                      v-model="form.imageUrls[idx]"
                      class="min-h-11 min-w-0 flex-1"
                      :data-testid="idx === 0 ? 'cigar-form-image-url' : `cigar-form-image-url-${idx}`"
                      placeholder="https://example.com/cigar.jpg" />
                    <Button
                      type="button"
                      class="min-h-11 min-w-11 shrink-0 touch-manipulation"
                      icon="pi pi-trash"
                      text
                      rounded
                      severity="secondary"
                      aria-label="Удалить строку"
                      @click="removeImageUrlRow(idx)" />
                  </div>
                  <Button
                    type="button"
                    class="min-h-11 w-full touch-manipulation sm:w-auto"
                    label="Добавить ссылку"
                    icon="pi pi-plus"
                    severity="secondary"
                    outlined
                    data-testid="cigar-form-add-image-url"
                    :disabled="form.imageUrls.length >= maxNewImageUrls"
                    @click="addImageUrlRow" />
                  <small class="text-stone-500 dark:text-stone-400">
                    До {{ maxNewImageUrls }} ссылок; сервер скачает изображения, первое успешное — главное.
                  </small>
                </div>
              </div>
            </div>
          </div>

          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-4 text-lg font-semibold text-stone-900 dark:text-rose-50/95">Хранение</h2>
            <div class="flex flex-col gap-4">
              <div class="flex min-h-11 flex-col gap-2 touch-manipulation sm:flex-row sm:items-center">
                <Checkbox
                  id="addToHumidor"
                  v-model="form.addToHumidor"
                  data-testid="cigar-form-add-to-humidor"
                  :binary="true"
                  input-id="addToHumidor" />
                <label
                  for="addToHumidor"
                  class="cursor-pointer text-sm font-medium text-stone-800 dark:text-stone-200 sm:ml-1">
                  Добавить в хьюмидор
                </label>
              </div>
              <small class="text-stone-500 dark:text-stone-400 sm:ml-9">
                Если отмечено, выберите хьюмидор ниже. Сигара всё равно сохраняется в коллекции.
              </small>

              <div class="flex flex-col gap-2">
                <label
                  for="humidorId"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Хьюмидор
                </label>
                <Dropdown
                  id="humidorId"
                  v-model="form.humidorId"
                  data-testid="cigar-form-humidor"
                  class="w-full"
                  input-class="min-h-11"
                  :options="humidors"
                  option-label="name"
                  option-value="id"
                  placeholder="Выберите хьюмидор"
                  show-clear
                  :loading="humidorsLoading"
                  :disabled="!form.addToHumidor || humidorsLoading" />
                <p
                  v-if="form.addToHumidor && !humidorsLoading && humidors.length === 0"
                  class="text-sm text-rose-900/90 dark:text-rose-200/80">
                  Хьюмидоров пока нет.
                  <router-link
                    class="font-medium text-rose-800 underline underline-offset-2 dark:text-rose-300"
                    :to="{ name: 'HumidorList' }">
                    Создайте хьюмидор
                  </router-link>
                  .
                </p>
              </div>

              <div
                v-if="selectedHumidor"
                class="mt-1 rounded-xl border border-rose-800/15 bg-rose-50/60 p-4 dark:border-rose-200/15 dark:bg-rose-950/25">
                <h3 class="mb-2 font-semibold text-stone-900 dark:text-rose-50/95">
                  {{ selectedHumidor.name }}
                </h3>
                <div class="grid grid-cols-2 gap-3 text-sm text-stone-700 dark:text-stone-300">
                  <div>
                    <span class="font-medium text-stone-600 dark:text-stone-400">Вместимость:</span>
                    {{ selectedHumidor.capacity }} сигар
                  </div>
                  <div v-if="selectedHumidor.humidity != null">
                    <span class="font-medium text-stone-600 dark:text-stone-400">Влажность:</span>
                    {{ selectedHumidor.humidity }}%
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div
            class="mt-2 flex flex-col-reverse gap-3 border-t border-stone-200/80 pt-6 dark:border-stone-700/80 sm:flex-row sm:justify-end">
            <Button
              data-testid="cigar-form-cancel"
              type="button"
              class="min-h-12 w-full touch-manipulation sm:min-h-11 sm:w-auto"
              label="Отмена"
              severity="secondary"
              outlined
              @click="handleCancel" />
            <Button
              data-testid="cigar-form-submit"
              type="submit"
              class="min-h-12 w-full touch-manipulation shadow-md shadow-rose-900/10 dark:shadow-black/40 sm:min-h-11 sm:w-auto"
              label="Сохранить в коллекции"
              icon="pi pi-check"
              :loading="saving" />
          </div>
        </form>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted, watch } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { useToast } from 'primevue/usetoast';
  import cigarService from '@/services/cigarService';
  import humidorService from '@/services/humidorService';
  import type { CigarBase } from '@/services/cigarService';
  import type { Humidor } from '@/services/humidorService';
  import { strengthOptions } from '@/utils/cigarOptions';
  import AutoComplete, { type AutoCompleteCompleteEvent } from 'primevue/autocomplete';
  import Checkbox from 'primevue/checkbox';
  import InputText from 'primevue/inputtext';
  import InputNumber from 'primevue/inputnumber';
  import Rating from 'primevue/rating';
  import Dropdown from 'primevue/dropdown';
  import Button from 'primevue/button';
  import Message from 'primevue/message';

  interface FormData {
    price: number | null;
    rating: number | null;
    taste: string;
    aroma: string;
    humidorId: number | null;
    imageUrls: string[];
    addToHumidor: boolean;
  }

  interface CigarGroup {
    brand: string;
    cigars: CigarBase[];
  }

  interface FormErrors {
    base?: string;
  }

  const route = useRoute();
  const router = useRouter();
  const toast = useToast();

  const saving = ref(false);
  const saveError = ref<string | null>(null);

  const humidors = ref<Humidor[]>([]);
  const humidorsLoading = ref(false);
  const errors = ref<FormErrors>({});
  const filteredCigars = ref<CigarGroup[]>([]);
  const selectedBase = ref<CigarBase | null>(null);
  const searchLoading = ref<boolean>(false);
  const searchCache = ref<Map<string, CigarGroup[]>>(new Map());

  const maxNewImageUrls = 12;

  const form = ref<FormData>({
    price: null,
    rating: null,
    taste: '',
    aroma: '',
    humidorId: null,
    imageUrls: [''],
    addToHumidor: false,
  });

  const previewUrl = computed(() => {
    for (const raw of form.value.imageUrls) {
      const t = raw?.trim() ?? '';
      if (t && (/^https?:\/\//i.test(t) || t.startsWith('data:'))) {
        return t;
      }
    }
    return '';
  });

  const selectedHumidor = computed<Humidor | null>(() => {
    if (!form.value.humidorId) return null;
    return humidors.value.find((h) => h.id === form.value.humidorId) || null;
  });

  function addImageUrlRow(): void {
    if (form.value.imageUrls.length >= maxNewImageUrls) return;
    form.value.imageUrls.push('');
  }

  function removeImageUrlRow(index: number): void {
    form.value.imageUrls.splice(index, 1);
    if (form.value.imageUrls.length === 0) {
      form.value.imageUrls.push('');
    }
  }

  function collectTrimmedNewImageUrls(): string[] {
    return form.value.imageUrls.map((u) => u.trim()).filter(Boolean);
  }

  async function loadHumidors(): Promise<void> {
    humidorsLoading.value = true;
    try {
      humidors.value = await humidorService.getHumidors();
    } catch {
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось загрузить хьюмидоры',
        life: 3000,
      });
    } finally {
      humidorsLoading.value = false;
    }
  }

  function validateForm(): boolean {
    errors.value = {};
    const id = selectedBase.value?.id;
    if (id == null || id <= 0 || !selectedBase.value?.name?.trim()) {
      errors.value.base = 'Выберите сигару из справочника (нельзя вводить произвольное название).';
    }
    return Object.keys(errors.value).length === 0;
  }

  async function handleSubmit(): Promise<void> {
    if (!validateForm()) return;
    const base = selectedBase.value!;
    saving.value = true;
    saveError.value = null;
    try {
      const urls = collectTrimmedNewImageUrls();
      await cigarService.createCigar({
        cigarBaseId: base.id,
        price: form.value.price,
        humidorId: form.value.addToHumidor ? form.value.humidorId : null,
        taste: form.value.taste || null,
        aroma: form.value.aroma || null,
        rating: form.value.rating,
        imageUrls: urls.length > 0 ? urls : null,
      });
      toast.add({
        severity: 'success',
        summary: 'Готово',
        detail: 'Сигара добавлена в коллекцию',
        life: 3000,
      });
      await router.push({ name: 'CigarList' });
    } catch {
      saveError.value = 'Не удалось добавить сигару. Проверьте данные и попробуйте снова.';
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: saveError.value,
        life: 5000,
      });
    } finally {
      saving.value = false;
    }
  }

  function handleCancel(): void {
    router.push({ name: 'CigarList' });
  }

  // eslint-disable-next-line no-unused-vars -- имя rest только в generic-ограничении
  const debounce = <T extends (...args: any[]) => any>(func: T, delay: number) => {
    let timeoutId: ReturnType<typeof setTimeout> | null = null;
    return (...params: Parameters<T>): void => {
      if (timeoutId) clearTimeout(timeoutId);
      timeoutId = setTimeout(() => {
        func(...params);
        timeoutId = null;
      }, delay);
    };
  };

  const CIGAR_BASES_PAGE_SIZE = 100;
  const INITIAL_SUGGESTIONS_CACHE_KEY = '__initial__';

  function groupCigarBasesToOptions(items: CigarBase[]): CigarGroup[] {
    const grouped: Record<string, CigarGroup> = {};
    items.forEach((cigar: CigarBase) => {
      if (!cigar || typeof cigar !== 'object') return;
      const brandName = cigar.brand?.name || 'Без бренда';
      if (!grouped[brandName]) {
        grouped[brandName] = { brand: brandName, cigars: [] };
      }
      grouped[brandName].cigars.push({ ...cigar });
    });
    return Object.values(grouped);
  }

  async function performSearch(query: string): Promise<void> {
    const trimmed = (query ?? '').trim();

    if (!trimmed) {
      if (searchCache.value.has(INITIAL_SUGGESTIONS_CACHE_KEY)) {
        filteredCigars.value = searchCache.value.get(INITIAL_SUGGESTIONS_CACHE_KEY) || [];
        return;
      }
      try {
        searchLoading.value = true;
        const response = await cigarService.getCigarBasesPaginated({
          page: 1,
          pageSize: CIGAR_BASES_PAGE_SIZE,
          excludeBinaryMedia: true,
        });
        if (!response?.items?.length) {
          filteredCigars.value = [];
          return;
        }
        const result = groupCigarBasesToOptions(response.items);
        searchCache.value.set(INITIAL_SUGGESTIONS_CACHE_KEY, result);
        filteredCigars.value = result;
      } catch {
        filteredCigars.value = [];
      } finally {
        searchLoading.value = false;
      }
      return;
    }

    const cacheKey = trimmed.toLowerCase();
    if (searchCache.value.has(cacheKey)) {
      filteredCigars.value = searchCache.value.get(cacheKey) || [];
      return;
    }

    try {
      searchLoading.value = true;
      const response = await cigarService.getCigarBasesPaginated({
        search: trimmed,
        page: 1,
        pageSize: CIGAR_BASES_PAGE_SIZE,
        excludeBinaryMedia: true,
      });
      if (!response?.items) {
        filteredCigars.value = [];
        return;
      }
      const result = groupCigarBasesToOptions(response.items);
      searchCache.value.set(cacheKey, result);
      filteredCigars.value = result;
    } catch {
      filteredCigars.value = [];
    } finally {
      searchLoading.value = false;
    }
  }

  const debouncedSearch = debounce(performSearch, 300);

  function searchCigars(event: AutoCompleteCompleteEvent): void {
    debouncedSearch(event.query);
  }

  function flattenSuggestionCigars(): CigarBase[] {
    const out: CigarBase[] = [];
    for (const group of filteredCigars.value) {
      if (group?.cigars?.length) {
        out.push(...group.cigars);
      }
    }
    return out;
  }

  function findBaseByName(name: string): CigarBase | null {
    const n = name.trim().toLowerCase();
    if (!n) return null;
    return flattenSuggestionCigars().find((c) => (c.name || '').trim().toLowerCase() === n) ?? null;
  }

  function onCigarAutocompleteModelUpdate(val: unknown): void {
    if (val == null || val === '') {
      selectedBase.value = null;
      return;
    }
    if (typeof val === 'string') {
      const resolved = findBaseByName(val);
      selectedBase.value = resolved;
      return;
    }
    if (typeof val === 'object' && val !== null && 'id' in val && 'name' in val) {
      const o = val as CigarBase;
      const match = findBaseByName((o.name || '').trim());
      selectedBase.value = match ?? o;
    }
  }

  function getStrengthLabel(strength: string): string {
    if (!strength) return '';
    const option = strengthOptions.find((opt) => opt.value === strength);
    return option ? option.label : strength;
  }

  watch(
    () => form.value.addToHumidor,
    (on) => {
      if (on) {
        void loadHumidors();
      } else {
        form.value.humidorId = null;
      }
    },
  );

  async function applyQueryCigarBase(id: number): Promise<void> {
    try {
      const base = await cigarService.getCigarBase(id);
      selectedBase.value = base;
    } catch {
      toast.add({
        severity: 'warn',
        summary: 'Справочник',
        detail: 'Не удалось подгрузить сигару по ссылке.',
        life: 4000,
      });
    }
  }

  onMounted(() => {
    void loadHumidors();
    const q = route.query.cigarBaseId as string | undefined;
    if (q) {
      const id = parseInt(q, 10);
      if (!Number.isNaN(id)) {
        void applyQueryCigarBase(id);
      }
    }
  });
</script>

<style scoped>
  .cigar-form-root {
    position: relative;
    isolation: isolate;
  }

  .cigar-form-image-frame img {
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

  .cigar-form-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  .cigar-form-enter {
    animation: cigar-form-in 0.45s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes cigar-form-in {
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
    .cigar-form-enter {
      animation: none;
    }
  }
</style>
