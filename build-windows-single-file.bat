@echo off
echo 正在构建Windows单文件可执行程序...
echo.

REM 清理之前的发布文件
if exist "bin\Release\net8.0\win-x64\publish" (
    echo 清理旧的发布文件...
    rmdir /s /q "bin\Release\net8.0\win-x64\publish"
)

REM 发布为单文件可执行程序
echo 开始发布...
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ 构建成功！
    echo 📁 输出文件位置: bin\Release\net8.0\win-x64\publish\TetrisGame.exe
    echo.
    echo 💡 提示：
    echo    - 生成的exe文件包含了所有依赖项，可以在没有安装.NET运行时的Windows系统上运行
    echo    - 文件大小可能较大，但无需额外安装任何组件
    echo    - 支持Windows 10/11 x64系统
    echo.
    pause
) else (
    echo.
    echo ❌ 构建失败！请检查错误信息。
    echo.
    pause
)