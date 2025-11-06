CREATE MESSAGE TYPE MyType;
GO

CREATE CONTRACT MyContract (MyType SENT BY INITIATOR);
GO

-- queue is ignored
CREATE QUEUE SomeQueue
    WITH STATUS = OFF;
GO

CREATE SERVICE MyService
    ON QUEUE SomeQueue
    (MyContract);
GO
