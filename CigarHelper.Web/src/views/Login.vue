<template>
  <div class="flex items-center justify-center h-full px-4">
    <div class="w-full max-w-md">
      <Card class="shadow-2xl">
        <template #title>
          <h2 class="text-3xl font-bold text-center text-gray-800 dark:text-white">
            {{ isRegister ? 'Регистрация' : 'Вход' }}
          </h2>
        </template>

        <template #content>
          <!-- Общая ошибка -->
          <Message
            v-if="error"
            severity="error"
            :closable="false"
            class="mb-4"
            >{{ error }}</Message
          >

          <form
            @submit.prevent="submitForm"
            class="flex flex-col gap-6">
            <!-- Поля формы -->
            <div
              class="flex flex-col gap-2"
              v-if="isRegister">
              <label
                for="username"
                class="font-semibold text-gray-700 dark:text-gray-300"
                >Имя пользователя</label
              >
              <InputText
                id="username"
                v-model="form.username"
                :invalid="!!fieldErrors.username"
                aria-describedby="username-help"
                placeholder="Например, john_doe" />
              <small
                id="username-help"
                class="text-gray-500 dark:text-gray-400">
                Только буквы, цифры, _ и - (3-50 симв.)
              </small>
              <small
                v-if="fieldErrors.username"
                class="text-red-500"
                >{{ fieldErrors.username }}</small
              >
            </div>

            <div class="flex flex-col gap-2">
              <label
                for="email"
                class="font-semibold text-gray-700 dark:text-gray-300"
                >Email</label
              >
              <InputText
                id="email"
                v-model="form.email"
                type="email"
                :invalid="!!fieldErrors.email"
                placeholder="email@example.com" />
              <small
                v-if="fieldErrors.email"
                class="text-red-500"
                >{{ fieldErrors.email }}</small
              >
            </div>

            <div class="flex flex-col gap-2">
              <label
                for="password"
                class="font-semibold text-gray-700 dark:text-gray-300"
                >Пароль</label
              >
              <Password
                id="password"
                v-model="form.password"
                :invalid="!!fieldErrors.password"
                :feedback="isRegister"
                toggleMask
                placeholder="******" />
              <small
                v-if="fieldErrors.password"
                class="text-red-500"
                >{{ fieldErrors.password }}</small
              >
            </div>

            <div
              class="flex flex-col gap-2"
              v-if="isRegister">
              <label
                for="confirmPassword"
                class="font-semibold text-gray-700 dark:text-gray-300"
                >Подтверждение пароля</label
              >
              <Password
                id="confirmPassword"
                v-model="form.confirmPassword"
                :invalid="!!fieldErrors.confirmPassword"
                :feedback="false"
                toggleMask
                placeholder="******" />
              <small
                v-if="fieldErrors.confirmPassword"
                class="text-red-500"
                >{{ fieldErrors.confirmPassword }}</small
              >
            </div>

            <Button
              type="submit"
              :label="isRegister ? 'Зарегистрироваться' : 'Войти'"
              :loading="loading"
              class="w-full mt-4" />
          </form>
        </template>

        <template #footer>
          <div class="text-center">
            <a
              href="#"
              @click.prevent="toggleForm"
              class="text-blue-500 hover:underline">
              {{ isRegister ? 'Уже есть аккаунт? Войти' : 'Нужен аккаунт? Зарегистрироваться' }}
            </a>
          </div>
        </template>
      </Card>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, reactive, watch } from 'vue';
  import { useRouter, useRoute } from 'vue-router';
  import authService from '../services/authService';
  import type { AuthCredentials, RegisterData } from '../services/authService';

  // --- Interfaces ---
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

  // --- Component State ---
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

  // --- Methods ---
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

  const redirectAfterAuth = (): void => {
    const redirectUrl = (route.query.redirect as string) || '/humidors';
    router.push(redirectUrl);
    // Full page reload to ensure all states are reset correctly
    // setTimeout(() => window.location.reload(), 100);
  };

  const submitForm = async (): Promise<void> => {
    clearErrors();

    let hasError = false;
    for (const field in form) {
      const key = field as keyof LoginForm;
      // Skip validation for fields that are not part of the current form (login vs register)
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

      // redirectAfterAuth();
      const redirectUrl = (route.query.redirect as string) || '/';
      router.push(redirectUrl);
    } catch (err: any) {
      error.value =
        err.response?.data?.message || err.message || (isRegister.value ? 'Ошибка регистрации' : 'Ошибка входа');
    } finally {
      loading.value = false;
    }
  };
</script>

<style scoped>
  .login-page {
    padding-top: 2rem;
  }
  .card {
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  }
</style>
