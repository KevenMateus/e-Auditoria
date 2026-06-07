import {
  Button, Col, DatePicker, Empty, Form, Input, Modal, Row, Select,
  Space, Spin, Table, Tabs, Tag, Timeline, Tooltip, Typography, message,
} from 'antd'
import {
  CheckOutlined, ClockCircleOutlined, DownloadOutlined,
  HistoryOutlined, ReloadOutlined,
} from '@ant-design/icons'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import dayjs from 'dayjs'
import { useEffect, useState } from 'react'
import { useLocation } from 'react-router-dom'
import { empresasService } from '../services/empresas'
import { obrigacoesService } from '../services/obrigacoes'
import StatusBadge from '../components/StatusBadge'
import type { EntregaObrigacao, ObrigacaoAcessoria, RegistrarEntregaRequest, StatusObrigacao } from '../types'

const { Title, Text } = Typography

const STATUS_FILTROS: { value: StatusObrigacao | ''; label: string }[] = [
  { value: '', label: 'Todos os status' },
  { value: 'Pendente', label: 'Pendente' },
  { value: 'Atrasada', label: 'Atrasada' },
  { value: 'Entregue', label: 'Entregue' },
]

function CalendarioTab({
  empresaId, mes, ano, filtroStatus, setFiltroStatus, onMesChange,
}: {
  empresaId: string | null
  mes: number
  ano: number
  filtroStatus: StatusObrigacao | ''
  setFiltroStatus: (v: StatusObrigacao | '') => void
  onMesChange: (date: dayjs.Dayjs | null) => void
}) {
  const hoje = new Date()
  const [entregaModal, setEntregaModal] = useState<ObrigacaoAcessoria | null>(null)
  const [exportando, setExportando] = useState(false)
  const [form] = Form.useForm()
  const queryClient = useQueryClient()
  const [messageApi, contextHolder] = message.useMessage()

  const { data: obrigacoes = [], isLoading, refetch } = useQuery({
    queryKey: ['calendario', empresaId, mes, ano, filtroStatus],
    queryFn: () =>
      empresaId
        ? obrigacoesService.obterCalendario(empresaId, mes, ano, filtroStatus || undefined)
        : Promise.resolve([]),
    enabled: !!empresaId,
  })

  const gerarMutation = useMutation({
    mutationFn: () => obrigacoesService.gerar(empresaId!, mes, ano),
    onSuccess: async (data) => {
      queryClient.setQueryData(['calendario', empresaId, mes, ano, filtroStatus], data)
      await queryClient.invalidateQueries({ queryKey: ['calendario', empresaId] })
      messageApi.success('Obrigacoes geradas com sucesso!')
    },
    onError: (err: Error) => messageApi.error(err.message),
  })

  const entregaMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: RegistrarEntregaRequest }) =>
      obrigacoesService.registrarEntrega(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['calendario'] })
      queryClient.invalidateQueries({ queryKey: ['historico', empresaId] })
      queryClient.invalidateQueries({ queryKey: ['alertas'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
      messageApi.success('Entrega registrada!')
      setEntregaModal(null)
      form.resetFields()
    },
    onError: (err: Error) => messageApi.error(err.message),
  })

  async function handleExportarCsv() {
    if (!empresaId) return
    setExportando(true)
    try {
      await obrigacoesService.exportarCsv(empresaId, mes, ano)
      messageApi.success('CSV exportado com sucesso!')
    } catch {
      messageApi.error('Erro ao exportar CSV.')
    } finally {
      setExportando(false)
    }
  }

  const columns = [
    {
      title: 'Obrigacao',
      dataIndex: 'tipoDescricao',
      key: 'tipo',
      sorter: (a: ObrigacaoAcessoria, b: ObrigacaoAcessoria) =>
        a.tipoDescricao.localeCompare(b.tipoDescricao),
    },
    {
      title: 'Periodicidade',
      dataIndex: 'periodicidade',
      key: 'periodicidade',
      render: (v: string) => (
        <Tag color={v === 'Mensal' ? 'blue' : 'purple'} style={{ fontSize: 11 }}>{v}</Tag>
      ),
    },
    {
      title: 'Vencimento',
      dataIndex: 'vencimento',
      key: 'vencimento',
      render: (v: string) => {
        const data = new Date(v)
        const diff = Math.floor((data.getTime() - hoje.getTime()) / 86_400_000)
        const color = diff < 0 ? '#C62828' : diff <= 7 ? '#F57F17' : '#333'
        return (
          <Text style={{ color, fontWeight: diff <= 7 ? 600 : 400 }}>
            {data.toLocaleDateString('pt-BR')}
          </Text>
        )
      },
      sorter: (a: ObrigacaoAcessoria, b: ObrigacaoAcessoria) =>
        new Date(a.vencimento).getTime() - new Date(b.vencimento).getTime(),
      defaultSortOrder: 'ascend' as const,
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (status: StatusObrigacao) => <StatusBadge status={status} />,
    },
    {
      title: 'Entregue em',
      key: 'entrega',
      render: (_: unknown, record: ObrigacaoAcessoria) =>
        record.entrega
          ? <Text style={{ color: '#2E7D32' }}>{new Date(record.entrega.dataEntrega).toLocaleDateString('pt-BR')}</Text>
          : <Text type="secondary">-</Text>,
    },
    {
      title: '',
      key: 'acao',
      width: 60,
      render: (_: unknown, record: ObrigacaoAcessoria) =>
        record.status !== 'Entregue' ? (
          <Tooltip title="Registrar entrega">
            <Button
              type="primary"
              size="small"
              icon={<CheckOutlined />}
              style={{ background: '#2E7D32', borderColor: '#2E7D32' }}
              onClick={() => setEntregaModal(record)}
            />
          </Tooltip>
        ) : null,
    },
  ]

  return (
    <>
      {contextHolder}

      <Row gutter={[12, 12]} style={{ marginBottom: 20 }}>
        <Col xs={12} md={4}>
          <DatePicker
            picker="month"
            value={dayjs(`${ano}-${String(mes).padStart(2, '0')}-01`)}
            onChange={onMesChange}
            style={{ width: '100%' }}
            format="MM/YYYY"
          />
        </Col>
        <Col xs={12} md={4}>
          <Select
            value={filtroStatus}
            onChange={setFiltroStatus}
            style={{ width: '100%' }}
            options={STATUS_FILTROS}
          />
        </Col>
        <Col xs={24} md={16}>
          <Space wrap>
            <Button icon={<ReloadOutlined />} onClick={() => refetch()} disabled={!empresaId}>
              Atualizar
            </Button>
            <Button
              type="dashed"
              onClick={() => gerarMutation.mutate()}
              disabled={!empresaId}
              loading={gerarMutation.isPending}
            >
              Gerar Obrigacoes
            </Button>
            <Button
              icon={<DownloadOutlined />}
              onClick={handleExportarCsv}
              disabled={!empresaId || obrigacoes.length === 0}
              loading={exportando}
              style={{ borderColor: '#00ACC1', color: '#00ACC1' }}
            >
              Exportar CSV
            </Button>
          </Space>
        </Col>
      </Row>

      {!empresaId ? (
        <Empty description="Selecione uma empresa para visualizar o calendario." style={{ padding: 60 }} />
      ) : (
        <Spin spinning={isLoading}>
          <Table
            dataSource={obrigacoes}
            columns={columns}
            rowKey="id"
            pagination={false}
            size="middle"
            style={{ background: '#fff', borderRadius: 8 }}
            rowClassName={(record: ObrigacaoAcessoria) =>
              record.status === 'Atrasada' ? 'row-atrasada' : ''
            }
          />
        </Spin>
      )}

      <style>{`.row-atrasada td { background: #fff5f5 !important; }`}</style>

      <Modal
        title={`Registrar entrega - ${entregaModal?.tipoDescricao}`}
        open={!!entregaModal}
        onCancel={() => { setEntregaModal(null); form.resetFields() }}
        onOk={() => form.submit()}
        okText="Confirmar"
        cancelText="Cancelar"
        confirmLoading={entregaMutation.isPending}
        okButtonProps={{ style: { background: '#2E7D32', borderColor: '#2E7D32' } }}
      >
        <Form
          form={form}
          layout="vertical"
          initialValues={{ dataEntrega: dayjs() }}
          onFinish={(values) =>
            entregaMutation.mutate({
              id: entregaModal!.id,
              data: {
                dataEntrega: values.dataEntrega.toISOString(),
                observacao: values.observacao,
              },
            })
          }
          style={{ marginTop: 16 }}
        >
          <Form.Item
            label="Data de Entrega"
            name="dataEntrega"
            rules={[{ required: true, message: 'Informe a data de entrega' }]}
          >
            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
          </Form.Item>
          <Form.Item label="Observacao" name="observacao">
            <Input.TextArea rows={3} placeholder="Opcional" />
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}

