using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace ReportsServer.Core
{
    public static class EnumExtension
    {
        public static DisplayAttribute GetDisplayAttributesFrom(this System.Enum enumValue, Type enumType)
        {
            var member = enumType.GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DisplayAttribute>();

            if (member.ResourceType == null) return member;

            var resource = new ResourceManager(member.ResourceType);
            return new DisplayAttribute()
            {
                ShortName = string.IsNullOrEmpty(member.ShortName) ? null : resource.GetString(member.ShortName),
                Name = string.IsNullOrEmpty(member.Name) ? null : resource.GetString(member.Name),
                Prompt = string.IsNullOrEmpty(member.Prompt) ? null : resource.GetString(member.Prompt),
                Description = string.IsNullOrEmpty(member.Description) ? null : resource.GetString(member.Description),
            };
        }
    }
}