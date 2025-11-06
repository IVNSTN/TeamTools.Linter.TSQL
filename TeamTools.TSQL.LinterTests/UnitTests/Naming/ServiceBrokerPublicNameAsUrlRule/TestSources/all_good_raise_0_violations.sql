CREATE MESSAGE TYPE [//domain/folder/type];
GO

CREATE CONTRACT [//domain/service/contract] ([//domain/folder/type] SENT BY INITIATOR);
GO

-- queue is ignored
CREATE QUEUE SomeQueue
    WITH STATUS = OFF;
GO

CREATE SERVICE [//domain/group/service/]
    ON QUEUE SomeQueue
    ([//domain/service/contract]);
GO
