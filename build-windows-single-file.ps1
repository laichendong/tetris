# Windows单文件发布脚本
# 用于将Tetris游戏打包为独立的Windows可执行文件

Write-Host "🎮 Tetris游戏 - Windows单文件发布工具" -ForegroundColor Cyan
Write-Host "=" * 50 -ForegroundColor Gray
Write-Host ""

# 检查dotnet是否安装
try {
    $dotnetVersion = dotnet --version
    Write-Host "✅ 检测到 .NET SDK 版本: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ 错误: 未找到 .NET SDK，请先安装 .NET 8.0 SDK" -ForegroundColor Red
    Write-Host "下载地址: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    Read-Host "按任意键退出"
    exit 1
}

Write-Host ""

# 清理旧的发布文件
$publishPath = "bin/Release/net8.0/win-x64/publish"
if (Test-Path $publishPath) {
    Write-Host "🧹 清理旧的发布文件..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force $publishPath
    Write-Host "✅ 清理完成" -ForegroundColor Green
}

Write-Host ""
Write-Host "🔨 开始构建单文件可执行程序..." -ForegroundColor Cyan
Write-Host ""

# 执行发布命令
$publishCommand = "dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true"

try {
    Invoke-Expression $publishCommand
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "🎉 构建成功！" -ForegroundColor Green
        Write-Host "=" * 30 -ForegroundColor Gray
        
        $exePath = "bin/Release/net8.0/win-x64/publish/TetrisGame.exe"
        if (Test-Path $exePath) {
            $fileSize = [math]::Round((Get-Item $exePath).Length / 1MB, 2)
            Write-Host "📁 输出文件: $exePath" -ForegroundColor White
            Write-Host "📊 文件大小: $fileSize MB" -ForegroundColor White
            Write-Host ""
            Write-Host "💡 使用说明:" -ForegroundColor Yellow
            Write-Host "   • 生成的exe文件包含所有依赖项" -ForegroundColor Gray
            Write-Host "   • 可在未安装.NET的Windows系统上运行" -ForegroundColor Gray
            Write-Host "   • 支持Windows 10/11 x64系统" -ForegroundColor Gray
            Write-Host "   • 双击即可运行，无需额外安装" -ForegroundColor Gray
            
            Write-Host ""
            $openFolder = Read-Host "是否打开输出文件夹? (y/n)"
            if ($openFolder -eq 'y' -or $openFolder -eq 'Y') {
                Start-Process -FilePath "explorer.exe" -ArgumentList "/select,`"$(Resolve-Path $exePath)`""
            }
        }
    } else {
        throw "发布命令执行失败"
    }
} catch {
    Write-Host ""
    Write-Host "❌ 构建失败！" -ForegroundColor Red
    Write-Host "错误信息: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "🔧 可能的解决方案:" -ForegroundColor Yellow
    Write-Host "   1. 确保项目文件没有语法错误" -ForegroundColor Gray
    Write-Host "   2. 检查网络连接（需要下载依赖包）" -ForegroundColor Gray
    Write-Host "   3. 尝试运行 'dotnet restore' 恢复包" -ForegroundColor Gray
    Write-Host "   4. 确保有足够的磁盘空间" -ForegroundColor Gray
}

Write-Host ""
Read-Host "按任意键退出"