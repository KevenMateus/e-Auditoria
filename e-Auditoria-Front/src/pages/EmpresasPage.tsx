import {
  Button, Form, Input, Modal, Select, Space, Table, Tabs, Tag, Tooltip, Typography, message,
} from 'antd'
import {
  PlusOutlined, DeleteOutlined, CalendarOutlined,
  ReloadOutlined, ExclamationCircleOutlined,
} from '@ant-design/icons'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { empresasService } from '../services/empresas'
import type { CriarEmpresaRequest, Empresa, RegimeTributario } from '../types'

const { Title, Text } = Typography

const REGIMES: { value: RegimeTributario; label: string }[] = [
  { value: 'SimplesNacional',   label: 'Simples Nacional' },
  { value: 'LucroPresumido',   label: 'Lucro Presumido' },
  { value: 'LucroReal',        label: 'Lucro Real' },
  { value: 'ImunidadeIsencao', label: 'Imunidade / Isenção' },
]

const REGIME_COLORS: Record<RegimeTributario, string> = {
  SimplesNacional:   '#00ACC1',
  LucroPresumido:    '#1565C0',
  LucroReal:         '#1E88E5',
  ImunidadeIsencao:  '#9e9e9e',
}

function formatCnpj(cnpj: string) {
  return cnpj.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5')
}

function RegimeTag({ record }: { record: Empresa }) {
  return (
    <Tag
      style={{
        background: REGIME_COLORS[record.regimeTributario] + '18',
        borderColor: REGIME_COLORS[record.regimeTributario] + '55',
        color: REGIME_COLORS[record.regimeTributario],
        fontWeight: 600,
      }}
    >
      {record.regimeTributarioDescricao}
    </Tag>
  )
}

// Payload da resposta 409 do backend quando o CNPJ pertence a empresa inativa
interface EmpresaInativaConflict {
  status: number
  mensagem: string
  empresaInativaId: string
  razaoSocial: string
}

