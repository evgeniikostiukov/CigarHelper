import api from './api'

export default {
  // Get all cigars for the current user
  getCigars() {
    return api.get('/cigars')
  },

  // Get a specific cigar by ID
  getCigar(id) {
    return api.get(`/cigars/${id}`)
  },

  // Create a new cigar
  createCigar(cigar) {
    return api.post('/cigars', cigar)
  },

  // Update an existing cigar
  updateCigar(id, cigar) {
    return api.put(`/cigars/${id}`, cigar)
  },

  // Delete a cigar
  deleteCigar(id) {
    return api.delete(`/cigars/${id}`)
  }
} 