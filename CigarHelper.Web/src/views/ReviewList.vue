<template>
  <div class="review-list-page">
    <div class="container">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h1>Обзоры сигар</h1>
        <router-link v-if="isAuthenticated" to="/reviews/create" class="btn btn-primary">
          <i class="bi bi-plus-lg"></i> Написать обзор
        </router-link>
      </div>
      
      <!-- Фильтры -->
      <div class="filters mb-4">
        <div class="card">
          <div class="card-body">
            <div class="row g-3">
              <div class="col-md-4">
                <label for="brand-filter" class="form-label">Бренд</label>
                <select id="brand-filter" v-model="filters.brand" class="form-select">
                  <option value="">Все бренды</option>
                  <option v-for="brand in brands" :key="brand" :value="brand">{{ brand }}</option>
                </select>
              </div>
              <div class="col-md-4">
                <label for="rating-filter" class="form-label">Минимальная оценка</label>
                <select id="rating-filter" v-model="filters.minRating" class="form-select">
                  <option value="">Любая оценка</option>
                  <option v-for="rating in [1,2,3,4,5,6,7,8,9,10]" :key="rating" :value="rating">{{ rating }}</option>
                </select>
              </div>
              <div class="col-md-4">
                <label for="sort-by" class="form-label">Сортировка</label>
                <select id="sort-by" v-model="sortBy" class="form-select">
                  <option value="date-desc">Сначала новые</option>
                  <option value="date-asc">Сначала старые</option>
                  <option value="rating-desc">По оценке (лучшие)</option>
                  <option value="rating-asc">По оценке (худшие)</option>
                </select>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Загрузка -->
      <div v-if="loading" class="text-center my-5">
        <div class="spinner-border" role="status">
          <span class="visually-hidden">Загрузка...</span>
        </div>
        <p class="mt-2">Загрузка обзоров...</p>
      </div>
      
      <!-- Ошибка -->
      <div v-else-if="error" class="alert alert-danger">
        {{ error }}
      </div>
      
      <!-- Нет результатов -->
      <div v-else-if="filteredReviews.length === 0" class="alert alert-info">
        <p class="mb-0">Обзоров не найдено</p>
        <div v-if="hasActiveFilters" class="mt-2">
          <button @click="clearFilters" class="btn btn-sm btn-outline-primary">
            Сбросить фильтры
          </button>
        </div>
      </div>
      
      <!-- Список обзоров -->
      <div v-else class="row row-cols-1 row-cols-md-2 g-4">
        <div v-for="review in filteredReviews" :key="review.id" class="col">
          <div class="card h-100 review-card">
            <div class="card-header d-flex justify-content-between align-items-center">
              <div class="d-flex align-items-center">
                <div class="avatar me-2">
                  <img 
                    :src="review.userAvatarUrl || '/img/default-avatar.png'" 
                    :alt="review.username"
                    class="rounded-circle"
                  >
                </div>
                <div>
                  <div class="fw-bold">{{ review.username }}</div>
                  <div class="text-muted small">{{ formatDate(review.createdAt) }}</div>
                </div>
              </div>
              <div class="rating">
                <span class="badge bg-warning text-dark">
                  <i class="bi bi-star-fill"></i> {{ review.rating }}/10
                </span>
              </div>
            </div>
            <div v-if="review.mainImageUrl" class="review-image">
              <img :src="review.mainImageUrl" class="card-img-top" :alt="review.title">
              <div v-if="review.imageCount > 1" class="image-count">
                <i class="bi bi-images"></i> {{ review.imageCount }}
              </div>
            </div>
            <div class="card-body">
              <h5 class="card-title">{{ review.title }}</h5>
              <h6 class="card-subtitle mb-2 text-muted">{{ review.cigarBrand }} {{ review.cigarName }}</h6>
              <p class="card-text">{{ review.summary }}</p>
            </div>
            <div class="card-footer">
              <router-link :to="`/reviews/${review.id}`" class="btn btn-outline-primary">
                Читать полностью
              </router-link>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import api from '../services/api'
import authService from '../services/authService'

export default {
  data() {
    return {
      reviews: [],
      loading: true,
      error: null,
      filters: {
        brand: '',
        minRating: ''
      },
      sortBy: 'date-desc',
      brands: []
    }
  },
  computed: {
    isAuthenticated() {
      return authService.isAuthenticated()
    },
    hasActiveFilters() {
      return this.filters.brand || this.filters.minRating
    },
    filteredReviews() {
      let result = [...this.reviews]
      
      // Применяем фильтры
      if (this.filters.brand) {
        result = result.filter(review => review.cigarBrand === this.filters.brand)
      }
      
      if (this.filters.minRating) {
        result = result.filter(review => review.rating >= this.filters.minRating)
      }
      
      // Применяем сортировку
      switch (this.sortBy) {
        case 'date-asc':
          result.sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt))
          break
        case 'rating-desc':
          result.sort((a, b) => b.rating - a.rating)
          break
        case 'rating-asc':
          result.sort((a, b) => a.rating - b.rating)
          break
        case 'date-desc':
        default:
          result.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
          break
      }
      
      return result
    }
  },
  async created() {
    await this.fetchReviews()
  },
  methods: {
    async fetchReviews() {
      this.loading = true
      this.error = null
      
      try {
        const response = await api.get('/reviews')
        this.reviews = response.data
        
        // Извлекаем все уникальные бренды для фильтра
        this.brands = [...new Set(this.reviews.map(review => review.cigarBrand))].sort()
        
      } catch (error) {
        console.error('Ошибка при загрузке обзоров:', error)
        this.error = 'Не удалось загрузить обзоры. Пожалуйста, попробуйте позже.'
      } finally {
        this.loading = false
      }
    },
    formatDate(dateString) {
      const options = { year: 'numeric', month: 'long', day: 'numeric' }
      return new Date(dateString).toLocaleDateString('ru-RU', options)
    },
    clearFilters() {
      this.filters = {
        brand: '',
        minRating: ''
      }
    }
  }
}
</script>

<style scoped>
.review-list-page {
  padding: 2rem 0;
}

.review-card {
  transition: transform 0.2s, box-shadow 0.2s;
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