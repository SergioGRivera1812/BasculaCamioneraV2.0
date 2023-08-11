using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO.Ports;
using System.Windows.Forms;

namespace BasculaCamioneraV2._0
{
    public partial class Form1 : Form
    {
        private delegate void DelegadoAcceso(string accion);
        Configuracion c = new Configuracion();

        Conexion conexion;
        public Form1()
        {
            InitializeComponent();

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                c.Configuracion_Load(sender, e);

                string s = c.txtIPServer.Text;
                string b = c.txtBase.Text;
                string p = c.txtPortDB.Text;
                string us = c.txtUsuario.Text;
                string pass = c.txtPass.Text;
                string com = c.txtCOM.Text;
                int ba = Convert.ToInt32(c.txtBaudio.Text);

                conexion = new Conexion(s, b, p, us, pass);


                dataGridVista.DataSource = load();
                dataGridVista.Columns["Clave"].DisplayIndex = 0;
                dataGridVista.Columns["Fecha"].DisplayIndex = 1;
                dataGridVista.Columns["Placas"].DisplayIndex = 2;
                dataGridVista.Columns["Chofer"].DisplayIndex = 3;
                dataGridVista.Columns["HoraE"].DisplayIndex = 4;
                dataGridVista.Columns["HoraS"].DisplayIndex = 5;
                dataGridVista.Columns["PTara"].DisplayIndex = 6;
                dataGridVista.Columns["PNeto"].DisplayIndex = 7;
                dataGridVista.Columns["PBruto"].DisplayIndex = 8;


                serialPort1 = new SerialPort(com, ba, Parity.None, 8, StopBits.One);
                serialPort1.Handshake = Handshake.None;
                serialPort1.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
                serialPort1.ReadTimeout = 500;
                serialPort1.WriteTimeout = 500;
                serialPort1.Open();
                serialPort1.Write("P");
                

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Indicador.Text = "ERROR";
            }
        }

