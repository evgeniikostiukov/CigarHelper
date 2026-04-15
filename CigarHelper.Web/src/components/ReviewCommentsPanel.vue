<script setup lang="ts">
  import { computed, ref, watch } from 'vue';
  import PublicProfileAuthorBlock from '@/components/PublicProfileAuthorBlock.vue';
  import Textarea from 'primevue/textarea';
  import Button from 'primevue/button';
  import { useToast } from 'primevue/usetoast';
  import { useAuth } from '@/services/useAuth';
  import { hasAnyRole } from '@/utils/roles';
  import * as reviewCommentApi from '@/services/reviewCommentService';

  const props = defineProps<{
    reviewId: number;
  }>();

  const { isAuthenticated, user } = useAuth();
  const toast = useToast();

  const comments = ref<reviewCommentApi.ReviewCommentDto[]>([]);
  const loading = ref(false);
  const newBody = ref('');
  const submitting = ref(false);

  const hasTarget = computed(() => props.reviewId > 0);

  function formatWhen(iso: string): string {
    try {
      return new Date(iso).toLocaleString('ru-RU', {
        dateStyle: 'short',
        timeStyle: 'short',
      });
    } catch {
      return iso;
    }
  }

  async function load(): Promise<void> {
    if (!hasTarget.value) {
      comments.value = [];
      return;
    }
    loading.value = true;
    try {
      comments.value = await reviewCommentApi.fetchReviewComments(props.reviewId);
    } catch {
      comments.value = [];
    } finally {
      loading.value = false;
    }
  }

  watch(
    () => props.reviewId,
    () => {
      void load();
    },
    { immediate: true },
  );

  async function submit(): Promise<void> {
    if (!isAuthenticated.value) {
      toast.add({
        severity: 'warn',
        summary: 'Вход',
        detail: 'Войдите, чтобы оставить комментарий.',
        life: 4000,
      });
      return;
    }
    const body = newBody.value.trim();
    if (!body) {
      return;
    }
    submitting.value = true;
    try {
      const created = await reviewCommentApi.createReviewComment({
        reviewId: props.reviewId,
        body,
      });
      newBody.value = '';
      if (created.moderationStatus === 'Pending') {
        toast.add({
          severity: 'info',
          summary: 'На проверке',
          detail: 'Комментарий появится после одобрения модератором.',
          life: 4500,
        });
      } else {
        await load();
        toast.add({ severity: 'success', summary: 'Комментарий добавлен', life: 2500 });
      }
    } finally {
      submitting.value = false;
    }
  }

  async function removeComment(id: number): Promise<void> {
    try {
      await reviewCommentApi.deleteReviewComment(id);
      await load();
      toast.add({ severity: 'info', summary: 'Комментарий удалён', life: 2000 });
    } catch {
      /* interceptor */
    }
  }

  function canDelete(comment: reviewCommentApi.ReviewCommentDto): boolean {
    const uid = user.value?.id;
    if (uid != null && comment.authorUserId === uid) {
      return true;
    }
    return hasAnyRole(user.value, ['Admin', 'Moderator']);
  }
</script>

<template>
  <section
    class="rounded-2xl border border-stone-200/90 bg-stone-50/80 p-4 dark:border-stone-700/90 dark:bg-stone-950/40 sm:p-5"
    :aria-busy="loading"
    data-testid="review-comments-panel">
    <h3 class="mb-3 text-base font-semibold text-stone-900 dark:text-rose-50/95">Комментарии к обзору</h3>

    <p
      v-if="!isAuthenticated && hasTarget"
      class="mb-3 text-sm text-stone-600 dark:text-stone-400">
      Войдите в аккаунт, чтобы оставить комментарий. Просмотр доступен всем.
    </p>

    <template v-if="hasTarget">
      <div
        v-if="isAuthenticated"
        class="mb-4 flex flex-col gap-2">
        <label
          for="review-comment-body"
          class="text-xs font-medium text-stone-600 dark:text-stone-400">
          Новый комментарий
        </label>
        <Textarea
          id="review-comment-body"
          v-model="newBody"
          data-testid="review-comment-input"
          :disabled="submitting"
          rows="3"
          auto-resize
          class="w-full min-h-12"
          placeholder="Текст (до 2000 символов)…" />
        <div class="flex justify-end">
          <Button
            type="button"
            data-testid="review-comment-submit"
            label="Отправить"
            icon="pi pi-send"
            class="min-h-11 touch-manipulation"
            :loading="submitting"
            :disabled="!newBody.trim()"
            @click="submit" />
        </div>
      </div>

      <div
        v-if="loading"
        class="py-6 text-center text-sm text-stone-500">
        Загрузка…
      </div>

      <ul
        v-else-if="comments.length === 0 && hasTarget"
        class="rounded-xl border border-dashed border-stone-200/80 py-8 text-center text-sm text-stone-500 dark:border-stone-600/80 dark:text-stone-400">
        Пока нет комментариев.
      </ul>

      <ul
        v-else
        class="max-h-64 space-y-3 overflow-y-auto pr-1 sm:max-h-80">
        <li
          v-for="c in comments"
          :key="c.id"
          class="rounded-xl border border-stone-200/80 bg-white/90 px-3 py-2.5 dark:border-stone-600/70 dark:bg-stone-900/60"
          :data-testid="`review-comment-${c.id}`">
          <div class="flex flex-wrap items-baseline justify-between gap-2">
            <PublicProfileAuthorBlock
              :username="c.authorUsername"
              :is-author-profile-public="c.isAuthorProfilePublic === true"
              :show-avatar="false"
              name-class="text-sm font-medium text-rose-900 dark:text-rose-200/90"
              link-class="text-rose-900 dark:text-rose-200/90 hover:text-rose-950 dark:hover:text-rose-50" />
            <time
              class="text-xs text-stone-500 dark:text-stone-400"
              :datetime="c.createdAt">
              {{ formatWhen(c.createdAt) }}
            </time>
          </div>
          <p class="mt-1.5 whitespace-pre-wrap text-pretty text-sm text-stone-800 dark:text-stone-200">
            {{ c.body }}
          </p>
          <div
            v-if="canDelete(c)"
            class="mt-2 flex justify-end">
            <Button
              type="button"
              severity="danger"
              text
              rounded
              icon="pi pi-trash"
              class="min-h-10 min-w-10"
              :aria-label="`Удалить комментарий ${c.id}`"
              @click="removeComment(c.id)" />
          </div>
        </li>
      </ul>
    </template>
  </section>
</template>
