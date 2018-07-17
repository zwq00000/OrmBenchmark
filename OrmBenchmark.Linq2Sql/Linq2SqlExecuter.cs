using System.Collections.Generic;
using System.Linq;
using OrmBenchmark.Core;

namespace OrmBenchmark.Linq2Sql {
    public class Linq2SqlExecuter : IOrmExecuter {
        OrmBenchmarkDataContext ctx;

        public string Name {
            get {
                return "Linq To Sql";
            }
        }

        public void Init(string connectionStrong) {
            ctx = new OrmBenchmarkDataContext(connectionStrong) {
                ObjectTrackingEnabled = false
            };
        }

        public IPost GetItemAsObject(int id) {
            //return ctx.Posts.Where(p => p.Id == Id) as IPost;
            return ctx.Posts.FirstOrDefault(p => p.Id == id);

        }

        public dynamic GetItemAsDynamic(int id) {
            return ctx.Posts.FirstOrDefault(p => p.Id == id); ;
        }

        public IList<IPost> GetAllItemsAsObject() {
            return ctx.Posts.ToList<IPost>();
        }

        public IList<dynamic> GetAllItemsAsDynamic() {
            return ctx.Posts.ToList<dynamic>();
        }

        public void Finish() {
            ctx.Dispose();
        }
    }
}
