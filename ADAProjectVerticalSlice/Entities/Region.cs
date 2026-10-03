using System.ComponentModel.DataAnnotations;

namespace ADAProjectAPIVerticalSlice.Entities;

    public class Region
    {
   
        public Guid RegionId { get; set; }
        public string RegionName { get; set; } = string.Empty;
    }
