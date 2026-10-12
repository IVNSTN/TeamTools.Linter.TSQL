declare @sp sysname, @ret_code int
exec @sp -- var name - ignored

exec my_proc -- unknown proc

exec my_schema.sp_getapplock -- not sys procs in used-defined schema

exec sp_addextendedproperty -- explicitly ignored

exec @ret_code = sp_getapplock -- ok
