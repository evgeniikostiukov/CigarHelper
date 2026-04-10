<template>
  <section
    class="review-form-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-50 via-rose-50/40 to-stone-50 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-rose-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="review-form"
    aria-labelledby="review-form-heading">
    <Toast />
    <div
      class="review-form-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto max-w-4xl">
      <header class="flex flex-col gap-4 pb-6 sm:flex-row sm:items-end sm:justify-between sm:pb-8">
        <div class="min-w-0">
          <p
            class="mb-1.5 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55">
            Обзоры
          </p>
          <h1
            id="review-form-heading"
            class="text-balance text-3xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 sm:text-4xl">
            {{ isEditing ? 'Редактирование обзора' : 'Новый обзор' }}
          </h1>
          <p class="mt-1.5 max-w-xl text-pretty text-sm text-stone-600 dark:text-stone-400">
            {{
              isEditing
                ? 'Правки текста, оценок и фото сохраняются для всех читателей.'
                : 'Выберите сигару, опишите впечатления и при желании прикрепите фото.'
            }}
          </p>
        </div>
        <Button
          data-testid="review-form-back"
          class="min-h-12 w-full shrink-0 touch-manipulation sm:min-h-11 sm:w-auto"
          icon="pi pi-arrow-left"
          label="К обзорам"
          severity="secondary"
          outlined
          @click="router.push({ name: 'ReviewList' })" />
      </header>

      <div
        v-if="isEditing && loading"
        class="min-h-[16rem] space-y-5 rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/40 sm:p-6"
        data-testid="review-form-loading"
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
        v-else-if="error"
        class="max-w-2xl rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80"
        data-testid="review-form-error"
        role="alert">
        <Message
          severity="error"
          :closable="false">
          {{ error }}
        </Message>
        <div class="mt-4 flex flex-col gap-3 sm:flex-row sm:flex-wrap">
          <Button
            v-if="isEditing"
            data-testid="review-form-retry"
            class="min-h-12 w-full touch-manipulation sm:w-auto"
            label="Повторить загрузку"
            icon="pi pi-refresh"
            severity="secondary"
            outlined
            @click="fetchReview(route.params.id as string)" />
          <Button
            data-testid="review-form-error-back"
            class="min-h-12 w-full touch-manipulation sm:w-auto"
            label="К списку обзоров"
            icon="pi pi-list"
            severity="secondary"
            outlined
            @click="router.push({ name: 'ReviewList' })" />
        </div>
      </div>

      <div
        v-else
        class="review-form-enter rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/40 sm:p-6">
        <Message
          v-if="saveError"
          data-testid="review-form-save-error"
          class="mb-6"
          severity="error"
          :closable="false">
          {{ saveError }}
        </Message>

        <form
          data-testid="review-form-fields"
          class="flex flex-col gap-6 sm:gap-8"
          @submit.prevent="submitForm">
          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-1 flex items-center gap-2 text-lg font-semibold text-stone-900 dark:text-rose-50/95">
              <i
                class="pi pi-info-circle text-rose-700 dark:text-rose-400"
                aria-hidden="true" />
              Общая информация
            </h2>
            <p class="mb-4 text-sm text-stone-600 dark:text-stone-400">
              Сигара, заголовок и общая оценка — обязательны для публикации.
            </p>
            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <div class="flex flex-col gap-2 md:col-span-2">
                <label
                  for="cigar-select"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Сигара <span class="text-red-600 dark:text-red-400">*</span>
                </label>
                <AutoComplete
                  id="cigar-select"
                  v-model="selectedCigar"
                  data-testid="review-form-cigar"
                  :suggestions="filteredCigars"
                  input-class="min-h-11 w-full"
                  class="w-full"
                  :class="{ 'p-invalid': validationErrors.cigarBaseId }"
                  placeholder="Введите название сигары для поиска"
                  :show-clear="true"
                  :loading="searchLoading"
                  :delay="300"
                  :min-length="2"
                  option-label="displayName"
                  option-group-label="brand"
                  option-group-children="cigars"
                  :dropdown="true"
                  :virtual-scroller-options="{ itemSize: 50 }"
                  @complete="searchCigars"
                  @option-select="handleCigarSelect">
                  <template #optiongroup="slotProps">
                    <div class="p-2 font-semibold text-stone-800 dark:text-stone-200">
                      {{ slotProps.option.brand }}
                    </div>
                  </template>
                  <template #option="slotProps">
                    <div class="flex items-center">
                      <div class="flex-1">
                        <div class="font-semibold text-stone-900 dark:text-stone-100">{{ slotProps.option.name }}</div>
                        <div class="text-xs text-stone-500 dark:text-stone-400">
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
                      <div
                        v-if="slotProps.option.isUserCigar"
                        class="ml-2 shrink-0">
                        <span
                          class="rounded-full bg-rose-100 px-2 py-1 text-xs font-medium text-rose-900 dark:bg-rose-900/50 dark:text-rose-100">
                          Моя
                        </span>
                      </div>
                    </div>
                  </template>
                  <template #empty>
                    <div class="p-2 text-stone-500 dark:text-stone-400">
                      {{ searchLoading ? 'Поиск...' : 'Сигары не найдены. Введите название для поиска.' }}
                    </div>
                  </template>
                </AutoComplete>
                <small
                  v-if="validationErrors.cigarBaseId"
                  class="text-sm text-red-600 dark:text-red-400">
                  {{ validationErrors.cigarBaseId }}
                </small>
              </div>

              <div class="flex flex-col gap-2 md:col-span-2">
                <label
                  for="title"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Заголовок обзора <span class="text-red-600 dark:text-red-400">*</span>
                </label>
                <InputText
                  id="title"
                  v-model="form.title"
                  data-testid="review-form-title"
                  placeholder="Введите заголовок обзора"
                  class="min-h-11 w-full"
                  :class="{ 'p-invalid': validationErrors.title }"
                  maxlength="200" />
                <small
                  v-if="validationErrors.title"
                  class="text-sm text-red-600 dark:text-red-400">
                  {{ validationErrors.title }}
                </small>
                <small class="text-xs text-stone-500 dark:text-stone-500">{{ form.title.length }}/200 символов</small>
              </div>

              <div class="flex flex-col gap-3 md:col-span-2">
                <label
                  for="rating-slider"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Общая оценка <span class="text-red-600 dark:text-red-400">*</span>
                </label>
                <div class="flex min-w-0 w-full flex-col gap-4 sm:flex-row sm:items-center">
                  <div class="min-w-0 w-full sm:flex-1">
                    <Slider
                      id="rating-slider"
                      v-model="form.rating"
                      data-testid="review-form-rating"
                      class="w-full touch-manipulation"
                      :min="1"
                      :max="10"
                      :step="1" />
                  </div>
                  <Tag
                    class="shrink-0 self-start sm:self-center"
                    :value="`${form.rating}/10`"
                    icon="pi pi-star-fill"
                    severity="warning" />
                </div>
                <small
                  v-if="validationErrors.rating"
                  class="text-sm text-red-600 dark:text-red-400">
                  {{ validationErrors.rating }}
                </small>
              </div>
            </div>
          </div>

          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-1 flex items-center gap-2 text-lg font-semibold text-stone-900 dark:text-rose-50/95">
              <i
                class="pi pi-sparkles text-rose-700 dark:text-rose-400"
                aria-hidden="true" />
              Детали дегустации
            </h2>
            <p class="mb-4 text-sm text-stone-600 dark:text-stone-400">Дополнительные поля по желанию.</p>
            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <div class="space-y-4">
                <div class="flex flex-col gap-2">
                  <label
                    for="smokingExperience"
                    class="text-xs font-medium text-stone-600 dark:text-stone-400">
                    Опыт курения
                  </label>
                  <InputText
                    id="smokingExperience"
                    v-model="form.smokingExperience"
                    data-testid="review-form-smoking-experience"
                    placeholder="Например: 2 года, новичок"
                    class="min-h-11 w-full"
                    maxlength="50" />
                </div>
                <div class="flex flex-col gap-2">
                  <label
                    for="venue"
                    class="text-xs font-medium text-stone-600 dark:text-stone-400">
                    Место дегустации
                  </label>
                  <InputText
                    id="venue"
                    v-model="form.venue"
                    data-testid="review-form-venue"
                    placeholder="Например: дома, в клубе"
                    class="min-h-11 w-full"
                    maxlength="100" />
                </div>
                <div class="flex flex-col gap-2">
                  <label
                    for="smokingDate"
                    class="text-xs font-medium text-stone-600 dark:text-stone-400">
                    Дата дегустации
                  </label>
                  <Calendar
                    id="smokingDate"
                    v-model="form.smokingDate"
                    data-testid="review-form-smoking-date"
                    date-format="dd/mm/yy"
                    placeholder="Выберите дату"
                    class="w-full"
                    input-class="min-h-11 w-full" />
                </div>
              </div>
              <div class="space-y-4">
                <div class="flex flex-col gap-2">
                  <label
                    for="aroma"
                    class="text-xs font-medium text-stone-600 dark:text-stone-400">
                    Аромат
                  </label>
                  <InputText
                    id="aroma"
                    v-model="form.aroma"
                    data-testid="review-form-aroma"
                    placeholder="Например: древесный, пряный"
                    class="min-h-11 w-full"
                    maxlength="50" />
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
                    data-testid="review-form-taste"
                    placeholder="Например: сладкий, горький"
                    class="min-h-11 w-full"
                    maxlength="50" />
                </div>
                <div class="space-y-3">
                  <h3 class="text-sm font-semibold text-stone-800 dark:text-stone-200">Детальные оценки</h3>
                  <div class="space-y-3">
                    <div>
                      <label class="mb-1 block text-xs text-stone-600 dark:text-stone-400">
                        Конструкция: {{ form.construction ?? '?' }}/5
                      </label>
                      <Slider
                        v-model="form.construction"
                        data-testid="review-form-construction"
                        :min="1"
                        :max="5"
                        :step="1"
                        class="w-full touch-manipulation" />
                    </div>
                    <div>
                      <label class="mb-1 block text-xs text-stone-600 dark:text-stone-400">
                        Горение: {{ form.burnQuality ?? '?' }}/5
                      </label>
                      <Slider
                        v-model="form.burnQuality"
                        data-testid="review-form-burn"
                        :min="1"
                        :max="5"
                        :step="1"
                        class="w-full touch-manipulation" />
                    </div>
                    <div>
                      <label class="mb-1 block text-xs text-stone-600 dark:text-stone-400"
                        >Тяга: {{ form.draw ?? '?' }}/5</label
                      >
                      <Slider
                        v-model="form.draw"
                        data-testid="review-form-draw"
                        :min="1"
                        :max="5"
                        :step="1"
                        class="w-full touch-manipulation" />
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-1 flex items-center gap-2 text-lg font-semibold text-stone-900 dark:text-rose-50/95">
              <i
                class="pi pi-file-edit text-rose-700 dark:text-rose-400"
                aria-hidden="true" />
              Содержание обзора
              <span class="text-red-600 dark:text-red-400">*</span>
            </h2>
            <p class="mb-4 text-sm text-stone-600 dark:text-stone-400">
              Основной текст: впечатления, нюансы, с чем сочетали.
            </p>
            <div data-testid="review-form-content">
              <TextEditor v-model="form.content" />
            </div>
            <small
              v-if="validationErrors.content"
              class="mt-2 block text-sm text-red-600 dark:text-red-400">
              {{ validationErrors.content }}
            </small>
          </div>

          <FormImageGallerySection
            v-model="form.images"
            variant="section"
            title="Изображения"
            description="До пяти фото: файл или ссылка; подпись к кадру по желанию."
            show-captions
            url-entry-mode="multi"
            url-rows-test-id="review-form-image-urls"
            test-id="review-form-images"
            url-input-id="review-image-url"
            url-field-test-id="review-form-image-url"
            caption-test-id-prefix="review-form"
            url-help-text="В обзоре не больше пяти снимков. Ссылки сначала добавьте в галерею кнопкой ниже."
            url-help-detail="Можно ввести до двенадцати ссылок по очереди; в галерее останется не больше пяти кадров. Порядок на превью сохраняется при сохранении."
            grid-class="grid grid-cols-2 gap-3 sm:grid-cols-3 sm:gap-4 md:grid-cols-4" />

          <div
            class="flex flex-col gap-3 border-t border-stone-200/80 pt-6 dark:border-stone-700/80 sm:flex-row sm:items-center sm:justify-between">
            <Button
              data-testid="review-form-cancel"
              class="min-h-12 w-full touch-manipulation sm:order-1 sm:w-auto"
              label="Отмена"
              icon="pi pi-times"
              severity="secondary"
              outlined
              type="button"
              @click="router.push({ name: 'ReviewList' })" />
            <Button
              data-testid="review-form-submit"
              class="min-h-12 w-full touch-manipulation shadow-md shadow-rose-900/10 dark:shadow-black/40 sm:order-2 sm:w-auto"
              :label="isEditing ? 'Сохранить изменения' : 'Опубликовать обзор'"
              :icon="isEditing ? 'pi pi-check' : 'pi pi-send'"
              :loading="saving"
              type="submit" />
          </div>
        </form>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, reactive, computed, watch, onMounted } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import api from '../services/api';
  import TextEditor from '../components/TextEditor.vue';
  import FormImageGallerySection, { type FormGalleryImageItem } from '../components/FormImageGallerySection.vue';
  import AutoComplete, {
    type AutoCompleteCompleteEvent,
    type AutoCompleteOptionSelectEvent,
  } from 'primevue/autocomplete';
  import Slider from 'primevue/slider';
  import Calendar from 'primevue/calendar';
  import InputText from 'primevue/inputtext';
  import Button from 'primevue/button';
  import Toast from 'primevue/toast';
  import { useToast } from 'primevue/usetoast';
  import cigarService from '@/services/cigarService';
  import type { Brand } from '@/services/cigarService';

  /** Единый вариант выбора: каталог (CigarBase) или запись коллекции (UserCigar + CigarBaseId). */
  interface ReviewCigarOption {
    cigarBaseId: number;
    userCigarId: number | null;
    isUserCigar: boolean;
    displayName: string;
    name: string;
    brand: Brand;
    size?: string | null;
    strength?: string | null;
    country?: string | null;
  }

  interface ReviewFormModel {
    cigarBaseId: number | null;
    userCigarId: number | null;
    title: string;
    rating: number;
    content: string;
    smokingExperience: string;
    aroma: string;
    taste: string;
    construction: number;
    burnQuality: number;
    draw: number;
    venue: string;
    smokingDate: Date | null;
    images: FormGalleryImageItem[];
  }

  interface GroupedCigar {
    brand: string;
    cigars: ReviewCigarOption[];
  }

  interface ValidationErrors {
    [key: string]: string;
  }

  interface ReviewImageApiDto {
    id: number;
    /** Base64 от ASP.NET для byte[] */
    imageBytes?: string;
    caption?: string;
  }

  interface ReviewResponse {
    cigarBaseId: number;
    userCigarId?: number | null;
    cigarName: string;
    cigarBrand: string;
    title: string;
    rating: number;
    content: string;
    smokingExperience?: string;
    aroma?: string;
    taste?: string;
    construction?: number;
    burnQuality?: number;
    draw?: number;
    venue?: string;
    smokingDate?: string;
    images: ReviewImageApiDto[];
  }

  const route = useRoute();
  const router = useRouter();
  const toast = useToast();

  const isEditing = computed(() => Boolean(route.params.id));
  const loading = ref(Boolean(route.params.id));
  const error = ref<string | null>(null);
  const saveError = ref<string | null>(null);
  const saving = ref(false);
  const validationErrors = reactive<ValidationErrors>({});
  const filteredCigars = ref<GroupedCigar[]>([]);
  const selectedCigar = ref<ReviewCigarOption | null>(null);
  const searchLoading = ref(false);

  const form = reactive<ReviewFormModel>({
    cigarBaseId: null,
    userCigarId: null,
    title: '',
    rating: 5,
    content: '',
    smokingExperience: '',
    aroma: '',
    taste: '',
    construction: 3,
    burnQuality: 3,
    draw: 3,
    venue: '',
    smokingDate: new Date(),
    images: [],
  });

  const handleQueryParameters = (): void => {
    const query = route.query;

    if (query.cigarBaseId && query.cigarName) {
      const cigarBaseId = parseInt(query.cigarBaseId as string, 10);
      if (!Number.isFinite(cigarBaseId)) {
        return;
      }
      const brandName = (query.brandName as string) || 'Неизвестный бренд';

      const tempCigar: ReviewCigarOption = {
        cigarBaseId,
        userCigarId: null,
        isUserCigar: false,
        name: query.cigarName as string,
        brand: {
          id: 0,
          name: brandName,
          isModerated: true,
          createdAt: new Date().toISOString(),
        },
        country: (query.country as string) || '',
        size: (query.size as string) || '',
        strength: (query.strength as string) || '',
        displayName: `${brandName} ${query.cigarName as string}`,
      };

      selectedCigar.value = tempCigar;
      form.cigarBaseId = cigarBaseId;
      form.userCigarId = null;
      form.title = `Обзор: ${query.cigarName as string}`;

      let cigarInfo = `**Сигара:** ${query.cigarName as string}\n`;
      if (query.brandName) cigarInfo += `**Бренд:** ${query.brandName as string}\n`;
      if (query.country) cigarInfo += `**Страна:** ${query.country as string}\n`;
      if (query.size) cigarInfo += `**Размер:** ${query.size as string}\n`;
      if (query.strength) cigarInfo += `**Крепость:** ${getStrengthLabel(query.strength as string)}\n`;
      if (query.description) cigarInfo += `**Описание:** ${query.description as string}\n`;
      if (query.wrapper) cigarInfo += `**Покровный лист:** ${query.wrapper as string}\n`;
      if (query.binder) cigarInfo += `**Связующий лист:** ${query.binder as string}\n`;
      if (query.filler) cigarInfo += `**Наполнитель:** ${query.filler as string}\n`;

      cigarInfo += '\n---\n\n';
      form.content = cigarInfo;
    }
  };

  const fetchReview = async (id: string): Promise<void> => {
    loading.value = true;
    error.value = null;

    try {
      const { data: review } = await api.get<ReviewResponse>(`/reviews/${id}`);

      const cigarBrandName = review.cigarBrand || 'Неизвестный бренд';
      const cigarData: ReviewCigarOption = {
        cigarBaseId: review.cigarBaseId,
        userCigarId: review.userCigarId ?? null,
        isUserCigar: Boolean(review.userCigarId),
        name: review.cigarName || 'Неизвестная сигара',
        brand: {
          id: 0,
          name: cigarBrandName,
          isModerated: true,
          createdAt: new Date().toISOString(),
        },
        country: null,
        size: null,
        strength: null,
        displayName: `${cigarBrandName} ${review.cigarName || 'Неизвестная сигара'}`,
      };

      selectedCigar.value = cigarData;
      form.cigarBaseId = review.cigarBaseId;
      form.userCigarId = review.userCigarId ?? null;
      form.title = review.title;
      form.rating = review.rating;
      form.content = review.content;
      form.smokingExperience = review.smokingExperience || '';
      form.aroma = review.aroma || '';
      form.taste = review.taste || '';
      form.construction = review.construction || 3;
      form.burnQuality = review.burnQuality || 3;
      form.draw = review.draw || 3;
      form.venue = review.venue || '';
      form.smokingDate = review.smokingDate ? new Date(review.smokingDate) : null;
      form.images = review.images.map((img) => {
        const b64 = img.imageBytes?.trim() ?? '';
        return {
          id: img.id,
          preview: b64 ? `data:image/jpeg;base64,${b64}` : '',
          caption: img.caption ?? '',
          isExisting: true,
          markedForDeletion: false,
        };
      });
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка при загрузке обзора:', err);
      }
      error.value = 'Не удалось загрузить обзор. Возможно, он был удален или у вас нет к нему доступа.';
    } finally {
      loading.value = false;
    }
  };

  function readFileAsDataUrl(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = () => reject(reader.error ?? new Error('FileReader'));
      reader.readAsDataURL(file);
    });
  }

  async function buildImagePayloadsForCreate(
    items: FormGalleryImageItem[],
  ): Promise<{ imageUrl: string; caption: string }[]> {
    const out: { imageUrl: string; caption: string }[] = [];
    for (const img of items) {
      if (img.markedForDeletion) continue;
      let url = img.imageUrl?.trim();
      if (img.file) {
        url = await readFileAsDataUrl(img.file);
      }
      if (!url) continue;
      out.push({ imageUrl: url, caption: img.caption.trim() || '' });
    }
    return out;
  }

  async function buildImagesToAddForUpdate(
    items: FormGalleryImageItem[],
  ): Promise<{ imageUrl: string; caption: string }[]> {
    const out: { imageUrl: string; caption: string }[] = [];
    for (const img of items) {
      if (img.markedForDeletion || img.isExisting) continue;
      let url = img.imageUrl?.trim();
      if (img.file) {
        url = await readFileAsDataUrl(img.file);
      }
      if (!url) continue;
      out.push({ imageUrl: url, caption: img.caption.trim() || '' });
    }
    return out;
  }

  const validateForm = (): boolean => {
    Object.keys(validationErrors).forEach((key) => delete validationErrors[key]);
    let isValid = true;

    if (form.cigarBaseId == null) {
      validationErrors.cigarBaseId = 'Необходимо выбрать сигару';
      isValid = false;
    }
    if (!form.title) {
      validationErrors.title = 'Заголовок обзора обязателен';
      isValid = false;
    } else if (form.title.length < 3) {
      validationErrors.title = 'Заголовок должен содержать минимум 3 символа';
      isValid = false;
    }
    if (!form.rating || form.rating < 1 || form.rating > 10) {
      validationErrors.rating = 'Оценка должна быть от 1 до 10';
      isValid = false;
    }
    if (!form.content || form.content.trim() === '') {
      validationErrors.content = 'Содержание обзора обязательно';
      isValid = false;
    }
    return isValid;
  };

  const submitForm = async (): Promise<void> => {
    if (!validateForm()) {
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }

    saving.value = true;
    saveError.value = null;

    try {
      const commonFields = {
        title: form.title,
        rating: form.rating,
        content: form.content,
        smokingExperience: form.smokingExperience || null,
        aroma: form.aroma || null,
        taste: form.taste || null,
        construction: form.construction || null,
        burnQuality: form.burnQuality || null,
        draw: form.draw || null,
        venue: form.venue || null,
        smokingDate: form.smokingDate ? form.smokingDate.toISOString() : null,
      };

      if (isEditing.value && route.params.id) {
        const reviewId = route.params.id as string;

        const keptImageIds = form.images
          .filter((img) => img.isExisting && !img.markedForDeletion && typeof img.id === 'number')
          .map((img) => img.id as number);

        const imagesToAdd = await buildImagesToAddForUpdate(form.images);

        const { data: originalReview } = await api.get<ReviewResponse>(`/reviews/${reviewId}`);
        const imageIdsToRemove = originalReview.images
          .filter((img) => !keptImageIds.includes(img.id))
          .map((img) => img.id);

        const updateData = { ...commonFields, imagesToAdd, imageIdsToRemove };
        await api.put(`/reviews/${reviewId}`, updateData);
        await router.push({ name: 'ReviewDetail', params: { id: reviewId } });
      } else {
        const images = await buildImagePayloadsForCreate(form.images);
        const newReviewData = {
          ...commonFields,
          cigarBaseId: form.cigarBaseId!,
          userCigarId: form.userCigarId,
          images,
        };
        const response = await api.post<{ id: number }>('/reviews', newReviewData);
        await router.push({ name: 'ReviewDetail', params: { id: String(response.data.id) } });
      }
    } catch (err: unknown) {
      if (import.meta.env.DEV) {
        console.error('Ошибка при сохранении обзора:', err);
      }
      const axiosErr = err as { response?: { status?: number; data?: { errors?: ValidationErrors } } };
      if (axiosErr.response?.status === 400 && axiosErr.response.data?.errors) {
        Object.assign(validationErrors, axiosErr.response.data.errors);
        window.scrollTo({ top: 0, behavior: 'smooth' });
      } else {
        saveError.value = 'Не удалось сохранить обзор. Пожалуйста, проверьте введенные данные и попробуйте снова.';
      }
    } finally {
      saving.value = false;
    }
  };

  async function searchCigars(event: AutoCompleteCompleteEvent): Promise<void> {
    searchLoading.value = true;
    try {
      const [baseResult, userCigars] = await Promise.all([
        cigarService.getCigarBasesPaginated({ search: event.query, pageSize: 10 }),
        cigarService.getCigars({ name: event.query, pageSize: 10 }),
      ]);

      const collectionBaseIds = new Set<number>();
      const uniqueCigars: ReviewCigarOption[] = [];

      userCigars.forEach((cigar) => {
        const cbId = cigar.cigarBaseId;
        if (cbId == null) {
          return;
        }
        collectionBaseIds.add(cbId);
        uniqueCigars.push({
          cigarBaseId: cbId,
          userCigarId: cigar.id,
          isUserCigar: true,
          displayName: `${cigar.name} (${cigar.brand.name})`,
          name: cigar.name,
          brand: cigar.brand,
          size: cigar.size,
          strength: cigar.strength,
          country: cigar.country,
        });
      });

      baseResult.items.forEach((cigarBase) => {
        if (collectionBaseIds.has(cigarBase.id)) {
          return;
        }
        uniqueCigars.push({
          cigarBaseId: cigarBase.id,
          userCigarId: null,
          isUserCigar: false,
          displayName: `${cigarBase.name} (${cigarBase.brand.name})`,
          name: cigarBase.name,
          brand: cigarBase.brand,
          size: cigarBase.size,
          strength: cigarBase.strength,
          country: cigarBase.country,
        });
      });

      const grouped: Record<string, GroupedCigar> = {};
      uniqueCigars.forEach((cigar) => {
        const brandName = cigar.brand.name;
        if (!grouped[brandName]) {
          grouped[brandName] = { brand: brandName, cigars: [] };
        }
        grouped[brandName].cigars.push(cigar);
      });

      filteredCigars.value = Object.values(grouped);
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка поиска сигар:', err);
      }
      toast.add({ severity: 'error', summary: 'Ошибка', detail: 'Не удалось выполнить поиск сигар', life: 3000 });
    } finally {
      searchLoading.value = false;
    }
  }

  function handleCigarSelect(event: AutoCompleteOptionSelectEvent): void {
    const selected = event.value;
    if (!selected || typeof selected !== 'object') {
      return;
    }

    const cigar = selected as ReviewCigarOption;
    selectedCigar.value = cigar;
    form.cigarBaseId = cigar.cigarBaseId;
    form.userCigarId = cigar.userCigarId;

    if (!form.title) {
      form.title = `Обзор: ${cigar.name}`;
    }
  }

  function getStrengthLabel(strength?: string): string {
    if (!strength) return '';
    const strengthOptions = [
      { label: 'Очень легкая', value: 'very_mild' },
      { label: 'Легкая', value: 'mild' },
      { label: 'Средняя', value: 'medium' },
      { label: 'Полная', value: 'full' },
      { label: 'Очень полная', value: 'very_full' },
    ];
    return strengthOptions.find((opt) => opt.value === strength)?.label || strength;
  }

  watch(
    () => selectedCigar.value,
    (newCigar) => {
      if (newCigar) {
        form.cigarBaseId = newCigar.cigarBaseId;
        form.userCigarId = newCigar.userCigarId;
      }
    },
  );

  onMounted(async () => {
    const reviewId = route.params.id as string | undefined;
    if (reviewId) {
      await fetchReview(reviewId);
    } else {
      loading.value = false;
      handleQueryParameters();
    }
  });
