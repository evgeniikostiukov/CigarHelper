<template>
  <section
    class="humidor-list-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-50 via-rose-50/40 to-stone-50 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-rose-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="humidor-list"
    aria-labelledby="humidor-list-heading">
    <div
      class="humidor-list-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] max-w-7xl mx-auto">
      <header class="flex flex-col gap-4 sm:flex-row sm:items-end sm:justify-between pb-6 sm:pb-8">
        <div class="min-w-0">
          <p
            class="text-[0.65rem] uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55 font-semibold mb-1.5">
            Коллекция
          </p>
          <h1
            id="humidor-list-heading"
            class="text-3xl sm:text-4xl font-semibold text-stone-900 dark:text-rose-50/95 tracking-tight text-balance">
            Мои хьюмидоры
          </h1>
          <p class="mt-1.5 text-sm text-stone-600 dark:text-stone-400 max-w-xl text-pretty">
            Управляйте вместимостью и микроклиматом — касания рассчитаны на одну руку.
          </p>
        </div>
        <Button
          data-testid="humidor-list-add"
          class="w-full sm:w-auto shrink-0 min-h-12 px-5 sm:min-h-11 touch-manipulation shadow-md shadow-rose-900/10 dark:shadow-black/40"
          @click="$router.push({ name: 'HumidorForm' })"
          icon="pi pi-plus"
          label="Добавить хьюмидор" />
      </header>

      <div
        v-if="loading"
        data-testid="humidor-list-loading"
        class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5 sm:gap-6 min-h-[20rem]"
        aria-busy="true"
        aria-live="polite">
        <Skeleton
          v-for="n in 3"
          :key="n"
          class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          height="16rem"
          data-testid="humidor-list-skeleton" />
      </div>

      <div
        v-else-if="error"
        class="rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80 max-w-2xl"
        data-testid="humidor-list-error"
        role="alert">
        <Message severity="error">{{ error }}</Message>
        <Button
          data-testid="humidor-list-retry"
          class="mt-4 min-h-12 w-full sm:w-auto touch-manipulation"
          label="Повторить загрузку"
          icon="pi pi-refresh"
          severity="secondary"
          outlined
          @click="fetchHumidors" />
      </div>

      <div
        v-else-if="humidors.length === 0"
        class="text-center rounded-2xl border border-dashed border-rose-800/25 bg-white/80 px-5 py-12 dark:border-rose-200/15 dark:bg-stone-900/60 max-w-xl mx-auto"
        data-testid="humidor-list-empty">
        <span
          class="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-rose-100/90 text-rose-900 dark:bg-rose-900/40 dark:text-rose-100"
          aria-hidden="true">
          <i class="pi pi-box text-2xl" />
        </span>
        <h2 class="text-2xl font-semibold text-stone-900 dark:text-rose-50/95 mb-2">Пока пусто</h2>
        <p class="text-stone-600 dark:text-stone-400 mb-6 text-pretty">
          Создайте первый хьюмидор — с него начнётся учёт сигар и влажности.
        </p>
        <Button
          data-testid="humidor-list-empty-add"
          class="min-h-12 px-6 touch-manipulation"
          label="Создать хьюмидор"
          icon="pi pi-plus"
          @click="$router.push({ name: 'HumidorForm' })" />
      </div>

      <div
        v-else
        class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-5 sm:gap-6"
        data-testid="humidor-list-grid">
        <article
          v-for="(humidor, index) in humidors"
          :key="humidor.id"
          v-memo="[
            humidor.id,
            humidor.name,
            humidor.currentCount,
            humidor.capacity,
            humidor.humidity,
            humidor.description,
          ]"
          :data-testid="`humidor-card-${humidor.id}`"
          class="humidor-card-enter group relative flex flex-col overflow-hidden rounded-2xl border border-stone-200/90 bg-white/95 shadow-md shadow-stone-900/5 transition-[box-shadow,transform] duration-300 hover:shadow-lg hover:shadow-rose-900/10 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 dark:hover:shadow-black/70 dark:hover:border-rose-900/30 min-h-[16rem] motion-reduce:transition-none motion-reduce:animate-none"
          :style="{ animationDelay: `${Math.min(index, 8) * 48}ms` }">
          <router-link
            :to="{ name: 'HumidorDetail', params: { id: humidor.id } }"
            class="absolute inset-0 z-0 rounded-2xl focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-rose-700 dark:focus-visible:outline-rose-400"
            :aria-label="`Открыть хьюмидор ${humidor.name}`" />

          <div class="relative z-10 flex flex-1 flex-col gap-3 p-5 pointer-events-none min-h-0">
            <h2
              class="text-lg sm:text-xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 pr-2 line-clamp-2">
              {{ humidor.name }}
            </h2>
            <div class="flex flex-col gap-1.5 text-sm text-stone-600 dark:text-stone-400">
              <span>
                Вместимость:
                <span class="font-medium text-stone-800 dark:text-stone-200">
                  {{ humidor.currentCount ?? 0 }}/{{ humidor.capacity }}
                </span>
              </span>
              <span
                v-if="humidor.humidity"
                class="inline-flex flex-wrap items-center gap-2">
                Влажность:
                <Badge
                  :value="humidor.humidity"
                  :severity="humidorService.getHumiditySeverity(humidor.humidity)" />
              </span>
            </div>
            <p
              class="text-sm leading-relaxed text-stone-600 dark:text-stone-400 flex-1 line-clamp-3 mt-auto pt-1 border-t border-stone-100 dark:border-stone-700/80">
              {{ humidor.description || 'Нет описания.' }}
            </p>
          </div>

          <footer
            class="relative z-20 mt-auto flex justify-end gap-2 border-t border-stone-100 bg-stone-50/90 px-3 py-3 dark:border-stone-700/80 dark:bg-stone-950/50">
            <Button
              :data-testid="`humidor-edit-${humidor.id}`"
              class="min-h-11 min-w-11 touch-manipulation"
              icon="pi pi-pencil"
              text
              rounded
              severity="secondary"
              aria-label="Редактировать хьюмидор"
              @click.stop="$router.push({ name: 'HumidorEdit', params: { id: humidor.id } })" />
            <Button
              :data-testid="`humidor-delete-${humidor.id}`"
              class="min-h-11 min-w-11 touch-manipulation"
              icon="pi pi-trash"
              text
              rounded
              severity="danger"
              aria-label="Удалить хьюмидор"
              @click.stop="confirmDelete(humidor)" />
          </footer>
        </article>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import { useConfirm } from 'primevue/useconfirm';
  import { useToast } from 'primevue/usetoast';
  import humidorService from '../services/humidorService';
  import type { Humidor } from '../services/humidorService';

  const confirm = useConfirm();
  const toast = useToast();

  const humidors = ref<Humidor[]>([]);
  const loading = ref(true);
  const error = ref<string | null>(null);

  const fetchHumidors = async (): Promise<void> => {
    loading.value = true;
    error.value = null;
    try {
      humidors.value = await humidorService.getHumidors();
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка при загрузке хьюмидоров:', err);
      }
      error.value = 'Не удалось загрузить хьюмидоры. Попробуйте позже.';
    } finally {
      loading.value = false;
    }
  };

  const confirmDelete = (humidor: Humidor): void => {
    confirm.require({
      message: `Вы уверены, что хотите удалить хьюмидор "${humidor.name}"? Это действие нельзя отменить.`,
      header: 'Подтверждение удаления',
      icon: 'pi pi-exclamation-triangle',
      rejectClass: 'p-button-secondary p-button-outlined',
      acceptClass: 'p-button-danger',
      rejectLabel: 'Отмена',
      acceptLabel: 'Удалить',
      accept: async () => {
        if (!humidor.id) {
          toast.add({
            severity: 'error',
            summary: 'Ошибка',
            detail: 'Не удалось определить ID хьюмидора',
            life: 3000,
          });
          return;
        }
        try {
          await humidorService.deleteHumidor(humidor.id);
          humidors.value = humidors.value.filter((h) => h.id !== humidor.id);
          toast.add({
            severity: 'success',
            summary: 'Успешно',
            detail: 'Хьюмидор удален',
            life: 3000,
          });
        } catch (err) {
          if (import.meta.env.DEV) {
            console.error('Ошибка при удалении хьюмидора:', err);
          }
          toast.add({
            severity: 'error',
            summary: 'Ошибка',
            detail: 'Не удалось удалить хьюмидор',
            life: 3000,
          });
        }
      },
    });
  };

  onMounted(fetchHumidors);
</script>

<style scoped>
  .humidor-list-root {
    position: relative;
    isolation: isolate;
  }

  .humidor-list-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  :global(.dark) .humidor-list-grain {
    mix-blend-mode: soft-light;
  }

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

  .humidor-card-enter {
    animation: humidor-card-in 0.5s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes humidor-card-in {
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
    .humidor-card-enter {
      animation: none;
    }
  }
</style>
