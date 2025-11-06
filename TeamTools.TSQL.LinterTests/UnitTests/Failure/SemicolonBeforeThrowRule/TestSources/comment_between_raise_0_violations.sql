BEGIN; -- comment
    THROW 500001, 'test', 1;
END;

BEGIN; /*
comment
*/
    THROW 500001, 'test', 1;
END;
