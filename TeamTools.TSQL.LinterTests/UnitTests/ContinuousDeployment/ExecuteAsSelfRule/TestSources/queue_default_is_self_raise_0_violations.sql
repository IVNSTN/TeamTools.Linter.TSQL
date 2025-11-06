CREATE QUEUE dbo.far -- SELF is the default
GO

CREATE QUEUE foo.bar
    WITH
        STATUS = ON
        , POISON_MESSAGE_HANDLING (STATUS = OFF);
GO
