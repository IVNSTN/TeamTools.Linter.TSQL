-- wrong comma position must be detected by a separate rule
exec @res = dbo.foo_foo_foo
    @bar     = 'asdfsaf'
    , @zar   = 123.01
    , @xxx   = 'asdfasdf';
