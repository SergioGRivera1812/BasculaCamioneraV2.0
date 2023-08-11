using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace BasculaCamioneraV2._0
{
    public partial class Credenciales : Form
    {
        Conexion conexion;
        Configuracion c = new Configuracion();
        public Credenciales()
        {
            InitializeComponent();
        }

        private void Credenciales_Load(object sender, EventArgs e)
        {
            c.Configuracion_Load(sender, e);

            string s = c.txtIPServer.Text;
            string b = c.txtBase.Text;
            string p = c.txtPortDB.Text;
            string us = c.txtUsuario.Text;
            string pass = c.txtPass.Text;
            
            

            conexion = new Conexion(s, b, p, us, pass);

            dataGridCredenciales.DataSource = load();
            txtClave.Text = ObtenerNuevaClave();

        }

        private BindingSource bs = new BindingSource();

        private DataTable load()
        {
            DataTable vista = new DataTable();
            string vista_gral = "select*from Camiones;";
            using (MySqlCommand cmd = new MySqlCommand(vista_gral, conexion.GetConexion()))
            {
                MySqlDataReader reader = cmd.ExecuteReader();
                vista.Load(reader);

            }
            bs.DataSource = vista;
            return vista;
        }

        private void btnAlta_Click(object sender, EventArgs e)
        {
            string Guardar = "INSERT INTO Camiones(Clave,Placas,Chofer) values ('" +
                      txtClave.Text + "','" + txtPlacas.Text + "','" + txtNombre.Text + "');";

            MySqlCommand comando = new MySqlCommand(Guardar, conexion.GetConexion());
            comando.ExecuteNonQuery();
            MessageBox.Show("Credencial Creada", "ACCIÓN", MessageBoxButtons.OK, MessageBoxIcon.Question);
            dataGridCredenciales.DataSource = load();

            Zen.Barcode.Code128BarcodeDraw mGeneradorCB = Zen.Barcode.BarcodeDrawFactory.Code128WithChecksum;

            pictureBox1.Image = mGeneradorCB.Draw(txtClave.Text, 10);
            printDocument1 = new PrintDocument();
            PrinterSettings ps = new PrinterSettings();
            printDocument1.PrinterSettings = ps;
            printDocument1.PrintPage += Imprimir;
            printDocument1.Print();

            txtClave.Text = ObtenerNuevaClave();
            txtNombre.Text = "";
            txtPlacas.Text = "";

        }

        private void Imprimir(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(pictureBox1.Image, 10, 80, 120, 110);


        }

        private string ObtenerNuevaClave()
        {
            MySqlConnection connection = new MySqlConnection(conexion.getConexion());

            connection.Open();

            // Consulta para obtener el último consecutivo
            string query = "SELECT MAX(SUBSTRING(Clave, 3, LENGTH(Clave))) AS UltimoConsecutivo FROM Camiones";

            MySqlCommand command = new MySqlCommand(query, connection);

            // Ejecutar la consulta y obtener el resultado
            MySqlDataReader reader = command.ExecuteReader();

            int ultimoConsecutivo = 0;

            if (reader.Read() && !reader.IsDBNull(0))
            {
                string ultimaClave = reader.GetString(0);
                string numeroConsecutivo = ultimaClave.Substring(2);
                ultimoConsecutivo = int.Parse(numeroConsecutivo);
            }
            reader.Close();

            // Incrementar el último consecutivo y generar la nueva clave
            ultimoConsecutivo++;
            string nuevaClave = "C-" + ultimoConsecutivo.ToString("000");

            return nuevaClave;


        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }
    }
}
