namespace Ptiw.Host.EventSystem
{
    public partial class ReactionManager
    {
        private List<Reaction> ClinicChain
        {
            get
            {
                var result = new List<Reaction>
                {
                    new Reaction
                    {
                    JobReactTo = typeof(Jobs.Clinic.GetAppointments.Job),
                    NeedChangesToHappenToFire = true,
                    Action = ClinicChain_Link_1_Action
                    },
                      new Reaction
                    {
                    JobReactTo = typeof(Jobs.Clinic.GetAppointments.Job),
                    NeedChangesToHappenToFire = false,
                    Action = ClinicChain_Link_1_Action_OnHostStart
                    },
                    new Reaction
                    {
                    JobReactTo = typeof(Jobs.Clinic.FindAppointmentsForUser.Job),
                    NeedChangesToHappenToFire = true,
                    Action = ClinicChain_Link_2_Action
                    },
                };
                return result;
            }
        }

        private async void ClinicChain_Link_1_Action()
        {
            var userConfigs =
                _serviceContext.TaskConfigs
                .Where(tk =>
                tk.Enabled &&
                tk.JobTypeFullName == typeof(Jobs.Clinic.GetAppointments.Job).FullName).ToList();

            foreach (var config in userConfigs)
            {
                var job = JobBuilder.Create<Jobs.Clinic.FindAppointmentsForUser.Job>()
                    .UsingJobData("TaskConfiguration", JsonConvert.SerializeObject(config))
                    .WithIdentity(config.BelongsToUser.ToString(), typeof(Jobs.Clinic.FindAppointmentsForUser.Job).FullName)
                    .Build();
                await _scheduler.AddJob(job, true);
                await _scheduler.TriggerJob(job.Key);
            }
        }

        private async void ClinicChain_Link_1_Action_OnHostStart()
        {
            if (!Jobs.Clinic.FindAppointmentsForUser.Starter.StartedOnce)
            {
                ClinicChain_Link_1_Action();
                Jobs.Clinic.FindAppointmentsForUser.Starter.StartedOnce = true;
            }
        }

        private async void ClinicChain_Link_2_Action()
        {
            await Jobs.Notifier.Starter.Start(_scheduler);
        }
    }
}