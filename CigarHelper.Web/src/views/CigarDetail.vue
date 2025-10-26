<template>
  <div class="p-4 max-w-6xl mx-auto">
    <!-- Loading State -->
    <div
      v-if="loading"
      class="space-y-6">
      <Skeleton
        height="3rem"
        class="mb-4" />
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <Skeleton height="20rem" />
        <div class="lg:col-span-2 space-y-4">
          <Skeleton height="2rem" />
          <Skeleton height="1rem" />
          <Skeleton height="1rem" />
          <Skeleton height="1rem" />
        </div>
      </div>
    </div>

    <!-- Error State -->
    <div
      v-else-if="error"
      class="bg-red-50 dark:bg-red-900/20 border border-red-200 dark:border-red-800 rounded-lg p-4">
      <div class="flex">
        <div class="flex-shrink-0">
          <i class="pi pi-exclamation-triangle text-red-400"></i>
        </div>
        <div class="ml-3">
          <h3 class="text-sm font-medium text-red-800 dark:text-red-200">Ошибка</h3>
          <div class="mt-2 text-sm text-red-700 dark:text-red-300">
            {{ error }}
          </div>
        </div>
      </div>
    </div>

    <!-- Cigar Details -->
    <div
      v-else-if="cigar"
      class="space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
            {{ cigar.name }}
          </h1>
          <p class="text-gray-600 dark:text-gray-400 mt-2">
            {{ cigar.brand.name }}
          </p>
        </div>
        <div class="flex gap-2">
          <Button
            label="Редактировать"
            icon="pi pi-pencil"
            class="p-button-outlined"
            @click="editCigar" />
          <Button
            label="Удалить"
            icon="pi pi-trash"
            class="p-button-outlined p-button-danger"
            @click="confirmDelete" />
        </div>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <!-- Image Section -->
        <div class="lg:col-span-1">
          <Card class="h-fit">
            <template #content>
              <div
                v-if="cigar.images && cigar.images.length > 0"
                class="aspect-square overflow-hidden rounded-lg">
                <img
                  :src="`data:image/jpeg;base64,${cigar.images[0].imageData}`"
                  :alt="cigar.name"
                  class="w-full h-full object-cover" />
              </div>
              <div
                v-else
                class="aspect-square bg-gray-100 dark:bg-gray-800 rounded-lg flex items-center justify-center">
                <i class="pi pi-image text-6xl text-gray-400"></i>
              </div>
            </template>
          </Card>
        </div>

        <!-- Details Section -->
        <div class="lg:col-span-2 space-y-6">
          <!-- Basic Information -->
          <Card>
            <template #title>
              <div class="flex items-center gap-2">
                <i class="pi pi-info-circle text-blue-500"></i>
                <span>Основная информация</span>
              </div>
            </template>
            <template #content>
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div>
                  <label class="block text-sm font-medium text-gray-500 dark:text-gray-400">Название</label>
                  <p class="text-gray-900 dark:text-white font-medium">
                    {{ cigar.name }}
                  </p>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-500 dark:text-gray-400">Бренд</label>
                  <p class="text-gray-900 dark:text-white font-medium">
                    {{ cigar.brand.name }}
                  </p>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-500 dark:text-gray-400">Страна</label>
                  <p class="text-gray-900 dark:text-white">
                    {{ cigar.country || '-' }}
                  </p>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-500 dark:text-gray-400">Размер</label>
                  <p class="text-gray-900 dark:text-white">
                    {{ cigar.size || '-' }}
                  </p>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-500 dark:text-gray-400">Крепость</label>
                  <p class="text-gray-900 dark:text-white">
                    {{ getStrengthLabel(cigar.strength) || '-' }}
                  </p>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-500 dark:text-gray-400">Цена</label>
                  <p class="text-gray-900 dark:text-white">
                    {{ cigar.price ? `${cigar.price} ₽` : '-' }}
                  </p>
                </div>
              </div>
            </template>
          </Card>

          <!-- Rating -->
          <Card v-if="cigar.rating">
            <template #title>
              <div class="flex items-center gap-2">
                <i class="pi pi-star text-yellow-500"></i>
                <span>Оценка</span>
              </div>
            </template>
            <template #content>
              <div class="flex items-center gap-4">
                <Rating
                  :modelValue="cigar.rating"
                  :readonly="true"
                  :cancel="false"
                  :stars="10" />
                <span class="text-lg font-medium text-gray-900 dark:text-white"> {{ cigar.rating }}/10 </span>
              </div>
            </template>
          </Card>

          <!-- Cigar Structure -->
          <Card v-if="cigar.wrapper || cigar.binder || cigar.filler">
            <template #title>
              <div class="flex items-center gap-2">
                <i class="pi pi-layer-group text-green-500"></i>
                <span>Структура сигары</span>
              </div>
            </template>
            <template #content>
              <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                  <label class="block text-sm font-medium text-gray-500 dark:text-gray-400">Покровный лист</label>
                  <p class="text-gray-900 dark:text-white">
                    {{ cigar.wrapper || '-' }}
                  </p>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-500 dark:text-gray-400">Связующий лист</label>
                  <p class="text-gray-900 dark:text-white">
                    {{ cigar.binder || '-' }}
                  </p>
                </div>
                <div>
                  <label class="block text-sm font-medium text-gray-500 dark:text-gray-400">Наполнитель</label>
                  <p class="text-gray-900 dark:text-white">
                    {{ cigar.filler || '-' }}
                  </p>
                </div>
              </div>
            </template>
          </Card>

          <!-- Хранение -->
          <div class="bg-white dark:bg-gray-800 rounded-lg shadow-sm p-6">
            <h3 class="text-lg font-semibold text-gray-900 dark:text-white mb-4 flex items-center">
              <i class="pi pi-box text-blue-500 mr-2"></i>
              Хранение
            </h3>

            <div
              v-if="humidor"
              class="space-y-4">
              <div class="flex items-center justify-between">
                <h4 class="text-lg font-medium text-gray-900 dark:text-white">
                  {{ humidor.name }}
                </h4>
                <Button
                  :label="'Перейти к хьюмидору'"
                  icon="pi pi-external-link"
                  class="p-button-sm p-button-outlined"
                  @click="goToHumidor(humidor.id!)" />
              </div>

              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div class="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
                  <div class="text-sm text-gray-600 dark:text-gray-400">Вместимость</div>
                  <div class="text-lg font-semibold text-gray-900 dark:text-white">{{ humidor.capacity }} сигар</div>
                </div>

                <div
                  v-if="humidor.humidity"
                  class="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
                  <div class="text-sm text-gray-600 dark:text-gray-400">Текущая влажность</div>
                  <div class="text-lg font-semibold text-blue-600 dark:text-blue-400">{{ humidor.humidity }}%</div>
                </div>

                <div
                  v-if="humidor.description"
                  class="bg-gray-50 dark:bg-gray-700 rounded-lg p-4">
                  <div class="text-sm text-gray-600 dark:text-gray-400 mb-2">Описание</div>
                  <div class="text-gray-900 dark:text-white">{{ humidor.description }}</div>
                </div>

                <div
                  v-else
                  class="text-center py-8">
                  <i class="pi pi-box text-gray-400 text-4xl mb-4"></i>
                  <p class="text-gray-600 dark:text-gray-400">Сигара не хранится в хьюмидоре</p>
                  <Button
                    label="Добавить в хьюмидор"
                    icon="pi pi-plus"
                    class="mt-4"
                    @click="editCigar" />
                </div>
              </div>
            </div>
          </div>

          <!-- Description -->
          <Card v-if="cigar.description">
            <template #title>
              <div class="flex items-center gap-2">
                <i class="pi pi-file-text text-indigo-500"></i>
                <span>Описание</span>
              </div>
            </template>
            <template #content>
              <p class="text-gray-700 dark:text-gray-300 whitespace-pre-wrap">
                {{ cigar.description }}
              </p>
            </template>
          </Card>
        </div>
      </div>
    </div>

    <!-- Delete Confirmation Dialog -->
    <ConfirmDialog />
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { useConfirm } from 'primevue/useconfirm';
  import { useToast } from 'primevue/usetoast';
  import cigarService from '../services/cigarService';
  import humidorService from '../services/humidorService';
  import type { Cigar, Brand } from '../services/cigarService';
  import type { Humidor } from '../services/humidorService';
  import { strengthOptions } from '../utils/cigarOptions';

  // Composables
  const route = useRoute();
  const router = useRouter();
  const confirm = useConfirm();
  const toast = useToast();

  // Reactive data
  const cigar = ref<Cigar | null>(null);
  const humidor = ref<Humidor | null>(null);
  const loading = ref(true);
  const error = ref<string | null>(null);
  const brands = ref<Brand[]>([]);

  // Methods
  async function loadBrands(): Promise<void> {
    try {
      brands.value = await cigarService.getBrands();
    } catch (error) {
      console.error('Error loading brands:', error);
    }
  }

  async function loadCigar(id: string) {
    loading.value = true;
    try {
      cigar.value = await cigarService.getCigar(id);
      if (cigar.value && cigar.value.humidorId) {
        await loadHumidor(cigar.value.humidorId);
      }
    } catch (err) {
      error.value = 'Не удалось загрузить данные о сигаре.';
      console.error(err);
    } finally {
      loading.value = false;
    }
  }

  async function loadHumidor(id: number) {
    try {
      humidor.value = await humidorService.getHumidor(id);
    } catch (err) {
      console.error('Не удалось загрузить данные о хьюмидоре:', err);
      // Не показываем ошибку, так как это может быть ожидаемо
    }
  }

  function getStrengthLabel(strength?: string | null): string {
    if (!strength) return '';
    const strengthMap: Record<string, string> = {
      very_mild: 'Очень легкая',
      mild: 'Легкая',
      medium: 'Средняя',
      full: 'Полная',
      very_full: 'Очень полная',
    };
    return strengthMap[strength] || strength;
  }

  function editCigar(): void {
    if (!cigar.value) return;
    router.push(`/cigars/${cigar.value.id}/edit`);
  }

  function confirmDelete(): void {
    if (!cigar.value) return;
    confirm.require({
      message: `Вы уверены, что хотите удалить сигару "${cigar.value.name}"?`,
      header: 'Подтверждение удаления',
      icon: 'pi pi-exclamation-triangle',
      acceptClass: 'p-button-danger',
      accept: () => deleteCigar(),
      reject: () => {
        toast.add({
          severity: 'info',
          summary: 'Отменено',
          detail: 'Операция удаления отменена',
          life: 3000,
        });
      },
    });
  }

  async function deleteCigar(): Promise<void> {
    if (!cigar.value?.id) return;
    try {
      await cigarService.deleteCigar(cigar.value.id);
      toast.add({
        severity: 'success',
        summary: 'Успешно',
        detail: `Сигара "${cigar.value.name}" удалена`,
        life: 3000,
      });
      router.push('/cigars');
    } catch (err) {
      console.error('Error deleting cigar:', err);
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось удалить сигару',
        life: 3000,
      });
    }
  }

  function goToHumidor(humidorId: number): void {
    router.push(`/humidors/${humidorId}`);
  }

  // Lifecycle
  onMounted(async () => {
    await loadBrands();
    await loadCigar(route.params.id as string);
  });
</script>
