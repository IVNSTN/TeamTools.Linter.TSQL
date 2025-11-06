WHILE @a = @b AND (ORIGINAL_LOGIN() != 'asfd') AND SUSER_SNAME() != 'me'
    DELETE dbo.clients
WHILE @a = @b AND (SYSTEM_USER != 'asfd') AND CURRENT_USER != 'me' AND SESSION_USER != 'me'
    DELETE dbo.clients
