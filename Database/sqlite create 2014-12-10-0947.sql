
    PRAGMA foreign_keys = OFF;

    drop table if exists HrItem;

    drop table if exists Uom;

    drop table if exists Appointment;

    drop table if exists HrCategory;

    drop table if exists Speciality;

    drop table if exists SpecialityIcdBlocks;

    drop table if exists IcdDisease;

    drop table if exists IcdBlock;

    drop table if exists IcdChapter;

    drop table if exists Word;

    drop table if exists HealthRecord;

    drop table if exists Doctor;

    drop table if exists Course;

    drop table if exists Patient;

    PRAGMA foreign_keys = ON;

    create table HrItem (
        Id UNIQUEIDENTIFIER not null,
       HealthRecordID UNIQUEIDENTIFIER not null,
       TextRepr TEXT,
       IcdDiseaseID INT,
       WordID UNIQUEIDENTIFIER,
       UomID INT,
       MeasureValue DOUBLE,
       Ord INT default 0  not null,
       primary key (Id),
       constraint FK210074861D873B95 foreign key (HealthRecordID) references HealthRecord,
       constraint FK21007486C1AF1EF4 foreign key (IcdDiseaseID) references IcdDisease,
       constraint FK21007486A79E50D0 foreign key (WordID) references Word,
       constraint FK2100748648B02845 foreign key (UomID) references Uom
    );

    create table Uom (
        Id  integer primary key autoincrement,
       Abbr TEXT not null,
       Description TEXT,
       Factor DOUBLE not null,
       UomType INT not null
    );

    create table Appointment (
        Id UNIQUEIDENTIFIER not null,
       CourseID UNIQUEIDENTIFIER not null,
       DoctorID UNIQUEIDENTIFIER not null,
       DateAndTime DATETIME not null,
       primary key (Id),
       constraint FKBB6A20C0FFADC614 foreign key (CourseID) references Course,
       constraint FKBB6A20C05F10938C foreign key (DoctorID) references Doctor
    );

    create table HrCategory (
        Id  integer primary key autoincrement,
       Title TEXT not null,
       Ord INT not null
    );

    create table Speciality (
        Id  integer primary key autoincrement,
       Title TEXT not null
    );

    create table SpecialityIcdBlocks (
        Id  integer primary key autoincrement,
       SpecialityID INT not null,
       IcdBlockID INT not null,
       constraint FK204EF8A92FCCEEF5 foreign key (SpecialityID) references Speciality,
       constraint FK204EF8A94721EC9B foreign key (IcdBlockID) references IcdBlock
    );

    create table IcdDisease (
        Id  integer primary key autoincrement,
       IcdBlockID INT not null,
       Title TEXT not null,
       Code TEXT not null,
      unique (Title),
       constraint FK9063D7F54721EC9B foreign key (IcdBlockID) references IcdBlock
    );

    create table IcdBlock (
        Id  integer primary key autoincrement,
       ChapterID INT not null,
       Title TEXT not null,
       Code TEXT not null,
      unique (Title),
       constraint FK6EBF47EA3F6226F7 foreign key (ChapterID) references IcdChapter
    );

    create table IcdChapter (
        Id  integer primary key autoincrement,
       Title TEXT not null,
       Code TEXT not null,
      unique (Title)
    );

    create table Word (
        Id UNIQUEIDENTIFIER not null,
       Title TEXT not null,
       DefHrCategoryID INT,
       ParentID UNIQUEIDENTIFIER,
       primary key (Id),
      unique (Title),
       constraint FK91F219F91EEA4CA5 foreign key (DefHrCategoryID) references HrCategory,
       constraint FK91F219F9B19D34E5 foreign key (ParentID) references Word
    );

    create table HealthRecord (
        Id UNIQUEIDENTIFIER not null,
       PatientID UNIQUEIDENTIFIER,
       CourseID UNIQUEIDENTIFIER,
       AppointmentID UNIQUEIDENTIFIER,
       IsDeleted BOOL default 0  not null,
       HrCategoryID INT,
       FromDay TINYINT,
       FromMonth TINYINT,
       FromYear INT,
       Unit TEXT,
       primary key (Id),
       constraint FK80ADA1CA1EB4737E foreign key (PatientID) references Patient,
       constraint FK80ADA1CAFFADC614 foreign key (CourseID) references Course,
       constraint FK80ADA1CA2166F67F foreign key (AppointmentID) references Appointment,
       constraint FK80ADA1CA4D800310 foreign key (HrCategoryID) references HrCategory
    );

    create table Doctor (
        Id UNIQUEIDENTIFIER not null,
       FirstName TEXT not null,
       MiddleName TEXT,
       LastName TEXT,
       IsMale BOOL,
       Settings INT default 0  not null,
       SpecialityID INT,
       primary key (Id),
       constraint FKCE4594AA2FCCEEF5 foreign key (SpecialityID) references Speciality
    );

    create table Course (
        Id UNIQUEIDENTIFIER not null,
       PatientID UNIQUEIDENTIFIER not null,
       DoctorID UNIQUEIDENTIFIER not null,
       StartDate DATETIME not null,
       EndDate DATETIME,
       primary key (Id),
       constraint FKFFF434031EB4737E foreign key (PatientID) references Patient,
       constraint FKFFF434035F10938C foreign key (DoctorID) references Doctor
    );

    create table Patient (
        Id UNIQUEIDENTIFIER not null,
       FirstName TEXT,
       MiddleName TEXT,
       LastName TEXT,
       IsMale BOOL,
       BirthYear INT,
       BirthMonth TINYINT,
       BirthDay TINYINT,
       primary key (Id)
    );
