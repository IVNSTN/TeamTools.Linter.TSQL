namespace TeamTools.TSQL.Linter.Routines.TableDefinitionExtractor
{
    public enum SqlTableElementType
    {
        /// <summary>
        /// PK
        /// </summary>
        PrimaryKey,

        /// <summary>
        /// FK
        /// </summary>
        ForeignKey,

        /// <summary>
        /// CK
        /// </summary>
        CheckConstraint,

        /// <summary>
        /// UQ
        /// </summary>
        UniqueConstraint,

        /// <summary>
        /// DF
        /// </summary>
        DefaultConstraint,

        /// <summary>
        /// IDX
        /// </summary>
        Index,
    }
}
