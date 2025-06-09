using Microsoft.Data.SqlClient;

namespace simple_api;

public class DataAccessLayer
{
    public bool UpdateUserInformation(UserInformation username)
    {
        var connection = new SqlConnection("ConnectionString");

        var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM UserInformation WHERE Username = '{username.Username}'";
        command.Connection.Open();
        var rows = command.ExecuteNonQuery();
        return rows == 1;
    }
}

public class UserInformation
{
    public string Username { get; set; }
    public string FirstName {get; set;}
    public string LastName {get; set;}
}