// learn more at https://aka.ms/dab

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using Models;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Models
{
    public abstract class Poco { /* TODO */ }
}

namespace Models.dbo
{
    [DebuggerDisplay("Customers.Id = {Id}")]
    [Table("Customers", Schema = "dbo")]
    public partial class Customers : Poco
    {
        [Key]
        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("Id", TypeName = "int")]
        public Int32 @Id { get; set; } = default;

        [StringLength(50)]
        [Column("Name", TypeName = "nvarchar")]
        public String? @Name { get; set; } = default!;

        [Required(AllowEmptyStrings = true)]
        [StringLength(50)]
        [Column("City", TypeName = "varchar")]
        public String @City { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = true)]
        [StringLength(50)]
        [Column("State", TypeName = "varchar")]
        public String @State { get; set; } = string.Empty;

        [ReadOnly(true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("Address1", TypeName = "Computed")]
        public string @Address1 { get; } = default!;

        [ReadOnly(true)]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        [Column("Address2", TypeName = "Computed")]
        [JsonPropertyName("Address2")]
        public string @AddressLine2 { get; } = default!;

        // xxx/Customer/{Id/123}/{Id2/123}
        public string GetRequestJson(bool includeKeys = false) => string.Empty;
        public string GetResponseJson() => string.Empty;

    }

    [DebuggerDisplay("Lines.Id = {Id}")]
    [Table("Lines", Schema = "dbo")]
    public partial class Lines : Poco
    {
        [Key]
        [Required]
        [Column("Id", TypeName = "int")]
        public Int32 @Id { get; set; } = default;

        [Required]
        [Column("Quantity", TypeName = "int")]
        public Int32 @Quantity { get; set; } = default;

        [Required]
        [Column("ProductId", TypeName = "int")]
        public Int32 @ProductId { get; set; } = default;

        [Required]
        [Column("OrderId", TypeName = "int")]
        public Int32 @OrderId { get; set; } = default;

        [Required]
        [Column("PriceEach", TypeName = "money")]
        public Decimal @PriceEach { get; set; } = default;
    }

    [DebuggerDisplay("Orders.Id = {Id}")]
    [Table("Orders", Schema = "dbo")]
    public partial class Orders : Poco
    {
        [Key]
        [Required]
        [Column("Id", TypeName = "int")]
        public Int32 @Id { get; set; } = default;

        [Column("Date", TypeName = "datetime")]
        public DateTime? @Date { get; set; } = default!;

        [Required]
        [Column("CustomerId", TypeName = "int")]
        public Int32 @CustomerId { get; set; } = default;
    }

    [DebuggerDisplay("Products.Id = {Id}")]
    [Table("Products", Schema = "dbo")]
    public partial class Products : Poco
    {
        [Key]
        [Required]
        [Column("Id", TypeName = "int")]
        public Int32 @Id { get; set; } = default;

        [Required(AllowEmptyStrings = true)]
        [StringLength(50)]
        [Column("Name", TypeName = "varchar")]
        public String @Name { get; set; } = string.Empty;

        [Required]
        [Column("Price", TypeName = "money")]
        public Decimal @Price { get; set; } = default;
    }

    [Table("vCustomers", Schema = "dbo")] // View
    public partial class vCustomers : Poco
    {
        [Column("Id", TypeName = "string")]
        public string @Id { get; set; } = default!;

        [Column("Name", TypeName = "string")]
        public string @Name { get; set; } = default!;

        [Column("City", TypeName = "string")]
        public string @City { get; set; } = default!;

        [Column("State", TypeName = "string")]
        public string @State { get; set; } = default!;

        [Column("Address1", TypeName = "string")]
        public string @Address1 { get; set; } = default!;

        [Column("Address2", TypeName = "string")]
        public string @Address2 { get; set; } = default!;
    }
}
