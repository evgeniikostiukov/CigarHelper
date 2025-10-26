<template>
  <div class="p-4 sm:p-6 lg:p-8">
    <div class="max-w-7xl mx-auto">
      <!-- Заголовок и кнопка -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
        <h1 class="text-4xl font-extrabold text-gray-900 dark:text-white tracking-tight">Мои хьюмидоры</h1>
        <Button
          @click="$router.push('/humidors/new')"
          icon="pi pi-plus"
          label="Добавить хьюмидор" />
      </div>

      <!-- Состояния -->
      <div
        v-if="loading"
        class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        <Skeleton
          v-for="n in 3"
          :key="n"
          height="16rem" />
      </div>

      <Message
        v-else-if="error"
        severity="error"
        >{{ error }}</Message
      >

      <div
        v-else-if="humidors.length === 0"
        class="text-center p-8 bg-white dark:bg-gray-800 rounded-lg shadow">
        <h3 class="text-2xl font-bold mb-2">У вас пока нет хьюмидоров</h3>
        <p class="text-gray-500 dark:text-gray-400 mb-4">Начните с создания вашего первого хьюмидора.</p>
      </div>

      <!-- Список хьюмидоров -->
      <div
        v-else
        class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-6">
        <Card
          v-for="humidor in humidors"
          :key="humidor.id"
          class="flex flex-col">
          <template #title>
            <h3
              class="text-xl font-bold tracking-tight truncate"
              @click="$router.push(`/humidors/${humidor.id}`)">
              {{ humidor.name }}
            </h3>
          </template>
          <template #subtitle>
            <div
              class="flex flex-col"
              @click="$router.push(`/humidors/${humidor.id}`)">
              <span>Вместимость: {{ humidor.currentCount ?? 0 }}/{{ humidor.capacity }}</span>
              <span v-if="humidor.humidity"
                >Влажность:
                <Badge
                  :value="humidor.humidity"
                  :severity="humidorService.getHumiditySeverity(humidor.humidity)"></Badge
              ></span>
            </div>
          </template>
          <template #content>
            <p
              class="text-gray-600 dark:text-gray-400 flex-grow h-16 line-clamp-3"
              @click="$router.push(`/humidors/${humidor.id}`)">
              {{ humidor.description || 'Нет описания.' }}
            </p>
          </template>
          <template #footer>
            <div class="flex justify-end items-center">
              <div class="flex gap-2">
                <Button
                  @click="$router.push(`/humidors/${humidor.id}/edit`)"
                  icon="pi pi-pencil"
                  text
                  rounded
                  severity="secondary" />
                <Button
                  @click="confirmDelete(humidor)"
                  icon="pi pi-trash"
                  text
                  rounded
                  severity="danger" />
              </div>
            </div>
          </template>
        </Card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import { useConfirm } from 'primevue/useconfirm';
  import { useToast } from 'primevue/usetoast';
  import humidorService from '../services/humidorService';
  import type { Humidor } from '../services/humidorService';

  const router = useRouter();
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
      console.error('Ошибка при загрузке хьюмидоров:', err);
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
          console.error('Ошибка при удалении хьюмидора:', err);
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
  .line-clamp-3 {
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 3;
    overflow: hidden;
  }
</style>
