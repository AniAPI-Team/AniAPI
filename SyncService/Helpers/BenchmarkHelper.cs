using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncService.Helpers
{
    public static class BenchmarkHelper
    {
        private static ConcurrentDictionary<string, BenchmarkData> _data = new ConcurrentDictionary<string, BenchmarkData>();

        public static async Task Start(string id)
        {
            _data[id] = null;

            string dir = Path.Join(Environment.CurrentDirectory, "Benchmark");
            string path = Path.Join(dir, $"{id}.csv");

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            await File.AppendAllTextAsync(path, "Time;Op;Anime;Request (Kb);Response (ms);Err\n");
        }

        public static async Task Track(string id, BenchmarkData data)
        {
            _data[id] = data;

            await Benchmark(id);
        }

        public static async Task Benchmark(string id)
        {
            string path = Path.Join(Environment.CurrentDirectory, "Benchmark", $"{id}.csv");

            await File.AppendAllTextAsync(path, _data[id].ToBenchmarkText());
        }

        public class BenchmarkData
        {
            public string Anime { get; set; }
            public string Operation { get; set; }
            public double RequestSize { get; set; } = 0;
            public TimeSpan ResponseTime { get; set; } = TimeSpan.Zero;
            public bool Error { get; set; } = false;

            public string ToBenchmarkText()
            {
                return $"{DateTime.Now.ToString("hh:mm:ss.fff")};" +
                    $"{Operation};" +
                    $"{Anime};" +
                    $"{RequestSize};" +
                    $"{ResponseTime.TotalMilliseconds};" +
                    $"{(Error ? "X" : string.Empty)}" +
                    "\n";
            }
        }
    }
}