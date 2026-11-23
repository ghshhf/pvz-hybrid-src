# 植物大战僵尸杂交版 0.31 — 反编译源码工程

《植物大战僵尸杂交版》0.31（Windows x86_64）的 C# 程序集反编译产物，修复到**可编译、可替换运行、游戏资源全部正常加载**的状态。

游戏本身是 Godot 4.7.0 stable mono 工程。本仓库只包含一个自研程序集 `PlantsVsZombies.dll`（19.2 MB）反编译出的代码，**不含任何游戏美术、音频、关卡数据**。

> 仅供个人学习与逆向研究。游戏资源版权归原作者所有，请自行准备正版游戏目录，不要用本仓库传播任何游戏资源或成品程序。

## 这是什么

| 项 | 值 |
|---|---|
| 源程序集 | `PlantsVsZombies.dll`（19.2 MB，Godot 4.7.0 mono 单程序集） |
| 反编译器 | ILSpy 9.1（`ilspycmd -p` 整工程导出，dnSpy 会产出非法类名，不可用） |
| 规模 | 2,742 个 `.cs` / 131 万行（120 万非空） |
| 目标框架 | `net9.0` / `LangVersion 13.0` |
| 编译结果 | **0 error / 508 warning**，产出 21.4 MB dll |
| 原生扩展 | 无（无 GDExtension，代码 100% 在此 dll 内） |

代码覆盖面是完整的：pck 内虽然有 3,145 个 `.cs`，但全部是 0 字节占位文件，真实逻辑只在 dll 里；`.gdc` 编好的脚本仅 11 个且实质逻辑为空。也就是说这个程序集就是游戏全部的业务代码。

## 构建

需要 .NET 9 SDK（本仓库验证于 9.0.315，运行时 9.0.17）。若你的 SDK 不在默认位置：

```bash
DOTNET_ROOT='K:\DevTools\dotnet' /k/DevTools/dotnet/dotnet.exe build
```

或直接：

```bash
dotnet build
```

产物在 `bin/Debug/net9.0/PlantsVsZombies.dll`。

## 运行

把产出的 `PlantsVsZombies.dll` 覆盖到游戏目录：

```
<游戏目录>\data_PlantsVsZombies_windows_x86_64\PlantsVsZombies.dll
```

覆盖前请自行备份原版 dll。验证过的行为：

- **窗口模式**：Vulkan 1.3 Forward+ 正常渲染，`CharacterScene` 628 个 / `Packet` 741 个节点全部加载
- **无头模式**（`--headless`）：HWND = 0，无窗口，同样全部加载，空转 CPU 约 1.7%，内存约 507 MB

## 反编译残留修复（179 → 0）

ILSpy 导出的工程不能直接编译，逐条修完的错误：

1. `KeyValuePair<K,V>` 的 `Deconstruct` 被误还原成 `(T)(ref x)` 伪 cast → 改回普通赋值
2. `[InlineArray(N)]` 结构缺实例字段 → 补 `private T _element0;`（对应文件 `--y__InlineArray2.cs` 等）
3. 删除编译器专用程序集特性 `[module: RefSafetyRules(11)]`
4. `Container` 命名歧义（`System.ComponentModel.Container` vs `Godot.Container`）→ 加别名 `using Container = Godot.Container;`
5. `<>PrivateImplementationDetails` 被导出成空文件，导致 105 个 CS0400 → 手写 `Unsafe.As` + `MemoryMarshal.CreateReadOnlySpan` 等价实现（见 `__PrivateImplementationDetails.cs`）
6. `get; init;` 在构造器外赋值 → 统一放宽为 `set`（245 处），连带去掉相应 `readonly struct` 的 `readonly`
7. 变量名丢失（`out x` → `out var x`）、query 语法还原失败改方法链、闭包 DisplayClass 改直接捕获、`this._002Ector` → `: this(...)`
8. **程序集引用污染**：游戏目录 73 个 dll 的 IL 直接引用了 `System.Private.CoreLib`，与 .NET 9 的 `System.Runtime` 门面冲突（直接引用 CoreLib 会炸出 32 万错误）。用 Mono.Cecil 把 6 个 `Microsoft.Extensions.*` 的引用重定向到 `System.Runtime` 门面，修好的库放在 `libs/`，csproj 的 `HintPath` 全部指向该目录

## 唯一的逻辑改动：无头适配

`Loading.cs` 原本等 `RenderingServer.FramePostDraw` 信号，无头模式下 `RenderingServer` 是空实现、永不派发该信号，进程会永久挂起在启动阶段。改成按显示服务器类型分支：

```csharp
private static bool IsHeadlessDisplay() => string.IsNullOrEmpty(DisplayServer.GetName())
                                        || DisplayServer.GetName() == "headless";

private async Task WaitFirstFrameAsync()
{
    if (IsHeadlessDisplay())
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        return;
    }
    await RenderingServer.Singleton.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
}
```

这个写法与项目自身的 `ResourceManager.YieldAfterStageAsync` 的 headless 分支保持一致，窗口模式下行为完全不变。

## 目录

```
*.cs                      反编译出的全部源码（ILSpy 平铺导出，无命名空间分目录）
libs/                     修好的依赖库（Mono.Cecil 重定向后的 Microsoft.Extensions.* 等）
PlantsVsZombies.csproj    HintPath 指向 libs/
__PrivateImplementationDetails.cs  手写的 InlineArray / span 等价实现
```

## 已知未完成

- 游戏资源（541 MB / 28,089 条）不在逆向范围内：3,930 个 `.ctex` 纹理无法还原成可读画面
- 4,802 个 `.res` 数值表、1,356 个 `.scn` 场景尚未导出成文本
- 引擎是官方 Godot 4.7.0，未做改动

## 能拿它做什么

有了能编译的源码，做新植物、新僵尸、新关卡从"只能写 mod 钩子"变成"可以直接改逻辑并重新编译"。官方 mod 体系（`.pmod` / `mod.json` v3 / Roslyn 脚本 mod，钩子如 `damage.calculate.v1`、`plant.placement.v1`、`plant.cost.v1`）依然可用，两条路可以混着走：轻量逻辑用 mod，涉及深层数据结构改造走源码编译。
