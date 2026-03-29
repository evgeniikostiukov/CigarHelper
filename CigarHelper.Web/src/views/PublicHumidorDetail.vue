<template>
  <section
    class="public-humidor-detail-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-50 via-rose-50/40 to-stone-50 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-rose-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="public-humidor-detail"
    :aria-labelledby="humidor ? 'public-humidor-detail-heading' : undefined"
    :aria-busy="loading">
    <div
      class="public-humidor-detail-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto max-w-7xl">
      <div
        v-if="loading"
        class="min-h-[18rem] space-y-5"
        data-testid="public-humidor-detail-loading"
        aria-busy="true"
        aria-live="polite">
        <Skeleton
          class="max-w-md rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          height="3rem"
          data-testid="public-humidor-detail-skeleton" />
        <div class="grid grid-cols-1 gap-5 md:grid-cols-3">
          <Skeleton
            v-for="n in 3"
            :key="n"
            class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
            height="8rem" />
        </div>
        <Skeleton
          class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          height="14rem" />
      </div>

      <div
        v-else-if="error"
        class="max-w-2xl rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80"
        data-testid="public-humidor-detail-error"
        role="alert">
        <Message
          severity="error"
          :closable="false">
          {{ error }}
        </Message>
        <div class="mt-4 flex flex-col gap-3 sm:flex-row sm:flex-wrap">
          <Button
            data-testid="public-humidor-detail-retry"
            class="min-h-12 w-full touch-manipulation sm:w-auto"
            label="Повторить загрузку"
            icon="pi pi-refresh"
            severity="secondary"
            outlined
            @click="load" />
          <Button
            data-testid="public-humidor-detail-profile"
            class="min-h-12 w-full touch-manipulation sm:w-auto"
            label="К профилю"
            icon="pi pi-user"
            severity="secondary"
            outlined
            @click="goProfile" />
        </div>
      </div>

      <template v-else-if="humidor">
        <header class="flex flex-col gap-4 pb-6 sm:flex-row sm:items-end sm:justify-between sm:pb-8">
          <div class="min-w-0">
            <p
              class="mb-1.5 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55">
              Публичный просмотр · {{ ownerUsername }}
            </p>
            <h1
              id="public-humidor-detail-heading"
              class="text-balance text-3xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 sm:text-4xl">
              {{ humidor.name }}
            </h1>
            <p
              v-if="humidor.description"
              class="mt-1.5 max-w-xl text-pretty text-sm text-stone-600 dark:text-stone-400">
              {{ humidor.description }}
            </p>
            <p
              v-else
              class="mt-1.5 text-sm text-stone-500 dark:text-stone-500">
              Без описания.
            </p>
          </div>
          <Button
            data-testid="public-humidor-detail-to-profile"
            class="min-h-12 w-full shrink-0 touch-manipulation sm:min-h-11 sm:w-auto"
            label="К профилю"
            icon="pi pi-user"
            severity="secondary"
            outlined
            @click="goProfile" />
        </header>

        <div
          class="public-humidor-detail-enter space-y-6 sm:space-y-8"
          data-testid="public-humidor-detail-content">
          <div
            class="grid grid-cols-1 gap-5 sm:gap-6 md:grid-cols-3"
            data-testid="public-humidor-detail-stats">
            <div
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6">
              <h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-stone-500 dark:text-stone-400">
                Вместимость
              </h2>
              <p class="text-2xl font-semibold text-stone-900 dark:text-rose-50/95">
                {{ cigarCount }} / {{ humidor.capacity ?? '—' }}
              </p>
              <ProgressBar
                v-if="humidor.capacity"
                :value="capacityPercentage"
                class="mt-3"
                :show-value="false" />
            </div>
            <div
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6">
              <h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-stone-500 dark:text-stone-400">
                Температура, °C
              </h2>
              <p class="text-2xl font-semibold text-stone-700 dark:text-stone-200">—</p>
              <p class="mt-2 text-xs text-stone-500 dark:text-stone-500">Не публикуется в открытом виде.</p>
            </div>
            <div
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6">
              <h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-stone-500 dark:text-stone-400">
                Влажность
              </h2>
              <div v-if="humidor.humidity != null">
                <Badge
                  :value="`${humidor.humidity}%`"
                  :severity="humiditySeverity" />
              </div>
              <p
                v-else
                class="text-stone-600 dark:text-stone-400">
                Не указана
              </p>
            </div>
          </div>

          <section
            class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6"
            aria-labelledby="public-humidor-cigars-heading">
            <h2
              id="public-humidor-cigars-heading"
              class="mb-4 text-lg font-semibold text-stone-900 dark:text-rose-50/95">
              Сигары в хьюмидоре
            </h2>
            <DataTable
              :value="humidor.cigars || []"
              data-testid="public-humidor-detail-table"
              responsive-layout="scroll"
              :paginator="(humidor.cigars?.length ?? 0) > 8"
              removable-sort
              :rows="8">
              <Column
                field="name"
                header="Название"
                sortable />
              <Column
                field="brand.name"
                header="Бренд"
                sortable />
              <Column
                field="size"
                header="Размер"
                sortable>
                <template #body="slotProps">
                  {{ slotProps.data.size || '—' }}
                </template>
              </Column>
              <Column
                field="strength"
                header="Крепость"
                sortable>
                <template #body="slotProps">
                  {{ getStrengthLabel(slotProps.data.strength) || '—' }}
                </template>
              </Column>
              <Column
                field="rating"
                header="Рейтинг"
                sortable>
                <template #body="slotProps">
                  {{ slotProps.data.rating != null ? `${slotProps.data.rating}/10` : '—' }}
                </template>
              </Column>
              <template #empty>
                <div
                  class="rounded-xl border border-dashed border-rose-800/25 bg-stone-50/80 px-5 py-10 text-center text-stone-600 dark:border-rose-200/15 dark:bg-stone-950/40 dark:text-stone-400"
                  data-testid="public-humidor-detail-table-empty">
                  В этом хьюмидоре пока нет сигар.
                </div>
              </template>
            </DataTable>
          </section>
        </div>
      </template>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, computed, watch } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import Badge from 'primevue/badge';
  import Button from 'primevue/button';
  import DataTable from 'primevue/datatable';
  import Column from 'primevue/column';
  import Message from 'primevue/message';
  import ProgressBar from 'primevue/progressbar';
  import Skeleton from 'primevue/skeleton';
  import humidorService from '@/services/humidorService';
  import * as profileApi from '@/services/profileService';
  import type { Humidor } from '@/services/humidorService';
  import type { Cigar } from '@/services/cigarService';
  import { strengthOptions } from '@/utils/cigarOptions';

  interface PublicHumidorView extends Humidor {
    cigars: Cigar[];
  }

  const route = useRoute();
  const router = useRouter();

  const loading = ref(true);
  const error = ref<string | null>(null);
  const humidor = ref<PublicHumidorView | null>(null);

  const ownerUsername = computed(() => route.params.username as string);

  const cigarCount = computed(() => humidor.value?.cigars?.length ?? 0);

  const capacityPercentage = computed((): number => {
    const h = humidor.value;
    if (!h?.capacity) return 0;
    return Math.min(100, Math.round((cigarCount.value / h.capacity) * 100));
  });

  const humiditySeverity = computed(() => humidorService.getHumiditySeverity(humidor.value?.humidity));

  function getStrengthLabel(strength: string | null | undefined): string {
    if (!strength) return '';
    return strengthOptions.find((o) => o.value === strength)?.label || strength;
  }

  function goProfile(): void {
    router.push({ name: 'PublicUserProfile', params: { username: ownerUsername.value } });
  }

  async function load(): Promise<void> {
    loading.value = true;
    error.value = null;
    humidor.value = null;
    const username = route.params.username as string;
    const humidorId = Number(route.params.humidorId);
    if (!username || Number.isNaN(humidorId)) {
      error.value = 'Некорректная ссылка.';
      loading.value = false;
      return;
    }
    try {
      const h = await profileApi.getPublicHumidor(username, humidorId);
      humidor.value = { ...h, cigars: h.cigars ?? [] };
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка загрузки публичного хьюмидора:', err);
      }
      error.value = 'Хьюмидор не найден или профиль скрыт.';
    } finally {
      loading.value = false;
    }
  }

  watch(
    () => [route.params.username, route.params.humidorId],
    () => {
      void load();
    },
    { immediate: true },
  );
</script>

<style scoped>
  .public-humidor-detail-root {
    position: relative;
    isolation: isolate;
  }

  .public-humidor-detail-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  :global(.dark) .public-humidor-detail-grain {
    mix-blend-mode: soft-light;
  }

  .public-humidor-detail-enter {
    animation: public-humidor-detail-in 0.45s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes public-humidor-detail-in {
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
    .public-humidor-detail-enter {
      animation: none;
    }
  }
</style>
