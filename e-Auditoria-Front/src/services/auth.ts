import api from './api'

export interface LoginRequest {
  email: string
  senha: string
}

export interface AuthResponse {
  token: string
  tokenType: string
  expiresInSeconds: number
  nome: string
  email: string
  perfil: string
}

const TOKEN_KEY = 'eauditoria_token'
const USER_KEY = 'eauditoria_user'

export const authService = {
  async login(data: LoginRequest): Promise<AuthResponse> {
    const res = await api.post<AuthResponse>('/auth/login', data)
    const auth = res.data
    localStorage.setItem(TOKEN_KEY, auth.token)
    localStorage.setItem(USER_KEY, JSON.stringify({ nome: auth.nome, email: auth.email, perfil: auth.perfil }))
    return auth
  },

  logout() {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(USER_KEY)
  },

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY)
  },

  getUser(): { nome: string; email: string; perfil: string } | null {
    const raw = localStorage.getItem(USER_KEY)
    return raw ? JSON.parse(raw) : null
  },

  isAuthenticated(): boolean {
    return !!localStorage.getItem(TOKEN_KEY)
  },
}
