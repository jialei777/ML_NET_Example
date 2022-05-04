using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

public class Example
{
    static readonly int _timeout = 4500;
    static readonly CancellationTokenSource s_cts = new CancellationTokenSource();

    static async Task Main()
    {
        Console.WriteLine($">> Want to timeout in {_timeout} milliseconds");
        // TimeoutMethod1();
        TimeoutMethod1();
        //TimeoutMethod2();
    }

    static async Task TimeoutMethod1()
    {
        var task = tryMethod();
        if (task == await Task.WhenAny(task, Task.Delay(_timeout)))
        {
            var result = await task;
            Console.WriteLine(result);
        }
        else
        {
            Console.WriteLine("\n>> timeout successfully\n");
            throw new TimeoutException();
        }

        Console.WriteLine("Do whatever next 1...");
        Thread.Sleep(1000);

        Console.WriteLine("Do whatever next 2...");
        Thread.Sleep(1000);

        Console.WriteLine("Do whatever next 3...");
        Thread.Sleep(1000);

        Console.WriteLine("Done!");
    }

    static async Task TimeoutMethod2()
    {
        try
        {
            s_cts.CancelAfter(_timeout);

            var result = await tryMethod();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("\n>> timeout successfully\n");
        }
        finally
        {
            s_cts.Dispose();
        }


        Console.WriteLine("Do whatever next 1...");
        Thread.Sleep(1000);

        Console.WriteLine("Do whatever next 2...");
        Thread.Sleep(1000);

        Console.WriteLine("Do whatever next 3...");
        Thread.Sleep(1000);

        Console.WriteLine("Done!");
    }
    static async Task<string> tryMethod()
    {
        for (int i = 0; i < 10; i++)
        {

            Console.Write($"{i}: Sleeping...");
            Thread.Sleep(2000);
            Console.WriteLine("  slept 2000 milliseconds");
        }
        return ">> Finshed the long lasting function :(";
    }
}