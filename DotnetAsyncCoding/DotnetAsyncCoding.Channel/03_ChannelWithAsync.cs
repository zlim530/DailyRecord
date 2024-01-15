
internal class _03_ChannelWithAsync
{
    static async Task Main(string[] args)
    {
        var channel = Channel.CreateUnbounded<Message>();// 创建没有边界的 Channel

        var sender = SendMessageAsync(channel.Writer, 1);
        var sender2 = SendMessageAsync(channel.Writer, 2);

        // 3.使用异步方法，并使用 CancellationToken 来取消接收任务
        // using var cts = new CancellationTokenSource();
        // var receiver = ReceiveMessageAsync03(channel.Reader, 2, cts.Token);
        // 4. 使用 ChannelWriter.Complete 方法取代 CancellationToken
        //var receiver = ReceiveMessageAsync04(channel.Reader, 2);
        // 5. 使用 await foreach，即 IAsyncEnumerable 接口取代对于 Completed 状态的观察及 ChannelClosedException 异常的捕获
        var receiver = ReceiveMessageAsync(channel.Reader, 2);
        var receiver2 = ReceiveMessageAsync(channel.Reader, 3);

        //await sender;
        //await Task.Delay(100);
        await Task.WhenAll(sender, sender2); // 当有多个生产者时可以使用 WhenAll 来确保所有生产者都运行完成
        // 3.使用异步方法，并使用 CancellationToken 来取消接收任务
        //cts.Cancel();
        // 4. 使用 ChannelWriter.Complete 方法取代 CancellationToken
        // make sure all messages are received
        channel.Writer.Complete();
        //await receiver;
        await Task.WhenAll(receiver, receiver2);

        Console.WriteLine("Press any key to exit ... ");
    }


    static async Task SendMessageAsync(ChannelWriter<Message> writer, int id)
    { 
        for (int i = 1; i <= 20; i++) 
        {
            await writer.WriteAsync(new Message(id, i.ToString()));
            Console.WriteLine($"Thread {id} send {i}");
            await Task.Delay(100);
        }
        // we don't complete the writer here since there may be more than one senders
        // writer.Complete();// 如果只有一个生产者可以在确认生产者完成就调用 Complete() 方法
    }

    static async Task ReceiveMessageAsync03(ChannelReader<Message> reader, int id, CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                var message = await reader.ReadAsync(token);
                Console.WriteLine($"Thread {id} received {message.Content} from {message.FromId}");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"Thread {id} task cancelled.");
        }
    }

    static async Task ReceiveMessageAsync04(ChannelReader<Message> reader, int id)
    {
        try
        {
            while (!reader.Completion.IsCompleted)// 当 ChannelWriter 标记完成且 ChannelReader 读取完信道里的所有数据后才会被触发
            {
                var message = await reader.ReadAsync();
                Console.WriteLine($"Thread {id} received {message.Content} from {message.FromId}");
            }
        }
        catch (ChannelClosedException)
        {
            Console.WriteLine($"Thread {id} channel closed.");
        }
    }

    static async Task ReceiveMessageAsync(ChannelReader<Message> reader, int id)
    {
        // IAsyncEnumerable<Message>：ReadAllAsync() 的返回值，是 C#8.0 之后才有的
        // 使用 IAsyncEnumerable 连 try-catch 异常捕获都不需要了，IAsyncEnumerable 会智能处理
        await foreach (var message in reader.ReadAllAsync())
        {
            Console.WriteLine($"Thread {id} received {message.Content} from {message.FromId}");
        }
    }
}
