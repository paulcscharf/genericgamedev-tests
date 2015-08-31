using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace genericgamedev_tests
{
    static class TestHelper
    {
        public static void PrintTimes(params Tuple<string, List<TimeSpan>>[] timesLists)
        {
            PrintTimes((IEnumerable<Tuple<string, List<TimeSpan>>>)timesLists);    
        }

        public static void PrintTimes(IEnumerable<Tuple<string, List<TimeSpan>>> timesLists)
        {
            Console.Clear();

            foreach (var timesList in timesLists)
            {
                var name = timesList.Item1;
                var list = timesList.Item2;

                Console.WriteLine(name + " times (seconds):");
                var total = TimeSpan.Zero;
                foreach (var time in list)
                {
                    Console.Write(time.TotalSeconds);
                    Console.Write(" ");
                    total += time;
                }
                Console.WriteLine();
                var average = total.TotalSeconds / list.Count;
                Console.WriteLine("Average: " + average);

                Console.WriteLine();
            }
        }

        public static TimeSpan Measure(Action test)
        {
            var timer = Stopwatch.StartNew();

            test();

            return timer.Elapsed;
        }
    }
}