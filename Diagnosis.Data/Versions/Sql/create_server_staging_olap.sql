--2015 02 06

-- snowflake with Doctors

-- dbo - справочники
-- sync - framework
-- staging - первичные данные, для синхронизации
-- olap - обработанные, + Hash SHA256 по всем колонкам кроме ID

CREATE SCHEMA [sync]
GO
CREATE SCHEMA [staging]
GO
CREATE SCHEMA [olap]
GO

CREATE TABLE [dbo].[UomType] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [Title] nvarchar(20) NOT NULL
, [Ord] int NOT NULL
);
GO
CREATE TABLE [dbo].[Uom] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [Abbr] nvarchar(20) NOT NULL
, [Description] nvarchar(100) NULL
, [Factor] float NOT NULL
, [UomTypeID] uniqueidentifier NOT NULL
);
GO
CREATE TABLE [dbo].[Speciality] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [Title] nvarchar(50) NOT NULL
);
GO
CREATE TABLE [dbo].[IcdChapter] (
  [Id] int IDENTITY (1,1) NOT NULL
, [Title] nvarchar(150) NOT NULL
, [Code] nvarchar(10) NOT NULL
);
GO
CREATE TABLE [dbo].[IcdBlock] (
  [Id] int IDENTITY (1,1) NOT NULL
, [ChapterID] int NOT NULL
, [Title] nvarchar(150) NOT NULL
, [Code] nvarchar(10) NOT NULL
);
GO
CREATE TABLE [dbo].[SpecialityIcdBlocks] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [SpecialityID] uniqueidentifier NOT NULL
, [IcdBlockID] uniqueidentifier NOT NULL
);
GO
CREATE TABLE [dbo].[IcdDisease] (
  [Id] int IDENTITY (1,1) NOT NULL
, [IcdBlockID] int NOT NULL
, [Title] nvarchar(200) NOT NULL
, [Code] nvarchar(10) NOT NULL
);
GO
CREATE TABLE [dbo].[HrCategory] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [Title] nvarchar(20) NOT NULL
, [Ord] int NOT NULL
);
GO

-- ???
/* CREATE TABLE [staging].[Setting] (
  [Id] uniqueidentifier NOT NULL
, [Title] nvarchar(255) DEFAULT '' NOT NULL
, [Value] nvarchar(255) DEFAULT '' NOT NULL
, [DoctorID] uniqueidentifier NULL
);
GO

ALTER TABLE [Setting] ADD CONSTRAINT [PK__Setting] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Setting] ADD CONSTRAINT [FK_Setting_Doctor] FOREIGN KEY ([DoctorID]) REFERENCES [Doctor]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO */

 
-- staging

CREATE TABLE [staging].[Word] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [Title] nvarchar(100) NOT NULL
, [ParentID] uniqueidentifier NULL
);
GO

