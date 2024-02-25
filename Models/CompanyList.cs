namespace Gestion_des_entreprises_dans_la_registre_de_commerce_de_Suisse
{
    public class CompanyList
    {
        public string uid { get; set; }
        public string name { get; set; }
        public int legalSeatId { get; set; }
        public string legalSeat { get; set; }
        public int registryOfCommerceId { get; set; }
        public LegalForm legalForm { get; set; }
        public string deletionDate { get; set; }
    }
}
