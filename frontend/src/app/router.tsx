import { createBrowserRouter, Navigate } from 'react-router-dom'
import { RequireAuth } from './RequireAuth'
import { AppShell } from '../layouts/AppShell'
import { FactoryModelPage } from '../pages/factory/FactoryModelPage'
import { HeartbeatBenchPage } from '../pages/equipment/HeartbeatBenchPage'
import { HomePage } from '../pages/home/HomePage'
import { LoginPage } from '../pages/login/LoginPage'
import { PlaceholderPage } from '../pages/PlaceholderPage'

function place(pathname: string) {
  return <PlaceholderPage pathname={pathname} />
}

export const router = createBrowserRouter([
  { path: '/login', element: <LoginPage /> },
  {
    path: '/',
    element: (
      <RequireAuth>
        <AppShell />
      </RequireAuth>
    ),
    children: [
      { index: true, element: <HomePage /> },
      { path: 'master-data', element: place('/master-data') },
      { path: 'business-model', element: <FactoryModelPage /> },
      { path: 'production-plan', element: place('/production-plan') },
      { path: 'work-orders', element: place('/work-orders') },
      { path: 'execution', element: place('/execution') },
      { path: 'material', element: place('/material') },
      { path: 'quality', element: place('/quality') },
      { path: 'equipment', element: place('/equipment') },
      { path: 'heartbeat-bench', element: <HeartbeatBenchPage /> },
      { path: '*', element: <Navigate to="/" replace /> },
    ],
  },
])
