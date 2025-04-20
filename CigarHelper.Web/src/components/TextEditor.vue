<template>
  <div class="editor-wrapper">
    <div v-if="editor" class="editor-toolbar">
      <button 
        @click="editor.chain().focus().toggleBold().run()"
        :class="{ 'is-active': editor.isActive('bold') }"
        class="toolbar-button"
        title="Жирный"
      >
        <i class="bi bi-type-bold"></i>
      </button>
      <button 
        @click="editor.chain().focus().toggleItalic().run()"
        :class="{ 'is-active': editor.isActive('italic') }"
        class="toolbar-button"
        title="Курсив"
      >
        <i class="bi bi-type-italic"></i>
      </button>
      <button 
        @click="editor.chain().focus().toggleStrike().run()"
        :class="{ 'is-active': editor.isActive('strike') }"
        class="toolbar-button"
        title="Зачеркнутый"
      >
        <i class="bi bi-type-strikethrough"></i>
      </button>
      <div class="toolbar-divider"></div>
      <button 
        @click="editor.chain().focus().toggleHeading({ level: 2 }).run()"
        :class="{ 'is-active': editor.isActive('heading', { level: 2 }) }"
        class="toolbar-button"
        title="Заголовок 2"
      >
        <i class="bi bi-type-h2"></i>
      </button>
      <button 
        @click="editor.chain().focus().toggleHeading({ level: 3 }).run()"
        :class="{ 'is-active': editor.isActive('heading', { level: 3 }) }"
        class="toolbar-button"
        title="Заголовок 3"
      >
        <i class="bi bi-type-h3"></i>
      </button>
      <div class="toolbar-divider"></div>
      <button 
        @click="editor.chain().focus().toggleBulletList().run()"
        :class="{ 'is-active': editor.isActive('bulletList') }"
        class="toolbar-button"
        title="Маркированный список"
      >
        <i class="bi bi-list-ul"></i>
      </button>
      <button 
        @click="editor.chain().focus().toggleOrderedList().run()"
        :class="{ 'is-active': editor.isActive('orderedList') }"
        class="toolbar-button"
        title="Нумерованный список"
      >
        <i class="bi bi-list-ol"></i>
      </button>
      <div class="toolbar-divider"></div>
      <button 
        @click="editor.chain().focus().setHorizontalRule().run()"
        class="toolbar-button"
        title="Горизонтальная линия"
      >
        <i class="bi bi-hr"></i>
      </button>
      <div class="toolbar-divider"></div>
      <div class="image-toolbar-group">
        <button 
          @click="openImageMenu"
          class="toolbar-button"
          title="Добавить изображение"
          ref="imageButton"
        >
          <i class="bi bi-image"></i>
        </button>
        <div v-if="showImageOptions" class="image-options-menu" ref="imageMenu">
          <div class="image-options-header">
            <h6 class="mb-2">Добавить изображение</h6>
          </div>
          <div class="image-option" @click="addImageByUrl">
            <i class="bi bi-link me-2"></i> По URL
          </div>
          <div class="image-option" @click="triggerFileInput">
            <i class="bi bi-upload me-2"></i> Загрузить файл
          </div>
        </div>
        <input 
          type="file" 
          ref="fileInput" 
          @change="handleFileChange" 
          accept="image/*" 
          class="d-none"
        >
      </div>
    </div>
    
    <editor-content v-if="editor" :editor="editor" class="editor-content" />
    <div v-else class="editor-loading">
      <div class="text-center p-4">
        <div class="spinner-border spinner-border-sm" role="status">
          <span class="visually-hidden">Загрузка...</span>
        </div>
      </div>
    </div>
    
    <div class="editor-footer">
      <div class="char-count" :class="{ 'text-danger': isContentTooLong }">
        {{ contentLength }} / {{ maxLength }} символов
      </div>
    </div>
  </div>
</template>

<script>
import { Editor, EditorContent } from '@tiptap/vue-3'
import StarterKit from '@tiptap/starter-kit'
import Image from '@tiptap/extension-image'

