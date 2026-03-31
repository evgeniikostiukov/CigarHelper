<template>
  <section
    class="review-list-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-50 via-rose-50/40 to-stone-50 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-rose-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="review-list"
    aria-labelledby="review-list-heading">
    <div
      class="review-list-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] max-w-7xl mx-auto">
      <header class="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between pb-6 sm:pb-8">
        <div class="min-w-0">
          <p
            class="text-[0.65rem] uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55 font-semibold mb-1.5">
            Обзоры
          </p>
          <h1
            id="review-list-heading"
            class="text-3xl sm:text-4xl font-semibold text-stone-900 dark:text-rose-50/95 tracking-tight text-balance">
            Сигары в глазах сообщества
          </h1>
          <p class="mt-1.5 text-sm text-stone-600 dark:text-stone-400 max-w-xl text-pretty">
            Оценки, заметки и фото — фильтруйте по бренду и рейтингу; карточки открывают полный текст одним касанием.
          </p>
        </div>
        <Button
          v-if="isAuthenticated"
          data-testid="review-list-create"
          class="w-full sm:w-auto shrink-0 min-h-12 px-5 sm:min-h-11 touch-manipulation shadow-md shadow-rose-900/10 dark:shadow-black/40"
          icon="pi pi-plus"
          label="Написать обзор"
          @click="$router.push({ name: 'ReviewCreate' })" />
      </header>

      <div
        v-if="loading"
        data-testid="review-list-loading"
        class="grid grid-cols-1 lg:grid-cols-2 gap-5 sm:gap-6 min-h-[20rem]"
        aria-busy="true"
        aria-live="polite">
        <Skeleton
          v-for="n in 4"
          :key="n"
          class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          height="25rem"
          data-testid="review-list-skeleton" />
      </div>

      <div
        v-else-if="error"
        class="rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80 max-w-2xl"
        data-testid="review-list-error"
        role="alert">
        <Message severity="error">{{ error }}</Message>
        <Button
          data-testid="review-list-retry"
          class="mt-4 min-h-12 w-full sm:w-auto touch-manipulation"
          label="Повторить загрузку"
          icon="pi pi-refresh"
          severity="secondary"
          outlined
          @click="fetchReviews" />
      </div>

      <div
        v-else-if="reviews.length === 0"
        class="text-center rounded-2xl border border-dashed border-rose-800/25 bg-white/80 px-5 py-12 dark:border-rose-200/15 dark:bg-stone-900/60 max-w-xl mx-auto"
        data-testid="review-list-empty">
        <span
          class="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-rose-100/90 text-rose-900 dark:bg-rose-900/40 dark:text-rose-100"
          aria-hidden="true">
          <i class="pi pi-comments text-2xl" />
        </span>
        <h2 class="text-2xl font-semibold text-stone-900 dark:text-rose-50/95 mb-2">Пока нет обзоров</h2>
        <p class="text-stone-600 dark:text-stone-400 mb-6 text-pretty">
          Станьте первым, кто поделится впечатлением о сигаре, или зайдите позже.
        </p>
        <Button
          v-if="isAuthenticated"
          data-testid="review-list-empty-create"
          class="min-h-12 px-6 touch-manipulation"
          label="Написать обзор"
          icon="pi pi-plus"
          @click="$router.push({ name: 'ReviewCreate' })" />
      </div>

      <template v-else>
        <div
          class="mb-6 sm:mb-8 rounded-2xl border border-stone-200/90 bg-white/90 p-4 shadow-sm dark:border-stone-700/90 dark:bg-stone-900/70 sm:p-5"
          data-testid="review-list-filters">
          <div class="grid grid-cols-1 gap-4 md:grid-cols-3 md:items-end">
            <div class="flex flex-col min-w-0">
              <label
                for="review-brand-filter"
                class="mb-1.5 text-xs font-medium text-stone-600 dark:text-stone-400">
                Бренд
              </label>
              <Select
                id="review-brand-filter"
                v-model="filters.brand"
                data-testid="review-list-filter-brand"
                class="w-full"
                :options="brandSelectOptions"
                option-label="label"
                option-value="value"
                placeholder="Все бренды"
                show-clear
                filter />
            </div>
            <div class="flex flex-col min-w-0">
              <label
                for="review-rating-filter"
                class="mb-1.5 text-xs font-medium text-stone-600 dark:text-stone-400">
                Минимальная оценка
              </label>
              <Select
                id="review-rating-filter"
                v-model="filters.minRating"
                data-testid="review-list-filter-rating"
                class="w-full"
                :options="ratingSelectOptions"
                option-label="label"
                option-value="value"
                placeholder="Любая оценка"
                show-clear />
            </div>
            <div class="flex flex-col min-w-0">
              <label
                for="review-sort"
                class="mb-1.5 text-xs font-medium text-stone-600 dark:text-stone-400">
                Сортировка
              </label>
              <Select
                id="review-sort"
                v-model="sortBy"
                data-testid="review-list-filter-sort"
                class="w-full"
                :options="sortOptions"
                option-label="label"
                option-value="value" />
            </div>
          </div>
        </div>

        <div
          v-if="filteredReviews.length === 0"
          class="text-center rounded-2xl border border-dashed border-rose-800/25 bg-white/80 px-5 py-12 dark:border-rose-200/15 dark:bg-stone-900/60 max-w-xl mx-auto"
          data-testid="review-list-filter-empty">
          <span
            class="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-rose-100/90 text-rose-900 dark:bg-rose-900/40 dark:text-rose-100"
            aria-hidden="true">
            <i class="pi pi-filter-slash text-2xl" />
          </span>
          <h2 class="text-2xl font-semibold text-stone-900 dark:text-rose-50/95 mb-2">Ничего не нашлось</h2>
          <p class="text-stone-600 dark:text-stone-400 mb-6 text-pretty">
            Попробуйте смягчить фильтры — список обновится сразу.
          </p>
          <Button
            data-testid="review-list-filter-reset"
            class="min-h-12 px-6 touch-manipulation"
            label="Сбросить фильтры"
            icon="pi pi-times"
            severity="secondary"
            outlined
            @click="clearFilters" />
        </div>

        <div
          v-else
          class="grid grid-cols-1 lg:grid-cols-2 gap-5 sm:gap-6"
          data-testid="review-list-grid">
          <article
            v-for="(review, index) in filteredReviews"
            :key="review.id"
            v-memo="[
              review.id,
              review.title,
              review.rating,
              review.cigarBrand,
              review.cigarName,
              review.username,
              review.createdAt,
              review.content,
              review.images?.length,
              review.images?.[0]?.id,
            ]"
            :data-testid="`review-card-${review.id}`"
            class="review-card-enter group relative flex flex-col overflow-hidden rounded-2xl border border-stone-200/90 bg-white/95 shadow-md shadow-stone-900/5 transition-[box-shadow,transform] duration-300 hover:shadow-lg hover:shadow-rose-900/10 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 dark:hover:shadow-black/70 dark:hover:border-rose-900/30 motion-reduce:transition-none motion-reduce:animate-none"
            :style="{ animationDelay: `${Math.min(index, 8) * 48}ms` }">
            <router-link
              :to="{ name: 'ReviewDetail', params: { id: review.id } }"
              class="absolute inset-0 z-0 rounded-2xl focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-rose-700 dark:focus-visible:outline-rose-400"
              :aria-label="`Открыть обзор: ${review.title}`" />

            <div class="relative z-10 pointer-events-none shrink-0">
              <div
                v-if="review.images && review.images.length > 0"
                class="relative h-56 overflow-hidden border-b border-stone-100 bg-stone-100 dark:border-stone-700/80 dark:bg-stone-800/80">
                <img
                  :src="`data:image/jpeg;base64,${review.images[0].imageData}`"
                  :alt="review.title"
                  class="h-full w-full object-cover"
                  width="800"
                  height="448"
                  loading="lazy"
                  decoding="async" />
                <div
                  v-if="review.images.length > 1"
                  class="absolute bottom-2 right-2 flex items-center gap-1 rounded-full bg-black/55 px-2 py-1 text-xs text-white">
                  <i
                    class="pi pi-images"
                    aria-hidden="true" />
                  <span>{{ review.images.length }}</span>
                </div>
              </div>
              <div
                v-else
                class="flex h-40 items-center justify-center border-b border-stone-100 bg-gradient-to-br from-stone-50 to-rose-50/60 dark:border-stone-700/80 dark:from-stone-800/80 dark:to-rose-950/30"
                aria-hidden="true">
                <i class="pi pi-image text-4xl text-stone-400 dark:text-stone-500" />
              </div>
            </div>

            <div class="relative z-10 flex flex-1 flex-col gap-2 p-5 pointer-events-none min-h-0">
              <div class="flex items-start justify-between gap-3">
                <h2 class="text-lg font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 line-clamp-2 pr-2">
                  {{ review.title }}
                </h2>
                <Tag
                  class="shrink-0"
                  :value="review.rating + '/10'"
                  icon="pi pi-star-fill"
                  severity="warning" />
              </div>
              <p class="text-sm text-stone-600 dark:text-stone-400">{{ review.cigarBrand }} · {{ review.cigarName }}</p>
              <div class="flex items-center gap-2 text-sm text-stone-600 dark:text-stone-400">
                <Avatar
                  :image="review.userAvatarUrl || '/img/default-avatar.png'"
                  size="small"
                  shape="circle"
                  :aria-label="`Автор: ${review.username}`" />
                <span class="min-w-0 font-medium text-stone-800 dark:text-stone-200 truncate">{{
                  review.username
                }}</span>
                <span class="shrink-0 text-stone-500 dark:text-stone-500">· {{ formatDate(review.createdAt) }}</span>
              </div>
              <p
                class="line-clamp-3 text-sm leading-relaxed text-stone-700 dark:text-stone-300 pt-1 border-t border-stone-100 dark:border-stone-700/80">
                {{ review.content }}
              </p>
            </div>

            <footer
              class="relative z-20 mt-auto border-t border-stone-100 bg-stone-50/90 px-3 py-3 dark:border-stone-700/80 dark:bg-stone-950/50">
              <Button
                :data-testid="`review-open-${review.id}`"
                class="w-full min-h-11 touch-manipulation"
                label="Читать полностью"
                icon="pi pi-arrow-right"
                icon-pos="right"
                @click.stop="$router.push({ name: 'ReviewDetail', params: { id: review.id } })" />
            </footer>
          </article>
        </div>
      </template>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, reactive, computed, onMounted } from 'vue';
  import reviewService from '../services/reviewService';
  import { useAuth } from '@/services/useAuth';
  import type { Review } from '../services/reviewService';

  const { isAuthenticated } = useAuth();

  const reviews = ref<Review[]>([]);
  const loading = ref(true);
  const error = ref<string | null>(null);

  const filters = reactive({
    brand: null as string | null,
    minRating: null as number | null,
  });
  const sortBy = ref('date-desc');

  const ratingSelectOptions = Array.from({ length: 10 }, (_, i) => {
    const n = i + 1;
    return { label: `${n} и выше`, value: n };
  });

  const brandSelectOptions = computed(() => {
    const names = [...new Set(reviews.value.map((r) => r.cigarBrand))].sort();
    return names.map((b) => ({ label: b, value: b }));
  });

  const sortOptions = [
    { label: 'Сначала новые', value: 'date-desc' },
    { label: 'Сначала старые', value: 'date-asc' },
    { label: 'По оценке (лучшие)', value: 'rating-desc' },
    { label: 'По оценке (худшие)', value: 'rating-asc' },
  ];

  const filteredReviews = computed(() => {
    let result = [...reviews.value];

    if (filters.brand) {
      result = result.filter((review) => review.cigarBrand === filters.brand);
    }
    if (filters.minRating !== null) {
      result = result.filter((review) => review.rating >= filters.minRating!);
    }

    switch (sortBy.value) {
      case 'date-asc':
        result.sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
        break;
      case 'rating-desc':
        result.sort((a, b) => b.rating - a.rating);
        break;
      case 'rating-asc':
        result.sort((a, b) => a.rating - b.rating);
        break;
      case 'date-desc':
      default:
        result.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        break;
    }
    return result;
  });

  const fetchReviews = async (): Promise<void> => {
    loading.value = true;
    error.value = null;
    try {
      const response = await reviewService.getReviews({ pageSize: 100 });
      reviews.value = response;
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка при загрузке обзоров:', err);
      }
      error.value = 'Не удалось загрузить обзоры. Пожалуйста, попробуйте позже.';
    } finally {
      loading.value = false;
    }
  };

  const formatDate = (dateString: string): string =>
    new Date(dateString).toLocaleDateString('ru-RU', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });

  const clearFilters = (): void => {
    filters.brand = null;
    filters.minRating = null;
  };

  onMounted(fetchReviews);
</script>

<style scoped>
  .review-list-root {
    position: relative;
    isolation: isolate;
  }

  .review-list-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  /*:global(.dark) .review-list-grain {
    mix-blend-mode: soft-light;
  }*/

  .line-clamp-2 {
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 2;
    overflow: hidden;
  }

  .line-clamp-3 {
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 3;
    overflow: hidden;
  }

  .review-card-enter {
    animation: review-card-in 0.48s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes review-card-in {
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
    .review-card-enter {
      animation: none;
    }
  }
</style>
