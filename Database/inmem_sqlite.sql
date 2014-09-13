--
-- DUMP FILE
--
-- Database is ported from MS Access
--------------------------------------------------------------------
-- Created using "MS Access to MSSQL" form http://www.bullzip.com
-- Program Version 5.1.242
--
-- OPTIONS:
--   sourcefilename=C:\Users\smg\Documents\Visual Studio 2012\Projects\Diagnosis\Database\diagnosis — копия.accdb
--   sourceusername=
--   sourcepassword=
--   sourcesystemdatabase=
--   destinationserver=SERVER\SQLEXPRESS
--   destinationauthentication=Windows
--   destinationdatabase=movedb
--   dropdatabase=0
--   createtables=0
--   unicode=1
--   autocommit=1
--   transferdefaultvalues=1
--   transferindexes=1
--   transferautonumbers=1
--   transferrecords=1
--   columnlist=1
--   tableprefix=
--   negativeboolean=0
--   ignorelargeblobs=0
--   memotype=
--

IF NOT EXISTS (SELECT * FROM master.dbo.sysdatabases WHERE name = 'movedb') CREATE DATABASE [movedb]
USE [movedb]

--
-- Dumping data for table 'RecordCategory'
--

INSERT INTO [RecordCategory] ([ID], [Title], [Ord]) VALUES (0, 'Жалоба', 1);
INSERT INTO [RecordCategory] ([ID], [Title], [Ord]) VALUES (1, 'История', 2);
INSERT INTO [RecordCategory] ([ID], [Title], [Ord]) VALUES (2, 'Осмотр', 3);
INSERT INTO [RecordCategory] ([ID], [Title], [Ord]) VALUES (3, 'Обследование', 4);
INSERT INTO [RecordCategory] ([ID], [Title], [Ord]) VALUES (4, 'Диагноз', 5);
INSERT INTO [RecordCategory] ([ID], [Title], [Ord]) VALUES (5, 'Лечение', 6);
-- 6 records

SET IDENTITY_INSERT [IcdChapter] ON
GO

--
-- Dumping data for table 'IcdChapter'
--

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

SET IDENTITY_INSERT [IcdChapter] OFF
GO

CREATE UNIQUE INDEX [Code] ON [IcdChapter] ([Code]);
GO


SET IDENTITY_INSERT [IcdBlock] ON
GO

--
-- Dumping data for table 'IcdBlock'
--

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

-- 259 records

SET IDENTITY_INSERT [IcdBlock] OFF
GO

CREATE UNIQUE INDEX [BlockCode] ON [IcdBlock] ([Code]);
GO


SET IDENTITY_INSERT [Disease] ON
GO

--
-- Dumping data for table 'Disease'
--

INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3659, 'I00', 'Ревматическая лихорадка без упоминания о  вовлечении сердца', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3660, 'I01', 'Ревматическая лихорадка с вовлечением сердца', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3661, 'I01.0', 'Острый ревматический перикардит', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3662, 'I01.1', 'Острый ревматический эндокардит', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3663, 'I01.2', 'Острый ревматический миокардит', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3664, 'I01.8', 'Другие острые ревматические болезни сердца', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3665, 'I01.9', 'Острая ревматическая болезнь сердца неуточненная', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3666, 'I02', 'Ревматическая хорея', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3667, 'I02.0', 'Ревматическая хорея с вовлечением сердца', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3668, 'I02.9', 'Ревматическая хорея без вовлечения сердца', 91);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3669, 'I05', 'Ревматические болезни митрального клапана', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3670, 'I05.0', 'Митральный стеноз', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3671, 'I05.1', 'Ревматическая недостаточность митрального клапана', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3672, 'I05.2', 'Митральный стеноз с недостаточностью', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3673, 'I05.8', 'Другие болезни митрального клапана', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3674, 'I05.9', 'Болезнь митрального клапана неуточненная', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3675, 'I06', 'Ревматические болезни аортального клапана', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3676, 'I06.0', 'Ревматический аортальный стеноз', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3677, 'I06.1', 'Ревматическая недостаточность аортального клапана', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3678, 'I06.2', 'Ревматический аортальный стеноз с недостаточностью', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3679, 'I06.8', 'Другие ревматические болезни аортального клапана', 92);
INSERT INTO [Disease] ([ID], [Code], [Title], [BlockID]) VALUES (3680, 'I06.9', 'Ревматическая болезнь аортального клапана неуточненная (ый)', 92);
-- 11675 records

SET IDENTITY_INSERT [Disease] OFF
GO

