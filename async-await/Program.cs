for (int i = 0; i < 1000; i++)
{
    int localIndex = i;  // Create a local copy
    ThreadPool.QueueUserWorkItem(delegate
    {
        Console.WriteLine($"Starting a thread...{localIndex}");
        Thread.Sleep(1000);
    });
}

Console.WriteLine("Execution completed. Press any key to exit.");
Console.ReadLine();
