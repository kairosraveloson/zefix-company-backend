namespace Gestion_des_entreprises_dans_la_registre_de_commerce_de_Suisse
{
    public class ListSogc
    {
        public SogcPublication SogcPublication { get; set; }
        public CompanyShort CompanyShort { get; set; }
    }

    public class SogcPublication
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

    public class MutationType
    {
        public int Id { get; set; }
        public string Key { get; set; }
    }

    public class CompanyShort
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
