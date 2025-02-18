using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using pagadito_updater_service.Models;
using RestSharp;

namespace pagadito_updater_service
{
    class Program
    {
        public static IConfiguration Configuration;
        public static string[] PAGADITO_STATUSES = {"", "REGISTERED", "VERIFYING"};
        public static string[] PAGADITO_ENDED = {"REVOKED", "FAILED", "CANCELED", "EXPIRED", "COMPLETED"};
        static async Task Main(string[] args)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            while(true)
            {
                try
                {
                    await Check();
                    await Task.Delay(2000);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("ERROR: " + ex.Message);
                }
            }
        }

        public static async Task Check()
        {
            while(true)
            {
                using(var ctx = new UOnlineCTX())
                {
                    DateTime fecha_actual = DateTime.Now;
                    var result = await ctx.pel_ern_pagadito.Where(x=>PAGADITO_STATUSES.Contains(x.ern_status))
                                                           .OrderBy(x=>x.ern_fecha_verificacion).FirstOrDefaultAsync();
                    if(result == null)
                    {
                        break;
                    }
                    string url_pagadito = Configuration.GetSection("url_pagadito").Value;
                    url_pagadito = url_pagadito.Replace("{token}", result.ern_token).Replace("{ern}", result.ern_codigo);
                    RestClient client = new RestClient(url_pagadito);
                    RestRequest request = new RestRequest(Method.GET);
                    Console.WriteLine($"ENVIANDO...: {result.ern_codigo} ${result.ern_fecha_verificacion.ToString()}");
                    var response = await client.ExecuteAsync(request, new CancellationToken());
                    if(response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        PagaditoResponse apiResponse = JsonConvert.DeserializeObject<PagaditoResponse>(response.Content);
                        Console.WriteLine($"ERN: {result.ern_codigo} {apiResponse.Message}");
                        if(PAGADITO_ENDED.Contains(apiResponse.Message))
                        {
                            await Update(result.ern_codigo, result.ern_token);
                        }
                        result.ern_fecha_verificacion = DateTime.Now;
                        await ctx.SaveChangesAsync();
                    }
                    else
                    {
                        PagaditoResponse apiResponse = JsonConvert.DeserializeObject<PagaditoResponse>(response.Content);
                        Console.WriteLine($"ERN: {result.ern_codigo} {apiResponse.Message}");
                    }
                }
                await Task.Delay(5000);
            }
        }

        public static async Task Update(string ern, string token)
        {
            string url_update = Configuration.GetSection("url_update").Value;
            url_update = url_update.Replace("{token}", token).Replace("{ern}", ern);
            RestClient client = new RestClient(url_update);
            RestRequest request = new RestRequest(Method.GET);
            Console.WriteLine($"ENVIANDO...: ERN: {ern} Token: {token}");
            var response = await client.ExecuteAsync(request, new CancellationToken());
            if(response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"ERN: {ern} OK");
            }
            else
            {
                Console.WriteLine($"ERN: {ern} ERROR");
            }
        }
    }

    public class PagaditoResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
    }
}