import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Form, Input, Button, Card, Typography, Alert, Space } from 'antd'
import { LockOutlined, MailOutlined, SafetyCertificateOutlined } from '@ant-design/icons'
import { authService } from '../services/auth'

const { Title, Text } = Typography

interface LoginForm {
  email: string
  senha: string
}

export default function LoginPage() {
  const navigate = useNavigate()
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const onFinish = async (values: LoginForm) => {
    setLoading(true)
    setError(null)
    try {
      await authService.login({ email: values.email, senha: values.senha })
      navigate('/dashboard', { replace: true })
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha ao realizar login.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div
      style={{
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #0D1B2A 0%, #1565C0 100%)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        padding: 24,
      }}
    >
      <Card
        style={{
          width: '100%',
          maxWidth: 420,
          borderRadius: 12,
          boxShadow: '0 20px 60px rgba(0,0,0,0.4)',
          border: 'none',
        }}
        styles={{ body: { padding: '40px 40px 32px' } }}
      >
        <Space direction="vertical" align="center" style={{ width: '100%', marginBottom: 32 }}>
          <div
            style={{
              width: 56,
              height: 56,
              borderRadius: 14,
              background: 'linear-gradient(135deg, #1565C0, #00ACC1)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              marginBottom: 8,
            }}
          >
            <SafetyCertificateOutlined style={{ fontSize: 28, color: '#fff' }} />
          </div>
          <Title level={3} style={{ margin: 0, color: '#0D1B2A' }}>
            e-Auditoria
          </Title>
          <Text type="secondary" style={{ fontSize: 13 }}>
            Painel de Obrigações Acessórias
          </Text>
        </Space>

        {error && (
          <Alert
            message={error}
            type="error"
            showIcon
            style={{ marginBottom: 20, borderRadius: 8 }}
          />
        )}

        <Form<LoginForm>
          layout="vertical"
          onFinish={onFinish}
          requiredMark={false}
          initialValues={{ email: 'admin@eauditoria.com.br' }}
        >
          <Form.Item
            name="email"
            label="E-mail"
            rules={[
              { required: true, message: 'Informe o e-mail.' },
              { type: 'email', message: 'E-mail inválido.' },
            ]}
          >
            <Input
              prefix={<MailOutlined style={{ color: '#90a4ae' }} />}
              placeholder="seu@email.com"
              size="large"
              style={{ borderRadius: 8 }}
            />
          </Form.Item>

          <Form.Item
            name="senha"
            label="Senha"
            rules={[{ required: true, message: 'Informe a senha.' }]}
          >
            <Input.Password
              prefix={<LockOutlined style={{ color: '#90a4ae' }} />}
              placeholder="••••••••"
              size="large"
              style={{ borderRadius: 8 }}
            />
          </Form.Item>

          <Form.Item style={{ marginBottom: 0, marginTop: 8 }}>
            <Button
              type="primary"
              htmlType="submit"
              size="large"
              loading={loading}
              block
              style={{
                borderRadius: 8,
                height: 46,
                background: 'linear-gradient(135deg, #1565C0, #1E88E5)',
                border: 'none',
                fontWeight: 600,
                fontSize: 15,
              }}
            >
              Entrar
            </Button>
          </Form.Item>
        </Form>

        <div
          style={{
            marginTop: 24,
            padding: '12px 16px',
            background: '#f0f7ff',
            borderRadius: 8,
            border: '1px solid #bee3f8',
          }}
        >
          <Text style={{ fontSize: 12, color: '#1565C0', display: 'block', fontWeight: 600 }}>
            Credenciais de demonstração
          </Text>
          <Text style={{ fontSize: 12, color: '#546e7a' }}>
            admin@eauditoria.com.br / Admin@2025
          </Text>
        </div>
      </Card>
    </div>
  )
}
