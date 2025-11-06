declare @a int;

set @a = 2;
begin
    update f set
        x = y
    from foo as f
    where 1=0;
end;

GOTO LBL;

return 0;

LBL:
