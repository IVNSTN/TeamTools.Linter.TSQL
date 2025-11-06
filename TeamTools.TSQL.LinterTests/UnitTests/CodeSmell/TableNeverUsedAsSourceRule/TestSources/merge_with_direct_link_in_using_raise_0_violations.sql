DECLARE @RazdelTypeMap TABLE (place_code VARCHAR(20) NOT NULL, CodeRazdelType VARCHAR(50) NOT NULL, PRIMARY KEY (place_code, CodeRazdelType));

MERGE dbo.trgt
USING @RazdelTypeMap map
ON map.place_code = trgt.place_code
WHEN NOT MATCHED THEN
    INSERT (place_code)
    VALUES (map.place_code);
