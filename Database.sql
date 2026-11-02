-- Run this in SQL Server Management Studio (New Query -> Execute / F5)
IF DB_ID(N'ElevatorDB') IS NULL CREATE DATABASE ElevatorDB;
GO
USE ElevatorDB;
GO
IF OBJECT_ID(N'dbo.ElevatorLog', N'U') IS NULL
CREATE TABLE dbo.ElevatorLog
(
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    LogTime      DATETIME2(3)  NOT NULL,   -- timestamp of the event
    ElevatorName NVARCHAR(20)  NOT NULL,   -- Elevator A / Elevator B
    EventType    NVARCHAR(30)  NOT NULL,   -- Request, StateChanged, FloorReached, Sos, Resumed, Info
    StateName    NVARCHAR(30)  NOT NULL,   -- state when the event happened
    FloorNo      INT           NOT NULL,   -- last floor reached
    PositionPct  INT           NOT NULL,   -- 0 = Floor 0, 100 = Floor 1 (shows exactly where the lift is)
    Description  NVARCHAR(250) NOT NULL
);
GO
SELECT * FROM dbo.ElevatorLog ORDER BY Id DESC;
