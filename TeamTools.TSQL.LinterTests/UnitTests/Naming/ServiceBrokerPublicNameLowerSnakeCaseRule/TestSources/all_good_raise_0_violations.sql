CREATE MESSAGE TYPE my_type;
GO

CREATE CONTRACT my_contract (my_type SENT BY INITIATOR);
GO

-- queue is ignored
CREATE QUEUE SomeQueue
    WITH STATUS = OFF;
GO

CREATE SERVICE my_service
    ON QUEUE SomeQueue
    (my_contract);
GO
