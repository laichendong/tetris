# Tetris游戏 - Windows单文件发布指南

本指南将帮助您将Tetris游戏打包为独立的Windows可执行文件，无需在目标机器上安装.NET运行时。

## 🎯 功能特性

- ✅ **单文件发布**: 所有依赖项打包到一个exe文件中
- ✅ **自包含部署**: 无需在目标机器安装.NET运行时
- ✅ **原生库支持**: 自动包含所需的原生库文件
- ✅ **文件压缩**: 启用压缩以减小文件大小
- ✅ **跨平台构建**: 可在任何支持.NET的平台上构建Windows版本

## 🛠️ 构建方法

### 方法一：使用批处理脚本（推荐Windows用户）

1. 在项目根目录双击运行 `build-windows-single-file.bat`
2. 等待构建完成
3. 在 `bin/Release/net8.0/win-x64/publish/` 目录找到 `TetrisGame.exe`

### 方法二：使用PowerShell脚本（功能更丰富）

1. 右键点击 `build-windows-single-file.ps1`
2. 选择"使用PowerShell运行"
3. 按照提示完成构建
4. 脚本会自动显示构建结果和文件信息

### 方法三：手动命令行构建

在项目根目录打开命令提示符或PowerShell，执行：

```bash
# 基本发布命令
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# 完整发布命令（包含所有优化选项）
dotnet publish -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true \
  -p:PublishTrimmed=false \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:EnableCompressionInSingleFile=true
```

## 📋 系统要求

### 构建环境
- .NET 8.0 SDK 或更高版本
- Windows、macOS 或 Linux（可跨平台构建）

### 目标运行环境
- Windows 10 x64 或更高版本
- Windows 11 x64
- 无需安装.NET运行时

## 📁 输出文件说明

构建成功后，您将在以下位置找到可执行文件：
```
bin/Release/net8.0/win-x64/publish/TetrisGame.exe
```

### 文件特性
- **大小**: 约 80-120 MB（包含完整.NET运行时）
- **依赖**: 无外部依赖，可独立运行
- **音频**: 包含所有音效文件（WAV格式）
- **资源**: 包含游戏图标和界面资源

## 🔧 项目配置说明

项目文件 `TetrisGame.csproj` 中的关键配置：

```xml
<!-- 单文件发布配置 -->
<PublishSingleFile>true</PublishSingleFile>                    <!-- 启用单文件发布 -->
<SelfContained>true</SelfContained>                           <!-- 自包含部署 -->
<RuntimeIdentifier>win-x64</RuntimeIdentifier>                <!-- 目标平台 -->
<PublishTrimmed>false</PublishTrimmed>                        <!-- 禁用裁剪（确保兼容性） -->
<IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>  <!-- 包含原生库 -->
<EnableCompressionInSingleFile>true</EnableCompressionInSingleFile>              <!-- 启用压缩 -->
```

## 🚀 部署和分发

1. **简单分发**: 直接复制 `TetrisGame.exe` 到目标机器
2. **安装包制作**: 可使用 Inno Setup、NSIS 等工具制作安装程序
3. **便携版本**: exe文件本身就是便携版本，无需安装

## ⚠️ 注意事项

### 文件大小
- 单文件版本比普通发布版本大，因为包含了完整的.NET运行时
- 如果文件大小是关键考虑因素，可以考虑框架依赖发布

### 启动时间
- 首次启动可能比普通版本稍慢，因为需要解压运行时
- 后续启动速度正常

### 兼容性
- 确保目标机器是64位Windows系统
- 某些企业环境可能阻止运行未签名的exe文件

## 🔍 故障排除

### 构建失败
1. **检查.NET SDK版本**: 确保安装了.NET 8.0 SDK
2. **网络连接**: 首次构建需要下载依赖包
3. **磁盘空间**: 确保有足够的磁盘空间（至少1GB）
4. **权限问题**: 确保对项目目录有写入权限

### 运行时问题
1. **缺少Visual C++运行库**: 某些Windows系统可能需要安装Visual C++ Redistributable
2. **防病毒软件**: 某些防病毒软件可能误报，需要添加白名单
3. **权限不足**: 确保有执行权限

## 📞 技术支持

如果遇到问题，请检查：
1. .NET SDK是否正确安装
2. 项目是否能正常编译
3. 是否有网络连接下载依赖包
4. 目标机器是否满足系统要求

---

**提示**: 建议在发布前先在本地测试构建的exe文件，确保所有功能正常工作。