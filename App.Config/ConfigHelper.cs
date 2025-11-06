namespace App.Config
{
    public class ConfigHelper
    {
        private static string ConnectionString = string.Empty;
        private static string ConnectionStringLogDB = string.Empty;
        public static string Version = "03.06.17";


        public static string? GetConnectionVersion(string? dbName)
        {
            if (dbName.Equals("DB"))
                return ConnectionString.Substring(0, ConnectionString.IndexOf(";Tr"));
            else if (dbName.Equals("LogDB"))
                return ConnectionStringLogDB.Substring(0, ConnectionStringLogDB.IndexOf(";Tr"));

            throw new Exception("Not Found DBName (GetConnectionString)");
        }
        public static string? GetConnectionString(string? dbName)
        {
            if (dbName.Equals("DB"))  // -----------------------------------------------------------------
            {
                if (!string.IsNullOrEmpty(ConnectionString))
                    return ConnectionString;


#if  DEBUG
                if (Environment.MachineName == "DESKTOP-6AB411M")
                    ConnectionString = "Server=.;Database= ;TrustServerCertificate=True;Trusted_Connection=false;User ID= ;Password= ;";
                else
                    ConnectionString = "Server= ;Database= ;TrustServerCertificate=True;Trusted_Connection=false;User ID= ;Password= ;";


#else
                if (Environment.MachineName == "FARAZ")
                    ConnectionString = "Server=.;Database= ;TrustServerCertificate=True;Trusted_Connection=false;User ID= ;Password=  ;";
                else
                    ConnectionString = "Server=. ;Database= ;TrustServerCertificate=True;Trusted_Connection=false;User ID= ;Password=  ;";
#endif
                return ConnectionString;
            }


            throw new Exception($"Not Found DBName ({dbName}:GetConnectionString)");
        }



        //-----------------------------  Log ---------------------------------------------------------------
        public static string GetConnectionStringLogDB()
        {
            if (!string.IsNullOrEmpty(ConnectionStringLogDB))
                return ConnectionStringLogDB;


#if   DEBUG
            if (Environment.MachineName == "DESKTOP-6AB411M")
                ConnectionStringLogDB = "Server=.;Database= ;TrustServerCertificate=True;Trusted_Connection=false;User ID=sa;Password= -;";
            else
                ConnectionStringLogDB = "Server= ;Database= ;TrustServerCertificate=True;Trusted_Connection=false;User ID= ;Password= -;";

#else
            if (Environment.MachineName == "FARAZ")
                ConnectionStringLogDB = "Server=.;Database= ;TrustServerCertificate=True;Trusted_Connection=false;User ID= ;Password= -;";
            else
                ConnectionStringLogDB = "Server= ;Database= ;TrustServerCertificate=True;Trusted_Connection=false;User ID= ;Password=* ;";
#endif


            return ConnectionStringLogDB;
        }

    }

}