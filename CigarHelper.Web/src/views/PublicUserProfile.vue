<template>
  <div class="p-4 sm:p-6 max-w-5xl mx-auto space-y-8">
    <div
      v-if="loading"
      class="space-y-4">
      <Skeleton
        height="2.5rem"
        width="40%" />
      <Skeleton height="6rem" />
    </div>

    <Message
      v-else-if="error"
      severity="error"
      :closable="false"
      >{{ error }}</Message
    >

    <template v-else-if="data">
      <div>
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">{{ data.username }}</h1>
        <p class="text-gray-600 dark:text-gray-400 mt-2">
          На сайте с {{ formatDate(data.createdAt) }}
          <span v-if="data.lastLogin"> · Последняя активность: {{ formatDate(data.lastLogin) }}</span>
        </p>
      </div>

      <div>
        <h2 class="text-xl font-bold mb-4">Хьюмидоры</h2>
        <div
          v-if="!data.humidors.length"
          class="text-gray-500 dark:text-gray-400">
          Пока нет хьюмидоров.
        </div>
        <div
          v-else
          class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
          <Card
            v-for="h in data.humidors"
            :key="h.id"
            class="cursor-pointer shadow-sm hover:shadow-md transition-shadow"
            @click="openHumidor(h.id!)">
            <template #title>
              <span class="text-lg">{{ h.name }}</span>
            </template>
            <template #content>
              <p
                v-if="h.description"
                class="text-sm text-gray-600 dark:text-gray-400 line-clamp-2 mb-2">
                {{ h.description }}
              </p>
              <p class="text-sm">
                {{ h.currentCount ?? 0 }} / {{ h.capacity ?? '—' }} сигар
              </p>
            </template>
          </Card>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
  import { ref, watch } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import Card from 'primevue/card';
  import Message from 'primevue/message';
  import Skeleton from 'primevue/skeleton';
  import * as profileApi from '@/services/profileService';
  import type { PublicProfile } from '@/services/profileService';

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

  function openHumidor(id: number): void {
    const username = route.params.username as string;
    router.push({
      name: 'PublicHumidorDetail',
      params: { username, humidorId: String(id) },
    });
  }

  async function load(): Promise<void> {
    loading.value = true;
    error.value = null;
    data.value = null;
    const username = route.params.username as string;
    try {
      data.value = await profileApi.getPublicProfile(username);
    } catch {
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
