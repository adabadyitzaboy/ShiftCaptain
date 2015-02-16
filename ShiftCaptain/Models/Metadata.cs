using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ShiftCaptain.Models
{
    public class UserMetadata
    {
        [Required]
        [StringLength(500)]
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

    }
    public class UserViewMetadata : UserMetadata
    {
        [StringLength(200)]
        [Display(Name = "Address Line 1")]
        public string Line1;

        [StringLength(200)]
        [Display(Name = "Address Line 2")]
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

        [StringLength(100)]
        [Display(Name = "Manager Phone Number")]
        [DataType(DataType.PhoneNumber)]
        public string ManagerPhone { get; set; }

    }

    public class BuildingViewMetadata : BuildlingMetadata
    {
        [StringLength(200)]
        [Display(Name = "Address Line 1")]
        public string Line1;

        [StringLength(200)]
        [Display(Name = "Address Line 2")]
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