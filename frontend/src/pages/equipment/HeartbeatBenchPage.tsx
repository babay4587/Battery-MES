import { Button, Form, Input, message } from 'antd'
import { useState } from 'react'
import {
  enqueueControl,
  sendHeartBeat,
  type EnqueueControlResult,
  type EquipmentHeartbeat,
  type HeartBeatResponse,
} from './heartbeatBenchApi'

type ControlForm = {
  channelCode: string
  cmd: string
  reasonCode: string
  description: string
}

type Verdict = 'pass' | 'fail' | 'info'

type Beat = {
  id: number
  title: string
  verdict: Verdict
  verdictText: string
  body: string
}

const steps = [
  { n: 1, title: '发心跳', detail: '队列是空的，应提示暂无数据。' },
  { n: 2, title: '入一条控机', detail: 'MES 把控机放进队列，凭证回到空闲号。' },
  { n: 3, title: '再发心跳', detail: '请求不变，回包应带走刚才那条控机。' },
  { n: 4, title: '再发一拍', detail: '凭证没有还回去，应又是暂无数据。' },
]

function pretty(value: unknown) {
  return JSON.stringify(value, null, 2)
}

function firstEquipment(response: HeartBeatResponse): EquipmentHeartbeat | undefined {
  return response.EquipmentInfo?.[0]
}

function idleMessage(item: EquipmentHeartbeat | undefined) {
  return item?.ResultFlag === true && (item.Message ?? '').includes('暂无数据') && (item.EqptCtrlFlag ?? 0) === 0
}

