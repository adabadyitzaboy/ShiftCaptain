if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'ShiftPreference')
    drop table ShiftPreference;

if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Preference')
    drop table Preference;

if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Shift')
    drop table [Shift];
	
if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'UserInstance')
    drop table UserInstance;
	
if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'User')
    drop table [User];
		
if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'RoomHours')
    drop table RoomHours;

if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'RoomInstance')
    drop table RoomInstance;

if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Room')
    drop table Room;

if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Building')
    drop table Building;

if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Address')
    drop table [Address];

if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'Version')
    drop table Version;

if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'UserView')
    drop view UserView;
	
if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'BuildingView')
    drop view BuildingView;
	
if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'RoomView')
    drop view RoomView;

if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'ShiftView')
    drop view ShiftView;
	
if exists (select * from INFORMATION_SCHEMA.TABLES where TABLE_NAME = 'EmailTemplate')
    drop table EmailTemplate;
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_NAME = 'NoShiftCoverage')
    drop function [NoShiftCoverage];

if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_NAME = 'CantWorkViolation')
    drop function [CantWorkViolation];

if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_NAME = 'ConflictingShifts')
    drop function [ConflictingShifts];

if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_NAME = 'UTILfn_Split')
    drop function [UTILfn_Split];

if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_NAME = 'sp_clone_schedule')
    drop procedure [sp_clone_schedule];

	
if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_NAME = 'ValidateShiftPreference')
    drop function ValidateShiftPreference;

if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_NAME = 'ValidateShift')
    drop function ValidateShift;

if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_NAME = 'fn_GetDate')
    drop function fn_GetDate;

if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_NAME = 'fn_AdjustHour')
    drop function fn_AdjustHour

if exists (select * from INFORMATION_SCHEMA.ROUTINES where ROUTINE_NAME = 'fn_GetTOD')
    drop function fn_GetTOD

Create Table [Version]
(
	Id int Primary Key not null identity,
	Name nvarchar(500) not null,
	IsActive bit not null,
	IsVisible bit not null,
	IsReadyForApproval bit not null,
	IsApproved bit not null
)

Create Table [Address]
(
	Id int Primary Key not null identity,
	Line1 nvarchar(200) not null,
	Line2 nvarchar(200),
	City nvarchar(200) not null,
	[State] nvarchar(200) not null,
	ZipCode nvarchar(9) not null,
	Country nvarchar(200) not null	
)
Create Table Building
(
	Id int Primary Key not null identity,
	Name nvarchar(500) not null,
	AddressId int Foreign Key REFERENCES [Address](Id),
	PhoneNumber nvarchar(20),
	ManagerPhone nvarchar(20)
)

Create Table Room
(
	Id int Primary Key not null identity,
	Name nvarchar(500) not null,
	RoomNumber nvarchar(50) not null,
	BuildingId int not null Foreign Key REFERENCES Building(Id),
	PhoneNumber nvarchar(20)
)

Create Table RoomInstance
(
	Id int Primary Key not null identity,
	RoomId int not null Foreign Key REFERENCES Room(Id),
	VersionId int not null Foreign Key REFERENCES [Version](Id)
)

Create Table RoomHours
(
	Id int Primary Key not null identity,
	RoomInstanceId int not null Foreign Key REFERENCES RoomInstance(Id),
	[Day] int not null,
	StartTime time(0) not null,
	Duration decimal(9,1) not null
)

Create Table [User]
(
	Id int Primary Key not null identity,
	EmailAddress nvarchar(500) not null,
	Pass nvarchar(100),
	FName nvarchar(100) not null,
	MName nvarchar(100),
	LName nvarchar(100) not null,	
	NickName nvarchar(100) not null,	
	AddressId int Foreign Key REFERENCES [Address](Id),
	EmployeeId nvarchar(100) not null,
	PhoneNumber nvarchar(20),
	IsShiftManager bit not null,
	IsManager bit not null,
	Locked bit not null,
	NumTries int not null,
	LastLogin DateTime,
	IsActive bit not null,
	IsMale bit not null 
)

Create Table UserInstance
(
	Id int Primary Key not null identity,
	UserId int not null Foreign Key REFERENCES [User](Id),
	VersionId int not null Foreign Key REFERENCES [Version](Id),
	MinHours decimal(9,1) not null,
	MaxHours decimal(9,1) not null
)

Create Table [Shift]
(
	Id int Primary Key not null identity,
	UserId int not null Foreign Key REFERENCES [User](Id),
	RoomId int not null Foreign Key REFERENCES Room(Id),
	VersionId int not null Foreign Key REFERENCES [Version](Id),
	[Day] int not null,
	StartTime time(0) not null,
	Duration decimal(9,1) not null
)

