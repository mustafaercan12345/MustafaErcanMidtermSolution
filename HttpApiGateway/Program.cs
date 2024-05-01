using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace HttpApiGateway
{
    internal class Program
    {

        public static IReadOnlyDictionary<string, string> routingPathDictionary = (IReadOnlyDictionary<string, string>)new Dictionary<string, string> {
            {"/api/v1/Login","http://localhost:5077"},
            {"/api/v2/Login","http://localhost:5077"},
            {"/api/v1/Tuition","http://localhost:5077"},
            {"/api/v2/Tuition","http://localhost:5077"},
        };

        //buradaki her localhostu istediğimiz şekilde farklı farklı yapabilirdik. ama şimdilik göstermek için böyle yapıyorum.

        static void Main(string[] args)
        {
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost:8889/");

            httpListener.Start();

            while (true)
            {
                httpListener.GetContextAsync().ContinueWith(async (t) =>
                {
                    try
                    {
                        HttpListenerContext context = await t;
                        HttpListenerRequest request = context.Request;
                        NameValueCollection headers = request.Headers;
                        string path = request.Url.LocalPath;
                        string query = request.Url.Query;   
                        string body;

                        using (var streamReader = new StreamReader(request.InputStream))
                        {
                            body = streamReader.ReadToEnd();
                        }

                        if (request.HttpMethod == "OPTIONS")
                        {
                            context.Response.Headers.Clear();
                            context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                            context.Response.AppendHeader("Access-Control-Allow-Credentials", "true");
                            context.Response.AppendHeader("Access-Control-Allow-Methods", "GET,HEAD,OPTIONS,POST,PUT");
                            context.Response.AppendHeader("Access-Control-Allow-Headers", "Authorization, Origin,Accept, X-Requested-With, Content-Type, Access-Control-Request-Method, Access-Control-Request-Headers");
                            //corsa bu headerları ekliyerek izin veriyorum.
                            context.Response.StatusCode = (int)HttpStatusCode.OK;
                            context.Response.StatusDescription = "Status OK";

                            context.Response.Close();
                            return;
                        }


                        using (HttpClient client = new HttpClient()) 
                        using (HttpRequestMessage httpRequestMessage = new HttpRequestMessage(new HttpMethod(request.HttpMethod), routingPathDictionary[routingPathDictionary.Keys.First(s => path.StartsWith(s))] + path + query))
                        {
                            httpRequestMessage.Content = new StringContent(body, Encoding.UTF8, "application/json"); //burada bodyi
                            httpRequestMessage.Headers.Clear();
                            foreach (var header in headers.AllKeys)
                            {
                                httpRequestMessage.Headers.TryAddWithoutValidation(header, headers[header]); //burada headerları kopyalıyorum
                            }


                            using (HttpResponseMessage httpResponseMessage = await client.SendAsync(httpRequestMessage))//burada requesti atıp bekliyorum.
                            {

                                var responseBody = "";

                                using (var streamReader = new StreamReader(httpResponseMessage.Content.ReadAsStream()))
                                {
                                    responseBody = streamReader.ReadToEnd();
                                }

                                context.Response.Headers.Clear();
                                context.Response.StatusCode = (int)HttpStatusCode.OK;
                                context.Response.StatusDescription = "Status OK";
                                context.Response.AddHeader("Access-Control-Allow-Origin", "*");
                                context.Response.AddHeader("Access-Control-Allow-Credentials", "true");
                                context.Response.AddHeader("Access-Control-Allow-Methods", "GET,HEAD,OPTIONS,POST,PUT");
                                context.Response.AddHeader("Access-Control-Allow-Headers", "Access-Control-Allow-Headers, Origin,Accept, X-Requested-With, Content-Type, Access-Control-Request-Method, Access-Control-Request-Headers");
                                //corsa bu headerları ekliyerek izin veriyorum.

                                byte[] responseBuffer = Encoding.UTF8.GetBytes(responseBody);
                                context.Response.ContentLength64 = responseBuffer.Length;
                                context.Response.OutputStream.Write(responseBuffer, 0, responseBuffer.Length);
                                foreach (var item in httpResponseMessage.Headers)
                                {
                                    context.Response.Headers.Set((string)item.Key, item.Value.FirstOrDefault());
                                }

                                context.Response.OutputStream.Flush();

                                context.Response.Close();
                            }
                        }
                    }
                    catch (Exception e)
                    {

                        throw;
                    }


                });
            }

        }
    }
}
/*
 {
  "email": "admin@admin.com",
  "password": "admin"
}
 eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImFkbWluIiwiZXhwIjoxNzE0NTcyNDIxLCJpc3MiOiJtdXN0YWZhIGVyY2FuIiwiYXVkIjoicGVvcGxlIn0.OCH_PtP7u-RwlIRQsTevhXAcOCoCG1GQ_nJF1WxhJFY
 */