CREATE TABLE [staging].[Doctor] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()  
, [FirstName] nvarchar(20) NULL
, [MiddleName] nvarchar(20) NULL
, [LastName] nvarchar(20) NOT NULL
, [IsMale] bit NULL
, [SpecialityID] uniqueidentifier NULL
);
GO
CREATE TABLE [staging].[Patient] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [FirstName] nvarchar(20) NULL
, [MiddleName] nvarchar(20) NULL
, [LastName] nvarchar(20) NULL
, [IsMale] bit NULL
, [BirthYear] smallint NULL
, [BirthMonth] tinyint NULL
, [BirthDay] tinyint NULL
, [CreatedAt] datetime DEFAULT GETDATE() NOT NULL
, [UpdatedAt] datetime DEFAULT GETDATE() NOT NULL
);
GO
CREATE TABLE [staging].[Course] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID() 
, [PatientID] uniqueidentifier NOT NULL
, [DoctorID] uniqueidentifier NOT NULL
, [StartDate] datetime NOT NULL
, [EndDate] datetime NULL
, [CreatedAt] datetime DEFAULT GETDATE() NOT NULL
, [UpdatedAt] datetime DEFAULT GETDATE() NOT NULL
);
GO
CREATE TABLE [staging].[Appointment] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID() 
, [CourseID] uniqueidentifier NOT NULL
, [DoctorID] uniqueidentifier NOT NULL
, [DateAndTime] datetime NOT NULL
, [CreatedAt] datetime DEFAULT GETDATE() NOT NULL
, [UpdatedAt] datetime DEFAULT GETDATE() NOT NULL
);
GO
CREATE TABLE [staging].[HealthRecord] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID() 
, [Ord] int NOT NULL DEFAULT 0
, [CreatedAt] datetime DEFAULT GETDATE() NOT NULL
, [UpdatedAt] datetime DEFAULT GETDATE() NOT NULL
, [PatientID] uniqueidentifier NULL
, [CourseID] uniqueidentifier NULL
, [AppointmentID] uniqueidentifier NULL
, [DoctorID] uniqueidentifier NOT NULL
, [HrCategoryID] uniqueidentifier NULL
, [FromYear] smallint NULL
, [FromMonth] tinyint NULL
, [FromDay] tinyint NULL
, [Unit] nvarchar(20) NULL
);
GO
CREATE TABLE [staging].[HrItem] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID() 
, [Ord] int NOT NULL DEFAULT 0
, [IcdDiseaseID] int NULL
, [WordID] uniqueidentifier NULL
, [UomID] uniqueidentifier NULL
, [MeasureValue] numeric(18,6) NULL
, [HealthRecordID] uniqueidentifier NOT NULL
);
GO

-- olap

CREATE TABLE [olap].[Word] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [Title] nvarchar(100) NOT NULL
, [ParentID] uniqueidentifier NULL
, [Hash] char(66) NOT NULL
);
GO

CREATE TABLE [olap].[Doctor] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()  
, [FirstName] nvarchar(20) NULL
, [MiddleName] nvarchar(20) NULL
, [LastName] nvarchar(20) NOT NULL
, [IsMale] bit NULL
, [SpecialityID] uniqueidentifier NULL
, [Hash] char(66) NOT NULL
);
GO
CREATE TABLE [olap].[Patient] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [FirstName] nvarchar(20) NULL
, [MiddleName] nvarchar(20) NULL
, [LastName] nvarchar(20) NULL
, [IsMale] bit NULL
, [BirthYear] smallint NULL
, [BirthMonth] tinyint NULL
, [BirthDay] tinyint NULL
, [Hash] char(66) NOT NULL
);
GO
CREATE TABLE [olap].[Course] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID() 
, [PatientID] uniqueidentifier NOT NULL
, [DoctorID] uniqueidentifier NULL
, [StartDate] datetime NULL
, [EndDate] datetime NULL
, [Hash] char(66) NOT NULL
);
GO
CREATE TABLE [olap].[Appointment] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID() 
, [CourseID] uniqueidentifier NOT NULL
, [DoctorID] uniqueidentifier NULL
, [DateAndTime] datetime NULL
, [Hash] char(66) NOT NULL
);
GO
CREATE TABLE [olap].[HealthRecord] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID() 
, [CreatedAt] datetime NOT NULL
, [AppointmentID] uniqueidentifier NOT NULL
, [DoctorID] uniqueidentifier NOT NULL
, [HrCategoryID] uniqueidentifier NULL
, [FromYear] smallint NULL
, [FromMonth] tinyint NULL
, [FromDay] tinyint NULL
, [Unit] nvarchar(20) NULL
, [Hash] char(66) NOT NULL
);
GO
CREATE TABLE [olap].[HrItem] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID() 
, [IcdDiseaseID] int NULL
, [OriginalWordID] uniqueidentifier NULL
, [ReplaceWordID] uniqueidentifier NULL
, [UomID] uniqueidentifier NULL
, [MeasureValue] numeric(18,6) NULL
, [HealthRecordID] uniqueidentifier NOT NULL
, [Hash] char(66) NOT NULL
);
GO