        void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (this.Enabled == false)
            {
                MessageBox.Show("Error de comunicación", "ERROR");
            }
            else
            {
                try
                {
                    string data = serialPort1.ReadLine();

                    data = data.Replace("(kg)", "");
                    data = data.Replace("=", "");
                    string cadenaLimpia = data.Replace("\n", string.Empty).Replace("\t", string.Empty).Replace("", "").Replace(" ", "");
                    // Eliminar los ceros no significativos antes del punto decimal
                    cadenaLimpia = cadenaLimpia.TrimStart('0');

                    if (cadenaLimpia.StartsWith("."))

                        // Comprobar si solo hay un dígito después del punto decimal
                        cadenaLimpia = "0" + cadenaLimpia;  // Agregar un cero antes del punto decimal si es necesario
                    else
                        cadenaLimpia = cadenaLimpia.TrimStart('.');  // Eliminar el punto decimal inicial si hay más de un dígito después de él

                    this.BeginInvoke(new DelegadoAcceso(si_DataReceived), new object[] { cadenaLimpia });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("El indicador no envia datos " + ex, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }

        }
        private void si_DataReceived(string accion)
        {
            Indicador.Text = accion;
        }

        private void ID_KeyPress(object sender, KeyPressEventArgs e)
        {
            string cadena = Indicador.Text;
            String cadena2;
            cadena = cadena.Replace("KG", "");
            cadena2 = cadena.Replace("M", "");

            string cadenaLimpia = cadena2.Replace("\n", string.Empty).Replace("\t", string.Empty).Replace("", "").Replace(" ", "");
            int tara = Int32.Parse(cadenaLimpia);
            MySqlConnection connection = new MySqlConnection(conexion.getConexion());
           
            connection.Open();

            string queryS = "SELECT PTara, PBruto, PNeto, HoraS,Activo FROM RegistroC WHERE ID = @ID;";

            // Supongamos que se presiona la tecla 13 (Enter) y la variable ID.Text tiene el valor "C-001S"
            if (e.KeyChar == 13 && ID.Text.EndsWith("S"))
            {
                string idLimpioS = ID.Text.Replace("C-00", string.Empty).Replace("S", string.Empty);
                int ids = Convert.ToInt32(idLimpioS);
                e.Handled = true;

                // Verificar si el registro correspondiente existe y está activo en la tabla
                string selectQuery = "SELECT COUNT(*) FROM RegistroC WHERE IdCamion = ? AND Activo = 1;";
                MySqlCommand selectCommand = new MySqlCommand(selectQuery, conexion.GetConexion());
                selectCommand.Parameters.AddWithValue("@IdCamion", ids); // El nombre del parámetro no importa en MySQL, solo el orden

                int count = Convert.ToInt32(selectCommand.ExecuteScalar());

                if (count == 0)
                {
                    MessageBox.Show("El registro con clave " + idLimpioS + " no existe o no está activo.");
                }
                else
                {
                    // Obtener los valores de PTara, PBruto, PNeto y Activo del registro
                    string selectValuesQuery = "SELECT PTara, PBruto, PNeto, Activo FROM RegistroC WHERE IdCamion = ? AND Activo = 1;";
                    MySqlCommand selectValuesCommand = new MySqlCommand(selectValuesQuery, conexion.GetConexion());
                    selectValuesCommand.Parameters.AddWithValue("@IdCamion", idLimpioS);

                    using (MySqlDataReader reader = selectValuesCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int ptara = Convert.ToInt32(reader["PTara"]);
                            int pbruto = Convert.ToInt32(reader["PBruto"]);
                            int pneto = Convert.ToInt32(reader["PNeto"]);
                            bool activo = Convert.ToBoolean(reader["Activo"]);

                            int B = Int32.Parse(cadenaLimpia);
                            int T = ptara;
                            int N = B - T;
                            int ID = Int32.Parse(idLimpioS);

                            // Realizar la actualización del registro
                            string updateQuery = "UPDATE RegistroC SET HoraS = ?, PNeto = ?, PBruto= ?,Activo= ? WHERE IdCamion = ? AND Activo = 1;";
                            MySqlCommand updateCommand = new MySqlCommand(updateQuery, conexion.GetConexion());
                            updateCommand.Parameters.AddWithValue("@HoraS", DateTime.Now.ToString("t")); // Valor de la hora de salida
                            updateCommand.Parameters.AddWithValue("@PNeto", N); // Valor del PNeto correspondiente
                            updateCommand.Parameters.AddWithValue("@PBruto", B);
                            updateCommand.Parameters.AddWithValue("@IdCamion",ID);
                            updateCommand.Parameters.AddWithValue("@Activo", false);



                            reader.Close();
                            int rowsAffected = updateCommand.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Se ha actualizado el registro correspondiente a la clave " + idLimpioS + " con éxito.");
                                // Realizar aquí las acciones adicionales que desees después de la actualización exitosa
                                dataGridVista.DataSource = load();


                            }
                            else
                            {
                                MessageBox.Show("No se pudo actualizar el registro correspondiente a la clave " + idLimpioS + ".");
                                // Realizar aquí las acciones adicionales que desees en caso de fallo en la actualización
                            }
                            connection.Close();

                        }


                    }/*
                    string id = ID.Text.Replace("S", String.Empty);
                    string puerto = "COM4";  // Ajusta el puerto según corresponda (por ejemplo, COM1, COM2, etc.)
                    int baudRate = 9600;    // Ajusta la velocidad del puerto según corresponda
                    Parity parity = Parity.None;
                    int dataBits = 8;
                    StopBits stopBits = StopBits.One;

                    using (SerialPort puertoImpresora = new SerialPort(puerto, baudRate, parity, dataBits, stopBits))
                    {
                        //Acceso a la información
                        string query = "SELECT PTara, PBruto, PNeto, Clave, HoraE, HoraS, Placas FROM VistaRegistroC WHERE Clave = ? ORDER BY ID DESC LIMIT 1;";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Clave", id);

                        connection.Open();

                        MySqlDataReader readerT = command.ExecuteReader();

                        if (readerT.Read())
                        {
                            string ptara = readerT["PTara"].ToString();
                            string pbruto = readerT["PBruto"].ToString();
                            string pneto = readerT["PNeto"].ToString();
                            string clave = readerT["Clave"].ToString();
                            string HoraE = readerT["HoraE"].ToString();
                            string HoraS = readerT["HoraS"].ToString();
                            string Placas = readerT["Placas"].ToString();

                            puertoImpresora.Open();
                            puertoImpresora.WriteLine("                                    ");
                            puertoImpresora.WriteLine("Salida :  ");
                            puertoImpresora.WriteLine(DateTime.Now.ToString("d") + "  " + " " + HoraS);
                            puertoImpresora.WriteLine("Clave: " + clave);
                            puertoImpresora.WriteLine("Placas:  " + Placas);
                            puertoImpresora.WriteLine("                                    ");
                            puertoImpresora.WriteLine("Peso Bruto:  " + pbruto + "KG G");
                            puertoImpresora.WriteLine("Tara:        " + ptara + "KG T");
                            puertoImpresora.WriteLine("Peso Neto:   " + pneto + "KG N");
                            readerT.Close();
                            puertoImpresora.Close();
                        }
                        connection.Close();
                    

                    }*/
                }
            }
            else if (e.KeyChar == 13)
            {
                try
                {
                    e.Handled = true;
                    string id = ID.Text;
                    
                     // Verificar si el ID existe en la tabla referenciada y está activo
                    string idLimpio = id.Replace("C-00", string.Empty);
                    int ids = Convert.ToInt32(idLimpio);
                    
                    // Verificar si el ID existe en la tabla referenciada y está activo
                    string selectQuery = "SELECT COUNT(*) FROM RegistroC WHERE IdCamion = ? AND Activo = 1;";
                    MySqlCommand selectCommand = new MySqlCommand(selectQuery, conexion.GetConexion());
                    selectCommand.Parameters.AddWithValue("@IdCamion", ids); // El nombre del parámetro no importa en MySQL, solo el orden

                    int count = Convert.ToInt32(selectCommand.ExecuteScalar());

                        if (count > 0)
                        {
                            // El ID existe y está activo en la tabla referenciada
                            MessageBox.Show("Esta clave ya fue escaneada", "AVISO", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            ID.Text = "";

                            return; // Salir del método o realizar alguna otra acción
                        }
                    

                    // Verificar si ya existe un registro con la misma clave foránea activa
                    /*string existingSelectQuery = "SELECT COUNT(*) FROM RegistroC WHERE IdCamion = ? AND Activo = 1;";

                    MySqlCommand existingSelectCommand = new MySqlCommand(existingSelectQuery, conexion.GetConexion());

                        existingSelectCommand.Parameters.AddWithValue("@IdCamion", ids);

                        int existingCount = (int)existingSelectCommand.ExecuteScalar();

                        if (existingCount > 0)
                        {
                            // Ya existe un registro con la misma clave foránea activa en la tabla referenciada
                            MessageBox.Show("Esta clave ya fue escaneada", "AVISO", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            ID.Text = "";


                            return; // Salir del método o realizar alguna otra acción
                        }*/
                    

                    // Insertar un nuevo registro con el ID
                    // this.registroCTableAdapter2.Insert(tara, Fecha.Text, Hora.Text, "", 0, 0, Convert.ToInt32(idLimpio), true);
                    string Guardar = "INSERT INTO RegistroC (Ptara,Fecha,HoraE,HoraS,PBruto,PNeto,IdCamion,Activo) values ('" +
                       tara + "','" + DateTime.Now.ToString("d") + "','" + DateTime.Now.ToString("t")+ "','"+ "" + "','"+ 0 + "','" + 0 + "','" + ids + "','"+1+ "');";

                    MySqlCommand comando = new MySqlCommand(Guardar, conexion.GetConexion());
                    comando.ExecuteNonQuery();
                    dataGridVista.DataSource = load();



                    string puerto = "COM4";  // Ajusta el puerto según corresponda (por ejemplo, COM1, COM2, etc.)
                    int baudRate = 9600;    // Ajusta la velocidad del puerto según corresponda
                    Parity parity = Parity.None;
                    int dataBits = 8;
                    StopBits stopBits = StopBits.One;
                    /*
                    using (SerialPort puertoImpresora = new SerialPort(puerto, baudRate, parity, dataBits, stopBits))
                    {
                        //Acceso a la información
                        string query = "SELECT TOP 1 PTara, PBruto, PNeto, Clave, HoraE, HoraS, Placas FROM VistaRegistroC WHERE Clave = ? ORDER BY ID DESC;";
                        MySqlCommand command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@Clave", id);

                        connection.Open();

                        MySqlDataReader readerT = command.ExecuteReader();

                        if (readerT.Read())
                        {
                            string ptara = readerT["PTara"].ToString();
                            string pbruto = readerT["PBruto"].ToString();
                            string pneto = readerT["PNeto"].ToString();
                            string clave = readerT["Clave"].ToString();
                            string HoraE = readerT["HoraE"].ToString();
                            string HoraS = readerT["HoraS"].ToString();
                            string Placas = readerT["Placas"].ToString();

                            puertoImpresora.Open();
                            puertoImpresora.WriteLine("    Rancho el Compas ");
                            puertoImpresora.WriteLine("    Bascula Camionera");
                            puertoImpresora.WriteLine("                                    ");
                            puertoImpresora.WriteLine("Entrada:  ");
                            puertoImpresora.WriteLine("" + DateTime.Now.ToString("d") + "  " + HoraE);
                            puertoImpresora.WriteLine("Peso de Entrada :  " + ptara + " Kg");
                            puertoImpresora.WriteLine("                                    ");
                            readerT.Close();
                            puertoImpresora.Close();
                        }
                        connection.Close();


                    }*/
                    ID.Text = "";


                }
                catch (MySqlException E)
                {
                    MessageBox.Show("Esta clave no esta dada de alta   ", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private BindingSource bs = new BindingSource();

        private DataTable load()
        {
            DataTable vista = new DataTable();
            string vista_gral = "select*from VistaRegistroC;";
            using (MySqlCommand cmd = new MySqlCommand(vista_gral,conexion.GetConexion()))
            {
                MySqlDataReader reader = cmd.ExecuteReader();
                vista.Load(reader);
                
            }
            bs.DataSource = vista;
            return vista;
        }

        private void extrasToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Credenciales c = new Credenciales();
            c.Show();
            
        }

        private void configuraciónToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Configuracion c = new Configuracion();
            c.Show();
        }

        private void salirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
