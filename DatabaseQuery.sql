-- ========================================
-- Create the database
-- ========================================
CREATE DATABASE StudentSystem;
GO

USE StudentSystem;
GO

-- ========================================
-- Table: Headmasters
-- Stores information about headmasters
-- ========================================
CREATE TABLE Headmasters
(
    Id            INT IDENTITY(1,1) PRIMARY KEY,   -- Unique headmaster ID
    Username      NVARCHAR(50) NOT NULL,           -- Login username
    PasswordHash  NVARCHAR(255) NOT NULL,          -- Hashed password
    FullName      NVARCHAR(100) NOT NULL,          -- Full name
    SchoolName    NVARCHAR(100) NOT NULL,          -- School they manage
    ContactInfo   NVARCHAR(100) NOT NULL           -- Email or phone
);
GO

-- ========================================
-- Table: Reports
-- Stores reports created by headmasters
-- ========================================
CREATE TABLE Reports
(
    HeadmasterId INT NOT NULL,                     -- Reference to headmaster
    ReportInfo   NVARCHAR(255) NOT NULL,           -- Report details
    FOREIGN KEY (HeadmasterId) REFERENCES Headmasters(Id)
);
GO

-- ========================================
-- Table: Announcements
-- Stores announcements made by headmasters
-- ========================================
CREATE TABLE Announcements
(
    HeadmasterId       INT NOT NULL,              -- Reference to headmaster
    AnnouncementInfo   NVARCHAR(255) NOT NULL,	  -- Announcement content
    FOREIGN KEY (HeadmasterId) REFERENCES Headmasters(Id)
);
GO

-- ========================================
-- Table: Students
-- Stores student accounts and basic info
-- ========================================
CREATE TABLE Students
(
    StudentId    INT IDENTITY(1,1) PRIMARY KEY,  -- Unique student ID
    Username     NVARCHAR(50) NOT NULL,          -- Login username
    PasswordHash NVARCHAR(255) NOT NULL,         -- Hashed password
    FullName     NVARCHAR(100) NOT NULL,         -- Full name
    DateOfBirth  DATE NOT NULL                   -- Birthday
);
GO

-- ========================================
-- Table: Teachers
-- Stores teacher accounts
-- ========================================
CREATE TABLE Teachers
(
    TeacherId    INT IDENTITY(1,1) PRIMARY KEY,  -- Unique teacher ID
    Username     NVARCHAR(50) NOT NULL,          -- Login username
    PasswordHash NVARCHAR(255) NOT NULL,         -- Hashed password
    FullName     NVARCHAR(100) NOT NULL          -- Full name
);
GO

-- ========================================
-- Table: Subjects
-- Stores school subjects
-- ========================================
CREATE TABLE Subjects
(
    SubjectID   INT NOT NULL,                     -- Unique subject ID
    SubjectName VARCHAR(100) NOT NULL,            -- Name of the subject
    CONSTRAINT PK_Subjects PRIMARY KEY (SubjectID)
);
GO

-- ========================================
-- Table: StudentSubjectGrades
-- Stores grades of students for subjects
-- ========================================
CREATE TABLE StudentSubjectGrades
(
    StudentId INT NOT NULL,                       -- Reference to student
    SubjectId INT NOT NULL,                       -- Reference to subject
    Grade     FLOAT NOT NULL,                     -- Grade received
    FOREIGN KEY (StudentId) REFERENCES Students(StudentId),
    FOREIGN KEY (SubjectId) REFERENCES Subjects(SubjectID)
);
GO

-- ========================================
-- Table: TeacherSubjects
-- Tracks which subjects a teacher teaches
-- ========================================
CREATE TABLE TeacherSubjects
(
    TeacherId INT NOT NULL,                       -- Reference to teacher
    SubjectId INT NOT NULL,                       -- Reference to subject
    FOREIGN KEY (TeacherId) REFERENCES Teachers(TeacherId),
    FOREIGN KEY (SubjectId) REFERENCES Subjects(SubjectID)
);
GO

-- ========================================
-- Table: AccountRequests
-- Stores requests for new accounts
-- ========================================
CREATE TABLE AccountRequests
(
    UserId      INT IDENTITY(1,1) PRIMARY KEY,   -- Unique request ID
    Username    NVARCHAR(50) UNIQUE,             -- Requested username
    Password    NVARCHAR(255),                   -- Password (plain text temporarily)
    FullName    NVARCHAR(100),                   -- Full name
    DateOfBirth DATE                             -- Birthday
);
GO

-- ========================================
-- Table: Feedbacks
-- Stores feedback for students
-- ========================================
CREATE TABLE Feedbacks
(
    StudentId      INT NOT NULL,                 -- Reference to student
    PraiseOrRemark NVARCHAR(255) NULL,           -- Praise or remark about the student
    Info           NVARCHAR(255) NULL,           -- Additional info
    Added_By       NVARCHAR(100) NULL            -- Who added this feedback
);
GO
