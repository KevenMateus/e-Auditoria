import type { CSSProperties } from 'react'
import {
  Alert,
  Button,
  Card,
  Col,
  Progress,
  Row,
  Select,
  Space,
  Spin,
  Tag,
  Typography,
  message,
} from 'antd'
import {
  BankOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  DatabaseOutlined,
  ExclamationCircleOutlined,
  FileTextOutlined,
  RiseOutlined,
  WarningOutlined,
} from '@ant-design/icons'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import {
  Cell,
  Legend,
  Pie,
  PieChart,
  ResponsiveContainer,
  Tooltip,
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
} from 'recharts'
import { dashboardService } from '../services/dashboard'
import { empresasService } from '../services/empresas'
import { adminService } from '../services/admin'
import type { AlertaObrigacao } from '../types'

const { Title, Text } = Typography

const MESES = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

const STATUS_COLORS = {
  Entregues: '#2E7D32',
  Pendentes: '#1E88E5',
  Atrasadas: '#C62828',
}

function agruparAlertasPorSemana(alertas: AlertaObrigacao[]) {
  const grupos: Record<string, { label: string; Atrasadas: number; Urgentes: number; Normal: number }> = {}

  for (const a of alertas) {
    let faixa: string
    let label: string

    if (a.diasRestantes < 0) {
      faixa = 'atrasadas'
      label = 'Atrasadas'
    } else if (a.diasRestantes <= 7) {
      faixa = 'semana1'
      label = '0–7 dias'
    } else if (a.diasRestantes <= 14) {
      faixa = 'semana2'
      label = '8–14 dias'
    } else if (a.diasRestantes <= 21) {
      faixa = 'semana3'
      label = '15–21 dias'
    } else {
      faixa = 'semana4'
      label = '22–30 dias'
    }

    if (!grupos[faixa]) {
      grupos[faixa] = { label, Atrasadas: 0, Urgentes: 0, Normal: 0 }
    }

    if (a.diasRestantes < 0) grupos[faixa].Atrasadas++
    else if (a.diasRestantes <= 7) grupos[faixa].Urgentes++
    else grupos[faixa].Normal++
  }

  return ['atrasadas', 'semana1', 'semana2', 'semana3', 'semana4']
    .filter((k) => grupos[k])
    .map((k) => grupos[k])
}

function CustomPieLabel({ cx, cy, midAngle, innerRadius, outerRadius, percent }: any) {
  if (percent < 0.05) return null
  const RADIAN = Math.PI / 180
  const radius = innerRadius + (outerRadius - innerRadius) * 0.6
  const x = cx + radius * Math.cos(-midAngle * RADIAN)
  const y = cy + radius * Math.sin(-midAngle * RADIAN)
  return (
    <text x={x} y={y} fill="white" textAnchor="middle" dominantBaseline="central" fontSize={13} fontWeight={700}>
      {`${(percent * 100).toFixed(0)}%`}
    </text>
  )
}

