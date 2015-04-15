-- '00000001-7000-0000-0000-000000000001'
INSERT INTO HrCategory (ID, Title, Ord) VALUES ('00000001-7000-0000-0000-000000000001', 'Жалоба', 1);
INSERT INTO HrCategory (ID, Title, Ord) VALUES ('00000002-7000-0000-0000-000000000002', 'История', 2);
INSERT INTO HrCategory (ID, Title, Ord) VALUES ('00000003-7000-0000-0000-000000000003', 'Осмотр', 3);
INSERT INTO HrCategory (ID, Title, Ord) VALUES ('00000004-7000-0000-0000-000000000004', 'Обследование', 4);
INSERT INTO HrCategory (ID, Title, Ord) VALUES ('00000005-7000-0000-0000-000000000005', 'Диагноз', 5);
INSERT INTO HrCategory (ID, Title, Ord) VALUES ('00000006-7000-0000-0000-000000000006', 'Лечение', 6);

-- '00000001-8000-0000-0000-000000000001'
INSERT INTO UomType (ID, Title, Ord) VALUES ('00000001-8000-0000-0000-000000000001', 'Объем', 1);
INSERT INTO UomType (ID, Title, Ord) VALUES ('00000002-8000-0000-0000-000000000002', 'Время', 2);

-- '00000001-9000-0000-0000-000000000001'
INSERT INTO Uom (ID,Abbr,Factor,UomTypeID,Description) VALUES ('00000001-9000-0000-0000-000000000001','л',3,'00000001-8000-0000-0000-000000000001','литр');
INSERT INTO Uom (ID,Abbr,Factor,UomTypeID,Description) VALUES ('00000002-9000-0000-0000-000000000002','мл',0,'00000001-8000-0000-0000-000000000001','милилитр');
INSERT INTO Uom (ID,Abbr,Factor,UomTypeID,Description) VALUES ('00000003-9000-0000-0000-000000000003','мкл',-3,'00000001-8000-0000-0000-000000000001','микролитр');
INSERT INTO Uom (ID,Abbr,Factor,UomTypeID,Description) VALUES ('00000004-9000-0000-0000-000000000004','сут',0,'00000002-8000-0000-0000-000000000002','сутки');
INSERT INTO Uom (ID,Abbr,Factor,UomTypeID,Description) VALUES ('00000005-9000-0000-0000-000000000005','нед',0.8451,'00000002-8000-0000-0000-000000000002','неделя');
INSERT INTO Uom (ID,Abbr,Factor,UomTypeID,Description) VALUES ('00000006-9000-0000-0000-000000000006','мес',1.4771,'00000002-8000-0000-0000-000000000002','месяц');
INSERT INTO Uom (ID,Abbr,Factor,UomTypeID,Description) VALUES ('00000007-9000-0000-0000-000000000007','г',2.5623,'00000002-8000-0000-0000-000000000002','год');

-- for sqlce - use IDENTITY_INSERT and no empty lines after split by semicolon

--SET IDENTITY_INSERT IcdChapter ON;
INSERT INTO IcdChapter (ID, Code, Title) VALUES (1, 'I', 'Некоторые инфекционные и паразитарные болезни');
INSERT INTO IcdChapter (ID, Code, Title) VALUES (5, 'IX', 'Болезни системы кровообращения');
--SET IDENTITY_INSERT IcdChapter OFF;

--SET IDENTITY_INSERT IcdBlock ON;
INSERT INTO IcdBlock (ID, Code, Title, ChapterID) VALUES (91, '(I00-I02)', 'Острая ревматическая лихорадка', 5);
INSERT INTO IcdBlock (ID, Code, Title, ChapterID) VALUES (92, '(I05-I09)', 'Хронические ревматические болезни сердца', 5);
--SET IDENTITY_INSERT IcdBlock OFF;

