import { createContext, useContext, useState, useEffect, type ReactNode } from 'react'

export interface AppTheme {
  id: string
  label: string
  primaryColor: string
  sidebarBg: string
  headerBg: string
  description: string
}

export const PRESET_THEMES: AppTheme[] = [
  {
    id: 'eauditoria',
    label: 'e-Auditoria',
    primaryColor: '#1565C0',
    sidebarBg: '#0D1B2A',
    headerBg: '#ffffff',
    description: 'Tema padrão azul-marinho',
  },
  {
    id: 'ocean',
    label: 'Ocean',
    primaryColor: '#00ACC1',
    sidebarBg: '#012A36',
    headerBg: '#ffffff',
    description: 'Tons de ciano e azul profundo',
  },
  {
    id: 'forest',
    label: 'Forest',
    primaryColor: '#2E7D32',
    sidebarBg: '#1B2B1C',
    headerBg: '#ffffff',
    description: 'Verde corporativo',
  },
  {
    id: 'slate',
    label: 'Slate',
    primaryColor: '#455A64',
    sidebarBg: '#263238',
    headerBg: '#ffffff',
    description: 'Cinza grafite neutro',
  },
]

export interface ThemeState {
  activeThemeId: string
  primaryColor: string
  sidebarBg: string
  headerBg: string
  displayName: string
}

interface ThemeContextValue {
  theme: ThemeState
  setPreset: (themeId: string) => void
  setPrimaryColor: (color: string) => void
  setDisplayName: (name: string) => void
}

const STORAGE_KEY = 'eauditoria_theme'

const defaultTheme: ThemeState = {
  activeThemeId: 'eauditoria',
  primaryColor: PRESET_THEMES[0].primaryColor,
  sidebarBg: PRESET_THEMES[0].sidebarBg,
  headerBg: PRESET_THEMES[0].headerBg,
  displayName: '',
}

function load(): ThemeState {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    if (raw) return { ...defaultTheme, ...JSON.parse(raw) }
  } catch {}
  return defaultTheme
}

function save(state: ThemeState) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(state))
}

const ThemeContext = createContext<ThemeContextValue>({
  theme: defaultTheme,
  setPreset: () => {},
  setPrimaryColor: () => {},
  setDisplayName: () => {},
})

export function ThemeProvider({ children }: { children: ReactNode }) {
  const [theme, setTheme] = useState<ThemeState>(load)

  useEffect(() => {
    save(theme)
  }, [theme])

  const setPreset = (themeId: string) => {
    const preset = PRESET_THEMES.find((t) => t.id === themeId)
    if (!preset) return
    setTheme((prev) => ({
      ...prev,
      activeThemeId: themeId,
      primaryColor: preset.primaryColor,
      sidebarBg: preset.sidebarBg,
      headerBg: preset.headerBg,
    }))
  }

  const setPrimaryColor = (color: string) => {
    setTheme((prev) => ({ ...prev, primaryColor: color, activeThemeId: 'custom' }))
  }

  const setDisplayName = (name: string) => {
    setTheme((prev) => ({ ...prev, displayName: name }))
  }

  return (
    <ThemeContext.Provider value={{ theme, setPreset, setPrimaryColor, setDisplayName }}>
      {children}
    </ThemeContext.Provider>
  )
}

export function useTheme() {
  return useContext(ThemeContext)
}
