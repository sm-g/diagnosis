-- 2014 12 18

-- creates server's db as client but no word index

-- for sync

CREATE TABLE [Uom] (
  [Id] int IDENTITY (1,1) NOT NULL
, [Abbr] nvarchar(20) NOT NULL
, [Description] nvarchar(100) NULL
, [Factor] numeric(18,6) NOT NULL
, [UomType] int NOT NULL
);
GO
CREATE TABLE [Speciality] (
  [Id] int IDENTITY (1,1) NOT NULL
, [Title] nvarchar(50) NOT NULL
);
GO
CREATE TABLE [Patient] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
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
CREATE TABLE [IcdDisease] (
  [Id] int IDENTITY (1,1) NOT NULL
, [IcdBlockID] int NOT NULL
, [Title] nvarchar(200) NOT NULL
, [Code] nvarchar(10) NOT NULL
);
GO
CREATE TABLE [HrCategory] (
  [Id] int IDENTITY (1,1) NOT NULL
, [Title] nvarchar(20) NOT NULL
, [Ord] int NOT NULL
);
GO
CREATE TABLE [Word] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [Title] nvarchar(100) NOT NULL
, [DefHrCategoryID] int NULL
, [ParentID] uniqueidentifier NULL
);
GO
CREATE TABLE [Doctor] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()  
, [FirstName] nvarchar(20) NULL
, [MiddleName] nvarchar(20) NULL
, [LastName] nvarchar(20) NOT NULL
, [IsMale] bit NULL
, [Settings] int NOT NULL DEFAULT 0
, [SpecialityID] int NULL
);
GO
CREATE TABLE [Course] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID() 
, [PatientID] uniqueidentifier NOT NULL
, [DoctorID] uniqueidentifier NOT NULL
, [StartDate] datetime NOT NULL
, [EndDate] datetime NULL
);
GO
CREATE TABLE [Appointment] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID() 
, [CourseID] uniqueidentifier NOT NULL
, [DoctorID] uniqueidentifier NOT NULL
, [DateAndTime] datetime NOT NULL
);
GO
CREATE TABLE [HealthRecord] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID() 
, [Ord] tinyint NOT NULL DEFAULT 0
, [CreatedAt] datetime NOT NULL DEFAULT GETDATE()
, [PatientID] uniqueidentifier NULL
, [CourseID] uniqueidentifier NULL
, [AppointmentID] uniqueidentifier NULL
, [DoctorID] uniqueidentifier NOT NULL
, [HrCategoryID] int NULL
, [FromDay] smallint NULL
, [FromMonth] smallint NULL
, [FromYear] int NULL
, [Unit] nvarchar(20) NULL
, [IsDeleted] bit NOT NULL DEFAULT 0
);
GO
CREATE TABLE [HrItem] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID() 
, [Ord] int NOT NULL DEFAULT 0
, [IcdDiseaseID] int NULL
, [WordID] uniqueidentifier NULL
, [UomID] int NULL
, [MeasureValue] numeric(18,6) NULL
, [HealthRecordID] uniqueidentifier NOT NULL
, [TextRepr] nvarchar(255) NULL
);
GO
-- PK
ALTER TABLE [Uom] ADD CONSTRAINT [PK__Uom] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Speciality] ADD CONSTRAINT [PK__Speciality] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Patient] ADD CONSTRAINT [PK__Patient] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [IcdChapter] ADD CONSTRAINT [PK__IcdChapter] PRIMARY KEY ([Id]);
GO
ALTER TABLE [IcdBlock] ADD CONSTRAINT [PK__IcdBlock] PRIMARY KEY ([Id]);
GO
ALTER TABLE [SpecialityIcdBlocks] ADD CONSTRAINT [PK__SpecialityIcdBlocks] PRIMARY KEY ([Id]);
GO
ALTER TABLE [IcdDisease] ADD CONSTRAINT [PK__IcdDisease] PRIMARY KEY ([Id]);
GO
ALTER TABLE [HrCategory] ADD CONSTRAINT [PK__HrCategory] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Word] ADD CONSTRAINT [PK__Word] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [Doctor] ADD CONSTRAINT [PK__Doctor] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [Course] ADD CONSTRAINT [PK__Course] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [Appointment] ADD CONSTRAINT [PK__Appointment] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [HealthRecord] ADD CONSTRAINT [PK__HealthRecord] PRIMARY KEY NONCLUSTERED ([Id]);
GO
ALTER TABLE [HrItem] ADD CONSTRAINT [PK__HrItem] PRIMARY KEY NONCLUSTERED ([Id]);
GO
-- INDEX
CREATE UNIQUE INDEX [ChapterCode] ON [IcdChapter] ([Code] ASC);
GO
CREATE UNIQUE INDEX [BlockCode] ON [IcdBlock] ([Code] ASC);
GO
CREATE UNIQUE INDEX [DiseaseCode] ON [IcdDisease] ([Code] ASC);
GO
-- FK
ALTER TABLE [IcdBlock] ADD CONSTRAINT [FK_IcdBlock_IcdChapter] FOREIGN KEY ([ChapterID]) REFERENCES [IcdChapter]([Id]) ON DELETE cascade ON UPDATE cascade;
GO
ALTER TABLE [SpecialityIcdBlocks] ADD CONSTRAINT [FK_SpecialityIcdBlocks_IcdBlo] FOREIGN KEY ([IcdBlockID]) REFERENCES [IcdBlock]([Id]) ON DELETE cascade ON UPDATE cascade;
GO
ALTER TABLE [SpecialityIcdBlocks] ADD CONSTRAINT [FK_SpecialityIcdBlocks_Specia] FOREIGN KEY ([SpecialityID]) REFERENCES [Speciality]([Id]) ON DELETE cascade ON UPDATE cascade;
GO
ALTER TABLE [IcdDisease] ADD CONSTRAINT [FK_IcdDisease_IcdBlock] FOREIGN KEY ([IcdBlockID]) REFERENCES [IcdBlock]([Id]) ON DELETE cascade ON UPDATE cascade;
GO
ALTER TABLE [Word] ADD CONSTRAINT [FK_Word_HrCategory] FOREIGN KEY ([DefHrCategoryID]) REFERENCES [HrCategory]([Id]) ON DELETE set null ON UPDATE cascade;
GO
 -- circle Word
