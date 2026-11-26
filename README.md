# Battery MES

锂电池 MES。本仓库放新系统：ASP.NET Core 单宿主、MySQL、Vite / React 19 前端。

## 这一版有什么

- 浏览器外壳：应用大厅和侧栏。业务模型页可以维护工厂树（企业、基地、车间、工段、设备、工位）。
- 登录使用自己的用户表和 JWT。初始角色 `MES_ADMIN`。还没有页面按钮级权限。
- 工厂数据在 `FM_LEVEL`、`FM_NODE`。视图 `V_FACTORY_MODEL` 给出原来业务会用到的 `NID`、`NAME`、`LEVEL_NID`、`PNID`。
- 设备心跳 `POST /api/CZ2Mes/HeartBeat` 已按滁州报文重写：设备只报上位机名和设备号，待下发内容由这一拍回包带走。领走后不把空闲号设回去。

## 本机运行

1. 开发库在 `192.168.2.26` 的 MySQL 8.0.32，库名 `battery_mes`。连接串在 `backend/BatteryMes.Api/appsettings.Development.json`。程序启动时会补齐表，并在没有管理员时写入初始账号。
2. 启动接口：

```bash
dotnet run --project backend/BatteryMes.Api
```

接口地址是 `http://127.0.0.1:7800`。先在 `frontend` 执行 `npm run build`，再启动接口，浏览器打开 `http://127.0.0.1:7800/` 就是登录页。健康检查是 `/health`。

3. 启动页面：

```bash
cd frontend
npm install
npm run dev
```

打开 `http://127.0.0.1:5173`。开发环境把 `/api` 转到 7800。

初始账号 `admin`，密码 `Admin@123`。这只给本机第一次登录，上线前要改掉。

`E:\Workspace-锂电池MES` 里的设计参考、滁州代码和已有外壳保持原样，没有改。
