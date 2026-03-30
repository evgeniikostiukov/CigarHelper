<template>
  <section
    class="login-root -mx-2 flex min-h-[min(28rem,75dvh)] flex-col justify-center rounded-2xl bg-gradient-to-b from-stone-50 via-rose-50/40 to-stone-50 px-3 py-10 ring-1 ring-stone-900/5 sm:mx-0 sm:min-h-[min(32rem,70dvh)] sm:rounded-3xl dark:from-stone-950 dark:via-rose-950/20 dark:to-stone-950 dark:ring-stone-100/10 sm:px-6 sm:py-12"
    data-testid="login"
    aria-labelledby="login-heading">
    <div class="login-grain pointer-events-none absolute inset-0 rounded-[inherit] opacity-[0.35] dark:opacity-20" />

    <div class="relative z-[1] mx-auto w-full max-w-md">
      <header class="mb-6 text-center sm:mb-8">
        <p
          class="mb-1.5 text-[0.65rem] font-semibold uppercase tracking-[0.22em] text-rose-900/65 dark:text-rose-200/55">
          Cigar Helper
        </p>
        <h1
          id="login-heading"
          class="text-balance text-3xl font-semibold tracking-tight text-stone-900 dark:text-rose-50/95 sm:text-4xl">
          {{ isRegister ? 'Регистрация' : 'Вход' }}
        </h1>
        <p class="mx-auto mt-1.5 max-w-sm text-pretty text-sm text-stone-600 dark:text-stone-400">
          {{
            isRegister
              ? 'Создайте аккаунт — коллекция, хьюмидоры и обзоры в одном месте.'
              : 'Войдите по email, чтобы продолжить работу с коллекцией.'
          }}
        </p>
      </header>

      <div
        class="login-panel-enter rounded-2xl border border-stone-200/90 bg-white/95 p-5 shadow-md shadow-stone-900/5 dark:border-stone-700/90 dark:bg-stone-900/85 dark:shadow-black/50 sm:p-6">
        <Message
          v-if="error"
          severity="error"
          class="mb-4"
          data-testid="login-error"
          :closable="false">
          {{ error }}
        </Message>

        <form
          data-testid="login-form"
          class="flex flex-col gap-5"
          @submit.prevent="submitForm">
          <div
            v-if="isRegister"
            class="flex flex-col gap-2">
            <label
              for="login-username"
              class="text-xs font-medium text-stone-600 dark:text-stone-400">
              Имя пользователя
            </label>
            <InputText
              id="login-username"
              v-model="form.username"
              data-testid="login-username"
              class="min-h-11 w-full"
              :invalid="!!fieldErrors.username"
              aria-describedby="login-username-help"
              placeholder="Например, john_doe" />
            <small
              id="login-username-help"
              class="text-xs text-stone-500 dark:text-stone-500">
              Только буквы, цифры, _ и - (3–50 симв.)
            </small>
            <small
              v-if="fieldErrors.username"
              class="text-sm text-red-600 dark:text-red-400">
              {{ fieldErrors.username }}
            </small>
          </div>

          <div class="flex flex-col gap-2">
            <label
              for="login-email"
              class="text-xs font-medium text-stone-600 dark:text-stone-400">
              Email
            </label>
            <InputText
              id="login-email"
              v-model="form.email"
              data-testid="login-email"
              class="min-h-11 w-full"
              type="email"
              :invalid="!!fieldErrors.email"
              placeholder="email@example.com"
              autocomplete="email" />
            <small
              v-if="fieldErrors.email"
              class="text-sm text-red-600 dark:text-red-400">
              {{ fieldErrors.email }}
            </small>
          </div>

          <div class="flex flex-col gap-2">
            <label
              for="login-password"
              class="text-xs font-medium text-stone-600 dark:text-stone-400">
              Пароль
            </label>
            <Password
              id="login-password"
              v-model="form.password"
              data-testid="login-password"
              class="w-full"
              input-class="min-h-11 w-full"
              :invalid="!!fieldErrors.password"
              :feedback="isRegister"
              toggle-mask
              placeholder="••••••"
              :input-props="{ autocomplete: isRegister ? 'new-password' : 'current-password' }" />
            <small
              v-if="fieldErrors.password"
              class="text-sm text-red-600 dark:text-red-400">
              {{ fieldErrors.password }}
            </small>
          </div>

          <div
            v-if="isRegister"
            class="flex flex-col gap-2">
            <label
              for="login-confirm-password"
              class="text-xs font-medium text-stone-600 dark:text-stone-400">
              Подтверждение пароля
            </label>
            <Password
              id="login-confirm-password"
              v-model="form.confirmPassword"
              data-testid="login-confirm-password"
              class="w-full"
              input-class="min-h-11 w-full"
              :invalid="!!fieldErrors.confirmPassword"
              :feedback="false"
              toggle-mask
              placeholder="••••••"
              :input-props="{ autocomplete: 'new-password' }" />
            <small
              v-if="fieldErrors.confirmPassword"
              class="text-sm text-red-600 dark:text-red-400">
              {{ fieldErrors.confirmPassword }}
            </small>
          </div>

          <Button
            data-testid="login-submit"
            type="submit"
            class="mt-1 min-h-12 w-full touch-manipulation shadow-md shadow-rose-900/10 dark:shadow-black/40"
            :label="isRegister ? 'Зарегистрироваться' : 'Войти'"
            :loading="loading"
            :icon="isRegister ? 'pi pi-user-plus' : 'pi pi-sign-in'" />

          <div class="flex flex-col gap-3 border-t border-stone-200/80 pt-4 dark:border-stone-700/80">
            <Button
              data-testid="login-toggle-mode"
              type="button"
              class="min-h-12 w-full touch-manipulation text-stone-700 dark:text-stone-200"
              :label="isRegister ? 'Уже есть аккаунт? Войти' : 'Нужен аккаунт? Зарегистрироваться'"
              text
              @click="toggleForm" />
            <Button
              data-testid="login-home"
              type="button"
              class="min-h-12 w-full touch-manipulation"
              label="На главную"
              icon="pi pi-home"
              severity="secondary"
              outlined
              @click="router.push({ name: 'Home' })" />
          </div>
        </form>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
  import { ref, reactive, watch } from 'vue';
  import { useRouter, useRoute } from 'vue-router';
  import authService from '../services/authService';
  import type { AuthCredentials, RegisterData } from '../services/authService';

  interface LoginForm {
    username: string;
    email: string;
    password: string;
    confirmPassword: string;
  }

  interface FieldErrors {
    username: string | null;
    email: string | null;
    password: string | null;
    confirmPassword: string | null;
  }

  const router = useRouter();
  const route = useRoute();

  const isRegister = ref(false);
  const loading = ref(false);
  const error = ref<string | null>(null);

  const form = reactive<LoginForm>({
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
  });

  const fieldErrors = reactive<FieldErrors>({
    username: null,
    email: null,
    password: null,
    confirmPassword: null,
  });

  const validateField = (field: keyof LoginForm, value: string): string | null => {
    switch (field) {
      case 'username':
        if (isRegister.value) {
          if (!value) return 'Имя пользователя обязательно';
          if (value.length < 3) return 'Минимум 3 символа';
          if (!/^[a-zA-Z0-9_-]+$/.test(value)) return 'Недопустимые символы';
        }
        break;
      case 'email':
        if (!value) return 'Email обязателен';
        if (!/\S+@\S+\.\S+/.test(value)) return 'Некорректный формат email';
        break;
      case 'password':
        if (!value) return 'Пароль обязателен';
        if (value.length < 6) return 'Минимум 6 символов';
        if (isRegister.value && !/^(?=.*[a-z])(?=.*[A-Z0-9]).*$/.test(value)) {
          return 'Нужна строчная буква и заглавная/цифра';
        }
        break;
      case 'confirmPassword':
        if (isRegister.value) {
          if (!value) return 'Подтверждение пароля обязательно';
          if (value !== form.password) return 'Пароли не совпадают';
        }
        break;
    }
    return null;
  };

  watch(
    form,
    (newForm) => {
      for (const field in newForm) {
        const key = field as keyof LoginForm;
        fieldErrors[key] = validateField(key, newForm[key]);
      }
    },
    { deep: true },
  );

  const clearForm = (): void => {
    Object.assign(form, {
      username: '',
      email: '',
      password: '',
      confirmPassword: '',
    });
    clearErrors();
  };

  const toggleForm = (): void => {
    isRegister.value = !isRegister.value;
    clearForm();
  };

  const clearErrors = (): void => {
    error.value = null;
    Object.assign(fieldErrors, {
      username: null,
      email: null,
      password: null,
      confirmPassword: null,
    });
  };

  const submitForm = async (): Promise<void> => {
    clearErrors();

    let hasError = false;
    for (const field in form) {
      const key = field as keyof LoginForm;
      if (!isRegister.value && (key === 'username' || key === 'confirmPassword')) {
        continue;
      }
      const validationError = validateField(key, form[key]);
      if (validationError) {
        fieldErrors[key] = validationError;
        hasError = true;
      }
    }

    if (hasError) return;

    loading.value = true;
    try {
      if (isRegister.value) {
        const payload: RegisterData = {
          username: form.username,
          email: form.email,
          password: form.password,
          confirmPassword: form.confirmPassword,
        };
        const response = await authService.register(payload);
        if (!response.success) {
          throw new Error(response.message || 'Ошибка регистрации');
        }
        localStorage.setItem('needsOnboarding', '1');
      } else {
        const payload: AuthCredentials = {
          email: form.email,
          password: form.password,
        };
        const response = await authService.login(payload);
        if (!response.success) {
          throw new Error(response.message || 'Ошибка входа');
        }
      }

      if (isRegister.value) {
        await router.push({ name: 'Onboarding' });
      } else {
        const redirectUrl = (route.query.redirect as string) || '/';
        await router.push(redirectUrl);
      }
    } catch (err: unknown) {
      const ax = err as { response?: { data?: { message?: string } }; message?: string };
      error.value =
        ax.response?.data?.message || ax.message || (isRegister.value ? 'Ошибка регистрации' : 'Ошибка входа');
      if (import.meta.env.DEV && err instanceof Error) {
        console.error(err);
      }
    } finally {
      loading.value = false;
    }
  };
</script>

<style scoped>
  .login-root {
    position: relative;
    isolation: isolate;
  }

  .login-grain {
    background-image: url("data:image/svg+xml,%3Csvg viewBox='0 0 256 256' xmlns='http://www.w3.org/2000/svg'%3E%3Cfilter id='n'%3E%3CfeTurbulence type='fractalNoise' baseFrequency='0.85' numOctaves='4' stitchTiles='stitch'/%3E%3C/filter%3E%3Crect width='100%25' height='100%25' filter='url(%23n)'/%3E%3C/svg%3E");
    mix-blend-mode: multiply;
  }

  :global(.dark) .login-grain {
    mix-blend-mode: soft-light;
  }

  .login-panel-enter {
    animation: login-panel-in 0.45s cubic-bezier(0.22, 1, 0.36, 1) backwards;
  }

  @keyframes login-panel-in {
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
    .login-panel-enter {
      animation: none;
    }
  }
</style>
