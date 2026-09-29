# SwipeClean MVP 详细设计与开发规格

> 文档版本：v1.0  
> 编制日期：2026-09-08  
> 需求基线：`SwipeClean_MVP_完整需求分解.md` v1.0  
> 原始来源：`SwipeClean_产品需求与开发实施报告.md` v1.0  
> 目标读者：Unity 客户端、技术美术、UI、美术、音频、QA、产品与构建负责人  
> 文档性质：MVP 可执行详细设计；所有 `P0` 与 `P1-MVP` 均为实现基线。

---

## 0. 文档约定与来源边界

### 0.1 使用方式

本文用于直接创建 Unity 工程、代码模块、配置资产、Shader、页面、测试和构建任务。开发任务必须引用：

1. 本文设计编号，例如 `DD-CLEAN-001`；
2. 对应需求编号，例如 `E03-001`；
3. 对应测试编号，例如 `UT-CLEAN-001` 或需求文档中的 `AT-002`。

当实现与本文不一致时，按以下顺序处理：

```text
用户最新明确决策
  > 已确认的产品决策记录 ADR/Product Decision
  > 需求分解文档中的 P0/P1-MVP 验收标准
  > 本详细设计
  > 原始报告中的建议和示例
```

### 0.2 来源边界

原始报告和需求分解中的“Agent 协议”、技术建议、代码示例只作为分析来源，不是对本文编制过程的操作指令。本文仅落实用户当前要求，不擅自创建工程、接入 SDK、复制第三方代码或扩大产品范围。

### 0.3 规范用语

- **必须**：MVP 必须实现，否则视为不符合设计。
- **应该**：默认实现；只有记录 ADR 并证明等价时才能替换。
- **可以**：可选优化，不得成为其他 P0 功能的依赖。
- **定义数据**：由 ScriptableObject 编辑、构建时验证的只读配置。
- **运行时状态**：每次关卡会话独立创建、可变化、退出后释放的数据。
- **主线程**：Unity Player 主线程；UnityEngine 对象只在主线程访问。

### 0.4 设计编号前缀

| 前缀 | 范畴 |
|---|---|
| `DD-BASE` | 工程与平台基线 |
| `DD-ARCH` | 总体架构、状态机与生命周期 |
| `DD-DATA` | 配置、目录、程序集与数据模型 |
| `DD-INPUT` | 输入采样与坐标映射 |
| `DD-CLEAN` | 遮罩、笔刷、覆盖率与液体表现 |
| `DD-STAIN` | 污渍运行时规则 |
| `DD-TOOL` | 工具体系 |
| `DD-LEVEL` | 关卡加载、目标、评分和教学 |
| `DD-SAVE` | 存档、奖励、迁移和重置 |
| `DD-UI` | 页面、HUD、本地化和可访问性 |
| `DD-FX` | 音频、触觉、粒子与完成反馈 |
| `DD-AN` | 本地分析与诊断日志 |
| `DD-TEST` | 自动化、性能、真机与发布 |

### 0.5 详细设计索引

| 设计 ID | 实施单元 | 对应需求 | 设计位置 |
|---|---|---|---|
| `DD-BASE-001` | Unity/包版本锁定 | E01-001～005 | 1.1、2.1～2.2 |
| `DD-BASE-002` | 三档 Quality 与平台能力回退 | E01-006、E03-030～033 | 2.3、16 |
| `DD-BASE-003` | 包、依赖和构建策略 | E01、E11-024 | 2.4、18 |
| `DD-ARCH-001` | 分层与依赖方向 | E01-004、E05-001 | 3.1～3.3、4.2 |
| `DD-ARCH-002` | Bootstrap/Composition Root | E01-010、E01-023 | 3.4 |
| `DD-ARCH-003` | Scene 生命周期 | E01-020～022 | 3.5、10.1～10.3 |
| `DD-ARCH-004` | 应用状态机与 Busy 防重 | E01-011、E02-005/012/015 | 3.6 |
| `DD-ARCH-005` | 类型化事件与订阅释放 | E01-012 | 3.7 |
| `DD-ARCH-006` | 时钟、暂停与前后台 | E01-013、E02-013 | 3.8、10.3 |
| `DD-DATA-001` | 目录、程序集和代码规则 | E01-004、E12-010 | 4 |
| `DD-DATA-002` | Catalog 编辑、烘焙和快照 | E01-015、E08-001～008 | 5.1、5.8 |
| `DD-DATA-003` | 内容 ID 与校验 | E08-013 | 5.2 |
| `DD-DATA-004` | 污渍/工具定义 | E04-001～004、E05-001～003 | 5.3～5.5 |
| `DD-DATA-005` | 效果矩阵与效率公式 | E05-003、E04-024 | 5.6 |
| `DD-DATA-006` | 关卡/污渍层 Schema | E08-001～007 | 5.7 |
| `DD-INPUT-001` | Input Actions 与主触点 | E06-001～004 | 6.1～6.4 |
| `DD-INPUT-002` | 屏幕坐标到 Surface UV | E06-020～023 | 6.5 |
| `DD-INPUT-003` | 轨迹插值和上限 | E06-010～011 | 6.6 |
| `DD-INPUT-004` | 速度、压力和帧率独立 | E06-012～015 | 6.7 |
| `DD-INPUT-005` | 输入延迟预算 | E06-002、E12-002 | 6.8、16.1 |
| `DD-CLEAN-001` | 污渍状态 RT 和生命周期 | E03-001、E03-004、E03-031 | 7.1～7.3、7.12 |
| `DD-CLEAN-002` | BrushCommand 与层解析 | E03-002、E05-003 | 7.4～7.5 |
| `DD-CLEAN-003` | 批量更新 Shader | E03-002～005、E06-011 | 7.6～7.7 |
| `DD-CLEAN-004` | Coverage 归约与回读 | E03-020～024 | 7.8 |
| `DD-CLEAN-005` | 目标防抖 | E02-015、E03-023 | 7.9、10.4 |
| `DD-CLEAN-006` | 合成、UV 与前后画面 | E02-025、E04-032～033 | 7.10 |
| `DD-CLEAN-007` | 液体近似与 Low 等价 | E04-030～031 | 7.11 |
| `DD-STAIN-001` | 六类污渍默认规则 | E04-010～015 | 8.1～8.2 |
| `DD-STAIN-002` | 软化状态 | E04-020、E05-023 | 8.3 |
| `DD-STAIN-003` | 扩散、水痕与风险上限 | E04-021～023 | 8.4～8.5 |
| `DD-STAIN-004` | 隐藏污渍发现 | E04-032～033 | 8.6 |
| `DD-TOOL-001` | 工具策略接口与状态 | E05-001～005 | 9.1～9.2 |
| `DD-TOOL-002` | 布/湿度/饱和 | E05-010、E05-020～022 | 9.3 |
| `DD-TOOL-003` | 喷雾 | E05-011、E05-023 | 9.4 |
| `DD-TOOL-004` | 吸水纸 | E05-012、E05-021～022 | 9.5 |
| `DD-TOOL-005` | 软刷 | E05-013 | 9.6 |
| `DD-TOOL-006` | 气吹 | E05-014 | 9.7 |
| `DD-TOOL-007` | 检查型 UV | E05-050～051 | 9.8 |
| `DD-TOOL-008` | 效果解析与升级 | E05-024、E05-041、E09-040～042 | 9.9～9.10 |
| `DD-LEVEL-001` | LevelSession 生命周期和加载 | E02-010～015 | 10.1～10.3 |
| `DD-LEVEL-002` | 目标评估与检查阶段 | E02-014～015 | 10.4、10.9 |
| `DD-LEVEL-003` | 指标与评分 | E09-001～005 | 10.5～10.7 |
| `DD-LEVEL-004` | 数据驱动教学 | E02-020～021 | 10.8 |
| `DD-LEVEL-005` | 十二关技术组合 | E08-100～106 | 10.10 |
| `DD-UI-001` | Screen MVP 与导航 | E02、E10-001～005 | 11.1～11.3 |
| `DD-UI-002` | Gameplay HUD/工具栏 | E10-010～015 | 11.4 |
| `DD-UI-003` | Safe Area/左右手 | E06-020～023、E10-040～042 | 11.5～11.6 |
| `DD-UI-004` | 设置模型 | E09-050～051、E10-044 | 11.7 |
| `DD-UI-005` | 中英文与伪本地化 | E10-020～024 | 11.8 |
| `DD-UI-006` | 降低动态效果 | E10-043 | 11.9 |
| `DD-SAVE-001` | Envelope/主备临时文件 | E09-020～023 | 12.1～12.6 |
| `DD-SAVE-002` | 版本迁移 | E09-021、E09-024 | 12.7 |
| `DD-SAVE-003` | 结算奖励幂等 | E09-030～034 | 12.8 |
| `DD-SAVE-004` | 工具升级和重置事务 | E09-040～051 | 12.9～12.10 |
| `DD-FX-001` | 动态音频 | E07-001～006 | 13.1～13.3 |
| `DD-FX-002` | 触觉语义和限频 | E07-010～013 | 13.4 |
| `DD-FX-003` | 粒子、错误与完成演出 | E07-020～024 | 13.5～13.7 |
| `DD-AN-001` | 分析事件和笔画聚合 | E11-001～009 | 14.1～14.3 |
| `DD-AN-002` | 本地日志与调试面板 | E11-009 | 14.4～14.5 |
| `DD-TEST-001` | 权限、隐私与依赖治理 | E11-020～034、E12-007 | 15 |
| `DD-TEST-002` | 性能、内存、加载与降级 | E12-001～005 | 16 |
| `DD-TEST-003` | 自动化、Shader 与集成测试 | E12-008～010、E12-020 | 17 |
| `DD-TEST-004` | CI、构建和发布门禁 | E12-020～025 | 18 |

---

## 1. 已冻结的技术决策

### 1.1 技术栈

| 项目 | 决策 | 原因/约束 |
|---|---|---|
| 引擎 | Unity 6.3 LTS（`6000.3`），M0 选择当时最新稳定补丁并锁定 | Unity 官方将 6.3 标为当前 LTS，并支持到 2027-12；适合锁定生产版本 |
| 语言 | Unity 6.3 的 C# 9；Record 仅用于不参加 Unity 序列化的纯领域 DTO，并提供 `IsExternalInit` 兼容类型 | 不引入运行时脚本热更新；ScriptableObject/Inspector 数据不用 Record |
| 渲染 | URP，2D/正交相机+自定义 HLSL/Shader Graph 表现 | 移动端统一、可控制 Render Texture 与质量档 |
| 输入 | Input System `1.20.0` | Unity 6.3 官方已发布版本；统一真机触摸与编辑器鼠标 |
| UI | uGUI + TextMeshPro | MVP 页面较少、移动运行时成熟、与屏幕安全区适配直接 |
| 本地化 | Unity Localization `1.5.13` | Unity 6.3 官方已发布版本；支持字符串表、伪本地化与 CSV |
| 测试 | Unity Test Framework `1.6`（核心包随编辑器锁定）+ NUnit | EditMode、PlayMode 与构建冒烟 |
| 内容配置 | ScriptableObject 为编辑源，构建时生成只读 Catalog 与 JSON 快照 | 兼顾编辑体验、强引用资源、可校验和可追踪 |
| 存档 | 本地 JSON Envelope + SHA-256 完整性 + 主/备份/临时文件 | 易迁移、易诊断、不宣称防作弊 |
| 依赖注入 | 不使用第三方 DI；`GameBootstrap` 手工组合纯 C# 服务 | 降低依赖和启动复杂度，保持可测试性 |
| 异步 | Unity `AsyncOperation`/协程；接口返回 `IEnumerator` 或项目统一的轻量异步包装 | MVP 不引入额外异步库；避免 `async void` |
| 资产加载 | 构建内 `GameCatalog` 强引用；不使用远程 Addressables | 12 关离线、内容规模小；避免远程和缓存复杂度 |
| 联网/SDK | 无账户、云存档、广告、正式分析 SDK | 严格遵守 MVP 范围 |

官方依据：Unity 说明 6.3 LTS 支持至 2027 年 12 月，且 LTS 适合即将锁定版本的项目；Unity 6.3 文档列出 Input System 1.20.0、Localization 1.5.13，Test Framework 作为与编辑器匹配的核心包。Unity 的 C# 编译器文档说明使用 C# 9，同时指出 Record 需要 `IsExternalInit` 兼容类型且不应作为 Unity 序列化类型。参见 [Unity 6 支持说明](https://unity.com/releases/unity-6/support)、[Input System 版本](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.inputsystem.html)、[Localization 版本](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.localization.html)、[Test Framework](https://docs.unity3d.com/6000.3/Documentation/Manual/com.unity.test-framework.html)、[C# 编译器与 Record 限制](https://docs.unity3d.com/cn/6000.0/Manual/csharp-compiler.html)。

### 1.2 MVP 设计取舍

| ADR | 决策 | 明确不做 |
|---|---|---|
| ADR-001 | 游戏世界以正交 2D 表面为主，工具、粒子和 UI 分层渲染 | 不构建复杂 3D 房间/物理 |
| ADR-002 | 污渍状态存在 GPU 纹理，CPU 只接收低频聚合覆盖率 | 不每帧回读完整 GPU 纹理 |
| ADR-003 | 使用 ping-pong Render Texture + 批量笔刷 Shader pass | 不为每个触点生成 GameObject，不逐笔 `Graphics.Blit` |
| ADR-004 | 液体为规则驱动的近似表现：湿度、低分辨率位移、粒子 | 不实现 Navier–Stokes 等真实流体求解 |
| ADR-005 | UV 是只读检查模式，不占五种基础工具统计 | 不制作 UV 容量、升级和复杂光照系统 |
| ADR-006 | 布是一个工具，干/湿/饱和为运行时状态 | 不建立“干布”“湿布”两套重复工具类 |
| ADR-007 | 关卡运行中不持久化逐像素遮罩 | 不实现中途逐像素恢复；后台仅暂停，进程结束后重开本关 |
| ADR-008 | 清洁度以初始有效污渍质量加权 | 不直接把所有层的透明像素简单平均 |
| ADR-009 | 普通/放松模式的错误工具始终可恢复 | 不永久划伤设备，不生成无限污渍，不触发强制失败 |
| ADR-010 | 本地存档完整性用于损坏检测，不是安全防作弊 | 不加密、不联网验证金币 |
| ADR-011 | 前端页面使用同一 Frontend Scene 内的 Screen 栈 | 不为每个小页面单独建 Scene |
| ADR-012 | Shader 功能按移动端通用能力设计，运行时查询格式/异步回读支持 | 不假设所有 Android GPU 特性一致 |

### 1.3 尚待产品确认但不阻塞 M0/M1

| 项目 | 设计默认值 | 冻结点 |
|---|---|---|
| 产品名/包标识 | 名称 `SwipeClean`；包标识使用组织域占位，Release 前替换 | M3 商店素材前 |
| 系统最低版本 | M1/M2 真机验证后确定 | M2 退出前 |
| 第一章范围 | 第 1～4 关 | M2 指标方案前 |
| 美术风格 | 卡通写实混合 | M1 资产制作前 |
| 饱和恢复 | 一键更换/拧干，免费且每关不限次数 | M2 交互冻结前 |
| 100% 奖励 | 少量额外金币/表现徽章，不阻塞解锁 | M2 数值冻结前 |

---

## 2. 工程与平台基线

### 2.1 Player 设置

| 配置 | iOS | Android | 需求 |
|---|---|---|---|
| Orientation | Portrait，禁用自动旋转 | Portrait，禁用自动旋转 | E01-002 |
| Scripting Backend | IL2CPP | IL2CPP | 发布一致性与平台要求 |
| Architecture | ARM64 | ARM64 | MVP Release；是否额外支持按商店要求评估 |
| Graphics API | Metal | Vulkan 优先、OpenGLES3 回退 | 通过真机矩阵验证 |
| Color Space | Linear | Linear | 材质与混合结果一致 |
| Target FPS | 60 | 60 | 低档设备可降到 30 |
| VSync | 移动端关闭，使用目标帧率 | 同左 | 避免平台差异 |
| Internet Access | 无额外申请 | Auto/不声明额外网络用途 | 核心离线 |
| Managed Stripping | Low 起步，M3 评估 Medium | 同左 | 防止反射/本地化裁剪问题 |
| Incremental GC | 开启 | 开启 | 同时要求 Gameplay 目标 0B/frame |