Create Table Preference
(
	Id int Primary Key not null identity,
	Name nvarchar(50) not null,
	[Description] nvarchar(200) not null,
	Color nvarchar(100) not null,
	CanWork bit not null
)

Create Table ShiftPreference
(
	Id int Primary Key not null identity,
	UserId int not null Foreign Key REFERENCES [User](Id),
	VersionId int not null Foreign Key REFERENCES [Version](Id),
	PreferenceId int not null Foreign Key REFERENCES Preference(Id),
	[Day] int not null,
	StartTime time(0) not null,
	Duration decimal(9,1) not null
)
Go
Create View UserView
As
Select 
U.Id As UserId,
U.EmailAddress,
U.Pass,
U.FName,
U.MName,
U.LName,
U.NickName,
U.EmployeeId,
U.PhoneNumber,
U.IsShiftManager,
U.IsManager,
U.Locked,
U.LastLogin,
U.IsActive,
U.IsMale,
UI.MinHours,
UI.MaxHours,
(Select sum(Duration) from [Shift] S Where S.UserId = U.Id AND S.VersionId = UI.VersionId) As CurrentHours,
UI.VersionId,
A.Id As AddressId,
A.Line1,
A.Line2,
A.City,
A.[State],
A.ZipCode,
A.Country
from [User] U
LEFT JOIN UserInstance UI on UI.UserId = U.Id

LEFT JOIN [Address] A
on U.AddressId = A.Id
Go
Create View BuildingView
As
Select 
	B.Id As BuildingId,
	B.Name,
	B.PhoneNumber,
	B.ManagerPhone,
	A.Id As AddressId,
	A.Line1,
	A.Line2,
	A.City,
	A.[State],
	A.ZipCode,
	A.Country
from [Building] B
LEFT JOIN [Address] A
on B.AddressId = A.Id
Go
Create View RoomView
As
select 
 B.Id as BuildingId,
 B.Name as BuildingName,
 R.Id as RoomId,
 R.Name,
 R.RoomNumber,
 R.PhoneNumber,
 RI.Id as RoomInstanceId,
 RI.VersionId,
 Sun.[Day] as SundayId,
 Sun.Id as SundayInstanceId,
 Sun.StartTime as SundayStartTime,
 Sun.Duration as SundayDuration,
 Mon.[Day] as MondayId,
 Mon.Id as MondayInstanceId,
 Mon.StartTime as MondayStartTime,
 Mon.Duration as MondayDuration,
 Tues.[Day] as TuesdayId,
 Tues.Id as TuesdayInstanceId,
 Tues.StartTime as TuesdayStartTime,
 Tues.Duration as TuesdayDuration,
 Wed.[Day] as WednesdayId,
 Wed.Id as WednesdayInstanceId,
 Wed.StartTime as WednesdayStartTime,
 Wed.Duration as WednesdayDuration,
 Thurs.[Day] as ThursdayId,
 Thurs.Id as ThursdayInstanceId,
 Thurs.StartTime as ThursdayStartTime,
 Thurs.Duration as ThursdayDuration,
 Fri.[Day] as FridayId,
 Fri.Id as FridayInstanceId,
 Fri.StartTime as FridayStartTime,
 Fri.Duration as FridayDuration,
 Sat.[Day] as SaturdayId,
 Sat.Id as SaturdayInstanceId,
 Sat.StartTime as SaturdayStartTime,
 Sat.Duration as SaturdayDuration
