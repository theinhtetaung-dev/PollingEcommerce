SET NOCOUNT ON;
SET QUOTED_IDENTIFIER ON;
IF DB_ID(N'PollingEcommerceDB') IS NULL CREATE DATABASE PollingEcommerceDB;
GO
USE PollingEcommerceDB;
GO
SET QUOTED_IDENTIFIER ON;
GO
IF OBJECT_ID(N'dbo.tbl_user', N'U') IS NULL
CREATE TABLE dbo.tbl_user (
 user_id BIGINT IDENTITY PRIMARY KEY, full_name NVARCHAR(150) NOT NULL,
 email NVARCHAR(255) NOT NULL, phone_number NVARCHAR(30) NULL,
 add_date DATETIME2(0) NOT NULL CONSTRAINT DF_user_add DEFAULT SYSUTCDATETIME(),
 modify_date DATETIME2(0) NULL, delete_flag BIT NOT NULL CONSTRAINT DF_user_delete DEFAULT 0,
 CONSTRAINT UQ_user_email UNIQUE(email));
GO
IF OBJECT_ID(N'dbo.tbl_product_category', N'U') IS NULL
CREATE TABLE dbo.tbl_product_category (
 product_category_id INT IDENTITY PRIMARY KEY, category_name NVARCHAR(100) NOT NULL,
 description NVARCHAR(500) NULL, add_date DATETIME2(0) NOT NULL CONSTRAINT DF_category_add DEFAULT SYSUTCDATETIME(),
 modify_date DATETIME2(0) NULL, delete_flag BIT NOT NULL CONSTRAINT DF_category_delete DEFAULT 0,
 CONSTRAINT UQ_category_name UNIQUE(category_name));
