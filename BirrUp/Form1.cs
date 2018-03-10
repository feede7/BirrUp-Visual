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



namespace BirrUp
{
    public partial class Form1 : Form
    {
        Random Rand = new Random();
        byte[] AddrBytes = new byte[] { 192, 168, 0, 16 }; // byte array for server address.
        byte[] LocalhostBytes = new byte[] { 127, 0, 0, 1 }; // byte array for server address.
        bool netOK = false;

        public Form1()
        {
            InitializeComponent();
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

        private void SQL_Query(string query) {
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

                        dataGridView1.DataSource = table;

                        label1.Text = "Query done";
                        label1.ForeColor = Color.LawnGreen;

                        connection.Close();
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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Text = Rand.Next(255).ToString();

            SQL_Query("SELECT id,type FROM Birras");
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
    }
}
