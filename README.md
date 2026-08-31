本项目是一个金蝶BOS（Kingdee BOS）平台的操作服务插件库，主要用于单据操作时的业务逻辑拦截、数据推送及单据编号自定义。项目采用C#编写，支持热更新。

### 核心模块与功能

1. **单据与基础资料推送 (`BillData.cs`, `BasicData.cs`)**
   - 继承自 `AbstractOperationServicePlugIn`，在单据或基础资料执行操作后，提取关键信息（如`fid`、`fbillno`等），调用 `MWUTILS.MWData` 公共方法将数据推送至外部系统（如WMS）。

2. **退料申请自动下推与提交 (`PushPurMrapp.cs`)**
   - 拦截退料申请单的操作，自动调用 `WebApiServiceCall.Push` 下推生成退料单（`PUR_MRB`），并自动执行提交操作。包含详细的日志记录（`Logger.cs`）与结果状态校验。

3. **采购申请期望日期同步 (`SavePurRequisition.cs`)**
   - 采购申请保存时，通过SQL同步更新下游采购订单的“新期望日期”，并触发采购订单的反写与WMS推送操作（`DoNothing` 触发 `PostWms`）。

4. **生产订单自定义编号 (`MoFbillno/MOFbillno.cs`)**
   - 在单据新增或修改时，根据规则（序列号-产品编码-层级-流水号）动态生成并重写生产订单的单据编号。

5. **物料定时任务 (`Runs.cs`)**
   - 实现 `IScheduleService`，定时扫描未同步的物料基础资料，调用 `MWUTILS.MWData` 批量推送到外部系统。

### 关键依赖与工具
- **金蝶BOS核心库**：`Kingdee.BOS.*`（提供插件基类、ORM、WebApi等）
- **Newtonsoft.Json**：用于API请求响应的JSON序列化与解析。
- **日志工具**：`Logger.cs`，将插件运行日志按月及日期写入本地磁盘（`C:\ERPPostLog\`）。