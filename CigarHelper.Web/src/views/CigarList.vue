<template>
  <div class="cigar-list">
    <h1>My Cigars</h1>
    
    <div v-if="loading" class="text-center my-5">
      <div class="spinner-border" role="status">
        <span class="visually-hidden">Loading...</span>
      </div>
    </div>
    
    <div v-else-if="error" class="alert alert-danger">
      {{ error }}
    </div>
    
    <div v-else-if="cigars.length === 0" class="text-center my-5">
      <p class="lead">You don't have any cigars yet.</p>
      <p>Add cigars to your collection to start managing them.</p>
    </div>
    
    <div v-else>
      <div class="table-responsive">
        <table class="table table-striped">
          <thead>
            <tr>
              <th>Name</th>
              <th>Brand</th>
              <th>Size</th>
              <th>Strength</th>
              <th>Rating</th>
              <th>Humidor</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="cigar in cigars" :key="cigar.id">
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
                <router-link 
                  v-if="cigar.humidorId" 
                  :to="`/humidors/${cigar.humidorId}`"
                  class="text-decoration-none">
                  {{ cigar.humidorName || `Humidor #${cigar.humidorId}` }}
                </router-link>
                <span v-else class="text-muted">Not stored</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script>
import cigarService from '../services/cigarService'

export default {
  data() {
    return {
      cigars: [],
      loading: true,
      error: null
    }
  },
  async created() {
    try {
      const response = await cigarService.getCigars()
      this.cigars = response.data
      this.loading = false
    } catch (error) {
      this.error = 'Failed to load cigars'
      this.loading = false
      console.error(error)
    }
  }
}
</script> 