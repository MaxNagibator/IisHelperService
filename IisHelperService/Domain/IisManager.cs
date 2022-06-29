using Microsoft.Web.Administration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IisHelperService.Domain
{
    public static class IisManager
    {
        public static void CreateSite(
            Action<string> consoleWriteLine,
            string poolName,
            bool? poolEnable32BitAppOnWin64,
            string poolManagedRuntimeVersion,
            string siteName,
            string siteUrl,
            string siteFolder)
        {
            using (var serverManager = new ServerManager())
            {
                if (serverManager.ApplicationPools.Any(x => x.Name == poolName))
                {
                    consoleWriteLine("PULL EXISTS: " + poolName);
                }
                else
                {
                    ApplicationPool newPool = serverManager.ApplicationPools.Add(poolName);
                    newPool.Enable32BitAppOnWin64 = poolEnable32BitAppOnWin64 ?? false;
                    newPool.ManagedRuntimeVersion = poolManagedRuntimeVersion ?? "";
                    consoleWriteLine("TRY CREATE PULL: " + poolName + " enable32=" + newPool.Enable32BitAppOnWin64 + " clr=" + newPool.ManagedRuntimeVersion);
                }
                if (serverManager.Sites.Any(x => x.Name == siteName))
                {
                    consoleWriteLine("SITE EXISTS: " + siteName);
                }
                else
                {
                    var site = serverManager.Sites.Add(siteName, "http", siteUrl, siteFolder);
                    site.ApplicationDefaults.ApplicationPoolName = poolName;
                    site.ServerAutoStart = true;
                    consoleWriteLine("TRY CREATE SITE: " + siteName);
                }
                serverManager.CommitChanges();
                consoleWriteLine("SUCCESS!");
            }
        }

        public static void CreateApplication(
            Action<string> consoleWriteLine,
            string poolName,
            bool? poolEnable32BitAppOnWin64,
            string poolManagedRuntimeVersion,
            string siteName,
            string appPath,
            string phisicalPath)
        {
            using (var serverManager = new ServerManager())
            {
                if (serverManager.ApplicationPools.Any(x => x.Name == poolName))
                {
                    consoleWriteLine("PULL EXISTS: " + poolName);
                }
                else
                {
                    ApplicationPool newPool = serverManager.ApplicationPools.Add(poolName);
                    newPool.Enable32BitAppOnWin64 = poolEnable32BitAppOnWin64 ?? false;
                    newPool.ManagedRuntimeVersion = poolManagedRuntimeVersion ?? "";
                    consoleWriteLine("TRY CREATE PULL: " + poolName + " enable32=" + newPool.Enable32BitAppOnWin64 + " clr=" + newPool.ManagedRuntimeVersion);
                }

                var site = serverManager.Sites.FirstOrDefault(x => x.Name == siteName);
                if (site != null)
                {
                    if (site.Applications.Any(x => x.Path == appPath))
                    {
                        consoleWriteLine("APP EXISTS: " + appPath);
                    }
                    else
                    {
                        var app = site.Applications.Add(appPath, phisicalPath);
                        app.ApplicationPoolName = poolName;
                        consoleWriteLine("TRY CREATE APP: " + appPath);
                    }
                }
                else
                {
                    consoleWriteLine("SITE NOT EXISTS: " + siteName);
                }
                serverManager.CommitChanges();
                consoleWriteLine("SUCCESS!");
            }
        }

        public static void CreatePool(
            Action<string> consoleWriteLine, 
            string poolName,
            bool? poolEnable32BitAppOnWin64,
            string poolManagedRuntimeVersion)
        {
            using (var serverManager = new ServerManager())
            {
                var pool = serverManager.ApplicationPools.FirstOrDefault(x => x.Name == poolName);
                if (pool != null)
                {
                    consoleWriteLine("PULL EXISTS: " + poolName);
                }
                else
                {
                    ApplicationPool newPool = serverManager.ApplicationPools.Add(poolName);
                    newPool.Enable32BitAppOnWin64 = poolEnable32BitAppOnWin64 ?? false;
                    newPool.ManagedRuntimeVersion = poolManagedRuntimeVersion ?? "";
                    consoleWriteLine("TRY CREATE PULL: " + poolName + " enable32=" + newPool.Enable32BitAppOnWin64 + " clr=" + newPool.ManagedRuntimeVersion);
                    serverManager.CommitChanges();
                    consoleWriteLine("SUCCESS!");
                }
            }
        }

        public static void StartPool(Action<string> consoleWriteLine, string poolName)
        {
            using (var serverManager = new ServerManager())
            {
                var pool = serverManager.ApplicationPools.FirstOrDefault(x => x.Name == poolName);
                if (pool != null)
                {
                    if (pool.State != ObjectState.Stopped)
                    {
                        consoleWriteLine("PULL BAD STATE: " + pool.State);
                    }
                    else
                    {
                        consoleWriteLine("TRY PULL START " + poolName);
                        pool.Start();
                        consoleWriteLine("SUCCESS!");
                    }
                }
                else
                {
                    consoleWriteLine("PULL NOT EXISTS: " + poolName);
                }
            }
        }

        public static void StopPool(Action<string> consoleWriteLine, string poolName)
        {
            using (var serverManager = new ServerManager())
            {
                var pool = serverManager.ApplicationPools.FirstOrDefault(x => x.Name == poolName);
                if (pool != null)
                {
                    if (pool.State != ObjectState.Started)
                    {
                        consoleWriteLine("PULL BAD STATE: " + pool.State);
                    }
                    else
                    {
                        consoleWriteLine("TRY PULL STOP " + poolName);
                        pool.Stop();
                        consoleWriteLine("SUCCESS!");
                    }
                }
                else
                {
                    consoleWriteLine("PULL NOT EXISTS: " + poolName);
                }
            }
        }

        public static List<string> GetPoolWithoutSites(Action<string> consoleWriteLine)
        {
            using (var serverManager = new ServerManager())
            {
                var poolNames = serverManager.ApplicationPools.Select(x => x.Name).ToList();
                var sitesPoolsNames = serverManager.Sites.Select(x => x.Applications.First().ApplicationPoolName).ToList();
                poolNames = poolNames.Where(x => !sitesPoolsNames.Contains(x)).ToList();
                return poolNames;
            }
        }

        public static List<string> GetSiteNames(Action<string> consoleWriteLine, string search)
        {
            using (var serverManager = new ServerManager())
            {
                var poolNames = serverManager.Sites.Select(x => x.Name).ToList();
                if (!String.IsNullOrEmpty(search))
                {
                    poolNames = poolNames.Where(x => x.ToLower().Contains(search)).ToList();
                }
                return poolNames;
            }
        }

        public static void DeleteSiteWithPull(Action<string> consoleWriteLine, string siteName)
        {
            using (var serverManager = new ServerManager())
            {
                var site = serverManager.Sites.FirstOrDefault(x => x.Name == siteName);
                if (site == null)
                {
                    consoleWriteLine("SITE NOT EXISTS: " + siteName);
                }
                else
                {
                    var poolName = site.ApplicationDefaults.ApplicationPoolName;
                    consoleWriteLine("TRY SITE DELETE" + siteName);
                    serverManager.Sites.Remove(site);
                    var pool = serverManager.ApplicationPools.FirstOrDefault(x => x.Name == poolName);
                    if (pool == null)
                    {
                        consoleWriteLine("PULL NOT EXISTS: " + poolName);
                    }
                    else
                    {
                        consoleWriteLine("TRY PULL DELETE " + poolName);
                        serverManager.ApplicationPools.Remove(pool);
                    }
                    serverManager.CommitChanges();
                    consoleWriteLine("SUCCESS!");
                }
            }
        }

        public static void DeletePull(Action<string> consoleWriteLine, string poolName)
        {
            using (var serverManager = new ServerManager())
            {
                var pool = serverManager.ApplicationPools.FirstOrDefault(x => x.Name == poolName);
                if (pool == null)
                {
                    consoleWriteLine("PULL NOT EXISTS: " + poolName);
                }
                else
                {
                    consoleWriteLine("TRY PULL DELETE " + poolName);
                    serverManager.ApplicationPools.Remove(pool);
                }
                serverManager.CommitChanges();
                consoleWriteLine("SUCCESS!");
            }
        }
    }
}
