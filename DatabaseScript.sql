Build started...
Build succeeded.
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [Phone] nvarchar(20) NULL,
    [Fax] nvarchar(20) NULL,
    [ContactPerson] nvarchar(50) NULL,
    [TaxId] nvarchar(200) NULL,
    [PaymentMethod] int NOT NULL,
    [Remarks] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [PartNo] nvarchar(50) NOT NULL,
    [Unit] nvarchar(20) NOT NULL,
    [StandardPrice] decimal(18,2) NOT NULL,
    [TaxType] int NOT NULL,
    [Remarks] nvarchar(500) NULL,
    [IsActive] bit NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [RolePermissions] (
    [Id] int NOT NULL IDENTITY,
    [RoleName] nvarchar(50) NOT NULL,
    [MenuKey] nvarchar(50) NOT NULL,
    [MenuName] nvarchar(100) NOT NULL,
    [Controller] nvarchar(50) NULL,
    [Action] nvarchar(50) NULL,
    [IsVisible] bit NOT NULL,
    [Group] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [SystemSettings] (
    [Id] int NOT NULL IDENTITY,
    [SettingKey] nvarchar(50) NOT NULL,
    [SettingValue] nvarchar(500) NOT NULL,
    [Description] nvarchar(200) NULL,
    [Category] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(128) NOT NULL,
    [ProviderKey] nvarchar(128) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [CustomerAddresses] (
    [Id] int NOT NULL IDENTITY,
    [CustomerId] int NOT NULL,
    [Address] nvarchar(300) NOT NULL,
    [IsDefault] bit NOT NULL,
    CONSTRAINT [PK_CustomerAddresses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerAddresses_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [CustomerContacts] (
    [Id] int NOT NULL IDENTITY,
    [CustomerId] int NOT NULL,
    [Name] nvarchar(50) NOT NULL,
    [Phone] nvarchar(20) NULL,
    [Title] nvarchar(50) NULL,
    [IsPrimary] bit NOT NULL,
    CONSTRAINT [PK_CustomerContacts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerContacts_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [ShippingOrders] (
    [Id] int NOT NULL IDENTITY,
    [OrderNo] nvarchar(50) NOT NULL,
    [CustomerId] int NOT NULL,
    [InvoiceNo] nvarchar(50) NULL,
    [OrderDate] datetime2 NOT NULL,
    [ReferenceNo] nvarchar(50) NULL,
    [ProjectNo] nvarchar(50) NULL,
    [PaymentMethod] int NOT NULL,
    [DeliveryMethod] nvarchar(50) NULL,
    [DeliveryAddress] nvarchar(300) NOT NULL,
    [Remarks] nvarchar(500) NULL,
    [OtherExpenses] decimal(18,2) NOT NULL,
    [SubTotal] decimal(18,2) NOT NULL,
    [TaxAmount] decimal(18,2) NOT NULL,
    [Total] decimal(18,2) NOT NULL,
    [Status] int NOT NULL,
    [Handler] nvarchar(100) NULL,
    [ManagerApproved] bit NOT NULL,
    [ManagerName] nvarchar(100) NULL,
    [ManagerApprovedAt] datetime2 NULL,
    [AccountingApproved] bit NOT NULL,
    [AccountingName] nvarchar(100) NULL,
    [AccountingApprovedAt] datetime2 NULL,
    [HandlerApproved] bit NOT NULL,
    [HandlerApprovedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [CreatedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_ShippingOrders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShippingOrders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ProductPriceHistories] (
    [Id] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [ChangedAt] datetime2 NOT NULL,
    [ChangedBy] nvarchar(100) NULL,
    CONSTRAINT [PK_ProductPriceHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductPriceHistories_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [ShippingOrderDetails] (
    [Id] int NOT NULL IDENTITY,
    [ShippingOrderId] int NOT NULL,
    [LineNo] int NOT NULL,
    [ProductId] int NOT NULL,
    [ProductName] nvarchar(200) NULL,
    [PartNo] nvarchar(50) NULL,
    [Quantity] int NOT NULL,
    [Unit] nvarchar(20) NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_ShippingOrderDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShippingOrderDetails_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ShippingOrderDetails_ShippingOrders_ShippingOrderId] FOREIGN KEY ([ShippingOrderId]) REFERENCES [ShippingOrders] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE INDEX [IX_CustomerAddresses_CustomerId] ON [CustomerAddresses] ([CustomerId]);
GO


CREATE INDEX [IX_CustomerContacts_CustomerId] ON [CustomerContacts] ([CustomerId]);
GO


CREATE INDEX [IX_Customers_Name] ON [Customers] ([Name]);
GO


CREATE INDEX [IX_Customers_Phone] ON [Customers] ([Phone]);
GO


CREATE INDEX [IX_Customers_TaxId] ON [Customers] ([TaxId]);
GO


CREATE INDEX [IX_ProductPriceHistories_ProductId] ON [ProductPriceHistories] ([ProductId]);
GO


CREATE INDEX [IX_Products_Name] ON [Products] ([Name]);
GO


CREATE INDEX [IX_Products_PartNo] ON [Products] ([PartNo]);
GO


CREATE INDEX [IX_ShippingOrderDetails_ProductId] ON [ShippingOrderDetails] ([ProductId]);
GO


CREATE INDEX [IX_ShippingOrderDetails_ShippingOrderId] ON [ShippingOrderDetails] ([ShippingOrderId]);
GO


CREATE INDEX [IX_ShippingOrders_CustomerId] ON [ShippingOrders] ([CustomerId]);
GO


CREATE INDEX [IX_ShippingOrders_OrderDate] ON [ShippingOrders] ([OrderDate]);
GO


CREATE UNIQUE INDEX [IX_ShippingOrders_OrderNo] ON [ShippingOrders] ([OrderNo]);
GO


CREATE INDEX [IX_ShippingOrders_Status] ON [ShippingOrders] ([Status]);
GO



