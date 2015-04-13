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

SET IDENTITY_INSERT IcdChapter ON;
INSERT INTO IcdChapter (ID, Code, Title) VALUES (1, 'I', 'Некоторые инфекционные и паразитарные болезни');
INSERT INTO IcdChapter (ID, Code, Title) VALUES (5, 'IX', 'Болезни системы кровообращения');
SET IDENTITY_INSERT IcdChapter OFF;

SET IDENTITY_INSERT IcdBlock ON;
INSERT INTO IcdBlock (ID, Code, Title, ChapterID) VALUES (91, '(I00-I02)', 'Острая ревматическая лихорадка', 5);
INSERT INTO IcdBlock (ID, Code, Title, ChapterID) VALUES (92, '(I05-I09)', 'Хронические ревматические болезни сердца', 5);
SET IDENTITY_INSERT IcdBlock OFF;

SET IDENTITY_INSERT IcdDisease ON;
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
SET IDENTITY_INSERT IcdDisease OFF;

-- '00000001-1200-0000-0001-000000000001'
INSERT INTO Vocabulary (Id, Title) VALUES ('00000001-1200-0000-0000-000000000001', 'Общий словарь');
INSERT INTO Vocabulary (Id, Title) VALUES ('00000002-1200-0000-0000-000000000002', 'Слова кардиолога');
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