--SET IDENTITY_INSERT IcdDisease ON;
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (1 , 'I00', 'Ревматическая лихорадка без упоминания о  вовлечении сердца', 91);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (2 , 'I01', 'Ревматическая лихорадка с вовлечением сердца', 91);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (3 , 'I01.0', 'Острый ревматический перикардит', 91);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (4 , 'I01.1', 'Острый ревматический эндокардит', 91);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (5 , 'I01.2', 'Острый ревматический миокардит', 91);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (6 , 'I01.8', 'Другие острые ревматические болезни сердца', 91);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (7 , 'I01.9', 'Острая ревматическая болезнь сердца неуточненная', 91);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (8 , 'I02', 'Ревматическая хорея', 91);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (9 , 'I02.0', 'Ревматическая хорея с вовлечением сердца', 91);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (10, 'I02.9', 'Ревматическая хорея без вовлечения сердца', 91);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (11, 'I05', 'Ревматические болезни митрального клапана', 92);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (12, 'I05.0', 'Митральный стеноз', 92);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (13, 'I05.1', 'Ревматическая недостаточность митрального клапана', 92);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (14, 'I05.2', 'Митральный стеноз с недостаточностью', 92);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (15, 'I05.8', 'Другие болезни митрального клапана', 92);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (16, 'I05.9', 'Болезнь митрального клапана неуточненная', 92);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (17, 'I06', 'Ревматические болезни аортального клапана', 92);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (18, 'I06.0', 'Ревматический аортальный стеноз', 92);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (19, 'I06.1', 'Ревматическая недостаточность аортального клапана', 92);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (20, 'I06.2', 'Ревматический аортальный стеноз с недостаточностью', 92);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (21, 'I06.8', 'Другие ревматические болезни аортального клапана', 92);
INSERT INTO IcdDisease (ID, Code, Title, IcdBlockID) VALUES (22, 'I06.9', 'Ревматическая болезнь аортального клапана неуточненная (ый)', 92);
--SET IDENTITY_INSERT IcdDisease OFF;

-- '00000001-1200-0000-0001-000000000001'
-- только шаблоны
INSERT INTO Vocabulary (Id, Title) VALUES ('00000001-1200-0000-0000-000000000001', 'Общий словарь');
INSERT INTO Vocabulary (Id, Title) VALUES ('00000002-1200-0000-0000-000000000002', 'Слова кардиолога');
-- заполненный
INSERT INTO Vocabulary (Id, Title) VALUES ('00000003-1200-0000-0000-000000000003', 'Пользовательский d1');

-- '00000001-1300-0000-0001-000000000001'
INSERT INTO WordTemplate (Id,VocabularyID,Title) VALUES ('00000001-1300-0000-0000-000000000001','00000001-1200-0000-0000-000000000001','порфирия');
INSERT INTO WordTemplate (Id,VocabularyID,Title) VALUES ('00000002-1300-0000-0000-000000000002','00000001-1200-0000-0000-000000000001','тест');
INSERT INTO WordTemplate (Id,VocabularyID,Title) VALUES ('00000003-1300-0000-0000-000000000003','00000001-1200-0000-0000-000000000001','шаблон');
INSERT INTO WordTemplate (Id,VocabularyID,Title) VALUES ('00000004-1300-0000-0000-000000000004','00000002-1200-0000-0000-000000000002','понос'); -- w6
INSERT INTO WordTemplate (Id,VocabularyID,Title) VALUES ('00000005-1300-0000-0000-000000000005','00000002-1200-0000-0000-000000000002','повторно'); -- w5
INSERT INTO WordTemplate (Id,VocabularyID,Title) VALUES ('00000006-1300-0000-0000-000000000006','00000002-1200-0000-0000-000000000002','тест');
INSERT INTO WordTemplate (Id,VocabularyID,Title) VALUES ('00000007-1300-0000-0000-000000000007','00000002-1200-0000-0000-000000000002','шаблон');

-- '00000001-1000-0000-0001-000000000001'
INSERT INTO Speciality (Id, Title) VALUES ('00000001-1000-0000-0000-000000000001', 'Кардиолог');
INSERT INTO Speciality (Id, Title) VALUES ('00000002-1000-0000-0000-000000000002', 'Специальность без словаря');

-- '00000001-1100-0000-0001-000000000001'
INSERT INTO SpecialityIcdBlocks (Id,SpecialityID, IcdBlockID) VALUES ('00000001-1100-0000-0000-000000000001','00000001-1000-0000-0000-000000000001', 92);

