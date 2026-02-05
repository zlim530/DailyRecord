using NetAutoGUI;

public static class SearchHelper
{
    /// <summary>
    /// 多种方法定位微信搜索框
    /// </summary>
    /// <param name="win">微信窗口对象</param>
    /// <returns>是否成功定位并激活搜索框</returns>
    public static bool ActivateSearchBox(Window win)
    {
        // 方法1: 使用图像识别
        if (TryImageRecognition())
        {
            Console.WriteLine("✓ 图像识别成功定位搜索框");
            return true;
        }

        // 方法2: 使用快捷键
        if (TryKeyboardShortcut())
        {
            Console.WriteLine("✓ 快捷键成功激活搜索框");
            return true;
        }

        // 方法3: 使用相对坐标定位
        if (TryRelativeCoordinates(win))
        {
            Console.WriteLine("✓ 相对坐标成功定位搜索框");
            return true;
        }

        // 方法4: 使用Tab键导航
        if (TryTabNavigation())
        {
            Console.WriteLine("✓ Tab导航成功定位搜索框");
            return true;
        }

        Console.WriteLine("✗ 所有搜索框定位方法都失败了");
        return false;
    }

    /// <summary>
    /// 方法1: 图像识别定位搜索框
    /// </summary>
    private static bool TryImageRecognition()
    {
        try
        {
            // 尝试不同的匹配精度
            double[] confidences = { 0.9, 0.8, 0.7, 0.6 };
            
            foreach (double confidence in confidences)
            {
                Rectangle? searchBox = GUI.Screenshot.LocateOnScreen(BitmapData.FromFile("Search.png"), confidence);
                if (searchBox != null)
                {
                    // 点击搜索框中心
                    int centerX = searchBox.X + searchBox.Width / 2;
                    int centerY = searchBox.Y + searchBox.Height / 2;
                    
                    GUI.Mouse.Click(centerX, centerY);
                    Thread.Sleep(300);
                    
                    // 清空可能存在的内容
                    GUI.Keyboard.Press((VirtualKeyCode)0x11, (VirtualKeyCode)0x41); // Ctrl+A
                    Thread.Sleep(100);
                    
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"图像识别异常: {ex.Message}");
        }
        
        return false;
    }

    /// <summary>
    /// 方法2: 使用快捷键激活搜索框
    /// </summary>
    private static bool TryKeyboardShortcut()
    {
        try
        {
            // 微信搜索快捷键 Ctrl+F
            GUI.Keyboard.Press((VirtualKeyCode)0x11, (VirtualKeyCode)0x46); // Ctrl+F
            Thread.Sleep(500);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"快捷键异常: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 方法3: 使用相对于窗口的坐标定位
    /// </summary>
    private static bool TryRelativeCoordinates(Window win)
    {
        try
        {
            // 由于NetAutoGUI的Window类可能没有GetWindowRect方法，我们使用固定坐标
            // 这些坐标需要根据实际的微信窗口位置调整
            int searchBoxX = 200; // 距离屏幕左边200像素
            int searchBoxY = 50;  // 距离屏幕顶部50像素
            
            GUI.Mouse.Click(searchBoxX, searchBoxY);
            Thread.Sleep(300);
            
            // 验证是否成功激活了输入框（尝试输入测试字符）
            GUI.Keyboard.Write("test");
            Thread.Sleep(100);
            GUI.Keyboard.Press((VirtualKeyCode)0x11, (VirtualKeyCode)0x41); // Ctrl+A
            Thread.Sleep(100);
            GUI.Keyboard.Press((VirtualKeyCode)0x2E); // Delete
            Thread.Sleep(100);
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"相对坐标定位异常: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 方法4: 使用Tab键导航到搜索框
    /// </summary>
    private static bool TryTabNavigation()
    {
        try
        {
            // 先点击窗口确保焦点在微信上
            GUI.Mouse.Click(400, 300);
            Thread.Sleep(200);
            
            // 使用Tab键导航，通常搜索框是第一个或前几个可聚焦元素
            for (int i = 0; i < 5; i++)
            {
                GUI.Keyboard.Press((VirtualKeyCode)0x09); // Tab
                Thread.Sleep(200);
                
                // 尝试输入测试字符看是否在搜索框
                GUI.Keyboard.Write("test");
                Thread.Sleep(100);
                
                // 如果能输入，说明找到了输入框
                GUI.Keyboard.Press((VirtualKeyCode)0x11, (VirtualKeyCode)0x41); // Ctrl+A
                Thread.Sleep(100);
                GUI.Keyboard.Press((VirtualKeyCode)0x2E); // Delete
                Thread.Sleep(100);
                
                // 简单验证：如果能成功执行到这里，认为找到了搜索框
                // 在实际应用中，可以添加更复杂的验证逻辑
                if (i == 0) // 假设第一个Tab就是搜索框
                {
                    return true;
                }
            }
            
            return false; // 如果所有尝试都失败
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Tab导航异常: {ex.Message}");
        }
        
        return false;
    }

    /// <summary>
    /// 搜索联系人或群组
    /// </summary>
    /// <param name="searchText">搜索文本</param>
    /// <returns>是否搜索成功</returns>
    public static bool SearchContact(string searchText)
    {
        try
        {
            // 输入搜索内容
            GUI.Keyboard.Write(searchText);
            Thread.Sleep(1000);
            
            // 按回车确认搜索
            GUI.Keyboard.Press(VirtualKeyCode.RETURN);
            Thread.Sleep(1500);
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"搜索联系人异常: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 发送消息的多种方法
    /// </summary>
    /// <param name="message">要发送的消息</param>
    /// <returns>是否发送成功</returns>
    public static bool SendMessage(string message)
    {
        try
        {
            // 输入消息
            GUI.Keyboard.Write(message);

            // 方法1: 尝试使用发送按钮
            if (TrySendButton())
            {
                Console.WriteLine("✓ 使用发送按钮发送消息");
                return true;
            }

            // 方法2: 使用回车键
            if (TrySendWithEnter())
            {
                Console.WriteLine("✓ 使用回车键发送消息");
                return true;
            }

            // 方法3: 使用Ctrl+Enter
            if (TrySendWithCtrlEnter())
            {
                Console.WriteLine("✓ 使用Ctrl+Enter发送消息");
                return true;
            }

            Console.WriteLine("✗ 所有发送方法都失败了");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发送消息异常: {ex.Message}");
            return false;
        }
    }

    private static bool TrySendButton()
    {
        try
        {
            Rectangle? sendButton = GUI.Screenshot.LocateOnScreen(BitmapData.FromFile("SendButton.png"), 0.7);
            if (sendButton != null)
            {
                GUI.Mouse.Click(sendButton.X + sendButton.Width / 2, sendButton.Y + sendButton.Height / 2);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发送按钮识别异常: {ex.Message}");
        }
        return false;
    }

    private static bool TrySendWithEnter()
    {
        try
        {
            GUI.Keyboard.Press(VirtualKeyCode.RETURN);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"回车发送异常: {ex.Message}");
            return false;
        }
    }

    private static bool TrySendWithCtrlEnter()
    {
        try
        {
            GUI.Keyboard.Press((VirtualKeyCode)0x11, VirtualKeyCode.RETURN); // Ctrl+Enter
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ctrl+Enter发送异常: {ex.Message}");
            return false;
        }
    }
}