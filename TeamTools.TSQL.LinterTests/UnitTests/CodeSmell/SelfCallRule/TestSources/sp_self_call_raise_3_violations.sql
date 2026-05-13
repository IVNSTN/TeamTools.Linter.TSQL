create proc dbo.foo
as
begin
    exec foo

    exec [dbo].[foo]
end
GO

create proc far.bar;3
as
begin
    exec [far].bar;3
end
GO
