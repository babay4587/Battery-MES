# Battery MES

锂电池 MES。本仓库放新系统：ASP.NET Core 单宿主、MySQL、Vite / React 19 前端。

## 这一版有什么

- 浏览器外壳：应用大厅和侧栏。业务模型页可以维护工厂树（企业、基地、车间、工段、设备、工位）。
- 登录使用自己的用户表和 JWT。初始角色 `MES_ADMIN`。还没有页面按钮级权限。
- 工厂数据在 `FM_LEVEL`、`FM_NODE`。视图 `V_FACTORY_MODEL` 给出原来业务会用到的 `NID`、`NAME`、`LEVEL_NID`、`PNID`。
- 设备心跳 `POST /api/CZ2Mes/HeartBeat` 已按滁州报文重写：设备只报上位机名和设备号，待下发内容由这一拍回包带走。领走后不把空闲号设回去。
- 现场菜单「试验台」在「设备管理」下面。里面有心跳试验台、设备状态报文，以及上下料与寿命。心跳试验台：左边扮设备发心跳，右边入一条控机，中间按 1 到 4 看同一请求怎样从「暂无数据」变成带走控机，再变回「暂无数据」。入队是 `POST /api/heartbeat-bench/controls`，要登录。
- 设备状态报文已重写：`ClearEquipHeartBeatStatus`、`EquipStatus`、`EquipAlert`、`EquipRunRequest`、`EqptParaCHNG`、`EqptCtrl`、`ChannelCtrlFeedback`、`EquipControl`。都在 `POST /api/CZ2Mes/{动作}`。试验台页面可以手动发送，并看回包和库里的结果。运行请求这一版只看已上报的设备状态，不做到齐套检查。通道启停反馈只回成功。
- 上下料与寿命已重写：`MaterialLoad`、`MaterialUnload`、`PartLoad`、`PartUnLoad`、`PartLife`、`MaterialCheck`。都在 `POST /api/CZ2Mes/{动作}`，字段名保持原来的大小写。试验台各有一页，路径是 `/bench/material-load`、`/bench/material-unload`、`/bench/part-load`、`/bench/part-unload`、`/bench/part-life`、`/bench/material-check`。前四个是设备回执：先放一条待反馈记录（队列状态 2、执行参数状态 1），再发送。上机和零部件上机在结果为 true 时把执行参数改成 3，结果为 false 时改成 4，队列都改成 3。下机不看成功失败，队列和执行参数都改成 3。零部件寿命把剩余寿命改成上报值，并记一条履历；设备不存在时返回「设备不存在」。物料校验按标签对库存和当前设备的工序 BOM，通过后写上料记录，库存上料数量记成剩余数量。在制上料、下料、零部件装拆这一版不调用。齐套、维修工单、停机按天拆分没有做。建表在 `database/004_material_handling.sql`，接口启动时执行。

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
