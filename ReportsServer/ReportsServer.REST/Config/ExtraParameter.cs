using Newtonsoft.Json;
using System;
using ReportsServer.REST.Config.Interface;

namespace ReportsServer.REST.Config
{
    public class ExtraParameter: IExtraParameter
    {
        public string Value { get; set; }
        public string ValueType { get; set; }

        public Type GetValueType()
        {
            return Type.GetType(ValueType);
        } 

        public object GetValue()
        {
            return JsonConvert.DeserializeObject(Value, GetValueType());
        }

    }
}
