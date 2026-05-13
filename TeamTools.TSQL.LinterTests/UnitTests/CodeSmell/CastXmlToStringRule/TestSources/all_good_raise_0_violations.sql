-- correct approach
SELECT  STUFF(
            (
                SELECT  ', ' + v.name
                FROM    (
                    VALUES
                        ('bonnie & clyde'),
                        ('thelma & louise')
                )v(NAME)
                FOR XML PATH(''), TYPE
            ).value('.', 'VARCHAR(MAX)'), 1, 2, '')

-- FOR XML AUTO
SELECT  CAST(
            (
                SELECT  ', ' + v.name
                FROM    (
                    VALUES
                        ('bonnie & clyde'),
                        ('thelma & louise')
                )v(NAME)
                FOR XML AUTO, ROOT('values'))
            AS VARCHAR(1000))

-- no FOR XML
SELECT  CONVERT(NCHAR(10), (SELECT 1))

-- escaped XML is what expected
SELECT  '<table>'
        + CAST(
            (
                SELECT  v.name as [td]
                FROM    (
                    VALUES
                        ('bonnie & clyde'),
                        ('thelma & louise')
                )v(NAME)
                FOR XML PATH('tr')
            ) AS VARCHAR(100)) + '</table>'
