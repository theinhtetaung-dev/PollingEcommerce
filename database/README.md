# Database setup

Scripts are additive and never drop databases or tables.

```powershell
sqlcmd -S . -U sa -P $env:POLLING_SQL_PASSWORD -C -i database/01-create-ecommerce.sql
sqlcmd -S . -U sa -P $env:POLLING_SQL_PASSWORD -C -i database/02-create-payment.sql
```

Application connection strings are supplied through environment variables or .NET User Secrets, not committed files.
