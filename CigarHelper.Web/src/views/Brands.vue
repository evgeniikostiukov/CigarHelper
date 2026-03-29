<template>
  <section
    class="brands-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-100 via-amber-50/40 to-stone-100 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-amber-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="brands-page"
    aria-labelledby="brands-heading">
    <div class="brands-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto max-w-7xl">
      <header class="flex flex-col gap-4 pb-6 sm:flex-row sm:items-end sm:justify-between sm:pb-8">
        <div class="min-w-0">
          <p
            class="mb-1.5 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-amber-900/65 dark:text-amber-200/55">
            Администрирование
          </p>
          <h1
            id="brands-heading"
            class="text-balance text-3xl font-semibold tracking-tight text-stone-900 dark:text-amber-50/95 sm:text-4xl">
            Бренды сигар
          </h1>
          <p class="mt-1.5 max-w-xl text-pretty text-sm text-stone-600 dark:text-stone-400">
            Справочник брендов, модерация и логотипы — в том же визуальном стиле, что коллекция.
          </p>
        </div>
        <Button
          data-testid="brands-add"
          class="w-full shrink-0 px-5 shadow-md shadow-amber-900/10 dark:shadow-black/40 sm:w-auto sm:min-h-11 min-h-12 touch-manipulation"
          icon="pi pi-plus"
          label="Добавить бренд"
          @click="openCreateDialog" />
      </header>

      <div
        v-if="loading"
        class="min-h-[20rem] space-y-4"
        data-testid="brands-loading"
        aria-busy="true"
        aria-live="polite">
        <Skeleton
          class="h-24 rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          data-testid="brands-skeleton-filters" />
        <Skeleton
          v-for="n in 5"
          :key="n"
          class="rounded-xl border border-stone-200/80 dark:border-stone-700/80"
          height="3rem"
          data-testid="brands-skeleton-row" />
      </div>

      <div
        v-else-if="loadError"
        class="max-w-2xl rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80"
        data-testid="brands-error"
        role="alert">
        <Message severity="error">{{ loadError }}</Message>
        <Button
          data-testid="brands-retry"
          class="mt-4 min-h-12 w-full touch-manipulation sm:w-auto"
          label="Повторить загрузку"
          icon="pi pi-refresh"
          severity="secondary"
          outlined
          @click="loadBrands" />
      </div>

      <div
        v-else-if="brands.length === 0"
        class="mx-auto max-w-xl rounded-2xl border border-dashed border-amber-800/25 bg-white/80 px-5 py-12 text-center dark:border-amber-200/15 dark:bg-stone-900/60"
        data-testid="brands-empty">
        <span
          class="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-2xl bg-amber-100/90 text-amber-900 dark:bg-amber-900/40 dark:text-amber-100"
          aria-hidden="true">
          <i class="pi pi-tag text-2xl" />
        </span>
        <h2 class="mb-2 text-2xl font-semibold text-stone-900 dark:text-amber-50/95">Брендов пока нет</h2>
        <p class="mb-6 text-pretty text-stone-600 dark:text-stone-400">
          Добавьте первый бренд — он появится в базе и в фильтрах каталога.
        </p>
        <Button
          data-testid="brands-empty-add"
          class="min-h-12 px-6 touch-manipulation"
          label="Добавить бренд"
          icon="pi pi-plus"
          @click="openCreateDialog" />
      </div>

      <div
        v-else
        class="brands-panel-enter rounded-2xl border border-stone-200/90 bg-white/95 p-4 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6"
        data-testid="brands-content">
        <div class="mb-6 flex flex-col gap-4 md:flex-row md:items-end">
          <div class="min-w-0 flex-1">
            <label
              for="brands-search"
              class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
              Поиск
            </label>
            <span class="p-input-icon-left flex w-full [&_input]:min-h-11 [&_input]:w-full">
              <i class="pi pi-search text-stone-400" />
              <InputText
                id="brands-search"
                v-model="filters.search"
                data-testid="brands-filter-search"
                class="w-full"
                placeholder="Название бренда…"
                @input="applyFilters" />
            </span>
          </div>
          <div class="flex flex-col gap-4 sm:flex-row sm:flex-wrap">
            <div class="min-w-[min(100%,12rem)] flex-1 sm:flex-initial">
              <label
                for="brands-country"
                class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
                Страна
              </label>
              <Dropdown
                id="brands-country"
                v-model="filters.country"
                data-testid="brands-filter-country"
                class="w-full"
                input-class="min-h-11"
                :options="countryOptions"
                option-label="label"
                option-value="value"
                placeholder="Все страны"
                show-clear
                @update:model-value="applyFilters" />
            </div>
            <div class="min-w-[min(100%,12rem)] flex-1 sm:flex-initial">
              <label
                for="brands-status"
                class="mb-1.5 block text-xs font-medium text-stone-600 dark:text-stone-400">
                Статус
              </label>
              <Dropdown
                id="brands-status"
                v-model="filters.status"
                data-testid="brands-filter-status"
                class="w-full"
                input-class="min-h-11"
                :options="statusOptions"
                option-label="label"
                option-value="value"
                placeholder="Все статусы"
                show-clear
                @update:model-value="applyFilters" />
            </div>
          </div>
        </div>

        <DataTable
          :value="filteredBrands"
          data-testid="brands-table"
          striped-rows
          show-grid-lines
          responsive-layout="scroll"
          paginator
          :rows="10"
          :rows-per-page-options="[10, 25, 50]"
          class="brands-datatable text-sm"
          :pt="{ root: { class: 'border-0' } }">
          <template #empty>
            <div
              class="flex flex-col items-center gap-4 py-10 text-center"
              data-testid="brands-filter-empty">
              <p class="text-stone-600 dark:text-stone-400">Ничего не найдено по текущим фильтрам.</p>
              <Button
                data-testid="brands-filter-reset"
                class="min-h-11 touch-manipulation"
                label="Сбросить фильтры"
                icon="pi pi-filter-slash"
                severity="secondary"
                outlined
                @click="clearFilters" />
            </div>
          </template>

          <Column
            field="name"
            header="Название"
            sortable>
            <template #body="{ data }">
              <div class="flex min-w-0 items-center gap-3">
                <div
                  v-if="data.logoUrl"
                  class="h-9 w-9 shrink-0 overflow-hidden rounded-full bg-stone-100 dark:bg-stone-800">
                  <img
                    :src="data.logoUrl"
                    :alt="data.name"
                    class="h-full w-full object-cover"
                    loading="lazy"
                    decoding="async" />
                </div>
                <div
                  v-else
                  class="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-stone-200/80 dark:bg-stone-700">
                  <i class="pi pi-image text-sm text-stone-500 dark:text-stone-400" />
                </div>
                <div class="min-w-0 truncate font-medium text-stone-900 dark:text-stone-100">{{ data.name }}</div>
              </div>
            </template>
          </Column>

          <Column
            field="country"
            header="Страна"
            sortable>
            <template #body="{ data }">
              <span v-if="data.country">{{ data.country }}</span>
              <span
                v-else
                class="text-stone-400">—</span>
            </template>
          </Column>

          <Column
            field="description"
            header="Описание">
            <template #body="{ data }">
              <span
                v-if="data.description"
                class="line-clamp-2 block max-w-xs"
                :title="data.description">
                {{ data.description }}
              </span>
              <span
                v-else
                class="text-stone-400">—</span>
            </template>
          </Column>

          <Column
            field="isModerated"
            header="Статус"
            sortable>
            <template #body="{ data }">
              <Tag
                v-if="data.isModerated"
                value="Проверен"
                severity="success"
                class="text-xs" />
              <Tag
                v-else
                value="На модерации"
                severity="warn"
                class="text-xs" />
            </template>
          </Column>

          <Column
            field="createdAt"
            header="Создан"
            sortable>
            <template #body="{ data }">
              {{ formatDate(data.createdAt) }}
            </template>
          </Column>

          <Column
            header="Действия"
            :exportable="false"
            style="min-width: 9rem">
            <template #body="{ data }">
              <div class="flex flex-wrap items-center gap-1">
                <Button
                  :data-testid="`brands-view-${data.id}`"
                  class="min-h-11 min-w-11 touch-manipulation"
                  icon="pi pi-eye"
                  text
                  rounded
                  severity="secondary"
                  aria-label="Просмотр"
                  @click="viewBrand(data)" />
                <Button
                  :data-testid="`brands-edit-${data.id}`"
                  class="min-h-11 min-w-11 touch-manipulation"
                  icon="pi pi-pencil"
                  text
                  rounded
                  severity="secondary"
                  aria-label="Редактировать"
                  @click="editBrand(data)" />
                <Button
                  :data-testid="`brands-delete-${data.id}`"
                  class="min-h-11 min-w-11 touch-manipulation"
                  icon="pi pi-trash"
                  text
                  rounded
                  severity="danger"
                  aria-label="Удалить"
                  @click="deleteBrand(data)" />
              </div>
            </template>
          </Column>
        </DataTable>
      </div>
    </div>

    <Dialog
      v-model:visible="showBrandDialog"
      data-testid="brands-dialog-form"
      :header="isEditing ? 'Редактировать бренд' : 'Новый бренд'"
      class="w-[min(100vw-2rem,32rem)] sm:w-[min(90vw,36rem)]"
      modal
      :draggable="false"
      :content-style="{ overflow: 'auto' }"
      @hide="closeBrandDialog">
      <form
        class="flex flex-col gap-4"
        @submit.prevent="handleSubmit">
        <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
          <div class="flex flex-col gap-2">
            <label
              for="brand-name"
              class="text-xs font-medium text-stone-600 dark:text-stone-400">
              Название <span class="text-red-600 dark:text-red-400">*</span>
            </label>
            <InputText
              id="brand-name"
              v-model="brandForm.name"
              class="min-h-11 w-full"
              :class="{ 'p-invalid': errors.name }"
              placeholder="Название бренда"
              required />
            <small
              v-if="errors.name"
              class="text-sm text-red-600 dark:text-red-400">{{ errors.name }}</small>
          </div>
          <div class="flex flex-col gap-2">
            <label
              for="brand-country"
              class="text-xs font-medium text-stone-600 dark:text-stone-400">
              Страна
            </label>
            <InputText
              id="brand-country"
              v-model="brandForm.country"
              class="min-h-11 w-full"
              placeholder="Куба, Доминикана…" />
          </div>
        </div>
        <div class="flex flex-col gap-2">
          <label
            for="brand-description"
            class="text-xs font-medium text-stone-600 dark:text-stone-400">
            Описание
          </label>
          <Textarea
            id="brand-description"
            v-model="brandForm.description"
            class="min-h-[5rem] w-full"
            rows="3"
            auto-resize
            placeholder="История, особенности…" />
        </div>
        <div class="flex flex-col gap-2">
          <label
            for="brand-logo-url"
            class="text-xs font-medium text-stone-600 dark:text-stone-400">
            URL логотипа
          </label>
          <InputText
            id="brand-logo-url"
            v-model="brandForm.logoUrl"
            class="min-h-11 w-full"
            placeholder="https://…" />
        </div>
        <div class="flex min-h-11 items-center gap-3">
          <Checkbox
            id="brand-moderated"
            v-model="brandForm.isModerated"
            input-id="brand-moderated"
            :binary="true" />
          <label
            for="brand-moderated"
            class="cursor-pointer text-sm font-medium text-stone-800 dark:text-stone-200">
            Бренд проверен (модерация)
          </label>
        </div>
      </form>
      <template #footer>
        <div class="flex w-full flex-col-reverse gap-2 sm:flex-row sm:justify-end">
          <Button
            data-testid="brands-dialog-cancel"
            class="min-h-12 w-full touch-manipulation sm:min-h-11 sm:w-auto"
            label="Отмена"
            severity="secondary"
            outlined
            @click="closeBrandDialog" />
          <Button
            data-testid="brands-dialog-submit"
            class="min-h-12 w-full touch-manipulation shadow-md shadow-amber-900/10 dark:shadow-black/40 sm:min-h-11 sm:w-auto"
            :label="isEditing ? 'Сохранить' : 'Добавить'"
            icon="pi pi-check"
            :loading="submitting"
            @click="handleSubmit" />
        </div>
      </template>
    </Dialog>

    <Dialog
      v-model:visible="showDetailDialog"
      data-testid="brands-dialog-detail"
      :header="selectedBrand?.name || 'Бренд'"
      class="w-[min(100vw-2rem,28rem)]"
      modal
      :draggable="false">
      <div
        v-if="selectedBrand"
        class="flex flex-col gap-6">
        <div class="flex items-center gap-4">
          <div
            v-if="selectedBrand.logoUrl"
            class="h-16 w-16 shrink-0 overflow-hidden rounded-full bg-stone-100 dark:bg-stone-800">
            <img
              :src="selectedBrand.logoUrl"
              :alt="selectedBrand.name"
              class="h-full w-full object-cover"
              loading="lazy"
              decoding="async" />
          </div>
          <div
            v-else
            class="flex h-16 w-16 shrink-0 items-center justify-center rounded-full bg-stone-200/80 dark:bg-stone-700">
            <i class="pi pi-image text-xl text-stone-500" />
          </div>
          <div class="min-w-0">
            <h3 class="text-lg font-semibold text-stone-900 dark:text-amber-50/95">{{ selectedBrand.name }}</h3>
            <p
              v-if="selectedBrand.country"
              class="text-sm text-stone-600 dark:text-stone-400">
              {{ selectedBrand.country }}
            </p>
          </div>
        </div>
        <div v-if="selectedBrand.description">
          <h4 class="mb-2 text-xs font-semibold uppercase tracking-wide text-stone-500 dark:text-stone-400">
            Описание
          </h4>
          <p class="text-pretty text-sm leading-relaxed text-stone-700 dark:text-stone-300">
            {{ selectedBrand.description }}
          </p>
        </div>
        <div class="grid grid-cols-1 gap-3 text-sm sm:grid-cols-2">
          <div class="flex flex-wrap items-center gap-2">
            <span class="font-medium text-stone-600 dark:text-stone-400">Статус:</span>
            <Tag
              v-if="selectedBrand.isModerated"
              value="Проверен"
              severity="success" />
            <Tag
              v-else
              value="На модерации"
              severity="warn" />
          </div>
          <div>
            <span class="font-medium text-stone-600 dark:text-stone-400">Создан:</span>
            <span class="ml-1 text-stone-800 dark:text-stone-200">{{ formatDate(selectedBrand.createdAt) }}</span>
          </div>
        </div>
      </div>
      <template #footer>
        <div class="flex w-full flex-col-reverse gap-2 sm:flex-row sm:justify-end">
          <Button
            data-testid="brands-detail-close"
            class="min-h-12 w-full touch-manipulation sm:min-h-11 sm:w-auto"
            label="Закрыть"
            severity="secondary"
            outlined
            @click="showDetailDialog = false" />
          <Button
            v-if="selectedBrand"
            data-testid="brands-detail-edit"
            class="min-h-12 w-full touch-manipulation sm:min-h-11 sm:w-auto"
            label="Редактировать"
            icon="pi pi-pencil"
            @click="editBrand(selectedBrand)" />
        </div>
      </template>
    </Dialog>

    <ConfirmDialog />
  </section>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { useToast } from 'primevue/usetoast';
  import { useConfirm } from 'primevue/useconfirm';
  import cigarService from '@/services/cigarService';
  import type { Brand } from '@/services/cigarService';
  import DataTable from 'primevue/datatable';
  import Column from 'primevue/column';
  import InputText from 'primevue/inputtext';
  import Dropdown from 'primevue/dropdown';
  import Button from 'primevue/button';
  import Dialog from 'primevue/dialog';
  import Textarea from 'primevue/textarea';
  import Checkbox from 'primevue/checkbox';
  import Tag from 'primevue/tag';
  import ConfirmDialog from 'primevue/confirmdialog';

  interface BrandForm {
    name: string;
    country: string;
    description: string;
    logoUrl: string;
    isModerated: boolean;
  }

  interface Filters {
    search: string;
    country: string | null;
    status: boolean | null;
  }

  interface SelectOption {
    label: string;
    value: string | boolean;
  }

  interface FormErrors {
    name?: string;
  }

  const toast = useToast();
  const confirm = useConfirm();

  const loading = ref(true);
  const loadError = ref<string | null>(null);
  const submitting = ref(false);
  const brands = ref<Brand[]>([]);
  const filteredBrands = ref<Brand[]>([]);
  const showBrandDialog = ref(false);
  const showDetailDialog = ref(false);
  const selectedBrand = ref<Brand | null>(null);
  const isEditing = ref(false);

  const filters = ref<Filters>({
    search: '',
    country: null,
    status: null,
  });

  const brandForm = ref<BrandForm>({
    name: '',
    country: '',
    description: '',
    logoUrl: '',
    isModerated: false,
  });

  const errors = ref<FormErrors>({});

  const statusOptions: SelectOption[] = [
    { label: 'Проверенные', value: true },
    { label: 'На модерации', value: false },
  ];

  const countryOptions = computed<SelectOption[]>(() => {
    const countries = Array.from(new Set(brands.value.map((b) => b.country).filter(Boolean)));
    return countries.map((c) => ({
      label: c as string,
      value: c as string,
    }));
  });

  function applyFilters(): void {
    let result = [...brands.value];

    if (filters.value.search.trim()) {
      const q = filters.value.search.toLowerCase();
      result = result.filter((b) => b.name.toLowerCase().includes(q));
    }
    if (filters.value.country) {
      result = result.filter((b) => b.country === filters.value.country);
    }
    if (filters.value.status !== null && filters.value.status !== undefined) {
      result = result.filter((b) => b.isModerated === filters.value.status);
    }

    filteredBrands.value = result;
  }

  function clearFilters(): void {
    filters.value = { search: '', country: null, status: null };
    applyFilters();
  }

  async function loadBrands(): Promise<void> {
    loading.value = true;
    loadError.value = null;
    try {
      brands.value = await cigarService.getBrands();
      applyFilters();
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка загрузки брендов:', err);
      }
      loadError.value = 'Не удалось загрузить бренды. Попробуйте позже.';
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось загрузить список брендов.',
        life: 4000,
      });
    } finally {
      loading.value = false;
    }
  }

  function formatDate(dateString: string | undefined): string {
    if (!dateString) return '—';
    return new Date(dateString).toLocaleDateString('ru-RU');
  }

  function validateForm(): boolean {
    errors.value = {};
    if (!brandForm.value.name?.trim()) {
      errors.value.name = 'Укажите название бренда';
    }
    return Object.keys(errors.value).length === 0;
  }

  function openCreateDialog(): void {
    isEditing.value = false;
    selectedBrand.value = null;
    brandForm.value = {
      name: '',
      country: '',
      description: '',
      logoUrl: '',
      isModerated: false,
    };
    errors.value = {};
    showBrandDialog.value = true;
  }

  async function handleSubmit(): Promise<void> {
    if (!validateForm()) return;

    submitting.value = true;
    try {
      if (isEditing.value && selectedBrand.value?.id) {
        await cigarService.updateBrand(selectedBrand.value.id, brandForm.value);
        toast.add({
          severity: 'success',
          summary: 'Готово',
          detail: 'Бренд обновлён',
          life: 3000,
        });
      } else {
        await cigarService.createBrand(brandForm.value);
        toast.add({
          severity: 'success',
          summary: 'Готово',
          detail: 'Бренд добавлен',
          life: 3000,
        });
      }
      closeBrandDialog();
      await loadBrands();
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка сохранения бренда:', err);
      }
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: isEditing.value ? 'Не удалось обновить бренд' : 'Не удалось добавить бренд',
        life: 4000,
      });
    } finally {
      submitting.value = false;
    }
  }

  function viewBrand(brand: Brand): void {
    selectedBrand.value = brand;
    showDetailDialog.value = true;
  }

  function editBrand(brand: Brand): void {
    selectedBrand.value = brand;
    isEditing.value = true;
    brandForm.value = {
      name: brand.name,
      country: brand.country || '',
      description: brand.description || '',
      logoUrl: brand.logoUrl || '',
      isModerated: brand.isModerated,
    };
    showBrandDialog.value = true;
    showDetailDialog.value = false;
  }

  function deleteBrand(brand: Brand): void {
    confirm.require({
      message: `Удалить бренд «${brand.name}»? Действие необратимо.`,
      header: 'Подтверждение удаления',
      icon: 'pi pi-exclamation-triangle',
      rejectClass: 'p-button-secondary p-button-outlined',
      acceptClass: 'p-button-danger',
      rejectLabel: 'Отмена',
      acceptLabel: 'Удалить',
      accept: async () => {
        if (!brand.id) {
          toast.add({ severity: 'error', summary: 'Ошибка', detail: 'Не указан ID бренда', life: 3000 });
          return;
        }
        try {
          await cigarService.deleteBrand(brand.id);
          toast.add({ severity: 'success', summary: 'Готово', detail: 'Бренд удалён', life: 3000 });
          await loadBrands();
        } catch (err) {
          if (import.meta.env.DEV) {
            console.error('Ошибка удаления бренда:', err);
          }
          toast.add({ severity: 'error', summary: 'Ошибка', detail: 'Не удалось удалить бренд', life: 4000 });
        }
      },
    });
  }

  function closeBrandDialog(): void {
    showBrandDialog.value = false;
    isEditing.value = false;
    selectedBrand.value = null;
    brandForm.value = {
      name: '',
      country: '',
      description: '',
      logoUrl: '',
      isModerated: false,
    };
    errors.value = {};
  }

  onMounted(() => {
    void loadBrands();
  });
</script>

<style scoped>
  .brands-root {
    position: relative;
    isolation: isolate;
  }

  .brands-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  :global(.dark) .brands-grain {
    mix-blend-mode: soft-light;
  }

  .line-clamp-2 {
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 2;
    overflow: hidden;
  }

  .brands-panel-enter {
    animation: brands-panel-in 0.45s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes brands-panel-in {
    from {
      opacity: 0;
      transform: translateY(8px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  @media (prefers-reduced-motion: reduce) {
    .brands-panel-enter {
      animation: none;
    }
  }
</style>
