using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Forms;
using System.IO;

namespace SerialUSB
{
    public partial class Form1 : Form
    {

        struct rx_data
        {
            public int[] pin_status;
            public int time_tick;
        }

        int start_time = 0;
        List<rx_data> list_data = new List<rx_data>();
        Int64 X_Axis_Base = 0;
        bool first_data = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void SerialPortReset()
        {
            foreach (var port_name in SerialPort.GetPortNames())
            {
                if(!comboBox1.Items.Contains(port_name))
                {
                    comboBox1.Items.Add(port_name);
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SerialPortReset();
        }

        private void FormsPlot1_Load(object sender, EventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen && comboBox1.SelectedIndex != -1)
            {
                serialPort1.PortName = ((string)comboBox1.SelectedItem);
                serialPort1.Open();

                char[] st = { 's', 't', 'a', 'r', 't' };
                serialPort1.Write(st, 0, 5);
                button1.Text = "STOP";
                button1.BackColor = System.Drawing.Color.Red;
                first_data = true;
                return;
            }

            if (serialPort1.IsOpen)
            {
                char[] st = { 's', 't', 'o', 'p' };
                serialPort1.Write(st, 0, 4);
                serialPort1.Close();
                button1.Text = "START";
                button1.BackColor = System.Drawing.Color.Green;
                first_data = false;
                timer1.Enabled = false;
                return;
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            SerialPortReset();
        }

        rx_data old_result;
        rx_data result;
        private void ReadRxData(string data)
        {
            var temp_port_value = "";
            var temp_time = "";
            int port = 0, time = 0;

            for (int i = 0; i < 4; i++)
                temp_port_value += Convert.ToChar(data[i]);
            Int32.TryParse(temp_port_value, out port);

            for (int i = 5; i < 11; i++)
                temp_time += Convert.ToChar(data[i]);
            temp_time = temp_time.PadLeft(6, '0');
            Int32.TryParse(temp_time, out time);

            if (first_data)
            {
                timer1.Enabled = true;
                old_result.pin_status = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                result.pin_status = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
                first_data = false;
                start_time = time;
            }

            string binary = Convert.ToString(port, 2);
            binary = binary.PadLeft(8, '0');

            int[] pin_status = new int[8];
            for (int i = 0; i < 8; i++)
            {
                pin_status[i] = Convert.ToInt32(binary[i]) - 48;
            }
            old_result.pin_status = result.pin_status;
            old_result.time_tick = time - start_time;
            result.pin_status = pin_status;
            result.time_tick = time - start_time;

            list_data.Add(old_result);
            list_data.Add(result);
        }

        private void DrawRxData()
        {
            List<double> pin0 = new List<double>();
            List<double> pin1 = new List<double>();
            List<double> pin2 = new List<double>();
            List<double> pin3 = new List<double>();
            List<double> pin4 = new List<double>();
            List<double> pin5 = new List<double>();
            List<double> pin6 = new List<double>();
            List<double> pin7 = new List<double>();
            List<double> time_tick = new List<double>();
            foreach (var data in list_data)
            {
                pin0.Add(data.pin_status[0]);
                pin1.Add(data.pin_status[1]);
                pin2.Add(data.pin_status[2]);
                pin3.Add(data.pin_status[3]);
                pin4.Add(data.pin_status[4]);
                pin5.Add(data.pin_status[5]);
                pin6.Add(data.pin_status[6]);
                pin7.Add(data.pin_status[7]);

                time_tick.Add(data.time_tick);
            }
            //formsPlot1.plt.PlotScatter(time_tick.ToArray(), pin0.ToArray());
            //formsPlot1.Render();

            formsPlot1.plt.Clear();
            formsPlot1.plt.PlotSignal(pin0.ToArray(), markerSize: 0);
            formsPlot1.plt.PlotSignal(pin1.ToArray(), markerSize: 0);
            formsPlot1.plt.PlotSignal(pin2.ToArray(), markerSize: 0);
            formsPlot1.plt.PlotSignal(pin3.ToArray(), markerSize: 0);
            formsPlot1.plt.PlotSignal(pin4.ToArray(), markerSize: 0);
            formsPlot1.plt.PlotSignal(pin5.ToArray(), markerSize: 0);
            formsPlot1.plt.PlotSignal(pin6.ToArray(), markerSize: 0);
            formsPlot1.plt.PlotSignal(pin7.ToArray(), markerSize: 0);

            if (X_Axis_Base < 1000)
            {
                formsPlot1.plt.Axis(y1: -0.2, y2: 1.2, x1: 0, x2: 1000);
            }
            else
            {
                formsPlot1.plt.Axis(y1: -0.2, y2: 1.2, x1: X_Axis_Base - 1000, x2: X_Axis_Base);
            }

            X_Axis_Base++;
            //formsPlot1.plt.AxisAutoX();
            textBox2.Invoke((MethodInvoker)delegate
            {
                textBox2.Text = pin0.Count.ToString();
            });
            formsPlot1.Render();
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            char[] byte_rx = new char[13];
            serialPort1.Read(byte_rx, 0, 13);
            serialPort1.DiscardInBuffer();

            string serial_rx = "";

            for (int i = 0; i < 13; i++)
            {
                serial_rx += byte_rx[i];
            }

            textBox1.Invoke((MethodInvoker)delegate
            {
                textBox1.Text += serial_rx;
                textBox1.Text += '\n';
                textBox1.SelectionStart = textBox1.Text.Length;
                textBox1.ScrollToCaret();
            });

            ReadRxData(serial_rx);
            DrawRxData();
        }

        private void Button3_Click(object sender, EventArgs e)
        {

        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            string result_port = string.Join("", result.pin_status);
            string result_time = (result.time_tick+1).ToString();
            result_time = result_time.PadLeft(6, '0');
            string temp_data = result_port + ":" + result_time;
            ReadRxData(temp_data);
            DrawRxData();
        }

        private void Button3_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog sfdlg = new SaveFileDialog();
            sfdlg.Filter = "Text Files (*.txt) | *.txt";
            if (sfdlg.ShowDialog() == DialogResult.OK)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(sfdlg.FileName))
                {
                    foreach (var line in list_data)
                    {
                        string result_time = line.time_tick.ToString();
                        result_time = result_time.PadLeft(6, '0');
                        string result_port = string.Join("", line.pin_status);
                        file.Write(result_port);
                        file.Write(":");
                        file.WriteLine(result_time);
                    }
                }
            }
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            old_result.pin_status = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            result.pin_status = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            string temp_data = string.Empty;
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Open a Text File";
            ofd.Filter = "Text Files (*.txt) | *.txt | All Files(*.*) | *.*";
            DialogResult dr = ofd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                StreamReader sr = new StreamReader(ofd.FileName);
                while ((temp_data = sr.ReadLine()) != null)
                {
                    ReadRxData(temp_data);
                    DrawRxData();
                }
                sr.Close();
            }
        }
    }
}
