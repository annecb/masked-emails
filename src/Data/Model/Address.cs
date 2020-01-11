﻿using System;

namespace Data.Model
{
    public class Address
    {
        public long Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string EmailAddress { get; set; }
        public bool EnableForwarding { get; set; }

        // statistics

        public int Received { get; set; }

        public DateTime CreatedUtc { get; set; }

        // navigation properties

        public virtual Profile Profile { get; set; }
    }
}
