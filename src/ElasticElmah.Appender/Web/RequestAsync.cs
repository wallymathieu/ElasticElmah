﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace ElasticElmah.Appender.Web
{
    public class RequestAsync : IRequest
    {
        public Func<Tuple<HttpStatusCode, string>> Async(Uri uri, string method, string bytes)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri).Tap(r =>
            {
                Request(r, method, bytes);
            });

            var iar = request.BeginGetResponse(null, null);
            return () =>
            {
                iar.AsyncWaitHandle.WaitOne();
                return Response(request, iar);
            };
        }

        public void Async(Uri uri, string method, string bytes, Action<HttpStatusCode,string> onsuccess)
        {
            var request = (HttpWebRequest)WebRequest.Create(uri).Tap(r =>
            {
                Request(r, method, bytes);
            });

            request.BeginGetResponse(iar =>
            {
                var resp = Response(request, iar);
                onsuccess(resp.Item1,resp.Item2);
            }, null);
        }

        private static Tuple<HttpStatusCode, string> Response(HttpWebRequest request, IAsyncResult iar)
        {
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.EndGetResponse(iar);
                using (var rstream = response.GetResponseStream())
                using (var reader = new StreamReader(rstream, Encoding.UTF8))
                {
                    var c = reader.ReadToEnd();
                    //if (response.StatusCode != HttpStatusCode.OK&& response.st)
                    //{
                    //    throw new RequestException(response.StatusCode, c);
                    //}
                    //else
                    //{
                    return new Tuple<HttpStatusCode, string>(response.StatusCode,c);
                    //}
                }
            }
            finally { if (response != null) response.Close(); }
        }

        private static void Request(WebRequest r, string method, string data)
        {
            r.Method = method;
            r.ContentType = "application/json; charset=utf-8";
            if (!string.IsNullOrEmpty(data))
            {
                var bytes = Encoding.UTF8.GetBytes(data);
                using (var stream = r.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
        }
    }
}
