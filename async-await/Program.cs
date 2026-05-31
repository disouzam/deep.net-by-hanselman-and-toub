using System.Collections.Concurrent;

AsyncLocal<int> myValue = new AsyncLocal<int>();
for(int i = 0; i < 1000; i++)
{
    myValue.Value = i;
    MyThreadPool.QueueUserWorkItem(delegate
    {
        Console.WriteLine($"Starting a thread...{myValue.Value}");
        Thread.Sleep(100);
    });
}

Console.WriteLine("Execution completed. Press any key to exit.");
Console.ReadLine();

class MyTask
{
    public bool IsComppleted { get; }

    public void SetResult() { }

    public void SetException(Exception ex) { }

    public void Wait() { }

    public void ContinueWith(Action action) { }
}

static class MyThreadPool
{
    private static readonly BlockingCollection<(Action, ExecutionContext?)> _workItems = new BlockingCollection<(Action, ExecutionContext?)>();

    public static void QueueUserWorkItem(Action action) => _workItems.Add((action, ExecutionContext.Capture()));

    static MyThreadPool()
    {
        for(int i = 0; i < Environment.ProcessorCount; i++)
        {
            new Thread(() =>
            {
                while(true)
                {
                    (Action workItem, ExecutionContext? context) = _workItems.Take();
                    if(context is null)
                    {
                        workItem();
                    }
                    else
                    {
                        ExecutionContext.Run(context, state => ((Action)state!).Invoke(), workItem);
                    }
                }
            })
            { IsBackground = true }.Start();
        }
    }
}