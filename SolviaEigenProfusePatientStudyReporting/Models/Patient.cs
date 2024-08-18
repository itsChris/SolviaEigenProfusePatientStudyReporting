namespace SolviaEigenProfusePatientStudyReporting.Models
{
    public class Patient
    {
        public DateTime RecDateTime { get; set; }
        public DateTime TimeLastUpdate { get; set; }
        public string PatDicom { get; set; }
        public string PatDOB { get; set; }
        public string PatID { get; set; }
        public string PatGender { get; set; }
        public string PatAge { get; set; }
        public string PatWeight { get; set; }
        public string PatComments { get; set; }
    }
}
