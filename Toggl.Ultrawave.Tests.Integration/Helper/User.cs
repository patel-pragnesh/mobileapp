using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using Toggl.Ultrawave.Network;
using Toggl.Ultrawave.Tests.Integration.Helper;

namespace Toggl.Ultrawave.Tests.Integration
{
    internal static class User
    {
        public static async Task<Credentials> Create()
        {
            var (email, password) = await CreateEmailPassword();

            return Credentials.WithPassword(email, password);
        }

        public static async Task<(Email email, Password password)> CreateEmailPassword()
        {
            var email = $"{Guid.NewGuid()}@mocks.toggl.com".ToEmail();
            var password = "123456".ToPassword();

            var api = new TogglApi(new ApiConfiguration(ApiEnvironment.Staging, Credentials.None, Configuration.UserAgent));
            await api.User.SignUp(email, password, true, 237);

            return (email, password);
        }
    }
}
