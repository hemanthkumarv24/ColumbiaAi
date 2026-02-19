using System;
using System.Data.SqlClient;

public class CodeQLValidationController
{
    public void GetUser(string userInput)
    {
        // ❌ Intentional SQL Injection vulnerability for CodeQL validation
        string query = "SELECT * FROM Users WHERE Name = '" + userInput + "'";

        using (var connection = new SqlConnection("Server=.;Database=TestDb;Trusted_Connection=True;"))
        {
            connection.Open();
            var command = new SqlCommand(query, connection);
            command.ExecuteReader();
        }
    }
}

public class SecretsTest
{
    // ❌ Hardcoded secret (intentional)
    private const string ApiKey = "SuperSecretProductionKey123!";
}



public class FileTest
{
    public string ReadFile(string fileName)
    {
        // ❌ Vulnerable: user controls file path
        return File.ReadAllText("/app/data/" + fileName);
    }
}
