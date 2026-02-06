# MoveAwayAndroid

> 一键清理Android设备自动在OTG存储生成的各类文件/文件夹
>
> 程序使用豆包编写，感谢豆包，感谢豆包，感谢豆包

我们的主页：https://github.com/iqonli/iqonli

本项目使用[GNU Lesser General Public License v2.1](LICENSE)许可证，程序使用C# WinForm开发。

本项目是Alone于2024.11.23编写的bat清理脚本的升级版本，原bat脚本实现了基础的文件移动与删除功能，本项目在此基础上新增可视化界面、智能检查、自定义配置等实用功能，让清理操作更安全、更丝滑。

Alone和PerryDing是同一个人，`Alone`是他的，好吧，我的网名，`PerryDing`是我的真名。

你可以在仓库的`old`文件夹下载到bat清理脚本的MoveAwayAndroid。

## 重要提示

1. 程序**必须放在OTG存储（U盘/移动硬盘）的根目录下**运行，否则无法识别并处理目标文件/文件夹

2. **≥1MB的项会红色加粗标红**，防止意外删除，需确认此类项非个人文件后再勾选操作

3. 配置文件`MoveAwayAndroid.cfg`为**UTF-8编码**，使用记事本/Notepad++编辑时请确认编码，避免乱码

## 介绍

1. **程序思路**：

延续原bat脚本的逻辑，列出Android系统在OTG存储中自动生成的文件/文件夹、文件数量、文件夹大小，用户可以选择并移动、删除。移动时，会覆盖移动到`.\$MoveAwayAndroid`文件夹；删除时，会先删除勾选项，再提示删除`.\$MoveAwayAndroid`文件夹。

2. **核心功能**：

- **可视化表格**：显示项目的勾选状态、名称、文件数、大小

- **智能勾选、标红提醒**：<1MB的项自动勾选，≥1MB的项不勾选，且加粗标红

- **自定义配置**：通过`MoveAwayAndroid.cfg`自定义待清理项，一行一个名称，适配不同Android设备的生成规则

- **在线下载配置**：提供3个优先级下载地址，一键下载最新配置，如下：

```
https://v6.gh-proxy.org/https://raw.githubusercontent.com/iqonli/MoveAwayAndroid/main/MoveAwayAndroid.cfg
https://gh-proxy.org/https://raw.githubusercontent.com/iqonli/MoveAwayAndroid/main/MoveAwayAndroid.cfg
https://raw.githubusercontent.com/iqonli/MoveAwayAndroid/main/MoveAwayAndroid.cfg
```

## 安装与使用

1. 下载项目[release](https://github.com/iqonli/MoveAwayAndroid/releases)版的zip压缩包

2. 解压后，将**所有文件**复制到你的OTG存储（U盘/移动硬盘）的**根目录**

3. 运行`MoveAwayAndroid.exe`，程序自动检测`MoveAwayAndroid.cfg`，无文件则自动创建空的UTF-8编码的配置文件

4. 编辑配置文件：

    - 手动编辑：打开`MoveAwayAndroid.cfg`，一行一个填写要清理的文件/文件夹名，保存后重启程序

    - 在线下载：点击`下载配置`，程序会按优先级尝试3个地址，下载成功后自动刷新表格

5. 表格自动过滤OTG根目录下不存在的项，仅显示存在的项，其中<1MB项自动勾选，≥1MB项加粗标红

6. 手动检查标红项，确认非个人文件后再勾选

7. 点击`移动`，将勾选的项覆盖移动至`.\$MoveAwayAndroid`文件夹，移动完成后表格自动刷新

8. 点击`删除`，第一步删除表格中的勾选项（若有），第二步弹窗显示`.\$MoveAwayAndroid`的大小和文件数，**确认无误后再删除**

有事没事请加QQ群：743278470，你可以添加句子，报告错误，寻求帮助，官匹开黑