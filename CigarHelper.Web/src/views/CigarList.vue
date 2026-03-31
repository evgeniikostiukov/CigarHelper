<template>
  <section
    class="cigar-list-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-50 via-rose-50/40 to-stone-50 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-rose-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="cigar-list"
    aria-labelledby="cigar-list-heading">
    <div
      class="cigar-list-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] max-w-7xl mx-auto">
      <header class="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between pb-6 sm:pb-8">
        <div class="min-w-0">
          <p
            class="text-[0.65rem] uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55 font-semibold mb-1.5">
            Коллекция
          </p>
          <h1
            id="cigar-list-heading"
            class="text-3xl sm:text-4xl font-semibold text-stone-900 dark:text-rose-50/95 tracking-tight text-balance">
            Мои сигары
          </h1>
          <p class="mt-1.5 text-sm text-stone-600 dark:text-stone-400 max-w-xl text-pretty">
            Каталог, фото и оценки — одним касанием к деталке, каруселью листаем снимки на месте.
          </p>
        </div>
        <Button
          data-testid="cigar-list-add"
          class="w-full sm:w-auto shrink-0 min-h-12 px-5 sm:min-h-11 touch-manipulation shadow-md shadow-rose-900/10 dark:shadow-black/40"
          @click="$router.push({ name: 'CigarNew' })"
          icon="pi pi-plus"
          label="Добавить сигару" />
      </header>

      <div
        v-if="loading"
        data-testid="cigar-list-loading"
        class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5 sm:gap-6 min-h-[20rem]"
        aria-busy="true"
        aria-live="polite">
        <Skeleton
          v-for="n in 3"
          :key="n"
          class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          height="22rem"
          data-testid="cigar-list-skeleton" />
      </div>

      <div
        v-else-if="error"
        class="rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80 max-w-2xl"
        data-testid="cigar-list-error"
        role="alert">
        <Message severity="error">{{ error }}</Message>
        <Button
          data-testid="cigar-list-retry"
          class="mt-4 min-h-12 w-full sm:w-auto touch-manipulation"
          label="Повторить загрузку"
          icon="pi pi-refresh"
          severity="secondary"
          outlined
          @click="loadCigars" />
      </div>

      <div
        v-else-if="cigars.length === 0"
        class="text-center rounded-2xl border border-dashed border-rose-800/25 bg-white/80 px-5 py-12 dark:border-rose-200/15 dark:bg-stone-900/60 max-w-xl mx-auto"
        data-testid="cigar-list-empty">
        <span
          class="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-rose-100/90 text-rose-900 dark:bg-rose-900/40 dark:text-rose-100"
          aria-hidden="true">
          <i class="pi pi-bookmark text-2xl" />
        </span>
        <h2 class="text-2xl font-semibold text-stone-900 dark:text-rose-50/95 mb-2">Пока пусто</h2>
        <p class="text-stone-600 dark:text-stone-400 mb-6 text-pretty">
          Добавьте первую сигару — бренд, формат и ваши заметки останутся под рукой.
        </p>
        <Button
          data-testid="cigar-list-empty-add"
          class="min-h-12 px-6 touch-manipulation"
          label="Добавить сигару"
          icon="pi pi-plus"
          @click="$router.push({ name: 'CigarNew' })" />
      </div>

      <div
        v-else
        class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5 sm:gap-6"
        data-testid="cigar-list-grid">
        <article
          v-for="(cigar, index) in cigars"
          :key="cigar.id"
          v-memo="memoKey(cigar)"
          :data-testid="`cigar-card-${cigar.id}`"
          class="cigar-card-enter group relative flex flex-col overflow-hidden rounded-2xl border border-stone-200/90 bg-white/95 shadow-md shadow-stone-900/5 transition-[box-shadow,transform] duration-300 hover:shadow-lg hover:shadow-rose-900/10 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 dark:hover:shadow-black/70 dark:hover:border-rose-900/30 min-h-[20rem] motion-reduce:transition-none motion-reduce:animate-none"
          :style="{ animationDelay: `${Math.min(index, 8) * 48}ms` }">
          <div
            class="relative z-20 shrink-0 h-48 rounded-t-2xl overflow-hidden border-b border-stone-100 dark:border-stone-700/80 bg-stone-100 dark:bg-stone-800/80">
            <Carousel
              :value="cigar.images ?? []"
              :num-visible="1"
              :num-scroll="1"
              class="cigar-carousel w-full h-full max-h-48"
              :show-indicators="(cigar.images?.length ?? 0) > 1"
              :show-navigators="(cigar.images?.length ?? 0) > 1">
              <template #item="slotProps">
                <button
                  type="button"
                  class="cigar-list-card-image-frame relative h-48 w-full min-h-0 cursor-pointer touch-manipulation border-0 bg-transparent p-0 text-left"
                  :aria-label="`Открыть сигару ${cigar.name}`"
                  @click="viewCigar(cigar)">
                  <div
                    class="cigar-list-card-image-inner absolute inset-0 box-border flex min-h-0 min-w-0 items-center justify-center p-2">
                    <img
                      v-if="carouselItemImageSrc(slotProps.data)"
                      :src="carouselItemImageSrc(slotProps.data)"
                      :alt="cigar.name"
                      loading="lazy"
                      decoding="async" />
                    <i
                      v-else
                      class="pi pi-image text-5xl text-stone-400 dark:text-stone-500"
                      aria-hidden="true" />
                  </div>
                </button>
              </template>
              <template #empty>
                <button
                  type="button"
                  class="cigar-list-card-image-frame relative h-48 w-full min-h-0 cursor-pointer touch-manipulation border-0 bg-transparent p-0 text-left"
                  :aria-label="`Открыть сигару ${cigar.name}`"
                  @click="viewCigar(cigar)">
                  <div
                    class="cigar-list-card-image-inner absolute inset-0 box-border flex min-h-0 min-w-0 items-center justify-center p-2">
                    <i
                      class="pi pi-image text-5xl text-stone-400 dark:text-stone-500"
                      aria-hidden="true" />
                  </div>
                </button>
              </template>
            </Carousel>
          </div>

          <router-link
            :to="{ name: 'CigarDetail', params: { id: String(cigar.id) } }"
            class="relative z-10 flex flex-1 flex-col gap-2.5 p-5 min-h-0 no-underline text-inherit focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-rose-700 dark:focus-visible:outline-rose-400 rounded-none">
            <h2
              class="text-lg sm:text-xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 pr-1 line-clamp-2">
              {{ cigar.name }}
            </h2>
            <p class="text-sm font-medium text-stone-700 dark:text-stone-300 line-clamp-1">
              {{ cigar.brand.name }}
            </p>
            <div class="flex flex-wrap items-center gap-x-3 gap-y-1 text-sm text-stone-600 dark:text-stone-400">
              <span v-if="cigar.size">{{ cigar.size }}</span>
              <span v-if="cigar.strength">{{ cigar.strength }}</span>
            </div>
            <div
              class="flex flex-wrap items-center justify-between gap-2 mt-auto pt-2 border-t border-stone-100 dark:border-stone-700/80">
              <div
                v-if="cigar.rating != null"
                class="flex items-center gap-1">
                <div
                  class="flex text-rose-500 dark:text-rose-400"
                  aria-hidden="true">
                  <svg
                    v-for="i in 5"
                    :key="i"
                    class="h-4 w-4 shrink-0"
                    :class="(cigar.rating ?? 0) >= i * 2 - 1 ? 'opacity-100' : 'opacity-25'"
                    fill="currentColor"
                    viewBox="0 0 20 20">
                    <path
                      d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                  </svg>
                </div>
                <span class="text-sm text-stone-600 dark:text-stone-400">{{ cigar.rating }}/10</span>
              </div>
              <span
                v-else
                class="text-sm text-stone-500 dark:text-stone-500"
                >Без оценки</span
              >
              <span
                v-if="cigar.price != null"
                class="text-sm font-semibold text-stone-900 dark:text-stone-100 whitespace-nowrap">
                {{ formatPrice(cigar.price) }}
              </span>
            </div>
            <div
              v-if="cigar.humidorId"
              class="flex items-center gap-2 text-sm text-stone-600 dark:text-stone-400 pt-1">
              <i
                class="pi pi-box text-rose-700 dark:text-rose-400 shrink-0"
                aria-hidden="true" />
              <span>В хьюмидоре</span>
            </div>
            <div
              v-if="cigar.isSmoked"
              class="flex items-center gap-2 text-sm text-emerald-700 dark:text-emerald-300 pt-1">
              <i
                class="pi pi-check-circle shrink-0"
                aria-hidden="true" />
              <span>Уже выкурена</span>
            </div>
          </router-link>

          <footer
            class="relative z-20 mt-auto flex justify-end gap-2 border-t border-stone-100 bg-stone-50/90 px-3 py-3 dark:border-stone-700/80 dark:bg-stone-950/50">
            <Button
              v-if="!cigar.isSmoked"
              :data-testid="`cigar-smoke-${cigar.id}`"
              class="min-h-11 min-w-11 touch-manipulation"
              icon="pi pi-check-circle"
              text
              rounded
              severity="success"
              aria-label="Отметить как выкуренную"
              @click="confirmMarkAsSmoked(cigar)" />
            <Button
              :data-testid="`cigar-edit-${cigar.id}`"
              class="min-h-11 min-w-11 touch-manipulation"
              icon="pi pi-pencil"
              text
              rounded
              severity="secondary"
              aria-label="Редактировать сигару"
              @click="$router.push({ name: 'CigarEdit', params: { id: String(cigar.id) } })" />
            <Button
              :data-testid="`cigar-delete-${cigar.id}`"
              class="min-h-11 min-w-11 touch-manipulation"
              icon="pi pi-trash"
              text
              rounded
              severity="danger"
              aria-label="Удалить сигару"
              @click="confirmDelete(cigar)" />
          </footer>
        </article>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import { useConfirm } from 'primevue/useconfirm';
  import { useToast } from 'primevue/usetoast';
  import cigarService from '../services/cigarService';
  import type { Cigar, CigarImage } from '../services/cigarService';
  import { arrayBufferToBase64 } from '@/utils/imageUtils';

  const router = useRouter();
  const confirm = useConfirm();
  const toast = useToast();

  const loading = ref(true);
  const error = ref<string | null>(null);
  const cigars = ref<Cigar[]>([]);

  function imageSrc(imageData: string | Array<number> | undefined | null): string {
    const b64 = arrayBufferToBase64(imageData ?? undefined);
    return b64 ? `data:image/jpeg;base64,${b64}` : '';
  }

  /** Список с /api/cigars отдаёт байты в `data`; часть ответов — в `imageData` */
  function cigarImageRawBytes(img: CigarImage | undefined): string | number[] | undefined {
    if (!img) return undefined;
    return img.imageData ?? img.data;
  }

  function carouselItemImageSrc(img: CigarImage | undefined): string {
    return imageSrc(cigarImageRawBytes(img) ?? null);
  }

  function memoKey(cigar: Cigar): (string | number | null | undefined)[] {
    const first = cigar.images?.[0];
    const raw = cigarImageRawBytes(first);
    const imgRef = raw == null ? 0 : typeof raw === 'string' ? raw.length : (raw as Array<number>).length;
    return [
      cigar.id,
      cigar.name,
      cigar.brand?.name,
      cigar.size,
      cigar.strength,
      cigar.rating,
      cigar.price,
      cigar.humidorId,
      cigar.images?.length,
      imgRef,
    ];
  }

  async function loadCigars(): Promise<void> {
    loading.value = true;
    error.value = null;
    try {
      cigars.value = await cigarService.getCigars();
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка загрузки сигар:', err);
      }
      error.value = 'Не удалось загрузить сигары. Попробуйте позже.';
    } finally {
      loading.value = false;
    }
  }

  function viewCigar(cigar: Cigar): void {
    if (cigar.id != null) {
      router.push({ name: 'CigarDetail', params: { id: String(cigar.id) } });
    }
  }

  function formatPrice(price: number): string {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: 'RUB',
    }).format(price);
  }

  function confirmDelete(cigar: Cigar): void {
    confirm.require({
      message: `Удалить сигару «${cigar.name}»? Действие нельзя отменить.`,
      header: 'Подтверждение удаления',
      icon: 'pi pi-exclamation-triangle',
      rejectClass: 'p-button-secondary p-button-outlined',
      acceptClass: 'p-button-danger',
      rejectLabel: 'Отмена',
      acceptLabel: 'Удалить',
      accept: async () => {
        if (cigar.id == null) {
          toast.add({
            severity: 'error',
            summary: 'Ошибка',
            detail: 'Не удалось определить ID сигары',
            life: 3000,
          });
          return;
        }
        try {
          await cigarService.deleteCigar(cigar.id);
          cigars.value = cigars.value.filter((c) => c.id !== cigar.id);
          toast.add({ severity: 'success', summary: 'Удалено', detail: 'Сигара удалена', life: 3000 });
        } catch (err) {
          if (import.meta.env.DEV) {
            console.error('Ошибка удаления сигары:', err);
          }
          toast.add({
            severity: 'error',
            summary: 'Ошибка',
            detail: 'Не удалось удалить сигару',
            life: 3000,
          });
        }
      },
    });
  }

  function confirmMarkAsSmoked(cigar: Cigar): void {
    confirm.require({
      message: `Отметить «${cigar.name}» как выкуренную? Сигара будет убрана из хьюмидора.`,
      header: 'Подтверждение',
      icon: 'pi pi-check-circle',
      rejectClass: 'p-button-secondary p-button-outlined',
      acceptClass: 'p-button-success',
      rejectLabel: 'Отмена',
      acceptLabel: 'Отметить',
      accept: async () => {
        if (cigar.id == null) {
          toast.add({
            severity: 'error',
            summary: 'Ошибка',
            detail: 'Не удалось определить ID сигары',
            life: 3000,
          });
          return;
        }
        try {
          const updated = await cigarService.markCigarAsSmoked(cigar.id);
          cigars.value = cigars.value.map((item) => (item.id === updated.id ? updated : item));
          toast.add({
            severity: 'success',
            summary: 'Готово',
            detail: 'Сигара отмечена как выкуренная',
            life: 3000,
          });
        } catch (err) {
          if (import.meta.env.DEV) {
            console.error('Ошибка отметки выкуривания:', err);
          }
          toast.add({
            severity: 'error',
            summary: 'Ошибка',
            detail: 'Не удалось отметить сигару как выкуренную',
            life: 3000,
          });
        }
      },
    });
  }

  onMounted(() => {
    loadCigars();
  });
