using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrmBenchmark.Core
{
    public class Benchmarker
    {
        private List<IOrmExecuter> Executers { get; }
        public List<BenchmarkResult> Results { get;  }
        public List<BenchmarkResult> ResultsForAllItems { get;  }
        public List<BenchmarkResult> ResultsForDynamicItem { get;  }
        public List<BenchmarkResult> ResultsForAllDynamicItems { get;  }
        public List<BenchmarkResult> ResultsWarmUp { get;  }
        private int IterationCount { get; }
        private string ConnectionString { get; }

        public Benchmarker(string connectionString, int iterationCount)
        {
            ConnectionString = connectionString;
            IterationCount = iterationCount;
            Executers = new List<IOrmExecuter>();
            Results = new List<BenchmarkResult>();
            ResultsForDynamicItem = new List<BenchmarkResult>();
            ResultsForAllItems = new List<BenchmarkResult>();
            ResultsForAllDynamicItems = new List<BenchmarkResult>();
            ResultsWarmUp = new List<BenchmarkResult>();
        }

        public void RegisterOrmExecuter(IOrmExecuter executer)
        {
            Executers.Add(executer);
        }

        public void Run(bool warmUp = false)
        {
            PrepareDatabase();

            Results.Clear();
            ResultsForDynamicItem.Clear();
            ResultsForAllItems.Clear();
            ResultsForAllDynamicItems.Clear();
            ResultsWarmUp.Clear();

            var rand = new Random();
            foreach (IOrmExecuter executer in Executers.OrderBy(ignore => rand.Next())){

                Console.WriteLine("\texecut {0}",executer.Name);

                executer.Init(ConnectionString);

                // Warm-up
                if (warmUp)
                {
                    Stopwatch watchForWaemUp = new Stopwatch();
                    watchForWaemUp.Start();
                    executer.GetItemAsObject(IterationCount + 1);
                    executer.GetItemAsDynamic(IterationCount + 1);
                    watchForWaemUp.Stop();
                    ResultsWarmUp.Add(new BenchmarkResult { Name = executer.Name, ExecTime = watchForWaemUp.ElapsedMilliseconds });
                }

                // Object
                Stopwatch watch = new Stopwatch();
                long firstItemExecTime = 0;
                for (int i = 1; i <= IterationCount; i++)
                {
                    watch.Start();
                    var obj = executer.GetItemAsObject(i);
                    Debug.Assert(obj.Id == i,"obj.Id == i");
                    watch.Stop();
                    //if (obj?.Id != i)
                    //    throw new ApplicationException("Invalid object returned.");
                    if (i == 1)
                        firstItemExecTime = watch.ElapsedMilliseconds;
                }
                Results.Add(new BenchmarkResult { Name = executer.Name, ExecTime = watch.ElapsedMilliseconds, FirstItemExecTime = firstItemExecTime});
                
                // Dynamic
                Stopwatch watchForDynamic = new Stopwatch();
                firstItemExecTime = 0;
                for (int i = 1; i <= IterationCount; i++)
                {
                    watchForDynamic.Start();
                    var dynamicObj = executer.GetItemAsDynamic(i);
                    //bool checkResult = dynamicObj.Id == i;
                    //Debug.Assert(dynamicObj!=null,"dynamicObj!=null");
                    //Debug.Assert(checkResult,"dynamicObj.Id == i");
                    watchForDynamic.Stop();
                    //if (dynamicObj?.Id != i)
                    //    throw new ApplicationException("Invalid object returned.");
                    if (i == 1)
                        firstItemExecTime = watchForDynamic.ElapsedMilliseconds;
                }
                ResultsForDynamicItem.Add(new BenchmarkResult { Name = executer.Name, ExecTime = watchForDynamic.ElapsedMilliseconds, FirstItemExecTime = firstItemExecTime });
                
                // All Objects
                Stopwatch watchForAllItems = new Stopwatch();
                watchForAllItems.Start();
                var elements = executer.GetAllItemsAsObject();
                Debug.Assert(elements.Count > 5000);
                watchForAllItems.Stop();
                ResultsForAllItems.Add(new BenchmarkResult { Name = executer.Name, ExecTime = watchForAllItems.ElapsedMilliseconds });

                // All Dynamics
                Stopwatch watchForAllDynamicItems = new Stopwatch();
                watchForAllDynamicItems.Start();
                executer.GetAllItemsAsDynamic();
                watchForAllDynamicItems.Stop();
                ResultsForAllDynamicItems.Add(new BenchmarkResult { Name = executer.Name, ExecTime = watchForAllDynamicItems.ElapsedMilliseconds });

                executer.Finish();

                GC.Collect();
            }
        }

        private void PrepareDatabase()
        {
            Console.WriteLine("PrepareDatabase {0}",this.ConnectionString);
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();

                var cmd = conn.CreateCommand();
                cmd.CommandText = @"
                    if (OBJECT_ID('Posts') is null)
                    begin
	                    create table Posts
	                    (
		                    Id int identity primary key, 
		                    [Text] varchar(max) not null, 
		                    CreationDate datetime not null, 
		                    LastChangeDate datetime not null,
		                    Counter1 int,
		                    Counter2 int,
		                    Counter3 int,
		                    Counter4 int,
		                    Counter5 int,
		                    Counter6 int,
		                    Counter7 int,
		                    Counter8 int,
		                    Counter9 int
	                    )
	   
	                    set nocount on 

	                    declare @i int
	                    declare @c int
	                    declare @id int
	                    set @i = 0

	                    while @i <= 5001
	                    begin 
		                    insert Posts ([Text], CreationDate, LastChangeDate) values (replicate('x', 2000), GETDATE(), GETDATE())
		                    set @id = @@IDENTITY
		
		                    set @i = @i + 1
	                    end
                    end";

                cmd.Connection = conn;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
    }
}
