import api from './api'

export default {
  // Get all humidors for the current user
  getHumidors() {
    return api.get('/humidors')
  },

  // Get a specific humidor by ID
  getHumidor(id) {
    return api.get(`/humidors/${id}`)
  },

  // Create a new humidor
  createHumidor(humidor) {
    return api.post('/humidors', humidor)
  },

  // Update an existing humidor
  updateHumidor(id, humidor) {
    return api.put(`/humidors/${id}`, humidor)
  },

  // Delete a humidor
  deleteHumidor(id) {
    return api.delete(`/humidors/${id}`)
  },

  // Get all cigars in a humidor
  getCigarsInHumidor(humidorId) {
    return api.get(`/humidors/${humidorId}/cigars`)
  },

  // Add a cigar to a humidor
  addCigarToHumidor(humidorId, cigarId) {
    return api.post(`/humidors/${humidorId}/cigars/${cigarId}`)
  },

  // Remove a cigar from a humidor
  removeCigarFromHumidor(humidorId, cigarId) {
    return api.delete(`/humidors/${humidorId}/cigars/${cigarId}`)
  }
} 