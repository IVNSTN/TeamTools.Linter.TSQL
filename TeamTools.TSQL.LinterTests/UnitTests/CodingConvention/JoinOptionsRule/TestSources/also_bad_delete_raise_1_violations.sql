DELETE dbo.foo WITH(ROWLOCK)
    output deleted.far into dbo.jar(x)
FROM bar AS ba
JOIN dbo.car on car.bar_code = ba.bar_code
    AND ba.is_atctive = 'N';
