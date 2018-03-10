using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.NetworkInformation;
using MySql.Data.MySqlClient;
using System.IO.Ports;


namespace BirrUp
{
    public partial class Form1 : Form
    {
        Random Rand = new Random();
        byte[] AddrBytes = new byte[] { 192, 168, 0, 16 }; // byte array for server address.
        byte[] LocalhostBytes = new byte[] { 127, 0, 0, 1 }; // byte array for server address.

        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            if (check_comports() == "")
            {
                label1.Text = "Sin COM";
                label1.ForeColor = Color.Coral;
            }
        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadExisting();
            if (indata.Length == 14)
            {
                present_card(indata.Replace("\r\n", string.Empty).Replace(" ", string.Empty));
            }

        }

        private void present_card(string data) {
            label6.Text = data;
            label6.ForeColor = Color.Blue;
            label9.ForeColor = Color.MediumVioletRed;
            if (check_connection())
            {
                string aux_query = "SELECT * FROM Cards WHERE card LIKE '" + data + "'";
                DataTable Table_aux = SQL_Query(aux_query);
                if (Table_aux.Rows.Count == 0)
                {
                    button3.Text = "Add";
                    label9.Text = "Sin registrar";
                    button3.Enabled = true;
                }
                else
                {
                    button4.Enabled = true;
                    label9.Text = "$" + Table_aux.Rows[0]["saldo"].ToString();
                    string s = Table_aux.Rows[0]["enable"].ToString();
                    if (Table_aux.Rows[0]["enable"].ToString() == "False")
                    {
                        button3.Text = "Enable";
                        textBox_saldo.Enabled = true;
                    }
                    else
                        button3.Text = "Disable";
                    button3.Enabled = true;
                }
            }
            else
            {
                label1.Text = "Sin server";
                label1.ForeColor = Color.Coral;
            }
        }
       
        private string check_comports() {
            SerialPort ComPort = new SerialPort();
            string[] ArrayComPortsNames = null;

            ArrayComPortsNames = SerialPort.GetPortNames();

            if (ArrayComPortsNames.Length > 0)
            {
                try
                {
                    ComPort.PortName = ArrayComPortsNames[ArrayComPortsNames.Length-1];
                    ComPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
                    ComPort.BaudRate = 115200;
                    ComPort.Open();
                    label1.Text = "COM abierto";
                    label1.ForeColor = Color.LawnGreen;
                    return ArrayComPortsNames[ArrayComPortsNames.Length-1];
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show(ex.Message);
                    label1.Text = "Error COM: " + ex.Message;
                    label1.ForeColor = Color.Coral;
                    return "Error";
                }
            }
            return "";
        }

        private bool check_connection() {
            using (System.Net.NetworkInformation.Ping png = new System.Net.NetworkInformation.Ping())
            {
                System.Net.IPAddress addr;
                System.Net.IPAddress localhostaddr;
                addr = new System.Net.IPAddress(AddrBytes);
                localhostaddr = new System.Net.IPAddress(LocalhostBytes);

                try
                {
                    if(checkBox1.Checked ? (png.Send(localhostaddr, 1500, new byte[] { 0, 1, 2, 3 }).Status == IPStatus.Success) : (png.Send(addr, 1500, new byte[] { 0, 1, 2, 3 }).Status == IPStatus.Success))
                        return true;
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    return false;
                }
            }
            return false;
        }

