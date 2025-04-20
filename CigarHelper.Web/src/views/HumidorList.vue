<template>
  <div class="humidor-list">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h1>My Humidors</h1>
      <router-link to="/humidors/new" class="btn btn-primary">
        <i class="bi bi-plus-circle"></i> Add New Humidor
      </router-link>
    </div>

    <div v-if="loading" class="text-center my-5">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <div v-else-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <div v-else-if="humidors.length === 0" class="text-center my-5">
      <p class="lead">You don't have any humidors yet.</p>
      <router-link to="/humidors/new" class="btn btn-primary">
        Create Your First Humidor
      </router-link>
    </div>

    <div v-else class="row">
      <div v-for="humidor in humidors" :key="humidor.id" class="col-md-4 mb-4">
        <div class="card h-100">
          <div class="card-body">
            <h5 class="card-title">{{ humidor.name }}</h5>
            <p class="card-text text-truncate-2">{{ humidor.description || 'No description' }}</p>
            <div class="d-flex justify-content-between mb-3">
              <span class="badge bg-primary">Capacity: {{ humidor.cigarCount }}/{{ humidor.capacity }}</span>
              <span class="badge" :class="humidityClass(humidor.currentHumidity)">
                {{ humidor.currentHumidity || '?' }}% RH
              </span>
            </div>
            <div class="d-flex justify-content-between">
              <router-link :to="`/humidors/${humidor.id}`" class="btn btn-sm btn-outline-primary">
                View Details
              </router-link>
              <div>
                <router-link :to="`/humidors/${humidor.id}/edit`" class="btn btn-sm btn-outline-secondary me-1">
                  Edit
                </router-link>
                <button @click="confirmDelete(humidor)" class="btn btn-sm btn-outline-danger">
                  Delete
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Delete Confirmation Modal -->
    <div v-if="showDeleteModal" class="modal show d-block" tabindex="-1">
      <div class="modal-dialog">
        <div class="modal-content">
          <div class="modal-header">
            <h5 class="modal-title">Delete Humidor</h5>
            <button type="button" class="btn-close" @click="cancelDelete"></button>
          </div>
          <div class="modal-body">
            <p>Are you sure you want to delete <strong>{{ humidorToDelete?.name }}</strong>?</p>
            <p class="text-danger">This action cannot be undone.</p>
          </div>
          <div class="modal-footer">
            <button type="button" class="btn btn-secondary" @click="cancelDelete">Cancel</button>
            <button type="button" class="btn btn-danger" @click="deleteHumidor">Delete</button>
          </div>
        </div>
      </div>
      <div class="modal-backdrop show"></div>
    </div>
  </div>
</template>

<script>
import humidorService from '../services/humidorService'

export default {
  data() {
    return {
      humidors: [],
      loading: true,
      error: null,
      showDeleteModal: false,
      humidorToDelete: null
    }
  },
  async created() {
    try {
      const response = await humidorService.getHumidors()
      this.humidors = response.data
      this.loading = false
    } catch (error) {
      this.error = 'Failed to load humidors'
      this.loading = false
      console.error(error)
    }
  },
  methods: {
    humidityClass(humidity) {
      if (!humidity) return 'bg-secondary'
      if (humidity < 62) return 'bg-danger'
      if (humidity > 75) return 'bg-danger'
      if (humidity < 65) return 'bg-warning'
      if (humidity > 72) return 'bg-warning'
      return 'bg-success'
    },
    confirmDelete(humidor) {
      this.humidorToDelete = humidor
      this.showDeleteModal = true
    },
    cancelDelete() {
      this.humidorToDelete = null
      this.showDeleteModal = false
    },
    async deleteHumidor() {
      if (!this.humidorToDelete) return
      
      try {
        await humidorService.deleteHumidor(this.humidorToDelete.id)
        
        // Remove from the list
        this.humidors = this.humidors.filter(h => h.id !== this.humidorToDelete.id)
        
        // Close modal
        this.cancelDelete()
      } catch (error) {
        this.error = 'Failed to delete humidor'
        console.error(error)
      }
    }
  }
}
</script>

<style scoped>
.modal-backdrop {
  opacity: 0.5;
}
</style> 