using System.Data;
using System.Data.Common;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Data.Sqlite;

// -Lisää kantaan lemmikkejä(id, nimi, laji, omistajan_id)

// -Päivittää omistajan puhelinnumeron

// -Etsii lemminkin nimen perusteella omistajan puhelinnumeron

namespace SQLite
{
    class Program
    {
        static void Main()
        {   
            //Luodaan uusi tietokanta yhteys
            using(var connection = new SqliteConnection("Data Source=lemmikit.db"))
            {
                connection.Open();

                CreateTables(connection);

                while(true)
                {
                    //Komentorivi käyttöliittymä
                    Console.WriteLine("Mitä haluat tehdä? (1) Lisää omistaja (2) Lisää lemmikki (3) Päivitä omistajan puhelinnumero (4) Etsi lemmikin nimi omistajan puhelinnumeron perusteella (5) Lopeta");
                    string? input = Console.ReadLine();

                    switch(input)
                    {
                        case "1": // Lisää kantaan Omistajia (id, nimi, puhelin)
                            Console.WriteLine("Anna omistajan nimi:");
                            string? ownerName = Console.ReadLine();

                            Console.WriteLine("Anna omistajan puhelinnumero:");
                            string? strOwnerPhonenumber = Console.ReadLine();
                            int ownerPhonenumber = Convert.ToInt32(strOwnerPhonenumber);
                            
                            AddOwner(connection, ownerName, ownerPhonenumber);
                            break;

                        case "2": // Lisää kantaan lemmikkejä(id, nimi, laji, omistajan_id) // Kysyy puhelinnumeron, jonka perusteella se tarkistaa omistajan Idn ja siten osaa lisätä oikean omistaja_id:n.
                            Console.WriteLine("Anna lemmikin nimi:");
                            string? petName = Console.ReadLine();

                            Console.WriteLine("Anna lemmikin laji:");
                            string? species = Console.ReadLine();

                            Console.WriteLine("Anna omistajan puhelinnumero:");
                            string? petOwnerPhonenumber = Console.ReadLine();

                            AddPet(connection, petName, species, petOwnerPhonenumber);
                            break;

                        case "3": // Päivittää omistajan puhelinnumeron
                            break;

                        case "4": // Etsi lemmikin nimi omistajan puhelinnumeron perusteella
                            break;

                        case "5": // Lopeta
                            connection.Close();
                            return;

                        default:
                            Console.WriteLine("Virheellinen syöte");
                            break;
                    }
                }

            }
        }

        static void CreateTables(SqliteConnection connection)
        {
            //Luodaan taulu Asiakkaat        
            var createTableOmistajat = connection.CreateCommand();    
            createTableOmistajat.CommandText = @"CREATE TABLE IF NOT EXISTS Omistajat (
                id INTEGER PRIMARY KEY,
                nimi TEXT NOT NULL,
                puhnum INTEGER NOT NULL)";
            createTableOmistajat.ExecuteNonQuery();

            //Luodaan taulu Tuotteet
            var createTableCmd2 = connection.CreateCommand();
            createTableCmd2.CommandText = 
            @"CREATE TABLE IF NOT EXISTS Lemmikit (
                id INTEGER PRIMARY KEY,
                nimi TEXT NOT NULL,
                laji TEXT NOT NULL,
                omistaja_id INTEGER,
                FOREIGN KEY (omistaja_id) REFERENCES Omistajat(id))";
            createTableCmd2.ExecuteNonQuery();
        }

        // Lisätään omistaja tietokantaan
        static void AddOwner(SqliteConnection connection, string ownerName, int ownerPhonenumber)
        {
            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = "INSERT INTO Omistajat (nimi, puhnum) VALUES ($nimi, $puhnum)";
            insertCmd.Parameters.AddWithValue("$nimi", ownerName);
            insertCmd.Parameters.AddWithValue("$puhnum", ownerPhonenumber);
            insertCmd.ExecuteNonQuery();
        }
        
        // Lisää kantaan lemmikkejä(id, nimi, laji, omistajan_id)
        static void AddPet(SqliteConnection connection, string petName, string petSpecie, string ownerPhoneNumber)
        {
            int ownerId = GetOwnerIdByPhoneNumber(connection, ownerPhoneNumber);

            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = "INSERT INTO Lemmikit (nimi, laji, omistaja_id) VALUES ($nimi, $laji, $omistaja_id)";
            insertCmd.Parameters.AddWithValue("$nimi", petName);
            insertCmd.Parameters.AddWithValue("$laji", petSpecie);
            insertCmd.Parameters.AddWithValue("$omistaja_id", ownerId);
            insertCmd.ExecuteNonQuery();
        }
        
        // Hae omistajan ID puhelinnumeron perusteella
        static int GetOwnerIdByPhoneNumber(SqliteConnection connection, string ownerPhonenumber)
        {
            int ownerId = 0;

            var getCmd = connection.CreateCommand();

            // Valitse id tablesta Omistajat missä puhelinnumero on sama kuin parametrillä ownerPhonenumber
            getCmd.CommandText = "SELECT id FROM Omistajat WHERE puhnum = $puhnum";
            getCmd.Parameters.AddWithValue("$puhnum", ownerPhonenumber);
            var result = getCmd.ExecuteReader();

            if (result.Read())
            {
                ownerId = result.GetInt32(0);
            }

            return ownerId;
        }
    }
}


