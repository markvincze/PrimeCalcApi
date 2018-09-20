using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PrimeCalcApi.LoadTest
{
    class Program
    {
        private static int requestPerMinute = 120;
        private const int maxRequestPerMinute = 800;
        private const int requestPerMinuteIncreaseStep = 60;
        private static readonly TimeSpan loadIncreaseInterval = TimeSpan.FromMinutes(5);

        private const string apiRootUrl = "http://[ENTER_ADDRESS_HERE]";

        private static readonly ConcurrentDictionary<Task, Task> tasksInProgress = new ConcurrentDictionary<Task, Task>();
        private static readonly HttpClient httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(5),
            BaseAddress = new Uri(apiRootUrl)
        };

        static async Task LoadTest()
        {
            var bgTask = Task.Run(async () =>
            {
                while (requestPerMinute <= maxRequestPerMinute)
                {
                    await Task.Delay(loadIncreaseInterval);

                    Console.WriteLine("Increasing the request per minute from {0} to {1}", requestPerMinute, requestPerMinute + requestPerMinuteIncreaseStep);
                    Interlocked.Add(ref requestPerMinute, requestPerMinuteIncreaseStep);
                }
            });

            while (requestPerMinute <= maxRequestPerMinute)
            {
                var task = SendPrimeRequest();

                tasksInProgress.AddOrUpdate(task, task, (t1, t2) => t2);

                task.ContinueWith((t) =>
                {
                    tasksInProgress.Remove(t, out Task _);
                });

                await Task.Delay(TimeSpan.FromMinutes(1) / requestPerMinute);
            }

            await bgTask;
        }

        static async Task SendPrimeRequest()
        {
            var result = await httpClient.GetAsync("/prime");

            if(!result.IsSuccessStatusCode)
            {
                Console.WriteLine("Request failed. Status: {0}, Body: {1}", result.StatusCode, await result.Content.ReadAsStringAsync());
            }
            //else
            //{
            //    Console.WriteLine("Request success. Status: {0}, Body: {1}", result.StatusCode, await result.Content.ReadAsStringAsync());
            //}
        }

        static void Main(string[] args)
        {
            LoadTest().Wait();
        }
    }
}
