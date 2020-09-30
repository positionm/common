using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices.WindowsRuntime;

namespace serialcommon
{
    public partial class Form1 : Form
    {
        SerialPort sp = null;
        bool isOpen = false;
        bool isSetProperty = false;
        private bool Listening = false;
        private bool Closing_usr = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void RadioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void Button2_Click(object sender, EventArgs e)
        {
            bool comExistence = false;
            cbxCOMPort.Items.Clear();
            for (int i = 0; i < 20; i++)
            {
                try
                {
                    SerialPort sp = new SerialPort("COM" + (i + 1).ToString());
                    sp.Open();
                    sp.Close();
                    cbxCOMPort.Items.Add("COM" + (i + 1).ToString());
                    comExistence = true;
                }

                catch (Exception)
                {
                    continue;
                }
            }
            if (comExistence)
            {
                cbxCOMPort.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show("没有找到可用串口！", "错误提示！");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.MaximizeBox = false;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;

            for (int i = 0; i < 20; i++)
            {
                cbxCOMPort.Items.Add("COM" + (i + 1).ToString());
            }
            cbxCOMPort.SelectedIndex = 0;

            cbxBaudRate.Items.Add("1200");
            cbxBaudRate.Items.Add("2400");
            cbxBaudRate.Items.Add("4800");
            cbxBaudRate.Items.Add("9600");
            cbxBaudRate.Items.Add("19200");
            cbxBaudRate.Items.Add("38400");
            cbxBaudRate.Items.Add("115200");
            cbxBaudRate.SelectedIndex = 6;

            cbxStopbits.Items.Add("0");
            cbxStopbits.Items.Add("1");
            cbxStopbits.Items.Add("1.5");
            cbxStopbits.Items.Add("2");
            cbxStopbits.SelectedIndex = 1;

            cbxParity.Items.Add("无");
            cbxParity.Items.Add("奇校验");
            cbxParity.Items.Add("偶校验");
            cbxParity.SelectedIndex = 0;

            cbxDatabits.Items.Add("8");
            cbxDatabits.Items.Add("7");
            cbxDatabits.Items.Add("6");
            cbxDatabits.Items.Add("5");
            cbxDatabits.SelectedIndex = 0;

            rbnchar.Checked = true;
            /*添加时间显示*/
            timer1.Interval = 1000;
            timer1.Start();
        }
        private bool CheckPortSetting()
        {
            if (cbxCOMPort.Text.Trim() == "") return false;
            if (cbxBaudRate.Text.Trim() == "") return false;
            if (cbxStopbits.Text.Trim() == "") return false;
            if (cbxParity.Text.Trim() == "") return false;
            if (cbxDatabits.Text.Trim() == "") return false;
            return true;
        }
        private bool CheckSendData()
        {
            if (senddata.Text.Trim() == "") return false;
            return true;
        }

        private void COMPort_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void SetProperty()
        {
            sp = new SerialPort();
            sp.PortName = cbxCOMPort.Text.Trim();
            sp.BaudRate = Convert.ToInt32(cbxBaudRate.Text.Trim());
            if (cbxStopbits.Text.Trim() == "0")
            {
                sp.StopBits = StopBits.None;
            }
            else if (cbxStopbits.Text.Trim() == "1.5")
            {
                sp.StopBits = StopBits.OnePointFive;
            }
            else if (cbxStopbits.Text.Trim() == "2")
            {
                sp.StopBits = StopBits.Two;
            }
            else
            {
                sp.StopBits = StopBits.One;
            }

            sp.DataBits = Convert.ToInt16(cbxDatabits.Text.Trim());

            if (cbxParity.Text.Trim() == "奇校验")
            {
                sp.Parity = Parity.Odd;

            }
            else if (cbxParity.Text.Trim() == "偶校验")
            {
                sp.Parity = Parity.Even;
            }
            else
            {
                sp.Parity = Parity.None;
            }
            sp.ReadTimeout = -1;
            sp.RtsEnable = true;

            sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
        }
        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs eg)
        {

            System.Threading.Thread.Sleep(100);
            if (Closing_usr) return;//如果正在关闭，忽略操作，直接返回，尽快完成串口监听线程的一次循环
            try
            {
                Listening = true;//说明开始处理数据
                StringBuilder sb = new StringBuilder();
                long rec_count = 0;
                int num = sp.BytesToRead;
                int s = DateTime.Now.Second;//时间
                byte[] recbuf = new byte[num];
                rec_count += num;

                sp.Read(recbuf, 0, num);
                sb.Clear();
                this.Invoke((EventHandler)delegate//异步执行 一个线程
                {
                    if (!rbnhex.Checked)//如果未选中name为rbnHex的控件
                    {
                        if (recbuf != null)
                        {
                            sb.Append(DateTime.Now.ToString()+":" + " ");
                            foreach (byte b in recbuf)
                            {
                                sb.Append(b.ToString("X2") + " ");
                            }
                            sb.Append("\r\n");
                        }
                        //try
                        //{
                        //    Invoke((EventHandler)(delegate
                        //    {
                        //        sb.Append(Encoding.ASCII.GetString(recbuf));  //将整个数组解码为ASCII数组
                        //        textrevdata.AppendText(sb.ToString());
                        //    }
                        //    )
                        //    );
                        //}

                        //catch
                        //{
                        //    MessageBox.Show("请勾选换行", "错误提示");
                        //}
                    }
                    else
                    {
                        if(null != recbuf)
                        {
                            sb.Append(DateTime.Now.ToString() + ":" + " ");
                            sb.Append(Encoding.ASCII.GetString(recbuf));
                            sb.Append("\r\n");//换行
                        }
                    }
                //else if (rbnhex.Checked)//如果选中
                //    {
                //    Byte[] receiveddata = new Byte[sp.BytesToRead];
                //    sp.Read(receiveddata, 0, receiveddata.Length);

                //    string recvdatatext = null;

                //    for (int i = 0; i < receiveddata.Length; i++)
                //    {
                //        recvdatatext += (receiveddata[i].ToString("x2") + " ");//数组里接收到的数据转化为16进制
                //    }
                //    textrevdata.Text += recvdatatext;
                //}
                this.textrevdata.AppendText(sb.ToString());
                //sp.DiscardInBuffer();
            });
            }
            finally
            {
                Listening = false;//我用完了，UI可以关闭串口
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void opencom_Click(object sender, EventArgs e)
        {
            if (isOpen == false)
            {
                if (!CheckPortSetting())
                {
                    MessageBox.Show("串口未设置", "错误提示");
                    return;
                }
                if (!isSetProperty)
                {
                    SetProperty();
                    isSetProperty = true;
                }
                try
                {
                    sp.Open();
                    isOpen = true;
                    opencom.Text = "关闭串口";
                    cbxCOMPort.Enabled = false;
                    cbxBaudRate.Enabled = false;
                    cbxDatabits.Enabled = false;
                    cbxParity.Enabled = false;
                    cbxStopbits.Enabled = false;
                    rbnchar.Enabled = false;
                    rbnhex.Enabled = false;
                }
                catch (Exception)
                {
                    isSetProperty = false;
                    isOpen = false;
                    MessageBox.Show("串口无效或已被占用", "错误提示");
                }
            }
            else if (isOpen == true)
            {
                Closing_usr = true;
                while (Listening) Application.DoEvents();
                try
                {
                    if (!timebox3.Checked)
                    {
                        sp.Close();//关闭端口
                        isOpen = false;
                        opencom.Text = "打开串口";
                        cbxCOMPort.Enabled = true;
                        cbxBaudRate.Enabled = true;
                        cbxDatabits.Enabled = true;
                        cbxParity.Enabled = true;
                        cbxStopbits.Enabled = true;
                        rbnchar.Enabled = true;
                        rbnhex.Enabled = true;
                    }
                    else
                    {
                        MessageBox.Show("请先关闭自动发送", "错误提示");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("关闭串口时发生错误", "错误提示");
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.toolStripStatusLabel1.Text = "当前时间" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
        }

        private void senddata_Click(object sender, EventArgs e)
        {
            byte[] textchar = new byte[1];
            int num2 = 0;
            if (isOpen)
            {
                try
                {
                    if (!hexsend.Checked)//如果没有选中十六进制发送
                    {
                        if (!checkBox2.Checked)//未选中回车换行
                        {
                            sp.Write(textsenddata.Text);//串口发送 （发送框里的东西）
                        }
                        else
                        {
                            sp.WriteLine(textsenddata.Text);
                        }
                    }
                    else//选择十六进制发送的时候
                    {

                        string buf = textsenddata.Text;
                        string bartenm = @"\s";//正则表达式
                        string replace = "";

                        Regex rgx = new Regex(bartenm);
                        string senddata = rgx.Replace(buf, replace);
                        num2 = (senddata.Length - senddata.Length % 2) / 2;

                        for (int a = 0; a < num2; a++)
                        {
                            textchar[0] = Convert.ToByte(senddata.Substring(a * 2, 2), 16);
                            sp.Write(textchar, 0, 1);
                        }


                        if (senddata.Length % 2 != 0)
                        {
                            textchar[0] = Convert.ToByte(senddata.Substring(textsenddata.Text.Length - 1, 2), 16);
                            sp.Write(textchar, 0, 1);
                            num2++;
                        }
                    }
                }
                catch
                {
                    MessageBox.Show("发送数据时发生错误！", "错误提示");
                    return;
                }
            }
            else
            {
                MessageBox.Show("串口未打开错误提示！", "错误提示");
            }
            if (!CheckSendData())
            {
                MessageBox.Show("请输入要发送的数据", "错误提示");
            }
        }

        private void cleardata_Click(object sender, EventArgs e)
        {
            if (!timebox3.Checked)
            {
                textrevdata.Text = "";
                textsenddata.Text = "";
            }
            else
            {
                MessageBox.Show("请先关闭自动发送", "错误提示");
            }
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            senddata_Click(senddata, new EventArgs());
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void timebox3_CheckedChanged(object sender, EventArgs e)
        {
            System.Windows.Forms.Timer txTimer = new System.Windows.Forms.Timer();

            if (timebox3.Checked)
            {
                if (numericUpDown1.Value != 0)
                {
                    if (CheckSendData())
                    {
                        txTimer.Enabled = false;
                        timer2.Interval = (int)numericUpDown1.Value; //定时器赋初值  
                        timer2.Start();
                    }
                    else if (!CheckSendData())
                    {
                        timer2.Stop();
                    }
                }
                else if (numericUpDown1.Value == 0)
                {
                    timer2.Stop();
                }
            }
            else
            {
                txTimer.Enabled = true;
                timer2.Stop();
            }
        }
    }
}
