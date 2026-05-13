-- another rule should detect undeclared variable references
EXEC sys.sp_executesql
    @script,
    @args,
    @variable;
