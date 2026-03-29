<template>
  <section
    class="cigar-form-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-100 via-amber-50/40 to-stone-100 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-amber-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="cigar-form"
    aria-labelledby="cigar-form-heading">
    <div class="cigar-form-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto max-w-4xl">
      <header class="flex flex-col gap-4 pb-6 sm:flex-row sm:items-end sm:justify-between sm:pb-8">
        <div class="min-w-0">
          <p
            class="mb-1.5 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-amber-900/65 dark:text-amber-200/55">
            Коллекция
          </p>
          <h1
            id="cigar-form-heading"
            class="text-balance text-3xl font-semibold tracking-tight text-stone-900 dark:text-amber-50/95 sm:text-4xl">
            {{ isEditing ? 'Редактировать сигару' : 'Новая сигара' }}
          </h1>
          <p class="mt-1.5 max-w-xl text-pretty text-sm text-stone-600 dark:text-stone-400">
            {{
              isEditing
                ? 'Обновите поля и сохраните — данные останутся в вашей коллекции.'
                : 'Выберите сигару из справочника или введите название вручную, затем уточните детали и хранение.'
            }}
          </p>
        </div>
        <Button
          data-testid="cigar-form-back"
          class="min-h-12 w-full shrink-0 touch-manipulation sm:min-h-11 sm:w-auto"
          icon="pi pi-arrow-left"
          label="К списку"
          severity="secondary"
          outlined
          @click="router.push({ name: 'CigarList' })" />
      </header>

      <div
        v-if="isEditing && pageLoading"
        class="min-h-[16rem] space-y-5 rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/40 sm:p-6"
        data-testid="cigar-form-loading"
        aria-busy="true"
        aria-live="polite">
        <div
          v-for="n in 6"
          :key="n"
          class="flex flex-col gap-2">
          <Skeleton
            class="rounded-md"
            height="1rem"
            width="8rem" />
          <Skeleton
            class="rounded-xl border border-stone-200/60 dark:border-stone-600/60"
            height="2.75rem" />
        </div>
      </div>

      <div
        v-else-if="isEditing && loadError"
        class="max-w-2xl rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80"
        data-testid="cigar-form-error"
        role="alert">
        <Message
          severity="error"
          :closable="false">
          {{ loadError }}
        </Message>
        <Button
          data-testid="cigar-form-retry"
          class="mt-4 min-h-12 w-full touch-manipulation sm:w-auto"
          label="Повторить загрузку"
          icon="pi pi-refresh"
          severity="secondary"
          outlined
          @click="loadCigar" />
        <Button
          data-testid="cigar-form-error-back"
          class="mt-3 min-h-12 w-full touch-manipulation sm:mt-4 sm:ml-0 sm:w-auto"
          label="К списку сигар"
          icon="pi pi-list"
          severity="secondary"
          outlined
          @click="router.push({ name: 'CigarList' })" />
      </div>

      <div
        v-else
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
            <h2 class="mb-4 text-lg font-semibold text-stone-900 dark:text-amber-50/95">
              Основная информация
            </h2>
            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <div class="flex flex-col gap-2">
                <label
                  for="name"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Название <span class="text-red-600 dark:text-red-400">*</span>
                </label>
                <AutoComplete
                  id="name"
                  v-model="form.cigar"
                  data-testid="cigar-form-name"
                  :suggestions="filteredCigars"
                  class="w-full"
                  input-class="min-h-11 w-full"
                  :class="{ 'p-invalid': errors.name }"
                  field="name"
                  option-label="name"
                  option-group-label="brand"
                  option-group-children="cigars"
                  :dropdown="true"
                  :virtual-scroller-options="{ itemSize: 50 }"
                  :loading="searchLoading"
                  :delay="300"
                  :min-length="2"
                  placeholder="Введите или выберите название сигары"
                  @complete="searchCigars"
                  @option-select="handleCigarSelect"
                  @change="handleCigarNameChange">
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
                            class="mr-2">{{ slotProps.option.size }}</span>
                          <span v-if="slotProps.option.strength">{{ getStrengthLabel(slotProps.option.strength) }}</span>
                        </div>
                      </div>
                    </div>
                  </template>
                  <template #empty>
                    <div class="p-2 text-stone-500 dark:text-stone-400">
                      {{ searchLoading ? 'Поиск...' : 'Сигары не найдены. Введите имя для создания новой.' }}
                    </div>
                  </template>
                </AutoComplete>
                <small
                  v-if="errors.name"
                  class="text-sm text-red-600 dark:text-red-400">{{ errors.name }}</small>
                <div
                  v-if="selectedBrand"
                  class="mt-1 text-sm font-medium text-amber-900 dark:text-amber-200/90">
                  Бренд: {{ selectedBrand.name }}
                </div>
              </div>

              <div class="flex flex-col gap-2">
                <label
                  for="country"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Страна
                </label>
                <InputText
                  id="country"
                  v-model="form.country"
                  data-testid="cigar-form-country"
                  class="min-h-11 w-full"
                  placeholder="Например: Куба, Доминикана" />
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
            </div>
          </div>

          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-4 text-lg font-semibold text-stone-900 dark:text-amber-50/95">Структура сигары</h2>
            <div class="grid grid-cols-1 gap-6 md:grid-cols-3">
              <div class="flex flex-col gap-2">
                <label
                  for="wrapper"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Покровный лист
                </label>
                <InputText
                  id="wrapper"
                  v-model="form.wrapper"
                  data-testid="cigar-form-wrapper"
                  class="min-h-11 w-full"
                  placeholder="Например: Connecticut, Maduro" />
                <small class="text-stone-500 dark:text-stone-400">Внешний лист сигары</small>
              </div>

              <div class="flex flex-col gap-2">
                <label
                  for="binder"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Связующий лист
                </label>
                <InputText
                  id="binder"
                  v-model="form.binder"
                  data-testid="cigar-form-binder"
                  class="min-h-11 w-full"
                  placeholder="Например: Nicaraguan, Dominican" />
                <small class="text-stone-500 dark:text-stone-400">Средний лист</small>
              </div>

              <div class="flex flex-col gap-2">
                <label
                  for="filler"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Наполнитель
                </label>
                <InputText
                  id="filler"
                  v-model="form.filler"
                  data-testid="cigar-form-filler"
                  class="min-h-11 w-full"
                  placeholder="Например: Nicaraguan, Dominican, Cuban" />
                <small class="text-stone-500 dark:text-stone-400">Основной табак</small>
              </div>
            </div>
          </div>

          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-4 text-lg font-semibold text-stone-900 dark:text-amber-50/95">Характеристики</h2>
            <div class="grid grid-cols-1 gap-6 md:grid-cols-3">
              <div class="flex flex-col gap-2">
                <label
                  for="size"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Размер
                </label>
                <InputText
                  id="size"
                  v-model="form.size"
                  data-testid="cigar-form-size"
                  class="min-h-11 w-full"
                  placeholder="Например: 6x52" />
              </div>

              <div class="flex flex-col gap-2">
                <label
                  for="strength"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Крепость
                </label>
                <Dropdown
                  id="strength"
                  v-model="form.strength"
                  data-testid="cigar-form-strength"
                  class="w-full"
                  input-class="min-h-11"
                  :options="strengthOptions"
                  option-label="label"
                  option-value="value"
                  placeholder="Выберите крепость" />
              </div>

              <div class="flex flex-col gap-2">
                <label
                  for="rating"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Оценка
                </label>
                <Rating
                  id="rating"
                  v-model="form.rating"
                  data-testid="cigar-form-rating"
                  :stars="10"
                  :cancel="false"
                  class="w-full" />
                <small class="text-stone-500 dark:text-stone-400">От 1 до 10</small>
              </div>
            </div>
          </div>

          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-4 text-lg font-semibold text-stone-900 dark:text-amber-50/95">Описание и изображение</h2>
            <div class="flex flex-col gap-6">
              <div class="flex flex-col gap-2">
                <label
                  for="description"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Описание
                </label>
                <Textarea
                  id="description"
                  v-model="form.description"
                  data-testid="cigar-form-description"
                  class="min-h-[6rem] w-full"
                  rows="4"
                  placeholder="Вкус, аромат, сочетания…" />
              </div>

              <div class="flex flex-col gap-2">
                <label
                  for="imageUrl"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  URL изображения
                </label>
                <InputText
                  id="imageUrl"
                  v-model="form.imageUrl"
                  data-testid="cigar-form-image-url"
                  class="min-h-11 w-full"
                  placeholder="https://example.com/cigar-image.jpg" />
                <small class="text-stone-500 dark:text-stone-400">Ссылка на изображение сигары</small>
              </div>
            </div>
          </div>

          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-4 text-lg font-semibold text-stone-900 dark:text-amber-50/95">Хранение</h2>
            <div class="flex flex-col gap-4">
              <div
                v-if="!isEditing"
                class="flex flex-col gap-2">
                <div class="flex min-h-11 items-center gap-3 touch-manipulation">
                  <Checkbox
                    id="addToCollection"
                    v-model="form.addToCollection"
                    data-testid="cigar-form-add-to-collection"
                    :binary="true"
                    input-id="addToCollection" />
                  <label
                    for="addToCollection"
                    class="cursor-pointer text-sm font-medium text-stone-800 dark:text-stone-200">
                    Добавить сигару в мою коллекцию
                  </label>
                </div>
                <small class="ml-9 text-stone-500 dark:text-stone-400 md:ml-11">
                  Отметьте, чтобы сохранить цену, рейтинг и хьюмидор в личной коллекции.
                </small>
              </div>

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
                  placeholder="Выберите хьюмидор для хранения"
                  :disabled="!isEditing && !form.addToCollection" />
                <small class="text-stone-500 dark:text-stone-400">Оставьте пустым, если сигара не в хьюмидоре</small>
              </div>

              <div
                v-if="selectedHumidor"
                class="mt-1 rounded-xl border border-amber-800/15 bg-amber-50/60 p-4 dark:border-amber-200/15 dark:bg-amber-950/25">
                <h3 class="mb-2 font-semibold text-stone-900 dark:text-amber-50/95">
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

          <div class="mt-2 flex flex-col-reverse gap-3 border-t border-stone-200/80 pt-6 dark:border-stone-700/80 sm:flex-row sm:justify-end">
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
              class="min-h-12 w-full touch-manipulation shadow-md shadow-amber-900/10 dark:shadow-black/40 sm:min-h-11 sm:w-auto"
              :label="isEditing ? 'Сохранить изменения' : 'Добавить сигару'"
              icon="pi pi-check"
              :loading="saving" />
          </div>
        </form>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { useToast } from 'primevue/usetoast';
  import cigarService from '@/services/cigarService';
  import humidorService from '@/services/humidorService';
  import type { Cigar, CigarBase, Brand } from '@/services/cigarService';
  import type { Humidor } from '@/services/humidorService';
  import { strengthOptions } from '@/utils/cigarOptions';
  import AutoComplete, {
    type AutoCompleteCompleteEvent,
    type AutoCompleteOptionSelectEvent,
  } from 'primevue/autocomplete';
  import Checkbox from 'primevue/checkbox';
  import InputText from 'primevue/inputtext';
  import InputNumber from 'primevue/inputnumber';
  import Textarea from 'primevue/textarea';
  import Dropdown from 'primevue/dropdown';
  import Rating from 'primevue/rating';
  import Button from 'primevue/button';

  interface FormData {
    cigar: Cigar;
    country: string;
    size: string;
    strength: string | null;
    rating: number;
    price: number | null;
    description: string;
    humidorId: number | null;
    imageUrl: string;
    wrapper: string;
    binder: string;
    filler: string;
    addToCollection: boolean;
  }

  interface CigarGroup {
    brand: string;
    cigars: Cigar[];
  }

  interface FormErrors {
    name?: string;
    brandId?: string;
    [key: string]: string | undefined;
  }

  const route = useRoute();
  const router = useRouter();
  const toast = useToast();

  const pageLoading = ref(false);
  const saving = ref(false);
  const loadError = ref<string | null>(null);
  const saveError = ref<string | null>(null);

  const humidors = ref<Humidor[]>([]);
  const brands = ref<Brand[]>([]);
  const errors = ref<FormErrors>({});
  const filteredCigars = ref<CigarGroup[]>([]);
  const selectedCigar = ref<Cigar | null>(null);
  const searchLoading = ref<boolean>(false);
  const searchCache = ref<Map<string, CigarGroup[]>>(new Map());

  const form = ref<FormData>({
    cigar: {} as Cigar,
    country: '',
    size: '',
    strength: null,
    rating: 0,
    price: null,
    description: '',
    humidorId: null,
    imageUrl: '',
    wrapper: '',
    binder: '',
    filler: '',
    addToCollection: false,
  });

  const isEditing = computed<boolean>(() => !!route.params.id);

  const selectedBrand = computed<Brand | null>(() => {
    if (!form.value.cigar || !form.value.cigar?.brand?.id) return null;
    return brands.value.find((brand) => brand.id === form.value.cigar.brand.id) || null;
  });

  const selectedHumidor = computed<Humidor | null>(() => {
    if (!form.value.humidorId) return null;
    return humidors.value.find((humidor) => humidor.id === form.value.humidorId) || null;
  });

  async function setInitialBrand(brandId: number) {
    if (brands.value.length === 0) {
      await loadBrands();
    }
    const foundBrand = brands.value.find((b) => b.id === brandId);
    if (foundBrand) {
      if (!form.value.cigar) {
        form.value.cigar = {} as Cigar;
      }
      form.value.cigar.brand = foundBrand;
    }
  }

  async function loadBrands(): Promise<void> {
    try {
      brands.value = await cigarService.getBrands();
    } catch (error) {
      if (import.meta.env.DEV) {
        console.error('Ошибка загрузки брендов:', error);
      }
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось загрузить бренды',
        life: 3000,
      });
    }
  }

  async function loadHumidors(): Promise<void> {
    try {
      humidors.value = await humidorService.getHumidors();
    } catch (error) {
      if (import.meta.env.DEV) {
        console.error('Ошибка загрузки хьюмидоров:', error);
      }
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось загрузить хьюмидоры',
        life: 3000,
      });
    }
  }

  async function loadCigar(): Promise<void> {
    if (!isEditing.value) return;

    pageLoading.value = true;
    loadError.value = null;
    try {
      const cigar = await cigarService.getCigar(route.params.id as string);

      form.value = {
        cigar: {
          name: cigar.name || '',
          brand: cigar.brand,
          id: cigar.id,
          country: cigar.country || '',
          size: cigar.size || '',
          strength: cigar.strength || null,
          price: cigar.price || null,
          description: cigar.description || '',
          humidorId: cigar.humidorId || null,
          wrapper: cigar.wrapper || '',
          binder: cigar.binder || '',
          filler: cigar.filler || '',
          rating: cigar.rating ?? 0,
          images: cigar.images || [],
        },
        country: cigar.country || '',
        size: cigar.size || '',
        strength: cigar.strength || null,
        rating: cigar.rating ?? 0,
        price: cigar.price || null,
        description: cigar.description || '',
        humidorId: cigar.humidorId || null,
        imageUrl: cigar.images?.[0]?.imageData || '',
        wrapper: cigar.wrapper || '',
        binder: cigar.binder || '',
        filler: cigar.filler || '',
        addToCollection: false,
      };
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка загрузки сигары:', err);
      }
      loadError.value = 'Не удалось загрузить данные сигары.';
    } finally {
      pageLoading.value = false;
    }
  }

  function validateForm(): boolean {
    errors.value = {};

    if (!form.value.cigar?.name?.trim()) {
      errors.value.name = 'Название обязательно для заполнения';
    }

    return Object.keys(errors.value).length === 0;
  }

  async function handleSubmit(): Promise<void> {
    if (!validateForm()) return;

    saving.value = true;
    saveError.value = null;

    try {
      const cigarData: Omit<Cigar, 'id' | 'brandName'> = {
        name: form.value.cigar.name,
        brand: form.value.cigar.brand,
        country: form.value.country || null,
        description: form.value.description || null,
        strength: form.value.strength || null,
        size: form.value.size || null,
        wrapper: form.value.wrapper || null,
        binder: form.value.binder || null,
        filler: form.value.filler || null,
        images: form.value.cigar.images || [],
        price: form.value.price,
        rating: form.value.rating,
        humidorId: form.value.humidorId,
      };

      if (isEditing.value) {
        await cigarService.updateCigar(parseInt(route.params.id as string, 10), cigarData);
        toast.add({
          severity: 'success',
          summary: 'Успешно',
          detail: 'Сигара успешно обновлена',
          life: 3000,
        });
        await router.push({ name: 'CigarList' });
      } else {
        if (form.value.addToCollection) {
          await cigarService.createCigar(cigarData);
          toast.add({
            severity: 'success',
            summary: 'Успешно',
            detail: 'Сигара успешно добавлена в вашу коллекцию',
            life: 3000,
          });
          await router.push({ name: 'CigarList' });
        } else {
          toast.add({
            severity: 'info',
            summary: 'Информация',
            detail:
              'Сигара добавлена только в базу данных. Для добавления в коллекцию отметьте соответствующий флажок.',
            life: 5000,
          });
          await router.push({ name: 'CigarBases' });
        }
      }
    } catch (error) {
      if (import.meta.env.DEV) {
        console.error('Ошибка сохранения сигары:', error);
      }
      const msg = isEditing.value ? 'Не удалось обновить сигару.' : 'Не удалось добавить сигару.';
      saveError.value = `${msg} Проверьте данные и попробуйте снова.`;
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

  const debounce = <T extends (...args: any[]) => any>(func: T, delay: number): ((...args: Parameters<T>) => void) => {
    let timeoutId: ReturnType<typeof setTimeout> | null = null;
    return (...args: Parameters<T>): void => {
      if (timeoutId) clearTimeout(timeoutId);
      timeoutId = setTimeout(() => {
        func(...args);
        timeoutId = null;
      }, delay);
    };
  };

  async function performSearch(query: string): Promise<void> {
    if (!query || query.length < 2) {
      filteredCigars.value = [];
      return;
    }

    const cacheKey = query.toLowerCase();
    if (searchCache.value.has(cacheKey)) {
      filteredCigars.value = searchCache.value.get(cacheKey) || [];
      return;
    }

    try {
      searchLoading.value = true;
      const response = await cigarService.getCigarBasesPaginated({ search: query, pageSize: 500 });

      if (!response || !response.items) {
        filteredCigars.value = [];
        return;
      }

      const groupedCigars: Record<string, CigarGroup> = {};

      response.items.forEach((cigar: CigarBase) => {
        if (!cigar || typeof cigar !== 'object') return;

        const brandName = cigar.brand.name || 'Без бренда';

        if (!groupedCigars[brandName]) {
          groupedCigars[brandName] = {
            brand: brandName,
            cigars: [],
          };
        }

        const formattedCigar = { ...cigar } as Cigar;
        groupedCigars[brandName].cigars.push(formattedCigar);
      });

      const result = Object.values(groupedCigars);

      searchCache.value.set(cacheKey, result);
      filteredCigars.value = result;
    } catch (error) {
      if (import.meta.env.DEV) {
        console.error('Ошибка при поиске сигар:', error);
      }
      filteredCigars.value = [];
    } finally {
      searchLoading.value = false;
    }
  }

  const debouncedSearch = debounce(performSearch, 300);

  function searchCigars(event: AutoCompleteCompleteEvent): void {
    debouncedSearch(event.query);
  }

  function handleCigarSelect(event: AutoCompleteOptionSelectEvent): void {
    const selectedCigarData = event.value as Cigar;

    if (!selectedCigarData || typeof selectedCigarData !== 'object') {
      if (import.meta.env.DEV) {
        console.warn('Выбрана некорректная сигара:', selectedCigarData);
      }
      return;
    }

    selectedCigar.value = selectedCigarData;

    if (!isEditing.value) {
      form.value = {
        cigar: selectedCigarData,
        country: selectedCigarData.country || '',
        size: selectedCigarData.size || '',
        strength: selectedCigarData.strength || null,
        rating: 0,
        price: null,
        description: selectedCigarData.description || '',
        humidorId: form.value.humidorId,
        imageUrl: '',
        wrapper: selectedCigarData.wrapper || '',
        binder: selectedCigarData.binder || '',
        filler: selectedCigarData.filler || '',
        addToCollection: true,
      };
    }
  }

  function handleCigarNameChange(event: any): void {
    if (typeof event.value !== 'object' || !('name' in event.value)) {
      const inputValue = event.value as string;

      form.value.cigar = {
        name: inputValue,
        brand: form.value.cigar.brand,
        id: form.value.cigar.id,
        country: form.value.country || '',
        size: form.value.size || '',
        strength: form.value.strength || null,
        price: form.value.price || null,
        description: form.value.description || '',
        humidorId: form.value.humidorId || null,
        wrapper: form.value.wrapper || '',
        binder: form.value.binder || '',
        filler: form.value.filler || '',
        rating: form.value.rating || 0,
        images: form.value.cigar.images || [],
      };

      return;
    }

    const selectedCigarData = event.value as Cigar;

    if (!selectedCigarData.name) {
      form.value.cigar = {} as Cigar;
      return;
    }

    const allCigars: Cigar[] = [];
    if (Array.isArray(filteredCigars.value)) {
      filteredCigars.value.forEach((group) => {
        if (group && group.cigars && Array.isArray(group.cigars)) {
          allCigars.push(...group.cigars);
        }
      });
    }

    const matchingCigar = allCigars.find((cigar) => {
      if (!cigar || typeof cigar !== 'object') return false;

      const name = cigar.name || '';

      return name.toLowerCase() === selectedCigarData.name.toLowerCase();
    });

    if (!matchingCigar && selectedCigar.value) {
      selectedCigar.value = null;

      let cigarName = selectedCigarData.name;

      if (cigarName.includes('(')) {
        cigarName = cigarName.split('(')[0].trim();
      }

      if (Array.isArray(brands.value)) {
        const brandNames = brands.value.map((b) => b.name || '').filter((name) => name);
        for (const brandName of brandNames) {
          if (cigarName.toLowerCase().startsWith(brandName.toLowerCase() + ' ')) {
            cigarName = cigarName.substring(brandName.length + 1).trim();
            break;
          }
        }
      }

      const currentHumidorId = form.value.humidorId;

      form.value = {
        cigar: {
          name: cigarName,
          brand: form.value.cigar.brand,
          id: form.value.cigar.id,
          country: form.value.country || '',
          size: form.value.size || '',
          strength: form.value.strength || null,
          price: form.value.price || null,
          description: form.value.description || '',
          humidorId: form.value.humidorId || null,
          wrapper: form.value.wrapper || '',
          binder: form.value.binder || '',
          filler: form.value.filler || '',
          rating: form.value.rating || 0,
          images: form.value.cigar.images || [],
        },
        country: '',
        size: '',
        strength: null,
        rating: 0,
        price: null,
        description: '',
        humidorId: currentHumidorId,
        imageUrl: '',
        wrapper: '',
        binder: '',
        filler: '',
        addToCollection: form.value.addToCollection,
      };
    }
  }

  function getStrengthLabel(strength: string): string {
    if (!strength) return '';

    const option = strengthOptions.find((opt) => opt.value === strength);
    return option ? option.label : strength;
  }

  onMounted(() => {
    void loadBrands();
    void loadHumidors();
    void loadCigar();

    const initialBrandId = route.query.brandId ? parseInt(route.query.brandId as string, 10) : null;

    if (!isEditing.value && Object.keys(route.query).length > 0) {
      if (initialBrandId) {
        void setInitialBrand(initialBrandId);
      }
      const cigarObj: Partial<Cigar> = {
        name: (route.query.name as string) || '',
      };

      form.value.cigar = { ...form.value.cigar, ...cigarObj } as Cigar;

      if (route.query.country) {
        form.value.country = route.query.country as string;
      }
      if (route.query.size) {
        form.value.size = route.query.size as string;
      }
      if (route.query.strength) {
        form.value.strength = route.query.strength as string;
      }
      if (route.query.description) {
        form.value.description = route.query.description as string;
      }
      if (route.query.wrapper) {
        form.value.wrapper = route.query.wrapper as string;
      }
      if (route.query.binder) {
        form.value.binder = route.query.binder as string;
      }
      if (route.query.filler) {
        form.value.filler = route.query.filler as string;
      }

      if (route.query.cigarId) {
        form.value.addToCollection = true;
      }
    }
  });
</script>

<style scoped>
  .cigar-form-root {
    position: relative;
    isolation: isolate;
  }

  .cigar-form-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  :global(.dark) .cigar-form-grain {
    mix-blend-mode: soft-light;
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
