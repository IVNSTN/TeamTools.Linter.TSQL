-- A default constraint existence check (OBJECT_ID type 'D') is normalized to the
-- constraint family ('C'). A subsequent ALTER TABLE DROP CONSTRAINT must be treated
-- as guarded, so no violations are expected.
IF OBJECT_ID('dbo.foo', 'D') IS NOT NULL
    ALTER TABLE dbo.foo DROP CONSTRAINT foo;
GO