export default function DashboardPage() {
  const hoje = new Date()
  const [mes, setMes] = useState(hoje.getMonth() + 1)
  const [ano, setAno] = useState(hoje.getFullYear())
  const queryClient = useQueryClient()
  const [messageApi, contextHolder] = message.useMessage()

  const { data, isLoading } = useQuery({
    queryKey: ['dashboard', mes, ano],
    queryFn: () => dashboardService.obter(mes, ano),
  })

  const { data: empresas = [] } = useQuery({
    queryKey: ['empresas'],
    queryFn: empresasService.listar,
  })

  const { data: alertas = [] } = useQuery({
    queryKey: ['alertas'],
    queryFn: dashboardService.obterAlertas,
  })

  const seedMutation = useMutation({
    mutationFn: adminService.seed,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
      queryClient.invalidateQueries({ queryKey: ['empresas'] })
      queryClient.invalidateQueries({ queryKey: ['alertas'] })
      messageApi.success('Dados de demonstração carregados com sucesso!')
    },
    onError: () => messageApi.error('Erro ao carregar dados de demonstração.'),
  })

  const anos = Array.from({ length: 5 }, (_, i) => hoje.getFullYear() - 2 + i)
  const semObrigacoes = data?.obrigacoesMes === 0 && empresas.length > 0

  const pieData = data && data.obrigacoesMes > 0
    ? [
        { name: 'Entregues', value: data.entregues, color: STATUS_COLORS.Entregues },
        { name: 'Pendentes', value: data.pendentes, color: STATUS_COLORS.Pendentes },
        { name: 'Atrasadas', value: data.atrasadas, color: STATUS_COLORS.Atrasadas },
      ].filter((d) => d.value > 0)
    : []

  const barData = agruparAlertasPorSemana(alertas)

  const taxaEntrega = data && data.obrigacoesMes > 0
    ? Math.round((data.entregues / data.obrigacoesMes) * 100)
    : 0
  const taxaAtraso = data && data.obrigacoesMes > 0
    ? Math.round((data.atrasadas / data.obrigacoesMes) * 100)
    : 0
  const urgentes = alertas.filter((a) => a.diasRestantes >= 0 && a.diasRestantes <= 7).length
  const totalAtrasadas = alertas.filter((a) => a.diasRestantes < 0).length

  return (
    <div>
      {contextHolder}

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 }}>
        <div>
          <Title level={4} style={{ margin: 0, color: '#0D1B2A' }}>Dashboard</Title>
          <Text type="secondary" style={{ fontSize: 13 }}>
            {MESES[mes - 1]} {ano} — visão consolidada das obrigações
          </Text>
        </div>
        <Space>
          <Select
            value={mes}
            onChange={setMes}
            style={{ width: 140 }}
            options={MESES.map((m, i) => ({ value: i + 1, label: m }))}
          />
          <Select
            value={ano}
            onChange={setAno}
            style={{ width: 100 }}
            options={anos.map((a) => ({ value: a, label: String(a) }))}
          />
        </Space>
      </div>

      {(empresas.length === 0 || semObrigacoes) && (
        <Alert
          type="info"
          showIcon
          style={{ marginBottom: 24, borderRadius: 8 }}
          message="Banco sem dados de demonstração"
          description={
            empresas.length === 0
              ? 'Nenhuma empresa cadastrada. Carregue os dados de demonstração para ver o sistema funcionando.'
              : 'Empresas encontradas sem obrigações geradas. Clique para popular o calendário com dados de demonstração.'
          }
          action={
            <Button
              type="primary"
              icon={<DatabaseOutlined />}
              loading={seedMutation.isPending}
              onClick={() => seedMutation.mutate()}
              style={{ background: '#1565C0' }}
            >
              Carregar dados demo
            </Button>
          }
        />
      )}

      <Spin spinning={isLoading}>

        <Row gutter={[16, 16]}>
          <Col xs={12} sm={8} lg={4}>
            <Card bordered={false} style={cardStyle('#EEF2FF')}>
              <div style={kpiIcon('#1565C0')}><BankOutlined /></div>
              <div style={kpiValue('#1565C0')}>{data?.totalEmpresas ?? 0}</div>
              <div style={kpiLabel}>Empresas</div>
            </Card>
          </Col>

          <Col xs={12} sm={8} lg={4}>
            <Card bordered={false} style={cardStyle('#E0F7FA')}>
              <div style={kpiIcon('#00ACC1')}><FileTextOutlined /></div>
              <div style={kpiValue('#00ACC1')}>{data?.obrigacoesMes ?? 0}</div>
              <div style={kpiLabel}>Obrigações no Mês</div>
            </Card>
          </Col>

          <Col xs={12} sm={8} lg={4}>
            <Card bordered={false} style={cardStyle('#E8F5E9')}>
              <div style={kpiIcon('#2E7D32')}><CheckCircleOutlined /></div>
              <div style={kpiValue('#2E7D32')}>{data?.entregues ?? 0}</div>
              <div style={kpiLabel}>Entregues</div>
            </Card>
          </Col>

          <Col xs={12} sm={8} lg={4}>
            <Card bordered={false} style={cardStyle('#E3F2FD')}>
              <div style={kpiIcon('#1E88E5')}><ClockCircleOutlined /></div>
              <div style={kpiValue('#1E88E5')}>{data?.pendentes ?? 0}</div>
              <div style={kpiLabel}>Pendentes</div>
            </Card>
          </Col>

          <Col xs={12} sm={8} lg={4}>
            <Card bordered={false} style={cardStyle('#FFEBEE')}>
              <div style={kpiIcon('#C62828')}><WarningOutlined /></div>
              <div style={kpiValue('#C62828')}>{data?.atrasadas ?? 0}</div>
              <div style={kpiLabel}>Atrasadas</div>
            </Card>
          </Col>

          <Col xs={12} sm={8} lg={4}>
            <Card bordered={false} style={cardStyle('#FFF8E1')}>
              <div style={kpiIcon('#F57F17')}><ExclamationCircleOutlined /></div>
              <div style={kpiValue('#F57F17')}>{urgentes}</div>
              <div style={kpiLabel}>Vencem em 7 dias</div>
            </Card>
          </Col>
        </Row>

        {data && data.obrigacoesMes > 0 && (
          <>
            <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
              <Col xs={24} md={12}>
                <Card bordered={false} style={{ borderRadius: 12, height: '100%' }}>
                  <Title level={5} style={{ color: '#0D1B2A', marginBottom: 20 }}>
                    <RiseOutlined style={{ marginRight: 8, color: '#1565C0' }} />
                    Desempenho do Mês
                  </Title>

                  <div style={{ marginBottom: 20 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
                      <Text strong>Taxa de Entrega</Text>
                      <Text strong style={{ color: '#2E7D32' }}>{taxaEntrega}%</Text>
                    </div>
                    <Progress
                      percent={taxaEntrega}
                      strokeColor="#2E7D32"
                      trailColor="#E8F5E9"
                      strokeWidth={10}
                      showInfo={false}
                    />
                  </div>

                  <div style={{ marginBottom: 20 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
                      <Text strong>Taxa de Atraso</Text>
                      <Text strong style={{ color: '#C62828' }}>{taxaAtraso}%</Text>
                    </div>
                    <Progress
                      percent={taxaAtraso}
                      strokeColor="#C62828"
                      trailColor="#FFEBEE"
                      strokeWidth={10}
                      showInfo={false}
                    />
                  </div>

                  <div style={{ marginBottom: 4 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: 6 }}>
                      <Text strong>Pendentes</Text>
                      <Text strong style={{ color: '#1E88E5' }}>
                        {data.obrigacoesMes > 0
                          ? Math.round((data.pendentes / data.obrigacoesMes) * 100)
                          : 0}%
                      </Text>
                    </div>
                    <Progress
                      percent={data.obrigacoesMes > 0
                        ? Math.round((data.pendentes / data.obrigacoesMes) * 100)
                        : 0}
                      strokeColor="#1E88E5"
                      trailColor="#E3F2FD"
                      strokeWidth={10}
                      showInfo={false}
                    />
                  </div>

                  <div style={{ display: 'flex', gap: 8, marginTop: 24, flexWrap: 'wrap' }}>
                    <Tag color="success" style={{ borderRadius: 20, padding: '2px 12px' }}>
                      {data.entregues} entregues
                    </Tag>
                    <Tag color="processing" style={{ borderRadius: 20, padding: '2px 12px' }}>
                      {data.pendentes} pendentes
                    </Tag>
                    <Tag color="error" style={{ borderRadius: 20, padding: '2px 12px' }}>
                      {data.atrasadas} atrasadas
                    </Tag>
                  </div>
                </Card>
              </Col>

              <Col xs={24} md={12}>
                <Card bordered={false} style={{ borderRadius: 12, height: '100%' }}>
                  <Title level={5} style={{ color: '#0D1B2A', marginBottom: 4 }}>
                    Distribuição de Status
                  </Title>
                  <Text type="secondary" style={{ fontSize: 12 }}>
                    {MESES[mes - 1]} {ano}
                  </Text>

                  <ResponsiveContainer width="100%" height={220}>
                    <PieChart>
                      <Pie
                        data={pieData}
                        cx="50%"
                        cy="50%"
                        innerRadius={55}
                        outerRadius={90}
                        paddingAngle={3}
                        dataKey="value"
                        labelLine={false}
                        label={CustomPieLabel}
                      >
                        {pieData.map((entry, index) => (
                          <Cell key={`cell-${index}`} fill={entry.color} />
                        ))}
                      </Pie>
                      <text x="50%" y="46%" textAnchor="middle" dominantBaseline="central"
                        style={{ fontSize: 22, fontWeight: 700, fill: '#0D1B2A' }}>
                        {data.obrigacoesMes}
                      </text>
                      <text x="50%" y="56%" textAnchor="middle" dominantBaseline="central"
                        style={{ fontSize: 11, fill: '#64748b' }}>
                        total
                      </text>
                      <Tooltip
                        formatter={(value: number, name: string) => [value, name]}
                        contentStyle={{ borderRadius: 8, border: 'none', boxShadow: '0 4px 12px rgba(0,0,0,0.1)' }}
                      />
                      <Legend
                        iconType="circle"
                        iconSize={10}
                        formatter={(value) => (
                          <span style={{ fontSize: 12, color: '#374151' }}>{value}</span>
                        )}
                      />
                    </PieChart>
                  </ResponsiveContainer>
                </Card>
              </Col>
            </Row>

            {barData.length > 0 && (
              <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
                <Col xs={24}>
                  <Card bordered={false} style={{ borderRadius: 12 }}>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: 4 }}>
                      <div>
                        <Title level={5} style={{ color: '#0D1B2A', marginBottom: 2 }}>
                          Alertas por Janela de Tempo
                        </Title>
                        <Text type="secondary" style={{ fontSize: 12 }}>
                          {alertas.length} obrigações monitoradas — {totalAtrasadas} atrasadas, {urgentes} vencem em até 7 dias
                        </Text>
                      </div>
                      <Space>
                        {totalAtrasadas > 0 && (
                          <Tag color="error" style={{ borderRadius: 20 }}>
                            {totalAtrasadas} atrasadas
                          </Tag>
                        )}
                        {urgentes > 0 && (
                          <Tag color="warning" style={{ borderRadius: 20 }}>
                            {urgentes} urgentes
                          </Tag>
                        )}
                      </Space>
                    </div>

                    <ResponsiveContainer width="100%" height={220}>
                      <BarChart data={barData} barCategoryGap="30%">
                        <CartesianGrid strokeDasharray="3 3" stroke="#F1F5F9" />
                        <XAxis
                          dataKey="label"
                          tick={{ fontSize: 12, fill: '#64748b' }}
                          axisLine={false}
                          tickLine={false}
                        />
                        <YAxis
                          tick={{ fontSize: 12, fill: '#64748b' }}
                          axisLine={false}
                          tickLine={false}
                          allowDecimals={false}
                        />
                        <Tooltip
                          contentStyle={{
                            borderRadius: 8,
                            border: 'none',
                            boxShadow: '0 4px 12px rgba(0,0,0,0.1)',
                          }}
                        />
                        <Legend
                          iconType="circle"
                          iconSize={10}
                          formatter={(value) => (
                            <span style={{ fontSize: 12, color: '#374151' }}>{value}</span>
                          )}
                        />
                        <Bar dataKey="Atrasadas" fill="#C62828" radius={[4, 4, 0, 0]} />
                        <Bar dataKey="Urgentes" fill="#F57F17" radius={[4, 4, 0, 0]} />
                        <Bar dataKey="Normal" fill="#1E88E5" radius={[4, 4, 0, 0]} name="Dentro do prazo" />
                      </BarChart>
                    </ResponsiveContainer>
                  </Card>
                </Col>
              </Row>
            )}
          </>
        )}

        {data && data.obrigacoesMes === 0 && empresas.length > 0 && (
          <Card bordered={false} style={{ marginTop: 16, borderRadius: 12, textAlign: 'center', padding: '40px 0' }}>
            <FileTextOutlined style={{ fontSize: 48, color: '#CBD5E1', marginBottom: 16 }} />
            <div style={{ color: '#94A3B8', fontSize: 14 }}>
              Nenhuma obrigação encontrada para {MESES[mes - 1]} {ano}.
            </div>
          </Card>
        )}

      </Spin>
    </div>
  )
}

const cardStyle = (bg: string): CSSProperties => ({
  background: bg,
  borderRadius: 12,
  position: 'relative',
  overflow: 'hidden',
})

const kpiIcon = (color: string): CSSProperties => ({
  fontSize: 20,
  color,
  marginBottom: 8,
  opacity: 0.7,
})

const kpiValue = (color: string): CSSProperties => ({
  fontSize: 28,
  fontWeight: 800,
  color,
  lineHeight: 1.1,
  marginBottom: 4,
})

const kpiLabel: CSSProperties = {
  fontSize: 11,
  color: '#64748b',
  fontWeight: 500,
  textTransform: 'uppercase',
  letterSpacing: '0.5px',
}
