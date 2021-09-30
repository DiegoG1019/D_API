using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace D_API.Enums
{
    [Flags]
    public enum Service
    {
        Users = 1 << 0,
        Authorization = 1 << 1,
        Data = 1 << 2,
    }

    public static class EnumExtensions
    {
        private readonly static Service[] ServiceValues = Enum.GetValues<Service>();
        public static IEnumerable<Service> Services => ServiceValues;

        private static string AuthRoles(Service service)
            => service switch
            {
                Service.Data => AuthorizationRoles.AppDataHost,
                _ => $"The service {service} does not have an attached Role or is not supported"
            };

        private static string UserRoles(Service service)
            => service switch
            {
                Service.Data => UserAccessRoles.AppDataHost,
                _ => $"The service {service} does not have an attached Role or is not supported"
            };

        public static string GetUserRoles(this Service service)
        {
            var sb = new List<string>(ServiceValues.Length);
            foreach (var s in ServiceValues)
                if (service.HasFlag(s))
                    sb.Add(UserRoles(s));
            return string.Join(",", sb);
        }

        public static string GetAuthenticationRoles(this Service service)
        {
            var sb = new List<string>(ServiceValues.Length);
            foreach (var s in ServiceValues)
                if (service.HasFlag(s))
                    sb.Add(AuthRoles(s));
            return string.Join(",", sb);
        }
    }
}
