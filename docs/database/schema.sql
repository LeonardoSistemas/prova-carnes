IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717231039_Initial'
)
BEGIN
    CREATE TABLE [Carnes] (
        [Id] int NOT NULL IDENTITY,
        [Descricao] nvarchar(200) NOT NULL,
        [Origem] int NOT NULL,
        CONSTRAINT [PK_Carnes] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717231039_Initial'
)
BEGIN
    CREATE TABLE [Estados] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(100) NOT NULL,
        [Uf] nvarchar(2) NOT NULL,
        CONSTRAINT [PK_Estados] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717231039_Initial'
)
BEGIN
    CREATE TABLE [Cidades] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(100) NOT NULL,
        [EstadoId] int NOT NULL,
        CONSTRAINT [PK_Cidades] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Cidades_Estados_EstadoId] FOREIGN KEY ([EstadoId]) REFERENCES [Estados] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717231039_Initial'
)
BEGIN
    CREATE TABLE [Compradores] (
        [Id] int NOT NULL IDENTITY,
        [Nome] nvarchar(150) NOT NULL,
        [Documento] nvarchar(20) NOT NULL,
        [CidadeId] int NOT NULL,
        CONSTRAINT [PK_Compradores] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Compradores_Cidades_CidadeId] FOREIGN KEY ([CidadeId]) REFERENCES [Cidades] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717231039_Initial'
)
BEGIN
    CREATE TABLE [Pedidos] (
        [Id] int NOT NULL IDENTITY,
        [Data] datetime2 NOT NULL,
        [CompradorId] int NOT NULL,
        CONSTRAINT [PK_Pedidos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Pedidos_Compradores_CompradorId] FOREIGN KEY ([CompradorId]) REFERENCES [Compradores] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717231039_Initial'
)
BEGIN
    CREATE TABLE [PedidoItens] (
        [Id] int NOT NULL IDENTITY,
        [PedidoId] int NOT NULL,
        [CarneId] int NOT NULL,
        [Preco] decimal(18,2) NOT NULL,
        [Moeda] int NOT NULL,
        [CotacaoUsada] decimal(18,6) NOT NULL,
        CONSTRAINT [PK_PedidoItens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PedidoItens_Carnes_CarneId] FOREIGN KEY ([CarneId]) REFERENCES [Carnes] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PedidoItens_Pedidos_PedidoId] FOREIGN KEY ([PedidoId]) REFERENCES [Pedidos] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717231039_Initial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nome', N'Uf') AND [object_id] = OBJECT_ID(N'[Estados]'))
        SET IDENTITY_INSERT [Estados] ON;
    EXEC(N'INSERT INTO [Estados] ([Id], [Nome], [Uf])
    VALUES (1, N''São Paulo'', N''SP''),
    (2, N''Rio de Janeiro'', N''RJ''),
    (3, N''Minas Gerais'', N''MG''),
    (4, N''Rio Grande do Sul'', N''RS''),
    (5, N''Paraná'', N''PR''),
    (6, N''Bahia'', N''BA''),
    (7, N''Distrito Federal'', N''DF''),
    (8, N''Santa Catarina'', N''SC''),
    (9, N''Pernambuco'', N''PE''),
    (10, N''Ceará'', N''CE''),
    (11, N''Goiás'', N''GO''),
    (12, N''Espírito Santo'', N''ES''),
    (13, N''Pará'', N''PA''),
    (14, N''Amazonas'', N''AM''),
    (15, N''Mato Grosso'', N''MT''),
    (16, N''Mato Grosso do Sul'', N''MS''),
    (17, N''Maranhão'', N''MA''),
    (18, N''Paraíba'', N''PB''),
    (19, N''Rio Grande do Norte'', N''RN''),
    (20, N''Alagoas'', N''AL''),
    (21, N''Sergipe'', N''SE''),
    (22, N''Piauí'', N''PI''),
    (23, N''Tocantins'', N''TO''),
    (24, N''Rondônia'', N''RO''),
    (25, N''Roraima'', N''RR''),
    (26, N''Acre'', N''AC''),
    (27, N''Amapá'', N''AP'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nome', N'Uf') AND [object_id] = OBJECT_ID(N'[Estados]'))
        SET IDENTITY_INSERT [Estados] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717231039_Initial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'EstadoId', N'Nome') AND [object_id] = OBJECT_ID(N'[Cidades]'))
        SET IDENTITY_INSERT [Cidades] ON;
    EXEC(N'INSERT INTO [Cidades] ([Id], [EstadoId], [Nome])
    VALUES (1, 1, N''São Paulo''),
    (2, 1, N''Campinas''),
    (3, 1, N''Guarulhos''),
    (4, 2, N''Rio de Janeiro''),
    (5, 2, N''Niterói''),
    (6, 3, N''Belo Horizonte''),
    (7, 3, N''Uberlândia''),
    (8, 4, N''Porto Alegre''),
    (9, 5, N''Curitiba''),
    (10, 6, N''Salvador''),
    (11, 7, N''Brasília''),
    (12, 8, N''Florianópolis''),
    (13, 9, N''Recife''),
    (14, 10, N''Fortaleza''),
    (15, 11, N''Goiânia''),
    (16, 12, N''Vitória''),
    (17, 13, N''Belém''),
    (18, 14, N''Manaus''),
    (19, 15, N''Cuiabá''),
    (20, 16, N''Campo Grande''),
    (21, 17, N''São Luís''),
    (22, 18, N''João Pessoa''),
    (23, 19, N''Natal''),
    (24, 20, N''Maceió''),
    (25, 21, N''Aracaju''),
    (26, 22, N''Teresina''),
    (27, 23, N''Palmas''),
    (28, 24, N''Porto Velho''),
    (29, 25, N''Boa Vista''),
    (30, 26, N''Rio Branco''),
    (31, 27, N''Macapá'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'EstadoId', N'Nome') AND [object_id] = OBJECT_ID(N'[Cidades]'))
        SET IDENTITY_INSERT [Cidades] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717231039_Initial'
)
BEGIN
    CREATE INDEX [IX_Cidades_EstadoId] ON [Cidades] ([EstadoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717231039_Initial'
)
BEGIN
    CREATE INDEX [IX_Compradores_CidadeId] ON [Compradores] ([CidadeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717231039_Initial'
)
BEGIN
    CREATE INDEX [IX_PedidoItens_CarneId] ON [PedidoItens] ([CarneId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717231039_Initial'
)
BEGIN
    CREATE INDEX [IX_PedidoItens_PedidoId] ON [PedidoItens] ([PedidoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717231039_Initial'
)
BEGIN
    CREATE INDEX [IX_Pedidos_CompradorId] ON [Pedidos] ([CompradorId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260717231039_Initial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260717231039_Initial', N'8.0.11');
END;
GO

COMMIT;
GO