        private DataTable SQL_Query(string query) {
            var connection = checkBox1.Checked ? new SqlConnection("Data Source=DESKTOP-BH2UN3B\\SQLEXPRESS;Integrated Security=True") : new SqlConnection("Data Source=192.168.0.16;Initial Catalog=BirrUp;User ID=sa;Password=BirrUp-root;");

            try
            {
                // Table to store the query results
                DataTable table = new DataTable();

                if (check_connection())
                {
                    // Creates a SQL connection
                    using (connection)
                    {
                        connection.Open();

                        // Creates a SQL command
                        using (var command = new SqlCommand(query, connection))
                        {
                            // Loads the query results into the table
                            table.Load(command.ExecuteReader());
                        }

                        connection.Close();

                        label1.Text = "Query done";
                        label1.ForeColor = Color.LawnGreen;

                        return table;
                    }
                }
                else
                {
                    label1.Text = "Sin server";
                    label1.ForeColor = Color.Coral;
                }
            }
            catch (SqlException ex)
            {
                SqlError err = ex.Errors[0];
                string mensaje = string.Empty;
                switch (err.Number)
                {
                    case 109:
                        mensaje = "Problemas con insert"; break;
                    case 110:
                        mensaje = "Más problemas con insert"; break;
                    case 113:
                        mensaje = "Problemas con comentarios"; break;
                    case 156:
                        mensaje = "Error de sintaxis"; break;
                    default:
                        mensaje = err.ToString(); break;
                }
                label1.Text = "Error con BBDD: {0}" + mensaje;
                label1.ForeColor = Color.Coral;
            }
            catch (Exception ex)
            {
                label1.Text = "Error de otra cosa: {0}" + ex.Message;
                label1.ForeColor = Color.Coral;
            }
            finally
            {
                connection.Close();
            }
            return null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Table to store the query results
            DataTable table_query = new DataTable();

            table_query = SQL_Query("SELECT id,type FROM Birras");

            dataGridView1.DataSource = table_query;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int aux;
            if (string.Equals(textBox_Type.Text.ToString(), "")) {
                textBox_Type.BackColor = Color.Coral;
                label1.Text = "Error campo Type";
                label1.ForeColor = Color.Coral;
            }
            else if (textBox_Price.Text == null || !int.TryParse(textBox_Price.Text, out aux)) {
                textBox_Price.BackColor = Color.Coral;
                label1.Text = "Error campo Price";
                label1.ForeColor = Color.Coral;
            }
            else if (textBox_Available.Text != "SI" && textBox_Available.Text != "NO") {
                textBox_Available.BackColor = Color.Coral;
                label1.Text = "Error campo Available";
                label1.ForeColor = Color.Coral;
            }
            else
            {
                // ID no se manda porque está configurado como autoincrement
                string available_aux = string.Equals(textBox_Available.Text.ToString(), "SI") ? "1" : "0";
                string aux_insert = "INSERT INTO Birras VALUES('" + textBox_Type.Text.ToString() + "'," + textBox_Price.Text.ToString() + "," + available_aux + ")";

                SQL_Query(aux_insert);
            }
        }

        private void textBox_Type_TextChanged(object sender, EventArgs e)
        {
            textBox_Type.BackColor = Color.White;
            label1.Text = "";
        }

        private void textBox_Price_TextChanged(object sender, EventArgs e)
        {
            textBox_Price.BackColor = Color.White;
            label1.Text = "";
        }

        private void textBox_Available_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox_Available.BackColor = Color.White;
            label1.Text = "";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string aux_update = null;
            int aux;
            if (button3.Text != "Enable" || int.TryParse(textBox_saldo.Text, out aux))
            {
                switch (button3.Text)
                {
                    case "Add":
                        aux_update = "INSERT INTO Cards VALUES('" + label6.Text.ToString() + "',0,0)";
                        break;
                    case "Enable":
                        aux_update = "UPDATE Cards SET enable = 1,saldo = " + int.Parse(textBox_saldo.Text.ToString()) + " WHERE card LIKE '" + label6.Text.ToString() + "'";
                        break;
                    case "Disable":
                        aux_update = "UPDATE Cards SET enable = 0,saldo = 0 WHERE card LIKE '" + label6.Text.ToString() + "'";
                        break;
                }
                SQL_Query(aux_update);
                label6.Text = "";
                label9.Text = "";
                button3.Text = "Accion";
                textBox_saldo.Text = "";
                textBox_saldo.Enabled = false;
                button3.Enabled = false;
                button4.Enabled = false;
            }
            else
                textBox_saldo.BackColor = Color.Coral;
        }

        private void textBox_saldo_TextChanged(object sender, EventArgs e)
        {
            textBox_saldo.BackColor = Color.White;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string aux_delete = "DELETE FROM Cards WHERE card LIKE '" + label6.Text.ToString() + "'";
            SQL_Query(aux_delete);
            label6.Text = "";
            label9.Text = "";
            button3.Text = "Accion";
            textBox_saldo.Text = "";
            textBox_saldo.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
        }
    }
}
