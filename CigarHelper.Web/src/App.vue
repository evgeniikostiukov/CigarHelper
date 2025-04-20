<template>
  <div class="app-container">
    <nav class="navbar navbar-expand-lg navbar-dark bg-dark">
      <div class="container">
        <router-link class="navbar-brand" to="/">Cigar Helper</router-link>
        <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav"
          aria-controls="navbarNav" aria-expanded="false" aria-label="Toggle navigation">
          <span class="navbar-toggler-icon"></span>
        </button>
        <div class="collapse navbar-collapse" id="navbarNav">
          <ul class="navbar-nav me-auto">
            <li class="nav-item">
              <router-link class="nav-link" to="/humidors">Humidors</router-link>
            </li>
            <li class="nav-item">
              <router-link class="nav-link" to="/cigars">My Cigars</router-link>
            </li>
            <li class="nav-item">
              <router-link class="nav-link" to="/reviews">Reviews</router-link>
            </li>
          </ul>
          <div class="d-flex">
            <button v-if="isAuthenticated" @click="logout" class="btn btn-outline-light">Logout</button>
            <router-link v-else class="btn btn-outline-light" to="/login">Login</router-link>
          </div>
        </div>
      </div>
    </nav>

    <div class="container mt-4">
      <router-view />
    </div>
  </div>
</template>

<script>
export default {
  data() {
    return {
      isAuthenticated: false
    }
  },
  created() {
    // Check if user is authenticated
    this.isAuthenticated = !!localStorage.getItem('token')
  },
  methods: {
    logout() {
      localStorage.removeItem('token')
      localStorage.removeItem('user')
      this.isAuthenticated = false
      this.$router.push('/login')
    }
  }
}
</script>

<style>
.app-container {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.container {
  flex: 1;
}
</style> 