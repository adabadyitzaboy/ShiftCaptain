using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ShiftCaptain.Models
{
    [MetadataType(typeof(UserViewMetadata))]
    public partial class UserView
    {
    }
    [MetadataType(typeof(BuildingViewMetadata))]
    public partial class BuildingView
    {
    }
    //[MetadataType(typeof(RoomViewMetadata))]
    public partial class RoomView
    {
    }
}