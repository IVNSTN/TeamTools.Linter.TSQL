insert into bar(a)
exec dbo.foo;

insert into bar(a)
SELECT * FROM 
openquery([zar], 'exec dbo.foo');
