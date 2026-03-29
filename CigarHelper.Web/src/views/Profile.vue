<template>
  <section
    class="profile-root -mx-2 sm:mx-0 rounded-2xl sm:rounded-3xl bg-gradient-to-b from-stone-50 via-rose-50/40 to-stone-50 px-3 py-6 ring-1 ring-stone-900/5 dark:from-stone-950 dark:via-rose-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-8"
    data-testid="profile"
    aria-labelledby="profile-heading">
    <div class="profile-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto max-w-3xl">
      <header class="pb-6 sm:pb-8">
        <div class="min-w-0">
          <p
            class="mb-1.5 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55">
            Аккаунт
          </p>
          <h1
            id="profile-heading"
            class="text-3xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 text-balance sm:text-4xl">
            Профиль
          </h1>
          <p class="mt-1.5 max-w-xl text-pretty text-sm text-stone-600 dark:text-stone-400">
            Имя, email, публичная страница и смена пароля — всё в одном месте.
          </p>
        </div>
      </header>

      <div
        v-if="loading"
        class="flex min-h-[20rem] flex-col gap-6"
        data-testid="profile-loading"
        aria-busy="true"
        aria-live="polite">
        <Skeleton
          v-for="n in 2"
          :key="n"
          class="rounded-2xl border border-stone-200/80 dark:border-stone-700/80"
          height="14rem"
          data-testid="profile-skeleton" />
      </div>

      <div
        v-else-if="loadError"
        class="max-w-2xl rounded-2xl border border-red-200/80 bg-white/90 p-5 dark:border-red-900/50 dark:bg-stone-900/80"
        data-testid="profile-error"
        role="alert">
        <Message
          severity="error"
          :closable="false">
          {{ loadError }}
        </Message>
        <Button
          data-testid="profile-retry"
          class="mt-4 min-h-12 w-full touch-manipulation sm:w-auto"
          label="Повторить загрузку"
          icon="pi pi-refresh"
          severity="secondary"
          outlined
          @click="load" />
      </div>

      <div
        v-else
        class="flex flex-col gap-6 sm:gap-8"
        data-testid="profile-content">
        <div
          v-if="profile"
          class="profile-panel-enter rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6"
          data-testid="profile-section-personal"
          :style="{ animationDelay: '0ms' }">
          <h2 class="mb-1 text-lg font-semibold text-stone-900 dark:text-rose-50/95">Личные данные</h2>
          <p class="mb-5 text-sm text-stone-600 dark:text-stone-400">
            Эти поля видны только вам; публичная страница показывает то, что вы разрешили ниже.
          </p>
          <div class="flex flex-col gap-5">
            <div class="flex flex-col gap-2">
              <label
                for="profile-username"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Имя пользователя
              </label>
              <InputText
                id="profile-username"
                v-model="form.username"
                data-testid="profile-username"
                class="min-h-11 w-full"
                :invalid="!!fieldErrors.username" />
              <small
                v-if="fieldErrors.username"
                class="text-sm text-red-600 dark:text-red-400">
                {{ fieldErrors.username }}
              </small>
            </div>
            <div class="flex flex-col gap-2">
              <label
                for="profile-email"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Email
              </label>
              <InputText
                id="profile-email"
                v-model="form.email"
                data-testid="profile-email"
                type="email"
                class="min-h-11 w-full"
                :invalid="!!fieldErrors.email" />
              <small
                v-if="fieldErrors.email"
                class="text-sm text-red-600 dark:text-red-400">
                {{ fieldErrors.email }}
              </small>
            </div>
            <div class="flex flex-col gap-3 sm:flex-row sm:items-center">
              <InputSwitch
                v-model="form.isProfilePublic"
                data-testid="profile-public"
                input-id="profile-public" />
              <label
                for="profile-public"
                class="cursor-pointer text-sm text-stone-700 dark:text-stone-300">
                Публичный профиль (страница
                <span class="font-mono text-rose-900/90 dark:text-rose-200/90"
                  >/u/{{ form.username.trim() || '…' }}</span
                >)
              </label>
            </div>
            <p
              class="text-sm text-stone-600 dark:text-stone-400"
              data-testid="profile-meta">
              Роль:
              <strong class="text-stone-800 dark:text-stone-200">{{ profile.role }}</strong>
              · Регистрация: {{ formatDate(profile.createdAt) }}
              <span v-if="profile.lastLogin"> · Последний вход: {{ formatDate(profile.lastLogin) }}</span>
            </p>
            <div class="flex flex-col gap-3 sm:flex-row sm:flex-wrap">
              <Button
                data-testid="profile-save"
                class="min-h-12 w-full touch-manipulation shadow-md shadow-rose-900/10 dark:shadow-black/40 sm:w-auto"
                label="Сохранить"
                icon="pi pi-check"
                :loading="saving"
                @click="saveProfile" />
              <Button
                v-if="form.isProfilePublic"
                data-testid="profile-preview"
                class="min-h-12 w-full touch-manipulation sm:w-auto"
                label="Как меня видят другие"
                icon="pi pi-external-link"
                severity="secondary"
                outlined
                @click="goPublicPreview" />
            </div>
          </div>
        </div>

        <div
          class="profile-panel-enter rounded-xl border border-stone-200/70 bg-stone-50/50 p-5 dark:border-stone-700/60 dark:bg-stone-950/35 sm:p-6"
          data-testid="profile-section-password"
          :style="{ animationDelay: '48ms' }">
          <h2 class="mb-1 text-lg font-semibold text-stone-900 dark:text-rose-50/95">Смена пароля</h2>
          <p class="mb-5 text-sm text-stone-600 dark:text-stone-400">
            Минимальная длина и сложность могут проверяться на сервере; после смены войдите на других устройствах
            заново.
          </p>
          <div class="flex flex-col gap-5">
            <div class="flex flex-col gap-2">
              <label
                for="profile-pwd-current"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Текущий пароль
              </label>
              <Password
                id="profile-pwd-current"
                v-model="pwd.currentPassword"
                data-testid="profile-password-current"
                class="w-full"
                :feedback="false"
                toggle-mask
                input-class="min-h-11 w-full" />
            </div>
            <div class="flex flex-col gap-2">
              <label
                for="profile-pwd-new"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Новый пароль
              </label>
              <Password
                id="profile-pwd-new"
                v-model="pwd.newPassword"
                data-testid="profile-password-new"
                class="w-full"
                toggle-mask
                input-class="min-h-11 w-full" />
            </div>
            <div class="flex flex-col gap-2">
              <label
                for="profile-pwd-confirm"
                class="text-xs font-medium text-stone-600 dark:text-stone-400">
                Подтверждение
              </label>
              <Password
                id="profile-pwd-confirm"
                v-model="pwd.confirmNewPassword"
                data-testid="profile-password-confirm"
                class="w-full"
                :feedback="false"
                toggle-mask
                input-class="min-h-11 w-full" />
            </div>
            <small
              v-if="pwdError"
              class="text-sm text-red-600 dark:text-red-400"
              data-testid="profile-password-error">
              {{ pwdError }}
            </small>
            <Button
              data-testid="profile-password-submit"
              class="min-h-12 w-full touch-manipulation sm:w-auto"
              label="Сменить пароль"
              icon="pi pi-key"
              severity="secondary"
              outlined
              :loading="pwdLoading"
              @click="submitPassword" />
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, reactive, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import axios from 'axios';
  import { useToast } from 'primevue/usetoast';
  import Button from 'primevue/button';
  import InputText from 'primevue/inputtext';
  import InputSwitch from 'primevue/inputswitch';
  import Password from 'primevue/password';
  import Message from 'primevue/message';
  import authService from '@/services/authService';
  import * as profileApi from '@/services/profileService';
  import type { MyProfile, ChangePasswordResponse } from '@/services/profileService';

  const router = useRouter();
  const toast = useToast();

  const profile = ref<MyProfile | null>(null);
  const loading = ref(true);
  const loadError = ref<string | null>(null);
  const saving = ref(false);
  const pwdLoading = ref(false);
  const pwdError = ref<string | null>(null);

  const form = reactive({
    username: '',
    email: '',
    isProfilePublic: false,
  });

  const pwd = reactive({
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: '',
  });

  const fieldErrors = reactive<Record<string, string>>({});

  function formatDate(iso: string): string {
    try {
      return new Date(iso).toLocaleString('ru-RU');
    } catch {
      return iso;
    }
  }

  function goPublicPreview(): void {
    if (!form.username.trim()) return;
    router.push({ name: 'PublicUserProfile', params: { username: form.username.trim() } });
  }

  async function load(): Promise<void> {
    loading.value = true;
    loadError.value = null;
    try {
      const p = await profileApi.getMyProfile();
      profile.value = p;
      form.username = p.username;
      form.email = p.email;
      form.isProfilePublic = p.isProfilePublic;
    } catch (err) {
      if (import.meta.env.DEV) {
        console.error('Ошибка загрузки профиля:', err);
      }
      loadError.value = 'Не удалось загрузить профиль.';
    } finally {
      loading.value = false;
    }
  }

  function parseApiErrors(err: unknown): Record<string, string> {
    const out: Record<string, string> = {};
    if (axios.isAxiosError(err) && err.response?.data?.errors) {
      const errors = err.response.data.errors as Record<string, string[]>;
      for (const [k, v] of Object.entries(errors)) {
        const key = k.charAt(0).toLowerCase() + k.slice(1);
        if (v?.length) out[key] = v.join(' ');
      }
    }
    return out;
  }

  async function saveProfile(): Promise<void> {
    Object.keys(fieldErrors).forEach((k) => delete fieldErrors[k]);
    saving.value = true;
    try {
      const res = await profileApi.updateProfile({
        username: form.username.trim(),
        email: form.email.trim(),
        isProfilePublic: form.isProfilePublic,
      });
      if (res.newToken) {
        authService.setToken(res.newToken);
      }
      if (res.profile) {
        profile.value = res.profile;
      }
      toast.add({ severity: 'success', summary: 'Сохранено', detail: res.message || 'Профиль обновлён', life: 2500 });
    } catch (err) {
      const pe = parseApiErrors(err);
      if (Object.keys(pe).length) {
        Object.assign(fieldErrors, pe);
      }
      const data = axios.isAxiosError(err) ? err.response?.data : undefined;
      const msg =
        data && typeof data === 'object' && 'message' in data && data.message
          ? String((data as { message: string }).message)
          : 'Не удалось сохранить профиль.';
      toast.add({ severity: 'error', summary: 'Ошибка', detail: msg, life: 4000 });
    } finally {
      saving.value = false;
    }
  }

  async function submitPassword(): Promise<void> {
    pwdError.value = null;
    if (pwd.newPassword !== pwd.confirmNewPassword) {
      pwdError.value = 'Новый пароль и подтверждение не совпадают.';
      return;
    }
    pwdLoading.value = true;
    try {
      await profileApi.changePassword({
        currentPassword: pwd.currentPassword,
        newPassword: pwd.newPassword,
        confirmNewPassword: pwd.confirmNewPassword,
      });
      pwd.currentPassword = '';
      pwd.newPassword = '';
      pwd.confirmNewPassword = '';
      toast.add({ severity: 'success', summary: 'Готово', detail: 'Пароль изменён', life: 2500 });
    } catch (err) {
      if (axios.isAxiosError(err) && err.response?.status === 429) {
        const data = err.response.data as ChangePasswordResponse;
        const sec = data.retryAfterSeconds ?? 3600;
        pwdError.value = data.message || `Повторите через ${sec} с.`;
        toast.add({
          severity: 'warn',
          summary: 'Подождите',
          detail: data.message || `Интервал смены пароля: 1 час.`,
          life: 5000,
        });
        return;
      }
      const msg =
        axios.isAxiosError(err) && err.response?.data?.message
          ? String(err.response.data.message)
          : 'Не удалось сменить пароль.';
      pwdError.value = msg;
      toast.add({ severity: 'error', summary: 'Ошибка', detail: msg, life: 4000 });
    } finally {
      pwdLoading.value = false;
    }
  }

  onMounted(() => {
    void load();
  });
</script>

<style scoped>
  .profile-root {
    position: relative;
    isolation: isolate;
  }

  .profile-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  :global(.dark) .profile-grain {
    mix-blend-mode: soft-light;
  }

  .profile-panel-enter {
    animation: profile-panel-in 0.48s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes profile-panel-in {
    from {
      opacity: 0;
      transform: translateY(10px);
    }
    to {
      opacity: 1;
      transform: translateY(0);
    }
  }

  @media (prefers-reduced-motion: reduce) {
    .profile-panel-enter {
      animation: none;
    }
  }
</style>
