<template>
  <div class="humidor-form">
    <h1>{{ isEdit ? 'Edit' : 'Create' }} Humidor</h1>
    
    <div v-if="loading" class="text-center my-5">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>
    
    <div v-else-if="error" class="alert alert-danger">
      {{ error }}
    </div>
    
    <form v-else @submit.prevent="saveHumidor" class="mt-4">
      <div class="mb-3">
        <label for="name" class="form-label">Name</label>
        <input 
          type="text" 
          class="form-control" 
          id="name" 
          v-model="form.name"
          required
          maxlength="100"
        >
      </div>
      
      <div class="mb-3">
        <label for="description" class="form-label">Description</label>
        <textarea 
          class="form-control" 
          id="description" 
          v-model="form.description"
          rows="3"
          maxlength="500"
        ></textarea>
      </div>
      
      <div class="mb-3">
        <label for="capacity" class="form-label">Capacity</label>
        <input 
          type="number" 
          class="form-control" 
          id="capacity" 
          v-model.number="form.capacity"
          required
          min="1" 
          max="1000"
        >
        <div class="form-text">Maximum number of cigars this humidor can hold.</div>
      </div>
      
      <div class="mb-3">
        <label for="currentHumidity" class="form-label">Current Humidity (%)</label>
        <input 
          type="number" 
          class="form-control" 
          id="currentHumidity" 
          v-model.number="form.currentHumidity"
          min="0" 
          max="100"
        >
      </div>
      
      <div class="mb-3">
        <label for="currentTemperature" class="form-label">Current Temperature (°C)</label>
        <input 
          type="number" 
          class="form-control" 
          id="currentTemperature" 
          v-model.number="form.currentTemperature"
          min="0" 
          max="50"
          step="0.1"
        >
      </div>
      
      <div class="d-flex gap-2">
        <button type="submit" class="btn btn-primary" :disabled="saving">
          <span v-if="saving" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
          {{ isEdit ? 'Update' : 'Create' }} Humidor
        </button>
        <router-link to="/humidors" class="btn btn-outline-secondary">Cancel</router-link>
      </div>
    </form>
  </div>
</template>

<script>
import humidorService from '../services/humidorService'

export default {
  data() {
    return {
      isEdit: false,
      loading: false,
      saving: false,
      error: null,
      form: {
        name: '',
        description: '',
        capacity: 20,
        currentHumidity: null,
        currentTemperature: null
      }
    }
  },
  created() {
    this.isEdit = this.$route.meta.isEdit
    
    if (this.isEdit) {
      this.loadHumidor()
    }
  },
  methods: {
    async loadHumidor() {
      this.loading = true
      this.error = null
      
      try {
        const response = await humidorService.getHumidor(this.$route.params.id)
        const humidor = response.data
        
        this.form = {
          name: humidor.name,
          description: humidor.description,
          capacity: humidor.capacity,
          currentHumidity: humidor.currentHumidity,
          currentTemperature: humidor.currentTemperature
        }
      } catch (error) {
        this.error = 'Failed to load humidor details'
        console.error(error)
      } finally {
        this.loading = false
      }
    },
    async saveHumidor() {
      this.saving = true
      this.error = null
      
      try {
        if (this.isEdit) {
          await humidorService.updateHumidor(this.$route.params.id, this.form)
        } else {
          await humidorService.createHumidor(this.form)
        }
        
        // Navigate back to humidor list
        this.$router.push('/humidors')
      } catch (error) {
        this.error = `Failed to ${this.isEdit ? 'update' : 'create'} humidor`
        console.error(error)
        this.saving = false
      }
    }
  }
}
</script> 