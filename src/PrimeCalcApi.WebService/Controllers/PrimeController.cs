﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace PrimeCalcApi.WebService.Controllers
{
    [Route("[controller]")]
    public class PrimeController : Controller
    {
        private readonly IHttpContextAccessor httpContextAccessor; 
        private static readonly Random rnd = new Random();

        public PrimeController(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }
        
        [HttpGet]
        public IActionResult Get()
        {
            var elapsed = FakeCpuIntensiveWork(rnd.Next(10000, 300000), httpContextAccessor.HttpContext.RequestAborted);
            return Ok($"Calculating primes took {elapsed}");
        }

        [HttpGet("{n}")]
        public IActionResult Get(int n)
        {
            var elapsed = FakeCpuIntensiveWork(n, httpContextAccessor.HttpContext.RequestAborted);
            return Ok($"Calculating primes took {elapsed}");
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