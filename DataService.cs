using System;
using System.Collections.Generic;
using System.Data.SqlClient;        // For SqlConnection, SqlCommand, SqlDataReader.
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncidentReport
{
    public static class DataService
    {
        public static void CreateIncident(string sqlConnectionString, Incident currentIncident)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                /* First query : find the maximum number of the primary key */
                string sqlQuery = "SELECT Max(IncidentId) FROM Incidents;";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    //"Select" command execution returns a first column first row value from the query which is Max(IncidentId);
                    int newIncidentId = (int)command.ExecuteScalar() + 1;                       //Add 1 for increasing the database by 1 record.

                    currentIncident.SetIncidentId(newIncidentId);                               //Complete an object for INSERT command below.
                }

                /* Second query : insert new data to a new number of the primary key */
                sqlQuery = $"INSERT INTO Incidents " +
                           "(IncidentId, IncidentDate, ProjectName, VendorCompanyName, VendorContactName, VendorContactEmail, IncidentCost, IncidentDescription)" +
                           "VALUES (" +
                           $"{currentIncident.GetIncidentId()}, " +
                           $"'{currentIncident.GetIncidentDate()}', " +
                           $"'{currentIncident.GetProjectName()}', " +
                           $"'{currentIncident.GetVendorCompanyName()}', " +
                           $"'{currentIncident.GetVendorContactName()}', " +
                           $"'{currentIncident.GetVendorContactEmail()}', " +
                           $"'{currentIncident.GetIncidentCost()}', " +
                           $"'{currentIncident.GetIncidentDescription()}');";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.ExecuteNonQuery();                                                  //"Insert" command execution statement.
                }

                connection.Close();
            }
        }

        public static List<Incident> ReadIncident(string sqlConnectionString)
        {
            string sqlQuery = "SELECT * FROM Incidents;";
            List<Incident> dataList = new List<Incident>();

            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        Incident newIncident = new Incident();

                        while (reader.Read())
                        {
                            newIncident = new Incident((int)reader["IncidentId"],
                                                       (DateTime)reader["IncidentDate"],
                                                       (string)reader["ProjectName"],
                                                       (string)reader["VendorCompanyName"],
                                                       (string)reader["VendorContactName"],
                                                       (string)reader["VendorContactEmail"],
                                                       (decimal)reader["IncidentCost"],
                                                       (string)reader["IncidentDescription"]);
                            
                            dataList.Add(newIncident);
                        }
                    }
                }

                connection.Close();
            }

            return dataList;
        }

        public static void UpdateIncident(string sqlConnectionString, Incident currentIncident)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                string sqlQuery = $"UPDATE Incidents SET " +
                                  $"IncidentDate = '{currentIncident.GetIncidentDate()}', " +
                                  $"ProjectName = '{currentIncident.GetProjectName()}', " +
                                  $"VendorCompanyName = '{currentIncident.GetVendorCompanyName()}', " +
                                  $"VendorContactName = '{currentIncident.GetVendorContactName()}', " +
                                  $"VendorContactEmail = '{currentIncident.GetVendorContactEmail()}', " +
                                  $"IncidentCost = '{currentIncident.GetIncidentCost()}', " +
                                  $"IncidentDescription = '{currentIncident.GetIncidentDescription()}' " +
                                  $"WHERE IncidentId = {currentIncident.GetIncidentId()};";     //Update to the specific primary key number.

                using (SqlCommand Command = new SqlCommand(sqlQuery, connection))
                {
                    Command.ExecuteNonQuery();                                                  //"Update" command execution statement.
                }

                connection.Close();
            }
        }

        public static void DeleteIncident(string sqlConnectionString, Incident currentIncident)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                string sqlQuery = $"DELETE FROM Incidents WHERE IncidentId = {currentIncident.GetIncidentId()};";   //Delete at a specific primary key number.

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.ExecuteNonQuery();                                                  //"Delete" command execution statement.
                }

                connection.Close();
            }
        }
    }
}