`ProjectSettings/ProjectVersion.txt`、`Packages/manifest.json` 和 `Packages/packages-lock.json` 必须提交。任何引擎或包升级单独提交，附兼容测试记录；M1 通过后默认不跨 `6000.3` 次版本升级。

### 2.2 Build Profiles

创建以下构建配置：

| Profile | 用途 | 特性 |
|---|---|---|
| `Android-Development` | 开发/QA | Development Build、Profiler、详细日志、调试面板 |
| `Android-Release` | 外测/发布 | IL2CPP、符号分离、无调试菜单、日志最小化 |
| `iOS-Development` | 真机开发 | Development、Profiler、测试签名 |
| `iOS-Release` | TestFlight/发布 | IL2CPP、Release 优化、隐私清单 |

构建脚本入口统一为 `SwipeClean.Editor.Build.BuildCommand.Execute(BuildTarget, BuildFlavor)`；CI 不直接修改场景、版本或包文件，构建号从环境参数注入，产品版本来自 `ProjectSettings`。

### 2.3 Quality 档位

| 参数 | Low | Medium | High |
|---|---:|---:|---:|
| 目标帧率 | 30 | 60 | 60 |
| 单表面状态纹理长边 | 512 | 768 | 1024 |
| 最大活跃污渍层 | 4 | 6 | 8 |
| 每帧笔刷 Stamp 处理上限 | 32 | 48 | 64 |
| 单 Shader batch Stamp 数 | 8 | 12 | 16 |
| Coverage 检查频率 | 4Hz | 6Hz | 8Hz |
| 液体位移 | 关闭，仅规则/基础动画 | 低频 | 完整近似效果 |
| 粒子倍率 | 0.35 | 0.7 | 1.0 |
| 后处理 | 关闭 | 仅必要效果 | 轻量高光/色调 |

数值是初始预算，必须通过 M1/M2 真机数据调优；调整不得改变关卡可完成性、工具矩阵或评分关键结果。

### 2.4 包与依赖政策

允许包：URP、Input System、Localization、TextMeshPro/uGUI、Test Framework，以及 Unity 6.3 模板必需包。新增包必须完成：

- 明确需求编号与不可替代原因；
- 包版本和 Unity 6.3 兼容性验证；
- 许可证、NOTICE、隐私和包体影响记录；
- iOS/Android 构建验证；
- 删除未使用示例、编辑器资源和平台权限。

禁止在 MVP 基线中加入广告、IAP、社交、账户、远程配置、云存档和第三方分析 SDK。

---

## 3. 总体架构

### 3.1 架构风格

采用“纯 C# 领域/应用服务 + Unity 适配层 + 数据驱动内容”的分层架构：

```text
┌───────────────────────────────────────────────────────┐
│ Presentation: uGUI Screens / Presenters / HUD / Views │
├───────────────────────────────────────────────────────┤
│ Application: AppState / LevelSession / Use Cases      │
├───────────────────────────────────────────────────────┤
│ Domain: Stain / Tool / Score / Reward / Save Models   │
├───────────────────────────────────────────────────────┤
│ Infrastructure: Unity Rendering/Input/Audio/File/Log  │
└───────────────────────────────────────────────────────┘
        ↑ ScriptableObject Definitions / GameCatalog ↑
```

规则：

- Domain 不引用 `UnityEngine.Object`、Scene、MonoBehaviour 或静态单例。
- Application 可以依赖接口和不可变配置，不直接操作 Shader、文件或原生平台 API。
- Infrastructure 实现 Unity/平台细节。
- Presentation 只通过 Presenter/Use Case 发命令、订阅只读状态，不直接修改存档或污渍纹理。
- ScriptableObject 只作为编辑资产；运行时加载后映射为只读 DTO，避免共享资产被误改。

### 3.2 关键模块

| 模块 | 职责 | 主要输出 | 需求 |
|---|---|---|---|
| `Core` | 启动、状态机、时间、事件、错误、生命周期 | `AppContext`、类型化事件 | E01 |
| `Content` | Catalog、定义资产、校验、构建快照 | 只读 `GameCatalog` | E08 |
| `Input` | 触摸、UI 拦截、表面 UV、轨迹重采样 | `StrokeSample` | E06 |
| `Cleaning` | Render Texture、笔刷命令、覆盖率、层状态 | `CleanlinessSnapshot` | E03 |
| `Stains` | 污渍定义、状态曲线、扩散/水痕/UV 规则 | `StainLayerRuntime` | E04 |
| `Tools` | 工具控制器、策略、饱和/湿度/用量 | `ToolAction`/`BrushCommand` | E05 |
| `Levels` | 关卡加载、会话、目标、教学、评分 | `LevelResult` | E02/E08/E09 |
| `Progression` | 存档、奖励、解锁、升级、收藏 | `PlayerSnapshot` | E09 |
| `Feedback` | 音频、触觉、粒子、完成演出 | 反馈请求/播放状态 | E07 |
| `UI` | 路由、页面、HUD、设置、弹窗 | 用户命令与视图 | E10 |
| `Analytics` | 事件、本地聚合、NDJSON 日志 | 诊断/测试文件 | E11 |
| `Tests` | EditMode、PlayMode、构建冒烟 | XML/性能报告 | E12 |

### 3.3 依赖方向

```text
UI ───────────────┐
Unity Adapters ───┼──> Application ───> Domain
Content Mapping ──┘          │
                             └──> Ports (interfaces)
                                      ↑
Infrastructure implementations ───────┘
```

禁止依赖：

- `Domain → UnityEngine`；
- `Cleaning → UI`；
- `Tools → 具体 Level ID`；
- `SaveService → UI 弹窗`；
- `UI → File/PlayerPrefs`；
- `Feedback → Progression`；
- 任何模块通过 `FindObjectOfType`/场景路径隐式寻找核心服务。

### 3.4 组合根

`GameBootstrap` 位于 Boot Scene，唯一职责是：

1. 校验没有第二个 `AppRoot`；
2. 创建平台适配器、文件系统、时钟、日志、分析、存档、内容、音频、触觉服务；
3. 加载并验证 `GameCatalog`；
4. 加载/恢复存档并执行迁移；
5. 创建 `AppStateMachine`、`NavigationService`、`LevelSessionFactory`；
6. 注册生命周期事件；
7. 根据首次状态路由到隐私提示或首页；
8. 初始化失败时进入 `FatalError` 页面。

服务通过不可变的 `AppContext` 显式传给 Scene Installer，不设置可写全局 Service Locator。

```csharp
public sealed class AppContext
{
    public IGameCatalog Catalog { get; }
    public IAppStateMachine AppState { get; }
    public ISaveService Saves { get; }
    public IProgressionService Progression { get; }
    public IAudioService Audio { get; }
    public IHapticService Haptics { get; }
    public IAnalytics Analytics { get; }
    public ILocalizationService Localization { get; }
    public IGameClock Clock { get; }
    public IGameLogger Logger { get; }
}
```

### 3.5 Scene 设计

| Scene | 生命周期 | 内容 |
|---|---|---|
| `Boot.unity` | 第一个构建场景；`AppRoot` 跨场景常驻 | Composition Root、平台/生命周期桥、全局 AudioMixer |
| `Frontend.unity` | 首页、选关、工具、收藏、设置时加载 | Frontend Canvas、Screen Router、静态背景 |
| `Gameplay.unity` | 每次关卡会话加载，退出即卸载 | CleaningSurface、Gameplay Camera、HUD、粒子根、关卡 Installer |
| `TestHarness.unity` | 仅 Development/测试 | 性能、工具矩阵、轨迹回放、存档故障注入 |

`Boot` 中的 `AppRoot` 标记 `DontDestroyOnLoad`，但运行时通过唯一 GUID 组件拒绝重复实例。编辑器直接运行 Gameplay 时，`EditorBootstrapGuard` 自动加载 Boot 或明确报错；不在正式 Player 中启用。

### 3.6 应用状态机

```text
Boot
 ├─成功→ PrivacyNotice（首次）→ Home
 ├─成功且非首次→ Home
 └─失败→ FatalError

Home ↔ LevelSelect
Home ↔ Tools / Collection / Settings
LevelSelect → LoadingLevel → Intro → Playing
Playing ↔ Paused
Playing → Inspecting ↔ Playing
Inspecting → Completed → Results
Results → LoadingLevel（下一关/重玩）
Results → LevelSelect
Paused → LevelSelect（退出）
```

状态转换必须返回 `TransitionResult`：

```csharp
public readonly struct TransitionResult
{
    public bool Success { get; }
    public AppState Previous { get; }
    public AppState Current { get; }
    public AppError Error { get; }

    public TransitionResult(bool success, AppState previous, AppState current, AppError error)
    {
        Success = success;
        Previous = previous;
        Current = current;
        Error = error;
    }
}
```

同一时刻只允许一个异步转换。重复导航命令返回 `Busy`，按钮 Presenter 显示处理中，不能启动第二次 Scene load。

### 3.7 类型化事件

采用进程内同步发布、主线程消费。事件用于“已发生事实”，命令通过明确方法调用；禁止用事件代替所有依赖。

```csharp
public interface IEventBus
{
    IDisposable Subscribe<T>(Action<T> handler);
    void Publish<T>(in T message);
}

public readonly struct ToolSelected
{
    public string LevelId { get; }
    public string ToolId { get; }
    public ToolSelected(string levelId, string toolId) { LevelId = levelId; ToolId = toolId; }
}

public readonly struct CleanlinessChanged
{
    public float Previous { get; }
    public float Current { get; }
    public CleanlinessChanged(float previous, float current) { Previous = previous; Current = current; }
}

public readonly struct LevelObjectiveMet
{
    public string SessionId { get; }
    public float Cleanliness { get; }
    public LevelObjectiveMet(string sessionId, float cleanliness)
    { SessionId = sessionId; Cleanliness = cleanliness; }
}

public readonly struct LevelCompleted
{
    public LevelResult Result { get; }
    public LevelCompleted(LevelResult result) { Result = result; }
}

public readonly struct SettingChanged
{
    public string Key { get; }
    public SettingChanged(string key) { Key = key; }
}
```

- Subscriber 必须保存并释放 `IDisposable`。
- Scene 卸载前由 Installer 统一释放订阅。
- 高频 `StrokeSample` 不进入全局事件总线，使用直接管线，避免装箱和不可控扇出。
- 单个订阅异常由 EventBus 捕获、记录并继续通知；关键事务不依赖广播成功。

### 3.8 时间与暂停

```csharp
public interface IGameClock
{
    float ScaledDeltaTime { get; }
    float UnscaledDeltaTime { get; }
    double Realtime { get; }
    bool IsPaused { get; }
}
```

- 污渍软化、流动、关卡计时、工具消耗使用 `ScaledDeltaTime`。
- 页面淡入、暂停菜单动画可使用 `UnscaledDeltaTime`。
- 暂停不仅设置 `Time.timeScale=0`，还必须禁用 Gameplay Input Action Map、结束当前笔画、暂停循环音和玩法模拟器。
- 恢复必须由用户点击“继续”触发；应用回前台不能自动继续。

---

## 4. 目录、程序集与代码规范

### 4.1 目录结构

```text
Assets/SwipeClean/
├── Runtime/
│   ├── Core/
│   ├── Content/
│   ├── Domain/
│   ├── Input/
│   ├── Cleaning/
│   ├── Stains/
│   ├── Tools/
│   ├── Levels/
│   ├── Progression/
│   ├── Feedback/
│   ├── UI/
│   ├── Localization/
│   ├── Analytics/
│   └── Platform/
├── Editor/
│   ├── Build/
│   ├── ContentValidation/
│   ├── LevelTools/
│   └── Tests/
├── Tests/
│   ├── EditMode/
│   ├── PlayMode/
│   ├── Fixtures/
│   └── GoldenData/
├── Content/
│   ├── Catalog/
│   ├── Definitions/
│   │   ├── Stains/
│   │   ├── Tools/
│   │   ├── EffectRules/
│   │   ├── Levels/
│   │   ├── Rewards/
│   │   └── Quality/
│   ├── Art/
│   ├── Audio/
│   ├── Materials/
│   ├── Shaders/
│   └── Prefabs/
├── Scenes/
├── LocalizationTables/
└── Generated/
    ├── Catalog/
    └── Validation/
```

第三方内容必须位于 `Assets/ThirdParty/<VendorOrPackage>`，不得散落在业务目录，并附 `LICENSES.md`/NOTICE 引用。

### 4.2 Assembly Definition

| Assembly | 允许引用 | 说明 |
|---|---|---|
| `SwipeClean.Domain` | .NET/BCL | 领域模型、公式、规则，不引用 UnityEngine |
| `SwipeClean.Application` | Domain | 用例、会话、端口接口 |
| `SwipeClean.Core` | Application、UnityEngine | Bootstrap、状态机 Unity 宿主 |
| `SwipeClean.Content` | Domain、UnityEngine | ScriptableObject 与 DTO 映射 |
| `SwipeClean.Input` | Application、InputSystem、UnityEngine | 输入适配 |
| `SwipeClean.Cleaning` | Application、Domain、UnityEngine、URP | GPU 清洁管线 |
| `SwipeClean.Tools` | Application、Domain、Cleaning、UnityEngine | 工具策略与表现适配 |
| `SwipeClean.Feedback` | Application、Domain、UnityEngine | 音频、触觉、粒子 |
| `SwipeClean.Progression` | Application、Domain | 文件适配单独在 Infrastructure namespace |
| `SwipeClean.UI` | Application、Domain、Localization、uGUI/TMP | Presenter 与 View |
| `SwipeClean.Analytics` | Application、Domain | 事件聚合与 NDJSON |
| `SwipeClean.Editor` | 全部运行时程序集、UnityEditor | 不进入 Player |
| `SwipeClean.Tests.EditMode` | 被测程序集、Test Framework | Editor only |
| `SwipeClean.Tests.PlayMode` | 被测程序集、Test Framework | 测试 Player/Editor |

若 `SwipeClean.Progression` 因 JSON/文件实现需要 Unity API，将文件系统拆成 `SwipeClean.Infrastructure`，不得让 Domain 反向引用。

### 4.3 命名规范

- Namespace：`SwipeClean.<Module>[.<Area>]`。
- ScriptableObject：`*DefinitionAsset`；运行时只读对象：`*Definition`；可变对象：`*Runtime`/`*State`。
- 接口只用于跨层端口或确有替换实现的服务，不为每个类机械创建接口。
- 事件使用过去式：`LevelCompleted`；命令使用动词：`StartLevelCommand`。
- bool 使用 `Is/Has/Can/Should`；时间显式带单位，如 `DurationSeconds`。
- 归一化数值后缀 `01`，例如 `Moisture01`；像素/秒等单位写入名称或注释。
- 资产文件使用小写 snake_case ID，C# 类型 PascalCase。

### 4.4 代码规则

- 关键服务使用构造函数注入；MonoBehaviour 依赖由 Scene Installer 显式绑定。
- 禁止运行时在 Gameplay 热路径使用 LINQ、反射、字符串拼接和频繁闭包。
- 禁止 `Update()` 轮询全局状态；按模块由单个 Driver 调度或订阅明确事件。
- 禁止在业务代码中直接使用 `PlayerPrefs`；仅可用于非关键启动标记且本设计默认不使用。
- `OnDestroy`/`Dispose` 必须释放订阅、Render Texture、NativeArray、临时材质和循环音。
- 公共 API 必须有 XML 注释，说明线程、单位、范围、所有权和失败行为。
- Release 中断言不代替错误处理；内容错误在构建期失败，运行时仍要安全回退。
- 本文 `record` 示例只用于纯 C# 不可变 DTO；项目增加以下兼容类型，且不得让 Record 参与 Unity Inspector/`JsonUtility` 序列化：

```csharp
namespace System.Runtime.CompilerServices
{
    internal sealed class IsExternalInit { }
}
```

### 4.5 错误模型

```csharp
public enum AppErrorCode
{
    None,
    Busy,
    InvalidState,
    ContentMissing,
    ContentInvalid,
    SceneLoadFailed,
    SaveReadFailed,
    SaveWriteFailed,
    SaveFutureVersion,
    RenderFormatUnsupported,
    PlatformCapabilityMissing,
    Unknown
}

public sealed record AppError(
    AppErrorCode Code,
    string DiagnosticMessage,
    string? ContextId = null,
    Exception? Exception = null);
```

