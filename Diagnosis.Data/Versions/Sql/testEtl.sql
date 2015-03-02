select t0.Id, t0.Val, t0.AppID, t0.CourseID, t.Id as Id1 
from Tbl0 as t0 full join Tbl as t
on t0.Id = t.Id


declare @appId int, @cId int = ?
select @appId = a.Id
from App as a
where a.CourseID = @cId and a.Date is null

if @appId is null
	begin
	insert into App (CourseID) values (@cId)
    select @appId = IDENT_CURRENT('dbo.App')
	end
    
insert into Tbl
(Val, AppID)
Values
(?, @appId)

-----
MERGE INTO Tbl AS Target
USING (SELECT * FROM Tbl0) AS Source ON Target.Id = Source.Id
when matched THEN
	update set Val = Source.Val
WHEN NOT MATCHED BY TARGET THEN	
    INSERT (Val, AppId) VALUES (Source.Val, 
        (select -- не работает, нужен триггер
        IF Source.CourseID is null
            select Source.AppID
        ELSE
        begin
            declare @appId uniqueidentifier

            select @appId = a.Id
            from Appointment as a
            where a.CourseID = CourseID and a.DateAndTime is null

            if @appId is null
                begin
                select @appId = newid()
                insert into App(Id, CourseID) values (@appId, CourseID)
                end
            select @appId
        end
        )
    )
WHEN NOT MATCHED BY SOURCE THEN
	DELETE
OUTPUT $action, Inserted.*, Deleted.*;