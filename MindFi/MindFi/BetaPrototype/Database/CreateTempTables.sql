CREATE TABLE [tempEntities] (
  [ID] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, 
  [Name] NVARCHAR(255) NOT NULL, 
  [Type] NVARCHAR(255) NOT NULL, 
  [Active] BOOL);

CREATE INDEX [tempEntity_byName] ON [tempEntities] ([Name]);

CREATE TABLE [tempOrganizationData] (
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

CREATE INDEX [tempOrganization_byID] ON [tempOrganizationData] ([OrganizationID]);

CREATE INDEX [tempOrganization_bySNID] ON [tempOrganizationData] ([SocialNetwork], [SNID]);

CREATE TABLE [tempPersonData] (
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
  [UpdatedInSN] datetime, 
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
  CONSTRAINT [] PRIMARY KEY ([PartitionDate], [PartitionID]) ON CONFLICT ABORT);

CREATE INDEX [tempPerson_byID] ON [tempPersonData] ([PersonID]);

CREATE INDEX [tempPerson_bySNID] ON [tempPersonData] ([SocialNetwork], [SNID]);

CREATE TABLE [tempPostData] (
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
  CONSTRAINT [] PRIMARY KEY ([PartitionDate], [PartitionID]) ON CONFLICT ABORT);

CREATE INDEX [tempPost_byID] ON [tempPostData] ([PostID]);

CREATE INDEX [tempPost_bySNID] ON [tempPostData] ([SocialNetwork], [SNID]);

CREATE TABLE [tempRelationsData] (
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

CREATE INDEX [tempRelations_bySubject] ON [RelationsData] ([SubjectID]);

CREATE INDEX [tempRelations_byObject] ON [RelationsData] ([ObjectID]);


