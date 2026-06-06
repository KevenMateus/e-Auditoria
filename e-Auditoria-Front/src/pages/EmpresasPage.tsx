import {
  Button, Form, Input, Modal, Select, Space, Table, Tag, Tooltip, Typography, message,
} from 'antd'
import { PlusOutlined, DeleteOutlined, CalendarOutlined } from '@ant-design/icons'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { empresasService } from '../services/empresas'
import type { CriarEmpresaRequest, Empresa, RegimeTributario } from '../types'

const { Title } = Typography

const REGIMES: { value: RegimeTributario; label: string }[] = [
  { value: 'SimplesNacional', label: 'Simples Nacional' },
  { value: 'LucroPresumido', label: 'Lucro Presumido' },
  { value: 'LucroReal', label: 'Lucro Real' },
  { value: 'ImunidadeIsencao', label: 'Imunidade / Isenção' },
]

const REGIME_COLORS: Record<RegimeTributario, string> = {
  SimplesNacional: '#00ACC1',
  LucroPresumido: '#1565C0',
  LucroReal: '#1E88E5',
  ImunidadeIsencao: '#9e9e9e',
}

function formatCnpj(cnpj: string) {
  return cnpj.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5')
}

export default function EmpresasPage() {
  const [form] = Form.useForm()
  const [modalOpen, setModalOpen] = useState(false)
  const queryClient = useQueryClient()
  const navigate = useNavigate()
  const [messageApi, contextHolder] = message.useMessage()

  const { data: empresas = [], isLoading } = useQuery({
    queryKey: ['empresas'],
    queryFn: empresasService.listar,
  })

  const criarMutation = useMutation({
    mutationFn: (data: CriarEmpresaRequest) => empresasService.criar(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['empresas'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
      messageApi.success('Empresa cadastrada com sucesso!')
      setModalOpen(false)
      form.resetFields()
    },
    onError: (err: Error) => {
      messageApi.error(err.message)
    },
  })

  const removerMutation = useMutation({
    mutationFn: (id: string) => empresasService.remover(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['empresas'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
      messageApi.success('Empresa removida.')
    },
    onError: (err: Error) => {
      messageApi.error(err.message)
    },
  })

  const columns = [
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
      render: (desc: string, record: Empresa) => (
        <Tag
          style={{
            background: REGIME_COLORS[record.regimeTributario] + '18',
            borderColor: REGIME_COLORS[record.regimeTributario] + '55',
            color: REGIME_COLORS[record.regimeTributario],
            fontWeight: 600,
          }}
        >
          {desc}
        </Tag>
      ),
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
              onClick={() =>
                navigate('/calendario', { state: { empresaId: record.id } })
              }
            />
          </Tooltip>
          <Tooltip title="Remover empresa">
            <Button
              danger
              size="small"
              icon={<DeleteOutlined />}
              loading={removerMutation.isPending}
              onClick={() =>
                Modal.confirm({
                  title: 'Remover empresa?',
                  content: `"${record.razaoSocial}" será desativada.`,
                  okText: 'Remover',
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

      <Table
        dataSource={empresas}
        columns={columns}
        rowKey="id"
        loading={isLoading}
        pagination={{ pageSize: 15, showTotal: (total) => `${total} empresas` }}
        style={{ background: '#fff', borderRadius: 8 }}
        size="middle"
      />

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
    </div>
  )
}
