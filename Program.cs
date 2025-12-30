using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// =================== add data   start===================
app.MapPost("/student", (Student student) =>
{
    using SqlConnection con = new SqlConnection(connectionString);
    con.Open();
    // test

    string query = "INSERT INTO Students (Name,Age,Email,City) VALUES (@Name, @Age,@Email,@City)";
    using SqlCommand cmd = new SqlCommand(query, con);
    cmd.Parameters.AddWithValue("@Name", student.Name);
    cmd.Parameters.AddWithValue("@Age", student.Age);
    cmd.Parameters.AddWithValue("@Email", student.Email);
    cmd.Parameters.AddWithValue("@City", student.City);

    cmd.ExecuteNonQuery();

    return Results.Ok("Student inserted successfully");
});
// =================== add data end ===================
// =================== GET ALL  start===================
// GET ALL STUDENTS
app.MapGet("/students", () =>
{
    List<object> list = new();

    using (SqlConnection con = new SqlConnection(connectionString))
    {
        con.Open();
        using (SqlCommand cmd = new SqlCommand("SELECT * FROM Students", con))
        {
            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    list.Add(new
                    {
                        Id = dr.GetInt32(0),
                        Name = dr.GetString(1),
                        Age = dr.GetInt32(2)
                       // Email = dr.GetString(3)
                       // City = dr.GetString(4)
                    });
                }
            }
        }
    }

    return Results.Ok(list);
});
// =================== GET ALL  end===================
// =================== GET SELECT by ID endpoint start===================
app.MapGet("/students/{id}", (int id) =>
{
    object result = null;

    using (SqlConnection con = new SqlConnection(connectionString))
    {
        con.Open();

        using (SqlCommand cmd =
            new SqlCommand("SELECT Id, Name, Age FROM Students WHERE Id=@Id", con))
        {
            cmd.Parameters.AddWithValue("@Id", id);

            using (SqlDataReader dr = cmd.ExecuteReader())
            {
                if (dr.Read())
                {
                    result = new
                    {
                        Id = dr.GetInt32(0),
                        Name = dr.GetString(1),
                        Age = dr.GetInt32(2)
                    };
                }
            }
        }
    }

    return result is null ? Results.NotFound() : Results.Ok(result);
});
// =================== SELECT by ID endpoint  end===================
// =================== UPDATE record  start===================
app.MapPut("/students/{id}", async (int id, Student updatedStudent) =>
{
    using (SqlConnection con = new SqlConnection(connectionString))
    {
        await con.OpenAsync();

        var query = @"UPDATE Students 
                      SET Name = @Name, Age = @Age 
                      WHERE Id = @Id";

        using (SqlCommand cmd = new SqlCommand(query, con))
        {
            cmd.Parameters.AddWithValue("@Id", id);
            cmd.Parameters.AddWithValue("@Name", updatedStudent.Name);
            cmd.Parameters.AddWithValue("@Age", updatedStudent.Age);

            int rows = await cmd.ExecuteNonQueryAsync();

            if (rows == 0)
                return Results.NotFound("Record not found");

            return Results.Ok("Updated successfully");
        }
    }
});
// =================== UPDATE record  end===================
// =================== DELETE record  start===================
app.MapDelete("/students/{id}", async (int id) =>
{
    using (SqlConnection con = new SqlConnection(connectionString))
    {
        await con.OpenAsync();

        var query = "DELETE FROM Students WHERE Id=@Id";

        using (SqlCommand cmd = new SqlCommand(query, con))
        {
            cmd.Parameters.AddWithValue("@Id", id);

            int rows = await cmd.ExecuteNonQueryAsync();

            if (rows == 0)
                return Results.NotFound("Record not found");

            return Results.Ok("Deleted successfully");
        }
    }
});
// =================== DELETE record  end===================
app.Run();
