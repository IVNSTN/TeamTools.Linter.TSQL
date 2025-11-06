CREATE DEFAULT foo AS 'BAR';
GO
exec sp_binddefault 'foo', 'dbo.tbl.col';
exec sp_unbinddefault 'dbo.tbl.col';
GO
CREATE RULE far AS @col IN ('BAR', 'DAR');
GO
exec sp_bindrule 'far', 'dbo.tbl.col';
exec sp_unbindrule 'dbo.tbl.col';
