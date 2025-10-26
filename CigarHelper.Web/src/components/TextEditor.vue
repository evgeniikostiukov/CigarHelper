<script lang="ts" setup>
  import { ref, watch, onMounted, onBeforeUnmount, computed } from 'vue';
  import { Editor, EditorContent } from '@tiptap/vue-3';
  import StarterKit from '@tiptap/starter-kit';
  import Image from '@tiptap/extension-image';
  import Placeholder from '@tiptap/extension-placeholder';
  import CharacterCount from '@tiptap/extension-character-count';

  // --- Props & Emits ---

  interface Props {
    modelValue: string;
    placeholder?: string;
    maxLength?: number;
  }

  const props = withDefaults(defineProps<Props>(), {
    modelValue: '',
    placeholder: 'Начните писать здесь...',
    maxLength: 10000,
  });

  const emit = defineEmits<{
    (e: 'update:modelValue', value: string): void;
  }>();

  // --- Refs ---

  const editor = ref<Editor | null>(null);
  const fileInput = ref<HTMLInputElement | null>(null);

  // --- Logic ---

  onMounted(() => {
    editor.value = new Editor({
      extensions: [
        StarterKit.configure({
          heading: {
            levels: [2, 3],
          },
        }),
        Image,
        Placeholder.configure({
          placeholder: props.placeholder,
        }),
        CharacterCount.configure({
          limit: props.maxLength,
        }),
      ],
      content: props.modelValue,
      onUpdate: ({ editor: updatedEditor }) => {
        emit('update:modelValue', updatedEditor.getHTML());
      },
    });
  });

  onBeforeUnmount(() => {
    editor.value?.destroy();
  });

  watch(
    () => props.modelValue,
    (newValue) => {
      if (editor.value && editor.value.getHTML() !== newValue) {
        editor.value.commands.setContent(newValue, false);
      }
    },
  );

  const characterCount = computed(() => {
    return editor.value?.storage.characterCount.characters() || 0;
  });

  const isContentTooLong = computed(() => {
    if (!editor.value || !props.maxLength) return false;
    return characterCount.value > props.maxLength;
  });

  const tiptapEditor = computed(() => editor.value || undefined);

  function addImageByUrl() {
    if (!editor.value) return;
    const url = window.prompt('Введите URL изображения:');
    if (url) {
      editor.value.chain().focus().setImage({ src: url }).run();
    }
  }

  function triggerFileInput() {
    fileInput.value?.click();
  }

  function handleFileChange(event: Event) {
    const target = event.target as HTMLInputElement;
    const file = target.files?.[0];

    if (!file || !editor.value) return;

    if (!file.type.startsWith('image/')) {
      alert('Выбранный файл не является изображением.');
      return;
    }
    if (file.size > 15 * 1024 * 1024) {
      // 15MB
      alert('Размер файла не должен превышать 15МБ.');
      return;
    }

    const reader = new FileReader();
    reader.onload = (e) => {
      const src = e.target?.result as string;
      if (src) {
        editor.value?.chain().focus().setImage({ src }).run();
      }
    };
    reader.readAsDataURL(file);
  }
</script>

<template>
  <div class="border rounded-lg overflow-hidden">
    <div
      v-if="editor"
      class="flex flex-wrap items-center gap-2 p-2 bg-gray-50 border-b">
      <button
        @click="editor.chain().focus().toggleBold().run()"
        :disabled="!editor.can().chain().focus().toggleBold().run()"
        :class="{ 'is-active': editor.isActive('bold') }"
        class="toolbar-button"
        title="Жирный">
        <i class="pi pi-bold"></i>
      </button>
      <button
        @click="editor.chain().focus().toggleItalic().run()"
        :disabled="!editor.can().chain().focus().toggleItalic().run()"
        :class="{ 'is-active': editor.isActive('italic') }"
        class="toolbar-button"
        title="Курсив">
        <i class="pi pi-italic"></i>
      </button>
      <button
        @click="editor.chain().focus().toggleStrike().run()"
        :disabled="!editor.can().chain().focus().toggleStrike().run()"
        :class="{ 'is-active': editor.isActive('strike') }"
        class="toolbar-button"
        title="Зачеркнутый">
        <i class="pi pi-strikethrough"></i>
      </button>

      <div class="w-px h-5 bg-gray-300 mx-1"></div>

      <button
        @click="editor.chain().focus().toggleHeading({ level: 2 }).run()"
        :class="{ 'is-active': editor.isActive('heading', { level: 2 }) }"
        class="toolbar-button"
        title="Заголовок 2">
        H2
      </button>
      <button
        @click="editor.chain().focus().toggleHeading({ level: 3 }).run()"
        :class="{ 'is-active': editor.isActive('heading', { level: 3 }) }"
        class="toolbar-button"
        title="Заголовок 3">
        H3
      </button>

      <div class="w-px h-5 bg-gray-300 mx-1"></div>

      <button
        @click="editor.chain().focus().toggleBulletList().run()"
        :class="{ 'is-active': editor.isActive('bulletList') }"
        class="toolbar-button"
        title="Маркированный список">
        <i class="pi pi-list"></i>
      </button>
      <button
        @click="editor.chain().focus().toggleOrderedList().run()"
        :class="{ 'is-active': editor.isActive('orderedList') }"
        class="toolbar-button"
        title="Нумерованный список">
        <i class="pi pi-bars"></i>
      </button>

      <div class="w-px h-5 bg-gray-300 mx-1"></div>

      <button
        @click="addImageByUrl"
        class="toolbar-button"
        title="Добавить изображение по ссылке">
        <i class="pi pi-link"></i>
      </button>
      <button
        @click="triggerFileInput"
        class="toolbar-button"
        title="Загрузить изображение">
        <i class="pi pi-upload"></i>
      </button>
      <input
        ref="fileInput"
        type="file"
        @change="handleFileChange"
        accept="image/*"
        class="hidden" />
    </div>

    <div v-if="editor">
      <editor-content
        :editor="editor as any"
        class="p-3 min-h-[150px]" />
    </div>

    <div
      v-if="editor"
      class="text-xs text-right p-2 border-t text-gray-500"
      :class="{ 'text-red-500': isContentTooLong }">
      {{ characterCount }} / {{ maxLength }}
    </div>
  </div>
</template>

<style>
  .ProseMirror {
    outline: none;
    line-height: 1.6;
  }

  .ProseMirror p.is-editor-empty:first-child::before {
    content: attr(data-placeholder);
    float: left;
    color: #adb5bd;
    pointer-events: none;
    height: 0;
  }

  .toolbar-button {
    @apply px-2 py-1 rounded hover:bg-gray-200;
  }

  .toolbar-button.is-active {
    @apply bg-gray-300;
  }
</style>
