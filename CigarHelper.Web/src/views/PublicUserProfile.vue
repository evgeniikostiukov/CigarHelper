<template>
  <section
    class="public-profile-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-100 via-amber-50/40 to-stone-100 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-amber-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="public-profile"
    :aria-labelledby="data ? 'public-profile-heading' : undefined"
    :aria-busy="loading">
    <div class="public-profile-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto max-w-5xl">
      <div
        v-if="loading"
        class="grid min-h-[16rem] grid-cols-1 gap-5 sm:grid-cols-2 sm:gap-6 lg:grid-cols-3"
        data-testid="public-profile-loading"
        aria-busy="true"
        aria-live="polite">
        <Skeleton
          v-for="n in 3"
          :key="n"
          class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          height="14rem"
          data-testid="public-profile-skeleton" />
      </div>

      <div
        v-else-if="error"
        class="max-w-2xl rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80"
        data-testid="public-profile-error"
        role="alert">
        <Message
          severity="error"
          :closable="false">
          {{ error }}
        </Message>
        <div class="mt-4 flex flex-col gap-3 sm:flex-row sm:flex-wrap">
          <Button
            data-testid="public-profile-retry"
            class="min-h-12 w-full touch-manipulation sm:w-auto"
            label="Повторить загрузку"
            icon="pi pi-refresh"
            severity="secondary"
            outlined
            @click="load" />
          <Button
            data-testid="public-profile-home"
            class="min-h-12 w-full touch-manipulation sm:w-auto"
            label="На главную"
            icon="pi pi-home"
            severity="secondary"
            outlined
            @click="router.push({ name: 'Home' })" />
        </div>
      </div>

      <template v-else-if="data">
        <header class="pb-6 sm:pb-8">
          <div class="min-w-0">
            <p
              class="mb-1.5 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-amber-900/65 dark:text-amber-200/55">
              Публичный профиль
            </p>
            <h1
              id="public-profile-heading"
              class="text-balance text-3xl font-semibold tracking-tight text-stone-900 dark:text-amber-50/95 sm:text-4xl">
              {{ data.username }}
            </h1>
            <p class="mt-1.5 max-w-xl text-pretty text-sm text-stone-600 dark:text-stone-400">
              На сайте с {{ formatDate(data.createdAt) }}
              <span v-if="data.lastLogin"> · Последняя активность: {{ formatDate(data.lastLogin) }}</span>
            </p>
          </div>
        </header>

        <div
          class="public-profile-enter space-y-5 sm:space-y-6"
          data-testid="public-profile-content">
          <h2 class="text-lg font-semibold text-stone-900 dark:text-amber-50/95 sm:text-xl">
            Хьюмидоры
          </h2>

          <div
            v-if="!data.humidors.length"
            class="mx-auto max-w-xl rounded-2xl border border-dashed border-amber-800/25 bg-white/80 px-5 py-12 text-center dark:border-amber-200/15 dark:bg-stone-900/60"
            data-testid="public-profile-empty">
            <span
              class="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-amber-100/90 text-amber-900 dark:bg-amber-900/40 dark:text-amber-100"
              aria-hidden="true">
              <i class="pi pi-box text-2xl" />
            </span>
            <p class="text-stone-600 dark:text-stone-400 text-pretty">
              У пользователя пока нет опубликованных хьюмидоров.
            </p>
          </div>

          <div
            v-else
            class="grid grid-cols-1 gap-5 sm:grid-cols-2 sm:gap-6 lg:grid-cols-3"
            data-testid="public-profile-humidors">
            <article
              v-for="(h, index) in data.humidors"
              :key="h.id"
              v-memo="[h.id, h.name, h.currentCount, h.capacity, h.description, h.humidity]"
              :data-testid="`public-profile-humidor-${h.id}`"
              class="public-humidor-card-enter group relative flex min-h-[13rem] flex-col overflow-hidden rounded-2xl border border-stone-200/90 bg-white/95 shadow-md shadow-stone-900/5 transition-[box-shadow,transform] duration-300 hover:shadow-lg hover:shadow-amber-900/10 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 dark:hover:border-amber-900/30 dark:hover:shadow-black/70 motion-reduce:transition-none motion-reduce:animate-none"
              :style="{ animationDelay: `${Math.min(index, 8) * 48}ms` }">
              <router-link
                :to="{
                  name: 'PublicHumidorDetail',
                  params: { username: route.params.username, humidorId: String(h.id) },
                }"
                class="absolute inset-0 z-0 rounded-2xl focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-amber-700 dark:focus-visible:outline-amber-400"
                :aria-label="`Открыть хьюмидор ${h.name}`" />

              <div class="relative z-10 flex flex-1 flex-col gap-3 p-5 pointer-events-none min-h-0">
                <h3 class="pr-2 text-lg font-semibold tracking-tight text-stone-900 dark:text-amber-50/95 line-clamp-2">
                  {{ h.name }}
                </h3>
                <div class="flex flex-col gap-1.5 text-sm text-stone-600 dark:text-stone-400">
                  <span>
                    Вместимость:
                    <span class="font-medium text-stone-800 dark:text-stone-200">
                      {{ h.currentCount ?? 0 }}/{{ h.capacity ?? '—' }}
                    </span>
                  </span>
                  <span
                    v-if="h.humidity"
                    class="inline-flex flex-wrap items-center gap-2">
                    Влажность:
                    <Badge
                      :value="h.humidity"
                      :severity="humidorService.getHumiditySeverity(h.humidity)" />
                  </span>
                </div>
                <p
                  class="line-clamp-3 mt-auto flex-1 border-t border-stone-100 pt-2 text-sm leading-relaxed text-stone-600 dark:border-stone-700/80 dark:text-stone-400">
                  {{ h.description || 'Нет описания.' }}
                </p>
              </div>

              <footer class="relative z-20 mt-auto border-t border-stone-100 bg-stone-50/90 px-3 py-3 dark:border-stone-700/80 dark:bg-stone-950/50">
                <span
                  class="flex min-h-11 w-full items-center justify-center gap-2 text-sm font-medium text-amber-900 dark:text-amber-200/90">
                  Смотреть состав
                  <i class="pi pi-arrow-right text-xs" aria-hidden="true" />
                </span>
              </footer>
            </article>
          </div>
        </div>
      </template>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, watch } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import * as profileApi from '@/services/profileService';
  import type { PublicProfile } from '@/services/profileService';
  import humidorService from '@/services/humidorService';

  const route = useRoute();
  const router = useRouter();

  const loading = ref(true);
  const error = ref<string | null>(null);
  const data = ref<PublicProfile | null>(null);

  function formatDate(iso: string): string {
    try {
      return new Date(iso).toLocaleString('ru-RU');
    } catch {
      return iso;
    }
  }

  async function load(): Promise<void> {
    loading.value = true;
    error.value = null;
    data.value = null;
    const username = route.params.username as string;
    try {
      data.value = await profileApi.getPublicProfile(username);
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка загрузки публичного профиля:', err);
      }
      error.value =
        'Профиль не найден или скрыт. Пользователь мог сделать профиль приватным.';
    } finally {
      loading.value = false;
    }
  }

  watch(
    () => route.params.username,
    () => {
      void load();
    },
    { immediate: true },
  );
</script>

<style scoped>
  .public-profile-root {
    position: relative;
    isolation: isolate;
  }

  .public-profile-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  :global(.dark) .public-profile-grain {
    mix-blend-mode: soft-light;
  }

  .public-profile-enter {
    animation: public-profile-in 0.42s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes public-profile-in {
    from {
      opacity: 0;
      transform: translateY(8px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  .public-humidor-card-enter {
    animation: public-humidor-card-in 0.48s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes public-humidor-card-in {
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
    .public-profile-enter,
    .public-humidor-card-enter {
      animation: none;
    }
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
</style>
