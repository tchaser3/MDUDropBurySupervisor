/* Title:           Schedule Technicians
 * Date:            10-19-17
 * Author:          Terry Holmes
 * 
 * Description:     This form will display the schedule */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.Maps.MapControl.WPF.Design;
using System.Xml;
using System.Net;
using System.Xml.XPath;
using NewEventLogDLL;
using WorkOrderScheduleDLL;
using DateSearchDLL;
using CustomersDLL;
using WorkTypeDLL;

namespace MDUDropBurySupervisor
{
    /// <summary>
    /// Interaction logic for ScheduleTechnicians.xaml
    /// </summary>
    public partial class ScheduleTechnicians : Window
    {
        WPFMessagesClass TheMessagesClass = new WPFMessagesClass();
        EventLogClass TheEventLogClass = new EventLogClass();
        WorkOrderScheduleClass TheWorkOrderScheduleClass = new WorkOrderScheduleClass();
        DateSearchClass TheDateSearchClass = new DateSearchClass();
        CustomersClass TheCustomerClass = new CustomersClass();
        WorkTypeClass TheWorkTypeClass = new WorkTypeClass();

        //setting data
        FindScheduledWorkOrdersByDateRangeDataSet TheFindScheduledWorkOrdersByDateRangeDataSet = new FindScheduledWorkOrdersByDateRangeDataSet();
        WorkOrdersSelectedDataSet TheWorkOrdersSelectedDataSet = new WorkOrdersSelectedDataSet();
        FindWorkTypeSortedDataSet TheFindWorkTypeSortedDataSet = new FindWorkTypeSortedDataSet();
        FindWorkZonesDataSet TheFindWorkZoneDataSet = new FindWorkZonesDataSet();
        FindScheduledWorkByTypeStatusDateRangeDataSet TheFindScheduledWorkByTypeStatusDateRangeDataSet = new FindScheduledWorkByTypeStatusDateRangeDataSet();

        string BingMapsKey = "rlOQHqvgydklMdwaQpTs~2ABi0R5AuQXzlDyIS5RJwQ~Ajh8Q9JoMtW_PkcY-IQBgLRvc-3SOz8tDR52P-UtRD1uIUksrk0mdpmhNOp8K2Nz";

        DateTime gdatStartDate;
        DateTime gdatEndDate;

        public ScheduleTechnicians()
        {
            InitializeComponent();
        }

