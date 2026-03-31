<template>
  <section
    class="dashboard-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-50 via-rose-50/40 to-stone-50 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-rose-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="dashboard"
    aria-labelledby="dashboard-heading">
    <div
      class="dashboard-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto max-w-7xl">
      <div
        v-if="loading"
        data-testid="dashboard-loading"
        class="space-y-6 sm:space-y-8"
        aria-busy="true"
        aria-live="polite">
        <Skeleton
          class="max-w-md rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          height="3rem" />
        <div class="grid grid-cols-1 gap-5 sm:grid-cols-3 sm:gap-6">
          <Skeleton
            v-for="n in 3"
            :key="n"
            class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
            height="7rem" />
        </div>
        <Skeleton
          class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          height="10rem" />
        <Skeleton
          class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          height="10rem" />
      </div>

      <div
        v-else-if="error"
        class="max-w-2xl rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80"
        data-testid="dashboard-error"
        role="alert">
        <Message
          severity="error"
          :closable="false">
          {{ error }}
        </Message>
        <Button
          data-testid="dashboard-retry"
          class="mt-4 min-h-12 w-full touch-manipulation sm:w-auto"
          label="Повторить загрузку"
          icon="pi pi-refresh"
          severity="secondary"
          outlined
          @click="loadSummary" />
      </div>

      <template v-else>
        <header class="flex flex-col gap-4 pb-6 sm:flex-row sm:items-end sm:justify-between sm:pb-8">
          <div class="min-w-0">
            <p
              class="mb-1.5 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55">
              Коллекция
            </p>
            <h1
              id="dashboard-heading"
              class="text-balance text-3xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 sm:text-4xl">
              Сводка коллекции
            </h1>
            <p class="mt-1.5 max-w-xl text-pretty text-sm text-stone-600 dark:text-stone-400">
              Краткий обзор ваших хьюмидоров, брендов и недавних обзоров.
            </p>
          </div>
        </header>

        <div
          class="dashboard-enter space-y-6 sm:space-y-8"
          data-testid="dashboard-content">
          <section
            class="grid grid-cols-1 gap-5 sm:grid-cols-2 xl:grid-cols-4 sm:gap-6"
            aria-label="Основные показатели коллекции">
            <article
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6"
              data-testid="dashboard-summary-total-cigars">
              <h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-stone-500 dark:text-stone-400">
                Сигар в коллекции
              </h2>
              <p class="text-3xl font-semibold text-stone-900 dark:text-rose-50/95">
                {{ summary.totalCigars }}
              </p>
              <p class="mt-2 text-xs text-stone-500 dark:text-stone-500">
                Учитываются все сигары, в том числе вне хьюмидоров.
              </p>
            </article>

            <article
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6"
              data-testid="dashboard-summary-total-humidors">
              <h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-stone-500 dark:text-stone-400">
                Хьюмидоры
              </h2>
              <p class="text-3xl font-semibold text-stone-900 dark:text-rose-50/95">
                {{ summary.totalHumidors }}
              </p>
              <p class="mt-2 text-xs text-stone-500 dark:text-stone-500">
                Суммарная вместимость: {{ summary.totalCapacity }} сигар.
              </p>
            </article>

            <article
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6"
              data-testid="dashboard-summary-fill">
              <h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-stone-500 dark:text-stone-400">
                Средняя заполненность
              </h2>
              <p class="text-3xl font-semibold text-stone-900 dark:text-rose-50/95">
                {{ summary.averageFillPercent.toFixed(1) }}%
              </p>
              <ProgressBar
                :value="summary.averageFillPercent"
                class="mt-3"
                :show-value="false" />
              <p class="mt-2 text-xs text-stone-500 dark:text-stone-500">
                В расчёте учтены только хьюмидоры с заданной вместимостью.
              </p>
            </article>

            <article
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6"
              data-testid="dashboard-summary-aging">
              <h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-stone-500 dark:text-stone-400">
                Средний срок до выкуривания
              </h2>
              <p class="text-3xl font-semibold text-stone-900 dark:text-rose-50/95">
                {{ summary.averageDaysToSmoke }} дн.
              </p>
              <p class="mt-2 text-xs text-stone-500 dark:text-stone-500">
                Считается по сигарам, где отмечена дата выкуривания.
              </p>
            </article>
          </section>

          <section
            class="grid grid-cols-1 gap-5 lg:grid-cols-2 lg:gap-6"
            aria-label="Бренды и недавние обзоры">
            <article
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6"
              data-testid="dashboard-brands">
              <div class="flex items-center justify-between gap-2">
                <div>
                  <h2 class="text-lg font-semibold text-stone-900 dark:text-rose-50/95">Бренды в коллекции</h2>
                  <p class="mt-1 text-sm text-stone-600 dark:text-stone-400">Топ брендов по количеству сигар.</p>
                </div>
              </div>

              <div
                v-if="summary.brandBreakdown.length === 0"
                class="mt-4">
                <div
                  class="rounded-xl border border-dashed border-rose-800/25 bg-stone-50/80 px-5 py-8 text-center text-sm text-stone-600 dark:border-rose-200/15 dark:bg-stone-950/40 dark:text-stone-400"
                  data-testid="dashboard-brands-empty">
                  Как только у вас появятся сигары в коллекции, здесь появится разрез по брендам.
                </div>
              </div>
              <ul
                v-else
                class="mt-4 space-y-3">
                <li
                  v-for="item in summary.brandBreakdown"
                  :key="item.brandId"
                  :data-testid="`dashboard-brands-item-${item.brandId}`"
                  class="flex items-center justify-between gap-3 rounded-xl border border-stone-100 bg-stone-50/80 px-4 py-3 text-sm dark:border-stone-700/70 dark:bg-stone-950/40">
                  <div class="min-w-0">
                    <p class="truncate font-medium text-stone-900 dark:text-rose-50/95">
                      {{ item.brandName }}
                    </p>
                    <p class="mt-0.5 text-xs text-stone-500 dark:text-stone-400">
                      {{ item.cigarCount }} {{ item.cigarCount === 1 ? 'сигара' : 'сигар' }}
                    </p>
                  </div>
                  <div class="flex flex-col items-end gap-1">
                    <span
                      v-if="item.averageRating != null"
                      class="inline-flex items-center gap-1 rounded-full bg-amber-50 px-2 py-1 text-xs font-semibold text-amber-800 dark:bg-amber-900/40 dark:text-amber-200">
                      <i
                        class="pi pi-star-fill text-xs"
                        aria-hidden="true" />
                      {{ item.averageRating.toFixed(1) }}/10
                    </span>
                    <span
                      v-else
                      class="text-xs text-stone-500 dark:text-stone-400">
                      Нет оценок
                    </span>
                  </div>
                </li>
              </ul>
            </article>

            <article
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6"
              data-testid="dashboard-reviews">
              <div class="flex items-center justify-between gap-2">
                <div>
                  <h2 class="text-lg font-semibold text-stone-900 dark:text-rose-50/95">Недавние обзоры</h2>
                  <p class="mt-1 text-sm text-stone-600 dark:text-stone-400">Последние впечатления от сигар.</p>
                </div>
                <Button
                  class="hidden min-h-10 touch-manipulation text-sm sm:inline-flex"
                  label="Все обзоры"
                  icon="pi pi-arrow-right"
                  icon-pos="right"
                  text
                  @click="goToReviews" />
              </div>

              <div
                v-if="summary.recentReviews.length === 0"
                class="mt-4">
                <div
                  class="rounded-xl border border-dashed border-rose-800/25 bg-stone-50/80 px-5 py-8 text-center text-sm text-stone-600 dark:border-rose-200/15 dark:bg-stone-950/40 dark:text-stone-400"
                  data-testid="dashboard-reviews-empty">
                  У вас ещё нет обзоров. Попробуйте оформить заметки о любимых сигарах.
                </div>
              </div>
              <ul
                v-else
                class="mt-4 space-y-3">
                <li
                  v-for="review in summary.recentReviews"
                  :key="review.id"
                  :data-testid="`dashboard-review-${review.id}`">
                  <button
                    type="button"
                    class="flex w-full items-start gap-3 rounded-xl border border-stone-100 bg-stone-50/80 px-4 py-3 text-left text-sm transition-colors hover:border-rose-300 hover:bg-rose-50/80 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-rose-600 dark:border-stone-700/70 dark:bg-stone-950/40 dark:hover:border-rose-500 dark:hover:bg-rose-950/25 dark:focus-visible:ring-rose-400"
                    @click="openReview(review.id)">
                    <div class="mt-0.5 flex flex-col items-center gap-1">
                      <span
                        class="inline-flex items-center justify-center rounded-full bg-amber-50 px-2 py-1 text-xs font-semibold text-amber-800 dark:bg-amber-900/40 dark:text-amber-200">
                        <i
                          class="pi pi-star-fill mr-1 text-xs"
                          aria-hidden="true" />
                        {{ review.rating }}/10
                      </span>
                      <span class="text-[0.65rem] text-stone-500 dark:text-stone-400">
                        {{ formatDate(review.createdAt) }}
                      </span>
                    </div>
                    <div class="min-w-0 flex-1">
                      <p class="truncate text-sm font-semibold text-stone-900 dark:text-rose-50/95">
                        {{ review.title }}
                      </p>
                      <p class="mt-0.5 truncate text-xs text-stone-600 dark:text-stone-400">
                        {{ review.cigarBrand }} · {{ review.cigarName }}
                      </p>
                    </div>
                    <i
                      class="pi pi-chevron-right mt-1 text-xs text-stone-400 dark:text-stone-500"
                      aria-hidden="true" />
                  </button>
                </li>
              </ul>

              <Button
                class="mt-4 flex w-full min-h-11 touch-manipulation text-sm sm:hidden"
                label="Все обзоры"
                icon="pi pi-arrow-right"
                icon-pos="right"
                text
                @click="goToReviews" />
            </article>
          </section>

          <section
            class="grid grid-cols-1 gap-5 lg:grid-cols-2 lg:gap-6"
            aria-label="История и напоминания">
            <article
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6"
              data-testid="dashboard-timeline">
              <h2 class="text-lg font-semibold text-stone-900 dark:text-rose-50/95">История во времени</h2>
              <p class="mt-1 text-sm text-stone-600 dark:text-stone-400">
                Помесячно: сколько сигар куплено и выкурено.
              </p>
              <ul class="mt-4 space-y-2">
                <li
                  v-for="point in summary.timeline"
                  :key="point.period"
                  class="flex items-center justify-between gap-2 rounded-xl border border-stone-100 bg-stone-50/80 px-3 py-2 text-sm dark:border-stone-700/70 dark:bg-stone-950/40">
                  <span class="font-medium text-stone-800 dark:text-stone-200">{{ formatPeriod(point.period) }}</span>
                  <span class="text-stone-600 dark:text-stone-400">
                    купил: {{ point.purchasedCount }} · выкурил: {{ point.smokedCount }}
                  </span>
                </li>
              </ul>
            </article>

            <article
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6"
              data-testid="dashboard-reminders">
              <h2 class="text-lg font-semibold text-stone-900 dark:text-rose-50/95">Мягкие напоминания</h2>
              <p class="mt-1 text-sm text-stone-600 dark:text-stone-400">Сигары, к которым вы давно не возвращались.</p>
              <div
                v-if="summary.staleCigarReminders.length === 0"
                class="mt-4 rounded-xl border border-dashed border-rose-800/25 bg-stone-50/80 px-5 py-6 text-center text-sm text-stone-600 dark:border-rose-200/15 dark:bg-stone-950/40 dark:text-stone-400"
                data-testid="dashboard-reminders-empty">
                Сейчас все сигары под контролем — напоминаний нет.
              </div>
              <ul
                v-else
                class="mt-4 space-y-3">
                <li
                  v-for="item in summary.staleCigarReminders"
                  :key="item.cigarId"
                  :data-testid="`dashboard-reminder-${item.cigarId}`"
                  class="rounded-xl border border-stone-100 bg-stone-50/80 px-4 py-3 text-sm dark:border-stone-700/70 dark:bg-stone-950/40">
                  <p class="font-medium text-stone-900 dark:text-rose-50/95">
                    {{ item.brandName }} · {{ item.cigarName }}
                  </p>
                  <p class="mt-1 text-xs text-stone-500 dark:text-stone-400">
                    Не трогали {{ item.daysUntouched }} дн. (с {{ formatDate(item.lastTouchedAt) }})
                  </p>
                </li>
              </ul>
            </article>
          </section>

          <section
            v-if="summary.totalCigars === 0"
            class="rounded-2xl border border-dashed border-rose-800/25 bg-white/90 p-5 text-center shadow-md shadow-stone-900/5 dark:border-rose-200/20 dark:bg-stone-900/85 dark:shadow-black/40 sm:p-6"
            aria-label="Подсказка по первому шагу">
            <p class="text-sm text-stone-700 dark:text-stone-300">
              Коллекция пока пуста — начните с создания хьюмидора и добавления первой сигары.
            </p>
            <div class="mt-4 flex flex-col gap-3 sm:flex-row sm:justify-center">
              <Button
                class="min-h-11 w-full touch-manipulation sm:w-auto"
                label="Создать хьюмидор"
                icon="pi pi-plus"
                @click="goToHumidors" />
            </div>
          </section>
        </div>
      </template>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { onMounted, ref } from 'vue';
  import { useRouter } from 'vue-router';
  import Button from 'primevue/button';
  import Message from 'primevue/message';
  import ProgressBar from 'primevue/progressbar';
  import Skeleton from 'primevue/skeleton';
  import dashboardService, { type DashboardSummary } from '@/services/dashboardService';

  const router = useRouter();

  const loading = ref(true);
  const error = ref<string | null>(null);
  const summary = ref<DashboardSummary>({
    totalHumidors: 0,
    totalCigars: 0,
    totalCapacity: 0,
    averageFillPercent: 0,
    averageDaysToSmoke: 0,
    brandBreakdown: [],
    recentReviews: [],
    timeline: [],
    staleCigarReminders: [],
  });

  const loadSummary = async (): Promise<void> => {
    loading.value = true;
    error.value = null;
    try {
      summary.value = await dashboardService.getDashboardSummary();
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка загрузки дашборда:', err);
      }
      error.value = 'Не удалось загрузить сводку коллекции.';
    } finally {
      loading.value = false;
    }
  };

  const formatDate = (iso: string): string => {
    const date = new Date(iso);
    return date.toLocaleDateString('ru-RU', {
      year: 'numeric',
      month: 'short',
      day: '2-digit',
    });
  };

  const goToReviews = (): void => {
    router.push({ name: 'ReviewList' });
  };

  const formatPeriod = (period: string): string => {
    const [year, month] = period.split('-').map(Number);
    if (!year || !month) {
      return period;
    }
    return new Date(Date.UTC(year, month - 1, 1)).toLocaleDateString('ru-RU', {
      year: 'numeric',
      month: 'short',
    });
  };

  const goToHumidors = (): void => {
    router.push({ name: 'HumidorList' });
  };

  const openReview = (id: number): void => {
    router.push({ name: 'ReviewDetail', params: { id } });
  };

  onMounted(() => {
    void loadSummary();
  });
</script>

<style scoped>
  .dashboard-root {
    position: relative;
    isolation: isolate;
  }

  .dashboard-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  /*:global(.dark) .dashboard-grain {
    mix-blend-mode: soft-light;
  }*/

  .dashboard-enter {
    animation: dashboard-in 0.45s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes dashboard-in {
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
    .dashboard-enter {
      animation: none;
    }
  }
</style>
