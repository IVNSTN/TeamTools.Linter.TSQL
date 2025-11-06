namespace TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor
{
    public enum SqlTableType
    {
        /// <summary>
        /// normal persistent table
        /// </summary>
        Default,

        /// <summary>
        /// temp table
        /// </summary>
        TempTable,

        /// <summary>
        /// table variable
        /// </summary>
        TableVariable,

        /// <summary>
        /// UDT table type
        /// </summary>
        TypeTable,
    }
}
