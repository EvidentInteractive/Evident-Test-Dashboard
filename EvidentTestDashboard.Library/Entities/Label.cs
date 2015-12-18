﻿using System.Collections.Generic;

namespace EvidentTestDashboard.Library.Entities
{
    public class Label
    {
        public Label()
        {
            TestOccurrences = new List<TestOccurrence>();
        }

        public int LabelId { get; set; }
        public string LabelName { get; set; }

        public ICollection<TestOccurrence> TestOccurrences { get; set; } 
    }
}