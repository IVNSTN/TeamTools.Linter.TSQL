SELECT  STUFF(
            (
                SELECT  ', ' + v.name
                FROM    (
                    VALUES
                        ('bonnie & clyde'),
                        ('thelma & louise')
                )v(NAME)
                FOR XML PATH(''), TYPE
            ), 1, 2, '') -- 1

SELECT  CAST(
            (
                SELECT  ', ' + v.name
                FROM    (
                    VALUES
                        ('bonnie & clyde'),
                        ('thelma & louise')
                )v(NAME)
                FOR XML PATH('')
            ) AS VARCHAR(100)) -- 2
