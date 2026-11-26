import { MenuFoldOutlined, MenuUnfoldOutlined } from '@ant-design/icons'
import { NavLink } from 'react-router-dom'
import { homeNav, navGroups, type NavItem } from '../app/navigation'
import { useUiStore } from '../stores/uiStore'

function NavItemLink({ item, end = false }: { item: NavItem; end?: boolean }) {
  const Icon = item.icon
  return (
    <NavLink
      to={item.path}
      end={end}
      className={({ isActive }) => `side-item${isActive ? ' active' : ''}`}
      aria-label={item.label}
    >
      <Icon />
      <span className="side-label">{item.label}</span>
    </NavLink>
  )
}

export function SideNavigation() {
  const collapsed = useUiStore((state) => state.sidebarCollapsed)
  const toggleSidebar = useUiStore((state) => state.toggleSidebar)

  return (
    <aside className="sidebar" aria-label="业务导航">
      <div className="side-head">
        <span className="side-label">制造工作空间</span>
        <button
          type="button"
          className="icon-button"
          aria-label={collapsed ? '展开侧边导航' : '收起侧边导航'}
          onClick={toggleSidebar}
        >
          {collapsed ? <MenuUnfoldOutlined /> : <MenuFoldOutlined />}
        </button>
      </div>
      <div className="side-scroll">
        <NavItemLink item={homeNav} end />
        {navGroups.map((group) => (
          <div key={group.title}>
            <div className="side-group">{group.title}</div>
            {group.items.map((item) => (
              <NavItemLink key={item.path} item={item} />
            ))}
          </div>
        ))}
      </div>
      <div className="side-footer">
        <span>
          CELL / 制造运营平台
        </span>
      </div>
    </aside>
  )
}
