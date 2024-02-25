namespace Gestion_des_entreprises_dans_la_registre_de_commerce_de_Suisse
{
    public class CompanyInfo
    {
        public string uid { get; set; }
        public string name { get; set; }
        public CompanyAdresse adresse { get; set; }
        public LegalForm legalForm { get; set; }
        public string legalSeat { get; set; }
        public string deletionDate { get; set; }
    }
}
