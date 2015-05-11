-- Script Date: 01.04.2015 10:02

-- use DEFAULT NEWID() since SqlServer CE don't support NEWSEQUENTIALID()
-- with NHibernate we can use GuidComb

CREATE TABLE [Word] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [Title] nvarchar(100) NOT NULL
, [ParentID] uniqueidentifier NULL
);
GO
CREATE TABLE [Vocabulary] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [Title] nvarchar(50) NOT NULL
);
GO
CREATE TABLE [WordTemplate] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [Title] nvarchar(100) NOT NULL
, [VocabularyID] uniqueidentifier NOT NULL
);
GO
--CREATE TABLE [VocabularyWords] (
--  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
--, [VocabularyID] uniqueidentifier NOT NULL
--, [WordID] uniqueidentifier NOT NULL
--);
--GO
CREATE TABLE [UomType] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [Title] nvarchar(50) NOT NULL
, [Ord] int NOT NULL
);
GO
CREATE TABLE [Uom] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [Abbr] nvarchar(20) NOT NULL
, [Description] nvarchar(100) NOT NULL
, [Factor] numeric(18,6) NOT NULL
, [UomTypeID] uniqueidentifier NOT NULL
);
GO
CREATE TABLE [Speciality] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [Title] nvarchar(50) NOT NULL
);
GO
CREATE TABLE [SpecialityVocabularies] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [VocabularyID] uniqueidentifier NOT NULL
, [SpecialityID] uniqueidentifier NOT NULL
);
GO
CREATE TABLE [Patient] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [FirstName] nvarchar(20) NULL
, [MiddleName] nvarchar(20) NULL
, [LastName] nvarchar(50) NULL
, [IsMale] bit NULL
, [BirthYear] int NULL
, [BirthMonth] smallint NULL
, [BirthDay] smallint NULL
, [CreatedAt] datetime DEFAULT GETDATE() NOT NULL
, [UpdatedAt] datetime DEFAULT GETDATE() NOT NULL
);
GO
CREATE TABLE [Passport] (
  [Id] uniqueidentifier NOT NULL
, [HashAndSalt] nchar(70) NULL
, [Remember] bit DEFAULT 0 NOT NULL
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
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [IcdBlockID] int NOT NULL
, [SpecialityID] uniqueidentifier NOT NULL
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
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [Title] nvarchar(20) NOT NULL
, [Ord] int NOT NULL
);
GO
CREATE TABLE [Doctor] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [FirstName] nvarchar(20) NULL
, [MiddleName] nvarchar(20) NULL
, [LastName] nvarchar(20) NOT NULL
, [IsMale] bit NULL
, [SpecialityID] uniqueidentifier NULL
, [CustomVocabularyID] uniqueidentifier NULL
);
GO
CREATE TABLE [Setting] (
  [Id] uniqueidentifier NOT NULL
, [DoctorID] uniqueidentifier NULL
, [Title] nvarchar(255) NOT NULL
, [Value] nvarchar(255) NOT NULL
);
GO
CREATE TABLE [Course] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [PatientID] uniqueidentifier NOT NULL
, [DoctorID] uniqueidentifier NOT NULL
, [StartDate] datetime NOT NULL
, [EndDate] datetime NULL
, [CreatedAt] datetime DEFAULT GETDATE() NOT NULL
, [UpdatedAt] datetime DEFAULT GETDATE() NOT NULL
);
GO
CREATE TABLE [Appointment] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [CourseID] uniqueidentifier NOT NULL
, [DoctorID] uniqueidentifier NOT NULL
, [DateAndTime] datetime NOT NULL
, [CreatedAt] datetime DEFAULT GETDATE() NOT NULL
, [UpdatedAt] datetime DEFAULT GETDATE() NOT NULL
);
GO
CREATE TABLE [HealthRecord] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [HrCategoryID] uniqueidentifier NULL
, [PatientID] uniqueidentifier NULL
, [CourseID] uniqueidentifier NULL
, [AppointmentID] uniqueidentifier NULL
, [DoctorID] uniqueidentifier NOT NULL
, [Ord] tinyint DEFAULT 0 NOT NULL
, [FromDay] smallint NULL
, [FromMonth] smallint NULL
, [FromYear] int NULL
, [Unit] nvarchar(20) NULL
, [IsDeleted] bit DEFAULT 0 NOT NULL
, [CreatedAt] datetime DEFAULT GETDATE() NOT NULL
, [UpdatedAt] datetime DEFAULT GETDATE() NOT NULL
);
GO
CREATE TABLE [HrItem] (
  [Id] uniqueidentifier NOT NULL ROWGUIDCOL DEFAULT NEWID()
, [HealthRecordID] uniqueidentifier NOT NULL
, [Confidence] nvarchar(20) DEFAULT 'Present' NOT NULL
, [Ord] int DEFAULT 0 NOT NULL
, [IcdDiseaseID] int NULL
, [WordID] uniqueidentifier NULL
, [MeasureValue] numeric(18,6) NULL
, [TextRepr] nvarchar(255) NULL
, [UomID] uniqueidentifier NULL
);
GO

