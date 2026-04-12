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
            Справочник → коллекция
          </p>
          <h1
            id="cigar-form-heading"
            class="text-balance text-3xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 sm:text-4xl">
            Добавить в коллекцию
          </h1>
          <p class="mt-1.5 max-w-xl text-pretty text-sm text-stone-600 dark:text-stone-400">
            Выберите сигару из справочника или добавьте новую через список под полем (пустой поиск) или если ничего не
            нашлось. Затем укажите цену, количество, оценку, вкус и аромат, при необходимости фото и хьюмидор.
          </p>
        </div>
        <Button
          data-testid="cigar-form-back"
          class="min-h-12 w-full shrink-0 touch-manipulation sm:min-h-11 sm:w-auto"
          icon="pi pi-arrow-left"
          label="К списку сигар"
          severity="secondary"
          outlined
          @click="router.push({ name: 'CigarList' })" />
      </header>

      <div
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
            <h2 class="mb-4 text-lg font-semibold text-stone-900 dark:text-rose-50/95">Карточка сигары</h2>
            <div class="grid grid-cols-1 gap-6 md:grid-cols-2">
              <div class="flex flex-col gap-2 md:col-span-2">
                <label
                  for="name"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Название <span class="text-red-600 dark:text-red-400">*</span>
                </label>
                <AutoComplete
                  id="name"
                  data-testid="cigar-form-name"
                  :model-value="selectedBase"
                  :suggestions="filteredCigars"
                  class="w-full"
                  input-class="min-h-11 w-full"
                  :class="{ 'p-invalid': errors.base }"
                  data-key="id"
                  option-label="name"
                  option-group-label="brand"
                  option-group-children="cigars"
                  :dropdown="true"
                  :virtual-scroller-options="{ itemSize: 50 }"
                  :loading="searchLoading"
                  :delay="300"
                  :min-length="0"
                  placeholder="Выберите сигару из справочника"
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
                      <span class="text-stone-500 dark:text-stone-400">
                        {{ searchLoading ? 'Поиск…' : 'Сигары не найдены.' }}
                      </span>
                      <Button
                        v-if="!searchLoading && lastAutocompleteQueryTrimmed.length > 0"
                        data-testid="cigar-form-add-new-empty"
                        type="button"
                        class="min-h-10 w-full touch-manipulation justify-center text-left"
                        severity="secondary"
                        outlined
                        :label="addNewFromSearchButtonLabel"
                        icon="pi pi-plus"
                        @click="openNewCigarDialog(lastAutocompleteQueryTrimmed)" />
                    </div>
                  </template>
                  <template #footer>
                    <div
                      v-if="!searchLoading && lastAutocompleteQueryTrimmed.length === 0"
                      class="border-t border-stone-200/90 p-2 dark:border-stone-600/80">
                      <Button
                        data-testid="cigar-form-add-new-footer"
                        type="button"
                        class="min-h-10 w-full touch-manipulation justify-center"
                        label="Добавить новую сигару в справочник…"
                        icon="pi pi-plus"
                        severity="secondary"
                        outlined
                        @click="openNewCigarDialog()" />
                      <p class="mt-1.5 px-0.5 text-xs text-stone-500 dark:text-stone-400">
                        Карточка сохранится как непромодерированная; после проверки она появится в общем каталоге.
                      </p>
                    </div>
                  </template>
                </AutoComplete>
                <small
                  v-if="errors.base"
                  class="text-sm text-red-600 dark:text-red-400"
                  >{{ errors.base }}</small
                >
              </div>

              <div class="flex flex-col gap-2 md:col-span-2">
                <label
                  for="cigar-form-brand-readonly"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Бренд
                </label>
                <InputText
                  id="cigar-form-brand-readonly"
                  data-testid="cigar-form-brand"
                  class="min-h-11 w-full"
                  readonly
                  :model-value="selectedBase?.brand?.name ?? ''"
                  placeholder="Появится после выбора сигары" />
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

              <div class="flex flex-col gap-2 md:col-span-2">
                <label
                  for="cigar-form-quantity"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Количество (шт.)
                </label>
                <InputNumber
                  id="cigar-form-quantity"
                  v-model="form.quantity"
                  data-testid="cigar-form-quantity"
                  class="flex! w-full"
                  input-class="min-h-11"
                  :min="1"
                  :max="9999"
                  :step="1"
                  show-buttons
                  button-layout="horizontal"
                  fluid />
              </div>

              <div class="flex flex-col gap-2 md:col-span-2">
                <label
                  for="cigar-form-rating"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Оценка
                </label>
                <div class="flex flex-col gap-2 sm:flex-row sm:flex-wrap sm:items-center sm:gap-4">
                  <Rating
                    id="cigar-form-rating"
                    :model-value="form.rating ?? undefined"
                    data-testid="cigar-form-rating"
                    :stars="10"
                    @update:model-value="(v) => (form.rating = v ?? null)" />
                  <span class="text-sm text-stone-600 dark:text-stone-400">
                    {{ form.rating != null ? `${form.rating}/10` : 'Без оценки' }}
                  </span>
                </div>
                <small class="text-stone-500 dark:text-stone-400">
                  Повторный клик по выбранной звезде снимает оценку.
                </small>
              </div>

              <div class="flex flex-col gap-2">
                <label
                  for="taste"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Вкус
                </label>
                <InputText
                  id="taste"
                  v-model="form.taste"
                  data-testid="cigar-form-taste"
                  class="min-h-11 w-full"
                  placeholder="Ваши заметки о вкусе" />
              </div>

              <div class="flex flex-col gap-2">
                <label
                  for="aroma"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Аромат
                </label>
                <InputText
                  id="aroma"
                  v-model="form.aroma"
                  data-testid="cigar-form-aroma"
                  class="min-h-11 w-full"
                  placeholder="Ваши заметки об аромате" />
              </div>
            </div>
          </div>

          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-4 text-lg font-semibold text-stone-900 dark:text-rose-50/95">Фото</h2>
            <div class="flex flex-col gap-6 lg:grid lg:grid-cols-[minmax(0,14rem)_1fr] lg:items-start lg:gap-8">
              <div class="flex flex-col gap-2 lg:sticky lg:top-4">
                <span class="text-xs font-medium text-stone-600 dark:text-stone-400">Предпросмотр</span>
                <div
                  class="cigar-form-image-frame relative aspect-[4/5] w-full max-w-[14rem] min-h-0 overflow-hidden rounded-xl border border-stone-200/90 bg-stone-100/90 sm:aspect-square dark:border-stone-600/80 dark:bg-stone-900/60"
                  data-testid="cigar-form-image-preview">
                  <div
                    class="cigar-form-image-frame-inner absolute inset-0 box-border flex min-h-0 min-w-0 items-center justify-center p-2">
                    <img
                      v-if="previewUrl"
                      :src="previewUrl"
                      alt=""
                      loading="lazy"
                      decoding="async" />
                    <div
                      v-else
                      class="flex flex-col items-center gap-2 px-3 py-6 text-center text-stone-400 dark:text-stone-500">
                      <i
                        class="pi pi-image text-3xl opacity-70"
                        aria-hidden="true" />
                      <span class="text-xs leading-snug">Здесь будет превью первого кадра галереи</span>
                    </div>
                  </div>
                </div>
              </div>

              <FormImageGallerySection
                v-model="cigarFormImages"
                variant="bare"
                tone="review"
                url-entry-mode="multi"
                :max-files="maxNewImageUrls"
                :max-url-rows="maxNewImageUrls"
                test-id="cigar-form-image-gallery"
                url-input-id="cigar-form-gallery-url"
                url-field-test-id="cigar-form-image-url"
                url-rows-test-id="cigar-form-image-urls"
                add-url-row-test-id="cigar-form-add-image-url"
                apply-urls-to-gallery-test-id="cigar-form-apply-image-gallery"
                url-placeholder="https://example.com/cigar.jpg"
                url-help-text="До двенадцати кадров: ссылки через «Добавить в галерею» или файлы ниже."
                url-help-detail="Порядок на превью сохраняется; первое фото обычно показывается как главное."
                grid-class="grid grid-cols-2 gap-3 sm:grid-cols-3 sm:gap-4 md:grid-cols-4" />
            </div>
          </div>

          <div
            class="rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6">
            <h2 class="mb-4 text-lg font-semibold text-stone-900 dark:text-rose-50/95">Хранение</h2>
            <div class="flex flex-col gap-4">
              <div class="flex min-h-11 flex-col gap-2 touch-manipulation sm:flex-row sm:items-center">
                <Checkbox
                  id="addToHumidor"
                  v-model="form.addToHumidor"
                  data-testid="cigar-form-add-to-humidor"
                  :binary="true"
                  input-id="addToHumidor" />
                <label
                  for="addToHumidor"
                  class="cursor-pointer text-sm font-medium text-stone-800 dark:text-stone-200 sm:ml-1">
                  Добавить в хьюмидор
                </label>
              </div>
              <small class="text-stone-500 dark:text-stone-400 sm:ml-9">
                Если отмечено, выберите хьюмидор ниже. Сигара всё равно сохраняется в коллекции.
              </small>

              <div class="flex flex-col gap-2">
                <label
                  for="humidorId"
                  class="text-xs font-medium text-stone-600 dark:text-stone-400">
                  Хьюмидор
                </label>
                <Select
                  label-id="humidorId"
                  v-model="form.humidorId"
                  data-testid="cigar-form-humidor"
                  class="w-full"
                  label-class="min-h-11"
                  :options="humidors"
                  option-label="name"
                  option-value="id"
                  placeholder="Выберите хьюмидор"
                  show-clear
                  :loading="humidorsLoading"
                  :disabled="!form.addToHumidor || humidorsLoading" />
                <p
                  v-if="form.addToHumidor && !humidorsLoading && humidors.length === 0"
                  class="text-sm text-rose-900/90 dark:text-rose-200/80">
                  Хьюмидоров пока нет.
                  <RouterLink
                    class="font-medium text-rose-800 underline underline-offset-2 dark:text-rose-300"
                    :to="{ name: 'HumidorList' }">
                    Создайте хьюмидор
                  </RouterLink>
                  .
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
              label="Сохранить в коллекции"
              icon="pi pi-check"
              :loading="saving" />
          </div>
        </form>
      </div>
    </div>

    <Dialog
      v-model:visible="newCigarDialogVisible"
      header="Новая сигара в справочнике"
      modal
      :style="{ width: 'min(48rem, 92vw)' }"
      :content-style="{ maxHeight: 'min(85vh, 720px)', overflow: 'auto' }"
      :dismissable-mask="true"
      :closable="!newCigarSaving"
      data-testid="cigar-form-new-base-dialog"
      @hide="resetNewCigarDialog">
      <div class="flex flex-col gap-6 pt-1">
        <div class="grid grid-cols-1 gap-6 lg:grid-cols-2">
          <div class="flex flex-col gap-4">
            <h3 class="text-sm font-semibold text-stone-800 dark:text-stone-200">Основное</h3>
            <div class="flex flex-col gap-2">
              <label
                for="cigar-form-dialog-name"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Название <span class="text-red-600 dark:text-red-400">*</span>
              </label>
              <InputText
                id="cigar-form-dialog-name"
                v-model="draftName"
                data-testid="cigar-form-dialog-name"
                class="min-h-11 w-full"
                :class="{ 'p-invalid': dialogErrors.name }"
                maxlength="100"
                placeholder="Как на этикетке или в каталоге"
                autocomplete="off" />
              <small
                v-if="dialogErrors.name"
                class="text-sm text-red-600 dark:text-red-400"
                >{{ dialogErrors.name }}</small
              >
            </div>
            <div class="flex flex-col gap-2">
              <label
                for="cigar-form-dialog-brand"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Бренд <span class="text-red-600 dark:text-red-400">*</span>
              </label>
              <Select
                id="cigar-form-dialog-brand"
                v-model="draftBrandId"
                data-testid="cigar-form-dialog-brand"
                class="w-full"
                label-class="min-h-11"
                :options="catalogBrands"
                option-label="name"
                option-value="id"
                placeholder="Выберите бренд"
                show-clear
                filter
                :loading="brandsLoading"
                :class="{ 'p-invalid': dialogErrors.brand }" />
              <small
                v-if="dialogErrors.brand"
                class="text-sm text-red-600 dark:text-red-400"
                >{{ dialogErrors.brand }}</small
              >
              <small class="text-stone-500 dark:text-stone-400">
                Только промодерированные бренды. Нет нужного —
                <RouterLink
                  class="font-medium text-rose-800 underline underline-offset-2 dark:text-rose-300"
                  :to="{ name: 'CigarBases' }">
                  база сигар
                </RouterLink>
                .
              </small>
            </div>
            <div class="flex flex-col gap-2">
              <label
                for="cigar-form-dialog-country"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Страна
              </label>
              <InputText
                id="cigar-form-dialog-country"
                v-model="draftCountry"
                data-testid="cigar-form-dialog-country"
                class="min-h-11 w-full"
                maxlength="100"
                placeholder="Необязательно"
                autocomplete="off" />
            </div>
            <div class="flex flex-col gap-2">
              <label
                for="cigar-form-dialog-size"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Размер (vitola)
              </label>
              <InputText
                id="cigar-form-dialog-size"
                v-model="draftSize"
                data-testid="cigar-form-dialog-size"
                class="min-h-11 w-full"
                maxlength="50"
                placeholder="Например 6×50 или Robusto"
                autocomplete="off" />
            </div>
            <div class="flex flex-col gap-2">
              <label
                for="cigar-form-dialog-strength"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Крепость
              </label>
              <Select
                id="cigar-form-dialog-strength"
                v-model="draftStrength"
                data-testid="cigar-form-dialog-strength"
                class="w-full"
                label-class="min-h-11"
                :options="strengthOptions"
                option-label="label"
                option-value="value"
                placeholder="Не указано"
                show-clear />
            </div>
          </div>

          <div class="flex flex-col gap-4">
            <h3 class="text-sm font-semibold text-stone-800 dark:text-stone-200">Структура</h3>
            <div class="flex flex-col gap-2">
              <label
                for="cigar-form-dialog-wrapper"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Покровный лист (wrapper)
              </label>
              <InputText
                id="cigar-form-dialog-wrapper"
                v-model="draftWrapper"
                data-testid="cigar-form-dialog-wrapper"
                class="min-h-11 w-full"
                maxlength="100"
                placeholder="Необязательно"
                autocomplete="off" />
            </div>
            <div class="flex flex-col gap-2">
              <label
                for="cigar-form-dialog-binder"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Связующий лист (binder)
              </label>
              <InputText
                id="cigar-form-dialog-binder"
                v-model="draftBinder"
                data-testid="cigar-form-dialog-binder"
                class="min-h-11 w-full"
                maxlength="100"
                placeholder="Необязательно"
                autocomplete="off" />
            </div>
            <div class="flex flex-col gap-2">
              <label
                for="cigar-form-dialog-filler"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Наполнитель (filler)
              </label>
              <InputText
                id="cigar-form-dialog-filler"
                v-model="draftFiller"
                data-testid="cigar-form-dialog-filler"
                class="min-h-11 w-full"
                maxlength="100"
                placeholder="Необязательно"
                autocomplete="off" />
            </div>
            <div class="flex flex-col gap-2">
              <label
                for="cigar-form-dialog-description"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Описание
              </label>
              <Textarea
                id="cigar-form-dialog-description"
                v-model="draftDescription"
                data-testid="cigar-form-dialog-description"
                class="w-full min-h-[6rem]"
                :rows="4"
                :auto-resize="true"
                maxlength="500"
                placeholder="Необязательно" />
            </div>
          </div>
        </div>

        <div class="flex flex-col gap-2">
          <h3 class="text-sm font-semibold text-stone-800 dark:text-stone-200">Фото карточки справочника</h3>
          <p class="text-xs text-stone-500 dark:text-stone-400">
            До пяти кадров: файлы с устройства или ссылки (http/https); сервер скачивает картинки по ссылкам при
            сохранении.
          </p>
          <p class="text-xs text-stone-600 dark:text-stone-400">
            {{ CIGAR_BASE_CATALOG_PHOTO_HINT }}
          </p>
          <FormImageGallerySection
            v-model="draftBaseImages"
            variant="bare"
            tone="dialog"
            url-entry-mode="multi"
            show-main-image-star
            :max-files="5"
            :max-url-rows="5"
            test-id="cigar-form-dialog-base-gallery"
            url-input-id="cigar-form-dialog-base-gallery-url"
            url-rows-test-id="cigar-form-dialog-base-gallery-urls"
            add-url-row-test-id="cigar-form-dialog-base-add-url-row"
            apply-urls-to-gallery-test-id="cigar-form-dialog-base-apply-gallery"
            url-help-text="Файлы ниже или ссылки через «Добавить в галерею» (до пяти кадров)."
            url-help-detail="Порядок в галерее сохраняется; звёздочка — главное фото карточки."
            grid-class="grid grid-cols-2 gap-3 sm:grid-cols-3 sm:gap-4" />
        </div>
      </div>
      <template #footer>
        <div class="flex flex-col-reverse gap-2 sm:flex-row sm:justify-end">
          <Button
            type="button"
            label="Отмена"
            severity="secondary"
            outlined
            class="min-h-11"
            :disabled="newCigarSaving"
            @click="newCigarDialogVisible = false" />
          <Button
            type="button"
            label="Добавить в справочник и выбрать"
            icon="pi pi-check"
            class="min-h-11"
            :loading="newCigarSaving"
            data-testid="cigar-form-dialog-submit"
            @click="submitNewCigarToCatalog" />
        </div>
      </template>
    </Dialog>
  </section>
