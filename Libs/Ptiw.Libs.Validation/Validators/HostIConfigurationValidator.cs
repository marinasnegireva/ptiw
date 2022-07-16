using Microsoft.Extensions.Configuration;
using Ptiw.Libs.Common;

namespace Ptiw.Libs.Validation.Validators
{
    public class HostIConfigurationValidator : AbstractValidator<IConfiguration>
    {
        public HostIConfigurationValidator()
        {
            RuleFor(config => config).NotNull();

            RuleFor(config => config[Constants.SettingNames.TelegramToken]).NotNull().NotEmpty().Matches("[0-9]{9,10}:[a-zA-Z0-9_-]{35}")
                .WithMessage(Constants.SettingNames.TelegramToken + " is not a telegram token, current value: {PropertyValue}");

            RuleFor(config => config[Constants.SettingNames.AdminTelegramToken]).NotNull().NotEmpty().Matches("[0-9]{9,10}:[a-zA-Z0-9_-]{35}")
                .WithMessage(Constants.SettingNames.AdminTelegramToken + " is not a telegram token, current value: {PropertyValue}");

            RuleFor(config => config[Constants.SettingNames.AdminUserId]).NotNull().NotEmpty().Must(adminId => long.TryParse(adminId, out _))
                 .WithMessage(Constants.SettingNames.AdminUserId + " is not a valid telegram user id, current value: {PropertyValue}");

            RuleFor(config => config[Constants.SettingNames.ServiceContext]).NotNull().NotEmpty()
                .WithMessage($"{Constants.SettingNames.ServiceContext} is null or empty");
        }
    }
}