export type RegimeTributario =
  | 'SimplesNacional'
  | 'LucroPresumido'
  | 'LucroReal'
  | 'ImunidadeIsencao'

export type StatusObrigacao =
  | 'Pendente'
  | 'Atrasada'
  | 'Entregue'
  | 'NaoAplicavel'

export type TipoObrigacao =
  | 'DAS'
  | 'DEFIS'
  | 'DCTF'
  | 'EFD_ICMS_IPI'
  | 'EFD_Contribuicoes'
  | 'EFD_Reinf'
  | 'SPED_ECD'
  | 'SPED_ECF'
  | 'ESocial'
  | 'DIRF'
  | 'RAIS'

export interface Empresa {
  id: string
  razaoSocial: string
  cnpj: string
  regimeTributario: RegimeTributario
  regimeTributarioDescricao: string
  ativo: boolean
  criadoEm: string
}

export interface EntregaObrigacao {
  id: string
  obrigacaoId: string
  dataEntrega: string
  observacao?: string
  criadoEm: string
}

export interface ObrigacaoAcessoria {
  id: string
  empresaId: string
  empresaNome: string
  tipo: TipoObrigacao
  tipoDescricao: string
  periodicidade: 'Mensal' | 'Anual'
  competencia: number
  anoCompetencia: number
  vencimento: string
  status: StatusObrigacao
  statusDescricao: string
  entrega?: EntregaObrigacao
}

export interface DashboardData {
  totalEmpresas: number
  obrigacoesMes: number
  pendentes: number
  entregues: number
  atrasadas: number
  mes: number
  ano: number
}

export interface AlertaObrigacao {
  obrigacaoId: string
  empresaId: string
  empresaNome: string
  cnpj: string
  tipo: TipoObrigacao
  tipoDescricao: string
  vencimento: string
  diasRestantes: number
  status: StatusObrigacao
  statusDescricao: string
}

export interface CriarEmpresaRequest {
  razaoSocial: string
  cnpj: string
  regimeTributario: RegimeTributario
}

export interface RegistrarEntregaRequest {
  dataEntrega: string
  observacao?: string
}