        private void btnMainMenu_Click(object sender, RoutedEventArgs e)
        {
            MainMenu MainMenu = new MainMenu();
            MainMenu.Show();
            Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            TheMessagesClass.CloseTheProgram();
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string strAddress = "";
            int intCounter;
            int intNumberOfRecords;
            int intItemCounter;
            string strItemCounter;
            string strWorkOrder;

            PleaseWait PleaseWait = new PleaseWait();
            PleaseWait.Show();

            try
            {
                LoadComboBoxes();

                //getting ready for the loop
                gdatStartDate = DateTime.Now;
                gdatStartDate = TheDateSearchClass.RemoveTime(gdatStartDate);
                gdatEndDate = TheDateSearchClass.AddingDays(gdatStartDate, 1);
                intItemCounter = 0;

                //loading the data set
                TheFindScheduledWorkOrdersByDateRangeDataSet = TheWorkOrderScheduleClass.FindScheduledWorkOrdersByDateRange(gdatStartDate, gdatEndDate);

                intNumberOfRecords = TheFindScheduledWorkOrdersByDateRangeDataSet.FindScheduledWorkOrdersByDateRange.Rows.Count - 1;

                if(intNumberOfRecords > -1)
                {
                    for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                    {
                        strAddress = TheFindScheduledWorkOrdersByDateRangeDataSet.FindScheduledWorkOrdersByDateRange[intCounter].StreetAddress + ", ";
                        strAddress += TheFindScheduledWorkOrdersByDateRangeDataSet.FindScheduledWorkOrdersByDateRange[intCounter].City + ", OH";

                        intItemCounter++;
                        strItemCounter = Convert.ToString(intItemCounter);
                        strWorkOrder = strItemCounter + "\n";
                        strWorkOrder += TheFindScheduledWorkOrdersByDateRangeDataSet.FindScheduledWorkOrdersByDateRange[intCounter].WorkOrderNumber + "\n";
                        strWorkOrder += TheFindScheduledWorkOrdersByDateRangeDataSet.FindScheduledWorkOrdersByDateRange[intCounter].FirstName + " ";
                        strWorkOrder += TheFindScheduledWorkOrdersByDateRangeDataSet.FindScheduledWorkOrdersByDateRange[intCounter].LastName + "\n";
                        strWorkOrder += TheFindScheduledWorkOrdersByDateRangeDataSet.FindScheduledWorkOrdersByDateRange[intCounter].StreetAddress + "\n";
                        strWorkOrder += TheFindScheduledWorkOrdersByDateRangeDataSet.FindScheduledWorkOrdersByDateRange[intCounter].City + ", OH";
                        WorkOrdersSelectedDataSet.workordersRow NewOrderRow = TheWorkOrdersSelectedDataSet.workorders.NewworkordersRow();

                        NewOrderRow.Orders = strWorkOrder;

                        TheWorkOrdersSelectedDataSet.workorders.Rows.Add(NewOrderRow);

                        XmlDocument searchResponse = Geocode(strAddress);

                        FindandDisplayNearbyPOI(searchResponse, strItemCounter);
                    }

                    dgrWorkOrders.ItemsSource = TheWorkOrdersSelectedDataSet.workorders;
                }
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "MDU Drop Bury Supervisor // Schedule Technicians // Window Loaded " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }

            PleaseWait.Close();
        }
        private void LoadComboBoxes()
        {
            int intCounter;
            int intNumberOfRecords;

            try
            {
                TheFindWorkTypeSortedDataSet = TheWorkTypeClass.FindWorkTypeSorted();

                intNumberOfRecords = TheFindWorkTypeSortedDataSet.FindWorkTypeSorted.Rows.Count - 1;
                cboWorkType.Items.Add("Select Work Types");

                for(intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    cboWorkType.Items.Add(TheFindWorkTypeSortedDataSet.FindWorkTypeSorted[intCounter].WorkType);
                }

                cboWorkType.SelectedIndex = 0;

                TheFindWorkZoneDataSet = TheCustomerClass.FindWorkZones();
                intNumberOfRecords = TheFindWorkZoneDataSet.FindWorkZones.Rows.Count - 1;
                cboZone.Items.Add("Select Zone");

                for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                {
                    cboZone.Items.Add(TheFindWorkZoneDataSet.FindWorkZones[intCounter].ZoneLocation);
                }

                cboZone.SelectedIndex = 0;
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "MDU Drop Bury Supervisor // Schedule Technicians // Load Combo Boxes " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }
        }
        public XmlDocument Geocode(string addressQuery)
        {
            //Create REST Services geocode request using Locations API
            string geocodeRequest = "http://dev.virtualearth.net/REST/v1/Locations/" + addressQuery + "?o=xml&key=" + BingMapsKey;

            //Make the request and get the response
            XmlDocument geocodeResponse = GetXmlResponse(geocodeRequest);

            return (geocodeResponse);
        }

        private XmlDocument GetXmlResponse(string requestUrl)
        {
            System.Diagnostics.Trace.WriteLine("Request URL (XML): " + requestUrl);
            HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(String.Format("Server error (HTTP {0}: {1}).",
                    response.StatusCode,
                    response.StatusDescription));
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(response.GetResponseStream());
                return xmlDoc;
            }
        }
        private void FindandDisplayNearbyPOI(XmlDocument xmlDoc, string strItemCounter)
        {
            //Get location information from geocode response 

            //Create namespace manager
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("rest", "http://schemas.microsoft.com/search/local/ws/rest/v1");
            XmlNodeList locationElements = xmlDoc.SelectNodes("//rest:Location", nsmgr);

            //Get the geocode location points that are used for display (UsageType=Display)
            XmlNodeList displayGeocodePoints = locationElements[0].SelectNodes(".//rest:GeocodePoint/rest:UsageType[.='Display']/parent::node()", nsmgr);
            string latitude = displayGeocodePoints[0].SelectSingleNode(".//rest:Latitude", nsmgr).InnerText;
            string longitude = displayGeocodePoints[0].SelectSingleNode(".//rest:Longitude", nsmgr).InnerText;


            Location location = new Location(Convert.ToDouble(latitude), Convert.ToDouble(longitude));
            Pushpin pushpin = new Pushpin();
            pushpin.Location = location;
            pushpin.Content = strItemCounter;
            myMap.Children.Add(pushpin);
            
        }

