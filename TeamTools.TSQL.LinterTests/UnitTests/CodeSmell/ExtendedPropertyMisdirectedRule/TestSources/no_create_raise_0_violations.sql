EXEC sp_addextendedproperty
    @name = N'MS_Description'
    , @value = N'Some description'
    , @level0type = N'SCHEMA'
    , @level0name = N'my_schema'
    , @level1type = N'TABLE'
    , @level1name = N'my_table'
    , @level2type = N'COLUMN'
    , @level2name = N'my_column';
