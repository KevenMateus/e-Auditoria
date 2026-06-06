import { Button, Card, Col, DatePicker, Form, Input, Modal, Row, Spin, Table, Tooltip, Typography, message } from 'antd'
import { CheckOutlined, ExclamationCircleOutlined, WarningOutlined } from '@ant-design/icons'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import dayjs from 'dayjs'
import { dashboardService } from '../services/dashboard'
import { obrigacoesService } from '../services/obrigacoes'
import StatusBadge from '../components/StatusBadge'
import type { AlertaObrigacao, RegistrarEntregaRequest } from '../types'

const { Title, Text } = Typography

function formatCnpj(cnpj: string) {
  return cnpj.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2})$/, '$1.$2.$3/$4-$5')
}

function DiasRestantesCell({ dias }: { dias: number }) {
  if (dias < 0)
    return <Text strong style={{ color: '#C62828' }}>{Math.abs(dias)}d em atraso</Text>
  if (dias === 0)
    return <Text strong style={{ color: '#C62828' }}>Vence hoje</Text>
  if (dias <= 7)
    return <Text strong style={{ color: '#F57F17' }}>Em {dias}d</Text>
  return <Text style={{ color: '#1565C0' }}>Em {dias}d</Text>
}

export default function AlertasPage() {
  const [entregaModal, setEntregaModal] = useState<AlertaObrigacao | null>(null)
  const [form] = Form.useForm()
  const queryClient = useQueryClient()
  const [messageApi, contextHolder] = message.useMessage()

  const { data: alertas = [], isLoading } = useQuery({
    queryKey: ['alertas'],
    queryFn: dashboardService.obterAlertas,
    refetchInterval: 60_000,
  })

  const entregaMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: RegistrarEntregaRequest }) =>
      obrigacoesService.registrarEntrega(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['alertas'] })
      queryClient.invalidateQueries({ queryKey: ['calendario'] })
      queryClient.invalidateQueries({ queryKey: ['historico'] })
      queryClient.invalidateQueries({ queryKey: ['dashboard'] })
      messageApi.success('Entrega registrada!')
      setEntregaModal(null)
      form.resetFields()
    },
    onError: (err: Error) => messageApi.error(err.message),
  })

  const atrasadas = alertas.filter((a) => a.diasRestantes < 0)
  const vencendo = alertas.filter((a) => a.diasRestantes >= 0)

  const columns = [
    {
      title: 'Empresa',
      key: 'empresa',
      render: (_: unknown, record: AlertaObrigacao) => (
        <div>
          <div style={{ fontWeight: 600 }}>{record.empresaNome}</div>
          <div style={{ fontSize: 12, color: '#64748b', fontFamily: 'monospace' }}>
            {formatCnpj(record.cnpj)}
          </div>
        </div>
      ),
    },
    {
      title: 'Obrigação',
      dataIndex: 'tipoDescricao',
      key: 'tipo',
    },
    {
      title: 'Vencimento',
      dataIndex: 'vencimento',
      key: 'vencimento',
      render: (v: string) => new Date(v).toLocaleDateString('pt-BR'),
    },
    {
      title: 'Prazo',
      dataIndex: 'diasRestantes',
      key: 'diasRestantes',
      render: (dias: number) => <DiasRestantesCell dias={dias} />,
    },
    {
      title: 'Status',
      dataIndex: 'status',
      key: 'status',
      render: (_: unknown, record: AlertaObrigacao) => <StatusBadge status={record.status} />,
    },
    {
      title: '',
      key: 'acao',
      width: 60,
      render: (_: unknown, record: AlertaObrigacao) => (
        <Tooltip title="Registrar entrega">
          <Button
            type="primary"
            size="small"
            icon={<CheckOutlined />}
            style={{ background: '#2E7D32', borderColor: '#2E7D32' }}
            onClick={() => setEntregaModal(record)}
          />
        </Tooltip>
      ),
    },
  ]

  return (
    <div>
      {contextHolder}

      <Title level={4} style={{ marginBottom: 24, color: '#0D1B2A' }}>
        Painel de Alertas
      </Title>

      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={12}>
          <Card bordered={false} style={{ borderRadius: 8, borderLeft: '4px solid #C62828' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
              <ExclamationCircleOutlined style={{ color: '#C62828', fontSize: 24 }} />
              <div>
                <div style={{ fontSize: 28, fontWeight: 700, color: '#C62828', lineHeight: 1 }}>
                  {atrasadas.length}
                </div>
                <div style={{ color: '#64748b', fontSize: 13 }}>Obrigações atrasadas</div>
              </div>
            </div>
          </Card>
        </Col>
        <Col xs={24} sm={12}>
          <Card bordered={false} style={{ borderRadius: 8, borderLeft: '4px solid #F57F17' }}>
            <div style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
              <WarningOutlined style={{ color: '#F57F17', fontSize: 24 }} />
              <div>
                <div style={{ fontSize: 28, fontWeight: 700, color: '#F57F17', lineHeight: 1 }}>
                  {vencendo.length}
                </div>
                <div style={{ color: '#64748b', fontSize: 13 }}>Vencendo em 30 dias</div>
              </div>
            </div>
          </Card>
        </Col>
      </Row>

      <Spin spinning={isLoading}>
        <Table
          dataSource={alertas}
          columns={columns}
          rowKey="obrigacaoId"
          pagination={{ pageSize: 20, showTotal: (total) => `${total} alertas` }}
          style={{ background: '#fff', borderRadius: 8 }}
          size="middle"
          rowClassName={(record: AlertaObrigacao) =>
            record.diasRestantes < 0
              ? 'row-atrasada'
              : record.diasRestantes <= 7
              ? 'row-urgente'
              : ''
          }
        />
      </Spin>

      <style>{`
        .row-atrasada td { background: #fff5f5 !important; }
        .row-urgente td  { background: #fffde7 !important; }
      `}</style>

      <Modal
        title={`Registrar entrega — ${entregaModal?.tipoDescricao}`}
        open={!!entregaModal}
        onCancel={() => { setEntregaModal(null); form.resetFields() }}
        onOk={() => form.submit()}
        okText="Confirmar"
        cancelText="Cancelar"
        confirmLoading={entregaMutation.isPending}
        okButtonProps={{ style: { background: '#2E7D32', borderColor: '#2E7D32' } }}
      >
        {entregaModal && (
          <div style={{ marginBottom: 12, padding: '8px 12px', background: '#f8fafc', borderRadius: 6 }}>
            <Text style={{ fontSize: 13, color: '#64748b' }}>
              {entregaModal.empresaNome} · {formatCnpj(entregaModal.cnpj)}
            </Text>
          </div>
        )}
        <Form
          form={form}
          layout="vertical"
          initialValues={{ dataEntrega: dayjs() }}
          onFinish={(values) =>
            entregaMutation.mutate({
              id: entregaModal!.obrigacaoId,
              data: {
                dataEntrega: values.dataEntrega.toISOString(),
                observacao: values.observacao,
              },
            })
          }
          style={{ marginTop: 8 }}
        >
          <Form.Item
            label="Data de Entrega"
            name="dataEntrega"
            rules={[{ required: true, message: 'Informe a data de entrega' }]}
          >
            <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
          </Form.Item>
          <Form.Item label="Observação" name="observacao">
            <Input.TextArea rows={3} placeholder="Opcional" />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}
