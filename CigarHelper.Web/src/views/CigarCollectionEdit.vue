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
            Карточка справочника не меняется — только цена, оценка, хьюмидор, ваши заметки и личные фото.
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
            <div class="flex flex-col gap-2 md:col-span-2">
              <label
                for="edit-rating"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Оценка
              </label>
              <div class="flex flex-col gap-2 sm:flex-row sm:flex-wrap sm:items-center sm:gap-4">
                <Rating
                  id="edit-rating"
                  v-model="form.rating"
                  data-testid="cigar-edit-rating"
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
            <Dropdown
              id="edit-humidor"
              v-model="form.humidorId"
              class="w-full"
              input-class="min-h-11"
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
              <div class="flex flex-col gap-2">
                <span class="text-xs font-medium text-stone-600 dark:text-stone-400">Предпросмотр</span>
                <div
                  class="cigar-edit-image-frame relative aspect-square w-full max-w-[14rem] overflow-hidden rounded-xl border border-stone-200/90 bg-stone-100/90 dark:border-stone-600/80 dark:bg-stone-900/60">
                  <div class="absolute inset-0 flex items-center justify-center p-2">
                    <img
                      v-if="previewSrc"
                      :src="previewSrc"
                      alt=""
                      class="max-h-full max-w-full object-contain"
                      loading="lazy" />
                    <span
                      v-else
                      class="text-xs text-stone-400 dark:text-stone-500"
                      >Нет превью</span
                    >
                  </div>
                </div>
              </div>
              <div class="flex flex-col gap-4">
                <div
                  v-if="editingUserImages.length > 0"
                  class="flex flex-col gap-2">
                  <span class="text-xs font-medium text-stone-600 dark:text-stone-400">Сохранённые</span>
                  <div class="flex flex-wrap gap-3">
                    <div
                      v-for="img in editingUserImages"
                      :key="img.id"
                      class="relative h-24 w-24 shrink-0 overflow-hidden rounded-xl border border-stone-200/90 bg-stone-100 dark:border-stone-600/80 dark:bg-stone-900/60">
                      <img
                        v-if="thumbUrls[img.id]"
                        :src="thumbUrls[img.id]"
                        alt=""
                        class="h-full w-full object-contain"
                        loading="lazy" />
                      <Button
                        v-if="!img.isMain"
                        type="button"
                        class="absolute left-0.5 top-0.5 h-8 min-h-8 w-8 min-w-8"
                        icon="pi pi-star"
                        text
                        rounded
                        severity="secondary"
                        :loading="settingMainId === img.id"
                        :disabled="settingMainId != null"
                        aria-label="Главное фото"
                        @click="makeMain(img.id)" />
                      <Button
                        type="button"
                        class="absolute right-0.5 top-0.5 h-8 min-h-8 w-8 min-w-8"
                        icon="pi pi-times"
                        text
                        rounded
                        severity="danger"
                        aria-label="Удалить"
                        @click="markRemove(img.id)" />
                    </div>
                  </div>
                </div>
                <div class="flex flex-col gap-2">
                  <span class="text-xs font-medium text-stone-600 dark:text-stone-400">Новые по URL</span>
                  <div
                    v-for="(_u, idx) in form.imageUrls"
                    :key="'u' + idx"
                    class="flex gap-2">
                    <InputText
                      v-model="form.imageUrls[idx]"
                      class="min-h-11 min-w-0 flex-1" />
                    <Button
                      type="button"
                      icon="pi pi-trash"
                      text
                      rounded
                      class="min-h-11 shrink-0"
                      @click="removeUrlRow(idx)" />
                  </div>
                  <Button
                    type="button"
                    label="Строка URL"
                    icon="pi pi-plus"
                    severity="secondary"
                    outlined
                    class="min-h-11 w-full sm:w-auto"
                    :disabled="form.imageUrls.length >= 12"
                    @click="addUrlRow" />
                </div>
              </div>
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
  import { ref, computed, watch, onMounted, onUnmounted } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { useToast } from 'primevue/usetoast';
  import api from '@/services/api';
  import cigarService from '@/services/cigarService';
  import humidorService from '@/services/humidorService';
  import type { Cigar } from '@/services/cigarService';
  import type { Humidor } from '@/services/humidorService';
  import Button from 'primevue/button';
  import Dropdown from 'primevue/dropdown';
  import InputNumber from 'primevue/inputnumber';
  import InputText from 'primevue/inputtext';
  import Rating from 'primevue/rating';
  import Message from 'primevue/message';
  import Skeleton from 'primevue/skeleton';

  const route = useRoute();
  const router = useRouter();
  const toast = useToast();

  const pageLoading = ref(true);
  const saving = ref(false);
  const loadError = ref<string | null>(null);
  const saveError = ref<string | null>(null);
  const cigar = ref<Cigar | null>(null);
  const humidors = ref<Humidor[]>([]);
  const humidorsLoading = ref(false);
  const thumbUrls = ref<Record<number, string>>({});
  const settingMainId = ref<number | null>(null);

  const form = ref({
    price: null as number | null,
    rating: null as number | null,
    humidorId: null as number | null,
    taste: '',
    aroma: '',
    imageUrls: [''] as string[],
    removedImageIds: [] as number[],
  });

  /** Личные фото коллекции (удаление / главное — только для них). */
  const editingUserImages = computed(() => {
    const imgs = cigar.value?.images ?? [];
    const rm = new Set(form.value.removedImageIds);
    return imgs.filter((i) => !rm.has(i.id) && i.userCigarId != null);
  });

  function revokeThumbs(): void {
    for (const u of Object.values(thumbUrls.value)) {
      if (u.startsWith('blob:')) URL.revokeObjectURL(u);
    }
    thumbUrls.value = {};
  }

  watch(
    () =>
      editingUserImages.value
        .map((i) => i.id)
        .sort((a, b) => a - b)
        .join(','),
    async () => {
      revokeThumbs();
      const next: Record<number, string> = {};
      for (const img of editingUserImages.value) {
        try {
          const { data } = await api.get<Blob>(`cigarimages/${img.id}/thumbnail`, { responseType: 'blob' });
          next[img.id] = URL.createObjectURL(data);
        } catch {
          /* ignore */
        }
      }
      thumbUrls.value = next;
    },
    { flush: 'post' },
  );

  const previewSrc = computed(() => {
    for (const raw of form.value.imageUrls) {
      const t = raw?.trim() ?? '';
      if (t && (/^https?:\/\//i.test(t) || t.startsWith('data:'))) {
        return t;
      }
    }
    for (const img of editingUserImages.value) {
      const u = thumbUrls.value[img.id];
      if (u) return u;
    }
    return '';
  });

  function addUrlRow(): void {
    if (form.value.imageUrls.length >= 12) return;
    form.value.imageUrls.push('');
  }

  function removeUrlRow(i: number): void {
    form.value.imageUrls.splice(i, 1);
    if (form.value.imageUrls.length === 0) form.value.imageUrls.push('');
  }

  function markRemove(id: number): void {
    if (!form.value.removedImageIds.includes(id)) {
      form.value.removedImageIds.push(id);
    }
  }

  async function makeMain(imageId: number): Promise<void> {
    if (settingMainId.value != null) return;
    settingMainId.value = imageId;
    try {
      await cigarService.setCigarImageMain(imageId);
      const imgs = cigar.value?.images;
      if (imgs?.length) {
        for (const im of imgs) {
          im.isMain = im.id === imageId;
        }
      }
      toast.add({ severity: 'success', summary: 'Главное фото', life: 2000 });
    } catch {
      toast.add({ severity: 'error', summary: 'Не удалось назначить главное', life: 4000 });
    } finally {
      settingMainId.value = null;
    }
  }

  function collectNewUrls(): string[] {
    return form.value.imageUrls.map((u) => u.trim()).filter(Boolean);
  }

  async function load(): Promise<void> {
    pageLoading.value = true;
    loadError.value = null;
    const id = route.params.id as string;
    try {
      const c = await cigarService.getCigar(id);
      cigar.value = c;
      form.value = {
        price: c.price ?? null,
        rating: c.rating ?? null,
        humidorId: c.humidorId ?? null,
        taste: c.taste ?? '',
        aroma: c.aroma ?? '',
        imageUrls: [''],
        removedImageIds: [],
      };
    } catch {
      loadError.value = 'Не удалось загрузить сигару.';
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
      const newUrls = collectNewUrls();
      await cigarService.updateCigar(idNum, {
        price: form.value.price,
        humidorId: form.value.humidorId,
        taste: form.value.taste,
        aroma: form.value.aroma,
        rating: form.value.rating,
        imageUrlsToAdd: newUrls.length > 0 ? newUrls : undefined,
        imageIdsToRemove: form.value.removedImageIds.length > 0 ? [...form.value.removedImageIds] : undefined,
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
    revokeThumbs();
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
