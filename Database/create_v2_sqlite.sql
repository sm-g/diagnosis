
    PRAGMA foreign_keys = OFF;
    drop table if exists Uom;
    drop table if exists Appointment;
    drop table if exists HrCategory;
    drop table if exists Speciality;
    drop table if exists SpecialityIcdBlocks;
    drop table if exists Disease;
    drop table if exists IcdBlock;
    drop table if exists IcdChapter;
    drop table if exists Word;
    drop table if exists HealthRecord;
    drop table if exists HrItem;
    drop table if exists Doctor;
    drop table if exists Course;
    drop table if exists Patient;
    PRAGMA foreign_keys = ON;

    create table Appointment (
        Id integer primary key autoincrement,
        CourseID INT not null,
        DoctorID INT not null,
        DateAndTime DATETIME not null,
        constraint FK_Appoinmtment_Course foreign key (CourseID) references Course,
        constraint FK_Appoinmtment_Doctor foreign key (DoctorID) references Doctor
    );
    create table Course (
        Id integer primary key autoincrement,
        PatientID INT not null,
        DoctorID INT not null,
        StartDate DATETIME not null,
        EndDate DATETIME,
        constraint FK_Course_Patient foreign key (PatientID) references Patient,
        constraint FK_Course_Doctor foreign key (DoctorID) references Doctor
    );

    create table Disease (
        Id integer primary key autoincrement,
        IcdBlockID INT not null,
        Title TEXT not null,
        Code TEXT not null,
        constraint FK_Disease_IcdBlock foreign key (IcdBlockID) references IcdBlock
    );

    create table Doctor (
        Id integer primary key autoincrement,
        FirstName TEXT,
        MiddleName TEXT,
        LastName TEXT not null,
        IsMale BOOL,
        Settings INT not null,
        SpecialityID INT,
        constraint FK_Doctor_Speciality foreign key (SpecialityID) references Speciality
    );

    create table HealthRecord (
        Id integer primary key autoincrement,
        PatientID INT,
        CourseID INT,
        AppointmentID INT,
        Comment TEXT,
        HrCategoryID INT,
        FromDay TINYINT,
        FromMonth TINYINT,
        FromYear INT,
        Unit TEXT,
        IsDeleted BOOL not null,
        constraint FK_Hr_HrCategory foreign key (HrCategoryID) references HrCategory,
        constraint FK_Hr_Patient foreign key (PatientID) references Patient,
        constraint FK_Hr_Course foreign key (CourseID) references Course
        constraint FK_Hr_Appointment foreign key (AppointmentID) references Appointment,
        constraint CHK_Hr_PatientCourseAppointment CHECK ( -- patient or course or app
        (CASE WHEN PatientID IS NOT NULL THEN 1 ELSE 0 END
            + CASE WHEN CourseID IS NOT NULL THEN 1 ELSE 0 END
            + CASE WHEN AppointmentID IS NOT NULL THEN 1 ELSE 0 END)
            = 1
        )
    );

    create table HrCategory (
        Id integer primary key autoincrement,
        Title TEXT not null,
        Ord INT not null
    );

    create table HrItem (
        Id integer primary key autoincrement,
        Ord INT not null,
        DiseaseID INT,
        WordID INT,
        UomID INT,
        MeasureValue FLOAT,
        HealthRecordID INT not null, 
        constraint FK_HrItem_Disease foreign key (DiseaseID) references Disease,
        constraint FK_HrItem_Word foreign key (WordID) references Word,
        constraint FK_HrItem_Uom foreign key (UomID) references Uom,
        constraint FK_HrItem_Hr foreign key (HealthRecordID) references HealthRecord
    );
    
    create table IcdBlock (
        Id integer primary key autoincrement,
        ChapterID INT not null,
        Title TEXT not null,
        Code TEXT not null,
        constraint FK_IcdBlock_IcdChapter foreign key (ChapterID) references IcdChapter
    );

    create table IcdChapter (
        Id integer primary key autoincrement,
        Title TEXT not null,
        Code TEXT not null
    );

    create table Patient (
        Id integer primary key autoincrement,
        Label TEXT,
        FirstName TEXT,
        MiddleName TEXT,
        LastName TEXT,
        IsMale BOOL,
        BirthYear INT,
        BirthMonth TINYINT,
        BirthDay TINYINT
    );

    create table Speciality (
        Id integer primary key autoincrement,
        Title TEXT not null
    );

    create table SpecialityIcdBlocks (
        Id integer primary key autoincrement,
        SpecialityID INT not null,
        IcdBlockID INT not null,
        constraint FK_SpecialityIcdBlocks_Speciality foreign key (SpecialityID) references Speciality,
        constraint FK_SpecialityIcdBlocks_IcdBlock foreign key (IcdBlockID) references IcdBlock
    );

    create table Uom (
        Id integer primary key autoincrement,
        Abbr TEXT not null,
        Description TEXT,
        Factor FLOAT not null,
        UomType INT not null
    );

    create table Word (
        Id integer primary key autoincrement,
        Title TEXT not null,
        DefHrCategoryID INT,
        ParentID INT,
        constraint FK_Word_HrCategory foreign key (DefHrCategoryID) references HrCategory,
        constraint FK_Word_Word foreign key (ParentID) references Word
    );

    CREATE UNIQUE INDEX IF NOT EXISTS BlockCode ON IcdBlock(Code);
    CREATE UNIQUE INDEX IF NOT EXISTS ChapterCode ON IcdChapter(Code);
    CREATE UNIQUE INDEX IF NOT EXISTS DiseaseCode ON Disease(Code);
    
    CREATE INDEX IF NOT EXISTS WordTitle ON Word(Title);