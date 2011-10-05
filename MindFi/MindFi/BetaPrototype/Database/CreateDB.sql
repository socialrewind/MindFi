CREATE TABLE [Config] (
  [Username] NVARCHAR(255) NOT NULL ON CONFLICT FAIL, 
  [CurrentStep] INT);

CREATE TABLE [DailyID] (
  [Day] numeric(8) NOT NULL, 
  [ReferenceTable] int NOT NULL, 
  [FreeID] int NOT NULL, 
  CONSTRAINT [] PRIMARY KEY ([Day], [ReferenceTable]) ON CONFLICT ABORT);

CREATE TABLE [Entities] (
  [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
  [Name] NVARCHAR(255) NOT NULL, 
  [Type] NVARCHAR(255) NOT NULL, 
  [Active] BOOL);

CREATE INDEX [Entity_byName] ON [Entities] ([Name]);

CREATE TABLE [Languages] (
  [ID] INT, 
  [NAME] NVARCHAR(255), 
  [CODE] CHAR(2), 
  CONSTRAINT [] PRIMARY KEY ([ID]) ON CONFLICT ABORT);

CREATE TABLE [AlbumData] (
  [PartitionDate] numeric(8) NOT NULL, 
  [PartitionID] int NOT NULL, 
  [AlbumID] int NOT NULL, 
  [SocialNetwork] int NOT NULL, 
  [SNID] string NOT NULL, 
  [Active] bit NOT NULL, 
  [LastUpdate] datetime NOT NULL, 
  [PreviousDate] numeric(8), 
  [PreviousID] int, 
  [FromID] NVARCHAR(255), 
  [FromName] NVARCHAR(255), 
  [Description] NVARCHAR(255), 
  [Location] NVARCHAR(255), 
  [Link] TEXT, 
  [PhotoCount] int, 
  [Privacy] NVARCHAR(255), 
  [PrivacyValue] NVARCHAR(255), 
  [Created] DATETIME, 
  [Updated] DATETIME, 
  [Path] TEXT, 
  [CoverPicture] TEXT, 
  [AlbumType] NVARCHAR(255), 
  CONSTRAINT [sqlite_autoindex_AlbumData_1] PRIMARY KEY ([PartitionDate], [PartitionID]));

CREATE INDEX [Album_byID] ON [AlbumData] ([AlbumID]);

CREATE INDEX [Album_bySNID] ON [AlbumData] ([SocialNetwork], [SNID]);

CREATE TABLE [PhotoData] (
  [PartitionDate] numeric(8) NOT NULL, 
  [PartitionID] int NOT NULL, 
  [PhotoID] int NOT NULL, 
  [SocialNetwork] int NOT NULL, 
  [SNID] string NOT NULL, 
  [Active] bit NOT NULL, 
  [LastUpdate] datetime NOT NULL, 
  [PreviousDate] numeric(8), 
  [PreviousID] int, 
  [FromID] NVARCHAR(255), 
  [FromName] NVARCHAR(255), 
  [Icon] TEXT, 
  [Source] TEXT, 
  [Link] TEXT,   
  [Height] INT,  
  [Width] INT,
  [Created] DATETIME, 
  [Updated] DATETIME, 
  [Path] TEXT, 
  CONSTRAINT [sqlite_autoindex_PhotoData_1] PRIMARY KEY ([PartitionDate], [PartitionID]));

CREATE INDEX [Photo_byID] ON [PhotoData] ([PhotoID]);

CREATE INDEX [Photo_bySNID] ON [PhotoData] ([SocialNetwork], [SNID]);

CREATE TABLE [TagData] (
  [PartitionDate] numeric(8) NOT NULL, 
  [PartitionID] int NOT NULL, 
  [SocialNetwork] int NOT NULL, 
  [PersonSNID] string NOT NULL, 
  [PhotoSNID] string NOT NULL, 
  [Active] bit NOT NULL, 
  [LastUpdate] datetime NOT NULL, 
  [PreviousDate] numeric(8), 
  [PreviousID] int, 
  [X] INT,  
  [Y] INT,
  [Created] DATETIME, 
  [Updated] DATETIME, 
  CONSTRAINT [sqlite_autoindex_TagData_1] PRIMARY KEY ([PartitionDate], [PartitionID]));

CREATE INDEX [Tag_byPhotoSNID] ON [TagData] ([SocialNetwork], [PhotoSNID]);

CREATE INDEX [Tag_byPersonSNID] ON [TagData] ([SocialNetwork], [PersonSNID]);

CREATE TABLE [MessageData] (
  [PartitionDate] numeric(8) NOT NULL, 
  [PartitionID] int NOT NULL, 
  [MessageID] int NOT NULL, 
  [SocialNetwork] int NOT NULL, 
  [SNID] string NOT NULL, 
  [Active] bit NOT NULL, 
  [LastUpdate] datetime NOT NULL, 
  [PreviousDate] numeric, 
  [PreviousID] int, 
  [FromID] NVARCHAR(255), 
  [FromName] NVARCHAR(255), 
  [FromEmail] NVARCHAR(255), 
  [ToID] NVARCHAR(255), 
  [ToName] TEXT, 
  [ToEmail] TEXT, 
  [Message] TEXT, 
  [Subject] NVARCHAR(255), 
  [Created] DATETIME, 
  [Updated] DATETIME, 
  [ParentID] NVARCHAR(255), 
  CONSTRAINT [sqlite_autoindex_MessageData_1] PRIMARY KEY ([PartitionDate], [PartitionID]));

CREATE INDEX [Message_byID] ON [MessageData] ([MessageID]);

CREATE INDEX [Message_bySNID] ON [MessageData] ([SocialNetwork], [SNID]);

CREATE TABLE [OrganizationData] (
  [PartitionDate] numeric(8) NOT NULL, 
  [PartitionID] int NOT NULL, 
  [OrganizationID] int NOT NULL, 
  [SocialNetwork] int NOT NULL, 
  [SNID] string NOT NULL, 
  [Active] bit NOT NULL, 
  [LastUpdate] datetime NOT NULL, 
  [PreviousDate] numeric, 
  [PreviousID] int, 
  [Link] nvarchar, 
  [UpdatedInSN] datetime, 
  CONSTRAINT [] PRIMARY KEY ([PartitionDate], [PartitionID]) ON CONFLICT ABORT);

CREATE INDEX [Organization_byID] ON [OrganizationData] ([OrganizationID]);

CREATE INDEX [Organization_bySNID] ON [OrganizationData] ([SocialNetwork], [SNID]);

CREATE TABLE [PersonData] (
  [PartitionDate] numeric(8) NOT NULL, 
  [PartitionID] int NOT NULL, 
  [PersonID] int NOT NULL, 
  [SocialNetwork] int NOT NULL, 
  [SNID] string NOT NULL, 
  [Active] bit NOT NULL, 
  [Distance] int, 
  [LastUpdate] datetime NOT NULL, 
  [PreviousDate] numeric, 
  [PreviousID] int, 
  [ProfilePic] nvarchar, 
  [Link] nvarchar, 
  [Updated] datetime, 
  [FirstName] nvarchar, 
  [MiddleName] nvarchar, 
  [LastName] nvarchar, 
  [BirthDay] tinyint, 
  [BirthMonth] tinyint, 
  [BirthYear] smallint, 
  [UserName] nvarchar, 
  [Gender] nchar, 
  [Locale] nvarchar, 
  [UserTimezone] int, 
  [About] text, 
  [Bio] text, 
  [Quotes] text, 
  [Hometown] int, 
  [Location] int, 
  [InterestedIn] nvarchar, 
  [MeetingFor] nvarchar, 
  [Political] nvarchar, 
  [Religion] nvarchar, 
  [RelationshipStatus] nvarchar, 
  [SignificantOther] int, 
  [Verified] bit, 
  CONSTRAINT [sqlite_autoindex_PersonData_1] PRIMARY KEY ([PartitionDate], [PartitionID]));

CREATE INDEX [Person_byBDay] ON [PersonData] ([BirthDay], [BirthMonth], [BirthYear]);

CREATE INDEX [Person_byFirstName] ON [PersonData] ([FirstName]);

CREATE INDEX [Person_byID] ON [PersonData] ([PersonID]);

CREATE INDEX [Person_byLastname] ON [PersonData] ([LastName]);

CREATE INDEX [Person_bySNID] ON [PersonData] ([SocialNetwork], [SNID]);

CREATE TABLE [PostData] (
  [PartitionDate] numeric(8) NOT NULL, 
  [PartitionID] int NOT NULL, 
  [PostID] int NOT NULL, 
  [SocialNetwork] int NOT NULL, 
  [SNID] string NOT NULL, 
  [Active] bit NOT NULL, 
  [LastUpdate] datetime NOT NULL, 
  [PreviousDate] numeric, 
  [PreviousID] int, 
  [FromID] NVARCHAR(255), 
  [FromName] NVARCHAR(255), 
  [ToID] NVARCHAR(255), 
  [ToName] NVARCHAR(255), 
  [Message] TEXT, 
  [Picture] TEXT, 
  [Link] TEXT, 
  [Caption] NVARCHAR(255), 
  [Description] NVARCHAR(255), 
  [Source] NVARCHAR(255), 
  [Icon] NVARCHAR(255), 
  [Attribution] NVARCHAR(255), 
  [Privacy] NVARCHAR(255), 
  [PrivacyValue] NVARCHAR(255), 
  [Created] DATETIME, 
  [Updated] DATETIME, 
  [ActionsID] NVARCHAR(255), 
  [ActionsName] NVARCHAR(255), 
  [ApplicationID] NVARCHAR(255), 
  [ApplicationName] NVARCHAR(255), 
  [PostType] NVARCHAR(255), 
  [ObjectID] NVARCHAR(255), 
  [CommentCount] INT, 
  [LikesCount] INT, 
  [ParentID] NVARCHAR(255), 
  CONSTRAINT [sqlite_autoindex_PostData_1] PRIMARY KEY ([PartitionDate], [PartitionID]));

CREATE INDEX [Post_byID] ON [PostData] ([PostID]);

CREATE INDEX [Post_bySNID] ON [PostData] ([SocialNetwork], [SNID]);

CREATE TABLE [RelationsData] (
  [PartitionDate] numeric(8) NOT NULL, 
  [PartitionID] int NOT NULL, 
  [SubjectID] int NOT NULL, 
  [VerbID] int NOT NULL, 
  [ObjectID] INT, 
  [Adverb] string, 
  [IndirectObject] string, 
  [StartTimeDay] tinyint, 
  [StartTimeMonth] tinyint, 
  [StartTimeYear] smallint, 
  [EndTimeDay] tinyint, 
  [EndTimeMonth] tinyint, 
  [EndTimeYear] smallint, 
  [Created] datetime NOT NULL, 
  [LastUpdate] datetime NOT NULL, 
  [PreviousDate] numeric, 
  [PreviousID] int, 
  [Active] bit, 
  CONSTRAINT [] PRIMARY KEY ([PartitionDate], [PartitionID]) ON CONFLICT ABORT);

CREATE INDEX [Relations_bySubject] ON [RelationsData] ([SubjectID]);

CREATE INDEX [Relations_byObject] ON [RelationsData] ([ObjectID]);

CREATE INDEX [tempRelations_bySubject] ON [RelationsData] ([SubjectID]);

CREATE INDEX [tempRelations_byObject] ON [RelationsData] ([ObjectID]);

CREATE TABLE [RequestsQueue] (
  [ID] INT NOT NULL ON CONFLICT FAIL, 
  [RequestType] nvarchar(50) NOT NULL ON CONFLICT FAIL, 
  [ParentID] INT, 
  [ParentSNID] NVARCHAR(255), 
  [Priority] INT NOT NULL, 
  [Created] DATETIME NOT NULL ON CONFLICT FAIL, 
  [Updated] DATETIME, 
  [RequestString] TEXT, 
  [ResponseValue] TEXT, 
  [Filename] NVARCHAR(255), 
  [State] INT NOT NULL ON CONFLICT FAIL, 
  [MethodToCall] INT, 
  [AddToken] bit NOT NULL ON CONFLICT FAIL, 
  [Dependency] INT CONSTRAINT [FK_RequestsQueueDependency] REFERENCES [RequestsQueue]([ID]) ON DELETE CASCADE ON UPDATE CASCADE MATCH SIMPLE NOT DEFERRABLE, 
  CONSTRAINT [sqlite_autoindex_RequestsQueue_1] PRIMARY KEY ([ID]));

CREATE INDEX [Requests_byPID] ON [RequestsQueue] ([ParentID]);

CREATE INDEX [Requests_ByPriority] ON [RequestsQueue] ([Priority]);

CREATE INDEX [Requests_byPSNID] ON [RequestsQueue] ([ParentSNID]);

CREATE TABLE [SNAccounts] (
  [AccountID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
  [Name] NVARCHAR(255) NOT NULL, 
  [EMail] NVARCHAR(255), 
  [SocialNetwork] INT NOT NULL, 
  [URL] NVARCHAR(255), 
  [SNID] NVARCHAR(255), 
  [PersonID] INT, 
  [Active] BOOL);

CREATE TABLE [SocialNetworks] (
  [ID] int NOT NULL, 
  [SNName] nvarchar(255) NOT NULL, 
  CONSTRAINT [] PRIMARY KEY ([ID]) ON CONFLICT ABORT);

CREATE TABLE [Temps] (
  [ID] INT, 
  [Name] NVARCHAR(255), 
  CONSTRAINT [] PRIMARY KEY ([ID]) ON CONFLICT ABORT);

CREATE TABLE [VERBALIZATION] (
  [VERBID] INT, 
  [TIMEID] INT, 
  [LANGID] INT, 
  [PATTERN] NVARCHAR(255), 
  CONSTRAINT [] PRIMARY KEY ([VERBID], [TIMEID], [LANGID]) ON CONFLICT ABORT);

CREATE TABLE [Verbs] (
  [VerbID] INT, 
  [VerbName] NVARCHAR(255), 
  CONSTRAINT [] PRIMARY KEY ([VerbID]) ON CONFLICT ABORT);


INSERT INTO LANGUAGES (ID, NAME, CODE) 
	VALUES (1, 'ENGLISH', 'EN');

INSERT INTO LANGUAGES (ID, NAME, CODE) 
	VALUES (2, 'ESPAÑOL', 'ES');

INSERT INTO SOCIALNETWORKS(ID, SNName)
	VALUES (1, 'Facebook');

INSERT INTO SOCIALNETWORKS(ID, SNName)
	VALUES (2, 'Twitter');

INSERT INTO SOCIALNETWORKS(ID, SNName)
	VALUES (3, 'LinkedIn');

INSERT INTO SOCIALNETWORKS(ID, SNName)
	VALUES (4, 'Google+');

INSERT INTO TEMPS(ID, NAME)
	VALUES(1, 'PRESENT');

INSERT INTO TEMPS(ID, NAME)
	VALUES( 2, 'PAST');

INSERT INTO TEMPS(ID, NAME)
	VALUES( 3, 'FUTURE' );

INSERT INTO VERBS ( VerbID, VerbName )
	VALUES ( 1 , 'FRIENDOF' );


INSERT INTO VERBS ( VerbID, VerbName )
	VALUES ( 2 , 'SIGNIFICANOTHEROF' );


INSERT INTO VERBS ( VerbID, VerbName )
	VALUES ( 3 , 'WORKAT' );


INSERT INTO VERBS ( VerbID, VerbName )
	VALUES ( 4 , 'STUDYAT' );


INSERT INTO VERBS ( VerbID, VerbName )
	VALUES ( 5 , 'SPEAK' );


INSERT INTO VERBS ( VerbID, VerbName )
	VALUES ( 6 , 'LIVESAT' );


INSERT INTO VERBS ( VerbID, VerbName )
	VALUES ( 7 , 'ORIGINALLYFROM' );


INSERT INTO VERBS ( VerbID, VerbName )
	VALUES ( 8 , 'FANOF' );


INSERT INTO VERBS ( VerbID, VerbName )
	VALUES ( 9 , 'TAG' );


INSERT INTO VERBS ( VerbID, VerbName )
	VALUES ( 10 , 'LIKE' );


INSERT INTO VERBS ( VerbID, VerbName )
	VALUES ( 11 , 'COMMENT
' );

INSERT INTO VERBALIZATION ( VERBID, TIMEID, LANGID, PATTERN )
	VALUES (1,1,1,'%s and %s are now friends');
INSERT INTO VERBALIZATION ( VERBID, TIMEID, LANGID, PATTERN )
	VALUES (2,1,1,'%s and %s are now in a relationship and is %s');
INSERT INTO VERBALIZATION ( VERBID, TIMEID, LANGID, PATTERN )
	VALUES (3,1,1,'%s now works at %s');
INSERT INTO VERBALIZATION ( VERBID, TIMEID, LANGID, PATTERN )
	VALUES (4,1,1,'%s studies at %s');
INSERT INTO VERBALIZATION ( VERBID, TIMEID, LANGID, PATTERN )
	VALUES (5,1,1,'%s speaks %s');
INSERT INTO VERBALIZATION ( VERBID, TIMEID, LANGID, PATTERN )
	VALUES (6,1,1,'%s lives at %s');
INSERT INTO VERBALIZATION ( VERBID, TIMEID, LANGID, PATTERN )
	VALUES (7,1,1,'%s is originally from %s');
INSERT INTO VERBALIZATION ( VERBID, TIMEID, LANGID, PATTERN )
	VALUES (8,1,1,'%s is a fan of %s');
INSERT INTO VERBALIZATION ( VERBID, TIMEID, LANGID, PATTERN )
	VALUES (9,1,1,'%s tagged %s in %s');
INSERT INTO VERBALIZATION ( VERBID, TIMEID, LANGID, PATTERN )
	VALUES (10,1,1,'%s likes %s');
INSERT INTO VERBALIZATION ( VERBID, TIMEID, LANGID, PATTERN )
	VALUES (11,1,1,'%s commented on %s');
INSERT INTO VERBALIZATION ( VERBID, TIMEID, LANGID, PATTERN )
	VALUES (1,1,2,'%s y %s son amigos');
INSERT INTO VERBALIZATION ( VERBID, TIMEID, LANGID, PATTERN )
	VALUES (3,2,1,'%s worked at %s
');
