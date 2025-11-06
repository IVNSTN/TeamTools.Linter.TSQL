declare
    @VarOne int

exec my_proc
    'dummy for missing param name failure check',
    @varoNe = @VarOne,
    @zar = 'far';
