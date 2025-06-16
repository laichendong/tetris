@echo off
echo 启动Tetris游戏开发版本...
echo.

REM 在开发模式下运行，不指定RuntimeIdentifier
dotnet run --configuration Debug

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ❌ 运行失败！
    echo 💡 提示：如果是首次运行，请先执行 'dotnet restore' 恢复包
    echo.
    pause
) else (
    echo.
    echo ✅ 游戏已退出
    pause
)