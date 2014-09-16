
    PRAGMA foreign_keys = OFF;
    drop table if exists Appointment;
    drop table if exists RecordCategory;
    drop table if exists Symptom;
    drop table if exists SymptomWords;
    drop table if exists Speciality;
    drop table if exists SpecialityIcdBlocks;
    drop table if exists Disease;
    drop table if exists IcdBlock;
    drop table if exists IcdChapter;
    drop table if exists Word;
    drop table if exists HealthRecord;
    drop table if exists Doctor;
    drop table if exists Course;
    drop table if exists PropertyValue;
    drop table if exists Property;
    drop table if exists PatientRecordProperties;
    drop table if exists Patient;
    PRAGMA foreign_keys = ON;

    create table Appointment (
        Id  integer primary key autoincrement,
       CourseID INT,
       DoctorID INT,
       DateAndTime DATETIME,
       constraint FKBB6A20C0FFADC614 foreign key (CourseID) references Course,
       constraint FKBB6A20C05F10938C foreign key (DoctorID) references Doctor
    );

    create table RecordCategory (
        Id  integer primary key autoincrement,
       Title TEXT,
       Ord INT
    );

    create table Symptom (
        Id  integer primary key autoincrement,
       DiseaseID INT,
       DefaultCategoryID INT,
       IsDiagnosis BOOL,
       constraint FKF044991F3A363251 foreign key (DiseaseID) references Disease,
       constraint FKF044991FC70C9732 foreign key (DefaultCategoryID) references RecordCategory
    );

    create table SymptomWords (
        Id integer primary key autoincrement,
       SymptomID INT,
       WordID INT,
       constraint FKD8EFCF9E46390459 foreign key (SymptomID) references Symptom,
       constraint FKD8EFCF9EA79E50D0 foreign key (WordID) references Word
    );

    create table Speciality (
        Id  integer primary key autoincrement,
       Title TEXT
    );

    create table Disease (
        Id  integer primary key autoincrement,
       BlockID INT,
       Title TEXT,
       Code TEXT,
       constraint FK52F139E34AE2417 foreign key (BlockID) references IcdBlock
    );

    create table IcdBlock (
        Id  integer primary key autoincrement,
       ChapterID INT,
       Title TEXT,
       Code TEXT,
       constraint FK6EBF47EA3F6226F7 foreign key (ChapterID) references IcdChapter
    );

    create table IcdChapter (
        Id  integer primary key autoincrement,
       Title TEXT,
       Code TEXT
    );

    create table SpecialityIcdBlocks (
        Id  integer primary key autoincrement,
       SpecialityID INT,
       IcdBlockID INT,
       constraint FK6394EB482FCCEEF5 foreign key (SpecialityID) references Speciality,
       constraint FK6394EB484721EC9B foreign key (IcdBlockID) references IcdBlock
    );

    create table Word (
        Id  integer primary key autoincrement,
       Title TEXT not null,
       Priority TINYINT not null,
       DefaultCategoryID INT,
       ParentID INT,
       constraint FK91F219F9C70C9732 foreign key (DefaultCategoryID) references RecordCategory,
       constraint FK91F219F9B19D34E5 foreign key (ParentID) references Word
    );

    create table HealthRecord (
        Id  integer primary key autoincrement,
       AppointmentID INT,
       Comment TEXT,
       CategoryID INT,
       DiseaseID INT,
       SymptomID INT,
       NumValue NUMERIC,
       FromDay TINYINT,
       FromMonth TINYINT,
       FromYear INT,
       constraint FK80ADA1CA2166F67F foreign key (AppointmentID) references Appointment,
       constraint FK80ADA1CA773D5754 foreign key (CategoryID) references RecordCategory,
       constraint FK80ADA1CA3A363251 foreign key (DiseaseID) references Disease,
       constraint FK80ADA1CA46390459 foreign key (SymptomID) references Symptom
    );

    create table Doctor (
        Id  integer primary key autoincrement,
       FirstName TEXT,
       MiddleName TEXT,
       LastName TEXT,
       IsMale BOOL,
       Settings INT,
       SpecialityID INT,
       constraint FKCE4594AA2FCCEEF5 foreign key (SpecialityID) references Speciality
    );

    create table Course (
        Id  integer primary key autoincrement,
       PatientID INT,
       DoctorID INT,
       StartDate DATETIME,
       EndDate DATETIME,
       constraint FKFFF434031EB4737E foreign key (PatientID) references Patient,
       constraint FKFFF434035F10938C foreign key (DoctorID) references Doctor
    );

    create table PropertyValue (
        Id  integer primary key autoincrement,
       Title TEXT,
       PropertyID INT,
       constraint FK5E90D77B6753BF93 foreign key (PropertyID) references Property
    );

    create table Property (
        Id  integer primary key autoincrement,
       Title TEXT
    );

    create table PatientRecordProperties (
        Id  integer primary key autoincrement,
       PatientID INT,
       ValueID INT,
       PropertyID INT,
       RecordID INT,
       constraint FK1DBC80761EB4737E foreign key (PatientID) references Patient,
       constraint FK1DBC80767AA56DF foreign key (ValueID) references PropertyValue,
       constraint FK1DBC80766753BF93 foreign key (PropertyID) references Property,
       constraint FK1DBC80768C621197 foreign key (RecordID) references HealthRecord
    );

    create table Patient (
        Id  integer primary key autoincrement,
       Label TEXT,
       FirstName TEXT,
       MiddleName TEXT,
       LastName TEXT,
       IsMale BOOL,
       BirthYear INT,
       BirthMonth TINYINT,
       BirthDay TINYINT
    );
