<template>
  <section
    class="cigar-edit-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-50 via-rose-50/40 to-stone-50 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-rose-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="cigar-collection-edit"
    aria-labelledby="cigar-edit-heading">
    <div
      class="cigar-edit-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto max-w-4xl">
      <header class="flex flex-col gap-4 pb-6 sm:flex-row sm:items-end sm:justify-between sm:pb-8">
        <div class="min-w-0">
          <p
            class="mb-1.5 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55">
            Коллекция
          </p>
          <h1
            id="cigar-edit-heading"
            class="text-balance text-3xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 sm:text-4xl">
            Редактировать в коллекции
          </h1>
          <p class="mt-1.5 max-w-xl text-pretty text-sm text-stone-600 dark:text-stone-400">
            Редактируются поля коллекции: цена, количество, оценка, хьюмидор, заметки и личные фото.
          </p>
        </div>
        <Button
          data-testid="cigar-edit-back"
          class="min-h-12 w-full shrink-0 touch-manipulation sm:min-h-11 sm:w-auto"
          icon="pi pi-arrow-left"
          label="Назад"
          severity="secondary"
          outlined
          @click="router.push({ name: 'CigarDetail', params: { id: route.params.id } })" />
      </header>

      <div
        v-if="pageLoading"
        class="min-h-[16rem] space-y-5 rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md dark:border-stone-700/90 dark:bg-stone-900/85 sm:p-6"
        data-testid="cigar-edit-loading"
        aria-busy="true">
        <div
          v-for="n in 5"
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
        v-else-if="loadError"
        class="max-w-2xl rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80"
        data-testid="cigar-edit-error"
        role="alert">
        <Message
          severity="error"
          :closable="false">
          {{ loadError }}
        </Message>
        <Button
          class="mt-4 min-h-12 w-full touch-manipulation sm:w-auto"
          label="К списку"
          icon="pi pi-list"
          severity="secondary"
          outlined
          @click="router.push({ name: 'CigarList' })" />
      </div>

      <div
        v-else-if="cigar"
        class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/40 sm:p-6 cigar-edit-enter">
        <Message
          v-if="saveError"
          class="mb-6"
          severity="error"
          :closable="false">
          {{ saveError }}
        </Message>

        <div
          class="mb-6 rounded-xl border border-stone-200/70 bg-stone-50/50 p-4 dark:border-stone-700/60 dark:bg-stone-950/35">
          <p class="text-xs font-medium uppercase tracking-wide text-stone-500 dark:text-stone-400">Из справочника</p>
          <p class="mt-1 text-lg font-semibold text-stone-900 dark:text-rose-50/95">{{ cigar.name }}</p>
          <p class="text-sm text-stone-600 dark:text-stone-400">{{ cigar.brand.name }}</p>
        </div>

        <form
          class="flex flex-col gap-6 sm:gap-8"
          @submit.prevent="handleSubmit">
          <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
            <div class="flex flex-col gap-2 md:col-span-2">
              <label
                for="edit-price"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Цена (₽)
              </label>
              <InputNumber
                id="edit-price"
                v-model="form.price"
                class="flex! w-full"
                input-class="min-h-11"
                :min-fraction-digits="2"
                :max-fraction-digits="2"
                suffix=" ₽"
                fluid />
            </div>
            <div class="flex flex-col gap-2">
              <label
                for="edit-quantity"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Количество (шт.)
              </label>
              <InputNumber
                id="edit-quantity"
                v-model="form.quantity"
                data-testid="cigar-edit-quantity"
                class="flex! w-full"
                input-class="min-h-11"
                :min="1"
                :max="9999"
                :step="1"
                show-buttons
                button-layout="horizontal"
                fluid />
            </div>
            <div class="flex flex-col gap-2 md:col-span-2">
              <label
                for="edit-rating"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Оценка
              </label>
              <div class="flex flex-col gap-2 sm:flex-row sm:flex-wrap sm:items-center sm:gap-4">
                <Rating
                  id="edit-rating"
                  :model-value="form.rating ?? undefined"
                  data-testid="cigar-edit-rating"
                  :stars="10"
                  @update:model-value="(v) => (form.rating = v ?? null)" />
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
                for="edit-taste"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Вкус
              </label>
              <InputText
                id="edit-taste"
                v-model="form.taste"
                class="min-h-11 w-full" />
            </div>
            <div class="flex flex-col gap-2">
              <label
                for="edit-aroma"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Аромат
              </label>
              <InputText
                id="edit-aroma"
                v-model="form.aroma"
                class="min-h-11 w-full" />
            </div>
          </div>

          <div class="flex flex-col gap-2">
            <label
              for="edit-humidor"
              class="text-xs font-medium text-stone-600 dark:text-stone-400">
              Хьюмидор
            </label>
            <Select
              label-id="edit-humidor"
              v-model="form.humidorId"
              class="w-full"
              label-class="min-h-11"
              :options="humidors"
              option-label="name"
              option-value="id"
              placeholder="Не в хьюмидоре"
              show-clear
              :loading="humidorsLoading" />
          </div>

          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-4 text-lg font-semibold text-stone-900 dark:text-rose-50/95">Фото в коллекции</h2>
            <div class="flex flex-col gap-6 lg:grid lg:grid-cols-[minmax(0,14rem)_1fr] lg:items-start lg:gap-8">
              <div class="flex flex-col gap-2 lg:sticky lg:top-4">
                <span class="text-xs font-medium text-stone-600 dark:text-stone-400">Предпросмотр</span>
                <div
                  class="cigar-edit-image-frame relative aspect-square w-full max-w-[14rem] overflow-hidden rounded-xl border border-stone-200/90 bg-stone-100/90 dark:border-stone-600/80 dark:bg-stone-900/60"
                  data-testid="cigar-edit-image-preview">
                  <div class="absolute inset-0 flex items-center justify-center p-2">
                    <img
                      v-if="galleryPreviewSrc"
                      :src="galleryPreviewSrc"
                      alt=""
                      class="max-h-full max-w-full object-contain"
                      loading="lazy"
                      decoding="async" />
                    <div
                      v-else
                      class="flex flex-col items-center gap-2 px-3 py-6 text-center text-stone-400 dark:text-stone-500">
                      <i
                        class="pi pi-image text-3xl opacity-70"
                        aria-hidden="true" />
                      <span class="text-xs leading-snug">Добавьте фото справа — превью первого кадра</span>
                    </div>
                  </div>
                </div>
              </div>
              <FormImageGallerySection
                v-model="galleryImages"
                variant="bare"
                tone="review"
                url-entry-mode="multi"
                :max-files="maxGalleryImages"
                :max-url-rows="maxGalleryImages"
                :show-main-image-star="true"
                test-id="cigar-edit-image-gallery"
                url-input-id="cigar-edit-gallery-url"
                url-field-test-id="cigar-edit-image-url"
                url-rows-test-id="cigar-edit-image-urls"
                add-url-row-test-id="cigar-edit-add-image-url"
                apply-urls-to-gallery-test-id="cigar-edit-apply-image-gallery"
                url-placeholder="https://example.com/cigar.jpg"
                url-help-text="До 12 кадров: ссылки или файлы, «Добавить в галерею»; при сохранении сервер примет URL и data URL. У сохранённых фото звезда сразу назначает главное на сервере."
                grid-class="grid grid-cols-2 gap-3 sm:grid-cols-3 sm:gap-4 md:grid-cols-4"
                @existing-main-set="onExistingMainSet" />
            </div>
          </div>

          <div
            class="flex flex-col-reverse gap-3 border-t border-stone-200/80 pt-6 dark:border-stone-700/80 sm:flex-row sm:justify-end">
            <Button
              type="button"
              label="Отмена"
              severity="secondary"
              outlined
              class="min-h-12 w-full sm:w-auto"
              @click="router.push({ name: 'CigarDetail', params: { id: route.params.id } })" />
            <Button
              type="submit"
              label="Сохранить"
              icon="pi pi-check"
              class="min-h-12 w-full sm:w-auto"
              :loading="saving" />
          </div>
        </form>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted, onUnmounted } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { useToast } from 'primevue/usetoast';
  import api from '@/services/api';
  import cigarService from '@/services/cigarService';
  import humidorService from '@/services/humidorService';
  import type { Cigar } from '@/services/cigarService';
  import type { Humidor } from '@/services/humidorService';
  import Button from 'primevue/button';
  import Select from 'primevue/select';
  import InputNumber from 'primevue/inputnumber';
  import InputText from 'primevue/inputtext';
  import Rating from 'primevue/rating';
  import Message from 'primevue/message';
  import Skeleton from 'primevue/skeleton';
  import FormImageGallerySection, { type FormGalleryImageItem } from '@/components/FormImageGallerySection.vue';

  const TRANSPARENT_PIXEL = 'data:image/gif;base64,R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7';

  const route = useRoute();
  const router = useRouter();
  const toast = useToast();

  const maxGalleryImages = 12;

  const pageLoading = ref(true);
  const saving = ref(false);
  const loadError = ref<string | null>(null);
  const saveError = ref<string | null>(null);
  const cigar = ref<Cigar | null>(null);
  const humidors = ref<Humidor[]>([]);
  const humidorsLoading = ref(false);
  const galleryImages = ref<FormGalleryImageItem[]>([]);

  const form = ref({
    price: null as number | null,
    quantity: 1,
    rating: null as number | null,
    humidorId: null as number | null,
    taste: '',
    aroma: '',
  });

  const galleryPreviewSrc = computed(() => {
    const active = galleryImages.value.find((i) => !i.markedForDeletion);
    return active?.preview?.trim() ? active.preview : '';
  });

  function revokeGalleryPreviews(items: FormGalleryImageItem[]): void {
    for (const img of items) {
      if (img.preview.startsWith('blob:')) {
        URL.revokeObjectURL(img.preview);
      }
    }
  }

  function readFileAsDataUrl(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = () => reject(reader.error ?? new Error('FileReader'));
      reader.readAsDataURL(file);
    });
  }

  async function buildGalleryFromCigar(c: Cigar): Promise<FormGalleryImageItem[]> {
    const imgs = (c.images ?? []).filter((i) => i.userCigarId != null);
    const out: FormGalleryImageItem[] = [];
    for (const img of imgs) {
      let preview = TRANSPARENT_PIXEL;
      try {
        const { data } = await api.get<Blob>(`cigarimages/${img.id}/thumbnail`, { responseType: 'blob' });
        preview = URL.createObjectURL(data);
      } catch {
        /* миниатюра необязательна */
      }
      out.push({
        id: img.id,
        preview,
        caption: '',
        isExisting: true,
        markedForDeletion: false,
        isMain: Boolean(img.isMain),
      });
    }
    return out;
  }

  async function onExistingMainSet(imageId: number): Promise<void> {
    try {
      await cigarService.setCigarImageMain(imageId);
      const imgs = cigar.value?.images;
      if (imgs?.length) {
        for (const im of imgs) {
          im.isMain = im.id === imageId;
        }
      }
      for (const g of galleryImages.value) {
        if (g.isExisting && g.id != null) {
          g.isMain = g.id === imageId;
        }
      }
      toast.add({ severity: 'success', summary: 'Главное фото', life: 2000 });
    } catch {
      toast.add({ severity: 'error', summary: 'Не удалось назначить главное', life: 4000 });
      await load();
    }
  }

  async function collectGalleryPayload(): Promise<{
    imageUrlsToAdd: string[] | undefined;
    imageIdsToRemove: number[] | undefined;
  }> {
    const removed: number[] = [];
    for (const img of galleryImages.value) {
      if (img.isExisting && img.markedForDeletion && img.id != null) {
        removed.push(img.id);
      }
    }
    const activeNew = galleryImages.value.filter((i) => !i.isExisting && !i.markedForDeletion);
    const mainIdx = activeNew.findIndex((i) => i.isMain);
    let orderedNew = activeNew;
    if (mainIdx > 0) {
      const m = activeNew[mainIdx]!;
      orderedNew = [...activeNew.slice(0, mainIdx), ...activeNew.slice(mainIdx + 1)];
      orderedNew = [m, ...orderedNew];
    }

    const urls: string[] = [];
    for (const img of orderedNew) {
      let u = img.imageUrl?.trim();
      if (img.file) {
        u = await readFileAsDataUrl(img.file);
      }
      if (u) {
        urls.push(u);
      }
    }
    return {
      imageIdsToRemove: removed.length > 0 ? removed : undefined,
      imageUrlsToAdd: urls.length > 0 ? urls : undefined,
    };
  }

  async function load(): Promise<void> {
    pageLoading.value = true;
    loadError.value = null;
    const id = route.params.id as string;
    try {
      const c = await cigarService.getCigar(id);
      cigar.value = c;
      const q = c.quantity;
      const quantity = q != null && Number.isFinite(q) ? Math.min(9999, Math.max(1, Math.trunc(q))) : 1;
      revokeGalleryPreviews(galleryImages.value);
      galleryImages.value = await buildGalleryFromCigar(c);
      form.value = {
        price: c.price ?? null,
        quantity,
        rating: c.rating ?? null,
        humidorId: c.humidorId ?? null,
        taste: c.taste ?? '',
        aroma: c.aroma ?? '',
      };
    } catch {
      loadError.value = 'Не удалось загрузить сигару.';
      revokeGalleryPreviews(galleryImages.value);
      galleryImages.value = [];
    } finally {
      pageLoading.value = false;
    }
  }

  async function handleSubmit(): Promise<void> {
    if (!cigar.value) return;
    saving.value = true;
    saveError.value = null;
    const idNum = parseInt(route.params.id as string, 10);
    try {
      const { imageUrlsToAdd, imageIdsToRemove } = await collectGalleryPayload();
      await cigarService.updateCigar(idNum, {
        price: form.value.price,
        humidorId: form.value.humidorId,
        taste: form.value.taste,
        aroma: form.value.aroma,
        rating: form.value.rating,
        quantity: form.value.quantity ?? 1,
        imageUrlsToAdd,
        imageIdsToRemove,
      });
      toast.add({ severity: 'success', summary: 'Сохранено', life: 2500 });
      await router.push({ name: 'CigarDetail', params: { id: route.params.id } });
    } catch {
      saveError.value = 'Не удалось сохранить.';
      toast.add({ severity: 'error', summary: 'Ошибка', detail: saveError.value, life: 4000 });
    } finally {
      saving.value = false;
    }
  }

  onUnmounted(() => {
    revokeGalleryPreviews(galleryImages.value);
  });

  onMounted(() => {
    void load();
    humidorsLoading.value = true;
    humidorService
      .getHumidors()
      .then((h) => {
        humidors.value = h;
      })
      .finally(() => {
        humidorsLoading.value = false;
      });
  });
</script>

<style scoped>
  .cigar-edit-root {
    position: relative;
    isolation: isolate;
  }

  .cigar-edit-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  .cigar-edit-enter {
    animation: cigar-edit-in 0.4s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes cigar-edit-in {
    from {
      opacity: 0;
      transform: translateY(6px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  @media (prefers-reduced-motion: reduce) {
    .cigar-edit-enter {
      animation: none;
    }
  }
</style>
