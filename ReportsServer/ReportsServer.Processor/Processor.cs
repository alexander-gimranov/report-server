using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using ReportsServer.Core;
using ReportsServer.FileModule;
using ReportsServer.Processor.Helpers;
using ReportsServer.REST;
using ReportsServer.REST.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReportsServer.Processor
{
    public class Processor : IProcessor
    {
        private readonly IDownloadHelper _downloadHelper;
        private readonly log4net.ILog Log;
        private readonly ConnectionsConfig _config;
        private readonly RestServices _rest;
        private readonly IRestService _aclService;
        private readonly IRestService _rVisionService;

        public Processor(log4net.ILog log, string configDirectory)
        {
            _downloadHelper = new DownloadHelper();
            Log = log;
            _config = CreateHelper.Create<ConnectionsConfig>(Path.Combine(configDirectory, "connections.config.json"));
            _rest = new RestServices(Path.Combine(configDirectory, "rest.services.config.json"));
            _aclService = _rest.Service("ACL");
            _rVisionService = _rest.Service("rVision");
        }


        public string GetReports()
        {
            IReadOnlyDictionary<string, object> param = new Dictionary<string, object>()
            {
                {"startDate", (object) "date"},
                {"endDate", (object) "date"},
            };
            var list = Enum.GetValues(typeof(ReportNames))
                .Cast<ReportNames>()
                .Select(value => new Report()
                {
                    Id = (int) value,
                    Name = value,
                    Params = param
                })
                .ToList();

            return GetJson(list);
        }

        public string GetJson(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private void AddFieldToCollections(ref List<Dictionary<string, object>> collection, IReadOnlyDictionary<string, string> cookies, string keyField, string newField)
        {
            if (collection == null || !collection.Any() || !(collection.First().TryGetValue(keyField, out var keyValue) && (keyValue != null))) return;

            var tasks = new List<Task>();
            var semaphore = new SemaphoreSlim(initialCount: 50, maxCount: 50);
            IDictionary<string, Dictionary<string, object>> ids = new Dictionary<string, Dictionary<string, object>>();
            var cntInOneRequest = _rVisionService.GetParameterValue<int>("CountInOneRequest");
            if (cntInOneRequest == -1) cntInOneRequest = 50;
            foreach (var row in collection)
            {
                if (!(row.TryGetValue(keyField, out keyValue) && (keyValue != null))) continue;
                var localRow = row;
                localRow.Add(newField, null);
                if (ids.Count <  cntInOneRequest)
                {
                    if (ids.Keys.Contains(keyValue.ToString()))
                    {
                        Log.Error(keyValue + " contains");
                        continue;
                    }
                    ids.Add(keyValue.ToString(), localRow);
                    continue;
                }
                var localIds = ids.ToDictionary(k => k.Key, v => v.Value);
                ids.Clear();
                
                tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await semaphore.WaitAsync();
                            await _rVisionService.FillFieldFormRest(cookies, localIds, newField);
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }
                ));
            }

            Task.WaitAll(tasks.ToArray());
        }

        protected virtual void FillPrepareReport(IPrepareReport report, IReadOnlyDictionary<string, string> cookies = null)
        {
            switch (report.Name)
            {
                case ReportNames.AttendenceRecords:
                    if (_config.Connections.TryGetValue(DAL.DatabaseType.MSSQL, out var connection))
                    {
                        var dalHelper = new DAL.CommonHelper(DAL.DatabaseType.MSSQL, connection.ToString());
                        report.Template = "c:\\Template.xlsx";
                        using (var conn = dalHelper.GetConnection())
                        {
                            var collection =
                                dalHelper.GetData(conn, "exec [dbo].[PR_RE_AttendanceRecords]", report.Params) as
                                    IReadOnlyCollection<IReadOnlyDictionary<string, object>>;
                            var objects = collection.Select(i => i as Dictionary<string, object>).ToList();
                            AddFieldToCollections(ref objects, cookies, "UID", "EmployeePosition");
                            var lManagment = new List<Dictionary<string, object>>();
                            var lOthers = new List<Dictionary<string, object>>();
                            foreach (var row in objects)
                            {
                                if (row.TryGetValue("IsManagement", out var magament))
                                {
                                    if (magament.Equals(1))
                                    {
                                        lManagment.Add(row);
                                    }
                                    else
                                    {
                                        lOthers.Add(row);
                                    }
                                }
                            }
                            report.Collections = new[]
                            {
                                lManagment,
                                lOthers
                            };
                        }
                    }

                    break;
                case ReportNames.Test:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual IPrepareReport GetPrepareReport(string json)
        {
            return JsonConvert.DeserializeObject<IPrepareReport>(json);
        }

        public FileContentResult GenerateReportFile(string json, FileTypes fileType)
        {
            var report = GetPrepareReport(json);
            return GenerateReportFile(report, fileType);
        }

        public FileContentResult GenerateReportFile(string json, string fileType)
        {
            var type = JsonConvert.DeserializeObject<FileTypes>(fileType);
            return GenerateReportFile(json, type);
        }

        public FileContentResult GenerateReportFile(IPrepareReport report, FileTypes fileType)
        {
            FillPrepareReport(report);
            var fileProcessor = new FileProcessor(fileType);
            var file = fileProcessor.GenerateFromReport(report);
            return _downloadHelper.GetFileContentResult(file, "download");
        }

        public bool TryGenerateReportFile(IRequestReport report, FileTypes fileType, out DownloadFile file)
        {
            try
            {
                var rep = new PrepareReport(report);
                FillPrepareReport(rep, report.Cookies);
                var fileProcessor = new FileProcessor(fileType);
                var path = fileProcessor.GenerateFromReport(rep);
                file = _downloadHelper.GetFileInfo(path);
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e);
                file = null;
                return false;
            }
        }

        public async Task<DownloadFile> GenerateReportFile(IRequestReport report, FileTypes fileType)
        {
            return await Task.Run(() =>
                {
                    try
                    {
                        if (!HasPermissions(report)) throw new AccessViolationException();
                        Log.Debug(JsonConvert.SerializeObject(report) + " -- START --");
                        var rep = new PrepareReport(report);
                        FillPrepareReport(rep, report.Cookies);
                        var fileProcessor = new FileProcessor(fileType);
                        var path = fileProcessor.GenerateFromReport(rep);
                        Log.Debug(JsonConvert.SerializeObject(report) + " -- END --");
                        return _downloadHelper.GetFileInfo(path);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        return null;
                    }
                }
            );
        }

        private bool HasPermissions(IRequestReport report)
        {
            var permissions = _aclService?.GetUserPermissionsAsync(report.Cookies).Result;
            if (permissions == null || permissions.Count == 0) return false;
            return permissions.Contains(report.Name.GetDisplayAttributesFrom(typeof(ReportNames)).Name);
        }

        public async Task<string> GenerateReportHtml(IRequestReport report, 
            ICompositeViewEngine viewEngine, ITempDataDictionary tempData, 
            ActionContext actionContext, ViewDataDictionary viewData)
        {
            return await Task.Run(async () =>
                {
                    try
                    {
                        if (!HasPermissions(report)) throw new AccessViolationException();
                        var rep = new PrepareReport(report);
                        FillPrepareReport(rep, report.Cookies);
                        var fileProcessor = new FileProcessor(viewEngine, tempData, actionContext, viewData);
                        return await fileProcessor.GenerateFromReportAsync(rep);
                    }
                    catch (Exception e)
                    {
                        Log.Error(e);
                        return null;
                    }
                }
            );

        }

        public IPrepareReport GetFillPrepareReport(IRequestReport report)
        {
            try
            {
                if (!HasPermissions(report)) throw new AccessViolationException();
                var rep = new PrepareReport(report);
                FillPrepareReport(rep, report.Cookies);
                return rep;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }
    }
}