Create Database InscripcionesBrDb
GO

USE [InscripcionesBrDb]
GO


CREATE TABLE [dbo].[Persona] (
    [Id]              INT           IDENTITY (1, 1) NOT NULL,
    [Rut]             NVARCHAR (12) NULL,
    [Nombre]          NVARCHAR (50) NOT NULL,
    [FechaNacimiento] DATE          NOT NULL,
    [Email]           NCHAR (50)    NOT NULL,
    [Direcci�n]       NCHAR (50)    NULL,
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
    [validityYearBegin]             INT           NULL,
    [validityYearFinish]             INT           NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
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
INSERT [dbo].[Persona] ([Id], [Rut], [Nombre], [FechaNacimiento], [Email], [Direcci�n]) VALUES (1, N'10915348-6', N'Mario Abellan', CAST(N'1982-01-15' AS Date), N'marioabellan@gmail.com', N'Galvarino Gallardo 1534')
GO
SET IDENTITY_INSERT [dbo].[Persona] OFF
GO


CREATE TABLE Communes (
  [Id]              INT           IDENTITY (1, 1) NOT NULL PRIMARY KEY,
  Name VARCHAR(50) NOT NULL,
);

INSERT INTO Communes (Name) 
VALUES 
('Algarrobo'),
('Alto Biob�o'),
('Alto del Carmen'),
('Ancud'),
('Andacollo'),
('Angol'),
('Ant�rtica'),
('Antofagasta'),
('Antuco'),
('Arauco'),
('Arica'),
('Arica y Parinacota'),
('Ays�n'),
('Buin'),
('Bulnes'),
('Cabildo'),
('Cabo de Hornos'),
('Cabrero'),
('Calama'),
('Caldera'),
('Calera de Tango'),
('Calle Larga'),
('Canela'),
('Carahue'),
('Cartagena'),
('Casablanca'),
('Castro'),
('Catemu'),
('Cauquenes'),
('Ca�ete'),
('Cerro Navia'),
('Chait�n'),
('Cha�aral'),
('Ch�pica'),
('Chill�n'),
('Chill�n Viejo'),
('Chimbarongo'),
('Cholchol'),
('Cisnes'),
('Cobquecura'),
('Cocham�'),
('Cochrane'),
('Codegua'),
('Coelemu'),
('Coinco'),
('Colb�n'),
('Colchane'),
('Colina'),
('Collipulli'),
('Coltauco'),
('Combarbal�'),
('Concepci�n'),
('Conc�n'),
('Constituci�n'),
('Contulmo'),
('Copiap�'),
('Coquimbo'),
('Coronel'),
('Corral'),
('Coyhaique'),
('Cunco'),
('Curacaut�n'),
('Curacav�'),
('Curaco de V�lez'),
('Curanilahue'),
('Curarrehue'),
('Curepto'),
('Curic�'),
('Dalcahue'),
('Diego de Almagro'),
('Do�ihue'),
('El Bosque'),
('El Carmen'),
('El Monte'),
('El Quisco'),
('El Tabo'),
('Empedrado'),
('Ercilla'),
('Estaci�n Central'),
('Florida'),
('Freire'),
('Freirina'),
('Fresia'),
('Frutillar'),
('Futaleuf�'),
('Futrono'),
('Galvarino'),
('General Lagos'),
('Gorbea'),
('Graneros'),
('Guaitecas'),
('Hijuelas'),
('Hualaihu�'),
('Huala��'),
('Hualp�n'),
('Hualqui'),
('Huara'),
('Huasco'),
('Huechuraba'),
('Illapel'),
('Independencia'),
('Iquique'),
('Isla de Maipo'),
('Isla de Pascua'),
('Juan Fern�ndez'),
('La Calera'),
('La Cisterna'),
('La Cruz'),
('La Estrella'),
('La Florida'),
('La Granja'),
('La Higuera'),
('La Ligua'),
('La Pintana'),
('La Reina'),
('La Serena'),
('La Uni�n'),
('Lago Ranco'),
('Lago Verde'),
('Laguna Blanca'),
('Laja'),
('Lampa'),
('Lampazos'),
('Las Cabras'),
('Las Condes'),
('Lautaro'),
('Lebu'),
('Licant�n'),
('Limache'),
('Linares'),
('Litueche'),
('Llanquihue'),
('Lo Barnechea'),
('Lo Espejo'),
('Lo Prado'),
('Lolol'),
('Loncoche'),
('Longav�'),
('Lonquimay'),
('Los Alamos'),
('Los Andes'),
('Los �ngeles'),
('Los Lagos'),
('Los Muermos'),
('Los Sauces'),
('Los Vilos'),
('Lota'),
('Lumaco'),
('Machal�'),
('Macul'),
('M�fil'),
('Maip�'),
('Malloa'),
('Marchihue'),
('Mar�a Elena'),
('Mar�a Pinto'),
('Mariquina'),
('Maule'),
('Mejillones'),
('Melipeuco'),
('Melipilla'),
('Molina'),
('Monte Patria'),
('Mostazal'),
('Mulch�n'),
('Nacimiento'),
('Nancagua'),
('Natales'),
('Navidad'),
('Negrete'),
('Ninhue'),
('Nogales'),
('Nueva Imperial'),
('�iqu�n'),
('�u�oa'),
('OHiggins'),
('Olivar'),
('Olmu�'),
('Osorno'),
('Ovalle'),
('Padre Hurtado'),
('Padre Las Casas'),
('Paihuano'),
('Palena'),
('Palmilla'),
('Panguipulli'),
('Panquehue'),
('Papudo'),
('Paredones'),
('Parral'),
('Pedro Aguirre Cerda'),
('Pelarco'),
('Pelluhue'),
('Pemuco'),
('Pencahue'),
('Penco'),
('Pe�aflor'),
('Pe�alol�n'),
('Peralillo'),
('Perquenco'),
('Petorca'),
('Peumo'),
('Pica'),
('Pichidegua'),
('Pichilemu'),
('Pinto'),
('Pirque'),
('Pitrufqu�n'),
('Placilla'),
('Portezuelo'),
('Porvenir'),
('Pozo Almonte'),
('Primavera'),
('Providencia'),
('Puchuncav�'),
('Puc�n'),
('Pudahuel'),
('Puente Alto'),
('Puerto Montt'),
('Puerto Octay'),
('Puerto Varas'),
('Pumanque'),
('Punitaqui'),
('Punta Arenas'),
('Puqueld�n'),
('Pur�n'),
('Purranque'),
('Puyehue'),
('Queil�n'),
('Quell�n'),
('Quemchi'),
('Quilaco'),
('Quilicura'),
('Quilleco'),
('Quill�n'),
('Quillota'),
('Quinta de Tilcoco'),
('Quinta Normal'),
('Quintero'),
('Quirihue'),
('Rancagua'),
('R�nquil'),
('Rauco'),
('Recoleta'),
('Renaico'),
('Renca'),
('Rengo'),
('Requ�noa'),
('Retiro'),
('Rinconada'),
('R�o Bueno'),
('R�o Claro'),
('R�o Hurtado'),
('R�o Ib��ez'),
('R�o Negro'),
('R�o Verde'),
('Romeral'),
('Saavedra'),
('Sagrada Familia'),
('Salamanca'),
('San Antonio'),
('San Bernardo'),
('San Carlos'),
('San Clemente'),
('San Esteban'),
('San Fabi�n'),
('San Felipe'),
('San Fernando'),
('San Gregorio'),
('San Ignacio'),
('San Javier'),
('San Joaqu�n'),
('San Jos� de Maipo'),
('San Juan de la Costa'),
('San Miguel'),
('San Nicol�s'),
('San Pablo'),
('San Pedro'),
('San Pedro de Atacama'),
('San Pedro de la Paz'),
('San Rafael'),
('San Ram�n'),
('San Rosendo'),
('San Vicente de Tagua Tagua'),
('Santa B�rbara'),
('Santa Cruz'),
('Santa Juana'),
('Santa Mar�a'),
('Santiago'),
('Santo Domingo'),
('Sierra Gorda'),
('Talagante'),
('Talca'),
('Talcahuano'),
('Taltal'),
('Temuco'),
('Teno'),
('Teodoro Schmidt'),
('Tierra Amarilla'),
('Til Til'),
('Timaukel'),
('Tir�a'),
('Tocopilla'),
('Tolt�n'),
('Tom�'),
('Torres del Paine'),
('Tortel'),
('Traigu�n'),
('Treguaco'),
('Tucapel'),
('Valdivia'),
('Vallenar'),
('Valpara�so'),
('Vichuqu�n'),
('Victoria'),
('Vicu�a'),
('Vilc�n'),
('Villa Alegre'),
('Villa Alemana'),
('Villarrica'),
('Vi�a del Mar'),
('Vitacura'),
('Yerbas Buenas'),
('Yumbel'),
('Yungay');