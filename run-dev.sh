#!/bin/bash

echo "🎮 启动Tetris游戏开发版本..."
echo

# 检查dotnet是否安装
if ! command -v dotnet &> /dev/null; then
    echo "❌ 错误: 未找到 .NET SDK，请先安装 .NET 8.0 SDK"
    echo "下载地址: https://dotnet.microsoft.com/download"
    exit 1
fi

echo "✅ 检测到 .NET SDK 版本: $(dotnet --version)"
echo

# 在开发模式下运行，不指定RuntimeIdentifier
echo "🚀 启动游戏..."
dotnet run --configuration Debug

if [ $? -eq 0 ]; then
    echo
    echo "✅ 游戏已正常退出"
else
    echo
    echo "❌ 运行失败！"
    echo "💡 提示：如果是首次运行，请先执行 'dotnet restore' 恢复包"
    echo
fi