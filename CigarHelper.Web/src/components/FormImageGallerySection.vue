<script lang="ts">
  export interface FormGalleryImageItem {
    id?: number;
    file?: File;
    preview: string;
    imageUrl?: string;
    caption: string;
    isExisting: boolean;
    markedForDeletion: boolean;
    /** Главное фото (например база сигары); для обзоров не задаётся */
    isMain?: boolean;
  }
</script>

<script setup lang="ts">
  import { computed, onUnmounted, ref } from 'vue';
  import { useToast } from 'primevue/usetoast';
  import ImageUploader from './ImageUploader.vue';
  import InputText from 'primevue/inputtext';
  import Button from 'primevue/button';

  const props = withDefaults(
    defineProps<{
      maxFiles?: number;
      variant?: 'section' | 'bare';
      title?: string;
      description?: string;
      showCaptions?: boolean;
      showMainImageStar?: boolean;
      urlHelpText?: string;
      urlPlaceholder?: string;
      testId?: string;
      urlInputId?: string;
      /** data-testid поля URL (по умолчанию общий) */
      urlFieldTestId?: string;
      /** Префикс для caption: `{prefix}-image-caption-{index}` */
      captionTestIdPrefix?: string;
      resolveUrlImport?: (url: string) => Promise<FormGalleryImageItem | null>;
      gridClass?: string;
      tone?: 'review' | 'dialog';
      /** Одно поле URL или несколько строк как в CigarForm */
      urlEntryMode?: 'single' | 'multi';
      maxUrlRows?: number;
      multiUrlLabel?: string;
      addUrlRowButtonLabel?: string;
      applyUrlsToGalleryButtonLabel?: string;
      urlRowsTestId?: string;
      addUrlRowTestId?: string;
      applyUrlsToGalleryTestId?: string;
    }>(),
    {
      maxFiles: 5,
      variant: 'bare',
      showCaptions: false,
      showMainImageStar: false,
      urlHelpText: 'Сервер скачает изображение по HTTP(S); файлы с устройства сохраняются при отправке формы.',
      urlPlaceholder: 'https://…',
      urlInputId: 'form-gallery-image-url',
      urlFieldTestId: 'form-gallery-image-url',
      captionTestIdPrefix: 'form-gallery',
      gridClass: 'grid grid-cols-2 gap-3 sm:grid-cols-3 sm:gap-4 md:grid-cols-4 lg:grid-cols-5',
      tone: 'review',
      urlEntryMode: 'single',
      maxUrlRows: 12,
      multiUrlLabel: 'Ссылки на изображения',
      addUrlRowButtonLabel: 'Добавить ссылку',
      applyUrlsToGalleryButtonLabel: 'Добавить в галерею',
      urlRowsTestId: 'form-gallery-multi-urls',
      addUrlRowTestId: 'form-gallery-add-url-row',
      applyUrlsToGalleryTestId: 'form-gallery-apply-urls',
    },
  );

  const images = defineModel<FormGalleryImageItem[]>({ required: true });

  const toast = useToast();
  const uploaderRef = ref<InstanceType<typeof ImageUploader> | null>(null);
  const newImageUrl = ref('');
  const urlImportBusy = ref(false);
  /** Черновые строки URL (режим multi) */
  const pendingUrls = ref<string[]>(['']);

  const activeCount = computed(() => images.value.filter((img) => !img.markedForDeletion).length);

  const hasAnyPendingUrl = computed(() => pendingUrls.value.some((u) => u.trim().length > 0));

  const labelClass = computed(() =>
    props.tone === 'dialog'
      ? 'block text-sm font-medium text-gray-700 dark:text-gray-300'
      : 'text-xs font-medium text-stone-600 dark:text-stone-400',
  );

  const helpTextClass = computed(() =>
    props.tone === 'dialog' ? 'text-sm text-gray-500' : 'text-xs text-stone-500 dark:text-stone-500',
  );

  const cellFrameClass = computed(() =>
    props.tone === 'dialog'
      ? 'relative aspect-square overflow-hidden rounded-lg bg-slate-100 dark:bg-slate-800/80'
      : 'relative aspect-square overflow-hidden rounded-xl border border-stone-200/80 bg-stone-100 dark:border-stone-600/80 dark:bg-stone-900/60',
  );

  const emptyBoxClass = computed(() =>
    props.tone === 'dialog'
      ? 'rounded-lg border-2 border-dashed py-8 text-center'
      : 'rounded-xl border-2 border-dashed border-stone-300/90 py-10 text-center dark:border-stone-600/80',
  );

  const emptyIconClass = computed(() =>
    props.tone === 'dialog'
      ? 'pi pi-image mb-2 text-4xl text-gray-400'
      : 'pi pi-image mb-2 text-4xl text-stone-400 dark:text-stone-500',
  );

  const emptyTextClass = computed(() =>
    props.tone === 'dialog' ? 'text-sm text-gray-500' : 'text-sm text-stone-500 dark:text-stone-400',
  );

  const urlRowClass = computed(() =>
    props.tone === 'dialog' ? 'flex gap-2' : 'flex flex-col gap-2 sm:flex-row sm:items-stretch',
  );

  const urlInputClass = computed(() => (props.tone === 'dialog' ? 'flex-1 w-full' : 'min-h-11 w-full flex-1'));

  function revokeBlob(preview: string): void {
    if (preview.startsWith('blob:')) {
      URL.revokeObjectURL(preview);
    }
  }

  onUnmounted(() => {
    images.value.forEach((img) => revokeBlob(img.preview));
  });

  function openPicker(): void {
    uploaderRef.value?.open();
  }

  function handleFilesSelected(files: File[]): void {
    const remaining = props.maxFiles - activeCount.value;
    if (remaining <= 0) {
      toast.add({
        severity: 'warn',
        summary: 'Лимит изображений',
        detail: `Можно не более ${props.maxFiles} изображений.`,
        life: 4000,
      });
      return;
    }
    const slice = files.slice(0, remaining);
    slice.forEach((file) => {
      const item: FormGalleryImageItem = {
        file,
        preview: URL.createObjectURL(file),
        caption: '',
        isExisting: false,
        markedForDeletion: false,
      };
      if (props.showMainImageStar) {
        item.isMain = false;
      }
      images.value.push(item);
    });
    if (files.length > remaining) {
      toast.add({
        severity: 'warn',
        summary: 'Лимит изображений',
        detail: `Добавлено ${slice.length} из ${files.length} файлов (лимит ${props.maxFiles}).`,
        life: 4000,
      });
    }
  }

  function isUrlAlreadyInGallery(url: string): boolean {
    const t = url.trim();
    return images.value.some(
      (img) => !img.markedForDeletion && (img.imageUrl === t || (!img.file && img.preview === t)),
    );
  }

  function addPendingUrlRow(): void {
    if (pendingUrls.value.length >= props.maxUrlRows) return;
    pendingUrls.value.push('');
  }

  function removePendingUrlRow(index: number): void {
    pendingUrls.value.splice(index, 1);
    if (pendingUrls.value.length === 0) {
      pendingUrls.value.push('');
    }
  }

  function isValidHttpUrl(raw: string): boolean {
    try {
      const parsed = new URL(raw.trim());
      return parsed.protocol === 'http:' || parsed.protocol === 'https:';
    } catch {
      return false;
    }
  }

  async function addByUrl(): Promise<void> {
    const raw = newImageUrl.value.trim();
    if (!raw) return;
    if (activeCount.value >= props.maxFiles) {
      toast.add({
        severity: 'warn',
        summary: 'Лимит',
        detail: `Не более ${props.maxFiles} изображений.`,
        life: 3000,
      });
      return;
    }

    if (props.resolveUrlImport) {
      urlImportBusy.value = true;
      try {
        const item = await props.resolveUrlImport(raw);
        if (item) {
          images.value.push(item);
          newImageUrl.value = '';
        }
      } finally {
        urlImportBusy.value = false;
      }
      return;
    }

    if (!isValidHttpUrl(raw)) {
      toast.add({
        severity: 'warn',
        summary: 'Ссылка',
        detail: 'Укажите корректный адрес с протоколом http или https.',
        life: 4000,
      });
      return;
    }

    urlImportBusy.value = true;
    try {
      images.value.push({
        preview: raw,
        imageUrl: raw,
        caption: '',
        isExisting: false,
        markedForDeletion: false,
      });
      newImageUrl.value = '';
      toast.add({
        severity: 'success',
        summary: 'Добавлено',
        detail: 'Изображение будет сохранено при отправке формы.',
        life: 2500,
      });
    } finally {
      urlImportBusy.value = false;
    }
  }

  async function addAllPendingUrlsToGallery(): Promise<void> {
    const candidates = pendingUrls.value.map((u) => u.trim()).filter(Boolean);
    if (candidates.length === 0) return;

    urlImportBusy.value = true;
    let added = 0;
    let skipped = 0;
    let stoppedByLimit = false;

    try {
      for (const raw of candidates) {
        if (activeCount.value >= props.maxFiles) {
          stoppedByLimit = true;
          break;
        }
        if (isUrlAlreadyInGallery(raw)) {
          skipped++;
          continue;
        }

        if (props.resolveUrlImport) {
          const item = await props.resolveUrlImport(raw);
          if (item) {
            images.value.push(item);
            added++;
          } else {
            skipped++;
          }
        } else if (isValidHttpUrl(raw)) {
          images.value.push({
            preview: raw,
            imageUrl: raw,
            caption: '',
            isExisting: false,
            markedForDeletion: false,
          });
          added++;
        } else {
          skipped++;
        }
      }

      if (added > 0) {
        pendingUrls.value = [''];
        toast.add({
          severity: 'success',
          summary: 'Добавлено',
          detail: added === 1 ? 'В галерею добавлено 1 изображение.' : `В галерею добавлено изображений: ${added}.`,
          life: 2500,
        });
      }

      if (stoppedByLimit) {
        toast.add({
          severity: 'warn',
          summary: 'Лимит',
          detail: `В галерее не более ${props.maxFiles} изображений. Остальные ссылки не добавлены.`,
          life: 4500,
        });
      } else if (added === 0 && skipped > 0) {
        toast.add({
          severity: 'warn',
          summary: 'Ссылки',
          detail: 'Не удалось добавить ни одной ссылки. Проверьте формат (http/https) и дубликаты.',
          life: 4000,
        });
      } else if (skipped > 0 && added > 0) {
        toast.add({
          severity: 'info',
          summary: 'Часть ссылок пропущена',
          detail: 'Пропущены неверные, повторяющиеся или уже добавленные адреса.',
          life: 3500,
        });
      }
    } finally {
      urlImportBusy.value = false;
    }
  }

  function removeAt(index: number): void {
    const image = images.value[index];
    if (!image) return;
    if (image.isExisting) {
      image.markedForDeletion = true;
      if (props.showMainImageStar && image.isMain) {
        const next = images.value.find((img, i) => i !== index && !img.markedForDeletion);
        if (next) {
          next.isMain = true;
        }
      }
    } else {
      revokeBlob(image.preview);
      images.value.splice(index, 1);
    }
  }

  function restoreAt(index: number): void {
    const image = images.value[index];
    if (!image?.markedForDeletion) return;
    image.markedForDeletion = false;
    if (props.showMainImageStar) {
      const hasMain = images.value.some((img) => img.isMain && !img.markedForDeletion);
      if (!hasMain) {
        image.isMain = true;
      }
    }
  }

  function setMainAt(index: number): void {
    if (!props.showMainImageStar) return;
    images.value.forEach((img, i) => {
      img.isMain = i === index;
    });
  }

  function showStarButton(image: FormGalleryImageItem): boolean {
    return props.showMainImageStar && Boolean(image.isExisting || image.id);
  }

  defineExpose({ openPicker });
