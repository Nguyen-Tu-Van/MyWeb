using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWeb.Models
{
    [Table("User")]
    public class User : IdentityUser
    {
        //[Required(ErrorMessage = "Bạn chưa nhập tên")]
        [StringLength(250,MinimumLength =3,ErrorMessage ="Chiều dài không hợp lệ")]
        public string? Name { get; set; }

        public DateTime? Birthday { get; set; }

        public List<Order>? Orders { get; set; }

    }
}
