using System;
using System.Collections.Generic;

namespace PollingPayment.DataAccess.Models;

public partial class tbl_payment_event
{
    public long payment_event_id { get; set; }

    public long payment_id { get; set; }

    public string? previous_status { get; set; }

    public string new_status { get; set; } = null!;

    public string? event_description { get; set; }

    public DateTime add_date { get; set; }

    public virtual tbl_payment payment { get; set; } = null!;
}
