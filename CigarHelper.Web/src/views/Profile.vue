<template>
  <div class="p-4 sm:p-6 max-w-3xl mx-auto space-y-8">
    <div>
      <h1 class="text-3xl font-bold text-gray-900 dark:text-white">Профиль</h1>
      <p class="text-gray-600 dark:text-gray-400 mt-2">Данные аккаунта и видимость для других пользователей</p>
    </div>

    <Message
      v-if="loadError"
      severity="error"
      :closable="false"
      >{{ loadError }}</Message
    >

    <Card v-if="profile">
      <template #title>Личные данные</template>
      <template #content>
        <div class="flex flex-col gap-4">
          <div class="flex flex-col gap-2">
            <label class="font-semibold">Имя пользователя</label>
            <InputText
              v-model="form.username"
              class="w-full"
              :invalid="!!fieldErrors.username" />
            <small
              v-if="fieldErrors.username"
              class="text-red-500"
              >{{ fieldErrors.username }}</small
            >
          </div>
          <div class="flex flex-col gap-2">
            <label class="font-semibold">Email</label>
            <InputText
              v-model="form.email"
              type="email"
              class="w-full"
              :invalid="!!fieldErrors.email" />
            <small
              v-if="fieldErrors.email"
              class="text-red-500"
              >{{ fieldErrors.email }}</small
            >
          </div>
          <div class="flex items-center gap-3">
            <InputSwitch v-model="form.isProfilePublic" inputId="pub" />
            <label for="pub">Публичный профиль (доступна страница /u/{{ form.username || '…' }})</label>
          </div>
          <p class="text-sm text-gray-500 dark:text-gray-400">
            Роль: <strong>{{ profile.role }}</strong> · Регистрация: {{ formatDate(profile.createdAt) }}
            <span v-if="profile.lastLogin"> · Последний вход: {{ formatDate(profile.lastLogin) }}</span>
          </p>
          <div class="flex flex-wrap gap-2">
            <Button
              label="Сохранить"
              icon="pi pi-check"
              :loading="saving"
              @click="saveProfile" />
            <Button
              v-if="form.isProfilePublic"
              label="Как меня видят другие"
              icon="pi pi-external-link"
              severity="secondary"
              outlined
              @click="goPublicPreview" />
          </div>
        </div>
      </template>
    </Card>

    <Card>
      <template #title>Смена пароля</template>
      <template #content>
        <div class="flex flex-col gap-4">
          <div class="flex flex-col gap-2">
            <label class="font-semibold">Текущий пароль</label>
            <Password
              v-model="pwd.currentPassword"
              class="w-full"
              :feedback="false"
              toggleMask
              inputClass="w-full" />
          </div>
          <div class="flex flex-col gap-2">
            <label class="font-semibold">Новый пароль</label>
            <Password
              v-model="pwd.newPassword"
              class="w-full"
              toggleMask
              inputClass="w-full" />
          </div>
          <div class="flex flex-col gap-2">
            <label class="font-semibold">Подтверждение</label>
            <Password
              v-model="pwd.confirmNewPassword"
              class="w-full"
              :feedback="false"
              toggleMask
              inputClass="w-full" />
          </div>
          <small
            v-if="pwdError"
            class="text-red-500"
            >{{ pwdError }}</small
          >
          <Button
            label="Сменить пароль"
            icon="pi pi-key"
            severity="secondary"
            :loading="pwdLoading"
            @click="submitPassword" />
        </div>
      </template>
    </Card>
  </div>
</template>

<script setup lang="ts">
  import { ref, reactive, onMounted } from 'vue';
  import { useRouter } from 'vue-router';
  import axios from 'axios';
  import { useToast } from 'primevue/usetoast';
  import Card from 'primevue/card';
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
    loadError.value = null;
    try {
      const p = await profileApi.getMyProfile();
      profile.value = p;
      form.username = p.username;
      form.email = p.email;
      form.isProfilePublic = p.isProfilePublic;
    } catch {
      loadError.value = 'Не удалось загрузить профиль.';
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
