//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ShiftCaptain.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserInstance
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int VersionId { get; set; }
        public System.TimeSpan MinHours { get; set; }
        public System.TimeSpan MaxHours { get; set; }
        public decimal CurrentHours { get; set; }
    
        public virtual User User { get; set; }
        public virtual Version Version { get; set; }
    }
}
