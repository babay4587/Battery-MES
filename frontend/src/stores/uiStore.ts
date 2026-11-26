import { create } from 'zustand'

export const factories = ['苏州 · 电芯一厂', '常州 · 电芯二厂'] as const

type UiState = {
  sidebarCollapsed: boolean
  factoryName: (typeof factories)[number]
  toggleSidebar: () => void
  setFactoryName: (name: (typeof factories)[number]) => void
}

export const useUiStore = create<UiState>((set) => ({
  sidebarCollapsed: false,
  factoryName: factories[0],
  toggleSidebar: () => set((state) => ({ sidebarCollapsed: !state.sidebarCollapsed })),
  setFactoryName: (factoryName) => set({ factoryName }),
}))