</script>

<style scoped>
  .review-form-root {
    position: relative;
    isolation: isolate;
  }

  .review-form-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  /*:global(.dark) .review-form-grain {
    mix-blend-mode: soft-light;
  }*/

  .review-form-enter {
    animation: review-form-in 0.4s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes review-form-in {
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
    .review-form-enter {
      animation: none;
    }
  }

  :deep(.p-slider) {
    margin: 0;
  }

  /*
   * На мобильной ширине ручка слайдера (Aura) смещена на половину ширины наружу от 0%/100%;
   * без внутреннего отступа трек и ручки визуально упираются в края карточки или обрезаются.
   */
  @media (max-width: 639px) {
    .review-form-root :deep(.p-slider.p-slider-horizontal) {
      box-sizing: border-box;
      padding-inline: 0.875rem;
    }
  }

  :deep(.p-slider .p-slider-handle) {
    background: #d97706;
    border: 2px solid #ffffff;
    box-shadow: 0 2px 6px rgba(120, 53, 15, 0.25);
  }

  :global(.dark) :deep(.p-slider .p-slider-handle) {
    background: #fbbf24;
    border-color: #1c1917;
  }

  :deep(.p-slider .p-slider-range) {
    background: linear-gradient(90deg, #b45309, #d97706);
  }

  :global(.dark) :deep(.p-slider .p-slider-range) {
    background: linear-gradient(90deg, #b45309, #fbbf24);
  }

  :deep(.p-calendar) {
    width: 100%;
  }

  :deep(.p-inputtext) {
    width: 100%;
  }
</style>
