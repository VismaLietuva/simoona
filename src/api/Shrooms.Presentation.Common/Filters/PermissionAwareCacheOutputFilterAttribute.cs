using System;

namespace Shrooms.Presentation.Common.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class PermissionAwareCacheOutputFilterAttribute : Attribute
    {
        private readonly string[] _permissions;

        public int ServerTimeSpan { get; set; }

        public string[] Permissions => _permissions;

        public PermissionAwareCacheOutputFilterAttribute(params string[] permissions)
        {
            _permissions = permissions;
        }
    }
}
