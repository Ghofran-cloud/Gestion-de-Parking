using System.Windows.Forms;

namespace GestionParking.Forms
{
    public class FormClients : Form
    {
        public FormClients()
        {
            Theme.AppliquerFenetre(this, "Clients (en construction)", 900, 600);
            Controls.Add(Theme.CreerEntete("Clients", "Écran en construction"));
        }
    }
}
