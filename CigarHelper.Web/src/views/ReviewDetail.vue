<template>
  <div class="review-detail-page">
    <div class="container">
      <!-- Загрузка -->
      <div v-if="loading" class="text-center my-5">
        <div class="spinner-border" role="status">
          <span class="visually-hidden">Загрузка...</span>
        </div>
        <p class="mt-2">Загрузка обзора...</p>
      </div>
      
      <!-- Ошибка -->
      <div v-else-if="error" class="alert alert-danger">
        {{ error }}
        <div class="mt-3">
          <router-link to="/reviews" class="btn btn-outline-primary">
            Вернуться к списку обзоров
          </router-link>
        </div>
      </div>
      
      <!-- Содержимое обзора -->
      <div v-else class="review-content">
        <div class="d-flex justify-content-between align-items-center mb-3">
          <nav aria-label="breadcrumb">
            <ol class="breadcrumb mb-0">
              <li class="breadcrumb-item"><router-link to="/reviews">Обзоры</router-link></li>
              <li class="breadcrumb-item active" aria-current="page">{{ review.title }}</li>
            </ol>
          </nav>
          
          <div v-if="isCurrentUserReview" class="review-actions">
            <router-link :to="`/reviews/${review.id}/edit`" class="btn btn-sm btn-outline-primary me-2">
              <i class="bi bi-pencil"></i> Редактировать
            </router-link>
            <button @click="confirmDelete" class="btn btn-sm btn-outline-danger">
              <i class="bi bi-trash"></i> Удалить
            </button>
          </div>
        </div>
        
        <div class="review-header">
          <h1 class="mb-3">{{ review.title }}</h1>
          
          <div class="review-meta d-flex justify-content-between align-items-center mb-4">
            <div class="d-flex align-items-center">
              <div class="avatar me-3">
                <img 
                  :src="review.userAvatarUrl || '/img/default-avatar.png'" 
                  :alt="review.username"
                  class="rounded-circle"
                >
              </div>
              <div>
                <div class="fw-bold">{{ review.username }}</div>
                <div class="text-muted">{{ formatDate(review.createdAt) }}</div>
              </div>
            </div>
            
            <div class="rating d-flex align-items-center">
              <span class="me-2">Оценка:</span>
              <span class="badge bg-warning text-dark fs-5">
                <i class="bi bi-star-fill"></i> {{ review.rating }}/10
              </span>
            </div>
          </div>
          
          <div class="alert alert-light d-flex align-items-center">
            <i class="bi bi-info-circle-fill me-2"></i>
            <div>Обзор на сигару <strong>{{ review.cigarBrand }} {{ review.cigarName }}</strong></div>
          </div>
        </div>
        
        <!-- Карусель изображений -->
        <div v-if="review.images && review.images.length > 0" class="review-images mb-4">
          <div id="reviewImagesCarousel" class="carousel slide" data-bs-ride="carousel">
            <div class="carousel-indicators">
              <button 
                v-for="(image, index) in review.images" 
                :key="`indicator-${image.id}`"
                type="button" 
                data-bs-target="#reviewImagesCarousel" 
                :data-bs-slide-to="index" 
                :class="{ active: index === 0 }"
                :aria-current="index === 0 ? 'true' : 'false'"
                :aria-label="`Слайд ${index + 1}`"
              ></button>
            </div>
            <div class="carousel-inner">
              <div 
                v-for="(image, index) in review.images" 
                :key="`slide-${image.id}`"
                class="carousel-item" 
                :class="{ active: index === 0 }"
              >
                <img :src="image.imageUrl" class="d-block w-100" :alt="image.caption || review.title">
                <div v-if="image.caption" class="carousel-caption d-none d-md-block">
                  <p>{{ image.caption }}</p>
                </div>
              </div>
            </div>
            <button v-if="review.images.length > 1" class="carousel-control-prev" type="button" data-bs-target="#reviewImagesCarousel" data-bs-slide="prev">
              <span class="carousel-control-prev-icon" aria-hidden="true"></span>
              <span class="visually-hidden">Предыдущий</span>
            </button>
            <button v-if="review.images.length > 1" class="carousel-control-next" type="button" data-bs-target="#reviewImagesCarousel" data-bs-slide="next">
              <span class="carousel-control-next-icon" aria-hidden="true"></span>
              <span class="visually-hidden">Следующий</span>
            </button>
          </div>
        </div>
        
        <!-- Характеристики обзора -->
        <div class="row mb-4">
          <div class="col-md-6">
            <div class="card mb-3">
              <div class="card-header">
                <h5 class="mb-0">Характеристики</h5>
              </div>
              <div class="card-body">
                <div class="row">
                  <div v-if="review.smokingExperience" class="col-6 mb-2">
                    <div class="fw-bold">Опыт курения:</div>
                    <div>{{ review.smokingExperience }}</div>
                  </div>
                  <div v-if="review.aroma" class="col-6 mb-2">
                    <div class="fw-bold">Аромат:</div>
                    <div>{{ review.aroma }}</div>
                  </div>
                  <div v-if="review.taste" class="col-6 mb-2">
                    <div class="fw-bold">Вкус:</div>
                    <div>{{ review.taste }}</div>
                  </div>
                  <div v-if="review.construction" class="col-6 mb-2">
                    <div class="fw-bold">Конструкция:</div>
                    <div>{{ review.construction }}</div>
                  </div>
                  <div v-if="review.burnQuality" class="col-6 mb-2">
                    <div class="fw-bold">Качество горения:</div>
                    <div>{{ review.burnQuality }}</div>
                  </div>
                  <div v-if="review.draw" class="col-6 mb-2">
                    <div class="fw-bold">Тяга:</div>
                    <div>{{ review.draw }}</div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div class="col-md-6">
            <div class="card mb-3">
              <div class="card-header">
                <h5 class="mb-0">Детали дегустации</h5>
              </div>
              <div class="card-body">
                <div v-if="review.venue" class="mb-2">
                  <div class="fw-bold">Место:</div>
                  <div>{{ review.venue }}</div>
                </div>
                <div class="mb-2">
                  <div class="fw-bold">Дата дегустации:</div>
                  <div>{{ formatDate(review.smokingDate) }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Содержание обзора -->
        <div class="review-body card mb-4">
          <div class="card-header">
            <h3 class="mb-0">Обзор</h3>
          </div>
          <div class="card-body">
            <div class="content" v-html="formattedContent"></div>
          </div>
        </div>
        
        <div class="d-flex justify-content-between">
          <router-link to="/reviews" class="btn btn-outline-secondary">
            <i class="bi bi-arrow-left"></i> Назад к списку
          </router-link>
          
          <router-link :to="`/cigars/${review.cigarId}`" class="btn btn-primary">
            Перейти к сигаре <i class="bi bi-arrow-right"></i>
          </router-link>
        </div>
      </div>
    </div>
    
    <!-- Модальное окно подтверждения удаления -->
    <div class="modal fade" id="deleteConfirmModal" tabindex="-1" aria-labelledby="deleteConfirmModalLabel" aria-hidden="true">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title" id="deleteConfirmModalLabel">Подтвердите удаление</h5>
            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Закрыть"></button>
          </div>
          <div class="modal-body">
            Вы уверены, что хотите удалить этот обзор? Это действие нельзя отменить.
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Отмена</button>
            <button type="button" class="btn btn-danger" @click="deleteReview" :disabled="deleting">
              <span v-if="deleting" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
              Удалить
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import api from '../services/api'
import authService from '../services/authService'
import { Modal } from 'bootstrap'

export default {
  data() {
    return {
      review: null,
      loading: true,
      error: null,
      deleteModal: null,
      deleting: false
    }
  },
  computed: {
    isCurrentUserReview() {
      if (!this.review) return false
      
      const currentUser = authService.getCurrentUser()
      return currentUser && currentUser.id === this.review.userId
    },
    formattedContent() {
      if (!this.review) return ''
      
      // Преобразуем текст в HTML с абзацами
      return this.review.content
        .split('\n')
        .filter(paragraph => paragraph.trim().length > 0)
        .map(paragraph => `<p>${paragraph}</p>`)
        .join('')
    }
  },
  async mounted() {
    await this.fetchReview()
    
    // Инициализируем модальное окно Bootstrap
    this.deleteModal = new Modal(document.getElementById('deleteConfirmModal'))
  },
  methods: {
    async fetchReview() {
      this.loading = true
      this.error = null
      
      try {
        const reviewId = this.$route.params.id
        const response = await api.get(`/reviews/${reviewId}`)
        this.review = response.data
      } catch (error) {
        console.error('Ошибка при загрузке обзора:', error)
        this.error = 'Не удалось загрузить обзор. Возможно, он был удален или у вас нет к нему доступа.'
      } finally {
        this.loading = false
      }
    },
    formatDate(dateString) {
      const options = { year: 'numeric', month: 'long', day: 'numeric' }
      return new Date(dateString).toLocaleDateString('ru-RU', options)
    },
    confirmDelete() {
      this.deleteModal.show()
    },
    async deleteReview() {
      this.deleting = true
      
      try {
        await api.delete(`/reviews/${this.review.id}`)
        this.deleteModal.hide()
        this.$router.push('/reviews')
      } catch (error) {
        console.error('Ошибка при удалении обзора:', error)
        alert('Не удалось удалить обзор. Пожалуйста, попробуйте позже.')
      } finally {
        this.deleting = false
      }
    }
  }
}
</script>

<style scoped>
.review-detail-page {
  padding: 2rem 0;
}

.avatar img {
  width: 50px;
  height: 50px;
  object-fit: cover;
}

.carousel-inner {
  max-height: 500px;
  background-color: #f8f9fa;
}

.carousel-inner img {
  object-fit: contain;
  max-height: 500px;
  margin: 0 auto;
}

.content p {
  margin-bottom: 1rem;
  line-height: 1.6;
}

.content img {
  max-width: 100%;
  height: auto;
  margin: 1rem 0;
}
</style> 