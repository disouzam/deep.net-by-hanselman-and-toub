for (int i = 0; i<1000; i++)
{
    ThreadPool.QueueUserWorkItem(delegate
    {
        Console.WriteLine($"Starting a thread...{i}");
        Thread.Sleep( 1000 );
    });
}

Console.WriteLine("Execution completed. Press any key to exit.");
Console.ReadLine();
