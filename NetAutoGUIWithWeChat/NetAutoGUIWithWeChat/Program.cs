using NetAutoGUI;

GUIWindows.Initialize(); // Initialize the NetAutoGUI Windows platform.

Window? win = GUI.Application.FindWindowByTitle("微信");
if (win == null)
{
    // 如果微信窗口没找到，尝试点击托盘图标启动微信
    Rectangle? rectWXIcon = GUI.Screenshot.LocateOnScreen(BitmapData.FromFile("WeChatIcon.png"));
	if (rectWXIcon == null)
	{
        Console.WriteLine("微信托盘图标没找到！");
		return;
	}
	GUI.Mouse.Click(rectWXIcon.X, rectWXIcon.Y);
	win = GUI.Application.WaitForWindowByTitle("微信");
}
else
{
	win.Activate();
}

win.WaitAndClick(BitmapData.FromFile("Search.png"), 0.7);
GUI.Keyboard.Write("微信助手");
Thread.Sleep(1000);
GUI.Keyboard.Press(VirtualKeyCode.RETURN);
Thread.Sleep(1000);
GUI.Keyboard.Write("大家好！这是使用NetAutoGUI自动发送的消息。");
//win.WaitAndClick(BitmapData.FromFile("SendButton.png"), 0.7);
GUI.Keyboard.Press(VirtualKeyCode.RETURN);

Thread.Sleep(1000);


