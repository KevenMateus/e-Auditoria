import axios from 'axios'

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
})

api.interceptors.request.use((config) => {
  const token = localStorage.getItem('eauditoria_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

api.interceptors.response.use(
  (res) => res,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('eauditoria_token')
      localStorage.removeItem('eauditoria_user')
      if (!window.location.pathname.includes('/login')) {
        window.location.href = '/login'
      }
    }
    const msg = error.response?.data?.detail
      ?? error.response?.data?.mensagem
      ?? 'Erro ao comunicar com o servidor.'
    return Promise.reject(new Error(msg))
  },
)

export default api
