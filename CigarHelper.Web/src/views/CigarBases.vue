<template>
  <div class="p-4 max-w-7xl mx-auto">
    <div class="mb-6 flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold text-gray-900 dark:text-white">База сигар</h1>
        <p class="text-gray-600 dark:text-gray-400 mt-2">Просмотр всех доступных сигар в базе данных</p>
      </div>
      <Button
        label="Добавить новую сигару"
        icon="pi pi-plus"
        class="p-button-success"
        @click="createNewCigar" />
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
                placeholder="Поиск по названию или бренду..."
                class="w-full"
                @input="onSearch" />
            </span>
          </div>
          <div class="flex gap-2">
            <Dropdown
              v-model="filters.brand"
              :options="brandOptions"
              optionLabel="label"
              optionValue="value"
              placeholder="Все бренды"
              class="min-w-[200px]"
              :showClear="true"
              @change="onFilterChange" />
            <Dropdown
              v-model="filters.strength"
              :options="strengthOptions"
              optionLabel="label"
              optionValue="value"
              placeholder="Все крепости"
              class="min-w-[200px]"
              :showClear="true"
              @change="onFilterChange" />
          </div>
        </div>

        <!-- Десктопная таблица (скрыта на мобильных) -->
        <div class="hidden lg:block">
          <DataTable
            :value="cigars"
            :loading="loading"
            :paginator="true"
            :rows="pagination.rows"
            :totalRecords="pagination.totalRecords"
            :lazy="true"
            :rowsPerPageOptions="[10, 20, 50, 100]"
            :sortField="sortField"
            :sortOrder="sortOrder"
            @page="onPage"
            @sort="onSort"
            stripedRows
            showGridlines
            responsiveLayout="scroll"
            class="p-datatable-sm"
            :virtualScrollerOptions="{ itemSize: 60 }"
            virtualScrollerHeight="600px">
            <Column
              header="Логотип"
              style="width: 60px">
              <template #body="{ data }">
                <div class="w-10 h-10 rounded-full overflow-hidden bg-gray-100 flex items-center justify-center">
                  <img
                    v-if="getBrandLogoSrc(data)"
                    :src="getBrandLogoSrc(data)"
                    :alt="data.brandName"
                    class="w-full h-full object-cover"
                    loading="lazy" />
                  <i
                    v-else
                    class="pi pi-image text-gray-400 text-lg"></i>
                </div>
              </template>
            </Column>
            <Column
              field="name"
              header="Название"
              sortable>
              <template #body="{ data }">
                <div class="font-semibold">{{ data.name }}</div>
              </template>
            </Column>

            <Column
              field="brand.name"
              header="Бренд"
              sortable>
              <template #body="{ data }">
                <div class="flex items-center gap-2">
                  <span class="font-medium">{{ data.brand.name }}</span>
                  <span
                    v-if="data.country"
                    class="text-xs text-gray-500">
                    ({{ data.country }})
                  </span>
                </div>
              </template>
            </Column>

            <Column
              field="size"
              header="Размер"
              sortable>
              <template #body="{ data }">
                <span v-if="data.size">{{ data.size }}</span>
                <span
                  v-else
                  class="text-gray-400"
                  >-</span
                >
              </template>
            </Column>

            <Column
              field="strength"
              header="Крепость"
              sortable>
              <template #body="{ data }">
                <span
                  v-if="data.strength"
                  class="px-2 py-1 rounded-full text-xs font-medium"
                  :class="getStrengthBadgeClass(data.strength)">
                  {{ getStrengthLabel(data.strength) }}
                </span>
                <span
                  v-else
                  class="text-gray-400"
                  >-</span
                >
              </template>
            </Column>

            <Column
              field="wrapper"
              header="Покровный лист">
              <template #body="{ data }">
                <span v-if="data.wrapper">{{ data.wrapper }}</span>
                <span
                  v-else
                  class="text-gray-400"
                  >-</span
                >
              </template>
            </Column>

            <Column
              field="binder"
              header="Связующий лист">
              <template #body="{ data }">
                <span v-if="data.binder">{{ data.binder }}</span>
                <span
                  v-else
                  class="text-gray-400"
                  >-</span
                >
              </template>
            </Column>

            <Column
              field="filler"
              header="Наполнитель">
              <template #body="{ data }">
                <span v-if="data.filler">{{ data.filler }}</span>
                <span
                  v-else
                  class="text-gray-400"
                  >-</span
                >
              </template>
            </Column>

            <Column
              header="Действия"
              :exportable="false"
              style="min-width: 12rem">
              <template #body="{ data }">
                <Button
                  icon="pi pi-eye"
                  class="p-button-rounded p-button-text p-button-sm"
                  @click="viewCigar(data)"
                  v-tooltip.top="'Просмотр деталей'" />
                <Button
                  icon="pi pi-pencil"
                  class="p-button-rounded p-button-text p-button-sm p-button-warning"
                  @click="writeReview(data)"
                  v-tooltip.top="'Написать отзыв'" />
                <Button
                  icon="pi pi-plus"
                  class="p-button-rounded p-button-text p-button-sm p-button-success"
                  @click="addToCollection(data)"
                  v-tooltip.top="'Добавить в коллекцию'" />
                <Button
                  icon="pi pi-copy"
                  class="p-button-rounded p-button-text p-button-sm p-button-info"
                  @click="createSimilarCigar(data)"
                  v-tooltip.top="'Создать похожую'" />
              </template>
            </Column>
          </DataTable>
        </div>

        <!-- Мобильные карточки (показаны только на мобильных) -->
        <div class="lg:hidden">
          <!-- Пагинация для мобильных -->
          <div
            v-if="!loading"
            class="mb-4 flex justify-between items-center">
            <span class="text-sm text-gray-600">
              Показано {{ pagination.first + 1 }}-{{
                Math.min(pagination.first + pagination.rows, pagination.totalRecords)
              }}
              из {{ pagination.totalRecords }}
            </span>
            <div class="flex gap-2">
              <Button
                icon="pi pi-chevron-left"
                class="p-button-rounded p-button-text p-button-sm"
                :disabled="pagination.first === 0"
                @click="previousPage" />
              <Button
                icon="pi pi-chevron-right"
                class="p-button-rounded p-button-text p-button-sm"
                :disabled="pagination.first + pagination.rows >= pagination.totalRecords"
                @click="nextPage" />
            </div>
          </div>

          <!-- Карточки сигар -->
          <div
            v-if="loading"
            class="flex justify-center py-8">
            <ProgressSpinner />
          </div>

          <div
            v-else
            class="space-y-4">
            <div
              v-for="cigar in cigars"
              :key="cigar.id"
              class="bg-white dark:bg-gray-800 rounded-lg shadow-sm border border-gray-200 dark:border-gray-700 p-4">
              <div class="flex gap-4">
                <!-- Изображение сигары -->
                <div class="flex-shrink-0">
                  <div class="w-20 h-20 rounded-lg overflow-hidden bg-gray-100 flex items-center justify-center">
                    <img
                      v-if="cigar.images && cigar.images.length > 0 && cigar.images[0].imageData"
                      :src="`data:image/jpeg;base64,${arrayBufferToBase64(cigar.images[0].imageData)}`"
                      :alt="cigar.name"
                      class="w-full h-full object-cover"
                      loading="lazy"
                      @error="handleImageError" />
                    <i
                      v-else
                      class="pi pi-image text-gray-400 text-2xl"></i>
                  </div>
                </div>

                <!-- Основная информация -->
                <div class="flex-1 min-w-0">
                  <div class="flex items-start justify-between mb-2">
                    <div class="flex-1 min-w-0">
                      <h3 class="font-semibold text-gray-900 dark:text-white truncate">
                        {{ cigar.name }}
                      </h3>
                      <div class="flex items-center gap-2 mt-1">
                        <img
                          v-if="getBrandLogoSrc(cigar)"
                          :src="getBrandLogoSrc(cigar)"
                          :alt="cigar.brand.name"
                          class="w-4 h-4 object-contain rounded"
                          loading="lazy" />
                        <span class="text-sm font-medium text-gray-700 dark:text-gray-300">
                          {{ cigar.brand.name }}
                        </span>
                        <span
                          v-if="cigar.country"
                          class="text-xs text-gray-500">
                          ({{ cigar.country }})
                        </span>
                      </div>
                    </div>
                  </div>

                  <!-- Ключевые характеристики -->
                  <div class="flex flex-wrap gap-2 mb-3">
                    <span
                      v-if="cigar.size"
                      class="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded-full">
                      {{ cigar.size }}
                    </span>
                    <span
                      v-if="cigar.strength"
                      class="text-xs px-2 py-1 rounded-full font-medium"
                      :class="getStrengthBadgeClass(cigar.strength)">
                      {{ getStrengthLabel(cigar.strength) }}
                    </span>
                  </div>

                  <!-- Действия -->
                  <div class="flex gap-2">
                    <Button
                      icon="pi pi-eye"
                      class="p-button-rounded p-button-text p-button-sm"
                      @click="viewCigar(cigar)"
                      v-tooltip.top="'Просмотр деталей'" />
                    <Button
                      icon="pi pi-pencil"
                      class="p-button-rounded p-button-text p-button-sm p-button-warning"
                      @click="writeReview(cigar)"
                      v-tooltip.top="'Написать отзыв'" />
                    <Button
                      icon="pi pi-plus"
                      class="p-button-rounded p-button-text p-button-sm p-button-success"
                      @click="addToCollection(cigar)"
                      v-tooltip.top="'Добавить в коллекцию'" />
                    <Button
                      icon="pi pi-copy"
                      class="p-button-rounded p-button-text p-button-sm p-button-info"
                      @click="createSimilarCigar(cigar)"
                      v-tooltip.top="'Создать похожую'" />
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- Пагинация внизу для мобильных -->
          <div
            v-if="!loading && cigars.length > 0"
            class="mt-6 flex justify-center">
            <Paginator
              :first="pagination.first"
              :rows="pagination.rows"
              :totalRecords="pagination.totalRecords"
              :rowsPerPageOptions="[10, 20, 50]"
              @page="onPage"
              template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown" />
          </div>
        </div>
      </template>
    </Card>

    <!-- Диалог детального просмотра -->
    <CigarDetailDialog
      v-model:visible="showDetailDialog"
      :cigar="selectedCigar"
      @writeReview="writeReview"
      @addToCollection="addToCollection"
      @createSimilarCigar="createSimilarCigar"
      @editBaseCigar="editBaseCigar" />

    <!-- Диалог редактирования базовой сигары -->
    <CigarBaseEditDialog
      v-model:visible="showEditDialog"
      :cigar="editingCigar"
      @saved="onCigarSaved" />
  </div>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted, watch } from 'vue';
  import { useRouter } from 'vue-router';
  import { useToast } from 'primevue/usetoast';
  import cigarService from '@/services/cigarService';
  import type { CigarBase, PaginatedResult, Brand } from '@/services/cigarService';
  import type { DataTablePageEvent, DataTableSortEvent } from 'primevue/datatable';
  import Card from 'primevue/card';
  import DataTable from 'primevue/datatable';
  import Column from 'primevue/column';
  import InputText from 'primevue/inputtext';
  import Dropdown from 'primevue/dropdown';
  import Button from 'primevue/button';
  import ProgressSpinner from 'primevue/progressspinner';
  import Paginator, { type PageState } from 'primevue/paginator';
  import CigarDetailDialog from '../components/CigarDetailDialog.vue';
  import CigarBaseEditDialog from '../components/CigarBaseEditDialog.vue';
  import { strengthOptions } from '@/utils/cigarOptions';
  import { arrayBufferToBase64 } from '@/utils/imageUtils';

  interface Filters {
    search: string;
    brand: number | null;
    strength: string | null;
  }

  interface Pagination {
    first: number;
    rows: number;
    totalRecords: number;
  }

  interface SelectOption {
    label: string;
    value: string | number;
  }

  // Composables
  const router = useRouter();
  const toast = useToast();

  // Reactive data
  const loading = ref<boolean>(false);
  const cigars = ref<CigarBase[]>([]);
  const brands = ref<Brand[]>([]);
  const showDetailDialog = ref<boolean>(false);
  const selectedCigar = ref<CigarBase>();
  const showEditDialog = ref<boolean>(false);
  const editingCigar = ref<CigarBase>();

  const filters = ref<Filters>({
    search: '',
    brand: null,
    strength: null,
  });

  const sortField = ref<string>('name');
  const sortOrder = ref<1 | -1>(1);

  const pagination = ref<Pagination>({
    first: 0,
    rows: 20,
    totalRecords: 0,
  });

  // Debounce для поиска
  let searchTimeout: ReturnType<typeof setTimeout> | null = null;

  // Options
  const brandOptions = ref<SelectOption[]>([]); // Заменяем computed на ref

  // Methods
  async function loadCigars(): Promise<void> {
    loading.value = true;
    try {
      const params = {
        page: pagination.value.first / pagination.value.rows + 1,
        pageSize: pagination.value.rows,
        sortField: sortField.value,
        sortOrder: sortOrder.value === 1 ? 'asc' : 'desc',
        name: filters.value.search,
        brandId: filters.value.brand,
        strength: filters.value.strength,
      };
      const result: PaginatedResult<CigarBase> = await cigarService.getCigarBasesPaginated(params);
      cigars.value = result.items || [];
      pagination.value.totalRecords = result.totalCount || 0;
    } catch (error) {
      console.error('Failed to load cigars:', error);
      toast.add({
        severity: 'error',
        summary: 'Error',
        detail: 'Failed to load cigar base data.',
        life: 3000,
      });
    } finally {
      loading.value = false;
    }
  }

  async function loadBrands(): Promise<void> {
    try {
      const allBrands = await cigarService.getBrands();
      // Наполняем ref напрямую
      brandOptions.value = allBrands.map((brand: Brand) => ({
        label: brand.name,
        value: brand.id,
      }));
    } catch (error) {
      console.error('Failed to load brands:', error);
    }
  }

  function viewCigar(cigar: CigarBase): void {
    selectedCigar.value = cigar;
    showDetailDialog.value = true;
  }

  function addToCollection(cigar: CigarBase): void {
    // Переходим на страницу добавления сигары с предзаполненными данными
    router.push({
      name: 'cigarNew',
      query: {
        cigarBaseId: cigar.id,
        name: cigar.name,
        brandId: cigar.brand.id,
        brandName: cigar.brand.name,
        country: cigar.country,
        size: cigar.size,
        strength: cigar.strength,
        description: cigar.description,
        wrapper: cigar.wrapper,
        binder: cigar.binder,
        filler: cigar.filler,
      },
    });
  }

  function createNewCigar(): void {
    // Переходим на страницу создания новой сигары
    router.push({
      name: 'cigarNew',
    });
  }

  function createSimilarCigar(cigar: CigarBase): void {
    // Переходим на страницу создания новой сигары с предзаполненными данными
    router.push({
      name: 'cigarNew',
      query: {
        name: cigar.name,
        brandId: cigar.brand.id,
        country: cigar.country,
        size: cigar.size,
        strength: cigar.strength,
        description: cigar.description,
        wrapper: cigar.wrapper,
        binder: cigar.binder,
        filler: cigar.filler,
      },
    });
  }

  function editBaseCigar(cigar: CigarBase): void {
    editingCigar.value = { ...cigar };
    showDetailDialog.value = false;
    showEditDialog.value = true;
  }

  function onCigarSaved(updatedCigar: CigarBase): void {
    // Обновляем данные в списке
    const index = cigars.value.findIndex((c: CigarBase) => c.id === updatedCigar.id);
    if (index !== -1) {
      cigars.value[index] = { ...cigars.value[index], ...updatedCigar };
    }

    // Обновляем выбранную сигару в детальном просмотре
    if (selectedCigar.value && selectedCigar.value.id === updatedCigar.id) {
      selectedCigar.value = { ...selectedCigar.value, ...updatedCigar };
    }

    // Закрываем диалог редактирования
    showEditDialog.value = false;
    editingCigar.value = undefined;

    toast.add({
      severity: 'success',
      summary: 'Успешно',
      detail: 'Базовая сигара обновлена',
      life: 3000,
    });
  }

  function writeReview(cigar: CigarBase): void {
    // Переходим на страницу создания отзыва с предзаполненными данными о сигаре
    router.push({
      name: 'reviewCreate',
      query: { cigarBaseId: cigar.id, brandName: cigar.brand.name, cigarName: cigar.name },
    });
  }

  function onSearch(): void {
    // Сбрасываем пагинацию при поиске
    pagination.value.first = 0;

    // Debounce поиска для избежания частых запросов
    if (searchTimeout) {
      clearTimeout(searchTimeout);
    }

    searchTimeout = setTimeout(() => {
      loadCigars();
    }, 300);
  }

  function onFilterChange(): void {
    // Сбрасываем пагинацию при изменении фильтров
    pagination.value.first = 0;
    loadCigars();
  }

  function onPage(event: DataTablePageEvent | PageState): void {
    pagination.value.first = event.first;
    pagination.value.rows = event.rows;
    loadCigars();
  }

  function onSort(event: DataTableSortEvent): void {
    if (event.sortField && (event.sortOrder === 1 || event.sortOrder === -1)) {
      sortField.value = event.sortField as string;
      sortOrder.value = event.sortOrder;
      loadCigars();
    }
  }

  // Функции для работы с крепостью в мобильных карточках
  function getStrengthLabel(strength: string | null | undefined): string {
    if (!strength) return '';

    const option = strengthOptions.find((opt) => opt.value === strength);
    return option ? option.label : strength;
  }

  function getStrengthBadgeClass(strength: string | null | undefined): string {
    if (!strength) return 'bg-gray-100 text-gray-800';
    const classes: Record<string, string> = {
      very_mild: 'bg-green-100 text-green-800',
      mild: 'bg-blue-100 text-blue-800',
      medium: 'bg-yellow-100 text-yellow-800',
      full: 'bg-orange-100 text-orange-800',
      very_full: 'bg-red-100 text-red-800',
    };
    return classes[strength] || 'bg-gray-100 text-gray-800';
  }

  // Функции для работы с изображениями в мобильных карточках
  function getBrandLogoSrc(cigar: CigarBase): string | undefined {
    if (cigar.brand && cigar.brand.logoBytes) {
      return `data:image/png;base64,${cigar.brand.logoBytes}`;
    }
    return undefined;
  }

  function handleImageError(event: Event): void {
    const target = event.target as HTMLImageElement;
    console.error('Ошибка загрузки изображения:', target.src);

    target.style.display = 'none';

    const container = target.parentElement;
    if (container) {
      const existingFallback = container.querySelector('.pi-image-fallback');
      if (!existingFallback) {
        const fallback = document.createElement('i');
        fallback.className = 'pi pi-image text-gray-400 text-2xl pi-image-fallback';
        container.appendChild(fallback);
      }
    }
  }

  function previousPage(): void {
    if (pagination.value.first > 0) {
      pagination.value.first = Math.max(0, pagination.value.first - pagination.value.rows);
      loadCigars();
    }
  }

  function nextPage(): void {
    if (pagination.value.first + pagination.value.rows < pagination.value.totalRecords) {
      pagination.value.first += pagination.value.rows;
      loadCigars();
    }
  }

  // Watchers
  watch(
    filters,
    (newFilters, oldFilters) => {
      if (JSON.stringify(newFilters) !== JSON.stringify(oldFilters)) {
        pagination.value.first = 0;
        loadCigars();
      }
    },
    { deep: true },
  );

  // Lifecycle
  onMounted(() => {
    loadCigars();
    loadBrands();
  });
</script>
