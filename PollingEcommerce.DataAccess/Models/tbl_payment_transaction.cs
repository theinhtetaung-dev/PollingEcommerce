using System;
using System.Collections.Generic;

namespace PollingEcommerce.DataAccess.Models;

public partial class tbl_payment_transaction
{
    public long payment_transaction_id { get; set; }

    public long order_id { get; set; }

    public int attempt_number { get; set; }

    public Guid? payment_reference { get; set; }

    public decimal amount { get; set; }

    public string payment_status { get; set; } = null!;

    public DateTime? last_polled_date { get; set; }

    public DateTime? completed_date { get; set; }

    public string? failure_reason { get; set; }

    public DateTime add_date { get; set; }

    public DateTime? modify_date { get; set; }

    public bool delete_flag { get; set; }

    public virtual tbl_order order { get; set; } = null!;
}
