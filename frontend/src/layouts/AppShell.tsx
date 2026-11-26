import { AppstoreOutlined } from '@ant-design/icons'
import { Link, Outlet, useLocation } from 'react-router-dom'
import { findNavItem } from '../app/navigation'
import { useUiStore } from '../stores/uiStore'
import { GlobalHeader } from './GlobalHeader'
import { SideNavigation } from './SideNavigation'

export function AppShell() {
  const collapsed = useUiStore((state) => state.sidebarCollapsed)
  const { pathname } = useLocation()
  const current = findNavItem(pathname)

  return (
    <div className="shell">
      <GlobalHeader />
      <div className={collapsed ? 'workspace collapsed' : 'workspace'}>
        <SideNavigation />
        <div className="content">
          <nav className="crumbs" aria-label="当前位置">
            <Link className="crumb-link" to="/">
              <AppstoreOutlined />
              应用
            </Link>
            <span>/</span>
            <span>{current.label}</span>
          </nav>
          <main className="main">
            <Outlet />
          </main>
        </div>
      </div>
    </div>
  )
}
