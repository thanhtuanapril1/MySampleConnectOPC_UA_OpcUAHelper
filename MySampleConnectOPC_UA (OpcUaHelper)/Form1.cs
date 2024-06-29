using Microsoft.VisualBasic;
using Opc.Ua;
using Opc.Ua.Client;
using OpcUaHelper;
using System.Net.NetworkInformation;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

/*******************************
Docs for library OpcUaHelper
https://www.youtube.com/watch?v=A1xNi4quuk4
https://github.com/dathlin/OpcUaHelper/tree/master
********************************/

namespace ConnectOPC_UA
{
    public partial class Form1 : Form
    {
        OpcUaClient myClient = new OpcUaClient();

        public Form1()
        {
            InitializeComponent();
            myClient.UserIdentity = new UserIdentity(new AnonymousIdentityToken());
        }

        private async void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                await myClient.ConnectServer(tbxServerAddress.Text);
                if (myClient.Connected)
                {
                    groupBox1.Enabled = true;
                    groupBox2.Enabled = true;
                    btnDisonnect.Enabled = true;
                    btnConnect.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void btnDisonnect_Click(object sender, EventArgs e)
        {
            try
            {
                myClient.Disconnect();

                if (!myClient.Connected)
                {
                    groupBox1.Enabled = false;
                    groupBox2.Enabled = false;
                    btnDisonnect.Enabled = false;
                    btnConnect.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private async void btnRead_Click(object sender, EventArgs e)
        {
            try
            {
                var tag = myClient.ReadNode(tbxTag.Text);

                if (tag.StatusCode.Code == 0)   //Status code == good
                {
                    tbxValue.Text = tag.Value.ToString();
                    tbxValueDataType.Text = tag.WrappedValue.TypeInfo.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnWrite_Click(object sender, EventArgs e)
        {
            try
            {
                var tag = myClient.ReadNode(tbxTag.Text);
                if (tag != null)
                {
                    string tagType = tag.WrappedValue.TypeInfo.ToString();   //Try to get the data type of node

                    bool resultStatus = false;
                    if (tagType == "Boolean") resultStatus = myClient.WriteNode(tbxTag.Text, bool.Parse(tbxWriteValue.Text));
                    else if (tagType == "String") resultStatus = myClient.WriteNode(tbxTag.Text, tbxWriteValue.Text);
                    else if (tagType == "UInt16") resultStatus = myClient.WriteNode(tbxTag.Text, UInt16.Parse(tbxWriteValue.Text));
                    else if (tagType == "UInt32") resultStatus = myClient.WriteNode(tbxTag.Text, UInt32.Parse(tbxWriteValue.Text));
                    else if (tagType == "Int16") resultStatus = myClient.WriteNode(tbxTag.Text, Int16.Parse(tbxWriteValue.Text));
                    else if (tagType == "Int32") resultStatus = myClient.WriteNode(tbxTag.Text, Int32.Parse(tbxWriteValue.Text));
                    else if (tagType == "Float") resultStatus = myClient.WriteNode(tbxTag.Text, float.Parse(tbxWriteValue.Text));
                    else if (tagType == "Double") resultStatus = myClient.WriteNode(tbxTag.Text, double.Parse(tbxWriteValue.Text));
                    else if (tagType == "DateTime") resultStatus = myClient.WriteNode(tbxTag.Text, DateTime.Parse(tbxWriteValue.Text));
                    //Add more if need

                    if (resultStatus = false) MessageBox.Show("Write Status is Bad: ");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnReadMulti_Click(object sender, EventArgs e)
        {
            try
            {
                List<NodeId> nodeIds = new List<NodeId>();
                nodeIds.Add(new NodeId(tbxTag1.Text));
                nodeIds.Add(new NodeId(tbxTag2.Text));
                nodeIds.Add(new NodeId(tbxTag3.Text));
                nodeIds.Add(new NodeId(tbxTag4.Text));

                List<DataValue> dataValues = myClient.ReadNodes(nodeIds.ToArray());

                if (dataValues.Count > 0)
                {
                    int i = 0;
                    foreach (DataValue value in dataValues)
                    {
                        if (i == 0 && value.Value != null) { tbxValue1.Text = value.Value.ToString(); tbxDataType1.Text = value.WrappedValue.TypeInfo.ToString(); }
                        if (i == 1 && value.Value != null) { tbxValue2.Text = value.Value.ToString(); tbxDataType2.Text = value.WrappedValue.TypeInfo.ToString(); }
                        if (i == 2 && value.Value != null) { tbxValue3.Text = value.Value.ToString(); tbxDataType3.Text = value.WrappedValue.TypeInfo.ToString(); }
                        if (i == 3 && value.Value != null) { tbxValue4.Text = value.Value.ToString(); tbxDataType4.Text = value.WrappedValue.TypeInfo.ToString(); }

                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void btnWriteMulti_Click(object sender, EventArgs e)
        {
            List<string> list = new List<string>(); ;
            List<object> values = new List<object>();

            if (tbxTag1.Text != "" && tbxValue1.Text != "") { list.Add(tbxTag1.Text); values.Add(UInt16.Parse(tbxValue1.Text)); }
            if (tbxTag2.Text != "" && tbxValue2.Text != "") { list.Add(tbxTag2.Text); values.Add(UInt32.Parse(tbxValue2.Text)); }
            if (tbxTag3.Text != "" && tbxValue3.Text != "") { list.Add(tbxTag3.Text); values.Add(tbxValue3.Text); }
            if (tbxTag4.Text != "" && tbxValue4.Text != "") { list.Add(tbxTag4.Text); values.Add(UInt16.Parse(tbxValue4.Text)); }


            bool success = myClient.WriteNodes(list.ToArray(), values.ToArray());
            if (success) MessageBox.Show("Write Successfully");
            else MessageBox.Show("Write Failed");
        }

        private void btnSubcribe_Click(object sender, EventArgs e)
        {
            try
            {
                myClient.AddSubscription("A", tbxTag.Text, SubCallback);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnUnsubcribe_Click(object sender, EventArgs e)
        {
            try
            {
                myClient.RemoveSubscription("A");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        List<string> monitorNodeTags = new List<string>();
        private void btnSubscribeMulti_Click(object sender, EventArgs e)
        {
            try
            {
                monitorNodeTags.Clear();

                if (tbxTag1.Text != "") monitorNodeTags.Add(tbxTag1.Text);
                if (tbxTag2.Text != "") monitorNodeTags.Add(tbxTag2.Text);
                if (tbxTag3.Text != "") monitorNodeTags.Add(tbxTag3.Text);
                if (tbxTag4.Text != "") monitorNodeTags.Add(tbxTag4.Text);

                myClient.AddSubscription("B", monitorNodeTags.ToArray(), SubCallback);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnUnsubscribeMulti_Click(object sender, EventArgs e)
        {
            try
            {
                myClient.RemoveSubscription("B");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SubCallback(string key, MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, MonitoredItem, MonitoredItemNotificationEventArgs>(SubCallback), key, monitoredItem, e);
                return;
            }

            if (key == "A")
            {
                // If multiple subscription values are associated with the current method, they can be distinguished by key and monitoredItem
                MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;
                if (notification != null)
                {
                    tbxValue.Text = notification.Value.WrappedValue.Value.ToString();
                    tbxValueDataType.Text = notification.Value.WrappedValue.TypeInfo.ToString();
                }
            }
            else if (key == "B")
            {
                // Need to distinguish each different node information
                MonitoredItemNotification notification = e.NotificationValue as MonitoredItemNotification;
                if (monitoredItem.StartNodeId.ToString() == monitorNodeTags[0])
                {
                    tbxValue1.Text = notification.Value.WrappedValue.Value.ToString();
                    tbxDataType1.Text = notification.Value.WrappedValue.TypeInfo.ToString();
                }
                else if (monitoredItem.StartNodeId.ToString() == monitorNodeTags[1])
                {
                    tbxValue2.Text = notification.Value.WrappedValue.Value.ToString();
                    tbxDataType2.Text = notification.Value.WrappedValue.TypeInfo.ToString();
                }
                else if (monitoredItem.StartNodeId.ToString() == monitorNodeTags[2])
                {
                    tbxValue3.Text = notification.Value.WrappedValue.Value.ToString();
                    tbxDataType3.Text = notification.Value.WrappedValue.TypeInfo.ToString();
                }
                else if (monitoredItem.StartNodeId.ToString() == monitorNodeTags[3])
                {
                    tbxValue4.Text = notification.Value.WrappedValue.Value.ToString();
                    tbxDataType4.Text = notification.Value.WrappedValue.TypeInfo.ToString();
                }
            }

        }

        private void btnBrowseNode_Click(object sender, EventArgs e)
        {
            try
            {
                OpcNodeAttribute[] nodeAttributes = myClient.ReadNoteAttributes(tbxTag.Text);
                Console.ForegroundColor = ConsoleColor.Blue;
                foreach (var item in nodeAttributes)
                {
                    Console.Write(string.Format("{0,-30}", item.Name));
                    Console.Write(string.Format("{0,-20}", item.Type));
                    Console.Write(string.Format("{0,-20}", item.StatusCode));
                    Console.WriteLine(string.Format("{0,20}", item.Value));
                }

                Console.WriteLine();
                Console.ForegroundColor = ConsoleColor.DarkGreen;

                ReferenceDescription[] references = myClient.BrowseNodeReference(tbxTag.Text);
                foreach (var item in references)
                {
                    Console.Write(string.Format("{0,-30}", item.NodeClass));
                    Console.Write(string.Format("{0,-30}", item.BrowseName));
                    Console.Write(string.Format("{0,-20}", item.DisplayName));
                    Console.WriteLine(string.Format("{0,-20}", item.NodeId.ToString()));
                }

                Console.ResetColor();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