-- словарь кардиолога
INSERT INTO SpecialityVocabularies (Id,SpecialityID, VocabularyID) VALUES ('00000001-1500-0000-0000-000000000001','00000001-1000-0000-0000-000000000001','00000002-1200-0000-0000-000000000002');


-- CLIENT only (do not change this comment)

-- '00000001-0000-0000-0001-000000000001'
INSERT INTO Word (Id,Title) VALUES ('00000001-0000-0000-0001-000000000001','анемия');
INSERT INTO Word (Id,Title) VALUES ('00000002-0000-0000-0001-000000000002','озноб');
INSERT INTO Word (Id,Title) VALUES ('00000003-0000-0000-0001-000000000003','кашель');
INSERT INTO Word (Id,Title) VALUES ('00000004-0000-0000-0001-000000000004','впервые');
INSERT INTO Word (Id,Title) VALUES ('00000005-0000-0000-0001-000000000005','повторно');
INSERT INTO Word (Id,Title) VALUES ('00000006-0000-0000-0001-000000000006','понос');
INSERT INTO Word (Id,Title) VALUES ('00000007-0000-0000-0001-000000000007','пневмоторекс');
INSERT INTO Word (Id,Title) VALUES ('00000008-0000-0000-0001-000000000008','одышка');
INSERT INTO Word (Id,Title) VALUES ('00000009-0000-0000-0001-000000000009','удушье');
INSERT INTO Word (Id,Title) VALUES ('00000010-0000-0000-0001-000000000010','кровохаркание');
INSERT INTO Word (Id,Title) VALUES ('00000011-0000-0000-0001-000000000011','потери сознания');
INSERT INTO Word (Id,Title) VALUES ('00000012-0000-0000-0001-000000000012','ожирение');
INSERT INTO Word (Id,Title) VALUES ('00000013-0000-0000-0001-000000000013','гипертрофия');
INSERT INTO Word (Id,Title) VALUES ('00000014-0000-0000-0001-000000000014','кардиомегалия');
INSERT INTO Word (Id,Title) VALUES ('00000015-0000-0000-0001-000000000015','асцит');
INSERT INTO Word (Id,Title) VALUES ('00000016-0000-0000-0001-000000000016','головокружение');
INSERT INTO Word (Id,Title) VALUES ('00000017-0000-0000-0001-000000000017','инфильтрация');
INSERT INTO Word (Id,Title) VALUES ('00000018-0000-0000-0001-000000000018','беременность');
INSERT INTO Word (Id,Title) VALUES ('00000019-0000-0000-0001-000000000019','лихорадка');
INSERT INTO Word (Id,Title) VALUES ('00000020-0000-0000-0001-000000000020','лейкоцитоз');
INSERT INTO Word (Id,Title) VALUES ('00000021-0000-0000-0001-000000000021','запоры');
INSERT INTO Word (Id,Title) VALUES ('00000022-0000-0000-0001-000000000022','роды');
INSERT INTO Word (Id,Title) VALUES ('00000023-0000-0000-0001-000000000023','кровотечение');
INSERT INTO Word (Id,Title) VALUES ('00000024-0000-0000-0001-000000000024','гематурия');

INSERT INTO Word (Id,Title) VALUES ('00000031-0000-0000-0001-000000000031','щитовидная железа');
INSERT INTO Word (Id,Title) VALUES ('00000032-0000-0000-0001-000000000032','легкие');

INSERT INTO Word (Id,Title) VALUES ('00000040-0000-0000-0001-000000000040','сидя');

INSERT INTO Word (Id,Title) VALUES ('00000051-0000-0000-0001-000000000051','фибрилляция предсердий');

INSERT INTO Word (Id,Title) VALUES ('00000070-0000-0000-0001-000000000070','лазикс');
INSERT INTO Word (Id,Title) VALUES ('00000071-0000-0000-0001-000000000071','варфорин');
INSERT INTO Word (Id,Title) VALUES ('00000072-0000-0000-0001-000000000072','антибиотики');
INSERT INTO Word (Id,Title) VALUES ('00000073-0000-0000-0001-000000000073','обезболивание');
INSERT INTO Word (Id,Title) VALUES ('00000074-0000-0000-0001-000000000074','нитроглицерин');

