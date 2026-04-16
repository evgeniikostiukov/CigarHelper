<template>
  <Dialog
    v-model:visible="dialogVisible"
    :header="isEditing ? 'Редактировать базовую сигару' : 'Создать базовую сигару'"
    :style="{ width: '90vw', maxWidth: '800px' }"
    :modal="true"
    :closable="true"
    @hide="() => emit('update:visible', false)">
    <form
      @submit.prevent="saveCigar"
      class="space-y-6">
      <!-- Основная информация -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div class="space-y-4">
          <h3 class="text-lg font-semibold">Основная информация</h3>

          <div>
            <label
              for="name"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Название *
            </label>
            <InputText
              id="name"
              v-model="form.name"
              placeholder="Введите название сигары"
              class="w-full"
              :class="{ 'p-invalid': errors.name }"
              required />
            <small
              v-if="errors.name"
              class="p-error"
              >{{ errors.name }}</small
            >
          </div>

          <div>
            <label
              for="brand"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Бренд *
            </label>
            <Select
              label-id="brand"
              v-model="form.brandId"
              :options="brandOptions"
              option-label="name"
              option-value="id"
              placeholder="Выберите бренд"
              class="w-full"
              :class="{ 'p-invalid': errors.brandId }"
              :loading="brandsLoading"
              required />
            <small
              v-if="errors.brandId"
              class="p-error"
              >{{ errors.brandId }}</small
            >
          </div>

          <div>
            <label
              for="country"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Страна
            </label>
            <InputText
              id="country"
              v-model="form.country"
              placeholder="Введите страну"
              class="w-full" />
          </div>

          <div class="grid grid-cols-2 gap-3">
            <div>
              <label
                for="lengthInput"
                class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Длина
              </label>
              <div class="flex flex-col gap-2 sm:flex-row sm:items-stretch">
                <InputNumber
                  id="lengthInput"
                  v-model="form.lengthInput"
                  class="w-full sm:min-w-0 sm:flex-1"
                  input-class="w-full"
                  :min="1"
                  :max="form.lengthUnit === 'mm' ? 999 : 30"
                  :min-fraction-digits="0"
                  :max-fraction-digits="form.lengthUnit === 'mm' ? 0 : 2"
                  :use-grouping="false"
                  placeholder="—" />
                <Select
                  input-id="lengthUnit"
                  :model-value="form.lengthUnit"
                  class="w-full sm:w-[7.5rem] sm:shrink-0"
                  :options="lengthUnitSelectOptions"
                  option-label="label"
                  option-value="value"
                  @update:model-value="onLengthUnitChange" />
              </div>
            </div>
            <div>
              <label
                for="diameter"
                class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
                Кольцо
              </label>
              <InputNumber
                id="diameter"
                v-model="form.diameter"
                class="w-full"
                input-class="w-full"
                :min="1"
                :max="99"
                :use-grouping="false"
                placeholder="—" />
            </div>
          </div>

          <div>
            <label
              for="strength"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Крепость
            </label>
            <Select
              label-id="strength"
              v-model="form.strength"
              :options="strengthOptions"
              option-label="label"
              option-value="value"
              placeholder="Выберите крепость"
              class="w-full"
              show-clear />
          </div>
        </div>

        <div class="space-y-4">
          <h3 class="text-lg font-semibold">Структура</h3>

          <div>
            <label
              for="wrapper"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Покровный лист
            </label>
            <InputText
              id="wrapper"
              v-model="form.wrapper"
              placeholder="Введите тип покровного листа"
              class="w-full" />
          </div>

          <div>
            <label
              for="binder"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Связующий лист
            </label>
            <InputText
              id="binder"
              v-model="form.binder"
              placeholder="Введите тип связующего листа"
              class="w-full" />
          </div>

          <div>
            <label
              for="filler"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Наполнитель
            </label>
            <InputText
              id="filler"
              v-model="form.filler"
              placeholder="Введите тип наполнителя"
              class="w-full" />
          </div>

          <div>
            <label
              for="description"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Описание
            </label>
            <Textarea
              id="description"
              v-model="form.description"
              placeholder="Введите описание сигары"
              class="w-full"
              :rows="4"
              :autoResize="true" />
          </div>
        </div>
      </div>

      <!-- Изображения -->
      <div class="space-y-4">
        <h3 class="text-lg font-semibold">Изображения</h3>
        <p class="text-xs text-gray-600 dark:text-gray-400">
          {{ CIGAR_BASE_CATALOG_PHOTO_HINT }}
        </p>
        <FormImageGallerySection
          v-model="form.images"
          variant="bare"
          tone="dialog"
          show-main-image-star
          url-entry-mode="multi"
          url-rows-test-id="cigar-base-image-urls"
          :resolve-url-import="resolveCigarBaseUrlImport"
          url-help-text="До пяти фото в галерее. Несколько ссылок — через «Добавить в галерею»."
          url-help-detail="До двенадцати строк со ссылками; после добавления в активной галерее остаётся не больше пяти кадров."
          url-placeholder="Введите URL изображения"
          url-input-id="cigar-base-gallery-url"
          test-id="cigar-base-form-images" />
      </div>

      <div class="flex justify-end gap-2 pt-4 border-t">
        <Button
          label="Отмена"
          icon="pi pi-times"
          class="p-button-text"
          @click="dialogVisible = false"
          :disabled="isSaving" />
        <Button
          type="submit"
          :label="isEditing ? 'Сохранить изменения' : 'Создать сигару'"
          icon="pi pi-check"
          :loading="isSaving" />
      </div>
    </form>
  </Dialog>
