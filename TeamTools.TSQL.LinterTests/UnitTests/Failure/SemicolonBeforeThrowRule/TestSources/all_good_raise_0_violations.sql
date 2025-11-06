SET @foo = 'bar
asdf';

SELECT @foo
;THROW 500001, 'test', 1;

BEGIN;
    THROW 500001, 'test', 1;
END;
