﻿using System;
using System.Web.Mvc;
using ElasticElmah.Core;
using ElasticElmah.Core.ErrorLog;
using ElasticElmahMVC.Code;
using ElasticElmahMVC.Models;
using Environment = ElasticElmahMVC.Code.Environment;
using System.Threading.Tasks;
using log4net.Core;
using System.Reflection;

namespace ElasticElmahMVC.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index(int? size = null, int? page = null)
        {
            const int _defaultPageSize = 15;
            const int _maximumPageSize = 100;

            int _pageSize = Math.Min(_maximumPageSize, Math.Max(0, size ?? 0));

            if (_pageSize == 0)
            {
                _pageSize = _defaultPageSize;
            }

            int _pageIndex = Math.Max(1, page ?? 0) - 1;
            var errorlog = Helper.GetDefault(HttpContext);
            var env = new Environment(HttpContext);
            ViewBag.ErrorLog = errorlog;
            var errors = errorlog.GetErrors(_pageIndex, _pageSize);
            return View(new ErrorLogPage(env, errors, _pageIndex, _pageSize).OnLoad());
        }

        public ActionResult About()
        {
            ViewBag.ErrorLog = Helper.GetDefault(HttpContext);
            return View(new AboutModel(new Environment(HttpContext)));
        }

        public ActionResult Test()
        {
            throw new TestException();
        }

        public ActionResult TestRaw()
        {
            var log = LoggerManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType.Assembly,
    MethodBase.GetCurrentMethod().DeclaringType.FullName);
            var loggingevent = new LoggingEvent(new LoggingEventData { Message="msg", Level=Level.Error, LocationInfo=new LocationInfo("test","test","file.cs","12") });
            log.Log(loggingevent);
            return Content("test");
        }
    }
}