from [room] R
JOIN Building B on B.Id = R.BuildingId
LEFT JOIN RoomInstance RI on RI.RoomId = R.Id
LEFT JOIN RoomHours Sun on RI.Id = Sun.RoomInstanceId and Sun.[Day] = 0
LEFT JOIN RoomHours Mon on RI.Id = Mon.RoomInstanceId and Mon.[Day] = 1
LEFT JOIN RoomHours Tues on RI.Id = Tues.RoomInstanceId and Tues.[Day] = 2
LEFT JOIN RoomHours Wed on RI.Id = Wed.RoomInstanceId and Wed.[Day] = 3
LEFT JOIN RoomHours Thurs on RI.Id = Thurs.RoomInstanceId and Thurs.[Day] = 4
LEFT JOIN RoomHours Fri on RI.Id = Fri.RoomInstanceId and Fri.[Day] = 5
LEFT JOIN RoomHours Sat on RI.Id = Sat.RoomInstanceId and Sat.[Day] = 6
Go
--Create View ShiftView
--As
--select 
--S.VersionId as VersionId,
--S.Id as ShiftId,
--S.UserId,
--U.NickName,
--S.[Day],
--S.StartTime,
--S.Duration
--from [Shift] S
--JOIN [User] U on S.UserId = U.Id
--Go
Create Table EmailTemplate
(
	Name nvarchar(50) Primary Key not null,
	IsActive bit not null,
	[Subject] nvarchar(100) not null,
	[Body] nvarchar(4000) not null
)
GO
CREATE function [dbo].[NoShiftCoverage](
	@versionId int 
)
returns @Rooms table ([RoomId] int, Name nvarchar(500), [Day] int, [Time] decimal(9,1))
BEGIN
	Declare @hours As table
	(
		id int identity Primary Key,
		[time] decimal(9) not null
	)
	Declare @counter int
	Set @counter = 0
	While(@counter < 1440 /*(24 * 60)*/)
	Begin
		insert into @hours
		Values (@counter)

		set @counter += 30;
	END

	insert into @Rooms
	select RI.Roomid, R.Name, RH.[Day], 
		case when H.[time] < cast(DATEDIFF(minute, '00:00:00',RH.StartTime) as int) THEN H.[time] + 1440 /*(24 * 60)*/ ELSE H.[time] END / 60
	from Room R
	JOIN [RoomInstance] RI on RI.RoomId = R.Id
	JOIN RoomHours RH on RH.RoomInstanceId = RI.Id
	JOIN @Hours H on 
	(
		H.[time] >= cast(DATEDIFF(minute, '00:00:00',RH.StartTime) as decimal(9))
		and H.[time] < cast(DATEDIFF(minute, '00:00:00',RH.StartTime) as decimal(9)) + Duration
	) OR
	(
		cast(DATEDIFF(minute, '00:00:00',RH.StartTime) as decimal(9)) + Duration >= 1440 /*(24 * 60)*/
		and H.[time] < (cast(DATEDIFF(minute, '00:00:00',RH.StartTime) as decimal(9)) + Duration) % 1440 /*(24 * 60)*/
	)
	where RI.VersionId = @versionId and
	not exists(Select 1 from [Shift] s where s.VersionId = RI.VersionId AND s.RoomId = RI.RoomId AND s.[Day] = RH.[Day] AND
		( 
			cast(DATEDIFF(minute, '00:00:00',s.StartTime) as decimal(9)) = H.[time]  OR
			(cast(DATEDIFF(minute, '00:00:00',s.StartTime) as decimal(9)) < H.[time] AND cast(DATEDIFF(minute, '00:00:00',s.StartTime) as decimal(9)) + (s.Duration * 60)> H.[time] ) OR 
			(/*overflow*/cast(DATEDIFF(minute, '00:00:00',RH.StartTime) as decimal(9)) > H.[time] AND cast(DATEDIFF(minute, '00:00:00',s.StartTime) as decimal(9)) <= H.[time] + 1440 /*(24 * 60)*/ AND cast(DATEDIFF(minute, '00:00:00',s.StartTime) as decimal(9)) + (s.Duration * 60)> H.[time] + 1440 /*(24 * 60)*/)
		)
	)
	return
