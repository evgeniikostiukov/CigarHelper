<script lang="ts" setup>
  import { ref, computed } from 'vue';

  // --- Props & Emits ---
  interface Props {
    maxFiles?: number;
    currentFileCount?: number;
  }

  const props = withDefaults(defineProps<Props>(), {
    maxFiles: 5,
    currentFileCount: 0,
  });

  const emit = defineEmits<{
    'files-selected': [File[]];
  }>();

  // --- Refs ---
  const fileInput = ref<HTMLInputElement | null>(null);

  // --- Computed ---
  const remainingSlots = computed(() => {
    return props.maxFiles - props.currentFileCount;
  });

  // --- Methods ---
  function open() {
    if (fileInput.value) {
      fileInput.value.click();
    }
  }

  function handleFileChange(event: Event) {
    const target = event.target as HTMLInputElement;
    if (target.files) {
      const files = Array.from(target.files);
      emit('files-selected', files);

      // Очистка input для повторного выбора того же файла
      if (fileInput.value) {
        fileInput.value.value = '';
      }
    }
  }

  // --- Expose ---
  defineExpose({
    open,
  });
</script>

<template>
  <div>
    <input
      ref="fileInput"
      type="file"
      accept="image/png, image/jpeg, image/gif, image/webp"
      multiple
      class="hidden"
      @change="handleFileChange"
      :disabled="remainingSlots <= 0" />
  </div>
</template>
