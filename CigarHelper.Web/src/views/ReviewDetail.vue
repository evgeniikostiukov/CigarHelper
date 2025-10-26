<template>
  <div class="p-4 sm:p-6 lg:p-8">
    <div class="max-w-4xl mx-auto">
      <!-- Загрузка -->
      <div v-if="loading">
        <Skeleton
          height="2rem"
          width="50%"
          class="mb-4" />
        <Skeleton
          height="3rem"
          class="mb-6" />
        <Skeleton
          height="25rem"
          class="mb-6" />
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Skeleton height="15rem" />
          <Skeleton height="15rem" />
        </div>
      </div>

      <!-- Ошибка -->
      <div v-else-if="error">
        <Message
          severity="error"
          :closable="false"
          class="mb-4"
          >{{ error }}</Message
        >
        <Button
          @click="$router.push('/reviews')"
          label="Вернуться к списку"
          icon="pi pi-arrow-left" />
      </div>

      <!-- Содержимое обзора -->
      <div
        v-else-if="review"
        class="space-y-8">
        <!-- Хлебные крошки и действия -->
        <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
          <Breadcrumb
            :home="home"
            :model="breadcrumbItems" />
          <div
            v-if="isCurrentUserReview"
            class="flex items-center gap-2">
            <Button
              @click="$router.push(`/reviews/${review.id}/edit`)"
              label="Редактировать"
              icon="pi pi-pencil"
              severity="secondary"
              outlined />
            <Button
              @click="confirmDelete"
              label="Удалить"
              icon="pi pi-trash"
              severity="danger" />
          </div>
        </div>

        <!-- Заголовок и мета-информация -->
        <header class="space-y-4">
          <h1 class="text-4xl md:text-5xl font-extrabold text-gray-900 dark:text-white tracking-tighter">
            {{ review.title }}
          </h1>
          <div
            class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4 p-4 bg-gray-100 dark:bg-gray-800 rounded-lg">
            <div class="flex items-center gap-3">
              <Avatar
                :image="review.userAvatarUrl || '/img/default-avatar.png'"
                size="large"
                shape="circle" />
              <div>
                <p class="font-bold text-lg">{{ review.username }}</p>
                <p class="text-sm text-gray-500 dark:text-gray-400">
                  {{ formatDate(review.createdAt) }}
                </p>
              </div>
            </div>
            <div class="flex items-center gap-2">
              <span class="font-bold text-lg">Оценка:</span>
              <Tag
                :value="review.rating + '/10'"
                icon="pi pi-star-fill"
                severity="warning"
                class="text-lg px-3 py-2" />
            </div>
          </div>
          <Message
            severity="info"
            :closable="false"
            >Обзор на сигару:
            <strong class="font-semibold">{{ review.cigarBrand }} {{ review.cigarName }}</strong></Message
          >
        </header>

        <!-- Галерея изображений -->
        <Galleria
          v-if="review.images && review.images.length"
          :value="review.images"
          :numVisible="5"
          containerClass="w-full"
          :showThumbnails="review.images.length > 1"
          :showItemNavigators="review.images.length > 1">
          <template #item="slotProps">
            <img
              :src="`data:image/jpeg;base64,${slotProps.item.imageData}`"
              :alt="slotProps.item.caption || review.title"
              class="w-full h-auto max-h-[70vh] object-contain block" />
          </template>
          <template #thumbnail="slotProps">
            <img
              :src="`data:image/jpeg;base64,${slotProps.item.imageData}`"
              :alt="slotProps.item.caption || review.title"
              class="w-24 h-16 object-cover" />
          </template>
          <template #caption="slotProps">
            <p
              v-if="slotProps.item.caption"
              class="text-white">
              {{ slotProps.item.caption }}
            </p>
          </template>
        </Galleria>

        <!-- Характеристики -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
          <Card>
            <template #title>Характеристики</template>
            <template #content>
              <ul class="space-y-4">
                <li v-if="review.smokingExperience">
                  <strong class="font-semibold">Опыт курения:</strong>
                  {{ review.smokingExperience }}
                </li>
                <li v-if="review.aroma">
                  <strong class="font-semibold">Аромат:</strong>
                  {{ review.aroma }}
                </li>
                <li v-if="review.taste">
                  <strong class="font-semibold">Вкус:</strong>
                  {{ review.taste }}
                </li>
                <li
                  v-if="review.construction"
                  class="flex items-center gap-2">
                  <strong class="font-semibold">Конструкция:</strong>
                  <Rating
                    :modelValue="review.construction"
                    readonly
                    :cancel="false" />
                </li>
                <li
                  v-if="review.burnQuality"
                  class="flex items-center gap-2">
                  <strong class="font-semibold">Качество горения:</strong>
                  <Rating
                    :modelValue="review.burnQuality"
                    readonly
                    :cancel="false" />
                </li>
                <li
                  v-if="review.draw"
                  class="flex items-center gap-2">
                  <strong class="font-semibold">Тяга:</strong>
                  <Rating
                    :modelValue="review.draw"
                    readonly
                    :cancel="false" />
                </li>
              </ul>
            </template>
          </Card>
          <Card>
            <template #title>Детали дегустации</template>
            <template #content>
              <ul class="space-y-4">
                <li v-if="review.venue">
                  <strong class="font-semibold">Место:</strong>
                  {{ review.venue }}
                </li>
                <li>
                  <strong class="font-semibold">Дата дегустации:</strong>
                  {{ formatDate(review.smokingDate) }}
                </li>
              </ul>
            </template>
          </Card>
        </div>

        <!-- Содержание -->
        <Card>
          <template #title>Обзор</template>
          <template #content>
            <div
              class="prose dark:prose-invert max-w-none"
              v-html="formattedContent"></div>
          </template>
        </Card>

        <!-- Навигация -->
        <div class="flex justify-between items-center">
          <Button
            @click="$router.push('/reviews')"
            label="Назад к списку"
            icon="pi pi-arrow-left"
            severity="secondary" />
          <Button
            @click="$router.push(`/cigars/${review.cigarId}`)"
            label="Перейти к сигаре"
            icon="pi pi-arrow-right"
            iconPos="right" />
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { useConfirm } from 'primevue/useconfirm';
  import { useToast } from 'primevue/usetoast';
  import reviewService from '../services/reviewService';
  import type { Review } from '../services/reviewService';
  import authService from '../services/authService';
  import DOMPurify from 'dompurify';

  // --- Interfaces ---
  interface ReviewImage {
    imageUrl: string;
    caption?: string;
  }

  // --- Component State ---
  const route = useRoute();
  const router = useRouter();
  const confirm = useConfirm();
  const toast = useToast();

  const review = ref<Review | null>(null);
  const loading = ref(true);
  const error = ref<string | null>(null);

  const home = ref({ icon: 'pi pi-home', to: '/' });

  // --- Computed Properties ---
  const breadcrumbItems = computed(() => [
    { label: 'Обзоры', to: '/reviews' },
    { label: review.value?.title || '...' },
  ]);

  const isCurrentUserReview = computed(() => {
    if (!review.value) return false;
    const currentUser = authService.state.user;
    return currentUser && currentUser.id === review.value.userId;
  });

  const formattedContent = computed(() => {
    if (!review.value?.content) return '';
    // A more robust HTML sanitization library like DOMPurify is recommended for production
    return review.value.content
      .split('\n')
      .filter((p) => p.trim().length > 0)
      .map((p) => `<p>${p}</p>`)
      .join('');
  });

  // --- Methods ---
  const loadReview = async (): Promise<void> => {
    loading.value = true;
    error.value = null;
    try {
      const reviewId = route.params.id as string;
      review.value = await reviewService.getReview(reviewId);
    } catch (err) {
      console.error('Ошибка загрузки обзора:', err);
      error.value = 'Не удалось загрузить обзор. Возможно, он был удален или ссылка неверна.';
    } finally {
      loading.value = false;
    }
  };

  const formatDate = (dateString: string | undefined) => {
    if (!dateString) return 'Дата не указана';
    return new Date(dateString).toLocaleDateString('ru-RU', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    });
  };

  const confirmDelete = () => {
    confirm.require({
      message: 'Вы уверены, что хотите удалить этот обзор? Это действие нельзя отменить.',
      header: 'Подтверждение удаления',
      icon: 'pi pi-exclamation-triangle',
      rejectClass: 'p-button-secondary p-button-outlined',
      acceptClass: 'p-button-danger',
      rejectLabel: 'Отмена',
      acceptLabel: 'Удалить',
      accept: async () => {
        if (!review.value) return;
        try {
          await reviewService.deleteReview(review.value.id);
          toast.add({
            severity: 'success',
            summary: 'Успешно',
            detail: 'Обзор удален',
            life: 3000,
          });
          router.push('/reviews');
        } catch (err) {
          console.error('Ошибка при удалении обзора:', err);
          toast.add({
            severity: 'error',
            summary: 'Ошибка',
            detail: 'Не удалось удалить обзор',
            life: 3000,
          });
        }
      },
    });
  };

  // --- Lifecycle Hooks ---
  onMounted(loadReview);
</script>

<style>
  /* Стили для контента из v-html, Tailwind's Typography plugin - лучшая альтернатива */
  .prose p {
    margin-bottom: 1rem;
  }
  .prose h1,
  .prose h2,
  .prose h3 {
    margin-bottom: 1rem;
    margin-top: 1.5rem;
  }
</style>
