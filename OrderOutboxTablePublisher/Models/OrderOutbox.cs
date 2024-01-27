using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderOutboxTablePublisher.Models
{
    public class OrderOutbox
    {
        [Key]
        public Guid IdempotentToken { get; set; }
        public DateTime OccuredOn { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string Type { get; set; }
        public string Payload { get; set; }
    }
}
