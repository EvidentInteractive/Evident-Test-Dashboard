using System.Collections.Generic;
using EvidentTestDashboard.Library.Entities;

namespace EvidentTestDashboard.Web.ViewModels
{
    public class BuildViewModel
    {
        public int TestOccurrencesNoRun { get; set; }
        public Build Build { get; set; }
        public Environment Environment { get; set; }
        public IDictionary<string, List<TestOccurrence>> TestOccurrences { get; set; }
    }
}