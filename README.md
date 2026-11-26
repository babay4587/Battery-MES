# Battery MES

锂电池 MES。本仓库放新系统：ASP.NET Core 单宿主、MySQL、Vite / React 19 前端。

## 这一版有什么

- 浏览器外壳：应用大厅和侧栏。业务模型页可以维护工厂树（企业、基地、车间、工段、设备、工位）。
- 登录使用自己的用户表和 JWT。初始角色 `MES_ADMIN`。还没有页面按钮级权限。
- 工厂数据在 `FM_LEVEL`、`FM_NODE`。视图 `V_FACTORY_MODEL` 给出原来业务会用到的 `NID`、`NAME`、`LEVEL_NID`、`PNID`。
- 设备心跳 `POST /api/CZ2Mes/HeartBeat` 这一步先不实现。

## 本机运行

1. 本机 MySQL 8。编辑 `backend/BatteryMes.Api/appsettings.Development.json`，把连接串里的 `Password=` 填成你的密码。库名用 `battery_mes`，程序启动时会建库、建表，并在没有管理员时写入初始账号。
2. 启动接口：

```bash
dotnet run --project backend/BatteryMes.Api
```

接口地址是 `http://127.0.0.1:5080`。健康检查是 `/health`。

3. 启动页面：

```bash
cd frontend
npm install
npm run dev
```

打开 `http://127.0.0.1:5173`。开发环境把 `/api` 转到 5080。

初始账号 `admin`，密码 `Admin@123`。这只给本机第一次登录，上线前要改掉。

`E:\Workspace-锂电池MES` 里的设计参考、滁州代码和已有外壳保持原样，没有改。