GO
IF OBJECT_ID(N'dbo.tbl_product', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.tbl_product (
  product_id BIGINT IDENTITY PRIMARY KEY, product_category_id INT NOT NULL,
  product_name NVARCHAR(200) NOT NULL, description NVARCHAR(1000) NULL,
  unit_price DECIMAL(18,2) NOT NULL, stock_quantity INT NOT NULL,
  is_active BIT NOT NULL CONSTRAINT DF_product_active DEFAULT 1,
  add_date DATETIME2(0) NOT NULL CONSTRAINT DF_product_add DEFAULT SYSUTCDATETIME(),
  modify_date DATETIME2(0) NULL, delete_flag BIT NOT NULL CONSTRAINT DF_product_delete DEFAULT 0,
  CONSTRAINT FK_product_category FOREIGN KEY(product_category_id) REFERENCES dbo.tbl_product_category(product_category_id),
  CONSTRAINT CK_product_price CHECK(unit_price >= 0), CONSTRAINT CK_product_stock CHECK(stock_quantity >= 0));
 CREATE INDEX IX_product_category ON dbo.tbl_product(product_category_id, is_active, delete_flag);
END
GO
IF OBJECT_ID(N'dbo.tbl_order', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.tbl_order (
  order_id BIGINT IDENTITY PRIMARY KEY, order_number UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_order_number DEFAULT NEWSEQUENTIALID(),
  user_id BIGINT NOT NULL, total_amount DECIMAL(18,2) NOT NULL,
  order_status VARCHAR(30) NOT NULL CONSTRAINT DF_order_status DEFAULT 'PendingPayment',
  add_date DATETIME2(0) NOT NULL CONSTRAINT DF_order_add DEFAULT SYSUTCDATETIME(), modify_date DATETIME2(0) NULL,
  delete_flag BIT NOT NULL CONSTRAINT DF_order_delete DEFAULT 0,
  CONSTRAINT UQ_order_number UNIQUE(order_number), CONSTRAINT FK_order_user FOREIGN KEY(user_id) REFERENCES dbo.tbl_user(user_id),
  CONSTRAINT CK_order_total CHECK(total_amount >= 0), CONSTRAINT CK_order_status CHECK(order_status IN ('PendingPayment','Paid','PaymentFailed','Cancelled')));
 CREATE INDEX IX_order_user ON dbo.tbl_order(user_id, add_date DESC);
END
GO
IF OBJECT_ID(N'dbo.tbl_order_item', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.tbl_order_item (
  order_item_id BIGINT IDENTITY PRIMARY KEY, order_id BIGINT NOT NULL, product_id BIGINT NOT NULL,
  quantity INT NOT NULL, unit_price DECIMAL(18,2) NOT NULL, line_total AS (CONVERT(DECIMAL(18,2),quantity*unit_price)) PERSISTED,
  add_date DATETIME2(0) NOT NULL CONSTRAINT DF_item_add DEFAULT SYSUTCDATETIME(), modify_date DATETIME2(0) NULL,
  delete_flag BIT NOT NULL CONSTRAINT DF_item_delete DEFAULT 0,
  CONSTRAINT FK_item_order FOREIGN KEY(order_id) REFERENCES dbo.tbl_order(order_id),
  CONSTRAINT FK_item_product FOREIGN KEY(product_id) REFERENCES dbo.tbl_product(product_id),
  CONSTRAINT CK_item_quantity CHECK(quantity > 0), CONSTRAINT CK_item_price CHECK(unit_price >= 0));
 CREATE INDEX IX_item_order ON dbo.tbl_order_item(order_id);
END
GO
IF OBJECT_ID(N'dbo.tbl_payment_transaction', N'U') IS NULL
BEGIN
 CREATE TABLE dbo.tbl_payment_transaction (
  payment_transaction_id BIGINT IDENTITY PRIMARY KEY, order_id BIGINT NOT NULL, attempt_number INT NOT NULL,
  payment_reference UNIQUEIDENTIFIER NULL, amount DECIMAL(18,2) NOT NULL,
  payment_status VARCHAR(30) NOT NULL CONSTRAINT DF_tx_status DEFAULT 'Pending', last_polled_date DATETIME2(0) NULL,
  completed_date DATETIME2(0) NULL, failure_reason NVARCHAR(500) NULL,
  add_date DATETIME2(0) NOT NULL CONSTRAINT DF_tx_add DEFAULT SYSUTCDATETIME(), modify_date DATETIME2(0) NULL,
  delete_flag BIT NOT NULL CONSTRAINT DF_tx_delete DEFAULT 0,
  CONSTRAINT FK_tx_order FOREIGN KEY(order_id) REFERENCES dbo.tbl_order(order_id),
  CONSTRAINT UQ_tx_attempt UNIQUE(order_id,attempt_number), CONSTRAINT CK_tx_attempt CHECK(attempt_number > 0),
  CONSTRAINT CK_tx_amount CHECK(amount >= 0), CONSTRAINT CK_tx_status CHECK(payment_status IN ('Pending','Processing','Succeeded','Failed','Unknown')));
 CREATE UNIQUE INDEX UX_tx_reference ON dbo.tbl_payment_transaction(payment_reference) WHERE payment_reference IS NOT NULL;
 CREATE INDEX IX_tx_polling ON dbo.tbl_payment_transaction(payment_status,last_polled_date);
END
GO
IF NOT EXISTS(SELECT 1 FROM dbo.tbl_product_category)
BEGIN
 INSERT dbo.tbl_product_category(category_name,description) VALUES (N'Electronics',N'Devices and accessories'),(N'Home',N'Everyday home essentials'),(N'Books',N'Books and learning');
 INSERT dbo.tbl_product(product_category_id,product_name,description,unit_price,stock_quantity) VALUES
 ((SELECT product_category_id FROM dbo.tbl_product_category WHERE category_name=N'Electronics'),N'Wireless Headphones',N'Comfortable Bluetooth headphones',85000,20),
 ((SELECT product_category_id FROM dbo.tbl_product_category WHERE category_name=N'Home'),N'Insulated Bottle',N'750 ml reusable bottle',25000,40),
 ((SELECT product_category_id FROM dbo.tbl_product_category WHERE category_name=N'Books'),N'ASP.NET Core Guide',N'Practical web development guide',42000,15);
END
IF NOT EXISTS(SELECT 1 FROM dbo.tbl_user WHERE email=N'demo@example.com')
 INSERT dbo.tbl_user(full_name,email,phone_number) VALUES(N'Demo Customer',N'demo@example.com',N'09123456789');
GO
