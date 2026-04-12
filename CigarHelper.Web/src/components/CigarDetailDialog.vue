<script lang="ts" setup>
  import { computed } from 'vue';
  import Dialog from 'primevue/dialog';
  import Carousel from 'primevue/carousel';
  import Button from 'primevue/button';
  import type { CigarBase, CigarImage } from '@/services/cigarService';
  import { strengthOptions } from '@/utils/cigarOptions';
  import { formatVitola } from '@/utils/vitola';
  import { arrayBufferToBase64 } from '@/utils/imageUtils';
  import CigarCommentsPanel from './CigarCommentsPanel.vue';

  // --- Props ---

  interface Props {
    visible: boolean;
    cigar: CigarBase | undefined;
    /** Редактирование записи справочника (CigarBase) — только Admin/Moderator. */
    canEditCatalog?: boolean;
  }

  const props = withDefaults(defineProps<Props>(), {
    visible: false,
    cigar: undefined,
    canEditCatalog: false,
  });

  // --- Emits ---

  const emit = defineEmits<{
    'update:visible': [boolean];
    writeReview: [CigarBase];
    addToCollection: [CigarBase];
    createSimilarCigar: [CigarBase];
    editBaseCigar: [CigarBase];
  }>();

  // --- Computed ---

  const dialogVisible = computed({
    get: () => props.visible,
    set: (value) => emit('update:visible', value),
  });

  /** Как на CigarBases: API отдаёт байты в `data`, не URL; GET /api/cigarimages/{id} — JSON, не картинка. */
  function cigarImageBytes(img: CigarImage): string | number[] | undefined {
    const raw = img.imageData ?? img.data;
    return raw ?? undefined;
  }

  function cigarImagePreviewSrc(img: CigarImage): string {
    const raw = cigarImageBytes(img);
    const hasBytes = raw != null && (typeof raw === 'string' ? raw.length > 0 : raw.length > 0);

    if (hasBytes) {
      const b64 = arrayBufferToBase64(raw);
      if (b64) {
        const mime = (img.contentType ?? '').trim();
        const safeMime = mime.startsWith('image/') ? mime : 'image/jpeg';
        return `data:${safeMime};base64,${b64}`;
      }
    }

    // MinIO / внешнее хранилище — используем публичный API-эндпоинт
    return img.id ? `/api/cigarimages/${img.id}/thumbnail` : '';
  }

  const cigarImages = computed(() => {
    if (!props.cigar?.images) {
      return [];
    }
    return props.cigar.images
      .map((img) => ({
        ...img,
        preview: cigarImagePreviewSrc(img),
      }))
      .filter((item) => item.preview.length > 0);
  });

  // --- Methods ---

  function getStrengthLabel(strength: string | undefined | null): string {
    if (!strength) return '';
    const option = strengthOptions.find((opt) => opt.value === strength);
    return option ? option.label : strength;
  }

  function getStrengthBadgeClass(strength: string | undefined | null): string {
    if (!strength) return 'bg-gray-100 text-gray-800';
    const classes: Record<string, string> = {
      very_mild: 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200',
      mild: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200',
      medium: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200',
      full: 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-200',
      very_full: 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200',
    };
    return classes[strength] || 'bg-gray-100 text-gray-800';
  }

  function handleImageError(event: Event) {
    const target = event.target as HTMLImageElement;
    target.style.display = 'none';
    const parent = target.parentElement;
    if (parent) {
      const fallback = parent.querySelector('.image-fallback');
      if (fallback) {
        (fallback as HTMLElement).style.display = 'flex';
      }
    }
  }

  function close() {
    dialogVisible.value = false;
  }

  function writeReview() {
    if (props.cigar) {
      emit('writeReview', props.cigar);
      close();
    }
  }

  function addToCollection() {
    if (props.cigar) {
      emit('addToCollection', props.cigar);
      close();
    }
  }

  function createSimilarCigar() {
    if (props.cigar) {
      emit('createSimilarCigar', props.cigar);
      close();
    }
  }

  function editBaseCigar() {
    if (props.cigar) {
      emit('editBaseCigar', props.cigar);
      // Не закрываем диалог, чтобы основное окно могло открыть диалог редактирования
    }
  }
</script>

