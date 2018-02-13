using System;

namespace DiReCT.Models
{
    interface IRecordTable
    {
        Guid DisasterId { get; set; }
        Guid StaffId { get; set; }
    }
}
