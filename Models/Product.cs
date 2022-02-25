using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWeb.Models
{
    [Table("Product")]
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Bạn chưa nhập tên sản phẩm")]
        [StringLength(250,MinimumLength =3,ErrorMessage ="Chiều dài không hợp lệ")]
        public string Name { get; set; }

        [StringLength(250, MinimumLength = 3, ErrorMessage = "Chiều dài không hợp lệ")]
        public string? Slug { get; set; }

        public string? Image { get; set; }

        [Required(ErrorMessage = "Bạn chưa nhập giá")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Bạn chưa nhập số lượng")]
        public int Limit { get; set; }

        [Required(ErrorMessage = "Bạn chưa chọn trạng thái")]
        public int Status { get; set; }

        public string? Content { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public Category? category { get; set; }

        public List<ProductPhoto>? productPhotos { get; set; }

        public List<OrderDetail>? orderDetails { get; set; }
    }
}
