<template>
  <div class="login-page">
    <div class="row justify-content-center">
      <div class="col-md-6">
        <div class="card">
          <div class="card-body">
            <h3 class="card-title text-center mb-4">{{ isRegister ? 'Регистрация' : 'Вход' }}</h3>
            
            <!-- Общая ошибка -->
            <div v-if="error" class="alert alert-danger">{{ error }}</div>
            
            <form @submit.prevent="submitForm">
              <!-- Username field - only for register -->
              <div class="mb-3" v-if="isRegister">
                <label for="username" class="form-label">Имя пользователя</label>
                <input
                  type="text"
                  class="form-control"
                  :class="{'is-invalid': fieldErrors.username}"
                  id="username"
                  v-model="form.username"
                  required
                >
                <div v-if="fieldErrors.username" class="invalid-feedback">
                  {{ fieldErrors.username }}
                </div>
                <small class="form-text text-muted">
                  Используйте только буквы, цифры, подчеркивания и дефисы (3-50 символов)
                </small>
              </div>
              
              <!-- Email field -->
              <div class="mb-3">
                <label for="email" class="form-label">Email</label>
                <input
                  type="email"
                  class="form-control"
                  :class="{'is-invalid': fieldErrors.email}"
                  id="email"
                  v-model="form.email"
                  required
                >
                <div v-if="fieldErrors.email" class="invalid-feedback">
                  {{ fieldErrors.email }}
                </div>
              </div>
              
              <!-- Password field -->
              <div class="mb-3">
                <label for="password" class="form-label">Пароль</label>
                <input
                  type="password"
                  class="form-control"
                  :class="{'is-invalid': fieldErrors.password}"
                  id="password"
                  v-model="form.password"
                  required
                >
                <div v-if="fieldErrors.password" class="invalid-feedback">
                  {{ fieldErrors.password }}
                </div>
                <small v-if="isRegister" class="form-text text-muted">
                  Минимум 6 символов, включая минимум одну строчную букву и одну заглавную букву или цифру
                </small>
              </div>
              
              <!-- Confirm Password field - only for register -->
              <div class="mb-3" v-if="isRegister">
                <label for="confirmPassword" class="form-label">Подтверждение пароля</label>
                <input
                  type="password"
                  class="form-control"
                  :class="{'is-invalid': fieldErrors.confirmPassword}"
                  id="confirmPassword"
                  v-model="form.confirmPassword"
                  required
                >
                <div v-if="fieldErrors.confirmPassword" class="invalid-feedback">
                  {{ fieldErrors.confirmPassword }}
                </div>
              </div>
              
              <div class="d-grid">
                <button type="submit" class="btn btn-primary" :disabled="loading">
                  <span v-if="loading" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
                  {{ isRegister ? 'Зарегистрироваться' : 'Войти' }}
                </button>
              </div>
            </form>
            
            <div class="text-center mt-3">
              <a href="#" @click.prevent="toggleForm">
                {{ isRegister ? 'Уже есть аккаунт? Войти' : 'Нужен аккаунт? Зарегистрироваться' }}
              </a>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import authService from '../services/authService'