</script>

<template>
  <div
    :class="
      variant === 'section'
        ? 'rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6'
        : undefined
    ">
    <template v-if="variant === 'section'">
      <h2
        v-if="title"
        class="mb-1 flex items-center gap-2 text-lg font-semibold text-stone-900 dark:text-rose-50/95">
        <i
          class="pi pi-images text-rose-700 dark:text-rose-400"
          aria-hidden="true" />
        {{ title }}
      </h2>
      <p
        v-if="description"
        class="mb-4 text-sm text-stone-600 dark:text-stone-400">
        {{ description }}
      </p>
    </template>

    <div
      :data-testid="testId"
      class="space-y-4">
      <ImageUploader
        ref="uploaderRef"
        :max-files="maxFiles"
        :current-file-count="activeCount"
        @files-selected="handleFilesSelected" />

      <div
        v-if="urlEntryMode === 'single'"
        class="space-y-2">
        <label
          :for="urlInputId"
          :class="labelClass">
          Добавить изображение по ссылке
        </label>
        <div :class="urlRowClass">
          <InputText
            :id="urlInputId"
            v-model="newImageUrl"
            :data-testid="urlFieldTestId"
            :placeholder="urlPlaceholder"
            :class="urlInputClass"
            :disabled="urlImportBusy" />
          <Button
            label="Добавить"
            icon="pi pi-link"
            severity="success"
            :class="tone === 'dialog' ? undefined : 'min-h-11 w-full shrink-0 sm:w-auto'"
            :loading="urlImportBusy"
            :disabled="!newImageUrl.trim() || urlImportBusy || activeCount >= maxFiles"
            type="button"
            @click="addByUrl" />
        </div>
        <p :class="helpTextClass">
          {{ urlHelpText }}
        </p>
      </div>

      <div
        v-else
        class="space-y-2">
        <span :class="labelClass">{{ multiUrlLabel }}</span>
        <div
          class="flex flex-col gap-2"
          :data-testid="urlRowsTestId">
          <div
            v-for="(_u, idx) in pendingUrls"
            :key="'gallery-pending-url-' + idx"
            class="flex items-center gap-2">
            <InputText
              v-model="pendingUrls[idx]"
              class="min-h-11 min-w-0 flex-1"
              :placeholder="urlPlaceholder"
              :data-testid="idx === 0 ? urlFieldTestId : `${urlFieldTestId}-${idx}`"
              :disabled="urlImportBusy" />
            <Button
              type="button"
              class="min-h-11 min-w-11 shrink-0 touch-manipulation"
              icon="pi pi-trash"
              text
              rounded
              severity="secondary"
              aria-label="Удалить строку"
              :disabled="urlImportBusy"
              @click="removePendingUrlRow(idx)" />
          </div>
          <div class="flex flex-col gap-2 sm:flex-row sm:flex-wrap sm:items-center">
            <Button
              type="button"
              class="min-h-11 w-full touch-manipulation sm:w-auto"
              :label="addUrlRowButtonLabel"
              icon="pi pi-plus"
              severity="secondary"
              outlined
              :data-testid="addUrlRowTestId"
              :disabled="pendingUrls.length >= maxUrlRows || urlImportBusy"
              @click="addPendingUrlRow" />
            <Button
              type="button"
              class="min-h-11 w-full touch-manipulation sm:w-auto"
              :label="applyUrlsToGalleryButtonLabel"
              icon="pi pi-images"
              severity="success"
              :class="tone === 'dialog' ? undefined : 'sm:ml-0'"
              :loading="urlImportBusy"
              :data-testid="applyUrlsToGalleryTestId"
              :disabled="!hasAnyPendingUrl || urlImportBusy || activeCount >= maxFiles"
              @click="addAllPendingUrlsToGallery" />
          </div>
        </div>
        <p :class="helpTextClass">
          {{ urlHelpText }}
        </p>
      </div>

      <Button
        label="Выбрать файлы для загрузки"
        icon="pi pi-upload"
        outlined
        :severity="tone === 'review' ? 'secondary' : undefined"
        :class="tone === 'review' ? 'min-h-11 w-full sm:w-auto' : undefined"
        type="button"
        :disabled="activeCount >= maxFiles"
        @click="openPicker" />

      <div
        v-if="images.length > 0"
        :class="gridClass">
        <div
          v-for="(image, index) in images"
          :key="image.id ?? image.preview"
          class="group relative flex flex-col gap-2">
          <div :class="cellFrameClass">
            <div
              v-if="image.markedForDeletion"
              :class="
                tone === 'dialog'
                  ? 'absolute inset-0 z-10 flex flex-col items-center justify-center rounded-lg bg-red-500/70'
                  : 'absolute inset-0 z-10 flex flex-col items-center justify-center rounded-[inherit] bg-red-600/75 p-2 text-center'
              ">
              <i
                class="pi pi-trash mb-1 text-2xl text-white"
                aria-hidden="true" />
              <p :class="tone === 'dialog' ? 'font-semibold text-white' : 'text-xs font-semibold text-white'">
                Будет удалено
              </p>
              <Button
                icon="pi pi-undo"
                :rounded="tone === 'review'"
                :text="tone === 'review'"
                :class="
                  tone === 'dialog' ? 'p-button-rounded p-button-text p-button-sm mt-2 text-white' : 'mt-1 text-white'
                "
                type="button"
                v-tooltip.top="'Восстановить'"
                @click="restoreAt(index)" />
            </div>
            <img
              :src="image.preview"
              :alt="`Изображение ${index + 1}`"
              class="h-full w-full object-contain object-center transition-opacity duration-200"
              :class="
                tone === 'dialog'
                  ? { 'opacity-30': image.markedForDeletion }
                  : { 'opacity-35': image.markedForDeletion }
              "
              loading="lazy"
              decoding="async" />
            <div
              v-if="!image.markedForDeletion"
              :class="
                tone === 'dialog'
                  ? 'absolute inset-0 flex items-center justify-center rounded-lg bg-black/0 transition-all duration-300 group-hover:bg-black/50'
                  : 'absolute inset-0 flex items-center justify-center rounded-[inherit] bg-black/0 transition-colors duration-200 group-hover:bg-black/45'
              ">
              <div
                :class="
                  tone === 'dialog'
                    ? 'flex gap-2 opacity-0 transition-opacity duration-300 group-hover:opacity-100'
                    : 'flex gap-2 opacity-0 transition-opacity duration-200 group-hover:opacity-100'
                ">
                <Button
                  v-if="showStarButton(image)"
                  icon="pi pi-star"
                  rounded
                  severity="warning"
                  :outlined="!image.isMain"
                  :class="tone === 'dialog' ? 'p-button-sm' : '!h-9 !w-9'"
                  type="button"
                  v-tooltip.top="image.isMain ? 'Главное изображение' : 'Сделать главным'"
                  @click.stop="setMainAt(index)" />
                <Button
                  icon="pi pi-times"
                  :rounded="tone === 'review'"
                  severity="danger"
                  :class="tone === 'dialog' ? 'p-button-rounded p-button-danger p-button-sm' : '!h-9 !w-9'"
                  type="button"
                  v-tooltip.top="'Удалить'"
                  @click.stop="removeAt(index)" />
              </div>
            </div>
            <div
              v-if="image.isMain && !image.markedForDeletion && showMainImageStar"
              class="absolute left-2 top-2 rounded-full bg-yellow-500 px-2 py-1 text-xs font-bold text-white shadow-md">
              Главное
            </div>
          </div>
          <InputText
            v-if="showCaptions"
            v-model="image.caption"
            :data-testid="`${captionTestIdPrefix}-image-caption-${index}`"
            placeholder="Подпись (необязательно)"
            class="min-h-9 w-full text-xs"
            :disabled="image.markedForDeletion"
            maxlength="200" />
        </div>
      </div>
      <div
        v-else
        :class="emptyBoxClass">
        <i
          :class="emptyIconClass"
          aria-hidden="true" />
        <p :class="emptyTextClass">Нет добавленных изображений</p>
      </div>
    </div>
  </div>
</template>
