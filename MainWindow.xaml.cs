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

        string sqlConnectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename='C:\Users\panut\Desktop\Git\IncidentReport\ProjectIncidents.mdf';Integrated Security=True";
        
        public MainWindow()
        {
            InitializeComponent();

            //Initial the application by connecting to the database then download to show all records.
            LoadDatabase();
        }

        
        /*
         * Display the current incident.
         */
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

        /*
         * Get index and display the selected incident.
         */
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

        /*
         * Clear all input boxes.
         */
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

        /*
         * Display total cost of all the projects in the list box (incident list).
         */
        private void DisplayTotalCost(List<Incident> incidentList)
        {
            decimal totalCost = 0;

            foreach (Incident incident in incidentList)
            {
                totalCost += incident.GetIncidentCost();
            }

            TextBoxTotalCost.Text = $"{totalCost,0:C2}";
        }

        /*
         * Refresh the box list from an incident list (originally desiged for the most recent list).
         */
        private void ReloadIncidentBoxList(List<Incident> incidentList)
        {
            ListBoxIncidents.Items.Clear();
            ListBoxIncidents.Items.Add(incidentList[0].IncidentPrintHeader());

            foreach (Incident incident in incidentList)
            {
                ListBoxIncidents.Items.Add(incident);
            }

            DisplayTotalCost(incidentList);
        }

        /*
         * Filter and display project names with multi-word capability.
         */
        private void ButtonApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            TextBoxErrorMessage.Text = "";                                                        //Clear error messages.

            LoadDatabase();                                                                       //Always load all database before matching with the filter.

            if (TextBoxFilter.Text != "")
            {
                Boolean foundRecord = false;
                char delimiter = ' ';
                string[] searchList = TextBoxFilter.Text.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);     //Split seaching words into a list.

                DisplayEmptyIncident();
                
                TextBoxTotalCost.Text = string.Empty;
                List<Incident> tempList = new List<Incident>();

                try
                {
                    foreach (Incident incident in incidentList)
                    {
                        if (searchList.All(word => incident.GetProjectName().Contains(word, StringComparison.OrdinalIgnoreCase)))
                        //if (searchList.Any(word => incident.GetProjectName().Contains(word, StringComparison.OrdinalIgnoreCase)))
                        {
                            tempList.Add(incident);
                            foundRecord = true;
                        }
                    }

                    if (foundRecord)
                    {
                        incidentList = tempList;
                    }
                }
                catch (Exception ex)
                {
                    TextBoxErrorMessage.Text = "An error occurred: " + ex.Message;
                }

                ReloadIncidentBoxList(incidentList);
            }
        }

        /*
         * Database (Create), add a new incident.
         */
        private void ButtonSaveNew_Click(object sender, RoutedEventArgs e)
        {
            bool validInput = false;
            validInput = ValidateInputFromTextBoxes(currentIncident);                               //Input validation from the text boxes.

            if (validInput)
            {
                DataService.CreateIncident(sqlConnectionString, currentIncident);

                LoadDatabase();
                DisplayIncidentToTextBoxes();
            }
        }

        /*
         * Database (Read), load all incidents.
         */
        private void LoadDatabase()
        {
            incidentList = DataService.ReadIncident(sqlConnectionString);

            ReloadIncidentBoxList(incidentList);                                                  //Every time database is reloaded, the list box should be reloaded also.
        }

        /*
         * Database (Update), update an incident.
         */
        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            bool validInput = false;
            validInput = ValidateInputFromTextBoxes(currentIncident);                               //Input validation from the text boxes.

            if (validInput)
            {
                DataService.UpdateIncident(sqlConnectionString, currentIncident);

                LoadDatabase();                                                                     //Reflesh the data
            }
        }
        
        /*
         * Database (Delete), delete an incident.
         */
        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if(currentIncident != null)
            {
                DataService.DeleteIncident(sqlConnectionString, currentIncident);

                LoadDatabase();                                                                     //Reflesh the data

                //Clear all the text boxes and the current incident for not allowing continuously deleting records.
                DisplayEmptyIncident();
            }
        }

        /*
         * Reflesh the data and boxlist when filter is no longer applied.
         */
        private void TextBoxFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(TextBoxFilter.Text == "")
            {
                LoadDatabase();
                TextBoxErrorMessage.Text = "";                                                      //Clear messages in the error text box.
            }
        }

        /*
         * Validate input boxes.
         * Output: validation result as true or false.
         */
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
                //Selecting only the cost number; discard the "$" sign.
                if (TextBoxIncidentCost.Text.Substring(0, 1) == "$")
                {
                    incidentCostText = TextBoxIncidentCost.Text.Substring(1);
                }
                else
                {
                    incidentCostText = TextBoxIncidentCost.Text;
                }

                //Use current time if date time is not provided or in an incorrect format.
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
