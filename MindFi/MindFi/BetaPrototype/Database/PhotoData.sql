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


