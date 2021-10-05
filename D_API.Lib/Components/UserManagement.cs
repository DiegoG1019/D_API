using System;
using System.Threading.Tasks;

namespace D_API.Lib.Components
{
    public class UserManagement : Component
    {
        internal UserManagement(D_APIClient client) : base(client) { }

        [Obsolete("This method is not implemented yet. Calling this method will immediately result in a NotImplementedException throw")]
        public Task CreateUser() => throw new NotImplementedException();
    }
}