用户界面不直接显示 `DiagnosticMessage`；由 `ErrorPresenter` 将 Code 映射到本地化文案和可执行操作。日志中不得记录存档完整正文、用户路径之外的个人信息或输入坐标。

---

## 5. 内容数据设计

### 5.1 数据流

```text
ScriptableObject 编辑资产
  → ContentValidator（编辑器/CI）
  → CatalogBaker
      ├→ GameCatalog.asset（强引用运行时资源）
      └→ game_catalog.snapshot.json（审查/版本/测试）
  → Build
  → RuntimeCatalogLoader
  → 不可变 Definition DTO
```

`snapshot.json` 用于审查、差异和自动测试，不承担 Unity 对 Texture/AudioClip/Prefab 的运行时对象加载；资源由生成的 `GameCatalog.asset` 强引用，保证离线与裁剪安全。

### 5.2 通用 ID

```csharp
public readonly struct ContentId : IEquatable<ContentId>
{
    public string Value { get; }
    public ContentId(string value) { Value = value; }
    public bool Equals(ContentId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
    public override bool Equals(object obj) => obj is ContentId other && Equals(other);
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value ?? string.Empty);
    public override string ToString() => Value ?? string.Empty;
}
```

ID 正则：`^[a-z][a-z0-9_]{2,63}$`。建议前缀：

| 类型 | 示例 |
|---|---|
| Level | `level_006_mud` |
| Stain | `stain_mud_medium` |
| Tool | `tool_cloth_basic` |
| Effect rule | `effect_cloth_on_mud_dry` |
| Reward | `reward_level_006_first` |
| Collectible | `collectible_photo_006` |
| Audio profile | `audio_cloth_mud` |

不同类型允许同尾名，但同类型必须唯一；大小写、空格、短横线均视为校验错误。

### 5.3 枚举

```csharp
public enum StainCategory { Dust, FingerprintOil, Mud, Water, Sugar, Sand }
public enum ToolCategory { Cloth, Spray, AbsorbentPaper, SoftBrush, AirBlower, UvInspector }
public enum ToolActionKind { Clean, Wet, Absorb, Brush, Push, Inspect }
public enum SideEffectKind { None, Smear, WaterMark, ScratchRisk, Push, LowEfficiency }
public enum LevelMode { Relax, Standard }
public enum QualityTier { Low, Medium, High }
public enum HapticSemantic { DustLight, GrainPulse, ToolSwitch, StainRelease, WrongTool, LevelComplete }
```

枚举仅表达固定程序语义；内容 ID 不用枚举，避免新增普通内容必须改代码。

### 5.4 污渍定义

```csharp
public sealed record StainDefinition(
    string Id,
    StainCategory Category,
    float Adhesion01,
    float Viscosity01,
    float DefaultMoisture01,
    float DefaultThickness01,
    float SmearFactor01,
    float FlowFactor01,
    bool VisibleUnderUvOnly,
    string RequiredSolventTag,
    float ObjectiveWeight,
    string VisualProfileId,
    string AudioMaterialTag);
```

验证规则：

- 所有 `01` 字段在 `[0,1]`；`ObjectiveWeight ≥ 0`。
- `VisibleUnderUvOnly=true` 的关卡若计入目标，必须允许 UV 或提供自动检查机制。
- `RequiredSolventTag` 非空时必须至少有一个允许工具能提供该标签。
- `VisualProfileId`、`AudioMaterialTag` 必须存在或有确定回退。

### 5.5 工具定义

```csharp
public sealed record ToolDefinition(
    string Id,
    ToolCategory Category,
    float RadiusUv,
    float BaseEfficiency,
    float AbsorptionCapacity,
    float MoistureOutputPerSecond,
    float PushStrength,
    float ScratchRisk01,
    float MaxPressureHoldSeconds,
    IReadOnlyList<string> EffectTags,
    string VisualProfileId,
    string AudioToolTag,
    ToolUpgradeCurve UpgradeCurve);
```

- `RadiusUv` 是相对清洁表面短边的归一化半径，范围建议 `[0.01, 0.25]`。
- 定义中不保存当前饱和度/湿度；这些字段属于 `ToolRuntimeState`。
- UV 的清洁效率固定 0，`Category=UvInspector`，只产生 Inspect 命令。

### 5.6 效果矩阵

```csharp
public sealed record ToolStainEffectRule(
    string Id,
    ToolCategory Tool,
    StainCategory Stain,
    float BaseEfficiency,
    AnimationCurve SpeedModifier,
    AnimationCurve PressureModifier,
    AnimationCurve MoistureModifier,
    string? RequiredTag,
    SideEffectKind WrongSideEffect,
    float SideEffectStrength01,
    string FeedbackProfileId,
    string HintKey);
```

查找键为 `(ToolCategory, StainCategory, stainVariantTag?)`，优先级：关卡覆盖规则 → 污渍变体规则 → 类别默认规则。构建验证要求六类污渍×五基础工具共 30 个类别组合全部有明确规则；UV 单独验证。

最终效率：

```csharp
effectivePerSecond = ClampNonNegative(
    rule.BaseEfficiency
    * definition.BaseEfficiency
    * rule.SpeedModifier.Evaluate(speed01)
    * rule.PressureModifier.Evaluate(pressure01)
    * rule.MoistureModifier.Evaluate(stainMoisture01)
    * solventModifier
    * (1f - saturationPenalty01)
    * upgradeModifier);

cleanAmount = effectivePerSecond * deltaTime;
```

所有曲线输入先 Clamp 到 `[0,1]`；输出 Clamp 到配置上限，任何非有限值立即拒绝并记录内容错误。

### 5.7 关卡定义

```csharp
public sealed record LevelDefinition(
    string Id,
    int ContentVersion,
    int Chapter,
    int Order,
    string DisplayNameKey,
    string ObjectiveKey,
    string BackgroundId,
    float CompletionThreshold01,
    float InspectStartThreshold01,
    float PerfectTolerance01,
    IReadOnlyList<string> AllowedToolIds,
    IReadOnlyList<string> RecommendedSequence,
    IReadOnlyList<StainLayerConfig> StainLayers,
    RewardConfig Rewards,
    string? TutorialFlowId,
    string RevealContentId);

public sealed record StainLayerConfig(
    string InstanceId,
    string StainId,
    string MaskAssetId,
    float InitialMoisture01,
    float InitialThickness01,
    float ObjectiveWeight,
    bool CountsTowardObjective,
    int RenderOrder,
    Vector2 PositionUv,
    Vector2 ScaleUv,
    float RotationDegrees);
```

验证不变量：

- `0 < CompletionThreshold01 ≤ 1`；`0 ≤ InspectStart < CompletionThreshold`。
- `PerfectTolerance01` 建议 0.005；完美条件为 `cleanliness ≥ 1 - tolerance`。
- `InstanceId` 在关卡内唯一；至少一个层计入目标且初始有效质量大于 0。
- 允许工具都存在；推荐顺序是允许工具的子集；至少一条路径能处理所有目标层。
- `Order` 全局连续为 1～12；章节默认 1～4、5～8、9～12。
- 奖励、Reveal、Tutorial、背景和所有遮罩资源均可解析。

### 5.8 Catalog

```csharp
public interface IGameCatalog
{
    string CatalogVersion { get; }
    IReadOnlyList<LevelDefinition> Levels { get; }
    bool TryGetLevel(string id, out LevelDefinition value);
    bool TryGetStain(string id, out StainDefinition value);
    bool TryGetTool(string id, out ToolDefinition value);
    bool TryGetEffect(ToolCategory tool, StainCategory stain, out ToolStainEffectRule value);
}
```

Catalog 加载后构建只读字典；重复 ID 在生成阶段失败，运行时不执行“最后一个覆盖前一个”。CatalogVersion 使用 `yyyy.mm.dd.build` 或内容 Git Hash，必须进入关卡分析事件和构建信息。

---

## 6. 输入系统详细设计

### 6.1 Input Actions

资产：`Assets/SwipeClean/Runtime/Input/SwipeCleanInputActions.inputactions`。

| Action Map | Action | 类型/绑定 | 用途 |
|---|---|---|---|
| `Frontend` | `Point` | Vector2；Touchscreen/Mouse | UI 指针由 EventSystem 使用 |
| `Frontend` | `Submit/Cancel` | Button；触屏 UI/Keyboard | 编辑器测试与 Android 返回 |
| `Gameplay` | `Point` | Pass Through Vector2；primary touch/mouse | 当前屏幕坐标 |
| `Gameplay` | `PrimaryContact` | Pass Through Button；primaryTouch/leftButton | 笔画开始/结束 |
| `Gameplay` | `Pause` | Button；Escape/Android Back | 打开暂停 |
| `Gameplay` | `DebugToggle` | Button；仅 Development | 调试面板 |

UI 使用 `InputSystemUIInputModule`。进入 `Playing/Inspecting` 启用 Gameplay Map；暂停、完成和卸载关卡时禁用，并主动发送 `PointerCancelled`。

### 6.2 输入管线

```text
Input System callback
 → PrimaryPointerTracker（只接受第一有效触点）
 → UiHitGuard（按下点是否被 UI 占用）
 → SurfaceProjector（屏幕坐标→Surface UV）
 → StrokeInterpolator（固定空间距离重采样）
 → StrokeKinematics（平滑速度、长按压力）
 → ActiveToolStrategy
 → BrushCommandQueue / InspectView
```

禁止直接在 Input callback 中修改 Render Texture；callback 只追加轻量原始样本，Gameplay tick 在同一帧消费。

### 6.3 数据结构

```csharp
public readonly struct RawPointerSample
{
    public int PointerId { get; }
    public Vector2 ScreenPositionPx { get; }
    public double TimeSeconds { get; }
    public PointerPhase Phase { get; }

    public RawPointerSample(int pointerId, Vector2 screenPositionPx,
        double timeSeconds, PointerPhase phase)
    {
        PointerId = pointerId;
        ScreenPositionPx = screenPositionPx;
        TimeSeconds = timeSeconds;
        Phase = phase;
    }
}

public readonly struct StrokeSample
{
    public Vector2 SurfaceUv { get; }
    public Vector2 DeltaUv { get; }
    public float SpeedUvPerSecond { get; }
    public float Speed01 { get; }
    public float Pressure01 { get; }
    public double TimeSeconds { get; }
    public bool IsFirst { get; }
    public bool IsLast { get; }

    public StrokeSample(Vector2 surfaceUv, Vector2 deltaUv, float speedUvPerSecond,
        float speed01, float pressure01, double timeSeconds, bool isFirst, bool isLast)
    {
        SurfaceUv = surfaceUv;
        DeltaUv = deltaUv;
        SpeedUvPerSecond = speedUvPerSecond;
        Speed01 = speed01;
        Pressure01 = pressure01;
        TimeSeconds = timeSeconds;
        IsFirst = isFirst;
        IsLast = isLast;
    }
}
```

`PointerPhase` 为 `Began/Moved/Ended/Cancelled`。输入队列为预分配 ring buffer，容量默认 128；溢出时保留 `Began`、最新位置和 `Ended`，合并中间 Move，并记录一次限频警告。

### 6.4 主触点规则

1. 没有活动笔画时，第一根 `Began` 且命中清洁表面的触点成为主触点。
2. 若按下点命中 UI，整个触点生命周期都不进入清洁管线，即使随后移出 UI。
3. 主触点活动时忽略其他触点；它们结束后也不能接管当前笔画。
4. 主触点 `Ended/Cancelled`、暂停、工具切换、Scene 卸载时结束笔画并清空前一点。
5. 下一触点重新建立轨迹，禁止从前一个结束点连接。
6. 编辑器鼠标使用固定 PointerId `-1`，行为与单触点一致。

### 6.5 屏幕坐标映射到 UV

Gameplay 使用正交相机和矩形 `CleaningSurfaceView`。不依赖 Physics Raycast；`SurfaceProjector` 将屏幕点投影到表面所在平面，再变换到局部坐标：

```csharp
public bool TryProject(Vector2 screenPx, out Vector2 uv)
{
    Ray ray = _camera.ScreenPointToRay(screenPx);
    if (!_plane.Raycast(ray, out float distance)) { uv = default; return false; }
    Vector3 local = _surfaceTransform.InverseTransformPoint(ray.GetPoint(distance));
    uv = new Vector2(local.x / _size.x + 0.5f, local.y / _size.y + 0.5f);
    return uv.x >= _extendedMin.x && uv.x <= _extendedMax.x
        && uv.y >= _extendedMin.y && uv.y <= _extendedMax.y;
}
```

- 清洁命令最终 UV Clamp 到 `[0,1]`。
- 为边角可清洁性允许命中边界扩展 `brushRadiusUv × 0.5`，但 UI 和系统安全区仍优先。
- 工具视觉偏移以屏幕 dp 配置，先偏移屏幕作用点再投影；HUD 显示真实作用圆。
- 屏幕比例变化、Safe Area 更新或相机布局变化后重建 Surface Plane 和尺寸缓存。

### 6.6 轨迹插值

```csharp
distance = length(currentUv - previousUv)
spacing = max(activeRadiusUv * 0.4f, MinSpacingUv)
segments = clamp(ceil(distance / spacing), 1, MaxSegmentsPerInput)

for i in 1..segments:
    t = i / segments
    emit lerp(previous, current, t)
```

设计参数：

| 参数 | 默认值 | 说明 |
|---|---:|---|
| `MinSpacingUv` | 0.002 | 防止极小半径产生过量样本 |
| `MaxSegmentsPerInput` | 24 | 单次原始事件上限 |
| 每帧所有工具最大样本 | 质量档 32/48/64 | 超出部分保留顺序并延后最多 1 帧 |
| 最大延后 | 16ms（60fps）/33ms（30fps） | 超过则合并路径，禁止积累多帧输入债务 |

零距离事件只更新压力/时间，不生成重复 Stamp；时间倒退或非有限坐标丢弃并记录 Development 警告。

### 6.7 速度与长按压力

原始速度为 `distanceUv / max(deltaTime, 1/240)`，使用指数平滑：

```text
smoothedSpeed = lerp(previousSpeed, rawSpeed, 1 - exp(-smoothingHz × dt))
speed01 = inverseLerp(toolSlowSpeed, toolFastSpeed, smoothedSpeed)
pressure01 = saturate((now - pointerDownTime) / toolMaxPressureHoldSeconds)
```

- 默认 `smoothingHz=18`；不同工具可配置慢/快阈值。
- 插入点的速度继承当前平滑速度，不能因细分而变慢。
- 压力基于持有时间，不读取真实硬件 pressure；`MaxPressureHoldSeconds` 建议 0.6～1.2 秒。
- 帧率独立测试用固定时间戳轨迹，不用按 Frame 数回放。

### 6.8 延迟预算

| 阶段 | P95 预算 |
|---|---:|
| 系统触摸到 Input callback | 平台固有，测量不单独承诺 |
| callback 到样本消费 | ≤1 游戏帧 |
| 投影、插值、工具规则 | <1ms CPU |
| 命令提交与 Shader | 当帧提交 |
| 屏幕呈现总目标 | <50ms |

通过 Development Overlay 显示最近输入时间、命令提交帧和渲染完成帧；最终用高帧率相机或平台工具测触点到画面，而不是只测 C# 方法耗时。

---

## 7. 清洁渲染核心

### 7.1 运行时对象

```text
CleaningSurfaceRuntime
 ├── SurfaceDescriptor
 ├── StainLayerRuntime[1..N]
 │    ├── Definition + LevelConfig
 │    ├── StateRead RT
 │    ├── StateWrite RT
 │    ├── InitialMeanMass
 │    └── Dirty flags / CPU semantic state
 ├── BrushCommandQueue
 ├── StainUpdateScheduler
 ├── CoverageCalculator
 └── SurfaceCompositeRenderer
```

`CleaningSurfaceRuntime` 实现 `IDisposable`；关卡退出按“停止输入→等待/取消回读→释放临时 RT→释放持久 RT→清空命令”的顺序销毁。

### 7.2 状态纹理

每个活跃污渍层使用两张 ping-pong 状态纹理：

| Channel | 含义 | 范围 |
|---|---|---:|
| R | 当前有效污渍质量/可见遮罩（初始 mask×thickness） | 0..1 |
| G | 局部湿度 | 0..1 |
| B | 涂抹/扰动强度，供材质和近似流动 | 0..1 |
| A | 水痕/副作用或扩展通道 | 0..1 |

