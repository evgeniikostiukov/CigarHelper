<template>
  <section
    class="humidor-form-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-50 via-rose-50/40 to-stone-50 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-rose-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="humidor-form"
    aria-labelledby="humidor-form-heading">
    <div
      class="humidor-form-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto max-w-2xl">
      <header class="flex flex-col gap-4 pb-6 sm:flex-row sm:items-end sm:justify-between sm:pb-8">
        <div class="min-w-0">
          <p
            class="mb-1.5 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55">
            Коллекция
          </p>
          <h1
            id="humidor-form-heading"
            class="text-balance text-3xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 sm:text-4xl">
            {{ isEdit ? 'Редактировать хьюмидор' : 'Новый хьюмидор' }}
          </h1>
          <p class="mt-1.5 max-w-xl text-pretty text-sm text-stone-600 dark:text-stone-400">
            {{
              isEdit
                ? 'Обновите название, описание, вместимость и влажность — изменения сохранятся в вашей коллекции.'
                : 'Задайте вместимость и опционально влажности — позже добавите сигары из каталога или коллекции.'
            }}
          </p>
        </div>
        <Button
          data-testid="humidor-form-back"
          class="min-h-12 w-full shrink-0 touch-manipulation sm:min-h-11 sm:w-auto"
          icon="pi pi-arrow-left"
          label="К списку"
          severity="secondary"
          outlined
          @click="$router.push({ name: 'HumidorList' })" />
      </header>

      <div
        v-if="loading"
        class="min-h-[14rem] space-y-5 rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/40 sm:p-6"
        data-testid="humidor-form-loading"
        aria-busy="true"
        aria-live="polite">
        <div
          v-for="n in 5"
          :key="n"
          class="flex flex-col gap-2">
          <Skeleton
            class="rounded-md"
            height="1rem"
            width="8rem" />
          <Skeleton
            class="rounded-xl border border-stone-200/60 dark:border-stone-600/60"
            height="2.75rem" />
        </div>
      </div>

      <div
        v-else-if="loadError"
        class="max-w-2xl rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80"
        data-testid="humidor-form-error"
        role="alert">
        <Message
          severity="error"
          :closable="false">
          {{ loadError }}
        </Message>
        <Button
          v-if="isEdit && lastLoadedId != null"
          data-testid="humidor-form-retry"
          class="mt-4 min-h-12 w-full touch-manipulation sm:w-auto"
          label="Повторить загрузку"
          icon="pi pi-refresh"
          severity="secondary"
          outlined
          @click="loadHumidor(lastLoadedId)" />
        <Button
          v-else
          data-testid="humidor-form-error-back"
          class="mt-4 min-h-12 w-full touch-manipulation sm:w-auto"
          label="К списку хьюмидоров"
          icon="pi pi-list"
          severity="secondary"
          outlined
          @click="$router.push({ name: 'HumidorList' })" />
      </div>

      <div
        v-else
        class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/40 sm:p-6">
        <Message
          v-if="saveError"
          data-testid="humidor-form-save-error"
          class="mb-6"
          severity="error"
          :closable="false">
          {{ saveError }}
        </Message>

        <form
          data-testid="humidor-form-fields"
          class="flex flex-col gap-6"
          @submit.prevent="saveHumidor">
          <div class="flex flex-col gap-2">
            <label
              for="humidor-name"
              class="text-xs font-medium text-stone-600 dark:text-stone-400">
              Название <span class="text-red-600 dark:text-red-400">*</span>
            </label>
            <InputText
              id="humidor-name"
              v-model="form.name"
              data-testid="humidor-form-name"
              required
              maxlength="100"
              class="min-h-11 w-full"
              placeholder="Например, «Основной на столе»" />
          </div>

          <div class="flex flex-col gap-2">
            <label
              for="humidor-description"
              class="text-xs font-medium text-stone-600 dark:text-stone-400">
              Описание
            </label>
            <Textarea
              id="humidor-description"
              v-model="form.description"
              data-testid="humidor-form-description"
              class="min-h-[6rem] w-full"
              rows="4"
              auto-resize
              maxlength="500"
              placeholder="Заметки о деревянном ящике, увлажнителе, сезоне…" />
          </div>

          <div class="flex flex-col gap-2">
            <label
              for="humidor-capacity"
              class="text-xs font-medium text-stone-600 dark:text-stone-400">
              Вместимость (сигар) <span class="text-red-600 dark:text-red-400">*</span>
            </label>
            <InputNumber
              id="humidor-capacity"
              v-model="form.capacity"
              data-testid="humidor-form-capacity"
              class="flex! w-full"
              input-class="min-h-11"
              required
              :min="1"
              :max="3000"
              fluid
              show-buttons />
            <small class="text-stone-500 dark:text-stone-400">
              Максимальное число сигар, которое вы планируете хранить в этом хьюмидоре.
            </small>
          </div>

          <div class="flex flex-col gap-2">
            <label
              for="humidor-humidity"
              class="text-xs font-medium text-stone-600 dark:text-stone-400">
              Влажность
            </label>
            <InputNumber
              id="humidor-humidity"
              v-model="form.humidity"
              data-testid="humidor-form-humidity"
              class="flex! w-full"
              input-class="min-h-11"
              :min="1"
              :max="3000"
              fluid
              show-buttons />
            <small class="text-stone-500 dark:text-stone-400"
              >Необязательно: целевой уровень или заметка для себя.</small
            >
          </div>

          <div class="mt-2 flex flex-col-reverse gap-3 sm:flex-row sm:justify-end">
            <Button
              id="humidor-form-cancel"
              data-testid="humidor-form-cancel"
              type="button"
              class="min-h-12 w-full touch-manipulation sm:min-h-11 sm:w-auto"
              label="Отмена"
              severity="secondary"
              outlined
              @click="$router.push({ name: 'HumidorList' })" />
            <Button
              id="humidor-form-submit"
              data-testid="humidor-form-submit"
              type="submit"
              class="min-h-12 w-full touch-manipulation shadow-md shadow-rose-900/10 dark:shadow-black/40 sm:min-h-11 sm:w-auto"
              :label="isEdit ? 'Сохранить' : 'Создать'"
              icon="pi pi-check"
              :loading="saving" />
          </div>
        </form>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, reactive, computed, onMounted } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { useToast } from 'primevue/usetoast';
  import humidorService from '../services/humidorService';
  import type { Humidor } from '../services/humidorService';

  interface HumidorFormModel {
    name: string;
    description: string | null;
    capacity: number | null;
    humidity: number | null;
  }

  const route = useRoute();
  const router = useRouter();
  const toast = useToast();

  const isEdit = computed(() => route.name === 'HumidorEdit');
  const loading = ref(false);
  const saving = ref(false);
  const loadError = ref<string | null>(null);
  const saveError = ref<string | null>(null);
  const lastLoadedId = ref<number | null>(null);

  const form = reactive<HumidorFormModel>({
    name: '',
    description: null,
    capacity: 20,
    humidity: null,
  });

  async function loadHumidor(id: number): Promise<void> {
    loading.value = true;
    loadError.value = null;
    lastLoadedId.value = id;
    try {
      const humidor = await humidorService.getHumidor(id);
      form.name = humidor.name;
      form.description = humidor.description ?? null;
      form.capacity = humidor.capacity ?? null;
      form.humidity = humidor.humidity ?? null;
    } catch (err) {
      loadError.value = 'Не удалось загрузить данные хьюмидора.';
      if (import.meta.env.DEV) {
        console.error(err);
      }
    } finally {
      loading.value = false;
    }
  }

  async function saveHumidor(): Promise<void> {
    saving.value = true;
    saveError.value = null;

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
        detail: `Хьюмидор успешно ${isEdit.value ? 'обновлён' : 'создан'}.`,
        life: 3000,
      });
      await router.push({ name: 'HumidorList' });
    } catch (err) {
      const action = isEdit.value ? 'обновить' : 'создать';
      saveError.value = `Не удалось ${action} хьюмидор. Проверьте данные и попробуйте снова.`;
      if (import.meta.env.DEV) {
        console.error(err);
      }
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: saveError.value,
        life: 5000,
      });
    } finally {
      saving.value = false;
    }
  }

  onMounted(() => {
    if (!isEdit.value) {
      return;
    }
    const humidorId = Number(route.params.id);
    if (Number.isFinite(humidorId) && humidorId > 0) {
      loadHumidor(humidorId);
    } else {
      loadError.value = 'Некорректный идентификатор хьюмидора.';
    }
  });
</script>

<style scoped>
  .humidor-form-root {
    position: relative;
    isolation: isolate;
  }

  .humidor-form-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  :global(.dark) .humidor-form-grain {
    mix-blend-mode: soft-light;
  }
</style>
