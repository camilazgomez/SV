using Microsoft.Extensions.Hosting;

namespace SV.Models
{
    public class ExpandedDetailsOfForms
    {
        public List<Person>? Sellers { get; set; }
        public List<Person>? Buyers { get; set; }

        public RealStateForm RealStateForm { get; set; }

        public ExpandedDetailsOfForms(List<Person>? sellers, List<Person>? buyers, RealStateForm realStateForm)
        {
            Sellers = sellers;
            Buyers = buyers;
            RealStateForm = realStateForm;
        }
    }
    
}
