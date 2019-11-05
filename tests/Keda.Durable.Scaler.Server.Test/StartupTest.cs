using System;
using Xunit;

namespace Keda.Durable.Scaler.Server.Test
{
    public class SetupTest
    {
        [Fact]
        public void ConfigrationSettingWithEnvrionmentVariables()
        {
            Environment.SetEnvironmentVariable(Startup.ConnectionString, "foo");
            Environment.SetEnvironmentVariable(Startup.TaskHub, "bar");
            Environment.SetEnvironmentVariable(Startup.MaxPollingIntervalMillisecond, "30000");
            var startup = new Startup();
            var context = startup.GetDurableTaskContext();
            Assert.Equal("foo", context.StorageAccount);
            Assert.Equal("bar", context.TaskHub);
            Assert.Equal(30000, context.MaxPollingIntervalMillisecond);
        }
    }
}
