using System;
using System.Collections.Generic;

namespace PollingEcommerce.DataAccess.Models;

public partial class tbl_order_item
{
    public long order_item_id { get; set; }

    public long order_id { get; set; }

    public long product_id { get; set; }

    public int quantity { get; set; }

    public decimal unit_price { get; set; }

    public decimal? line_total { get; set; }

    public DateTime add_date { get; set; }

    public DateTime? modify_date { get; set; }

    public bool delete_flag { get; set; }

    public virtual tbl_order order { get; set; } = null!;

    public virtual tbl_product product { get; set; } = null!;
}
