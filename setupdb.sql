USE [master]

IF NOT EXISTS (SELECT * FROM sys.databases where [name] = 'WhenIsRelease')
BEGIN

    CREATE LOGIN logwrite WITH PASSWORD = 'b34UcA2jTPaJMRzW';
    CREATE DATABASE WhenIsRelease;

END

GO

IF NOT EXISTS (SELECT * FROM sys.tables where [name] = 'Log')
BEGIN

    USE WhenIsRelease;
    CREATE USER logwrite FOR LOGIN logwrite; 

    CREATE TABLE [dbo].[Log] (
     [ID] [int] IDENTITY(1,1) NOT NULL,
     [MachineName] [nvarchar](200) NULL,
     [Logged] [datetime] NOT NULL,
     [Level] [varchar](5) NOT NULL,
     [Message] [nvarchar](max) NOT NULL,
     [Logger] [nvarchar](300) NULL,
     [Properties] [nvarchar](max) NULL,
     [Callsite] [nvarchar](300) NULL,
     [Exception] [nvarchar](max) NULL,
    CONSTRAINT [PK_dbo.Log] PRIMARY KEY CLUSTERED ([ID] ASC) 
     WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

END

GO

IF EXISTS (SELECT * FROM sys.procedures where [name] = 'Log_AddEntry_p')
DROP PROCEDURE [Log_AddEntry_p]
GO

CREATE PROCEDURE [dbo].[Log_AddEntry_p] (
    @machineName nvarchar(200),
    @logged datetime,
    @level varchar(5),
    @message nvarchar(max),
    @logger nvarchar(300),
    @properties nvarchar(max),
    @callsite nvarchar(300),
    @exception nvarchar(max)
    ) AS
	BEGIN
    INSERT INTO [dbo].[Log] (
      [MachineName],
      [Logged],
      [Level],
      [Message],
      [Logger],
      [Properties],
      [Callsite],
      [Exception]
    ) VALUES (
      @machineName,
      @logged,
      @level,
      @message,
      @logger,
      @properties,
      @callsite,
      @exception
    );
	END

GO

GRANT EXECUTE ON Log_AddEntry_p TO logwrite;

GO