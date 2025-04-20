<template>
  <div class="home">
    <div class="jumbotron bg-light p-5 rounded-3 mb-4">
      <h1 class="display-4">Welcome to Cigar Helper</h1>
      <p class="lead">
        Your one-stop solution for managing your cigar collection and humidors.
      </p>
      <hr class="my-4">
      <p>
        Keep track of your cigars, monitor humidor conditions, and never lose sight of your collection.
      </p>
      <div class="mt-4">
        <router-link v-if="isAuthenticated" to="/humidors" class="btn btn-primary btn-lg me-2">
          View My Humidors
        </router-link>
        <router-link v-if="isAuthenticated" to="/cigars" class="btn btn-secondary btn-lg">
          Browse My Cigars
        </router-link>
        <router-link v-else to="/login" class="btn btn-primary btn-lg">
          Get Started
        </router-link>
      </div>
    </div>

    <div class="row">
      <div class="col-md-4 mb-4">
        <div class="card h-100">
          <div class="card-body">
            <h5 class="card-title">Track Your Humidors</h5>
            <p class="card-text">
              Create multiple humidors, monitor humidity and temperature, and track capacity.
            </p>
          </div>
        </div>
      </div>
      <div class="col-md-4 mb-4">
        <div class="card h-100">
          <div class="card-body">
            <h5 class="card-title">Manage Your Cigars</h5>
            <p class="card-text">
              Keep details about your cigars, including brand, size, strength, and rating.
            </p>
          </div>
        </div>
      </div>
      <div class="col-md-4 mb-4">
        <div class="card h-100">
          <div class="card-body">
            <h5 class="card-title">Stay Organized</h5>
            <p class="card-text">
              Easily add or remove cigars from humidors and know exactly where everything is stored.
            </p>
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
      isAuthenticated: false
    }
  },
  created() {
    this.isAuthenticated = authService.isAuthenticated()
  }
}
</script> 