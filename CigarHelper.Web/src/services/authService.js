import api from './api'

const AUTH_TOKEN_KEY = 'token'
const USER_DATA_KEY = 'user'

export default {
  // Login
  login(credentials) {
    return api.post('/auth/login', credentials)
  },

  // Register
  register(userData) {
    return api.post('/auth/register', userData)
  },

  // Get current user
  getCurrentUser() {
    const user = localStorage.getItem(USER_DATA_KEY)
    return user ? JSON.parse(user) : null
  },

  // Get authentication token
  getToken() {
    return localStorage.getItem(AUTH_TOKEN_KEY)
  },

  // Set token and user data
  setAuth(token, user) {
    localStorage.setItem(AUTH_TOKEN_KEY, token)
    localStorage.setItem(USER_DATA_KEY, JSON.stringify(user))
  },

  // Clear auth data
  clearAuth() {
    localStorage.removeItem(AUTH_TOKEN_KEY)
    localStorage.removeItem(USER_DATA_KEY)
  },

  // Check if user is authenticated
  isAuthenticated() {
    const token = this.getToken()
    if (!token) return false
    
    try {
      // Простая проверка формата JWT без проверки подписи
      const tokenParts = token.split('.')
      if (tokenParts.length !== 3) return false
      
      // Проверка срока действия токена
      const payload = JSON.parse(atob(tokenParts[1]))
      const expiration = payload.exp * 1000 // Переводим секунды в миллисекунды
      
      return Date.now() < expiration
    } catch (e) {
      console.error('Ошибка при проверке JWT токена:', e)
      this.clearAuth() // Очищаем невалидный токен
      return false
    }
  },
  
  // Преобразовать данные ответа в формат для отображения пользователю
  formatUserData(responseData) {
    return {
      username: responseData.username || '',
      token: responseData.token || '',
      expiration: responseData.expiration || null
    }
  },
  
  // Получить информативное сообщение об ошибке
  getErrorMessage(error) {
    if (!error.response) {
      return 'Не удалось подключиться к серверу'
    }
    
    const { status, data } = error.response
    
    switch (status) {
      case 400:
        return data.message || 'Ошибка в запросе'
      case 401:
        return 'Неверный email или пароль'
      case 403:
        return 'Доступ запрещен'
      case 404:
        return 'Ресурс не найден'
      case 500:
        return 'Внутренняя ошибка сервера'
      default:
        return `Ошибка ${status}: ${data.message || 'Что-то пошло не так'}`
    }
  }
} 