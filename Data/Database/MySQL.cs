//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Data;
//using System.Configuration;
////using MySql.Data.MySqlClient;
//
//namespace Data
//{
//    public class MySqlClient : CardStore
//    {
//        private MySqlConnection sql;
//        private static string MySqlConnectionString = ConfigurationManager.AppSettings["MySqlConnectionString"];
//
//        private DataRow dbRow(String query)
//        {
//            MySqlCommand command = sql.CreateCommand();
//            command.CommandText = query;
//
//            DataTable selectDT = new DataTable();
//            MySqlDataAdapter dataAd = new MySqlDataAdapter(command);
//
//            dataAd.Fill(selectDT);
//
//            if (selectDT.Rows.Count > 0)
//                return selectDT.Rows[0];
//            else
//                return null;
//        }
//
//        private int lastInsertId()
//        {
//            DataRow r = dbRow("SELECT last_insert_id() as lid");
//
//            Int64 id = (Int64)r[0];
//
//            return (int)id;
//        }
//
//        private int affectedRows()
//        {
//            DataRow r = dbRow("SELECT ROW_COUNT()");
//            int id = (int)r[0];
//
//            return id;
//        }
//
//        private DataTable dbResult(String query)
//        {
//            MySqlCommand command = sql.CreateCommand();
//            command.CommandText = query;
//
//            DataTable selectDT = new DataTable();
//            MySqlDataAdapter dataAd = new MySqlDataAdapter(command);
//
//            dataAd.Fill(selectDT);
//
//            return selectDT;
//
//        }
//
//        internal int dbNone(string query)
//        {
//            MySqlCommand command = sql.CreateCommand();
//            command.CommandText = query;
//            return command.ExecuteNonQuery();
//        }
//
//        public MySqlClient()
//        {
//            sql = new MySqlConnection(MySqlConnectionString);
//            sql.Open();
//        }
//
//        private DateTime ConvertFromUnixTimestamp(double timestamp)
//        {
//            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
//            return origin.AddSeconds(timestamp);
//        }
//
//        private double ConvertToUnixTimestamp(DateTime date)
//        {
//            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
//            TimeSpan diff = date - origin;
//            return Math.Floor(diff.TotalSeconds);
//        }
//
//        public IEnumerable<ReferenceCard> GetCards()
//        {
//            var ret = new List<ReferenceCard>();
//
//            var reader = this.dbResult("SELECT * FROM cards");
//            foreach (DataRow r in reader.Rows)
//            {
//                ReferenceCard card = new ReferenceCard();
//                card.cardId = (String)r["id"];
//                card.name = (String)r["Name"];
//                card.pHash = (UInt64)r["pHash"];
//                card.dataRow = r;
//
//                ret.Add(card);
//            }
//
//            return ret;
//        }
//
//        public void UpdateHash(string id, ulong phash)
//        {
//            this.dbNone("UPDATE cards SET pHash=" + phash.ToString() + " WHERE id=" + id);
//        }
//    }
//}
