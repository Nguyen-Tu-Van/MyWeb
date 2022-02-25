using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWeb.Models
{
    [Table("Order")]
    public class Order
    {
        [Key]
        public int Id { get; set; }

        public decimal Total { get; set; }

        [Required(ErrorMessage = "Bạn chưa nhập địa chỉ")]
        [StringLength(250,MinimumLength =3,ErrorMessage ="Chiều dài không hợp lệ")]
        public string Address { get; set; }
        public int Payment { get; set; }
        public int Status { get; set; }
        public string Content { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }

        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User user { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }
    }
}
