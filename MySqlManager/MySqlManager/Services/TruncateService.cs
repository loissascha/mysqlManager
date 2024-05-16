namespace MySqlManager.Services;

public class TruncateService
{
    public void TruncateTable(string database, string table)
    {
        /*
         * USE databaseName;
         * SET FOREIGN_KEY_CHECKS = 0;
         * TRUNCATE tableName;
         * SET FOREIGN_KEY_CHECKS = 1;
         */
    }
}