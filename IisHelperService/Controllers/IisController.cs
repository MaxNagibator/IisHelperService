using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Http;

using IisHelperService.Domain;

namespace IisHelperService.Controllers
{
    public class Result
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<string> Additional { get; set; }
    }

    public class CreateSiteRequest
    {
        public string PoolName { get; set; }
        public bool? PoolEnable32BitAppOnWin64 { get; set; }
        public string PoolManagedRuntimeVersion { get; set; }
        public string SiteName { get; set; }
        public string SiteUrl { get; set; }
        public string SiteFolder { get; set; }
    }

    public class CreateApplicationRequest
    {
        public string PoolName { get; set; }
        public bool? PoolEnable32BitAppOnWin64 { get; set; }
        public string PoolManagedRuntimeVersion { get; set; }
        public string SiteName { get; set; }
        public string AppPath { get; set; }
        public string PhisicalPath { get; set; }
    }

    public class DeleteSiteWithPullRequest
    {
        public string SiteName { get; set; }
    }

    public class PoolRequest
    {
        public string PoolName { get; set; }
    }

    public class CreatePoolRequest
    {
        public string PoolName { get; set; }
        public bool? PoolEnable32BitAppOnWin64 { get; set; }
        public string PoolManagedRuntimeVersion { get; set; }
    }

    public class GetPoolWithoutSitesRequest
    {
    }
    public class GetSiteNamesRequest
    {
        public string Search { get; set; }
    }

    [RoutePrefix("api/Iis")]
    public class IisController : ApiController
    {
        private Result Execute(Action<Action<string>> action)
        {
            var stb = new StringBuilder();
            Action<string> act = (x) => stb.AppendLine(x);
            try
            {
                action(act);
                return new Result { Success = true, Message = stb.ToString() };
            }
            catch (Exception ex)
            {
                act(ex.Message);
                act(ex.StackTrace);
                return new Result { Success = false, Message = stb.ToString() };
            }
        }

        [HttpPost]
        [Route("CreateSite")]
        public Result CreateSite([FromBody] CreateSiteRequest request)
        {
            return Execute((act) => IisManager.CreateSite(
                act,
                request.PoolName,
                request.PoolEnable32BitAppOnWin64,
                request.PoolManagedRuntimeVersion,
                request.SiteName,
                request.SiteUrl,
                request.SiteFolder));
        }

        [HttpPost]
        [Route("CreateApplication")]
        public Result CreateApplication([FromBody] CreateApplicationRequest request)
        {
            return Execute((act) => IisManager.CreateApplication(
                act,
                request.PoolName,
                request.PoolEnable32BitAppOnWin64,
                request.PoolManagedRuntimeVersion,
                request.SiteName,
                request.AppPath,
                request.PhisicalPath));
        }

        [HttpPost]
        [Route("DeleteSiteWithPull")]
        public Result DeleteSiteWithPull([FromBody] DeleteSiteWithPullRequest request)
        {
            return Execute((act) => IisManager.DeleteSiteWithPull(act, request.SiteName));
        }

        [HttpPost]
        [Route("CreatePool")]
        public Result CreatePool([FromBody] CreatePoolRequest request)
        {
            return Execute((act) => IisManager.CreatePool(
                act,
                request.PoolName,
                request.PoolEnable32BitAppOnWin64,
                request.PoolManagedRuntimeVersion));
        }

        [HttpPost]
        [Route("StartPool")]
        public Result StartPool([FromBody] PoolRequest request)
        {
            return Execute((act) => IisManager.StartPool(act, request.PoolName));
        }

        [HttpPost]
        [Route("StopPool")]
        public Result StopPool([FromBody] PoolRequest request)
        {
            return Execute((act) => IisManager.StopPool(act, request.PoolName));
        }

        [HttpPost]
        [Route("DeletePull")]
        public Result DeletePull([FromBody] PoolRequest request)
        {
            return Execute((act) => IisManager.DeletePull(act, request.PoolName));
        }

        [HttpPost]
        [Route("GetPoolWithoutSites")]
        public Result GetPoolWithoutSites([FromBody] GetPoolWithoutSitesRequest request)
        {
            List<string> test = null;
            var r = Execute((act) => test = IisManager.GetPoolWithoutSites(act));
            r.Additional = test;
            return r;
        }

        [HttpPost]
        [Route("GetSiteNames")]
        public Result GetSiteNames([FromBody] GetSiteNamesRequest request)
        {
            List<string> test = null;
            var r = Execute((act) => test = IisManager.GetSiteNames(act, request.Search));
            r.Additional = test;
            return r;
        }
    }
}
