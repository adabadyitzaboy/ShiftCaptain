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
    
    public partial class CantWorkViolation_Result
    {
        public Nullable<int> PreferenceId { get; set; }
        public Nullable<int> ShiftId { get; set; }
        public string NickName { get; set; }
        public Nullable<int> PreferenceDay { get; set; }
        public Nullable<System.TimeSpan> PreferenceTime { get; set; }
        public Nullable<decimal> PreferenceDuration { get; set; }
        public string ShiftRoom { get; set; }
        public Nullable<int> ShiftDay { get; set; }
        public Nullable<System.TimeSpan> ShiftTime { get; set; }
        public Nullable<decimal> ShiftDuration { get; set; }
    }
}