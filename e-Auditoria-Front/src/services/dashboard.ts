import api from './api'
import type { DashboardData, AlertaObrigacao } from '../types'

export const dashboardService = {
  obter: (mes?: number, ano?: number) =>
    api.get<DashboardData>('/dashboard', { params: { mes, ano } }).then((r) => r.data),

  obterAlertas: () =>
    api.get<AlertaObrigacao[]>('/dashboard/alertas').then((r) => r.data),
}
