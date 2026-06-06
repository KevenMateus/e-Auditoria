import React from 'react'
import ReactDOM from 'react-dom/client'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { ReactQueryDevtools } from '@tanstack/react-query-devtools'
import { ConfigProvider } from 'antd'
import ptBR from 'antd/locale/pt_BR'
import dayjs from 'dayjs'
import 'dayjs/locale/pt-br'
import App from './App'
import { ThemeProvider, useTheme } from './contexts/ThemeContext'
import './styles/global.css'

dayjs.locale('pt-br')

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: 1,
    },
  },
})

function ThemedApp() {
  const { theme } = useTheme()

  return (
    <ConfigProvider
      locale={ptBR}
      theme={{
        token: {
          colorPrimary: theme.primaryColor,
          colorLink: theme.primaryColor,
          colorLinkHover: theme.primaryColor + 'CC',
          colorSuccess: '#2E7D32',
          colorWarning: '#F57F17',
          colorError: '#C62828',
          colorInfo: '#00ACC1',
          borderRadius: 6,
          fontFamily: "'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
        },
      }}
    >
      <App />
    </ConfigProvider>
  )
}

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <ThemedApp />
      </ThemeProvider>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  </React.StrictMode>,
)
