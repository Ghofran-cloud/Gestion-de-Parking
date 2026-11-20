using System.Windows.Forms;

namespace GestionParking.Forms
{
   
    public class FormParking : Form
    {
        public FormParking()
        {
            Theme.AppliquerFenetre(this, "Parking (en construction)", 900, 600);
            Controls.Add(Theme.CreerEntete("Parking", "Écran en construction"));
        }
    }
}
