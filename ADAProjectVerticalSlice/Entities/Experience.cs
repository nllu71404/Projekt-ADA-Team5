using System.ComponentModel.DataAnnotations;

namespace ADAProjectAPIVerticalSlice.Entities
{
    public enum Experience
    {
        [Display(Name = "<1 år")]
        LessThan1Year,

        [Display(Name = "1-2 år")]
        From1To2Years,

        [Display(Name = "3-5 år")]
        From3To5Years,

        [Display(Name = "5+ år")]
        MoreThan5Years
    }
}