ALTER TABLE [Word] ADD CONSTRAINT [FK_Word_Word] FOREIGN KEY ([ParentID]) REFERENCES [Word]([Id]) ON DELETE no action ON UPDATE no action;
GO
ALTER TABLE [Doctor] ADD CONSTRAINT [FK_Doctor_Speciality] FOREIGN KEY ([SpecialityID]) REFERENCES [Speciality]([Id]) ON DELETE set null ON UPDATE cascade;
GO
ALTER TABLE [Course] ADD CONSTRAINT [FK_Course_Doctor] FOREIGN KEY ([DoctorID]) REFERENCES [Doctor]([Id]) ON DELETE cascade ON UPDATE cascade;
GO
ALTER TABLE [Course] ADD CONSTRAINT [FK_Course_Patient] FOREIGN KEY ([PatientID]) REFERENCES [Patient]([Id]) ON DELETE cascade ON UPDATE cascade;
GO
 --circle - [Course]
ALTER TABLE [Appointment] ADD CONSTRAINT [FK_Appoinmtment_Course] FOREIGN KEY ([CourseID]) REFERENCES [Course]([Id]) ON DELETE no action ON UPDATE no action;
GO
ALTER TABLE [Appointment] ADD CONSTRAINT [FK_Appoinmtment_Doctor] FOREIGN KEY ([DoctorID]) REFERENCES [Doctor]([Id]) ON DELETE no action ON UPDATE no action;
GO
 -- circle - [Patient]
ALTER TABLE [HealthRecord] ADD CONSTRAINT [FK_Hr_Appointment] FOREIGN KEY ([AppointmentID]) REFERENCES [Appointment]([Id]) ON DELETE no action ON UPDATE no action;
GO
ALTER TABLE [HealthRecord] ADD CONSTRAINT [FK_Hr_Course] FOREIGN KEY ([CourseID]) REFERENCES [Course]([Id]) ON DELETE no action ON UPDATE no action;
GO
ALTER TABLE [HealthRecord] ADD CONSTRAINT [FK_Hr_Patient] FOREIGN KEY ([PatientID]) REFERENCES [Patient]([Id]) ON DELETE no action ON UPDATE no action;
GO
ALTER TABLE [HealthRecord] ADD CONSTRAINT [FK_Hr_HrCategory] FOREIGN KEY ([HrCategoryID]) REFERENCES [HrCategory]([Id]) ON DELETE set null ON UPDATE cascade;
GO
ALTER TABLE [HealthRecord] ADD CONSTRAINT [FK_Hr_Doctor] FOREIGN KEY ([DoctorID]) REFERENCES [Doctor]([Id]) ON DELETE no action ON UPDATE no action;
GO
ALTER TABLE [HrItem] ADD CONSTRAINT [FK_HrItem_IcdDisease] FOREIGN KEY ([IcdDiseaseID]) REFERENCES [IcdDisease]([Id]) ON DELETE set null ON UPDATE cascade;
GO
ALTER TABLE [HrItem] ADD CONSTRAINT [FK_HrItem_Hr] FOREIGN KEY ([HealthRecordID]) REFERENCES [HealthRecord]([Id]) ON DELETE cascade ON UPDATE cascade;
GO
ALTER TABLE [HrItem] ADD CONSTRAINT [FK_HrItem_Uom] FOREIGN KEY ([UomID]) REFERENCES [Uom]([Id]) ON DELETE set null ON UPDATE cascade;
GO
 -- circle - [HrCategory]
ALTER TABLE [HrItem] ADD CONSTRAINT [FK_HrItem_Word] FOREIGN KEY ([WordID]) REFERENCES [Word]([Id]) ON DELETE no action ON UPDATE no action;
GO