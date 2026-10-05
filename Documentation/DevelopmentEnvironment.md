# SwipeClean 开发环境

## 已锁定工具链

| 工具 | 版本/位置 |
|---|---|
| Unity Editor | `6000.3.23f1`，`D:\Unity\Hub\Editor\6000.3.23f1` |
| Visual Studio | Visual Studio Community 2022 17.14，含 Unity 游戏开发工作负载 |
| Git | 2.54 |
| Android JDK | Unity 内置 OpenJDK 17 |
| Android SDK | Unity 内置 SDK，Build Tools / compileSdk / targetSdk 36 |
| Android NDK | 27.2.12479018（r27c） |
| Android 构建 | Gradle 9.1.0，Android Gradle Plugin 9.0.0 |
| iOS Support | Unity iOS Build Support；最终 Xcode 构建必须在 macOS 完成 |

Android 工具链应位于 `Editor/Data/PlaybackEngines/AndroidPlayer` 下。项目不得绑定 Android Studio 自带的 JDK 25，以免 Gradle/IL2CPP 与 Unity 版本产生漂移。

## 初始化流程

项目设置由 `SwipeClean.Editor.ProjectSetup.ProjectConfigurator` 集中维护。它负责：

- 创建 Boot、Frontend、Gameplay、TestHarness 场景；
- 配置场景构建顺序；
- 设置产品名、包标识、竖屏、Linear 色彩空间与 Input System；
- 设置 Android/iOS IL2CPP 与 ARM64；
- 创建并绑定 URP Render Pipeline Asset；
- 将四个清洁 Shader 写入 Always Included Shaders，并在每次 Player 构建前校验、自动补齐；
- 写入项目配置标记，保证重复执行是幂等的。

首次克隆后或调整引擎版本后，应重新执行配置菜单并提交 Unity 生成或更新的 `.meta`、`ProjectSettings` 与 `packages-lock.json`。

## 分支与提交

- 默认分支：`main`
- 引擎或 Package 升级必须独立提交。
- 不提交 `Library`、`Temp`、`Logs`、构建产物、签名材料或本机路径。
- 业务代码位于 `Assets/SwipeClean`；第三方内容必须位于 `Assets/ThirdParty/<Vendor>` 并附许可证说明。

## 设计文档索引

| 文档 | 内容 | 对应里程碑 |
|---|---|---|
| [DevelopmentEnvironment.md](DevelopmentEnvironment.md) | 工具链锁定、初始化流程、本地验证顺序 | 全阶段 |
| [DevelopmentStatus.md](DevelopmentStatus.md) | M0/M1 完成项与验证结果、下一阶段 | 全阶段 |
| [污渍系统需求文档.md](污渍系统需求文档.md) | 干/湿双相态与 14 子类定义、64 种物质需求、83 条 `REQ-STAIN-###`、20 条非功能需求 | M2 输入 |
| [污渍系统可行性分析文档.md](污渍系统可行性分析文档.md) | M1 代码能力盘点、技术/性能/内存/跨平台可行性、16 条 `RISK-STAIN-###`、验证计划 | M2 决策 |
| [污渍系统设计文档.md](污渍系统设计文档.md) | 分类与数据模型、64 张物质卡、相变状态机、效果矩阵与降维、Shader 通道/Pass 改造、12 关编排、10 步迁移路线 | M2/M3 实施 |

三份污渍文档的章节交叉引用约定：需求文档 §1~§12、可行性分析文档 §1~§11、设计文档 §1~§15 + 附录 A/B/C（附录 C 为分册自检与勘误记录）。

## 本地验证

最低验证顺序：

1. EditMode 测试通过；
2. Boot Scene Play Mode 冒烟通过；
3. Android Development 构建成功；
4. 在 Android 真机验证竖屏、触摸输入、暂停/恢复和 30/60 FPS；
5. iOS 工程导出后在 macOS/Xcode 完成签名与真机构建。

2026-09-21 已完成触摸修复后全链路验证：EditMode 19/19、PlayMode 2/2（含真实 Touchscreen 生命周期），通过 IL2CPP/ARM64、NDK、CMake、Gradle 与 APK v2 调试签名；触摸修复入口、GPU 回读兜底和四个清洁 Shader 均存在于最终 Player 数据中。生成物位于 `Builds/Android/SwipeClean-development.apk`，该目录按约定不提交版本库。

## 已知一次性人工步骤

Unity 命令行无法替用户获取账号许可证。若批处理日志出现 `No valid Unity Editor license found`，请先在 Unity Hub 登录并激活许可证，然后重新运行配置和测试命令。此步骤不影响源码和 Android/iOS 模块安装。

Localization `1.5.13` 将在 M2 创建 String/Asset Table 时加入 `Packages/manifest.json`；M1 没有本地化资源，不为未使用功能增加运行时依赖。
