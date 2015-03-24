CREATE TABLE [UomType] (
  [Id] int IDENTITY (1,1) NOT NULL PRIMARY KEY
, [Title] nvarchar(20) NOT NULL
, [Ord] int NOT NULL
);
GO
CREATE TABLE [Uom] (
  [Id] int IDENTITY (1,1) NOT NULL PRIMARY KEY
, [Abbr] nvarchar(20) NOT NULL
, [Description] nvarchar(100) NULL
, [Factor] numeric(18,6) NOT NULL
, [UomTypeID] int NOT NULL
);
GO
ALTER TABLE [Uom] ADD CONSTRAINT [FK_Uom_UomType] FOREIGN KEY ([UomTypeID]) REFERENCES [UomType]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

INSERT INTO [UomType] ([Title],[Ord]) VALUES (N'Объем',1);
GO
INSERT INTO [UomType] ([Title],[Ord]) VALUES (N'Время',2);
GO


INSERT INTO [Uom] ([Abbr],[Description],[Factor],[UomTypeID]) VALUES (N'л',N'литр',3.000000,1);
GO
INSERT INTO [Uom] ([Abbr],[Description],[Factor],[UomTypeID]) VALUES (N'мл',N'милилитр',0.000000,1);
GO
INSERT INTO [Uom] ([Abbr],[Description],[Factor],[UomTypeID]) VALUES (N'мкл',N'микролитр',-3.000000,1);
GO
INSERT INTO [Uom] ([Abbr],[Description],[Factor],[UomTypeID]) VALUES (N'сут',N'сутки',0.000000,2);
GO
INSERT INTO [Uom] ([Abbr],[Description],[Factor],[UomTypeID]) VALUES (N'нед',N'неделя',0.845100,2);
GO
INSERT INTO [Uom] ([Abbr],[Description],[Factor],[UomTypeID]) VALUES (N'мес',N'месяц',1.477100,2);
GO
INSERT INTO [Uom] ([Abbr],[Description],[Factor],[UomTypeID]) VALUES (N'г',N'год',2.562300,2);
GO
