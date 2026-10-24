using System;
using System.Collections.Generic;

namespace PollingEcommerce.DataAccess.Models;

public partial class tbl_product_category
{
    public int product_category_id { get; set; }

    public string category_name { get; set; } = null!;

    public string? description { get; set; }

    public DateTime add_date { get; set; }

    public DateTime? modify_date { get; set; }

    public bool delete_flag { get; set; }

    public virtual ICollection<tbl_product> tbl_products { get; set; } = new List<tbl_product>();
}
