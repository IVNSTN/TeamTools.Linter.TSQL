SET @foo = 'bar
asdf';
                
SELECT @foo;

-- ;comment;

BEGIN;
    RAISERROR('test', 16, 1);
END;
GO
