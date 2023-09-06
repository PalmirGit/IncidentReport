using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IncidentReport
{
    public partial class MainWindow : Window
    {
        private List<Incident> incidentList = new List<Incident>();
        private Incident currentIncident = new Incident();
        private int currentIndex;
        private string projectNameFilter = "";

        /******* IMPORTANT *****
         * Tutorial videos indicate that relative path of SQL Connection String can not be used.
         * Please consider changing the SQL Connection String below if this application running path is changed.
         * String Location: click "ConnectionName".mdf, then copy Connection String from Properties.
         */
        //Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename="C:\Users\panut\Desktop\COMP255 (Visual Application Development)\Csharp App\255FPTavilsup\ProjectIncidents.mdf";Integrated Security=True
        string sqlConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='C:\Users\panut\Desktop\Git\IncidentReport\ProjectIncidents.mdf';Integrated Security=True";
        
        public MainWindow()
        {
            InitializeComponent();

            //Initial the application by connecting to the database then download to show all records.
            LoadDatabase("");
        }

        private void ListBoxIncidents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBoxErrorMessage.Text = "";

            currentIndex = ListBoxIncidents.SelectedIndex - 1;                                      //Index 0 of the list box is the data header. 
            if (currentIndex >= 0)
            {
                currentIncident = (Incident)ListBoxIncidents.SelectedItem;                          //Convert the selected item into a class.
            }
            
            DisplayIncidentToTextBoxes();
        }
        private void DisplayIncidentToTextBoxes()
        {
            if (currentIncident != null)
            {
                TextBoxIncidentID.Text = currentIncident.GetIncidentId().ToString();
                TextBoxIncidentDate.Text = currentIncident.GetIncidentDate().ToShortDateString();
                TextBoxProjectName.Text = currentIncident.GetProjectName();
                TextBoxCompanyName.Text = currentIncident.GetVendorCompanyName();
                TextBoxContactName.Text = currentIncident.GetVendorContactName();
                TextBoxContactEmail.Text = currentIncident.GetVendorContactEmail();
                TextBoxIncidentCost.Text = $"{currentIncident.GetIncidentCost(),0:C2}";
                TextBoxIncidentDescription.Text = currentIncident.GetIncidentDescription();
            }
            else
            {
                DisplayEmptyIncident();
            }
        }
        private void DisplayEmptyIncident()
        {
            TextBoxIncidentID.Text = string.Empty;
            TextBoxIncidentDate.Text = string.Empty;
            TextBoxProjectName.Text = string.Empty;
            TextBoxCompanyName.Text = string.Empty;
            TextBoxContactName.Text = string.Empty;
            TextBoxContactEmail.Text = string.Empty;
            TextBoxIncidentCost.Text = string.Empty;
            TextBoxIncidentDescription.Text = string.Empty;

            currentIncident = null;                                                                 //Deselect current item.
        }

        private void ButtonApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            TextBoxErrorMessage.Text = "";                                                          //Clear error messages.

            LoadDatabase("");                                                                       //Always load all database before matching with the filter.

            if (TextBoxFilter.Text != "")
            {
                Boolean foundRecord = false;
                char delimiter = ' ';
                string[] searchList = TextBoxFilter.Text.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);

                DisplayEmptyIncident();
                ListBoxIncidents.Items.Clear();
                ListBoxIncidents.Items.Add(incidentList[0].IncidentPrintHeader());
                TextBoxTotalCost.Text = string.Empty;
                List<Incident> tempList = new List<Incident>();

                try
                {
                    foreach (Incident incident in incidentList)
                    {
                        //if (searchList.All(word => incident.GetProjectName().Contains(word, StringComparison.OrdinalIgnoreCase)))
                        if (searchList.Any(word => incident.GetProjectName().Contains(word, StringComparison.OrdinalIgnoreCase)))
                        {
                            tempList.Add(incident);
                            ListBoxIncidents.Items.Add(incident);
                            foundRecord = true;
                        }
                    }

                    if (foundRecord)
                    {
                        incidentList = tempList;
                        DisplayTotalCost();
                    }
                }
                catch (ArgumentException argEx)
                {
                    TextBoxErrorMessage.Text = "Invalid regular expression pattern: " + argEx.Message;
                }
                catch (Exception ex)
                {
                    TextBoxErrorMessage.Text = "An error occurred: " + ex.Message;
                }
            }
        }

        private void LoadDatabase(string loadOption)                                                //Bioption database loader.
        {
            incidentList.Clear();
            ListBoxIncidents.Items.Clear();
            string sqlQuery;

            /**
              * 4 Steps of Database Connection Layers:
              * 1. Allocate, configure, and open your connection object.
              * 2. Allocate and configure a command object, specifying the connection object as a constructor argument or with the Connection property.
              * 3. Call ExecuteReader() on the configured command class.
              * 4. Process each record using the Read() method of the data reader.
              */
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))               //Step 1. Using "using" for a temporary object; to be cleared out after finish.
            {
                connection.Open();

                if (loadOption != "")
                {
                    sqlQuery = $"SELECT * FROM Incidents WHERE ProjectName = '{loadOption}';";      // 1st Option: download records with a Project Name filter.
                }
                else
                {
                    sqlQuery = "SELECT * FROM Incidents;";                                          // 2nd Option: download all records.
                }

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))                   //Step 2.
                {
                    using (SqlDataReader reader = command.ExecuteReader())                          //Step 3.
                    {
                        Incident newIncident = new Incident();
                        ListBoxIncidents.Items.Add(newIncident.IncidentPrintHeader());

                        while (reader.Read())                                                       //Step 4.
                        {
                            newIncident = new Incident((int)reader["IncidentId"],
                                                       (DateTime)reader["IncidentDate"],
                                                       (string)reader["ProjectName"],
                                                       (string)reader["VendorCompanyName"],
                                                       (string)reader["VendorContactName"],
                                                       (string)reader["VendorContactEmail"],
                                                       (decimal)reader["IncidentCost"],
                                                       (string)reader["IncidentDescription"]);
                            
                            incidentList.Add(newIncident);
                            ListBoxIncidents.Items.Add(newIncident);
                        }
                    }
                }

                connection.Close();
            }
            DisplayTotalCost();
        }

        private void DisplayTotalCost()
        {
            decimal totalCost = 0;

            //Sum the total cost with the current records in the downloaded list only.
            for (int i=0; i<incidentList.Count; i++)
            {
                totalCost += incidentList[i].GetIncidentCost();
            }

            TextBoxTotalCost.Text = $"{totalCost,0:C2}";
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            bool validInput = false;
            validInput = ValidateInputFromTextBoxes(currentIncident);                               //Input validation from the text boxes.

            if (validInput)
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

                LoadDatabase(projectNameFilter);
            }
        }

        private void ButtonSaveNew_Click(object sender, RoutedEventArgs e)
        {
            bool validInput = false;
            validInput = ValidateInputFromTextBoxes(currentIncident);                               //Input validation from the text boxes.

            if (validInput)
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

                LoadDatabase(projectNameFilter);
                DisplayIncidentToTextBoxes();
            }
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if(currentIncident != null)
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

                LoadDatabase(projectNameFilter);
                
                //Clear all the text boxes and the current incident for not allowing continuously deleting records.
                DisplayEmptyIncident();
            }
        }

        private void TextBoxFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(TextBoxFilter.Text == "")
            {
                //Refresh the list box by re-download all the record from the database. This helps user to know Project Names for the next filtering.
                projectNameFilter = "";
                LoadDatabase(projectNameFilter);
                
                TextBoxErrorMessage.Text = "";
            }
        }

        private Boolean ValidateInputFromTextBoxes(Incident incidentObject)
        {
            TextBoxErrorMessage.Text = "";
            decimal incidentCost;
            string incidentCostText;
            DateTime incidentDate;

            if (TextBoxIncidentCost.Text == "")
            {
                TextBoxErrorMessage.Text = "Invalid Input!";
                return false;
            }
            else
            {
                //Selecting only the number; discard the "$" sign.
                if (TextBoxIncidentCost.Text.Substring(0, 1) == "$")
                {
                    incidentCostText = TextBoxIncidentCost.Text.Substring(1);
                }
                else
                {
                    incidentCostText = TextBoxIncidentCost.Text;
                }

                if (!DateTime.TryParse(TextBoxIncidentDate.Text, out incidentDate))
                {
                    incidentDate = DateTime.Now;
                }

                //Main data validation.
                if (TextBoxProjectName.Text != "" &&
                    TextBoxCompanyName.Text != "" &&
                    TextBoxContactName.Text != "" &&
                    TextBoxContactEmail.Text != "" &&
                    decimal.TryParse(incidentCostText, out incidentCost))
                {
                    incidentObject.SetIncidentDate(incidentDate);
                    incidentObject.SetProjectName(TextBoxProjectName.Text);
                    incidentObject.SetVendorCompanyName(TextBoxCompanyName.Text);
                    incidentObject.SetVendorContactName(TextBoxContactName.Text);
                    incidentObject.SetVendorContactEmail(TextBoxContactEmail.Text);
                    incidentObject.SetIncidentCost(incidentCost);
                    incidentObject.SetIncidentDescription(TextBoxIncidentDescription.Text);

                    return true;
                }
                else
                {
                    TextBoxErrorMessage.Text = "Invalid Input!";
                    return false;
                }
            }
        }
    }
}
