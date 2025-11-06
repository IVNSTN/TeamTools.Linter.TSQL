SELECT
    lg.act_id
    , lg.p_code
    , lg.db_time
    , REPLACE(
        LEFT('a', 2)
        , ISNULL(NULLIF(CHARINDEX(CHAR(9), SUBSTRING(lg.text, CHARINDEX('warning:', lg.text) + 8, 12)), ''),
            (asd))
        , (SUBSTRING(lg.text, CHARINDEX('warning:', lg.text), 123))) AS nom
FROM dbo.hist_log AS lg
