using System.ComponentModel.DataAnnotations;

namespace ReportsServer.Core
{
    public enum ReportNames
    {
        [Display(Name = "wt_r_attendance_records_report")]
        AttendenceRecords = 1,
        Test = 2,
    }
}