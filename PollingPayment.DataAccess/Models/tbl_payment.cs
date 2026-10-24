using System;
using System.Collections.Generic;

namespace PollingPayment.DataAccess.Models;

public partial class tbl_payment
{
    public long payment_id { get; set; }

    public Guid payment_reference { get; set; }

    public Guid merchant_order_number { get; set; }

    public decimal amount { get; set; }

    public string currency_code { get; set; } = null!;

    public string payment_status { get; set; } = null!;

    public string test_mode { get; set; } = null!;

    public DateTime? process_after { get; set; }

    public DateTime? completed_date { get; set; }

    public string? failure_reason { get; set; }

    public Guid idempotency_key { get; set; }

    public DateTime add_date { get; set; }

    public DateTime? modify_date { get; set; }

    public bool delete_flag { get; set; }

    public virtual ICollection<tbl_payment_event> tbl_payment_events { get; set; } = new List<tbl_payment_event>();
}