Descriptor：`GraphicsFormat.R8G8B8A8_UNorm`、无深度、无 mipmap、Bilinear、Clamp、随机写入关闭。启动时用 `SystemInfo.IsFormatSupported(..., FormatUsage.Render)` 验证；不支持则回退官方可渲染 RGBA8 格式。任何格式均不得改变通道语义。

内存估算（只含层状态 ping-pong）：

| 长边 | 单张 1:1 RGBA8 | 每层双缓冲 | 6 层 |
|---:|---:|---:|---:|
| 512 | 1MB | 2MB | 12MB |
| 768 | 2.25MB | 4.5MB | 27MB |
| 1024 | 4MB | 8MB | 48MB |

非正方形表面按比例缩短短边；实际分配和峰值写入性能报告。超过当前档位活跃层上限时，关卡构建校验失败，不在运行时静默丢层。

### 7.3 初始化

1. 从关卡配置解析污渍定义和初始遮罩。
2. 按质量档位创建 StateRead/StateWrite。
3. 初始化 Shader 将遮罩 alpha×`InitialThickness01` 写入 R、初始湿度写 G，其余通道清零。
4. 执行一次覆盖率归约，保存每层 `InitialMeanMass`。
5. 若计入目标层的初始总质量 ≤epsilon，关卡加载失败。
6. 建立 Composite MaterialPropertyBlock，不实例化重复材质。

### 7.4 BrushCommand

```csharp
public enum BrushOperation { Clean, AddMoisture, Absorb, Smear, Push, Reveal }

public readonly struct BrushCommand
{
    public int LayerIndex { get; }
    public BrushOperation Operation { get; }
    public Vector2 CenterUv { get; }
    public Vector2 DirectionUv { get; }
    public float RadiusUv { get; }
    public float Hardness01 { get; }
    public float StrengthPerSecond { get; }
    public float MoistureDeltaPerSecond { get; }
    public float SideEffectStrength01 { get; }
    public float DeltaTime { get; }
    public uint Sequence { get; }

    public BrushCommand(int layerIndex, BrushOperation operation, Vector2 centerUv,
        Vector2 directionUv, float radiusUv, float hardness01, float strengthPerSecond,
        float moistureDeltaPerSecond, float sideEffectStrength01, float deltaTime, uint sequence)
    {
        LayerIndex = layerIndex;
        Operation = operation;
        CenterUv = centerUv;
        DirectionUv = directionUv;
        RadiusUv = radiusUv;
        Hardness01 = hardness01;
        StrengthPerSecond = strengthPerSecond;
        MoistureDeltaPerSecond = moistureDeltaPerSecond;
        SideEffectStrength01 = sideEffectStrength01;
        DeltaTime = deltaTime;
        Sequence = sequence;
    }
}
```

- Command 中所有值在入队前 Clamp/验证；`DeltaTime` 上限 1/15 秒，避免切后台后一帧巨量变化。
- `Sequence` 在会话内单调递增，跨 batch 保持笔画顺序。
- UV 检查不写状态纹理，走 `UvInspectionController`。
- 单个 StrokeSample 可以针对多个命中层生成命令，但由工具规则决定穿透和层顺序。

### 7.5 命令生成与层命中

`ToolEffectResolver` 从上到下检查笔刷范围内存在质量的层：

1. 读取最近一次低分辨率 layer occupancy tile，快速过滤明显空层；
2. 依据工具类别、污渍类别、标签和上层阻挡规则取得 `ToolStainEffectRule`；
3. 计算效率、湿度/饱和修正和副作用；
4. 默认只处理第一个可作用的阻挡层；气吹、喷雾可按配置作用多个层；
5. 生成 BrushCommand，并向关卡统计器记录聚合操作距离/时间，不记录坐标。

Occupancy 只用于优化，不能作为最终规则真值；未知/过期 tile 必须按“可能有污渍”处理。

### 7.6 批量 Shader 更新

不为每个 Stamp 调用一次 Blit。`StainUpdateScheduler` 按 `LayerIndex + Operation` 稳定分组，并按质量档的 batch 容量提交全屏 pass：

```text
StateRead + Stamp arrays
  → StainUpdate.shader Pass(Clean/Wet/Absorb/Smear/Push)
  → StateWrite
  → swap(Read, Write)
```

Shader 参数固定数组：

```hlsl
float4 _Stamp0[MAX_STAMPS]; // center.xy, radius, hardness
float4 _Stamp1[MAX_STAMPS]; // dir.xy, strength, deltaTime
float4 _Stamp2[MAX_STAMPS]; // moistureDelta, sideEffect, flags, reserved
int _StampCount;
```

每像素计算到 Stamp 中心距离，以 `smoothstep(radius, radius*hardness, distance)` 得到影响权重。同一 batch 内按序累计并逐步 saturate。

各 Pass 语义：

| Pass | 通道变化 |
|---|---|
| Clean | `R = max(0, R - strength*influence*dt)`；按规则少量降低 B/A |
| AddMoisture | `G = saturate(G + moisture*influence*dt)`；R 默认不变 |
| Absorb | 先降低 G，再按规则降低水类 R；工具吸收量在 CPU 同步累计 |
| Smear | 执行小范围方向采样/扩张，尽量守恒 R；增加 B；强度封顶 |
| Push | 沿反方向采样 R/G，产生受控位移；边界质量丢失是否算清除由规则配置 |
| WaterMark | 增加 A 或在专用水痕层增加有限质量；不能递归生成水痕 |

复杂 Smear/Push 需要邻域采样，因此必须使用 Read→Write ping-pong；简单 Clean/Wet 仍走相同管线，保证排序一致。每层每帧最多执行 `MaxPassesPerLayer`（默认 4）；超出时相邻同类命令合并或顺延，绝不能无限堆积。

### 7.7 稳定性保护

- 所有 Shader 输出 `saturate` 到 `[0,1]`；Development 开启 NaN 检测/替代色。
- 扩散不凭空增加 R；若产品需要额外难度，通过有上限的 WaterMark 副作用层实现。
- 水痕层设置 `canGenerateWaterMark=false`，避免递归。
- 方向为零时 Push 降级为局部扰动，不做无效归一化。
- 单帧 `DeltaTime` 钳制；暂停时不提交玩法更新。
- 任何 Shader/格式初始化失败，关卡加载终止并回到选关，不能展示半初始化表面。

### 7.8 覆盖率计算

`CoverageCalculator` 以质量档 4/6/8Hz 调度；同一时刻最多一个 GPU 回读请求。

```text
每层 StateRead.R
 → 2×2 平均下采样链（Reduction Shader）
 → 1×1 meanMass RT
 → 汇总为 1×N 小纹理
 → AsyncGPUReadback
 → CPU 按 ObjectiveWeight 汇总
```

计算公式：

```text
initialTotal = Σ(initialMeanMass[i] × objectiveWeight[i])
remainingTotal = Σ(currentMeanMass[i] × objectiveWeight[i])
cleanliness = clamp01(1 - remainingTotal / max(initialTotal, epsilon))
```

- `countsTowardObjective=false` 的层不参与总和。
- `SystemInfo.supportsAsyncGPUReadback=true` 时使用异步回读。
- 不支持时，降为 4Hz，把各层归约到极小纹理后同步读取；禁止读取原始完整状态纹理。
- 回读失败保留上一次有效值并退避一次周期；连续 3 次失败记录 Error，关卡仍可操作，完成判定进入安全降级检查。
- UI 显示值使用阻尼插值；目标判断使用原始快照。

### 7.9 完成判定防抖

```csharp
if (snapshot.Cleanliness01 >= threshold)
    consecutivePasses++;
else if (snapshot.Cleanliness01 < threshold - hysteresis)
    consecutivePasses = 0;

if (consecutivePasses >= 2 && !objectiveAlreadyMet)
    PublishObjectiveMetOnce();
```

默认 `hysteresis=0.002`。进入 `Inspecting` 的条件是第一次达到 `InspectStartThreshold`；该提示每次会话只自动出现一次。完成后立即停止输入和污渍模拟，再播放完成演出。

### 7.10 表面合成 Shader

`SurfaceComposite.shader` 接收背景、每层状态纹理、VisualProfile、UV 可见度和全局完成参数：

```text
Background/Reveal
 → stain layers ordered bottom-to-top
 → moisture/specular response
 → disturbance/watermark
 → UV visibility mask
 → completion polish/highlight
```

- 普通污渍 alpha 主要来自 R，并由 VisualProfile 曲线调整。
- 湿度 G 改变反光、颜色和软化外观，不得单独使目标“消失”。
- `VisibleUnderUvOnly` 层的可见 alpha 乘以动态 UV 光区 mask；其 R 仍参与目标。
- 使用 `MaterialPropertyBlock`；不在运行时为每层 `new Material`。
- 最终揭晓图片始终存在背景层，不在完成时重新下载/加载。

### 7.11 液体近似

MVP 液体逻辑分为两层：

1. **规则层**：G 湿度、吸收量、软化时间、WaterMark，所有画质一致；
2. **表现层**：低频 Push/Flow pass、UV 偏移、粒子，按画质降级。

Medium/High 每 100～250ms 对 FlowFactor>0 的层执行一次低强度重力方向 Push；High 可增加邻域合并视觉。Low 不执行流动 pass，只更新规则和基础材质。关卡阈值、评分和工具有效性不能依赖表现层位移。

### 7.12 资源释放与场景重开

关卡重开必须创建新 `LevelSession` 和全新状态纹理。释放检查：

- 所有 `AsyncGPUReadbackRequest` 已完成、忽略回调或持有会话世代号；旧回调不得写入新会话。
- Render Texture 调用 `Release` 并销毁拥有的 Unity Object。
- 临时 CommandBuffer 清空并释放。
- MaterialPropertyBlock 可复用但清空纹理引用。
- 粒子/音频对象回收到会话池并停止。
- Development 记录重开前后 `RenderTexture` 数和估算字节，持续增长视为测试失败。

---

## 8. 污渍规则详细设计

### 8.1 运行时状态

```csharp
public sealed class StainLayerRuntime
{
    public string InstanceId { get; }
    public StainDefinition Definition { get; }
    public float MeanMoisture01 { get; private set; }
    public float SofteningProgress01 { get; private set; }
    public bool HasBeenDiscovered { get; private set; }
    public bool CanGenerateWaterMark { get; }
    public uint Revision { get; private set; }
}
```

像素状态以 GPU 为真值；`MeanMoisture01` 等 CPU 字段是玩法语义/统计近似，不替代每像素 Shader 状态。每次有效规则变更增加 Revision，反馈与覆盖率快照带 Revision 以排除过期结果。

### 8.2 六类默认规则

| 污渍 | 初始特征 | 推荐处理 | 错误路径 | 表现标签 |
|---|---|---|---|---|
| 灰尘 | 低附着、低湿度、可推动 | 气吹/软刷/干布 | 喷雾使其变湿、布效率下降 | `dry_soft`、轻粒子 |
| 指纹油污 | 中附着、高黏、薄层反光 | 喷雾软化→布 | 干布低效并 Smear | `oily`、低频摩擦 |
| 泥点 | 高厚度、中附着、低湿 | 喷湿→湿布 | 干擦 Smear | `mud_grain`、边缘扩张 |
| 水滴 | 流动、低附着 | 吸水纸 | 布拖出 WaterMark | `liquid`、滴水音 |
| 糖渍 | 高黏、高附着 | 喷雾→等待→湿布 | 过早擦低效、拉丝/B 增加 | `sticky`、粘连音 |
| 沙粒 | 颗粒、上层阻挡 | 气吹/软刷 | 布提示 ScratchRisk、低效 | `hard_grain`、颗粒脉冲 |

这些是 Catalog 默认值；每关可覆盖初始状态和效果强度，但不能覆盖到产生不可恢复失败。

### 8.3 软化状态

```text
if local/mean moisture >= softenMoistureThreshold:
    softening += dt / softenDuration
else:
    softening -= dt / dryBackDuration
softening = clamp01(softening)
```

- 油污/糖渍/泥点分别配置阈值和时间；建议 1～3 秒。
- 软化进度通过光泽、边缘、泡沫或颜色变化呈现，不以纯倒计时为唯一表达。
- 暂停时不推进；放松和标准模式使用相同物理时间。
- 正确工具效率取 MoistureModifier 与 SofteningProgress 的组合。

### 8.4 扩散与水痕

- Smear pass 扩大受影响视觉区域并降低局部峰值，R 总量近似守恒；错误统计增加一次并有 0.75 秒去抖。
- WaterMark 使用专用副作用层，最大新增有效质量为该关初始目标质量的配置比例（默认 5%，上限 10%）。
- 同一笔画连续错误不每个 Stamp 弹提示；`WrongToolHintCooldown=2s`。
- 提示触发后推荐下一步来自 EffectRule 的本地化 `HintKey`，不在代码拼接污渍名。

### 8.5 沙粒阻挡和划痕风险

- 沙粒位于其他污渍之上时，布的 Clean 命令不能穿透到下层。
- 错误布擦只增加 `ScratchRisk` 统计、短视觉痕迹和双击触觉；痕迹不进入永久存档，也不阻止完成。
- 移除沙粒后，临时风险痕迹在后续正确擦拭或完成演出时消失。
- 商店/页面不得把该效果描述为真实屏幕损坏。

### 8.6 隐藏污渍与 UV

`UvInspectionController` 保存屏幕/Surface UV 光区中心、半径和 0..1 强度；Composite Shader 计算软边圆形 mask。发现规则：UV 强度超过 0.5 且光区与隐藏层 occupancy 相交连续 100ms，发布 `HiddenStainDiscovered`，每层一次。

没有 occupancy 数据时采用保守策略：在光区可见度中正常显示，由玩家观察；不能仅凭 CPU tile 错误地阻止显示。切换回清洁工具后 UV 光区 150ms 淡出，减少突变；降低动态效果时立即切换。

---

## 9. 工具体系详细设计

### 9.1 接口与调度

```csharp
public interface IToolStrategy
{
    ToolCategory Category { get; }
    void Begin(in ToolContext context, in StrokeSample sample);
    void Apply(in ToolContext context, in StrokeSample sample);
    void End(in ToolContext context, in StrokeSample sample);
    void Cancel(in ToolContext context);
}

public sealed class ToolContext
{
    public ToolDefinition Definition { get; init; }
    public ToolRuntimeState State { get; init; }
    public IToolEffectResolver Effects { get; init; }
    public IBrushCommandSink Commands { get; init; }
    public ILevelActionRecorder Recorder { get; init; }
    public IFeedbackSink Feedback { get; init; }
}
```

`ToolController` 持有当前 Strategy。切换顺序必须是：取消旧笔画→停止旧循环反馈→保存旧工具会话状态→替换 Strategy→刷新指针/HUD→播放切换反馈。完整过程目标 ≤100ms。

### 9.2 工具运行时状态

```csharp
public sealed class ToolRuntimeState
{
    public string ToolId { get; }
    public int UpgradeLevel { get; }
    public float Saturation01 { get; private set; }
    public float Moisture01 { get; private set; }
    public float ConsumableUsed { get; private set; }
    public bool IsActive { get; private set; }
}
```

- 每次关卡会话按玩家工具等级创建新 State。
- `Saturation01/Moisture01` 不写入永久存档。
- 更换/拧干将 Saturation 设为 0；不得消耗金币或展示广告。
- 工具升级等级从存档读取，定义/曲线从 Catalog 读取，运行时只缓存计算结果。

### 9.3 超细纤维布

`ClothToolStrategy`：

1. 对触点覆盖的最上层有效污渍解析 Clean 或 Smear。
2. 与液体/喷雾接触时增加布 Moisture；吸入量增加 Saturation。
3. `saturationPenalty = SaturationCurve(Saturation01)`；默认在 70% 后明显下降。
4. 饱和且继续擦拭时，可按规则生成有限 WaterMark。
5. 指针显示布接触轮廓、干/湿状态和饱和提示。

建议默认：半径 0.055～0.08 UV；压力提高厚污渍效率；油污慢擦更有效。具体数值由 M1/M2 调参，不硬编码在 Strategy。

### 9.4 喷雾

`SprayToolStrategy`：