INSERT INTO Word (Id,Title) VALUES ('00000094-0000-0000-0001-000000000094','ЛЖ');

INSERT INTO Word (Id,Title) VALUES ('00000100-0000-0000-0001-000000000100','нитроминт');


-- '00000001-1400-0000-0001-000000000001'
-- все слова в пользовательском d1
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000001-1400-0000-0000-000000000001', '00000003-1200-0000-0000-000000000003', '00000001-0000-0000-0001-000000000001');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000002-1400-0000-0000-000000000002', '00000003-1200-0000-0000-000000000003', '00000002-0000-0000-0001-000000000002');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000003-1400-0000-0000-000000000003', '00000003-1200-0000-0000-000000000003', '00000003-0000-0000-0001-000000000003');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000004-1400-0000-0000-000000000004', '00000003-1200-0000-0000-000000000003', '00000004-0000-0000-0001-000000000004');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000005-1400-0000-0000-000000000005', '00000003-1200-0000-0000-000000000003', '00000005-0000-0000-0001-000000000005');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000006-1400-0000-0000-000000000006', '00000003-1200-0000-0000-000000000003', '00000006-0000-0000-0001-000000000006');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000007-1400-0000-0000-000000000007', '00000003-1200-0000-0000-000000000003', '00000007-0000-0000-0001-000000000007');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000008-1400-0000-0000-000000000008', '00000003-1200-0000-0000-000000000003', '00000008-0000-0000-0001-000000000008');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000009-1400-0000-0000-000000000009', '00000003-1200-0000-0000-000000000003', '00000009-0000-0000-0001-000000000009');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000010-1400-0000-0000-000000000010', '00000003-1200-0000-0000-000000000003', '00000010-0000-0000-0001-000000000010');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000011-1400-0000-0000-000000000011', '00000003-1200-0000-0000-000000000003', '00000011-0000-0000-0001-000000000011');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000012-1400-0000-0000-000000000012', '00000003-1200-0000-0000-000000000003', '00000012-0000-0000-0001-000000000012');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000013-1400-0000-0000-000000000013', '00000003-1200-0000-0000-000000000003', '00000013-0000-0000-0001-000000000013');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000014-1400-0000-0000-000000000014', '00000003-1200-0000-0000-000000000003', '00000014-0000-0000-0001-000000000014');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000015-1400-0000-0000-000000000015', '00000003-1200-0000-0000-000000000003', '00000015-0000-0000-0001-000000000015');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000016-1400-0000-0000-000000000016', '00000003-1200-0000-0000-000000000003', '00000016-0000-0000-0001-000000000016');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000017-1400-0000-0000-000000000017', '00000003-1200-0000-0000-000000000003', '00000017-0000-0000-0001-000000000017');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000018-1400-0000-0000-000000000018', '00000003-1200-0000-0000-000000000003', '00000018-0000-0000-0001-000000000018');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000019-1400-0000-0000-000000000019', '00000003-1200-0000-0000-000000000003', '00000019-0000-0000-0001-000000000019');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000020-1400-0000-0000-000000000020', '00000003-1200-0000-0000-000000000003', '00000020-0000-0000-0001-000000000020');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000021-1400-0000-0000-000000000021', '00000003-1200-0000-0000-000000000003', '00000021-0000-0000-0001-000000000021');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000022-1400-0000-0000-000000000022', '00000003-1200-0000-0000-000000000003', '00000022-0000-0000-0001-000000000022');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000023-1400-0000-0000-000000000023', '00000003-1200-0000-0000-000000000003', '00000023-0000-0000-0001-000000000023');
INSERT INTO VocabularyWords (Id, VocabularyID, WordID) VALUES ('00000024-1400-0000-0000-000000000024', '00000003-1200-0000-0000-000000000003', '00000024-0000-0000-0001-000000000024');