export default {
  data() {
    return {
      isRegister: false,
      loading: false,
      error: null,
      validationErrors: {},
      fieldErrors: {
        username: null,
        email: null,
        password: null,
        confirmPassword: null
      },
      form: {
        username: '',
        email: '',
        password: '',
        confirmPassword: ''
      }
    }
  },
  watch: {
    // Валидация при вводе данных
    'form.username': function(value) {
      if (!value && this.isRegister) {
        this.fieldErrors.username = 'Имя пользователя обязательно'
      } else if (this.isRegister && value.length < 3) {
        this.fieldErrors.username = 'Минимум 3 символа'
      } else if (this.isRegister && !/^[a-zA-Z0-9_-]+$/.test(value)) {
        this.fieldErrors.username = 'Допустимы только буквы, цифры, подчеркивания и дефисы'
      } else {
        this.fieldErrors.username = null
      }
    },
    'form.email': function(value) {
      if (!value) {
        this.fieldErrors.email = 'Email обязателен'
      } else if (!/\S+@\S+\.\S+/.test(value)) {
        this.fieldErrors.email = 'Некорректный формат email'
      } else {
        this.fieldErrors.email = null
      }
    },
    'form.password': function(value) {
      if (!value) {
        this.fieldErrors.password = 'Пароль обязателен'
      } else if (value.length < 6) {
        this.fieldErrors.password = 'Минимум 6 символов'
      } else if (this.isRegister && !/^(?=.*[a-z])(?=.*[A-Z0-9]).*$/.test(value)) {
        this.fieldErrors.password = 'Должен содержать строчную букву и заглавную букву или цифру'
      } else {
        this.fieldErrors.password = null
      }
      
      // Проверка подтверждения пароля при изменении самого пароля
      if (this.isRegister && this.form.confirmPassword && value !== this.form.confirmPassword) {
        this.fieldErrors.confirmPassword = 'Пароли не совпадают'
      } else if (this.isRegister && this.form.confirmPassword) {
        this.fieldErrors.confirmPassword = null
      }
    },
    'form.confirmPassword': function(value) {
      if (!value && this.isRegister) {
        this.fieldErrors.confirmPassword = 'Подтверждение пароля обязательно'
      } else if (this.isRegister && value !== this.form.password) {
        this.fieldErrors.confirmPassword = 'Пароли не совпадают'
      } else {
        this.fieldErrors.confirmPassword = null
      }
    }
  },
  methods: {
    toggleForm() {
      this.isRegister = !this.isRegister
      this.clearErrors()
      
      // Сбросить форму
      this.form = {
        username: '',
        email: '',
        password: '',
        confirmPassword: ''
      }
    },
    clearErrors() {
      this.error = null
      this.validationErrors = {}
      this.fieldErrors = {
        username: null,
        email: null,
        password: null,
        confirmPassword: null
      }
    },
    async submitForm() {
      this.clearErrors()
      
      // Проверить форму перед отправкой
      if (!this.validateForm()) {
        return
      }
      
      this.loading = true
      
      try {
        if (this.isRegister) {
          // Registration
          const response = await authService.register({
            username: this.form.username,
            email: this.form.email,
            password: this.form.password,
            confirmPassword: this.form.confirmPassword
          })
          
          const userData = authService.formatUserData(response.data)
          authService.setAuth(userData.token, userData)
          this.redirectAfterAuth()
        } else {
          // Login
          const response = await authService.login({
            email: this.form.email,
            password: this.form.password
          })
          
          const userData = authService.formatUserData(response.data)
          authService.setAuth(userData.token, userData)
          this.redirectAfterAuth()
        }
      } catch (error) {
        this.handleError(error)
      } finally {
        this.loading = false
      }
    },
    validateForm() {
      let isValid = true
      
      // Проверяем email для всех форм
      if (!this.form.email) {
        this.fieldErrors.email = 'Email обязателен'
        isValid = false
      } else if (!/\S+@\S+\.\S+/.test(this.form.email)) {
        this.fieldErrors.email = 'Некорректный формат email'
        isValid = false
      }
      
      // Проверяем пароль для всех форм
      if (!this.form.password) {
        this.fieldErrors.password = 'Пароль обязателен'
        isValid = false
      } else if (this.isRegister && this.form.password.length < 6) {
        this.fieldErrors.password = 'Минимум 6 символов'
        isValid = false
      } else if (this.isRegister && !/^(?=.*[a-z])(?=.*[A-Z0-9]).*$/.test(this.form.password)) {
        this.fieldErrors.password = 'Должен содержать строчную букву и заглавную букву или цифру'
        isValid = false
      }
      
      // Дополнительные проверки только для формы регистрации
      if (this.isRegister) {
        // Проверка имени пользователя
        if (!this.form.username) {
          this.fieldErrors.username = 'Имя пользователя обязательно'
          isValid = false
        } else if (this.form.username.length < 3) {
          this.fieldErrors.username = 'Минимум 3 символа'
          isValid = false
        } else if (!/^[a-zA-Z0-9_-]+$/.test(this.form.username)) {
          this.fieldErrors.username = 'Допустимы только буквы, цифры, подчеркивания и дефисы'
          isValid = false
        }
        
        // Проверка подтверждения пароля
        if (!this.form.confirmPassword) {
          this.fieldErrors.confirmPassword = 'Подтверждение пароля обязательно'
          isValid = false
        } else if (this.form.confirmPassword !== this.form.password) {
          this.fieldErrors.confirmPassword = 'Пароли не совпадают'
          isValid = false
        }
      }
      
      return isValid
    },
    handleError(error) {
      if (error.response) {
        const { data } = error.response
        
        // Обработка ошибок валидации
        if (data.errors && typeof data.errors === 'object') {
          this.validationErrors = data.errors
          this.error = data.message || 'Ошибка валидации данных формы'
        } 
        // Обработка других ошибок с помощью сервиса аутентификации
        else {
          this.error = authService.getErrorMessage(error)
        }
      } else {
        this.error = authService.getErrorMessage(error)
      }
    },
    redirectAfterAuth() {
      const redirectUrl = this.$route.query.redirect || '/humidors'
      this.$router.push(redirectUrl)
    }
  }
}
</script>

<style scoped>
.login-page {
  padding-top: 2rem;
}
.card {
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}
</style> 