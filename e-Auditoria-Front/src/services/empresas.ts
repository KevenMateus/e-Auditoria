import api from './api'
import type { Empresa, CriarEmpresaRequest } from '../types'

export const empresasService = {
  listar: () =>
    api.get<Empresa[]>('/empresas').then((r) => r.data),

  listarInativas: () =>
    api.get<Empresa[]>('/empresas/inativas').then((r) => r.data),

  criar: (data: CriarEmpresaRequest) =>
    api.post<Empresa>('/empresas', data).then((r) => r.data),

  reativar: (id: string) =>
    api.post<Empresa>(`/empresas/${id}/reativar`).then((r) => r.data),

  remover: (id: string) =>
    api.delete(`/empresas/${id}`),
}
