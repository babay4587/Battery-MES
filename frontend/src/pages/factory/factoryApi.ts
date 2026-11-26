import { api } from '../../api/http'

export type FactoryNode = {
  nid: string
  name: string
  description: string | null
  levelNid: string
  levelName: string
  level: number
  pnid: string | null
  children: FactoryNode[]
}

export function queryFactoryTree() {
  return api<{ nodes: FactoryNode[] }>('/api/factory-model/tree')
}

export function createFactoryNode(body: { name: string; description?: string; pnid?: string | null }) {
  return api<FactoryNode>('/api/factory-model/nodes', {
    method: 'POST',
    body: JSON.stringify(body),
  })
}

export function updateFactoryNode(nid: string, body: { name: string; description?: string }) {
  return api<FactoryNode>(`/api/factory-model/nodes/${nid}`, {
    method: 'PUT',
    body: JSON.stringify(body),
  })
}

export function deleteFactoryNode(nid: string) {
  return api<void>(`/api/factory-model/nodes/${nid}`, { method: 'DELETE' })
}

export function findFactoryNode(nodes: FactoryNode[], nid: string): FactoryNode | null {
  for (const node of nodes) {
    if (node.nid === nid) return node
    const child = findFactoryNode(node.children, nid)
    if (child) return child
  }
  return null
}
