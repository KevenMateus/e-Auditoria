import { useState } from 'react'
import { Outlet, useNavigate, useLocation } from 'react-router-dom'
import { Layout, Menu, Typography, Badge, Button, Space, Avatar, Tooltip } from 'antd'
import {
  DashboardOutlined,
  BankOutlined,
  CalendarOutlined,
  BellOutlined,
  LogoutOutlined,
  SettingOutlined,
} from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { dashboardService } from '../../services/dashboard'
import { authService } from '../../services/auth'
import { useTheme } from '../../contexts/ThemeContext'
import UserSettingsModal from '../UserSettingsModal'

const { Sider, Header, Content } = Layout
const { Text } = Typography

const menuItems = [
  { key: '/dashboard', icon: <DashboardOutlined />, label: 'Dashboard' },
  { key: '/empresas', icon: <BankOutlined />, label: 'Empresas' },
  { key: '/calendario', icon: <CalendarOutlined />, label: 'Calendario' },
  { key: '/alertas', icon: <BellOutlined />, label: 'Alertas' },
]

export default function MainLayout() {
  const navigate = useNavigate()
  const location = useLocation()
  const user = authService.getUser()
  const { theme } = useTheme()
  const [settingsOpen, setSettingsOpen] = useState(false)

  const { data: alertas } = useQuery({
    queryKey: ['alertas'],
    queryFn: dashboardService.obterAlertas,
    refetchInterval: 60_000,
  })

  const totalAlertas = alertas?.length ?? 0
  const atrasadas = alertas?.filter((a) => a.diasRestantes < 0).length ?? 0

  const handleLogout = () => {
    authService.logout()
    navigate('/login', { replace: true })
  }

  const displayName = theme.displayName || user?.nome || 'Usuario'
  const initials = displayName
    .split(' ')
    .slice(0, 2)
    .map((w: string) => w[0])
    .join('')
    .toUpperCase()

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider
        width={240}
        style={{
          background: theme.sidebarBg,
          position: 'fixed',
          height: '100vh',
          left: 0,
          top: 0,
          display: 'flex',
          flexDirection: 'column',
        }}
      >
        <div style={{ padding: '24px 20px 16px' }}>
          <Text strong style={{ color: '#fff', fontSize: 18, letterSpacing: '-0.3px' }}>
            e-Auditoria
          </Text>
          <br />
          <Text style={{ color: '#90a4ae', fontSize: 12 }}>Obrigacoes Acessorias</Text>
        </div>

        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[location.pathname]}
          style={{ background: 'transparent', borderRight: 'none', flex: 1 }}
          onClick={({ key }) => navigate(key)}
          items={menuItems.map((item) =>
            item.key === '/alertas'
              ? {
                  ...item,
                  label: (
                    <span>
                      {item.label}
                      {totalAlertas > 0 && (
                        <Badge
                          count={atrasadas > 0 ? atrasadas : totalAlertas}
                          color={atrasadas > 0 ? '#C62828' : '#F57F17'}
                          style={{ marginLeft: 8 }}
                          size="small"
                        />
                      )}
                    </span>
                  ),
                }
              : item,
          )}
        />

        <div
          onClick={() => setSettingsOpen(true)}
          style={{
            position: 'absolute',
            bottom: 0,
            left: 0,
            right: 0,
            padding: '14px 16px',
            borderTop: '1px solid rgba(255,255,255,0.08)',
            background: 'rgba(0,0,0,0.25)',
            cursor: 'pointer',
            transition: 'background 0.2s',
          }}
          onMouseEnter={(e) =>
            (e.currentTarget.style.background = 'rgba(255,255,255,0.07)')
          }
          onMouseLeave={(e) =>
            (e.currentTarget.style.background = 'rgba(0,0,0,0.25)')
          }
        >
          <Space style={{ width: '100%', justifyContent: 'space-between' }}>
            <Space size={10}>
              <Avatar
                size={34}
                style={{
                  background: theme.primaryColor,
                  flexShrink: 0,
                  fontSize: 13,
                  fontWeight: 700,
                  border: '1px solid rgba(255,255,255,0.2)',
                }}
              >
                {initials}
              </Avatar>
              <div style={{ lineHeight: 1.3 }}>
                <Text
                  style={{ color: '#fff', fontSize: 12, fontWeight: 600, display: 'block', maxWidth: 120 }}
                  ellipsis={{ tooltip: displayName }}
                >
                  {displayName}
                </Text>
                <Text style={{ color: '#90a4ae', fontSize: 11 }}>{user?.perfil ?? ''}</Text>
              </div>
            </Space>
            <Space size={4}>
              <Tooltip title="Configuracoes">
                <SettingOutlined style={{ color: '#90a4ae', fontSize: 14 }} />
              </Tooltip>
              <Tooltip title="Sair">
                <Button
                  type="text"
                  icon={<LogoutOutlined style={{ color: '#90a4ae', fontSize: 14 }} />}
                  onClick={(e) => {
                    e.stopPropagation()
                    handleLogout()
                  }}
                  style={{ padding: 2, height: 'auto' }}
                />
              </Tooltip>
            </Space>
          </Space>
        </div>
      </Sider>

      <Layout style={{ marginLeft: 240 }}>
        <Header
          style={{
            background: theme.headerBg,
            padding: '0 24px',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            borderBottom: '1px solid #e8ecf0',
            height: 56,
          }}
        >
          <Text style={{ color: '#64748b', fontSize: 13 }}>
            {new Date().toLocaleDateString('pt-BR', {
              weekday: 'long',
              year: 'numeric',
              month: 'long',
              day: 'numeric',
            })}
          </Text>
          <Text style={{ color: '#90a4ae', fontSize: 12 }}>{user?.email}</Text>
        </Header>

        <Content style={{ padding: 24, minHeight: 'calc(100vh - 56px)' }}>
          <Outlet />
        </Content>
      </Layout>

      <UserSettingsModal open={settingsOpen} onClose={() => setSettingsOpen(false)} />
    </Layout>
  )
}
