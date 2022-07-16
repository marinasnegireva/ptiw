using Microsoft.Extensions.Configuration;
using Ptiw.HostApp.Tasks.CheckNpcpnSchedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ptiw.Libs.Validation.Validators.JobConfigurationValidators
{
    public class GetAppointmentsJobConfigValidator : AbstractValidator<GetAppointmentsJob>
    {
        public GetAppointmentsJobConfigValidator()
        {
            RuleFor(job => job.IsEnabled).NotNull();
            RuleFor(job => job.ClinicUri).NotNull().Must(uri=> !string.IsNullOrEmpty(uri.AbsoluteUri));
        }
    }
}
