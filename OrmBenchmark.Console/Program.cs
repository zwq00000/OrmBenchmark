using OrmBenchmark.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OrmBenchmark.ConsoleUI.Properties;
using OrmBenchmark.Linq2Sql;

namespace OrmBenchmark.ConsoleUI
{
    class Program
    {
        static void Main(string[] args)
        {
            //string connStr = ConfigurationManager.ConnectionStrings["OrmBenchmark.ConsoleUI.Properties.Settings.OrmBenchmarkConnectionString"].ConnectionString;
            string connStr = Settings.Default.OrmBenchmarkConnectionString; //ConfigurationManager.ConnectionStrings["sqlServerLocal"].ConnectionString;
            TestConnection(connStr);

            bool warmUp = false;

            var benchmarker = new Benchmarker(connStr, 500);
            
            benchmarker.RegisterOrmExecuter(new Ado.PureAdoExecuter());
            //benchmarker.RegisterOrmExecuter(new Ado.PureAdoExecuterGetValues());
            //benchmarker.RegisterOrmExecuter(new SimpleData.SimpleDataExecuter());
            benchmarker.RegisterOrmExecuter(new Dapper.DapperExecuter());
            /* benchmarker.RegisterOrmExecuter(new Dapper.DapperBufferedExecuter());
             benchmarker.RegisterOrmExecuter(new Dapper.DapperFirstOrDefaultExecuter());
             benchmarker.RegisterOrmExecuter(new Dapper.DapperContribExecuter());
             benchmarker.RegisterOrmExecuter(new PetaPoco.PetaPocoExecuter());
             benchmarker.RegisterOrmExecuter(new PetaPoco.PetaPocoFastExecuter());
             benchmarker.RegisterOrmExecuter(new PetaPoco.PetaPocoFetchExecuter());
             benchmarker.RegisterOrmExecuter(new PetaPoco.PetaPocoFetchFastExecuter());
             benchmarker.RegisterOrmExecuter(new OrmToolkit.OrmToolkitExecuter());
             benchmarker.RegisterOrmExecuter(new OrmToolkit.OrmToolkitNoQueryExecuter());
             benchmarker.RegisterOrmExecuter(new OrmToolkit.OrmToolkitAutoMapperExecuter());
             benchmarker.RegisterOrmExecuter(new OrmToolkit.OrmToolkitTestExecuter());
             */
            benchmarker.RegisterOrmExecuter(new EntityFramework.EntityFrameworkExecuter());
            benchmarker.RegisterOrmExecuter(new EntityFrameworkCore.EntityFrameworkCoreExecuter());
            benchmarker.RegisterOrmExecuter(new Linq2SqlExecuter());
            /*benchmarker.RegisterOrmExecuter(new InsightDatabase.InsightDatabaseExecuter());
            benchmarker.RegisterOrmExecuter(new InsightDatabase.InsightSingleDatabaseExecuter());
            benchmarker.RegisterOrmExecuter(new OrmLite.OrmLiteExecuter());
            */

            Console.Write("\nDo you like to have a warm-up stage(y/[n])?");
            var str = Console.ReadLine();
            if (str.Trim().ToLower() == "y" || str.Trim().ToLower() == "yes")
                warmUp = true;

            Console.WriteLine(".NET: " + Environment.Version);
            Console.WriteLine("Connection string: {0}", connStr);
            Console.WriteLine("\nRunning...");
            
            for (int i = 0; i < 10; i++) {
                Console.WriteLine("\tStep {0}",i+1);
                benchmarker.Run(warmUp);
                OutputResult(benchmarker,warmUp);
            }
            Console.WriteLine("Finished.");

            Console.ReadLine();
        }

        private static void OutputResult(Benchmarker benchmarker,bool warmUp) {
            Console.ForegroundColor = ConsoleColor.Red;
            if (warmUp) {
                Console.WriteLine("\nPerformance of Warm-up:");
                ShowResults(benchmarker.ResultsWarmUp, false, false);
            }

            Console.WriteLine("\nPerformance of select and map a row to a POCO object over 500 iterations:");
            ShowResults(benchmarker.Results, true);

            Console.WriteLine("\nPerformance of select and map a row to a Dynamic object over 500 iterations:");
            ShowResults(benchmarker.ResultsForDynamicItem, true);

            Console.WriteLine("\nPerformance of mapping 5000 rows to POCO objects in one iteration:");
            ShowResults(benchmarker.ResultsForAllItems);

            Console.WriteLine("\nPerformance of mapping 5000 rows to Dynamic objects in one iteration:");
            ShowResults(benchmarker.ResultsForAllDynamicItems);
        }

        private static void TestConnection(string connectionString) {
            using (var conn = new SqlConnection(connectionString)) {
                conn.Open();
                Console.WriteLine("Connect Database {0}",conn.Database);
            }
        }

        static void ShowResults(List<BenchmarkResult> results, bool showFirstRun = false, bool ignoreZeroTimes = true)
        {
            var defaultColor = Console.ForegroundColor;
            //Console.ForegroundColor = ConsoleColor.Gray;

            int i = 0;
            var list = results.OrderBy(o => o.ExecTime);
            if(ignoreZeroTimes)
                list = results.FindAll(o => o.ExecTime > 0).OrderBy(o => o.ExecTime);

            foreach (var result in list)
            {
                Console.ForegroundColor = i < 3 ? ConsoleColor.Green : ConsoleColor.Gray;

                if (showFirstRun)
                    Console.WriteLine(string.Format("{0,2}-{1,-40} {2,5} ms (First run: {3,3} ms)", ++i, result.Name, result.ExecTime, result.FirstItemExecTime));
                else
                    Console.WriteLine(string.Format("{0,2}-{1,-40} {2,5} ms", ++i, result.Name, result.ExecTime));
            }

            Console.ForegroundColor = defaultColor;
        }
    }
}
