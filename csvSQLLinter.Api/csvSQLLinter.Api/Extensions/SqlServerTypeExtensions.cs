namespace csvSQLLinter.Api.Extensions
{
    public enum SqlServerType
    {
        Int,
        Varchar,
        Nvarchar,
        DateTime,
        Float,
        Decimal
    }
    public static class SqlServerTypeExtensions
    {
        public static Type ToSystemType(this SqlServerType sqlType)
        {
            switch (sqlType)
            {
                case SqlServerType.Int:
                    return typeof(int);
                case SqlServerType.Varchar:
                case SqlServerType.Nvarchar:
                    return typeof(string);
                case SqlServerType.DateTime:
                    return typeof(DateTime);
                case SqlServerType.Float:
                    return typeof(float);
                case SqlServerType.Decimal:
                    return typeof(decimal);
                default:
                    throw new ArgumentOutOfRangeException(nameof(sqlType), $"Unsupported SQL Server type: {sqlType}");
            }
        }
    }
}
