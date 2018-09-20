using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace PrimeCalcApi.WebService.Controllers
{
    [Route("[controller]")]
    public class PrimeController : Controller
    {
        private static readonly Random rnd = new Random();
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IOptions<PrimeOptions> primeOptions;

        public PrimeController(IHttpContextAccessor httpContextAccessor, IOptions<PrimeOptions> primeOptions)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.primeOptions = primeOptions;
        }

        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                Startup.PrimeRequestInFlightMetric.Inc(1);
                var elapsed = FakeCpuIntensiveWork(rnd.Next(10000, primeOptions.Value.MaxValue), httpContextAccessor.HttpContext.RequestAborted);
                return Ok($"Calculating primes took {elapsed}");
            }
            finally
            {
                Startup.PrimeRequestInFlightMetric.Dec(1);
            }
        }

        [HttpGet("{n}")]
        public IActionResult Get(int n)
        {
            try
            {
                Startup.PrimeRequestInFlightMetric.Inc(1);
                var elapsed = FakeCpuIntensiveWork(n, httpContextAccessor.HttpContext.RequestAborted);
                return Ok($"Calculating primes took {elapsed}");
            }
            finally
            {
                Startup.PrimeRequestInFlightMetric.Dec(1);
            }
        }

        private static TimeSpan FakeCpuIntensiveWork(int n) => FakeCpuIntensiveWork(n, CancellationToken.None);

        private static TimeSpan FakeCpuIntensiveWork(int n, CancellationToken cts)
        {
            var sw = Stopwatch.StartNew();

            int count = 0;
            long a = 2;
            while (count < n)
            {
                cts.ThrowIfCancellationRequested();
                long b = 2;
                int prime = 1; // to check if found a prime
                while (b * b <= a)
                {
                    if (a % b == 0)
                    {
                        prime = 0;
                        break;
                    }
                    b++;
                }
                if (prime > 0)
                {
                    count++;
                }
                a++;
            }

            sw.Stop();
            return sw.Elapsed;
        }
    }
}
