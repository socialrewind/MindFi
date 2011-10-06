CREATE TABLE [TagData] (
  [PartitionDate] numeric(8) NOT NULL, 
  [PartitionID] int NOT NULL, 
  [SocialNetwork] int NOT NULL, 
  [PhotoSNID] string NOT NULL, 
  [PersonSNID] string NOT NULL, 
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

