import api from './api'

export const adminService = {
  seed: () => api.post('/admin/seed').then((r) => r.data),
}
