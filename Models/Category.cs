using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWeb.Models
{
    [Table("Category")]
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Bạn chưa nhập tên danh mục")]
        [StringLength(250,MinimumLength =3,ErrorMessage ="Chiều dài không hợp lệ")]
        public string Name { get; set; }

        [StringLength(250, MinimumLength = 3, ErrorMessage = "Chiều dài không hợp lệ")]
        public string? Slug { get; set; }

        [Required]
        public string Description { get; set; }

        public int? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public Category? Parent { get; set; }

        public List<Category>? ChildCategories { get; set; }

    }
}