<template>
  <Dialog
    v-model:visible="dialogVisible"
    :header="cigar?.name || 'Детали сигары'"
    :style="{ width: '90vw', maxWidth: '800px' }"
    :modal="true"
    :closable="true">
    <div
      v-if="cigar"
      class="space-y-6 p-2">
      <!-- Изображения сигары -->
      <div
        v-if="cigarImages.length > 0"
        class="mb-6">
        <Carousel
          :value="cigarImages"
          :numVisible="1"
          :numScroll="1"
          :circular="cigarImages.length > 1"
          :showIndicators="cigarImages.length > 1"
          class="w-full">
          <template #item="slotProps">
            <div
              class="flex justify-center items-center bg-gray-100 dark:bg-gray-800 rounded-lg overflow-hidden aspect-video">
              <img
                :src="slotProps.data.preview"
                :alt="cigar.name"
                class="max-h-[400px] w-auto h-auto object-contain"
                loading="lazy"
                @error="handleImageError" />
              <div class="image-fallback hidden items-center justify-center w-full h-full">
                <i class="pi pi-image text-gray-400 text-5xl"></i>
              </div>
            </div>
          </template>
        </Carousel>
      </div>

      <div
        v-else
        class="flex flex-col items-center justify-center py-10 bg-gray-50 dark:bg-gray-800 rounded-lg">
        <i class="pi pi-image text-5xl text-gray-400 dark:text-gray-500"></i>
        <p class="mt-2 text-gray-500 dark:text-gray-400">Нет изображений</p>
      </div>

      <!-- Основная информация -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-x-8 gap-y-6">
        <div class="space-y-4">
          <h3 class="text-xl font-semibold border-b pb-2">Основная информация</h3>
          <div class="flex justify-between">
            <span class="font-medium text-gray-600 dark:text-gray-400">Название:</span>
            <span class="text-right">{{ cigar.name }}</span>
          </div>
          <div class="flex justify-between items-center">
            <span class="font-medium text-gray-600 dark:text-gray-400">Бренд:</span>
            <div class="flex items-center gap-2">
              <img
                v-if="cigar.brand.logoBytes"
                :src="`data:image/png;base64,${cigar.brand.logoBytes}`"
                :alt="cigar.brand.name"
                class="w-6 h-6 object-contain rounded"
                loading="lazy" />
              <span>{{ cigar.brand.name }}</span>
            </div>
          </div>
          <div
            v-if="cigar.country"
            class="flex justify-between">
            <span class="font-medium text-gray-600 dark:text-gray-400">Страна:</span>
            <span class="text-right">{{ cigar.country }}</span>
          </div>
          <div
            v-if="formatVitola(cigar.lengthMm, cigar.diameter)"
            class="flex justify-between">
            <span class="font-medium text-gray-600 dark:text-gray-400">Размер:</span>
            <span class="text-right">{{ formatVitola(cigar.lengthMm, cigar.diameter) }}</span>
          </div>
          <div
            v-if="cigar.strength"
            class="flex justify-between items-center">
            <span class="font-medium text-gray-600 dark:text-gray-400">Крепость:</span>
            <span
              class="px-2 py-1 rounded-full text-xs font-medium"
              :class="getStrengthBadgeClass(cigar.strength)">
              {{ getStrengthLabel(cigar.strength) }}
            </span>
          </div>
        </div>

        <div class="space-y-4">
          <h3 class="text-xl font-semibold border-b pb-2">Структура</h3>
          <div
            v-if="cigar.wrapper"
            class="flex justify-between">
            <span class="font-medium text-gray-600 dark:text-gray-400">Покровный лист:</span>
            <span class="text-right">{{ cigar.wrapper }}</span>
          </div>
          <div
            v-if="cigar.binder"
            class="flex justify-between">
            <span class="font-medium text-gray-600 dark:text-gray-400">Связующий лист:</span>
            <span class="text-right">{{ cigar.binder }}</span>
          </div>
          <div
            v-if="cigar.filler"
            class="flex justify-between">
            <span class="font-medium text-gray-600 dark:text-gray-400">Наполнитель:</span>
            <span class="text-right">{{ cigar.filler }}</span>
          </div>
        </div>
      </div>

      <!-- Описание -->
      <div v-if="cigar.description">
        <h3 class="text-xl font-semibold border-b pb-2 mb-3">Описание</h3>
        <p class="text-gray-700 dark:text-gray-300 leading-relaxed">
          {{ cigar.description }}
        </p>
      </div>

      <CigarCommentsPanel :cigar-base-id="cigar.id" />
    </div>

    <div
      v-else
      class="text-center p-8">
      <i class="pi pi-spin pi-spinner text-4xl text-gray-400"></i>
      <p class="mt-2 text-gray-500">Загрузка данных...</p>
    </div>

    <template #footer>
      <div class="flex flex-wrap justify-end gap-2">
        <Button
          v-if="canEditCatalog"
          label="Редактировать"
          icon="pi pi-pencil"
          class="p-button-secondary"
          @click="editBaseCigar"
          v-tooltip.top="'Редактировать базовую информацию'" />
        <Button
          label="Написать отзыв"
          icon="pi pi-comment"
          class="p-button-info"
          @click="writeReview"
          v-tooltip.top="'Оставить свой отзыв об этой сигаре'" />
        <Button
          label="В коллекцию"
          icon="pi pi-plus"
          class="p-button-success"
          @click="addToCollection"
          v-tooltip.top="'Добавить экземпляр в свою коллекцию'" />
        <Button
          label="Создать похожую"
          icon="pi pi-copy"
          class="p-button-secondary"
          @click="createSimilarCigar"
          v-tooltip.top="'Новая сигара с теми же характеристиками'" />
        <Button
          label="Закрыть"
          icon="pi pi-times"
          class="p-button-text"
          @click="close" />
      </div>
    </template>
  </Dialog>
</template>
