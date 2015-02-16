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

Create Table [Version]
(
	Id int Primary Key not null identity,
	Name nvarchar(500) not null,
	IsActive bit not null,
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
	Duration time(0) not null
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
	LastLogin DateTime,
	IsActive bit not null,
	IsMale bit not null 
)

Create Table UserInstance
(
	Id int Primary Key not null identity,
	UserId int not null Foreign Key REFERENCES [User](Id),
	VersionId int not null Foreign Key REFERENCES [Version](Id),
	MinHours time(0) not null,
	MaxHours time(0) not null,
	CurrentHours decimal(9,1) not null
)

Create Table [Shift]
(
	Id int Primary Key not null identity,
	UserId int not null Foreign Key REFERENCES [User](Id),
	RoomId int not null Foreign Key REFERENCES Room(Id),
	VersionId int not null Foreign Key REFERENCES [Version](Id),
	[Day] int not null,
	StartTime time(0) not null,
	Duration time(0) not null
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
	Duration time(0) not null
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
UI.CurrentHours,
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
 Sun.Id as SundayId,
 Sun.[Day] as SundayInstanceId,
 Sun.StartTime as SundayStartTime,
 Sun.Duration as SundayDuration,
 Mon.Id as MondayId,
 Mon.[Day] as MondayInstanceId,
 Mon.StartTime as MondayStartTime,
 Mon.Duration as MondayDuration,
 Tues.Id as TuesdayId,
 Tues.[Day] as TuesdayInstanceId,
 Tues.StartTime as TuesdayStartTime,
 Tues.Duration as TuesdayDuration,
 Wed.Id as WednesdayId,
 Wed.[Day] as WednesdayInstanceId,
 Wed.StartTime as WednesdayStartTime,
 Wed.Duration as WednesdayDuration,
 Thurs.Id as ThursdayId,
 Thurs.[Day] as ThursdayInstanceId,
 Thurs.StartTime as ThursdayStartTime,
 Thurs.Duration as ThursdayDuration,
 Fri.Id as FridayId,
 Fri.[Day] as FridayInstanceId,
 Fri.StartTime as FridayStartTime,
 Fri.Duration as FridayDuration,
 Sat.Id as SaturdayId,
 Sat.[Day] as SaturdayInstanceId,
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