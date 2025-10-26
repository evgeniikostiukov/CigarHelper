<template>
  <div class="p-4 max-w-4xl mx-auto">
    <div class="mb-6">
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
        {{ isEditing ? 'Редактировать сигару' : 'Добавить новую сигару' }}
      </h1>
      <p class="text-gray-600 dark:text-gray-400 mt-2">
        {{ isEditing ? 'Обновите информацию о сигаре' : 'Заполните информацию о новой сигаре' }}
      </p>
    </div>

    <Card class="shadow-sm">
      <template #content>
        <form
          @submit.prevent="handleSubmit"
          class="space-y-6">
          <!-- Основная информация -->
          <div class="bg-gray-50 dark:bg-gray-800 rounded-lg p-6">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Основная информация</h3>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
              <div class="space-y-2">
                <label
                  for="name"
                  class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Название *
                </label>
                <AutoComplete
                  id="name"
                  v-model="form.cigar"
                  :suggestions="filteredCigars"
                  @complete="searchCigars"
                  @option-select="handleCigarSelect"
                  @change="handleCigarNameChange"
                  placeholder="Введите или выберите название сигары"
                  :class="{ 'p-invalid': errors.name }"
                  class="w-full"
                  field="name"
                  optionLabel="name"
                  optionGroupLabel="brand"
                  optionGroupChildren="cigars"
                  :dropdown="true"
                  :virtualScrollerOptions="{ itemSize: 50 }"
                  :loading="searchLoading"
                  :delay="300"
                  :minLength="2">
                  <template #optiongroup="slotProps">
                    <div class="font-semibold text-gray-700 dark:text-gray-300 p-2">
                      {{ slotProps.option.brand }}
                    </div>
                  </template>
                  <template #option="slotProps">
                    <div class="flex items-center">
                      <div>
                        <div class="font-semibold">
                          {{ slotProps.option.name }}
                        </div>
                        <div class="text-xs text-gray-500">
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
                    <div class="p-2 text-gray-500">
                      {{ searchLoading ? 'Поиск...' : 'Сигары не найдены. Введите имя для создания новой.' }}
                    </div>
                  </template>
                </AutoComplete>
                <small
                  v-if="errors.name"
                  class="text-red-500"
                  >{{ errors.name }}</small
                >
                <!-- Добавляю отображение информации о бренде выбранной сигары -->
                <div
                  v-if="selectedBrand"
                  class="mt-2 text-sm text-blue-600 dark:text-blue-400">
                  Бренд: {{ selectedBrand.name }}
                </div>
              </div>

              <div class="space-y-2">
                <label
                  for="country"
                  class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Страна
                </label>
                <InputText
                  id="country"
                  v-model="form.country"
                  class="w-full"
                  placeholder="Например: Куба, Доминикана" />
              </div>

              <div class="space-y-2">
                <label
                  for="price"
                  class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Цена (₽)
                </label>
                <InputNumber
                  id="price"
                  v-model="form.price"
                  class="w-full"
                  :minFractionDigits="2"
                  :maxFractionDigits="2"
                  placeholder="0.00"
                  suffix=" ₽" />
              </div>
            </div>
          </div>

          <!-- Структура сигары -->
          <div class="bg-gray-50 dark:bg-gray-800 rounded-lg p-6">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Структура сигары</h3>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div class="space-y-2">
                <label
                  for="wrapper"
                  class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Покровный лист
                </label>
                <InputText
                  id="wrapper"
                  v-model="form.wrapper"
                  class="w-full"
                  placeholder="Например: Connecticut, Maduro" />
                <small class="text-gray-500">Внешний лист сигары</small>
              </div>

              <div class="space-y-2">
                <label
                  for="binder"
                  class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Связующий лист
                </label>
                <InputText
                  id="binder"
                  v-model="form.binder"
                  class="w-full"
                  placeholder="Например: Nicaraguan, Dominican" />
                <small class="text-gray-500">Средний лист</small>
              </div>

              <div class="space-y-2">
                <label
                  for="filler"
                  class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Наполнитель
                </label>
                <InputText
                  id="filler"
                  v-model="form.filler"
                  class="w-full"
                  placeholder="Например: Nicaraguan, Dominican, Cuban" />
                <small class="text-gray-500">Основной табак</small>
              </div>
            </div>
          </div>

          <!-- Характеристики -->
          <div class="bg-gray-50 dark:bg-gray-800 rounded-lg p-6">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Характеристики</h3>
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
              <div class="space-y-2">
                <label
                  for="size"
                  class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Размер
                </label>
                <InputText
                  id="size"
                  v-model="form.size"
                  class="w-full"
                  placeholder="Например: 6x52" />
              </div>

              <div class="space-y-2">
                <label
                  for="strength"
                  class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Крепость
                </label>
                <Dropdown
                  id="strength"
                  v-model="form.strength"
                  :options="strengthOptions"
                  optionLabel="label"
                  optionValue="value"
                  placeholder="Выберите крепость"
                  class="w-full" />
              </div>

              <div class="space-y-2">
                <label
                  for="rating"
                  class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Оценка
                </label>
                <Rating
                  v-model="form.rating"
                  :stars="10"
                  :cancel="false"
                  class="w-full" />
                <small class="text-gray-500">Оцените сигару от 1 до 10</small>
              </div>
            </div>
          </div>

          <!-- Описание и изображение -->
          <div class="bg-gray-50 dark:bg-gray-800 rounded-lg p-6">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Описание и изображение</h3>
            <div class="space-y-6">
              <div class="space-y-2">
                <label
                  for="description"
                  class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Описание
                </label>
                <Textarea
                  id="description"
                  v-model="form.description"
                  class="w-full"
                  rows="4"
                  placeholder="Опишите вкус, аромат и другие характеристики сигары..." />
              </div>

              <div class="space-y-2">
                <label
                  for="imageUrl"
                  class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  URL изображения
                </label>
                <InputText
                  id="imageUrl"
                  v-model="form.imageUrl"
                  class="w-full"
                  placeholder="https://example.com/cigar-image.jpg" />
                <small class="text-gray-500">Ссылка на изображение сигары</small>
              </div>
            </div>
          </div>

          <!-- Хранение -->
          <div class="bg-gray-50 dark:bg-gray-800 rounded-lg p-6">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4">Хранение</h3>
            <div class="space-y-4">
              <!-- Чекбокс для добавления в коллекцию (только при создании новой сигары) -->
              <div
                v-if="!isEditing"
                class="space-y-2">
                <div class="flex items-center space-x-3">
                  <Checkbox
                    id="addToCollection"
                    v-model="form.addToCollection"
                    :binary="true"
                    class="p-checkbox-sm" />
                  <label
                    for="addToCollection"
                    class="text-sm font-medium text-gray-700 dark:text-gray-300 cursor-pointer">
                    Добавить сигару в мою коллекцию
                  </label>
                </div>
                <small class="text-gray-500 block ml-6">
                  Отметьте, чтобы добавить сигару в вашу личную коллекцию с указанными ценой, рейтингом и хьюмидором
                </small>
              </div>

              <div class="space-y-2">
                <label
                  for="humidorId"
                  class="block text-sm font-medium text-gray-700 dark:text-gray-300">
                  Хьюмидор
                </label>
                <Dropdown
                  id="humidorId"
                  v-model="form.humidorId"
                  :options="humidors"
                  optionLabel="name"
                  optionValue="id"
                  placeholder="Выберите хьюмидор для хранения"
                  class="w-full"
                  :disabled="!isEditing && !form.addToCollection" />
                <small class="text-gray-500">Оставьте пустым, если сигара не хранится в хьюмидоре</small>
              </div>

              <!-- Информация о выбранном хьюмидоре -->
              <div
                v-if="selectedHumidor"
                class="mt-4 p-4 bg-blue-50 dark:bg-blue-900/20 rounded-lg">
                <h4 class="font-medium text-blue-900 dark:text-blue-100 mb-2">
                  {{ selectedHumidor.name }}
                </h4>
                <div class="grid grid-cols-2 gap-4 text-sm text-blue-800 dark:text-blue-200">
                  <div>
                    <span class="font-medium">Вместимость:</span>
                    {{ selectedHumidor.capacity }} сигар
                  </div>
                  <div v-if="selectedHumidor.humidity">
                    <span class="font-medium">Влажность:</span>
                    {{ selectedHumidor.humidity }}%
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Кнопки -->
          <div class="flex flex-col sm:flex-row gap-3 pt-6 border-t border-gray-200 dark:border-gray-700">
            <Button
              type="submit"
              :label="isEditing ? 'Сохранить изменения' : 'Добавить сигару'"
              icon="pi pi-check"
              class="p-button-primary flex-1"
              :loading="loading" />
            <Button
              type="button"
              label="Отмена"
              icon="pi pi-times"
              class="p-button-secondary"
              @click="handleCancel" />
          </div>
        </form>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted, watch } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { useToast } from 'primevue/usetoast';
  import cigarService from '@/services/cigarService';
  import humidorService from '@/services/humidorService';
  import type { Cigar, CigarBase, Brand, PaginatedResult } from '@/services/cigarService';
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
  import Card from 'primevue/card';

  // Определяем интерфейсы для типизации
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

  interface StrengthOption {
    label: string;
    value: string;
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

  // Composables
  const route = useRoute();
  const router = useRouter();
  const toast = useToast();

  // Reactive data
  const loading = ref<boolean>(false);
  const humidors = ref<Humidor[]>([]);
  const brands = ref<Brand[]>([]);
  const errors = ref<FormErrors>({});
  const filteredCigars = ref<CigarGroup[]>([]);
  const selectedCigar = ref<Cigar | null>(null);
  const searchLoading = ref<boolean>(false);
  const searchCache = ref<Map<string, CigarGroup[]>>(new Map()); // Кэш для результатов поиска

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
    addToCollection: false, // По умолчанию НЕ добавляем в коллекцию
  });

  // Computed
  const isEditing = computed<boolean>(() => !!route.params.id);

  // Обновляю для поддержки отображения имени бренда при редактировании
  const selectedBrand = computed<Brand | null>(() => {
    if (!form.value.cigar || !form.value.cigar?.brand?.id) return null;
    return brands.value.find((brand) => brand.id === form.value.cigar.brand.id) || null;
  });

  const selectedHumidor = computed<Humidor | null>(() => {
    if (!form.value.humidorId) return null;
    return humidors.value.find((humidor) => humidor.id === form.value.humidorId) || null;
  });

  // --- Methods ---

  // Новая функция для установки начального бренда
  async function setInitialBrand(brandId: number) {
    // Ждем, пока бренды не будут загружены
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
      console.error('Error loading brands:', error);
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
      console.error('Error loading humidors:', error);
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

    try {
      loading.value = true;
      const cigar = await cigarService.getCigar(route.params.id as string);

      console.log('Данные с сервера при редактировании:', cigar); // Отладка
      console.log('Filler с сервера:', cigar.filler); // Отладка filler
      console.log('Filler тип с сервера:', typeof cigar.filler); // Отладка типа filler

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
    } catch (error) {
      console.error('Error loading cigar:', error);
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось загрузить данные сигары',
        life: 3000,
      });
      router.push('/cigars');
    } finally {
      loading.value = false;
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

    try {
      loading.value = true;
      const cigarData: Omit<Cigar, 'id' | 'brandName'> = {
        name: form.value.cigar.name, // Используем имя из объекта name
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

      console.log('Отправляемые данные:', cigarData); // Отладка
      console.log('Filler значение:', form.value.filler); // Отладка filler
      console.log('Filler тип:', typeof form.value.filler); // Отладка типа filler

      if (isEditing.value) {
        await cigarService.updateCigar(parseInt(route.params.id as string, 10), cigarData);
        toast.add({
          severity: 'success',
          summary: 'Успешно',
          detail: 'Сигара успешно обновлена',
          life: 3000,
        });
        router.push('/cigars');
      } else {
        // При создании новой сигары проверяем, нужно ли добавлять в коллекцию
        if (form.value.addToCollection) {
          // Добавляем в коллекцию пользователя
          await cigarService.createCigar(cigarData);
          toast.add({
            severity: 'success',
            summary: 'Успешно',
            detail: 'Сигара успешно добавлена в вашу коллекцию',
            life: 3000,
          });
          router.push('/cigars');
        } else {
          // Только создаем базовую сигару (это нужно реализовать в API)
          // Пока что просто показываем сообщение
          toast.add({
            severity: 'info',
            summary: 'Информация',
            detail:
              'Сигара добавлена только в базу данных. Для добавления в коллекцию отметьте соответствующий чекбокс.',
            life: 5000,
          });
          router.push('/cigar-bases');
        }
      }
    } catch (error) {
      console.error('Error saving cigar:', error);
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: isEditing.value ? 'Не удалось обновить сигару' : 'Не удалось добавить сигару',
        life: 3000,
      });
    } finally {
      loading.value = false;
    }
  }

  function handleCancel(): void {
    router.push('/cigars');
  }

  // Метод для ленивой загрузки сигар при поиске
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

  // Метод для ленивой загрузки сигар при поиске
  async function performSearch(query: string): Promise<void> {
    if (!query || query.length < 2) {
      filteredCigars.value = [];
      return;
    }

    // Проверяем кэш
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

      // Группируем сигары по брендам
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

        // Создаем копию объекта с displayName
        const formattedCigar = { ...cigar } as Cigar;
        let displayText = cigar.name || 'Без названия';
        if (cigar.strength) {
          const strengthLabel = getStrengthLabel(cigar.strength);
          displayText += ` (${strengthLabel})`;
        }
        // formattedCigar.displayName = displayText; // Убираем, так как нет в интерфейсе

        groupedCigars[brandName].cigars.push(formattedCigar);
      });

      const result = Object.values(groupedCigars);

      // Сохраняем в кэш
      searchCache.value.set(cacheKey, result);
      filteredCigars.value = result;
    } catch (error) {
      console.error('Ошибка при поиске сигар:', error);
      filteredCigars.value = [];
    } finally {
      searchLoading.value = false;
    }
  }

  // Создаем debounced версию поиска
  const debouncedSearch = debounce(performSearch, 300);

  // Метод для поиска сигар (вызывается AutoComplete)
  function searchCigars(event: AutoCompleteCompleteEvent): void {
    const query = event.query;
    debouncedSearch(query);
  }

  // Обработчик выбора существующей сигары
  function handleCigarSelect(event: AutoCompleteOptionSelectEvent): void {
    const selectedCigarData = event.value as Cigar;

    // Проверяем, что выбранная сигара существует
    if (!selectedCigarData || typeof selectedCigarData !== 'object') {
      console.warn('Выбрана некорректная сигара:', selectedCigarData);
      return;
    }

    selectedCigar.value = selectedCigarData;

    if (!isEditing.value) {
      // Заполняем форму данными выбранной базовой сигары
      form.value = {
        cigar: selectedCigarData, // Сохраняем полный объект сигары
        country: selectedCigarData.country || '',
        size: selectedCigarData.size || '',
        strength: selectedCigarData.strength || null,
        rating: 0,
        price: null,
        description: selectedCigarData.description || '',
        humidorId: form.value.humidorId, // Сохраняем выбранный хьюмидор
        imageUrl: '', // У базовых сигар нет imageURL
        wrapper: selectedCigarData.wrapper || '',
        binder: selectedCigarData.binder || '',
        filler: selectedCigarData.filler || '',
        addToCollection: true, // При выборе существующей сигары включаем добавление в коллекцию
      };
    }
  }

  // Обработка изменения имени сигары
  function handleCigarNameChange(event: any): void {
    // Если объект value не имеет нужных полей, значит это просто ручной ввод имени
    if (typeof event.value !== 'object' || !('name' in event.value)) {
      const inputValue = event.value as string;

      // Создаем объект сигары на основе введенного текста
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

    // Если это объект сигары
    const selectedCigarData = event.value as Cigar;

    // Если поле пустое, сбрасываем форму
    if (!selectedCigarData.name) {
      form.value.cigar = {} as Cigar;
      return;
    }

    // Получаем все сигары из текущих отфильтрованных результатов
    const allCigars: Cigar[] = [];
    if (Array.isArray(filteredCigars.value)) {
      filteredCigars.value.forEach((group) => {
        if (group && group.cigars && Array.isArray(group.cigars)) {
          allCigars.push(...group.cigars);
        }
      });
    }

    // Проверяем, есть ли такая сигара в списке по имени или displayName
    const matchingCigar = allCigars.find((cigar) => {
      if (!cigar || typeof cigar !== 'object') return false;

      const name = cigar.name || '';
      // const displayName = cigar.displayName || '';

      return (
        name.toLowerCase() === selectedCigarData.name.toLowerCase()
        // || displayName.toLowerCase() === selectedCigarData.name.toLowerCase()
      );
    });

    // Если нет совпадений и форма была заполнена данными существующей сигары,
    // сбрасываем форму, сохраняя введенное имя и выбранный хьюмидор
    if (!matchingCigar && selectedCigar.value) {
      selectedCigar.value = null;

      // Извлекаем только имя сигары, если пользователь ввел полное имя с брендом и крепостью
      let cigarName = selectedCigarData.name;

      // Если в имени есть скобки, удаляем их и всё содержимое внутри
      if (cigarName.includes('(')) {
        cigarName = cigarName.split('(')[0].trim();
      }

      // Если имя содержит название бренда в начале, пытаемся его удалить
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
      const currentBrandId = form.value.cigar.brand.id;

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

  // Получить имя бренда по ID
  function getBrandName(brandId: number): string {
    const brand = brands.value.find((b) => b.id === brandId);
    return brand ? brand.name : 'Бренд не указан';
  }

  // Получить метку крепости по значению
  function getStrengthLabel(strength: string): string {
    if (!strength) return '';

    const option = strengthOptions.find((opt) => opt.value === strength);
    return option ? option.label : strength;
  }

  // Lifecycle
  onMounted(() => {
    loadBrands();
    loadHumidors();
    loadCigar();

    const initialBrandId = route.query.brandId ? parseInt(route.query.brandId as string) : null;

    // Обработка query параметров для предзаполнения формы
    if (!isEditing.value && Object.keys(route.query).length > 0) {
      if (initialBrandId) {
        setInitialBrand(initialBrandId);
      }
      // Создаем объект name для сигары
      const cigarObj: Partial<Cigar> = {
        name: (route.query.name as string) || '',
      };

      // Предзаполняем форму данными из query параметров
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

      // Если переданы данные сигары, автоматически включаем добавление в коллекцию
      if (route.query.cigarId) {
        form.value.addToCollection = true;
      }
    }
  });
</script>
