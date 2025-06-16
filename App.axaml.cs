using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;

namespace TetrisGame;

public partial class App : Application
{
    private MainWindow? _mainWindow;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _mainWindow = new MainWindow();
            desktop.MainWindow = _mainWindow;
            
            // 订阅应用程序退出事件，确保在任何情况下都能清理资源
            desktop.ShutdownRequested += OnShutdownRequested;
            
            // 处理进程退出事件
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            Console.CancelKeyPress += OnCancelKeyPress;
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        CleanupResources();
    }
    
    private void OnProcessExit(object? sender, EventArgs e)
    {
        CleanupResources();
    }
    
    private void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        CleanupResources();
    }
    
    private void CleanupResources()
    {
        try
        {
            // 确保主窗口中的游戏控制器被正确释放
            if (_mainWindow != null)
            {
                // 检查是否在UI线程中，如果不是则调度到UI线程
                if (Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
                {
                    // 已在UI线程中，直接执行清理
                    PerformCleanup();
                }
                else
                {
                    // 不在UI线程中，调度到UI线程执行
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        PerformCleanup();
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"清理资源时出错: {ex.Message}");
        }
    }
    
    private void PerformCleanup()
    {
        try
        {
            // 直接调用MainWindow的清理方法
            var gameView = _mainWindow?.FindControl<Views.GameView>("GameView");
            var gameController = gameView?.GetGameController();
            gameController?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"执行清理时出错: {ex.Message}");
        }
    }
}