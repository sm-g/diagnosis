-- Script Date: 04.11.2014 0:15  - ErikEJ.SqlCeScripting version 3.5.2.40
-- Database information:
-- Locale Identifier: 1049
-- Encryption Mode: 
-- Case Sensitive: False
-- Database: C:\Users\smg\Documents\Visual Studio 2012\Projects\Diagnosis\Database\diagnosis.sdf
-- ServerVersion: 4.0.8876.1
-- DatabaseSize: 448 KB
-- Created: 03.11.2014 21:18

-- User Table information:
-- Number of tables: 14
-- Appointment: 0 row(s)
-- Course: 0 row(s)
-- Disease: 0 row(s)
-- Doctor: 2 row(s)
-- HealthRecord: 2 row(s)
-- HrCategory: 6 row(s)
-- HrItem: 3 row(s)
-- IcdBlock: 246 row(s)
-- IcdChapter: 21 row(s)
-- Patient: 5 row(s)
-- Speciality: 1 row(s)
-- SpecialityIcdBlocks: 9 row(s)
-- Uom: 7 row(s)
-- Word: 92 row(s)

CREATE TABLE [Uom] (
  [Id] int IDENTITY (1,1) NOT NULL
, [Abbr] nvarchar(10) NOT NULL
, [Description] nvarchar(100) NULL
, [Factor] float NOT NULL
, [UomType] int NOT NULL
);
GO
CREATE TABLE [Speciality] (
  [Id] int IDENTITY (1,1) NOT NULL
, [Title] nvarchar(20) NOT NULL
);
GO
CREATE TABLE [Patient] (
  [Id] int IDENTITY (1,1) NOT NULL
, [Label] nvarchar(10) NULL
, [FirstName] nvarchar(20) NULL
, [MiddleName] nvarchar(20) NULL
, [LastName] nvarchar(20) NULL
, [IsMale] bit NULL
, [BirthYear] int NULL
, [BirthMonth] smallint NULL
, [BirthDay] smallint NULL
);
GO
CREATE TABLE [IcdChapter] (
  [Id] int IDENTITY (1,1) NOT NULL
, [Title] nvarchar(150) NOT NULL
, [Code] nvarchar(10) NOT NULL
);
GO
CREATE TABLE [IcdBlock] (
  [Id] int IDENTITY (1,1) NOT NULL
, [ChapterID] int NOT NULL
, [Title] nvarchar(150) NOT NULL
, [Code] nvarchar(10) NOT NULL
);
GO
CREATE TABLE [SpecialityIcdBlocks] (
  [Id] int IDENTITY (1,1) NOT NULL
, [SpecialityID] int NOT NULL
, [IcdBlockID] int NOT NULL
);
GO
CREATE TABLE [HrCategory] (
  [Id] int IDENTITY (1,1) NOT NULL
, [Title] nvarchar(20) NOT NULL
, [Ord] int NOT NULL
);
GO
CREATE TABLE [Word] (
  [Id] int IDENTITY (1,1) NOT NULL
, [Title] nvarchar(100) NOT NULL
, [DefHrCategoryID] int NULL
, [ParentID] int NULL
);
GO
CREATE TABLE [Doctor] (
  [Id] int IDENTITY (1,1) NOT NULL
, [FirstName] nvarchar(20) NULL
, [MiddleName] nvarchar(20) NULL
, [LastName] nvarchar(20) NOT NULL
, [IsMale] bit NULL
, [Settings] int NOT NULL
, [SpecialityID] int NULL
);
GO
CREATE TABLE [Disease] (
  [Id] int IDENTITY (1,1) NOT NULL
, [IcdBlockID] int NOT NULL
, [Title] nvarchar(200) NOT NULL
, [Code] nvarchar(10) NOT NULL
);
GO
CREATE TABLE [Course] (
  [Id] int IDENTITY (1,1) NOT NULL
, [PatientID] int NOT NULL
, [DoctorID] int NOT NULL
, [StartDate] datetime NOT NULL
, [EndDate] datetime NULL
);
GO
CREATE TABLE [Appointment] (
  [Id] int IDENTITY (1,1) NOT NULL
, [CourseID] int NOT NULL
, [DoctorID] int NOT NULL
, [DateAndTime] datetime NOT NULL
);
GO
CREATE TABLE [HealthRecord] (
  [Id] int IDENTITY (1,1) NOT NULL
, [PatientID] int NULL
, [CourseID] int NULL
, [AppointmentID] int NULL
, [Comment] nvarchar(1000) NULL
, [HrCategoryID] int NULL
, [FromDay] smallint NULL
, [FromMonth] smallint NULL
, [FromYear] int NULL
, [Unit] nvarchar(20) NULL
, [IsDeleted] bit NOT NULL
);
GO
CREATE TABLE [HrItem] (
  [Id] int IDENTITY (1,1) NOT NULL
, [Ord] int NOT NULL
, [DiseaseID] int NULL
, [WordID] int NULL
, [UomID] int NULL
, [MeasureValue] float NULL
, [HealthRecordID] int NOT NULL
, [TextRepr] nvarchar(255) NULL
);
GO
ALTER TABLE [Uom] ADD CONSTRAINT [PK__Uom__000000000000008E] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Speciality] ADD CONSTRAINT [PK__Speciality__0000000000000055] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Patient] ADD CONSTRAINT [PK__Patient__00000000000000A6] PRIMARY KEY ([Id]);
GO
ALTER TABLE [IcdChapter] ADD CONSTRAINT [PK__IcdChapter__0000000000000029] PRIMARY KEY ([Id]);
GO
ALTER TABLE [IcdBlock] ADD CONSTRAINT [PK__IcdBlock__0000000000000037] PRIMARY KEY ([Id]);
GO
ALTER TABLE [SpecialityIcdBlocks] ADD CONSTRAINT [PK__SpecialityIcdBlocks__0000000000000061] PRIMARY KEY ([Id]);
GO
ALTER TABLE [HrCategory] ADD CONSTRAINT [PK__HrCategory__000000000000000F] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Word] ADD CONSTRAINT [PK__Word__0000000000000171] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Doctor] ADD CONSTRAINT [PK__Doctor__00000000000000BA] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Disease] ADD CONSTRAINT [PK__Disease__0000000000000048] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Course] ADD CONSTRAINT [PK__Course__000000000000011E] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Appointment] ADD CONSTRAINT [PK__Appointment__0000000000000132] PRIMARY KEY ([Id]);
GO
ALTER TABLE [HealthRecord] ADD CONSTRAINT [PK__HealthRecord__0000000000000156] PRIMARY KEY ([Id]);
GO
ALTER TABLE [HrItem] ADD CONSTRAINT [PK__HrItem__000000000000018A] PRIMARY KEY ([Id]);
GO
CREATE UNIQUE INDEX [ChapterCode] ON [IcdChapter] ([Code] ASC);
GO
CREATE UNIQUE INDEX [BlockCode] ON [IcdBlock] ([Code] ASC);
GO
CREATE UNIQUE INDEX [WordTitle] ON [Word] ([Title] ASC);
GO
CREATE UNIQUE INDEX [DiseaseCode] ON [Disease] ([Code] ASC);
GO
ALTER TABLE [IcdBlock] ADD CONSTRAINT [FK_IcdBlock_IcdChapter] FOREIGN KEY ([ChapterID]) REFERENCES [IcdChapter]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [SpecialityIcdBlocks] ADD CONSTRAINT [FK_SpecialityIcdBlocks_IcdBlo] FOREIGN KEY ([IcdBlockID]) REFERENCES [IcdBlock]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [SpecialityIcdBlocks] ADD CONSTRAINT [FK_SpecialityIcdBlocks_Specia] FOREIGN KEY ([SpecialityID]) REFERENCES [Speciality]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [Word] ADD CONSTRAINT [FK_Word_HrCategory] FOREIGN KEY ([DefHrCategoryID]) REFERENCES [HrCategory]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [Word] ADD CONSTRAINT [FK_Word_Word] FOREIGN KEY ([ParentID]) REFERENCES [Word]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [Doctor] ADD CONSTRAINT [FK_Doctor_Speciality] FOREIGN KEY ([SpecialityID]) REFERENCES [Speciality]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [Disease] ADD CONSTRAINT [FK_Disease_IcdBlock] FOREIGN KEY ([IcdBlockID]) REFERENCES [IcdBlock]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [Course] ADD CONSTRAINT [FK_Course_Doctor] FOREIGN KEY ([DoctorID]) REFERENCES [Doctor]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [Course] ADD CONSTRAINT [FK_Course_Patient] FOREIGN KEY ([PatientID]) REFERENCES [Patient]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [Appointment] ADD CONSTRAINT [FK_Appoinmtment_Course] FOREIGN KEY ([CourseID]) REFERENCES [Course]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [Appointment] ADD CONSTRAINT [FK_Appoinmtment_Doctor] FOREIGN KEY ([DoctorID]) REFERENCES [Doctor]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [HealthRecord] ADD CONSTRAINT [FK_Hr_Appointment] FOREIGN KEY ([AppointmentID]) REFERENCES [Appointment]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [HealthRecord] ADD CONSTRAINT [FK_Hr_Course] FOREIGN KEY ([CourseID]) REFERENCES [Course]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [HealthRecord] ADD CONSTRAINT [FK_Hr_HrCategory] FOREIGN KEY ([HrCategoryID]) REFERENCES [HrCategory]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [HealthRecord] ADD CONSTRAINT [FK_Hr_Patient] FOREIGN KEY ([PatientID]) REFERENCES [Patient]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [HrItem] ADD CONSTRAINT [FK_HrItem_Disease] FOREIGN KEY ([DiseaseID]) REFERENCES [Disease]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [HrItem] ADD CONSTRAINT [FK_HrItem_Hr] FOREIGN KEY ([HealthRecordID]) REFERENCES [HealthRecord]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [HrItem] ADD CONSTRAINT [FK_HrItem_Uom] FOREIGN KEY ([UomID]) REFERENCES [Uom]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [HrItem] ADD CONSTRAINT [FK_HrItem_Word] FOREIGN KEY ([WordID]) REFERENCES [Word]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

