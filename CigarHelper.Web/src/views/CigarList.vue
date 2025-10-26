<template>
  <div class="max-w-7xl mx-auto p-6">
    <div class="flex justify-between items-center mb-8">
      <h1 class="text-3xl font-bold text-gray-900">Мои сигары</h1>
      <router-link
        to="/cigars/new"
        class="bg-blue-600 text-white px-6 py-2 rounded-md hover:bg-blue-700 transition-colors">
        Добавить сигару
      </router-link>
    </div>

    <div
      v-if="loading"
      class="text-center py-12">
      <div class="inline-block animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600"></div>
      <p class="mt-2 text-gray-600">Загрузка сигар...</p>
    </div>

    <div
      v-else-if="cigars.length === 0"
      class="text-center py-12">
      <div class="text-gray-400 mb-4">
        <svg
          class="mx-auto h-12 w-12"
          fill="none"
          viewBox="0 0 24 24"
          stroke="currentColor">
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
        </svg>
      </div>
      <h3 class="text-lg font-medium text-gray-900 mb-2">У вас пока нет сигар</h3>
      <p class="text-gray-600 mb-6">Начните добавлять сигары в свою коллекцию</p>
      <router-link
        to="/cigars/new"
        class="bg-blue-600 text-white px-6 py-2 rounded-md hover:bg-blue-700 transition-colors">
        Добавить первую сигару
      </router-link>
    </div>

    <div
      v-else
      class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-6">
      <div
        v-for="cigar in cigars"
        :key="cigar.id"
        class="bg-white rounded-lg shadow-md overflow-hidden flex flex-col">
        <Carousel
          :value="cigar.images"
          :numVisible="1"
          :numScroll="1"
          class="w-full"
          :showIndicators="cigar.images && cigar.images.length > 1">
          <template #item="slotProps">
            <div
              class="h-48 flex items-center justify-center bg-gray-100 cursor-pointer"
              @click="viewCigar(cigar)">
              <img
                v-if="slotProps.data.imageData"
                :src="`data:image/jpeg;base64,${arrayBufferToBase64(slotProps.data.imageData)}`"
                :alt="cigar.name"
                class="w-full h-full object-cover" />
              <i
                v-else
                class="pi pi-image text-5xl text-gray-400"></i>
            </div>
          </template>
          <template #empty>
            <div
              class="w-full h-48 flex items-center justify-center bg-gray-100 cursor-pointer"
              @click="viewCigar(cigar)">
              <i class="pi pi-image text-5xl text-gray-400"></i>
            </div>
          </template>
        </Carousel>

        <div
          class="p-4 flex-grow flex flex-col justify-between cursor-pointer"
          @click="viewCigar(cigar)">
          <div>
            <h3 class="font-semibold text-lg text-gray-900 mb-1 truncate">
              {{ cigar.name }}
            </h3>
            <p class="text-sm text-gray-600 mb-2">{{ cigar.brand.name }}</p>

            <div class="flex items-center justify-between text-sm text-gray-500 mb-3">
              <span v-if="cigar.size">{{ cigar.size }}</span>
              <span v-if="cigar.strength">{{ cigar.strength }}</span>
            </div>

            <div class="flex items-center justify-between">
              <div
                v-if="cigar.rating"
                class="flex items-center">
                <div class="flex text-yellow-400">
                  <svg
                    v-for="i in 5"
                    :key="i"
                    class="h-4 w-4"
                    :class="i <= cigar.rating ? 'text-yellow-400' : 'text-gray-300'"
                    fill="currentColor"
                    viewBox="0 0 20 20">
                    <path
                      d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                  </svg>
                </div>
                <span class="ml-1 text-sm text-gray-600">{{ cigar.rating }}/10</span>
              </div>
              <div
                v-if="cigar.price"
                class="text-sm font-medium text-gray-900">
                {{ formatPrice(cigar.price) }}
              </div>
            </div>
          </div>

          <div
            v-if="cigar.humidorId"
            class="mt-4 pt-3 border-t border-gray-200">
            <div class="flex items-center space-x-2 text-sm text-gray-600 dark:text-gray-400">
              <i class="pi pi-box text-blue-500"></i>
              <span>В хьюмидоре</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import cigarService from '../services/cigarService';
  import type { Cigar } from '../services/cigarService';
  import { arrayBufferToBase64 } from '@/utils/imageUtils';
  import Carousel from 'primevue/carousel';

  const router = useRouter();
  const loading = ref(true);
  const cigars = ref<Cigar[]>([]);

  async function loadCigars(): Promise<void> {
    try {
      const result = await cigarService.getCigars();
      cigars.value = result;
    } catch (error) {
      console.error('Ошибка загрузки сигар:', error);
    } finally {
      loading.value = false;
    }
  }

  function viewCigar(cigar: Cigar): void {
    if (cigar.id) {
      router.push(`/cigars/${cigar.id}`);
    }
  }

  function formatPrice(price: number): string {
    return new Intl.NumberFormat('ru-RU', {
      style: 'currency',
      currency: 'RUB',
    }).format(price);
  }

  onMounted(() => {
    loadCigars();
  });
</script>
