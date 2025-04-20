<template>
  <div class="review-form-page">
    <div class="container">
      <div class="d-flex justify-content-between align-items-center mb-4">
        <h1>{{ isEditing ? 'Редактирование обзора' : 'Новый обзор' }}</h1>
      </div>
      
      <!-- Загрузка -->
      <div v-if="loading" class="text-center my-5">
        <div class="spinner-border" role="status">
          <span class="visually-hidden">Загрузка...</span>
        </div>
        <p class="mt-2">{{ isEditing ? 'Загрузка обзора...' : 'Загрузка данных...' }}</p>
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
      
      <!-- Форма обзора -->
      <div v-else>
        <form @submit.prevent="submitForm" class="review-form">
          <!-- Общая информация -->
          <div class="card mb-4">
            <div class="card-header">
              <h5 class="mb-0">Общая информация</h5>
            </div>
            <div class="card-body">
              <!-- Выбор сигары -->
              <div class="mb-3">
                <label for="cigarId" class="form-label">Сигара *</label>
                <select
                  id="cigarId"
                  v-model="form.cigarId"
                  class="form-select"
                  :class="{'is-invalid': validationErrors.cigarId}"
                  required
                >
                  <option value="" disabled>Выберите сигару</option>
                  <option v-for="cigar in cigars" :key="cigar.id" :value="cigar.id">
                    {{ cigar.brand }} {{ cigar.name }}
                  </option>
                </select>
                <div v-if="validationErrors.cigarId" class="invalid-feedback">
                  {{ validationErrors.cigarId }}
                </div>
              </div>
              
              <!-- Название обзора -->
              <div class="mb-3">
                <label for="title" class="form-label">Заголовок обзора *</label>
                <input
                  type="text"
                  class="form-control"
                  id="title"
                  v-model="form.title"
                  :class="{'is-invalid': validationErrors.title}"
                  required
                  maxlength="200"
                >
                <div v-if="validationErrors.title" class="invalid-feedback">
                  {{ validationErrors.title }}
                </div>
              </div>
              
              <!-- Оценка -->
              <div class="mb-3">
                <label for="rating" class="form-label">Оценка *</label>
                <div class="d-flex align-items-center">
                  <div class="rating-slider me-3 flex-grow-1">
                    <input
                      type="range"
                      class="form-range"
                      id="rating"
                      v-model.number="form.rating"
                      min="1"
                      max="10"
                      step="1"
                    >
                  </div>
                  <div class="rating-badge">
                    <span class="badge bg-warning text-dark p-2 fs-5">
                      <i class="bi bi-star-fill"></i> {{ form.rating }}/10
                    </span>
                  </div>
                </div>
                <div v-if="validationErrors.rating" class="text-danger small">
                  {{ validationErrors.rating }}
                </div>
              </div>
            </div>
          </div>
          
          <!-- Детали дегустации -->
          <div class="card mb-4">
            <div class="card-header">
              <h5 class="mb-0">Детали дегустации</h5>
            </div>
            <div class="card-body">
              <div class="row">
                <div class="col-md-6">
                  <div class="mb-3">
                    <label for="smokingExperience" class="form-label">Опыт курения</label>
                    <input
                      type="text"
                      class="form-control"
                      id="smokingExperience"
                      v-model="form.smokingExperience"
                      maxlength="50"
                    >
                  </div>
                  
                  <div class="mb-3">
                    <label for="venue" class="form-label">Место дегустации</label>
                    <input
                      type="text"
                      class="form-control"
                      id="venue"
                      v-model="form.venue"
                      maxlength="100"
                    >
                  </div>
                  
                  <div class="mb-3">
                    <label for="smokingDate" class="form-label">Дата дегустации</label>
                    <input
                      type="date"
                      class="form-control"
                      id="smokingDate"
                      v-model="form.smokingDate"
                    >
                  </div>
                </div>
                
                <div class="col-md-6">
                  <div class="mb-3">
                    <label for="aroma" class="form-label">Аромат</label>
                    <input
                      type="text"
                      class="form-control"
                      id="aroma"
                      v-model="form.aroma"
                      maxlength="50"
                    >
                  </div>
                  
                  <div class="mb-3">
                    <label for="taste" class="form-label">Вкус</label>
                    <input
                      type="text"
                      class="form-control"
                      id="taste"
                      v-model="form.taste"
                      maxlength="50"
                    >
                  </div>
                  
                  <div class="row">
                    <div class="col-md-4">
                      <div class="mb-3">
                        <label for="construction" class="form-label">Конструкция</label>
                        <div class="d-flex align-items-center">
                          <div class="rating-slider me-2 flex-grow-1">
                            <input
                              type="range"
                              class="form-range"
                              id="construction"
                              v-model.number="form.construction"
                              min="1"
                              max="5"
                              step="1"
                            >
                          </div>
                          <div class="rating-value">
                            <span class="badge bg-secondary">{{ form.construction || '?' }}/5</span>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-4">
                      <div class="mb-3">
                        <label for="burnQuality" class="form-label">Горение</label>
                        <div class="d-flex align-items-center">
                          <div class="rating-slider me-2 flex-grow-1">
                            <input
                              type="range"
                              class="form-range"
                              id="burnQuality"
                              v-model.number="form.burnQuality"
                              min="1"
                              max="5"
                              step="1"
                            >
                          </div>
                          <div class="rating-value">
                            <span class="badge bg-secondary">{{ form.burnQuality || '?' }}/5</span>
                          </div>
                        </div>
                      </div>
                    </div>
                    <div class="col-md-4">
                      <div class="mb-3">
                        <label for="draw" class="form-label">Тяга</label>
                        <div class="d-flex align-items-center">
                          <div class="rating-slider me-2 flex-grow-1">
                            <input
                              type="range"
                              class="form-range"
                              id="draw"
                              v-model.number="form.draw"
                              min="1"
                              max="5"
                              step="1"
                            >
                          </div>
                          <div class="rating-value">
                            <span class="badge bg-secondary">{{ form.draw || '?' }}/5</span>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          
          <!-- Содержание обзора -->
          <div class="card mb-4">
            <div class="card-header">
              <h5 class="mb-0">Содержание обзора *</h5>
            </div>
            <div class="card-body">
              <text-editor v-model="form.content" />
              
              <div v-if="validationErrors.content" class="text-danger small mt-2">
                {{ validationErrors.content }}
              </div>
            </div>
          </div>
          
          <!-- Изображения -->
          <div class="card mb-4">
            <div class="card-header">
              <h5 class="mb-0">Изображения</h5>
            </div>
            <div class="card-body">
              <image-uploader v-model="form.images" />
            </div>
          </div>
          
          <!-- Кнопки управления -->
          <div class="d-flex justify-content-between">
            <router-link to="/reviews" class="btn btn-outline-secondary">
              <i class="bi bi-x-lg"></i> Отмена
            </router-link>
            
            <button type="submit" class="btn btn-primary" :disabled="saving">
              <span v-if="saving" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
              <i v-else class="bi bi-check-lg"></i>
              {{ isEditing ? 'Сохранить изменения' : 'Опубликовать обзор' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script>
import api from '../services/api'
import TextEditor from '../components/TextEditor.vue'
import ImageUploader from '../components/ImageUploader.vue'

export default {
  components: {
    TextEditor,
    ImageUploader
  },
  data() {
    return {
      isEditing: false,
      loading: true,
      error: null,
      saving: false,
      validationErrors: {},
      cigars: [],
      form: {
        cigarId: '',
        title: '',
        rating: 5,
        content: '',
        smokingExperience: '',
        aroma: '',
        taste: '',
        construction: 3,
        burnQuality: 3,
        draw: 3,
        venue: '',
        smokingDate: new Date().toISOString().split('T')[0],
        images: []
      }
    }
  },
  async created() {
    // Определяем, создаем новый обзор или редактируем существующий
    const reviewId = this.$route.params.id
    this.isEditing = !!reviewId
    
    // Загружаем необходимые данные
    await this.fetchInitialData()
    
    // Если редактируем, загружаем данные обзора
    if (this.isEditing) {
      await this.fetchReview(reviewId)
    }
  },
  methods: {
    async fetchInitialData() {
      try {
        // Загружаем список сигар для выбора
        const response = await api.get('/cigars')
        this.cigars = response.data
        
        this.loading = false
      } catch (error) {
        console.error('Ошибка при загрузке данных:', error)
        this.error = 'Не удалось загрузить необходимые данные. Пожалуйста, попробуйте позже.'
        this.loading = false
      }
    },
    
    async fetchReview(id) {
      this.loading = true
      
      try {
        const response = await api.get(`/reviews/${id}`)
        const review = response.data
        
        // Заполняем форму данными
        this.form.cigarId = review.cigarId
        this.form.title = review.title
        this.form.rating = review.rating
        this.form.content = review.content
        this.form.smokingExperience = review.smokingExperience || ''
        this.form.aroma = review.aroma || ''
        this.form.taste = review.taste || ''
        this.form.construction = review.construction || 3
        this.form.burnQuality = review.burnQuality || 3
        this.form.draw = review.draw || 3
        this.form.venue = review.venue || ''
        
        // Форматируем дату
        if (review.smokingDate) {
          this.form.smokingDate = new Date(review.smokingDate).toISOString().split('T')[0]
        }
        
        // Загружаем изображения
        this.form.images = review.images.map(img => ({
          id: img.id,
          imageUrl: img.imageUrl,
          caption: img.caption || ''
        }))
        
      } catch (error) {
        console.error('Ошибка при загрузке обзора:', error)
        this.error = 'Не удалось загрузить обзор. Возможно, он был удален или у вас нет к нему доступа.'
      } finally {
        this.loading = false
      }
    },
    
    validateForm() {
      this.validationErrors = {}
      let isValid = true
      
      // Проверка обязательных полей
      if (!this.form.cigarId) {
        this.validationErrors.cigarId = 'Необходимо выбрать сигару'
        isValid = false
      }
      
      if (!this.form.title) {
        this.validationErrors.title = 'Заголовок обзора обязателен'
        isValid = false
      } else if (this.form.title.length < 3) {
        this.validationErrors.title = 'Заголовок должен содержать минимум 3 символа'
        isValid = false
      }
      
      if (!this.form.rating || this.form.rating < 1 || this.form.rating > 10) {
        this.validationErrors.rating = 'Оценка должна быть от 1 до 10'
        isValid = false
      }
      
      if (!this.form.content || this.form.content.trim() === '') {
        this.validationErrors.content = 'Содержание обзора обязательно'
        isValid = false
      }
      
      return isValid
    },
    
    async submitForm() {
      // Валидация формы
      if (!this.validateForm()) {
        window.scrollTo({
          top: 0,
          behavior: 'smooth'
        })
        return
      }
      
      this.saving = true
      
      try {
        // Подготавливаем данные для отправки
        const reviewData = {
          cigarId: this.form.cigarId,
          title: this.form.title,
          rating: this.form.rating,
          content: this.form.content,
          smokingExperience: this.form.smokingExperience || null,
          aroma: this.form.aroma || null,
          taste: this.form.taste || null,
          construction: this.form.construction || null,
          burnQuality: this.form.burnQuality || null,
          draw: this.form.draw || null,
          venue: this.form.venue || null,
          smokingDate: this.form.smokingDate || null
        }
        
        // Если редактируем обзор
        if (this.isEditing) {
          const reviewId = this.$route.params.id
          
          // Добавляем информацию о изображениях
          const existingImageIds = this.form.images
            .filter(img => img.id && !img.id.startsWith('new-'))
            .map(img => img.id)
          
          const imagesToAdd = this.form.images
            .filter(img => !img.id || img.id.startsWith('new-'))
            .map(img => ({
              imageUrl: img.imageUrl,
              caption: img.caption
            }))
          
          // Находим ID изображений, которые нужно удалить
          const originalImages = (await api.get(`/reviews/${reviewId}`)).data.images
          const imageIdsToRemove = originalImages
            .filter(img => !existingImageIds.includes(img.id))
            .map(img => img.id)
          
          const updateData = {
            ...reviewData,
            imagesToAdd,
            imageIdsToRemove
          }
          
          await api.put(`/reviews/${reviewId}`, updateData)
          this.$router.push(`/reviews/${reviewId}`)
        } 
        // Если создаем новый обзор
        else {
          // Добавляем изображения
          reviewData.images = this.form.images.map(img => ({
            imageUrl: img.imageUrl,
            caption: img.caption
          }))
          
          const response = await api.post('/reviews', reviewData)
          this.$router.push(`/reviews/${response.data.id}`)
        }
      } catch (error) {
        console.error('Ошибка при сохранении обзора:', error)
        
        if (error.response && error.response.status === 400 && error.response.data.errors) {
          this.validationErrors = error.response.data.errors
          window.scrollTo({
            top: 0,
            behavior: 'smooth'
          })
        } else {
          alert('Не удалось сохранить обзор. Пожалуйста, проверьте введенные данные и попробуйте снова.')
        }
      } finally {
        this.saving = false
      }
    }
  }
}
</script>

<style scoped>
.review-form-page {
  padding: 2rem 0;
}

.rating-badge {
  width: 80px;
  text-align: center;
}

.rating-value {
  width: 40px;
  text-align: center;
}
</style> 