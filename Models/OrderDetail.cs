using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWeb.Models
{
    [Table("OrderDetail")]
    public class OrderDetail
    {
        [Key]
        public int Id { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public int OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Order order { get; set; }

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product product { get; set; }

    }
}
