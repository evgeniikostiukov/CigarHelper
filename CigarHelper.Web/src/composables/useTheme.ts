import { readonly } from 'vue';
import { useDark, useToggle } from '@vueuse/core';

/**
 * Синглтон-composable управления тёмной/светлой темой.
 * - Читает system preference при первом запуске.
 * - Сохраняет выбор пользователя в localStorage (ключ 'cigar-color-scheme').
 * - Добавляет/убирает класс .dark на <html>, который подхватывают:
 *   • Tailwind (@custom-variant dark)
 *   • PrimeVue (darkModeSelector: '.dark')
 *   • Ручные CSS-правила (.dark { })
 */
const isDark = useDark({
  storageKey: 'cigar-color-scheme',
  valueDark: 'dark',
  valueLight: '',
});

const toggleDark = useToggle(isDark);

export function useTheme() {
  return {
    isDark: readonly(isDark),
    toggleTheme: toggleDark,
    setTheme: (dark: boolean) => {
      isDark.value = dark;
    },
  };
}