</template>

<script setup lang="ts">
  import { ref, computed, onMounted, watch } from 'vue';
  import { RouterLink, useRoute, useRouter } from 'vue-router';
  import { useToast } from 'primevue/usetoast';
  import cigarService from '@/services/cigarService';
  import humidorService from '@/services/humidorService';
  import type { CigarBase, Brand } from '@/services/cigarService';
  import type { Humidor } from '@/services/humidorService';
  import { strengthOptions } from '@/utils/cigarOptions';
  import AutoComplete, { type AutoCompleteCompleteEvent } from 'primevue/autocomplete';
  import Checkbox from 'primevue/checkbox';
  import InputText from 'primevue/inputtext';
  import InputNumber from 'primevue/inputnumber';
  import Rating from 'primevue/rating';
  import Select from 'primevue/select';
  import Button from 'primevue/button';
  import Dialog from 'primevue/dialog';
  import Message from 'primevue/message';
  import Textarea from 'primevue/textarea';
  import FormImageGallerySection, { type FormGalleryImageItem } from '@/components/FormImageGallerySection.vue';
  import { CIGAR_BASE_CATALOG_PHOTO_HINT } from '@/constants/cigarBaseCatalogPhotoHint';

  interface FormData {
    price: number | null;
    quantity: number;
    rating: number | null;
    taste: string;
    aroma: string;
    humidorId: number | null;
    addToHumidor: boolean;
  }

  interface CigarGroup {
    brand: string;
    cigars: CigarBase[];
  }

  interface FormErrors {
    base?: string;
  }

  interface NewCigarDialogErrors {
    name?: string;
    brand?: string;
  }

  const route = useRoute();
  const router = useRouter();
  const toast = useToast();

  const saving = ref(false);
  const saveError = ref<string | null>(null);

  const humidors = ref<Humidor[]>([]);
  const humidorsLoading = ref(false);
  const errors = ref<FormErrors>({});
  const filteredCigars = ref<CigarGroup[]>([]);
  const selectedBase = ref<CigarBase | null>(null);
  const searchLoading = ref<boolean>(false);
  const searchCache = ref<Map<string, CigarGroup[]>>(new Map());
  const lastAutocompleteQuery = ref('');

  const catalogBrands = ref<Brand[]>([]);
  const brandsLoading = ref(false);
  const brandsLoaded = ref(false);

  const newCigarDialogVisible = ref(false);
  const newCigarSaving = ref(false);
  const draftName = ref('');
  const draftBrandId = ref<number | null>(null);
  const draftStrength = ref<string | null>(null);
  const draftSize = ref('');
  const draftCountry = ref('');
  const draftWrapper = ref('');
  const draftBinder = ref('');
  const draftFiller = ref('');
  const draftDescription = ref('');
  const draftBaseImages = ref<FormGalleryImageItem[]>([]);
  const dialogErrors = ref<NewCigarDialogErrors>({});

  const maxNewImageUrls = 12;

  const cigarFormImages = ref<FormGalleryImageItem[]>([]);

  const form = ref<FormData>({
    price: null,
    quantity: 1,
    rating: null,
    taste: '',
    aroma: '',
    humidorId: null,
    addToHumidor: false,
  });

  const previewUrl = computed(() => {
    for (const img of cigarFormImages.value) {
      if (img.markedForDeletion) continue;
      const p = img.preview?.trim() ?? '';
      if (p && (/^https?:\/\//i.test(p) || p.startsWith('data:') || p.startsWith('blob:'))) {
        return p;
      }
    }
    return '';
  });

  const selectedHumidor = computed<Humidor | null>(() => {
    if (!form.value.humidorId) return null;
    return humidors.value.find((h) => h.id === form.value.humidorId) || null;
  });

  const lastAutocompleteQueryTrimmed = computed(() => (lastAutocompleteQuery.value ?? '').trim());

  const addNewFromSearchButtonLabel = computed(() => {
    const q = lastAutocompleteQueryTrimmed.value;
    if (!q) return 'Добавить в справочник';
    const short = q.length > 36 ? `${q.slice(0, 35)}…` : q;
    return `Добавить «${short}» в справочник`;
  });

  function readFileAsDataUrl(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = () => reject(reader.error ?? new Error('FileReader'));
      reader.readAsDataURL(file);
    });
  }

  async function collectImagePayloadUrls(): Promise<string[]> {
    const out: string[] = [];
    for (const img of cigarFormImages.value) {
      if (img.markedForDeletion) continue;
      let url = img.imageUrl?.trim();
      if (img.file) {
        url = await readFileAsDataUrl(img.file);
      }
      if (url) {
        out.push(url);
      }
    }
    return out;
  }

  async function ensureBrandsLoaded(): Promise<void> {
    if (brandsLoaded.value) return;
    brandsLoading.value = true;
    try {
      catalogBrands.value = await cigarService.getAllBrands();
      brandsLoaded.value = true;
    } catch {
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось загрузить бренды',
        life: 3000,
      });
    } finally {
      brandsLoading.value = false;
    }
  }

  function openNewCigarDialog(namePrefill?: string): void {
    draftName.value = (namePrefill ?? '').trim();
    draftBrandId.value = null;
    draftStrength.value = null;
    draftSize.value = '';
    draftCountry.value = '';
    draftWrapper.value = '';
    draftBinder.value = '';
    draftFiller.value = '';
    draftDescription.value = '';
    draftBaseImages.value = [];
    dialogErrors.value = {};
    void ensureBrandsLoaded();
    newCigarDialogVisible.value = true;
  }

  function resetNewCigarDialog(): void {
    draftName.value = '';
    draftBrandId.value = null;
    draftStrength.value = null;
    draftSize.value = '';
    draftCountry.value = '';
    draftWrapper.value = '';
    draftBinder.value = '';
    draftFiller.value = '';
    draftDescription.value = '';
    draftBaseImages.value = [];
    dialogErrors.value = {};
  }

  function validateNewCigarDialog(): boolean {
    dialogErrors.value = {};
    if (!draftName.value?.trim()) {
      dialogErrors.value.name = 'Укажите название.';
    }
    if (draftBrandId.value == null) {
      dialogErrors.value.brand = 'Выберите бренд.';
    }
    return Object.keys(dialogErrors.value).length === 0;
  }

  function buildDraftCigarBaseFormData(): FormData {
    const fd = new FormData();
    fd.append('Name', draftName.value.trim());
    fd.append('BrandId', String(draftBrandId.value!));
    const c = draftCountry.value?.trim();
    if (c) fd.append('Country', c);
    const st = draftStrength.value?.trim();
    if (st) fd.append('Strength', st);
    const sz = draftSize.value?.trim();
    if (sz) fd.append('Size', sz);
    const w = draftWrapper.value?.trim();
    if (w) fd.append('Wrapper', w);
    const b = draftBinder.value?.trim();
    if (b) fd.append('Binder', b);
    const f = draftFiller.value?.trim();
    if (f) fd.append('Filler', f);
    const d = draftDescription.value?.trim();
    if (d) fd.append('Description', d);

    const active = draftBaseImages.value.filter((img) => !img.markedForDeletion);
    let fileIndex = 0;
    let urlIndex = 0;
    for (const img of active) {
      if (img.file) {
        fd.append(`NewImages[${fileIndex}].File`, img.file);
        fd.append(`NewImages[${fileIndex}].IsMain`, String(img.isMain ?? false));
        fileIndex++;
        continue;
      }
      const rawUrl = img.imageUrl?.trim();
      if (rawUrl && /^https?:\/\//i.test(rawUrl)) {
        fd.append(`ImageUrls[${urlIndex}]`, rawUrl);
        fd.append(`ImageUrlIsMain[${urlIndex}]`, String(img.isMain ?? false));
        urlIndex++;
      }
    }

    return fd;
  }

  async function submitNewCigarToCatalog(): Promise<void> {
    if (!validateNewCigarDialog()) return;
    newCigarSaving.value = true;
    try {
      const created = await cigarService.createCigarBase(buildDraftCigarBaseFormData());
      selectedBase.value = created;
      newCigarDialogVisible.value = false;
      searchCache.value = new Map();
      toast.add({
        severity: 'success',
        summary: 'Справочник',
        detail: 'Сигара добавлена и выбрана. Заполните поля коллекции и сохраните.',
        life: 4000,
      });
    } catch {
      toast.add({
        severity: 'error',
        summary: 'Ошибка',
        detail: 'Не удалось создать карточку. Проверьте данные или попробуйте позже.',
        life: 5000,
      });
    } finally {
      newCigarSaving.value = false;
    }
  }

  async function loadHumidors(): Promise<void> {
    humidorsLoading.value = true;
    try {
      humidors.value = await humidorService.getHumidors();
    } catch {
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

  function validateForm(): boolean {
    errors.value = {};
    const id = selectedBase.value?.id;
    if (id == null || id <= 0 || !selectedBase.value?.name?.trim()) {
      errors.value.base = 'Выберите сигару из справочника или добавьте новую через список.';
    }
    return Object.keys(errors.value).length === 0;
  }

  async function handleSubmit(): Promise<void> {
    if (!validateForm()) return;
    saving.value = true;
    saveError.value = null;
    try {
      const cigarBaseId = selectedBase.value!.id;
      const urls = await collectImagePayloadUrls();
      await cigarService.createCigar({
        cigarBaseId,
        price: form.value.price,
        quantity: form.value.quantity ?? 1,
        humidorId: form.value.addToHumidor ? form.value.humidorId : null,
        taste: form.value.taste || null,
        aroma: form.value.aroma || null,
        rating: form.value.rating,
        imageUrls: urls.length > 0 ? urls : null,
      });
      toast.add({
        severity: 'success',
        summary: 'Готово',
        detail: 'Сигара добавлена в коллекцию',
        life: 3000,
      });
      await router.push({ name: 'CigarList' });
    } catch {
      saveError.value = 'Не удалось добавить сигару. Проверьте данные и попробуйте снова.';
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

  const CIGAR_BASES_PAGE_SIZE = 100;
  const INITIAL_SUGGESTIONS_CACHE_KEY = '__initial__';

  function groupCigarBasesToOptions(items: CigarBase[]): CigarGroup[] {
    const grouped: Record<string, CigarGroup> = {};
    items.forEach((cigar: CigarBase) => {
      if (!cigar || typeof cigar !== 'object') return;
      const brandName = cigar.brand?.name || 'Без бренда';
      if (!grouped[brandName]) {
        grouped[brandName] = { brand: brandName, cigars: [] };
      }
      grouped[brandName].cigars.push({ ...cigar });
    });
    return Object.values(grouped);
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
      } catch {
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
    } catch {
      filteredCigars.value = [];
    } finally {
      searchLoading.value = false;
    }
  }

  const debouncedSearch = debounce(performSearch, 300);

  function searchCigars(event: AutoCompleteCompleteEvent): void {
    lastAutocompleteQuery.value = event.query ?? '';
    debouncedSearch(event.query);
  }

  function flattenSuggestionCigars(): CigarBase[] {
    const out: CigarBase[] = [];
    for (const group of filteredCigars.value) {
      if (group?.cigars?.length) {
        out.push(...group.cigars);
      }
    }
    return out;
  }

  function findBaseByName(name: string): CigarBase | null {
    const n = name.trim().toLowerCase();
    if (!n) return null;
    return flattenSuggestionCigars().find((c) => (c.name || '').trim().toLowerCase() === n) ?? null;
  }

  function onCigarAutocompleteModelUpdate(val: unknown): void {
    if (val == null || val === '') {
      selectedBase.value = null;
      return;
    }
    if (typeof val === 'string') {
      const resolved = findBaseByName(val);
      selectedBase.value = resolved;
      return;
    }
    if (typeof val === 'object' && val !== null && 'id' in val && 'name' in val) {
      const o = val as CigarBase;
      const match = findBaseByName((o.name || '').trim());
      selectedBase.value = match ?? o;
    }
  }

  function getStrengthLabel(strength: string): string {
    if (!strength) return '';
    const option = strengthOptions.find((opt) => opt.value === strength);
    return option ? option.label : strength;
  }

  watch(
    () => form.value.addToHumidor,
    (on) => {
      if (on) {
        void loadHumidors();
      } else {
        form.value.humidorId = null;
      }
    },
  );

  async function applyQueryCigarBase(id: number): Promise<void> {
    try {
      const base = await cigarService.getCigarBase(id);
      selectedBase.value = base;
    } catch {
      toast.add({
        severity: 'warn',
        summary: 'Справочник',
        detail: 'Не удалось подгрузить сигару по ссылке.',
        life: 4000,
      });
    }
  }

  onMounted(() => {
    void loadHumidors();
    const q = route.query.cigarBaseId as string | undefined;
    if (q) {
      const id = parseInt(q, 10);
      if (!Number.isNaN(id)) {
        void applyQueryCigarBase(id);
      }
    }
  });
</script>

<style scoped>
  .cigar-form-root {
    position: relative;
    isolation: isolate;
  }

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