-- PK

ALTER TABLE [Word] ADD CONSTRAINT [PK__Word] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Vocabulary] ADD CONSTRAINT [PK__Vocabulary] PRIMARY KEY ([Id]);
GO
ALTER TABLE [WordTemplate] ADD CONSTRAINT [PK__WordTemplate] PRIMARY KEY ([Id]);
GO
--ALTER TABLE [VocabularyWords] ADD CONSTRAINT [PK__VocabularyWords] PRIMARY KEY ([Id]);
--GO
ALTER TABLE [UomType] ADD CONSTRAINT [PK__UomType] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Uom] ADD CONSTRAINT [PK__Uom] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Speciality] ADD CONSTRAINT [PK__Speciality] PRIMARY KEY ([Id]);
GO
ALTER TABLE [SpecialityVocabularies] ADD CONSTRAINT [PK__SpecialityVocabularies] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Patient] ADD CONSTRAINT [PK__Patient] PRIMARY KEY ([Id]);
GO
--ALTER TABLE [Passport] ADD CONSTRAINT [PK__Passport] PRIMARY KEY ([Id]);
--GO
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
ALTER TABLE [Doctor] ADD CONSTRAINT [PK__Doctor] PRIMARY KEY ([Id]);
GO
--ALTER TABLE [Setting] ADD CONSTRAINT [PK__Setting] PRIMARY KEY ([Id]);
--GO
ALTER TABLE [Course] ADD CONSTRAINT [PK__Course] PRIMARY KEY ([Id]);
GO
ALTER TABLE [Appointment] ADD CONSTRAINT [PK__Appointment] PRIMARY KEY ([Id]);
GO
ALTER TABLE [HealthRecord] ADD CONSTRAINT [PK__HealthRecord] PRIMARY KEY ([Id]);
GO
ALTER TABLE [HrItem] ADD CONSTRAINT [PK__HrItem] PRIMARY KEY ([Id]);
GO


-- INDEX

--CREATE UNIQUE INDEX [WordTitle] ON [Word] ([Title] ASC);
--GO
CREATE UNIQUE INDEX [ChapterCode] ON [IcdChapter] ([Code] ASC);
GO
CREATE UNIQUE INDEX [BlockCode] ON [IcdBlock] ([Code] ASC);
GO
CREATE UNIQUE INDEX [DiseaseCode] ON [IcdDisease] ([Code] ASC);
GO


-- FK
-- 3 каскада для упрощения отладки

