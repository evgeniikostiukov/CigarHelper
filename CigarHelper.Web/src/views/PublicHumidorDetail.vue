<template>
  <div class="p-4 sm:p-6 lg:p-8">
    <div
      v-if="loading"
      class="max-w-7xl mx-auto space-y-6">
      <Skeleton
        height="3rem"
        width="40%" />
      <Skeleton height="12rem" />
    </div>

    <Message
      v-else-if="error"
      severity="error"
      :closable="false"
      >{{ error }}</Message
    >

    <div
      v-else-if="humidor"
      class="max-w-7xl mx-auto space-y-8">
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <p class="text-sm text-gray-500 dark:text-gray-400 mb-1">
            Публичный просмотр · {{ ownerUsername }}
          </p>
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
            label="К профилю"
            icon="pi pi-user"
            severity="secondary"
            outlined
            @click="goProfile" />
        </div>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card>
          <template #title>Вместимость</template>
          <template #content>
            <div class="text-2xl font-bold">{{ cigarCount }} / {{ humidor.capacity ?? '—' }}</div>
            <ProgressBar
              v-if="humidor.capacity"
              :value="capacityPercentage"
              class="mt-2" />
          </template>
        </Card>
        <Card v-if="humidor.humidity != null">
          <template #title>Влажность, %</template>
          <template #content>
            <Badge
              :value="humidor.humidity"
              :severity="humiditySeverity" />
          </template>
        </Card>
      </div>

      <div>
        <h2 class="text-2xl font-bold mb-4">Сигары в хьюмидоре</h2>
        <DataTable
          :value="humidor.cigars || []"
          responsiveLayout="scroll"
          :paginator="(humidor.cigars?.length ?? 0) > 8"
          removableSort
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
            sortable />
          <Column
            field="strength"
            header="Крепость"
            sortable />
          <Column
            field="rating"
            header="Рейтинг"
            sortable>
            <template #body="slotProps">
              {{ slotProps.data.rating != null ? `${slotProps.data.rating}/10` : '—' }}
            </template>
          </Column>
          <template #empty>
            <div class="text-center p-4">В этом хьюмидоре пока нет сигар.</div>
          </template>
        </DataTable>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, computed, watch } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import Card from 'primevue/card';
  import Button from 'primevue/button';
  import Message from 'primevue/message';
  import Skeleton from 'primevue/skeleton';
  import DataTable from 'primevue/datatable';
  import Column from 'primevue/column';
  import ProgressBar from 'primevue/progressbar';
  import Badge from 'primevue/badge';
  import humidorService from '@/services/humidorService';
  import * as profileApi from '@/services/profileService';
  import type { Humidor } from '@/services/humidorService';

  interface HumidorDetail extends Humidor {
    cigars?: Array<Record<string, unknown>>;
  }

  const route = useRoute();
  const router = useRouter();

  const loading = ref(true);
  const error = ref<string | null>(null);
  const humidor = ref<HumidorDetail | null>(null);

  const ownerUsername = computed(() => route.params.username as string);

  const cigarCount = computed(() => humidor.value?.cigars?.length ?? 0);

  const capacityPercentage = computed((): number => {
    const h = humidor.value;
    if (!h?.capacity) return 0;
    return Math.round((cigarCount.value / h.capacity) * 100);
  });

  const humiditySeverity = computed(() => humidorService.getHumiditySeverity(humidor.value?.humidity));

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
      humidor.value = { ...h, cigars: h.cigars || [] };
    } catch {
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
