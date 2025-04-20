<template>
  <div class="image-uploader">
    <div class="mb-3">
      <label class="form-label">Добавить изображение</label>
      <div class="upload-options d-flex">
        <ul class="nav nav-tabs">
          <li class="nav-item">
            <a class="nav-link" :class="{ active: activeTab === 'url' }" href="#" @click.prevent="activeTab = 'url'">
              По URL
            </a>
          </li>
          <li class="nav-item">
            <a class="nav-link" :class="{ active: activeTab === 'file' }" href="#" @click.prevent="activeTab = 'file'">
              Загрузить файл
            </a>
          </li>
        </ul>
      </div>
      
      <!-- URL-вкладка -->
      <div v-if="activeTab === 'url'" class="mt-3">
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
            @click="addImageByUrl"
            :disabled="!imageUrl || saving"
          >
            <span v-if="saving" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
            Добавить
          </button>
        </div>
        <div class="form-text">Вставьте URL изображения</div>
      </div>
      
      <!-- Загрузка файла -->
      <div v-if="activeTab === 'file'" class="mt-3">
        <div class="mb-3">
          <input 
            type="file" 
            class="form-control" 
            id="imageFile" 
            ref="fileInput"
            accept="image/*" 
            @change="handleFileChange"
          >
          <div class="form-text">Поддерживаемые форматы: JPG, PNG, GIF. Максимальный размер: 15 МБ</div>
        </div>
        <div v-if="filePreview" class="file-preview mb-3">
          <div class="card">
            <div class="card-body">
              <div class="d-flex">
                <div class="preview-image me-3">
                  <img :src="filePreview" alt="Preview" class="img-thumbnail">
                </div>
                <div class="flex-grow-1">
                  <h6 class="mb-1">{{ fileName }}</h6>
                  <p class="text-muted mb-1 small">{{ fileSize }}</p>
                  <div class="mt-2">
                    <button 
                      @click="uploadFile" 
                      class="btn btn-primary btn-sm me-2"
                      :disabled="uploading"
                    >
                      <span v-if="uploading" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
                      Загрузить
                    </button>
                    <button @click="cancelFileUpload" class="btn btn-outline-secondary btn-sm">
                      Отмена
                    </button>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
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
      activeTab: 'url',
      images: [...this.modelValue],
      imageUrl: '',
      saving: false,
      file: null,
      filePreview: null,
      fileName: '',
      fileSize: '',
      uploading: false
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
    addImageByUrl() {
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
    
    handleFileChange(event) {
      this.file = event.target.files[0]
      
      if (!this.file) {
        this.clearFileUpload()
        return
      }
      
      // Проверка типа файла
      if (!this.file.type.match('image.*')) {
        alert('Выбранный файл не является изображением')
        this.clearFileUpload()
        return
      }
      
      // Проверка размера (15MB)
      if (this.file.size > 15 * 1024 * 1024) {
        alert('Размер файла превышает 15 МБ')
        this.clearFileUpload()
        return
      }
      
      // Отображаем превью
      this.fileName = this.file.name
      this.fileSize = this.formatFileSize(this.file.size)
      
      const reader = new FileReader()
      reader.onload = (e) => {
        this.filePreview = e.target.result
      }
      reader.readAsDataURL(this.file)
    },
    
    formatFileSize(bytes) {
      if (bytes < 1024) {
        return bytes + ' байт'
      } else if (bytes < 1024 * 1024) {
        return (bytes / 1024).toFixed(1) + ' КБ'
      } else {
        return (bytes / (1024 * 1024)).toFixed(1) + ' МБ'
      }
    },
    
    cancelFileUpload() {
      this.clearFileUpload()
    },
    
    clearFileUpload() {
      this.file = null
      this.filePreview = null
      this.fileName = ''
      this.fileSize = ''
      if (this.$refs.fileInput) {
        this.$refs.fileInput.value = null
      }
    },
    
    async uploadFile() {
      if (!this.file) return
      
      this.uploading = true
      
      try {
        // В реальном API здесь нужен загрузчик на сервер
        // Для демонстрации просто конвертируем в base64
        const base64 = await this.getBase64(this.file)
        
        // Создаем новый объект изображения
        const newImage = {
          id: `new-${Date.now()}`,
          imageUrl: base64,
          caption: ''
        }
        
        // Добавляем в список и оповещаем родителя
        this.images.push(newImage)
        this.$emit('update:modelValue', this.images)
        
        // Очищаем форму
        this.clearFileUpload()
        
      } catch (error) {
        console.error('Ошибка при загрузке файла:', error)
        alert('Не удалось загрузить изображение. Попробуйте снова.')
      } finally {
        this.uploading = false
      }
    },
    
    getBase64(file) {
      return new Promise((resolve, reject) => {
        const reader = new FileReader()
        reader.readAsDataURL(file)
        reader.onload = () => resolve(reader.result)
        reader.onerror = (error) => reject(error)
      })
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

.preview-image {
  width: 100px;
  height: 100px;
  overflow: hidden;
}

.preview-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}
</style> 