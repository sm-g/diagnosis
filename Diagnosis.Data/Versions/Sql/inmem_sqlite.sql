-- '00000001-7000-0000-0000-000000000001'
INSERT INTO [HrCategory] ([ID], [Title], [Ord]) VALUES ('00000001-7000-0000-0000-000000000001', 'Жалоба', 1);
INSERT INTO [HrCategory] ([ID], [Title], [Ord]) VALUES ('00000002-7000-0000-0000-000000000002', 'История', 2);
INSERT INTO [HrCategory] ([ID], [Title], [Ord]) VALUES ('00000003-7000-0000-0000-000000000003', 'Осмотр', 3);
INSERT INTO [HrCategory] ([ID], [Title], [Ord]) VALUES ('00000004-7000-0000-0000-000000000004', 'Обследование', 4);
INSERT INTO [HrCategory] ([ID], [Title], [Ord]) VALUES ('00000005-7000-0000-0000-000000000005', 'Диагноз', 5);
INSERT INTO [HrCategory] ([ID], [Title], [Ord]) VALUES ('00000006-7000-0000-0000-000000000006', 'Лечение', 6);

-- '00000001-8000-0000-0000-000000000001'
INSERT INTO [UomType] ([ID], [Title], [Ord]) VALUES ('00000001-8000-0000-0000-000000000001', 'Объем', 1);
INSERT INTO [UomType] ([ID], [Title], [Ord]) VALUES ('00000002-8000-0000-0000-000000000002', 'Время', 2);

-- '00000001-9000-0000-0000-000000000001'
INSERT INTO [Uom] ([ID],[Abbr],[Factor],[UomTypeID],[Description]) VALUES ('00000001-9000-0000-0000-000000000001','л',3,'00000001-8000-0000-0000-000000000001','литр');
INSERT INTO [Uom] ([ID],[Abbr],[Factor],[UomTypeID],[Description]) VALUES ('00000002-9000-0000-0000-000000000002','мл',0,'00000001-8000-0000-0000-000000000001','милилитр');
INSERT INTO [Uom] ([ID],[Abbr],[Factor],[UomTypeID],[Description]) VALUES ('00000003-9000-0000-0000-000000000003','мкл',-3,'00000001-8000-0000-0000-000000000001','микролитр');
INSERT INTO [Uom] ([ID],[Abbr],[Factor],[UomTypeID],[Description]) VALUES ('00000004-9000-0000-0000-000000000004','сут',0,'00000002-8000-0000-0000-000000000002','сутки');
INSERT INTO [Uom] ([ID],[Abbr],[Factor],[UomTypeID],[Description]) VALUES ('00000005-9000-0000-0000-000000000005','нед',0.8451,'00000002-8000-0000-0000-000000000002','неделя');
INSERT INTO [Uom] ([ID],[Abbr],[Factor],[UomTypeID],[Description]) VALUES ('00000006-9000-0000-0000-000000000006','мес',1.4771,'00000002-8000-0000-0000-000000000002','месяц');
INSERT INTO [Uom] ([ID],[Abbr],[Factor],[UomTypeID],[Description]) VALUES ('00000007-9000-0000-0000-000000000007','г',2.5623,'00000002-8000-0000-0000-000000000002','год');

INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (1, 'I', 'Некоторые инфекционные и паразитарные болезни');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (5, 'IX', 'Болезни системы кровообращения');

INSERT INTO [IcdBlock] ([ID], [Code], [Title], [ChapterID]) VALUES (91, '(I00-I02)', 'Острая ревматическая лихорадка', 5);
INSERT INTO [IcdBlock] ([ID], [Code], [Title], [ChapterID]) VALUES (92, '(I05-I09)', 'Хронические ревматические болезни сердца', 5);

INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (1 , 'I00', 'Ревматическая лихорадка без упоминания о  вовлечении сердца', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (2 , 'I01', 'Ревматическая лихорадка с вовлечением сердца', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3 , 'I01.0', 'Острый ревматический перикардит', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (4 , 'I01.1', 'Острый ревматический эндокардит', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (5 , 'I01.2', 'Острый ревматический миокардит', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (6 , 'I01.8', 'Другие острые ревматические болезни сердца', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (7 , 'I01.9', 'Острая ревматическая болезнь сердца неуточненная', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (8 , 'I02', 'Ревматическая хорея', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (9 , 'I02.0', 'Ревматическая хорея с вовлечением сердца', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (10, 'I02.9', 'Ревматическая хорея без вовлечения сердца', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (11, 'I05', 'Ревматические болезни митрального клапана', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (12, 'I05.0', 'Митральный стеноз', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (13, 'I05.1', 'Ревматическая недостаточность митрального клапана', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (14, 'I05.2', 'Митральный стеноз с недостаточностью', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (15, 'I05.8', 'Другие болезни митрального клапана', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (16, 'I05.9', 'Болезнь митрального клапана неуточненная', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (17, 'I06', 'Ревматические болезни аортального клапана', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (18, 'I06.0', 'Ревматический аортальный стеноз', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (19, 'I06.1', 'Ревматическая недостаточность аортального клапана', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (20, 'I06.2', 'Ревматический аортальный стеноз с недостаточностью', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (21, 'I06.8', 'Другие ревматические болезни аортального клапана', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (22, 'I06.9', 'Ревматическая болезнь аортального клапана неуточненная (ый)', 92);

-- '00000001-1200-0000-0001-000000000001'
INSERT INTO Vocabulary ([Id], [Title]) VALUES ('00000001-1200-0000-0000-000000000001', 'Общий словарь');
INSERT INTO Vocabulary ([Id], [Title]) VALUES ('00000002-1200-0000-0000-000000000002', 'Слова кардиолога');

-- '00000001-1300-0000-0001-000000000001'
INSERT INTO [WordTemplate] ([Id],[VocabularyID],[Title]) VALUES ('00000001-1300-0000-0000-000000000001','00000001-1200-0000-0000-000000000001','порфирия');
INSERT INTO [WordTemplate] ([Id],[VocabularyID],[Title]) VALUES ('00000002-1300-0000-0000-000000000002','00000001-1200-0000-0000-000000000001','тест');
INSERT INTO [WordTemplate] ([Id],[VocabularyID],[Title]) VALUES ('00000003-1300-0000-0000-000000000003','00000001-1200-0000-0000-000000000001','шаблон');
INSERT INTO [WordTemplate] ([Id],[VocabularyID],[Title]) VALUES ('00000004-1300-0000-0000-000000000004','00000002-1200-0000-0000-000000000002','понос'); -- w6
INSERT INTO [WordTemplate] ([Id],[VocabularyID],[Title]) VALUES ('00000005-1300-0000-0000-000000000005','00000002-1200-0000-0000-000000000002','повторно');
INSERT INTO [WordTemplate] ([Id],[VocabularyID],[Title]) VALUES ('00000006-1300-0000-0000-000000000006','00000002-1200-0000-0000-000000000002','тест');
INSERT INTO [WordTemplate] ([Id],[VocabularyID],[Title]) VALUES ('00000007-1300-0000-0000-000000000007','00000002-1200-0000-0000-000000000002','шаблон');

-- '00000001-1400-0000-0001-000000000001'
--INSERT INTO [SpecialityIcdBlocks] ([Id],[VocabularyID], [WordTemplateID]) VALUES ('00000001-1400-0000-0001-000000000001','00000001-1000-0000-0001-000000000001', 92);


-- '00000001-0000-0000-0001-000000000001'
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000001-0000-0000-0001-000000000001','анемия');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000002-0000-0000-0001-000000000002','озноб');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000003-0000-0000-0001-000000000003','кашель');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000004-0000-0000-0001-000000000004','впервые');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000005-0000-0000-0001-000000000005','повторно');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000006-0000-0000-0001-000000000006','понос');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000007-0000-0000-0001-000000000007','пневмоторекс');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000008-0000-0000-0001-000000000008','одышка');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000009-0000-0000-0001-000000000009','удушье');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000010-0000-0000-0001-000000000010','кровохаркание');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000011-0000-0000-0001-000000000011','потери сознания');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000012-0000-0000-0001-000000000012','ожирение');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000013-0000-0000-0001-000000000013','гипертрофия');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000014-0000-0000-0001-000000000014','кардиомегалия');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000015-0000-0000-0001-000000000015','асцит');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000016-0000-0000-0001-000000000016','головокружение');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000017-0000-0000-0001-000000000017','инфильтрация');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000018-0000-0000-0001-000000000018','беременность');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000019-0000-0000-0001-000000000019','лихорадка');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000020-0000-0000-0001-000000000020','лейкоцитоз');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000021-0000-0000-0001-000000000021','запоры');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000022-0000-0000-0001-000000000022','роды');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000023-0000-0000-0001-000000000023','кровотечение');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000024-0000-0000-0001-000000000024','гематурия');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000025-0000-0000-0001-000000000025','аритмия');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000026-0000-0000-0001-000000000026','цианоз');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000027-0000-0000-0001-000000000027','тромбоэмболия');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000028-0000-0000-0001-000000000028','желудочковые экстрасистолы');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000029-0000-0000-0001-000000000029','гемоглобин');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000030-0000-0000-0001-000000000030','легочная артерия');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000031-0000-0000-0001-000000000031','щитовидная железа');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000032-0000-0000-0001-000000000032','легкие');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000033-0000-0000-0001-000000000033','ЭКГ');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000034-0000-0000-0001-000000000034','коронарография');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000035-0000-0000-0001-000000000035','функция');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000036-0000-0000-0001-000000000036','гипергликемия');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000037-0000-0000-0001-000000000037','белок');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000038-0000-0000-0001-000000000038','лейкоциты');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000039-0000-0000-0001-000000000039','стоя');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000040-0000-0000-0001-000000000040','сидя');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000041-0000-0000-0001-000000000041','лежа');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000042-0000-0000-0001-000000000042','справа');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000043-0000-0000-0001-000000000043','слева');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000044-0000-0000-0001-000000000044','пароксизм');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000045-0000-0000-0001-000000000045','физическая нагрузка');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000046-0000-0000-0001-000000000046','инфаркт миокарда');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000047-0000-0000-0001-000000000047','язвенная болезнь');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000048-0000-0000-0001-000000000048','гепатит');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000049-0000-0000-0001-000000000049','инсульт');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000050-0000-0000-0001-000000000050','сахарный диабет');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000051-0000-0000-0001-000000000051','фибрилляция предсердий');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000052-0000-0000-0001-000000000052','трикуспидальный');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000053-0000-0000-0001-000000000053','митральный');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000054-0000-0000-0001-000000000054','аортальный');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000055-0000-0000-0001-000000000055','транзиторная ишемическая атака');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000056-0000-0000-0001-000000000056','в груди');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000057-0000-0000-0001-000000000057','в голове');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000058-0000-0000-0001-000000000058','в ноге');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000059-0000-0000-0001-000000000059','в животе');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000060-0000-0000-0001-000000000060','в моче');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000061-0000-0000-0001-000000000061','отделение');
INSERT INTO [Word] ([Id],[Title],[ParentID]) VALUES ('00000062-0000-0000-0001-000000000062','пульмонологическое','00000061-0000-0000-0001-000000000061');
INSERT INTO [Word] ([Id],[Title],[ParentID]) VALUES ('00000063-0000-0000-0001-000000000063','глазное','00000061-0000-0000-0001-000000000061');
INSERT INTO [Word] ([Id],[Title],[ParentID]) VALUES ('00000064-0000-0000-0001-000000000064','хирургическое','00000061-0000-0000-0001-000000000061');
INSERT INTO [Word] ([Id],[Title],[ParentID]) VALUES ('00000065-0000-0000-0001-000000000065','травматологическое','00000061-0000-0000-0001-000000000061');
INSERT INTO [Word] ([Id],[Title],[ParentID]) VALUES ('00000066-0000-0000-0001-000000000066','терапевтическое','00000061-0000-0000-0001-000000000061');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000067-0000-0000-0001-000000000067','поликлиника');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000068-0000-0000-0001-000000000068','скорая мед. помощь');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000069-0000-0000-0001-000000000069','гепарин');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000070-0000-0000-0001-000000000070','лазикс');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000071-0000-0000-0001-000000000071','варфорин');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000072-0000-0000-0001-000000000072','антибиотики');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000073-0000-0000-0001-000000000073','обезболивание');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000074-0000-0000-0001-000000000074','нитроглицерин');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000075-0000-0000-0001-000000000075','госпитализация');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000076-0000-0000-0001-000000000076','операция');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000077-0000-0000-0001-000000000077','Новокузнецк');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000078-0000-0000-0001-000000000078','Кемерово');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000079-0000-0000-0001-000000000079','Новосибирск');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000080-0000-0000-0001-000000000080','Томск');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000081-0000-0000-0001-000000000081','образование');
INSERT INTO [Word] ([Id],[Title],[ParentID]) VALUES ('00000082-0000-0000-0001-000000000082','высшее','00000081-0000-0000-0001-000000000081');
INSERT INTO [Word] ([Id],[Title],[ParentID]) VALUES ('00000083-0000-0000-0001-000000000083','среднее','00000081-0000-0000-0001-000000000081');
INSERT INTO [Word] ([Id],[Title],[ParentID]) VALUES ('00000084-0000-0000-0001-000000000084','начальное','00000081-0000-0000-0001-000000000081');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000085-0000-0000-0001-000000000085','эндокардит');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000086-0000-0000-0001-000000000086','миокардит');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000087-0000-0000-0001-000000000087','перикардит');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000088-0000-0000-0001-000000000088','давность');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000089-0000-0000-0001-000000000089','группа крови');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000090-0000-0000-0001-000000000090','стенокардия');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000091-0000-0000-0001-000000000091','АД');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000092-0000-0000-0001-000000000092','мелкоочаговый');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000093-0000-0000-0001-000000000093','передне-боковой стенки');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000094-0000-0000-0001-000000000094','ЛЖ');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000095-0000-0000-0001-000000000095','давящая');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000096-0000-0000-0001-000000000096','боль');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000097-0000-0000-0001-000000000097','грудина');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000098-0000-0000-0001-000000000098','иррадиация');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000099-0000-0000-0001-000000000099','левая лопатка');
INSERT INTO [Word] ([Id],[Title]) VALUES ('00000100-0000-0000-0001-000000000100','нитроминт');