-- '00000001-1000-0000-0000-000000000001'
INSERT INTO Passport (ID) VALUES ('00000001-1000-0000-0000-000000000001');
INSERT INTO Passport (ID,Remember) VALUES ('00000002-1000-0000-0000-000000000002',1);
INSERT INTO Doctor (ID, LastName, MiddleName, FirstName, IsMale, CustomVocabularyID) VALUES ('00000001-1000-0000-0000-000000000001', 'Смирнов', NULL, 'Василий', 1, '00000003-1200-0000-0000-000000000003');
INSERT INTO Doctor (ID, LastName, MiddleName, FirstName, IsMale, SpecialityID) VALUES ('00000002-1000-0000-0000-000000000002', 'Вахрушев', 'Константинович', 'Александр', 1, '00000001-1000-0000-0000-000000000001');
-- '00000001-2000-0000-0000-000000000001'
INSERT INTO Patient (ID, LastName, MiddleName, FirstName, IsMale, BirthYear, BirthMonth, BirthDay,CreatedAt,UpdatedAt) VALUES ('00000001-2000-0000-0000-000000000001', 'Андреев', NULL, 'Иван', 1, 1982, 11, NULL,'2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO Patient (ID, LastName, MiddleName, FirstName, IsMale, BirthYear, BirthMonth, BirthDay,CreatedAt,UpdatedAt) VALUES ('00000002-2000-0000-0000-000000000002', 'Петров', 'Иванович', 'Иван', 1, 1980, 2, 15,'2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO Patient (ID, LastName, MiddleName, FirstName, IsMale, BirthYear, BirthMonth, BirthDay,CreatedAt,UpdatedAt) VALUES ('00000003-2000-0000-0000-000000000003', 'Иванов', 'Иванович', 'Иван', 1, 1995, NULL, 10,'2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO Patient (ID, LastName, MiddleName, FirstName, IsMale, BirthYear, BirthMonth, BirthDay,CreatedAt,UpdatedAt) VALUES ('00000004-2000-0000-0000-000000000004', 'Михайлов', NULL, 'Валентин', 1, 1973, NULL, NULL,'2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO Patient (ID, LastName, MiddleName, FirstName, IsMale,CreatedAt,UpdatedAt) VALUES ('00000005-2000-0000-0000-000000000005', NULL, NULL, NULL, 0,'2014-11-19 00:00:00','2014-11-19 00:00:00');
-- '00000001-3000-0000-0000-000000000001'
INSERT INTO Course (ID, PatientID, StartDate, EndDate, DoctorID,CreatedAt,UpdatedAt) VALUES ('00000001-3000-0000-0000-000000000001', '00000001-2000-0000-0000-000000000001', '2014-03-07 00:00:00', '2014-03-14 00:00:00', '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO Course (ID, PatientID, StartDate, EndDate, DoctorID,CreatedAt,UpdatedAt) VALUES ('00000002-3000-0000-0000-000000000002', '00000001-2000-0000-0000-000000000001', '2013-05-29 00:00:00', NULL, '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO Course (ID, PatientID, StartDate, EndDate, DoctorID,CreatedAt,UpdatedAt) VALUES ('00000003-3000-0000-0000-000000000003', '00000005-2000-0000-0000-000000000005', '2014-05-04 00:00:00', NULL, '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO Course (ID, PatientID, StartDate, EndDate, DoctorID,CreatedAt,UpdatedAt) VALUES ('00000004-3000-0000-0000-000000000004', '00000003-2000-0000-0000-000000000003', '2014-05-04 00:00:00', NULL, '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
-- '00000001-4000-0000-0000-000000000001'
INSERT INTO Appointment (ID, DateAndTime, CourseID, DoctorID,CreatedAt,UpdatedAt) VALUES ('00000001-4000-0000-0000-000000000001', '2014-03-10 07:39:48', '00000001-3000-0000-0000-000000000001', '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO Appointment (ID, DateAndTime, CourseID, DoctorID,CreatedAt,UpdatedAt) VALUES ('00000002-4000-0000-0000-000000000002', '2014-03-14 09:03:56', '00000001-3000-0000-0000-000000000001', '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO Appointment (ID, DateAndTime, CourseID, DoctorID,CreatedAt,UpdatedAt) VALUES ('00000003-4000-0000-0000-000000000003', '2014-05-29 02:45:51', '00000002-3000-0000-0000-000000000002', '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO Appointment (ID, DateAndTime, CourseID, DoctorID,CreatedAt,UpdatedAt) VALUES ('00000004-4000-0000-0000-000000000004', '2014-06-03 05:39:52', '00000002-3000-0000-0000-000000000002', '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO Appointment (ID, DateAndTime, CourseID, DoctorID,CreatedAt,UpdatedAt) VALUES ('00000005-4000-0000-0000-000000000005', '2014-06-03 05:39:52', '00000004-3000-0000-0000-000000000004', '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
-- '00000001-5000-0000-0000-000000000001'
-- open interval date
INSERT INTO HealthRecord (ID,CreatedAt,UpdatedAt,DescribedAt,PatientID,CourseID,AppointmentID,DoctorID,HrCategoryID,FromYear,FromMonth) VALUES ('00000001-5000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00','2014-11-19 00:00:00',NULL,NULL,'00000001-4000-0000-0000-000000000001','00000001-1000-0000-0000-000000000001','00000005-7000-0000-0000-000000000005',2013,11);
-- point date
INSERT INTO HealthRecord (ID,CreatedAt,UpdatedAt,DescribedAt,PatientID,CourseID,AppointmentID,DoctorID,HrCategoryID,FromYear,FromMonth,ToYear,ToMonth,Unit) VALUES ('00000002-5000-0000-0000-000000000002','2014-11-19 00:00:00','2014-11-19 00:00:00','2014-11-19 00:00:00',NULL,NULL,'00000001-4000-0000-0000-000000000001','00000001-1000-0000-0000-000000000001','00000003-7000-0000-0000-000000000003',2013,12,2013,12,'ByAge');
-- close interval date
INSERT INTO HealthRecord (ID,CreatedAt,UpdatedAt,DescribedAt,PatientID,CourseID,AppointmentID,DoctorID,HrCategoryID,FromYear,FromMonth,FromDay,ToYear,ToMonth,Unit) VALUES ('00000020-5000-0000-0000-000000000020','2014-11-20 13:27:46','2014-11-20 13:27:46','2014-11-19 00:00:00',NULL,NULL,'00000002-4000-0000-0000-000000000002','00000001-1000-0000-0000-000000000001','00000001-7000-0000-0000-000000000001',2014,1,30,2015,1,'Month');
INSERT INTO HealthRecord (ID,CreatedAt,UpdatedAt,DescribedAt,PatientID,CourseID,AppointmentID,DoctorID,HrCategoryID) VALUES ('00000021-5000-0000-0000-000000000021','2014-11-21 13:27:46','2014-11-20 13:27:46','2014-11-21 13:27:46',NULL,NULL,'00000002-4000-0000-0000-000000000002','00000001-1000-0000-0000-000000000001','00000005-7000-0000-0000-000000000005');
INSERT INTO HealthRecord (ID,CreatedAt,UpdatedAt,DescribedAt,PatientID,CourseID,AppointmentID,DoctorID,HrCategoryID) VALUES ('00000022-5000-0000-0000-000000000022','2014-11-22 13:27:46','2014-11-20 13:27:46','2014-11-22 13:27:46',NULL,NULL,'00000002-4000-0000-0000-000000000002','00000001-1000-0000-0000-000000000001','00000002-7000-0000-0000-000000000002');
INSERT INTO HealthRecord (ID,CreatedAt,UpdatedAt,DescribedAt,PatientID,CourseID,AppointmentID,DoctorID,HrCategoryID) VALUES ('00000030-5000-0000-0000-000000000030','2014-11-19 00:00:00','2014-11-19 00:00:00','2014-11-19 00:00:00',NULL,NULL,'00000003-4000-0000-0000-000000000003','00000001-1000-0000-0000-000000000001','00000001-7000-0000-0000-000000000001');
INSERT INTO HealthRecord (ID,CreatedAt,UpdatedAt,DescribedAt,PatientID,CourseID,AppointmentID,DoctorID,HrCategoryID) VALUES ('00000031-5000-0000-0000-000000000031','2014-11-19 00:00:00','2014-11-19 00:00:00','2014-11-19 00:00:00',NULL,NULL,'00000003-4000-0000-0000-000000000003','00000001-1000-0000-0000-000000000001','00000002-7000-0000-0000-000000000002');
INSERT INTO HealthRecord (ID,CreatedAt,UpdatedAt,DescribedAt,PatientID,CourseID,AppointmentID,DoctorID,HrCategoryID) VALUES ('00000032-5000-0000-0000-000000000032','2014-11-19 00:00:00','2014-11-19 00:00:00','2014-11-19 00:00:00',NULL,NULL,'00000003-4000-0000-0000-000000000003','00000001-1000-0000-0000-000000000001','00000001-7000-0000-0000-000000000001');
INSERT INTO HealthRecord (ID,CreatedAt,UpdatedAt,DescribedAt,PatientID,CourseID,AppointmentID,DoctorID,HrCategoryID,FromYear,FromMonth,FromDay,Unit) VALUES ('00000040-5000-0000-0000-000000000040','2014-11-19 00:00:00','2014-11-19 00:00:00','2014-11-19 00:00:00',NULL,NULL,'00000004-4000-0000-0000-000000000004','00000001-1000-0000-0000-000000000001','00000002-7000-0000-0000-000000000002',2005,2,10,'Year');
INSERT INTO HealthRecord (ID,CreatedAt,UpdatedAt,DescribedAt,PatientID,CourseID,AppointmentID,DoctorID,HrCategoryID) VALUES ('00000070-5000-0000-0000-000000000070','2014-11-19 00:00:00','2014-11-19 00:00:00','2014-11-19 00:00:00',NULL,'00000001-3000-0000-0000-000000000001',NULL,'00000001-1000-0000-0000-000000000001','00000001-7000-0000-0000-000000000001');
INSERT INTO HealthRecord (ID,CreatedAt,UpdatedAt,DescribedAt,PatientID,CourseID,AppointmentID,DoctorID,HrCategoryID) VALUES ('00000071-5000-0000-0000-000000000071','2014-11-19 00:00:00','2014-11-19 00:00:00','2014-11-19 00:00:00',NULL,'00000002-3000-0000-0000-000000000002',NULL,'00000001-1000-0000-0000-000000000001','00000001-7000-0000-0000-000000000001');
INSERT INTO HealthRecord (ID,CreatedAt,UpdatedAt,DescribedAt,PatientID,CourseID,AppointmentID,DoctorID) VALUES ('00000072-5000-0000-0000-000000000072','2014-11-19 00:00:00','2014-11-19 00:00:00','2014-11-19 00:00:00','00000001-2000-0000-0000-000000000001',NULL,NULL,'00000001-1000-0000-0000-000000000001');
INSERT INTO HealthRecord (ID,CreatedAt,UpdatedAt,DescribedAt,PatientID,CourseID,AppointmentID,DoctorID) VALUES ('00000073-5000-0000-0000-000000000073','2014-11-19 00:00:00','2014-11-19 00:00:00','2014-11-19 00:00:00','00000002-2000-0000-0000-000000000002',NULL,NULL,'00000001-1000-0000-0000-000000000001');
INSERT INTO HealthRecord (ID,CreatedAt,UpdatedAt,DescribedAt,PatientID,CourseID,AppointmentID,DoctorID) VALUES ('00000074-5000-0000-0000-000000000074','2014-11-19 00:00:00','2014-11-19 00:00:00','2014-11-19 00:00:00','00000002-2000-0000-0000-000000000002',NULL,NULL,'00000001-1000-0000-0000-000000000001');

-- '00000001-6000-0000-0000-000000000001'
INSERT INTO HrItem (ID,HealthRecordID,TextRepr) VALUES ('00000001-6000-0000-0000-000000000001','00000021-5000-0000-0000-000000000021','comment');
INSERT INTO HrItem (ID,HealthRecordID,WordID,TextRepr,Ord) VALUES ('00000002-6000-0000-0000-000000000002','00000021-5000-0000-0000-000000000021','00000001-0000-0000-0001-000000000001','text repr with word',1);
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000003-6000-0000-0000-000000000003','00000022-5000-0000-0000-000000000022','00000001-0000-0000-0001-000000000001');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000004-6000-0000-0000-000000000004','00000022-5000-0000-0000-000000000022','00000022-0000-0000-0001-000000000022');
INSERT INTO HrItem (ID,HealthRecordID,WordID,MeasureValue,UomID) VALUES ('00000005-6000-0000-0000-000000000005','00000022-5000-0000-0000-000000000022','00000003-0000-0000-0001-000000000003',50,'00000001-9000-0000-0000-000000000001');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000006-6000-0000-0000-000000000006','00000030-5000-0000-0000-000000000030','00000005-0000-0000-0001-000000000005');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000007-6000-0000-0000-000000000007','00000001-5000-0000-0000-000000000001','00000001-0000-0000-0001-000000000001');
INSERT INTO HrItem (ID,HealthRecordID,WordID,Ord) VALUES ('00000008-6000-0000-0000-000000000008','00000001-5000-0000-0000-000000000001','00000002-0000-0000-0001-000000000002',1);
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000009-6000-0000-0000-000000000009','00000002-5000-0000-0000-000000000002','00000001-0000-0000-0001-000000000001');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000010-6000-0000-0000-000000000010','00000002-5000-0000-0000-000000000002','00000004-0000-0000-0001-000000000004');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000011-6000-0000-0000-000000000011','00000031-5000-0000-0000-000000000031','00000051-0000-0000-0001-000000000051');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000012-6000-0000-0000-000000000012','00000031-5000-0000-0000-000000000031','00000094-0000-0000-0001-000000000094');
INSERT INTO HrItem (ID,HealthRecordID,IcdDiseaseID	) VALUES ('00000013-6000-0000-0000-000000000013','00000031-5000-0000-0000-000000000031',1);
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000014-6000-0000-0000-000000000014','00000032-5000-0000-0000-000000000032','00000003-0000-0000-0001-000000000003');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000015-6000-0000-0000-000000000015','00000032-5000-0000-0000-000000000032','00000004-0000-0000-0001-000000000004');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000016-6000-0000-0000-000000000016','00000040-5000-0000-0000-000000000040','00000022-0000-0000-0001-000000000022');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000017-6000-0000-0000-000000000017','00000020-5000-0000-0000-000000000020','00000001-0000-0000-0001-000000000001');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000018-6000-0000-0000-000000000018','00000070-5000-0000-0000-000000000070','00000022-0000-0000-0001-000000000022');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000019-6000-0000-0000-000000000019','00000030-5000-0000-0000-000000000030','00000031-0000-0000-0001-000000000031');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000020-6000-0000-0000-000000000020','00000030-5000-0000-0000-000000000030','00000032-0000-0000-0001-000000000032');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000021-6000-0000-0000-000000000021','00000072-5000-0000-0000-000000000072','00000022-0000-0000-0001-000000000022');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000022-6000-0000-0000-000000000022','00000073-5000-0000-0000-000000000073','00000022-0000-0000-0001-000000000022');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000023-6000-0000-0000-000000000023','00000074-5000-0000-0000-000000000074','00000022-0000-0000-0001-000000000022');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000024-6000-0000-0000-000000000024','00000070-5000-0000-0000-000000000070','00000022-0000-0000-0001-000000000022');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000025-6000-0000-0000-000000000025','00000031-5000-0000-0000-000000000031','00000031-0000-0000-0001-000000000031');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000026-6000-0000-0000-000000000026','00000020-5000-0000-0000-000000000020','00000094-0000-0000-0001-000000000094');
INSERT INTO HrItem (ID,HealthRecordID,WordID,MeasureValue,UomID) VALUES ('00000027-6000-0000-0000-000000000027','00000020-5000-0000-0000-000000000020','00000003-0000-0000-0001-000000000003',60,'00000001-9000-0000-0000-000000000001');
INSERT INTO HrItem (ID,HealthRecordID,WordID) VALUES ('00000028-6000-0000-0000-000000000028','00000072-5000-0000-0000-000000000072','00000001-0000-0000-0001-000000000001');