END
GO
CREATE function [dbo].[CantWorkViolation](
	@VersionId int 
)
returns @Preferences table ([PreferenceId] int, [ShiftId] int, NickName nvarchar(100), PreferenceDay int, PreferenceTime time(7), PreferenceDuration decimal(9,1), ShiftRoom nvarchar(500), ShiftDay int, ShiftTime  time(7), ShiftDuration decimal(9,1))
BEGIN

	Declare @RoomHours table
	(
		[Day] int Primary Key,
		[start] int
	)

	insert into @RoomHours
	select [Day], Min(DATEDIFF(minute, '00:00:00',StartTime))
	From RoomHours RH
	JOIN RoomInstance RI on RI.Id = RH.RoomInstanceId
	Where RI.VersionId = @VersionId
	Group By [Day]


	insert into @Preferences
	select P.Id, S.Id, U.NickName, P.[Day], P.StartTime, P.Duration, R.Name, S.[Day], S.StartTime, S.Duration
	from [ShiftPreference] P 
	JOIN [Preference] Pref on Pref.Id = P.PreferenceId
	JOIN @RoomHours RH on P.[Day] = RH.[Day]
	JOIN [Shift] S 
	on
		S.UserId = P.UserId AND
		S.VersionId = P.VersionId AND
		S.[Day] = P.[Day] 
	JOIN RoomInstance RI
	on RI.Id = S.RoomId AND RI.VersionId = P.VersionId
	JOIN RoomHours RHS
	on RHS.[Day] = S.[Day] AND RHS.RoomInstanceId = RI.Id
	JOIN Room R
	on R.Id = RI.RoomId
	JOIN [User] U
	on S.UserId = U.Id
	WHERE P.VersionId = @VersionId AND 
	Pref.CanWork = 0 AND
		( 
			cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) = cast(DATEDIFF(minute, '00:00:00',P.StartTime) as decimal(9))  OR
			(cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) < cast(DATEDIFF(minute, '00:00:00',P.StartTime) as decimal(9)) AND cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) + (s.Duration * 60)> cast(DATEDIFF(minute, '00:00:00',P.StartTime) as decimal(9)) ) OR --shift starts before preference but ends after
			(/*overflow*/cast(DATEDIFF(minute, '00:00:00',P.StartTime) as decimal(9)) < RH.[start] AND cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) <= cast(DATEDIFF(minute, '00:00:00',P.StartTime) as decimal(9)) + 1440 /*(24 * 60)*/ AND cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) + (s.Duration * 60) > cast(DATEDIFF(minute, '00:00:00',P.StartTime) as decimal(9)) + 1440 /*(24 * 60)*/) OR--overflow and shift starts before preference but ends after
			(cast(DATEDIFF(minute, '00:00:00',P.StartTime) as decimal(9)) < cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) AND cast(DATEDIFF(minute, '00:00:00',P.StartTime) as decimal(9)) + (P.Duration * 60)> cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) ) OR --preference starts before shift but ends after
			(/*overflow*/cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) < cast(DATEDIFF(minute, '00:00:00',RHS.StartTime) as decimal(9)) AND cast(DATEDIFF(minute, '00:00:00',P.StartTime) as decimal(9)) <= cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) + 1440 /*(24 * 60)*/ AND cast(DATEDIFF(minute, '00:00:00',P.StartTime) as decimal(9)) + (P.Duration * 60) > cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) + 1440 /*(24 * 60)*/)--overflow and preference starts before shift but ends after
		)
	return
END
GO
CREATE function [dbo].[ConflictingShifts](
	@VersionId int 
)
returns @ConflictingShifts table (UserId int, NickName nvarchar(100), [Shift1Id] int, Shift1Room nvarchar(500), Shift1Day int, Shift1Time  time(7), Shift1Duration decimal(9,1), [Shift2Id] int, Shift2Room nvarchar(500), Shift2Day int, Shift2Time  time(7), Shift2Duration decimal(9,1))
BEGIN
	Declare @RoomHours table
	(
		[Day] int Primary Key,
		[start] int
	)

	insert into @RoomHours
	select [Day], Min(DATEDIFF(minute, '00:00:00',StartTime))
	From RoomHours RH
	JOIN RoomInstance RI on RI.Id = RH.RoomInstanceId
	Where RI.VersionId = @VersionId
	Group By [Day]


	insert into @ConflictingShifts
	select U.id, U.NickName, S1.Id, R1.Name, S1.[Day], S1.StartTime, S1.Duration, S2.Id, R2.Name, S2.[Day], S2.StartTime, S2.Duration
	from [Shift] S1
	JOIN [Shift] S2 
	on
		S1.UserId = S2.UserId AND
		S1.VersionId = S2.VersionId AND
		S1.[Day] = S2.[Day] AND
		S1.Id < S2.Id--So that this only gets reported once.
	JOIN RoomInstance RI1
	on RI1.Id = S1.RoomId AND RI1.VersionId = S1.VersionId
	JOIN RoomHours RHS1
	on RHS1.[Day] = S1.[Day] AND RHS1.RoomInstanceId = RI1.Id
	JOIN Room R1
	on R1.Id = RI1.RoomId
	JOIN RoomInstance RI2
	on RI2.Id = S2.RoomId AND RI2.VersionId = S2.VersionId
	JOIN RoomHours RHS2
	on RHS2.[Day] = S2.[Day] AND RHS2.RoomInstanceId = RI2.Id
	JOIN Room R2
	on R2.Id = RI2.RoomId
	JOIN [User] U
	on S1.UserId = U.Id
	WHERE S1.VersionId = @VersionId AND 
		( 
			cast(DATEDIFF(minute, '00:00:00',S1.StartTime) as decimal(9)) = cast(DATEDIFF(minute, '00:00:00',S2.StartTime) as decimal(9))  OR
			(cast(DATEDIFF(minute, '00:00:00',S1.StartTime) as decimal(9)) < cast(DATEDIFF(minute, '00:00:00',S2.StartTime) as decimal(9)) AND cast(DATEDIFF(minute, '00:00:00',S1.StartTime) as decimal(9)) + (S1.Duration * 60)> cast(DATEDIFF(minute, '00:00:00',S2.StartTime) as decimal(9)) ) OR --shift1 starts before shift2 but ends after
			(/*overflow*/cast(DATEDIFF(minute, '00:00:00',S2.StartTime) as decimal(9)) < cast(DATEDIFF(minute, '00:00:00',RHS2.StartTime) as decimal(9)) AND cast(DATEDIFF(minute, '00:00:00',S1.StartTime) as decimal(9)) <= cast(DATEDIFF(minute, '00:00:00',S2.StartTime) as decimal(9)) + 1440 /*(24 * 60)*/ AND cast(DATEDIFF(minute, '00:00:00',S1.StartTime) as decimal(9)) + (S1.Duration * 60) > cast(DATEDIFF(minute, '00:00:00',S2.StartTime) as decimal(9)) + 1440 /*(24 * 60)*/) OR--overflow and shift1 starts before shift2 but ends after
			(cast(DATEDIFF(minute, '00:00:00',S2.StartTime) as decimal(9)) < cast(DATEDIFF(minute, '00:00:00',S1.StartTime) as decimal(9)) AND cast(DATEDIFF(minute, '00:00:00',S2.StartTime) as decimal(9)) + (S2.Duration * 60)> cast(DATEDIFF(minute, '00:00:00',S1.StartTime) as decimal(9)) ) OR --shift2 starts before shift1 but ends after
			(/*overflow*/cast(DATEDIFF(minute, '00:00:00',S1.StartTime) as decimal(9)) < cast(DATEDIFF(minute, '00:00:00',RHS1.StartTime) as decimal(9)) AND cast(DATEDIFF(minute, '00:00:00',S2.StartTime) as decimal(9)) <= cast(DATEDIFF(minute, '00:00:00',S1.StartTime) as decimal(9)) + 1440 /*(24 * 60)*/ AND cast(DATEDIFF(minute, '00:00:00',S2.StartTime) as decimal(9)) + (S2.Duration * 60) > cast(DATEDIFF(minute, '00:00:00',S1.StartTime) as decimal(9)) + 1440 /*(24 * 60)*/)--overflow and shift2 starts before shift1 but ends after
		)

	return
