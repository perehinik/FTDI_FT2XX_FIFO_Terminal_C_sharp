using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using FTD2XX_NET;


namespace FT_FIFO_Example
{
    public partial class Form1 : Form
    {

        UInt32 ftdiDeviceCount = 0;
        FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;
        // Create new instance of the FTDI device class
        FTDI myFtdiDevice = new FTDI();

        int deviceIndex = -1;

        // Allocate storage for device info list
        FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList;

        public Form1()
        {
            InitializeComponent();
            ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[5];
            RxCheckTimer.Enabled =  false;
            textBox4.Text = "Logic";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text =  "";
            // Determine the number of FTDI devices connected to the machine
            ftStatus = myFtdiDevice.GetNumberOfDevices(ref ftdiDeviceCount);
            // Check status
            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                textBox1.Text += "Number of FTDI devices: " + ftdiDeviceCount.ToString()+ "\r\n";
            }
            else 
            {
                // Wait for a key press
                textBox1.Text += "Failed to get number of devices (error " + ftStatus.ToString() + ")\r\n";
                return;
            }

            // If no devices available, return
            if (ftdiDeviceCount == 0)
            {
                // Wait for a key press
                textBox1.Text += "Failed to get number of devices (error " + ftStatus.ToString() + ")\r\n";
                return;
            }

            // Populate our device list
            ftStatus = myFtdiDevice.GetDeviceList(ftdiDeviceList);

            if (ftStatus == FTDI.FT_STATUS.FT_OK)
            {
                for (UInt32 i = 0; i < ftdiDeviceCount; i++)
                {
                    string tempDescription = ftdiDeviceList[i].Description.ToString();
                    string tempSerial = ftdiDeviceList[i].SerialNumber.ToString();
                    if (tempDescription.Contains(textBox4.Text) && tempSerial.Contains(textBox3.Text))
                    {
                        deviceIndex = (int)i;
                        textBox1.Text += "Device Index: " + i.ToString() + "\r\n";
                        textBox1.Text += "Flags: " + String.Format("{0:x}", ftdiDeviceList[i].Flags) + "\r\n";
                        textBox1.Text += "Type: " + ftdiDeviceList[i].Type.ToString() + "\r\n";
                        textBox1.Text += "ID: " + String.Format("{0:x}", ftdiDeviceList[i].ID) + "\r\n";
                        textBox1.Text += "Location ID: " + String.Format("{0:x}", ftdiDeviceList[i].LocId) + "\r\n";
                        textBox1.Text += "Serial Number: " + ftdiDeviceList[i].SerialNumber.ToString() + "\r\n";
                        textBox1.Text += "Description: " + ftdiDeviceList[i].Description.ToString() + "\r\n\r\n";
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (deviceIndex >= 0)
            {
                ftStatus = myFtdiDevice.OpenBySerialNumber(ftdiDeviceList[deviceIndex].SerialNumber);
                if (ftStatus != FTDI.FT_STATUS.FT_OK)
                {
                    // Wait for a key press
                    textBox1.Text += "Failed to open device (error " + ftStatus.ToString() + ")\r\n";
                    return;
                }
                else textBox1.Text += "Device opened !!!\r\n";
            }

            ftStatus = myFtdiDevice.SetBitMode(0x00, 0x40);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                textBox1.Text += "Failed to set bit mode (error " + ftStatus.ToString() + ")\r\n";
                return;
            }
            else textBox1.Text += "Bit mode = 0x40 !!!\r\n";

        }

        private void button3_Click(object sender, EventArgs e)
        {
            ftStatus = myFtdiDevice.Close();
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                // Wait for a key press
                textBox1.Text += "Failed to close device (error " + ftStatus.ToString() + ")\r\n";
                return;
            }
            else textBox1.Text += "Device closed !!!" + "\r\n";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
        }

        private void RxCheckTimer_Tick(object sender, EventArgs e)
        {
            RxCheckTimer.Enabled = false;
            uint RxCount = 0;
            var rx_buf = new byte[1000];
            string rx_str_buf = "";
            uint data_read = 0;
            ftStatus = myFtdiDevice.GetRxBytesAvailable(ref RxCount);
            if (ftStatus != FTDI.FT_STATUS.FT_OK)
            {
                textBox1.Text = "<<ERROR: " + ftStatus.ToString() + "\r\n";
            }
            else if (RxCount > 0)
            {
                ftStatus = myFtdiDevice.Read(rx_buf, RxCount, ref data_read);
                //rx_str_buf = System.Text.Encoding.Default.GetString(rx_buf);
                rx_str_buf = BitConverter.ToString(rx_buf);
                textBox1.Text += "Received " + RxCount + " bytes   <<" + rx_str_buf + ">>\r\n\r\n";
            }
            else
            {
                //textBox1.Text += "<> " + RxCount + " <>" + "\r\n";
            }
            RxCheckTimer.Enabled = true;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            bool timerState = RxCheckTimer.Enabled;
            RxCheckTimer.Enabled = false;
            var tx_buf = new byte[1000];
            int tx_length = 0;
            for (int i = 0; i < 998; i++)
            {
                tx_buf[i] = 0x00;
            }
            for (int i = 0; i < textBox2.Text.Length; i++)
            {
                if (i < 999)
                {
                    tx_buf[i] = (byte)textBox2.Text[i];
                    tx_length = i + 1;
                }
            }
            uint data_written = 0;
            ftStatus = myFtdiDevice.Write(tx_buf, tx_length, ref data_written);
            System.Threading.Thread.Sleep(200);
            textBox1.Text = data_written + " bytes send\r\n";
            RxCheckTimer.Enabled = timerState;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (RxCheckTimer.Enabled == true)
            {
                RxCheckTimer.Enabled = false;
                button6.BackColor = Color.Transparent;
            }else
            {
                RxCheckTimer.Enabled = true;
                button6.BackColor = Color.Green;
            }
        }
    }
}
