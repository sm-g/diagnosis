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
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (2, 'II', 'Новообразования');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (3, 'III', 'Болезни крови, кроветворных органов и отдельные нарушения, вовлекающие иммунный механизм');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (4, 'IV', 'Болезни эндокринной системы, расстройства питания и нарушения обмена веществ');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (5, 'IX', 'Болезни системы кровообращения');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (6, 'V', 'Психические расстройства и расстройства поведения');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (7, 'VI', 'Болезни нервной системы');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (8, 'VII', 'Болезни глаза и его придаточного аппарата');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (9, 'VIII', 'Болезни уха и сосцевидного отростка');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (10, 'X', 'Болезни органов дыхания');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (11, 'XI', 'Болезни органов пищеварения');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (12, 'XII', 'Болезни кожи и подкожной клетчатки');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (13, 'XIII', 'Болезни костно-мышечной системы и соединительной ткани');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (14, 'XIV', 'Болезни мочеполовой системы');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (15, 'XIX', 'Травмы, отравления и некоторые другие последствия воздействия внешних причин');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (16, 'XV', 'Беременность, роды и послеродовой период');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (17, 'XVI', 'Отдельные состояния, возникающие в перинатальном периоде');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (18, 'XVII', 'Врожденные аномалии [пороки крови], деформации и хромосомные нарушения');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (19, 'XVIII', 'Симптомы, признаки и отклонения от нормы, выявленные при клинических и лабораторных исследованиях, не классифицированные в других рубриках');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (20, 'XX', 'Внешние причины заболеваемости и смертности');
INSERT INTO [IcdChapter] ([ID], [Code], [Title]) VALUES (21, 'XXI', 'Факторы, влияющие на состояние здоровья населения и обращения в учреждения здравоохранения');
-- 21 records

INSERT INTO [IcdBlock] ([ID], [Code], [Title], [ChapterID]) VALUES (91, '(I00-I02)', 'Острая ревматическая лихорадка', 5);
INSERT INTO [IcdBlock] ([ID], [Code], [Title], [ChapterID]) VALUES (92, '(I05-I09)', 'Хронические ревматические болезни сердца', 5);
INSERT INTO [IcdBlock] ([ID], [Code], [Title], [ChapterID]) VALUES (93, '(I10-I15)', 'Болезни, характеризующиеся повышенным кровяным давлением', 5);
INSERT INTO [IcdBlock] ([ID], [Code], [Title], [ChapterID]) VALUES (94, '(I20-I25)', 'Ишемическая болезнь сердца', 5);
INSERT INTO [IcdBlock] ([ID], [Code], [Title], [ChapterID]) VALUES (95, '(I26-I28)', 'Легочное сердце и нарушения легочного кровообращения', 5);
INSERT INTO [IcdBlock] ([ID], [Code], [Title], [ChapterID]) VALUES (96, '(I30-I52)', 'Другие болезни сердца', 5);
INSERT INTO [IcdBlock] ([ID], [Code], [Title], [ChapterID]) VALUES (97, '(I60-I69)', 'Цереброваскулярные болезни', 5);
INSERT INTO [IcdBlock] ([ID], [Code], [Title], [ChapterID]) VALUES (98, '(I70-I79)', 'Болезни артерий, артериол и капилляров', 5);
INSERT INTO [IcdBlock] ([ID], [Code], [Title], [ChapterID]) VALUES (99, '(I80-I89)', 'Болезни вен, лимфатических сосудов и лимфатических узлов, не классифицированные в других рубриках', 5);
INSERT INTO [IcdBlock] ([ID], [Code], [Title], [ChapterID]) VALUES (100, '(I95-I99)', 'Другие и неуточненные болезни системы кровообращения', 5);


INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3659, 'I00', 'Ревматическая лихорадка без упоминания о  вовлечении сердца', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3660, 'I01', 'Ревматическая лихорадка с вовлечением сердца', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3661, 'I01.0', 'Острый ревматический перикардит', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3662, 'I01.1', 'Острый ревматический эндокардит', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3663, 'I01.2', 'Острый ревматический миокардит', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3664, 'I01.8', 'Другие острые ревматические болезни сердца', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3665, 'I01.9', 'Острая ревматическая болезнь сердца неуточненная', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3666, 'I02', 'Ревматическая хорея', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3667, 'I02.0', 'Ревматическая хорея с вовлечением сердца', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3668, 'I02.9', 'Ревматическая хорея без вовлечения сердца', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3669, 'I05', 'Ревматические болезни митрального клапана', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3670, 'I05.0', 'Митральный стеноз', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3671, 'I05.1', 'Ревматическая недостаточность митрального клапана', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3672, 'I05.2', 'Митральный стеноз с недостаточностью', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3673, 'I05.8', 'Другие болезни митрального клапана', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3674, 'I05.9', 'Болезнь митрального клапана неуточненная', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3675, 'I06', 'Ревматические болезни аортального клапана', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3676, 'I06.0', 'Ревматический аортальный стеноз', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3677, 'I06.1', 'Ревматическая недостаточность аортального клапана', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3678, 'I06.2', 'Ревматический аортальный стеноз с недостаточностью', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3679, 'I06.8', 'Другие ревматические болезни аортального клапана', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [IcdBlockID]) VALUES (3680, 'I06.9', 'Ревматическая болезнь аортального клапана неуточненная (ый)', 92);

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
INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [IcdBlockID]) VALUES (2, 1, 93);
INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [IcdBlockID]) VALUES (3, 1, 94);
INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [IcdBlockID]) VALUES (4, 1, 95);
INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [IcdBlockID]) VALUES (5, 1, 96);
INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [IcdBlockID]) VALUES (6, 1, 97);
INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [IcdBlockID]) VALUES (7, 1, 98);
INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [IcdBlockID]) VALUES (8, 1, 99);
INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [IcdBlockID]) VALUES (9, 1, 100);


