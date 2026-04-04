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
            <Dropdown
              id="brand"
              v-model="form.brandId"
              :options="brandOptions"
              optionLabel="name"
              optionValue="id"
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

          <div>
            <label
              for="size"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Размер
            </label>
            <InputText
              id="size"
              v-model="form.size"
              placeholder="Например: 6x50"
              class="w-full" />
          </div>

          <div>
            <label
              for="strength"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">
              Крепость
            </label>
            <Dropdown
              id="strength"
              v-model="form.strength"
              :options="strengthOptions"
              optionLabel="label"
              optionValue="value"
              placeholder="Выберите крепость"
              class="w-full"
              :showClear="true" />
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
        <ImageUploader
          ref="uploader"
          @files-selected="handleFilesSelected"
          :max-files="5"
          :current-file-count="form.images.filter((img) => !img.markedForDeletion).length" />
        <!-- Загрузка по ссылке -->
        <div class="space-y-2">
          <label class="block text-sm font-medium text-gray-700 dark:text-gray-300">
            Добавить изображение по ссылке
          </label>
          <div class="flex gap-2">
            <InputText
              v-model="newImageUrl"
              placeholder="Введите URL изображения"
              class="flex-1"
              :disabled="addingImageByUrl" />
            <Button
              label="Добавить"
              icon="pi pi-link"
              class="p-button-success"
              :loading="addingImageByUrl"
              :disabled="!newImageUrl || addingImageByUrl"
              @click="addImageByUrl" />
          </div>
          <small class="text-gray-500"> Изображение будет автоматически скачано и сохранено на сервере. </small>
        </div>

        <!-- Кнопка загрузки, которая триггерит ImageUploader -->
        <Button
          label="Выбрать файлы для загрузки"
          icon="pi pi-upload"
          class="p-button-outlined"
          @click="addImage"
          :disabled="form.images.filter((img) => !img.markedForDeletion).length >= 5" />

        <!-- Список изображений -->
        <div
          v-if="form.images.length > 0"
          class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
          <div
            v-for="(image, index) in form.images"
            :key="image.id || image.preview"
            class="relative group aspect-square overflow-hidden rounded-lg bg-slate-100 dark:bg-slate-800/80">
            <div
              v-if="image.markedForDeletion"
              class="absolute inset-0 flex flex-col items-center justify-center rounded-lg z-10 bg-red-500/70">
              <i class="pi pi-trash text-white text-3xl mb-2"></i>
              <p class="text-white font-semibold">Будет удалено</p>
              <Button
                icon="pi pi-undo"
                class="p-button-rounded p-button-text p-button-sm text-white mt-2"
                @click="restoreImage(index)"
                v-tooltip.top="'Восстановить'" />
            </div>
            <img
              :src="image.preview"
              :alt="`Изображение ${index + 1}`"
              class="w-full h-full object-contain object-center transition-transform duration-300"
              :class="{ 'opacity-30': image.markedForDeletion }" />
            <div
              v-if="!image.markedForDeletion"
              class="absolute inset-0 flex items-center justify-center rounded-lg bg-black/0 transition-all duration-300 group-hover:bg-black/50">
              <div class="flex gap-2 opacity-0 group-hover:opacity-100 transition-opacity duration-300">
                <Button
                  v-if="image.isExisting || image.id"
                  icon="pi pi-star"
                  :class="image.isMain ? 'p-button-warning' : 'p-button-outlined p-button-warning'"
                  class="p-button-rounded p-button-sm"
                  @click.stop="setMainImage(index)"
                  v-tooltip.top="image.isMain ? 'Главное изображение' : 'Сделать главным'" />
                <Button
                  icon="pi pi-times"
                  class="p-button-rounded p-button-danger p-button-sm"
                  @click.stop="removeImage(index)"
                  v-tooltip.top="'Удалить'" />
              </div>
            </div>
            <div
              v-if="image.isMain && !image.markedForDeletion"
              class="absolute top-2 left-2 bg-yellow-500 text-white text-xs font-bold px-2 py-1 rounded-full shadow-md">
              Главное
            </div>
          </div>
        </div>
        <div
          v-else
          class="text-center py-8 border-2 border-dashed rounded-lg">
          <i class="pi pi-image text-4xl text-gray-400 mb-2"></i>
          <p class="text-gray-500">Нет добавленных изображений</p>
        </div>
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
  import ImageUploader from './ImageUploader.vue'; // Предполагается, что этот компонент существует
  import { strengthOptions } from '@/utils/cigarOptions';

  // --- Interfaces and Types ---

  interface CigarImage {
    id?: number;
    file?: File;
    preview?: string;
    isMain: boolean;
    isExisting: boolean;
    markedForDeletion: boolean;
    url?: string; // для существующих изображений
  }

  interface CigarFormState {
    id: number | null;
    name: string;
    brandId: number | null;
    country: string;
    strength: string;
    size: string;
    wrapper: string | undefined;
    binder: string | undefined;
    filler: string | undefined;
    description: string | undefined;
    images: CigarImage[];
  }

  // --- Props and Emits ---

  interface Props {
    cigar?: CigarBase | null;
    visible: boolean;
  }

  const props = withDefaults(defineProps<Props>(), {
    cigar: null,
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
    size: '',
    wrapper: undefined,
    binder: '',
    filler: '',
    description: '',
    images: [],
  });

  const brands = ref<Brand[]>([]);
  const brandsLoading = ref(false);
  const isSaving = ref(false);
  const newImageUrl = ref('');
  const addingImageByUrl = ref(false);
  const errors = ref<Record<string, string>>({});
  const uploader = ref<InstanceType<typeof ImageUploader> | null>(null);

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

  function initializeForm(cigarData: CigarBase | null) {
    if (cigarData) {
      form.id = cigarData.id;
      form.name = cigarData.name;
      form.brandId = cigarData.brand.id;
      form.country = cigarData.country;
      form.strength = cigarData.strength;
      form.size = cigarData.size;
      form.wrapper = cigarData.wrapper;
      form.binder = cigarData.binder;
      form.filler = cigarData.filler;
      form.description = cigarData.description;
      form.images =
        cigarData.images?.map((img) => ({
          id: img.id,
          preview: `/api/cigarimages/${img.id}`,
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
      form.size = '';
      form.wrapper = '';
      form.binder = '';
      form.filler = '';
      form.description = '';
      form.images = [];
    }
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
      formData.append('Size', form.size);
      if (form.wrapper) formData.append('Wrapper', form.wrapper);
      if (form.binder) formData.append('Binder', form.binder);
      if (form.filler) formData.append('Filler', form.filler);
      if (form.description) formData.append('Description', form.description);

      const imagesToUpload = form.images.filter((img) => img.file && !img.markedForDeletion);
      imagesToUpload.forEach((img, index) => {
        if (img.file) {
          formData.append(`NewImages[${index}].File`, img.file);
          formData.append(`NewImages[${index}].IsMain`, String(img.isMain));
        }
      });

      const imagesToUpdate = form.images.filter((img) => img.id && !img.markedForDeletion && img.isExisting);
      imagesToUpdate.forEach((img, index) => {
        if (img.id) {
          formData.append(`ExistingImages[${index}].Id`, img.id.toString());
          formData.append(`ExistingImages[${index}].IsMain`, String(img.isMain));
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

  function addImage() {
    if (uploader.value) {
      uploader.value.open();
    }
  }

  function handleFilesSelected(files: File[]) {
    const remainingSlots = 5 - form.images.filter((img) => !img.markedForDeletion).length;
    const filesToAdd = files.slice(0, remainingSlots);

    filesToAdd.forEach((file) => {
      const newImage: CigarImage = {
        file: file,
        preview: URL.createObjectURL(file),
        isMain: false,
        isExisting: false,
        markedForDeletion: false,
      };
      form.images.push(newImage);
    });

    if (files.length > remainingSlots) {
      toast.add({
        severity: 'warn',
        summary: 'Лимит изображений',
        detail: `Можно добавить еще ${remainingSlots} изображений. Лишние файлы были проигнорированы.`,
        life: 4000,
      });
    }
  }

  async function addImageByUrl() {
    if (!newImageUrl.value) return;

    const remainingSlots = 5 - form.images.filter((img) => !img.markedForDeletion).length;
    if (remainingSlots <= 0) {
      toast.add({ severity: 'warn', summary: 'Лимит', detail: 'Достигнут лимит в 5 изображений.', life: 3000 });
      return;
    }

    addingImageByUrl.value = true;
    try {
      const response = await cigarService.uploadImageByUrl(newImageUrl.value, form.id);
      const newImage: CigarImage = {
        id: response.id,
        preview: `/api/cigarimages/${response.id}`,
        isMain: false,
        isExisting: true, // Считаем его существующим, так как оно уже на сервере
        markedForDeletion: false,
        // Мы не можем получить File, но он и не нужен, т.к. isExisting=true
      };

      // Если это первое изображение, делаем его главным
      if (form.images.filter((img) => !img.markedForDeletion).length === 0) {
        newImage.isMain = true;
      }

      form.images.push(newImage);
      newImageUrl.value = '';
      toast.add({
        severity: 'success',
        summary: 'Успех',
        detail: 'Изображение добавлено по ссылке.',
        life: 3000,
      });
    } catch (error) {
      console.error('Ошибка при добавлении изображения по URL:', error);
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось добавить изображение по ссылке.',
        life: 3000,
      });
    } finally {
      addingImageByUrl.value = false;
    }
  }

  function removeImage(index: number) {
    const image = form.images[index];
    if (image.isExisting) {
      // Если изображение уже было на сервере, помечаем его для удаления
      image.markedForDeletion = true;
      // Если удаляемое изображение было главным, нужно назначить новое главное
      if (image.isMain) {
        const nextAvailableImage = form.images.find((img, i) => i !== index && !img.markedForDeletion);
        if (nextAvailableImage) {
          nextAvailableImage.isMain = true;
        }
      }
    } else {
      // Если это новое, еще не загруженное изображение, просто удаляем его из массива
      form.images.splice(index, 1);
    }
  }

  function restoreImage(index: number) {
    const image = form.images[index];
    if (image.markedForDeletion) {
      image.markedForDeletion = false;
      // Если после восстановления не осталось главных изображений, делаем это главным
      const hasMainImage = form.images.some((img) => img.isMain && !img.markedForDeletion);
      if (!hasMainImage) {
        image.isMain = true;
      }
    }
  }

  function setMainImage(index: number) {
    form.images.forEach((img, i) => {
      img.isMain = i === index;
    });
  }

  // --- Watchers ---

  watch(
    () => props.visible,
    (newValue) => {
      if (newValue) {
        nextTick(() => {
          initializeForm(props.cigar);
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
