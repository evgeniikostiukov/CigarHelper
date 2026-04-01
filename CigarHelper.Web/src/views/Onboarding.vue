<template>
  <section
    class="onboarding-root -mx-2 rounded-2xl bg-gradient-to-b from-stone-50 via-rose-50/40 to-stone-50 px-3 py-6 ring-1 ring-stone-900/5 sm:mx-0 sm:rounded-3xl sm:px-6 sm:py-8 dark:from-stone-950 dark:via-rose-950/20 dark:to-stone-950 dark:ring-stone-100/10"
    data-testid="onboarding"
    aria-labelledby="onboarding-heading">
    <div
      class="onboarding-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto max-w-5xl">
      <header class="flex flex-col gap-4 pb-6 sm:flex-row sm:items-end sm:justify-between sm:pb-8">
        <div class="min-w-0">
          <p
            class="mb-1.5 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55">
            Первые шаги
          </p>
          <h1
            id="onboarding-heading"
            class="text-balance text-3xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 sm:text-4xl">
            Настроим коллекцию за минуту
          </h1>
          <p class="mt-1.5 max-w-2xl text-pretty text-sm text-stone-600 dark:text-stone-400">
            Создадим первый хьюмидор и добавим 1–2 сигары из каталога, чтобы дашборд и списки сразу стали «живыми».
          </p>
        </div>

        <Button
          v-if="step === 2"
          data-testid="onboarding-skip"
          class="min-h-12 w-full shrink-0 touch-manipulation sm:min-h-11 sm:w-auto"
          severity="secondary"
          outlined
          icon="pi pi-forward"
          label="Пропустить"
          @click="finishOnboarding" />
      </header>

      <div class="grid grid-cols-1 gap-6 lg:grid-cols-[minmax(0,1fr)_18rem]">
        <div class="min-w-0">
          <div
            v-if="globalError"
            class="mb-6 max-w-2xl rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80"
            data-testid="onboarding-error"
            role="alert">
            <Message
              severity="error"
              :closable="false"
              >{{ globalError }}</Message
            >
          </div>

          <div
            class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/40 sm:p-6"
            data-testid="onboarding-panel">
            <div class="flex items-start justify-between gap-4">
              <div class="min-w-0">
                <h2 class="text-lg font-semibold text-stone-900 dark:text-rose-50/95">
                  {{ step === 1 ? 'Шаг 1 — первый хьюмидор' : 'Шаг 2 — добавьте 1–2 сигары' }}
                </h2>
                <p class="mt-1 text-sm text-stone-600 dark:text-stone-400">
                  {{
                    step === 1
                      ? 'Назовите место хранения — можно минимально, остальное потом.'
                      : 'Выберите пару позиций из базы — добавим их в вашу коллекцию и привяжем к хьюмидору.'
                  }}
                </p>
              </div>
              <Tag
                data-testid="onboarding-step-tag"
                :value="`${step}/2`"
                severity="secondary" />
            </div>

            <Divider class="my-5" />

            <form
              v-if="step === 1"
              data-testid="onboarding-step1"
              class="flex flex-col gap-5"
              @submit.prevent="createFirstHumidor">
              <div class="grid grid-cols-1 gap-5 md:grid-cols-2">
                <div class="flex flex-col gap-2 md:col-span-2">
                  <label
                    for="onboarding-humidor-name"
                    class="text-xs font-medium text-stone-600 dark:text-stone-400">
                    Название <span class="text-red-600 dark:text-red-400">*</span>
                  </label>
                  <InputText
                    id="onboarding-humidor-name"
                    v-model="humidorForm.name"
                    data-testid="onboarding-humidor-name"
                    class="min-h-11 w-full"
                    :invalid="!!humidorErrors.name"
                    placeholder="Например: Домашний, Кабинет, Путешествия" />
                  <small
                    v-if="humidorErrors.name"
                    class="text-sm text-red-600 dark:text-red-400">
                    {{ humidorErrors.name }}
                  </small>
                </div>

                <div class="flex flex-col gap-2 md:col-span-2">
                  <label
                    for="onboarding-humidor-description"
                    class="text-xs font-medium text-stone-600 dark:text-stone-400">
                    Описание (опционально)
                  </label>
                  <Textarea
                    id="onboarding-humidor-description"
                    v-model="humidorForm.description"
                    data-testid="onboarding-humidor-description"
                    class="min-h-[6rem] w-full"
                    rows="3"
                    placeholder="Например: коробка на полке, cedar box, travel case…" />
                </div>

                <div class="flex flex-col gap-2">
                  <label
                    for="onboarding-humidor-capacity"
                    class="text-xs font-medium text-stone-600 dark:text-stone-400">
                    Вместимость (сигар) <span class="text-red-600 dark:text-red-400">*</span>
                  </label>
                  <InputNumber
                    id="onboarding-humidor-capacity"
                    v-model="humidorForm.capacity"
                    data-testid="onboarding-humidor-capacity"
                    class="flex! w-full"
                    input-class="min-h-11"
                    :min="1"
                    :max="1000"
                    placeholder="Например: 50"
                    fluid />
                  <small
                    v-if="humidorErrors.capacity"
                    class="text-sm text-red-600 dark:text-red-400">
                    {{ humidorErrors.capacity }}
                  </small>
                  <small class="text-stone-500 dark:text-stone-400">Нужно, чтобы видеть заполненность.</small>
                </div>

                <div class="flex flex-col gap-2">
                  <label
                    for="onboarding-humidor-humidity"
                    class="text-xs font-medium text-stone-600 dark:text-stone-400">
                    Влажность (опционально)
                  </label>
                  <InputNumber
                    id="onboarding-humidor-humidity"
                    v-model="humidorForm.humidity"
                    data-testid="onboarding-humidor-humidity"
                    class="flex! w-full"
                    input-class="min-h-11"
                    :min="0"
                    :max="100"
                    placeholder="Например: 68"
                    suffix="%"
                    fluid />
                  <small class="text-stone-500 dark:text-stone-400">Можно оставить пустым.</small>
                </div>
              </div>

              <div
                class="mt-1 flex flex-col-reverse gap-3 border-t border-stone-200/80 pt-6 dark:border-stone-700/80 sm:flex-row sm:justify-end">
                <Button
                  data-testid="onboarding-cancel"
                  type="button"
                  class="min-h-12 w-full touch-manipulation sm:min-h-11 sm:w-auto"
                  severity="secondary"
                  outlined
                  icon="pi pi-times"
                  label="Отложить"
                  @click="finishOnboarding" />
                <Button
                  data-testid="onboarding-step1-submit"
                  type="submit"
                  class="min-h-12 w-full touch-manipulation shadow-md shadow-rose-900/10 dark:shadow-black/40 sm:min-h-11 sm:w-auto"
                  icon="pi pi-check"
                  :loading="step1Loading"
                  label="Создать хьюмидор" />
              </div>
            </form>

            <div
              v-else
              data-testid="onboarding-step2"
              class="flex flex-col gap-5">
              <div
                v-if="!createdHumidor"
                class="rounded-xl border border-dashed border-rose-800/25 bg-rose-50/60 p-4 dark:border-rose-200/15 dark:bg-rose-950/25"
                data-testid="onboarding-step2-missing-humidor">
                <p class="text-sm text-stone-700 dark:text-stone-300">Хьюмидор ещё не создан. Вернитесь на шаг 1.</p>
                <Button
                  data-testid="onboarding-back-to-step1"
                  class="mt-3 min-h-11 touch-manipulation"
                  severity="secondary"
                  outlined
                  icon="pi pi-arrow-left"
                  label="К шагу 1"
                  @click="step = 1" />
              </div>

              <div
                v-else
                class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-4 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-5"
                data-testid="onboarding-humidor-summary">
                <div class="flex flex-wrap items-center justify-between gap-3">
                  <div class="min-w-0">
                    <p class="text-xs font-medium text-stone-600 dark:text-stone-400">Хьюмидор</p>
                    <p class="text-base font-semibold text-stone-900 dark:text-rose-50/95">
                      {{ createdHumidor.name }}
                    </p>
                    <p
                      v-if="createdHumidor.capacity != null"
                      class="mt-1 text-sm text-stone-600 dark:text-stone-400">
                      Вместимость: {{ createdHumidor.capacity }} сигар
                    </p>
                  </div>
                  <Button
                    data-testid="onboarding-open-humidor"
                    class="min-h-11 touch-manipulation"
                    severity="secondary"
                    outlined
                    icon="pi pi-external-link"
                    label="Открыть"
                    @click="router.push({ name: 'HumidorDetail', params: { id: String(createdHumidor.id) } })" />
                </div>
              </div>

              <div class="grid grid-cols-1 gap-4 md:grid-cols-[minmax(0,1fr)_12rem]">
                <div class="min-w-0">
                  <label
                    for="onboarding-cigar-search"
                    class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
                    Поиск по базе
                  </label>
                  <IconField class="w-full">
                    <InputIcon
                      class="pi pi-search text-stone-400"
                      aria-hidden="true" />
                    <InputText
                      id="onboarding-cigar-search"
                      v-model="search"
                      data-testid="onboarding-cigar-search"
                      class="w-full min-h-12 sm:min-h-11"
                      placeholder="Название или бренд…"
                      autocomplete="off"
                      @input="onSearchChanged" />
                  </IconField>
                </div>
                <div class="flex flex-col gap-2">
                  <label class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400"> Действия </label>
                  <Button
                    data-testid="onboarding-finish"
                    class="min-h-12 w-full touch-manipulation shadow-md shadow-rose-900/10 dark:shadow-black/40 sm:min-h-11"
                    icon="pi pi-flag-checkered"
                    :disabled="step2Loading"
                    label="Завершить"
                    @click="finishOnboarding" />
                </div>
              </div>

              <div
                v-if="step2Error"
                class="rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80"
                data-testid="onboarding-step2-error"
                role="alert">
                <Message
                  severity="error"
                  :closable="false"
                  >{{ step2Error }}</Message
                >
                <Button
                  data-testid="onboarding-step2-retry"
                  class="mt-4 min-h-11 touch-manipulation"
                  severity="secondary"
                  outlined
                  icon="pi pi-refresh"
                  label="Повторить загрузку"
                  @click="loadCigarBases" />
              </div>

              <div
                v-if="step2Loading"
                data-testid="onboarding-step2-loading"
                class="grid grid-cols-1 gap-4 sm:grid-cols-2"
                aria-busy="true"
                aria-live="polite">
                <Skeleton
                  v-for="n in 4"
                  :key="n"
                  class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
                  height="9rem" />
              </div>

              <div
                v-else
                class="grid grid-cols-1 gap-4 sm:grid-cols-2"
                data-testid="onboarding-cigar-grid">
                <article
                  v-for="cigar in cigarBases"
                  :key="cigar.id"
                  class="rounded-2xl border border-stone-200/90 bg-white/95 p-4 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/40">
                  <div class="flex items-start justify-between gap-3">
                    <div class="min-w-0">
                      <h3 class="line-clamp-2 text-base font-semibold text-stone-900 dark:text-rose-50/95">
                        {{ cigar.name }}
                      </h3>
                      <p class="mt-1 text-sm text-stone-600 dark:text-stone-400">
                        {{ cigar.brand?.name }}
                        <span
                          v-if="cigar.size"
                          class="text-stone-500 dark:text-stone-500"
                          >· {{ cigar.size }}</span
                        >
                      </p>
                    </div>
                    <Button
                      :data-testid="`onboarding-add-${cigar.id}`"
                      class="min-h-11 min-w-11 touch-manipulation"
                      icon="pi pi-plus"
                      rounded
                      :disabled="
                        !createdHumidor ||
                        selectedIds.size >= 2 ||
                        creatingIds.has(cigar.id) ||
                        selectedIds.has(cigar.id)
                      "
                      :loading="creatingIds.has(cigar.id)"
                      aria-label="Добавить в коллекцию"
                      @click="addBaseToCollection(cigar)" />
                  </div>

                  <div class="mt-3 flex flex-wrap gap-2">
                    <Tag
                      v-if="cigar.strength"
                      :value="strengthLabel(cigar.strength)"
                      severity="secondary" />
                    <Tag
                      v-if="selectedIds.has(cigar.id)"
                      value="Добавлено"
                      severity="success" />
                  </div>
                </article>
              </div>

              <div
                class="mt-1 rounded-xl border border-stone-200/70 bg-stone-50/50 p-4 text-sm text-stone-700 dark:border-stone-700/60 dark:bg-stone-950/35 dark:text-stone-300"
                data-testid="onboarding-hint">
                <p class="font-medium text-stone-800 dark:text-stone-200">Подсказка</p>
                <ul class="mt-2 list-disc pl-5 space-y-1">
                  <li>Достаточно 1–2 позиций — остальное добавите из «Базы сигар» позже.</li>
                  <li>Если не хотите сейчас — нажмите «Завершить» или «Пропустить».</li>
                </ul>
              </div>
            </div>
          </div>
        </div>

        <aside class="hidden lg:block">
          <div
            class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/40"
            data-testid="onboarding-sidebar">
            <h3 class="text-sm font-semibold text-stone-900 dark:text-rose-50/95">Что будет дальше</h3>
            <ol class="mt-3 space-y-3 text-sm text-stone-700 dark:text-stone-300">
              <li class="flex gap-3">
                <span
                  class="mt-0.5 flex h-6 w-6 items-center justify-center rounded-lg bg-rose-100/90 text-rose-900 dark:bg-rose-900/40 dark:text-rose-100">
                  1
                </span>
                <span>Хьюмидоры → редактирование, влажность, вместимость.</span>
              </li>
              <li class="flex gap-3">
                <span
                  class="mt-0.5 flex h-6 w-6 items-center justify-center rounded-lg bg-rose-100/90 text-rose-900 dark:bg-rose-900/40 dark:text-rose-100">
                  2
                </span>
                <span>База сигар → поиск и быстрые действия.</span>
              </li>
              <li class="flex gap-3">
                <span
                  class="mt-0.5 flex h-6 w-6 items-center justify-center rounded-lg bg-rose-100/90 text-rose-900 dark:bg-rose-900/40 dark:text-rose-100">
                  3
                </span>
                <span>Обзоры → заметки и история впечатлений.</span>
              </li>
            </ol>
          </div>
        </aside>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, reactive, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import { useToast } from 'primevue/usetoast';
  import { isOfflineQueued } from '@/services/api';
  import InputText from 'primevue/inputtext';
  import Textarea from 'primevue/textarea';
  import InputNumber from 'primevue/inputnumber';
  import Button from 'primevue/button';
  import Message from 'primevue/message';
  import Skeleton from 'primevue/skeleton';
  import Divider from 'primevue/divider';
  import Tag from 'primevue/tag';
  import humidorService from '@/services/humidorService';
  import cigarService from '@/services/cigarService';
  import type { Humidor } from '@/services/humidorService';
  import type { CigarBase, Cigar } from '@/services/cigarService';
  import { strengthOptions } from '@/utils/cigarOptions';

  const router = useRouter();
  const toast = useToast();

  const step = ref<1 | 2>(1);
  const globalError = ref<string | null>(null);

  const step1Loading = ref(false);
  const createdHumidor = ref<Humidor | null>(null);

  const step2Loading = ref(false);
  const step2Error = ref<string | null>(null);
  const cigarBases = ref<CigarBase[]>([]);
  const search = ref('');
  let searchTimeout: ReturnType<typeof setTimeout> | null = null;

  const selectedIds = reactive(new Set<number>());
  const creatingIds = reactive(new Set<number>());

  const humidorForm = reactive<{
    name: string;
    description: string;
    capacity: number;
    humidity: number | null;
  }>({
    name: 'Мой первый хьюмидор',
    description: '',
    capacity: 20,
    humidity: null,
  });

  const humidorErrors = reactive<{ name: string | null; capacity: string | null }>({ name: null, capacity: null });

  function validateHumidor(): boolean {
    humidorErrors.name = null;
    humidorErrors.capacity = null;
    if (!humidorForm.name?.trim()) {
      humidorErrors.name = 'Название обязательно';
      return false;
    }
    if (humidorForm.name.trim().length < 2) {
      humidorErrors.name = 'Минимум 2 символа';
      return false;
    }
    if (!Number.isFinite(humidorForm.capacity) || humidorForm.capacity < 1) {
      humidorErrors.capacity = 'Вместимость должна быть числом от 1';
      return false;
    }
    return true;
  }

  async function createFirstHumidor(): Promise<void> {
    globalError.value = null;
    if (!validateHumidor()) return;

    step1Loading.value = true;
    try {
      const payload = {
        name: humidorForm.name.trim(),
        description: humidorForm.description?.trim() ? humidorForm.description.trim() : null,
        capacity: Math.floor(humidorForm.capacity),
        humidity: humidorForm.humidity == null ? null : Math.floor(humidorForm.humidity),
      };
      const h = await humidorService.createHumidor(payload);
      createdHumidor.value = h;
      toast.add({
        severity: 'success',
        summary: 'Готово',
        detail: 'Хьюмидор создан',
        life: 2500,
      });
      step.value = 2;
      await loadCigarBases();
    } catch (err) {
      if (isOfflineQueued(err)) return;
      if (import.meta.env.DEV) {
        console.error('Failed to create humidor:', err);
      }
      globalError.value = 'Не удалось создать хьюмидор. Попробуйте позже.';
    } finally {
      step1Loading.value = false;
    }
  }

  async function loadCigarBases(): Promise<void> {
    step2Loading.value = true;
    step2Error.value = null;
    try {
      const result = await cigarService.getCigarBasesPaginated({
        page: 1,
        pageSize: 12,
        sortField: 'name',
        sortOrder: 'asc',
        search: search.value?.trim() ? search.value.trim() : undefined,
        excludeBinaryMedia: true,
      });
      cigarBases.value = result.items ?? [];
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Failed to load cigar bases:', err);
      }
      step2Error.value = 'Не удалось загрузить базу сигар. Попробуйте позже.';
    } finally {
      step2Loading.value = false;
    }
  }

  function onSearchChanged(): void {
    if (searchTimeout) clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
      void loadCigarBases();
    }, 300);
  }

  function strengthLabel(value: string): string {
    const opt = strengthOptions.find((o) => o.value === value);
    return opt?.label ?? value;
  }

  function cigarBaseToCreatePayload(base: CigarBase, humidorId: number): Omit<Cigar, 'id' | 'brandName'> {
    return {
      name: base.name,
      brand: base.brand,
      country: base.country ?? null,
      size: base.size ?? null,
      strength: base.strength ?? null,
      price: null,
      rating: null,
      description: base.description ?? null,
      wrapper: base.wrapper ?? null,
      binder: base.binder ?? null,
      filler: base.filler ?? null,
      humidorId,
      images: [],
    };
  }

  async function addBaseToCollection(base: CigarBase): Promise<void> {
    globalError.value = null;
    if (!createdHumidor.value?.id) return;
    if (selectedIds.size >= 2) return;
    if (selectedIds.has(base.id)) return;

    creatingIds.add(base.id);
    try {
      const payload = cigarBaseToCreatePayload(base, createdHumidor.value.id);
      await cigarService.createCigar(payload, null);
      selectedIds.add(base.id);
      toast.add({
        severity: 'success',
        summary: 'Добавлено',
        detail: base.name,
        life: 2000,
      });
    } catch (err) {
      if (isOfflineQueued(err)) return;
      if (import.meta.env.DEV) {
        console.error('Failed to create cigar:', err);
      }
      globalError.value = 'Не удалось добавить сигару в коллекцию. Попробуйте ещё раз.';
    } finally {
      creatingIds.delete(base.id);
    }
  }

  function finishOnboarding(): void {
    localStorage.removeItem('needsOnboarding');
    const hid = createdHumidor.value?.id;
    if (hid != null) {
      void router.push({ name: 'HumidorDetail', params: { id: String(hid) } });
      return;
    }
    void router.push({ name: 'Dashboard' });
  }

  onMounted(() => {
    const needs = localStorage.getItem('needsOnboarding') === '1';
    if (!needs) {
      void router.replace({ name: 'Dashboard' });
      return;
    }
    void loadCigarBases();
  });
</script>

<style scoped>
  .onboarding-root {
    position: relative;
    isolation: isolate;
  }

  .onboarding-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  /*:global(.dark) .onboarding-grain {
    mix-blend-mode: soft-light;
  }*/

  .line-clamp-2 {
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 2;
    overflow: hidden;
  }
</style>
