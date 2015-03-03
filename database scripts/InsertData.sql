insert into [Version]
Values ('Spring 2014', 1, 0, 0)

Insert into Building
Values ('ECS South', null, null, null)

Insert into Room
Values ('Comp. Lab #1', '201', @@IDENTITY, null),
('Comp. Lab #2', '202', @@IDENTITY, null)

Insert into RoomInstance
Select R.ID,
V.Id
From Room R
JOIN [Version] V on 1 = 1

--Declare @start datetime
--set @start = cast('1753-1-1' as datetime)
Insert into RoomHours
Select RI.ID,
1,--day
cast('08:00' as time),--start time
8--duration
From RoomInstance RI
JOIN Room R on R.Id = RI.RoomId
UNION Select RI.ID,
2,--day
cast('08:00' as time),--start time
8--duration
From RoomInstance RI
JOIN Room R on R.Id = RI.RoomId
UNION Select RI.ID,
3,--day
cast('08:00' as time),--start time
8--duration
From RoomInstance RI
JOIN Room R on R.Id = RI.RoomId
UNION Select RI.ID,
4,--day
cast('08:00' as time),--start time
8--duration
From RoomInstance RI
JOIN Room R on R.Id = RI.RoomId
UNION Select RI.ID,
5,--day
cast('08:00' as time),--start time
12--duration
From RoomInstance RI
JOIN Room R on R.Id = RI.RoomId
Select RI.ID,
6,--day
cast('08:00' as time),--start time
12--duration
From RoomInstance RI
JOIN Room R on R.Id = RI.RoomId

Insert into [Address]
Select '123 Easy',
null,
'Plano',
'Texas',
'75075',
'USA'

declare @address1 int
set @address1 = @@IDENTITY

Insert into [User]
Select 'Manager@ShiftCaptain.com',--EmailAddress
'1234567890',--password
'First',--FName
null,--MName
'Last',--LName
'Billy',--UserName
@address1,--Address
'8890732',--EmployeeId
null,--PhoneNumber
0,--IsShiftManager,
1,--IsManager
0,--Locked
null,--LastLogin
1,--IsActive
1 --IsMale
UNION Select 'ShiftManager@ShiftCaptain.com',--EmailAddress
'1234567890',--password
'First',--FName
null,--MName
'Last',--LName
'Shifty',--UserName
null,--Address
'8890732123',--EmployeeId
null,--PhoneNumber
1,--IsShiftManager,
0,--IsManager
0,--Locked
null,--LastLogin
1,--IsActive
1 --IsMale
UNION SELECT 'employee1@ShiftCaptain.com',--EmailAddress
'1234567890',--password
'First',--FName
null,--MName
'Last',--LName
'Bob',--UserName
null,--Address
'8890733',--EmployeeId
null,--PhoneNumber
0,--IsShiftManager,
0,--IsManager
0,--Locked
null,--LastLogin
1,--IsActive
1 --IsMale
UNION SELECT 'employee2@ShiftCaptain.com',--EmailAddress
'1234567890',--password
'First',--FName
null,--MName
'Last',--LName
'Bob 2',--UserName
null,--Address
'8890734',--EmployeeId
null,--PhoneNumber
0,--IsShiftManager,
0,--IsManager
0,--Locked
null,--LastLogin
1,--IsActive
1 --IsMale

UNION SELECT 'employee3@ShiftCaptain.com',--EmailAddress
'1234567890',--password
'First',--FName
null,--MName
'Last',--LName
'Jim',--UserName
null,--Address
'8890743',--EmployeeId
null,--PhoneNumber
0,--IsShiftManager,
0,--IsManager
0,--Locked
null,--LastLogin
1,--IsActive
1 --IsMale

UNION SELECT 'employee4@ShiftCaptain.com',--EmailAddress
'1234567890',--password
'First',--FName
null,--MName
'Last',--LName
'John',--UserName
null,--Address
'8890943',--EmployeeId
null,--PhoneNumber
0,--IsShiftManager,
0,--IsManager
0,--Locked
null,--LastLogin
1,--IsActive
1 --IsMale


UNION SELECT 'employee5@ShiftCaptain.com',--EmailAddress
'1234567890',--password
'First',--FName
null,--MName
'Last',--LName
'Tom',--UserName
null,--Address
'8990743',--EmployeeId
null,--PhoneNumber
0,--IsShiftManager,
0,--IsManager
0,--Locked
null,--LastLogin
1,--IsActive
1 --IsMale


UNION SELECT 'employee6@ShiftCaptain.com',--EmailAddress
'1234567890',--password
'First',--FName
null,--MName
'Last',--LName
'Molly',--UserName
null,--Address
'8890763',--EmployeeId
null,--PhoneNumber
0,--IsShiftManager,
0,--IsManager
0,--Locked
null,--LastLogin
1,--IsActive
0 --IsMale


UNION SELECT 'employee7@ShiftCaptain.com',--EmailAddress
'1234567890',--password
'First',--FName
null,--MName
'Last',--LName
'Blue',--UserName
null,--Address
'8891723',--EmployeeId
null,--PhoneNumber
0,--IsShiftManager,
0,--IsManager
0,--Locked
null,--LastLogin
1,--IsActive
1 --IsMale