-- '00000001-1000-0000-0001-000000000001'
INSERT INTO [Speciality] ([Id], [Title]) VALUES ('00000001-1000-0000-0000-000000000001', 'Кардиолог');

-- '00000001-1100-0000-0001-000000000001'
INSERT INTO [SpecialityIcdBlocks] ([Id],[SpecialityID], [IcdBlockID]) VALUES ('00000001-1100-0000-0000-000000000001','00000001-1000-0000-0000-000000000001', 92);

-- '00000001-1000-0000-0000-000000000001'
INSERT INTO [Passport] ([ID]) VALUES ('00000001-1000-0000-0000-000000000001');
INSERT INTO [Passport] ([ID],[Remember]) VALUES ('00000002-1000-0000-0000-000000000002',1);
INSERT INTO [Doctor] ([ID], [LastName], [MiddleName], [FirstName], [IsMale]) VALUES ('00000001-1000-0000-0000-000000000001', 'Смирнов', NULL, 'Василий', 1);
INSERT INTO [Doctor] ([ID], [LastName], [MiddleName], [FirstName], [IsMale], [SpecialityID]) VALUES ('00000002-1000-0000-0000-000000000002', 'Вахрушев', 'Константинович', 'Александр', 1, '00000001-1000-0000-0000-000000000001');
-- '00000001-2000-0000-0000-000000000001'
INSERT INTO [Patient] ([ID], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay],[CreatedAt],[UpdatedAt]) VALUES ('00000001-2000-0000-0000-000000000001', 'Андреев', NULL, 'Иван', 1, 1982, 11, NULL,'2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO [Patient] ([ID], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay],[CreatedAt],[UpdatedAt]) VALUES ('00000002-2000-0000-0000-000000000002', 'Петров', 'Иванович', 'Иван', 1, 1980, 2, 15,'2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO [Patient] ([ID], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay],[CreatedAt],[UpdatedAt]) VALUES ('00000003-2000-0000-0000-000000000003', 'Иванов', 'Иванович', 'Иван', 1, 1995, NULL, 10,'2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO [Patient] ([ID], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay],[CreatedAt],[UpdatedAt]) VALUES ('00000004-2000-0000-0000-000000000004', 'Михайлов', NULL, 'Валентин', 1, 1973, NULL, NULL,'2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO [Patient] ([ID], [LastName], [MiddleName], [FirstName], [IsMale],[CreatedAt],[UpdatedAt]) VALUES ('00000005-2000-0000-0000-000000000005', NULL, NULL, NULL, 0,'2014-11-19 00:00:00','2014-11-19 00:00:00');
-- '00000001-3000-0000-0000-000000000001'
INSERT INTO [Course] ([ID], [PatientID], [StartDate], [EndDate], [DoctorID],[CreatedAt],[UpdatedAt]) VALUES ('00000001-3000-0000-0000-000000000001', '00000001-2000-0000-0000-000000000001', '2014-03-07 00:00:00', '2014-03-14 00:00:00', '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO [Course] ([ID], [PatientID], [StartDate], [EndDate], [DoctorID],[CreatedAt],[UpdatedAt]) VALUES ('00000002-3000-0000-0000-000000000002', '00000001-2000-0000-0000-000000000001', '2013-05-29 00:00:00', NULL, '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO [Course] ([ID], [PatientID], [StartDate], [EndDate], [DoctorID],[CreatedAt],[UpdatedAt]) VALUES ('00000003-3000-0000-0000-000000000003', '00000005-2000-0000-0000-000000000005', '2014-05-04 00:00:00', NULL, '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO [Course] ([ID], [PatientID], [StartDate], [EndDate], [DoctorID],[CreatedAt],[UpdatedAt]) VALUES ('00000004-3000-0000-0000-000000000004', '00000003-2000-0000-0000-000000000003', '2014-05-04 00:00:00', NULL, '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
-- '00000001-4000-0000-0000-000000000001'
INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID],[CreatedAt],[UpdatedAt]) VALUES ('00000001-4000-0000-0000-000000000001', '2014-03-10 07:39:48', '00000001-3000-0000-0000-000000000001', '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID],[CreatedAt],[UpdatedAt]) VALUES ('00000002-4000-0000-0000-000000000002', '2014-03-14 09:03:56', '00000001-3000-0000-0000-000000000001', '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID],[CreatedAt],[UpdatedAt]) VALUES ('00000003-4000-0000-0000-000000000003', '2014-05-29 02:45:51', '00000002-3000-0000-0000-000000000002', '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID],[CreatedAt],[UpdatedAt]) VALUES ('00000004-4000-0000-0000-000000000004', '2014-06-03 05:39:52', '00000002-3000-0000-0000-000000000002', '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID],[CreatedAt],[UpdatedAt]) VALUES ('00000005-4000-0000-0000-000000000005', '2014-06-03 05:39:52', '00000004-3000-0000-0000-000000000004', '00000001-1000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00');
-- '00000001-5000-0000-0000-000000000001'
INSERT INTO [HealthRecord] ([ID],[CreatedAt],[UpdatedAt],[PatientID],[CourseID],[AppointmentID],[DoctorID],[HrCategoryID],[FromYear],[FromMonth],[FromDay]) VALUES ('00000001-5000-0000-0000-000000000001','2014-11-19 00:00:00','2014-11-19 00:00:00',NULL,NULL,'00000001-4000-0000-0000-000000000001','00000001-1000-0000-0000-000000000001','00000005-7000-0000-0000-000000000005',2013,11,NULL);
INSERT INTO [HealthRecord] ([ID],[CreatedAt],[UpdatedAt],[PatientID],[CourseID],[AppointmentID],[DoctorID],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit]) VALUES ('00000002-5000-0000-0000-000000000002','2014-11-19 00:00:00','2014-11-19 00:00:00',NULL,NULL,'00000001-4000-0000-0000-000000000001','00000001-1000-0000-0000-000000000001','00000003-7000-0000-0000-000000000003',2013,12,NULL,'ByAge');
INSERT INTO [HealthRecord] ([ID],[CreatedAt],[UpdatedAt],[PatientID],[CourseID],[AppointmentID],[DoctorID],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit]) VALUES ('00000020-5000-0000-0000-000000000020','2014-11-20 13:27:46','2014-11-20 13:27:46',NULL,NULL,'00000002-4000-0000-0000-000000000002','00000001-1000-0000-0000-000000000001','00000001-7000-0000-0000-000000000001',2014,1,30,'Month');
INSERT INTO [HealthRecord] ([ID],[CreatedAt],[UpdatedAt],[PatientID],[CourseID],[AppointmentID],[DoctorID],[HrCategoryID]) VALUES ('00000021-5000-0000-0000-000000000021','2014-11-21 13:27:46','2014-11-20 13:27:46',NULL,NULL,'00000002-4000-0000-0000-000000000002','00000001-1000-0000-0000-000000000001','00000005-7000-0000-0000-000000000005');
INSERT INTO [HealthRecord] ([ID],[CreatedAt],[UpdatedAt],[PatientID],[CourseID],[AppointmentID],[DoctorID],[HrCategoryID]) VALUES ('00000022-5000-0000-0000-000000000022','2014-11-22 13:27:46','2014-11-20 13:27:46',NULL,NULL,'00000002-4000-0000-0000-000000000002','00000001-1000-0000-0000-000000000001','00000002-7000-0000-0000-000000000002');
INSERT INTO [HealthRecord] ([ID],[CreatedAt],[UpdatedAt],[PatientID],[CourseID],[AppointmentID],[DoctorID],[HrCategoryID]) VALUES ('00000030-5000-0000-0000-000000000030','2014-11-19 00:00:00','2014-11-19 00:00:00',NULL,NULL,'00000003-4000-0000-0000-000000000003','00000001-1000-0000-0000-000000000001','00000001-7000-0000-0000-000000000001');
INSERT INTO [HealthRecord] ([ID],[CreatedAt],[UpdatedAt],[PatientID],[CourseID],[AppointmentID],[DoctorID],[HrCategoryID]) VALUES ('00000031-5000-0000-0000-000000000031','2014-11-19 00:00:00','2014-11-19 00:00:00',NULL,NULL,'00000003-4000-0000-0000-000000000003','00000001-1000-0000-0000-000000000001','00000002-7000-0000-0000-000000000002');
INSERT INTO [HealthRecord] ([ID],[CreatedAt],[UpdatedAt],[PatientID],[CourseID],[AppointmentID],[DoctorID],[HrCategoryID]) VALUES ('00000032-5000-0000-0000-000000000032','2014-11-19 00:00:00','2014-11-19 00:00:00',NULL,NULL,'00000003-4000-0000-0000-000000000003','00000001-1000-0000-0000-000000000001','00000001-7000-0000-0000-000000000001');
INSERT INTO [HealthRecord] ([ID],[CreatedAt],[UpdatedAt],[PatientID],[CourseID],[AppointmentID],[DoctorID],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit]) VALUES ('00000040-5000-0000-0000-000000000040','2014-11-19 00:00:00','2014-11-19 00:00:00',NULL,NULL,'00000004-4000-0000-0000-000000000004','00000001-1000-0000-0000-000000000001','00000002-7000-0000-0000-000000000002',2005,2,10,'Year');
INSERT INTO [HealthRecord] ([ID],[CreatedAt],[UpdatedAt],[PatientID],[CourseID],[AppointmentID],[DoctorID],[HrCategoryID]) VALUES ('00000070-5000-0000-0000-000000000070','2014-11-19 00:00:00','2014-11-19 00:00:00',NULL,'00000001-3000-0000-0000-000000000001',NULL,'00000001-1000-0000-0000-000000000001','00000001-7000-0000-0000-000000000001');
INSERT INTO [HealthRecord] ([ID],[CreatedAt],[UpdatedAt],[PatientID],[CourseID],[AppointmentID],[DoctorID],[HrCategoryID]) VALUES ('00000071-5000-0000-0000-000000000071','2014-11-19 00:00:00','2014-11-19 00:00:00',NULL,'00000002-3000-0000-0000-000000000002',NULL,'00000001-1000-0000-0000-000000000001','00000001-7000-0000-0000-000000000001');
INSERT INTO [HealthRecord] ([ID],[CreatedAt],[UpdatedAt],[PatientID],[CourseID],[AppointmentID],[DoctorID]) VALUES ('00000072-5000-0000-0000-000000000072','2014-11-19 00:00:00','2014-11-19 00:00:00','00000001-2000-0000-0000-000000000001',NULL,NULL,'00000001-1000-0000-0000-000000000001');
INSERT INTO [HealthRecord] ([ID],[CreatedAt],[UpdatedAt],[PatientID],[CourseID],[AppointmentID],[DoctorID]) VALUES ('00000073-5000-0000-0000-000000000073','2014-11-19 00:00:00','2014-11-19 00:00:00','00000002-2000-0000-0000-000000000002',NULL,NULL,'00000001-1000-0000-0000-000000000001');
INSERT INTO [HealthRecord] ([ID],[CreatedAt],[UpdatedAt],[PatientID],[CourseID],[AppointmentID],[DoctorID]) VALUES ('00000074-5000-0000-0000-000000000074','2014-11-19 00:00:00','2014-11-19 00:00:00','00000002-2000-0000-0000-000000000002',NULL,NULL,'00000001-1000-0000-0000-000000000001');

