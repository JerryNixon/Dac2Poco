// more info at https://aka.ms/dab

using System;
using System.Diagnostics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Models
{
    public abstract class Poco { /* TODO */ }
}

namespace Models.dbo
{
    [DebuggerDisplay("dbo.PageCustomers (StartIndex = {Parameters.StartIndex}, PageSize = {Parameters.PageSize}) [procedure]")]
    public class @PageCustomers : Poco 
    {
        [JsonPropertyName("city")]
        public string @City { get; set; } = default!;

        [JsonPropertyName("id")]
        public int @Id { get; set; } = default!;

        [JsonPropertyName("name")]
        public string @Name { get; set; } = default!;

        [JsonPropertyName("state")]
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
        [JsonPropertyName("id")]
        public int @Id { get; set; } = default!;

        [JsonPropertyName("city")]
        public string @City { get; set; } = default!;

        [JsonPropertyName("name")]
        public string @Name { get; set; } = default!;

        [JsonPropertyName("specialrank")]
        public decimal @SpecialRank { get; set; } = default!;

        [JsonPropertyName("state")]
        public string @State { get; set; } = default!;

        [ReadOnly(true)]
        [JsonPropertyName("location")]
        public string @Location { get; set; } = default!;

    }

    [DebuggerDisplay("dbo.Lines (Id = {Id}) [table]")]
    public class @Lines : Poco 
    {
        [Key]
        [JsonPropertyName("id")]
        public int @Id { get; set; } = default!;

        [JsonPropertyName("orderid")]
        public int @OrderId { get; set; } = default!;

        [JsonPropertyName("priceeach")]
        public double @PriceEach { get; set; } = default!;

        [JsonPropertyName("productid")]
        public int @ProductId { get; set; } = default!;

        [JsonPropertyName("quantity")]
        public int @Quantity { get; set; } = default!;

    }

    [DebuggerDisplay("dbo.Orders (Id = {Id}) [table]")]
    public class @Orders : Poco 
    {
        [Key]
        [JsonPropertyName("id")]
        public int @Id { get; set; } = default!;

        [JsonPropertyName("customerid")]
        public int @CustomerId { get; set; } = default!;

        [JsonPropertyName("date")]
        public DateTime? @Date { get; set; } = default!;

    }

    [DebuggerDisplay("dbo.Products (Id = {Id}) [table]")]
    public class @Products : Poco 
    {
        [Key]
        [JsonPropertyName("id")]
        public int @Id { get; set; } = default!;

        [JsonPropertyName("name")]
        public string @Name { get; set; } = default!;

        [JsonPropertyName("price")]
        public double @Price { get; set; } = default!;

    }

    [DebuggerDisplay("dbo.Sample (Id1 = {Id1}, Id2 = {Id2}, Id3 = {Id3}) [table]")]
    public class @Sample : Poco 
    {
        [Key]
        [JsonPropertyName("id1")]
        public int @Id1 { get; set; } = default!;

        [Key]
        [JsonPropertyName("id2")]
        public int @Id2 { get; set; } = default!;

        [Key]
        [JsonPropertyName("id3")]
        public int @Id3 { get; set; } = default!;

        [JsonPropertyName("name")]
        public string @Name { get; set; } = default!;

        [ReadOnly(true)]
        [JsonPropertyName("uppername")]
        public string @UpperName { get; set; } = default!;

    }

    [DebuggerDisplay("dbo.SampleEdgeTwo (keyless) [table]")]
    public class @SampleEdgeTwo : Poco 
    {
        [JsonPropertyName("id")]
        public string @Id { get; set; } = default!;

    }

    [DebuggerDisplay("dbo.SampleNode (Id = {Id}) [table]")]
    public class @SampleNode : Poco 
    {
        [Key]
        [JsonPropertyName("id")]
        public int @Id { get; set; } = default!;

    }

    [DebuggerDisplay("dbo.vCustomers (keyless) [view]")]
    public class @vCustomers : Poco 
    {
        [JsonPropertyName("city")]
        public string @City { get; set; } = default!;

        [JsonPropertyName("id")]
        public int @Id { get; set; } = default!;

        [JsonPropertyName("location")]
        public string @Location { get; set; } = default!;

        [JsonPropertyName("name")]
        public string @Name { get; set; } = default!;

        [JsonPropertyName("specialrank")]
        public decimal @SpecialRank { get; set; } = default!;

        [JsonPropertyName("state")]
        public string @State { get; set; } = default!;

    }

}