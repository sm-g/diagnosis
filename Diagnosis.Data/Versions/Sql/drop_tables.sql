-- drop client tables

-- FK
ALTER TABLE [Uom] DROP CONSTRAINT [FK_Uom_UomType]
GO

ALTER TABLE [IcdBlock] DROP CONSTRAINT [FK_IcdBlock_IcdChapter]
GO

ALTER TABLE [SpecialityIcdBlocks] DROP CONSTRAINT [FK_SpecialityIcdBlocks_IcdBlo]
GO

ALTER TABLE [SpecialityIcdBlocks] DROP CONSTRAINT [FK_SpecialityIcdBlocks_Specia]
GO

ALTER TABLE [IcdDisease] DROP CONSTRAINT [FK_IcdDisease_IcdBlock]
GO

ALTER TABLE [Word] DROP CONSTRAINT [FK_Word_HrCategory]
GO

ALTER TABLE [Word] DROP CONSTRAINT [FK_Word_Word]
GO

ALTER TABLE [Doctor] DROP CONSTRAINT [FK_Doctor_Speciality]
GO

ALTER TABLE [Course] DROP CONSTRAINT [FK_Course_Doctor]
GO

ALTER TABLE [Course] DROP CONSTRAINT [FK_Course_Patient]
GO

ALTER TABLE [Appointment] DROP CONSTRAINT [FK_Appoinmtment_Course]
GO

ALTER TABLE [Appointment] DROP CONSTRAINT [FK_Appoinmtment_Doctor]
GO

ALTER TABLE [HealthRecord] DROP CONSTRAINT [FK_Hr_Appointment]
GO

ALTER TABLE [HealthRecord] DROP CONSTRAINT [FK_Hr_Course]
GO

ALTER TABLE [HealthRecord] DROP CONSTRAINT [FK_Hr_Patient]
GO

ALTER TABLE [HealthRecord] DROP CONSTRAINT [FK_Hr_HrCategory]
GO

ALTER TABLE [HrItem] DROP CONSTRAINT [FK_HrItem_IcdDisease]
GO

ALTER TABLE [HrItem] DROP CONSTRAINT [FK_HrItem_Hr]
GO

ALTER TABLE [HrItem] DROP CONSTRAINT [FK_HrItem_Uom]
GO

ALTER TABLE [HrItem] DROP CONSTRAINT [FK_HrItem_Word]
GO

ALTER TABLE [Setting] ADD CONSTRAINT [FK_Setting_Doctor]
GO


DROP TABLE [Uom]
GO

DROP TABLE [UomType]
GO

DROP TABLE [Speciality]
GO

DROP TABLE [Patient]
GO

DROP TABLE [IcdChapter]
GO

DROP TABLE [IcdBlock]
GO

DROP TABLE [SpecialityIcdBlocks]
GO

DROP TABLE [IcdDisease]
GO

DROP TABLE [HrCategory]
GO

DROP TABLE [Word]
GO

DROP TABLE [Doctor]
GO

DROP TABLE [Course]
GO

DROP TABLE [Appointment]
GO

DROP TABLE [HealthRecord]
GO

DROP TABLE [HrItem]
GO

DROP TABLE [Setting]
GO