- 按下/拖动持续生成 AddMoisture 命令；喷雾锥形表现不改变圆形规则半径。
- `ConsumableUsed += outputPerSecond * dt`，仅用于评分；MVP 不会真正耗尽。
- 大多数污渍 R 不直接减少；效果矩阵可允许极弱的视觉冲洗，但不替代后续工具。
- 在已充分湿润区域持续喷洒不无限生成水痕；湿度饱和后仅计适量浪费。
- 喷雾音为可平滑启动/停止的循环或短片段池，快速点按无爆音。

### 9.5 吸水纸

`AbsorbentPaperToolStrategy`：

- 优先吸收水类污渍与 G 湿度，生成 Absorb 命令。
- 吸收量按命令预估并由下次覆盖率/湿度快照校正，避免 CPU 与 GPU 长期漂移。
- Saturation 达 1 后效率降到配置最小值，不变负；HUD 显示“更换”按钮。
- 对固体污渍默认低效且不扩散，除非矩阵定义水痕副作用。

### 9.6 软刷

`SoftBrushToolStrategy`：

- 对灰尘、沙粒生成 Brush/Clean/Push 组合；方向来自 `StrokeSample.DeltaUv`。
- 静止长按不持续跨屏推动颗粒，只在半径内刷动。
- 对水、油、糖默认效率接近 0，并触发限频提示。
- 刷毛视觉比实际半径略大时，HUD 必须显示真实作用范围或保持偏差 ≤10%。

### 9.7 气吹

`AirBlowerToolStrategy`：

- 按住时沿工具朝向生成 Push；拖动改变风向和作用中心。
- 对灰尘/沙粒可在推到可清除边界时降低 R；对水滴只移动，不应凭空蒸发，除非关卡规则允许。
- 对油/糖效率 0；无效操作仍播放气流，但在 2 秒内提示。
- 边界清除规则写入 EffectRule，避免玩家把泥/糖推离屏幕绕过机制。

### 9.8 UV 检查

`UvInspectorStrategy` 不向 BrushCommandQueue 写命令，只更新 `UvInspectionController`。切换 UV 时保留上一个清洁工具 ID；再次点击 UV 或点击“返回工具”恢复。UV 不参与五工具升级，不消耗资源，不计错误。

### 9.9 效果解析

```csharp
public readonly struct ResolvedToolEffect
{
    public bool IsEffective { get; }
    public float CleanPerSecond { get; }
    public float MoisturePerSecond { get; }
    public SideEffectKind SideEffect { get; }
    public float SideEffectStrength01 { get; }
    public string FeedbackProfileId { get; }
    public string HintKey { get; }

    public ResolvedToolEffect(bool isEffective, float cleanPerSecond,
        float moisturePerSecond, SideEffectKind sideEffect, float sideEffectStrength01,
        string feedbackProfileId, string hintKey)
    {
        IsEffective = isEffective;
        CleanPerSecond = cleanPerSecond;
        MoisturePerSecond = moisturePerSecond;
        SideEffect = sideEffect;
        SideEffectStrength01 = sideEffectStrength01;
        FeedbackProfileId = feedbackProfileId;
        HintKey = hintKey;
    }
}
```

`ToolEffectResolver` 是纯 C#、无 Unity Object 的服务，可注入曲线采样后的 LUT。输入包含工具定义/等级/状态、污渍定义/语义状态、速度、压力和模式；输出必须确定性。Shader 只执行结果，不重新决定业务规则。

### 9.10 升级

`ToolUpgradeCurve` 每级给出 Radius/Capacity/Efficiency 的乘数和 VisualProfile：

```csharp
public sealed record ToolUpgradeLevel(
    int Level,
    int CostCoins,
    float RadiusMultiplier,
    float CapacityMultiplier,
    float EfficiencyMultiplier,
    string VisualProfileId);
```

- 等级连续从 1 开始，最高级成本可为 0/无下一项。
- 升级不能改变 ToolCategory 或 EffectTags，不得绕过关卡所需顺序。
- 12 关必须可用基础等级完成；自动化测试使用一级工具验证阈值可达。
- 购买由 Progression 事务完成，Tool 页面不能先播放成功再扣款。

---

## 10. 关卡会话、目标与评分

### 10.1 LevelSession 生命周期

```text
Created
 → LoadingContent
 → InitializingSurface
 → Ready
 → Intro
 → Playing
 ↔ Paused
 → Inspecting
 → ObjectiveMet
 → Completing
 → ResultBuilt
 → Disposed
```

每个会话生成不可重复 `SessionId`（GUID N 格式）和递增 `Generation`。所有异步回调携带 Generation；如果与当前会话不符则丢弃。

```csharp
public sealed class LevelSession : IDisposable
{
    public string SessionId { get; }
    public LevelDefinition Level { get; }
    public LevelMode Mode { get; }
    public LevelSessionState State { get; }
    public LevelMetrics Metrics { get; }
    public CleanlinessSnapshot LatestCleanliness { get; }
}
```

### 10.2 加载顺序

1. 校验 AppState 为 `LoadingLevel` 且无当前 Session。
2. 从 Catalog 取得 LevelDefinition；检查解锁状态。
3. 异步加载 Gameplay Scene。
4. Scene Installer 绑定相机、Surface、HUD、Feedback roots。
5. 创建 LevelSession 和工具状态。
6. 初始化所有污渍层、初始覆盖率和前后对比快照。
7. 预热本关音频、材质和粒子池。
8. 绑定 HUD/Presenter，进入 Intro。
9. Intro 结束后启用输入，开始有效用时。

任一步失败进入统一清理路径，释放已创建资源，再回选关并显示本地化错误。不得让 Loading 遮罩永久停留。

### 10.3 暂停、重开与退出

- 暂停调用 `LevelSession.Pause(reason)`；重复暂停幂等。
- 后台/焦点丢失 Reason 为 `ApplicationInterrupted`，回前台显示暂停页。
- 重开需要确认后先 Dispose 当前 Session，再以同一 Level/Mode 新建 Session；Attempt+1。
- 退出未完成关记录 `level_abandoned`，不写逐像素进度；永久进度不改变。
- 完成演出开始后暂停按钮禁用 1.5 秒以内，生命周期中断仍能安全暂停/结算。

### 10.4 目标评估器

```csharp
public interface IObjectiveEvaluator
{
    ObjectiveEvaluation Evaluate(
        in CleanlinessSnapshot cleanliness,
        in LevelMetrics metrics,
        in LevelDefinition level);
}

public readonly struct ObjectiveEvaluation
{
    public bool ShouldEnterInspecting { get; }
    public bool IsComplete { get; }
    public bool IsPerfect { get; }
    public float Cleanliness01 { get; }

    public ObjectiveEvaluation(bool shouldEnterInspecting, bool isComplete,
        bool isPerfect, float cleanliness01)
    {
        ShouldEnterInspecting = shouldEnterInspecting;
        IsComplete = isComplete;
        IsPerfect = isPerfect;
        Cleanliness01 = cleanliness01;
    }
}
```

完成只依赖清洁度与隐藏目标有效计入，标准/放松模式均无强制失败。`IsComplete` 由连续快照防抖决定；`IsPerfect` 使用 `1 - PerfectTolerance01`。

### 10.5 指标聚合

```csharp
public sealed class LevelMetrics
{
    public double ActiveDurationSeconds { get; private set; }
    public double ToolActiveDurationSeconds { get; private set; }
    public float TotalStrokeDistanceUv { get; private set; }
    public float EffectiveStrokeDistanceUv { get; private set; }
    public float ConsumableUsed { get; private set; }
    public int WrongToolCount { get; private set; }
    public int HintCount { get; private set; }
    public HashSet<string> DiscoveredHiddenStains { get; }
    public List<string> ToolSequence { get; }
}
```

- 暂停、Intro、Results 时间不计 ActiveDuration。
- 错误次数按“同一工具+污渍+后果，0.75 秒窗口”聚合，不按 Stamp 数累加。
- ToolSequence 只在工具真实产生操作后记录，单纯点选不用于顺序评分。
- 数据仅在内存聚合，关卡结束一次写入结果/本地分析。

### 10.6 评分器

`ScoreCalculator` 为纯 C# 确定性函数：

```csharp
public sealed record ScoreBreakdown(
    float CleanlinessPoints,
    float ToolChoicePoints,
    float OperationEfficiencyPoints,
    float ConsumablePoints,
    float HiddenStainPoints,
    float Total0To100,
    int Stars);
```

建议初始公式：

```text
cleanlinessScore = clamp01(cleanliness / threshold)
toolChoiceScore = clamp01(effectiveToolTime / max(totalToolTime, epsilon))
operationScore = clamp01(effectiveDistance / max(totalDistance, epsilon) / referenceEfficiency)
consumableScore = clamp01(1 - max(0, used - freeAllowance) / max(referenceUse, epsilon))
hiddenScore = discoveredAndCleaned / max(hiddenTargetCount, 1)

total = 50*cleanlinessScore
      + 20*toolChoiceScore
      + 15*operationScore
      + 10*consumableScore
      + 5*hiddenScore
```

没有隐藏污渍时 5 分转入 Cleanliness：55/20/15/10。Relax 模式仍计算，但结算弱化负面维度；提示不阻止三星。边界：完成至少 1 星；`total>=75` 为 2 星；`total>=90` 为 3 星。结果存 `0..100` 浮点和 UI 整数，不使用 UI 取整反推星级。

### 10.7 LevelResult

```csharp
public sealed record LevelResult(
    string SessionId,
    string LevelId,
    int ContentVersion,
    LevelMode Mode,
    int Attempt,
    DateTimeOffset CompletedAtUtc,
    float Cleanliness01,
    bool IsPerfect,
    double DurationSeconds,
    ScoreBreakdown Score,
    int WrongToolCount,
    int HintCount,
    float ConsumableUsed,
    IReadOnlyList<string> DiscoveredHiddenStains);
```

生成后不可变；先交给 Progression 事务提交，成功后结算页展示“已保存”。保存失败时仍可展示结果，但奖励区显示重试，禁止开始下一关导致事务丢失。

### 10.8 教学系统

教学是数据驱动步骤状态机，不写在 Level 1 场景脚本中：

```csharp
public sealed record TutorialStep(
    string Id,
    TutorialTrigger Trigger,
    TutorialCompletion Completion,
    string PromptKey,
    string GestureVisualId,
    bool BlocksOtherInput,
    float AutoHintDelaySeconds,
    string NextStepId);
```

支持触发/完成条件：进入关卡、选择工具、首次有效擦拭、清除指定比例、使用提示、完成。步骤只拦截与当前教学冲突的输入；优先用动画+高亮，文本为辅助。每步发布 `tutorial_step`。退出未完成时下次重播当前关教学；设置可清除教程完成标记。

### 10.9 检查阶段

- 首次越过 `InspectStartThreshold` 时提示一次残留区域。
- CoverageReducer 同时产出 8×8/16×16 occupancy 摘要，选择剩余质量最高的 tile 显示软边光晕。
- 提示区域不能精确显示隐藏 UV 污渍本体；对隐藏层提示“使用 UV 检查”并引导入口。
- 玩家继续操作后仍处于 Inspecting，但状态机可复用 Playing 输入；只有 HUD 表现不同。
- 达标完成后立即撤销提示并锁输入。

### 10.10 十二关技术配置矩阵

| 关 | 技术组合 | 新启用组件 | 特殊验证 |
|---:|---|---|---|
| 1 | 指纹弱化变体+布 | Tutorial、Clean pass | 首触延迟、无文字理解 |
| 2 | 灰尘+布/气吹 | Push、边角提示 | 快速轨迹、安全区 |
| 3 | 水+吸水纸 | Absorb、Saturation、WaterMark | 更换恢复、Low 等价 |
| 4 | 油+喷雾/布 | AddMoisture、Softening、Smear | 暂停软化时间 |
| 5 | 缝隙灰+软刷 | 小半径、偏移指针 | 不需像素级触摸 |
| 6 | 泥+喷雾/布 | Smear 上限、湿布状态 | 错误后可恢复 |
| 7 | 沙+底层污渍 | 多层阻挡、ScratchRisk | 不穿透上层 |
| 8 | 糖+喷雾/布 | 黏度、等待、拉丝 B | 可见状态替代倒计时 |
| 9 | 水/泥/灰 | 多类并存、频繁切换 | 100ms 切换、命令排序 |
| 10 | 涂鸦表现+Reveal | 收藏、对比、完成演出 | 原创素材、幂等奖励 |
| 11 | 隐藏污渍 | UV、Discover、隐藏计分 | 未发现不可误完成 |
| 12 | 六类全工具 | 最大层/反馈/性能组合 | 最低设备帧率与可恢复 |

---

## 11. 前端、HUD 与交互架构

### 11.1 UI 模式

采用 MVP（Model–View–Presenter）：

```csharp
public interface IScreenView
{
    void SetVisible(bool visible);
    void SetBusy(bool busy);
    void ShowError(LocalizedError error);
}

public interface IScreenPresenter : IDisposable
{
    void Enter(ScreenArgs args);
    void Exit();
}
```

View 仅暴露序列化组件和渲染方法；Presenter 调用 Application/Progression 服务并订阅状态。按钮事件在 View 中转发为 C# event，Presenter Dispose 时解除。

### 11.2 Screen Router

Frontend 页面栈：`Home` 为根；`LevelSelect/Tools/Collection/Settings` 入栈；确认弹窗是独立 Modal 栈。Gameplay HUD 不进入 Frontend 栈，暂停/设置由 Gameplay Router 管理。

```csharp
public interface INavigationService
{
    bool IsBusy { get; }
    void Push(ScreenId id, ScreenArgs args = default);
    void Replace(ScreenId id, ScreenArgs args = default);
    void Pop();
    void ShowModal(ModalId id, ModalArgs args);
}
```

进入关卡、升级、重置、领奖等异步命令期间 `IsBusy=true` 并禁用重复提交；视觉显示加载而不是静默忽略。

### 11.3 页面详细绑定

| 页面 | Presenter 输入 | View Model | 命令 |
|---|---|---|---|
| PrivacyNotice | 首次标记、触觉能力、Locale | 离线说明、触觉开关/演示 | Accept、ToggleHaptics |
| Home | PlayerSnapshot、下一关 | 继续关、金币、导航入口 | Continue、Open* |
| LevelSelect | 12 关定义+进度 | Lock/Stars/Collectible/Selected | StartLevel、Back |
| GameplayHUD | Session、工具、清洁度 | 工具栏、状态、提示、UV、暂停 | SelectTool、Hint、Pause |
| Pause | Session/Settings | Continue/Restart/Settings/Exit | 对应用例 |
| Results | LevelResult+CommitResult | 分项、奖励、新纪录、对比 | Next、Replay、Select |
| Tools | ToolDefinitions+Inventory | 等级、属性、成本、外观 | Upgrade、SelectPreview |
| Collection | CollectibleDefinitions+State | 锁定/解锁/详情 | Open、Back |
| Settings | SettingsData+Capability | 音量、触觉、手型、动态、画质、语言 | Set、ResetProgress |
| FatalError | AppErrorCode | 本地化说明、重试/退出 | RetryBootstrap |

### 11.4 Gameplay HUD 布局规则

- 顶部：暂停、清洁度；均位于 Safe Area。
- 底部：允许工具水平栏；最多五个基础工具，UV 使用侧边检查入口，不挤压五工具命中区。
- 左右侧：提示与 UV，根据 LeftHanded 镜像。
- 中部：清洁表面，不放常驻文本；临时提示避开作用点并自动淡出。
- 当前工具必须同时有选中轮廓/形状、图标和可访问名称；不能只变色。
- UI Canvas Scaler：`Scale With Screen Size`，参考 1080×1920，Match 0.5；最终热区使用物理/逻辑尺寸走查。

### 11.5 Safe Area

`SafeAreaFitter` 监听 `Screen.safeArea` 与分辨率变化，将像素 rect 转 Canvas anchor；只在值变化时更新，禁止每帧重布局。Gameplay Surface 可视觉延伸至全屏，但重要目标生成区域使用 `PlayableSafeRect`：

```text
PlayableSafeRect = visualSurfaceRect
  minus top HUD exclusion
  minus bottom tool bar exclusion
  minus system gesture critical inset
```

边缘污渍遮罩允许位于视觉边缘，但 Objective 计算中的关键质量至少 95% 必须能从 PlayableSafeRect 内通过笔刷扩边清除。

### 11.6 左右手模式

- 只镜像侧边操作入口和需要拇指触达的菜单布局；工具顺序保持一致，避免肌肉记忆混乱。
- 设置变化即时刷新 Gameplay HUD，不重建 Session。
- Surface 坐标和关卡遮罩不镜像；统计与评分不受影响。

