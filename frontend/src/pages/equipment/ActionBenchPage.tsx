import { Button, Form, Input, Space, message } from 'antd'
import { useState } from 'react'
import { api } from '../../api/http'
import type { BenchConfig } from './actionBenches'

export function ActionBenchPage({ config }: { config: BenchConfig }) {
  const [values, setValues] = useState(config.defaults)
  const [busy, setBusy] = useState(false)
  const [responseText, setResponseText] = useState('')
  const [snapshotText, setSnapshotText] = useState('')

  async function loadSnapshot() {
    if (!config.snapshot) return
    const code = values.equipmentCode?.trim() ?? ''
    const snapshot = await api<unknown>(
      `/api/equipment-bench/snapshot?equipmentCode=${encodeURIComponent(code)}&topic=${config.snapshot}`,
    )
    setSnapshotText(JSON.stringify(snapshot, null, 2))
  }

  async function send(path: string, body: unknown, title: string) {
    if (!values.equipmentCode?.trim()) {
      message.warning('请先填写设备号')
      return
    }
    setBusy(true)
    try {
      const response = await api<unknown>(path, {
        method: 'POST',
        body: JSON.stringify(body),
      })
      setResponseText(JSON.stringify(response, null, 2))
      await loadSnapshot()
      message.success(title)
    } catch (error) {
      message.error(error instanceof Error ? error.message : '发送失败')
    } finally {
      setBusy(false)
    }
  }

  return (
    <section>
      <h1 className="page-title">{config.title}</h1>
      <p className="page-hint">{config.hint}</p>
      <div className="bench-layout bench-two">
        <div className="panel">
          <div className="panel-toolbar">
            <strong>发送</strong>
          </div>
          <Form layout="vertical" size="small">
            {config.fields.map((field) => (
              <Form.Item key={field.key} label={field.label}>
                <Input
                  value={values[field.key] ?? ''}
                  onChange={(event) => setValues({ ...values, [field.key]: event.target.value })}
                />
              </Form.Item>
            ))}
            <Space>
              {config.seed ? (
                <Button loading={busy} onClick={() => void send(config.seed!.path, config.seed!.build(values), '已放入')}>
                  {config.seed.label}
                </Button>
              ) : null}
              <Button type="primary" loading={busy} onClick={() => void send(config.endpoint, config.build(values), '已发送')}>
                发送
              </Button>
            </Space>
          </Form>
        </div>
        <div className="panel">
          <div className="panel-toolbar">
            <strong>回包和库里的结果</strong>
          </div>
          {responseText ? <pre className="bench-inline">{responseText}</pre> : <p className="page-hint">还没有发送。</p>}
          {config.snapshot ? (
            <>
              <p className="page-hint bench-gap">当前设备在库里的相关记录：</p>
              {snapshotText ? <pre className="bench-inline">{snapshotText}</pre> : <p className="page-hint">发送后显示。</p>}
            </>
          ) : null}
        </div>
      </div>
    </section>
  )
}
