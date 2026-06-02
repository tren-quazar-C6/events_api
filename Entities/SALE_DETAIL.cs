namespace events_api.Entities;

public partial class SaleDetail
{
    public int IdSaleDetail { get; set; }

    public int IdVenta { get; set; }

    public int IdEventoAsiento { get; set; }

    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; }

    public decimal Subtotal { get; set; }

    public virtual Venta IdVentaNavigation { get; set; } = null!;

    public virtual EventoAsiento IdEventoAsientoNavigation { get; set; } = null!;
}