INSERT INTO [Doctor] ([ID], [LastName], [MiddleName], [FirstName], [IsMale], [SpecialityID], [Settings]) VALUES (1, 'Смирнов', NULL, 'Василий', -1, 1, 0);
INSERT INTO [Doctor] ([ID], [LastName], [MiddleName], [FirstName], [IsMale], [SpecialityID], [Settings]) VALUES (2, 'Вахрушев', 'Константинович', 'Александр', -1, 1, 1);

INSERT INTO [Patient] ([ID], [Label], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay]) VALUES (1, 'абв', 'Андреев', NULL, 'Иван', -1, 1982, 11, NULL);
INSERT INTO [Patient] ([ID], [Label], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay]) VALUES (2, NULL, 'Петров', 'Иванович', 'Иван', -1, 1980, 2, 15);
INSERT INTO [Patient] ([ID], [Label], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay]) VALUES (3, NULL, 'Иванов', 'Иванович', 'Иван', -1, 1995, NULL, 10);
INSERT INTO [Patient] ([ID], [Label], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay]) VALUES (4, NULL, 'Михайлов', NULL, 'Валентин', -1, 1973, NULL, NULL);
INSERT INTO [Patient] ([ID], [Label], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay]) VALUES (5, '5', NULL, NULL, NULL, 0, NULL, NULL, NULL);

INSERT INTO [Course] ([ID], [PatientID], [StartDate], [EndDate], [DoctorID]) VALUES (1, 1, '2014-03-07 00:00:00', '2014-03-14 00:00:00', 1);
INSERT INTO [Course] ([ID], [PatientID], [StartDate], [EndDate], [DoctorID]) VALUES (2, 1, '2013-05-29 00:00:00', NULL, 1);
INSERT INTO [Course] ([ID], [PatientID], [StartDate], [EndDate], [DoctorID]) VALUES (3, 5, '2014-05-04 00:00:00', NULL, 1);

INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID]) VALUES (1, '2014-03-10 07:39:48', 1, 1);
INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID]) VALUES (2, '2014-05-04 09:03:56', 1, 1);
INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID]) VALUES (3, '2014-05-29 02:45:51', 2, 1);
INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID]) VALUES (4, '2014-06-03 05:39:52', 2, 1);

INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit]) VALUES (6,NULL,NULL,1,NULL,5,2013,11,NULL,'NotSet');
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit]) VALUES (7,NULL,NULL,1,NULL,3,2013,12,NULL,'NotSet');
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit]) VALUES (11,NULL,NULL,2,'q',1,2014,1,30,'Month');
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit]) VALUES (38,NULL,NULL,2,'22',5,NULL,NULL,NULL,'NotSet');
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit]) VALUES (65,NULL,NULL,2,NULL,2,NULL,NULL,NULL,'NotSet');
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit]) VALUES (66,NULL,NULL,3,NULL,1,NULL,NULL,NULL,'NotSet');
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit]) VALUES (67,NULL,NULL,3,NULL,2,NULL,NULL,NULL,'NotSet');
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit]) VALUES (68,NULL,NULL,3,NULL,1,NULL,NULL,NULL,'NotSet');
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit]) VALUES (69,NULL,NULL,4,NULL,2,2005,2,10,'Year');
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit]) VALUES (70,NULL,1,NULL,'запись курса',1,NULL,NULL,NULL,'NotSet');
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit]) VALUES (71,NULL,2,NULL,'запись курса 2',1,NULL,NULL,NULL,'NotSet');
INSERT INTO [HealthRecord] ([ID],[PatientID],[CourseID],[AppointmentID],[Comment],[HrCategoryID],[FromYear],[FromMonth],[FromDay],[Unit]) VALUES (72,1,NULL,NULL,'запись пациента',NULL,NULL,NULL,NULL,'NotSet');


INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (2,38,1,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (3,65,1,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (4,65,22,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (5,65,NULL,50,7,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (6,66,5,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (7,6,7,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (8,6,NULL,40,7,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (9,7,1,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (10,7,4,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (11,67,51,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (12,67,94,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (13,67,NULL,30,7,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (14,68,3,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (15,68,4,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (16,69,22,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (17,11,1,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (18,11,44,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (19,66,67,NULL,NULL,NULL,0);
INSERT INTO [HrItem] ([ID],[HealthRecordID],[WordID],[MeasureValue],[UomID],[DiseaseID],[Ord]) VALUES (20,66,68,NULL,NULL,NULL,0);

