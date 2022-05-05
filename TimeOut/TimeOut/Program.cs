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

        TimeoutMethod();
    }
    static async Task TimeoutMethod1()
    {
        await tryMethod().WaitAsync(TimeSpan.FromSeconds(_timeout));

        Console.WriteLine("Do whatever next 1...");
        Thread.Sleep(1000);

        Console.WriteLine("Do whatever next 2...");
        Thread.Sleep(1000);

        Console.WriteLine("Do whatever next 3...");
        Thread.Sleep(1000);

        Console.WriteLine("Done!");
    }
    static async Task TimeoutMethod()
    {
        try
        {
            s_cts.CancelAfter(_timeout);

            var result = await tryMethodToken(s_cts.Token);
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
    static async Task<string> tryMethodToken(CancellationToken token)
    {
        for (int i = 0; i < 10; i++)
        {
            token.ThrowIfCancellationRequested();
            Console.Write($"{i}: Sleeping...");
            Thread.Sleep(2000);
            Console.WriteLine("  slept 2000 milliseconds");
        }
        return ">> Finshed the long lasting function :(";
    }
}