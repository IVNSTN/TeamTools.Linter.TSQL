CREATE TABLE foo.bar
(
    id INT NOT NULL
)
GO

EXEC sp_addextendedproperty
    @name = N'MS_Description'
    , @value = N'Some description'
    , @level0type = N'SCHEMA'
    , @level0name = N'foo'
    , @level1type = N'TABLE'
    , @level1name = N'asdfasdf'     -- different table name must be ignored
    , @level2type = N'COLUMN'
    , @level2name = N'bad_name';