export default {
  components: {
    EditorContent,
  },
  props: {
    modelValue: {
      type: String,
      default: ''
    },
    placeholder: {
      type: String,
      default: 'Начните писать здесь...'
    },
    maxLength: {
      type: Number,
      default: 10000
    }
  },
  data() {
    return {
      editor: null,
      contentLength: 0,
      showImageOptions: false
    }
  },
  computed: {
    isContentTooLong() {
      return this.contentLength > this.maxLength
    }
  },
  watch: {
    modelValue(newValue) {
      // Обновляем содержимое редактора, если modelValue изменился извне
      if (this.editor) {
        const currentContent = this.editor.getHTML()
        if (newValue !== currentContent) {
          this.editor.commands.setContent(newValue, false)
        }
      }
    }
  },
  mounted() {
    this.editor = new Editor({
      extensions: [
        StarterKit,
        Image
      ],
      content: this.modelValue,
      onUpdate: ({ editor }) => {
        const html = editor.getHTML()
        this.$emit('update:modelValue', html)
        
        // Подсчет символов без HTML-тегов
        const text = editor.getText()
        this.contentLength = text.length
      },
      editorProps: {
        attributes: {
          class: 'form-control editor-input',
          spellcheck: 'true',
        },
      },
    })
    
    // Обработчик клика вне меню для его закрытия
    document.addEventListener('click', this.closeImageMenuOnClickOutside)
  },
  beforeUnmount() {
    if (this.editor) {
      this.editor.destroy()
    }
    // Удаляем обработчик перед размонтированием компонента
    document.removeEventListener('click', this.closeImageMenuOnClickOutside)
  },
  methods: {
    openImageMenu() {
      if (!this.editor) return
      this.showImageOptions = true
    },
    
    closeImageMenuOnClickOutside(event) {
      if (this.showImageOptions && 
          this.$refs.imageMenu && 
          !this.$refs.imageMenu.contains(event.target) &&
          this.$refs.imageButton && 
          !this.$refs.imageButton.contains(event.target)) {
        this.showImageOptions = false
      }
    },
    
    addImageByUrl() {
      if (!this.editor) return
      
      const url = prompt('Введите URL изображения:')
      
      if (url) {
        this.editor.chain().focus().setImage({ src: url }).run()
      }
      
      this.showImageOptions = false
    },
    
    triggerFileInput() {
      if (!this.editor) return
      this.$refs.fileInput.click()
    },
    
    handleFileChange(event) {
      if (!this.editor) return
      
      const file = event.target.files[0]
      
      if (!file) return
      
      // Проверка типа файла
      if (!file.type.match('image.*')) {
        alert('Выбранный файл не является изображением')
        this.$refs.fileInput.value = null
        return
      }
      
      // Проверка размера (15MB)
      if (file.size > 15 * 1024 * 1024) {
        alert('Размер файла превышает 15 МБ')
        this.$refs.fileInput.value = null
        return
      }
      
      // Конвертируем файл в base64
      const reader = new FileReader()
      reader.onload = (e) => {
        // Вставляем изображение в редактор
        this.editor.chain().focus().setImage({ src: e.target.result }).run()
        
        // Сбрасываем input
        this.$refs.fileInput.value = null
      }
      reader.readAsDataURL(file)
      
      this.showImageOptions = false
    }
  }
}
</script>

<style scoped>
.editor-wrapper {
  border: 1px solid #ced4da;
  border-radius: 0.25rem;
}

.editor-toolbar {
  display: flex;
  padding: 0.5rem;
  border-bottom: 1px solid #e2e8f0;
  background-color: #f8f9fa;
  border-top-left-radius: 0.25rem;
  border-top-right-radius: 0.25rem;
  flex-wrap: wrap;
}

.toolbar-button {
  background: none;
  border: none;
  color: #4a5568;
  font-size: 1rem;
  cursor: pointer;
  border-radius: 0.25rem;
  padding: 0.25rem 0.5rem;
  margin-right: 0.25rem;
}

.toolbar-button:hover {
  background-color: #e2e8f0;
}

.toolbar-button.is-active {
  background-color: #e2e8f0;
  color: #2b6cb0;
}

.toolbar-divider {
  width: 1px;
  background-color: #e2e8f0;
  margin: 0 0.5rem;
}

.image-toolbar-group {
  position: relative;
}

.image-options-menu {
  position: absolute;
  top: 100%;
  left: 0;
  z-index: 1000;
  min-width: 200px;
  padding: 0.5rem 0;
  margin: 0.5rem 0 0;
  background-color: #fff;
  border: 1px solid rgba(0, 0, 0, 0.15);
  border-radius: 0.25rem;
  box-shadow: 0 0.5rem 1rem rgba(0, 0, 0, 0.15);
}

.image-options-header {
  padding: 0.25rem 1rem;
  border-bottom: 1px solid #e2e8f0;
}

.image-option {
  padding: 0.5rem 1rem;
  cursor: pointer;
}

.image-option:hover {
  background-color: #f8f9fa;
}

.editor-content {
  padding: 1rem;
  min-height: 200px;
  max-height: 500px;
  overflow-y: auto;
}

:deep(.editor-input) {
  outline: none;
  min-height: 150px;
  border: none;
  padding: 0;
}

:deep(.editor-input p) {
  margin-bottom: 1rem;
}

:deep(.editor-input img) {
  max-width: 100%;
  height: auto;
  margin: 1rem 0;
}

.editor-footer {
  padding: 0.5rem;
  display: flex;
  justify-content: flex-end;
  border-top: 1px solid #e2e8f0;
  background-color: #f8f9fa;
  border-bottom-left-radius: 0.25rem;
  border-bottom-right-radius: 0.25rem;
}

.char-count {
  font-size: 0.8rem;
  color: #666;
}
</style> 