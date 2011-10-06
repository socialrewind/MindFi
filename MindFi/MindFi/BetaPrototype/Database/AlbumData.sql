CREATE TABLE [AlbumData] (
  [PartitionDate] numeric(8) NOT NULL, 
  [PartitionID] int NOT NULL, 
  [AlbumID] int NOT NULL, 
  [SocialNetwork] int NOT NULL, 
  [SNID] string NOT NULL, 
  [Active] bit NOT NULL, 
  [LastUpdate] datetime NOT NULL, 
  [PreviousDate] numeric(8) NOT NULL, 
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
  CONSTRAINT [] PRIMARY KEY ([PartitionDate], [PartitionID]) ON CONFLICT ABORT);

CREATE INDEX [Album_byID] ON [AlbumData] ([AlbumID]);

CREATE INDEX [Album_bySNID] ON [AlbumData] ([SocialNetwork], [SNID]);


