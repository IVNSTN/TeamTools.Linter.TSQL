-- compatibility level min: 130
SELECT
    src.Id
    , sub.IdLogin
FROM
    OPENJSON(@DtJson, '$.CustomNotificationMessage')
        WITH (Id INT '$.IdCustomNotificationMessage', Logins NVARCHAR(MAX) '$.Logins' AS JSON) AS src
OUTER APPLY
    OPENJSON(src.Logins)
        WITH (IdLogin INT '$') AS sub
