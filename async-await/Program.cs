using System.Collections.Concurrent;

for (int i = 0; i < 1000; i++)
{
    int localIndex = i;  // Create a local copy
    MyThreadPool.QueueUserWorkItem(delegate
    {
        Console.WriteLine($"Starting a thread...{localIndex}");
        Thread.Sleep(1000);
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