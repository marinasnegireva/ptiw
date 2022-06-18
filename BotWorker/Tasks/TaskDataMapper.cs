using Ptiw.DataAccess.Tables;

namespace Ptiw.HostApp.Tasks
{
    internal static class TaskDataMapper
    {
        internal static NotificationData MapToNotificationData(this FindAppointmentTaskData npcpnTaskData)
        {
            return new NotificationData
            {
                AppointmentTime = npcpnTaskData.AppointmentTime,
                AppointmentDayOfWeek = npcpnTaskData.AppointmentDayOfWeek,
                AppointmentDate = npcpnTaskData.AppointmentDate,
                DoctorName = npcpnTaskData.DoctorName
            };
        }
    }
}