-- PK
ALTER TABLE [dbo].[UomType] ADD CONSTRAINT [PK__UomType] PRIMARY KEY ([Id]);
GO
ALTER TABLE [dbo].[Uom] ADD CONSTRAINT [PK__Uom] PRIMARY KEY ([Id]);
GO
ALTER TABLE [dbo].[Speciality] ADD CONSTRAINT [PK__Speciality] PRIMARY KEY ([Id]);
GO
ALTER TABLE [dbo].[IcdChapter] ADD CONSTRAINT [PK__IcdChapter] PRIMARY KEY ([Id]);
GO
ALTER TABLE [dbo].[IcdBlock] ADD CONSTRAINT [PK__IcdBlock] PRIMARY KEY ([Id]);
GO
ALTER TABLE [dbo].[SpecialityIcdBlocks] ADD CONSTRAINT [PK__SpecialityIcdBlocks] PRIMARY KEY ([Id]);
GO
ALTER TABLE [dbo].[IcdDisease] ADD CONSTRAINT [PK__IcdDisease] PRIMARY KEY ([Id]);
GO
ALTER TABLE [dbo].[HrCategory] ADD CONSTRAINT [PK__HrCategory] PRIMARY KEY ([Id]);
GO

ALTER TABLE [staging].[Word] ADD CONSTRAINT [PK_staging_Word] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [staging].[Doctor] ADD CONSTRAINT [PK_staging_Doctor] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [staging].[Patient] ADD CONSTRAINT [PK_staging_Patient] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [staging].[Course] ADD CONSTRAINT [PK_staging_Course] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [staging].[Appointment] ADD CONSTRAINT [PK_staging_Appointment] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [staging].[HealthRecord] ADD CONSTRAINT [PK_staging_HealthRecord] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [staging].[HrItem] ADD CONSTRAINT [PK_staging_HrItem] PRIMARY KEY NONCLUSTERED ([Id]);
GO

ALTER TABLE [olap].[Word] ADD CONSTRAINT [PK_olap_Word] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [olap].[Doctor] ADD CONSTRAINT [PK_olap_Doctor] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [olap].[Patient] ADD CONSTRAINT [PK_olap_Patient] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [olap].[Course] ADD CONSTRAINT [PK_olap_Course] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [olap].[Appointment] ADD CONSTRAINT [PK_olap_Appointment] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [olap].[HealthRecord] ADD CONSTRAINT [PK_olap_HealthRecord] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [olap].[HrItem] ADD CONSTRAINT [PK_olap_HrItem] PRIMARY KEY NONCLUSTERED ([Id]);
GO
-- INDEX
CREATE UNIQUE INDEX [ChapterCode] ON [IcdChapter] ([Code] ASC);
GO
CREATE UNIQUE INDEX [BlockCode] ON [IcdBlock] ([Code] ASC);
GO
CREATE UNIQUE INDEX [DiseaseCode] ON [IcdDisease] ([Code] ASC);
GO
-- FK

--dbo

ALTER TABLE [IcdBlock] ADD CONSTRAINT [FK_IcdBlock_IcdChapter] FOREIGN KEY ([ChapterID]) REFERENCES [IcdChapter]([Id]) ON DELETE cascade ON UPDATE cascade;
GO
ALTER TABLE [SpecialityIcdBlocks] ADD CONSTRAINT [FK_SpecialityIcdBlocks_IcdBlo] FOREIGN KEY ([IcdBlockID]) REFERENCES [IcdBlock]([Id]) ON DELETE cascade ON UPDATE cascade;
GO
ALTER TABLE [SpecialityIcdBlocks] ADD CONSTRAINT [FK_SpecialityIcdBlocks_Specia] FOREIGN KEY ([SpecialityID]) REFERENCES [Speciality]([Id]) ON DELETE cascade ON UPDATE cascade;
GO
ALTER TABLE [IcdDisease] ADD CONSTRAINT [FK_IcdDisease_IcdBlock] FOREIGN KEY ([IcdBlockID]) REFERENCES [IcdBlock]([Id]) ON DELETE cascade ON UPDATE cascade;
GO
ALTER TABLE [Uom] ADD CONSTRAINT [FK_Uom_UomType] FOREIGN KEY ([UomTypeID]) REFERENCES [UomType]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- staging

