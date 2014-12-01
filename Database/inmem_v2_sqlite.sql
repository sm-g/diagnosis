INSERT INTO [HrCategory] ([ID], [Title], [Ord]) VALUES (1, 'Жалоба', 1);
INSERT INTO [HrCategory] ([ID], [Title], [Ord]) VALUES (2, 'История', 2);
INSERT INTO [HrCategory] ([ID], [Title], [Ord]) VALUES (3, 'Осмотр', 3);
INSERT INTO [HrCategory] ([ID], [Title], [Ord]) VALUES (4, 'Обследование', 4);
INSERT INTO [HrCategory] ([ID], [Title], [Ord]) VALUES (5, 'Диагноз', 5);
INSERT INTO [HrCategory] ([ID], [Title], [Ord]) VALUES (6, 'Лечение', 6);
-- 6 records


INSERT INTO [Uom] ([ID],[Abbr],[Factor],[UomType],[Description]) VALUES (1,'л',3,1,'литр');
INSERT INTO [Uom] ([ID],[Abbr],[Factor],[UomType],[Description]) VALUES (2,'мл',0,1,'милилитр');
INSERT INTO [Uom] ([ID],[Abbr],[Factor],[UomType],[Description]) VALUES (3,'мкл',-3,1,'микролитр');
INSERT INTO [Uom] ([ID],[Abbr],[Factor],[UomType],[Description]) VALUES (4,'сут',0,2,'сутки');
INSERT INTO [Uom] ([ID],[Abbr],[Factor],[UomType],[Description]) VALUES (5,'нед',0.8451,2,'неделя');
INSERT INTO [Uom] ([ID],[Abbr],[Factor],[UomType],[Description]) VALUES (6,'мес',1.4771,2,'месяц');
INSERT INTO [Uom] ([ID],[Abbr],[Factor],[UomType],[Description]) VALUES (7,'г',2.5623,2,'год');

INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (1, 'I', 'Некоторые инфекционные и паразитарные болезни');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (5, 'IX', 'Болезни системы кровообращения');

INSERT INTO [IcdBlock] ([ID], [Code], [Title], [ChapterID]) VALUES (91, '(I00-I02)', 'Острая ревматическая лихорадка', 5);
INSERT INTO [IcdBlock] ([ID], [Code], [Title], [ChapterID]) VALUES (92, '(I05-I09)', 'Хронические ревматические болезни сердца', 5);

INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (1   , 'I00', 'Ревматическая лихорадка без упоминания о  вовлечении сердца', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (2   , 'I01', 'Ревматическая лихорадка с вовлечением сердца', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3   , 'I01.0', 'Острый ревматический перикардит', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (4   , 'I01.1', 'Острый ревматический эндокардит', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (5   , 'I01.2', 'Острый ревматический миокардит', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (6   , 'I01.8', 'Другие острые ревматические болезни сердца', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (7   , 'I01.9', 'Острая ревматическая болезнь сердца неуточненная', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (8   , 'I02', 'Ревматическая хорея', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (9   , 'I02.0', 'Ревматическая хорея с вовлечением сердца', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (10  , 'I02.9', 'Ревматическая хорея без вовлечения сердца', 91);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (11  , 'I05', 'Ревматические болезни митрального клапана', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (12  , 'I05.0', 'Митральный стеноз', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (13  , 'I05.1', 'Ревматическая недостаточность митрального клапана', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (14  , 'I05.2', 'Митральный стеноз с недостаточностью', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (15  , 'I05.8', 'Другие болезни митрального клапана', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (16  , 'I05.9', 'Болезнь митрального клапана неуточненная', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (17  , 'I06', 'Ревматические болезни аортального клапана', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (18  , 'I06.0', 'Ревматический аортальный стеноз', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (19  , 'I06.1', 'Ревматическая недостаточность аортального клапана', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (20  , 'I06.2', 'Ревматический аортальный стеноз с недостаточностью', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (21  , 'I06.8', 'Другие ревматические болезни аортального клапана', 92);
INSERT INTO [IcdDisease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (22  , 'I06.9', 'Ревматическая болезнь аортального клапана неуточненная (ый)', 92);

INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (1,'анемия',1,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (2,'озноб',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (3,'кашель',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (4,'впервые',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (5,'повторно',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (6,'понос',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (7,'порок сердца',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (8,'пневмоторекс',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (9,'одышка',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (10,'удушье',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (12,'кровохаркание',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (13,'потери сознания',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (14,'ожирение',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (15,'гипертрофия',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (16,'кардиомегалия',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (17,'асцит',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (18,'головокружение',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (19,'инфильтрация',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (20,'беременность',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (21,'лихорадка',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (22,'роды',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (23,'запоры',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (24,'лейкоцитоз',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (25,'кровотечение',5,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (26,'черный стул',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (27,'гематурия',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (28,'аритмия',1,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (29,'цианоз',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (30,'тромбоэмболия',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (31,'желудочковые экстрасистолы',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (32,'гемоглобин',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (33,'легочная артерия',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (34,'щитовидная железа',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (35,'легкие',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (36,'ЭКГ',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (37,'коронарография',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (38,'функция',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (39,'гипергликемия',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (40,'белок',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (41,'лейкоциты',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (42,'стоя',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (43,'сидя',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (44,'лежа',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (45,'справа',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (46,'слева',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (47,'повышена',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (48,'понижена',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (49,'пароксизм',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (50,'физическая нагрузка',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (51,'инфаркт миокарда',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (52,'язвенная болезнь',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (53,'гепатит',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (54,'инсульт',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (55,'сахарный диабет',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (56,'фибрилляция предсердий',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (57,'трикуспидальный',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (58,'митральный',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (59,'аортальный',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (60,'транзиторная ишемическая атака',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (61,'боли',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (62,'в груди',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (63,'в голове',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (64,'в ноге',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (65,'в животе',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (66,'в моче',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (67,'отделение',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (68,'пульмонологическое',NULL,67);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (69,'глазное',NULL,67);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (70,'хирургическое',NULL,67);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (71,'травматологическое',NULL,67);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (72,'терапевтическое',NULL,67);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (73,'поликлиника',NULL,67);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (74,'скорая мед. помощь',NULL,67);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (75,'гепарин',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (76,'лазикс',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (77,'варфорин',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (78,'антибиотики',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (79,'обезболивание',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (80,'нитроглицерин',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (81,'госпитализация',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (82,'операция',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (83,'миокардит',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (84,'Новокузнецк',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (85,'Кемерово',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (86,'Новосибирск',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (87,'Томск',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (88,'образование',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (89,'высшее',NULL,88);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (90,'среднее',NULL,88);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (93,'перикардит',NULL,NULL);
INSERT INTO [Word] ([Id],[Title],[DefHrCategoryID],[ParentID]) VALUES (94,'давность',NULL,NULL);



INSERT INTO [Speciality] ([ID], [Title]) VALUES (1, 'Кардиолог');


INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [IcdBlockID]) VALUES (1, 1, 92);


INSERT INTO [Doctor] ([ID], [LastName], [MiddleName], [FirstName], [IsMale], [SpecialityID], [Settings]) VALUES (1, 'Смирнов', NULL, 'Василий', 1, 1, 0);
INSERT INTO [Doctor] ([ID], [LastName], [MiddleName], [FirstName], [IsMale], [SpecialityID], [Settings]) VALUES (2, 'Вахрушев', 'Константинович', 'Александр', 1, 1, 1);

INSERT INTO [Patient] ([ID], [Label], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay]) VALUES (1, 'абв', 'Андреев', NULL, 'Иван', 1, 1982, 11, NULL);
INSERT INTO [Patient] ([ID], [Label], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay]) VALUES (2, NULL, 'Петров', 'Иванович', 'Иван', 1, 1980, 2, 15);
INSERT INTO [Patient] ([ID], [Label], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay]) VALUES (3, NULL, 'Иванов', 'Иванович', 'Иван', 1, 1995, NULL, 10);
INSERT INTO [Patient] ([ID], [Label], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay]) VALUES (4, NULL, 'Михайлов', NULL, 'Валентин', 1, 1973, NULL, NULL);
INSERT INTO [Patient] ([ID], [Label], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay]) VALUES (5, '5', NULL, NULL, NULL, 0, NULL, NULL, NULL);

INSERT INTO [Course] ([ID], [PatientID], [StartDate], [EndDate], [DoctorID]) VALUES (1, 1, '2014-03-07 00:00:00', '2014-03-14 00:00:00', 1);
INSERT INTO [Course] ([ID], [PatientID], [StartDate], [EndDate], [DoctorID]) VALUES (2, 1, '2013-05-29 00:00:00', NULL, 1);
INSERT INTO [Course] ([ID], [PatientID], [StartDate], [EndDate], [DoctorID]) VALUES (3, 5, '2014-05-04 00:00:00', NULL, 1);
INSERT INTO [Course] ([ID], [PatientID], [StartDate], [EndDate], [DoctorID]) VALUES (4, 3, '2014-05-04 00:00:00', NULL, 1);

INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID]) VALUES (1, '2014-03-10 07:39:48', 1, 1);
INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID]) VALUES (2, '2014-03-14 09:03:56', 1, 1);
INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID]) VALUES (3, '2014-05-29 02:45:51', 2, 1);
INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID]) VALUES (4, '2014-06-03 05:39:52', 2, 1);
INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID]) VALUES (5, '2014-06-03 05:39:52', 4, 1);

INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit],[IsDeleted]) VALUES (1,NULL,NULL,1,NULL,5,2013,11,NULL,'NotSet',0);
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit],[IsDeleted]) VALUES (2,NULL,NULL,1,NULL,3,2013,12,NULL,'NotSet',0);
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit],[IsDeleted]) VALUES (20,NULL,NULL,2,'q',1,2014,1,30,'Month',0);
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit],[IsDeleted]) VALUES (21,NULL,NULL,2,'22',5,NULL,NULL,NULL,'NotSet',0);
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit],[IsDeleted]) VALUES (22,NULL,NULL,2,NULL,2,NULL,NULL,NULL,'NotSet',0);
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit],[IsDeleted]) VALUES (30,NULL,NULL,3,NULL,1,NULL,NULL,NULL,'NotSet',0);
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit],[IsDeleted]) VALUES (31,NULL,NULL,3,NULL,2,NULL,NULL,NULL,'NotSet',0);
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit],[IsDeleted]) VALUES (32,NULL,NULL,3,NULL,1,NULL,NULL,NULL,'NotSet',0);
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit],[IsDeleted]) VALUES (40,NULL,NULL,4,NULL,2,2005,2,10,'Year',0);
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit],[IsDeleted]) VALUES (70,NULL,1,NULL,'курс',1,NULL,NULL,NULL,'NotSet',0);
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit],[IsDeleted]) VALUES (71,NULL,2,NULL,'курс 2',1,NULL,NULL,NULL,'NotSet',0);
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit],[IsDeleted]) VALUES (72,1,NULL,NULL,'пациент',NULL,NULL,NULL,NULL,'NotSet',0);
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit],[IsDeleted]) VALUES (73,2,NULL,NULL,'пациент 2',NULL,NULL,NULL,NULL,'NotSet',0);
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit],[IsDeleted]) VALUES (74,2,NULL,NULL,'пациент 2 дубль',NULL,NULL,NULL,NULL,'NotSet',0);


INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[TextRepr],[Ord]) VALUES (2,21,1,'text repr with word',1);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (3,22,1,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (4,22,22,NULL,NULL,NULL,0);
--INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (5,22,NULL,50,4,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (6,30,5,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (7,1,1,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (8,1,2,NULL,NULL,NULL,1);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (9,2,1,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (10,2,4,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (11,31,51,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (12,31,94,NULL,NULL,NULL,0);
--INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (13,31,NULL,30,4,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (14,32,3,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (15,32,4,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (16,40,22,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (17,20,1,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (18,70,22,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (19,30,31,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (20,30,32,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (21,72,22,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (22,73,22,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (23,74,22,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (25,70,22,NULL,NULL,NULL,0);