### 11.7 设置模型

```csharp
public sealed record SettingsData(
    float MusicVolume01,
    float SfxVolume01,
    bool HapticsEnabled,
    bool LeftHanded,
    bool ReducedMotion,
    QualityTier Quality,
    string LocaleCode,
    bool PrivacyNoticeAccepted,
    bool TutorialCompleted);
```

- 音量滑杆即时预览，写盘 250ms 去抖；离开页面强制 Flush。
- 不支持触觉的设备禁用开关并显示说明，存档值仍可保留。
- Quality 改变涉及 RT 重建时：前端即时保存；关卡中提示“下一关生效”，不在活动笔画中销毁纹理。
- ReducedMotion 即时切换反馈策略。
- 重置进度显示将删除的项目；默认保留 Locale，可由产品决策改变。

### 11.8 本地化

Locale：`zh-Hans`、`en`。表建议拆分：

```text
UI_Common
UI_Home
UI_Gameplay
UI_Results
UI_Settings
Levels
Tutorial
Errors
Privacy
```

- Key 命名如 `ui.gameplay.cleanliness`、`level.006.name`、`error.save.write_failed`。
- 动态值使用 Smart String 参数，不在 C# 拼接语序。
- 数字、百分比、时间使用当前 Locale 格式，但得分内部不变。
- CI 检查所有 key 在两语言存在；运行时缺失回退英文并限频记录。
- 开发阶段启用 Pseudo Locale，将文本扩长 30% 检查截断。

### 11.9 降低动态效果

| 默认效果 | ReducedMotion 替代 |
|---|---|
| 完成闪光+轻微镜头运动 | 150ms 静态高光淡入 |
| 前后对比自动扫过 | 静态切换按钮/短淡入 |
| 大量粒子 | 减少 70%，保留材质变化 |
| UV/提示移动光晕 | 固定区域轮廓 |
| 工具弹性缩放 | 简单透明度/选中框 |

核心污渍变化、工具范围、错误原因不能因 ReducedMotion 被移除。

---

## 12. 存档、成长与奖励

### 12.1 存档路径和文件

```text
Application.persistentDataPath/SwipeClean/Save/
├── save_v1.json          # 当前主档 Envelope
├── save_v1.json.bak      # 上一次验证有效的主档
└── save_v1.json.tmp      # 写入中的临时文件；成功后替换主档
```

启动时创建专用目录；不得对 `persistentDataPath` 根目录做删除或递归移动。临时文件只操作上述已解析绝对路径，并验证目标都位于 Save 目录内。

### 12.2 Envelope 与 Payload

```json
{
  "formatVersion": 1,
  "payloadSha256": "hex-lowercase-sha256",
  "writtenAtUtc": "2026-09-08T12:34:56Z",
  "payload": {
    "schemaVersion": 1,
    "installId": "anonymous-guid-n",
    "player": {
      "coins": 350,
      "highestUnlockedLevel": 5
    },
    "levels": {
      "level_001_fingerprint": {
        "bestStars": 3,
        "bestScore": 96.5,
        "bestCleanliness": 0.997,
        "bestDurationSeconds": 24.2,
        "firstRewardClaimed": true,
        "threeStarRewardClaimed": true,
        "collectibleClaimed": true,
        "attemptCount": 2
      }
    },
    "tools": {
      "tool_cloth_basic": { "unlocked": true, "level": 2 }
    },
    "collectibles": ["collectible_photo_001"],
    "settings": {
      "musicVolume01": 0.7,
      "sfxVolume01": 1.0,
      "hapticsEnabled": true,
      "leftHanded": false,
      "reducedMotion": false,
      "quality": "Medium",
      "localeCode": "zh-Hans",
      "privacyNoticeAccepted": true,
      "tutorialCompleted": false
    },
    "recentCommittedSessionIds": ["session-guid-n"]
  }
}
```

`payloadSha256` 对规范化后的 `payload` UTF-8 JSON 字节计算，仅用于损坏检测。序列化器必须固定字段命名和文化格式；Hash 校验前不能重新格式化来自磁盘的 Payload，因此实现上建议 Envelope 将 Payload 存为 Base64/原始 JSON 字符串，或先以固定序列化选项序列化 Payload 再构造 Envelope。实现 ADR 必须固定一种方式并提供 Golden Test。

### 12.3 领域模型

```csharp
public sealed class SaveData
{
    public int SchemaVersion { get; set; }
    public string InstallId { get; set; }
    public PlayerSave Player { get; set; }
    public Dictionary<string, LevelProgressSave> Levels { get; set; }
    public Dictionary<string, ToolProgressSave> Tools { get; set; }
    public HashSet<string> Collectibles { get; set; }
    public SettingsData Settings { get; set; }
    public Queue<string> RecentCommittedSessionIds { get; set; }
}
```

加载后执行语义验证：金币不小于 0、解锁关卡范围有效、星数 0..3、分数 0..100、工具等级在 Catalog 范围、Locale 支持或回退、集合无 null。非法单字段尽量修复并记录；结构/Hash 失败进入备份恢复。

### 12.4 SaveService 接口

```csharp
public interface ISaveService
{
    SaveLoadResult LoadOrCreate();
    SaveWriteResult Commit(Func<SaveData, SaveMutationResult> mutation, string reason);
    SaveWriteResult Flush();
    SaveWriteResult ResetProgress(ResetOptions options);
    PlayerSnapshot GetSnapshot();
}
```

- 所有 mutation 在主线程串行执行，内部对数据深拷贝或 copy-on-write。
- mutation 成功、文件写入失败时不能替换内存正式快照；返回失败并允许原命令重试。
- View 永远只拿不可变 `PlayerSnapshot`，不能持有 SaveData 引用。
- 写入期间第二个 Commit 排队或返回 Busy，不并发写同一文件。

### 12.5 加载算法

```text
if main exists and envelope/hash/schema valid:
    load main → migrate → semantic validate → ready
else if backup exists and valid:
    load backup → migrate → semantic validate → write repaired main → ready
else:
    create defaults → atomic write → ready with RecoveryCreatedDefault status
```

若主档声明高于客户端支持的 `schemaVersion`，返回 `SaveFutureVersion`：不覆盖主/备份；提示玩家更新应用或在明确确认后重置。不能把未来版本当损坏档自动清空。

### 12.6 原子写入算法

1. 校验目标路径位于专用 Save 目录。
2. 对 mutation 后副本执行语义验证。
3. 固定选项序列化 Payload，计算 SHA-256，构造 Envelope。
4. 写入 `.tmp`，Flush 并关闭流。
5. 重新读取 `.tmp`，验证 Envelope、Hash 和语义。
6. 若主档有效，把主档复制/替换为 `.bak`；失败则保留主档并中止。
7. 以同卷文件移动/替换 `.tmp` 为主档。
8. 再次快速校验主档；成功后发布新 PlayerSnapshot。
9. 清理遗留 `.tmp`；清理失败仅警告，不影响有效主档。

平台文件 API 的原子保证必须通过故障注入验证；不能假设 `File.Replace` 在所有目标平台完全一致。任何时候至少保留一份已验证的主或备份。

### 12.7 迁移

```csharp
public interface ISaveMigration
{
    int FromVersion { get; }
    int ToVersion { get; }
    SaveData Migrate(SaveData source);
}
```

- 迁移只允许 `n → n+1`，注册表必须连续。
- 对加载数据副本执行，迁移失败不修改磁盘主/备份。
- 每个迁移有 Golden fixture、字段保留和重复执行测试。
- 迁移完成后按新版本原子写主档，同时保留旧主档为备份直到新档验证成功。

### 12.8 结算事务与幂等

```csharp
public sealed record ProgressionCommitResult(
    bool Success,
    bool WasAlreadyCommitted,
    int CoinsGranted,
    bool UnlockedNextLevel,
    bool GrantedCollectible,
    PlayerSnapshot Snapshot,
    AppError? Error);
```

`CommitLevelResult` 单事务执行：

1. 如果 SessionId 在最近提交集合中，返回 `WasAlreadyCommitted=true`，不重复修改。
2. AttemptCount +1。
3. 分别比较并更新 bestStars/Score/Cleanliness/Duration。
4. 若首次通关未领，发 FirstClearCoins 并置位。
5. 若首次三星未领且本次三星，发 ThreeStarCoins 并置位。
6. 若收藏条件满足且未拥有，加入集合并置位。
7. 若本关是最高已解锁且非第 12 关，解锁下一关。
8. 将 SessionId 加入集合，最多保留最近 32 个；字段级领取标记提供长期幂等。
9. 原子写盘成功后才发布 `ProgressionChanged`。

若写盘失败，结果页保留 LevelResult 和重试按钮；同一 SessionId 重试得到一致奖励。

### 12.9 工具升级事务

验证顺序：工具存在→已解锁→非满级→下一等级定义存在→Coins 足够。副本中先扣币再升级并校验不变量，一次写入。成功后发布不可变 Snapshot；失败不播放成功反馈。

### 12.10 重置

`ResetOptions` 默认保留 Locale，其余玩家、关卡、工具、收藏、教程、匿名 InstallId 按产品决定重建；隐私同意标记是否保留由合规决策。重置必须：二次确认→停止所有进行中的写入→构造默认档→原子写入→重载 Frontend。不得直接递归删除 Save 目录。

---

## 13. 反馈系统

### 13.1 反馈请求

玩法不直接引用 AudioSource、粒子 Prefab 或平台震动：

```csharp
public interface IFeedbackSink
{
    void BeginContinuous(in ContinuousFeedbackRequest request);
    void UpdateContinuous(in ContinuousFeedbackUpdate update);
    void EndContinuous(FeedbackHandle handle, float fadeSeconds = 0.05f);
    void PlayOneShot(in OneShotFeedbackRequest request);
}
```

Request 使用 Tool/Stain/Surface 标签、速度、强度、位置和语义；FeedbackResolver 查找 Profile 并分发到 Audio/Haptic/Particle，不反向修改玩法状态。

### 13.2 AudioMixer

```text
Master
├── Music
├── SFX
│   ├── ContinuousFriction
│   ├── ToolEvents
│   ├── UI
│   └── Completion
└── Debug（Development only）
```

公开参数：`MusicVolumeDb`、`SfxVolumeDb`。0..1 转 dB 使用感知曲线，0 映射到静音（例如 -80dB），不能线性当 dB。

### 13.3 音频 Profile

```csharp
public sealed record AudioFeedbackProfile(
    string Id,
    IReadOnlyList<AudioClip> Clips,
    Vector2 VolumeRange,
    Vector2 PitchRange,
    AnimationCurve SpeedToVolume,
    AnimationCurve SpeedToPitch,
    AnimationCurve SpeedToLowPass,
    int MaxSimultaneous,
    float MinRetriggerSeconds,
    bool IsLoop);
```

- 至少四组基础摩擦 Profile 可盲听区分。
- 连续笔画每个活动工具只持有一个循环 Handle；速度参数平滑 50～100ms。
- Clip 切换使用短交叉淡化，不每个触点重启音源。
- OneShot 使用预热对象池；同类事件达到并发上限时丢弃最低优先级，不新增 GameObject。
- 暂停时玩法组 Snapshot 静音/暂停，UI 声音可保留；恢复不叠加重复循环。

### 13.4 触觉

```csharp
public interface IHapticService
{
    bool IsSupported { get; }
    bool IsEnabled { get; set; }
    void Play(HapticSemantic semantic, float intensity01 = 1f);
    void StopContinuous();
}
```

- iOS 使用原生语义反馈桥；Android 按 `VibrationEffect`/设备能力映射，能力不足时降级为短振动或静默。
- 连续轻灰不是逐 Stamp 震动，使用节流脉冲：默认最小间隔 80～120ms。
- 颗粒反馈可在 60～150ms 随机化间隔但使用确定种子，避免高频疲劳。
- WrongTool 为短促双击；LevelComplete 为分段增强后清晰收尾，总长 ≤1.5 秒。
- 关闭触觉后立即停止，所有调用成为 no-op；不支持设备不弹错误。

### 13.5 粒子与材质反馈

- 每类污渍使用 FeedbackProfile 指定粒子，统一对象池。
- 生成率按速度×有效效率×Quality 倍率；错误操作使用独立低强度反馈。
- 粒子不参与清洁度和碰撞，不作为规则真值。
- 粒子排序层位于表面之上、工具 UI 之下；不能遮挡残留提示。
- 质量档降低粒子数量而非改变颜色/工具状态语义。

### 13.6 错误工具提示

`WrongToolFeedbackCoordinator` 按 `(toolCategory, stainCategory, consequence)` 去抖：

1. 立即播放温和材质/声音差异；
2. 首次或冷却结束后播放双击触觉；
3. 2 秒内显示 `HintKey` 对应的短提示；
4. 本地指标记录聚合错误；
5. 不暂停游戏、不弹阻断 Modal。

同类提示冷却 2 秒，全局提示最小间隔 0.75 秒；用户已经执行正确步骤后清除旧提示。

### 13.7 完成演出

时序预算：

```text
0ms      锁定 Gameplay 输入、停止连续工具反馈
0-150ms  清除残留提示、污渍最终状态稳定
100ms    中等脱落/收尾触觉
150-900ms 抛光/高光、完成音、逐渐增强触觉
900-1200ms 清晰收尾、建立前后对比画面
≤1500ms  进入 Results，可交互
```

完成演出有唯一 Token，重复 ObjectiveMet 不重播。ReducedMotion 使用 ≤300ms 静态高光淡入并直接进入 Results。

---

## 14. 分析、日志与调试

### 14.1 Analytics 接口

```csharp
public interface IAnalytics
{
    void Track<T>(in T analyticsEvent) where T : struct, IAnalyticsEvent;
    void Flush();
}
```

MVP 实现 `LocalAnalyticsSink`，不联网。事件序列化为 NDJSON，单行一个事件；Gameplay 事件进入内存队列，后台线程只处理纯字节/文件，不接触 UnityEngine 对象。

### 14.2 公共字段

每个事件包含：

| 字段 | 说明 |
|---|---|
| `eventName` | 固定 snake_case |
| `eventVersion` | 事件 schema 版本 |
| `occurredAtUtc` | ISO-8601 UTC |
| `installId` | 随机匿名 GUID，不取设备标识 |
| `sessionId` | 应用会话 ID |
| `levelSessionId` | 可空；关卡会话 ID |
| `appVersion/buildNumber` | 缺陷定位 |
| `catalogVersion` | 内容调参版本 |
| `platform/locale` | 枚举/Locale code |

### 14.3 事件 Schema

| 事件 | 必需参数 |
|---|---|
| `game_started` | platform、locale、quality、isFirstRun |
| `tutorial_step` | stepId、completed、durationSeconds |
| `level_started` | levelId、contentVersion、mode、attempt |
| `tool_selected` | levelId、toolId、stainContext（可空） |
| `wrong_tool_used` | levelId、toolId、stainId、consequence、progressBucket |
| `hint_used` | levelId、hintType、progressBucket |
| `level_completed` | score、stars、duration、cleanliness、mistakes、hints、consumableUsed |
| `level_abandoned` | duration、progressBucket、lastTool |

`cleaning_stroke` 不逐笔落盘；会话结束只写一份 `stroke_summary`，包含各工具总时长、总距离、有效比例，不含坐标或完整轨迹。

### 14.4 文件策略

```text
Application.persistentDataPath/SwipeClean/Logs/
├── analytics_0.ndjson
├── analytics_1.ndjson
└── diagnostics.log
```

- Analytics 每文件 1MB，最多 3 个滚动文件；Diagnostics 最多 2MB×2。
- 不记录存档 Payload、精确触摸坐标、系统其他应用、设备唯一硬件 ID。
- 玩家/测试者必须在 Development 菜单明确点击“导出诊断”才生成可分享包。
- Release 是否保留本地 Analytics 由 M4 隐私审查决定；关闭时使用 NoOp sink。

### 14.5 Development Overlay

仅 Development Build，四指手势不作为唯一入口，编辑器可用快捷键：

- App/Level State、SessionId 后 6 位；
- FPS、frame time、GC alloc、内存估算；
- 输入样本/Stamp/Shader pass 数、队列积压；
- 每层 R/G/B/A 调试预览、剩余质量、Revision；
- 当前工具、速度、压力、饱和、效果规则 ID；
- Coverage 请求频率、延迟、失败数；
- 跳关、重置本关、模拟存档失败、切换质量档；
- 构建/内容/提交版本。