CREATE UNIQUE INDEX [DiseaseCode] ON [Disease] ([Code]);
GO
SET IDENTITY_INSERT [Speciality] ON
GO


SET IDENTITY_INSERT [Symptom] ON
GO

--
-- Dumping data for table 'Symptom'
--

INSERT INTO [Symptom] ([ID], [DiseaseID], [DefaultCategoryID], [IsDiagnosis]) VALUES (2, NULL, 1, 0);
INSERT INTO [Symptom] ([ID], [DiseaseID], [DefaultCategoryID], [IsDiagnosis]) VALUES (4, NULL, 3, 0);
INSERT INTO [Symptom] ([ID], [DiseaseID], [DefaultCategoryID], [IsDiagnosis]) VALUES (10, NULL, NULL, 0);
INSERT INTO [Symptom] ([ID], [DiseaseID], [DefaultCategoryID], [IsDiagnosis]) VALUES (14, NULL, NULL, 0);
INSERT INTO [Symptom] ([ID], [DiseaseID], [DefaultCategoryID], [IsDiagnosis]) VALUES (15, NULL, NULL, 0);
INSERT INTO [Symptom] ([ID], [DiseaseID], [DefaultCategoryID], [IsDiagnosis]) VALUES (18, NULL, NULL, 0);
INSERT INTO [Symptom] ([ID], [DiseaseID], [DefaultCategoryID], [IsDiagnosis]) VALUES (20, NULL, NULL, 0);
INSERT INTO [Symptom] ([ID], [DiseaseID], [DefaultCategoryID], [IsDiagnosis]) VALUES (21, NULL, NULL, 0);
INSERT INTO [Symptom] ([ID], [DiseaseID], [DefaultCategoryID], [IsDiagnosis]) VALUES (31, NULL, NULL, 0);
-- 9 records

SET IDENTITY_INSERT [Symptom] OFF
GO

SET IDENTITY_INSERT [Word] ON
GO

--
-- Dumping data for table 'Word'
--

INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (1, 'анемия', 0, 1, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (2, 'озноб', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (3, 'кашель', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (4, 'впервые', 1, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (5, 'повторно', 1, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (6, 'понос', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (7, 'порок сердца', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (8, 'пневмоторекс', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (9, 'одышка', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (10, 'удушье', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (12, 'кровохаркание', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (13, 'потери сознания', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (14, 'ожирение', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (15, 'гипертрофия', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (16, 'кардиомегалия', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (17, 'асцит', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (18, 'головокружение', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (19, 'инфильтрация', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (20, 'беременность', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (21, 'лихорадка', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (22, 'роды', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (23, 'запоры', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (24, 'лейкоцитоз', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (25, 'кровотечение', 0, 0, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (26, 'черный стул', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (27, 'гематурия', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (28, 'аритмия', 0, 0, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (29, 'цианоз', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (30, 'тромбоэмболия', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (31, 'желудочковые экстрасистолы', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (32, 'гемоглобин', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (33, 'легочная артерия', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (34, 'щитовидная железа', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (35, 'легкие', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (36, 'ЭКГ', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (37, 'коронарография', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (38, 'функция', 1, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (39, 'гипергликемия', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (40, 'белок', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (41, 'лейкоциты', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (42, 'стоя', 1, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (43, 'сидя', 1, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (44, 'лежа', 1, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (45, 'справа', 1, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (46, 'слева', 1, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (47, 'повышена', 1, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (48, 'понижена', 1, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (49, 'пароксизм', 1, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (50, 'физическая нагрузка', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (51, 'инфаркт миокарда', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (52, 'язвенная болезнь', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (53, 'гепатит', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (54, 'инсульт', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (55, 'сахарный диабет', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (56, 'фибрилляция предсердий', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (57, 'трикуспидальный', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (58, 'митральный', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (59, 'аортальный', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (60, 'транзиторная ишемическая атака', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (61, 'боли', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (62, 'в груди', 1, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (63, 'в голове', 1, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (64, 'в ноге', 1, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (65, 'в животе', 1, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (66, 'в моче', 1, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (67, 'отделение', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (68, 'пульмонологическое', 0, NULL, 67);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (69, 'глазное', 0, NULL, 67);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (70, 'хирургическое', 0, NULL, 67);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (71, 'травматологическое', 0, NULL, 67);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (72, 'терапевтическое', 0, NULL, 67);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (73, 'поликлиника', 0, NULL, 67);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (74, 'скорая мед. помощь', 0, NULL, 67);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (75, 'гепарин', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (76, 'лазикс', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (77, 'варфорин', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (78, 'антибиотики', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (79, 'обезболивание', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (80, 'нитроглицерин', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (81, 'госпитализация', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (82, 'операция', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (83, 'миокардит', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (84, 'Новокузнецк', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (85, 'Кемерово', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (86, 'Новосибирск', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (87, 'Томск', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (88, 'образование', 0, NULL, NULL);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (89, 'высшее', 0, NULL, 88);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (90, 'среднее', 0, NULL, 88);
INSERT INTO [Word] ([ID], [Title], [Priority], [DefaultCategoryID], [ParentID]) VALUES (93, 'перикардит', 0, NULL, NULL);
-- 90 records

SET IDENTITY_INSERT [Word] OFF
GO

SET IDENTITY_INSERT [SymptomWords] ON
GO

--
-- Dumping data for table 'SymptomWords'
--

--INSERT INTO [SymptomWords] ([ID], [WordID], [SymptomID]) VALUES (8, 1, 2);
--INSERT INTO [SymptomWords] ([ID], [WordID], [SymptomID]) VALUES (11, 2, 4);
--INSERT INTO [SymptomWords] ([ID], [WordID], [SymptomID]) VALUES (16, 3, 10);
--INSERT INTO [SymptomWords] ([ID], [WordID], [SymptomID]) VALUES (17, 1, 10);
--INSERT INTO [SymptomWords] ([ID], [WordID], [SymptomID]) VALUES (22, 1, 14);
--INSERT INTO [SymptomWords] ([ID], [WordID], [SymptomID]) VALUES (23, 4, 14);
--INSERT INTO [SymptomWords] ([ID], [WordID], [SymptomID]) VALUES (24, 47, 14);
--INSERT INTO [SymptomWords] ([ID], [WordID], [SymptomID]) VALUES (25, 93, 15);
--INSERT INTO [SymptomWords] ([ID], [WordID], [SymptomID]) VALUES (30, 5, 18);
--INSERT INTO [SymptomWords] ([ID], [WordID], [SymptomID]) VALUES (31, 24, 18);
--INSERT INTO [SymptomWords] ([ID], [WordID], [SymptomID]) VALUES (35, 78, 20);
--INSERT INTO [SymptomWords] ([ID], [WordID], [SymptomID]) VALUES (36, 1, 20);
--INSERT INTO [SymptomWords] ([ID], [WordID], [SymptomID]) VALUES (37, 93, 21);
--INSERT INTO [SymptomWords] ([ID], [WordID], [SymptomID]) VALUES (38, 1, 21);
--INSERT INTO [SymptomWords] ([ID], [WordID], [SymptomID]) VALUES (39, 5, 21);
--INSERT INTO [SymptomWords] ([ID], [WordID], [SymptomID]) VALUES (40, 7, 21);
--INSERT INTO [SymptomWords] ([ID], [WordID], [SymptomID]) VALUES (58, 8, 31);
-- 17 records

SET IDENTITY_INSERT [SymptomWords] OFF
GO
--
-- Dumping data for table 'Speciality'
--

INSERT INTO [Speciality] ([ID], [Title]) VALUES (1, 'Кардиолог');
-- 1 records

SET IDENTITY_INSERT [Speciality] OFF
GO

SET IDENTITY_INSERT [SpecialityIcdBlocks] ON
GO

--
-- Dumping data for table 'SpecialityIcdBlocks'
--

--INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [BlockID]) VALUES (1, 1, 92);
--INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [BlockID]) VALUES (2, 1, 93);
--INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [BlockID]) VALUES (3, 1, 94);
--INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [BlockID]) VALUES (4, 1, 95);
--INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [BlockID]) VALUES (5, 1, 96);
--INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [BlockID]) VALUES (6, 1, 97);
--INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [BlockID]) VALUES (7, 1, 98);
--INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [BlockID]) VALUES (8, 1, 99);
--INSERT INTO [SpecialityIcdBlocks] ([ID], [SpecialityID], [BlockID]) VALUES (9, 1, 100);
-- 9 records

SET IDENTITY_INSERT [SpecialityIcdBlocks] OFF
GO


SET IDENTITY_INSERT [Doctor] ON
GO

--
-- Dumping data for table 'Doctor'
--

INSERT INTO [Doctor] ([ID], [LastName], [MiddleName], [FirstName], [IsMale], [SpecialityID], [Settings]) VALUES (2, 'Смирнов', NULL, 'Василий', -1, 1, 1);
INSERT INTO [Doctor] ([ID], [LastName], [MiddleName], [FirstName], [IsMale], [SpecialityID], [Settings]) VALUES (3, 'Вахрушев', 'Константинович', 'Александр', -1, 1, 1);
-- 2 records

SET IDENTITY_INSERT [Doctor] OFF
GO

SET IDENTITY_INSERT [Patient] ON
GO

--
-- Dumping data for table 'Patient'
--

INSERT INTO [Patient] ([ID], [Label], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay]) VALUES (1, NULL, 'Петров', 'Иванович', 'Иван', -1, 1980, 2, 15);
INSERT INTO [Patient] ([ID], [Label], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay]) VALUES (5, 'абв', 'Андреев', NULL, 'Иван', -1, 1982, 11, NULL);
INSERT INTO [Patient] ([ID], [Label], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay]) VALUES (6, NULL, 'Иванов', 'Иванович', 'Иван', -1, 1995, NULL, 10);
INSERT INTO [Patient] ([ID], [Label], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay]) VALUES (8, NULL, 'Михайлов', NULL, 'Валентин', -1, 1973, NULL, NULL);
INSERT INTO [Patient] ([ID], [Label], [LastName], [MiddleName], [FirstName], [IsMale], [BirthYear], [BirthMonth], [BirthDay]) VALUES (10, '10', NULL, NULL, NULL, 0, NULL, NULL, NULL);
-- 5 records

SET IDENTITY_INSERT [Patient] OFF
GO


SET IDENTITY_INSERT [Course] ON
GO

--
-- Dumping data for table 'Course'
--

INSERT INTO [Course] ([ID], [PatientID], [StartDate], [EndDate], [DoctorID]) VALUES (1, 5, '2014-03-07 00:00:00', '2014-03-14 00:00:00', 2);
INSERT INTO [Course] ([ID], [PatientID], [StartDate], [EndDate], [DoctorID]) VALUES (7, 10, '2014-05-04 00:00:00', NULL, 2);
INSERT INTO [Course] ([ID], [PatientID], [StartDate], [EndDate], [DoctorID]) VALUES (8, 5, '2013-05-29 00:00:00', NULL, 2);
-- 3 records

SET IDENTITY_INSERT [Course] OFF
GO

SET IDENTITY_INSERT [Appointment] ON
GO

--
-- Dumping data for table 'Appointment'
--

INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID]) VALUES (1, '2014-03-10 07:39:48', 1, 2);
INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID]) VALUES (4, '2014-05-04 09:03:56', 7, 2);
INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID]) VALUES (8, '2014-05-29 02:45:51', 8, 2);
INSERT INTO [Appointment] ([ID], [DateAndTime], [CourseID], [DoctorID]) VALUES (20, '2014-06-03 05:39:52', 8, 2);
-- 4 records

SET IDENTITY_INSERT [Appointment] OFF
GO



SET IDENTITY_INSERT [HealthRecord] ON
GO

--
-- Dumping data for table 'HealthRecord'
--

INSERT INTO [HealthRecord] ([ID], [AppointmentID], [Comment], [SymptomId], [DiseaseID], [CategoryID], [NumValue], [FromYear], [FromMonth], [FromDay]) VALUES (5, 1, NULL, 2, NULL, 1, 4.003, NULL, NULL, NULL);
INSERT INTO [HealthRecord] ([ID], [AppointmentID], [Comment], [SymptomId], [DiseaseID], [CategoryID], [NumValue], [FromYear], [FromMonth], [FromDay]) VALUES (6, 1, NULL, 4, 3664, 5, 5, 2013, 11, NULL);
INSERT INTO [HealthRecord] ([ID], [AppointmentID], [Comment], [SymptomId], [DiseaseID], [CategoryID], [NumValue], [FromYear], [FromMonth], [FromDay]) VALUES (7, 1, NULL, 2, NULL, 3, .0001, 2013, 12, NULL);
INSERT INTO [HealthRecord] ([ID], [AppointmentID], [Comment], [SymptomId], [DiseaseID], [CategoryID], [NumValue], [FromYear], [FromMonth], [FromDay]) VALUES (8, 1, NULL, 10, 3667, 2, 1.012, NULL, NULL, NULL);
INSERT INTO [HealthRecord] ([ID], [AppointmentID], [Comment], [SymptomId], [DiseaseID], [CategoryID], [NumValue], [FromYear], [FromMonth], [FromDay]) VALUES (11, 4, 'qwe', 31, NULL, 0, NULL, 2014, NULL, NULL);
INSERT INTO [HealthRecord] ([ID], [AppointmentID], [Comment], [SymptomId], [DiseaseID], [CategoryID], [NumValue], [FromYear], [FromMonth], [FromDay]) VALUES (12, 1, NULL, 15, 3666, 3, NULL, NULL, NULL, NULL);
INSERT INTO [HealthRecord] ([ID], [AppointmentID], [Comment], [SymptomId], [DiseaseID], [CategoryID], [NumValue], [FromYear], [FromMonth], [FromDay]) VALUES (13, 1, NULL, 14, 3664, 1, NULL, NULL, NULL, NULL);
INSERT INTO [HealthRecord] ([ID], [AppointmentID], [Comment], [SymptomId], [DiseaseID], [CategoryID], [NumValue], [FromYear], [FromMonth], [FromDay]) VALUES (14, 1, 'комментарий', 18, NULL, 1, NULL, NULL, NULL, NULL);
INSERT INTO [HealthRecord] ([ID], [AppointmentID], [Comment], [SymptomId], [DiseaseID], [CategoryID], [NumValue], [FromYear], [FromMonth], [FromDay]) VALUES (19, 4, NULL, 2, NULL, 0, NULL, 2013, 12, NULL);
INSERT INTO [HealthRecord] ([ID], [AppointmentID], [Comment], [SymptomId], [DiseaseID], [CategoryID], [NumValue], [FromYear], [FromMonth], [FromDay]) VALUES (24, 8, '111', 21, NULL, 1, NULL, NULL, NULL, NULL);
INSERT INTO [HealthRecord] ([ID], [AppointmentID], [Comment], [SymptomId], [DiseaseID], [CategoryID], [NumValue], [FromYear], [FromMonth], [FromDay]) VALUES (27, 8, NULL, 20, NULL, 0, NULL, NULL, NULL, NULL);
-- 11 records

SET IDENTITY_INSERT [HealthRecord] OFF
GO


SET IDENTITY_INSERT [Property] ON
GO

--
-- Dumping data for table 'Property'
--

INSERT INTO [Property] ([ID], [Title]) VALUES (1, 'Образование');
INSERT INTO [Property] ([ID], [Title]) VALUES (2, 'Характер работы');
INSERT INTO [Property] ([ID], [Title]) VALUES (3, 'Место жительства');
INSERT INTO [Property] ([ID], [Title]) VALUES (4, 'Группа крови');
-- 4 records

SET IDENTITY_INSERT [Property] OFF
GO

SET IDENTITY_INSERT [PropertyValue] ON
GO

--
-- Dumping data for table 'PropertyValue'
--

INSERT INTO [PropertyValue] ([ID], [PropertyID], [Title]) VALUES (2, 1, 'Высшее');
INSERT INTO [PropertyValue] ([ID], [PropertyID], [Title]) VALUES (3, 1, 'Неоконченное высшее');
INSERT INTO [PropertyValue] ([ID], [PropertyID], [Title]) VALUES (4, 1, 'Среднее');
INSERT INTO [PropertyValue] ([ID], [PropertyID], [Title]) VALUES (5, 2, 'Сидячая');
INSERT INTO [PropertyValue] ([ID], [PropertyID], [Title]) VALUES (6, 2, 'Подвижная');
INSERT INTO [PropertyValue] ([ID], [PropertyID], [Title]) VALUES (7, 3, 'Город');
INSERT INTO [PropertyValue] ([ID], [PropertyID], [Title]) VALUES (8, 3, 'Деревня');
-- 7 records

SET IDENTITY_INSERT [PropertyValue] OFF
GO

CREATE INDEX [PropertyValueVal] ON [PropertyValue] ([Title]);
GO



SET IDENTITY_INSERT [PatientRecordProperties] ON
GO

--
-- Dumping data for table 'PatientRecordProperties'
--

INSERT INTO [PatientRecordProperties] ([ID], [PropertyID], [ValueID], [PatientID], [RecordID]) VALUES (1, 1, 4, 1, NULL);
INSERT INTO [PatientRecordProperties] ([ID], [PropertyID], [ValueID], [PatientID], [RecordID]) VALUES (2, 2, 6, 1, NULL);
INSERT INTO [PatientRecordProperties] ([ID], [PropertyID], [ValueID], [PatientID], [RecordID]) VALUES (5, 1, 3, NULL, NULL);
INSERT INTO [PatientRecordProperties] ([ID], [PropertyID], [ValueID], [PatientID], [RecordID]) VALUES (7, 1, 3, 5, NULL);
-- 4 records

SET IDENTITY_INSERT [PatientRecordProperties] OFF
GO

CREATE UNIQUE INDEX [PatientProperty] ON [PatientRecordProperties] ([PropertyID], [PatientID]);
GO