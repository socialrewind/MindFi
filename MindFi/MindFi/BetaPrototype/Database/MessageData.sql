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
  [CommentCount] INT, 
  CONSTRAINT [] PRIMARY KEY ([PartitionDate], [PartitionID]) ON CONFLICT ABORT);

CREATE INDEX [Message_byID] ON [MessageData] ([MessageID]);

CREATE INDEX [Message_bySNID] ON [MessageData] ([SocialNetwork], [SNID]);


