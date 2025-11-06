namespace TeamTools.TSQL.Linter.Interfaces
{
    public enum NamingNotationKind
    {
        /// <summary>
        /// something not specific
        /// </summary>
        Unknown,

        /// <summary>
        /// PascalCase
        /// </summary>
        PascalCase,

        /// <summary>
        /// camelCase
        /// </summary>
        CamelCase,

        /// <summary>
        /// snake_case
        /// </summary>
        SnakeLowerCase,

        /// <summary>
        /// SNAKE_CASE
        /// </summary>
        SnakeUpperCase,

        /// <summary>
        /// kebab-case
        /// </summary>
        KebabCase,
    }

    public interface INotationResolver
    {
        NamingNotationKind Resolve(string identifier);
    }
}
