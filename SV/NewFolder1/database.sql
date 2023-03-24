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
    [inscriptionDate]     INT          NOT NULL,
    [inscriptionNumber]   INT           NOT NULL,
    [formsId]             INT           NULL,
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
INSERT [dbo].[Persona] ([Id], [Rut], [Nombre], [FechaNacimiento], [Email], [Dirección]) VALUES (1, N'10915348-6', N'Mario Abellan', CAST(N'1982-01-15' AS Date), N'marioabellan@gmail.com                            ', N'Galvarino Gallardo 1534                           ')
GO
SET IDENTITY_INSERT [dbo].[Persona] OFF
GO
