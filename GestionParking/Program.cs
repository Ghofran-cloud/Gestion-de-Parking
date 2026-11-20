using System;
using System.Windows.Forms;
using GestionParking.Data;
using GestionParking.Forms;
using Microsoft.EntityFrameworkCore;

namespace GestionParking
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            
            using (var db = new ParkingContext())
            {
                db.Database.Migrate();
            }

            Application.Run(new MainForm());
        }
    }
}

