using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace OrmBenchmark.EntityFrameworkCore
{
    class OrmBenchmarkContext : DbContext
    {
        public OrmBenchmarkContext(DbContextOptions options) : base(options) {

        }

        public virtual DbSet<Post> Posts { get; set; }

       
    }
}
