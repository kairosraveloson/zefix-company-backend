namespace Gestion_des_entreprises_dans_la_registre_de_commerce_de_Suisse
{
    public class CompanyObject
    {
        public string name { get; set; }
        public int ehraid { get; set; }
        public string Uid { get; set; }
        public string chid { get; set; }
        public int legalSeatId { get; set; }
        public string legalSeat { get; set; }
        public int registryOfCommerceId { get; set; }
        public LegalForm legalForm { get; set; }
        public string status { get; set; }
        public string sogcDate { get; set; }
        public string deletionDate { get; set; }
        public string translation { get; set; }
        public string purpose { get; set; }
        public List<SogcPub> sogcPub { get; set; }
        public CompanyAdresse address { get; set; }
        public string canton { get; set; }
        public string capitalNominal { get; set; }
        public string capitalCurrency { get; set; }
        public object headOffices { get; set; }
        public object furtherHeadOffices { get; set; }
        public object branchOffices { get; set; }
        public object hasTakenOver { get; set; }
        public List<WasTakenOverBy> wasTakenOverBy { get; set; }
        public object auditCompanies { get; set; }
        public object oldNames { get; set; }
        public string cantonalExcerptWeb { get; set; }
        public LanguagesOptions zefixDetailWeb { get; set; }
    }




    public class SogcPub
    {
        public string SogcDate { get; set; }
        public long SogcId { get; set; }
        public int RegistryOfCommerceId { get; set; }
        public string RegistryOfCommerceCanton { get; set; }
        public int RegistryOfCommerceJournalId { get; set; }
        public string RegistryOfCommerceJournalDate { get; set; }
        public string Message { get; set; }
        public List<MutationType> MutationTypes { get; set; }
    }



    public class WasTakenOverBy
    {
        public string Name { get; set; }
        public int Ehraid { get; set; }
        public string Uid { get; set; }
        public string Chid { get; set; }
        public int LegalSeatId { get; set; }
        public string LegalSeat { get; set; }
        public int RegistryOfCommerceId { get; set; }
        public LegalForm LegalForm { get; set; }
        public string Status { get; set; }
        public string SogcDate { get; set; }
        public string DeletionDate { get; set; }
    }
}
