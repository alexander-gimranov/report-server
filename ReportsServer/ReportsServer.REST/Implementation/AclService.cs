using System;
using ReportsServer.REST.Classes;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReportsServer.REST.Implementation
{
    internal sealed class AclService : RestService
    {
        public override async Task<List<string>> GetUserPermissionsAsync(IReadOnlyDictionary<string, string> cookies)
        {
            var list = ConverToNetCookies(cookies);
            var aclResult = await GetContentAsResultAsync<AclSuccessResult>(HttpMethod.Get, list,
                "get-privileges", null, true);
            return aclResult.Success ? aclResult.Result.Permissions : null;
        }
    }
}