</template>

<script setup lang="ts">
  import { ref, watch, computed, onMounted, reactive, nextTick } from 'vue';
  import { useToast } from 'primevue/usetoast';
  import { z } from 'zod';
  import cigarService, { type CigarBase, type Brand } from '@/services/cigarService';
  import FormImageGallerySection, { type FormGalleryImageItem } from './FormImageGallerySection.vue';
  import { CIGAR_BASE_CATALOG_PHOTO_HINT } from '@/constants/cigarBaseCatalogPhotoHint';
  import { strengthOptions } from '@/utils/cigarOptions';
  import InputNumber from 'primevue/inputnumber';
  import Select from 'primevue/select';
  import {
    lengthUnitSelectOptions,
    readStoredLengthUnit,
    writeStoredLengthUnit,
    lengthInputFromMm,
    lengthMmFromInput,
    convertLengthInputOnUnitChange,
    type CigarLengthUnit,
  } from '@/utils/cigarLengthUnit';
  import { maybeCompressImageFileForUpload } from '@/utils/imageClientCompress';

  // --- Interfaces and Types ---

  interface CigarFormState {
    id: number | null;
    name: string;
    brandId: number | null;
    country: string;
    strength: string;
    lengthInput: number | null;
    lengthUnit: CigarLengthUnit;
    diameter: number | null;
    wrapper: string | undefined;
    binder: string | undefined;
    filler: string | undefined;
    description: string | undefined;
    images: FormGalleryImageItem[];
  }

  // --- Props and Emits ---

  interface Props {
    cigar?: CigarBase | null;
    /** Шаблон для «Создать похожую»: новая запись, поля скопированы, id нет. Не используется вместе с `cigar`. */
    prefillSimilar?: CigarBase | null;
    visible: boolean;
  }

  const props = withDefaults(defineProps<Props>(), {
    cigar: null,
    prefillSimilar: null,
    visible: false,
  });

  const emit = defineEmits<{
    'update:visible': [value: boolean];
    saved: [updatedCigar: CigarBase];
  }>();

  // --- Composables ---

  const toast = useToast();

  // --- Refs and Reactive State ---

  const form = reactive<CigarFormState>({
    id: null,
    name: '',
    brandId: null,
    country: '',
    strength: '',
    lengthInput: null,
    lengthUnit: 'mm',
    diameter: null,
    wrapper: undefined,
    binder: '',
    filler: '',
    description: '',
    images: [],
  });

  const brands = ref<Brand[]>([]);
  const brandsLoading = ref(false);
  const isSaving = ref(false);
  const errors = ref<Record<string, string>>({});

  // --- Computed Properties ---

  const isEditing = computed(() => !!props.cigar);

  const brandOptions = computed(() => brands.value?.map((brand) => ({ id: brand.id, name: brand.name })));

  const dialogVisible = computed({
    get: () => props.visible,
    set: (value) => emit('update:visible', value),
  });

  // --- Validation Schema ---

  const cigarSchema = z.object({
    name: z.string().min(1, 'Название обязательно для заполнения'),
    brandId: z.number().min(1, 'Бренд обязателен для выбора'),
  });

  // --- Functions ---

  function onLengthUnitChange(next: CigarLengthUnit | null) {
    if (next == null) return;
    const prev = form.lengthUnit;
    if (prev === next) return;
    form.lengthInput = convertLengthInputOnUnitChange(form.lengthInput, prev, next);
    form.lengthUnit = next;
    writeStoredLengthUnit(next);
  }

  function initializeForm(cigarData: CigarBase | null) {
    form.lengthUnit = readStoredLengthUnit();
    if (cigarData) {
      form.id = cigarData.id;
      form.name = cigarData.name;
      form.brandId = cigarData.brand.id;
      form.country = cigarData.country;
      form.strength = cigarData.strength;
      form.lengthInput = lengthInputFromMm(cigarData.lengthMm ?? null, form.lengthUnit);
      form.diameter = cigarData.diameter ?? null;
      form.wrapper = cigarData.wrapper;
      form.binder = cigarData.binder;
      form.filler = cigarData.filler;
      form.description = cigarData.description;
      form.images =
        cigarData.images?.map((img) => ({
          id: img.id,
          preview: `/api/cigarimages/${img.id}/thumbnail`,
          caption: '',
          isMain: img.isMain,
          isExisting: true,
          markedForDeletion: false,
        })) || [];
    } else {
      // Reset to default state for new cigar
      form.id = null;
      form.name = '';
      form.brandId = null;
      form.country = '';
      form.strength = '';
      form.lengthInput = null;
      form.diameter = null;
      form.wrapper = '';
      form.binder = '';
      form.filler = '';
      form.description = '';
      form.images = [];
    }
    errors.value = {};
  }

  /** Новая карточка по образцу: копируем каталожные поля, без id и без привязки к чужим изображениям. */
  function initializeFormFromSimilar(source: CigarBase) {
    form.lengthUnit = readStoredLengthUnit();
    form.id = null;
    form.name = source.name ?? '';
    form.brandId = source.brand?.id ?? null;
    form.country = source.country ?? '';
    form.strength = source.strength ?? '';
    form.lengthInput = lengthInputFromMm(source.lengthMm ?? null, form.lengthUnit);
    form.diameter = source.diameter ?? null;
    form.wrapper = source.wrapper ?? '';
    form.binder = source.binder ?? '';
    form.filler = source.filler ?? '';
    form.description = source.description ?? '';
    form.images = [];
    errors.value = {};
  }

  async function fetchBrands() {
    brandsLoading.value = true;
    try {
      const response = await cigarService.getAllBrands({
        page: 1,
        pageSize: 1000,
      }); // Получаем все бренды
      brands.value = response;
    } catch (error) {
      console.error('Ошибка при загрузке брендов:', error);
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось загрузить список брендов',
        life: 3000,
      });
    } finally {
      brandsLoading.value = false;
    }
  }

  function validateForm(): boolean {
    errors.value = {};
    const result = cigarSchema.safeParse({
      name: form.name,
      brandId: form.brandId,
    });

    if (!result.success) {
      result.error.issues.forEach((err) => {
        const path = err.path[0] as string;
        errors.value[path] = err.message;
      });
      return false;
    }
    return true;
  }

  function isValidHttpUrl(raw: string): boolean {
    try {
      const parsed = new URL(raw.trim());
      return parsed.protocol === 'http:' || parsed.protocol === 'https:';
    } catch {
      return false;
    }
  }

  async function saveCigar() {
    if (!validateForm()) {
      toast.add({
        severity: 'warn',
        summary: 'Проверьте данные',
        detail: 'Пожалуйста, заполните все обязательные поля.',
        life: 3000,
      });
      return;
    }

    isSaving.value = true;
    try {
      const formData = new FormData();
      formData.append('Name', form.name);
      if (form.brandId) formData.append('BrandId', form.brandId.toString());
      formData.append('Country', form.country);
      formData.append('Strength', form.strength);
      {
        const mm = lengthMmFromInput(form.lengthInput, form.lengthUnit);
        if (mm != null) formData.append('LengthMm', String(mm));
      }
      if (form.diameter != null) formData.append('Diameter', String(form.diameter));
      if (form.wrapper) formData.append('Wrapper', form.wrapper);
      if (form.binder) formData.append('Binder', form.binder);
      if (form.filler) formData.append('Filler', form.filler);
      if (form.description) formData.append('Description', form.description);

      const activeImages = form.images.filter((img) => !img.markedForDeletion);
      let newImageFileIndex = 0;
      let newImageUrlIndex = 0;
      for (const img of activeImages) {
        if (img.file) {
          const file = await maybeCompressImageFileForUpload(img.file);
          formData.append(`NewImages[${newImageFileIndex}].File`, file);
          formData.append(`NewImages[${newImageFileIndex}].IsMain`, String(img.isMain ?? false));
          newImageFileIndex++;
          continue;
        }
        /** Пока нет id CigarBase, upload-by-url не привязывает к карточке; при создании — ImageUrls (как в CigarForm). */
        if (!isEditing.value) {
          const rawUrl = img.imageUrl?.trim();
          if (rawUrl && isValidHttpUrl(rawUrl)) {
            formData.append(`ImageUrls[${newImageUrlIndex}]`, rawUrl);
            formData.append(`ImageUrlIsMain[${newImageUrlIndex}]`, String(!!img.isMain));
            newImageUrlIndex++;
          }
        }
      }

      const imagesToUpdate = form.images.filter((img) => img.id && !img.markedForDeletion && img.isExisting);
      imagesToUpdate.forEach((img, index) => {
        if (img.id) {
          formData.append(`ExistingImages[${index}].Id`, img.id.toString());
          formData.append(`ExistingImages[${index}].IsMain`, String(img.isMain ?? false));
        }
      });

      const imageIdsToDelete = form.images.filter((img) => img.id && img.markedForDeletion).map((img) => img.id);

      imageIdsToDelete.forEach((id, index) => {
        if (id) {
          formData.append(`ImageIdsToDelete[${index}]`, id.toString());
        }
      });

      let cigarBase: CigarBase;

      if (isEditing.value && form.id) {
        cigarBase = await cigarService.updateCigarBase(form.id, formData);
        toast.add({
          severity: 'success',
          summary: 'Успех',
          detail: 'Данные сигары обновлены',
          life: 3000,
        });
      } else {
        cigarBase = await cigarService.createCigarBase(formData);
        toast.add({
          severity: 'success',
          summary: 'Успех',
          detail: 'Новая сигара создана',
          life: 3000,
        });
      }

      emit('saved', cigarBase);
      dialogVisible.value = false;
    } catch (error: any) {
      console.error('Ошибка при сохранении сигары:', error);
      const errorDetail = error.response?.data?.title || 'Произошла неизвестная ошибка при сохранении.';
      toast.add({
        severity: 'error',
        summary: 'Ошибка сохранения',
        detail: errorDetail,
        life: 5000,
      });
    } finally {
      isSaving.value = false;
    }
  }

  async function resolveCigarBaseUrlImport(url: string): Promise<FormGalleryImageItem | null> {
    const trimmed = url.trim();
    if (!isValidHttpUrl(trimmed)) {
      toast.add({
        severity: 'warn',
        summary: 'Ссылка',
        detail: 'Укажите корректный адрес с протоколом http или https.',
        life: 4000,
      });
      return null;
    }

    const active = form.images.filter((img) => !img.markedForDeletion);
    const isFirst = active.length === 0;

    /** Без id базы upload-by-url создаёт «сироту», не попадающую в POST создания — откладываем до ImageUrls. */
    if (form.id == null) {
      return {
        preview: trimmed,
        imageUrl: trimmed,
        caption: '',
        isMain: isFirst,
        isExisting: false,
        markedForDeletion: false,
      };
    }

    try {
      const response = await cigarService.uploadImageByUrl(trimmed, form.id);
      return {
        id: response.id,
        preview: `/api/cigarimages/${response.id}/thumbnail`,
        caption: '',
        isMain: isFirst,
        isExisting: true,
        markedForDeletion: false,
      };
    } catch (error) {
      console.error('Ошибка при добавлении изображения по URL:', error);
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось добавить изображение по ссылке.',
        life: 3000,
      });
      return null;
    }
  }

  // --- Watchers ---

  watch(
    () => props.visible,
    (newValue) => {
      if (newValue) {
        nextTick(() => {
          if (props.cigar) {
            initializeForm(props.cigar);
          } else if (props.prefillSimilar) {
            initializeFormFromSimilar(props.prefillSimilar);
          } else {
            initializeForm(null);
          }
          if (brands.value.length === 0) {
            fetchBrands();
          }
        });
      }
    },
  );

  // --- Lifecycle Hooks ---

  onMounted(() => {
    fetchBrands();
  });
</script>