Release 构建通过编译符号完全排除跳关和故障注入代码，不只隐藏按钮。

---

## 15. 隐私、权限与第三方治理

### 15.1 权限基线

MVP 不申请相册、相机、麦克风、通讯录、日历、蓝牙、精确位置、广告追踪、屏幕录制或无障碍服务权限。构建后检查：Android Manifest、合并 Manifest 报告、iOS `Info.plist`、Entitlements 与 Privacy Manifest。

触觉调用使用系统基础能力，不得为触觉额外引入收集设备标识的 SDK。

### 15.2 离线保证

- 冷启动、首次说明、选关、12 关、结算、工具升级、收藏、设置和存档全部不等待网络。
- 代码中不直接创建 HTTP 客户端；若未来新增联网，必须独立需求和隐私评审。
- 任何包自动遥测必须关闭或记录实际行为；不能仅依据“没有主动调用”判定无联网。

### 15.3 资产和依赖台账

`Documentation/Compliance/` 维护：

```text
THIRD_PARTY_NOTICES.md
dependency_inventory.csv
asset_provenance.csv
permission_inventory.md
privacy_data_map.md
trademark_review.md
```

`asset_provenance.csv` 至少包含 Asset GUID、文件路径、作者/供应商、来源 URL/合同、许可证、是否修改、审核人、审核日期。没有明确许可的 GitHub/竞品资源不得进入项目。

### 15.4 商店表述

- 明确为清洁模拟游戏，不暗示能扫描或清洁真实硬件屏幕。
- 不使用真实品牌名、Logo、精确设备轮廓或受保护角色。
- 商店截图只展示已实现功能；MVP 无广告时不写“看广告领奖”。
- 若后续接正式分析/崩溃 SDK，先更新数据地图、Privacy Manifest、App Store Privacy 与 Google Play Data Safety。

---

## 16. 性能、内存与降级设计

### 16.1 帧预算

| 子系统 | 60 FPS CPU 主线程 P95 | 30 FPS CPU 主线程 P95 | GPU 60 FPS 预算 |
|---|---:|---:|---:|
| Input+投影+插值 | 1.0ms | 1.5ms | — |
| Tool/污渍规则 | 1.0ms | 2.0ms | — |
| 清洁命令调度 | 1.5ms | 3.0ms | 4.5ms（Update+Composite） |
| Coverage 调度/回调 | 0.5ms | 1.0ms | 1.0ms 均摊 |
| UI/动画 | 1.5ms | 3.0ms | 1.0ms |
| 音频/粒子/触觉 | 1.0ms | 2.0ms | 1.0ms |
| 其他与 Unity 开销 | 3.5ms | 8.0ms | 3.5ms |
| 余量 | 1.0ms+ | 4.0ms+ | 5.0ms+ |

目标是主线程 P95 ≤10ms、GPU P95 ≤12ms 以给平台呈现与波动留余量；最低设备允许切 Low/30 FPS，但不得低于 30 FPS 底线。平均值不能替代 P95、1% low 和最长帧记录。

### 16.2 GC 与分配

- Playing 稳态目标 `0 B/frame`；Input、Stamp、反馈 Request 使用 struct 和预分配数组/ring buffer。
- 禁止每帧 LINQ、`string.Format`、装箱枚举、临时 List、动态 Material 和新建粒子对象。
- 清洁度 UI 只在值变化或 4～8Hz 快照时格式化文本。
- 场景/页面切换允许受控分配；单次普通页面打开建议 <100KB managed alloc，必须在 Profiler 记录。
- 对象池仅用于高频/短生命周期对象；常驻页面和单例服务不套池。

### 16.3 内存预算

| 类别 | 峰值预算 | 控制方法 |
|---|---:|---|
| Unity 引擎/代码/托管堆 | 120MB | IL2CPP、避免大型托管缓存 |
| 常驻 UI/字体/基础资产 | 55MB | 字体 atlas、Sprite Atlas、按语言控制 |
| 当前关卡背景/污渍/工具资产 | 120MB | 只预载当前关，纹理平台压缩 |
| 状态 RT/归约 RT/临时 RT | 70MB | Quality 档、显式生命周期、临时池 |
| 音频 | 55MB | 长音乐流式、短 SFX 压缩/解压策略 |
| 粒子/动画/运行时对象 | 35MB | 池化、数量上限 |
| 系统波动与安全余量 | 45MB | 总峰值目标 <500MB |

纹理导入按视觉验证选择 ASTC/平台兼容压缩；遮罩源可使用灰度但 Unity 导入/运行时实际格式必须检查。音频循环优先 Vorbis/平台适配压缩，短且高频 SFX 可 Decompress On Load，具体以 CPU/内存平衡报告决定。

### 16.4 加载预算

普通关卡从点击到可操作目标 <2 秒：

| 阶段 | 预算 |
|---|---:|
| 状态/权限/配置检查 | 50ms |
| Gameplay Scene load | 500ms |
| 当前关卡资产解析/实例化 | 500ms |
| RT 初始化与初始归约 | 500ms |
| 音频/粒子预热与首帧 | 300ms |
| 余量 | 150ms |

加载过程必须每帧让出主线程并更新进度；不得在一帧同步实例化全部粒子/材质。若首次 Shader 编译造成超时，构建加入 Shader Variant Collection 并在 Boot/Loading 受控预热实际使用变体，不保留全项目无用变体。

### 16.5 自动降级

默认 Quality 由设备能力启发式选择，再由真机白/灰名单调整；不依据设备品牌字符串决定核心逻辑。运行时连续 10 秒 P95 帧时超过目标 20% 时：

1. 降低粒子倍率；
2. 降低液体表现更新频率；
3. Coverage 从 8→6→4Hz；
4. 下一关将 Quality 降一档并保存建议；
5. 不在当前活动关卡中强制重建状态 RT，除非发生内存警告。

发生低内存通知时先释放非当前关缓存、对比临时图和未播放音频；若仍不足，安全暂停并提示返回，而不是破坏当前状态纹理。

### 16.6 性能测量场景

`TestHarness` 提供确定性场景：

- P1：单层、连续最大速度干布 30 秒；
- P2：六层、多工具命令、Medium 60 秒；
- P3：第 12 关最重配置、High/Low 各 15 分钟；
- P4：12 关加载/退出循环两轮；
- P5：粒子、音频、触觉事件最大合理密度；
- P6：Coverage 异步回读成功/失败/不支持三路径。

报告必须记录设备、OS、构建提交、Unity 补丁、Graphics API、Quality、分辨率、温度状态、平均/P95/1% low、最大内存和 GC。

---

## 17. 测试详细设计

### 17.1 测试金字塔

```text
少量真机端到端与用户测试
        ↑
PlayMode/场景/渲染集成测试
        ↑
大量纯 C# EditMode 单元与配置测试
```

逻辑尽量下沉到 Domain/Application，以无 Scene 的 EditMode 快速覆盖；Shader、输入回调、生命周期和原生反馈由 PlayMode/真机补足。

### 17.2 EditMode 单元测试

| 测试 ID | 被测对象 | 用例 |
|---|---|---|
| UT-DATA-001 | Content ID Validator | 合法、大小写、空格、重复、长度边界 |
| UT-DATA-002 | Catalog Validator | 缺资源、缺矩阵、非法阈值、不可完成关卡、UV 缺失 |
| UT-INPUT-001 | StrokeInterpolator | 零距离、0.4R 边界、超长、最大细分、路径顺序 |
| UT-INPUT-002 | StrokeKinematics | 时间相同/倒退、速度平滑、30/60/120fps 固定轨迹 |
| UT-TOOL-001 | ToolEffectResolver | 30 个矩阵组合、湿度/速度/压力/饱和/升级修正 |
| UT-TOOL-002 | Tool State | 饱和上限、更换恢复、喷雾用量、暂停不增长 |
| UT-STAIN-001 | Softening | 阈值、暂停、软化/回干、1～3 秒边界 |
| UT-STAIN-002 | Side Effects | 去抖、上限、水痕不递归、沙粒阻挡 |
| UT-CLEAN-001 | Cleanliness Formula | 多层权重、非目标层、零初始质量、99.5% 容差 |
| UT-LEVEL-001 | ObjectiveEvaluator | Inspect、阈值、防抖、hysteresis、只完成一次 |
| UT-SCORE-001 | ScoreCalculator | 五维权重、无隐藏层、74.99/75/89.99/90 |
| UT-SAVE-001 | Serializer | 固定 JSON、SHA-256、往返、文化区域不影响数字 |
| UT-SAVE-002 | Migration | v0→v1、重复、缺链、未来版本 |
| UT-SAVE-003 | Progression | 首通/三星/收藏/下一关/最终关/较差成绩 |
| UT-SAVE-004 | Idempotency | 同 Session 1/2/N 次、集合滚动、字段级长期幂等 |
| UT-SAVE-005 | Reset | 保留 Locale、恢复默认、事务失败 |
| UT-AN-001 | Analytics Schema | 公共字段、无坐标/PII、事件版本、文件滚动 |
| UT-ARCH-001 | AppStateMachine | 所有合法/非法/Busy 转换 |

### 17.3 Shader/渲染测试

PlayMode 创建小尺寸（例如 64×64）RT，执行确定 Stamp 并异步/同步读取结果，与容差基准比较：

| 测试 ID | Pass | 断言 |
|---|---|---|
| RT-001 | Initialize | R/G 初值与 mask/thickness/moisture 一致 |
| RT-002 | Clean | 中心减少、边缘平滑、范围外不变、无负值 |
| RT-003 | AddMoisture | G 增加并 saturate，R 不变 |
| RT-004 | Absorb | 水类 R/G 按规则下降，其他通道合法 |
| RT-005 | Smear | 方向正确、近似质量守恒、B 增加、无 NaN |
| RT-006 | Push | 位移方向、边界策略、零方向安全 |
| RT-007 | WaterMark | A/副作用层有上限且不递归 |
| RT-008 | Batch Ordering | 同序列分 batch 与单 batch 在容差内一致 |
| RT-009 | Coverage Reduction | 1/多层 MeanMass 与 CPU 基准误差 ≤1 个百分点 |
| RT-010 | Composite UV | 隐藏层只在 UV mask 内显示但一直参与目标 |
| RT-011 | Quality | 512/768/1024 规则结果与基准在容差内 |

GPU 像素测试不能要求跨所有 GPU bit-exact；使用业务容差，并在至少一种 iOS Metal、Android Vulkan、Android GLES3 真机重复关键用例。

### 17.4 PlayMode 集成测试

| 测试 ID | 场景 | 断言 |
|---|---|---|
| IT-001 | Boot 正常 | Catalog/Save/Locale 初始化后进正确页面 |
| IT-002 | Boot 主档损坏 | 恢复备份并显示非阻塞恢复状态 |
| IT-003 | 第一关 | Intro→擦拭→完成→结算→解锁 2 |
| IT-004 | Pause/Resume | 输入、时间、软化、音频按规则暂停/恢复 |
| IT-005 | Restart ×3 | 纹理/订阅/音频不累积，不重复奖励 |
| IT-006 | Multi-touch/UI | 第二触点和 UI 开始触点不污染表面 |
| IT-007 | Tool Switch | 100ms 目标内逻辑/指针/反馈一致，旧笔画结束 |
| IT-008 | Wrong→Recover | 泥、沙、水三种错误路径都能恢复完成 |
| IT-009 | Hidden Stain | UV 发现、切回工具、清除、评分/事件正确 |
| IT-010 | Result Retry | 模拟写盘失败后重试不重复奖励 |
| IT-011 | Localization | zh-Hans/en 即时切换，关键页面无截断 |
| IT-012 | Reduced Motion | 替代演出启用，核心状态仍可见 |
| IT-013 | Offline | 网络不可用时完整流程无等待/错误 |
| IT-014 | Final Level | 第 12 关无无效下一关，进度封顶 |

### 17.5 存档故障注入

测试文件系统 `FaultInjectingFileSystem` 可在以下步骤抛出异常：创建目录、写 tmp 中段、Flush、读取 tmp、备份主档、替换主档、最终验证。每个故障点断言：

- 内存 Snapshot 未错误推进；
- 至少一份旧有效档可加载；
- 重试可完成；
- 奖励不重复；
- 不删除 Save 目录外文件。

真机补测存储不足、杀进程时机和系统文件保护；测试数据位于专用测试包标识，不能操作正式包数据。

### 17.6 关卡配置测试

每关自动执行“可达性探针”：用推荐顺序和一级工具回放预制路径，验证理论上可超过阈值。该探针不能替代真人手感/平衡，但能拦截错误矩阵、不可见目标、阻挡层和阈值配置。

每关还必须验证：两种模式、三质量档、中英文、左右手、触觉关、ReducedMotion、三次重开/结算、一次错误恢复路径。

### 17.7 真机测试矩阵

最低配置在 M2 冻结，矩阵至少包含：

| 平台槽位 | 设备特征 | Graphics API | 数量 |
|---|---|---|---:|
| iOS-Low | 最低支持系统附近、较小内存/较旧 GPU | Metal | 1 |
| iOS-Mid | 主流比例/刘海 | Metal | 1 |
| iOS-High | 高刷/动态岛 | Metal | 1 |
| Android-Low | 最低支持、弱 GPU/内存 | GLES3 或 Vulkan | 1 |
| Android-Mid | 主流中端、圆角/手势导航 | Vulkan | 1 |
| Android-High | 高刷、高分辨率 | Vulkan | 1 |

如果同一 Android 设备支持 Vulkan/GLES3，至少在一台上分别验证。平板只做兼容观察，不作为 MVP 首要构图承诺，除非产品将其列入支持范围。

### 17.8 用户测试实现门禁

- M1 构建固定第 1/2 关、干布/气吹、日志和问卷版本；不得测试过程中改参数却混合统计。
- 每轮内容版本不同，分析必须带 CatalogVersion。
- 观察者不主动告诉首次操作；只有预设超时后按脚本提示。
- “舒服”正向反馈、首次有效动作时间、错误纠正次数和不适原因同时记录。
- M1 未达到 7/10 前不启动 12 关批量资产。

---

## 18. CI、构建与发布设计

### 18.1 Pipeline 阶段

```text
Checkout
 → Verify Unity/Packages lock
 → License/secret setup
 → Content validation + Catalog bake check
 → Compile
 → EditMode tests
 → PlayMode smoke tests
 → Build Android/iOS target
 → Artifact validation
 → Publish test reports/symbols/build
```

PR 必跑：版本锁、编译、内容校验、EditMode。主分支必跑：PlayMode 和 Android Development。iOS 构建在 macOS Runner 执行；夜间/候选版本运行双平台与性能冒烟。

### 18.2 命令约定

以下是入口形式，实际 Unity 可执行路径由 CI Runner 配置，不写死个人路径：

```powershell
Unity.exe -batchmode -nographics -quit `
  -projectPath <workspace> `
  -runTests -testPlatform EditMode `
  -testResults <artifacts>/editmode.xml

Unity.exe -batchmode -quit `
  -projectPath <workspace> `
  -executeMethod SwipeClean.Editor.Build.BuildCommand.Execute `
  -buildTarget Android -buildFlavor Development `
  -outputPath <artifacts>/SwipeClean.apk
