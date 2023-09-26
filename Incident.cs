using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IncidentReport
{
    public class Incident
    {
        private int incidentID;
        private DateTime incidentDate;
        private string projectName, vendorCompanyName, vendorContactName, vendorContactEmail, incidentDescription;
        private decimal incidentCost;

        public Incident()
        {
            incidentID = 0;
            incidentDate = DateTime.Today;
            projectName = string.Empty;
            vendorCompanyName = string.Empty;
            vendorContactName = string.Empty;
            vendorContactEmail = string.Empty;
            incidentCost = 0;
            incidentDescription = string.Empty;
        }
        public Incident(int incidentID, DateTime incidentDate, string projectName,
                       string vendorCompanyName, string vendorContactName, string vendorContactEmail,
                       decimal incidentCost, string incidentDescription)
        {
            this.incidentID = incidentID;
            this.incidentDate = incidentDate;
            this.projectName = projectName;
            this.vendorCompanyName = vendorCompanyName;
            this.vendorContactName = vendorContactName;
            this.vendorContactEmail = vendorContactEmail;
            this.incidentCost = incidentCost;
            this.incidentDescription = incidentDescription;
        }
        
        /* IncidentId Field */
        public int GetIncidentId()
        {
            return incidentID;
        }
        public void SetIncidentId(int incidentID)
        {
            this.incidentID = incidentID;
        }
        
        /* IncidentDate Field */
        public DateTime GetIncidentDate()
        {
            return incidentDate;
        }
        public void SetIncidentDate(DateTime incidentDate)
        {
            this.incidentDate = incidentDate;
        }
        
        /* ProjectName Field */
        public string GetProjectName()
        {
            return projectName;
        }
        public void SetProjectName(string projectName)
        {
            this.projectName = projectName;
        }
        
        /* VendorCompanyName Field */
        public string GetVendorCompanyName()
        {
            return vendorCompanyName;
        }
        public void SetVendorCompanyName(string vendorCompanyName)
        {
            this.vendorCompanyName = vendorCompanyName;
        }
        
        /* VendorContactName Field */
        public string GetVendorContactName()
        {
            return vendorContactName;
        }
        public void SetVendorContactName(string vendorContactName)
        {
            this.vendorContactName = vendorContactName;
        }
        
        /* VendorContactEmail Field */
        public string GetVendorContactEmail()
        {
            return vendorContactEmail;
        }
        public void SetVendorContactEmail(string vendorContactEmail)
        {
            this.vendorContactEmail = vendorContactEmail;
        }
        
        /* IncidentCost Field */
        public decimal GetIncidentCost()
        {
            return incidentCost;
        }
        public void SetIncidentCost(decimal incidentCost)
        {
            this.incidentCost = incidentCost;
        }
        
        /* IncidentDescription Field */
        public string GetIncidentDescription()
        {
            return incidentDescription;
        }
        public void SetIncidentDescription(string incidentDescription)
        {
            this.incidentDescription = incidentDescription;
        }

        /* Printing Section */
        public string IncidentPrintHeader()
        {
            return "Incident ID \t Incident Date \t Project Name \t\t Vendor Company Name \t Vendor Contact Name \t Vendor Contact Email \t\t Incident Cost \t Incident Description";
        }
        public override string ToString()
        {
            return $"{incidentID} \t\t {incidentDate.ToShortDateString()} \t {projectName,-25} \t {vendorCompanyName,-30} \t {vendorContactName,-30} \t {vendorContactEmail,-32} \t {incidentCost,-10:C2} \t {incidentDescription}";
        }
    }
}
