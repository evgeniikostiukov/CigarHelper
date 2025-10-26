<template>
  <div class="p-4 sm:p-6 lg:p-8">
    <div class="max-w-2xl mx-auto">
      <Card>
        <template #title>
          <h1 class="text-3xl font-bold text-gray-900 dark:text-white">
            {{ isEdit ? 'Редактировать хьюмидор' : 'Создать новый хьюмидор' }}
          </h1>
        </template>

        <template #content>
          <div
            v-if="loading"
            class="space-y-6 mt-4">
            <div
              v-for="n in 5"
              :key="n"
              class="flex flex-col gap-2">
              <Skeleton
                height="1.5rem"
                width="10rem" />
              <Skeleton height="2.5rem" />
            </div>
          </div>

          <Message
            v-else-if="error"
            severity="error"
            :closable="false"
            >{{ error }}</Message
          >

          <form
            v-else
            @submit.prevent="saveHumidor"
            class="flex flex-col gap-6 mt-4">
            <div class="flex flex-col gap-2">
              <label
                for="name"
                class="font-semibold"
                >Название</label
              >
              <InputText
                id="name"
                v-model="form.name"
                required
                maxlength="100"
                placeholder="Например, 'Мой основной хьюмидор'" />
            </div>

            <div class="flex flex-col gap-2">
              <label
                for="description"
                class="font-semibold"
                >Описание</label
              >
              <Textarea
                id="description"
                v-model="form.description"
                rows="4"
                autoResize
                maxlength="500"
                placeholder="Любые заметки о хьюмидоре" />
            </div>

            <div class="flex flex-col gap-2">
              <label
                for="capacity"
                class="font-semibold"
                >Вместимость</label
              >
              <InputNumber
                id="capacity"
                class="flex!"
                v-model="form.capacity"
                required
                :min="1"
                :max="3000"
                fluid />
              <small class="text-gray-500 dark:text-gray-400"
                >Максимальное количество сигар, которое может вместить хьюмидор.</small
              >
            </div>

            <div class="flex flex-col gap-2">
              <label
                for="humidity"
                class="font-semibold"
                >Влажность</label
              >
              <InputNumber
                id="humidity"
                class="flex!"
                v-model="form.humidity"
                required
                :min="1"
                :max="3000"
                fluid />
              <small class="text-gray-500 dark:text-gray-400">Необязательное поле</small>
            </div>

            <div class="flex justify-end gap-2 mt-4">
              <Button
                type="button"
                label="Отмена"
                severity="secondary"
                outlined
                @click="$router.push('/humidors')" />
              <Button
                type="submit"
                :label="isEdit ? 'Обновить' : 'Создать'"
                :loading="saving" />
            </div>
          </form>
        </template>
      </Card>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, reactive, onMounted } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { useToast } from 'primevue/usetoast';
  import humidorService from '../services/humidorService';
  import type { Humidor } from '../services/humidorService';

  // --- Interfaces ---
  interface HumidorForm {
    name: string;
    description: string | null;
    capacity: number | null;
    humidity: number | null;
  }

  // --- Component State ---
  const route = useRoute();
  const router = useRouter();
  const toast = useToast();

  const isEdit = ref(false);
  const loading = ref(false);
  const saving = ref(false);
  const error = ref<string | null>(null);

  const form = reactive<HumidorForm>({
    name: '',
    description: null,
    capacity: 20,
    humidity: null,
  });

  // --- Methods ---
  const loadHumidor = async (id: number): Promise<void> => {
    loading.value = true;
    error.value = null;
    try {
      const humidor = await humidorService.getHumidor(id);
      form.name = humidor.name;
      form.description = humidor.description ?? null;
      form.capacity = humidor.capacity ?? null;
      form.humidity = humidor.humidity ?? null;
    } catch (err) {
      error.value = 'Не удалось загрузить данные хьюмидора.';
      console.error(err);
    } finally {
      loading.value = false;
    }
  };

  const saveHumidor = async (): Promise<void> => {
    saving.value = true;
    error.value = null;

    const payload: Omit<Humidor, 'id' | 'currentCount' | 'userId'> = {
      name: form.name,
      description: form.description,
      capacity: form.capacity,
      humidity: form.humidity,
    };

    try {
      if (isEdit.value) {
        const humidorId = Number(route.params.id);
        await humidorService.updateHumidor(humidorId, payload);
      } else {
        await humidorService.createHumidor(payload);
      }
      toast.add({
        severity: 'success',
        summary: 'Успешно',
        detail: `Хьюмидор успешно ${isEdit.value ? 'обновлен' : 'создан'}!`,
        life: 3000,
      });
      router.push('/humidors');
    } catch (err) {
      const action = isEdit.value ? 'обновить' : 'создать';
      error.value = `Не удалось ${action} хьюмидор. Проверьте введенные данные.`;
      console.error(err);
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: error.value,
        life: 5000,
      });
    } finally {
      saving.value = false;
    }
  };

  // --- Lifecycle Hooks ---
  onMounted(() => {
    const humidorId = Number(route.params.id);
    if (humidorId) {
      isEdit.value = true;
      loadHumidor(humidorId);
    }
  });
</script>