END
GO
CREATE function [dbo].[UTILfn_Split](
 @String nvarchar (max),
 @Delimiter nvarchar (10)
 )
returns @ValueTable table ([row] int identity Primary Key, [Value] nvarchar(max))
begin
 declare @NextString nvarchar(max)
 declare @Pos int
 declare @NextPos int
 declare @CommaCheck nvarchar(1)
 
 --Initialize
 set @NextString = ''
 set @CommaCheck = right(@String,1) 
 
 --Check for trailing Comma, if not exists, INSERT
 --if (@CommaCheck <> @Delimiter )
 set @String = @String + @Delimiter
 
 --Get position of first Comma
 set @Pos = charindex(@Delimiter,@String)
 set @NextPos = 1
 
 --Loop while there is still a comma in the String of levels
 while (@pos <>  0)  
 begin
  set @NextString = substring(@String,1,@Pos - 1)
 
  insert into @ValueTable ( [Value]) Values (@NextString)
 
  set @String = substring(@String,@pos +1,len(@String))
  
  set @NextPos = @Pos
  set @pos  = charindex(@Delimiter,@String)
 end
 
 return 
end

GO

CREATE PROCEDURE sp_clone_schedule
(
	@VersionId int,
	@VersionName nvarchar(500),
	@Users nvarchar(max),
	@Rooms nvarchar(max)
)
AS
BEGIN
	Set NOCOUNT ON;

	Declare @UserIds table ([row] int not null Primary Key, id int)
	insert into @UserIds 
	select [row], [value] from dbo.[UTILfn_Split](@Users, ',')
	

	Declare @RoomIds table ([row] int not null Primary Key, id int)
	insert into @RoomIds 
	select [row], [value] from dbo.[UTILfn_Split](@Rooms, ',')

	insert into [Version]
	select @VersionName, 0, 0, 0, 0
	
	Declare @NewVersionId int
	set @NewVersionId = @@IDENTITY

	insert into UserInstance
	select UserId, @NewVersionId, MinHours, MaxHours
	From UserInstance UI
	JOIN @UserIds ids
	on UI.UserId = ids.id
	Where UI.VersionId = @VersionId
	
	insert into RoomInstance
	select RoomId, @NewVersionId
	From RoomInstance RI
	JOIN @RoomIds ids
	on RI.RoomId = ids.id
	Where RI.VersionId = @VersionId
	
	insert into RoomHours
	Select RI.Id, [Day], StartTime, Duration
	From RoomHours RH
	JOIN RoomInstance ORI
	on RH.RoomInstanceId = ORI.Id and ORI.VersionId = @VersionId
	JOIN RoomInstance RI
	on RI.VersionId = @NewVersionId AND RI.RoomId = ORI.RoomId

	insert into [Shift]
	select S.UserId, S.RoomId, UI.VersionId, [Day], StartTime, Duration
	From [Shift] S
	JOIN UserInstance UI
	on UI.UserId = S.UserId AND UI.VersionId = @NewVersionId
	JOIN RoomInstance RI
	on RI.RoomId = S.RoomId AND RI.VersionId = @NewVersionId
	Where S.VersionId = @VersionId

	insert into [ShiftPreference]
	select SP.UserId, UI.VersionId, PreferenceId, [Day], StartTime, Duration
	From [ShiftPreference] SP
	JOIN UserInstance UI
	on UI.UserId = SP.UserId AND UI.VersionId = @NewVersionId
	Where SP.VersionId = @VersionId

	Select @NewVersionId
