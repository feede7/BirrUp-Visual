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


namespace BirrUp
{
    public partial class Form1 : Form
    {
        Random Rand = new Random();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Text = Rand.Next(255).ToString();

            // Table to store the query results
            DataTable table = new DataTable();

            // Creates a SQL connection
            using (var connection = new SqlConnection("Data Source=192.168.0.16;Initial Catalog=BirrUp;User ID=sa;Password=BirrUp-root"))
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
    }
}
