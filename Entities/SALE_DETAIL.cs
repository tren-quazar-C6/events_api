namespace events_api.Entities;

public partial class SALE_DETAIL
{
    public int id_sale_detail { get; set; }

    public int id_venta { get; set; }

    public int id_evento_asiento { get; set; }

    public decimal unit_price { get; set; }

    public int quantity { get; set; }

    public decimal subtotal { get; set; }

    public virtual VENTA id_ventaNavigation { get; set; } = null!;

    public virtual EVENTO_ASIENTO id_evento_asientoNavigation { get; set; } = null!;
}
