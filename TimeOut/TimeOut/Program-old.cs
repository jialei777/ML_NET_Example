using System;
using System.Threading;
using System.Threading.Tasks;

public class Program1
{
	static async Task Main1()
	{
		try {
			CancellationTokenSource cts = new CancellationTokenSource(2000);

			cts.Token.Register(() =>
				{
					Console.WriteLine(">>>> This would throw your Exception...");
					throw new TimeoutException();
				}
			);
			while (!cts.IsCancellationRequested)
			{
                var output = await tryMethod();
				//Console.WriteLine(output);
			}

		}
		catch (Exception ex)
        {
			Console.WriteLine("exception !!!!!!!!!!!!!!!!!!!!!");
		}

		Console.WriteLine("Do whatever next 1...");
		Thread.Sleep(1000);

		Console.WriteLine("Do whatever next 2...");
		Thread.Sleep(1000);

		Console.WriteLine("Do whatever next 3...");
		Thread.Sleep(1000);

		Console.WriteLine("Finished!");
	}

	public static async Task<int> tryMethod()
	{
		Console.Write("Sleeping...");
		Thread.Sleep(1500);
		Console.WriteLine("  slept 1500");
		return 1;
	}
}