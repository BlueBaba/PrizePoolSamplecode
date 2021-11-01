using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using MassTransit;
using Enterprise.Data;
using System.Threading;
using Microsoft.Extensions.Logging;
using App.Interface;

namespace App.Entities
{
    public class IntraBankDrCr :IIntraBankDrCr         
    {

        private readonly ILogger<IntraBankDrCr> _logger;
        private readonly IUnitOfWork _uow;
        private readonly IBusControl _bus;
        private readonly IConfiguration _config;
        IDistributedCache _cache;

        public IntraBankDrCr(
               IConfiguration config,
            IBusControl bus,
             IUnitOfWork uow,
             ILogger<IntraBankDrCr> logger,
             IDistributedCache cache)
        {
            _uow = uow;
            _config = config;
            _logger = logger;
            _bus = bus;
            _cache = cache;            
        }

        public async Task<string> APICall(IntraHeader intraHeader, string url, object data, string qsparam, string token)
            {
                try

                {

                    using (var bankservice = new HttpClient())
                    {
                        bankservice.BaseAddress = new Uri(intraHeader.baseurl);
                        bankservice.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        bankservice.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        bankservice.Timeout = new System.TimeSpan(0, 0, 1, 0);
                        bankservice.DefaultRequestHeaders.Add("Signature", intraHeader.signature);
                        bankservice.DefaultRequestHeaders.Add("Reference", intraHeader.reference);
                        bankservice.DefaultRequestHeaders.Add("UniqueKey", intraHeader.uniqueKey);
                        bankservice.DefaultRequestHeaders.Add("HBAuthorization", intraHeader.HBAuthorization);
                        var json = JsonConvert.SerializeObject(data);
                        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    _logger.LogInformation("request json: " + json);
                    _logger.LogInformation("url: " + url);

                    var resp = (intraHeader.httpVerb.ToUpper() == "POST") ? bankservice.PostAsync(url + qsparam, stringContent).Result : bankservice.GetAsync(url + qsparam).Result;
                        var resp2 = await resp.Content.ReadAsStringAsync();
                    _logger.LogInformation("response from url resource: " + resp2);

                    if (resp.IsSuccessStatusCode)
                        {
                        return resp2; 
                    }
                    else
                        {

                        }

                        return resp2;
                    }
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {
                        HttpWebResponse response = (System.Net.HttpWebResponse)e.Response;

                        if (response.StatusCode == HttpStatusCode.NotFound)
                            return null;
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                            return null;
                        if (response.StatusCode == HttpStatusCode.Forbidden)
                            return null;
                        else
                            return null;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

        public async Task<string> APICallAuth(IntraHeader intraHeader, string url, List<KeyValuePair<string, string>> keyValues)
            {
                try

                {
                    using (var bankservice = new HttpClient())
                    {
                        bankservice.BaseAddress = new Uri(intraHeader.baseurltoken);
                        bankservice.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        bankservice.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(intraHeader.clientId + ":" + intraHeader.clientSecret)));
                        bankservice.Timeout = new System.TimeSpan(0, 0, 1, 0);
                        bankservice.DefaultRequestHeaders.Add("Reference", intraHeader.reference);
                        bankservice.DefaultRequestHeaders.Add("UniqueKey", intraHeader.uniqueKey);
                        bankservice.DefaultRequestHeaders.Add("HBAuthorization", intraHeader.HBAuthorization);
                        bankservice.DefaultRequestHeaders.Add("Signature", intraHeader.signature);

                        var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(keyValues) };
                        var resp = bankservice.SendAsync(req).Result;
                        var resp2 = await resp.Content.ReadAsStringAsync();
                        if (resp.IsSuccessStatusCode)
                        {
                            return resp2;
                        }                            
                    return null;
                    }
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.ProtocolError)
                    {

                        HttpWebResponse response = (System.Net.HttpWebResponse)e.Response;

                        if (response.StatusCode == HttpStatusCode.NotFound)
                            return null;
                        if (response.StatusCode == HttpStatusCode.Unauthorized)
                            return null;

                        if (response.StatusCode == HttpStatusCode.Forbidden)
                            return null;
                        else
                            return null;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }

        //}
    }
}
