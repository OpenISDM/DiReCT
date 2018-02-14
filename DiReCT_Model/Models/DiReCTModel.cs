using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DiReCT.Record;

// Through Microsoft Entity Framework, 
// these class contains the functions which could insert the records
// to the corresponding table in the database.
// The objects in the class would be set as the field name in the table.
namespace DiReCT.Models
{
    public partial class DisasterInfo
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string DisasterName { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreateOn { get; set; }
    }

    // Record staff identity verification.
    // Reference from Microsoft.AspNet.Identity.Samples.
    #region Record staff
    public class SystemUser
    {
        [Key]
        [Required]
        public Guid Id { get; set; }

        [Required]
        public string Account { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public Guid CreateBy { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreateOn { get; set; }

        public Guid? UpdateBy { get; set; }

        public DateTime? UpdateOn { get; set; }

        public SystemUser()
        {
            this.Id = Guid.NewGuid();
            this.CreateBy = new Guid();

            this.SystemRoles = new List<SystemRole>();
        }

        // Realize the database association
        public ICollection<SystemRole> SystemRoles { get; set; }
    }

    public class SystemRole
    {
        [Key]
        [Required]
        public Guid ID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int Sort { get; set; }

        [Required]
        public Guid CreateBy { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTime CreateOn { get; set; }

        public Guid? UpdateBy { get; set; }

        public DateTime? UpdateOn { get; set; }

        public SystemRole()
        {
            this.ID = Guid.NewGuid();
            this.CreateBy = new Guid();

            this.SystemUsers = new List<SystemUser>();
        }

        // Realize the database association
        public ICollection<SystemUser> SystemUsers { get; set; }

    }
    #endregion

    #region Record table
    /// <summary>
    /// Debris flow records table on the database.
    /// </summary>
    public partial class Tbl_DebrisFlow : DebrisFlow, IRecordTable
    {
        // Associated with the Id of disaster information
        // TheDisasterInfo is defined below
        // public virtual DisasterInfo TheDisasterInfo { get; set; }
        [ForeignKey("TheDisasterInfo")]
        public Guid DisasterId { get; set; }

        // Associated with the Id of system user
        // TheSystemUser is defined below
        // public virtual SystemUser TheSystemUser { get; set; }
        [ForeignKey("TheSystemUser")]
        public Guid StaffId { get; set; }

        public virtual DisasterInfo TheDisasterInfo { get; set; }
        public virtual SystemUser TheSystemUser { get; set; }
    }

    /// <summary>
    ///  Flood records table on the database.
    /// </summary>
    public partial class Tbl_Flood : Flood, IRecordTable
    {
        [ForeignKey("TheDisasterInfo")]
        public Guid DisasterId { get; set; }
        [ForeignKey("TheSystemUser")]
        public Guid StaffId { get; set; }

        public virtual DisasterInfo TheDisasterInfo { get; set; }
        public virtual SystemUser TheSystemUser { get; set; }
    }

    /// <summary>
    /// General medical records table on the database.
    /// </summary>
    public partial class Tbl_GeneralMedicalRecord : 
        GeneralMedicalRecord, IRecordTable
    {
        [ForeignKey("TheDisasterInfo")]
        public Guid DisasterId { get; set; }
        [ForeignKey("TheSystemUser")]
        public Guid StaffId { get; set; }

        public virtual DisasterInfo TheDisasterInfo { get; set; }
        public virtual SystemUser TheSystemUser { get; set; }
    }

    /// <summary>
    /// General damage records table on the database.
    /// </summary>
    public partial class Tbl_GeneralDamageRecord :
        GeneralDamageRecord, IRecordTable
    {
        [ForeignKey("TheDisasterInfo")]
        public Guid DisasterId { get; set; }
        [ForeignKey("TheSystemUser")]
        public Guid StaffId { get; set; }

        public virtual DisasterInfo TheDisasterInfo { get; set; }
        public virtual SystemUser TheSystemUser { get; set; }
    }

    /// <summary>
    /// Total number Of people records table on the database.
    /// </summary>
    public partial class Tbl_TotalNumberOfPeople :
        TotalNumberOfPeople, IRecordTable
    {
        [ForeignKey("TheDisasterInfo")]
        public Guid DisasterId { get; set; }
        [ForeignKey("TheSystemUser")]
        public Guid StaffId { get; set; }

        public virtual DisasterInfo TheDisasterInfo { get; set; }
        public virtual SystemUser TheSystemUser { get; set; }
    }
    #endregion
}