function HistoricoTab({ empresaId }: { empresaId: string | null }) {
  const { data: historico = [], isLoading } = useQuery({
    queryKey: ['historico', empresaId],
    queryFn: () => obrigacoesService.obterHistorico(empresaId!),
    enabled: !!empresaId,
  })

  if (!empresaId) {
    return <Empty description="Selecione uma empresa para ver o historico." style={{ padding: 60 }} />
  }

  if (isLoading) {
    return <Spin style={{ display: 'block', padding: 60, textAlign: 'center' }} />
  }

  if (historico.length === 0) {
    return <Empty description="Nenhuma entrega registrada para esta empresa." style={{ padding: 60 }} />
  }

  const itens = [...historico]
    .sort((a: EntregaObrigacao, b: EntregaObrigacao) =>
      new Date(b.dataEntrega).getTime() - new Date(a.dataEntrega).getTime()
    )
    .map((e: EntregaObrigacao) => ({
      color: '#2E7D32',
      dot: <CheckOutlined style={{ fontSize: 14 }} />,
      children: (
        <div style={{ paddingBottom: 8 }}>
          <div style={{ fontWeight: 600, color: '#0D1B2A' }}>
            {new Date(e.dataEntrega).toLocaleDateString('pt-BR', {
              day: '2-digit', month: 'long', year: 'numeric',
            })}
          </div>
          {e.observacao && (
            <div style={{ color: '#64748b', fontSize: 13, marginTop: 2 }}>
              {e.observacao}
            </div>
          )}
          <div style={{ fontSize: 11, color: '#94a3b8', marginTop: 4 }}>
            Registrado em {new Date(e.criadoEm).toLocaleDateString('pt-BR')}
          </div>
        </div>
      ),
    }))

  return (
    <div style={{ maxWidth: 600, paddingTop: 8 }}>
      <Timeline mode="left" items={itens} />
    </div>
  )
}

