
    
alter table Crit  drop constraint FKBAE34A2AA1B7C41A
;

    
alter table UomFormat  drop constraint FK443D224148B02845
;

    
alter table CritWords  drop constraint FKE0FE71D68A252862
;

    
alter table CritWords  drop constraint FKE0FE71D6A79E50D0
;

    
alter table VocabularyWords  drop constraint FK4D532220A79E50D0
;

    
alter table VocabularyWords  drop constraint FK4D532220DF3DABF
;

    
alter table SpecialityVocabularies  drop constraint FKFEA2DF462FCCEEF5
;

    
alter table SpecialityVocabularies  drop constraint FKFEA2DF46DF3DABF
;

    
alter table HrItem  drop constraint FK210074861D873B95
;

    
alter table HrItem  drop constraint FK21007486C1AF1EF4
;

    
alter table HrItem  drop constraint FK21007486A79E50D0
;

    
alter table HrItem  drop constraint FK2100748648B02845
;

    
alter table Setting  drop constraint FK17A01CC95F10938C
;

    
alter table Uom  drop constraint FK50181CA48233E4B0
;

    
alter table Appointment  drop constraint FKBB6A20C0FFADC614
;

    
alter table Appointment  drop constraint FKBB6A20C05F10938C
;

    
alter table IcdDisease  drop constraint FK9063D7F54721EC9B
;

    
alter table IcdBlock  drop constraint FK6EBF47EA3F6226F7
;

    
alter table SpecialityIcdBlocks  drop constraint FK204EF8A92FCCEEF5
;

    
alter table SpecialityIcdBlocks  drop constraint FK204EF8A94721EC9B
;

    
alter table WordTemplate  drop constraint FK5DFA0876DF3DABF
;

    
alter table Word  drop constraint FK91F219F9B19D34E5
;

    
alter table Word  drop constraint FK91F219F948B02845
;

    
alter table HealthRecord  drop constraint FK80ADA1CA1EB4737E
;

    
alter table HealthRecord  drop constraint FK80ADA1CAFFADC614
;

    
alter table HealthRecord  drop constraint FK80ADA1CA2166F67F
;

    
alter table HealthRecord  drop constraint FK80ADA1CA5F10938C
;

    
alter table HealthRecord  drop constraint FK80ADA1CA4D800310
;

    
alter table Doctor  drop constraint FKCE4594AA2FCCEEF5
;

    
alter table Doctor  drop constraint FKCE4594AA3F1BA85C
;

    
alter table Doctor  drop constraint FKCE4594AA9921153D
;

    
alter table Course  drop constraint FKFFF434031EB4737E
;

    
alter table Course  drop constraint FKFFF434035F10938C
;

    drop table Crit;

    drop table UomFormat;

    drop table CritWords;

    drop table VocabularyWords;

    drop table SpecialityVocabularies;

    drop table Vocabulary;

    drop table UomType;

    drop table Passport;

    drop table HrItem;

    drop table Setting;

    drop table Uom;

    drop table Appointment;

    drop table HrCategory;

    drop table Speciality;

    drop table IcdDisease;

    drop table IcdBlock;

    drop table IcdChapter;

    drop table SpecialityIcdBlocks;

    drop table WordTemplate;

    drop table Word;

    drop table HealthRecord;

    drop table Doctor;

    drop table Course;

    drop table Patient;

    create table Crit (
        Id UNIQUEIDENTIFIER not null,
       CritType NVARCHAR(255) not null,
       Description NVARCHAR(2000) not null,
       Options ntext null,
       OptionsFormat NVARCHAR(10) null,
       ParentID UNIQUEIDENTIFIER null,
       Code NVARCHAR(50) null,
       Value NVARCHAR(50) null,
       primary key (Id)
    );

    create table UomFormat (
        Id UNIQUEIDENTIFIER not null,
       UomID UNIQUEIDENTIFIER not null,
       String NVARCHAR(50) not null,
       MeasureValue NUMERIC(18, 6) not null,
       primary key (Id)
    );

    create table CritWords (
        Id UNIQUEIDENTIFIER not null,
       CritID UNIQUEIDENTIFIER not null,
       WordID UNIQUEIDENTIFIER not null,
       primary key (Id)
    );

    create table VocabularyWords (
        Id UNIQUEIDENTIFIER not null,
       WordID UNIQUEIDENTIFIER not null,
       VocabularyID UNIQUEIDENTIFIER not null,
       primary key (Id)
    );

    create table SpecialityVocabularies (
        Id UNIQUEIDENTIFIER not null,
       SpecialityID UNIQUEIDENTIFIER not null,
       VocabularyID UNIQUEIDENTIFIER not null,
       primary key (Id)
    );

    create table Vocabulary (
        Id UNIQUEIDENTIFIER not null,
       Title NVARCHAR(50) not null,
       primary key (Id)
    );

    create table UomType (
        Id UNIQUEIDENTIFIER not null,
       Ord INT not null,
       Title NVARCHAR(50) not null,
       primary key (Id)
    );

    create table Passport (
        Id UNIQUEIDENTIFIER not null,
       HashAndSalt NVARCHAR(70) null,
       Remember BIT default 0  not null,
       primary key (Id)
    );

    create table HrItem (
        Id UNIQUEIDENTIFIER not null,
       HealthRecordID UNIQUEIDENTIFIER not null,
       Confidence NVARCHAR(255) null,
       IcdDiseaseID INT null,
       WordID UNIQUEIDENTIFIER null,
       TextRepr NVARCHAR(255) null,
       UomID UNIQUEIDENTIFIER null,
       MeasureValue NUMERIC(18, 6) null,
       Ord INT default 0  not null,
       primary key (Id)
    );

    create table Setting (
        Id UNIQUEIDENTIFIER not null,
       Title NVARCHAR(255) not null,
       Value NVARCHAR(255) not null,
       DoctorID UNIQUEIDENTIFIER null,
       primary key (Id)
    );

    create table Uom (
        Id UNIQUEIDENTIFIER not null,
       Abbr NVARCHAR(20) not null,
       Description NVARCHAR(100) not null,
       Factor NUMERIC(18, 6) not null,
       UomTypeID UNIQUEIDENTIFIER not null,
       primary key (Id)
    );

    create table Appointment (
        Id UNIQUEIDENTIFIER not null,
       CourseID UNIQUEIDENTIFIER not null,
       DoctorID UNIQUEIDENTIFIER not null,
       DateAndTime DATETIME not null,
       CreatedAt DATETIME default GETDATE()  not null,
       UpdatedAt DATETIME default GETDATE()  not null,
       primary key (Id)
    );

    create table HrCategory (
        Id UNIQUEIDENTIFIER not null,
       Title NVARCHAR(20) not null,
       Ord INT not null,
       primary key (Id)
    );

    create table Speciality (
        Id UNIQUEIDENTIFIER not null,
       Title NVARCHAR(50) not null,
       primary key (Id)
    );

    create table IcdDisease (
        Id INT IDENTITY NOT NULL,
       IcdBlockID INT not null,
       Title NVARCHAR(200) not null,
       Code NVARCHAR(10) not null,
       primary key (Id),
      unique (Title)
    );

    create table IcdBlock (
        Id INT IDENTITY NOT NULL,
       ChapterID INT not null,
       Title NVARCHAR(150) not null,
       Code NVARCHAR(10) not null,
       primary key (Id),
      unique (Title)
    );

    create table IcdChapter (
        Id INT IDENTITY NOT NULL,
       Title NVARCHAR(150) not null,
       Code NVARCHAR(10) not null,
       primary key (Id),
      unique (Title)
    );

    create table SpecialityIcdBlocks (
        Id UNIQUEIDENTIFIER not null,
       SpecialityID UNIQUEIDENTIFIER not null,
       IcdBlockID INT not null,
       primary key (Id)
    );

    create table WordTemplate (
        Id UNIQUEIDENTIFIER not null,
       Title NVARCHAR(100) not null,
       VocabularyID UNIQUEIDENTIFIER not null,
       primary key (Id)
    );

    create table Word (
        Id UNIQUEIDENTIFIER not null,
       Title NVARCHAR(100) not null,
       ParentID UNIQUEIDENTIFIER null,
       UomID UNIQUEIDENTIFIER null,
       primary key (Id),
      unique (Title)
    );

    create table HealthRecord (
        Id UNIQUEIDENTIFIER not null,
       PatientID UNIQUEIDENTIFIER null,
       CourseID UNIQUEIDENTIFIER null,
       AppointmentID UNIQUEIDENTIFIER null,
       DoctorID UNIQUEIDENTIFIER not null,
       IsDeleted BIT default 0  not null,
       HrCategoryID UNIQUEIDENTIFIER null,
       FromYear INT null,
       FromMonth INT null,
       FromDay INT null,
       ToYear INT null,
       ToMonth INT null,
       ToDay INT null,
       DescribedAt DATETIME default GETDATE()  not null,
       Unit NVARCHAR(255) null,
       Ord INT default 0  not null,
       CreatedAt DATETIME default GETDATE()  not null,
       UpdatedAt DATETIME default GETDATE()  not null,
       primary key (Id)
    );

    create table Doctor (
        Id UNIQUEIDENTIFIER not null,
       FirstName NVARCHAR(20) null,
       MiddleName NVARCHAR(20) null,
       LastName NVARCHAR(20) not null,
       IsMale BIT null,
       SpecialityID UNIQUEIDENTIFIER null,
       CustomVocabularyID UNIQUEIDENTIFIER null,
       primary key (Id)
    );

    create table Course (
        Id UNIQUEIDENTIFIER not null,
       PatientID UNIQUEIDENTIFIER not null,
       DoctorID UNIQUEIDENTIFIER not null,
       StartDate DATETIME not null,
       EndDate DATETIME null,
       CreatedAt DATETIME default GETDATE()  not null,
       UpdatedAt DATETIME default GETDATE()  not null,
       primary key (Id)
    );

    create table Patient (
        Id UNIQUEIDENTIFIER not null,
       FirstName NVARCHAR(20) null,
       MiddleName NVARCHAR(20) null,
       LastName NVARCHAR(20) null,
       IsMale BIT null,
       BirthYear INT null,
       BirthMonth INT null,
       BirthDay INT null,
       CreatedAt DATETIME default GETDATE()  not null,
       UpdatedAt DATETIME default GETDATE()  not null,
       primary key (Id)
    );

    alter table Crit 
        add constraint FKBAE34A2AA1B7C41A 
        foreign key (ParentID) 
        references Crit;

    alter table UomFormat 
        add constraint FK443D224148B02845 
        foreign key (UomID) 
        references Uom;

    alter table CritWords 
        add constraint FKE0FE71D68A252862 
        foreign key (CritID) 
        references Crit;

    alter table CritWords 
        add constraint FKE0FE71D6A79E50D0 
        foreign key (WordID) 
        references Word;

    alter table VocabularyWords 
        add constraint FK4D532220A79E50D0 
        foreign key (WordID) 
        references Word;

    alter table VocabularyWords 
        add constraint FK4D532220DF3DABF 
        foreign key (VocabularyID) 
        references Vocabulary;

    alter table SpecialityVocabularies 
        add constraint FKFEA2DF462FCCEEF5 
        foreign key (SpecialityID) 
        references Speciality;

    alter table SpecialityVocabularies 
        add constraint FKFEA2DF46DF3DABF 
        foreign key (VocabularyID) 
        references Vocabulary;

    alter table HrItem 
        add constraint FK210074861D873B95 
        foreign key (HealthRecordID) 
        references HealthRecord;

    alter table HrItem 
        add constraint FK21007486C1AF1EF4 
        foreign key (IcdDiseaseID) 
        references IcdDisease;

    alter table HrItem 
        add constraint FK21007486A79E50D0 
        foreign key (WordID) 
        references Word;

    alter table HrItem 
        add constraint FK2100748648B02845 
        foreign key (UomID) 
        references Uom;

    alter table Setting 
        add constraint FK17A01CC95F10938C 
        foreign key (DoctorID) 
        references Doctor;

    alter table Uom 
        add constraint FK50181CA48233E4B0 
        foreign key (UomTypeID) 
        references UomType;

    alter table Appointment 
        add constraint FKBB6A20C0FFADC614 
        foreign key (CourseID) 
        references Course;

    alter table Appointment 
        add constraint FKBB6A20C05F10938C 
        foreign key (DoctorID) 
        references Doctor;

    alter table IcdDisease 
        add constraint FK9063D7F54721EC9B 
        foreign key (IcdBlockID) 
        references IcdBlock;

    alter table IcdBlock 
        add constraint FK6EBF47EA3F6226F7 
        foreign key (ChapterID) 
        references IcdChapter;

    alter table SpecialityIcdBlocks 
        add constraint FK204EF8A92FCCEEF5 
        foreign key (SpecialityID) 
        references Speciality;

    alter table SpecialityIcdBlocks 
        add constraint FK204EF8A94721EC9B 
        foreign key (IcdBlockID) 
        references IcdBlock;

    alter table WordTemplate 
        add constraint FK5DFA0876DF3DABF 
        foreign key (VocabularyID) 
        references Vocabulary;

    alter table Word 
        add constraint FK91F219F9B19D34E5 
        foreign key (ParentID) 
        references Word;

    alter table Word 
        add constraint FK91F219F948B02845 
        foreign key (UomID) 
        references Uom;

    alter table HealthRecord 
        add constraint FK80ADA1CA1EB4737E 
        foreign key (PatientID) 
        references Patient;

    alter table HealthRecord 
        add constraint FK80ADA1CAFFADC614 
        foreign key (CourseID) 
        references Course;

    alter table HealthRecord 
        add constraint FK80ADA1CA2166F67F 
        foreign key (AppointmentID) 
        references Appointment;

    alter table HealthRecord 
        add constraint FK80ADA1CA5F10938C 
        foreign key (DoctorID) 
        references Doctor;

    alter table HealthRecord 
        add constraint FK80ADA1CA4D800310 
        foreign key (HrCategoryID) 
        references HrCategory;

    alter table Doctor 
        add constraint FKCE4594AA2FCCEEF5 
        foreign key (SpecialityID) 
        references Speciality;

    alter table Doctor 
        add constraint FKCE4594AA3F1BA85C 
        foreign key (CustomVocabularyID) 
        references Vocabulary;

    alter table Doctor 
        add constraint FKCE4594AA9921153D 
        foreign key (Id) 
        references Passport;

    alter table Course 
        add constraint FKFFF434031EB4737E 
        foreign key (PatientID) 
        references Patient;

    alter table Course 
        add constraint FKFFF434035F10938C 
        foreign key (DoctorID) 
        references Doctor;
