<template>
  <div class="p-4 sm:p-6 lg:p-8">
    <div
      v-if="loading"
      class="max-w-7xl mx-auto space-y-6">
      <Skeleton
        height="3rem"
        width="40%" />
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Skeleton height="8rem" />
        <Skeleton height="8rem" />
        <Skeleton height="8rem" />
      </div>
      <Skeleton
        height="2rem"
        width="30%" />
      <Skeleton height="20rem" />
    </div>

    <div
      v-else-if="error"
      class="max-w-7xl mx-auto">
      <Message
        severity="error"
        :closable="false"
        >{{ error }}</Message
      >
    </div>

    <div
      v-else-if="humidor"
      class="max-w-7xl mx-auto space-y-8">
      <!-- Заголовок и навигация -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 class="text-4xl font-extrabold text-gray-900 dark:text-white tracking-tight">
            {{ humidor.name }}
          </h1>
          <p
            v-if="humidor.description"
            class="mt-1 text-lg text-gray-500 dark:text-gray-400">
            {{ humidor.description }}
          </p>
        </div>
        <div class="flex items-center gap-2">
          <Button
            @click="$router.push(`/humidors/${humidor.id}/edit`)"
            label="Редактировать"
            icon="pi pi-pencil"
            severity="secondary"
            outlined />
          <Button
            @click="$router.push('/humidors')"
            label="К списку"
            icon="pi pi-arrow-left"
            severity="secondary" />
        </div>
      </div>

      <!-- Статистика -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card>
          <template #title>Вместимость</template>
          <template #content>
            <div class="text-2xl font-bold">{{ humidor.cigars.length }} / {{ humidor.capacity }}</div>
            <ProgressBar
              :value="capacityPercentage"
              class="mt-2" />
          </template>
        </Card>

        <Card>
          <template #title>Температура, ℃</template>
          <template #content> </template>
        </Card>

        <Card v-if="humidor.humidity">
          <template #title>Влажность, %</template>
          <template #content
            ><Badge
              :value="humidor.humidity"
              :severity="humidorService.getHumiditySeverity(humidor.humidity)"></Badge
          ></template>
        </Card>
      </div>

      <!-- Сигары в хьюмидоре -->
      <div>
        <h2 class="text-2xl font-bold mb-4">Сигары в этом хьюмидоре</h2>
        <DataTable
          :value="humidor.cigars"
          responsiveLayout="scroll"
          :paginator="humidor.cigars.length > 5"
          removableSort
          :rows="5">
          <Column
            field="name"
            header="Название"
            sortable></Column>
          <Column
            field="brand.name"
            header="Бренд"
            sortable></Column>
          <Column
            field="size"
            header="Размер"
            sortable></Column>
          <Column
            field="strength"
            header="Крепость"
            sortable></Column>
          <Column
            field="rating"
            header="Рейтинг"
            sortable>
            <template #body="slotProps">
              {{ slotProps.data.rating ? `${slotProps.data.rating}/10` : '-' }}
            </template>
          </Column>
          <Column
            header="Действия"
            style="width: 8rem; text-align: center">
            <template #body="slotProps">
              <Button
                @click="confirmRemoveCigar(slotProps.data)"
                icon="pi pi-trash"
                severity="danger"
                text
                rounded
                :disabled="!slotProps.data.id" />
            </template>
          </Column>
          <template #empty>
            <div class="text-center p-4">В этом хьюмидоре пока нет сигар.</div>
          </template>
        </DataTable>
      </div>

      <!-- Добавить сигары -->
      <div>
        <h2 class="text-2xl font-bold mb-4">Добавить сигары в хьюмидор</h2>
        <h5>Тут отображаются сигары без хьюмидоров</h5>
        <div
          v-if="loadingAvailableCigars"
          class="text-center p-4">
          <i class="pi pi-spin pi-spinner text-2xl"></i>
        </div>
        <Message
          v-else-if="availableCigarsError"
          severity="error"
          >{{ availableCigarsError }}</Message
        >
        <Message
          v-else-if="availableCigars.length === 0"
          severity="info"
          >Нет доступных сигар для добавления.</Message
        >
        <div
          v-else
          class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
          <div
            v-for="cigar in availableCigars"
            :key="cigar.id"
            class="bg-white dark:bg-gray-800 rounded-lg shadow-sm hover:shadow-md transition-shadow duration-200">
            <Card class="h-full flex flex-col">
              <template #title>
                <div class="text-base font-bold truncate text-gray-900 dark:text-white">
                  {{ cigar.name }}
                </div>
              </template>
              <template #subtitle>
                <div class="text-sm text-gray-600 dark:text-gray-400">
                  {{ cigar.brand.name }}
                </div>
              </template>
              <template #content>
                <div class="flex flex-wrap gap-2 text-sm">
                  <Tag
                    v-if="cigar.strength"
                    :value="cigar.strength"
                    severity="info" />
                  <Tag
                    v-if="cigar.size"
                    :value="cigar.size"
                    severity="secondary" />
                  <!-- ringGauge не существует в интерфейсе Cigar -->
                </div>
              </template>
              <template #footer>
                <Button
                  @click="addCigar(cigar.id!)"
                  label="Добавить"
                  icon="pi pi-plus"
                  size="small"
                  class="w-full"
                  :disabled="!cigar.id || addingCigar === cigar.id || isHumidorFull"
                  :loading="addingCigar === cigar.id" />
              </template>
            </Card>
          </div>
        </div>
        <Message
          v-if="isHumidorFull"
          severity="warn"
          class="mt-4"
          >Хьюмидор полон. Вы не можете добавить больше сигар.</Message
        >
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { useRoute } from 'vue-router';
  import { useConfirm } from 'primevue/useconfirm';
  import { useToast } from 'primevue/usetoast';
  import humidorService from '../services/humidorService';
  import cigarService from '../services/cigarService';
  import type { Humidor } from '../services/humidorService';
  import type { Cigar } from '../services/cigarService';

  // --- Interfaces ---
  interface HumidorDetail extends Humidor {
    cigars: Cigar[];
  }

  // --- Component State ---
  const route = useRoute();
  const confirm = useConfirm();
  const toast = useToast();

  const humidor = ref<HumidorDetail | null>(null);
  const loading = ref(true);
  const error = ref<string | null>(null);
  const removingCigar = ref<number | null>(null);
  const addingCigar = ref<number | null>(null);

  const availableCigars = ref<Cigar[]>([]);
  const loadingAvailableCigars = ref(false);
  const availableCigarsError = ref<string | null>(null);

  // --- Computed Properties ---
  const capacityPercentage = computed((): number => {
    if (!humidor.value || !humidor.value.capacity) return 0;
    return Math.round((humidor.value.cigars.length / humidor.value.capacity) * 100);
  });

  const isHumidorFull = computed((): boolean => {
    if (!humidor.value || !humidor.value.capacity) return false;
    return humidor.value.cigars.length >= humidor.value.capacity;
  });

  // --- Methods ---
  const loadHumidor = async (): Promise<void> => {
    loading.value = true;
    error.value = null;
    try {
      const humidorId = Number(route.params.id);
      const humidorData = await humidorService.getHumidor(humidorId);
      humidor.value = { ...humidorData, cigars: humidorData.cigars || [] };
    } catch (err) {
      console.error('Ошибка загрузки хьюмидора:', err);
      error.value = 'Не удалось загрузить данные хьюмидора.';
    } finally {
      loading.value = false;
    }
  };

  const loadAvailableCigars = async (): Promise<void> => {
    loadingAvailableCigars.value = true;
    availableCigarsError.value = null;
    try {
      const allCigarsResult = await cigarService.getCigars(); // Получаем все сигары
      const humidorCigarIds = new Set(humidor.value?.cigars.map((c) => c.id));
      availableCigars.value = allCigarsResult.filter((c) => !humidorCigarIds.has(c.id));
    } catch (err) {
      console.error('Ошибка загрузки доступных сигар:', err);
      availableCigarsError.value = 'Не удалось загрузить список доступных сигар.';
    } finally {
      loadingAvailableCigars.value = false;
    }
  };

  const addCigar = async (cigarId: number): Promise<void> => {
    if (!humidor.value?.id) return;
    addingCigar.value = cigarId;
    try {
      await humidorService.addCigarToHumidor(humidor.value.id, cigarId, 1); // Добавляем 1 сигару по умолчанию
      toast.add({
        severity: 'success',
        summary: 'Успех',
        detail: 'Сигара добавлена в хьюмидор',
        life: 2000,
      });
      // Обновляем оба списка
      loadHumidor();
      loadAvailableCigars();
    } catch (err) {
      console.error('Ошибка при добавлении сигары:', err);
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
      message: `Вы уверены, что хотите убрать сигару "${cigar.name}" из этого хьюмидора?`,
      header: 'Подтверждение',
      icon: 'pi pi-info-circle',
      rejectClass: 'p-button-text',
      acceptClass: 'p-button-danger',
      accept: async () => {
        removingCigar.value = cigarId;
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
          toast.add({
            severity: 'error',
            summary: 'Ошибка',
            detail: 'Не удалось убрать сигару',
            life: 3000,
          });
          console.error(err);
        } finally {
          removingCigar.value = null;
        }
      },
    });
  };

  // --- Lifecycle Hooks ---
  onMounted(async () => {
    await loadHumidor();
    if (!error.value) {
      loadAvailableCigars();
    }
  });
</script>
