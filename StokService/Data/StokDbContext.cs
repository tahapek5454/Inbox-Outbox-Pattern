using Microsoft.EntityFrameworkCore;
using StokService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StokService.Data
{
    public class StokDbContext: DbContext
    {
        public StokDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<OrderInbox> OrderInboxes { get; set; }
    }
}
