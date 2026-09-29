# 开发状态

更新日期：2026-09-21

## 已完成

### M0 工程骨架

- 锁定 Unity 6000.3.23f1 与核心 Package 版本；
- 建立 Domain、Application、Core、Input、Cleaning、Levels、UI、Editor 和 Tests 程序集边界；
- 建立 AppRoot、应用状态机、类型化事件总线、GameClock 和平台生命周期桥；
- 提供幂等项目配置器，首次导入时生成四个 Scene、构建场景列表、URP Asset 和双平台 Player Settings；
- 提供 Android/iOS Development 构建入口、Git 忽略规则与 CI 纯逻辑检查。

### M1 可玩纵向切片

- Input System 主指针采样、UI 起点屏蔽、UV 投影、轨迹空间插值、速度与长按压力计算；
- 修复 Android 触摸接触 Action 误用 `PassThrough` 导致 `started/canceled` 永不触发的问题，改用 Button 的 `performed/canceled` 完整笔画生命周期；
- 固定容量 BrushCommand 队列和每帧/每批处理上限；
- 单污渍层 ping-pong Render Texture 更新；
- 初始化、批量 Clean、覆盖率 2×2 归约与表面合成 Shader；
- 将四个运行时清洁 Shader 固定加入 Always Included Shaders，并增加 Android/iOS 构建前自动修复与缺失阻断，避免 `Shader.Find` 资源在 Player 构建时被裁剪；
- 1×1 异步 GPU 回读、清洁度计算、检查阈值和连续两次完成防抖；若移动设备异步回读失败，自动切换同步 1×1 回读，避免输入永久等待覆盖率初始化；
- 程序化首页、关卡 HUD、暂停、重开、完成和结果页；
- EditMode 单元测试、PlayMode 启动冒烟测试和不依赖 Unity 许可证的纯逻辑检查器。

## 当前验证结果

- `Tools/verify.ps1`：通过；
- 纯逻辑状态机、事件隔离、加权清洁度、评分、目标防抖和关卡会话：通过；
- Core、Input 纯管线、Cleaning 与 Levels 对 Unity 6.3 实际运行时程序集的 API 编译检查：0 警告、0 错误；
- 12 个 JSON/asmdef/Input Actions 文件：解析通过；
- Unity Personal 授权已激活；Unity 6000.3.23f1 首次导入、Package 解析、脚本与 Shader 编译通过；
- 已生成 Boot、Frontend、Gameplay、TestHarness 四个场景、URP Renderer/Pipeline Asset 与三场景构建列表；
- Unity EditMode：19/19 通过（含清洁 Shader 打包配置回归测试）；Unity PlayMode：2/2 通过（含真实 Touchscreen 按下、移动、抬起生命周期测试）；
- Android OpenJDK 17、SDK 36、NDK 27.2.12479018、CMake、IL2CPP ARM64、Gradle 9.1.0 与 AGP 9.0.0 全链路通过；
- Android Development APK 已生成并验证：`com.example.swipeclean`、版本 `0.1.2`（versionCode 3）、minSdk 25、targetSdk 36、APK v2 调试签名有效；
- 构建日志确认 `StainInitialize`、`StainUpdate`、`SurfaceComposite`、`CoverageReduce` 均已编译和序列化，最终 Player 数据也包含四个 `Hidden/SwipeClean/*` Shader；
- 最终 IL2CPP 元数据包含触摸修复入口与 GPU 回读兜底，Player 数据继续包含四个清洁 Shader；
- APK：`Builds/Android/SwipeClean-development.apk`（76.05 MiB，SHA-256 `CBA4A736214EC79B5935EEF8D1369BB4502E287F69ACA5F040EDB3BFFD7FC363`）。

当前 M1 不使用本地化字符串或资产表，Localization `1.5.13` 延后至 M2 内容管线接入，避免在未使用阶段增加包依赖。

## 下一阶段

1. 在 Android 真机安装 Development APK，验证竖屏、触摸输入、暂停/恢复、GPU 回读和内存；
2. 录制 30/60 FPS 基准轨迹与中低端设备 Profile；
3. 按 M1 用户测试迭代半径、强度、材质、摩擦音与触觉；
4. 在 macOS/Xcode 完成 iOS 导出工程的签名与真机构建；
5. 进入 M2：多层状态、效果矩阵、三工具、Catalog/Baker、存档、本地化和三关内容。
