using System;
using System.Collections.Generic;

namespace PollingEcommerce.DataAccess.Models;

public partial class tbl_user
{
    public long user_id { get; set; }

    public string full_name { get; set; } = null!;

    public string email { get; set; } = null!;

    public string? phone_number { get; set; }

    public DateTime add_date { get; set; }

    public DateTime? modify_date { get; set; }

    public bool delete_flag { get; set; }

    public virtual ICollection<tbl_order> tbl_orders { get; set; } = new List<tbl_order>();
}
