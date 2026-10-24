SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF DB_ID(N'PollingPaymentDB') IS NULL
BEGIN
    CREATE DATABASE PollingPaymentDB;
END;
GO

USE PollingPaymentDB;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'dbo.tbl_payment', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_payment
    (
        payment_id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tbl_payment PRIMARY KEY,
        payment_reference UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_payment_ref DEFAULT NEWSEQUENTIALID(),
        merchant_order_number UNIQUEIDENTIFIER NOT NULL,
        amount DECIMAL(18,2) NOT NULL,
        currency_code CHAR(3) NOT NULL CONSTRAINT DF_payment_currency DEFAULT ('MMK'),
        payment_status VARCHAR(30) NOT NULL CONSTRAINT DF_payment_status DEFAULT ('Pending'),
        test_mode VARCHAR(30) NOT NULL,
        process_after DATETIME2(0) NULL,
        completed_date DATETIME2(0) NULL,
        failure_reason NVARCHAR(500) NULL,
        idempotency_key UNIQUEIDENTIFIER NOT NULL,
        add_date DATETIME2(0) NOT NULL CONSTRAINT DF_payment_add DEFAULT SYSUTCDATETIME(),
        modify_date DATETIME2(0) NULL,
        delete_flag BIT NOT NULL CONSTRAINT DF_payment_delete DEFAULT (0),
        CONSTRAINT UQ_payment_ref UNIQUE (payment_reference),
        CONSTRAINT UQ_payment_idempotency UNIQUE (idempotency_key),
        CONSTRAINT CK_payment_amount CHECK (amount >= 0),
        CONSTRAINT CK_payment_status CHECK (payment_status IN ('Pending', 'Processing', 'Succeeded', 'Failed')),
        CONSTRAINT CK_payment_mode CHECK (test_mode IN ('Success', 'Failed', 'Delayed'))
    );

    CREATE INDEX IX_payment_merchant
        ON dbo.tbl_payment(merchant_order_number);

    CREATE INDEX IX_payment_processing
        ON dbo.tbl_payment(payment_status, process_after);
END;
GO

IF OBJECT_ID(N'dbo.tbl_payment_event', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.tbl_payment_event
    (
        payment_event_id BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_tbl_payment_event PRIMARY KEY,
        payment_id BIGINT NOT NULL,
        previous_status VARCHAR(30) NULL,
        new_status VARCHAR(30) NOT NULL,
        event_description NVARCHAR(500) NULL,
        add_date DATETIME2(0) NOT NULL CONSTRAINT DF_event_add DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_event_payment FOREIGN KEY (payment_id) REFERENCES dbo.tbl_payment(payment_id)
    );

    CREATE INDEX IX_event_payment_date
        ON dbo.tbl_payment_event(payment_id, add_date);
END;
GO

