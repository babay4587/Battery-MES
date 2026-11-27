import { Link } from 'react-router-dom'
import { navGroups, type NavItem } from '../../app/navigation'

function cards(items: NavItem[]) {
  return items.flatMap((item) => (item.children ? item.children : [item]))
}

export function HomePage() {
  return (
    <section>
      <h1 className="page-title">应用大厅</h1>
      <p className="page-hint">业务模型可以维护工厂树。现场里的试验台可以手动发送心跳、设备状态、上下料和寿命报文。其余入口仍是占位。</p>
      <div className="app-grid">
        {navGroups.flatMap((group) =>
          cards(group.items).map((item) => {
            const Icon = item.icon
            return (
              <Link key={item.path} className="app-card" to={item.path ?? '/'}>
                <Icon />
                <strong>{item.label}</strong>
                <span>{item.hint}</span>
              </Link>
            )
          }),
        )}
      </div>
    </section>
  )
}
