using System;
using System.Collections.Generic;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp1 {
    class Program {
        static void Main(string[] args) {
            OdbcHandler odbcTest = new OdbcHandler();
            odbcTest.Hostname = "127.0.0.1";
            odbcTest.Database = "demo";
            odbcTest.Username = "test";
            odbcTest.Password = "demotest";
            odbcTest.Port = 3306;
            odbcTest.Drivername = "MariaDB ODBC 3.1 Driver";
            if (odbcTest.Connect()) {
                odbcTest.Query("truncate mytesttable", null);

                ThreadPool.QueueUserWorkItem((o) => {
                    while (true) {
                        Console.WriteLine("[" + DateTime.Now.ToString() + "] 1 querying .. ");
                        odbcTest.Query("select * from mytesttable", null);
                        odbcTest.Query("insert into mytesttable (`sometext`, `somedate`) values (?, ?)", new object[] { "asdf asdf asdf asdf", DateTime.UtcNow });
                    }
                });

                ThreadPool.QueueUserWorkItem((o) => {
                    while (true) {
                        Console.WriteLine("[" + DateTime.Now.ToString() + "] 2 querying .. ");
                        odbcTest.Query("select * from mytesttable", null);
                        odbcTest.Query("insert into mytesttable (`sometext`, `somedate`) values (?, ?)", new object[] { "asdf asdf asdf asdf", DateTime.UtcNow });
                    }
                });

                Console.ReadLine();
            }
        }
    }


}
