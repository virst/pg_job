using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
// ReSharper disable InconsistentlySynchronizedField

namespace pg_job
{
    class Program
    {
        private static JobDbContext _jobDbContext;
        static void Main(string[] args)
        {
            Console.WriteLine("Run!");
            _jobDbContext = new JobDbContext(args[0]);
            Console.WriteLine("Begin!");
            while (true)
            {
                List<Job> jobs;
                lock (_jobDbContext)
                    jobs = _jobDbContext.Jobs.Where(t => t.Active && t.ThisDate == null && t.NextDate <= DateTime.Now)
                    .ToList();
                foreach (var job in jobs)
                {
                    lock (_jobDbContext)
                    {
                        job.ThisDate = DateTime.Now;
                        _jobDbContext.SaveChanges();
                    }
                    Console.WriteLine("Set Job - " + job.Id);
                    var t = new Thread(() => RunJob(job, _jobDbContext.DbConn));
                    t.Start();
                }
                Thread.Sleep(500);
            }
        }

        static void RunJob(Job job, string con)
        {
            Console.WriteLine("Begin Job - " + job.Id);
            using NpgsqlConnection conn = new NpgsqlConnection(con);
            conn.Open();
            var com = conn.CreateCommand();
            com.CommandText = job.What;
            try
            {
                com.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{job.Id}]-" + ex);
                job.Failures++;
            }
            com.CommandText = "SELECT " + job.Interval;
            job.NextDate = Convert.ToDateTime(com.ExecuteScalar());
            lock (_jobDbContext)
            {
                job.LastDate = job.ThisDate;
                job.ThisDate = null;
                _jobDbContext.SaveChanges();
            }
            Console.WriteLine("End Job - " + job.Id);
        }
    }
}
