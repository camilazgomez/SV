Create Database InscripcionesBrDb
GO

USE [InscripcionesBrDb]
GO


CREATE TABLE [dbo].[Persona] (
    [Id]              INT           IDENTITY (1, 1) NOT NULL,
    [Rut]             NVARCHAR (10) NULL,
    [Nombre]          NVARCHAR (50) NOT NULL,
    [FechaNacimiento] DATE          NOT NULL,
    [Email]           NCHAR (50)    NOT NULL,
    [Dirección]       NCHAR (50)    NULL,
    CONSTRAINT [PK_Persona] PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO


CREATE TABLE [dbo].[RealStateForm] (
    [attentionNumber]   INT           IDENTITY (1, 1) NOT NULL,
    [natureOfTheDeed]   VARCHAR (MAX) NOT NULL,
    [commune]           VARCHAR (MAX) NOT NULL,
    [block]             VARCHAR (MAX) NOT NULL,
    [property]          VARCHAR (MAX) NOT NULL,
    [sheets]            INT           NOT NULL,
    [inscriptionDate]   DATE          NOT NULL,
    [inscriptionNumber] INT           NOT NULL,
    PRIMARY KEY CLUSTERED ([attentionNumber] ASC)
);
GO



CREATE TABLE [dbo].[MultiOwner] (
    [Id]                  INT           IDENTITY (1, 1) NOT NULL,
    [rut]                 NCHAR (10)    NULL,
    [ownershipPercentage] FLOAT (53)    NULL,
    [commune]             VARCHAR (MAX) NOT NULL,
    [block]               VARCHAR (MAX) NOT NULL,
    [property]            VARCHAR (MAX) NOT NULL,
    [sheets]              INT           NOT NULL,
    [inscriptionDate]     DATE          NOT NULL,
    [inscriptionNumber]   INT           NOT NULL,
    [formsId]             INT           NULL,
    [validityYearBegin]             INT           NULL,
    [validityYearFinish]             INT           NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MultiOwner_(ToTableColumn)] FOREIGN KEY ([formsId]) REFERENCES [dbo].[RealStateForm] ([attentionNumber])
);
GO

CREATE TABLE [dbo].[People] (
    [Id]                  INT        IDENTITY (1, 1) NOT NULL,
    [rut]                 NCHAR (10) NULL,
    [ownershipPercentage] FLOAT (53) NULL,
    [uncreditedOwnership] BIT        NULL,
    [formsId]             INT        NULL,
    [seller]              BIT        NULL,
    [heir]                BIT        NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_People_(ToTableColumn)] FOREIGN KEY ([formsId]) REFERENCES [dbo].[RealStateForm] ([attentionNumber])
);
GO



USE [InscripcionesBrDb]
GO
SET IDENTITY_INSERT [dbo].[Persona] ON 
GO
INSERT [dbo].[Persona] ([Id], [Rut], [Nombre], [FechaNacimiento], [Email], [Dirección]) VALUES (1, N'10915348-6', N'Mario Abellan', CAST(N'1982-01-15' AS Date), N'marioabellan@gmail.com', N'Galvarino Gallardo 1534')
GO
SET IDENTITY_INSERT [dbo].[Persona] OFF
GO



USE [InscripcionesBrDb]
GO
SET IDENTITY_INSERT [dbo].[MultiOwner] ON 
GO
INSERT [dbo].[MultiOwner] (
    [Id], 
    [rut],
    [ownershipPercentage],
    [commune],
    [block], 
    [property], 
    [sheets],
    [inscriptionDate],
    [inscriptionNumber], 
    [formsId],
    [validityYearBegin],
    [validityYearFinish]
) 
VALUES 
(1, N'21853297-1', 20, N'Las Condes', N'18829', N'129', 1, CAST(N'2014-11-24' As Date), 23, 1, 2014),
(2, N'25797139-5', 90.4, N'Pudahuel', N'30105', N'57', 2, CAST(N'2013-05-26' As Date), 2, 1,2013 ),
(3, N'20476823-2', 46.4, N'Las Condes', N'18829', N'129', 3, CAST(N'2021-01-10' As Date), 3, 1, 2021),
(4, N'15992415-1', 57, N'Providencia', N'44136', N'79', 4, CAST(N'2019-10-28' As Date), 4, 1, 2019),
(5, N'18507947-9', 60, N'El Bosque', N'20011', N'202', 5, CAST(N'2018-08-01' As Date), 5, 1,2018 ),
(6, N'12841241-1', 62, N'La Cisterna', N'25926', N'125', 6, CAST(N'2019-04-19' As Date), 6, 1,2019 ),
(7, N'10419143-6', 62, N'Huechuraba', N'39058', N'179', 7, CAST(N'2013-07-07' As Date), 7, 1,2013 ),
(8, N'13008246-6', 62.4, N'Independencia', N'54765', N'225', 8, CAST(N'2020-12-24' As Date), 8, 1,2020 ),
(9, N'19995273-8', 59, N'Huechuraba', N'15333', N'47', 9, CAST(N'2011-11-07' As Date), 7 ,1, 2022),
(10, N'20909215-0', 50.4, N'Providencia', N'58060', N'163', 10, CAST(N'2018-09-08' As Date), 8 , 1,2018 ),
(11, N'10915348-6', 90.4, N'Las Condes', N'200', N'200', 11,CAST(N'1982-01-15' AS Date), 1, 1,1982 ),
(12, N'20457896-6', 50.4, N'Vitacura', N'200', N'200', 12,CAST(N'1982-01-15' AS Date), 2, 1, 1982),
(13, N'24531540-6', 59, N'Lo Espejo',    N'1744', N'154', 13, CAST(N'2022-04-28' As Date), 3, 1,2022 ),
(14, N'13126204-8', 50, N'Vitacura', N'1485', N'598', 14, CAST(N'2011-12-15' As Date), 13, 1, 2011),
(15, N'20623952-2', 46, N'Cerrillos', N'1691', N'851', 15, CAST(N'2021-05-13' As Date), 36, 1,2021 ),
(16, N'12893779-7', 32, N'La Reina', N'1326', N'194', 16, CAST(N'2015-10-22' As Date), 8, 1, 2015),
(17, N'13280657-8', 40, N'La Reina', N'1326', N'194', 17, CAST(N'2015-01-19' As Date), 4, 1,2015 )
GO
SET IDENTITY_INSERT [dbo].[MultiOwner] OFF
GO