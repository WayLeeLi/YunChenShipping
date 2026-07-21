-- ============================================
-- 允晨出貨單管理平台 - 建庫建表腳本 SQL Server
-- 优化版本：补默认值、注释、软删除、业务字段、索引、清除隐藏字符
-- ============================================

-- 建立資料庫
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'YunChenShippingDB')
BEGIN
    CREATE DATABASE [YunChenShippingDB];
END
GO

GO

-- ====================== Identity 身份认证表 AspNetCore 原生 ======================
-- 角色表
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NOT NULL DEFAULT N'',
    [NormalizedName] nvarchar(256) NOT NULL DEFAULT N'',
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO

-- 用户主表
CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [UserName] nvarchar(256) NOT NULL DEFAULT N'',
    [NormalizedUserName] nvarchar(256) NOT NULL DEFAULT N'',
    [Email] nvarchar(256) NOT NULL DEFAULT N'',
    [NormalizedEmail] nvarchar(256) NOT NULL DEFAULT N'',
    [EmailConfirmed] bit NOT NULL DEFAULT 0,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NOT NULL DEFAULT N'',
    [PhoneNumberConfirmed] bit NOT NULL DEFAULT 0,
    [TwoFactorEnabled] bit NOT NULL DEFAULT 0,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL DEFAULT 0,
    [AccessFailedCount] int NOT NULL DEFAULT 0,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);
GO

-- 角色声明
CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY(1,1),
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO

-- 用户声明
CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY(1,1),
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

-- 第三方登录
CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(128) NOT NULL,
    [ProviderKey] nvarchar(128) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

-- 用户角色关联
CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

-- 用户Token
CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO

-- ====================== 业务基础数据表 ======================
-- 客戶主檔
CREATE TABLE [Customers] (
    [Id] int NOT NULL IDENTITY(1,1) -- 客戶ID
    ,[Name] nvarchar(100) NOT NULL DEFAULT N'' -- 客戶名稱
    ,[Phone] nvarchar(20) NOT NULL DEFAULT N'' -- 聯絡電話
    ,[Fax] nvarchar(20) NOT NULL DEFAULT N'' -- 傳真
    ,[ContactPerson] nvarchar(50) NOT NULL DEFAULT N'' -- 主要聯絡人
    ,[TaxId] nvarchar(200) NOT NULL DEFAULT N'' -- 統一編號/稅號
    ,[PaymentMethod] int NOT NULL DEFAULT 0 -- 付款方式 枚举
    ,[Remarks] nvarchar(500) NOT NULL DEFAULT N'' -- 備註
    ,[IsActive] bit NOT NULL DEFAULT 1 -- 是否啟用
    ,[IsDeleted] bit NOT NULL DEFAULT 0 -- 軟刪除
    ,[CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE() -- 建立時間
    ,[CreatedBy] nvarchar(100) NOT NULL DEFAULT N'' -- 建立人員
    ,[UpdatedAt] datetime2 NULL -- 修改時間
    ,[UpdatedBy] nvarchar(100) NULL -- 修改人員
    CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
);
GO

-- 客戶多地址
CREATE TABLE [CustomerAddresses] (
    [Id] int NOT NULL IDENTITY(1,1)
    ,[CustomerId] int NOT NULL -- 客戶ID
    ,[Address] nvarchar(300) NOT NULL DEFAULT N'' -- 完整地址
    ,[IsDefault] bit NOT NULL DEFAULT 0 -- 是否預設送貨地址
    ,[IsDeleted] bit NOT NULL DEFAULT 0
    ,[CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE()
    CONSTRAINT [PK_CustomerAddresses] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerAddresses_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE
);
GO

-- 客戶多聯絡人
CREATE TABLE [CustomerContacts] (
    [Id] int NOT NULL IDENTITY(1,1)
    ,[CustomerId] int NOT NULL -- 客戶ID
    ,[Name] nvarchar(50) NOT NULL DEFAULT N'' -- 聯絡人姓名
    ,[Phone] nvarchar(20) NOT NULL DEFAULT N'' -- 聯絡電話
    ,[Title] nvarchar(50) NOT NULL DEFAULT N'' -- 職稱
    ,[IsPrimary] bit NOT NULL DEFAULT 0 -- 是否主要窗口
    ,[IsDeleted] bit NOT NULL DEFAULT 0
    ,[CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE()
    CONSTRAINT [PK_CustomerContacts] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CustomerContacts_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE CASCADE
);
GO

-- 產品主檔
CREATE TABLE [Products] (
    [Id] int NOT NULL IDENTITY(1,1) -- 產品ID
    ,[Name] nvarchar(200) NOT NULL DEFAULT N'' -- 產品名稱
    ,[PartNo] nvarchar(50) NOT NULL DEFAULT N'' -- 料號
    ,[Unit] nvarchar(20) NOT NULL DEFAULT N'' -- 單位
    ,[StandardPrice] decimal(18,2) NOT NULL DEFAULT 0 -- 標準售價
    ,[TaxType] int NOT NULL DEFAULT 0 -- 稅別 枚举
    ,[Remarks] nvarchar(500) NOT NULL DEFAULT N'' -- 備註
    ,[IsActive] bit NOT NULL DEFAULT 1 -- 是否啟用
    ,[IsDeleted] bit NOT NULL DEFAULT 0 -- 軟刪除
    ,[CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE()
    ,[CreatedBy] nvarchar(100) NOT NULL DEFAULT N''
    ,[UpdatedAt] datetime2 NULL
    ,[UpdatedBy] nvarchar(100) NULL
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
);
GO

-- 產品售價變更歷史
CREATE TABLE [ProductPriceHistories] (
    [Id] int NOT NULL IDENTITY(1,1)
    ,[ProductId] int NOT NULL -- 產品ID
    ,[Price] decimal(18,2) NOT NULL DEFAULT 0 -- 修改後單價
    ,[ChangedAt] datetime2 NOT NULL DEFAULT GETUTCDATE() -- 修改時間
    ,[ChangedBy] nvarchar(100) NOT NULL DEFAULT N'' -- 修改人員
    CONSTRAINT [PK_ProductPriceHistories] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductPriceHistories_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);
GO

-- 角色權限選單配置
CREATE TABLE [RolePermissions] (
    [Id] int NOT NULL IDENTITY(1,1)
    ,[RoleName] nvarchar(50) NOT NULL DEFAULT N'' -- 角色名稱
    ,[MenuKey] nvarchar(50) NOT NULL DEFAULT N'' -- 選單唯一Key
    ,[MenuName] nvarchar(100) NOT NULL DEFAULT N'' -- 選單顯示名稱
    ,[Controller] nvarchar(50) NOT NULL DEFAULT N'' -- 控制器名
    ,[Action] nvarchar(50) NOT NULL DEFAULT N'' -- Action方法
    ,[IsVisible] bit NOT NULL DEFAULT 0 -- 是否顯示選單
    ,[Group] nvarchar(50) NOT NULL DEFAULT N'' -- 選單分組
    CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([Id])
);
GO

-- 系統參數設定
CREATE TABLE [SystemSettings] (
    [Id] int NOT NULL IDENTITY(1,1)
    ,[SettingKey] nvarchar(50) NOT NULL DEFAULT N'' -- 參數Key
    ,[SettingValue] nvarchar(500) NOT NULL DEFAULT N'' -- 參數值
    ,[Description] nvarchar(200) NOT NULL DEFAULT N'' -- 說明
    ,[Category] nvarchar(50) NOT NULL DEFAULT N'' -- 分類
    CONSTRAINT [PK_SystemSettings] PRIMARY KEY ([Id])
);
GO

-- ====================== 出貨單主表 & 明細 ======================
-- 出貨單主檔
CREATE TABLE [ShippingOrders] (
    [Id] int NOT NULL IDENTITY(1,1)
    ,[OrderNo] nvarchar(50) NOT NULL DEFAULT N'' -- 出貨單號 唯一
    ,[CustomerId] int NOT NULL -- 客戶ID
    ,[InvoiceNo] nvarchar(50) NOT NULL DEFAULT N'' -- 發票號
    ,[OrderDate] datetime2 NOT NULL DEFAULT GETUTCDATE() -- 出貨日期
    ,[ReferenceNo] nvarchar(50) NOT NULL DEFAULT N'' -- 客戶參考單號
    ,[ProjectNo] nvarchar(50) NOT NULL DEFAULT N'' -- 專案編號
    ,[PaymentMethod] int NOT NULL DEFAULT 0 -- 付款方式
    ,[DeliveryMethod] nvarchar(50) NOT NULL DEFAULT N'' -- 配送方式
    ,[DeliveryAddress] nvarchar(300) NOT NULL DEFAULT N'' -- 送貨地址
    ,[Remarks] nvarchar(500) NOT NULL DEFAULT N'' -- 備註
    ,[OtherExpenses] decimal(18,2) NOT NULL DEFAULT 0 -- 其他費用
    ,[SubTotal] decimal(18,2) NOT NULL DEFAULT 0 -- 未稅合計
    ,[TaxAmount] decimal(18,2) NOT NULL DEFAULT 0 -- 稅額
    ,[Total] decimal(18,2) NOT NULL DEFAULT 0 -- 總金額(含稅)
    ,[Status] int NOT NULL DEFAULT 0 -- 單據狀態 枚举
    ,[Handler] nvarchar(100) NOT NULL DEFAULT N'' -- 承辦人員
    ,[ManagerApproved] bit NOT NULL DEFAULT 0 -- 主管核准
    ,[ManagerName] nvarchar(100) NOT NULL DEFAULT N'' -- 核准主管
    ,[ManagerApprovedAt] datetime2 NULL -- 主管核准時間
    ,[AccountingApproved] bit NOT NULL DEFAULT 0 -- 財務核准
    ,[AccountingName] nvarchar(100) NOT NULL DEFAULT N'' -- 財務人員
    ,[AccountingApprovedAt] datetime2 NULL -- 財務核准時間
    ,[HandlerApproved] bit NOT NULL DEFAULT 0 -- 承辦確認
    ,[HandlerApprovedAt] datetime2 NULL -- 承辦確認時間
    ,[IsDeleted] bit NOT NULL DEFAULT 0 -- 軟刪除
    ,[CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE()
    ,[CreatedBy] nvarchar(100) NOT NULL DEFAULT N''
    ,[UpdatedAt] datetime2 NULL
    ,[UpdatedBy] nvarchar(100) NULL
    CONSTRAINT [PK_ShippingOrders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShippingOrders_Customers_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Customers] ([Id]) ON DELETE NO ACTION
);
GO

-- 出貨單明細
CREATE TABLE [ShippingOrderDetails] (
    [Id] int NOT NULL IDENTITY(1,1)
    ,[ShippingOrderId] int NOT NULL -- 出貨單主表ID
    ,[LineNo] int NOT NULL DEFAULT 0 -- 行號
    ,[ProductId] int NOT NULL -- 產品ID
    ,[ProductName] nvarchar(200) NOT NULL DEFAULT N'' -- 產品名稱快照
    ,[PartNo] nvarchar(50) NOT NULL DEFAULT N'' -- 料號快照
    ,[Quantity] int NOT NULL DEFAULT 0 -- 數量
    ,[Unit] nvarchar(20) NOT NULL DEFAULT N'' -- 單位快照
    ,[UnitPrice] decimal(18,2) NOT NULL DEFAULT 0 -- 單價快照
    ,[LineAmount] decimal(18,2) NOT NULL DEFAULT 0 -- 本行未稅金額
    ,[LineTax] decimal(18,2) NOT NULL DEFAULT 0 -- 本行稅額
    CONSTRAINT [PK_ShippingOrderDetails] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ShippingOrderDetails_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ShippingOrderDetails_ShippingOrders_ShippingOrderId] FOREIGN KEY ([ShippingOrderId]) REFERENCES [ShippingOrders] ([Id]) ON DELETE CASCADE
);
GO

-- ====================== 索引建立 ======================
-- AspNet 索引
CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO
CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] <> N'';
GO
CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO
CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO
CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO
CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO
CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] <> N'';
GO

-- 客戶索引
CREATE INDEX [IX_CustomerAddresses_CustomerId] ON [CustomerAddresses] ([CustomerId]);
GO
CREATE INDEX [IX_CustomerContacts_CustomerId] ON [CustomerContacts] ([CustomerId]);
GO
CREATE INDEX [IX_Customers_Name] ON [Customers] ([Name], [IsDeleted]);
GO
CREATE INDEX [IX_Customers_Phone] ON [Customers] ([Phone], [IsDeleted]);
GO
CREATE INDEX [IX_Customers_TaxId] ON [Customers] ([TaxId], [IsDeleted]);
GO

-- 產品索引
CREATE INDEX [IX_ProductPriceHistories_ProductId] ON [ProductPriceHistories] ([ProductId]);
GO
CREATE INDEX [IX_Products_Name] ON [Products] ([Name], [IsDeleted]);
GO
CREATE INDEX [IX_Products_PartNo] ON [Products] ([PartNo], [IsDeleted]);
GO

-- 出貨單明細索引
CREATE INDEX [IX_ShippingOrderDetails_ProductId] ON [ShippingOrderDetails] ([ProductId]);
GO
CREATE INDEX [IX_ShippingOrderDetails_ShippingOrderId] ON [ShippingOrderDetails] ([ShippingOrderId]);
GO

-- 出貨單主表索引
CREATE INDEX [IX_ShippingOrders_CustomerId] ON [ShippingOrders] ([CustomerId], [IsDeleted]);
GO
CREATE INDEX [IX_ShippingOrders_OrderDate] ON [ShippingOrders] ([OrderDate], [IsDeleted]);
GO
CREATE UNIQUE INDEX [IX_ShippingOrders_OrderNo] ON [ShippingOrders] ([OrderNo]);
GO
CREATE INDEX [IX_ShippingOrders_Status] ON [ShippingOrders] ([Status], [IsDeleted]);
GO
CREATE INDEX [IX_ShippingOrders_InvoiceNo] ON [ShippingOrders] ([InvoiceNo], [IsDeleted]);
GO
CREATE INDEX [IX_ShippingOrders_ProjectNo] ON [ShippingOrders] ([ProjectNo], [IsDeleted]);
GO
CREATE INDEX [IX_ShippingOrders_ReferenceNo] ON [ShippingOrders] ([ReferenceNo], [IsDeleted]);
GO

PRINT '============================='
PRINT 'YunChenShippingDB 建庫建表完成！'
PRINT '已補全默認值、軟刪除、業務索引、明細金額欄位'
PRINT '============================='
GO