export default function EmpresasPage() {
  const [form] = Form.useForm()
  const [modalOpen, setModalOpen] = useState(false)
  const [conflito, setConflito] = useState<EmpresaInativaConflict | null>(null)
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const [messageApi, contextHolder] = message.useMessage()

  const { data: empresas = [], isLoading } = useQuery({
    queryKey: ['empresas'],
    queryFn: empresasService.listar,
  })

  const { data: inativas = [], isLoading: isLoadingInativas } = useQuery({
    queryKey: ['empresas-inativas'],
    queryFn: empresasService.listarInativas,
  })

  const invalidarTudo = () => {
    queryClient.invalidateQueries({ queryKey: ['empresas'] })
    queryClient.invalidateQueries({ queryKey: ['empresas-inativas'] })
    queryClient.invalidateQueries({ queryKey: ['dashboard'] })
  }

  const criarMutation = useMutation({
    mutationFn: (data: CriarEmpresaRequest) => empresasService.criar(data),
    onSuccess: () => {
      invalidarTudo()
      messageApi.success('Empresa cadastrada com sucesso!')
      setModalOpen(false)
      form.resetFields()
    },
    onError: (err: unknown) => {
      // 409 → empresa inativa com mesmo CNPJ
      const axErr = err as { response?: { status: number; data: EmpresaInativaConflict } }
      if (axErr?.response?.status === 409) {
        setConflito(axErr.response.data)
      } else {
        messageApi.error((err as Error).message ?? 'Erro ao cadastrar empresa.')
      }
    },
  })

  const reativarMutation = useMutation({
    mutationFn: (id: string) => empresasService.reativar(id),
    onSuccess: (empresa) => {
      invalidarTudo()
      setConflito(null)
      setModalOpen(false)
      form.resetFields()
      messageApi.success(`"${empresa.razaoSocial}" reativada com sucesso!`)
    },
    onError: (err: Error) => messageApi.error(err.message),
  })

  const removerMutation = useMutation({
    mutationFn: (id: string) => empresasService.remover(id),
    onSuccess: () => {
      invalidarTudo()
      messageApi.success('Empresa desativada.')
    },
    onError: (err: Error) => messageApi.error(err.message),
  })

  const colunasBase = [
    {
      title: 'Razão Social',
      dataIndex: 'razaoSocial',
      key: 'razaoSocial',
      sorter: (a: Empresa, b: Empresa) => a.razaoSocial.localeCompare(b.razaoSocial),
    },
    {
      title: 'CNPJ',
      dataIndex: 'cnpj',
      key: 'cnpj',
      render: (cnpj: string) => (
        <span style={{ fontFamily: 'monospace' }}>{formatCnpj(cnpj)}</span>
      ),
    },
    {
      title: 'Regime',
      dataIndex: 'regimeTributarioDescricao',
      key: 'regime',
      render: (_: string, record: Empresa) => <RegimeTag record={record} />,
      filters: REGIMES.map((r) => ({ text: r.label, value: r.label })),
      onFilter: (value: unknown, record: Empresa) =>
        record.regimeTributarioDescricao === value,
    },
    {
      title: 'Cadastro',
      dataIndex: 'criadoEm',
      key: 'criadoEm',
      render: (v: string) => new Date(v).toLocaleDateString('pt-BR'),
    },
  ]

  const colunasAtivas = [
    ...colunasBase,
    {
      title: '',
      key: 'acoes',
      width: 100,
      render: (_: unknown, record: Empresa) => (
        <Space size="small">
          <Tooltip title="Ver calendário">
            <Button
              size="small"
              icon={<CalendarOutlined />}
              style={{ borderColor: '#1565C0', color: '#1565C0' }}
              onClick={() => navigate('/calendario', { state: { empresaId: record.id } })}
            />
          </Tooltip>
          <Tooltip title="Desativar empresa">
            <Button
              danger
              size="small"
              icon={<DeleteOutlined />}
              loading={removerMutation.isPending}
              onClick={() =>
                Modal.confirm({
                  title: 'Desativar empresa?',
                  content: `"${record.razaoSocial}" será desativada. Você poderá reativá-la depois na aba Inativas.`,
                  okText: 'Desativar',
                  okButtonProps: { danger: true },
                  cancelText: 'Cancelar',
                  onOk: () => removerMutation.mutate(record.id),
                })
              }
            />
          </Tooltip>
        </Space>
      ),
    },
  ]

  const colunasInativas = [
    ...colunasBase,
    {
      title: '',
      key: 'acoes',
      width: 80,
      render: (_: unknown, record: Empresa) => (
        <Tooltip title="Reativar empresa">
          <Button
            size="small"
            icon={<ReloadOutlined />}
            style={{ borderColor: '#2E7D32', color: '#2E7D32' }}
            loading={reativarMutation.isPending}
            onClick={() =>
              Modal.confirm({
                title: 'Reativar empresa?',
                content: (
                  <span>
                    <b>{record.razaoSocial}</b> será reativada e as obrigações dos próximos
                    12 meses serão geradas automaticamente.
                  </span>
                ),
                okText: 'Reativar',
                okButtonProps: { style: { background: '#2E7D32', borderColor: '#2E7D32' } },
                cancelText: 'Cancelar',
                onOk: () => reativarMutation.mutate(record.id),
              })
            }
          />
        </Tooltip>
      ),
    },
  ]

  return (
    <div>
      {contextHolder}

      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginBottom: 24,
        }}
      >
        <Title level={4} style={{ margin: 0, color: '#0D1B2A' }}>
          Empresas
        </Title>
        <Button
          type="primary"
          icon={<PlusOutlined />}
          onClick={() => setModalOpen(true)}
          style={{ background: '#1565C0' }}
        >
          Nova Empresa
        </Button>
      </div>

      <Tabs
        defaultActiveKey="ativas"
        style={{ background: '#fff', borderRadius: 8, padding: '0 16px' }}
        items={[
          {
            key: 'ativas',
            label: `Ativas${empresas.length > 0 ? ` (${empresas.length})` : ''}`,
            children: (
              <Table
                dataSource={empresas}
                columns={colunasAtivas}
                rowKey="id"
                loading={isLoading}
                pagination={{ pageSize: 15, showTotal: (t) => `${t} empresas` }}
                size="middle"
              />
            ),
          },
          {
            key: 'inativas',
            label: (
              <span>
                Inativas
                {inativas.length > 0 && (
                  <Tag
                    style={{
                      marginLeft: 6,
                      fontSize: 11,
                      lineHeight: '18px',
                      padding: '0 5px',
                    }}
                    color="default"
                  >
                    {inativas.length}
                  </Tag>
                )}
              </span>
            ),
            children: (
              <Table
                dataSource={inativas}
                columns={colunasInativas}
                rowKey="id"
                loading={isLoadingInativas}
                pagination={{ pageSize: 15, showTotal: (t) => `${t} empresas` }}
                size="middle"
                locale={{ emptyText: 'Nenhuma empresa inativa.' }}
              />
            ),
          },
        ]}
      />

      {/* Modal: cadastrar nova empresa */}
      <Modal
        title="Cadastrar Empresa"
        open={modalOpen}
        onCancel={() => { setModalOpen(false); form.resetFields() }}
        onOk={() => form.submit()}
        okText="Cadastrar"
        cancelText="Cancelar"
        confirmLoading={criarMutation.isPending}
        okButtonProps={{ style: { background: '#1565C0' } }}
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={(values: CriarEmpresaRequest) => criarMutation.mutate(values)}
          style={{ marginTop: 16 }}
        >
          <Form.Item
            label="Razão Social"
            name="razaoSocial"
            rules={[{ required: true, message: 'Informe a razão social' }]}
          >
            <Input placeholder="Nome da empresa" />
          </Form.Item>

          <Form.Item
            label="CNPJ"
            name="cnpj"
            rules={[
              { required: true, message: 'Informe o CNPJ' },
              { pattern: /^[\d.\-/]+$/, message: 'CNPJ inválido' },
            ]}
          >
            <Input placeholder="00.000.000/0000-00" maxLength={18} />
          </Form.Item>

          <Form.Item
            label="Regime Tributário"
            name="regimeTributario"
            rules={[{ required: true, message: 'Selecione o regime tributário' }]}
          >
            <Select placeholder="Selecione" options={REGIMES} />
          </Form.Item>
        </Form>
      </Modal>

      {/* Modal: CNPJ pertence a empresa inativa (409) */}
      <Modal
        title={
          <span style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
            <ExclamationCircleOutlined style={{ color: '#F57F17', fontSize: 20 }} />
            Empresa inativa encontrada
          </span>
        }
        open={!!conflito}
        onCancel={() => setConflito(null)}
        footer={[
          <Button key="cancelar" onClick={() => setConflito(null)}>
            Cancelar
          </Button>,
          <Button
            key="reativar"
            type="primary"
            loading={reativarMutation.isPending}
            style={{ background: '#2E7D32', borderColor: '#2E7D32' }}
            onClick={() => conflito && reativarMutation.mutate(conflito.empresaInativaId)}
          >
            Reativar empresa
          </Button>,
        ]}
      >
        <div style={{ padding: '12px 0' }}>
          <p>{conflito?.mensagem}</p>
          <p style={{ marginTop: 12 }}>
            <Text type="secondary">
              Deseja reativar <b>{conflito?.razaoSocial}</b>? As obrigações dos próximos
              12 meses serão geradas automaticamente.
            </Text>
          </p>
        </div>
      </Modal>
    </div>
  )
}
