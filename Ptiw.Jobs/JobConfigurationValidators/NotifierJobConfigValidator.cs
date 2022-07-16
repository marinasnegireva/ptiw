using Microsoft.Extensions.Configuration;
using Ptiw.HostApp.Tasks.CheckNpcpnSchedule;
using Ptiw.HostApp.Tasks.NpcpnNotifier;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptiw.Libs.Validation.Validators.JobConfigurationValidators
{
    public class NotifierJobConfigValidator : AbstractValidator<NotifierJob>
    {
        public NotifierJobConfigValidator()
        {
            RuleFor(job => job.IsEnabled).NotNull();
        }
    }
}
