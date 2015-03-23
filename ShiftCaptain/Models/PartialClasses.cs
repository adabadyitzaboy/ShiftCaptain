using ShiftCaptain.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ShiftCaptain.Models
{
    public class NonNullableAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            return base.IsValid(value, validationContext);
        }
        public override bool IsValid(object value)
        {
            return value != null;
        }
        public override bool RequiresValidationContext
        {
            get
            {
                return false;
            }
        }

    }

    [MetadataType(typeof(UserViewMetadata))]
    public partial class UserView : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (MinHours.HasValue && !MaxHours.HasValue)
                yield return new ValidationResult("Must have max hours if given min hours");
            if (!MinHours.HasValue && MaxHours.HasValue)
                yield return new ValidationResult("Must have min hours if given max hours");
            if (!MinHours.HasValue && !MaxHours.HasValue)
            {
                var db = new ShiftCaptainEntities();
                var shiftCount = db.Shifts.Count(s => s.VersionId == SessionManager.VersionId && s.UserId == UserId);
                if (shiftCount > 0)
                {
                    yield return new ValidationResult("Min and Max Hours required - User cannot be removed from Version till all shifts are removed.");
                }
            } 
            if (MinHours.HasValue && MaxHours.HasValue && MaxHours.Value < MinHours.Value)
                yield return new ValidationResult("Max hours must be greater than Min hours");
        }
    }
    
    [MetadataType(typeof(BuildingViewMetadata))]
    public partial class BuildingView
    {
    }
    
    [MetadataType(typeof(RoomViewMetadata))]
    public partial class RoomView
    {
    }

    [MetadataType(typeof(PreferenceMetadata))]
    public partial class Preference
    {
    }

    [Bind(Exclude = "DisplayName")]
    [MetadataType(typeof(VersionMetadata))]
    public partial class Version : IValidatableObject
    {
        public String DisplayName
        {
            get
            {
                return IsActive? Name + " (Active) ": Name;
            }
        }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsVisible  && IsActive)
                yield return new ValidationResult("Version must be visible to set to active.");
        }
    }

    [Bind(Exclude = "To,Cc,Bcc,From,Name")]
    public partial class EmailTemplate
    {
        public String To { get; set; }
        public String Cc { get; set; }
        public String Bcc { get; set; }
        public String From { get; set; }

    }

    public partial class EmailNPass
    {

        [Required(ErrorMessage="Must enter email address")]
        [StringLength(500)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress(ErrorMessage="Must enter a valid email address")]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        [Required(ErrorMessage = "Must enter password")]
        [DataType(DataType.Password)]
        public string Pass { get; set; }
    }
    public class VersionErrors : Version
    {
        public int NumErrors { get; set; }
        public List<UserView> UserConstraintViolations { get; set; }
        public IQueryable<NoShiftCoverage_Result> NoShiftCoverages { get; set; }
        public IQueryable<CantWorkViolation_Result> CantWorkViolations { get; set; }
        public IQueryable<ConflictingShifts_Result> ConflictingShifts { get; set; }

    }
    public class Clone
    {
        public Version Version { get; set; }
        public IEnumerable<SelectListItem> User { get; set; }
        public IEnumerable<string> CloneUser { get; set; }
        public IEnumerable<SelectListItem> Room { get; set; }
        public IEnumerable<string> CloneRoom { get; set; }
    }
    public static class IEnumerableStringExtensions
    {
        public static IEnumerable<SelectListItem> ToSelectListItemList(this IEnumerable<string> items)
        {
            return IEnumerableStringExtensions.ToSelectListItemList(items, new List<string>());
        }

        public static IEnumerable<SelectListItem> ToSelectListItemList(this IEnumerable<string> items, string selectedValue)
        {
            return IEnumerableStringExtensions.ToSelectListItemList(items, new List<string>());
        }

        public static IEnumerable<SelectListItem> ToSelectListItemList(this IEnumerable<string> items, IEnumerable<string> selectedValues)
        {
            List<SelectListItem> listitems = new List<SelectListItem>(items.Count());

            foreach (string s in items)
            {
                listitems.Add(new SelectListItem()
                {
                    Text = s,
                    Value = s,
                    Selected = selectedValues.Contains(s)
                });
            }

            return listitems;
        }
    }
}