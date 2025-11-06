begin try
    select 1/0
end try
begin catch
    print 'error';
end catch
