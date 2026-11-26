import { api } from '../../api/http'

export type HeartBeatResponse = {
  ServerDateTime?: string
  EquipmentInfo?: EquipmentHeartbeat[]
}

export type EquipmentHeartbeat = {
  EquipmentCode?: string | null
  ResultFlag?: boolean
  Message?: string | null
  ReceiptNumber?: string | null
  EqptCtrlFlag?: number
  EqptCtrlParam?: EqptCtrlParam[] | null
}

export type EqptCtrlParam = {
  ChannelCode?: string | null
  Cmd?: string | null
  ReasonCode?: string | null
  Description?: string | null
}

export type EnqueueControlResult = {
  equipmentCode: string
  channelCode: string
  cmd: string
  reasonCode: string
  description: string
  message: string
}

export function sendHeartBeat(software: string, equipmentCode: string) {
  return api<HeartBeatResponse>('/api/CZ2Mes/HeartBeat', {
    method: 'POST',
    body: JSON.stringify({
      Software: software,
      EquipmentInfo: [{ EquipmentCode: equipmentCode }],
    }),
  })
}

export function enqueueControl(body: {
  equipmentCode: string
  channelCode: string
  cmd: string
  reasonCode: string
  description: string
}) {
  return api<EnqueueControlResult>('/api/heartbeat-bench/controls', {
    method: 'POST',
    body: JSON.stringify(body),
  })
}
