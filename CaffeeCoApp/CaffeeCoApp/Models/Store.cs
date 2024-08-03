using System.ComponentModel.DataAnnotations;

namespace CaffeeCoApp.Models
{
    public class Store
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public string Longitude { get; set; } = "";
        public string Latitude { get; set; } = "";
        [Range(0, int.MaxValue)]
        public int DailyPickUpLimit { get; set; }

    }
}
