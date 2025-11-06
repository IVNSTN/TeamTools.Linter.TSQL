CREATE FUNCTION dbo.fn (@arg INT)
RETURNS @res TABLE (id INT)
AS
BEGIN
    INSERT @res(id)
    VALUES (@arg)

    RETURN -- simple return in the end
END;
GO

CREATE FUNCTION dbo.return_from_all_conditional_paths(@arg INT)
RETURNS INT
AS
BEGIN
    SET @arg += 1;

    IF @arg > 0
        RETURN 1; -- all paths have RETURN
    ELSE
    BEGIN
        RETURN -1 -- all paths have RETURN
    END
END;
GO

CREATE FUNCTION dbo.multiple_nested_ifs (@arg INT)
RETURNS INT
AS
BEGIN
    SET @arg += 1;

    IF @arg > 0
        IF @arg > 1
            RETURN 1;
        ELSE
        BEGIN
            IF @arg > 2
                RETURN 2
            ELSE
                RETURN 3
        END
    ELSE IF @arg < -1
    BEGIN
        IF @arg < -10
            SET @arg = 0

        RETURN 4 -- not under condition
    END
    ELSE
    BEGIN
        BEGIN
            RETURN -1
        END
    END
END;
GO
