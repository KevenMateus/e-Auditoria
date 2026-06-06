import { Tag } from 'antd'
import type { StatusObrigacao } from '../types'

const config: Record<StatusObrigacao, { color: string; label: string }> = {
  Pendente:     { color: '#1565C0', label: 'Pendente' },
  Atrasada:     { color: '#C62828', label: 'Atrasada' },
  Entregue:     { color: '#2E7D32', label: 'Entregue' },
  NaoAplicavel: { color: '#9e9e9e', label: 'N/A' },
}

interface Props {
  status: StatusObrigacao
}

export default function StatusBadge({ status }: Props) {
  const { color, label } = config[status] ?? config.Pendente
  return (
    <Tag
      style={{
        background: color + '18',
        borderColor: color + '55',
        color,
        fontWeight: 600,
        fontSize: 12,
      }}
    >
      {label}
    </Tag>
  )
}
