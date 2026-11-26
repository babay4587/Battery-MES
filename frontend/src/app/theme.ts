import type { ThemeConfig } from 'antd'

export const theme: ThemeConfig = {
  token: {
    colorPrimary: '#347B75',
    colorInfo: '#347B75',
    colorText: '#2D3E38',
    colorTextSecondary: '#52645C',
    colorTextPlaceholder: '#63756B',
    colorBorder: '#DCE2E1',
    colorBgLayout: '#F1F3F3',
    colorBgContainer: '#FFFFFF',
    colorWarning: '#9A621F',
    colorError: '#AD4E4A',
    borderRadius: 4,
    fontFamily: '"Microsoft YaHei", "微软雅黑", sans-serif',
    fontSize: 14,
    controlHeight: 32,
  },
  components: {
    Button: {
      primaryShadow: 'none',
      defaultShadow: 'none',
      fontWeight: 400,
    },
  },
}
