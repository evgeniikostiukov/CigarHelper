<template>
  <div class="humidor-detail">
    <div v-if="loading" class="text-center my-5">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>

    <div v-else-if="error" class="alert alert-danger">
      {{ error }}
    </div>

    <div v-else>
      <div class="d-sm-flex justify-content-between align-items-center mb-4">
        <div>
          <h1 class="mb-1">{{ humidor.name }}</h1>
          <p v-if="humidor.description" class="text-muted mb-0">{{ humidor.description }}</p>
        </div>
        <div class="mt-3 mt-sm-0">
          <router-link :to="`/humidors/${humidor.id}/edit`" class="btn btn-outline-primary me-2">
            Edit Humidor
          </router-link>
          <router-link to="/humidors" class="btn btn-outline-secondary">
            Back to List
          </router-link>
        </div>
      </div>
      
      <div class="row mb-4">
        <div class="col-md-4 mb-3">
          <div class="card">
            <div class="card-body">
              <h5 class="card-title">Capacity</h5>
              <div class="progress">
                <div 
                  class="progress-bar" 
                  :class="capacityClass" 
                  role="progressbar" 
                  :style="`width: ${capacityPercentage}%`" 
                  :aria-valuenow="humidor.cigars.length" 
                  aria-valuemin="0" 
                  :aria-valuemax="humidor.capacity">
                  {{ humidor.cigars.length }}/{{ humidor.capacity }}
                </div>
              </div>
            </div>
          </div>
        </div>
        <div class="col-md-4 mb-3">
          <div class="card">
            <div class="card-body">
              <h5 class="card-title">Humidity</h5>
              <div class="d-flex align-items-center">
                <div class="display-6 me-2">{{ humidor.currentHumidity || '-' }}%</div>
                <span class="badge" :class="humidityClass">{{ humidityStatus }}</span>
              </div>
            </div>
          </div>
        </div>
        <div class="col-md-4 mb-3">
          <div class="card">
            <div class="card-body">
              <h5 class="card-title">Temperature</h5>
              <div class="display-6">{{ humidor.currentTemperature || '-' }}°C</div>
            </div>
          </div>
        </div>
      </div>
      
      <h3 class="mb-3">Cigars in this Humidor</h3>
      
      <div v-if="humidor.cigars.length === 0" class="alert alert-info">
        <p class="mb-0">This humidor is empty.</p>
      </div>
      
      <div v-else class="table-responsive">
        <table class="table table-striped">
          <thead>
            <tr>
              <th>Name</th>
              <th>Brand</th>
              <th>Size</th>
              <th>Strength</th>
              <th>Rating</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="cigar in humidor.cigars" :key="cigar.id">
              <td>{{ cigar.name }}</td>
              <td>{{ cigar.brand }}</td>
              <td>{{ cigar.size || '-' }}</td>
              <td>{{ cigar.strength || '-' }}</td>
              <td>
                <div v-if="cigar.rating">
                  {{ cigar.rating }}/10
                </div>
                <span v-else>-</span>
              </td>
              <td>
                <button 
                  @click="removeCigar(cigar.id)" 
                  class="btn btn-sm btn-outline-danger"
                  :disabled="removingCigar === cigar.id">
                  <span v-if="removingCigar === cigar.id" class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>
                  Remove
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      
      <h4 class="mt-4 mb-3">Add Cigars to Humidor</h4>
      
      <div v-if="loadingAvailableCigars" class="text-center my-3">
        <div class="spinner-border spinner-border-sm" role="status">
          <span class="visually-hidden">Loading...</span>
        </div>
        <span class="ms-2">Loading available cigars...</span>
      </div>
      
      <div v-else-if="availableCigarsError" class="alert alert-danger">
        {{ availableCigarsError }}
      </div>
      
      <div v-else-if="availableCigars.length === 0" class="alert alert-info">
        <p class="mb-0">You don't have any cigars available to add to this humidor.</p>
      </div>
      
      <div v-else>
        <div class="row row-cols-1 row-cols-md-3 g-4">
          <div v-for="cigar in availableCigars" :key="cigar.id" class="col">
            <div class="card h-100">
              <div class="card-body">
                <h5 class="card-title">{{ cigar.name }}</h5>
                <h6 class="card-subtitle mb-2 text-muted">{{ cigar.brand }}</h6>
                <div class="d-flex justify-content-between align-items-center mt-3">
                  <div>
                    <span v-if="cigar.strength" class="badge bg-secondary me-1">{{ cigar.strength }}</span>
                    <span v-if="cigar.size" class="badge bg-secondary">{{ cigar.size }}</span>
                  </div>
                  <button 
                    @click="addCigar(cigar.id)" 
                    class="btn btn-sm btn-primary"
                    :disabled="addingCigar === cigar.id || isHumidorFull">
                    <span v-if="addingCigar === cigar.id" class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span>
                    Add to Humidor
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import humidorService from '../services/humidorService'
import cigarService from '../services/cigarService'

