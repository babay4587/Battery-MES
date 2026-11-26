import { BellOutlined, SearchOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useAuthStore } from '../stores/authStore'
import { factories, useUiStore } from '../stores/uiStore'

export function GlobalHeader() {
  const navigate = useNavigate()
  const factoryName = useUiStore((state) => state.factoryName)
  const setFactoryName = useUiStore((state) => state.setFactoryName)
  const displayName = useAuthStore((state) => state.displayName)
  const clearSession = useAuthStore((state) => state.clearSession)
  const userLabel = displayName ?? '用户'

  return (
    <header className="header">
      <button type="button" className="brand" aria-label="返回应用大厅" onClick={() => navigate('/')}>
        <span className="brand-mark" aria-hidden="true">
          C
        </span>
        <span>
          CELL<span className="brand-light"> / MES</span>
        </span>
      </button>
      <span className="header-divider" />
      <label className="factory">
        <span className="side-label">当前工厂</span>
        <select
          aria-label="当前工厂"
          value={factoryName}
          onChange={(event) => {
            const next = factories.find((name) => name === event.target.value)
            if (next) setFactoryName(next)
          }}
        >
          {factories.map((name) => (
            <option key={name} value={name}>
              {name}
            </option>
          ))}
        </select>
      </label>
      <div className="header-actions">
        <button type="button" className="icon-button" aria-label="搜索">
          <SearchOutlined />
        </button>
        <button type="button" className="icon-button" aria-label="通知">
          <BellOutlined />
          <span className="bell-dot" />
        </button>
        <button
          type="button"
          className="user-chip"
          aria-label="退出登录"
          onClick={() => {
            clearSession()
            navigate('/login')
          }}
        >
          <span className="user-avatar">{userLabel.slice(0, 1)}</span>
          <span>{userLabel}</span>
          <span className="logout">退出</span>
        </button>
      </div>
    </header>
  )
}
