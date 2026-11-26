import { useAuthStore } from '../stores/authStore'

export class ApiError extends Error {
  status: number

  constructor(status: number, message: string) {
    super(message)
    this.status = status
  }
}

export async function api<T>(path: string, options: RequestInit = {}): Promise<T> {
  const token = useAuthStore.getState().token
  const headers = new Headers(options.headers)
  if (options.body !== undefined && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }
  if (token) headers.set('Authorization', `Bearer ${token}`)

  const response = await fetch(path, { ...options, headers })
  if (response.status === 401) {
    useAuthStore.getState().clearSession()
  }
  if (!response.ok) {
    const body = (await response.json().catch(() => null)) as { message?: string } | null
    throw new ApiError(response.status, body?.message ?? '请求失败')
  }
  if (response.status === 204) return undefined as T
  return (await response.json()) as T
}
