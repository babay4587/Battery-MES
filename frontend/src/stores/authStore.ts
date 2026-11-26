import { create } from 'zustand'

type Session = {
  token: string
  displayName: string
  loginName: string
}

const storageKey = 'cell-mes.session'

function readSession(): Session | null {
  const raw = sessionStorage.getItem(storageKey)
  if (!raw) return null
  try {
    const parsed = JSON.parse(raw) as Partial<Session>
    if (!parsed.token || !parsed.displayName || !parsed.loginName) return null
    return { token: parsed.token, displayName: parsed.displayName, loginName: parsed.loginName }
  } catch {
    return null
  }
}

type AuthState = {
  token: string | null
  displayName: string | null
  loginName: string | null
  setSession: (session: Session) => void
  clearSession: () => void
}

const initial = readSession()

export const useAuthStore = create<AuthState>((set) => ({
  token: initial?.token ?? null,
  displayName: initial?.displayName ?? null,
  loginName: initial?.loginName ?? null,
  setSession: (session) => {
    sessionStorage.setItem(storageKey, JSON.stringify(session))
    set({ token: session.token, displayName: session.displayName, loginName: session.loginName })
  },
  clearSession: () => {
    sessionStorage.removeItem(storageKey)
    set({ token: null, displayName: null, loginName: null })
  },
}))
