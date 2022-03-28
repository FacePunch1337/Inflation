using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Inflation
{
    class Program
    {
        #region CallBack
        private static async Task<String> ReadFileAsync(string path)
        {

            return await Task.Run(() =>
            {
                var sb = new StringBuilder();
                using (var reader = new StreamReader(path))
                {
                    while (!reader.EndOfStream)
                    {
                        sb.AppendLine(reader.ReadLine());
                    }
                }
                return sb.ToString();
            });
        }

        private static async Task<dynamic> ParseIniAsync(string str)
        {
            return await Task.Run(() =>
            {
                if (str == null)
                    return null;
                var lines = str.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                var ret = new Dictionary<String, int>();
                foreach (var line in lines)
                {
                    var data = line.Split('=');
                    if (data.Length != 2) throw new Exception("Invalid ini format");
                    ret[data[0].Trim()] = int.Parse(data[1].Trim());
                }
                return ret;
            });
        }

        // HW
        private static dynamic Display(dynamic obj)
        {
            if (obj != null)
            {
                foreach (var item in obj)
                {
                    Console.WriteLine(item);
                }
                return obj;
            }
            return null;
        }
        static async Task Main(string[] args)
        {
            //String content = await ReadFileAsync("./data.ini");
            //var dic = await ParseIniAsync(content);
            var task = ReadFileAsync("data.ini").

                ContinueWith(task => ParseIniAsync(task.Result).Result).
                ContinueWith(task => Display(task.Result)).Result;
            
            var dic = await task;
            Console.WriteLine(dic["x"]);
        }
        #endregion

        #region Inflation
        private static String ChooseMonth(int monthNum)
        {
            switch (monthNum)
            {
                case 1: return "January";
                case 2: return "February";
                case 3: return "March";
                case 4: return "April";
                case 5: return "May";
                case 6: return "June";
                case 7: return "July";
                case 8: return "August";
                case 9: return "September";
                case 10: return "October";
                case 11: return "November";
                case 12: return "December";
            }
            return "Invalid month num";
        }
        private static float price;
        private static readonly object priceLocker = new object();

        private static async Task InflationYear()
        {
            var infList = new List<Task>();
            for (int i = 1; i <= 12; i++)
            {
                infList.Add(InflationCacl(i));
            }
            await Task.WhenAll(infList);
        }
        private static async Task InflationCacl(int month)
        {
            await Task.Delay(100);
            float startCopyPrice = price;
            float endCopyPrice;
            lock (priceLocker)
            {
                price *= (((float)(month + 10) / (float)100) + 1);
                endCopyPrice = price;
            }
            Console.WriteLine($"{ChooseMonth(month)}: {startCopyPrice} + {month + 10}% = {endCopyPrice}");
        }

        private static float InflationYearAsync(float price_async)
        {
            for (int i = 1; i <= 12; i++)
            {
                Thread.Sleep(100);
                Console.WriteLine($"{ChooseMonth(i)}: {price_async} + {i + 10}% = {price_async * (((float)(i + 10) / (float)100) + 1)}");
                price_async *= (((float)(i + 10) / (float)100) + 1);
            }
            return price_async;
        }

        static async Task Main_Inflation(string[] args)
        {
            Console.Write("Enter a start price: ");
            price = Convert.ToSingle(Console.ReadLine());
            Console.WriteLine("_____________________________");

            Console.WriteLine("Without Async:");
            Stopwatch sw = Stopwatch.StartNew();
            float price_async = InflationYearAsync(price);
            sw.Stop();
            Console.WriteLine($"Price by end of year: {Math.Round(price_async, 2)}. Duration: {sw.ElapsedMilliseconds}ms");

            Console.WriteLine("_____________________________");

            Console.WriteLine("With Async:");
            sw = Stopwatch.StartNew();
            await InflationYear();
            sw.Stop();
            Console.WriteLine($"Price by end of year: {Math.Round(price, 2)}. Duration: {sw.ElapsedMilliseconds}ms");

        }

    }
    #endregion
}
