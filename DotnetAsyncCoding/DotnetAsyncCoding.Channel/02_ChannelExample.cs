/// <summary>
/// 2. 将阻塞集合修改为 Channel，并仍然使用同步方法
/// </summary>
internal class _02_ChannelExample
{
    static void Main02()
    {
        var option = new BoundedChannelOptions(20)
        { 
            Capacity = 20,
            FullMode = BoundedChannelFullMode.Wait, // 当 Channel 满了之后等待（还有删掉最新/老/取出去的那个元素的模式）
            SingleReader = true,
            SingleWriter = true
        };
        //var channel = Channel.CreateUnbounded<Message>();// 创建没有边界的 Channel
        var channel = Channel.CreateBounded<Message>(option);// 创建有边界的 Channel

        var sendThread1 = new Thread(SendMessageThread);
        var receiveThread1 = new Thread(ReceiveMessageThread);

        sendThread1.Start(1);
        receiveThread1.Start(2);

        sendThread1.Join();
        // make sure all messages are received 
        Thread.Sleep(100);
        receiveThread1.Interrupt();
        receiveThread1.Join();

        Console.WriteLine("Press any key to exit ... ");

        void SendMessageThread(object? arg)
        {
            int id = (int)arg!;

            for (int i = 1; i <= 20; i++)
            {
                if (channel.Writer.TryWrite(new Message(id , i.ToString())))
                    Console.WriteLine($"Thread {id} send {i}");
                Thread.Sleep(100);
            }
        }

        void ReceiveMessageThread(object? id)
        {
            try
            {
                while (true)
                {
                    if (channel.Reader.TryRead(out var message))
                        Console.WriteLine($"Thread {id} received {message.Content} from {message.FromId}");
                    Thread.Sleep(1);
                }
            }
            catch (ThreadInterruptedException)
            {
                Console.WriteLine($"Thread {id} interrupted");
            }
        }
    }

    
}