import {
  ApiOutlined,
  ApartmentOutlined,
  AppstoreOutlined,
  DatabaseOutlined,
  InboxOutlined,
  PlayCircleOutlined,
  ProfileOutlined,
  SafetyCertificateOutlined,
  ScheduleOutlined,
  ToolOutlined,
} from '@ant-design/icons'
import type { ComponentType } from 'react'

export type NavIcon = ComponentType<{ className?: string }>

export type NavItem = {
  label: string
  path?: string
  hint: string
  icon: NavIcon
  children?: NavItem[]
}

export type NavGroup = {
  title: string
  items: NavItem[]
}

export const homeNav = {
  label: '应用大厅',
  path: '/',
  hint: '从这里进入各个业务模块。',
  icon: AppstoreOutlined,
} satisfies NavItem

export const navGroups: NavGroup[] = [
  {
    title: '主数据',
    items: [
      {
        label: '基础数据',
        path: '/master-data',
        hint: '物料、单位、工厂日历等主数据以后放在这里。',
        icon: DatabaseOutlined,
      },
      {
        label: '业务模型',
        path: '/business-model',
        hint: '维护企业、基地、车间、工段、设备和工位。',
        icon: ApartmentOutlined,
      },
    ],
  },
  {
    title: '生产',
    items: [
      {
        label: '生产计划',
        path: '/production-plan',
        hint: '计划排程以后放在这里。',
        icon: ScheduleOutlined,
      },
      {
        label: '工单管理',
        path: '/work-orders',
        hint: '工单列表和详情以后放在这里。',
        icon: ProfileOutlined,
      },
      {
        label: '生产执行',
        path: '/execution',
        hint: '进出站和现场作业以后放在这里。',
        icon: PlayCircleOutlined,
      },
    ],
  },
  {
    title: '现场',
    items: [
      {
        label: '物料管理',
        path: '/material',
        hint: '线边库和领退料以后放在这里。',
        icon: InboxOutlined,
      },
      {
        label: '质量管理',
        path: '/quality',
        hint: '检验和异常以后放在这里。',
        icon: SafetyCertificateOutlined,
      },
      {
        label: '设备管理',
        path: '/equipment',
        hint: '设备台账和点检以后放在这里。',
        icon: ToolOutlined,
      },
      {
        label: '试验台',
        hint: '手动发送设备报文，看 MES 回包和库里的结果。',
        icon: ApiOutlined,
        children: [
          {
            label: '心跳试验台',
            path: '/bench/heartbeat',
            hint: '按四拍看设备心跳怎样领走一条控机。',
            icon: ApiOutlined,
          },
          {
            label: '清理心跳状态',
            path: '/bench/clear-heartbeat',
            hint: 'ClearEquipHeartBeatStatus',
            icon: ApiOutlined,
          },
          {
            label: '设备状态',
            path: '/bench/equip-status',
            hint: 'EquipStatus',
            icon: ApiOutlined,
          },
          {
            label: '设备报警',
            path: '/bench/equip-alert',
            hint: 'EquipAlert',
            icon: ApiOutlined,
          },
          {
            label: '运行请求',
            path: '/bench/equip-run',
            hint: 'EquipRunRequest',
            icon: ApiOutlined,
          },
          {
            label: '参数变更',
            path: '/bench/eqpt-para',
            hint: 'EqptParaCHNG',
            icon: ApiOutlined,
          },
          {
            label: '控机反馈',
            path: '/bench/eqpt-ctrl',
            hint: 'EqptCtrl',
            icon: ApiOutlined,
          },
          {
            label: '通道启停反馈',
            path: '/bench/channel-feedback',
            hint: 'ChannelCtrlFeedback',
            icon: ApiOutlined,
          },
          {
            label: '控机入队',
            path: '/bench/equip-control',
            hint: 'EquipControl',
            icon: ApiOutlined,
          },
        ],
      },
    ],
  },
]

export const allNavItems: NavItem[] = [
  homeNav,
  ...navGroups.flatMap((group) => group.items.flatMap((item) => [item, ...(item.children ?? [])])),
]

export function findNavItem(pathname: string): NavItem {
  if (pathname === '/') return homeNav
  return allNavItems.find((item) => item.path === pathname) ?? homeNav
}
