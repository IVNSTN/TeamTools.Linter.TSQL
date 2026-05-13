-- by position not supported
EXEC sp_dropextendedproperty
    N'MS_Description'
    , N'SCHEMA'
    , N'my_schema'
    , N'TABLE'
    , N'my_table'
    , N'COLUMN'
    , N'my_column';

-- db level
EXEC sp_addextendedproperty
    @name = N'MS_Description'
    , @value = N'Some description'
    , @level0type = NULL
    , @level0name = NULL
    , @level1type = NULL
    , @level1name = NULL
    , @level2type = NULL
    , @level2name = NULL;

-- all good
EXEC sp_addextendedproperty
    @name = N'MS_Description'
    , @value = N'Some description'
    , @level0type = N'SCHEMA'
    , @level0name = N'my_schema'
    , @level1type = N'TABLE'
    , @level1name = N'my_table'
    , @level2type = N'COLUMN'
    , @level2name = N'my_column';