ALTER TABLE [Word] ADD CONSTRAINT [FK_Word_Word] FOREIGN KEY ([ParentID]) REFERENCES [Word]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [WordTemplate] ADD CONSTRAINT [FK_WordTemplate_Vocabulary] FOREIGN KEY ([VocabularyID]) REFERENCES [Vocabulary]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
--ALTER TABLE [VocabularyWords] ADD CONSTRAINT [FK_VocabularyWords_Vocabulary] FOREIGN KEY ([VocabularyID]) REFERENCES [Vocabulary]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO
--ALTER TABLE [VocabularyWords] ADD CONSTRAINT [FK_VocabularyWords_Word] FOREIGN KEY ([WordID]) REFERENCES [Word]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO
ALTER TABLE [Uom] ADD CONSTRAINT [FK_Uom_UomType] FOREIGN KEY ([UomTypeID]) REFERENCES [UomType]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [SpecialityVocabularies] ADD CONSTRAINT [FK_SpecialityVocabularies_Speciality] FOREIGN KEY ([SpecialityID]) REFERENCES [Speciality]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [SpecialityVocabularies] ADD CONSTRAINT [FK_SpecialityVocabularies_Vocabulary] FOREIGN KEY ([VocabularyID]) REFERENCES [Vocabulary]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [IcdBlock] ADD CONSTRAINT [FK_IcdBlock_IcdChapter] FOREIGN KEY ([ChapterID]) REFERENCES [IcdChapter]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [SpecialityIcdBlocks] ADD CONSTRAINT [FK_SpecialityIcdBlocks_IcdBlo] FOREIGN KEY ([IcdBlockID]) REFERENCES [IcdBlock]([Id]) ON DELETE CASCADE ON UPDATE CASCADE;
 -- удалить связь со специальностью после удаления блока
GO
ALTER TABLE [SpecialityIcdBlocks] ADD CONSTRAINT [FK_SpecialityIcdBlocks_Specia] FOREIGN KEY ([SpecialityID]) REFERENCES [Speciality]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [IcdDisease] ADD CONSTRAINT [FK_IcdDisease_IcdBlock] FOREIGN KEY ([IcdBlockID]) REFERENCES [IcdBlock]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
--ALTER TABLE [Doctor] ADD CONSTRAINT [FK_Doctor_Passport] FOREIGN KEY ([Id]) REFERENCES [Passport]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO
ALTER TABLE [Doctor] ADD CONSTRAINT [FK_Doctor_Speciality] FOREIGN KEY ([SpecialityID]) REFERENCES [Speciality]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [Doctor] ADD CONSTRAINT [FK_Doctor_Vocabulary] FOREIGN KEY ([CustomVocabularyID]) REFERENCES [Vocabulary]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
--ALTER TABLE [Setting] ADD CONSTRAINT [FK_Setting_Doctor] FOREIGN KEY ([DoctorID]) REFERENCES [Doctor]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
--GO
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
ALTER TABLE [HealthRecord] ADD CONSTRAINT [FK_Hr_Doctor] FOREIGN KEY ([DoctorID]) REFERENCES [Doctor]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [HealthRecord] ADD CONSTRAINT [FK_Hr_HrCategory] FOREIGN KEY ([HrCategoryID]) REFERENCES [HrCategory]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [HealthRecord] ADD CONSTRAINT [FK_Hr_Patient] FOREIGN KEY ([PatientID]) REFERENCES [Patient]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [HrItem] ADD CONSTRAINT [FK_HrItem_Hr] FOREIGN KEY ([HealthRecordID]) REFERENCES [HealthRecord]([Id]) ON DELETE CASCADE ON UPDATE CASCADE;
 -- удалить запись со всеми элементами
GO
ALTER TABLE [HrItem] ADD CONSTRAINT [FK_HrItem_IcdDisease] FOREIGN KEY ([IcdDiseaseID]) REFERENCES [IcdDisease]([Id]) ON DELETE SET NULL ON UPDATE CASCADE;
 -- убрать болезнь из записей после удаления болезни
GO
ALTER TABLE [HrItem] ADD CONSTRAINT [FK_HrItem_Uom] FOREIGN KEY ([UomID]) REFERENCES [Uom]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO
ALTER TABLE [HrItem] ADD CONSTRAINT [FK_HrItem_Word] FOREIGN KEY ([WordID]) REFERENCES [Word]([Id]) ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