ALTER TABLE [staging].[Word] ADD CONSTRAINT [FK_staging_Word_HrCategory] FOREIGN KEY ([DefHrCategoryID]) REFERENCES [HrCategory]([Id]) ON DELETE set null ON UPDATE cascade;
GO
 -- circle Word
ALTER TABLE [staging].[Word] ADD CONSTRAINT [FK_staging_Word_Word] FOREIGN KEY ([ParentID]) REFERENCES [staging].[Word]([Id]) ON DELETE no action ON UPDATE no action;
GO
ALTER TABLE [staging].[Doctor] ADD CONSTRAINT [FK_staging_Doctor_Speciality] FOREIGN KEY ([SpecialityID]) REFERENCES [Speciality]([Id]) ON DELETE set null ON UPDATE cascade;
GO
ALTER TABLE [staging].[Course] ADD CONSTRAINT [FK_staging_Course_Doctor] FOREIGN KEY ([DoctorID]) REFERENCES [staging].[Doctor]([Id]) ON DELETE cascade ON UPDATE cascade;
GO
ALTER TABLE [staging].[Course] ADD CONSTRAINT [FK_staging_Course_Patient] FOREIGN KEY ([PatientID]) REFERENCES [staging].[Patient]([Id]) ON DELETE cascade ON UPDATE cascade;
GO
 --circle - [staging].[Course]
ALTER TABLE [staging].[Appointment] ADD CONSTRAINT [FK_staging_Appoinmtment_Course] FOREIGN KEY ([CourseID]) REFERENCES [staging].[Course]([Id]) ON DELETE no action ON UPDATE no action;
GO
ALTER TABLE [staging].[Appointment] ADD CONSTRAINT [FK_staging_Appoinmtment_Doctor] FOREIGN KEY ([DoctorID]) REFERENCES [staging].[Doctor]([Id]) ON DELETE no action ON UPDATE no action;
GO
 -- circle - [staging].[Patient]
ALTER TABLE [staging].[HealthRecord] ADD CONSTRAINT [FK_staging_Hr_Appointment] FOREIGN KEY ([AppointmentID]) REFERENCES [staging].[Appointment]([Id]) ON DELETE no action ON UPDATE no action;
GO
ALTER TABLE [staging].[HealthRecord] ADD CONSTRAINT [FK_staging_Hr_Course] FOREIGN KEY ([CourseID]) REFERENCES [staging].[Course]([Id]) ON DELETE no action ON UPDATE no action;
GO
ALTER TABLE [staging].[HealthRecord] ADD CONSTRAINT [FK_staging_Hr_Patient] FOREIGN KEY ([PatientID]) REFERENCES [staging].[Patient]([Id]) ON DELETE no action ON UPDATE no action;
GO
ALTER TABLE [staging].[HealthRecord] ADD CONSTRAINT [FK_staging_Hr_Doctor] FOREIGN KEY ([DoctorID]) REFERENCES [staging].[Doctor]([Id]) ON DELETE no action ON UPDATE no action;
GO
ALTER TABLE [staging].[HealthRecord] ADD CONSTRAINT [FK_staging_Hr_HrCategory] FOREIGN KEY ([HrCategoryID]) REFERENCES [HrCategory]([Id]) ON DELETE set null ON UPDATE cascade;
GO
ALTER TABLE [staging].[HrItem] ADD CONSTRAINT [FK_staging_HrItem_IcdDisease] FOREIGN KEY ([IcdDiseaseID]) REFERENCES [IcdDisease]([Id]) ON DELETE set null ON UPDATE cascade;
GO
ALTER TABLE [staging].[HrItem] ADD CONSTRAINT [FK_staging_HrItem_Hr] FOREIGN KEY ([HealthRecordID]) REFERENCES [staging].[HealthRecord]([Id]) ON DELETE cascade ON UPDATE cascade;
GO
ALTER TABLE [staging].[HrItem] ADD CONSTRAINT [FK_staging_HrItem_Uom] FOREIGN KEY ([UomID]) REFERENCES [Uom]([Id]) ON DELETE set null ON UPDATE cascade;
GO
 -- circle - [HrCategory]
