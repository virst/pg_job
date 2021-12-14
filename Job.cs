using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace pg_job
{
    [Table("jobs")]
    internal class Job
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("last_date")]
        public DateTime? LastDate { get; set; }

        [Column("this_date")]
        public DateTime? ThisDate { get; set; }

        [Column("next_date")]
        public DateTime? NextDate { get; set; }

        [Column("active")]
        public bool Active { get; set; }

        [Column("interval")]
        public string Interval { get; set; }

        [Column("failures")]
        public int Failures { get; set; }

        [Column("what")]
        public string What { get; set; }
    }
}