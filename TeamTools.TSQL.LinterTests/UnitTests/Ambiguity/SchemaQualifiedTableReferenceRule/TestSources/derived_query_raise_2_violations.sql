IF 1=0
    SET @new_local = (SELECT InternalGroup FROM [SpecialUsersForUpdate] WHERE Login = @login)
ELSE
    SET @new_local = (SELECT InternalGroup FROM [SpecialUsersForUpdate] WHERE Login = @login)
