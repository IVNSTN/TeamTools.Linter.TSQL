-- compatibility level min: 130
SELECT
    Id
    , IdLogin
FROM
    OPENJSON(@DtJson, '$.CustomNotificationMessage')
        WITH (Id INT '$.IdCustomNotificationMessage', Logins NVARCHAR(MAX) '$.Logins' AS JSON)
OUTER APPLY
    OPENJSON(Logins)
        WITH (IdLogin INT '$')
