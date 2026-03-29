<template>
  <section
    class="humidor-detail-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-100 via-amber-50/40 to-stone-100 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-amber-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="humidor-detail"
    aria-labelledby="humidor-detail-heading">
    <div
      class="humidor-detail-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto max-w-7xl">
      <div
        v-if="loading"
        data-testid="humidor-detail-loading"
        class="min-h-[20rem] space-y-6"
        aria-busy="true"
        aria-live="polite">
        <Skeleton
          class="max-w-md rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          height="3rem" />
        <div class="grid grid-cols-1 gap-5 sm:gap-6 md:grid-cols-3">
          <Skeleton
            v-for="n in 3"
            :key="n"
            class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
            height="8rem"
            data-testid="humidor-detail-skeleton" />
        </div>
        <Skeleton
          class="max-w-xs rounded-md"
          height="1.25rem" />
        <Skeleton
          class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          height="16rem" />
      </div>

      <div
        v-else-if="error"
        class="max-w-2xl rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80"
        data-testid="humidor-detail-error"
        role="alert">
        <Message
          severity="error"
          :closable="false">
          {{ error }}
        </Message>
        <div class="mt-4 flex flex-col gap-3 sm:flex-row sm:flex-wrap">
          <Button
            data-testid="humidor-detail-retry"
            class="min-h-12 w-full touch-manipulation sm:w-auto"
            label="Повторить загрузку"
            icon="pi pi-refresh"
            severity="secondary"
            outlined
            @click="loadHumidor" />
          <Button
            data-testid="humidor-detail-back"
            class="min-h-12 w-full touch-manipulation sm:w-auto"
            label="К списку хьюмидоров"
            icon="pi pi-list"
            severity="secondary"
            outlined
            @click="router.push({ name: 'HumidorList' })" />
        </div>
      </div>

      <template v-else-if="humidor">
        <header class="flex flex-col gap-4 pb-6 sm:flex-row sm:items-end sm:justify-between sm:pb-8">
          <div class="min-w-0">
            <p
              class="mb-1.5 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-amber-900/65 dark:text-amber-200/55">
              Коллекция
            </p>
            <h1
              id="humidor-detail-heading"
              class="text-balance text-3xl font-semibold tracking-tight text-stone-900 dark:text-amber-50/95 sm:text-4xl">
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
              Без описания — можно добавить при редактировании.
            </p>
          </div>
          <div class="flex w-full shrink-0 flex-col gap-2 sm:w-auto sm:flex-row sm:flex-wrap sm:justify-end">
            <Button
              data-testid="humidor-detail-edit"
              class="min-h-12 w-full touch-manipulation shadow-md shadow-amber-900/10 dark:shadow-black/40 sm:min-h-11 sm:w-auto"
              label="Редактировать"
              icon="pi pi-pencil"
              @click="router.push({ name: 'HumidorEdit', params: { id: humidor.id } })" />
            <Button
              data-testid="humidor-detail-to-list"
              class="min-h-12 w-full touch-manipulation sm:min-h-11 sm:w-auto"
              label="К списку"
              icon="pi pi-arrow-left"
              severity="secondary"
              outlined
              @click="router.push({ name: 'HumidorList' })" />
          </div>
        </header>

        <div
          class="humidor-detail-enter space-y-6 sm:space-y-8"
          data-testid="humidor-detail-content">
          <div
            class="grid grid-cols-1 gap-5 sm:gap-6 md:grid-cols-3"
            data-testid="humidor-detail-stats">
            <div
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6">
              <h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-stone-500 dark:text-stone-400">
                Вместимость
              </h2>
              <p class="text-2xl font-semibold text-stone-900 dark:text-amber-50/95">
                {{ humidor.cigars.length }} / {{ humidor.capacity }}
              </p>
              <ProgressBar
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
              <p class="mt-2 text-xs text-stone-500 dark:text-stone-500">Поле зарезервировано под датчики.</p>
            </div>
            <div
              class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6">
              <h2 class="mb-3 text-sm font-semibold uppercase tracking-wide text-stone-500 dark:text-stone-400">
                Влажность
              </h2>
              <div v-if="humidor.humidity">
                <Badge
                  :value="`${humidor.humidity}%`"
                  :severity="humidorService.getHumiditySeverity(humidor.humidity)" />
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
            aria-labelledby="humidor-detail-cigars-heading">
            <h2
              id="humidor-detail-cigars-heading"
              class="mb-4 text-lg font-semibold text-stone-900 dark:text-amber-50/95">
              Сигары в этом хьюмидоре
            </h2>
            <DataTable
              :value="humidor.cigars"
              class="humidor-detail-table"
              data-testid="humidor-detail-table"
              responsive-layout="scroll"
              :paginator="humidor.cigars.length > 5"
              removable-sort
              :rows="5">
              <Column
                field="name"
                header="Название"
                sortable>
                <template #body="slotProps">
                  <router-link
                    :to="{ name: 'CigarDetail', params: { id: String(slotProps.data.id) } }"
                    class="font-medium text-amber-900 underline-offset-2 hover:underline dark:text-amber-200/95"
                    @click.stop>
                    {{ slotProps.data.name }}
                  </router-link>
                </template>
              </Column>
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
              <Column
                header="Действия"
                class="w-[8rem] text-center">
                <template #body="slotProps">
                  <Button
                    :data-testid="`humidor-detail-remove-cigar-${slotProps.data.id}`"
                    class="min-h-11 min-w-11 touch-manipulation"
                    icon="pi pi-trash"
                    severity="danger"
                    text
                    rounded
                    :disabled="!slotProps.data.id"
                    aria-label="Убрать сигару из хьюмидора"
                    @click="confirmRemoveCigar(slotProps.data)" />
                </template>
              </Column>
              <template #empty>
                <div
                  class="rounded-xl border border-dashed border-amber-800/25 bg-stone-50/80 px-5 py-10 text-center text-stone-600 dark:border-amber-200/15 dark:bg-stone-950/40 dark:text-stone-400"
                  data-testid="humidor-detail-table-empty">
                  В этом хьюмидоре пока нет сигар.
                </div>
              </template>
            </DataTable>
          </section>

          <section
            class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6"
            aria-labelledby="humidor-detail-add-heading"
            data-testid="humidor-detail-available">
            <h2
              id="humidor-detail-add-heading"
              class="mb-1 text-lg font-semibold text-stone-900 dark:text-amber-50/95">
              Добавить сигары
            </h2>
            <p class="mb-5 text-sm text-stone-600 dark:text-stone-400">
              Ниже — сигары вашей коллекции, которых ещё нет в этом хьюмидоре (остальные хьюмидоры не учитываются).
            </p>

            <div
              v-if="loadingAvailableCigars"
              class="flex min-h-[8rem] flex-col items-center justify-center gap-3 rounded-xl border border-stone-200/70 bg-stone-50/50 dark:border-stone-700/60 dark:bg-stone-950/35"
              data-testid="humidor-detail-available-loading"
              aria-busy="true">
              <i
                class="pi pi-spin pi-spinner text-2xl text-amber-800 dark:text-amber-400"
                aria-hidden="true" />
              <span class="text-sm text-stone-600 dark:text-stone-400">Загрузка списка…</span>
            </div>

            <div
              v-else-if="availableCigarsError"
              class="rounded-xl border border-red-200/70 bg-red-50/50 p-4 dark:border-red-900/40 dark:bg-red-950/20"
              data-testid="humidor-detail-available-error"
              role="alert">
              <Message
                severity="error"
                :closable="false">
                {{ availableCigarsError }}
              </Message>
              <Button
                data-testid="humidor-detail-available-retry"
                class="mt-3 min-h-12 w-full touch-manipulation sm:w-auto"
                label="Повторить"
                icon="pi pi-refresh"
                severity="secondary"
                outlined
                @click="loadAvailableCigars" />
            </div>

            <div
              v-else-if="availableCigars.length === 0"
              class="rounded-xl border border-dashed border-amber-800/25 bg-stone-50/80 px-5 py-10 text-center dark:border-amber-200/15 dark:bg-stone-950/40"
              data-testid="humidor-detail-available-empty">
              <p class="text-stone-600 dark:text-stone-400">Нет сигар для добавления в этот хьюмидор.</p>
            </div>

            <div
              v-else
              class="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 sm:gap-6"
              data-testid="humidor-detail-available-grid">
              <article
                v-for="(cigar, index) in availableCigars"
                :key="cigar.id"
                v-memo="[
                  cigar.id,
                  cigar.name,
                  cigar.strength,
                  cigar.size,
                  cigar.brand?.name,
                  addingCigar === cigar.id,
                  isHumidorFull,
                ]"
                :data-testid="`humidor-detail-add-card-${cigar.id}`"
                class="available-card-enter flex flex-col overflow-hidden rounded-2xl border border-stone-200/90 bg-white/95 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50"
                :style="{ animationDelay: `${Math.min(index, 8) * 40}ms` }">
                <div class="flex flex-1 flex-col gap-2 border-b border-stone-100 p-4 dark:border-stone-700/80">
                  <h3 class="line-clamp-2 text-base font-semibold text-stone-900 dark:text-amber-50/95">
                    {{ cigar.name }}
                  </h3>
                  <p class="text-sm text-stone-600 dark:text-stone-400">
                    {{ cigar.brand.name }}
                  </p>
                  <div class="flex flex-wrap gap-2 pt-1">
                    <Tag
                      v-if="cigar.strength"
                      :value="getStrengthLabel(cigar.strength) || cigar.strength"
                      severity="secondary" />
                    <Tag
                      v-if="cigar.size"
                      :value="cigar.size"
                      severity="info" />
                  </div>
                </div>
                <footer class="mt-auto bg-stone-50/90 p-3 dark:bg-stone-950/50">
                  <Button
                    :data-testid="`humidor-detail-add-${cigar.id}`"
                    class="min-h-11 w-full touch-manipulation"
                    label="Добавить"
                    icon="pi pi-plus"
                    size="small"
                    :disabled="!cigar.id || addingCigar === cigar.id || isHumidorFull"
                    :loading="addingCigar === cigar.id"
                    @click="addCigar(cigar.id!)" />
                </footer>
              </article>
            </div>

            <Message
              v-if="isHumidorFull"
              severity="warn"
              class="mt-4"
              data-testid="humidor-detail-full"
              :closable="false">
              Хьюмидор заполнен — уберите сигары или увеличьте вместимость в настройках.
            </Message>
          </section>
        </div>
      </template>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { useConfirm } from 'primevue/useconfirm';
  import { useToast } from 'primevue/usetoast';
  import DataTable from 'primevue/datatable';
  import Column from 'primevue/column';
  import ProgressBar from 'primevue/progressbar';
  import humidorService from '../services/humidorService';
  import cigarService from '../services/cigarService';
  import type { Humidor } from '../services/humidorService';
  import type { Cigar } from '../services/cigarService';
  import { strengthOptions } from '../utils/cigarOptions';

  interface HumidorDetail extends Humidor {
    cigars: Cigar[];
  }

  const route = useRoute();
  const router = useRouter();
  const confirm = useConfirm();
  const toast = useToast();

  const humidor = ref<HumidorDetail | null>(null);
  const loading = ref(true);
  const error = ref<string | null>(null);
  const addingCigar = ref<number | null>(null);

  const availableCigars = ref<Cigar[]>([]);
  const loadingAvailableCigars = ref(false);
  const availableCigarsError = ref<string | null>(null);

  const capacityPercentage = computed((): number => {
    if (!humidor.value || !humidor.value.capacity) return 0;
    return Math.min(100, Math.round((humidor.value.cigars.length / humidor.value.capacity) * 100));
  });

  const isHumidorFull = computed((): boolean => {
    if (!humidor.value || !humidor.value.capacity) return false;
    return humidor.value.cigars.length >= humidor.value.capacity;
  });

  function getStrengthLabel(strength: string | null | undefined): string {
    if (!strength) return '';
    return strengthOptions.find((o) => o.value === strength)?.label || strength;
  }

  const loadHumidor = async (): Promise<void> => {
    loading.value = true;
    error.value = null;
    try {
      const humidorId = Number(route.params.id);
      const humidorData = await humidorService.getHumidor(humidorId);
      humidor.value = { ...humidorData, cigars: humidorData.cigars || [] };
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка загрузки хьюмидора:', err);
      }
      error.value = 'Не удалось загрузить данные хьюмидора.';
    } finally {
      loading.value = false;
    }
  };

  const loadAvailableCigars = async (): Promise<void> => {
    loadingAvailableCigars.value = true;
    availableCigarsError.value = null;
    try {
      const allCigarsResult = await cigarService.getCigars();
      const humidorCigarIds = new Set(humidor.value?.cigars.map((c) => c.id));
      availableCigars.value = allCigarsResult.filter((c) => !humidorCigarIds.has(c.id));
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка загрузки доступных сигар:', err);
      }
      availableCigarsError.value = 'Не удалось загрузить список доступных сигар.';
    } finally {
      loadingAvailableCigars.value = false;
    }
  };

  const addCigar = async (cigarId: number): Promise<void> => {
    if (!humidor.value?.id) return;
    addingCigar.value = cigarId;
    try {
      await humidorService.addCigarToHumidor(humidor.value.id, cigarId);
      toast.add({
        severity: 'success',
        summary: 'Успех',
        detail: 'Сигара добавлена в хьюмидор',
        life: 2000,
      });
      await loadHumidor();
      await loadAvailableCigars();
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка при добавлении сигары:', err);
      }
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось добавить сигару',
        life: 3000,
      });
    } finally {
      addingCigar.value = null;
    }
  };

  const confirmRemoveCigar = (cigar: Cigar): void => {
    if (!humidor.value || !humidor.value.id || !cigar.id) return;

    const humidorId = humidor.value.id;
    const cigarId = cigar.id;

    confirm.require({
      message: `Убрать сигару «${cigar.name}» из этого хьюмидора?`,
      header: 'Подтверждение',
      icon: 'pi pi-exclamation-triangle',
      rejectClass: 'p-button-secondary p-button-outlined',
      acceptClass: 'p-button-danger',
      rejectLabel: 'Отмена',
      acceptLabel: 'Убрать',
      accept: async () => {
        try {
          await humidorService.removeCigarFromHumidor(humidorId, cigarId);
          toast.add({
            severity: 'info',
            summary: 'Успешно',
            detail: 'Сигара убрана',
            life: 3000,
          });
          await Promise.all([loadHumidor(), loadAvailableCigars()]);
        } catch (err) {
          if (import.meta.env.DEV) {
            console.error(err);
          }
          toast.add({
            severity: 'error',
            summary: 'Ошибка',
            detail: 'Не удалось убрать сигару',
            life: 3000,
          });
        }
      },
    });
  };

  onMounted(async () => {
    await loadHumidor();
    if (!error.value) {
      void loadAvailableCigars();
    }
  });
</script>

<style scoped>
  .humidor-detail-root {
    position: relative;
    isolation: isolate;
  }

  .humidor-detail-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  :global(.dark) .humidor-detail-grain {
    mix-blend-mode: soft-light;
  }

  .humidor-detail-enter {
    animation: humidor-detail-in 0.45s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes humidor-detail-in {
    from {
      opacity: 0;
      transform: translateY(8px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  .available-card-enter {
    animation: humidor-available-card-in 0.42s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes humidor-available-card-in {
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
    .humidor-detail-enter,
    .available-card-enter {
      animation: none;
    }
  }

  .line-clamp-2 {
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 2;
    overflow: hidden;
  }
</style>
