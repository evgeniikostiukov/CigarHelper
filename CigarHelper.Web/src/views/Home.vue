<template>
  <div class="home-tailwind container mx-auto p-4 sm:p-6 lg:p-8">
    <!-- Hero Section -->
    <div class="bg-white dark:bg-gray-800 rounded-2xl shadow-xl p-8 mb-12 text-center">
      <h1 class="text-4xl md:text-5xl font-extrabold text-gray-900 dark:text-white mb-4">
        Добро пожаловать в Cigar Helper
      </h1>
      <p class="text-lg md:text-xl text-gray-600 dark:text-gray-300 max-w-3xl mx-auto mb-6">
        Ваше комплексное решение для управления коллекцией сигар и хьюмидорами.
      </p>
      <Divider class="my-8" />
      <p class="text-gray-700 dark:text-gray-400 max-w-3xl mx-auto mb-8">
        Отслеживайте ваши сигары, контролируйте условия в хьюмидорах и никогда не теряйте из виду свою коллекцию.
      </p>
      <div class="flex justify-center items-center flex-wrap gap-4 mt-8">
        <Button
          v-if="isAuthenticated"
          @click="$router.push('/humidors')"
          severity="primary"
          size="large"
          label="Мои хьюмидоры"
          icon="pi pi-box" />
        <Button
          v-if="isAuthenticated"
          @click="$router.push('/cigars')"
          severity="secondary"
          size="large"
          label="Мои сигары"
          icon="pi pi-star" />
        <Button
          v-else
          @click="$router.push('/login')"
          severity="primary"
          size="large"
          label="Начать"
          icon="pi pi-sign-in" />
      </div>
    </div>

    <!-- Features Section -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
      <!-- Feature 1 -->
      <div
        class="bg-white dark:bg-gray-800 rounded-2xl shadow-lg p-6 flex flex-col items-center text-center h-full transition-all duration-300 ease-in-out hover:shadow-2xl hover:-translate-y-2">
        <div class="mb-4 text-blue-500 dark:text-blue-400">
          <i class="pi pi-box text-5xl"></i>
        </div>
        <h3 class="text-2xl font-bold text-gray-900 dark:text-white mb-3">Отслеживайте хьюмидоры</h3>
        <p class="text-gray-600 dark:text-gray-300">
          Создавайте несколько хьюмидоров, контролируйте влажность и температуру, отслеживайте вместимость.
        </p>
      </div>

      <!-- Feature 2 -->
      <div
        class="bg-white dark:bg-gray-800 rounded-2xl shadow-lg p-6 flex flex-col items-center text-center h-full transition-all duration-300 ease-in-out hover:shadow-2xl hover:-translate-y-2">
        <div class="mb-4 text-blue-500 dark:text-blue-400">
          <i class="pi pi-star text-5xl"></i>
        </div>
        <h3 class="text-2xl font-bold text-gray-900 dark:text-white mb-3">Управляйте сигарами</h3>
        <p class="text-gray-600 dark:text-gray-300">
          Храните информацию о ваших сигарах, включая бренд, размер, крепость и оценку.
        </p>
      </div>

      <!-- Feature 3 -->
      <div
        class="bg-white dark:bg-gray-800 rounded-2xl shadow-lg p-6 flex flex-col items-center text-center h-full transition-all duration-300 ease-in-out hover:shadow-2xl hover:-translate-y-2">
        <div class="mb-4 text-blue-500 dark:text-blue-400">
          <i class="pi pi-check-circle text-5xl"></i>
        </div>
        <h3 class="text-2xl font-bold text-gray-900 dark:text-white mb-3">Будьте организованы</h3>
        <p class="text-gray-600 dark:text-gray-300">
          Легко добавляйте или удаляйте сигары из хьюмидоров и точно знайте, где что хранится.
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { useAuth } from '@/services/useAuth';

  const { isAuthenticated } = useAuth();
</script>

<style scoped lang="postcss">
  /* Стили для кнопок PrimeVue, если они не полностью подхватывают Tailwind */
  :deep(.p-button) {
    @apply font-semibold;
  }
</style>
