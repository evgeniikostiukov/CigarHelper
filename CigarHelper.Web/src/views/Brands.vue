<template>
  <div class="p-4 max-w-7xl mx-auto">
    <div class="mb-6 flex justify-between items-center">
      <div>
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Бренды сигар</h1>
        <p class="text-gray-600 dark:text-gray-400 mt-2">Управление брендами сигар в базе данных</p>
      </div>
      <Button
        label="Добавить бренд"
        icon="pi pi-plus"
        class="p-button-primary"
        @click="showBrandDialog = true" />
    </div>

    <Card class="shadow-sm">
      <template #content>
        <!-- Фильтры и поиск -->
        <div class="mb-6 flex flex-col md:flex-row gap-4">
          <div class="flex-1">
            <span class="p-input-icon-left w-full">
              <i class="pi pi-search" />
              <InputText
                v-model="filters.search"
                placeholder="Поиск по названию бренда..."
                class="w-full"
                @input="onSearch" />
            </span>
          </div>
          <div class="flex gap-2">
            <Dropdown
              v-model="filters.country"
              :options="countryOptions"
              optionLabel="label"
              optionValue="value"
              placeholder="Все страны"
              class="min-w-[200px]"
              :showClear="true"
              @change="onFilterChange" />
            <Dropdown
              v-model="filters.status"
              :options="statusOptions"
              optionLabel="label"
              optionValue="value"
              placeholder="Все статусы"
              class="min-w-[200px]"
              :showClear="true"
              @change="onFilterChange" />
          </div>
        </div>

        <!-- Таблица -->
        <DataTable
          :value="filteredBrands"
          :loading="loading"
          paginator
          :rows="10"
          :totalRecords="filteredBrands.length"
          :sortField="sortField"
          :sortOrder="sortOrder"
          @sort="onSort"
          stripedRows
          showGridlines
          responsiveLayout="scroll"
          class="p-datatable-sm">
          <Column
            field="name"
            header="Название"
            sortable>
            <template #body="{ data }">
              <div class="flex items-center gap-3">
                <div
                  v-if="data.logoUrl"
                  class="w-8 h-8 rounded-full overflow-hidden bg-gray-100">
                  <img
                    :src="data.logoUrl"
                    :alt="data.name"
                    class="w-full h-full object-cover" />
                </div>
                <div
                  v-else
                  class="w-8 h-8 rounded-full bg-gray-200 flex items-center justify-center">
                  <i class="pi pi-image text-gray-400 text-sm"></i>
                </div>
                <div class="font-semibold">{{ data.name }}</div>
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
                class="text-gray-400"
                >-</span
              >
            </template>
          </Column>

          <Column
            field="description"
            header="Описание">
            <template #body="{ data }">
              <span
                v-if="data.description"
                class="truncate max-w-xs block"
                :title="data.description">
                {{ data.description }}
              </span>
              <span
                v-else
                class="text-gray-400"
                >-</span
              >
            </template>
          </Column>

          <Column
            field="isModerated"
            header="Статус"
            sortable>
            <template #body="{ data }">
              <span
                v-if="data.isModerated"
                class="px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
                Проверен
              </span>
              <span
                v-else
                class="px-2 py-1 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
                На модерации
              </span>
            </template>
          </Column>

          <Column
            field="createdAt"
            header="Дата создания"
            sortable>
            <template #body="{ data }">
              {{ formatDate(data.createdAt) }}
            </template>
          </Column>

          <Column
            header="Действия"
            :exportable="false"
            style="min-width: 8rem">
            <template #body="{ data }">
              <Button
                icon="pi pi-eye"
                class="p-button-rounded p-button-text p-button-sm"
                @click="viewBrand(data)"
                v-tooltip.top="'Просмотр деталей'" />
              <Button
                icon="pi pi-pencil"
                class="p-button-rounded p-button-text p-button-sm p-button-warning"
                @click="editBrand(data)"
                v-tooltip.top="'Редактировать'" />
              <Button
                icon="pi pi-trash"
                class="p-button-rounded p-button-text p-button-sm p-button-danger"
                @click="deleteBrand(data)"
                v-tooltip.top="'Удалить'" />
            </template>
          </Column>
        </DataTable>
      </template>
    </Card>

    <!-- Диалог добавления/редактирования бренда -->
    <Dialog
      v-model:visible="showBrandDialog"
      :header="isEditing ? 'Редактировать бренд' : 'Добавить новый бренд'"
      :style="{ width: '50vw' }"
      :modal="true"
      :closable="true">
      <form
        @submit.prevent="handleSubmit"
        class="space-y-4">
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <div class="space-y-2">
            <label
              for="name"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Название бренда *
            </label>
            <InputText
              id="name"
              v-model="brandForm.name"
              class="w-full"
              :class="{ 'p-invalid': errors.name }"
              placeholder="Введите название бренда"
              required />
            <small
              v-if="errors.name"
              class="text-red-500"
              >{{ errors.name }}</small
            >
          </div>

          <div class="space-y-2">
            <label
              for="country"
              class="block text-sm font-medium text-gray-700 dark:text-gray-300">
              Страна
            </label>
            <InputText
              id="country"
              v-model="brandForm.country"
              class="w-full"
              placeholder="Например: Куба, Доминикана" />
          </div>
        </div>

        <div class="space-y-2">
          <label
            for="description"
            class="block text-sm font-medium text-gray-700 dark:text-gray-300">
            Описание
          </label>
          <Textarea
            id="description"
            v-model="brandForm.description"
            class="w-full"
            rows="3"
            placeholder="Описание бренда, история, особенности..." />
        </div>

        <div class="space-y-2">
          <label
            for="logoUrl"
            class="block text-sm font-medium text-gray-700 dark:text-gray-300">
            URL логотипа
          </label>
          <InputText
            id="logoUrl"
            v-model="brandForm.logoUrl"
            class="w-full"
            placeholder="https://example.com/logo.png" />
        </div>

        <div class="space-y-2">
          <label class="flex items-center">
            <Checkbox
              v-model="brandForm.isModerated"
              :binary="true"
              class="mr-2" />
            <span class="text-sm font-medium text-gray-700 dark:text-gray-300">
              Бренд проверен (прошел модерацию)
            </span>
          </label>
        </div>
      </form>

      <template #footer>
        <Button
          :label="isEditing ? 'Сохранить изменения' : 'Добавить бренд'"
          icon="pi pi-check"
          class="p-button-primary"
          :loading="submitting"
          @click="handleSubmit" />
        <Button
          label="Отмена"
          icon="pi pi-times"
          class="p-button-secondary"
          @click="closeBrandDialog" />
      </template>
    </Dialog>

    <!-- Диалог детального просмотра -->
    <Dialog
      v-model:visible="showDetailDialog"
      :header="selectedBrand?.name || 'Детали бренда'"
      :style="{ width: '40vw' }"
      :modal="true"
      :closable="true">
      <div
        v-if="selectedBrand"
        class="space-y-6">
        <div class="flex items-center gap-4">
          <div
            v-if="selectedBrand.logoUrl"
            class="w-16 h-16 rounded-full overflow-hidden bg-gray-100">
            <img
              :src="selectedBrand.logoUrl"
              :alt="selectedBrand.name"
              class="w-full h-full object-cover" />
          </div>
          <div
            v-else
            class="w-16 h-16 rounded-full bg-gray-200 flex items-center justify-center">
            <i class="pi pi-image text-gray-400 text-xl"></i>
          </div>
          <div>
            <h3 class="text-xl font-bold">{{ selectedBrand.name }}</h3>
            <p
              v-if="selectedBrand.country"
              class="text-gray-600">
              {{ selectedBrand.country }}
            </p>
          </div>
        </div>

        <div v-if="selectedBrand.description">
          <h4 class="font-semibold mb-2">Описание</h4>
          <p class="text-gray-700 dark:text-gray-300">{{ selectedBrand.description }}</p>
        </div>

        <div class="grid grid-cols-2 gap-4 text-sm">
          <div>
            <span class="font-medium">Статус:</span>
            <span
              v-if="selectedBrand.isModerated"
              class="ml-2 px-2 py-1 rounded-full text-xs font-medium bg-green-100 text-green-800">
              Проверен
            </span>
            <span
              v-else
              class="ml-2 px-2 py-1 rounded-full text-xs font-medium bg-yellow-100 text-yellow-800">
              На модерации
            </span>
          </div>
          <div>
            <span class="font-medium">Дата создания:</span>
            <span class="ml-2">{{ formatDate(selectedBrand.createdAt) }}</span>
          </div>
        </div>
      </div>

      <template #footer>
        <Button
          v-if="selectedBrand"
          label="Редактировать"
          icon="pi pi-pencil"
          class="p-button-warning"
          @click="editBrand(selectedBrand)" />
        <Button
          label="Закрыть"
          icon="pi pi-times"
          class="p-button-secondary"
          @click="showDetailDialog = false" />
      </template>
    </Dialog>

    <!-- Диалог подтверждения удаления -->
    <ConfirmDialog />
  </div>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted, reactive, watch } from 'vue';
  import { useRouter } from 'vue-router';
  import { useToast } from 'primevue/usetoast';
  import { useConfirm } from 'primevue/useconfirm';
  import cigarService from '@/services/cigarService';
  import type { Brand } from '@/services/cigarService';
  import type { DataTableSortEvent } from 'primevue/datatable';
  import Card from 'primevue/card';
  import DataTable from 'primevue/datatable';
  import Column from 'primevue/column';
  import InputText from 'primevue/inputtext';
  import Dropdown from 'primevue/dropdown';
  import Button from 'primevue/button';
  import Dialog from 'primevue/dialog';
  import Textarea from 'primevue/textarea';
  import Checkbox from 'primevue/checkbox';
  import ConfirmDialog from 'primevue/confirmdialog';
  // import { FilterMatchMode } from 'primevue/api'; // Удаляем этот импорт

  // --- Interfaces ---

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

  // Composables
  const router = useRouter();
  const toast = useToast();
  const confirm = useConfirm();

  // Reactive data
  const loading = ref(true);
  const submitting = ref<boolean>(false);
  const brands = ref<Brand[]>([]);
  const showBrandDialog = ref<boolean>(false);
  const showDetailDialog = ref<boolean>(false);
  const selectedBrand = ref<Brand | null>(null);
  const isEditing = ref<boolean>(false);

  const filters = ref<Filters>({
    search: '',
    country: null,
    status: null,
  });

  const sortField = ref<string>('name');
  const sortOrder = ref<1 | -1>(1);

  const brandForm = ref<BrandForm>({
    name: '',
    country: '',
    description: '',
    logoUrl: '',
    isModerated: false,
  });

  const errors = ref<FormErrors>({});

  const filteredBrands = ref<Brand[]>([]);

  // Options
  const statusOptions: SelectOption[] = [
    { label: 'Проверенные', value: true },
    { label: 'На модерации', value: false },
  ];

  // Computed
  const countryOptions = computed<SelectOption[]>(() => {
    const countries = Array.from(new Set(brands.value.map((brand) => brand.country).filter(Boolean)));
    return countries.map((country) => ({
      label: country as string,
      value: country as string,
    }));
  });

  // Methods
  async function loadBrands() {
    loading.value = true;
    try {
      brands.value = await cigarService.getBrands();
      applyFiltersAndSort(); // Применяем фильтры после загрузки
    } catch (error) {
      console.error('Failed to load brands:', error);
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to load brands.',
        life: 3000,
      });
    } finally {
      loading.value = false;
    }
  }

  function applyFiltersAndSort() {
    let result = [...brands.value];

    // Фильтрация
    if (filters.value.search) {
      const searchTerm = filters.value.search.toLowerCase();
      result = result.filter((brand) => brand.name.toLowerCase().includes(searchTerm));
    }
    if (filters.value.country) {
      result = result.filter((brand) => brand.country === filters.value.country);
    }
    if (filters.value.status !== null) {
      result = result.filter((brand) => brand.isModerated === filters.value.status);
    }

    // Сортировка
    if (sortField.value) {
      result.sort((a, b) => {
        // ... логика сортировки ...
        return 0;
      });
    }

    filteredBrands.value = result;
  }

  watch(filters, applyFiltersAndSort, { deep: true });

  function formatDate(dateString: string | undefined): string {
    if (!dateString) return '-';
    return new Date(dateString).toLocaleDateString('ru-RU');
  }

  function validateForm(): boolean {
    errors.value = {};

    if (!brandForm.value.name?.trim()) {
      errors.value.name = 'Название бренда обязательно для заполнения';
    }

    return Object.keys(errors.value).length === 0;
  }

  async function handleSubmit(): Promise<void> {
    if (!validateForm()) return;

    try {
      submitting.value = true;

      if (isEditing.value && selectedBrand.value?.id) {
        await cigarService.updateBrand(selectedBrand.value.id, brandForm.value);
        toast.add({
          severity: 'success',
          summary: 'Успешно',
          detail: 'Бренд успешно обновлен',
          life: 3000,
        });
      } else {
        await cigarService.createBrand(brandForm.value);
        toast.add({
          severity: 'success',
          summary: 'Успешно',
          detail: 'Бренд успешно добавлен',
          life: 3000,
        });
      }

      closeBrandDialog();
      await loadBrands();
    } catch (error) {
      console.error('Ошибка при сохранении бренда:', error);
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: isEditing.value ? 'Не удалось обновить бренд' : 'Не удалось добавить бренд',
        life: 3000,
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
      message: `Вы уверены, что хотите удалить бренд "${brand.name}"?`,
      header: 'Подтверждение удаления',
      icon: 'pi pi-exclamation-triangle',
      accept: async () => {
        try {
          if (brand.id) {
            await cigarService.deleteBrand(brand.id);
            toast.add({
              severity: 'success',
              summary: 'Успешно',
              detail: 'Бренд успешно удален',
              life: 3000,
            });
            await loadBrands();
          }
        } catch (error) {
          console.error('Ошибка при удалении бренда:', error);
          toast.add({
            severity: 'error',
            summary: 'Ошибка',
            detail: 'Не удалось удалить бренд',
            life: 3000,
          });
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

  function onSearch(): void {
    // Поиск происходит автоматически через computed
  }

  function onFilterChange(): void {
    // Фильтрация происходит автоматически через computed
  }

  function onSort(event: DataTableSortEvent): void {
    if (event.sortField && event.sortOrder) {
      sortField.value = event.sortField as string;
      sortOrder.value = event.sortOrder as 1 | -1;
    }
  }

  // Lifecycle
  onMounted(loadBrands);
</script>
