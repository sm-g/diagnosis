-- новые записи курса помещаем в осмотр с пустой датой в этом курсе
-- измененные — меняем в этом осмотре
-- удаленные — из этого осмотра

--! если удалены все записи курса, удалить спецосмотр

declare @appId uniqueidentifier
    , @cId uniqueidentifier = ?
    , @HrId uniqueidentifier = ?
    , @CreatedAt datetime = ?
    , @DoctorID uniqueidentifier = ?
    , @HrCategoryID int = ?
    , @FromYear smallint = ?
    , @FromMonth tinyint = ?
    , @FromDay tinyint = ?
    , @Unit nvarchar(20) = ?
    , @Hash char(66) = ?
    
select @appId = a.Id
from olap.Appointment as a
where a.CourseID = @cId and a.DateAndTime is null

if @appId is null
	begin
    SET @appId = NEWID()
	INSERT INTO [olap].[Appointment]
           ([Id]
           ,[CourseID]
           ,[DoctorID]
           ,[DateAndTime]
           ,[Hash])
    
     VALUES
           (@appId
           ,@cId
           ,NULL
           ,NULL
           ,'0x0000000000000000000000000000000000000000000000000000000000000000')
    select @appId
	end
    
INSERT INTO [olap].[HealthRecord]
           ([Id]
           ,[CreatedAt]
           ,[AppointmentID]
           ,[DoctorID]
           ,[HrCategoryID]
           ,[FromYear]
           ,[FromMonth]
           ,[FromDay]
           ,[Unit]
           ,[Hash])
     VALUES
           (@HrId
           ,@CreatedAt
           ,@appId
           ,@DoctorID
           ,@HrCategoryID
           ,@FromYear
           ,@FromMonth
           ,@FromDay
           ,@Unit
           ,@Hash)

-- записи пациента помещаем в осмотр с пустой датой в курсе в пустой датой

declare @appId uniqueidentifier
    , @cId uniqueidentifier
    , @pId uniqueidentifier = ?
    , @HrId uniqueidentifier = ?
    , @CreatedAt datetime = ?
    , @DoctorID uniqueidentifier = ?
    , @HrCategoryID int = ?
    , @FromYear smallint = ?
    , @FromMonth tinyint = ?
    , @FromDay tinyint = ?
    , @Unit nvarchar(20) = ?
    , @Hash char(66) = ?
    
select @cId = c.Id
from olap.Course as c
where c.PatientID = @pId and c.StartDate is null

if @cId is null
	begin
    SET @cId = NEWID()
	INSERT INTO [olap].[Course]
           ([Id]
           ,[PatientID]
           ,[DoctorID]
           ,[StartDate]
           ,[EndDate]
           ,[Hash])
    
     VALUES
           (@cId
           ,@pId
           ,NULL
           ,NULL
           ,NULL
           ,'0x0000000000000000000000000000000000000000000000000000000000000000')
    select @cId
	end
    
select @appId = a.Id
from olap.Appointment as a
where a.CourseID = @cId and a.DateAndTime is null

if @appId is null
	begin
    SET @appId = NEWID()
	INSERT INTO [olap].[Appointment]
           ([Id]
           ,[CourseID]
           ,[DoctorID]
           ,[DateAndTime]
           ,[Hash])
    
     VALUES
           (@appId
           ,@cId
           ,NULL
           ,NULL
           ,'0x0000000000000000000000000000000000000000000000000000000000000000')
    select @appId
	end
    
INSERT INTO [olap].[HealthRecord]
           ([Id]
           ,[CreatedAt]
           ,[AppointmentID]
           ,[DoctorID]
           ,[HrCategoryID]
           ,[FromYear]
           ,[FromMonth]
           ,[FromDay]
           ,[Unit]
           ,[Hash])
     VALUES
           (@HrId
           ,@CreatedAt
           ,@appId
           ,@DoctorID
           ,@HrCategoryID
           ,@FromYear
           ,@FromMonth
           ,@FromDay
           ,@Unit
           ,@Hash)