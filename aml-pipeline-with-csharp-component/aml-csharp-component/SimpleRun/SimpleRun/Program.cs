// See https://aka.ms/new-console-template for more information
Random rd = new Random();
int rand_num = rd.Next(100, 200); 

Console.WriteLine("********************************************************");
Console.WriteLine("\tHello World");
Thread.Sleep(5000);
Console.WriteLine("\t ... from Jialei!");
Console.WriteLine($"\tramdom number is {rand_num}");
Console.WriteLine("********************************************************");