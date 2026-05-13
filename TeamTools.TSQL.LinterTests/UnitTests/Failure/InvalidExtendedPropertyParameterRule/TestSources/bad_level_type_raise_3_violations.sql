EXEC sp_addextendedproperty
    @name = N'MS_Description'
    , @value = N'Some description'
    , @level0type = N'UNKNOWN'      -- 1
    , @level0name = N'my_schema'
    , @level1type = N'TABLE'
    , @level1name = N'my_table'
    , @level2type = N'COLUMN'
    , @level2name = N'my_column';

EXEC sp_updateextendedproperty
    @name = N'MS_Description'
    , @value = N'Some description'
    , @level0type = N'SCHEMA'
    , @level0name = N'my_schema'
    , @level1type = N'UNKNOWN'      -- 2
    , @level1name = N'my_table'
    , @level2type = N'COLUMN'
    , @level2name = N'my_column';

EXEC sp_dropextendedproperty
    @name = N'MS_Description'
    , @level0type = N'SCHEMA'
    , @level0name = N'my_schema'
    , @level1type = N'TABLE'
    , @level1name = N'my_table'
    , @level2type = N'UNKNOWN'      -- 3
    , @level2name = N'my_column';
