<template>
  <section
    data-testid="admin-dashboard"
    aria-labelledby="admin-dashboard-heading">
    <h2
      id="admin-dashboard-heading"
      class="sr-only">
      Разделы админ-панели
    </h2>
    <div class="grid gap-4 sm:grid-cols-2">
      <RouterLink
        :to="{ name: 'AdminUsers' }"
        data-testid="admin-dashboard-card-users"
        class="group flex min-h-[7.5rem] touch-manipulation flex-col justify-between rounded-2xl border border-stone-200/90 bg-white/95 p-5 no-underline shadow-md shadow-stone-900/5 ring-1 ring-transparent transition hover:border-rose-300/50 hover:ring-rose-200/30 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 dark:hover:border-rose-700/40">
        <div class="flex items-start gap-3">
          <span
            class="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-rose-100/90 text-rose-900 dark:bg-rose-900/45 dark:text-rose-100"
            aria-hidden="true">
            <i class="pi pi-users text-lg" />
          </span>
          <div>
            <p class="font-semibold text-stone-900 dark:text-rose-50/95">Пользователи</p>
            <p class="mt-0.5 text-sm text-stone-600 dark:text-stone-400">Роли, поиск и пагинация</p>
          </div>
        </div>
        <span class="text-sm font-medium text-rose-800 group-hover:underline dark:text-rose-300">
          Перейти
          <i
            class="pi pi-arrow-right ml-1 text-xs"
            aria-hidden="true" />
        </span>
      </RouterLink>

      <RouterLink
        :to="{ name: 'AdminImages' }"
        data-testid="admin-dashboard-card-images"
        class="group flex min-h-[7.5rem] touch-manipulation flex-col justify-between rounded-2xl border border-stone-200/90 bg-white/95 p-5 no-underline shadow-md shadow-stone-900/5 ring-1 ring-transparent transition hover:border-rose-300/50 hover:ring-rose-200/30 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 dark:hover:border-rose-700/40">
        <div class="flex items-start gap-3">
          <span
            class="flex h-11 w-11 shrink-0 items-center justify-center rounded-xl bg-rose-100/90 text-rose-900 dark:bg-rose-900/45 dark:text-rose-100"
            aria-hidden="true">
            <i class="pi pi-images text-lg" />
          </span>
          <div>
            <p class="font-semibold text-stone-900 dark:text-rose-50/95">Изображения</p>
            <p class="mt-0.5 text-sm text-stone-600 dark:text-stone-400">Все файлы в хранилище (каталог и коллекции)</p>
          </div>
        </div>
        <span class="text-sm font-medium text-rose-800 group-hover:underline dark:text-rose-300">
          Перейти
          <i
            class="pi pi-arrow-right ml-1 text-xs"
            aria-hidden="true" />
        </span>
      </RouterLink>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { RouterLink } from 'vue-router';
</script>
