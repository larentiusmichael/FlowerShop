using System.ComponentModel.DataAnnotations;

namespace FlowerShop.Models
{
    public class Flower
    {
        [Key] //primary key
        public int FlowerID { get; set; }

        public string FlowerName { get; set; }

        public string FlowerType { get; set; }

        public string FlowerDescription { get; set; }

        public DateTime DeliveryDate { get; set; }

        public decimal FlowerPrice { get; set; }
    }
}
