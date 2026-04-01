<template>
  <section
    class="cigar-form-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-50 via-rose-50/40 to-stone-50 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-rose-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="cigar-form"
    aria-labelledby="cigar-form-heading">
    <div
      class="cigar-form-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto max-w-4xl">
      <header class="flex flex-col gap-4 pb-6 sm:flex-row sm:items-end sm:justify-between sm:pb-8">
        <div class="min-w-0">
          <p
            class="mb-1.5 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55">
            Коллекция
          </p>
          <h1
            id="cigar-form-heading"
            class="text-balance text-3xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 sm:text-4xl">
            {{ isEditing ? 'Редактировать сигару' : 'Новая сигара' }}
          </h1>
          <p class="mt-1.5 max-w-xl text-pretty text-sm text-stone-600 dark:text-stone-400">
            {{
              isEditing
                ? 'Обновите поля и сохраните — данные останутся в вашей коллекции.'
                : 'Выберите сигару из справочника или введите название вручную, затем уточните детали и хранение.'
            }}
          </p>
        </div>
        <Button
          data-testid="cigar-form-back"
          class="min-h-12 w-full shrink-0 touch-manipulation sm:min-h-11 sm:w-auto"
          icon="pi pi-arrow-left"
          label="К списку"
          severity="secondary"
          outlined
          @click="router.push({ name: 'CigarList' })" />
      </header>

      <div
        v-if="isEditing && pageLoading"
        class="min-h-[16rem] space-y-5 rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/40 sm:p-6"
        data-testid="cigar-form-loading"
        aria-busy="true"
        aria-live="polite">
        <div
          v-for="n in 6"
          :key="n"
          class="flex flex-col gap-2">
          <Skeleton
            class="rounded-md"
            height="1rem"
            width="8rem" />
          <Skeleton
            class="rounded-xl border border-stone-200/60 dark:border-stone-600/60"
            height="2.75rem" />
        </div>
      </div>

      <div
        v-else-if="isEditing && loadError"
        class="max-w-2xl rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80"
        data-testid="cigar-form-error"
        role="alert">
        <Message
          severity="error"
          :closable="false">
          {{ loadError }}
        </Message>
        <Button
          data-testid="cigar-form-retry"
          class="mt-4 min-h-12 w-full touch-manipulation sm:w-auto"
          label="Повторить загрузку"
          icon="pi pi-refresh"
          severity="secondary"
          outlined
          @click="loadCigar" />
        <Button
          data-testid="cigar-form-error-back"
          class="mt-3 min-h-12 w-full touch-manipulation sm:mt-4 sm:ml-0 sm:w-auto"
          label="К списку сигар"
          icon="pi pi-list"
          severity="secondary"
          outlined
          @click="router.push({ name: 'CigarList' })" />
      </div>

      <div
        v-else
        class="rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/40 sm:p-6 cigar-form-enter">
        <Message
          v-if="saveError"
          data-testid="cigar-form-save-error"
          class="mb-6"
          severity="error"
          :closable="false">
          {{ saveError }}
        </Message>

        <form
          data-testid="cigar-form-fields"
          class="flex flex-col gap-6 sm:gap-8"
          @submit.prevent="handleSubmit">
          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-4 text-lg font-semibold text-stone-900 dark:text-rose-50/95">Основная информация</h2>
            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <div class="flex flex-col gap-2">
                <label
                  for="name"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Название <span class="text-red-600 dark:text-red-400">*</span>
                </label>
                <AutoComplete
                  id="name"
                  data-testid="cigar-form-name"
                  :model-value="form.cigar"
                  :suggestions="filteredCigars"
                  class="w-full"
                  input-class="min-h-11 w-full"
                  :class="{ 'p-invalid': errors.name }"
                  data-key="id"
                  option-label="name"
                  option-group-label="brand"
                  option-group-children="cigars"
                  :dropdown="true"
                  :virtual-scroller-options="{ itemSize: 50 }"
                  :loading="searchLoading"
                  :delay="300"
                  :min-length="0"
                  placeholder="Введите или выберите название сигары"
                  @update:model-value="onCigarAutocompleteModelUpdate"
                  @complete="searchCigars">
                  <template #optiongroup="slotProps">
                    <div class="p-2 font-semibold text-stone-800 dark:text-stone-200">
                      {{ slotProps.option.brand }}
                    </div>
                  </template>
                  <template #option="slotProps">
                    <div class="flex items-center">
                      <div>
                        <div class="font-semibold text-stone-900 dark:text-stone-100">
                          {{ slotProps.option.name }}
                        </div>
                        <div class="text-xs text-stone-500 dark:text-stone-400">
                          <span class="mr-2">{{ slotProps.option.brand.name }}</span>
                          <span
                            v-if="slotProps.option.size"
                            class="mr-2"
                            >{{ slotProps.option.size }}</span
                          >
                          <span v-if="slotProps.option.strength">{{
                            getStrengthLabel(slotProps.option.strength)
                          }}</span>
                        </div>
                      </div>
                    </div>
                  </template>
                  <template #empty>
                    <div class="flex flex-col gap-2 p-2">
                      <p class="text-stone-500 dark:text-stone-400">
                        {{ searchLoading ? 'Поиск...' : 'В справочнике нет совпадений.' }}
                      </p>
                      <Button
                        v-if="!isEditing && !searchLoading && autocompleteLastQuery.trim().length > 0"
                        data-testid="cigar-form-add-unmoderated"
                        type="button"
                        class="touch-manipulation"
                        severity="secondary"
                        outlined
                        size="small"
                        icon="pi pi-plus"
                        :label="`Добавить «${autocompleteLastQuery.trim()}» без модерации`"
                        @click="addAsUnmoderatedFromSearch" />
                      <p
                        v-if="!searchLoading && !isEditing"
                        class="text-xs text-stone-400 dark:text-stone-500">
                        Без модерации в базе; выберите бренд и при необходимости отметьте «В коллекцию».
                      </p>
                    </div>
                  </template>
                </AutoComplete>
                <small
                  v-if="errors.name"
                  class="text-sm text-red-600 dark:text-red-400"
                  >{{ errors.name }}</small
                >
              </div>

              <div class="flex flex-col gap-2">
                <label
                  for="country"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Страна
                </label>
                <InputText
                  id="country"
                  v-model="form.country"
                  data-testid="cigar-form-country"
                  class="min-h-11 w-full"
                  placeholder="Например: Куба, Доминикана" />
              </div>

              <div class="flex flex-col gap-2 md:col-span-2">
                <label
                  for="cigar-form-brand"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Бренд <span class="text-red-600 dark:text-red-400">*</span>
                </label>
                <Dropdown
                  id="cigar-form-brand"
                  data-testid="cigar-form-brand"
                  class="w-full"
                  input-class="min-h-11"
                  :model-value="form.cigar.brand?.id ?? null"
                  :options="brands"
                  option-label="name"
                  option-value="id"
                  placeholder="Выберите бренд"
                  :filter="brands.length > 8"
                  filter-placeholder="Поиск"
                  show-clear
                  :class="{ 'p-invalid': errors.brandId }"
                  :disabled="pageLoading"
                  @update:model-value="onBrandIdChange" />
                <small
                  v-if="errors.brandId"
                  class="text-sm text-red-600 dark:text-red-400"
                  >{{ errors.brandId }}</small
                >
                <small class="text-stone-500 dark:text-stone-400">
                  Нужен для сохранения в коллекции. При выборе сигары из справочника бренд подставляется автоматически.
                </small>
              </div>

              <div class="flex flex-col gap-2 md:col-span-2">
                <label
                  for="price"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Цена (₽)
                </label>
                <InputNumber
                  id="price"
                  v-model="form.price"
                  data-testid="cigar-form-price"
                  class="flex! w-full"
                  input-class="min-h-11"
                  :min-fraction-digits="2"
                  :max-fraction-digits="2"
                  placeholder="0.00"
                  suffix=" ₽"
                  fluid />
              </div>
            </div>
          </div>

          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-4 text-lg font-semibold text-stone-900 dark:text-rose-50/95">Структура сигары</h2>
            <div class="grid grid-cols-1 gap-6 md:grid-cols-3">
              <div class="flex flex-col gap-2">
                <label
                  for="wrapper"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Покровный лист
                </label>
                <InputText
                  id="wrapper"
                  v-model="form.wrapper"
                  data-testid="cigar-form-wrapper"
                  class="min-h-11 w-full"
                  placeholder="Например: Connecticut, Maduro" />
                <small class="text-stone-500 dark:text-stone-400">Внешний лист сигары</small>
              </div>

              <div class="flex flex-col gap-2">
                <label
                  for="binder"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Связующий лист
                </label>
                <InputText
                  id="binder"
                  v-model="form.binder"
                  data-testid="cigar-form-binder"
                  class="min-h-11 w-full"
                  placeholder="Например: Nicaraguan, Dominican" />
                <small class="text-stone-500 dark:text-stone-400">Средний лист</small>
              </div>

              <div class="flex flex-col gap-2">
                <label
                  for="filler"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Наполнитель
                </label>
                <InputText
                  id="filler"
                  v-model="form.filler"
                  data-testid="cigar-form-filler"
                  class="min-h-11 w-full"
                  placeholder="Например: Nicaraguan, Dominican, Cuban" />
                <small class="text-stone-500 dark:text-stone-400">Основной табак</small>
              </div>
            </div>
          </div>

          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-4 text-lg font-semibold text-stone-900 dark:text-rose-50/95">Характеристики</h2>
            <div class="grid grid-cols-1 gap-6 md:grid-cols-3">
              <div class="flex flex-col gap-2">
                <label
                  for="size"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Размер
                </label>
                <InputText
                  id="size"
                  v-model="form.size"
                  data-testid="cigar-form-size"
                  class="min-h-11 w-full"
                  placeholder="Например: 6x52" />
              </div>

              <div class="flex flex-col gap-2">
                <label
                  for="strength"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Крепость
                </label>
                <Dropdown
                  id="strength"
                  v-model="form.strength"
                  data-testid="cigar-form-strength"
                  class="w-full"
                  input-class="min-h-11"
                  :options="strengthOptions"
                  option-label="label"
                  option-value="value"
                  placeholder="Выберите крепость" />
              </div>

              <div class="flex flex-col gap-2">
                <label
                  for="rating"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Оценка
                </label>
                <Rating
                  id="rating"
                  v-model="form.rating"
                  data-testid="cigar-form-rating"
                  :stars="10"
                  :cancel="false"
                  class="w-full" />
                <small class="text-stone-500 dark:text-stone-400">От 1 до 10</small>
              </div>
            </div>
          </div>

          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-4 text-lg font-semibold text-stone-900 dark:text-rose-50/95">Описание и изображение</h2>
            <div class="flex flex-col gap-6 lg:grid lg:grid-cols-[minmax(0,14rem)_1fr] lg:items-start lg:gap-8">
              <div class="flex flex-col gap-2 lg:sticky lg:top-4">
                <span class="text-xs font-medium text-stone-600 dark:text-stone-400">Предпросмотр</span>
                <div
                  class="cigar-form-image-frame relative aspect-[4/5] w-full max-w-[14rem] min-h-0 overflow-hidden rounded-xl border border-stone-200/90 bg-stone-100/90 sm:aspect-square dark:border-stone-600/80 dark:bg-stone-900/60"
                  data-testid="cigar-form-image-preview">
                  <div
                    class="cigar-form-image-frame-inner absolute inset-0 box-border flex min-h-0 min-w-0 items-center justify-center p-2">
                    <img
                      v-if="cigarImagePreviewSrc"
                      :src="cigarImagePreviewSrc"
                      alt=""
                      loading="lazy"
                      decoding="async" />
                    <div
                      v-else
                      class="flex flex-col items-center gap-2 px-3 py-6 text-center text-stone-400 dark:text-stone-500">
                      <i
                        class="pi pi-image text-3xl opacity-70"
                        aria-hidden="true" />
                      <span class="text-xs leading-snug">Укажите URL или сохраните сигару с фото</span>
                    </div>
                  </div>
                </div>
              </div>

              <div class="flex min-w-0 flex-col gap-6">
                <div class="flex flex-col gap-2">
                  <label
                    for="description"
                    class="text-xs font-medium text-stone-600 dark:text-stone-400">
                    Описание
                  </label>
                  <Textarea
                    id="description"
                    v-model="form.description"
                    data-testid="cigar-form-description"
                    class="min-h-[6rem] w-full"
                    rows="4"
                    placeholder="Вкус, аромат, сочетания…" />
                </div>

                <div class="flex flex-col gap-2">
                  <label
                    for="imageUrl"
                    class="text-xs font-medium text-stone-600 dark:text-stone-400">
                    URL изображения
                  </label>
                  <InputText
                    id="imageUrl"
                    v-model="form.imageUrl"
                    data-testid="cigar-form-image-url"
                    class="min-h-11 w-full"
                    placeholder="https://example.com/cigar-image.jpg" />
                  <small class="text-stone-500 dark:text-stone-400">Ссылка на изображение сигары</small>
                </div>
              </div>
            </div>
          </div>

          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-4 text-lg font-semibold text-stone-900 dark:text-rose-50/95">Хранение</h2>
            <div class="flex flex-col gap-4">
              <div
                v-if="!isEditing"
                class="flex flex-col gap-2">
                <div class="flex min-h-11 items-center gap-3 touch-manipulation">
                  <Checkbox
                    id="addToCollection"
                    v-model="form.addToCollection"
                    data-testid="cigar-form-add-to-collection"
                    :binary="true"
                    input-id="addToCollection" />
                  <label
                    for="addToCollection"
                    class="cursor-pointer text-sm font-medium text-stone-800 dark:text-stone-200">
                    Добавить сигару в мою коллекцию
                  </label>
                </div>
                <small class="ml-9 text-stone-500 dark:text-stone-400 md:ml-11">
                  Отметьте, чтобы сохранить цену, рейтинг и хьюмидор в личной коллекции.
                </small>
              </div>

              <div
                v-if="(!isEditing && form.addToCollection) || (isEditing && !editingFullySmoked)"
                class="flex flex-col gap-2">
                <label
                  for="cigar-form-quantity"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Количество сигар
                </label>
                <InputNumber
                  id="cigar-form-quantity"
                  v-model="form.quantity"
                  data-testid="cigar-form-quantity"
                  class="flex! w-full"
                  input-class="min-h-11"
                  :min="1"
                  :max="10000"
                  show-buttons
                  :disabled="pageLoading"
                  fluid />
                <small class="text-stone-500 dark:text-stone-400">
                  Остаток уменьшается при отметке «прокурена» (на форме или в списке сигар).
                </small>
                <small
                  v-if="errors.quantity"
                  class="text-sm text-red-600 dark:text-red-400"
                  >{{ errors.quantity }}</small
                >
              </div>

              <div
                v-if="isEditing && editingFullySmoked"
                class="rounded-lg border border-stone-200/80 bg-stone-100/50 px-3 py-2 text-sm text-stone-600 dark:border-stone-600/80 dark:bg-stone-800/40 dark:text-stone-400">
                Выкурена (остаток 0). Количество менять нельзя.
              </div>

              <div
                v-if="isEditing && !editingFullySmoked && form.quantity > 0"
                class="flex flex-col gap-2">
                <span class="text-xs font-medium text-stone-600 dark:text-stone-400">Выкуривание</span>
                <Button
                  data-testid="cigar-form-mark-smoked"
                  type="button"
                  class="min-h-12 w-full touch-manipulation sm:min-h-11 sm:w-auto sm:self-start"
                  :label="form.quantity > 1 ? 'Прокурена (минус одна)' : 'Прокурена (последняя)'"
                  icon="pi pi-check"
                  severity="secondary"
                  outlined
                  :loading="markingSmoked"
                  :disabled="markingSmoked"
                  @click="confirmMarkOneSmoked" />
              </div>

              <div class="flex flex-col gap-2">
                <label
                  for="humidorId"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Хьюмидор
                </label>
                <Dropdown
                  id="humidorId"
                  v-model="form.humidorId"
                  data-testid="cigar-form-humidor"
                  class="w-full"
                  input-class="min-h-11"
                  :options="humidors"
                  option-label="name"
                  option-value="id"
                  placeholder="Выберите хьюмидор для хранения"
                  show-clear
                  :loading="humidorsLoading"
                  :disabled="(!isEditing && !form.addToCollection) || humidorsLoading" />
                <small class="text-stone-500 dark:text-stone-400">Оставьте пустым, если сигара не в хьюмидоре</small>
                <p
                  v-if="(isEditing || form.addToCollection) && !humidorsLoading && humidors.length === 0"
                  class="text-sm text-rose-900/90 dark:text-rose-200/80">
                  Хьюмидоров пока нет.
                  <router-link
                    class="font-medium text-rose-800 underline underline-offset-2 dark:text-rose-300"
                    :to="{ name: 'HumidorList' }">
                    Создайте хьюмидор
                  </router-link>
                  , затем обновите страницу или снова откройте форму.
                </p>
                <p
                  v-if="!isEditing && !form.addToCollection"
                  class="text-sm text-stone-500 dark:text-stone-400">
                  Чтобы назначить хьюмидор, отметьте «Добавить сигару в мою коллекцию» — список загрузится
                  автоматически.
                </p>
              </div>

              <div
                v-if="selectedHumidor"
                class="mt-1 rounded-xl border border-rose-800/15 bg-rose-50/60 p-4 dark:border-rose-200/15 dark:bg-rose-950/25">
                <h3 class="mb-2 font-semibold text-stone-900 dark:text-rose-50/95">
                  {{ selectedHumidor.name }}
                </h3>
                <div class="grid grid-cols-2 gap-3 text-sm text-stone-700 dark:text-stone-300">
                  <div>
                    <span class="font-medium text-stone-600 dark:text-stone-400">Вместимость:</span>
                    {{ selectedHumidor.capacity }} сигар
                  </div>
                  <div v-if="selectedHumidor.humidity != null">
                    <span class="font-medium text-stone-600 dark:text-stone-400">Влажность:</span>
                    {{ selectedHumidor.humidity }}%
                  </div>
                </div>
              </div>
            </div>
          </div>

          <div
            class="mt-2 flex flex-col-reverse gap-3 border-t border-stone-200/80 pt-6 dark:border-stone-700/80 sm:flex-row sm:justify-end">
            <Button
              data-testid="cigar-form-cancel"
              type="button"
              class="min-h-12 w-full touch-manipulation sm:min-h-11 sm:w-auto"
              label="Отмена"
              severity="secondary"
              outlined
              @click="handleCancel" />
            <Button
              data-testid="cigar-form-submit"
              type="submit"
              class="min-h-12 w-full touch-manipulation shadow-md shadow-rose-900/10 dark:shadow-black/40 sm:min-h-11 sm:w-auto"
              :label="isEditing ? 'Сохранить изменения' : 'Добавить сигару'"
              icon="pi pi-check"
              :loading="saving" />
          </div>
        </form>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted, watch } from 'vue';
  import { useRoute, useRouter } from 'vue-router';
  import { useToast } from 'primevue/usetoast';
  import { useConfirm } from 'primevue/useconfirm';
  import { isOfflineQueued } from '@/services/api';
  import cigarService from '@/services/cigarService';
  import humidorService from '@/services/humidorService';
  import type { Cigar, CigarBase, Brand, CigarImage } from '@/services/cigarService';
  import { arrayBufferToBase64 } from '@/utils/imageUtils';
  import type { Humidor } from '@/services/humidorService';
  import { strengthOptions } from '@/utils/cigarOptions';
  import AutoComplete, { type AutoCompleteCompleteEvent } from 'primevue/autocomplete';
  import Checkbox from 'primevue/checkbox';
  import InputText from 'primevue/inputtext';
  import InputNumber from 'primevue/inputnumber';
  import Textarea from 'primevue/textarea';
  import Dropdown from 'primevue/dropdown';
  import Rating from 'primevue/rating';
  import Button from 'primevue/button';

  interface FormData {
    cigar: Cigar;
    country: string;
    size: string;
    strength: string | null;
    rating: number;
    price: number | null;
    description: string;
    humidorId: number | null;
    imageUrl: string;
    wrapper: string;
    binder: string;
    filler: string;
    addToCollection: boolean;
    quantity: number;
  }

  interface CigarGroup {
    brand: string;
    cigars: Cigar[];
  }

  interface FormErrors {
    name?: string;
    brandId?: string;
    quantity?: string;
    [key: string]: string | undefined;
  }

  const route = useRoute();
  const router = useRouter();
  const toast = useToast();
  const confirm = useConfirm();

  const pageLoading = ref(false);
  const saving = ref(false);
  const loadError = ref<string | null>(null);
  const saveError = ref<string | null>(null);

  const humidors = ref<Humidor[]>([]);
  const humidorsLoading = ref(false);
  const brands = ref<Brand[]>([]);
  const errors = ref<FormErrors>({});
  const filteredCigars = ref<CigarGroup[]>([]);
  const selectedCigar = ref<Cigar | null>(null);
  const searchLoading = ref<boolean>(false);
  const searchCache = ref<Map<string, CigarGroup[]>>(new Map());
  const autocompleteLastQuery = ref('');
  const editingFullySmoked = ref(false);
  const markingSmoked = ref(false);

  const form = ref<FormData>({
    cigar: {} as Cigar,
    country: '',
    size: '',
    strength: null,
    rating: 0,
    price: null,
    description: '',
    humidorId: null,
    imageUrl: '',
    wrapper: '',
    binder: '',
    filler: '',
    addToCollection: false,
    quantity: 1,
  });

  const isEditing = computed<boolean>(() => !!route.params.id);

  function cigarImageRawBytes(img: CigarImage | undefined): string | number[] | undefined {
    if (!img) return undefined;
    return img.imageData ?? img.data;
  }

  /** Превью: приоритет URL в поле, иначе первое сохранённое фото (целиком в рамке, без обрезки). */
  const cigarImagePreviewSrc = computed(() => {
    const rawUrl = form.value.imageUrl?.trim();
    if (rawUrl) {
      if (/^https?:\/\//i.test(rawUrl) || rawUrl.startsWith('data:')) {
        return rawUrl;
      }
    }
    const first = form.value.cigar.images?.[0];
    const bytes = cigarImageRawBytes(first);
    const b64 = arrayBufferToBase64(bytes ?? undefined);
    return b64 ? `data:image/jpeg;base64,${b64}` : '';
  });

  const selectedHumidor = computed<Humidor | null>(() => {
    if (!form.value.humidorId) return null;
    return humidors.value.find((humidor) => humidor.id === form.value.humidorId) || null;
  });

  async function setInitialBrand(brandId: number) {
    if (brands.value.length === 0) {
      await loadBrands();
    }
    const foundBrand = brands.value.find((b) => b.id === brandId);
    if (foundBrand) {
      if (!form.value.cigar) {
        form.value.cigar = {} as Cigar;
      }
      form.value.cigar.brand = foundBrand;
    }
  }

  async function loadBrands(): Promise<void> {
    try {
      brands.value = await cigarService.getBrands();
    } catch (error) {
      if (import.meta.env.DEV) {
        console.error('Ошибка загрузки брендов:', error);
      }
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось загрузить бренды',
        life: 3000,
      });
    }
  }

  function onBrandIdChange(brandId: number | null): void {
    if (brandId == null) {
      form.value.cigar = {
        ...form.value.cigar,
        brand: { id: 0, name: '', isModerated: false, createdAt: '' },
      };
      return;
    }
    const found = brands.value.find((b) => b.id === brandId);
    if (found) {
      form.value.cigar = { ...form.value.cigar, brand: found };
    }
  }

  async function loadHumidors(): Promise<void> {
    humidorsLoading.value = true;
    try {
      humidors.value = await humidorService.getHumidors();
    } catch (error) {
      if (import.meta.env.DEV) {
        console.error('Ошибка загрузки хьюмидоров:', error);
      }
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось загрузить хьюмидоры',
        life: 3000,
      });
    } finally {
      humidorsLoading.value = false;
    }
  }

  async function loadCigar(): Promise<void> {
    if (!isEditing.value) return;

    pageLoading.value = true;
    loadError.value = null;
    try {
      const cigar = await cigarService.getCigar(route.params.id as string);

      editingFullySmoked.value = cigar.isSmoked === true;
      const q = cigar.quantity ?? (cigar.isSmoked ? 0 : 1);

      form.value = {
        cigar: {
          name: cigar.name || '',
          brand: cigar.brand,
          id: cigar.id,
          country: cigar.country || '',
          size: cigar.size || '',
          strength: cigar.strength || null,
          price: cigar.price || null,
          description: cigar.description || '',
          humidorId: cigar.humidorId || null,
          wrapper: cigar.wrapper || '',
          binder: cigar.binder || '',
          filler: cigar.filler || '',
          rating: cigar.rating ?? 0,
          images: cigar.images || [],
          isSmoked: cigar.isSmoked,
          quantity: q,
        },
        country: cigar.country || '',
        size: cigar.size || '',
        strength: cigar.strength || null,
        rating: cigar.rating ?? 0,
        price: cigar.price || null,
        description: cigar.description || '',
        humidorId: cigar.humidorId || null,
        imageUrl: cigar.images?.[0]?.imageData || '',
        wrapper: cigar.wrapper || '',
        binder: cigar.binder || '',
        filler: cigar.filler || '',
        addToCollection: false,
        quantity: q,
      };
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка загрузки сигары:', err);
      }
      loadError.value = 'Не удалось загрузить данные сигары.';
    } finally {
      pageLoading.value = false;
    }
  }

  function validateForm(): boolean {
    errors.value = {};

    if (!form.value.cigar?.name?.trim()) {
      errors.value.name = 'Название обязательно для заполнения';
    }

    if (isEditing.value || form.value.addToCollection) {
      const bid = form.value.cigar?.brand?.id;
      if (bid == null || bid <= 0) {
        errors.value.brandId = 'Выберите бренд из списка';
      }
    }

    const needQty = (!isEditing.value && form.value.addToCollection) || (isEditing.value && !editingFullySmoked.value);
    if (needQty) {
      const q = form.value.quantity;
      if (q == null || !Number.isFinite(q) || q < 1 || q > 10000) {
        errors.value.quantity = 'Укажите количество от 1 до 10000';
      }
    }

    return Object.keys(errors.value).length === 0;
  }

  async function handleSubmit(): Promise<void> {
    if (!validateForm()) return;

    saving.value = true;
    saveError.value = null;

    try {
      const cigarData: Omit<Cigar, 'id' | 'brandName'> = {
        name: form.value.cigar.name,
        brand: form.value.cigar.brand,
        country: form.value.country || null,
        description: form.value.description || null,
        strength: form.value.strength || null,
        size: form.value.size || null,
        wrapper: form.value.wrapper || null,
        binder: form.value.binder || null,
        filler: form.value.filler || null,
        images: form.value.cigar.images || [],
        price: form.value.price,
        rating: form.value.rating,
        humidorId: form.value.humidorId,
        quantity: form.value.quantity,
      };

      if (isEditing.value) {
        const updatePayload: Partial<Cigar> = { ...cigarData };
        if (editingFullySmoked.value) {
          delete updatePayload.quantity;
        }
        await cigarService.updateCigar(parseInt(route.params.id as string, 10), updatePayload, form.value.imageUrl);
        toast.add({
          severity: 'success',
          summary: 'Успешно',
          detail: 'Сигара успешно обновлена',
          life: 3000,
        });
        await router.push({ name: 'CigarList' });
      } else {
        if (form.value.addToCollection) {
          await cigarService.createCigar(cigarData, form.value.imageUrl);
          toast.add({
            severity: 'success',
            summary: 'Успешно',
            detail: 'Сигара успешно добавлена в вашу коллекцию',
            life: 3000,
          });
          await router.push({ name: 'CigarList' });
        } else {
          toast.add({
            severity: 'info',
            summary: 'Информация',
            detail:
              'Сигара добавлена только в базу данных. Для добавления в коллекцию отметьте соответствующий флажок.',
            life: 5000,
          });
          await router.push({ name: 'CigarBases' });
        }
      }
    } catch (error: unknown) {
      if (isOfflineQueued(error)) {
        await router.push({ name: 'CigarList' });
        return;
      }
      if (error instanceof Error && error.message === 'BRAND_REQUIRED') {
        errors.value.brandId = 'Выберите бренд из списка';
        saveError.value = 'Укажите бренд.';
        toast.add({
          severity: 'error',
          summary: 'Ошибка',
          detail: saveError.value,
          life: 5000,
        });
        return;
      }
      if (import.meta.env.DEV) {
        console.error('Ошибка сохранения сигары:', error);
      }
      const msg = isEditing.value ? 'Не удалось обновить сигару.' : 'Не удалось добавить сигару.';
      saveError.value = `${msg} Проверьте данные и попробуйте снова.`;
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: saveError.value,
        life: 5000,
      });
    } finally {
      saving.value = false;
    }
  }

  function handleCancel(): void {
    router.push({ name: 'CigarList' });
  }

  // eslint-disable-next-line no-unused-vars -- имя rest только в generic-ограничении
  const debounce = <T extends (...args: any[]) => any>(func: T, delay: number) => {
    let timeoutId: ReturnType<typeof setTimeout> | null = null;
    return (...params: Parameters<T>): void => {
      if (timeoutId) clearTimeout(timeoutId);
      timeoutId = setTimeout(() => {
        func(...params);
        timeoutId = null;
      }, delay);
    };
  };

  /** API ограничивает pageSize (см. CigarsController.GetCigarBasesPaginated). */
  const CIGAR_BASES_PAGE_SIZE = 100;

  const INITIAL_SUGGESTIONS_CACHE_KEY = '__initial__';

  function groupCigarBasesToOptions(items: CigarBase[]): CigarGroup[] {
    const groupedCigars: Record<string, CigarGroup> = {};
    items.forEach((cigar: CigarBase) => {
      if (!cigar || typeof cigar !== 'object') return;
      const brandName = cigar.brand?.name || 'Без бренда';
      if (!groupedCigars[brandName]) {
        groupedCigars[brandName] = { brand: brandName, cigars: [] };
      }
      groupedCigars[brandName].cigars.push({ ...cigar } as Cigar);
    });
    return Object.values(groupedCigars);
  }

  async function performSearch(query: string): Promise<void> {
    const trimmed = (query ?? '').trim();

    if (!trimmed) {
      if (searchCache.value.has(INITIAL_SUGGESTIONS_CACHE_KEY)) {
        filteredCigars.value = searchCache.value.get(INITIAL_SUGGESTIONS_CACHE_KEY) || [];
        return;
      }
      try {
        searchLoading.value = true;
        const response = await cigarService.getCigarBasesPaginated({
          page: 1,
          pageSize: CIGAR_BASES_PAGE_SIZE,
          excludeBinaryMedia: true,
        });
        if (!response?.items?.length) {
          filteredCigars.value = [];
          return;
        }
        const result = groupCigarBasesToOptions(response.items);
        searchCache.value.set(INITIAL_SUGGESTIONS_CACHE_KEY, result);
        filteredCigars.value = result;
      } catch (error) {
        if (import.meta.env.DEV) {
          console.error('Ошибка при загрузке списка сигар из базы:', error);
        }
        filteredCigars.value = [];
      } finally {
        searchLoading.value = false;
      }
      return;
    }

    const cacheKey = trimmed.toLowerCase();
    if (searchCache.value.has(cacheKey)) {
      filteredCigars.value = searchCache.value.get(cacheKey) || [];
      return;
    }

    try {
      searchLoading.value = true;
      const response = await cigarService.getCigarBasesPaginated({
        search: trimmed,
        page: 1,
        pageSize: CIGAR_BASES_PAGE_SIZE,
        excludeBinaryMedia: true,
      });

      if (!response?.items) {
        filteredCigars.value = [];
        return;
      }

      const result = groupCigarBasesToOptions(response.items);
      searchCache.value.set(cacheKey, result);
      filteredCigars.value = result;
    } catch (error) {
      if (import.meta.env.DEV) {
        console.error('Ошибка при поиске сигар:', error);
      }
      filteredCigars.value = [];
    } finally {
      searchLoading.value = false;
    }
  }

  const debouncedSearch = debounce(performSearch, 300);

  function searchCigars(event: AutoCompleteCompleteEvent): void {
    autocompleteLastQuery.value = event.query ?? '';
    debouncedSearch(event.query);
  }

  /** Плоский список сигар из текущих подсказок (группы). */
  function flattenSuggestionCigars(): Cigar[] {
    const out: Cigar[] = [];
    for (const group of filteredCigars.value) {
      if (group?.cigars?.length) {
        out.push(...group.cigars);
      }
    }
    return out;
  }

  /** Точное совпадение по имени с подсказкой из справочника (учёт регистра и trim). */
  function findCigarInFilteredSuggestionsByName(name: string): Cigar | null {
    const n = name.trim().toLowerCase();
    if (!n) {
      return null;
    }
    return flattenSuggestionCigars().find((c) => (c.name || '').trim().toLowerCase() === n) ?? null;
  }

  /** Заполнить форму данными из базы (новая сигара). */
  function applyDictionaryCigarToNewForm(selectedCigarData: Cigar | CigarBase): void {
    selectedCigar.value = selectedCigarData as Cigar;
    form.value = {
      cigar: selectedCigarData as Cigar,
      country: selectedCigarData.country || '',
      size: selectedCigarData.size || '',
      strength: selectedCigarData.strength || null,
      rating: 0,
      price: null,
      description: selectedCigarData.description || '',
      humidorId: form.value.humidorId,
      imageUrl: '',
      wrapper: selectedCigarData.wrapper || '',
      binder: selectedCigarData.binder || '',
      filler: selectedCigarData.filler || '',
      addToCollection: true,
      quantity: 1,
    };
  }

  function addAsUnmoderatedFromSearch(): void {
    const q = autocompleteLastQuery.value.trim();
    if (!q || isEditing.value) {
      return;
    }
    form.value.addToCollection = true;
    void loadHumidors();
    const prev = form.value.cigar && typeof form.value.cigar === 'object' ? form.value.cigar : ({} as Partial<Cigar>);
    form.value.cigar = {
      ...prev,
      name: q,
      id: 0,
    } as Cigar;
    selectedCigar.value = null;
    form.value.quantity = Math.max(1, form.value.quantity || 1);
    toast.add({
      severity: 'info',
      summary: 'Немодерированная позиция',
      detail: 'Выберите бренд и сохраните: новая запись попадёт в базу без модерации (как при ручном вводе названия).',
      life: 6000,
    });
  }

  function confirmMarkOneSmoked(): void {
    if (!isEditing.value || editingFullySmoked.value || form.value.quantity <= 0) {
      return;
    }
    const id = parseInt(route.params.id as string, 10);
    const leftAfter = form.value.quantity - 1;
    const msg =
      form.value.quantity > 1
        ? `Списать одну сигару? Останется ${leftAfter} шт. Запись останется в коллекции.`
        : 'Отметить последнюю сигару как выкуренную? Позиция будет помечена как выкуренная и убрана из хьюмидора.';

    confirm.require({
      message: msg,
      header: 'Подтверждение',
      icon: 'pi pi-check-circle',
      rejectClass: 'p-button-secondary p-button-outlined',
      acceptClass: 'p-button-success',
      rejectLabel: 'Отмена',
      acceptLabel: 'Списать',
      accept: async () => {
        markingSmoked.value = true;
        try {
          const updated = await cigarService.markCigarAsSmoked(id);
          form.value.quantity = updated.quantity ?? 0;
          editingFullySmoked.value = updated.isSmoked === true;
          form.value.cigar = {
            ...form.value.cigar,
            isSmoked: updated.isSmoked,
            quantity: updated.quantity,
            humidorId: updated.humidorId ?? null,
          };
          form.value.humidorId = updated.humidorId ?? null;
          toast.add({
            severity: 'success',
            summary: 'Готово',
            detail:
              updated.isSmoked === true
                ? 'Сигара полностью выкурена'
                : `Списана одна сигара, осталось ${updated.quantity ?? form.value.quantity} шт.`,
            life: 4000,
          });
        } catch (err) {
          if (isOfflineQueued(err)) {
            return;
          }
          if (import.meta.env.DEV) {
            console.error('Ошибка отметки выкуривания:', err);
          }
          toast.add({
            severity: 'error',
            summary: 'Ошибка',
            detail: 'Не удалось отметить сигару',
            life: 4000,
          });
        } finally {
          markingSmoked.value = false;
        }
      },
    });
  }

  /**
   * PrimeVue AutoComplete при typeahead на каждый ввод выставляет model в строку — после выбора объекта
   * из списка это затирает бренд. Нормализуем: строка → ищем полный объект в подсказках или сохраняем прошлый brand.
   */
  function onCigarAutocompleteModelUpdate(val: unknown): void {
    if (isEditing.value) {
      if (val == null || val === '') {
        return;
      }
      if (typeof val === 'string') {
        form.value.cigar = { ...form.value.cigar, name: val } as Cigar;
        return;
      }
      if (typeof val === 'object' && val !== null && 'name' in val) {
        const o = val as Cigar;
        const match = findCigarInFilteredSuggestionsByName((o.name || '').trim());
        if (match?.brand) {
          form.value.cigar = { ...form.value.cigar, name: match.name, brand: match.brand } as Cigar;
        } else if (o.brand) {
          form.value.cigar = { ...form.value.cigar, name: o.name, brand: o.brand } as Cigar;
        }
      }
      return;
    }

    if (val == null || val === '') {
      form.value.cigar = {} as Cigar;
      selectedCigar.value = null;
      form.value.quantity = 1;
      return;
    }

    if (typeof val === 'string') {
      const resolved = findCigarInFilteredSuggestionsByName(val);
      if (resolved) {
        applyDictionaryCigarToNewForm(resolved);
        return;
      }
      const prev = form.value.cigar && typeof form.value.cigar === 'object' ? form.value.cigar : ({} as Partial<Cigar>);
      form.value.cigar = {
        ...prev,
        name: val,
      } as Cigar;
      return;
    }

    if (typeof val === 'object' && val !== null && 'name' in val) {
      const o = val as Cigar;
      const match = findCigarInFilteredSuggestionsByName((o.name || '').trim());
      applyDictionaryCigarToNewForm(match ?? o);
    }
  }

  function getStrengthLabel(strength: string): string {
    if (!strength) return '';

    const option = strengthOptions.find((opt) => opt.value === strength);
    return option ? option.label : strength;
  }

  watch(
    () => form.value.addToCollection,
    (on) => {
      if (on) {
        void loadHumidors();
      }
    },
  );

  onMounted(() => {
    void loadBrands();
    void loadHumidors();
    void loadCigar();

    const initialBrandId = route.query.brandId ? parseInt(route.query.brandId as string, 10) : null;

    if (!isEditing.value && Object.keys(route.query).length > 0) {
      if (initialBrandId) {
        void setInitialBrand(initialBrandId);
      }
      const cigarObj: Partial<Cigar> = {
        name: (route.query.name as string) || '',
      };

      form.value.cigar = { ...form.value.cigar, ...cigarObj } as Cigar;

      if (route.query.country) {
        form.value.country = route.query.country as string;
      }
      if (route.query.size) {
        form.value.size = route.query.size as string;
      }
      if (route.query.strength) {
        form.value.strength = route.query.strength as string;
      }
      if (route.query.description) {
        form.value.description = route.query.description as string;
      }
      if (route.query.wrapper) {
        form.value.wrapper = route.query.wrapper as string;
      }
      if (route.query.binder) {
        form.value.binder = route.query.binder as string;
      }
      if (route.query.filler) {
        form.value.filler = route.query.filler as string;
      }

      if (route.query.cigarId) {
        form.value.addToCollection = true;
      }
    }
  });
</script>

<style scoped>
  .cigar-form-root {
    position: relative;
    isolation: isolate;
  }

  /* Вписываем превью целиком: flex+h-full на img часто даёт обрезку; здесь явный contain + auto размеры */
  .cigar-form-image-frame img {
    display: block;
    width: auto;
    height: auto;
    min-width: 0;
    min-height: 0;
    max-width: 100%;
    max-height: 100%;
    object-fit: contain;
    object-position: center;
  }

  .cigar-form-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  /*:global(.dark) .cigar-form-grain {
    mix-blend-mode: soft-light;
  }*/

  .cigar-form-enter {
    animation: cigar-form-in 0.45s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes cigar-form-in {
    from {
      opacity: 0;
      transform: translateY(8px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  @media (prefers-reduced-motion: reduce) {
    .cigar-form-enter {
      animation: none;
    }
  }
</style>
