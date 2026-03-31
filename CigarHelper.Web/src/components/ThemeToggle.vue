<script setup lang="ts">
  import Button from 'primevue/button';
  import { useTheme } from '@/composables/useTheme';

  const { isDark, toggleTheme } = useTheme();
</script>

<template>
  <Button
    :icon="isDark ? 'pi pi-sun' : 'pi pi-moon'"
    text
    rounded
    severity="secondary"
    class="min-h-11 min-w-11 touch-manipulation transition-transform duration-200 hover:scale-110"
    :aria-label="isDark ? 'Переключить на светлую тему' : 'Переключить на тёмную тему'"
    :title="isDark ? 'Светлая тема' : 'Тёмная тема'"
    data-testid="theme-toggle"
    @click="toggleTheme()" />
</template>