END
GO

CREATE function [dbo].[fn_GetDate](
	@Day int
)
returns nvarchar(10)
BEGIN
	Declare @DayOfWeek nvarchar(10)
	SELECT @DayOfWeek = CASE @Day
		when 0 then 'Sunday' 
		when 1 then 'Monday' 
		when 2 then 'Tuesday' 
		when 3 then 'Wednesday' 
		when 4 then 'Thursday' 
		when 5 then 'Friday' 
		when 6 then 'Saturday' 
	END

	return @DayOfWeek
END
Go

CREATE function [dbo].[fn_AdjustHour](
	@StartTime time(7),
	@DayStart int
)
returns nvarchar(100)
BEGIN
	Declare @Hour nvarchar(100)
	Declare @_StartTime decimal(9)
	select @_StartTime = cast(DATEDIFF(minute, '00:00:00',@StartTime) as decimal(9)) / 60
	if(@_StartTime > @DayStart / 60)
	BEGIN
		select @Hour = STR(@_StartTime, 10,1)
	END
	ELSE
	BEGIN
		select @Hour = STR(@_StartTime + 24, 10,1)
	END

	return @Hour
END
Go
CREATE function [dbo].[fn_GetTOD](
	@StartTime time(7)
)
returns nvarchar(100)
BEGIN
	Declare @TOD nvarchar(100)
	if(DATEDIFF(hour, '12:00:00',@StartTime) >= 0 AND DATEDIFF(hour, '23:59:59',@StartTime)<=0)
	BEGIN
		select @TOD = convert(nvarchar(5),@StartTime, 108) + ' PM'
	END
	ELSE
	BEGIN
		select @TOD = convert(nvarchar(5),@StartTime, 108) + ' AM'
	END
	return @TOD
END
Go
CREATE function [dbo].[ValidateShiftPreference](
	@Id int,
	@UserId int,
	@VersionId int,
	@PreferenceId int,
	@Day int,
	@StartTime time(7),
	@Duration decimal(9,1)
)
returns @Conflicts table ([message] nvarchar(max))
BEGIN

	Declare @RoomHours table
	(
		[Day] int Primary Key,
		[start] int
	)

	insert into @RoomHours
	select [Day], Min(DATEDIFF(minute, '00:00:00',StartTime))
	From RoomHours RH
	JOIN RoomInstance RI on RI.Id = RH.RoomInstanceId
	Where RI.VersionId = @VersionId
	AND RH.[Day] = @Day
	Group By [Day]

	if(@@ROWCOUNT = 0)
	BEGIN
		insert into @Conflicts
		select 'Invalid Day - ' + case when @Day >= 0 AND @Day < 7 then dbo.fn_GetDate(@Day) else cast(@Day as nvarchar) end
	END

	Declare @StartTimeMinutes Decimal(9)
	Declare @EndTimeMinutes Decimal(9)
	select @StartTimeMinutes = cast(DATEDIFF(minute, '00:00:00',@StartTime) as decimal(9)),
	 @EndTimeMinutes  = cast(DATEDIFF(minute, '00:00:00',@StartTime) as decimal(9)) + @Duration * 60

	if not exists(Select 1 from Preference P where P.Id = @PreferenceId)
	BEGIN
		insert into @Conflicts
		Select 'Preference ' + cast(@PreferenceId as nvarchar) + ' does not exist'
	END

	if not exists(Select 1 from [User] U where U.Id = @UserId)
	BEGIN
		insert into @Conflicts
		Select 'User ' + cast(@UserId as nvarchar) + ' does not exist'
	END
	
	if not exists(Select 1 from [UserInstance] U where U.UserId= @UserId AND U.VersionId = @VersionId)
	BEGIN
		insert into @Conflicts
		Select 'User ' + NickName + ' is not part of this version.'
		from [User] 
		where id = @UserId
	END

	--Ensure user has no other shift preference at this time.
	insert into @Conflicts
	select 'Conflicting Shift Preference for ' + U.NickName + ': ' + dbo.fn_GetDate(SP.[Day]) + ' ' + dbo.fn_GetTOD(SP.StartTime)
	from [ShiftPreference] SP 
	JOIN Preference P
	on P.id = @PreferenceId
	JOIN @RoomHours RH on SP.[Day] = RH.[Day]
	JOIN [User] U
	on U.Id = SP.UserId
	WHERE SP.VersionId = @VersionId AND 
	SP.UserId = @UserId AND
	SP.Id != @Id AND
		( 
			SP.StartTime = @StartTime  OR
			(cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) < @StartTimeMinutes AND cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) + (SP.Duration * 60)> @StartTimeMinutes ) OR --new shift preference starts before old shift preference but ends after
			(/*overflow*/cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) < RH.[start] AND cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) <= @StartTimeMinutes + 1440 /*(24 * 60)*/ AND cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) + (SP.Duration * 60) > @StartTimeMinutes + 1440 /*(24 * 60)*/) OR--overflow and old shift preference starts before new shift preference but ends after
			(@StartTimeMinutes < cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) AND @EndTimeMinutes > cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)))  OR --preference starts before shift but ends after
			(/*overflow*/cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) < RH.[start] AND @StartTimeMinutes <= cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) + 1440 /*(24 * 60)*/ AND @EndTimeMinutes > cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) + 1440 /*(24 * 60)*/)--overflow and new shift preference starts before old shift preference but ends after
		)
	 

	return 
