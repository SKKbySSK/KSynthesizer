using System;

namespace TestTool.Platform
{
    class Program
    {
        static void Main(string[] args)
        {
            var player = new FunctionPlayer();
			player.Source.Function = KSynthesizer.Sources.FunctionType.Sin;
			player.Source.SetFrequency(440);
            Console.WriteLine("Backend : " + player.Backend);
            player.Start();

			for (; ; )
			{
				var line = Console.ReadLine();
				if (line == "q")
				{
					break;
				}
				else if (float.TryParse(line, out var freq))
				{
					player.Source.SetFrequency(freq);
				}
			}

			player.Dispose();
		}
    }
}
