DECLARE @a CHAR(10), @b VARCHAR(100)


-- FIXME : no ImplicitTruncation violation should be generated
-- SET @a = CONVERT(VARCHAR(10), @b)
