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
    
    public partial class Room
    {
        public Room()
        {
            this.RoomInstances = new HashSet<RoomInstance>();
            this.Shifts = new HashSet<Shift>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string RoomNumber { get; set; }
        public int BuildingId { get; set; }
        public string PhoneNumber { get; set; }
    
        public virtual Building Building { get; set; }
        public virtual ICollection<RoomInstance> RoomInstances { get; set; }
        public virtual ICollection<Shift> Shifts { get; set; }
    }
}
