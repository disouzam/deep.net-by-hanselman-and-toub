using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

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
    private bool _completed;

    private Exception? _exception;

    private Action? _continuation;

    private ExecutionContext? _context;

    public bool IsCompleted
    {
        get
        {
            lock(this)
            {
                return _completed;
            }
        }
    }

    public void SetResult() => Complete(null);

    public void SetException(Exception exception) => Complete(exception);

    private void Complete(Exception? exception)
    {
        lock(this)
        {
            if(_completed)
            {
                throw new InvalidOperationException("Task is already completed.");
            }

            _completed = true;
            _exception = exception;

            if(_continuation is not null)
            {
                MyThreadPool.QueueUserWorkItem(delegate
                {
                    if(_context is null)
                    {
                        _continuation();
                    }
                    else
                    {
                        ExecutionContext.Run(_context, state => ((Action)state!).Invoke(), _continuation);
                    }
                }
                );
            }
        }
    }

    public void Wait()
    {
        ManualResetEventSlim? mres = null;

        lock(this)
        {
            if(!_completed)
            {
                mres = new ManualResetEventSlim();
                ContinueWith(mres.Set);
            }
        }

        mres?.Wait();

        if(_exception is not null)
        {
            ExceptionDispatchInfo.Throw(_exception);
            //throw new AggregateException(_exception);
        }
    }

    public void ContinueWith(Action action)
    {
        lock(this)
        {
            if(_completed)
            {
                MyThreadPool.QueueUserWorkItem(action);
            }
            else
            {
                _continuation = action;
                _context = ExecutionContext.Capture();
            }
        }
    }
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