-- '00000001-6000-0000-0000-000000000001'
INSERT INTO [HrItem] ([ID],[HealthRecordID],[TextRepr]) VALUES ('00000001-6000-0000-0000-000000000001','00000021-5000-0000-0000-000000000021','comment');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[TextRepr],[Ord]) VALUES ('00000002-6000-0000-0000-000000000002','00000021-5000-0000-0000-000000000021','00000001-0000-0000-0001-000000000001','text repr with word',1);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000003-6000-0000-0000-000000000003','00000022-5000-0000-0000-000000000022','00000001-0000-0000-0001-000000000001');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000004-6000-0000-0000-000000000004','00000022-5000-0000-0000-000000000022','00000022-0000-0000-0001-000000000022');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID]) VALUES ('00000005-6000-0000-0000-000000000005','00000022-5000-0000-0000-000000000022','00000003-0000-0000-0001-000000000003',50,'00000001-9000-0000-0000-000000000001');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000006-6000-0000-0000-000000000006','00000030-5000-0000-0000-000000000030','00000005-0000-0000-0001-000000000005');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000007-6000-0000-0000-000000000007','00000001-5000-0000-0000-000000000001','00000001-0000-0000-0001-000000000001');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[Ord]) VALUES ('00000008-6000-0000-0000-000000000008','00000001-5000-0000-0000-000000000001','00000002-0000-0000-0001-000000000002',1);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000009-6000-0000-0000-000000000009','00000002-5000-0000-0000-000000000002','00000001-0000-0000-0001-000000000001');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000010-6000-0000-0000-000000000010','00000002-5000-0000-0000-000000000002','00000004-0000-0000-0001-000000000004');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000011-6000-0000-0000-000000000011','00000031-5000-0000-0000-000000000031','00000051-0000-0000-0001-000000000051');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000012-6000-0000-0000-000000000012','00000031-5000-0000-0000-000000000031','00000094-0000-0000-0001-000000000094');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[IcdDiseaseID]	) VALUES ('00000013-6000-0000-0000-000000000013','00000031-5000-0000-0000-000000000031',1);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000014-6000-0000-0000-000000000014','00000032-5000-0000-0000-000000000032','00000003-0000-0000-0001-000000000003');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000015-6000-0000-0000-000000000015','00000032-5000-0000-0000-000000000032','00000004-0000-0000-0001-000000000004');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000016-6000-0000-0000-000000000016','00000040-5000-0000-0000-000000000040','00000022-0000-0000-0001-000000000022');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000017-6000-0000-0000-000000000017','00000020-5000-0000-0000-000000000020','00000001-0000-0000-0001-000000000001');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000018-6000-0000-0000-000000000018','00000070-5000-0000-0000-000000000070','00000022-0000-0000-0001-000000000022');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000019-6000-0000-0000-000000000019','00000030-5000-0000-0000-000000000030','00000031-0000-0000-0001-000000000031');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000020-6000-0000-0000-000000000020','00000030-5000-0000-0000-000000000030','00000032-0000-0000-0001-000000000032');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000021-6000-0000-0000-000000000021','00000072-5000-0000-0000-000000000072','00000022-0000-0000-0001-000000000022');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000022-6000-0000-0000-000000000022','00000073-5000-0000-0000-000000000073','00000022-0000-0000-0001-000000000022');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000023-6000-0000-0000-000000000023','00000074-5000-0000-0000-000000000074','00000022-0000-0000-0001-000000000022');
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID]) VALUES ('00000024-6000-0000-0000-000000000024','00000070-5000-0000-0000-000000000070','00000022-0000-0000-0001-000000000022');

