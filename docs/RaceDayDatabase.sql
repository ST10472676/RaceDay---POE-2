CREATE DATABASE RaceDayDB;
GO

USE RaceDayDB;
GO

CREATE TABLE Users
(
UserID INT IDENTITY(1,1) PRIMARY KEY,

FirstName NVARCHAR(50) NOT NULL,

LastName NVARCHAR(50) NOT NULL,

Email NVARCHAR(100) NOT NULL UNIQUE,

PasswordHash NVARCHAR(255) NOT NULL,

PhoneNumber NVARCHAR(20),

Role NVARCHAR(20) NOT NULL,

DateCreated DATETIME DEFAULT GETDATE(),

CONSTRAINT CK_Users_Role
CHECK (Role IN ('Organiser', 'Participant'))
);
GO


CREATE TABLE Routes
(
RouteID INT IDENTITY(1,1) PRIMARY KEY,

StartLocation NVARCHAR(100) NOT NULL,

EndLocation NVARCHAR(100) NOT NULL,

RouteDirection NVARCHAR(50) NOT NULL,

Distance DECIMAL(6,2) NOT NULL,

Description NVARCHAR(500)
);
GO

CREATE TABLE Events
(
EventID INT IDENTITY(1,1) PRIMARY KEY,

OrganiserID INT NOT NULL,

RouteID INT NOT NULL,

EventName NVARCHAR(150) NOT NULL,

Description NVARCHAR(1000),

EventDate DATE NOT NULL,

Location NVARCHAR(150) NOT NULL,

Distance DECIMAL(6,2) NOT NULL,

EventType NVARCHAR(20) NOT NULL,

BannerImageUrl NVARCHAR(500),

    CONSTRAINT FK_Events_Organiser
    FOREIGN KEY (OrganiserID)
    REFERENCES Users(UserID),

    CONSTRAINT FK_Events_Route
    FOREIGN KEY (RouteID)
    REFERENCES Routes(RouteID),

    CONSTRAINT CK_Events_EventType
    CHECK (EventType IN ('Run', 'Walk', 'Cycle'))
);
GO

CREATE TABLE Categories
(
CategoryID INT IDENTITY(1,1) PRIMARY KEY,

EventID INT NOT NULL,

CategoryName NVARCHAR(100) NOT NULL,

MinAge INT NOT NULL,

MaxAge INT NOT NULL,

    CONSTRAINT FK_Categories_Event
    FOREIGN KEY (EventID)
    REFERENCES Events(EventID),

    CONSTRAINT CK_Categories_Age
    CHECK (MaxAge >= MinAge)
);
GO

CREATE TABLE Enrolments
(
EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,

ParticipantID INT NOT NULL,

EventID INT NOT NULL,

CategoryID INT NOT NULL,

EnrolmentDate DATETIME DEFAULT GETDATE(),

Status NVARCHAR(30) DEFAULT 'Pending',

    CONSTRAINT FK_Enrolments_Participant
    FOREIGN KEY (ParticipantID)
    REFERENCES Users(UserID),

    CONSTRAINT FK_Enrolments_Event
    FOREIGN KEY (EventID)
    REFERENCES Events(EventID),

    CONSTRAINT FK_Enrolments_Category
    FOREIGN KEY (CategoryID)
    REFERENCES Categories(CategoryID),

    CONSTRAINT CK_Enrolments_Status
    CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled'))
);
GO

CREATE TABLE Results
(
ResultID INT IDENTITY(1,1) PRIMARY KEY,

EnrolmentID INT NOT NULL UNIQUE,

FinishTime TIME,

FinishingPosition INT,

ResultStatus NVARCHAR(30) DEFAULT 'Completed',

    CONSTRAINT FK_Results_Enrolment
    FOREIGN KEY (EnrolmentID)
    REFERENCES Enrolments(EnrolmentID),

    CONSTRAINT CK_Results_Position
    CHECK (FinishingPosition > 0)
);
GO

