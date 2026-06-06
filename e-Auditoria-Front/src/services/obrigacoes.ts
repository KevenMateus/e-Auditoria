import api from './api'
import type { EntregaObrigacao, ObrigacaoAcessoria, RegistrarEntregaRequest, StatusObrigacao } from '../types'

export const obrigacoesService = {
  obterCalendario: (empresaId: string, mes: number, ano: number, status?: StatusObrigacao) =>
    api
      .get<ObrigacaoAcessoria[]>('/obrigacoes/calendario', {
        params: { empresaId, mes, ano, status },
      })
      .then((r) => r.data),

  gerar: (empresaId: string, mes: number, ano: number) =>
    api
      .post<ObrigacaoAcessoria[]>('/obrigacoes/gerar', { empresaId, mes, ano })
      .then((r) => r.data),

  registrarEntrega: (obrigacaoId: string, data: RegistrarEntregaRequest) =>
    api
      .post(`/entregas/obrigacoes/${obrigacaoId}`, data)
      .then((r) => r.data),

  obterHistorico: (empresaId: string) =>
    api.get<EntregaObrigacao[]>(`/entregas/historico/${empresaId}`).then((r) => r.data),

  exportarCsv: (empresaId: string, mes: number, ano: number) =>
    api
      .get('/obrigacoes/exportar', {
        params: { empresaId, mes, ano },
        responseType: 'blob',
      })
      .then((r) => {
        const url = window.URL.createObjectURL(new Blob([r.data]))
        const link = document.createElement('a')
        link.href = url
        link.setAttribute('download', `obrigacoes_${String(mes).padStart(2, '0')}_${ano}.csv`)
        document.body.appendChild(link)
        link.click()
        link.remove()
        window.URL.revokeObjectURL(url)
      }),
}