        private void cboWorkType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int intSelectedIndex;
            int intCounter;
            int intNumberOfRecords;
            string strWorkType;
            string strAddress;
            int intItemCounter;
            string strItemCounter;
            string strWorkOrder;

            PleaseWait PleaseWait = new PleaseWait();
            PleaseWait.Show();
            myMap.Children.Clear();

            try
            {
                intSelectedIndex = cboWorkType.SelectedIndex;
                intItemCounter = 0;

                if (intSelectedIndex > 0)
                {
                    strWorkType = cboWorkType.SelectedItem.ToString();

                    TheWorkOrdersSelectedDataSet.workorders.Rows.Clear();

                    TheFindScheduledWorkByTypeStatusDateRangeDataSet = TheWorkOrderScheduleClass.FindScheduledWorkByTypeStatusDateRange(strWorkType, "SCHEDULED", gdatStartDate, gdatEndDate);

                    intNumberOfRecords = TheFindScheduledWorkByTypeStatusDateRangeDataSet.FindScheduleWorkByTypeStatusAndDateRange.Rows.Count - 1;

                    if (intNumberOfRecords > -1)
                    {
                        for (intCounter = 0; intCounter <= intNumberOfRecords; intCounter++)
                        {
                            strAddress = TheFindScheduledWorkByTypeStatusDateRangeDataSet.FindScheduleWorkByTypeStatusAndDateRange[intCounter].StreetAddress + ", ";
                            strAddress += TheFindScheduledWorkByTypeStatusDateRangeDataSet.FindScheduleWorkByTypeStatusAndDateRange[intCounter].City + ", OH";

                            intItemCounter++;
                            strItemCounter = Convert.ToString(intItemCounter);
                            strWorkOrder = strItemCounter + "\n";
                            strWorkOrder += TheFindScheduledWorkByTypeStatusDateRangeDataSet.FindScheduleWorkByTypeStatusAndDateRange[intCounter].WorkOrderNumber + "\n";
                            strWorkOrder += TheFindScheduledWorkByTypeStatusDateRangeDataSet.FindScheduleWorkByTypeStatusAndDateRange[intCounter].FirstName + " ";
                            strWorkOrder += TheFindScheduledWorkByTypeStatusDateRangeDataSet.FindScheduleWorkByTypeStatusAndDateRange[intCounter].LastName + "\n";
                            strWorkOrder += TheFindScheduledWorkByTypeStatusDateRangeDataSet.FindScheduleWorkByTypeStatusAndDateRange[intCounter].StreetAddress + "\n";
                            strWorkOrder += TheFindScheduledWorkByTypeStatusDateRangeDataSet.FindScheduleWorkByTypeStatusAndDateRange[intCounter].City + ", OH";
                            WorkOrdersSelectedDataSet.workordersRow NewOrderRow = TheWorkOrdersSelectedDataSet.workorders.NewworkordersRow();

                            NewOrderRow.Orders = strWorkOrder;

                            TheWorkOrdersSelectedDataSet.workorders.Rows.Add(NewOrderRow);

                            XmlDocument searchResponse = Geocode(strAddress);

                            FindandDisplayNearbyPOI(searchResponse, strItemCounter);
                        }

                        dgrWorkOrders.ItemsSource = TheWorkOrdersSelectedDataSet.workorders;
                    }
                }
            }
            catch (Exception Ex)
            {
                TheEventLogClass.InsertEventLogEntry(DateTime.Now, "MDU Drop Bury Supervisor // Schedule Technician // Work Type Combo Box " + Ex.Message);

                TheMessagesClass.ErrorMessage(Ex.ToString());
            }

            PleaseWait.Close();
        }
    }
}
