DECLARE @char VARCHAR(100);

SET @char = (SELECT 'a' FOR XML PATH); -- no TYPE option
