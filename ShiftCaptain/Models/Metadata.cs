using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ShiftCaptain.Models
{
    public class MinValueAttribute : ValidationAttribute
    {
        private readonly int _minValue;

        public MinValueAttribute(int minValue)
        {
            _minValue = minValue;
        }

        public override bool IsValid(object value)
        {
            return (int)value >= _minValue;
        }
    }

    [Bind(Exclude = "Locked,LastLogin,Pass,CurrentHours")]
    public class UserMetadata
    {
        [Required]
        [StringLength(500)]
        [EmailAddress]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string EmailAddress;

        [DataType(DataType.Password)]
        public string Pass;
        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FName { get; set; }
        [StringLength(100)]
        [Display(Name = "Middle Name")]
        public string MName { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LName { get; set; }
        [Required]
        [StringLength(100)]
        [Display(Name = "Nick Name")]
        public string NickName { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Employee Id")]
        public string EmployeeId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Shift Manager")]
        public bool IsShiftManager { get; set; }

        [Required]
        [Display(Name = "Manager")]
        public bool IsManager { get; set; }


        [Display(Name = "Last Login")]
        public DateTime? LastLogin { get; set; }


        [Required]
        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        [Required]
        [Display(Name = "Male?")]
        public bool IsMale { get; set; }

        [Display(Name = "Min Hours")]
        [DisplayFormat(DataFormatString = "{0:0.0}", ApplyFormatInEditMode = true)]
        public Decimal? MinHours { get; set; }

        [Display(Name = "Max Hours")]
        [DisplayFormat(DataFormatString = "{0:0.0}", ApplyFormatInEditMode = true)]
        public Decimal? MaxHours { get; set; }

        [Display(Name = "Current Hours")]
        [DisplayFormat(DataFormatString = "{0:0.0}", ApplyFormatInEditMode = true)]
        public Decimal? CurrentHours { get; set; }
    }

    public class UserViewMetadata : UserMetadata
    {
        [StringLength(200)]
        [Display(Name = "Line 1")]
        public string Line1;

        [StringLength(200)]
        [Display(Name = "Line 2")]
        public string Line2;

        [StringLength(200)]
        public string City;

        [StringLength(200)]
        public string State;

        [StringLength(9)]
        public string ZipCode;

        [StringLength(200)]
        public string Country;


    }

    public class BuildlingMetadata
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Building Name")]
        public string Name { get; set; }
        
        [StringLength(100)]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Manager Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string ManagerPhone { get; set; }

    }

    public class BuildingViewMetadata : BuildlingMetadata
    {
        [StringLength(200)]
        [Display(Name = "Line 1")]
        public string Line1;

        [StringLength(200)]
        [Display(Name = "Line 2")]
        public string Line2;

        [StringLength(200)]
        public string City;

        [StringLength(200)]
        public string State;

        [StringLength(9)]
        public string ZipCode;

        [StringLength(200)]
        public string Country;
    }


    [Bind(Exclude = "BuildingName")]
    public class RoomViewMetadata
    {
        [Required(ErrorMessage="Must select a building.")]
        [MinValue(1)]
        public int BuildingId { get; set; }

        [Display(Name = "Building Name")]
        public string BuildingName { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Room Number")]
        public string RoomNumber{ get; set; }

        [StringLength(20)]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }

        [Display(Name = "Sunday Start Time")]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan SundayStartTime { get; set; }

        [Display(Name = "Monday Start Time")]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan MondayStartTime { get; set; }

        [Display(Name = "Tuesday Start Time")]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan TuesdayStartTime { get; set; }

        [Display(Name = "Wednesday Start Time")]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan WednesdayStartTime { get; set; }

        [Display(Name = "Thursday Start Time")]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan ThursdayStartTime { get; set; }

        [Display(Name = "Friday Start Time")]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan FridayStartTime { get; set; }

        [Display(Name = "Saturday Start Time")]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan SaturdayStartTime { get; set; }

        [Display(Name = "Sunday Duration (Hours)")]
        [DisplayFormat(DataFormatString = "{0:0.0}", ApplyFormatInEditMode = true)]
        public Decimal SundayDuration { get; set; }

        [Display(Name = "Monday Duration (Hours)")]
        [DisplayFormat(DataFormatString = "{0:0.0}", ApplyFormatInEditMode = true)]
        public Decimal MondayDuration { get; set; }

        [Display(Name = "Tuesday Duration (Hours)")]
        [DisplayFormat(DataFormatString = "{0:0.0}", ApplyFormatInEditMode = true)]
        public Decimal TuesdayDuration { get; set; }

        [Display(Name = "Wednesday Duration (Hours)")]
        [DisplayFormat(DataFormatString = "{0:0.0}", ApplyFormatInEditMode = true)]
        public Decimal WednesdayDuration { get; set; }

        [Display(Name = "Thursday Duration (Hours)")]
        [DisplayFormat(DataFormatString = "{0:0.0}", ApplyFormatInEditMode = true)]
        public Decimal ThursdayDuration { get; set; }

        [Display(Name = "Friday Duration (Hours)")]
        [DisplayFormat(DataFormatString = "{0:0.0}", ApplyFormatInEditMode = true)]
        public Decimal FridayDuration { get; set; }

        [Display(Name = "Saturday Duration (Hours)")]
        [DisplayFormat(DataFormatString = "{0:0.0}", ApplyFormatInEditMode = true)]
        public Decimal SaturdayDuration { get; set; }

    }

    public class PreferenceMetadata
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Display Color")]
        public string Color{ get; set; }

        [Required]
        [Display(Name = "Can Work")]
        public bool CanWork { get; set; }
    }

    public class VersionMetadata
    {
        [Required]
        [StringLength(500)]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name="Active")]
        public bool IsActive { get; set; }

        [Required]
        [Display(Name = "Visible")]
        public bool IsVisible { get; set; }

        [Required]
        [Display(Name = "Ready For Approval")]
        public bool IsReadyForApproval { get; set; }

        [Required]
        [Display(Name = "Approved")]
        public bool IsApproved { get; set; }
    }
    //public class RoomViewMetadata : IValidatableObject
    //{
    //    //http://stackoverflow.com/questions/2417113/asp-net-mvc-conditional-validation
    //    //http://weblogs.asp.net/scottgu/introducing-asp-net-mvc-3-preview-1
    //    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    //    {
    //        //if (IsSenior && string.IsNullOrEmpty(Senior.Description))
    //        //    yield return new ValidationResult("Description must be supplied.");
            
    //    }
    //}
}