</script>

<style scoped>
  .cigar-list-root {
    position: relative;
    isolation: isolate;
  }

  .cigar-list-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  /*:global(.dark) .cigar-list-grain {
    mix-blend-mode: soft-light;
  }*/

  .cigar-list-card-image-frame {
    display: block;
    width: 100%;
    max-width: 100%;
  }

  .cigar-list-card-image-frame img {
    display: block;
    width: auto;
    height: auto;
    min-width: 0;
    min-height: 0;
    max-width: 100%;
    max-height: 100%;
    object-fit: contain;
    object-position: center;
  }

  .line-clamp-1 {
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 1;
    overflow: hidden;
  }

  .line-clamp-2 {
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 2;
    overflow: hidden;
  }

  /* PrimeVue: .p-carousel-content-container { overflow: auto } — убираем скролл в карточке */
  .cigar-carousel :deep(.p-carousel-content-container) {
    overflow: hidden;
  }

  /* Карусель: крупнее зоны нажатия для стрелок PrimeVue */
  .cigar-carousel :deep(.p-carousel-prev),
  .cigar-carousel :deep(.p-carousel-next) {
    min-width: 2.75rem;
    min-height: 2.75rem;
  }

  .cigar-card-enter {
    animation: cigar-card-in 0.5s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes cigar-card-in {
    from {
      opacity: 0;
      transform: translateY(10px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  @media (prefers-reduced-motion: reduce) {
    .cigar-card-enter {
      animation: none;
    }
  }
</style>
