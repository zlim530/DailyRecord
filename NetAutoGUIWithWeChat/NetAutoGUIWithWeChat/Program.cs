using NetAutoGUI;

Console.WriteLine("=== NetAutoGUI 微信自动化程序 ===");

GUIWindows.Initialize(); // Initialize the NetAutoGUI Windows platform.

// 查找微信窗口
Window? win = GUI.Application.FindWindowByTitle("微信");
if (win == null)
{
    Console.WriteLine("正在尝试启动微信...");
    // 如果微信窗口没找到，尝试点击托盘图标启动微信
    Rectangle? rectWXIcon = GUI.Screenshot.LocateOnScreen(BitmapData.FromFile("WeChatIcon.png"));
	if (rectWXIcon == null)
	{
        Console.WriteLine("❌ 微信托盘图标没找到！请确保微信已启动或图标文件存在。");
		return;
	}
	GUI.Mouse.Click(rectWXIcon.X, rectWXIcon.Y);
	win = GUI.Application.WaitForWindowByTitle("微信");
    
    if (win == null)
    {
        Console.WriteLine("❌ 无法启动微信窗口！");
        return;
    }
}

// 激活微信窗口
win.Activate();
Console.WriteLine("✓ 微信窗口已激活");

// 等待窗口完全加载
Thread.Sleep(1500);

// 使用辅助类定位搜索框
Console.WriteLine("\n--- 正在定位搜索框 ---");
if (!SearchHelper.ActivateSearchBox(win))
{
    Console.WriteLine("❌ 无法定位到搜索框！");
    return;
}

// 搜索联系人
Console.WriteLine("\n--- 正在搜索联系人 ---");
if (!SearchHelper.SearchContact("微信助手"))
{
    Console.WriteLine("❌ 搜索联系人失败！");
    return;
}

Console.WriteLine("✓ 搜索完成，等待结果加载...");

// 发送消息
Console.WriteLine("\n--- 正在发送消息 ---");
string message = "大家好！这是使用NetAutoGUI自动发送的消息。";
if (SearchHelper.SendMessage(message))
{
    Console.WriteLine("✅ 消息发送成功！");
}
else
{
    Console.WriteLine("❌ 消息发送失败！");
}

Console.WriteLine("\n=== 程序执行完成 ===");
Thread.Sleep(2000);