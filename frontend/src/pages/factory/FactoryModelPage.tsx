import { Button, Drawer, Form, Input, Popconfirm, Space, Tree, message } from 'antd'
import { useEffect, useMemo, useState } from 'react'
import {
  createFactoryNode,
  deleteFactoryNode,
  findFactoryNode,
  queryFactoryTree,
  updateFactoryNode,
  type FactoryNode,
} from './factoryApi'

type EditorState = {
  mode: 'create' | 'edit'
  parent: FactoryNode | null
  current: FactoryNode | null
}

const nextLevelName: Record<string, string> = {
  ENTERPRISE: '基地',
  SITE: '车间',
  WORKSHOP: '工段',
  SECTION: '设备',
  EQUIPMENT: '工位',
}

function collectKeys(nodes: FactoryNode[]): string[] {
  return nodes.flatMap((node) => [node.nid, ...collectKeys(node.children)])
}

export function FactoryModelPage() {
  const [nodes, setNodes] = useState<FactoryNode[]>([])
  const [selectedNid, setSelectedNid] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [saving, setSaving] = useState(false)
  const [editor, setEditor] = useState<EditorState | null>(null)
  const [form] = Form.useForm<{ name: string; description?: string }>()

  async function reload() {
    setLoading(true)
    try {
      const result = await queryFactoryTree()
      setNodes(result.nodes)
    } catch (error) {
      message.error(error instanceof Error ? error.message : '工厂树加载失败')
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    void reload()
  }, [])

  useEffect(() => {
    if (!editor) return
    if (editor.mode === 'edit' && editor.current) {
      form.setFieldsValue({ name: editor.current.name, description: editor.current.description ?? '' })
      return
    }
    form.setFieldsValue({ name: '', description: '' })
  }, [editor, form])

  const selected = selectedNid ? findFactoryNode(nodes, selectedNid) : null
  const expandedKeys = useMemo(() => collectKeys(nodes), [nodes])
  const treeData = useMemo(() => toTree(nodes), [nodes])

  function openCreate(parent: FactoryNode | null) {
    form.setFieldsValue({ name: '', description: '' })
    setEditor({ mode: 'create', parent, current: null })
  }

  function openEdit(current: FactoryNode) {
    form.setFieldsValue({ name: current.name, description: current.description ?? '' })
    setEditor({ mode: 'edit', parent: null, current })
  }

  const createLevelName = editor?.mode === 'create' ? (editor.parent ? nextLevelName[editor.parent.levelNid] : '企业') : ''

  return (
    <section>
      <h1 className="page-title">业务模型</h1>
      <p className="page-hint">维护企业、基地、车间、工段、设备和工位。刷新后仍从数据库读取。</p>
      <div className="factory-layout">
        <div className="panel">
          <div className="panel-toolbar">
            <Button size="small" type="primary" onClick={() => openCreate(null)}>
              新增企业
            </Button>
            <Button size="small" onClick={() => void reload()} loading={loading}>
              刷新
            </Button>
          </div>
          {nodes.length === 0 ? (
            <p className="page-hint">还没有工厂节点。</p>
          ) : (
            <Tree
              blockNode
              treeData={treeData}
              expandedKeys={expandedKeys}
              selectedKeys={selected ? [selected.nid] : []}
              onSelect={(keys) => setSelectedNid(keys.length > 0 ? String(keys[0]) : null)}
            />
          )}
        </div>
        <div className="panel">
          {selected ? (
            <>
              <div className="panel-toolbar">
                <strong>{selected.name}</strong>
                <Space>
                  {selected.levelNid !== 'STATION' ? (
                    <Button size="small" type="primary" onClick={() => openCreate(selected)}>
                      新增{nextLevelName[selected.levelNid]}
                    </Button>
                  ) : null}
                  <Button size="small" onClick={() => openEdit(selected)}>
                    编辑
                  </Button>
                  <Popconfirm title="删除这个节点？" okText="删除" cancelText="取消" onConfirm={() => void remove(selected.nid)}>
                    <Button size="small" danger>
                      删除
                    </Button>
                  </Popconfirm>
                </Space>
              </div>
              <dl className="detail-list">
                <div>
                  <dt>层级</dt>
                  <dd>{selected.levelName}</dd>
                </div>
                <div>
                  <dt>编码</dt>
                  <dd>{selected.nid}</dd>
                </div>
                <div>
                  <dt>说明</dt>
                  <dd>{selected.description || '—'}</dd>
                </div>
              </dl>
            </>
          ) : (
            <p className="page-hint">选择左侧节点查看，或新增一个企业。</p>
          )}
        </div>
      </div>
      <Drawer
        title={editor?.mode === 'edit' ? '编辑节点' : `新增${createLevelName}`}
        placement="right"
        width={640}
        maskClosable={false}
        destroyOnHidden
        open={editor !== null}
        onClose={() => setEditor(null)}
        extra={
          <Space>
            <Button size="small" onClick={() => setEditor(null)}>
              取消
            </Button>
            <Button size="small" type="primary" loading={saving} onClick={() => form.submit()}>
              保存
            </Button>
          </Space>
        }
      >
        <Form
          form={form}
          layout="vertical"
          size="small"
          onFinish={async (values) => {
            if (!editor) return
            setSaving(true)
            try {
              if (editor.mode === 'create') {
                const created = await createFactoryNode({
                  name: values.name,
                  description: values.description,
                  pnid: editor.parent?.nid ?? null,
                })
                setSelectedNid(created.nid)
                message.success('已保存')
              } else if (editor.current) {
                await updateFactoryNode(editor.current.nid, values)
                message.success('已保存')
              }
              setEditor(null)
              await reload()
            } catch (error) {
              message.error(error instanceof Error ? error.message : '保存失败')
            } finally {
              setSaving(false)
            }
          }}
        >
          <Form.Item label="名称" name="name" rules={[{ required: true, message: '请输入名称' }]}>
            <Input maxLength={200} />
          </Form.Item>
          <Form.Item label="说明" name="description">
            <Input.TextArea maxLength={500} rows={4} />
          </Form.Item>
        </Form>
      </Drawer>
    </section>
  )

  async function remove(nid: string) {
    try {
      await deleteFactoryNode(nid)
      if (selectedNid === nid) setSelectedNid(null)
      message.success('已删除')
      await reload()
    } catch (error) {
      message.error(error instanceof Error ? error.message : '删除失败')
    }
  }
}

function toTree(nodes: FactoryNode[]): { key: string; title: string; children: ReturnType<typeof toTree> }[] {
  return nodes.map((node) => ({
    key: node.nid,
    title: `${node.name}  ${node.levelName}`,
    children: toTree(node.children),
  }))
}
