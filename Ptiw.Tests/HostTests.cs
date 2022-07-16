using FluentValidation;
using Microsoft.Extensions.Hosting;
using Ptiw.Host;
using Ptiw.Libs.Validation.Validators;

namespace Ptiw.Tests
{
    public class HostTests
    {
        private IHostBuilder builder;

        public HostTests()
        {
            builder = HostHelper.GetBuilder();
            TestHelper.SetEnv();
        }

        [Fact]
        public void BuildHostTest()
        {
            builder.Build();
        }

        [Fact]
        public void ConfigTest()
        {
            var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var configValidator = new HostIConfigurationValidator();
            configValidator.ValidateAndThrow(config);
        }
    }
}