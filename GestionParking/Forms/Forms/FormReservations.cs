using System.Windows.Forms;

namespace GestionParking.Forms
{
    
    public class FormReservations : Form
    {
        public FormReservations()
        {
            Theme.AppliquerFenetre(this, "Réservations (en construction)", 900, 600);
            Controls.Add(Theme.CreerEntete("Réservations", "Écran en construction"));
        }
    }
}
