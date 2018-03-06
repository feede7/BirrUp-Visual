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



namespace BirrUp
{
    public partial class Form1 : Form
    {
        Random Rand = new Random();
        byte[] AddrBytes = new byte[] { 192, 168, 0, 16 }; // byte array for server address.
        bool netOK = false;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Text = Rand.Next(255).ToString();

            var connection = new SqlConnection("Data Source=192.168.0.16;Initial Catalog=BirrUp;User ID=sa;Password=BirrUp-root");

            try
            {
                // Table to store the query results
                DataTable table = new DataTable();

                if (netOK)
                {
                    // Creates a SQL connection
                    using (connection)
                    {
                        connection.Open();

                        // Creates a SQL command
                        using (var command = new SqlCommand("SELECT id,type FROM Birras", connection))
                        {
                            // Loads the query results into the table
                            table.Load(command.ExecuteReader());
                        }

                        dataGridView1.DataSource = table;
                        //dataGridView1.AutoResizeRows();
                        // dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader);

                        connection.Close();
                    }
                }
                else
                    Console.WriteLine("Sin server");
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

                Console.WriteLine("Error con BBDD: {0}", mensaje);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error de otra cosa: {0}", ex.Message);
            }
            finally
            {
                connection.Close();
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            using (System.Net.NetworkInformation.Ping png = new System.Net.NetworkInformation.Ping())
            {
                System.Net.IPAddress addr;
                addr = new System.Net.IPAddress(AddrBytes);

                try
                {
                    netOK = (png.Send(addr, 1500, new byte[] { 0, 1, 2, 3 }).Status == IPStatus.Success);
                }

                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    netOK = false;
                }
            }

            if (netOK)
            {
                label1.Text = "Server conectado";
                label1.ForeColor = Color.LawnGreen;
            }
            else
            {
                label1.Text = "Server desconectado";
                label1.ForeColor = Color.Coral;
            }
        }
    }
}
