using BasculaCamioneraV2._0.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BasculaCamioneraV2._0
{
    public partial class Configuracion : Form
    {
       
        System.Diagnostics.Process oskProcess = null;

        [DllImport("user32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        public static extern int SendMessage(int hWnd, uint Msg, int wParam, int lParam);

        public const int WM_SYSCOMMAND = 0x0112;
        public const int SC_CLOSE = 0xF060;
        public Configuracion()
        {
            InitializeComponent();
        }

        public void Configuracion_Load(object sender, EventArgs e)
        {
            CargarConfiguracionCOM();
        }

        private void CargarConfiguracionCOM()
        {
            txtIPServer.Text = Settings.Default.IpServer;
            txtBase.Text = Settings.Default.DataBase;
            txtUsuario.Text = Settings.Default.Usuario;
            txtPass.Text = Settings.Default.Password;
            txtPortDB.Text = Settings.Default.DataPort;

            txtBaudio.Text = Settings.Default.Baudio;
            txtCOM.Text = Settings.Default.COM;

        }

        private void GuardarCOM()
        {
            Settings.Default.IpServer = txtIPServer.Text;
            Settings.Default.DataBase = txtBase.Text;
            Settings.Default.Usuario = txtUsuario.Text;
            Settings.Default.Password = txtPass.Text;
            Settings.Default.DataPort = txtPortDB.Text;

            Settings.Default.Baudio = txtBaudio.Text;
            Settings.Default.COM = txtCOM.Text;
            Settings.Default.Save();
        }

        private void Configuracion_FormClosed(object sender, FormClosedEventArgs e)
        {
            Settings.Default.IpServer = txtIPServer.Text;
            Settings.Default.DataBase = txtBase.Text;
            Settings.Default.Usuario = txtUsuario.Text;
            Settings.Default.Password = txtPass.Text;
            Settings.Default.DataPort = txtPortDB.Text;

            Settings.Default.Baudio = txtBaudio.Text;
            Settings.Default.COM = txtCOM.Text;
        }

        
        private void txtIPServer_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                closeOnscreenKeyboard();
            }
        }
        private void closeOnscreenKeyboard()
        {

            int iHandle = FindWindow("IPTIP_Main_Window", "");
            if (iHandle > 0)
            {

                SendMessage(iHandle, WM_SYSCOMMAND, SC_CLOSE, 0);
            }
        }

        
        private void txtIPServer_Click_1(object sender, EventArgs e)
        {
            string progFiles = @"C:\Program Files\Common Files\Microsoft Shared\ink";
            string keyboardPath = Path.Combine(progFiles, "TabTip.exe");
            oskProcess = Process.Start(keyboardPath);
        }

        private void btnGuardar_Click_1(object sender, EventArgs e)
        {
            try
            {
                GuardarCOM();
                MessageBox.Show("Configuración guardada", "AVISO", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Visible = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la configuración", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtIPServer_KeyPress_1(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                closeOnscreenKeyboard();
            }
        }
    }

}

