<script setup lang="ts">
  import { computed } from 'vue';
  import { RouterLink } from 'vue-router';
  import Avatar from 'primevue/avatar';
  import { prefetchPublicProfileVisibility } from '@/services/profileService';
  import { publicUserProfileLocation } from '@/utils/publicProfileRoute';

  const props = withDefaults(
    defineProps<{
      username: string;
      isAuthorProfilePublic: boolean;
      /** Показывать аватар (обзоры); в комментариях обычно false. */
      showAvatar?: boolean;
      avatarUrl?: string | null;
      avatarSize?: 'small' | 'large' | 'xlarge';
      /** Доп. классы для строки с именем (например крупный заголовок на странице обзора). */
      nameClass?: string;
      /** Доп. классы для корневого RouterLink (тон строки в комментариях и т.п.). */
      linkClass?: string;
      /**
       * Карточка со слоем RouterLink на весь фон (список обзоров): поднять z-index
       * и остановить всплытие, чтобы клик по автору не открывал карточку обзора.
       */
      overlayCardMode?: boolean;
    }>(),
    {
      showAvatar: true,
      avatarUrl: null,
      avatarSize: 'small',
      nameClass: '',
      linkClass: '',
      overlayCardMode: false,
    },
  );

  const displayName = computed(() => props.username?.trim() ?? '');

  const toProfile = computed(() =>
    displayName.value ? publicUserProfileLocation(displayName.value) : { name: 'Home' },
  );

  const rootClass = computed(() => {
    const gap = props.avatarSize === 'large' || props.avatarSize === 'xlarge' ? 'gap-3' : 'gap-2';
    const base = `flex min-w-0 items-center ${gap}`;
    return props.overlayCardMode ? `relative z-[25] ${base} pointer-events-auto` : base;
  });

  const nameSpanClass = computed(() => {
    const base =
      'min-w-0 truncate font-medium text-stone-800 dark:text-stone-200 group-hover/author:text-rose-800 dark:group-hover/author:text-rose-100';
    return [base, props.nameClass].filter(Boolean).join(' ');
  });

  function onHoverPublic(): void {
    if (props.isAuthorProfilePublic && displayName.value) {
      prefetchPublicProfileVisibility(displayName.value);
    }
  }
</script>

<template>
  <div
    :class="rootClass"
    @click.stop>
    <template v-if="isAuthorProfilePublic && displayName">
      <RouterLink
        class="group/author flex min-w-0 rounded-lg text-left text-inherit no-underline outline-none ring-rose-600/0 transition hover:text-rose-700 focus-visible:ring-2 focus-visible:ring-rose-600 dark:text-stone-200 dark:ring-rose-400/0 dark:hover:text-rose-200"
        :class="[
          avatarSize === 'large' || avatarSize === 'xlarge' ? 'items-center gap-3' : 'items-center gap-2',
          linkClass,
        ]"
        :to="toProfile"
        :aria-label="`Публичный профиль: ${displayName}`"
        @mouseenter="onHoverPublic">
        <Avatar
          v-if="showAvatar"
          :image="avatarUrl || '/img/default-avatar.png'"
          :size="avatarSize"
          shape="circle"
          class="shrink-0 ring-1 ring-stone-200/80 transition group-hover/author:ring-rose-300 dark:ring-stone-600 dark:group-hover/author:ring-rose-500/60"
          :aria-hidden="true" />
        <div class="min-w-0">
          <span :class="nameSpanClass">
            {{ displayName }}
          </span>
          <div
            v-if="$slots.meta"
            class="mt-0.5 text-sm text-stone-600 dark:text-stone-400">
            <slot name="meta" />
          </div>
        </div>
      </RouterLink>
    </template>
    <div
      v-else
      v-tooltip.top="'Профиль пользователя скрыт или недоступен.'"
      class="flex min-w-0 cursor-default rounded-lg text-stone-600 dark:text-stone-400"
      :class="avatarSize === 'large' || avatarSize === 'xlarge' ? 'items-center gap-3' : 'items-center gap-2'">
      <Avatar
        v-if="showAvatar"
        :image="avatarUrl || '/img/default-avatar.png'"
        :size="avatarSize"
        shape="circle"
        class="shrink-0 opacity-90"
        :aria-hidden="true" />
      <div class="min-w-0">
        <span
          class="min-w-0 truncate font-medium text-stone-800 dark:text-stone-200"
          :class="nameClass">
          {{ displayName }}
        </span>
        <div
          v-if="$slots.meta"
          class="mt-0.5 text-sm text-stone-600 dark:text-stone-400">
          <slot name="meta" />
        </div>
      </div>
    </div>
  </div>
</template>
