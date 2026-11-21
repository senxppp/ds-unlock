# 飞智八爪鱼 5 DS 模式解锁开关 · Flydigi APEX 5 DS Mode Unlock

让飞智空间站的 DualSense（DS）模式对**任意游戏**生效。官方只对少数游戏（如《赛博朋克 2077》）开放这个能力，
本工具负责把那个前提条件补上。适用于飞智八爪鱼 5（APEX 5）等飞智手柄。

仓库里两个小程序：

- `Cyberpunk2077.exe` — 3.5KB 空进程，只提供进程名，其余什么也不做
- `DSSwitch.exe` — 8KB WinForms 开关，点一下开、点一下关

开启后，任何支持 DualSense 的游戏（实测《明日方舟：终末地》）都能获得自适应扳机阻力。

## 使用前提

四条必须全部满足，缺一条都不会生效：

1. 安装并运行飞智空间站（Flydigi Space Station），手柄保持连接；
2. 在空间站里打开《赛博朋克 2077》的「自适应扳机」功能（该游戏配置中的那个开关）；
3. 在空间站里切换到 DS 模式；
4. 以上就绪后，打开本工具的开关，再启动游戏。

原因是：空间站只对它白名单里的游戏开放 DS 管线。《赛博朋克 2077》的自适应扳机开关 +
DS 模式这个组合就是那把钥匙，本工具用一个同名进程把钥匙复用给其他游戏。

## 原理（实测逆向结论）

测试环境：飞智空间站 + 八爪鱼 5（2.4G 接收器，VID:PID `37D7:2501`），2026-09。

**1. 触发条件是进程名，且只看进程名。**
空间站按进程名白名单检测游戏（如 `Cyberpunk2077`），不校验路径、不校验签名。
证据：官方游戏 mod 的配置里写明 `"process_name": "Cyberpunk2077"`，
见 `mods/Cyberpunk2077/configs/*.default.json`。

**2. DS 模式是 PC 端软件模拟，不是手柄固件变身。**
检测命中后，空间站通过 GeniTech 虚拟手柄总线（`ROOT\GENITECH_VIRTUAL_GAMEPAD_DEVICE`）
在电脑上创建一个虚拟 DualSense（`VID_054C&PID_0CE6`，产品名 "DualSense Wireless Controller"）。
手柄本体和接收器全程保持原身份（`37D7`），固件从不切换。
实测证据：模式切换表现为虚拟设备整个销毁再重建（47 次 PnP 状态翻转记录），而接收器身份不变；
虚拟 DS5 输入流实测 61,368 条报文 / 10 分钟。

**3. 自适应扳机的翻译管线由官方打通。**
游戏向虚拟 DS5 发送标准 DualSense 自适应扳机输出报文 → 空间站翻译成飞智私有 FORCEADAPT 指令
→ 经接收器厂商 HID 通道（usage page `0xFFA0`，32 字节输出报文）发给手柄 → 扳机产生真实阻力。
普通模式走同一套路：物理手柄输入经虚拟 "Controller (Flydigi ...)" 转发（实测约 18 万条输入报文 / 10 分钟）。

**4. 所以一个同名空进程就能让整条管线对任意游戏可用。**
本工具全程不接触游戏进程——不注入、不读内存、不挂钩子。空间站若顺带拉起它的内存监视 mod，
读的也只是这个 3.5KB 小程序自己的内存。

> 数据基础：抓包约 34 万条 HID 报文 + 47 次模式切换的设备事件日志。

## 用法

1. 双击 `DSSwitch.exe`（可创建桌面快捷方式），点开关 → 开启 DS 模式
2. 再启动游戏（切换伴随设备重建，需 1~3 秒，务必先进模式再进游戏）
3. 玩完再点一下关掉

### 自己构建（只需 Windows 自带的编译器）

```bat
build.bat
```

等价于：

```bat
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /target:winexe /out:Cyberpunk2077.exe dummy.cs
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /target:winexe /out:DSSwitch.exe dsswitch.cs /r:System.Windows.Forms.dll /r:System.Drawing.dll
```

开关程序里硬编码了诱饵路径 `D:\ds-unlock\Cyberpunk2077.exe`。
**这是原作者机器的路径，换机器必须改 `dsswitch.cs` 顶部的常量并重新编译**，否则开关会找不到诱饵。

## 配套项目

DS 模式下，握把震动会丢失（游戏给 DualSense 的音圈马达触觉报文没有对应翻译）。补它的有两条路线：

| 方案 | 负责 | 手段 | 状态 |
|---|---|---|---|
| [apex-haptic-bridge](https://github.com/senxppp/apex-haptic-bridge) | 握把 + 扳机全链路 | USB/IP 虚拟设备，截官方引擎的包络流再翻译 | 维护中，**推荐** |
| [gamepad-haptic-trigger-vibration-research](https://github.com/senxppp/gamepad-haptic-trigger-vibration-research) 里的 `haptic-assistant/` | 握把震动 | 自建音频分析 → 直发 `0xFFA0` | 已停止维护，留作参考 |

两者都写同一个 `0xFFA0` 接口，实测可与飞智空间站并存无冲突。
与旧版震动小助手搭配时的顺序：先开 DS 模式 → 再启动震动小助手 → 最后进游戏。

## 已知问题

- **DualSense 音圈马达的触觉无法转译。** 游戏里专为双音圈线性马达设计的精细 HD 震动
  （以音频流形式写入输出报文的 haptics）不会被翻译：飞智的管线只处理扳机阻力效果（FORCEADAPT），
  而八爪鱼的转子马达与 DualSense 的音圈执行器硬件原理不同，这部分报文被直接丢弃。
  表现是扳机有阻力，但部分游戏的细腻震动缺失或退化成普通转子震动。
  替代方案见上面「配套项目」——能让握把随音频震动，但复刻不了音圈马达的质感。
- 空间站后续版本可能改变检测行为（当前结论基于进程名匹配）。
- 模式切换会销毁并重建虚拟设备（约 1~3 秒），不要在游戏进行中切换。

## 免责声明

与飞智（Flydigi）官方无关，个人逆向研究成果，使用风险自负。
本工具不修改任何游戏内存、不提供游戏内优势，只改变使用者自己手柄在系统中的呈现形态。

## License

MIT

---

## 相关项目

- **[apex-haptic-bridge](https://github.com/senxppp/apex-haptic-bridge)** — 震动桥：USB/IP 虚拟设备截官方引擎的包络流，
  翻译成目标手柄的马达与扳机帧（`0x12` / `0x51`）。带写入节流、停帧自愈、热插拔守护与 222 条离线回归。
- **[gamepad-haptic-trigger-vibration-research](https://github.com/senxppp/gamepad-haptic-trigger-vibration-research)** —
  上一代音频转震动的实现存档，以及跨厂商协议方法论（5 篇）。
