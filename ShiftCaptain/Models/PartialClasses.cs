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
    public partial class RoomView : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.SundayStartTime.HasValue && !this.SundayDuration.HasValue)
            {
                yield return new ValidationResult("Sunday must have a duration if it has a start time.");
            }
            if (this.MondayStartTime.HasValue && !this.MondayDuration.HasValue)
            {
                yield return new ValidationResult("Monday must have a duration if it has a start time.");
            }
            if (this.TuesdayStartTime.HasValue && !this.TuesdayDuration.HasValue)
            {
                yield return new ValidationResult("Tuesday must have a duration if it has a start time.");
            }
            if (this.WednesdayStartTime.HasValue && !this.WednesdayDuration.HasValue)
            {
                yield return new ValidationResult("Wednesday must have a duration if it has a start time.");
            }
            if (this.ThursdayStartTime.HasValue && !this.ThursdayDuration.HasValue)
            {
                yield return new ValidationResult("Thursday must have a duration if it has a start time.");
            }
            if (this.FridayStartTime.HasValue && !this.FridayDuration.HasValue)
            {
                yield return new ValidationResult("Friday must have a duration if it has a start time.");
            }
            if (this.SaturdayStartTime.HasValue && !this.SaturdayDuration.HasValue)
            {
                yield return new ValidationResult("Saturday must have a duration if it has a start time.");
            }

            if (!this.SundayStartTime.HasValue && this.SundayDuration.HasValue)
            {
                yield return new ValidationResult("Sunday must have a Start Time if it has a start time.");
            }
            if (!this.MondayStartTime.HasValue && this.MondayDuration.HasValue)
            {
                yield return new ValidationResult("Monday must have a Start Time if it has a start time.");
            }
            if (!this.TuesdayStartTime.HasValue && this.TuesdayDuration.HasValue)
            {
                yield return new ValidationResult("Tuesday must have a Start Time if it has a start time.");
            }
            if (!this.WednesdayStartTime.HasValue && this.WednesdayDuration.HasValue)
            {
                yield return new ValidationResult("Wednesday must have a Start Time if it has a start time.");
            }
            if (!this.ThursdayStartTime.HasValue && this.ThursdayDuration.HasValue)
            {
                yield return new ValidationResult("Thursday must have a Start Time if it has a start time.");
            }
            if (!this.FridayStartTime.HasValue && this.FridayDuration.HasValue)
            {
                yield return new ValidationResult("Friday must have a Start Time if it has a start time.");
            }
            if (!this.SaturdayStartTime.HasValue && this.SaturdayDuration.HasValue)
            {
                yield return new ValidationResult("Saturday must have a Start Time if it has a start time.");
            }
            var daySpan = TimeSpan.FromHours(24);
            if (this.SundayStartTime.HasValue && this.SundayDuration.HasValue && this.MondayStartTime.HasValue && this.MondayDuration.HasValue)
            {
                var end = this.SundayStartTime.Value.Add(TimeSpan.FromHours((double)this.SundayDuration.Value));
                if (end > daySpan && end.Subtract(daySpan) > this.MondayStartTime.Value)
                {
                    yield return new ValidationResult("Sunday's Duration is too high. Sunday is open past Monday's start time.");
                }
            }
            if (this.MondayStartTime.HasValue && this.MondayDuration.HasValue && this.TuesdayStartTime.HasValue && this.TuesdayDuration.HasValue)
            {
                var end = this.MondayStartTime.Value.Add(TimeSpan.FromHours((double)this.MondayDuration.Value));
                if (end > daySpan && end.Subtract(daySpan) > this.TuesdayStartTime.Value)
                {
                    yield return new ValidationResult("Monday's Duration is too high. Monday is open past Tuesday's start time.");
                }
            }
            if (this.TuesdayStartTime.HasValue && this.TuesdayDuration.HasValue && this.WednesdayStartTime.HasValue && this.WednesdayDuration.HasValue)
            {
                var end = this.TuesdayStartTime.Value.Add(TimeSpan.FromHours((double)this.TuesdayDuration.Value));
                if (end > daySpan && end.Subtract(daySpan) > this.WednesdayStartTime.Value)
                {
                    yield return new ValidationResult("Tuesday's Duration is too high. Tuesday is open past Wednesday's start time.");
                }
            }
            if (this.WednesdayStartTime.HasValue && this.WednesdayDuration.HasValue && this.ThursdayStartTime.HasValue && this.ThursdayDuration.HasValue)
            {
                var end = this.WednesdayStartTime.Value.Add(TimeSpan.FromHours((double)this.WednesdayDuration.Value));
                if (end > daySpan && end.Subtract(daySpan) > this.ThursdayStartTime.Value)
                {
                    yield return new ValidationResult("Wednesday's Duration is too high. Wednesday is open past Thursday's start time.");
                }
            }
            if (this.ThursdayStartTime.HasValue && this.ThursdayDuration.HasValue && this.FridayStartTime.HasValue && this.FridayDuration.HasValue)
            {
                var end = this.ThursdayStartTime.Value.Add(TimeSpan.FromHours((double)this.ThursdayDuration.Value));
                if (end > daySpan && end.Subtract(daySpan) > this.FridayStartTime.Value)
                {
                    yield return new ValidationResult("Thursday's Duration is too high. Thursday is open past Friday's start time.");
                }
            }
            if (this.FridayStartTime.HasValue && this.FridayDuration.HasValue && this.SaturdayStartTime.HasValue && this.SaturdayDuration.HasValue)
            {
                var end = this.FridayStartTime.Value.Add(TimeSpan.FromHours((double)this.FridayDuration.Value));
                if (end > daySpan && end.Subtract(daySpan) > this.SaturdayStartTime.Value)
                {
                    yield return new ValidationResult("Friday's Duration is too high. Friday is open past Saturday's start time.");
                }
            }
            if (this.SaturdayStartTime.HasValue && this.SaturdayDuration.HasValue && this.SundayStartTime.HasValue && this.SundayDuration.HasValue)
            {
                var end = this.SaturdayStartTime.Value.Add(TimeSpan.FromHours((double)this.SaturdayDuration.Value));
                if (end > daySpan && end.Subtract(daySpan) > this.SundayStartTime.Value)
                {
                    yield return new ValidationResult("Saturday's Duration is too high. Saturday is open past Sunday's start time.");
                }
            }
        }
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

    public class Clone: IValidatableObject
    {
        public Version Version { get; set; }
        public IEnumerable<SelectListItem> User { get; set; }
        public IEnumerable<string> CloneUser { get; set; }
        public IEnumerable<SelectListItem> Room { get; set; }
        public IEnumerable<string> CloneRoom { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var db = new ShiftCaptainEntities();
            var versionCount = db.Versions.Count(v =>v.Name == Version.Name);
            if (versionCount > 0)
                yield return new ValidationResult("Version Name must be unique.");
        }
    }
}