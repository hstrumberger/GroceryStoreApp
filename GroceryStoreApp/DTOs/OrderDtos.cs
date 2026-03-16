using System.ComponentModel.DataAnnotations;

namespace GroceryStoreApp.DTOs;

public class CheckoutRequest
{
    [Required]
    public ShippingAddressDto ShippingAddress { get; set; } = null!;

    [Required]
    public PaymentDto Payment { get; set; } = null!;
}

public class ShippingAddressDto
{
    [Required, MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string AddressLine1 { get; set; } = string.Empty;

    public string? AddressLine2 { get; set; }

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string State { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string PostalCode { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Country { get; set; } = string.Empty;
}

public class PaymentDto
{
    [Required]
    public string Method { get; set; } = string.Empty; // "CreditCard", "DebitCard"

    [Required, MaxLength(16), MinLength(16)]
    public string CardNumber { get; set; } = string.Empty; // simulated

    [Required, MaxLength(100)]
    public string CardHolderName { get; set; } = string.Empty;

    [Required, MaxLength(5)]
    public string ExpiryDate { get; set; } = string.Empty; // MM/YY

    [Required, MaxLength(4), MinLength(3)]
    public string Cvv { get; set; } = string.Empty;
}

public class OrderDto
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal Total { get; set; }
    public ShippingAddressDto ShippingAddress { get; set; } = null!;
    public string PaymentMethod { get; set; } = string.Empty;
    public string? PaymentLast4 { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
}

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal DiscountedPrice { get; set; }
    public decimal LineTotal { get; set; }
    public int Quantity { get; set; }
}

public class OrderSummaryDto
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