export function HeartbeatBenchPage() {
  const [software, setSoftware] = useState('试验台上位机')
  const [equipmentCode, setEquipmentCode] = useState('TEST-HB-01')
  const [control, setControl] = useState<ControlForm>({
    channelCode: 'CH-01',
    cmd: 'STOP',
    reasonCode: 'TEST',
    description: '试验台下发停机',
  })
  const [step, setStep] = useState(0)
  const [busy, setBusy] = useState(false)
  const [beats, setBeats] = useState<Beat[]>([])

  function pushBeat(title: string, verdict: Verdict, verdictText: string, body: unknown) {
    setBeats((current) => [
      { id: Date.now(), title, verdict, verdictText, body: pretty(body) },
      ...current,
    ])
  }

  function readEquipment() {
    const code = equipmentCode.trim()
    if (!code) {
      message.warning('请先填写设备号')
      return null
    }
    return code
  }

  async function beat() {
    const code = readEquipment()
    if (!code) return null
    const response = await sendHeartBeat(software.trim(), code)
    return { response, item: firstEquipment(response) }
  }

  async function onFreeBeat() {
    setBusy(true)
    try {
      const result = await beat()
      if (!result) return
      const item = result.item
      pushBeat(
        '自由发心跳',
        'info',
        item?.Message || (item?.EqptCtrlFlag === 1 ? '这一拍带走了控机' : '已收到回包'),
        result.response,
      )
    } catch (error) {
      message.error(error instanceof Error ? error.message : '心跳失败')
    } finally {
      setBusy(false)
    }
  }

  async function onFreeEnqueue() {
    const code = readEquipment()
    if (!code) return
    setBusy(true)
    try {
      const result = await enqueueControl({ equipmentCode: code, ...control })
      pushBeat('自由入队', 'info', result.message, result)
    } catch (error) {
      message.error(error instanceof Error ? error.message : '入队失败')
    } finally {
      setBusy(false)
    }
  }

  async function runStep(n: number) {
    if (n !== step + 1) return
    const code = readEquipment()
    if (!code) return
    setBusy(true)
    try {
      if (n === 1 || n === 3 || n === 4) {
        const result = await beat()
        if (!result) return
        const item = result.item
        if (n === 1 || n === 4) {
          const pass = idleMessage(item)
          pushBeat(
            `第 ${n} 拍 · 发心跳`,
            pass ? 'pass' : 'fail',
            pass
              ? '请求里只有上位机名和设备号。没有可领的队列，所以成功但暂无数据。'
              : '这一拍不是「暂无数据」。队列里可能还有没领走的内容，回包如下。',
            result.response,
          )
        } else {
          const params = item?.EqptCtrlParam ?? []
          const matched = params.some((row) => row.Cmd === control.cmd.trim() && row.ChannelCode === control.channelCode.trim())
          const pass = item?.ResultFlag === true && item.EqptCtrlFlag === 1 && matched
          pushBeat(
            '第 3 拍 · 再发心跳',
            pass ? 'pass' : 'fail',
            pass
              ? `同一请求带走了控机 ${control.cmd.trim()}，回执 ${item?.ReceiptNumber ?? ''}。`
              : '这一拍没有带走刚才入队的控机。',
            result.response,
          )
        }
      } else {
        const result: EnqueueControlResult = await enqueueControl({
          equipmentCode: code,
          channelCode: control.channelCode,
          cmd: control.cmd,
          reasonCode: control.reasonCode,
          description: control.description,
        })
        pushBeat('第 2 拍 · 入一条控机', 'pass', result.message, result)
      }
      setStep(n)
    } catch (error) {
      message.error(error instanceof Error ? error.message : '这一步失败')
    } finally {
      setBusy(false)
    }
  }

  return (
    <section>
      <h1 className="page-title">心跳试验台</h1>
      <p className="page-hint">
        左边是设备，只报上位机名和设备号。右边是 MES，先把一条控机放进队列。中间按 1 到 4 点。三次心跳的请求一样，回包不一样。
      </p>
      <div className="bench-layout">
        <div className="panel">
          <div className="panel-toolbar">
            <strong>设备</strong>
          </div>
          <Form layout="vertical" size="small">
            <Form.Item label="上位机名">
              <Input value={software} onChange={(event) => setSoftware(event.target.value)} />
            </Form.Item>
            <Form.Item label="设备号">
              <Input value={equipmentCode} onChange={(event) => setEquipmentCode(event.target.value)} />
            </Form.Item>
            <Button type="primary" onClick={() => void onFreeBeat()} loading={busy}>
              发心跳
            </Button>
          </Form>
        </div>
        <div className="panel">
          <div className="panel-toolbar">
            <strong>四拍</strong>
            <Button
              size="small"
              disabled={step === 0 || busy}
              onClick={() => {
                setStep(0)
              }}
            >
              重新开始
            </Button>
          </div>
          <ol className="bench-steps">
            {steps.map((item) => {
              const ready = item.n === step + 1
              const done = item.n <= step
              return (
                <li key={item.n} className={done ? 'done' : ready ? 'ready' : ''}>
                  <button type="button" disabled={!ready || busy} onClick={() => void runStep(item.n)}>
                    <span>{item.n}</span>
                    <strong>{item.title}</strong>
                  </button>
                  <p>{item.detail}</p>
                </li>
              )
            })}
          </ol>
        </div>
        <div className="panel">
          <div className="panel-toolbar">
            <strong>MES</strong>
          </div>
          <p className="page-hint">给设备号 {equipmentCode.trim() || '（未填）'} 入一条控机。</p>
          <Form layout="vertical" size="small">
            <Form.Item label="通道">
              <Input
                value={control.channelCode}
                onChange={(event) => setControl({ ...control, channelCode: event.target.value })}
              />
            </Form.Item>
            <Form.Item label="命令">
              <Input value={control.cmd} onChange={(event) => setControl({ ...control, cmd: event.target.value })} />
            </Form.Item>
            <Form.Item label="原因">
              <Input
                value={control.reasonCode}
                onChange={(event) => setControl({ ...control, reasonCode: event.target.value })}
              />
            </Form.Item>
            <Form.Item label="说明">
              <Input
                value={control.description}
                onChange={(event) => setControl({ ...control, description: event.target.value })}
              />
            </Form.Item>
            <Button onClick={() => void onFreeEnqueue()} loading={busy}>
              入队
            </Button>
          </Form>
        </div>
      </div>
      <div className="panel bench-log">
        <div className="panel-toolbar">
          <strong>这一轮看到的报文</strong>
        </div>
        {beats.length === 0 ? (
          <p className="page-hint">还没有发送。从中间的第 1 拍开始。</p>
        ) : (
          beats.map((beatItem) => (
            <article key={beatItem.id} className={`bench-beat ${beatItem.verdict}`}>
              <header>
                <strong>{beatItem.title}</strong>
                <span>{beatItem.verdictText}</span>
              </header>
              <pre>{beatItem.body}</pre>
            </article>
          ))
        )}
      </div>
    </section>
  )
}
