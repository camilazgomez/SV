using Microsoft.Extensions.Hosting;

namespace SV.Models
{
    public class expandedDetailsOfForms
    {
        public List<Person> Sellers { get; set; }
        public List<Person> Buyers { get; set; }

        public RealStateForm RealStateForm { get; set; }
    }
}
