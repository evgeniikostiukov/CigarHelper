<template>
  <div class="p-4 sm:p-6 lg:p-8">
    <div class="max-w-7xl mx-auto">
      <!-- Заголовок и кнопка -->
      <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-6 gap-4">
        <h1 class="text-4xl font-extrabold text-gray-900 dark:text-white tracking-tight">Обзоры сигар</h1>
        <Button
          v-if="isAuthenticated"
          @click="$router.push('/reviews/create')"
          icon="pi pi-plus"
          label="Написать обзор" />
      </div>

      <!-- Фильтры -->
      <Card class="mb-6">
        <template #content>
          <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="flex flex-col">
              <label
                for="brand-filter"
                class="mb-2 font-semibold"
                >Бренд</label
              >
              <Select
                id="brand-filter"
                v-model="filters.brand"
                :options="brands"
                placeholder="Все бренды"
                showClear
                filter />
            </div>
            <div class="flex flex-col">
              <label
                for="rating-filter"
                class="mb-2 font-semibold"
                >Минимальная оценка</label
              >
              <Select
                id="rating-filter"
                v-model="filters.minRating"
                :options="ratingOptions"
                placeholder="Любая оценка"
                showClear />
            </div>
            <div class="flex flex-col">
              <label
                for="sort-by"
                class="mb-2 font-semibold"
                >Сортировка</label
              >
              <Select
                id="sort-by"
                v-model="sortBy"
                :options="sortOptions"
                optionLabel="label"
                optionValue="value" />
            </div>
          </div>
        </template>
      </Card>

      <!-- Состояния -->
      <div
        v-if="loading"
        class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Skeleton
          v-for="n in 4"
          :key="n"
          height="25rem" />
      </div>
      <Message
        v-else-if="error"
        severity="error"
        >{{ error }}</Message
      >
      <div
        v-else-if="filteredReviews.length === 0"
        class="text-center p-8 bg-white dark:bg-gray-800 rounded-lg shadow">
        <h3 class="text-2xl font-bold mb-2">Обзоров не найдено</h3>
        <p class="text-gray-500 dark:text-gray-400 mb-4">Попробуйте изменить параметры фильтрации.</p>
        <Button
          v-if="hasActiveFilters"
          @click="clearFilters"
          label="Сбросить фильтры"
          severity="secondary"
          outlined />
      </div>

      <!-- Список обзоров -->
      <div
        v-else
        class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card
          v-for="review in filteredReviews"
          :key="review.id"
          class="overflow-hidden">
          <template #header>
            <div
              v-if="review.images && review.images.length > 0"
              class="relative">
              <img
                :src="`data:image/jpeg;base64,${review.images[0].imageData}`"
                :alt="review.title"
                class="w-full h-56 object-cover" />
              <div
                v-if="review.images.length > 1"
                class="absolute bottom-2 right-2 bg-black bg-opacity-50 text-white text-xs px-2 py-1 rounded-full flex items-center gap-1">
                <i class="pi pi-images"></i>
                <span>{{ review.images.length }}</span>
              </div>
            </div>
          </template>
          <template #title>
            <div class="flex justify-between items-start">
              <h3 class="text-xl font-bold tracking-tight">
                {{ review.title }}
              </h3>
              <Tag
                :value="review.rating + '/10'"
                icon="pi pi-star-fill"
                severity="warning" />
            </div>
            <p class="text-sm font-normal text-gray-500 dark:text-gray-400 mt-1">
              {{ review.cigarBrand }} {{ review.cigarName }}
            </p>
          </template>
          <template #subtitle>
            <div class="flex items-center gap-2 text-sm mt-2">
              <Avatar
                :image="review.userAvatarUrl || '/img/default-avatar.png'"
                size="small"
                shape="circle" />
              <span class="font-semibold">{{ review.username }}</span>
              <span class="text-gray-500 dark:text-gray-400">· {{ formatDate(review.createdAt) }}</span>
            </div>
          </template>
          <template #content>
            <p class="text-gray-700 dark:text-gray-300 line-clamp-3">{{ review.content }}</p>
          </template>
          <template #footer>
            <Button
              @click="$router.push(`/reviews/${review.id}`)"
              label="Читать полностью"
              class="w-full" />
          </template>
        </Card>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, reactive, computed, onMounted } from 'vue';
  import reviewService from '../services/reviewService';
  import authService from '../services/authService';
  import type { Review } from '../services/reviewService';

  // State
  const reviews = ref<Review[]>([]);
  const loading = ref(true);
  const error = ref<string | null>(null);
  const brands = ref<string[]>([]);
  const isAuthenticated = ref(authService.state.isAuthenticated);

  // Filters & Sorting
  const filters = reactive({
    brand: '',
    minRating: null as number | null,
  });
  const sortBy = ref('date-desc');

  const ratingOptions = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
  const sortOptions = [
    { label: 'Сначала новые', value: 'date-desc' },
    { label: 'Сначала старые', value: 'date-asc' },
    { label: 'По оценке (лучшие)', value: 'rating-desc' },
    { label: 'По оценке (худшие)', value: 'rating-asc' },
  ];

  // Computed
  const hasActiveFilters = computed(() => !!filters.brand || filters.minRating !== null);

  const filteredReviews = computed(() => {
    let result = [...reviews.value];

    if (filters.brand) {
      result = result.filter((review) => review.cigarBrand === filters.brand);
    }
    if (filters.minRating !== null) {
      result = result.filter((review) => review.rating >= filters.minRating!);
    }

    switch (sortBy.value) {
      case 'date-asc':
        result.sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime());
        break;
      case 'rating-desc':
        result.sort((a, b) => b.rating - a.rating);
        break;
      case 'rating-asc':
        result.sort((a, b) => a.rating - b.rating);
        break;
      case 'date-desc':
      default:
        result.sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());
        break;
    }
    return result;
  });

  // Methods
  const fetchReviews = async () => {
    loading.value = true;
    error.value = null;
    try {
      const response = await reviewService.getReviews({ pageSize: 100 }); // Получаем много, т.к. фильтрация на клиенте
      reviews.value = response;
      brands.value = [...new Set(response.map((review) => review.cigarBrand))].sort();
    } catch (err) {
      console.error('Ошибка при загрузке обзоров:', err);
      error.value = 'Не удалось загрузить обзоры. Пожалуйста, попробуйте позже.';
    } finally {
      loading.value = false;
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString('ru-RU', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  const clearFilters = () => {
    filters.brand = '';
    filters.minRating = null;
  };

  // Lifecycle
  onMounted(fetchReviews);
</script>

<style scoped>
  .review-list-page {
    padding: 2rem 0;
  }

  .review-card {
    transition:
      transform 0.2s,
      box-shadow 0.2s;
  }

  .review-card:hover {
    transform: translateY(-5px);
    box-shadow: 0 10px 20px rgba(0, 0, 0, 0.1);
  }

  .avatar img {
    width: 40px;
    height: 40px;
    object-fit: cover;
  }

  .review-image {
    position: relative;
    height: 200px;
    overflow: hidden;
  }

  .review-image img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .image-count {
    position: absolute;
    bottom: 10px;
    right: 10px;
    background-color: rgba(0, 0, 0, 0.6);
    color: white;
    padding: 3px 8px;
    border-radius: 15px;
    font-size: 0.8rem;
  }

  .card-text {
    display: -webkit-box;
    -webkit-line-clamp: 3;
    -webkit-box-orient: vertical;
    overflow: hidden;
  }
</style>
