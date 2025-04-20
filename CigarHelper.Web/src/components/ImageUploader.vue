<template>
  <div class="image-uploader">
    <div class="mb-3">
      <label for="imageUrl" class="form-label">URL изображения</label>
      <div class="input-group">
        <input 
          type="text" 
          class="form-control" 
          id="imageUrl" 
          v-model="imageUrl"
          placeholder="https://example.com/image.jpg"
        >
        <button 
          class="btn btn-outline-primary" 
          type="button"
          @click="addImage"
          :disabled="!imageUrl || saving"
        >
          <span v-if="saving" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
          Добавить
        </button>
      </div>
      <div class="form-text">Вставьте URL изображения</div>
    </div>
    
    <div v-if="images.length > 0" class="image-list mb-3">
      <h6 class="mb-3">Изображения ({{ images.length }})</h6>
      <div class="row row-cols-1 row-cols-md-3 g-3">
        <div 
          v-for="(image, index) in images" 
          :key="image.id || `temp-${index}`" 
          class="col"
        >
          <div class="card h-100">
            <div class="image-preview">
              <img :src="image.imageUrl" :alt="image.caption || 'Изображение'" class="card-img-top">
            </div>
            <div class="card-body">
              <div class="mb-2">
                <input 
                  type="text" 
                  class="form-control form-control-sm" 
                  v-model="image.caption"
                  placeholder="Подпись к изображению"
                  @input="updateImage(image)"
                >
              </div>
              <div class="d-flex">
                <button
                  @click="removeImage(image)" 
                  class="btn btn-sm btn-outline-danger ms-auto"
                >
                  <i class="bi bi-trash"></i> Удалить
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  props: {
    modelValue: {
      type: Array,
      default: () => []
    }
  },
  data() {
    return {
      images: [...this.modelValue],
      imageUrl: '',
      saving: false
    }
  },
  watch: {
    modelValue(newValue) {
      // Обновляем список изображений, если modelValue изменился извне
      if (JSON.stringify(newValue) !== JSON.stringify(this.images)) {
        this.images = [...newValue]
      }
    }
  },
  methods: {
    addImage() {
      if (!this.imageUrl) return
      
      this.saving = true
      
      // Простая проверка загрузки изображения
      const img = new Image()
      img.onload = () => {
        // Создаем новый объект изображения
        const newImage = {
          id: `new-${Date.now()}`, // Временный ID для новых изображений
          imageUrl: this.imageUrl,
          caption: ''
        }
        
        // Добавляем в список и оповещаем родителя
        this.images.push(newImage)
        this.$emit('update:modelValue', this.images)
        
        // Очищаем поле ввода
        this.imageUrl = ''
        this.saving = false
      }
      
      img.onerror = () => {
        alert('Не удалось загрузить изображение. Проверьте URL и попробуйте снова.')
        this.saving = false
      }
      
      img.src = this.imageUrl
    },
    
    removeImage(image) {
      const index = this.images.findIndex(i => 
        (i.id && i.id === image.id) || i.imageUrl === image.imageUrl
      )
      
      if (index !== -1) {
        this.images.splice(index, 1)
        this.$emit('update:modelValue', this.images)
      }
    },
    
    updateImage(image) {
      // Оповещаем родителя о изменении изображений
      this.$emit('update:modelValue', this.images)
    }
  }
}
</script>

<style scoped>
.image-preview {
  height: 160px;
  overflow: hidden;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: #f8f9fa;
}

.image-preview img {
  width: 100%;
  height: 160px;
  object-fit: cover;
}
</style> 