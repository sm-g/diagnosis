-- Uom, HrCategory, Speciality, SpecialityIcdBlocks
SET IDENTITY_INSERT [Uom] ON;
GO
INSERT INTO [Uom] ([Id],[Abbr],[Description],[Factor],[UomType]) VALUES (1,'л','литр',3,1);
GO
INSERT INTO [Uom] ([Id],[Abbr],[Description],[Factor],[UomType]) VALUES (2,'мл','милилитр',0,1);
GO
INSERT INTO [Uom] ([Id],[Abbr],[Description],[Factor],[UomType]) VALUES (3,'мкл','микролитр',-3,1);
GO
INSERT INTO [Uom] ([Id],[Abbr],[Description],[Factor],[UomType]) VALUES (4,'сут','сутки',0,2);
GO
INSERT INTO [Uom] ([Id],[Abbr],[Description],[Factor],[UomType]) VALUES (5,'нед','неделя',0.8451,2);
GO
INSERT INTO [Uom] ([Id],[Abbr],[Description],[Factor],[UomType]) VALUES (6,'мес','месяц',1.4771,2);
GO
INSERT INTO [Uom] ([Id],[Abbr],[Description],[Factor],[UomType]) VALUES (7,'г','год',2.5623,2);
GO
SET IDENTITY_INSERT [Uom] OFF;
GO

--
SET IDENTITY_INSERT [HrCategory] ON;
GO
INSERT INTO [HrCategory] ([Id],[Title],[Ord]) VALUES (1,'Жалоба',1);
GO
INSERT INTO [HrCategory] ([Id],[Title],[Ord]) VALUES (2,'История',2);
GO
INSERT INTO [HrCategory] ([Id],[Title],[Ord]) VALUES (3,'Осмотр',3);
GO
INSERT INTO [HrCategory] ([Id],[Title],[Ord]) VALUES (4,'Обследование',4);
GO
INSERT INTO [HrCategory] ([Id],[Title],[Ord]) VALUES (5,'Диагноз',5);
GO
INSERT INTO [HrCategory] ([Id],[Title],[Ord]) VALUES (6,'Лечение',6);
GO
SET IDENTITY_INSERT [HrCategory] OFF;
GO

--
SET IDENTITY_INSERT [Speciality] ON;
GO
INSERT INTO [Speciality] ([Id],[Title]) VALUES (1,'Кардиолог');
GO
SET IDENTITY_INSERT [Speciality] OFF;
GO