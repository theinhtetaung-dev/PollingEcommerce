using System;
using System.Collections.Generic;

namespace PollingEcommerce.DataAccess.Models;

public partial class tbl_order
{
    public long order_id { get; set; }

    public Guid order_number { get; set; }

    public long user_id { get; set; }

    public decimal total_amount { get; set; }

    public string order_status { get; set; } = null!;

    public DateTime add_date { get; set; }

    public DateTime? modify_date { get; set; }

    public bool delete_flag { get; set; }

    public virtual ICollection<tbl_order_item> tbl_order_items { get; set; } = new List<tbl_order_item>();

    public virtual ICollection<tbl_payment_transaction> tbl_payment_transactions { get; set; } = new List<tbl_payment_transaction>();

    public virtual tbl_user user { get; set; } = null!;
}
