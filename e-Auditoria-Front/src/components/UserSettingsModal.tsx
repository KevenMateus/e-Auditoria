import { useState } from 'react'
import {
  Modal,
  Tabs,
  Form,
  Input,
  Button,
  Space,
  Typography,
  Card,
  Tooltip,
  ColorPicker,
  Divider,
  Avatar,
  message,
} from 'antd'
import type { Color } from 'antd/es/color-picker'
import {
  UserOutlined,
  BgColorsOutlined,
  CheckOutlined,
} from '@ant-design/icons'
import { authService } from '../services/auth'
import { useTheme, PRESET_THEMES } from '../contexts/ThemeContext'

const { Text, Title } = Typography

interface Props {
  open: boolean
  onClose: () => void
}

export default function UserSettingsModal({ open, onClose }: Props) {
  const user = authService.getUser()
  const { theme, setPreset, setPrimaryColor, setDisplayName } = useTheme()

  const [displayNameInput, setDisplayNameInput] = useState(
    theme.displayName || user?.nome || '',
  )

  const handleSaveName = () => {
    setDisplayName(displayNameInput.trim())
    message.success('Nome atualizado.')
  }

  const handleColorChange = (color: Color) => {
    setPrimaryColor(color.toHexString())
  }

  const initials = (displayNameInput || user?.nome || 'U')
    .split(' ')
    .slice(0, 2)
    .map((w) => w[0])
    .join('')
    .toUpperCase()

  return (
    <Modal
      open={open}
      onCancel={onClose}
      footer={null}
      width={480}
      title={null}
      styles={{ body: { padding: 0 } }}
      centered
    >
      <div
        style={{
          padding: '24px 24px 16px',
          borderBottom: '1px solid #f0f0f0',
          background: `linear-gradient(135deg, ${theme.sidebarBg} 0%, ${theme.primaryColor} 100%)`,
          borderRadius: '8px 8px 0 0',
        }}
      >
        <Space align="center">
          <Avatar
            size={48}
            style={{ background: theme.primaryColor, border: '2px solid rgba(255,255,255,0.3)', fontSize: 18, fontWeight: 700 }}
          >
            {initials}
          </Avatar>
          <div>
            <Title level={5} style={{ margin: 0, color: '#fff' }}>
              {theme.displayName || user?.nome || 'Usuário'}
            </Title>
            <Text style={{ color: 'rgba(255,255,255,0.7)', fontSize: 12 }}>
              {user?.email} · {user?.perfil}
            </Text>
          </div>
        </Space>
      </div>

      <Tabs
        defaultActiveKey="perfil"
        style={{ padding: '0 24px 24px' }}
        items={[
          {
            key: 'perfil',
            label: (
              <Space>
                <UserOutlined />
                Perfil
              </Space>
            ),
            children: (
              <div style={{ paddingTop: 16 }}>
                <Form layout="vertical">
                  <Form.Item
                    label="Nome de exibição"
                    extra="Sobrescreve o nome do perfil na sidebar."
                  >
                    <Space.Compact style={{ width: '100%' }}>
                      <Input
                        value={displayNameInput}
                        onChange={(e) => setDisplayNameInput(e.target.value)}
                        placeholder={user?.nome}
                        maxLength={60}
                        onPressEnter={handleSaveName}
                      />
                      <Button
                        type="primary"
                        icon={<CheckOutlined />}
                        onClick={handleSaveName}
                        style={{ background: theme.primaryColor, borderColor: theme.primaryColor }}
                      />
                    </Space.Compact>
                  </Form.Item>

                  <Divider style={{ margin: '8px 0 16px' }} />

                  <div style={{ background: '#f8fafc', borderRadius: 8, padding: '12px 16px' }}>
                    <Text style={{ fontSize: 12, color: '#64748b', display: 'block', marginBottom: 4 }}>
                      Conta
                    </Text>
                    <Text strong style={{ fontSize: 13 }}>{user?.email}</Text>
                    <br />
                    <Text style={{ fontSize: 12, color: '#94a3b8' }}>Perfil: {user?.perfil}</Text>
                  </div>
                </Form>
              </div>
            ),
          },
          {
            key: 'aparencia',
            label: (
              <Space>
                <BgColorsOutlined />
                Aparência
              </Space>
            ),
            children: (
              <div style={{ paddingTop: 16 }}>
                <Text type="secondary" style={{ fontSize: 12, display: 'block', marginBottom: 12 }}>
                  Temas prontos
                </Text>
                <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 10, marginBottom: 24 }}>
                  {PRESET_THEMES.map((t) => {
                    const isActive = theme.activeThemeId === t.id
                    return (
                      <Tooltip key={t.id} title={t.description}>
                        <Card
                          hoverable
                          onClick={() => setPreset(t.id)}
                          style={{
                            borderRadius: 10,
                            cursor: 'pointer',
                            border: isActive
                              ? `2px solid ${t.primaryColor}`
                              : '2px solid transparent',
                            boxShadow: isActive ? `0 0 0 3px ${t.primaryColor}22` : undefined,
                            padding: 0,
                          }}
                          styles={{ body: { padding: '10px 12px' } }}
                        >
                          <div style={{ display: 'flex', gap: 6, marginBottom: 8 }}>
                            <div
                              style={{
                                width: 28,
                                height: 44,
                                borderRadius: 4,
                                background: t.sidebarBg,
                                flexShrink: 0,
                              }}
                            />
                            <div style={{ flex: 1 }}>
                              <div
                                style={{
                                  height: 14,
                                  background: '#f1f5f9',
                                  borderRadius: 3,
                                  marginBottom: 4,
                                }}
                              />
                              <div
                                style={{
                                  height: 24,
                                  background: t.primaryColor,
                                  borderRadius: 4,
                                  opacity: 0.9,
                                }}
                              />
                            </div>
                          </div>
                          <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
                            <Text strong style={{ fontSize: 12 }}>{t.label}</Text>
                            {isActive && (
                              <CheckOutlined style={{ color: t.primaryColor, fontSize: 12 }} />
                            )}
                          </div>
                        </Card>
                      </Tooltip>
                    )
                  })}
                </div>

                <Divider style={{ margin: '0 0 16px' }} />

                <Text type="secondary" style={{ fontSize: 12, display: 'block', marginBottom: 12 }}>
                  Cor primária personalizada
                </Text>
                <Space align="center">
                  <ColorPicker
                    value={theme.primaryColor}
                    onChange={handleColorChange}
                    showText
                    format="hex"
                    presets={[
                      {
                        label: 'Recomendadas',
                        colors: [
                          '#1565C0', '#1E88E5', '#00ACC1', '#0D47A1',
                          '#2E7D32', '#F57F17', '#C62828', '#455A64',
                          '#6A1B9A', '#AD1457',
                        ],
                      },
                    ]}
                  />
                  <Text style={{ fontSize: 12, color: '#94a3b8' }}>
                    Sobrescreve a cor do tema ativo
                  </Text>
                </Space>
              </div>
            ),
          },
        ]}
      />
    </Modal>
  )
}
