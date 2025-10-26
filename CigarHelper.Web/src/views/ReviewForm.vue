<template>
  <div class="p-4 max-w-6xl mx-auto">
    <Toast />
    <!-- Заголовок -->
    <div class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
        {{ isEditing ? 'Редактирование обзора' : 'Новый обзор' }}
      </h1>
      <p class="text-gray-600 dark:text-gray-400 mt-2">
        {{ isEditing ? 'Внесите изменения в ваш обзор' : 'Создайте новый обзор сигары' }}
      </p>
    </div>

    <!-- Загрузка -->
    <div
      v-if="loading"
      class="flex flex-col items-center justify-center py-12">
      <ProgressSpinner size="large" />
      <p class="mt-4 text-gray-600 dark:text-gray-400">
        {{ isEditing ? 'Загрузка обзора...' : 'Загрузка данных...' }}
      </p>
    </div>

    <!-- Ошибка -->
    <div
      v-else-if="error"
      class="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
      <div class="flex items-center">
        <i class="pi pi-exclamation-triangle text-red-500 text-xl mr-3"></i>
        <div>
          <h3 class="text-red-800 font-medium">Ошибка</h3>
          <p class="text-red-700 mt-1">{{ error }}</p>
        </div>
      </div>
      <div class="mt-4">
        <Button
          label="Вернуться к списку обзоров"
          icon="pi pi-arrow-left"
          class="p-button-outlined"
          @click="$router.push('/reviews')" />
      </div>
    </div>

    <!-- Форма обзора -->
    <div v-else>
      <form
        @submit.prevent="submitForm"
        class="space-y-6">
        <!-- Общая информация -->
        <Card class="shadow-sm">
          <template #title>
            <div class="flex items-center">
              <i class="pi pi-info-circle text-blue-500 mr-2"></i>
              <span>Общая информация</span>
            </div>
          </template>
          <template #content>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <!-- Выбор сигары -->
              <div class="md:col-span-2">
                <label
                  for="cigarId"
                  class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Сигара *
                </label>
                <AutoComplete
                  id="cigarId"
                  v-model="selectedCigar"
                  :suggestions="filteredCigars"
                  @complete="searchCigars"
                  @option-select="handleCigarSelect"
                  placeholder="Введите название сигары для поиска"
                  class="w-full"
                  :class="{ 'p-invalid': validationErrors.cigarId }"
                  :showClear="true"
                  :loading="searchLoading"
                  :delay="300"
                  :minLength="2"
                  optionLabel="displayName"
                  optionGroupLabel="brand"
                  optionGroupChildren="cigars"
                  :dropdown="true"
                  :virtualScrollerOptions="{ itemSize: 50 }">
                  <template #optiongroup="slotProps">
                    <div class="font-semibold text-gray-700 dark:text-gray-300 p-2">
                      {{ slotProps.option.brand }}
                    </div>
                  </template>
                  <template #option="slotProps">
                    <div class="flex items-center">
                      <div class="flex-1">
                        <div class="font-semibold">{{ slotProps.option.name }}</div>
                        <div class="text-xs text-gray-500">
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
                        class="ml-2">
                        <span class="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded-full"> Моя </span>
                      </div>
                    </div>
                  </template>
                  <template #empty>
                    <div class="p-2 text-gray-500">
                      {{ searchLoading ? 'Поиск...' : 'Сигары не найдены. Введите название для поиска.' }}
                    </div>
                  </template>
                </AutoComplete>
                <small
                  v-if="validationErrors.cigarId"
                  class="p-error">
                  {{ validationErrors.cigarId }}
                </small>
              </div>

              <!-- Название обзора -->
              <div class="md:col-span-2">
                <label
                  for="title"
                  class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                  Заголовок обзора *
                </label>
                <InputText
                  id="title"
                  v-model="form.title"
                  placeholder="Введите заголовок обзора"
                  class="w-full"
                  :class="{ 'p-invalid': validationErrors.title }"
                  maxlength="200" />
                <small
                  v-if="validationErrors.title"
                  class="p-error">
                  {{ validationErrors.title }}
                </small>
                <small class="text-gray-500"> {{ form.title.length }}/200 символов </small>
              </div>

              <!-- Оценка -->
              <div class="md:col-span-2">
                <label class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2"> Общая оценка * </label>
                <div class="flex items-center space-x-4">
                  <div class="flex-1">
                    <Slider
                      v-model="form.rating"
                      :min="1"
                      :max="10"
                      :step="1"
                      class="w-full" />
                  </div>
                  <div class="flex items-center space-x-2">
                    <span class="text-2xl">⭐</span>
                    <span class="text-xl font-bold text-yellow-600"> {{ form.rating }}/10 </span>
                  </div>
                </div>
                <small
                  v-if="validationErrors.rating"
                  class="p-error">
                  {{ validationErrors.rating }}
                </small>
              </div>
            </div>
          </template>
        </Card>

        <!-- Детали дегустации -->
        <Card class="shadow-sm">
          <template #title>
            <div class="flex items-center">
              <i class="pi pi-star text-yellow-500 mr-2"></i>
              <span>Детали дегустации</span>
            </div>
          </template>
          <template #content>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <!-- Левая колонка -->
              <div class="space-y-4">
                <div>
                  <label
                    for="smokingExperience"
                    class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    Опыт курения
                  </label>
                  <InputText
                    id="smokingExperience"
                    v-model="form.smokingExperience"
                    placeholder="Например: 2 года, новичок"
                    class="w-full"
                    maxlength="50" />
                </div>

                <div>
                  <label
                    for="venue"
                    class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    Место дегустации
                  </label>
                  <InputText
                    id="venue"
                    v-model="form.venue"
                    placeholder="Например: дома, в клубе"
                    class="w-full"
                    maxlength="100" />
                </div>

                <div>
                  <label
                    for="smokingDate"
                    class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    Дата дегустации
                  </label>
                  <Calendar
                    id="smokingDate"
                    v-model="form.smokingDate"
                    dateFormat="dd/mm/yy"
                    placeholder="Выберите дату"
                    class="w-full" />
                </div>
              </div>

              <!-- Правая колонка -->
              <div class="space-y-4">
                <div>
                  <label
                    for="aroma"
                    class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    Аромат
                  </label>
                  <InputText
                    id="aroma"
                    v-model="form.aroma"
                    placeholder="Например: древесный, пряный"
                    class="w-full"
                    maxlength="50" />
                </div>

                <div>
                  <label
                    for="taste"
                    class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-2">
                    Вкус
                  </label>
                  <InputText
                    id="taste"
                    v-model="form.taste"
                    placeholder="Например: сладкий, горький"
                    class="w-full"
                    maxlength="50" />
                </div>

                <!-- Детальные оценки -->
                <div class="space-y-3">
                  <h4 class="text-sm font-medium text-gray-700 dark:text-gray-300">Детальные оценки</h4>

                  <div class="space-y-3">
                    <div>
                      <label class="block text-xs text-gray-600 dark:text-gray-400 mb-1">
                        Конструкция: {{ form.construction || '?' }}/5
                      </label>
                      <Slider
                        v-model="form.construction"
                        :min="1"
                        :max="5"
                        :step="1"
                        class="w-full" />
                    </div>

                    <div>
                      <label class="block text-xs text-gray-600 dark:text-gray-400 mb-1">
                        Горение: {{ form.burnQuality || '?' }}/5
                      </label>
                      <Slider
                        v-model="form.burnQuality"
                        :min="1"
                        :max="5"
                        :step="1"
                        class="w-full" />
                    </div>

                    <div>
                      <label class="block text-xs text-gray-600 dark:text-gray-400 mb-1">
                        Тяга: {{ form.draw || '?' }}/5
                      </label>
                      <Slider
                        v-model="form.draw"
                        :min="1"
                        :max="5"
                        :step="1"
                        class="w-full" />
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </template>
        </Card>

        <!-- Содержание обзора -->
        <Card class="shadow-sm">
          <template #title>
            <div class="flex items-center">
              <i class="pi pi-file-edit text-green-500 mr-2"></i>
              <span>Содержание обзора *</span>
            </div>
          </template>
          <template #content>
            <TextEditor v-model="form.content" />
            <small
              v-if="validationErrors.content"
              class="p-error mt-2 block">
              {{ validationErrors.content }}
            </small>
          </template>
        </Card>

        <!-- Изображения -->
        <Card class="shadow-sm">
          <template #title>
            <div class="flex items-center">
              <i class="pi pi-image text-purple-500 mr-2"></i>
              <span>Изображения</span>
            </div>
          </template>
          <template #content>
            <ImageUploader v-model="form.images" />
          </template>
        </Card>

        <!-- Кнопки управления -->
        <div class="flex flex-col sm:flex-row justify-between gap-4 pt-6 border-t border-gray-200 dark:border-gray-700">
          <Button
            label="Отмена"
            icon="pi pi-times"
            class="p-button-outlined p-button-secondary"
            @click="$router.push('/reviews')" />

          <Button
            :label="isEditing ? 'Сохранить изменения' : 'Опубликовать обзор'"
            :icon="isEditing ? 'pi pi-check' : 'pi pi-send'"
            :loading="saving"
            class="p-button-primary"
            type="submit" />
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, reactive, computed, watch, onMounted } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import api from '../services/api';
  import TextEditor from '../components/TextEditor.vue';
  import ImageUploader from '../components/ImageUploader.vue';
  import Card from 'primevue/card';
  import InputText from 'primevue/inputtext';
  import AutoComplete, {
    type AutoCompleteCompleteEvent,
    type AutoCompleteOptionSelectEvent,
  } from 'primevue/autocomplete';
  import Slider from 'primevue/slider';
  import Calendar from 'primevue/calendar';
  import Button from 'primevue/button';
  import ProgressSpinner from 'primevue/progressspinner';
  import Toast from 'primevue/toast';
  import { useToast } from 'primevue/usetoast';
  import { useConfirm } from 'primevue/useconfirm';
  import cigarService from '@/services/cigarService';
  import reviewService from '@/services/reviewService';
  import type { Cigar, CigarBase, PaginatedResult } from '@/services/cigarService';
  import type { Review, CreateReviewDto } from '@/services/reviewService';
  import authService from '@/services/authService';

  // --- Interfaces ---
  interface Image {
    id?: number | string;
    imageUrl: string;
    caption?: string;
  }

  interface ReviewForm {
    cigarId: number | null;
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
    images: Image[];
  }

  interface SelectableCigar extends CigarBase {
    id: number; // Убедимся, что id есть
    displayName: string;
    isUserCigar?: boolean;
  }

  interface GroupedCigar {
    brand: string;
    cigars: Cigar[];
  }

  interface ValidationErrors {
    [key: string]: string;
  }

  interface ReviewResponse {
    cigarId: number;
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
    images: Image[];
  }

  interface PaginatedCigars {
    items: Cigar[];
    // ... other pagination fields if needed
  }

  // --- Router & Route ---
  const route = useRoute();
  const router = useRouter();
  const toast = useToast(); // Инициализация toast
  const confirm = useConfirm();

  // --- Reactive state ---
  const isEditing = ref(false);
  const loading = ref(true);
  const error = ref<string | null>(null);
  const saving = ref(false);
  const validationErrors = reactive<ValidationErrors>({});
  const filteredCigars = ref<GroupedCigar[]>([]);
  const selectedCigar = ref<Cigar | null>(null);
  const searchLoading = ref(false);
  const searchCache = ref(new Map<string, GroupedCigar[]>());

  const form = reactive<ReviewForm>({
    cigarId: null,
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

  // --- Methods ---
  const fetchInitialData = async () => {
    // Data is now loaded via search, so this can be simpler
    loading.value = false;
  };

  const handleQueryParameters = () => {
    const query = route.query;

    if (query.cigarId && query.cigarName) {
      const cigarId = parseInt(query.cigarId as string);

      const tempCigar: Cigar = {
        id: cigarId,
        name: query.cigarName as string,
        brandName: (query.brandName as string) || 'Неизвестный бренд',
        displayName: `${(query.brandName as string) || 'Неизвестный бренд'} ${query.cigarName as string}`,
        size: query.size as string,
        strength: query.strength as string,
        country: query.country as string,
        description: query.description as string,
        wrapper: query.wrapper as string,
        binder: query.binder as string,
        filler: query.filler as string,
      };

      selectedCigar.value = tempCigar;
      form.cigarId = cigarId;
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

  const fetchReview = async (id: string) => {
    loading.value = true;

    try {
      const { data: review } = await api.get<ReviewResponse>(`/reviews/${id}`);

      const cigarData: Cigar = {
        id: review.cigarId,
        name: review.cigarName || 'Неизвестная сигара',
        brandName: review.cigarBrand || 'Неизвестный бренд',
        displayName: `${review.cigarBrand || 'Неизвестный бренд'} ${review.cigarName || 'Неизвестная сигара'}`,
      };

      selectedCigar.value = cigarData;
      form.cigarId = review.cigarId;
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
      form.images = review.images.map((img) => ({
        id: img.id,
        imageUrl: img.imageUrl,
        caption: img.caption || '',
      }));
    } catch (err) {
      console.error('Ошибка при загрузке обзора:', err);
      error.value = 'Не удалось загрузить обзор. Возможно, он был удален или у вас нет к нему доступа.';
    } finally {
      loading.value = false;
    }
  };

  const validateForm = (): boolean => {
    Object.keys(validationErrors).forEach((key) => delete validationErrors[key]);
    let isValid = true;

    if (!form.cigarId) {
      validationErrors.cigarId = 'Необходимо выбрать сигару';
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

  const submitForm = async () => {
    if (!validateForm()) {
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }

    saving.value = true;

    try {
      const reviewData = {
        cigarId: form.cigarId,
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

      if (isEditing.value) {
        const reviewId = route.params.id as string;

        const existingImageIds = form.images.filter((img) => typeof img.id === 'number').map((img) => img.id);

        const imagesToAdd = form.images
          .filter((img) => !img.id || typeof img.id === 'string')
          .map((img) => ({ imageUrl: img.imageUrl, caption: img.caption || '' }));

        const { data: originalReview } = await api.get<ReviewResponse>(`/reviews/${reviewId}`);
        const imageIdsToRemove = originalReview.images
          .filter((img) => !existingImageIds.includes(img.id))
          .map((img) => img.id as number);

        const updateData = { ...reviewData, imagesToAdd, imageIdsToRemove };
        await api.put(`/reviews/${reviewId}`, updateData);
        router.push(`/reviews/${reviewId}`);
      } else {
        const newReviewData = {
          ...reviewData,
          images: form.images.map((img) => ({ imageUrl: img.imageUrl, caption: img.caption || '' })),
        };
        const response = await api.post<{ id: number }>('/reviews', newReviewData);
        router.push(`/reviews/${response.data.id}`);
      }
    } catch (err: any) {
      console.error('Ошибка при сохранении обзора:', err);
      if (err.response && err.response.status === 400 && err.response.data.errors) {
        Object.assign(validationErrors, err.response.data.errors);
        window.scrollTo({ top: 0, behavior: 'smooth' });
      } else {
        error.value = 'Не удалось сохранить обзор. Пожалуйста, проверьте введенные данные и попробуйте снова.';
      }
    } finally {
      saving.value = false;
    }
  };

  function debounce<T extends (...args: any[]) => any>(func: T, delay: number): (...args: Parameters<T>) => void {
    let timeoutId: ReturnType<typeof setTimeout>;
    return function (this: ThisParameterType<T>, ...args: Parameters<T>) {
      clearTimeout(timeoutId);
      timeoutId = setTimeout(() => {
        func.apply(this, args);
      }, delay);
    };
  }

  async function performSearch(query: string) {
    if (!query || query.length < 2) {
      filteredCigars.value = [];
      return;
    }

    const cacheKey = query.toLowerCase();
    if (searchCache.value.has(cacheKey)) {
      filteredCigars.value = searchCache.value.get(cacheKey)!;
      return;
    }

    try {
      searchLoading.value = true;
      const searchParams = `?page=1&pageSize=50&search=${encodeURIComponent(query)}&sortField=name&sortOrder=asc`;
      const baseResponse = await api.get<PaginatedCigars>(`/cigars/bases/paginated${searchParams}`);
      const userResponse = await api.get<Cigar[]>('/cigars');

      const allCigars: Cigar[] = [];

      if (baseResponse.data && baseResponse.data.items) {
        baseResponse.data.items.forEach((cigar) => {
          if (!cigar) return;
          allCigars.push({
            ...cigar,
            displayName: `${cigar.name || 'Без названия'} (${getStrengthLabel(cigar.strength)})`,
            isUserCigar: false,
          });
        });
      }

      if (userResponse.data) {
        userResponse.data.forEach((cigar) => {
          if (!cigar || allCigars.some((c) => c.id === cigar.id)) return;
          allCigars.push({
            ...cigar,
            displayName: `${cigar.name || 'Без названия'} (${getStrengthLabel(cigar.strength)})`,
            isUserCigar: true,
          });
        });
      }

      const groupedCigars: { [key: string]: GroupedCigar } = {};
      allCigars.forEach((cigar) => {
        const brandName = cigar.brandName || 'Без бренда';
        if (!groupedCigars[brandName]) {
          groupedCigars[brandName] = { brand: brandName, cigars: [] };
        }
        groupedCigars[brandName].cigars.push(cigar);
      });

      const result = Object.values(groupedCigars);
      searchCache.value.set(cacheKey, result);
      filteredCigars.value = result;
    } catch (err) {
      console.error('Ошибка при поиске сигар:', err);
      filteredCigars.value = [];
    } finally {
      searchLoading.value = false;
    }
  }

  const debouncedSearch = debounce(performSearch, 300);

  async function searchCigars(event: { query: string }) {
    searchLoading.value = true;
    try {
      const [baseResult, userResult] = await Promise.all([
        cigarService.getCigarBasesPaginated({ name: event.query, pageSize: 10 }),
        cigarService.getCigars({ name: event.query, pageSize: 10 }),
      ]);

      const allCigars: Map<number, SelectableCigar> = new Map();

      // Сначала добавляем сигары пользователя, они приоритетнее
      userResult.items.forEach((cigar) => {
        allCigars.set(cigar.id, {
          ...cigar,
          displayName: `${cigar.name} (${cigar.brand.name})`,
          isUserCigar: true,
        });
      });

      // Затем добавляем базовые сигары, если их еще нет в списке
      baseResult.items.forEach((cigarBase) => {
        if (!allCigars.has(cigarBase.id)) {
          allCigars.set(cigarBase.id, {
            ...cigarBase,
            displayName: `${cigarBase.name} (${cigarBase.brand.name})`,
            isUserCigar: false,
          });
        }
      });

      const uniqueCigars = Array.from(allCigars.values());

      // Группировка для AutoComplete
      const grouped: { [key: string]: { brand: string; cigars: SelectableCigar[] } } = {};
      uniqueCigars.forEach((cigar) => {
        const brandName = cigar.brand.name;
        if (!grouped[brandName]) {
          grouped[brandName] = { brand: brandName, cigars: [] };
        }
        grouped[brandName].cigars.push(cigar);
      });

      filteredCigars.value = Object.values(grouped);
    } catch (err) {
      console.error('Ошибка поиска сигар:', err);
      toast.add({ severity: 'error', summary: 'Ошибка', detail: 'Не удалось выполнить поиск сигар', life: 3000 });
    } finally {
      searchLoading.value = false;
    }
  }

  function handleCigarSelect(event: AutoCompleteOptionSelectEvent) {
    const selectedCigarData = event.value;
    if (!selectedCigarData || typeof selectedCigarData !== 'object') {
      return;
    }

    selectedCigar.value = selectedCigarData;
    form.cigarId = selectedCigarData.id;

    if (!form.title) {
      form.title = `Обзор: ${selectedCigarData.name}`;
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

  // --- Watchers ---
  watch(
    () => selectedCigar.value,
    (newCigar) => {
      if (newCigar) {
        form.cigarId = newCigar.id;
      }
    },
  );

  // --- Lifecycle ---
  onMounted(async () => {
    const reviewId = route.params.id as string;
    isEditing.value = !!reviewId;

    await fetchInitialData();

    if (isEditing.value) {
      await fetchReview(reviewId);
    } else {
      handleQueryParameters();
    }
  });
</script>

<style scoped>
  /* Дополнительные стили для слайдеров */
  :deep(.p-slider) {
    margin: 0;
  }

  :deep(.p-slider .p-slider-handle) {
    background: #3b82f6;
    border: 2px solid #ffffff;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  }

  :deep(.p-slider .p-slider-range) {
    background: #3b82f6;
  }

  /* Стили для календаря */
  :deep(.p-calendar) {
    width: 100%;
  }

  /* Стили для dropdown */
  :deep(.p-dropdown) {
    width: 100%;
  }

  /* Стили для input */
  :deep(.p-inputtext) {
    width: 100%;
  }
</style>
