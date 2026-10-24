# Polling E-Commerce

.NET 8 sample with separate E-commerce and simulated Payment databases/APIs. The MVC browser polls only the E-commerce application; the E-commerce API reconciles payment state through service-to-service HTTP.

## Local setup

1. Run `database/01-create-ecommerce.sql` and `database/02-create-payment.sql` with SQL Server.
2. Store connection strings outside source:
   `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..." --project PollingEcommerce.WebAPI`
   `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "..." --project PollingPayment.WebAPI`
3. Start `PollingPayment.WebAPI`, then `PollingEcommerce.WebAPI`, then `PollingEcommerce.MVC` using their `http` launch profiles.
4. Open `http://localhost:5129`.

Payment API: `POST /api/payments`, `GET /api/payments/{reference}`.
E-commerce API: `GET /api/products`, `POST /api/orders/checkout`, `GET /api/orders/{orderNumber}`.

Scaffolding commands use `--no-onconfiguring`, so credentials are never generated into the DbContext files.
