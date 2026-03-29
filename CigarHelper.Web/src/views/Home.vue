<template>
  <section
    class="home-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-100 via-amber-50/40 to-stone-100 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-amber-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="home"
    aria-labelledby="home-heading">
    <div class="home-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto max-w-7xl">
      <header class="pb-6 sm:pb-8">
        <p
          class="mb-1.5 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-amber-900/65 dark:text-amber-200/55">
          Cigar Helper
        </p>
        <h1
          id="home-heading"
          class="text-balance text-3xl font-semibold tracking-tight text-stone-900 dark:text-amber-50/95 sm:text-4xl">
          Добро пожаловать
        </h1>
        <p class="mt-1.5 max-w-2xl text-pretty text-sm text-stone-600 dark:text-stone-400">
          Коллекция сигар и хьюмидоров под рукой: влажность, вместимость и карточки — в одном стиле с остальным приложением.
        </p>
      </header>

      <div
        class="mb-8 rounded-2xl border border-stone-200/90 bg-white/95 p-6 shadow-md shadow-stone-900/5 sm:mb-10 sm:p-8 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50"
        data-testid="home-hero">
        <p class="mx-auto max-w-2xl text-center text-base leading-relaxed text-stone-700 dark:text-stone-300 sm:text-lg">
          Отслеживайте сигары и условия в хьюмидорах. Крупные кнопки и читаемые отступы рассчитаны на телефон и тёмную тему.
        </p>

        <div
          class="mt-8 flex flex-col items-stretch justify-center gap-3 sm:flex-row sm:flex-wrap sm:items-center"
          data-testid="home-cta-row">
          <Button
            v-if="isAuthenticated"
            data-testid="home-cta-humidors"
            class="min-h-12 w-full touch-manipulation shadow-md shadow-amber-900/10 dark:shadow-black/40 sm:w-auto sm:min-h-11"
            icon="pi pi-box"
            label="Мои хьюмидоры"
            @click="$router.push({ name: 'HumidorList' })" />
          <Button
            v-if="isAuthenticated"
            data-testid="home-cta-cigars"
            class="min-h-12 w-full touch-manipulation sm:w-auto sm:min-h-11"
            icon="pi pi-star"
            label="Мои сигары"
            severity="secondary"
            outlined
            @click="$router.push({ name: 'CigarList' })" />
          <Button
            v-else
            data-testid="home-cta-login"
            class="min-h-12 w-full touch-manipulation shadow-md shadow-amber-900/10 dark:shadow-black/40 sm:mx-auto sm:w-auto sm:min-w-[12rem] sm:min-h-11"
            icon="pi pi-sign-in"
            label="Начать"
            @click="$router.push({ name: 'Login' })" />
        </div>
      </div>

      <div
        class="grid grid-cols-1 gap-5 sm:grid-cols-2 sm:gap-6 lg:grid-cols-3"
        data-testid="home-features">
        <article
          v-for="(feature, index) in features"
          :key="feature.title"
          :data-testid="`home-feature-${feature.testid}`"
          class="home-feature-enter flex min-h-[14rem] flex-col items-center rounded-2xl border border-stone-200/90 bg-white/95 p-6 text-center shadow-md shadow-stone-900/5 transition-[box-shadow,transform] duration-300 hover:border-amber-800/20 hover:shadow-lg hover:shadow-amber-900/10 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 dark:hover:border-amber-900/30 dark:hover:shadow-black/70 motion-reduce:transition-none motion-reduce:hover:translate-y-0 sm:p-7"
          :style="{ animationDelay: `${Math.min(index, 8) * 48}ms` }">
          <span
            class="mb-4 flex h-14 w-14 shrink-0 items-center justify-center rounded-2xl bg-amber-100/90 text-amber-900 dark:bg-amber-900/40 dark:text-amber-100"
            aria-hidden="true">
            <i
              :class="feature.icon"
              class="text-2xl" />
          </span>
          <h2 class="mb-3 text-xl font-semibold tracking-tight text-stone-900 dark:text-amber-50/95">
            {{ feature.title }}
          </h2>
          <p class="max-w-sm text-pretty text-sm leading-relaxed text-stone-600 dark:text-stone-400">
            {{ feature.text }}
          </p>
        </article>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { useAuth } from '@/services/useAuth';

  const { isAuthenticated } = useAuth();

  const features = [
    {
      testid: 'humidors',
      icon: 'pi pi-box',
      title: 'Хьюмидоры',
      text: 'Несколько ящиков, влажность и вместимость — всё на одном экране.',
    },
    {
      testid: 'cigars',
      icon: 'pi pi-star',
      title: 'Сигары',
      text: 'Бренд, формат, крепость и оценка — карточка и фото в коллекции.',
    },
    {
      testid: 'organized',
      icon: 'pi pi-check-circle',
      title: 'Порядок',
      text: 'Понимайте, что лежит где: привязка к хьюмидору без лишних шагов.',
    },
  ] as const;
</script>

<style scoped>
  .home-root {
    position: relative;
    isolation: isolate;
  }

  .home-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  :global(.dark) .home-grain {
    mix-blend-mode: soft-light;
  }

  .home-feature-enter {
    animation: home-feature-in 0.5s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes home-feature-in {
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
    .home-feature-enter {
      animation: none;
    }
  }
</style>
