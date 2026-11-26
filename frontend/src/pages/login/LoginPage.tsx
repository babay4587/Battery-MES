import { Button, Form, Input, message } from 'antd'
import { Navigate, useNavigate } from 'react-router-dom'
import { api } from '../../api/http'
import { useAuthStore } from '../../stores/authStore'

type LoginResult = {
  token: string
  displayName: string
  loginName: string
}

export function LoginPage() {
  const token = useAuthStore((state) => state.token)
  const setSession = useAuthStore((state) => state.setSession)
  const navigate = useNavigate()

  if (token) return <Navigate to="/" replace />

  return (
    <main className="login-page">
      <Form
        className="login-card"
        layout="vertical"
        size="small"
        requiredMark={false}
        onFinish={async (values: { loginName: string; password: string }) => {
          try {
            const result = await api<LoginResult>('/api/auth/login', {
              method: 'POST',
              body: JSON.stringify(values),
            })
            setSession(result)
            navigate('/', { replace: true })
          } catch (error) {
            message.error(error instanceof Error ? error.message : '登录失败')
          }
        }}
      >
        <div className="login-brand">
          <span className="brand-mark" aria-hidden="true">
            C
          </span>
          <div>
            <strong>CELL / MES</strong>
            <p>使用本系统账号登录</p>
          </div>
        </div>
        <Form.Item label="账号" name="loginName" rules={[{ required: true, message: '请输入账号' }]}>
          <Input autoFocus autoComplete="username" />
        </Form.Item>
        <Form.Item label="密码" name="password" rules={[{ required: true, message: '请输入密码' }]}>
          <Input.Password autoComplete="current-password" />
        </Form.Item>
        <Button type="primary" htmlType="submit" block>
          登录
        </Button>
      </Form>
    </main>
  )
}
