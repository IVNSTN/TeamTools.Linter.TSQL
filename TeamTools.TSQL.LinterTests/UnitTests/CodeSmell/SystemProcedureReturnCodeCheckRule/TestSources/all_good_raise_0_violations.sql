declare @sp sysname, @ret_code int
exec @sp -- var name - ignored

exec my_proc -- not sys proc - ignored

exec @ret_code = sp_getapplock -- ok
