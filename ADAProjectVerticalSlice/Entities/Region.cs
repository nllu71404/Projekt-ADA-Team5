using System.ComponentModel.DataAnnotations;

namespace ADAProjectAPIVerticalSlice.Entities

{
    public enum Region
    {
   
        [Display(Name = "United Kingdom East")]
        UnitedKingdomEast,

        [Display(Name = "Continental Europe")]
        ContinentalEurope,

        [Display(Name = "United Kingdom West")]
        UnitedKingdomWest,

        Americas,

        [Display(Name = "Asia Pacific")]
        AsiaPacific,

        [Display(Name = "Middle East")]
        MiddleEast,

        Africa,
        Nordics,

        [Display(Name = "Central Europe")]
        CentralEurope,

        [Display(Name = "Southern Europe")]
        SouthernEurope
    }

}
