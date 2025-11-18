DECLARE
    @foo INT = (1 - 2
    * 3)
    , @bar VARCHAR(10) = 'aasdf
                            asdf
                                xcvzxv'
    , @dt DATETIME = (
        SELECT next_date
        FROM calendar
        WHERE dt = DATEADD(DD, -1, GETDATE())
    )