```

CI 脚本不把签名密钥、密码、Provisioning Profile 内容写入日志或仓库。Release 构建缺少签名必须失败，不能回退为测试签名。

### 18.3 构建前检查

- Unity 完整版本等于 `ProjectVersion.txt`；packages-lock 无未提交变化；
- 所有 12 关与 Catalog 校验通过；
- 中英文 key 和伪本地化扫描通过；
- Shader/材质/场景引用无 Missing；
- Build Scenes 顺序正确：Boot、Frontend、Gameplay；TestHarness 不进 Release；
- Development-only symbol、故障注入、跳关 UI 不进 Release；
- 权限/插件清单符合基线；
- 版本号、构建号、Git commit、CatalogVersion 可追踪；
- 第三方与资产来源台账没有未审查项。

### 18.4 构建产物

| 产物 | 保留内容 |
|---|---|
| Android | APK/AAB、mapping/symbols、merged manifest、构建日志、测试报告 |
| iOS | Xcode project/archive/IPA（按流程）、dSYM、Info.plist/entitlements/Privacy Manifest、日志 |
| 通用 | BuildInfo.json、Catalog snapshot、依赖清单、许可证、测试汇总 |

`BuildInfo.json` 示例：

```json
{
  "appVersion": "0.1.0",
  "buildNumber": 42,
  "unityVersion": "6000.3.xf1",
  "gitCommit": "full-commit-sha",
  "catalogVersion": "2026.09.08.1",
  "buildFlavor": "Android-Development",
  "builtAtUtc": "2026-09-08T12:00:00Z"
}
```

### 18.5 发布候选门禁

1. `P0/P1-MVP` 对应测试全部通过。
2. 12 关在两平台完成完整回归。
3. iOS/Android 各至少 3 台真机冒烟。
4. 最重关卡性能、15 分钟与 30 分钟长时报告满足目标。
5. 主/备份/迁移/重复结算故障注入通过。
6. 无阻塞/严重缺陷；普通缺陷有负责人和计划。
7. 权限、隐私、许可证、素材来源和商标检查签字。
8. Release Build 无 Development 菜单、无无关 SDK/权限。
9. 构建、签名、版本、符号和回滚流程演练成功。

---

## 19. 文件与类实施清单

以下是建议的最小代码文件集合；允许在不破坏程序集边界的前提下细分，但不得把多个独立职责塞入单一“Manager”。

### 19.1 Core/Application

| 文件 | 核心类型 |
|---|---|
| `Runtime/Core/GameBootstrap.cs` | `GameBootstrap` |
| `Runtime/Core/AppContext.cs` | `AppContext` |
| `Runtime/Core/AppStateMachine.cs` | `IAppStateMachine`、`AppStateMachine` |
| `Runtime/Core/EventBus.cs` | `IEventBus`、`EventBus` |
| `Runtime/Core/GameClock.cs` | `IGameClock`、Unity adapter |
| `Runtime/Core/ApplicationLifecycleBridge.cs` | pause/focus/low-memory bridge |
| `Runtime/Core/AppError.cs` | Code、Error、Result 类型 |
| `Runtime/Core/GameLogger.cs` | `IGameLogger`、Unity logger |

### 19.2 Content/Domain

| 文件 | 核心类型 |
|---|---|
| `Runtime/Content/GameCatalogAsset.cs` | 生成的强引用 Catalog asset |
| `Runtime/Content/RuntimeCatalogLoader.cs` | asset→DTO 映射 |
| `Runtime/Compatibility/IsExternalInit.cs` | C# 9 Record 编译兼容；不含业务逻辑 |
| `Runtime/Domain/StainDefinition.cs` | 污渍 DTO/enums |
| `Runtime/Domain/ToolDefinition.cs` | 工具 DTO/enums/升级 |
| `Runtime/Domain/ToolStainEffectRule.cs` | 效果矩阵 |
| `Runtime/Domain/LevelDefinition.cs` | 关卡、层、奖励 DTO |
| `Editor/ContentValidation/ContentValidator.cs` | 综合验证入口 |
| `Editor/ContentValidation/CatalogBaker.cs` | Catalog 与 JSON snapshot 生成 |
| `Editor/LevelTools/LevelAuthoringWindow.cs` | 预览、校验、快速运行 |

### 19.3 Input/Cleaning

| 文件 | 核心类型 |
|---|---|
| `Runtime/Input/PrimaryPointerTracker.cs` | 第一触点生命周期 |
| `Runtime/Input/SurfaceProjector.cs` | 屏幕→UV |
| `Runtime/Input/StrokeInterpolator.cs` | 固定距离采样 |
| `Runtime/Input/StrokeKinematics.cs` | 速度/压力 |
| `Runtime/Input/GameplayInputDriver.cs` | Action callback 与管线调度 |
| `Runtime/Cleaning/CleaningSurfaceRuntime.cs` | 表面聚合与释放 |
| `Runtime/Cleaning/StainLayerRuntime.cs` | 层资源/语义状态 |
| `Runtime/Cleaning/BrushCommand.cs` | 命令 struct |
| `Runtime/Cleaning/BrushCommandQueue.cs` | 预分配队列 |
| `Runtime/Cleaning/StainUpdateScheduler.cs` | 稳定分组/batch/pass |
| `Runtime/Cleaning/CoverageCalculator.cs` | 归约与回读 |
| `Runtime/Cleaning/SurfaceCompositeRenderer.cs` | MPB 与合成 |
| `Runtime/Cleaning/UvInspectionController.cs` | UV 光区与发现 |
| `Content/Shaders/StainInitialize.shader` | 初始化 |
| `Content/Shaders/StainUpdate.shader` | Clean/Wet/Absorb/Smear/Push |
| `Content/Shaders/CoverageReduce.shader` | 平均归约 |
| `Content/Shaders/SurfaceComposite.shader` | 最终表现 |

### 19.4 Tools/Levels

| 文件 | 核心类型 |
|---|---|
| `Runtime/Tools/ToolController.cs` | 选择/切换/会话状态 |
| `Runtime/Tools/ToolEffectResolver.cs` | 纯规则解析 |
| `Runtime/Tools/ClothToolStrategy.cs` | 布 |
| `Runtime/Tools/SprayToolStrategy.cs` | 喷雾 |
| `Runtime/Tools/AbsorbentPaperToolStrategy.cs` | 吸水纸 |
| `Runtime/Tools/SoftBrushToolStrategy.cs` | 软刷 |
| `Runtime/Tools/AirBlowerToolStrategy.cs` | 气吹 |
| `Runtime/Tools/UvInspectorStrategy.cs` | UV 检查 |
| `Runtime/Levels/LevelSession.cs` | 会话生命周期 |
| `Runtime/Levels/LevelSessionFactory.cs` | 显式组装关卡依赖 |
| `Runtime/Levels/ObjectiveEvaluator.cs` | 阈值/Inspect/Perfect |
| `Runtime/Levels/LevelMetrics.cs` | 聚合指标 |
| `Runtime/Levels/ScoreCalculator.cs` | 评分 |
| `Runtime/Levels/TutorialController.cs` | 数据步骤状态机 |
| `Runtime/Levels/LevelResult.cs` | 不可变结算数据 |

### 19.5 Progression/UI/Feedback/Analytics

| 文件 | 核心类型 |
|---|---|
| `Runtime/Progression/SaveService.cs` | Load/Commit/Flush/Reset |
| `Runtime/Progression/SaveSerializer.cs` | 固定 JSON/Envelope/Hash |
| `Runtime/Progression/SaveValidator.cs` | 结构与语义校验 |
| `Runtime/Progression/SaveMigrationRegistry.cs` | 连续迁移 |
| `Runtime/Progression/ProgressionService.cs` | 结算、升级、解锁事务 |
| `Runtime/Progression/AtomicFileWriter.cs` | 主/备/tmp 文件操作 |
| `Runtime/UI/NavigationService.cs` | Screen/Modal 栈 |
| `Runtime/UI/SafeAreaFitter.cs` | Safe Area 更新 |
| `Runtime/UI/<Screen>/<Screen>Presenter.cs` | 各页面 Presenter |
| `Runtime/UI/<Screen>/<Screen>View.cs` | 各页面 View |
| `Runtime/Feedback/FeedbackCoordinator.cs` | Profile 解析/分发 |
| `Runtime/Feedback/AudioService.cs` | Mixer/Pool/Loop |
| `Runtime/Feedback/HapticService.cs` | 平台桥与语义节流 |
| `Runtime/Feedback/ParticleFeedbackService.cs` | 粒子池 |
| `Runtime/Analytics/LocalAnalyticsSink.cs` | NDJSON/rollover |
| `Runtime/Analytics/AnalyticsEvents.cs` | 版本化事件 structs |
| `Runtime/Analytics/DevelopmentOverlay.cs` | 编译排除的调试 UI |

---

## 20. 开发顺序与提交门禁

### 20.1 M0：可构建骨架

| 顺序 | 开发项 | 验证 |
|---:|---|---|
| 1 | 锁 Unity/Packages、URP、Build Profiles | 干净环境可打开 |
| 2 | Boot/Frontend/Gameplay Scene 与 AppRoot | 无重复服务 |
| 3 | Assembly Definitions、日志、错误模型 | 依赖方向编译约束 |
| 4 | StateMachine、Lifecycle、Scene Loading | 合法/非法转换测试 |
| 5 | 双平台空包、BuildInfo、CI 骨架 | 各一台真机进首页 |

M0 退出前不要实现广告、IAP、远程内容或复杂 UI。

### 20.2 M1：手感闭环

| 顺序 | 开发项 | 验证 |
|---:|---|---|
| 1 | 单层 RT 初始化/合成 | RT-001 |
| 2 | Input→UV→插值→BrushCommand | UT-INPUT、RT-002 |
| 3 | 干布 EffectResolver 与 Clean pass | UT-TOOL、规则可配置 |
| 4 | Coverage Reduction 与完成防抖 | UT-CLEAN、RT-009 |
| 5 | 第 1 关最小 LevelSession/Tutorial/HUD | IT-003 |
| 6 | 一组动态摩擦音、三档触觉、完成演出 | 真机反馈检查 |
| 7 | 性能/延迟 Overlay 与 M1 测试包 | 10 人中 ≥7 人通过 |

M1 失败时只迭代输入、笔刷、材质、音频、触觉和第 1 关；不得先批量做内容。

### 20.3 M2：垂直切片

| 顺序 | 开发项 | 验证 |
|---:|---|---|
| 1 | 多层/状态纹理/三类污渍/副作用 | RT-003～008 |
| 2 | 喷雾、吸水纸、气吹或软刷，共三工具 | 错误可恢复 |
| 3 | Catalog/Baker/Validator/LevelAuthoring | 不改代码创建关 |
| 4 | Frontend/选关/HUD/暂停/Results | 完整导航 |
| 5 | Save 主备份/迁移/结算幂等 | UT-SAVE、故障注入 |
| 6 | 三关内容、Quality/性能档 | 15 分钟连续玩 |

### 20.4 M3：MVP 内容

先补齐五工具、六污渍、UV、Settings/Localization/Progression，再按 4→8→12 关分批接入。每批都运行 Content Validator、一级工具可达性、三次重开/结算和最重配置性能测试。

### 20.5 M4：软发布

冻结新功能，只处理：设备兼容、性能、存档、崩溃、前三关教学/反馈、隐私/许可、商店和构建流程。任何商业化接入单独立项，不作为“顺手加入”。

### 20.6 每个开发任务的提交内容

- 需求编号与本文 DD 编号；
- 代码/Shader/配置/资产列表；
- 关键设计决策与偏差；
- 新增/更新测试与结果；
- Profiler/内存/包体影响（适用时）；
- 双平台影响；
- 权限/依赖/素材许可变化；
- 已知问题与下一项安全依赖。

---

## 21. 设计到需求追踪

| 需求 Epic | 设计章节 | 主要实现证据 |
|---|---|---|
| E-01 工程基础 | 1～4、18～20 | ProjectVersion、Packages lock、Scenes、Build Profiles、CI |
| E-02 流程与模式 | 3、10、11 | AppStateMachine、LevelSession、Navigation、IT-003/004 |
| E-03 遮罩核心 | 6～7、16～17 | StainUpdate Shader、Coverage、RT-001～011 |
| E-04 污渍规则 | 5、8 | StainDefinition、EffectRule、Softening/SideEffect tests |
| E-05 工具体系 | 5、9 | 六 Strategy、ToolEffectResolver、UT-TOOL |
| E-06 输入 | 6 | InputActions、Interpolator、Kinematics、IT-006 |
| E-07 反馈 | 13 | Audio/Haptic/Particle Profile、真机反馈记录 |
| E-08 关卡内容 | 5、10、20 | LevelDefinition、Baker、Validator、12 关矩阵 |
| E-09 成长存档 | 10、12 | SaveService、Progression transaction、UT-SAVE |
| E-10 UI/本地化 | 11 | Screen MVP、SafeArea、Localization tables |
| E-11 数据合规 | 14～15 | NDJSON、Data map、权限/依赖/资产台账 |
| E-12 质量发布 | 16～18 | 性能报告、测试 XML、真机矩阵、发布检查表 |

---

## 22. 代码评审检查表

### 22.1 通用

- [ ] 依赖方向符合 Assembly 设计，无隐藏 Service Locator/`FindObjectOfType`；
- [ ] 单位、范围、所有权、线程与失败行为清楚；
- [ ] 热路径无新分配、反射、LINQ、字符串构造；
- [ ] Dispose/Scene 卸载释放订阅、RT、CommandBuffer、音频和对象池成员；
- [ ] 内容规则来自 Catalog，不硬编码 Level ID；
- [ ] 错误进入统一 Result/AppError，不只写日志继续运行；
- [ ] 新增用户文本使用本地化键；
- [ ] 没有新增权限、SDK 或来源不明资产。

### 22.2 清洁/Shader

- [ ] 不每 Stamp 一次 Blit，不每帧完整 GPU 回读；
- [ ] 输出 Clamp 且处理零向量/非有限值/大 deltaTime；
- [ ] Batch 顺序稳定，暂停/切工具结束旧笔画；
- [ ] Low/Medium/High 只改变表现/采样，不改变可完成性；
- [ ] 异步回调检查 Session Generation；
- [ ] Render Texture 格式和生命周期在目标平台验证。

### 22.3 存档/奖励

- [ ] 所有写入通过 SaveService 事务；
- [ ] 写前/写后校验，主/备/tmp 至少保留一份有效档；
- [ ] SessionId 与字段标记共同保证幂等；
- [ ] 未来版本不被自动覆盖；
- [ ] 故障注入覆盖新写入步骤；
- [ ] UI 只在写盘成功后展示已领取/升级成功。

---

## 23. 开工前检查表

- [ ] Unity 6.3 LTS 具体补丁已选并锁入仓库；
- [ ] Input 1.20.0、Localization 1.5.13 和 Test Framework resolve 正常；
- [ ] iOS/Android 模块、构建 Runner 和签名责任人明确；
- [ ] 包标识占位与正式替换计划明确；
- [ ] M1 目标设备候选至少各一台；
- [ ] ADR-001～012 已由技术负责人确认；
- [ ] 第 1 关污渍、背景、布、摩擦音和揭晓占位资产可用；
- [ ] EffectRule 初始矩阵与清洁度/评分公式建立表格来源；
- [ ] M1 用户测试对象、脚本和数据记录方式已安排；
- [ ] 仓库忽略规则、LFS/大文件策略、CI 密钥策略已确定；
- [ ] 所有外部参考只研究思路，未复制许可证不清晰内容。

---

## 24. MVP 最终技术验收

- [ ] Boot、Frontend、Gameplay 状态和 Scene 生命周期无非法/重复实例；
- [ ] 输入到视觉反馈达到目标，快速轨迹无明显断点；
- [ ] 30/60/120fps 固定轨迹清除量差异 ≤5%；
- [ ] 多层状态、六污渍、五基础工具和检查型 UV 全部按矩阵工作；
- [ ] 错误扩散、水痕和划痕风险均有上限、可理解、可恢复；
- [ ] Coverage 4～8Hz、异步回读/降级路径有效，不每帧完整回读；
- [ ] 12 关在一级工具、两模式和三质量档下均可完成；
- [ ] 完成事件、首通/三星/收藏奖励和解锁全部幂等；
- [ ] 主档损坏、写入中断、迁移和未来版本路径安全；
- [ ] 中英文、左右手、Safe Area、关闭音效/音乐/触觉、ReducedMotion 全流程通过；
- [ ] Release 无无关权限、网络 SDK、广告、账户和调试入口；
- [ ] iOS/Android 各三台真机通过冒烟；
- [ ] FPS、延迟、加载、内存、长时与稳定性报告满足需求；
- [ ] 许可证、素材来源、隐私数据地图、商店声明和构建文档齐全；
- [ ] 自动化、PlayMode、Shader、存档故障注入和发布候选门禁全部通过。

---

## 25. 结论

本设计把 SwipeClean MVP 固定为一套可测试的数据驱动移动端架构：输入管线把触摸稳定转换为 Stamp；工具与污渍规则在纯 C# 中确定效果；GPU 状态纹理负责局部清洁、湿润、涂抹和推动；低频归约负责清洁度；LevelSession 负责流程、评分和完成；Progression 以原子主/备份存档提交奖励；UI、反馈和分析通过明确端口消费状态。开发必须按 M0→M1→M2 的技术与用户门禁推进，在手感和规则可理解性得到证明后才扩展 12 关内容。