CREATE TABLE EventWeather
(
WeatherID INT IDENTITY(1,1) PRIMARY KEY,

EventID INT NOT NULL,

WeatherDate DATE NOT NULL,

Temperature DECIMAL(4,1),

WeatherCondition NVARCHAR(100),

Humidity INT,

    CONSTRAINT FK_EventWeather_Event
    FOREIGN KEY (EventID)
    REFERENCES Events(EventID),

    CONSTRAINT CK_EventWeather_Humidity
    CHECK (Humidity BETWEEN 0 AND 100)
);
GO

INSERT INTO Users
(
    FirstName,
    LastName,
    Email,
    PasswordHash,
    PhoneNumber,
    Role
)
VALUES
(
    'Unanne',
    'Munaka',
    'umunaka@raceday.co.za',
    'SampleHashedPassword1',
    '0762799412',
    'Organiser'
),
(
    'Naledi',
    'Khumalo',
    'naledi.khumalo@raceday.co.za',
    'SampleHashedPassword2',
    '0825551002',
    'Organiser'
),
(
    'Anele',
    'Ntaka',
    'anele.ntaka@email.co.za',
    'SampleHashedPassword3',
    '0696988781',
    'Participant'
),
(
    'eSihle',
    'Mokoena',
    'sihle.sincindi@email.co.za',
    'SampleHashedPassword4',
    '0677039634',
    'Participant'
);
GO

INSERT INTO Routes
(
    StartLocation,
    EndLocation,
    RouteDirection,
    Distance,
    Description
)
VALUES
(
    'Pietermaritzburg',
    'Durban',
    'Down Run',
    89.00,
    'Ultra-marathon route from Pietermaritzburg to Durban.'
),
(
    'Durban',
    'Pietermaritzburg',
    'Up Run',
    89.00,
    'Ultra-marathon route from Durban to Pietermaritzburg.'
),
(
    'Durban',
    'Durban',
    'Training Route',
    21.10,
    'Half-marathon preparation route.'
);
GO

INSERT INTO Events
(
    OrganiserID,
    RouteID,
    EventName,
    Description,
    EventDate,
    Location,
    Distance,
    EventType
)
VALUES
(
    1,
    1,
    'Comrades Marathon 2026',
    'South African ultra-marathon event.',
    '2026-06-14',
    'KwaZulu-Natal',
    89.00,
    'Run'
),
(
    2,
    2,
    'Comrades Marathon 2027',
    'Planned South African ultra-marathon event.',
    '2027-06-13',
    'KwaZulu-Natal',
    89.00,
    'Run'
),
(
    1,
    3,
    'Comrades Preparation Half Marathon',
    'Training event for long-distance runners.',
    '2026-03-22',
    'Durban',
    21.10,
    'Run'
);
GO

INSERT INTO Categories
(
    EventID,
    CategoryName,
    MinAge,
    MaxAge
)
VALUES
(1, 'Senior', 20, 39),
(1, 'Veteran', 40, 49),
(1, 'Masters', 50, 59),

(2, 'Senior', 20, 39),
(2, 'Veteran', 40, 49),
(2, 'Masters', 50, 59),

(3, 'Senior', 18, 39),
(3, 'Veteran', 40, 49);
GO

INSERT INTO Enrolments
(
    ParticipantID,
    EventID,
    CategoryID,
    Status
)
VALUES
(3, 1, 1, 'Confirmed'),
(4, 1, 2, 'Confirmed'),
(3, 3, 7, 'Confirmed'),
(4, 2, 5, 'Pending');
GO

INSERT INTO Results
(
    EnrolmentID,
    FinishTime,
    FinishingPosition,
    ResultStatus
)
VALUES
(1, '08:45:32', 47, 'Completed'),
(2, '09:15:18', 112, 'Completed'),
(3, '01:48:25', 25, 'Completed');
GO

INSERT INTO EventWeather
(
    EventID,
    WeatherDate,
    Temperature,
    WeatherCondition,
    Humidity
)
VALUES
(1, '2026-06-14', 18.5, 'Partly Cloudy', 65),
(2, '2027-06-13', 17.0, 'Clear', 60),
(3, '2026-03-22', 23.5, 'Sunny', 70);
GO


SELECT * FROM Users;

SELECT * FROM Routes;

SELECT * FROM Events;

SELECT * FROM Categories;

SELECT * FROM Enrolments;

SELECT * FROM Results;

SELECT * FROM EventWeather;