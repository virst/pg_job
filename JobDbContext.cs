using System;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace pg_job
{
    internal class JobDbContext : DbContext
    {
        internal readonly string DbConn;
        public DbSet<Job> Jobs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(DbConn);
            Console.WriteLine("DbConn=" + DbConn);
        }

        public JobDbContext(string dbConn)
        {
            DbConn = dbConn;
            using NpgsqlConnection con = new NpgsqlConnection(DbConn);
            con.Open();
            var com = con.CreateCommand();
            com.CommandText = "select count(1) from jobs";
            try
            {
                com.ExecuteScalar();
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "42P01")
                {
                    // ReSharper disable once VirtualMemberCallInConstructor
                    var bb = Database.GenerateCreateScript();
                    com.CommandText = bb;
                    com.ExecuteNonQuery();
                }
            }
        }
    }
}
