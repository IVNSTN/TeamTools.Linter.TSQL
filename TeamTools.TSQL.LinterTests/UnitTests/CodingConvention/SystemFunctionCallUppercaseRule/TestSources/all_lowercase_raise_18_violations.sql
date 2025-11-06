select
    upper(lower(left(right(coalesce(nullif('', ''), isnull('x', 'y')), 1), 1)))
    , round(cast(convert(numeric(10,2), cast(convert(numeric(10,2), replace('0', ' ', '')) as numeric(10,2))) as numeric(10,2)), 0)
    , dateadd(ms, datediff(y, getdate(), 0), sysdatetime())
    , year /* fn */ ()
