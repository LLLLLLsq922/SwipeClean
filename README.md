# SwipeClean

SwipeClean 是一个使用 Unity 6.3 LTS 开发的竖屏、离线清洁解压游戏。本仓库以需求分解和详细设计为规范来源，当前开发基线覆盖 M0 工程骨架与 M1 可玩纵向切片。

## 当前开发基线

- Unity `6000.3.23f1`（revision `09d2ecc7fb28`）
- Universal Render Pipeline `17.3.0`
- Input System `1.20.0`
- Unity Test Framework `1.6.0`
- 目标平台：Android / iOS，Portrait，IL2CPP，ARM64

Localization `1.5.13` 按设计保留为 M2 内容管线依赖；当前 M1 原型尚无字符串表或本地化资源，因此未提前加入运行时依赖。

## 第一次打开

1. 在 Unity Hub 登录并激活 Unity Personal 或组织许可证。
2. 将仓库目录作为已有项目打开。
3. 等待 Package Manager 完成依赖解析。
4. 执行菜单 `SwipeClean > Project > Configure Project`。
5. 打开 `Assets/SwipeClean/Scenes/Boot.unity` 并进入 Play Mode。

运行时会展示可操作的原型闭环：首页 → 第 1 关 → 擦除污渍 → 结果页。所有画面均为程序化占位内容，便于先验证输入、清洁度和状态流。

## 命令行

```powershell
$unity = 'D:\Unity\Hub\Editor\6000.3.23f1\Editor\Unity.exe'
& $unity -batchmode -projectPath 'D:\ChatGPT\SwipeClean' -runTests -testPlatform EditMode -testResults 'Builds\TestResults\editmode.xml' -quit
& $unity -batchmode -projectPath 'D:\ChatGPT\SwipeClean' -executeMethod SwipeClean.Editor.Build.BuildCommand.BuildAndroidDevelopment -quit
```

更完整的环境信息、目录约定和验证方式见 [Documentation/DevelopmentEnvironment.md](Documentation/DevelopmentEnvironment.md)。
当前里程碑的完成项和待验证项见 [Documentation/DevelopmentStatus.md](Documentation/DevelopmentStatus.md)。

## 规范文件

- `SwipeClean_MVP_完整需求分解.md`
- `SwipeClean_MVP_详细设计与开发规格.md`
