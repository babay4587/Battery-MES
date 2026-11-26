import { Link } from 'react-router-dom'
import { navGroups } from '../../app/navigation'

export function HomePage() {
  return (
    <section>
      <h1 className="page-title">应用大厅</h1>
      <p className="page-hint">业务模型可以维护工厂树。心跳试验台可以按四拍看设备怎样领走控机。其余入口仍是占位。</p>
      <div className="app-grid">
        {navGroups.flatMap((group) =>
          group.items.map((item) => {
            const Icon = item.icon
            return (
              <Link key={item.path} className="app-card" to={item.path}>
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
