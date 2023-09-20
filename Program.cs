using System.Data;
using System.Data.Common;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Data.Sqlite;

namespace SQLite
{
    class Program
    {
        static void Main()
        {   
            //Luodaan uusi tietokanta yhteys. Tekee lemmikit.db tämän projektin juureen, jos sitä ei vielä ole.
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
                            string? ownerPhonenumber = Console.ReadLine();
                            // int ownerPhonenumber = Convert.ToInt32(strOwnerPhonenumber);
                            
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
                            Console.WriteLine("Anna puhelinnumerosi, jonka haluat vaihtaa:");
                            string? oldPhonenumber = Console.ReadLine();
                            
                            Console.WriteLine("Anna uusi puhelinnumero:");
                            string? newPhonenumber = Console.ReadLine();

                            UpdatePhonenumber(connection, oldPhonenumber, newPhonenumber);
                            break;

                        case "4": // Etsii lemminkin nimen perusteella omistajan puhelinnumeron
                            Console.WriteLine("Anna lemmikin nimi:");
                            string? petName2 = Console.ReadLine();

                            PrintOwnerPhonenumberByPetName(connection, petName2);
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
                puhnum TEXT NOT NULL)";
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
        static void AddOwner(SqliteConnection connection, string ownerName, string ownerPhonenumber)
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

        // Funktiolle annetan parametriksi vanha ja uusi puhelinnumero. Kaikki joiden puhelinnumero on sama kuin oldPhonenumber päivitetään newPhonenumberiksi
        static void  UpdatePhonenumber(SqliteConnection connection, string oldPhonenumber, string newPhonenumber)
        {
            var updateCmd = connection.CreateCommand();

            updateCmd.CommandText = @"UPDATE Omistajat
            SET puhnum = $newPhonenumber
            WHERE puhnum = $oldPhonenumber";
            updateCmd.Parameters.AddWithValue("$newPhonenumber", newPhonenumber);
            updateCmd.Parameters.AddWithValue("$oldPhonenumber",oldPhonenumber);
            updateCmd.ExecuteNonQuery();
        }

        static void PrintOwnerPhonenumberByPetName(SqliteConnection connection, string petName)
        {
            string petName2 = "";

            // Haetaan lemmikin nimi
            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT nimi FROM Lemmikit WHERE nimi = $petName";
            selectCmd.Parameters.AddWithValue("$petName", petName);
            var result = selectCmd.ExecuteReader();

            result.Read();
            petName2 = result.GetString(0);

            // Haetaan lemmikin omitajan puhelinnumero
            var selectCmd2 = connection.CreateCommand();
            selectCmd2.CommandText = @"SELECT Omistajat.puhnum
            FROM Omistajat, Lemmikit
            WHERE Omistajat.id = Lemmikit.omistaja_id AND Lemmikit.nimi = $petName2";
            selectCmd2.Parameters.AddWithValue("$petName2", petName2);
            var result2 = selectCmd2.ExecuteReader();

            while(result2.Read())
            {
                Console.WriteLine(result2.GetString(0));
            }

        }
    }
}