ALTER TABLE [staging].[HrItem] ADD CONSTRAINT [FK_staging_HrItem_Word] FOREIGN KEY ([WordID]) REFERENCES [staging].[Word]([Id]) ON DELETE no action ON UPDATE no action;
GO

-- olap

 -- circle [olap].[Word]
ALTER TABLE [olap].[Word] ADD CONSTRAINT [FK_olap_Word_Word] FOREIGN KEY ([ParentID]) REFERENCES [olap].[Word]([Id]) ON DELETE no action ON UPDATE no action;
GO
ALTER TABLE [olap].[Doctor] ADD CONSTRAINT [FK_olap_Doctor_Speciality] FOREIGN KEY ([SpecialityID]) REFERENCES [Speciality]([Id]) ON DELETE set null ON UPDATE cascade;
GO
ALTER TABLE [olap].[Course] ADD CONSTRAINT [FK_olap_Course_Doctor] FOREIGN KEY ([DoctorID]) REFERENCES [olap].[Doctor]([Id]) ON DELETE cascade ON UPDATE cascade;
GO
ALTER TABLE [olap].[Course] ADD CONSTRAINT [FK_olap_Course_Patient] FOREIGN KEY ([PatientID]) REFERENCES [olap].[Patient]([Id]) ON DELETE cascade ON UPDATE cascade;
GO
 --circle - [olap].[Course]
ALTER TABLE [olap].[Appointment] ADD CONSTRAINT [FK_olap_Appoinmtment_Course] FOREIGN KEY ([CourseID]) REFERENCES [olap].[Course]([Id]) ON DELETE no action ON UPDATE no action;
GO
ALTER TABLE [olap].[Appointment] ADD CONSTRAINT [FK_olap_Appoinmtment_Doctor] FOREIGN KEY ([DoctorID]) REFERENCES [olap].[Doctor]([Id]) ON DELETE no action ON UPDATE no action;
GO
 -- circle - [olap].[Patient]
ALTER TABLE [olap].[HealthRecord] ADD CONSTRAINT [FK_olap_Hr_Appointment] FOREIGN KEY ([AppointmentID]) REFERENCES [olap].[Appointment]([Id]) ON DELETE no action ON UPDATE no action;
GO
ALTER TABLE [olap].[HealthRecord] ADD CONSTRAINT [FK_olap_Hr_Doctor] FOREIGN KEY ([DoctorID]) REFERENCES [olap].[Doctor]([Id]) ON DELETE no action ON UPDATE no action;
GO
ALTER TABLE [olap].[HealthRecord] ADD CONSTRAINT [FK_olap_Hr_HrCategory] FOREIGN KEY ([HrCategoryID]) REFERENCES [HrCategory]([Id]) ON DELETE set null ON UPDATE cascade;
GO
ALTER TABLE [olap].[HrItem] ADD CONSTRAINT [FK_olap_HrItem_IcdDisease] FOREIGN KEY ([IcdDiseaseID]) REFERENCES [IcdDisease]([Id]) ON DELETE set null ON UPDATE cascade;
GO
ALTER TABLE [olap].[HrItem] ADD CONSTRAINT [FK_olap_HrItem_Hr] FOREIGN KEY ([HealthRecordID]) REFERENCES [olap].[HealthRecord]([Id]) ON DELETE cascade ON UPDATE cascade;
GO
ALTER TABLE [olap].[HrItem] ADD CONSTRAINT [FK_olap_HrItem_Uom] FOREIGN KEY ([UomID]) REFERENCES [Uom]([Id]) ON DELETE set null ON UPDATE cascade;
GO
 -- circle - [HrCategory]
ALTER TABLE [olap].[HrItem] ADD CONSTRAINT [FK_olap_HrItem_OriginalWord] FOREIGN KEY ([OriginalWordID]) REFERENCES [olap].[Word]([Id]) ON DELETE no action ON UPDATE no action;
GO
ALTER TABLE [olap].[HrItem] ADD CONSTRAINT [FK_olap_HrItem_ReplaceWord] FOREIGN KEY ([ReplaceWordID]) REFERENCES [olap].[Word]([Id]) ON DELETE no action ON UPDATE no action;
GO

