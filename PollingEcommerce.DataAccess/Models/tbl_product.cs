using System;
using System.Collections.Generic;

namespace PollingEcommerce.DataAccess.Models;

public partial class tbl_product
{
    public long product_id { get; set; }

    public int product_category_id { get; set; }

    public string product_name { get; set; } = null!;

    public string? description { get; set; }

    public decimal unit_price { get; set; }

    public int stock_quantity { get; set; }

    public bool is_active { get; set; }

    public DateTime add_date { get; set; }

    public DateTime? modify_date { get; set; }

    public bool delete_flag { get; set; }

    public virtual tbl_product_category product_category { get; set; } = null!;

    public virtual ICollection<tbl_order_item> tbl_order_items { get; set; } = new List<tbl_order_item>();
}
