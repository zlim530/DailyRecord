/*
C# 中 Channel 的基本用法:
1. 使用传统的线程（Thread）与阻塞集合（BlockingCollection）
BlockingCollection：自带信号量的 ConcurrentQueue（线程安全的集合），但会阻塞当前线程
BlockingCollection<T> 是一个自带阻塞功能的线程安全集合类，和 ConcurrentQueue<T> 有点像，不同的是，BlockingCollection<T> 自带阻塞功能。
Channel：可以用于异步编程的，类似于 BlockingCollection

var queue = new BlockingCollection<Message>(new ConcurrentQueue<Message>());

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
		queue.Add(new Message(id, i.ToString()));
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
            //Take 方法用于从集合中获取元素。当集合为空时，Take 方法将阻塞，直到获取到新元素。
            var message = queue.Take();
            Console.WriteLine($"Thread {id} received {message.Content} from {message.FromId}");
			Thread.Sleep(1);
        }
	}
	catch (ThreadInterruptedException)
	{
        Console.WriteLine($"Thread {id} interrupted");
    }
}
*/

record Message(int FromId, string Content);
