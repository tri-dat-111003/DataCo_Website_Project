using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataCo_Website.Models
{
    public class CartItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CartItemId { get; set; }

        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        [StringLength(50)]
        public string Status { get; set; } = "InCart";
        [StringLength(50)]
        public string? SessionId { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? CheckedOutDate { get; set; }

        public virtual Cart Cart { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
