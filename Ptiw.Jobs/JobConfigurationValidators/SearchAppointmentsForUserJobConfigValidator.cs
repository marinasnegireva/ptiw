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
    public class SearchAppointmentsForUserJobConfigValidator : AbstractValidator<SearchAppointmentsForUserJob>
    {
        public SearchAppointmentsForUserJobConfigValidator()
        {
            RuleFor(job => job.TaskConfiguration.UserId).NotNull().NotEmpty();

            RuleFor(job => job.TaskConfiguration.HourNoEarlierThen).GreaterThan(job => job.TaskConfiguration.HourNoLaterThen);

            RuleFor(job => job.TaskConfiguration.DoctorListAction).NotNull().When(job => job.TaskConfiguration.SpecificDoctorList != null && job.TaskConfiguration.SpecificDoctorList.Any());

        }
    }
}
