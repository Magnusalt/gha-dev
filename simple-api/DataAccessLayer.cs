using Microsoft.Data.SqlClient;

namespace simple_api;

public class DataAccessLayer
{
    public UserInformation? GetUserInformation(string username)
    {
        var connection = new SqlConnection("ConnectionString");

        var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM UserInformation WHERE Username = '" + username + "'";
        command.Connection.Open();
        var reader = command.ExecuteReader();
        if (reader.HasRows && reader.Read())
        {
            return new UserInformation
            {
                Username = reader.GetString(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2)
            };
        }
        return null;
    }
}

public class UserInformation
{
    public string Username { get; set; }
    public string FirstName {get; set;}
    public string LastName {get; set;}
}