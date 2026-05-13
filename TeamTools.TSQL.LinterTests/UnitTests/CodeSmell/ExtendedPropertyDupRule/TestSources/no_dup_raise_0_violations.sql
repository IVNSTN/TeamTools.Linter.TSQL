EXEC sp_addextendedproperty
    @name = N'MS_Description'
    , @value = N'Some description'
    , @level0type = N'SCHEMA'
    , @level0name = N'my_schema'
    , @level1type = N'TABLE'
    , @level1name = N'my_table'
    , @level2type = N'COLUMN'
    , @level2name = N'my_column';

EXEC sp_addextendedproperty
    @name = N'MS_Description'
    , @value = N'Some description'
    , @level0type = N'SCHEMA'
    , @level0name = N'my_schema'
    , @level1type = N'TABLE'
    , @level1name = N'my_table'
    , @level2type = N'COLUMN'
    , @level2name = N'another_column'; -- different col

EXEC sp_addextendedproperty
    @name = N'Custom property'
    , @value = N'adsf'
    , @level0type = N'ASSEMBLY'
    , @level0name = N'some_assm'

EXEC sp_addextendedproperty
    @name = N'Additional property'          -- different property
    , @value = N'xxx'
    , @level0type = N'ASSEMBLY'
    , @level0name = N'some_assm'
