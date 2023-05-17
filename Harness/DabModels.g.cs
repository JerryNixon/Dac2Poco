// more info at https://aka.ms/dab

using System;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Models
{
    public abstract class Poco { /* TODO */ }
}

namespace Models.dbo
{
    [DebuggerDisplay("dbo.PageCustomers (StartIndex = {Parameters.StartIndex}, PageSize = {Parameters.PageSize}) [procedure]")]
    public class @PageCustomers : Poco 
    {
        public string @City { get; set; } = default!;
        public int @Id { get; set; } = default!;
        public string @Name { get; set; } = default!;
        public string @State { get; set; } = default!;
        public ParameterInfo Parameters { get; set; } = new();

        public class ParameterInfo
        {
            public int? @PageSize { get; set; } = default!;
            public int? @StartIndex { get; set; } = default!;
        }
    }

    [DebuggerDisplay("dbo.Customers (Id = {Id}) [table]")]
    public class @Customers : Poco 
    {
        [Key]
        public int @Id { get; set; } = default!;
        public string @City { get; set; } = default!;
        public string @Name { get; set; } = default!;
        public decimal @SpecialRank { get; set; } = default!;
        public string @State { get; set; } = default!;
        [ReadOnly(true)]
        public string @Location { get; set; } = default!;
    }

    [DebuggerDisplay("dbo.Lines (Id = {Id}) [table]")]
    public class @Lines : Poco 
    {
        [Key]
        public int @Id { get; set; } = default!;
        public int @OrderId { get; set; } = default!;
        public double @PriceEach { get; set; } = default!;
        public int @ProductId { get; set; } = default!;
        public int @Quantity { get; set; } = default!;
    }

    [DebuggerDisplay("dbo.Orders (Id = {Id}) [table]")]
    public class @Orders : Poco 
    {
        [Key]
        public int @Id { get; set; } = default!;
        public int @CustomerId { get; set; } = default!;
        public DateTime? @Date { get; set; } = default!;
    }

    [DebuggerDisplay("dbo.Products (Id = {Id}) [table]")]
    public class @Products : Poco 
    {
        [Key]
        public int @Id { get; set; } = default!;
        public string @Name { get; set; } = default!;
        public double @Price { get; set; } = default!;
    }

    [DebuggerDisplay("dbo.SampleEdgeTwo (keyless) [table]")]
    public class @SampleEdgeTwo : Poco 
    {
        public string @Id { get; set; } = default!;
    }

    [DebuggerDisplay("dbo.SampleNode (keyless) [table]")]
    public class @SampleNode : Poco 
    {
        public int @Id { get; set; } = default!;
    }

    [DebuggerDisplay("dbo.vCustomers (keyless) [view]")]
    public class @vCustomers : Poco 
    {
        public string @City { get; set; } = default!;
        public int @Id { get; set; } = default!;
        public string @Location { get; set; } = default!;
        public string @Name { get; set; } = default!;
        public decimal @SpecialRank { get; set; } = default!;
        public string @State { get; set; } = default!;
    }

}