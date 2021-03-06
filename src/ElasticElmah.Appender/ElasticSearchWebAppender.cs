﻿using System;
using System.Security;
using System.Web;
using System.Linq;
using log4net.Core;
using System.Collections.Generic;

namespace ElasticElmah.Appender
{
    public class ElasticSearchWebAppender : ElasticSearchAppender
    {
        /// <summary>
        /// Add a log event to the ElasticSearch Repo
        /// </summary>
        /// <param name="loggingEvent"></param>
        protected override void Append(LoggingEvent loggingEvent)
        {
            if (HttpContext.Current != null)
            {
                AddHttpContextProperties(loggingEvent, new HttpContextWrapper(HttpContext.Current));
            }
            base.Append(loggingEvent);
        }

        public class ErrorCodeAndHtmlMessage
        {
            public int StatusCode { get; set; }
            public string WebHostHtmlMessage { get; set; }
            public ErrorCodeAndHtmlMessage(int statusCode, string webHostHtmlMessage)
            {
                StatusCode = statusCode;
                WebHostHtmlMessage = webHostHtmlMessage;
            }
            public ErrorCodeAndHtmlMessage()
            {
            }
        }

        public static void AddHttpContextProperties(LoggingEvent loggingEvent, HttpContextBase context)
        {
            var errors = new List<ErrorCodeAndHtmlMessage>();
            if (context != null && context.AllErrors != null)
            {
                for (int i = 0; i < context.AllErrors.Length; i++)
                {
                    var error = context.AllErrors[i];
                    if (error is HttpException)
                    {
                        var httpEx = error as HttpException;
                        var statusCode = httpEx.GetHttpCode();
                        var webHostHtmlMessage = TryGetHtmlErrorMessage(httpEx);
                        errors.Add(new ErrorCodeAndHtmlMessage(statusCode, webHostHtmlMessage));
                    }
                }
                if (errors.Any())
                    loggingEvent.Properties["httpErrors"] = errors;
            }
            if (context != null)
            {
                var request = context.Request;

                var _serverVariables = MapToJson(CopyCollection(request.ServerVariables));

                if (_serverVariables != null)
                {
                    // Hack for issue #140:
                    // http://code.google.com/p/elmah/issues/detail?id=140

                    const string authPasswordKey = "AUTHPASSWORD";
                    var authPassword = _serverVariables[authPasswordKey];
                    if (authPassword != null) // yes, mask empty too!
                        _serverVariables[authPasswordKey] = "*****";
                    loggingEvent.Properties["serverVariables"] = _serverVariables;
                }
                loggingEvent.Properties["queryString"] = CopyCollection(request.QueryString);
                loggingEvent.Properties["form"] = CopyCollection(request.Form);
                loggingEvent.Properties["cookies"] = CopyCollection(request.Cookies);
            }
        }

        private static Dictionary<string, object> MapToJson(Dictionary<string, object> dictionary)
        {
            return dictionary.ToDictionary(kv => JsonNameConvention(kv.Key), kv => kv.Value,
                StringComparer.InvariantCultureIgnoreCase);
        }

        private static string JsonNameConvention(string key)
        {
            return string.Join("", (key ?? string.Empty).ToLower().Split('_')
                .Select(k => FirstLetterToUpper(k)).ToArray());
        }

        private static string FirstLetterToUpper(string val)
        {
            if (val.Length > 0)
                return val.First().ToString().ToUpper() + (val.Length > 1 ? val.Substring(1) : string.Empty);
            return string.Empty;
        }

        private static Dictionary<string, object> CopyCollection(HttpCookieCollection httpCookieCollection)
        {
            var dic = new Dictionary<string, object>();
            if (httpCookieCollection != null)
                foreach (var key in httpCookieCollection.AllKeys)
                {
                    var httpCookie = httpCookieCollection[key];
                    if (httpCookie != null) dic[key] = httpCookie.Value;
                }
            return dic;
        }

        private static Dictionary<string, object> CopyCollection(System.Collections.Specialized.NameValueCollection nameValueCollection)
        {
            var dic = new Dictionary<string, object>();
            if (nameValueCollection!=null)
                foreach (var key in nameValueCollection.AllKeys.Where(key=>null!=key))
                {
                    dic[key] = nameValueCollection[key];
                }
            return dic;
        }

        private static string TryGetHtmlErrorMessage(HttpException e)
        {
            try
            {
                return e.GetHtmlErrorMessage();
            }
            catch (SecurityException)
            {
                // In partial trust environments, HttpException.GetHtmlErrorMessage() 
                // has been known to throw:
                // System.Security.SecurityException: Request for the 
                // permission of type 'System.Web.AspNetHostingPermission' failed.
                // 
                // See issue #179 for more background:
                // http://code.google.com/p/elmah/issues/detail?id=179

                //Trace.WriteLine(se);
                return null;
            }
        }

    }
}
