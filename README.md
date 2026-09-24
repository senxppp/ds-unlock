# 飞智八爪鱼5 DS模式解锁开关 · Flydigi APEX 5 DS Mode Unlock

> **关键词 Keywords：飞智八爪鱼5 / Flydigi APEX 5 / 八爪鱼5 / DualSense 模拟 / DS 模式 / DS Mode / 手柄模拟器 Gamepad Emulator / 自适应扳机 Adaptive Triggers / 扳机阻力 Trigger Resistance / 飞智空间站 Flydigi Space Station / 赛博朋克2077 Cyberpunk 2077 / 明日方舟：终末地 Arknights: Endfield / PS5 手柄 DualSense Wireless Controller / DS Unlock / 进程诱饵 Process Decoy / FORCEADAPT / HD Haptics**

一个小开关，让飞智空间站的 **DualSense（DS）模式**为**任意游戏**开启——官方只给少数游戏（如《赛博朋克 2077》）开了这个特权。适用于飞智八爪鱼5（APEX 5）等飞智手柄。

- `Cyberpunk2077.exe`：3.5KB 空进程诱饵（就是个"占位小程序"）
- `DSSwitch.exe`：8KB WinForms 小开关，点一下开、点一下关

开启后，任何支持 DualSense 的游戏（实测《明日方舟：终末地》）都能获得**自适应扳机阻力**。

## ⚠️ 使用前提（必须全部满足，否则无效）

1. 安装并运行 **飞智空间站（Flydigi Space Station）**，手柄（如飞智八爪鱼5）保持连接；
2. 在飞智空间站里，**打开《赛博朋克 2077》的「自适应扳机」功能**（该游戏配置中的开关）；
3. 在飞智空间站里，**切换到 DS 模式**；
4. 以上就绪后，再打开本工具的开关（诱饵进程），然后启动游戏。

> 原理上：空间站只对它信任的游戏开放 DS 管线；《赛博朋克 2077》的自适应扳机开关 + DS 模式就是那把"钥匙"。本工具用一个同名诱饵进程，让这把钥匙为所有游戏转动。

## 原理（实测逆向结论）

测试环境：飞智空间站 + 八爪鱼5（2.4G 接收器，VID:PID 37D7:2501），2026-09。

1. **触发条件 = 进程名，且只看进程名。**
   空间站按进程名白名单检测游戏（如 `Cyberpunk2077`），不校验路径、不校验签名。证据：官方游戏 mod 的配置里写明 `"process_name": "Cyberpunk2077"`（见 `mods/Cyberpunk2077/configs/*.default.json`）。

2. **"DS 模式"是 PC 端软件模拟，不是手柄固件变身。**
   检测命中后，空间站通过 **GeniTech 虚拟手柄总线**（`ROOT\GENITECH_VIRTUAL_GAMEPAD_DEVICE`）在电脑上创建一个**虚拟 DualSense**（VID_054C&PID_0CE6，产品名 "DualSense Wireless Controller"）。手柄本体和接收器全程保持原身份（37D7），固件从不切换。
   实测证据：模式切换 = 虚拟设备整个销毁→重建（47 次 PnP 状态翻转记录），而接收器身份不变；虚拟 DS5 输入流实测 61,368 条报文/10 分钟。

3. **自适应扳机的翻译管线由官方打通。**
   游戏向虚拟 DS5 发送标准 DualSense 自适应扳机输出报文 → 空间站翻译成飞智私有 **FORCEADAPT** 指令 → 经接收器厂商 HID 通道（usage page `0xFFA0`，32 字节输出报文）发给手柄 → 扳机产生真实阻力。
   普通模式也是同一套路：物理手柄输入经虚拟 "Controller (Flydigi ...)" 转发（实测 ~18 万条输入报文/10 分钟）。

4. **所以：一个改名为 `Cyberpunk2077.exe` 的空进程即可骗开整条管线。**
   本工具全程**不接触游戏进程**——不注入、不读内存、不挂钩子，反作弊零风险。空间站如果顺手拉起它的内存监视 mod，读的也只是这个 3.5KB 小程序自己的内存，无害。

> 数据基础：抓包实测约 34 万条 HID 报文 + 47 次模式切换的设备事件日志。

## 用法

1. 双击 `DSSwitch.exe`（可创建桌面快捷方式），点开关 → 开启 DS 模式
2. **再启动游戏**（切换/设备重建需 1~3 秒，务必先进模式再进游戏）
3. 玩完再点一下关掉

### 自己构建（只需 Windows 自带编译器）

```bat
build.bat
```

等价于：

```bat
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /target:winexe /out:Cyberpunk2077.exe dummy.cs
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /nologo /target:winexe /out:DSSwitch.exe dsswitch.cs /r:System.Windows.Forms.dll /r:System.Drawing.dll
```

开关程序硬编码诱饵路径 `D:\ds-unlock\Cyberpunk2077.exe`（改 `dsswitch.cs` 顶部常量即可）。

## Known Issues

- ❗ **DualSense 音圈马达（voice coil）触觉无法转译。** 游戏里专为 DualSense 双音圈线性马达设计的精细 HD 震动（以音频流形式写入输出报文的 haptics）**目前不会被翻译**：飞智的管线只处理扳机阻力效果（FORCEADAPT），而八爪鱼的震动马达与 DualSense 的音圈执行器硬件完全不同，这部分报文被直接丢弃。表现：扳机有阻力，但部分游戏的细腻震动缺失或退化成普通转子震动。**欢迎 PR——等一位大佬出手拯救，呵呵。**
- 空间站未来更新可能改变检测行为（当前为进程名匹配）。
- 模式切换伴随虚拟设备销毁/重建（约 1~3 秒），不要在游戏进行中切换。

## 免责声明

与飞智（Flydigi）官方无关，个人逆向研究成果，使用风险自负。本工具不修改任何游戏内存、不提供任何游戏内优势，仅改变**你自己手柄的输入设备形态**。

## License

MIT
