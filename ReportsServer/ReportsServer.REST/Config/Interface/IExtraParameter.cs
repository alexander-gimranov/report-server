using System;

namespace ReportsServer.REST.Config.Interface
{
    public interface IExtraParameter
    {
        string Value { get; set; }
        string ValueType { get; set; }
        Type GetValueType();
        object GetValue();
    }
}