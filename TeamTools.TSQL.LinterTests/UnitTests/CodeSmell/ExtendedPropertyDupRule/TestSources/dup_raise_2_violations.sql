EXEC sp_addextendedproperty
    @name = N'MS_Description'
    , @value = N'Some description'
    , @level0type = N'SCHEMA'
    , @level0name = N'my_schema'
    , @level1type = N'TABLE'
    , @level1name = N'my_table'
    , @level2type = N'COLUMN'
    , @level2name = N'my_column';
GO

EXEC sp_addextendedproperty
    @name = N'MS_Description'           -- 1
    , @value = N'Some description'
    , @level0type = N'SCHEMA'
    , @level0name = N'my_schema'
    , @level1type = N'TABLE'
    , @level1name = N'my_table'
    , @level2type = N'COLUMN'
    , @level2name = N'my_column';
GO

EXEC sp_addextendedproperty
    @name = N'Custom property'
    , @value = N'adsf'
    , @level0type = N'ASSEMBLY'
    , @level0name = N'some_assm'
GO

EXEC sp_addextendedproperty
    @name = N'Custom property'          -- 2
    , @value = N'xxx'
    , @level0type = N'ASSEMBLY'
    , @level0name = N'some_assm'
