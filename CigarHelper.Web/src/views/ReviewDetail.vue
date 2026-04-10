<template>
  <section
    class="review-detail-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-50 via-rose-50/40 to-stone-50 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-rose-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="review-detail"
    :aria-labelledby="review ? 'review-detail-heading' : undefined"
    :aria-busy="loading">
    <div
      class="review-detail-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto max-w-4xl">
      <div
        v-if="loading"
        class="min-h-[20rem] space-y-5"
        data-testid="review-detail-loading"
        aria-busy="true"
        aria-live="polite">
        <Skeleton
          class="max-w-lg rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          height="2.5rem"
          data-testid="review-detail-skeleton" />
        <Skeleton
          class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          height="4rem" />
        <Skeleton
          class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          height="22rem" />
        <div class="grid grid-cols-1 gap-5 md:grid-cols-2">
          <Skeleton
            class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
            height="12rem" />
          <Skeleton
            class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
            height="12rem" />
        </div>
      </div>

      <div
        v-else-if="error"
        class="max-w-2xl rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80"
        data-testid="review-detail-error"
        role="alert">
        <Message
          severity="error"
          :closable="false">
          {{ error }}
        </Message>
        <div class="mt-4 flex flex-col gap-3 sm:flex-row sm:flex-wrap">
          <Button
            data-testid="review-detail-retry"
            class="min-h-12 w-full touch-manipulation sm:w-auto"
            label="Повторить загрузку"
            icon="pi pi-refresh"
            severity="secondary"
            outlined
            @click="loadReview" />
          <Button
            data-testid="review-detail-back-list"
            class="min-h-12 w-full touch-manipulation sm:w-auto"
            label="К списку обзоров"
            icon="pi pi-list"
            severity="secondary"
            outlined
            @click="router.push({ name: 'ReviewList' })" />
        </div>
      </div>

      <article
        v-else-if="review"
        class="review-detail-enter space-y-6 sm:space-y-8"
        data-testid="review-detail-content">
        <nav
          class="flex flex-wrap items-center gap-x-2 gap-y-1 text-sm text-stone-600 dark:text-stone-400"
          aria-label="Хлебные крошки">
          <RouterLink
            :to="{ name: 'Home' }"
            class="rounded font-medium text-rose-900 underline-offset-2 hover:text-rose-700 hover:underline dark:text-rose-200/90 dark:hover:text-rose-100">
            Главная
          </RouterLink>
          <span
            class="text-stone-400"
            aria-hidden="true"
            >/</span
          >
          <RouterLink
            :to="{ name: 'ReviewList' }"
            class="rounded font-medium text-rose-900 underline-offset-2 hover:text-rose-700 hover:underline dark:text-rose-200/90 dark:hover:text-rose-100">
            Обзоры
          </RouterLink>
          <span
            class="text-stone-400"
            aria-hidden="true"
            >/</span
          >
          <span class="line-clamp-2 min-w-0 font-medium text-stone-800 dark:text-stone-200">{{ review.title }}</span>
        </nav>

        <div
          v-if="isCurrentUserReview"
          class="flex flex-col gap-2 sm:flex-row sm:flex-wrap sm:justify-end">
          <Button
            data-testid="review-detail-edit"
            class="min-h-12 w-full touch-manipulation shadow-md shadow-rose-900/10 dark:shadow-black/40 sm:min-h-11 sm:w-auto"
            label="Редактировать"
            icon="pi pi-pencil"
            @click="router.push({ name: 'ReviewEdit', params: { id: String(review.id) } })" />
          <Button
            data-testid="review-detail-delete"
            class="min-h-12 w-full touch-manipulation sm:min-h-11 sm:w-auto"
            label="Удалить"
            icon="pi pi-trash"
            severity="danger"
            outlined
            @click="confirmDelete" />
        </div>

        <header class="space-y-4">
          <div class="min-w-0">
            <p
              class="mb-1.5 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55">
              Обзоры
            </p>
            <h1
              id="review-detail-heading"
              class="text-balance text-3xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 sm:text-4xl md:text-[2.5rem]">
              {{ review.title }}
            </h1>
          </div>
          <div
            class="flex flex-col gap-4 rounded-2xl border border-stone-200/90 bg-white/95 p-4 shadow-md shadow-stone-900/5 sm:flex-row sm:items-center sm:justify-between sm:p-5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50">
            <div class="flex min-w-0 items-center gap-3">
              <Avatar
                :image="review.userAvatarUrl || '/img/default-avatar.png'"
                size="large"
                shape="circle"
                :aria-label="`Автор: ${review.username}`" />
              <div class="min-w-0">
                <p class="truncate text-lg font-semibold text-stone-900 dark:text-rose-50/95">
                  {{ review.username }}
                </p>
                <p class="text-sm text-stone-600 dark:text-stone-400">
                  {{ formatDate(review.createdAt) }}
                </p>
              </div>
            </div>
            <div class="flex shrink-0 flex-wrap items-center gap-2">
              <span class="text-sm font-medium text-stone-600 dark:text-stone-400">Оценка</span>
              <Tag
                :value="`${review.rating}/10`"
                icon="pi pi-star-fill"
                severity="warning"
                class="text-base" />
            </div>
          </div>
          <div
            class="rounded-xl border border-rose-900/15 bg-rose-50/50 px-4 py-3 text-sm text-stone-800 dark:border-rose-400/20 dark:bg-rose-950/25 dark:text-stone-200"
            data-testid="review-detail-cigar-banner">
            Обзор на сигару:
            <strong class="font-semibold text-stone-900 dark:text-rose-50/95"
              >{{ review.cigarBrand }} · {{ review.cigarName }}</strong
            >
          </div>
        </header>

        <div
          v-if="review.images?.length"
          class="overflow-hidden rounded-2xl border border-stone-200/90 bg-stone-900/5 shadow-md dark:border-stone-700/90 dark:bg-stone-900/40"
          data-testid="review-detail-gallery">
          <Galleria
            :value="review.images"
            :num-visible="5"
            container-class="w-full"
            :show-thumbnails="review.images.length > 1"
            :show-item-navigators="review.images.length > 1">
            <template #item="slotProps">
              <img
                :src="`data:image/jpeg;base64,${slotProps.item.imageData}`"
                :alt="slotProps.item.caption || review.title"
                class="block max-h-[70vh] w-full object-contain"
                width="1200"
                height="900"
                loading="lazy"
                decoding="async" />
            </template>
            <template #thumbnail="slotProps">
              <img
                :src="`data:image/jpeg;base64,${slotProps.item.imageData}`"
                :alt="slotProps.item.caption || review.title"
                class="h-16 w-24 object-cover"
                width="96"
                height="64"
                loading="lazy"
                decoding="async" />
            </template>
            <template #caption="slotProps">
              <p
                v-if="slotProps.item.caption"
                class="text-sm text-stone-100">
                {{ slotProps.item.caption }}
              </p>
            </template>
          </Galleria>
        </div>

        <div class="grid grid-cols-1 gap-5 md:grid-cols-2 md:gap-6">
          <section
            class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6">
            <h2 class="mb-4 flex items-center gap-2 text-lg font-semibold text-stone-900 dark:text-rose-50/95">
              <i
                class="pi pi-sliders-h text-rose-800/80 dark:text-rose-400/90"
                aria-hidden="true" />
              Характеристики
            </h2>
            <ul class="space-y-4 text-sm text-stone-700 dark:text-stone-300">
              <li v-if="review.smokingExperience">
                <span class="font-medium text-stone-900 dark:text-stone-100">Опыт курения:</span>
                {{ review.smokingExperience }}
              </li>
              <li v-if="review.aroma">
                <span class="font-medium text-stone-900 dark:text-stone-100">Аромат:</span>
                {{ review.aroma }}
              </li>
              <li v-if="review.taste">
                <span class="font-medium text-stone-900 dark:text-stone-100">Вкус:</span>
                {{ review.taste }}
              </li>
              <li
                v-if="review.construction"
                class="flex flex-col gap-2 sm:flex-row sm:items-center">
                <span class="shrink-0 font-medium text-stone-900 dark:text-stone-100">Конструкция:</span>
                <Rating
                  :model-value="review.construction"
                  readonly
                  :cancel="false" />
              </li>
              <li
                v-if="review.burnQuality"
                class="flex flex-col gap-2 sm:flex-row sm:items-center">
                <span class="shrink-0 font-medium text-stone-900 dark:text-stone-100">Качество горения:</span>
                <Rating
                  :model-value="review.burnQuality"
                  readonly
                  :cancel="false" />
              </li>
              <li
                v-if="review.draw"
                class="flex flex-col gap-2 sm:flex-row sm:items-center">
                <span class="shrink-0 font-medium text-stone-900 dark:text-stone-100">Тяга:</span>
                <Rating
                  :model-value="review.draw"
                  readonly
                  :cancel="false" />
              </li>
            </ul>
          </section>
          <section
            class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6">
            <h2 class="mb-4 flex items-center gap-2 text-lg font-semibold text-stone-900 dark:text-rose-50/95">
              <i
                class="pi pi-calendar text-rose-800/80 dark:text-rose-400/90"
                aria-hidden="true" />
              Детали дегустации
            </h2>
            <ul class="space-y-4 text-sm text-stone-700 dark:text-stone-300">
              <li v-if="review.venue">
                <span class="font-medium text-stone-900 dark:text-stone-100">Место:</span>
                {{ review.venue }}
              </li>
              <li>
                <span class="font-medium text-stone-900 dark:text-stone-100">Дата дегустации:</span>
                {{ formatDate(review.smokingDate) }}
              </li>
            </ul>
          </section>
        </div>

        <section
          class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6"
          data-testid="review-detail-body"
          aria-labelledby="review-detail-body-heading">
          <h2
            id="review-detail-body-heading"
            class="mb-4 text-lg font-semibold text-stone-900 dark:text-rose-50/95">
            Текст обзора
          </h2>
          <div
            class="review-detail-prose prose-stone max-w-none dark:prose-invert"
            v-html="sanitizedContent" />
        </section>

        <footer
          class="flex flex-col gap-3 border-t border-stone-200/80 pt-6 dark:border-stone-700/80 sm:flex-row sm:items-center sm:justify-between">
          <Button
            data-testid="review-detail-back"
            class="min-h-12 w-full touch-manipulation sm:order-1 sm:w-auto"
            label="Назад к списку"
            icon="pi pi-arrow-left"
            severity="secondary"
            outlined
            @click="router.push({ name: 'ReviewList' })" />
          <Button
            data-testid="review-detail-cigar"
            class="min-h-12 w-full touch-manipulation shadow-md shadow-rose-900/10 dark:shadow-black/40 sm:order-2 sm:w-auto"
            :label="review.userCigarId ? 'Открыть сигару в коллекции' : 'Открыть в каталоге'"
            icon="pi pi-arrow-right"
            icon-pos="right"
            @click="goToCigar" />
        </footer>
      </article>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { RouterLink, useRoute, useRouter } from 'vue-router';
  import { useConfirm } from 'primevue/useconfirm';
  import { useToast } from 'primevue/usetoast';
  import reviewService from '../services/reviewService';
  import type { Review } from '../services/reviewService';
  import authService from '../services/authService';
  import DOMPurify from 'dompurify';

  const route = useRoute();
  const router = useRouter();
  const confirm = useConfirm();
  const toast = useToast();

  const review = ref<Review | null>(null);
  const loading = ref(true);
  const error = ref<string | null>(null);

  const isCurrentUserReview = computed(() => {
    if (!review.value) return false;
    const currentUser = authService.state.user;
    return Boolean(currentUser && currentUser.id === review.value.userId);
  });

  const sanitizedContent = computed(() => {
    if (!review.value?.content) return '';
    const rawHtml = review.value.content
      .split('\n')
      .filter((p) => p.trim().length > 0)
      .map((p) => `<p>${p}</p>`)
      .join('');
    return DOMPurify.sanitize(rawHtml, {
      USE_PROFILES: { html: true },
    });
  });

  const goToCigar = (): void => {
    if (!review.value) return;
    if (review.value.userCigarId != null) {
      void router.push({ name: 'CigarDetail', params: { id: String(review.value.userCigarId) } });
      return;
    }
    void router.push({
      name: 'CigarBases',
      query: { selectedCigarBaseId: String(review.value.cigarBaseId) },
    });
  };

  const loadReview = async (): Promise<void> => {
    loading.value = true;
    error.value = null;
    try {
      const reviewId = route.params.id as string;
      review.value = await reviewService.getReview(reviewId);
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка загрузки обзора:', err);
      }
      error.value = 'Не удалось загрузить обзор. Возможно, он был удален или ссылка неверна.';
    } finally {
      loading.value = false;
    }
  };

  const formatDate = (dateString: string | undefined): string => {
    if (!dateString) return 'Дата не указана';
    return new Date(dateString).toLocaleDateString('ru-RU', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  const confirmDelete = (): void => {
    confirm.require({
      message: 'Вы уверены, что хотите удалить этот обзор? Это действие нельзя отменить.',
      header: 'Подтверждение удаления',
      icon: 'pi pi-exclamation-triangle',
      rejectClass: 'p-button-secondary p-button-outlined',
      acceptClass: 'p-button-danger',
      rejectLabel: 'Отмена',
      acceptLabel: 'Удалить',
      accept: async () => {
        if (!review.value) return;
        try {
          await reviewService.deleteReview(review.value.id);
          toast.add({
            severity: 'success',
            summary: 'Успешно',
            detail: 'Обзор удален',
            life: 3000,
          });
          await router.push({ name: 'ReviewList' });
        } catch (err) {
          if (import.meta.env.DEV) {
            console.error('Ошибка при удалении обзора:', err);
          }
          toast.add({
            severity: 'error',
            summary: 'Ошибка',
            detail: 'Не удалось удалить обзор',
            life: 3000,
          });
        }
      },
    });
  };

  onMounted(() => {
    void loadReview();
  });
</script>

<style scoped>
  .review-detail-root {
    position: relative;
    isolation: isolate;
  }

  .review-detail-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  /*:global(.dark) .review-detail-grain {
    mix-blend-mode: soft-light;
  }*/

  .review-detail-enter {
    animation: review-detail-in 0.45s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes review-detail-in {
    from {
      opacity: 0;
      transform: translateY(8px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  @media (prefers-reduced-motion: reduce) {
    .review-detail-enter {
      animation: none;
    }
  }

  .line-clamp-2 {
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 2;
    overflow: hidden;
  }

  :deep(.review-detail-prose p) {
    margin-bottom: 1rem;
    line-height: 1.65;
  }

  :deep(.review-detail-prose h1),
  :deep(.review-detail-prose h2),
  :deep(.review-detail-prose h3) {
    margin-bottom: 0.75rem;
    margin-top: 1.25rem;
    font-weight: 600;
  }
</style>
