using System.Drawing;
using System.Windows.Forms;

namespace GestionParking.Forms
{
   
    public static class Theme
    {
       
        public static readonly Color Fond = Color.FromArgb(241, 245, 249);      
        public static readonly Color Carte = Color.White;
        public static readonly Color Entete = Color.FromArgb(30, 41, 59);       
        public static readonly Color Bleu = Color.FromArgb(37, 99, 235);
        public static readonly Color Vert = Color.FromArgb(22, 163, 74);
        public static readonly Color Rouge = Color.FromArgb(220, 38, 38);
        public static readonly Color Orange = Color.FromArgb(234, 88, 12);
        public static readonly Color Gris = Color.FromArgb(100, 116, 139);
        public static readonly Color Texte = Color.FromArgb(15, 23, 42);
        public static readonly Color Bordure = Color.FromArgb(226, 232, 240);

        
        public static readonly Font Normale = new Font("Segoe UI", 10F);
        public static readonly Font Gras = new Font("Segoe UI Semibold", 10F);
        public static readonly Font Titre = new Font("Segoe UI Semibold", 18F);
        public static readonly Font SousTitre = new Font("Segoe UI", 10F);
        public static readonly Font Chiffre = new Font("Segoe UI Semibold", 22F);

        
        public static void AppliquerFenetre(Form f, string titre, int largeur, int hauteur)
        {
            f.Text = titre;
            f.Size = new Size(largeur, hauteur);
            f.MinimumSize = new Size(largeur - 150, hauteur - 100);
            f.StartPosition = FormStartPosition.CenterScreen;
            f.BackColor = Fond;
            f.Font = Normale;
            f.ForeColor = Texte;
        }

        
        public static Panel CreerEntete(string titre, string sousTitre)
        {
            var panel = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Entete };
            panel.Controls.Add(new Label
            {
                Text = titre, Font = Titre, ForeColor = Color.White,
                AutoSize = true, Location = new Point(24, 12)
            });
            panel.Controls.Add(new Label
            {
                Text = sousTitre, Font = SousTitre, ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true, Location = new Point(27, 48)
            });
            return panel;
        }

        
        public static Button CreerBouton(string texte, Color couleur, int largeur = 140)
        {
            var b = new Button
            {
                Text = texte,
                Width = largeur,
                Height = 38,
                BackColor = couleur,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = Gras,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 8)
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(couleur, 0.1f);
            return b;
        }

        
        public static Panel CreerPanneauDroite(string titre, int largeur = 340)
        {
            var p = new Panel
            {
                Dock = DockStyle.Right,
                Width = largeur,
                BackColor = Carte,
                Padding = new Padding(20),
                AutoScroll = true
            };
            p.Controls.Add(new Label { Text = titre, Font = new Font("Segoe UI Semibold", 13F), AutoSize = true, Location = new Point(20, 16) });
            return p;
        }

        
        public static void AjouterChamp(Control parent, string libelle, Control champ, ref int y)
        {
            parent.Controls.Add(new Label { Text = libelle, ForeColor = Gris, AutoSize = true, Location = new Point(20, y) });
            champ.Location = new Point(20, y + 22);
            champ.Width = parent.Width - 50;
            champ.Font = Normale;
            parent.Controls.Add(champ);
            y += 60;
        }

        
        public static FlowLayoutPanel CreerRangeeBoutons(Control parent, int y, params Button[] boutons)
        {
            var flow = new FlowLayoutPanel
            {
                Location = new Point(20, y),
                Width = parent.Width - 40,
                Height = (boutons.Length + 1) / 2 * 46,   
                BackColor = Color.Transparent
            };
            flow.Controls.AddRange(boutons);
            parent.Controls.Add(flow);
            return flow;
        }

        
        public static void StyliserGrille(DataGridView g)
        {
            g.Dock = DockStyle.Fill;
            g.BackgroundColor = Carte;
            g.BorderStyle = BorderStyle.None;
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.AllowUserToResizeRows = false;
            g.MultiSelect = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            g.RowHeadersVisible = false;
            g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            g.GridColor = Bordure;
            g.RowTemplate.Height = 34;

            g.EnableHeadersVisualStyles = false;
            g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            g.ColumnHeadersHeight = 40;
            g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            g.ColumnHeadersDefaultCellStyle.BackColor = Bleu;
            g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            g.ColumnHeadersDefaultCellStyle.Font = Gras;
            g.ColumnHeadersDefaultCellStyle.SelectionBackColor = Bleu;

            g.DefaultCellStyle.Font = Normale;
            g.DefaultCellStyle.ForeColor = Texte;
            g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(219, 234, 254);
            g.DefaultCellStyle.SelectionForeColor = Texte;
            g.DefaultCellStyle.Padding = new Padding(6, 0, 6, 0);
            g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);

            
            g.DataBindingComplete += (s, e) =>
            {
                if (g.Columns.Contains("Id"))
                {
                    g.Columns["Id"]!.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    g.Columns["Id"]!.Width = 55;
                }
            };
        }

        
        public static void ColorerStatuts(DataGridView g, string nomColonne = "Statut")
        {
            g.CellFormatting += (s, e) =>
            {
                if (e.CellStyle == null || g.Columns[e.ColumnIndex].Name != nomColonne || e.Value == null) return;

                e.CellStyle.Font = Gras;
                switch (e.Value.ToString())
                {
                    case "LIBRE":
                    case "CONFIRMEE":
                    case "EFFECTUE":
                        e.CellStyle.ForeColor = Vert; break;
                    case "RESERVEE":
                    case "EN_ATTENTE":
                        e.CellStyle.ForeColor = Orange; break;
                    case "OCCUPEE":
                    case "ANNULEE":
                    case "REMBOURSE":
                        e.CellStyle.ForeColor = Rouge; break;
                    default:
                        e.CellStyle.ForeColor = Gris; break;
                }
            };
        }

        
        public static int? IdSelectionne(DataGridView g)
        {
            if (g.CurrentRow == null || !g.Columns.Contains("Id")) return null;
            return g.CurrentRow.Cells["Id"].Value as int?;
        }

        
        public static void Info(string message) =>
            MessageBox.Show(message, "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);

        public static void Erreur(string message) =>
            MessageBox.Show(message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Warning);

        public static bool Confirmer(string message) =>
            MessageBox.Show(message, "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
    }
}
