using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Newtonsoft.Json;
using ReportsServer.Core;

namespace ReportsServer.Processor
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(ErrorHandlingMiddleware));

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context /* other dependencies */)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // todo: code for various exceptions
            var code = HttpStatusCode.BadRequest;

            Log.Error($"Exception: {exception.Message} {Environment.NewLine} " +
                $"url: {context.Request.GetDisplayUrl()}; headers: {context.Request.Headers.ToStr()}",
                exception);

            var result = JsonConvert.SerializeObject(new {status = "error", error = exception.Message});
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int) code;
            return context.Response.WriteAsync(result);
        }
    }
}