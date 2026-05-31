using System.Collections.Concurrent;

AsyncLocal<int> myValue = new AsyncLocal<int>();
for (int i = 0; i < 1000; i++)
{
    myValue.Value = i;
    ThreadPool.QueueUserWorkItem(delegate
    {
        Console.WriteLine($"Starting a thread...{myValue.Value}");
        Thread.Sleep(10);
    });
}

Console.WriteLine("Execution completed. Press any key to exit.");
Console.ReadLine();

static class MyThreadPool
{
    private static readonly BlockingCollection<Action> _workItems = new BlockingCollection<Action>();

    public static void QueueUserWorkItem(Action action) => _workItems.Add(action);

    static MyThreadPool()
    {
        for (int i = 0; i < Environment.ProcessorCount; i++)
        {
            new Thread(() => 
            {
                while (true) 
                {
                    Action workItem = _workItems.Take();
                    workItem();
                }
            }) 
            { IsBackground = true }.Start();
        }
    }
}