export default {
  data() {
    return {
      humidor: {
        id: 0,
        name: '',
        description: '',
        capacity: 0,
        currentHumidity: null,
        currentTemperature: null,
        cigars: []
      },
      loading: true,
      error: null,
      removingCigar: null,
      availableCigars: [],
      loadingAvailableCigars: false,
      availableCigarsError: null,
      addingCigar: null
    }
  },
  computed: {
    capacityPercentage() {
      if (!this.humidor.capacity) return 0
      return (this.humidor.cigars.length / this.humidor.capacity) * 100
    },
    capacityClass() {
      const percentage = this.capacityPercentage
      if (percentage >= 90) return 'bg-danger'
      if (percentage >= 75) return 'bg-warning'
      return 'bg-success'
    },
    isHumidorFull() {
      return this.humidor.cigars.length >= this.humidor.capacity
    },
    humidityClass() {
      const humidity = this.humidor.currentHumidity
      if (!humidity) return 'bg-secondary'
      if (humidity < 62) return 'bg-danger'
      if (humidity > 75) return 'bg-danger'
      if (humidity < 65) return 'bg-warning'
      if (humidity > 72) return 'bg-warning'
      return 'bg-success'
    },
    humidityStatus() {
      const humidity = this.humidor.currentHumidity
      if (!humidity) return 'Unknown'
      if (humidity < 62) return 'Too Dry'
      if (humidity > 75) return 'Too Humid'
      if (humidity < 65) return 'Slightly Dry'
      if (humidity > 72) return 'Slightly Humid'
      return 'Optimal'
    }
  },
  async created() {
    await this.loadHumidor()
    this.loadAvailableCigars()
  },
  methods: {
    async loadHumidor() {
      this.loading = true
      this.error = null
      
      try {
        const response = await humidorService.getHumidor(this.$route.params.id)
        this.humidor = response.data
      } catch (error) {
        this.error = 'Failed to load humidor details'
        console.error(error)
      } finally {
        this.loading = false
      }
    },
    async loadAvailableCigars() {
      this.loadingAvailableCigars = true
      this.availableCigarsError = null
      
      try {
        const response = await cigarService.getCigars()
        // Filter out cigars that are already in this humidor
        this.availableCigars = response.data.filter(
          cigar => !cigar.humidorId || cigar.humidorId !== this.humidor.id
        )
      } catch (error) {
        this.availableCigarsError = 'Failed to load available cigars'
        console.error(error)
      } finally {
        this.loadingAvailableCigars = false
      }
    },
    async addCigar(cigarId) {
      if (this.isHumidorFull) return
      
      this.addingCigar = cigarId
      
      try {
        await humidorService.addCigarToHumidor(this.humidor.id, cigarId)
        // Reload humidor to get updated list
        await this.loadHumidor()
        // Reload available cigars
        await this.loadAvailableCigars()
      } catch (error) {
        this.error = 'Failed to add cigar to humidor'
        console.error(error)
      } finally {
        this.addingCigar = null
      }
    },
    async removeCigar(cigarId) {
      this.removingCigar = cigarId
      
      try {
        await humidorService.removeCigarFromHumidor(this.humidor.id, cigarId)
        // Reload humidor to get updated list
        await this.loadHumidor()
        // Reload available cigars
        await this.loadAvailableCigars()
      } catch (error) {
        this.error = 'Failed to remove cigar from humidor'
        console.error(error)
      } finally {
        this.removingCigar = null
      }
    }
  }
}
</script> 