export default function CalendarioPage() {
  const location = useLocation()
  const hoje = new Date()

  const [empresaId, setEmpresaId] = useState<string | null>(
    (location.state as { empresaId?: string } | null)?.empresaId ?? null
  )
  const [mes, setMes] = useState(hoje.getMonth() + 1)
  const [ano, setAno] = useState(hoje.getFullYear())
  const [filtroStatus, setFiltroStatus] = useState<StatusObrigacao | ''>('')
  const [abaAtiva, setAbaAtiva] = useState('calendario')

  useEffect(() => {
    if ((location.state as { empresaId?: string } | null)?.empresaId) {
      window.history.replaceState({}, '')
    }
  }, [location.state])

  const { data: empresas = [] } = useQuery({
    queryKey: ['empresas'],
    queryFn: empresasService.listar,
  })

  function handleMes(date: dayjs.Dayjs | null) {
    if (!date) return
    setMes(date.month() + 1)
    setAno(date.year())
  }

  const empresaSelecionada = empresas.find((e) => e.id === empresaId)

  return (
    <div>
      <Title level={4} style={{ marginBottom: 20, color: '#0D1B2A' }}>
        Calendario de Obrigacoes
      </Title>

      <Row gutter={[12, 12]} style={{ marginBottom: 20 }}>
        <Col xs={24} md={10}>
          <Select
            placeholder="Selecione uma empresa"
            style={{ width: '100%' }}
            value={empresaId}
            onChange={(v) => { setEmpresaId(v); setAbaAtiva('calendario') }}
            showSearch
            allowClear
            optionFilterProp="label"
            options={empresas.map((e) => ({ value: e.id, label: e.razaoSocial }))}
          />
        </Col>
        {empresaSelecionada && (
          <Col>
            <Tag
              style={{
                height: 32, lineHeight: '30px', fontSize: 13,
                background: '#1565C018', borderColor: '#1565C055', color: '#1565C0',
              }}
            >
              {empresaSelecionada.regimeTributarioDescricao}
            </Tag>
          </Col>
        )}
      </Row>

      <Tabs
        activeKey={abaAtiva}
        onChange={setAbaAtiva}
        style={{ background: '#fff', borderRadius: 8, padding: '0 16px' }}
        items={[
          {
            key: 'calendario',
            label: (
              <span>
                <ClockCircleOutlined style={{ marginRight: 6 }} />
                Calendario
              </span>
            ),
            children: (
              <div style={{ paddingBottom: 16 }}>
                <CalendarioTab
                  empresaId={empresaId}
                  mes={mes}
                  ano={ano}
                  filtroStatus={filtroStatus}
                  setFiltroStatus={setFiltroStatus}
                  onMesChange={handleMes}
                />
              </div>
            ),
          },
          {
            key: 'historico',
            label: (
              <span>
                <HistoryOutlined style={{ marginRight: 6 }} />
                Historico de Entregas
              </span>
            ),
            children: (
              <div style={{ paddingBottom: 16 }}>
                <HistoricoTab empresaId={empresaId} />
              </div>
            ),
          },
        ]}
      />
    </div>
  )
}