UNION SELECT 'employee8@ShiftCaptain.com',--EmailAddress
'1234567890',--password
'First',--FName
null,--MName
'Last',--LName
'Mark',--UserName
null,--Address
'1890743',--EmployeeId
null,--PhoneNumber
0,--IsShiftManager,
0,--IsManager
0,--Locked
null,--LastLogin
1,--IsActive
1 --IsMale


UNION SELECT 'employee9@ShiftCaptain.com',--EmailAddress
'1234567890',--password
'First',--FName
null,--MName
'Last',--LName
'Bob 3',--UserName
null,--Address
'88907431',--EmployeeId
null,--PhoneNumber
0,--IsShiftManager,
0,--IsManager
0,--Locked
null,--LastLogin
1,--IsActive
1 --IsMale


UNION SELECT 'employee10@ShiftCaptain.com',--EmailAddress
'1234567890',--password
'First',--FName
null,--MName
'Last',--LName
'Bob 4',--UserName
null,--Address
'28890743',--EmployeeId
null,--PhoneNumber
0,--IsShiftManager,
0,--IsManager
0,--Locked
null,--LastLogin
1,--IsActive
1 --IsMale

Insert into UserInstance
Select U.Id,
V.Id,
20,--Min Hours
20,--Max Hours
0--Current Hours
From [User] U
JOIN [Version] V on 1 = 1
WHERE U.IsManager = 0


Insert into [Shift]
Select UI.UserId,
R.Id,
UI.VersionId,
1,
cast('08:00:00' as datetime),
2
FROM [UserInstance] UI
JOIN Room R on R.Id = 1
UNION Select UI.UserId,
R.Id,
UI.VersionId,
2,
cast('08:00:00' as datetime),
2
FROM [UserInstance] UI
JOIN Room R on R.Id = 1
UNION Select UI.UserId,
R.Id,
UI.VersionId,
3,
cast('08:00:00' as datetime),
2
FROM [UserInstance] UI
JOIN Room R on R.Id = 1
UNION Select UI.UserId,
R.Id,
UI.VersionId,
4,
cast('08:00:00' as datetime),
2
FROM [UserInstance] UI
JOIN Room R on R.Id = 1
UNION Select UI.UserId,
R.Id,
UI.VersionId,
5,
cast('08:00:00' as datetime),
2
FROM [UserInstance] UI
JOIN Room R on R.Id = 1

Insert Into Preference
Values ('Unavailable', 'Shifts that you cannot work.', 'Red', 0),
('Rather not','Prefer not to work', 'Yellow', 1),
('Prefer', 'Would like to work this shift', 'Green', 1)

Insert into ShiftPreference
Select 
	UserId,
	V.Id,
	PreferenceId,
	DayId,
	StartTime,
	Duration
FROM

(
	Select U.Id As UserId,
	P.Id As PreferenceId,
	'1' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890733'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'2' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890733'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'3' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890733'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'4' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890733'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'5' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890733'
	UNION 
	Select U.Id As UserId,
	P.Id As PreferenceId,
	'1' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890734'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'2' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890734'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'3' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890734'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'4' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890734'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'5' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890734'
	UNION 
	Select U.Id As UserId,
	P.Id As PreferenceId,
	'1' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890743'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'2' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890743'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'3' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890743'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'4' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890743'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'5' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890743'
	UNION 
	Select U.Id As UserId,
	P.Id As PreferenceId,
	'1' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890943'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'2' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890943'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'3' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890943'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'4' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890943'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'5' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '8890943'
	UNION 
	Select U.Id As UserId,
	P.Id As PreferenceId,
	'1' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '88907431'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'2' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '88907431'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'3' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '88907431'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'4' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '88907431'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'5' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '88907431'
	UNION 
	Select U.Id As UserId,
	P.Id As PreferenceId,
	'1' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '28890743'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'2' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '28890743'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'3' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '28890743'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'4' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '28890743'
	UNION Select U.Id As UserId,
	P.Id As PreferenceId,
	'5' As DayId,
	cast('8:00:00' as time) as StartTime,
	2 as Duration
	FROM [User] U
	JOIN [Preference] P on P.Name = 'Prefer'
	Where EmployeeId = '28890743'
	
	
	
) U 
JOIN [Version] V on 1 = 1
Where U.UserId != 1


Insert into ShiftPreference
Values(
	1,--UserId
	1,--VersionId
	1,--PreferenceId
	1,--DayId
	cast('8:00:00' as time),--StartTime
	8--Duration
),
(
	1,--UserId
	1,--VersionId
	1,--PreferenceId
	2,--DayId
	cast('8:00:00' as time),--StartTime
	8--Duration
),
(
	1,--UserId
	1,--VersionId
	1,--PreferenceId
	3,--DayId
	cast('8:00:00' as time),--StartTime
	8--Duration
),
(
	1,--UserId
	1,--VersionId
	1,--PreferenceId
	4,--DayId
	cast('8:00:00' as time),--StartTime
	8--Duration
),
(
	1,--UserId
	1,--VersionId
	1,--PreferenceId
	5,--DayId
	cast('8:00:00' as time),--StartTime
	12--Duration
)


Update UserInstance 
set CurrentHours =
(Select Sum(Duration)
from [Shift] s
where S.VersionId = UserInstance.VersionId
and S.UserId = UserInstance.UserId
)