END
GO
CREATE function [dbo].[ValidateShift](
	@Id int,
	@UserId int,
	@VersionId int,
	@RoomId int,
	@Day int,
	@StartTime time(7),
	@Duration decimal(9,1)
)
returns @Conflicts table ([message] nvarchar(max))
BEGIN
	Declare @RoomHours table
	(
		[Day] int Primary Key,
		[start] int
	)

	insert into @RoomHours
	select [Day], Min(DATEDIFF(minute, '00:00:00',StartTime))
	From RoomHours RH
	JOIN RoomInstance RI on RI.Id = RH.RoomInstanceId
	Where RI.VersionId = @VersionId
	AND RH.[Day] = @Day
	Group By [Day]

	Declare @StartTimeMinutes Decimal(9)
	Declare @EndTimeMinutes Decimal(9)
	select @StartTimeMinutes = cast(DATEDIFF(minute, '00:00:00',@StartTime) as decimal(9)),
	 @EndTimeMinutes  = cast(DATEDIFF(minute, '00:00:00',@StartTime) as decimal(9)) + @Duration * 60

	--Ensure user doesn't have conflicting shift preference
	insert into @Conflicts 
	select 'Conflicting Shift Preference for ' + U.NickName + ': ' + dbo.fn_GetDate(SP.[Day]) + ' ' + dbo.fn_GetTOD(SP.StartTime)
	from [ShiftPreference] SP
	JOIN Preference P
	on P.CanWork = 0 AND SP.PreferenceId = P.Id
	JOIN @RoomHours RH on SP.[Day] = RH.[Day]
	JOIN [User] U
	on U.Id = SP.UserId
	WHERE SP.VersionId = @VersionId AND 
	SP.UserId = @UserId AND
	SP.Id != @Id AND
		( 
			SP.StartTime = @StartTime  OR
			(cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) < @StartTimeMinutes AND cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) + (SP.Duration * 60)> @StartTimeMinutes ) OR --new shift preference starts before old shift preference but ends after
			(/*overflow*/cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) < RH.start AND cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) <= @StartTimeMinutes + 1440 /*(24 * 60)*/ AND cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) + (SP.Duration * 60) > @StartTimeMinutes + 1440 /*(24 * 60)*/) OR--overflow and old shift preference starts before new shift preference but ends after
			(@StartTimeMinutes < cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) AND @EndTimeMinutes > cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)))  OR --preference starts before shift but ends after
			(/*overflow*/cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) < RH.start AND @StartTimeMinutes <= cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) + 1440 /*(24 * 60)*/ AND @EndTimeMinutes > cast(DATEDIFF(minute, '00:00:00',SP.StartTime) as decimal(9)) + 1440 /*(24 * 60)*/)--overflow and new shift preference starts before old shift preference but ends after
		)
		 

	if not exists(Select 1 from Room R where R.Id = @RoomId)
	BEGIN
		insert into @Conflicts
		Select 'Room ' + cast(@RoomId as nvarchar) + ' does not exist'
	END

	if not exists(Select 1 from RoomView RV where RV.RoomId = @RoomId AND RV.VersionId = @VersionId)
	BEGIN
		insert into @Conflicts
		Select 'Room ' + cast(@RoomId as nvarchar) + ' is not part of this version'
	END

	if not exists(Select 1 from Room R JOIN RoomInstance RI on R.Id = RI.RoomId JOIN RoomHours RH on RH.RoomInstanceId = RI.Id where R.Id = @RoomId AND RI.VersionId = @VersionId AND RH.[Day] = @Day AND 
		(
			(RH.StartTime = @StartTime AND cast(DATEDIFF(minute, '00:00:00',RH.StartTime) as decimal(9)) + RH.Duration * 60 >= @EndTimeMinutes) OR
			(RH.StartTime < @StartTime AND cast(DATEDIFF(minute, '00:00:00',RH.StartTime) as decimal(9)) + RH.Duration * 60 > @StartTimeMinutes AND cast(DATEDIFF(minute, '00:00:00',RH.StartTime) as decimal(9)) + RH.Duration * 60 >= @EndTimeMinutes) OR
			(RH.StartTime > @StartTime AND cast(DATEDIFF(minute, '00:00:00',RH.StartTime) as decimal(9)) + RH.Duration * 60 > @StartTimeMinutes + 1440 /*(24 * 60)*/ AND cast(DATEDIFF(minute, '00:00:00',RH.StartTime) as decimal(9)) + RH.Duration * 60 >= @EndTimeMinutes + 1440 /*(24 * 60)*/ )
		)
	)
	BEGIN
		insert into @Conflicts
		Select 'Shift is outside of room hours ' + dbo.fn_GetTOD(@StartTime)
	END

	if not exists(Select 1 from [User] U where U.Id = @UserId)
	BEGIN
		insert into @Conflicts
		Select 'User ' + cast(@UserId as nvarchar) + ' does not exist'
	END
	
	if not exists(Select 1 from [UserInstance] U where U.UserId= @UserId AND U.VersionId = @VersionId)
	BEGIN
		insert into @Conflicts
		Select 'User ' + NickName + ' is not part of this version.'
		from [User] 
		where id = @UserId
	END	

	if exists(Select 1 from [UserView] U where U.UserId= @UserId AND U.VersionId = @VersionId AND MaxHours < CurrentHours + @Duration)
	BEGIN
		insert into @Conflicts
		Select 'New Shift would cause ' + NickName + ' to have too many hours.'
		from [User] 
		where id = @UserId
	END	

	--Ensure user has no other shift at this time.
	insert into @Conflicts 
	select 'Conflicting Shift Preference for ' + U.NickName + ': ' + dbo.fn_GetDate(S.[Day]) + ' ' + dbo.fn_GetTOD(S.StartTime) + ' in ' + R.Name
	from [Shift] S
	JOIN Room R on R.Id = S.RoomId 
	JOIN RoomInstance RI on RI.VersionId = @VersionId AND RI.RoomId = S.RoomId
	JOIN RoomHours RH on S.[Day] = RH.[Day] AND RH.RoomInstanceId = RI.Id
	JOIN [User] U
	on U.Id = S.UserId
	WHERE S.VersionId = @VersionId AND 
	S.UserId = @UserId AND
	S.[Day] = @Day AND
	S.Id != @Id AND
		( 
			S.StartTime = @StartTime  OR
			(cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) < @StartTimeMinutes AND cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) + (S.Duration * 60)> @StartTimeMinutes ) OR --new shift preference starts before old shift preference but ends after
			(/*overflow*/cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) < cast(DATEDIFF(minute, '00:00:00',RH.StartTime) as decimal(9)) AND cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) <= @StartTimeMinutes + 1440 /*(24 * 60)*/ AND cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) + (S.Duration * 60) > @StartTimeMinutes + 1440 /*(24 * 60)*/) OR--overflow and old shift preference starts before new shift preference but ends after
			(@StartTimeMinutes < cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) AND @EndTimeMinutes > cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)))  OR --preference starts before shift but ends after
			(/*overflow*/cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) < cast(DATEDIFF(minute, '00:00:00',RH.StartTime) as decimal(9)) AND @StartTimeMinutes <= cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) + 1440 /*(24 * 60)*/ AND @EndTimeMinutes > cast(DATEDIFF(minute, '00:00:00',S.StartTime) as decimal(9)) + 1440 /*(24 * 60)*/)--overflow and new shift preference starts before old shift preference but ends after
		)
		 
	
	return
END
GO